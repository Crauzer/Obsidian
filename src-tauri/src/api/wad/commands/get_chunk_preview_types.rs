use uuid::Uuid;

use crate::{api::error::ApiError, core::wad::WadChunkPreviewType, state::MountedWadsState};

#[tauri::command]
pub fn get_chunk_preview_types(
    wad_id: Uuid,
    item_id: Uuid,
    mounted_wads: tauri::State<'_, MountedWadsState>,
) -> Result<Vec<WadChunkPreviewType>, ApiError> {
    let mounted_wads_guard = mounted_wads.0.lock();

    let Some(wad_tree) = mounted_wads_guard.wad_trees().get(&wad_id) else {
        return Err(ApiError::from_message(format!(
            "Wad tree not found: {}",
            wad_id
        )));
    };

    let Some(wad) = mounted_wads_guard.wads().get(&wad_id) else {
        return Err(ApiError::from_message(format!("Wad not found: {}", wad_id)));
    };

    let Some(item) = wad_tree.item_storage().get(&item_id) else {
        return Err(ApiError::from_message(format!(
            "Item not found: {}",
            item_id
        )));
    };

    Ok(vec![WadChunkPreviewType::Image])
}
