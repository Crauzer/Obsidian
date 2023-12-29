// Prevents additional console window on Windows in release, DO NOT REMOVE!!
#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]
#![feature(io_error_more)]

mod api;
mod core;
mod state;
mod utils;

use self::core::wad::{
    tree::{WadTreeExpandable, WadTreeItem, WadTreeParent, WadTreeSelectable},
    Wad,
};
use crate::{
    api::{
        actions::get_action_progress,
        fs::{get_app_directory, open_path, pick_directory, pick_file},
        hashtable::{get_wad_hashtable_status, load_wad_hashtables},
        settings::{get_settings, update_settings},
        wad::{extract_mounted_wad, get_mounted_wad_directory_path_components, move_mounted_wad},
    },
    paths::WAD_HASHTABLES_DIR,
    state::WadHashtable,
    utils::fs::try_create_dir,
};
use api::{
    error::ApiError,
    wad::{MountWadResponse, MountedWadDto, MountedWadsResponse, WadItemDto},
};
use color_eyre::eyre;
use itertools::Itertools;
use parking_lot::{lock_api::RwLock, Mutex};
use paths::LOGS_DIR;
use state::{
    ActionsState, MountedWads, MountedWadsState, Settings, SettingsState, WadHashtableState,
};
use std::{collections::HashMap, fs::File, io::stdout, path::Path};
use tauri::{App, Manager};
use tracing::info;
use tracing_subscriber::fmt::writer::MakeWriterExt;
use utils::log::create_log_filename;
use uuid::Uuid;

