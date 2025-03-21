mod extract_wad_items;
mod search_wad;

use color_eyre::owo_colors::OwoColorize;
pub use extract_wad_items::*;
use league_toolkit::core::wad::Wad;
pub use search_wad::*;
use tauri::Manager as _;
use tauri_plugin_dialog::{DialogExt as _, FileDialogBuilder};

use super::{
    MountWadResponse, MountedWadDto, MountedWadsResponse, WadItemDto, WadItemPathComponentDto,
};
use crate::core::wad;
use crate::core::wad::tree::{WadTree, WadTreeItem};
use crate::{
    api::error::ApiError,
    core::wad::tree::{WadTreeParent, WadTreePathable, WadTreeSelectable},
    state::{MountedWadsState, SettingsState, WadHashtableState},
    utils::actions::emit_action_progress,
};
use color_eyre::eyre::{self, eyre, Context, ContextCompat};
use itertools::Itertools;
use std::collections::HashMap;
use std::{
    collections::VecDeque,
    fs::File,
    path::{Path, PathBuf},
    str::FromStr,
    sync::Arc,
};
// use tauri_plugin_dialog::Dialog;
use tracing::info;
use uuid::Uuid;

#[tauri::command]
pub async fn mount_wads(
    app: tauri::AppHandle,
    wad_paths: Option<Vec<String>>,
    mounted_wads: tauri::State<'_, MountedWadsState>,
    wad_hashtable: tauri::State<'_, WadHashtableState>,
    settings: tauri::State<'_, SettingsState>,
) -> Result<MountWadResponse, ApiError> {
    let mut mounted_wads_guard = mounted_wads.0.lock();

    let wad_paths = match wad_paths {
        Some(wad_paths) => wad_paths.iter().map(PathBuf::from).collect_vec(),
        None => {
            let mut dialog = app
                .dialog()
                .file()
                .add_filter(".wad files", &["wad.client"]);

            if let Some(default_mount_directory) = &settings.0.read().default_mount_directory {
                dialog = dialog.set_directory(Path::new(default_mount_directory));
            }

            dialog
                .blocking_pick_files()
                .ok_or(ApiError::from_message("Failed to pick files"))?
                .into_iter()
                .filter_map(|p| p.into_path().ok())
                .collect()
        }
    };

    let wad_hashtable = wad_hashtable.0.lock();
    let mut wad_ids: Vec<Uuid> = vec![];

    for wad_path in &wad_paths {
        let wad = Wad::mount(File::open(wad_path).expect("failed to open wad file"))
            .expect("failed to mount wad file");

        wad_ids.push(
            mounted_wads_guard
                .mount_wad(wad, wad_path.to_str().unwrap().into(), &wad_hashtable)
                .wrap_err("failed to mount wad")?,
        )
    }

    Ok(MountWadResponse { wad_ids })
}

#[tauri::command]
pub async fn get_wad_parent_items(
    wad_id: Uuid,
    parent_id: Option<Uuid>,
    mounted_wads: tauri::State<'_, MountedWadsState>,
) -> Result<Vec<WadItemDto>, ApiError> {
    let mounted_wads_guard = mounted_wads.0.lock();

    let Some(wad_tree) = mounted_wads_guard.wad_trees().get(&wad_id) else {
        return Err(eyre!("failed to get wad tree (wad_id: {})", wad_id))?;
    };

    let parent_items = match parent_id {
        Some(parent_id) => {
            let Some(item) = wad_tree.item_storage().get(&parent_id) else {
                return Err(eyre!("failed to get item (item_id: {})", parent_id))?;
            };
            let WadTreeItem::Directory(directory) = item else {
                return Err(eyre!("item is not a directory (item_id: {})", parent_id))?;
            };

            directory.items().iter()
        }
        None => wad_tree.items().iter(),
    };

    Ok(parent_items
        .filter_map(|id| wad_tree.item_storage().get(id))
        .map(|item| WadItemDto::from(item))
        .collect_vec())
}

#[tauri::command]
pub async fn update_mounted_wad_item_selection(
    wad_id: Uuid,
    reset_selection: bool,
    item_selections: HashMap<Uuid, bool>,
    mounted_wads: tauri::State<'_, MountedWadsState>,
) -> Result<(), ApiError> {
    let mut mounted_wads = mounted_wads.0.lock();
    let Some((wad_tree, _wad)) = mounted_wads.get_wad_mut(wad_id) else {
        return Err(eyre!("failed to get wad tree (wad_id: {})", wad_id))?;
    };

    if reset_selection {
        wad_tree.item_storage_mut().values_mut().for_each(|item| {
            item.set_is_selected(false);
        });
    }

    // apply selection
    for (item_id, is_selected) in item_selections {
        if let Some(item) = wad_tree.item_storage_mut().get_mut(&item_id) {
            item.set_is_selected(is_selected);
        };
    }

    Ok(())
}

