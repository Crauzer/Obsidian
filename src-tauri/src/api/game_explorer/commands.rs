use crate::{
    api::error::ApiError,
    core::wad::tree::{WadTreeItem, WadTreePathable},
    state::{GameExplorer, GameExplorerState, MergedItemRef, SettingsState, WadHashtableState},
};
use camino::Utf8Path;
use color_eyre::eyre::eyre;
use league_toolkit::file::LeagueFileKind;
use serde::Serialize;
use uuid::Uuid;

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
#[serde(tag = "kind")]
pub enum GameExplorerItemDto {
    #[serde(rename = "file", rename_all = "camelCase")]
    File {
        id: String, // Composite ID: "wad_id:item_id"
        name: String,
        path: String,
        wad_id: Uuid,
        wad_name: String,
        item_id: Uuid,
        extension_kind: LeagueFileKind,
        compressed_size: u64,
        uncompressed_size: u64,
    },
    #[serde(rename = "directory", rename_all = "camelCase")]
    Directory {
        id: Uuid,
        name: String,
        path: String,
        item_count: usize,
    },
}

/// Convert a MergedItemRef to a DTO by looking up data from the underlying WadTrees
fn item_ref_to_dto(
    item_ref: &MergedItemRef,
    game_explorer: &GameExplorer,
) -> Option<GameExplorerItemDto> {
    match item_ref {
        MergedItemRef::File { wad_id, item_id } => {
            // Look up the actual file from the WadTree
            let tree = game_explorer.wad_trees().get(wad_id)?;
            let item = tree.item_storage().get(item_id)?;

            if let WadTreeItem::File(file) = item {
                let wad_name = game_explorer
                    .get_wad_name(wad_id)
                    .unwrap_or("unknown")
                    .to_string();

                let chunk = file.chunk();
                let extension_kind = file
                    .name()
                    .split('.')
                    .last()
                    .map(LeagueFileKind::from_extension)
                    .unwrap_or(LeagueFileKind::Unknown);

                Some(GameExplorerItemDto::File {
                    id: format!("{}:{}", wad_id, item_id),
                    name: file.name().to_string(),
                    path: file.path().to_string(),
                    wad_id: *wad_id,
                    wad_name,
                    item_id: *item_id,
                    extension_kind,
                    compressed_size: chunk.compressed_size() as u64,
                    uncompressed_size: chunk.uncompressed_size() as u64,
                })
            } else {
                None
            }
        }
        MergedItemRef::Directory(dir_id) => {
            let index = game_explorer.merged_index()?;
            let dir = index.get_directory(dir_id)?;

            Some(GameExplorerItemDto::Directory {
                id: *dir_id,
                name: dir.name.to_string(),
                path: dir.path.to_string(),
                item_count: dir.items.len(),
            })
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

    let base_path = Utf8Path::new(league_directory);
    if !base_path.exists() {
        return Err(eyre!(
            "League directory does not exist: {}",
            league_directory
        ))?;
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

    let index = game_explorer_guard
        .merged_index()
        .ok_or_else(|| eyre!("Merged index not available"))?;

    let items: Vec<MergedItemRef> = match parent_id {
        Some(dir_id) => {
            let dir = index
                .get_directory(&dir_id)
                .ok_or_else(|| eyre!("Directory not found: {}", dir_id))?;
            dir.items.clone()
        }
        None => index.root_items().to_vec(),
    };

    // Convert refs to DTOs, sorting files by name
    let mut dtos: Vec<GameExplorerItemDto> = items
        .iter()
        .filter_map(|item_ref| item_ref_to_dto(item_ref, &game_explorer_guard))
        .collect();

    // Sort: directories first (already sorted), then files by name
    dtos.sort_by(|a, b| match (a, b) {
        (GameExplorerItemDto::Directory { .. }, GameExplorerItemDto::File { .. }) => {
            std::cmp::Ordering::Less
        }
        (GameExplorerItemDto::File { .. }, GameExplorerItemDto::Directory { .. }) => {
            std::cmp::Ordering::Greater
        }
        (
            GameExplorerItemDto::Directory { name: a_name, .. },
            GameExplorerItemDto::Directory { name: b_name, .. },
        ) => a_name.cmp(b_name),
        (
            GameExplorerItemDto::File { name: a_name, .. },
            GameExplorerItemDto::File { name: b_name, .. },
        ) => a_name.cmp(b_name),
    });

    Ok(dtos)
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

    let index = game_explorer_guard
        .merged_index()
        .ok_or_else(|| eyre!("Merged index not available"))?;

    // Find the directory and build path from its path string
    let dir = index
        .get_directory(&item_id)
        .ok_or_else(|| eyre!("Directory not found: {}", item_id))?;

    let mut components = Vec::new();
    let mut current_path = String::new();

    for part in dir.path.split(['/', '\\']) {
        if part.is_empty() {
            continue;
        }

        if !current_path.is_empty() {
            current_path.push('/');
        }
        current_path.push_str(part);

        // Find the directory with this path
        for (id, stored_dir) in index.directories().iter() {
            if stored_dir.path.as_ref() == current_path {
                components.push(GameExplorerPathComponentDto {
                    id: *id,
                    name: part.to_string(),
                    path: current_path.clone(),
                });
                break;
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
