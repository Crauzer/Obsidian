use super::{
    MountWadResponse, MountedWadDto, MountedWadsResponse, WadItemDto, WadItemPathComponentDto,
    WadItemSelectionUpdate,
};
use crate::{
    api::error::ApiError,
    core::wad::{
        tree::{WadTreeItem, WadTreeItemKey, WadTreeParent, WadTreePathable, WadTreeSelectable},
        Wad, WadChunk, WadDecoder,
    },
    state::{MountedWadsState, SettingsState, WadHashtable, WadHashtableState},
    utils::actions::emit_action_progress,
};
use color_eyre::eyre::{self, ContextCompat};
use eyre::Context;
use itertools::Itertools;
use std::{
    collections::VecDeque,
    fs::{self, DirBuilder, File},
    io::{self, Read, Seek},
    ops::IndexMut,
    path::{Path, PathBuf},
    str::FromStr,
    sync::Arc,
};
use tauri::{api::dialog, Manager};
use tracing::{error, info, warn};
use uuid::Uuid;

#[tauri::command]
pub async fn mount_wads(
    mounted_wads: tauri::State<'_, MountedWadsState>,
    wad_hashtable: tauri::State<'_, WadHashtableState>,
    settings: tauri::State<'_, SettingsState>,
) -> Result<MountWadResponse, ApiError> {
    let mut mounted_wads_guard = mounted_wads.0.lock();

    let mut dialog =
        dialog::blocking::FileDialogBuilder::new().add_filter(".wad files", &["wad.client"]);

    if let Some(default_mount_directory) = &settings.0.read().default_mount_directory {
        dialog = dialog.set_directory(Path::new(default_mount_directory));
    }

    if let Some(wad_paths) = dialog.pick_files() {
        let wad_hashtable = wad_hashtable.0.lock();
        let mut wad_ids: Vec<Uuid> = vec![];

        for wad_path in &wad_paths {
            let wad = Wad::mount(File::open(&wad_path).expect("failed to open wad file"))
                .expect("failed to mount wad file");

            wad_ids.push(
                mounted_wads_guard
                    .mount_wad(wad, wad_path.to_str().unwrap().into(), &wad_hashtable)
                    .map_err(|_| ApiError::from_message("failed to mount wad"))?,
            )
        }

        return Ok(MountWadResponse { wad_ids });
    }

    Err(ApiError::from_message("Failed to pick file"))
}

#[tauri::command]
pub fn get_wad_items(
    wad_id: String,
    mounted_wads: tauri::State<'_, MountedWadsState>,
) -> Result<Vec<WadItemDto>, ApiError> {
    let mounted_wads_guard = mounted_wads.0.lock();

    if let Some(wad_tree) = mounted_wads_guard.wad_trees().get(
        &uuid::Uuid::parse_str(&wad_id)
            .map_err(|_| ApiError::from_message("failed to parse wad_id"))?,
    ) {
        return Ok(wad_tree
            .items()
            .iter()
            .map(|(_, item)| WadItemDto::from(item))
            .collect_vec());
    }

    Err(ApiError::from_message(format!(
        "failed to get wad tree ({})",
        wad_id
    )))
}

#[tauri::command]
pub async fn get_mounted_wad_directory_items(
    wad_id: String,
    item_id: String,
    mounted_wads: tauri::State<'_, MountedWadsState>,
) -> Result<Vec<WadItemDto>, ApiError> {
    let wad_id = uuid::Uuid::parse_str(&wad_id)
        .map_err(|_| ApiError::from_message("failed to parse wad_id"))?;
    let item_id = uuid::Uuid::parse_str(&item_id)
        .map_err(|_| ApiError::from_message("failed to parse item_id"))?;

    let mounted_wads_guard = mounted_wads.0.lock();

    if let Some(wad_tree) = mounted_wads_guard.wad_trees().get(&wad_id) {
        let item = wad_tree.find_item(|item| item.id() == item_id);
        let item = item.ok_or(ApiError::from_message("failed to find item"))?;

        return match item {
            WadTreeItem::File(_) => Err(ApiError::from_message("cannot get items of file")),
            WadTreeItem::Directory(directory) => Ok(directory
                .items()
                .iter()
                .map(|(_, item)| WadItemDto::from(item))
                .collect_vec()),
        };
    }

    Err(ApiError::from_message(format!(
        "failed to get wad tree ({})",
        wad_id
    )))
}

#[tauri::command]
pub async fn update_mounted_wad_item_selection(
    wad_id: Uuid,
    parent_item_id: Option<Uuid>,
    reset_selection: bool,
    item_selections: Vec<WadItemSelectionUpdate>,
    mounted_wads: tauri::State<'_, MountedWadsState>,
) -> Result<(), ApiError> {
    if let Some(wad_tree) = mounted_wads.0.lock().wad_trees_mut().get_mut(&wad_id) {
        match parent_item_id {
            None => {
                update_parent_items_selection(wad_tree, reset_selection, &item_selections);
            }
            Some(parent_item_id) => {
                let parent_item = wad_tree
                    .find_item_mut(|item| item.id() == parent_item_id)
                    .wrap_err(format!(
                        "failed to find parent wad item (parent_item_id = {})",
                        parent_item_id
                    ))?;

                if let WadTreeItem::Directory(directory) = parent_item {
                    update_parent_items_selection(directory, reset_selection, &item_selections);
                }
            }
        };
    }

    Ok(())
}

pub(crate) fn update_parent_items_selection(
    parent: &mut impl WadTreeParent,
    reset_selection: bool,
    item_selections: &Vec<WadItemSelectionUpdate>,
) {
    let parent_items = parent.items_mut();

    if reset_selection {
        for (_, item) in parent_items.iter_mut() {
            item.set_is_selected(false);
        }
    }

    // apply selection
    for item_selection_update in item_selections {
        parent_items
            .index_mut(item_selection_update.index)
            .set_is_selected(item_selection_update.is_selected);
    }
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

    if let Some(window) = app_handle.get_window(format!("wad_{}", wad_id).as_str()) {
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
