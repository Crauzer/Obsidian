use std::iter;
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
    parent_item_id: Option<Uuid>,
    items: Vec<Uuid>,
    extract_directory: String,
    mounted_wads: tauri::State<'_, MountedWadsState>,
    wad_hashtable: tauri::State<'_, WadHashtableState>,
) -> Result<(), ApiError> {
    tracing::info!("parent_item_id = {:?}", &parent_item_id);
    let mut mounted_wads = mounted_wads.0.lock();

    let (wad_tree, wad) = mounted_wads
        .get_wad_mut(wad_id)
        .wrap_err("failed to find wad")?;

    let parent_item = parent_item_id.map_or(None, |parent_item_id| {
        tracing::info!("parent_item_id = {}", &parent_item_id);
        match wad_tree.find_item(|item| item.id() == parent_item_id) {
            Some(WadTreeItem::Directory(parent)) => Some(parent),
            _ => return None,
        }
    });

    let extraction_items = match parent_item {
        Some(parent_item) => resolve_items(items.iter(), parent_item),
        None => resolve_items(items.iter(), wad_tree),
    };

    // get chunks for extraction
    let chunks = extraction_items
        .iter()
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
        parent_item.map(|x| Path::new(x.path())),
        &wad_hashtable.0.lock(),
        &extract_directory,
    )?;
    let progress_offset = 0.1;

    tracing::info!("{:#?}", &parent_item);
    wad::extract_wad_chunks_relative(
        &mut decoder,
        &chunks,
        parent_item.map(|x| Path::new(x.path())),
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

fn resolve_items<'parent>(
    extraction_items: impl Iterator<Item = &Uuid>,
    parent: &'parent impl WadTreeParent,
) -> Vec<&'parent WadTreeItem> {
    extraction_items
        .filter_map(|item_id| {
            // this is quite sub-optimal since it's a linear search
            parent.find_item(|item| item.id() == *item_id)
        })
        .collect_vec()
}