mod error;
mod paths;

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
    wad_hashtable: tauri::State<'_, WadHashtableState>,
) -> Result<MountWadResponse, ApiError> {
    let mut mounted_wads_guard = mounted_wads.0.lock();

    let file_path = tauri::api::dialog::blocking::FileDialogBuilder::new()
        .add_filter(".wad files", &["wad.client"])
        .pick_files();

    if let Some(wad_paths) = file_path {
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

fn main() -> eyre::Result<()> {
    tauri::Builder::default()
        .plugin(tauri_plugin_window_state::Builder::default().build())
        .plugin(tauri_plugin_upload::init())
        .manage(MountedWadsState(Mutex::new(MountedWads::new())))
        .manage(SettingsState(RwLock::new(Settings::default())))
        .manage(WadHashtableState(Mutex::new(WadHashtable::default())))
        .manage(ActionsState(RwLock::new(HashMap::default())))
        .setup(|app| {
            let app_handle = app.handle();

            println!(
                "resource_dir: {}",
                app_handle
                    .path_resolver()
                    .resource_dir()
                    .unwrap_or(std::path::PathBuf::new())
                    .to_string_lossy()
            );
            println!(
                "app_config_dir: {}",
                app_handle
                    .path_resolver()
                    .app_config_dir()
                    .unwrap_or(std::path::PathBuf::new())
                    .to_string_lossy()
            );
            println!(
                "app_data_dir: {}",
                app_handle
                    .path_resolver()
                    .app_data_dir()
                    .unwrap_or(std::path::PathBuf::new())
                    .to_string_lossy()
            );
            println!(
                "app_local_data_dir: {}",
                app_handle
                    .path_resolver()
                    .app_local_data_dir()
                    .unwrap_or(std::path::PathBuf::new())
                    .to_string_lossy()
            );
            println!(
                "app_cache_dir: {}",
                app_handle
                    .path_resolver()
                    .app_cache_dir()
                    .unwrap_or(std::path::PathBuf::new())
                    .to_string_lossy()
            );
            println!(
                "app_log_dir: {}",
                app_handle
                    .path_resolver()
                    .app_log_dir()
                    .unwrap_or(std::path::PathBuf::new())
                    .to_string_lossy()
            );
            println!(
                "{}",
                tauri::api::path::data_dir()
                    .unwrap_or(std::path::PathBuf::new())
                    .to_string_lossy()
            );
            println!(
                "{}",
                tauri::api::path::local_data_dir()
                    .unwrap_or(std::path::PathBuf::new())
                    .to_string_lossy()
            );
            println!(
                "{}",
                tauri::api::path::cache_dir()
                    .unwrap_or(std::path::PathBuf::new())
                    .to_string_lossy()
            );
            println!(
                "{}",
                tauri::api::path::config_dir()
                    .unwrap_or(std::path::PathBuf::new())
                    .to_string_lossy()
            );
            println!(
                "{}",
                tauri::api::path::executable_dir()
                    .unwrap_or(std::path::PathBuf::new())
                    .to_string_lossy()
            );
            println!(
                "{}",
                tauri::api::path::public_dir()
                    .unwrap_or(std::path::PathBuf::new())
                    .to_string_lossy()
            );
            println!(
                "{}",
                tauri::api::path::runtime_dir()
                    .unwrap_or(std::path::PathBuf::new())
                    .to_string_lossy()
            );
            println!(
                "{}",
                tauri::api::path::template_dir()
                    .unwrap_or(std::path::PathBuf::new())
                    .to_string_lossy()
            );
            println!(
                "{}",
                tauri::api::path::font_dir()
                    .unwrap_or(std::path::PathBuf::new())
                    .to_string_lossy()
            );
            println!(
                "{}",
                tauri::api::path::home_dir()
                    .unwrap_or(std::path::PathBuf::new())
                    .to_string_lossy()
            );
            println!(
                "{}",
                tauri::api::path::audio_dir()
                    .unwrap_or(std::path::PathBuf::new())
                    .to_string_lossy()
            );
            println!(
                "{}",
                tauri::api::path::desktop_dir()
                    .unwrap_or(std::path::PathBuf::new())
                    .to_string_lossy()
            );
            println!(
                "{}",
                tauri::api::path::document_dir()
                    .unwrap_or(std::path::PathBuf::new())
                    .to_string_lossy()
            );
            println!(
                "{}",
                tauri::api::path::download_dir()
                    .unwrap_or(std::path::PathBuf::new())
                    .to_string_lossy()
            );
            println!(
                "{}",
                tauri::api::path::picture_dir()
                    .unwrap_or(std::path::PathBuf::new())
                    .to_string_lossy()
            );

            initialize_logging(app)?;
            create_app_directories(app)?;

            *app.state::<SettingsState>().0.write() = Settings::load_or_default(
                app.path_resolver()
                    .app_config_dir()
                    .unwrap()
                    .join("settings.json"),
            )?;

            *app.state::<WadHashtableState>().0.lock() = WadHashtable::new()?;

            Ok(())
        })
        .invoke_handler(tauri::generate_handler![
            expand_wad_tree_item,
            extract_mounted_wad,
            get_action_progress,
            get_app_directory,
            get_mounted_wad_directory_items,
            get_mounted_wad_directory_path_components,
            get_mounted_wads,
            get_settings,
            get_wad_hashtable_status,
            get_wad_items,
            load_wad_hashtables,
            mount_wads,
            move_mounted_wad,
            pick_directory,
            pick_file,
            select_wad_tree_item,
            unmount_wad,
            open_path,
            update_settings,
        ])
        .on_window_event(|event| {
            if let tauri::WindowEvent::CloseRequested { .. } = event.event() {
                let app_handle = event.window().app_handle();
                let settings = app_handle.state::<SettingsState>();
                let settings = settings.0.read();

                settings
                    .save(
                        app_handle
                            .path_resolver()
                            .app_config_dir()
                            .unwrap()
                            .join("settings.json"),
                    )
                    .expect("failed to save settings")
            }
        })
        .run(tauri::generate_context!())
        .expect("error while running tauri application");

    Ok(())
}

fn initialize_logging(app: &mut App) -> eyre::Result<()> {
    color_eyre::install()?;

    let appender = tracing_appender::rolling::never(
        app.path_resolver().app_data_dir().unwrap().join(LOGS_DIR),
        create_log_filename(),
    );
    let (non_blocking_appender, _guard) = tracing_appender::non_blocking(appender);

    let subscriber = tracing_subscriber::fmt()
        .with_file(true)
        .with_line_number(true)
        .with_ansi(false)
        .with_writer(stdout.and(non_blocking_appender))
        .finish();

    tracing::subscriber::with_default(subscriber, || {
        tracing::event!(tracing::Level::INFO, "Initialized logging");
        info!(
            "{}",
            app.path_resolver()
                .app_config_dir()
                .unwrap()
                .join(LOGS_DIR)
                .display()
        );
    });

    Ok(())
}

fn create_app_directories(app: &mut App) -> eyre::Result<()> {
    info!("creating app directories");
    try_create_dir(
        app.path_resolver()
            .app_data_dir()
            .unwrap()
            .join(WAD_HASHTABLES_DIR),
    )?;

    Ok(())
}
