use image::ImageFormat;
use league_toolkit::{file::LeagueFileKind, render::texture::Texture};
use std::io::Cursor;
use uuid::Uuid;

use crate::{api::error::ApiError, core::wad::tree::WadTreeItem, state::MountedWadsState};

#[tauri::command]
pub fn get_image_bytes(
    wad_id: Uuid,
    item_id: Uuid,
    mounted_wads: tauri::State<'_, MountedWadsState>,
) -> Result<Vec<u8>, ApiError> {
    let mut mounted_wads_guard = mounted_wads.0.lock();

    let Some(wad_tree) = mounted_wads_guard.wad_trees().get(&wad_id) else {
        return Err(ApiError::from_message(format!(
            "Wad tree not found: {}",
            wad_id
        )));
    };

    let Some(item) = wad_tree.item_storage().get(&item_id) else {
        return Err(ApiError::from_message(format!(
            "Item not found: {}",
            item_id
        )));
    };

    let WadTreeItem::File(wad_tree_file) = item else {
        return Err(ApiError::from_message(format!(
            "Item is not a file: {}",
            item_id
        )));
    };

    let chunk = *wad_tree_file.chunk();

    let Some(wad) = mounted_wads_guard.wads_mut().get_mut(&wad_id) else {
        return Err(ApiError::from_message(format!("Wad not found: {}", wad_id)));
    };

    let (mut decoder, _) = wad.decode();

    let chunk_data = decoder
        .load_chunk_decompressed(&chunk)
        .map_err(|e| ApiError::from_message(format!("Failed to load chunk: {}", e)))?;

    match LeagueFileKind::identify_from_bytes(&chunk_data) {
        LeagueFileKind::Texture | LeagueFileKind::TextureDds => {
            let mut reader = Cursor::new(chunk_data);
            let mut writer = Cursor::new(Vec::new());

            let texture = Texture::from_reader(&mut reader)
                .map_err(|e| ApiError::from_message(format!("Failed to load texture: {}", e)))?;

            let image = texture
                .decode_mipmap(0)
                .map_err(|e| ApiError::from_message(format!("Failed to decode texture: {}", e)))?;

            let image = image.into_rgba_image().map_err(|e| {
                ApiError::from_message(format!("Failed to convert texture to image: {}", e))
            })?;

            image
                .write_to(&mut writer, ImageFormat::Png)
                .map_err(|e| ApiError::from_message(format!("Failed to save image: {}", e)))?;

            Ok(writer.into_inner())
        }
        _ => {
            Err(ApiError::from_message(format!(
                "Item is not a texture: {}",
                item_id
            )))
        }
    }
}
