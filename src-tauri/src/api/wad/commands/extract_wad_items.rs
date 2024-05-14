use std::iter;
use std::path::{Path, PathBuf};
use std::str::FromStr;

use color_eyre::eyre::{eyre, Context, ContextCompat};
use itertools::Itertools;
use tauri::Manager;
use uuid::Uuid;

use crate::api::wad::commands::ApiError;
use crate::core::wad::tree::{WadTreeItem, WadTreeParent, WadTreePathable};
use crate::core::wad::{self, WadChunk};
use crate::state::SettingsState;
use crate::utils::actions::emit_action_progress;
use crate::{MountedWadsState, WadHashtableState};

#[tauri::command]
pub async fn extract_wad_items(
    app_handle: tauri::AppHandle,
    wad_id: Uuid,
    action_id: Uuid,
    parent_item_id: Option<Uuid>,
    items: Vec<Uuid>,
    extract_directory: String,
    mounted_wads: tauri::State<'_, MountedWadsState>,
    wad_hashtable: tauri::State<'_, WadHashtableState>,
    settings: tauri::State<'_, SettingsState>,
) -> Result<(), ApiError> {
    let mut mounted_wads = mounted_wads.0.lock();

    let (wad_tree, wad) = mounted_wads
        .get_wad_mut(wad_id)
        .wrap_err("failed to find wad")?;

    let parent_item = parent_item_id.map_or(None, |parent_item_id| {
        match wad_tree.item_storage().get(&parent_item_id) {
            Some(WadTreeItem::Directory(parent)) => Some(parent),
            _ => None,
        }
    });

    // get chunks for extraction
    let chunks = items
        .iter()
        .filter_map(|item_id| match wad_tree.item_storage().get(item_id) {
            Some(WadTreeItem::File(item)) => Some(*item.chunk()),
            _ => None,
        })
        .collect_vec();

    let (mut decoder, _) = wad.decode();

    emit_action_progress(
        &app_handle,
        action_id,
        0.0,
        Some("Preparing extraction directories...".into()),
    )?;
    wad::prepare_extraction_directories_relative(
        chunks.iter(),
        parent_item.map(|x| PathBuf::from_str(&x.path()).unwrap()),
        &wad_hashtable.0.lock(),
        &extract_directory,
    )?;
    let progress_offset = 0.1;

    wad::extract_wad_chunks_relative(
        &mut decoder,
        &chunks,
        parent_item.map(|x| PathBuf::from_str(&x.path()).unwrap()),
        &wad_hashtable.0.lock(),
        PathBuf::from(extract_directory.clone()),
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
        open::that(&extract_directory).wrap_err(format!(
            "failed to open extraction directory: {}",
            extract_directory
        ))?;
    }

    Ok(())
}
