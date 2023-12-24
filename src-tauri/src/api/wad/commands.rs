use super::WadItemPathComponentDto;
use crate::{
    api::error::ApiError,
    core::wad::{
        tree::{WadTreeItem, WadTreeParent, WadTreePathable},
        Wad, WadChunk, WadDecoder,
    },
    state::{MountedWadsState, WadHashtable, WadHashtableState},
    utils::actions::emit_action_progress,
};
use color_eyre::eyre;
use eyre::Context;
use itertools::Itertools;
use std::{
    collections::VecDeque,
    fs::{self, DirBuilder},
    io::{self, Read, Seek},
    path::{Path, PathBuf},
    str::FromStr,
    sync::Arc,
};
use tracing::{error, info, warn};
use uuid::Uuid;

#[tauri::command]
pub async fn extract_mounted_wad(
    app_handle: tauri::AppHandle,
    wad_id: String,
    action_id: String,
    extract_directory: String,
    mounted_wads: tauri::State<'_, MountedWadsState>,
    wad_hashtable: tauri::State<'_, WadHashtableState>,
) -> Result<(), ApiError> {
    info!("extracting mounted wad (wad_id: {})", wad_id);

    let action_id = Uuid::from_str(&action_id).wrap_err(format!(
        "failed to parse action_id (action_id = {})",
        action_id
    ))?;
    let mut mounted_wads = mounted_wads.0.lock();
    let wad_hashtable = wad_hashtable.0.lock();

    let wad_id = uuid::Uuid::parse_str(&wad_id)
        .map_err(|_| ApiError::from_message("failed to parse wad_id"))?;
    let wad = mounted_wads
        .wads_mut()
        .get_mut(&wad_id)
        .ok_or(ApiError::from_message(format!(
            "failed to find wad (wad_id: {})",
            wad_id
        )))?;

    let extract_directory = PathBuf::from(extract_directory);

    emit_action_progress(
        &app_handle,
        action_id,
        0.0,
        Some("Preparing extraction directories...".into()),
    )?;

    // pre-create all chunk directories
    prepare_extraction_directories(&wad, &wad_hashtable, &extract_directory)?;
    let progress_offset = 0.1;

    // extract all chunks
    extract_wad_chunks(
        wad,
        &wad_hashtable,
        extract_directory,
        |progress, message| {
            emit_action_progress(
                &app_handle,
                action_id,
                progress_offset + (progress * (1.0 - progress_offset)),
                message.map(|x| x.to_string()),
            )
        },
    )?;

    info!("extraction complete (wad_id = {})", wad_id);

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

fn prepare_extraction_directories<TSource>(
    wad: &Wad<TSource>,
    wad_hashtable: &WadHashtable,
    extraction_directory: impl AsRef<Path>,
) -> eyre::Result<()>
where
    TSource: Read + Seek,
{
    info!("preparing extraction directories");

    let chunk_directories = wad.chunks().iter().map(|(_, chunk)| {
        Path::new(wad_hashtable.resolve_path(chunk.path_hash()).as_ref())
            .parent()
            .map(|path| path.to_path_buf())
    });
    for chunk_directory in chunk_directories {
        if let Some(chunk_directory) = chunk_directory {
            DirBuilder::new()
                .recursive(true)
                .create(extraction_directory.as_ref().join(chunk_directory))?;
        }
    }

    Ok(())
}

fn extract_wad_chunks<TSource: Read + Seek>(
    wad: &mut Wad<TSource>,
    wad_hashtable: &WadHashtable,
    extract_directory: PathBuf,
    report_progress: impl Fn(f64, Option<&str>) -> eyre::Result<()>,
) -> eyre::Result<()> {
    info!("extracting chunks");

    let mut i = 0;
    let (mut decoder, chunks) = wad.decode();
    for (_, chunk) in chunks {
        let chunk_path = wad_hashtable.resolve_path(chunk.path_hash());
        let chunk_path = Path::new(chunk_path.as_ref());

        report_progress(i as f64 / chunks.len() as f64, chunk_path.to_str())?;

        extract_wad_chunk(&mut decoder, &chunk, &chunk_path, &extract_directory)?;

        i = i + 1;
    }

    Ok(())
}

fn extract_wad_chunk<'wad, TSource: Read + Seek>(
    decoder: &mut WadDecoder<'wad, TSource>,
    chunk: &WadChunk,
    chunk_path: impl AsRef<Path>,
    extract_directory: impl AsRef<Path>,
) -> eyre::Result<()> {
    let chunk_data = decoder.load_chunk_decompressed(chunk).wrap_err(format!(
        "failed to decompress chunk (chunk_path: {})",
        chunk_path.as_ref().display()
    ))?;

    if let Err(error) = fs::write(&extract_directory.as_ref().join(&chunk_path), &chunk_data) {
        if error.kind() == io::ErrorKind::InvalidFilename {
            let hashed_path = format!("{:#0x}", chunk.path_hash());
            let hashed_path = Path::new(&hashed_path);

            warn!(
                "invalid chunk filename, writing as {}",
                hashed_path.display()
            );

            fs::write(&extract_directory.as_ref().join(hashed_path), &chunk_data)?;
        } else {
            error!("{:#?}", error);
        }
    }

    Ok(())
}
