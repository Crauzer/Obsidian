use std::{collections::HashMap, str, sync::Arc};

use color_eyre::eyre::{Context, ContextCompat};
use fst::{self, IntoStreamer, Streamer};
use itertools::Itertools;
use league_toolkit::wad::WadChunk;
use uuid::Uuid;

use crate::{
    api::{
        error::ApiError,
        wad::{SearchWadResponse, SearchWadResponseItem, guess_file_kind},
    },
    core::wad::tree::{WadTreeItem, WadTreePathable},
    state::{MountedWadsState, WadHashtableState},
};

#[tauri::command]
pub async fn search_wad(
    app_handle: tauri::AppHandle,
    wad_id: Uuid,
    query: String,
    mounted_wads: tauri::State<'_, MountedWadsState>,
    wad_hashtable: tauri::State<'_, WadHashtableState>,
) -> Result<SearchWadResponse, ApiError> {
    let mounted_wads = mounted_wads.0.lock();

    if query.trim().is_empty() {
        return Ok(SearchWadResponse { items: vec![] });
    }

    let (wad_tree, wad) = mounted_wads
        .get_wad(wad_id)
        .wrap_err("failed to find wad")?;

    let mut item_map = HashMap::new();
    for (_, item) in wad_tree.item_storage() {
        if let WadTreeItem::File(item) = item {
            item_map.insert(
                item.path().to_string(),
                SearchWadItemStorage {
                    id: item.id(),
                    parent_id: item.parent_id(),
                    name: item.name(),
                    path: item.path(),
                    chunk: item.chunk().clone(),
                },
            );
        }
    }

    let set = fst::Set::from_iter(
        item_map
            .keys()
            .into_iter()
            .sorted_by(|a, b| a.to_lowercase().cmp(&b.to_lowercase())),
    )
    .wrap_err("failed to create set")?;
    let subseq = fst::automaton::Subsequence::new(&query);
    let mut stream = set.search(&subseq).into_stream();

    let mut i = 0;
    let mut results = vec![];
    while let Some(key) = stream.next()
        && i < 50
    {
        let key = str::from_utf8(key).unwrap();
        let item_storage = item_map.get(key).unwrap();

        results.push(SearchWadResponseItem {
            id: item_storage.id,
            parent_id: item_storage.parent_id,
            name: item_storage.name.to_string(),
            path: item_storage.path.to_string(),
            extension_kind: guess_file_kind(&item_storage.name),
        });

        i = i + 1;
    }

    Ok(SearchWadResponse { items: results })
}

struct SearchWadItemStorage {
    pub id: Uuid,
    pub parent_id: Option<Uuid>,
    pub name: Arc<str>,
    pub path: Arc<str>,
    pub chunk: WadChunk,
}
