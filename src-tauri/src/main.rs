// Prevents additional console window on Windows in release, DO NOT REMOVE!!
#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

mod api;
mod core;
mod state;
mod utils;

use self::core::wad::{
    tree::{WadTreeExpandable, WadTreeItem, WadTreeParent, WadTreeSelectable},
    Wad,
};
use crate::{
    api::wad::{get_mounted_wad_directory_path_components, reorder_mounted_wad},
    core::wad_hashtable::WadHashtable,
};
use api::{
    error::ApiError,
    wad::{MountWadResponse, MountedWadDto, MountedWadsResponse, WadItemDto},
};
use itertools::Itertools;
use parking_lot::Mutex;
use state::mounted_wads::{MountedWads, MountedWadsState};
use std::{fs::File, path::Path};
use tauri::Manager;
use tracing::info;
use uuid::Uuid;

#[tauri::command]
fn get_wad_items(
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
fn get_mounted_wad_directory_items(
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
fn expand_wad_tree_item(
    wad_id: String,
    item_id: String,
    is_expanded: bool,
    mounted_wads: tauri::State<'_, MountedWadsState>,
) -> Result<(), ApiError> {
    let mut mounted_wads_guard = mounted_wads.0.lock();

    let item_id =
        Uuid::parse_str(&item_id).map_err(|_| ApiError::from_message("failed to parse item_id"))?;
    let wad_tree = mounted_wads_guard
        .wad_trees_mut()
        .get_mut(
            &Uuid::parse_str(&wad_id)
                .map_err(|_| ApiError::from_message("failed to parse wad_id"))?,
        )
        .ok_or(ApiError::from_message("failed to find wad tree"))?;

    // TODO: search and break after
    wad_tree.traverse_items_mut(&mut |item| {
        if let WadTreeItem::Directory(directory) = item {
            if directory.id() == item_id {
                directory.set_is_expanded(is_expanded);
            }
        }
    });

    Ok(())
}

#[tauri::command]
fn select_wad_tree_item(
    wad_id: String,
    item_id: String,
    is_selected: bool,
    mounted_wads: tauri::State<'_, MountedWadsState>,
) -> Result<(), ApiError> {
    let mut mounted_wads_guard = mounted_wads.0.lock();

    let item_id =
        Uuid::parse_str(&item_id).map_err(|_| ApiError::from_message("failed to parse item_id"))?;
    let wad_tree = mounted_wads_guard
        .wad_trees_mut()
        .get_mut(
            &Uuid::parse_str(&wad_id)
                .map_err(|_| ApiError::from_message("failed to parse wad_id"))?,
        )
        .ok_or(ApiError::from_message("failed to find wad tree"))?;

    wad_tree.traverse_items_mut(&mut |item| {
        if item.id() == item_id {
            item.set_is_selected(is_selected)
        } else if is_selected {
            item.set_is_selected(false)
        }
    });

    Ok(())
}

#[tauri::command]
async fn get_mounted_wads(
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
async fn mount_wads(
    mounted_wads: tauri::State<'_, MountedWadsState>,
) -> Result<MountWadResponse, ApiError> {
    let mut mounted_wads_guard = mounted_wads.0.lock();

    let file_path = tauri::api::dialog::blocking::FileDialogBuilder::new()
        .add_filter(".wad files", &["wad.client"])
        .pick_files();

    if let Some(wad_paths) = file_path {
        info!("Creating hashtable");
        let mut hashtable = WadHashtable::new();
        hashtable
            .add_from_file(
                &mut File::open("C:/Users/Filip/Downloads/hashes.game.txt")
                    .map_err(|_| ApiError::from_message("failed to open hashtable file"))?,
            )
            .map_err(|error| ApiError::from_message(error))?;

        let mut wad_ids: Vec<Uuid> = vec![];
        for wad_path in &wad_paths {
            let wad = Wad::mount(File::open(&wad_path).expect("failed to open wad file"))
                .expect("failed to mount wad file");

            wad_ids.push(
                mounted_wads_guard
                    .mount_wad(wad, wad_path.to_str().unwrap().into(), &hashtable)
                    .map_err(|_| ApiError::from_message("failed to mount wad"))?,
            )
        }

        return Ok(MountWadResponse { wad_ids });
    }

    Err(ApiError::from_message("Failed to pick file"))
}

#[tauri::command]
async fn unmount_wad(
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

fn main() {
    let subscriber = tracing_subscriber::fmt()
        .with_file(true)
        .with_line_number(true)
        .finish();

    tracing::subscriber::set_global_default(subscriber)
        .expect("failed to set global default log subscriber");

    tauri::Builder::default()
        .manage(MountedWadsState(Mutex::new(MountedWads::new())))
        .invoke_handler(tauri::generate_handler![
            expand_wad_tree_item,
            get_mounted_wad_directory_items,
            get_mounted_wad_directory_path_components,
            get_mounted_wads,
            get_wad_items,
            mount_wads,
            reorder_mounted_wad,
            select_wad_tree_item,
            unmount_wad,
        ])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
