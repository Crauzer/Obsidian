use std::collections::HashMap;
use std::path::PathBuf;

use color_eyre::eyre::ContextCompat;
use uuid::Uuid;

use crate::api::wad::commands::ApiError;
use crate::core::wad::{self};
use crate::utils::actions::emit_action_progress;
use crate::{MountedWadsState, WadHashtableState};

#[tauri::command]
pub async fn extract_wad_chunks(
    app_handle: tauri::AppHandle,
    wad_id: Uuid,
    action_id: Uuid,
    chunk_path_hashes: Vec<u64>,
    extract_directory: String,
    mounted_wads: tauri::State<'_, MountedWadsState>,
    wad_hashtable: tauri::State<'_, WadHashtableState>,
) -> Result<(), ApiError> {
    let mut mounted_wads = mounted_wads.0.lock();

    let wad = mounted_wads
        .wads_mut()
        .get_mut(&wad_id)
        .wrap_err("failed to find wad")?;

    let (mut decoder, chunks) = wad.decode();
    let chunks: HashMap<_, _> = chunk_path_hashes
        .iter()
        .filter_map(|chunk_path_hash| match chunks.get(chunk_path_hash) {
            Some(chunk) => Some((*chunk_path_hash, *chunk)),
            None => {
                tracing::warn!(
                    "failed to find chunk (chunk_path_hash: {}, wad_id: {}))",
                    chunk_path_hash,
                    wad_id
                );
                None
            }
        })
        .collect();

    emit_action_progress(
        &app_handle,
        action_id,
        0.0,
        Some("Preparing extraction directories...".into()),
    )?;
    wad::prepare_extraction_directories(
        chunks.iter(),
        &wad_hashtable.0.lock(),
        &extract_directory,
    )?;
    let progress_offset = 0.1;

    wad::extract_wad_chunks(
        &mut decoder,
        &chunks,
        &wad_hashtable.0.lock(),
        PathBuf::from(extract_directory),
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

    Ok(())
}
