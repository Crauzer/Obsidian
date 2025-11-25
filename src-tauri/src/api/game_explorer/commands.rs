use crate::{
    api::error::ApiError,
    state::{GameExplorer, GameExplorerItem, GameExplorerState, SettingsState, WadHashtableState},
};
use color_eyre::eyre::eyre;
use league_toolkit::file::LeagueFileKind;
use serde::Serialize;
use std::path::Path;
use uuid::Uuid;

use crate::core::wad::tree::WadTreeItem;

#[derive(Debug, Serialize)]
#[serde(rename_all = "camelCase")]
pub struct GameExplorerStatusResponse {
    pub is_initialized: bool,
    pub wad_count: usize,
    pub base_path: Option<String>,
}

#[derive(Debug, Serialize)]
#[serde(rename_all = "camelCase")]
pub struct MountGameExplorerResponse {
    pub wad_count: usize,
    pub base_path: String,
}

#[derive(Debug, Serialize)]
#[serde(rename_all = "camelCase", tag = "kind")]
pub enum GameExplorerItemDto {
    #[serde(rename = "file")]
    File {
        id: Uuid,
        name: String,
        path: String,
        wad_id: Uuid,
        wad_name: String,
        item_id: Uuid,
        extension_kind: String,
        compressed_size: u64,
        uncompressed_size: u64,
    },
    #[serde(rename = "directory")]
    Directory {
        id: Uuid,
        name: String,
        path: String,
        item_count: usize,
    },
}

impl GameExplorerItemDto {
    pub fn from_item(item: &GameExplorerItem, game_explorer: &GameExplorer) -> Self {
        match item {
            GameExplorerItem::File(file) => {
                // Get file details from the source WAD tree
                let (extension_kind, compressed_size, uncompressed_size) = game_explorer
                    .wad_trees()
                    .get(&file.wad_id)
                    .and_then(|tree| tree.item_storage().get(&file.item_id))
                    .map(|item| {
                        if let WadTreeItem::File(wad_file) = item {
                            let chunk = wad_file.chunk();
                            let ext_kind = file.name.split('.').last()
                                .map(|ext| LeagueFileKind::from_extension(ext))
                                .unwrap_or(LeagueFileKind::Unknown);
                            (
                                format!("{:?}", ext_kind).to_lowercase(),
                                chunk.compressed_size() as u64,
                                chunk.uncompressed_size() as u64,
                            )
                        } else {
                            ("unknown".to_string(), 0, 0)
                        }
                    })
                    .unwrap_or(("unknown".to_string(), 0, 0));

                GameExplorerItemDto::File {
                    id: file.id,
                    name: file.name.to_string(),
                    path: file.path.to_string(),
                    wad_id: file.wad_id,
                    wad_name: file.wad_name.to_string(),
                    item_id: file.item_id,
                    extension_kind,
                    compressed_size,
                    uncompressed_size,
                }
            }
            GameExplorerItem::Directory(dir) => GameExplorerItemDto::Directory {
                id: dir.id,
                name: dir.name.to_string(),
                path: dir.path.to_string(),
                item_count: dir.items.len(),
            },
        }
    }
}

#[tauri::command]
pub async fn get_game_explorer_status(
    settings: tauri::State<'_, SettingsState>,
    game_explorer: tauri::State<'_, GameExplorerState>,
) -> Result<GameExplorerStatusResponse, ApiError> {
    let settings_guard = settings.0.read();
    let game_explorer_guard = game_explorer.0.lock();

    // Return base_path from settings (so UI knows if it's configured)
    // but is_initialized and wad_count from game explorer state
    Ok(GameExplorerStatusResponse {
        is_initialized: game_explorer_guard.is_initialized(),
        wad_count: game_explorer_guard.wad_count(),
        base_path: settings_guard.league_directory.clone(),
    })
}

#[tauri::command]
pub async fn mount_game_explorer(
    settings: tauri::State<'_, SettingsState>,
    wad_hashtable: tauri::State<'_, WadHashtableState>,
    game_explorer: tauri::State<'_, GameExplorerState>,
) -> Result<MountGameExplorerResponse, ApiError> {
    let settings_guard = settings.0.read();

    let league_directory = settings_guard
        .league_directory
        .as_ref()
        .ok_or_else(|| eyre!("League directory not configured"))?;

    let base_path = Path::new(league_directory);
    if !base_path.exists() {
        return Err(eyre!("League directory does not exist: {}", league_directory))?;
    }

    let hashtable = wad_hashtable.0.lock();
    let mut game_explorer_guard = game_explorer.0.lock();

    game_explorer_guard
        .mount_from_directory(base_path, &hashtable)
        .map_err(|e| eyre!("Failed to mount game explorer: {}", e))?;

    Ok(MountGameExplorerResponse {
        wad_count: game_explorer_guard.wad_count(),
        base_path: league_directory.clone(),
    })
}

#[tauri::command]
pub async fn get_game_explorer_items(
    parent_id: Option<Uuid>,
    game_explorer: tauri::State<'_, GameExplorerState>,
) -> Result<Vec<GameExplorerItemDto>, ApiError> {
    let game_explorer_guard = game_explorer.0.lock();

    if !game_explorer_guard.is_initialized() {
        return Err(eyre!("Game explorer not initialized"))?;
    }

    let unified_tree = game_explorer_guard
        .unified_tree()
        .ok_or_else(|| eyre!("Unified tree not available"))?;

    let items: Vec<GameExplorerItemDto> = match parent_id {
        Some(parent_id) => {
            unified_tree
                .get_directory_items(&parent_id)
                .iter()
                .map(|item| GameExplorerItemDto::from_item(item, &game_explorer_guard))
                .collect()
        }
        None => {
            unified_tree
                .root_items()
                .iter()
                .filter_map(|id| unified_tree.get_item(id))
                .map(|item| GameExplorerItemDto::from_item(item, &game_explorer_guard))
                .collect()
        }
    };

    Ok(items)
}

#[tauri::command]
pub async fn get_game_explorer_path_components(
    item_id: Uuid,
    game_explorer: tauri::State<'_, GameExplorerState>,
) -> Result<Vec<GameExplorerPathComponentDto>, ApiError> {
    let game_explorer_guard = game_explorer.0.lock();

    if !game_explorer_guard.is_initialized() {
        return Err(eyre!("Game explorer not initialized"))?;
    }

    let unified_tree = game_explorer_guard
        .unified_tree()
        .ok_or_else(|| eyre!("Unified tree not available"))?;

    let mut components = Vec::new();

    // Get the item and build path from its path string
    if let Some(item) = unified_tree.get_item(&item_id) {
        let path_str = item.path();
        let mut current_path = String::new();

        for (i, part) in path_str.split(['/', '\\']).enumerate() {
            if part.is_empty() {
                continue;
            }

            if !current_path.is_empty() {
                current_path.push('/');
            }
            current_path.push_str(part);

            // Find the directory with this path
            for (id, stored_item) in unified_tree.item_storage().iter() {
                if stored_item.path() == current_path
                    && matches!(stored_item, GameExplorerItem::Directory(_))
                {
                    components.push(GameExplorerPathComponentDto {
                        id: *id,
                        name: part.to_string(),
                        path: current_path.clone(),
                    });
                    break;
                }
            }
        }
    }

    Ok(components)
}

#[derive(Debug, Serialize)]
#[serde(rename_all = "camelCase")]
pub struct GameExplorerPathComponentDto {
    pub id: Uuid,
    pub name: String,
    pub path: String,
}
