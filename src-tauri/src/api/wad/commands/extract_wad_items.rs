use std::path::{Path, PathBuf};

use color_eyre::eyre::{eyre, ContextCompat};
use itertools::Itertools;
use uuid::Uuid;

use crate::api::wad::commands::ApiError;
use crate::core::wad::tree::{WadTreeItem, WadTreeParent, WadTreePathable};
use crate::core::wad::{self, WadChunk};
use crate::utils::actions::emit_action_progress;
use crate::{MountedWadsState, WadHashtableState};

#[tauri::command]
pub async fn extract_wad_items(
    app_handle: tauri::AppHandle,
    wad_id: Uuid,
    action_id: Uuid,

    parent_item_id: Uuid,
    items: Vec<Uuid>,
    extract_directory: String,
    mounted_wads: tauri::State<'_, MountedWadsState>,
    wad_hashtable: tauri::State<'_, WadHashtableState>,
) -> Result<(), ApiError> {
    let mut mounted_wads = mounted_wads.0.lock();

    let (wad_tree, wad) = mounted_wads
        .get_wad_mut(wad_id)
        .wrap_err("failed to find wad")?;

    // get the parent item
    let parent_item = wad_tree
        .find_item(|item| item.id() == parent_item_id)
        .wrap_err(format!(
            "failed to find parent item (parent_item = {})",
            parent_item_id
        ))?;
    let WadTreeItem::Directory(parent_item) = parent_item else {
        return Err(eyre!("parent item must be a directory"))?;
    };

    // get items for extraction
    let items = items.iter().filter_map(|item_id| {
        // this is quite sub-optimal since it's a linear search
        parent_item.find_item(|item| item.id() == *item_id)
    });

    // get chunks for extraction
    let chunks = items
        .flat_map(|item| match item {
            WadTreeItem::File(item) => vec![*item.chunk()].into_iter(),
            WadTreeItem::Directory(item) => {
                let mut chunks = Vec::<WadChunk>::new();

                item.traverse_items(&mut |item| {
                    if let WadTreeItem::File(item) = item {
                        chunks.push(*item.chunk());
                    }
                });

                chunks.into_iter()
            }
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
        Path::new(parent_item.path()),
        &wad_hashtable.0.lock(),
        &extract_directory,
    )?;
    let progress_offset = 0.1;

    wad::extract_wad_chunks_relative(
        &mut decoder,
        &chunks,
        Path::new(parent_item.path()),
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
