use std::{collections::VecDeque, sync::Arc};

use itertools::Itertools;
use uuid::Uuid;

use crate::{
    api::error::ApiError,
    core::wad::tree::{WadTreeItem, WadTreeParent, WadTreePathable},
    state::MountedWadsState,
};

use super::WadItemPathComponentDto;

#[tauri::command]
async fn extract_mounted_wad(
    mounted_wads: tauri::State<'_, MountedWadsState>,
) -> Result<(), String> {
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
    wad_id: String,
    item_id: String,
    mounted_wads: tauri::State<'_, MountedWadsState>,
) -> Result<Vec<WadItemPathComponentDto>, ApiError> {
    let wad_id = uuid::Uuid::parse_str(&wad_id)
        .map_err(|_| ApiError::from_message("failed to parse wad_id"))?;
    let item_id = uuid::Uuid::parse_str(&item_id)
        .map_err(|_| ApiError::from_message("failed to parse item_id"))?;

    let mounted_wads_guard = mounted_wads.0.lock();

    if let Some(wad_tree) = mounted_wads_guard.wad_trees().get(&wad_id) {
        let mut path_components = VecDeque::<PathComponentInternal>::new();
        collect_path_components(wad_tree, &mut path_components, &|item| item.id() == item_id);

        return Ok(path_components
            .iter()
            .skip(1) // skip tree root
            .map(|component| WadItemPathComponentDto {
                item_id: component.id,
                name: component.name.to_string(),
                path: component.path.to_string(),
            })
            .collect_vec());
    }

    Err(ApiError::from_message(format!(
        "failed to get wad tree ({})",
        wad_id
    )))
}

#[derive(Debug)]
struct PathComponentInternal {
    id: Uuid,
    name: Arc<str>,
    path: Arc<str>,
}

fn collect_path_components<'p>(
    parent: &'p (impl WadTreeParent + WadTreePathable),
    path_components: &mut VecDeque<PathComponentInternal>,
    condition: &dyn Fn(&WadTreeItem) -> bool,
) -> Option<&'p WadTreeItem> {
    path_components.push_back(PathComponentInternal {
        id: parent.id(),
        name: parent.name().into(),
        path: parent.path().into(),
    });

    for (_, item) in parent.items() {
        if condition(&item) {
            path_components.push_back(PathComponentInternal {
                id: item.id(),
                name: item.name().into(),
                path: item.path().into(),
            });
            return Some(item);
        }

        if let WadTreeItem::Directory(directory) = item {
            if let Some(item) = collect_path_components(directory, path_components, condition) {
                return Some(item);
            }
        }
    }

    path_components.pop_back();

    None
}
