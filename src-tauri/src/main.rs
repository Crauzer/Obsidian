// Prevents additional console window on Windows in release, DO NOT REMOVE!!
#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]
#![feature(io_error_more)]
#![feature(anonymous_lifetime_in_impl_trait)]
#![feature(let_chains)]

#[macro_use]
extern crate lazy_static;

mod api;
mod core;
mod state;
mod utils;

use crate::{
    api::{
        actions::get_action_progress,
        fs::{get_app_directory, open_path, pick_directory, pick_file},
        hashtable::{get_wad_hashtable_status, load_wad_hashtables},
        settings::{get_settings, update_settings},
        wad::{
            extract_mounted_wad, extract_wad_items, get_mounted_wad_directory_path_components,
            get_mounted_wads, get_wad_parent_items, mount_wads, move_mounted_wad, search_wad,
            unmount_wad, update_mounted_wad_item_selection,
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

mod error;
mod paths;

lazy_static! {
    static ref LOG_GUARD: Mutex<Option<tracing_appender::non_blocking::WorkerGuard>> =
        Mutex::new(None);
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
            LOG_GUARD.lock().replace(initialize_logging(&app.handle())?);

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
            extract_wad_items,
            get_action_progress,
            get_app_directory,
            get_mounted_wad_directory_path_components,
            get_mounted_wads,
            get_settings,
            get_wad_hashtable_status,
            get_wad_parent_items,
            load_wad_hashtables,
            mount_wads,
            move_mounted_wad,
            open_path,
            pick_directory,
            pick_file,
            search_wad,
            unmount_wad,
            update_mounted_wad_item_selection,
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

fn initialize_logging(
    app_handle: &AppHandle,
) -> eyre::Result<tracing_appender::non_blocking::WorkerGuard> {
    color_eyre::install()?;

    let appender = tracing_appender::rolling::hourly(
        app_handle
            .path_resolver()
            .app_data_dir()
            .unwrap()
            .join(LOGS_DIR),
        "obsidian",
    );
    let (non_blocking_appender, guard) = tracing_appender::non_blocking(appender);

    let subscriber = tracing_subscriber::fmt()
        .with_file(true)
        .with_line_number(true)
        .with_ansi(false)
        .with_writer(stdout.and(non_blocking_appender))
        .finish();

    tracing::subscriber::set_global_default(subscriber)?;

    Ok(guard)
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
