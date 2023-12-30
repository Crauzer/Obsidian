// Prevents additional console window on Windows in release, DO NOT REMOVE!!
#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]
#![feature(io_error_more)]

mod api;
mod core;
mod state;
mod utils;

use crate::api::wad::get_mounted_wad_directory_items;
use crate::api::wad::get_mounted_wads;
use crate::api::wad::get_wad_items;
use crate::api::wad::unmount_wad;
use crate::{
    api::{
        actions::get_action_progress,
        fs::{get_app_directory, open_path, pick_directory, pick_file},
        hashtable::{get_wad_hashtable_status, load_wad_hashtables},
        settings::{get_settings, update_settings},
        wad::{
            extract_mounted_wad, get_mounted_wad_directory_path_components, mount_wads,
            move_mounted_wad,
        },
    },
    paths::WAD_HASHTABLES_DIR,
    state::WadHashtable,
    utils::fs::try_create_dir,
};
use color_eyre::eyre;
use parking_lot::{lock_api::RwLock, Mutex};
use paths::LOGS_DIR;
use state::{
    ActionsState, MountedWads, MountedWadsState, Settings, SettingsState, WadHashtableState,
};
use std::{collections::HashMap, io::stdout};
use tauri::{App, AppHandle, Manager};
use tracing::info;
use tracing_subscriber::fmt::writer::MakeWriterExt;
use utils::log::create_log_filename;

mod error;
mod paths;

fn main() -> eyre::Result<()> {
    tauri::Builder::default()
        .plugin(tauri_plugin_window_state::Builder::default().build())
        .plugin(tauri_plugin_upload::init())
        .manage(MountedWadsState(Mutex::new(MountedWads::new())))
        .manage(SettingsState(RwLock::new(Settings::default())))
        .manage(WadHashtableState(Mutex::new(WadHashtable::default())))
        .manage(ActionsState(RwLock::new(HashMap::default())))
        .setup(|app| {
            initialize_logging(&app.handle())?;
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

fn initialize_logging(app_handle: &AppHandle) -> eyre::Result<()> {
    color_eyre::install()?;

    let appender = tracing_appender::rolling::never(
        app_handle
            .path_resolver()
            .app_data_dir()
            .unwrap()
            .join(LOGS_DIR),
        create_log_filename(),
    );
    let (non_blocking_appender, _guard) = tracing_appender::non_blocking(appender);

    let subscriber = tracing_subscriber::fmt()
        .with_file(true)
        .with_line_number(true)
        .with_ansi(false)
        .with_writer(stdout.and(non_blocking_appender))
        .finish();

    tracing::subscriber::set_global_default(subscriber)?;

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