#[tauri::command]
pub async fn get_mounted_wads(
    mounted_wads: tauri::State<'_, MountedWadsState>,
) -> Result<MountedWadsResponse, ApiError> {
    let mounted_wads_guard = mounted_wads.0.lock();

    Ok(MountedWadsResponse {
        wads: mounted_wads_guard
            .wad_trees()
            .iter()
            .map(|(tree_id, tree)| {
                let wad_path_string = tree.wad_path().to_string();
                let wad_path = Path::new(&wad_path_string);

                MountedWadDto {
                    id: *tree_id,
                    name: wad_path.file_name().unwrap().to_str().unwrap().to_string(),
                    wad_path: wad_path_string,
                }
            })
            .collect_vec(),
    })
}

#[tauri::command]
pub async fn unmount_wad(
    app_handle: tauri::AppHandle,
    wad_id: String,
    mounted_wads: tauri::State<'_, MountedWadsState>,
) -> Result<(), ApiError> {
    let mut mounted_wads_guard = mounted_wads.0.lock();

    let wad_id =
        Uuid::parse_str(&wad_id).map_err(|_| ApiError::from_message("failed to parse wad_id"))?;

    if let Some(window) = app_handle.get_webview_window(format!("wad_{}", wad_id).as_str()) {
        window
            .close()
            .map_err(|_| ApiError::from_message("failed to close window"))?;
    }

    mounted_wads_guard.unmount_wad(wad_id);

    Ok(())
}

#[tauri::command]
pub async fn extract_mounted_wad(
    app_handle: tauri::AppHandle,
    wad_id: Uuid,
    action_id: Uuid,
    extract_directory: String,
    mounted_wads: tauri::State<'_, MountedWadsState>,
    wad_hashtable: tauri::State<'_, WadHashtableState>,
    settings: tauri::State<'_, SettingsState>,
) -> Result<(), ApiError> {
    info!("extracting mounted wad (wad_id: {})", wad_id);

    let mut mounted_wads = mounted_wads.0.lock();
    let wad_hashtable = wad_hashtable.0.lock();

    let wad = mounted_wads
        .wads_mut()
        .get_mut(&wad_id)
        .ok_or(ApiError::from_message(format!(
            "failed to find wad (wad_id: {})",
            wad_id
        )))?;

    emit_action_progress(
        &app_handle,
        action_id,
        0.0,
        Some("Preparing extraction directories...".into()),
    )?;

    // pre-create all chunk directories
    let extract_directory = PathBuf::from(extract_directory);
    wad::prepare_extraction_directories_absolute(
        wad.chunks().iter(),
        &wad_hashtable,
        &extract_directory,
    )?;
    let progress_offset = 0.1;

    // extract all chunks
    let (mut decoder, chunks) = wad.decode();
    wad::extract_wad_chunks(
        &mut decoder,
        &chunks,
        &wad_hashtable,
        extract_directory.clone(),
        |progress, message| {
            emit_action_progress(
                &app_handle,
                action_id,
                progress_offset + (progress * (1.0 - progress_offset)),
                message.map(|x| x.to_string()),
            )
        },
    )?;

    tracing::info!("extraction complete (wad_id = {})", wad_id);

    if settings.0.read().open_directory_after_extraction {
        open::that(extract_directory.as_os_str()).wrap_err(format!(
            "failed to open extraction directory: {}",
            extract_directory.display()
        ))?;
    }

    Ok(())
}

#[tauri::command]
pub async fn move_mounted_wad(
    source_index: usize,
    dest_index: usize,
    mounted_wads: tauri::State<'_, MountedWadsState>,
) -> Result<(), String> {
    let mut mounted_wads_guard = mounted_wads.0.lock();

    mounted_wads_guard
        .wad_trees_mut()
        .move_index(source_index, dest_index);

    Ok(())
}

#[tauri::command]
pub fn get_mounted_wad_directory_path_components(
    wad_id: Uuid,
    item_id: Uuid,
    mounted_wads: tauri::State<'_, MountedWadsState>,
) -> Result<Vec<WadItemPathComponentDto>, ApiError> {
    let mounted_wads_guard = mounted_wads.0.lock();

    let Some(wad_tree) = mounted_wads_guard.wad_trees().get(&wad_id) else {
        return Err(ApiError::from_message(format!(
            "failed to get wad tree ({})",
            wad_id
        )));
    };

    let mut path_components = VecDeque::<PathComponentInternal>::new();
    collect_path_components(item_id, &mut path_components, wad_tree)?;

    Ok(path_components
        .iter()
        .map(|component| WadItemPathComponentDto {
            item_id: component.id,
            name: component.name.to_string(),
            path: component.path.to_string(),
        })
        .collect_vec())
}

#[derive(Debug)]
struct PathComponentInternal {
    id: Uuid,
    name: Arc<str>,
    path: Arc<str>,
}

fn collect_path_components<'wad>(
    item_id: Uuid,
    path_components: &mut VecDeque<PathComponentInternal>,
    wad_tree: &'wad WadTree,
) -> eyre::Result<()> {
    let item = wad_tree
        .item_storage()
        .get(&item_id)
        .wrap_err("failed to find item")?;

    path_components.push_front(PathComponentInternal {
        id: item.id(),
        name: item.name().into(),
        path: item.path().into(),
    });

    let Some(parent_id) = item.parent_id() else {
        return Ok(());
    };

    collect_path_components(parent_id, path_components, wad_tree)
}
