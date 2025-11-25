// Prevents additional console window on Windows in release, DO NOT REMOVE!!
#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]
#![feature(io_error_more)]
#![feature(anonymous_lifetime_in_impl_trait)]

#[macro_use]
extern crate lazy_static;

mod api;
mod core;
mod state;
mod utils;

use crate::{
    paths::WAD_HASHTABLES_DIR,
    state::WadHashtable,
    utils::fs::try_create_dir,
};
use color_eyre::eyre;
use parking_lot::{lock_api::RwLock, Mutex};
use paths::{LOGS_DIR, SETTINGS_FILE};
use state::{
    ActionsState, GameExplorer, GameExplorerState, MountedWads, MountedWadsState, Settings,
    SettingsState, WadHashtableState,
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
        .plugin(tauri_plugin_dialog::init())
        .plugin(tauri_plugin_http::init())
        .manage(MountedWadsState(Mutex::new(MountedWads::new())))
        .manage(SettingsState(RwLock::new(Settings::default())))
        .manage(WadHashtableState(Mutex::new(WadHashtable::default())))
        .manage(ActionsState(RwLock::new(HashMap::default())))
        .manage(GameExplorerState(Mutex::new(GameExplorer::new())))
        .setup(|app| {
            LOG_GUARD.lock().replace(initialize_logging(app.handle())?);

            create_app_directories(app)?;

            *app.state::<SettingsState>().0.write() =
                Settings::load_or_default(app.path().app_config_dir().unwrap().join(SETTINGS_FILE));

            *app.state::<WadHashtableState>().0.lock() = WadHashtable::new()?;

            Ok(())
        })
        .invoke_handler(generate_command_handler!())
        .run(tauri::generate_context!())
        .expect("error while running tauri application");

    Ok(())
}

fn initialize_logging(
    app_handle: &AppHandle,
) -> eyre::Result<tracing_appender::non_blocking::WorkerGuard> {
    color_eyre::install()?;

    let appender = tracing_appender::rolling::hourly(
        app_handle.path().app_data_dir().unwrap().join(LOGS_DIR),
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
    try_create_dir(app.path().app_data_dir().unwrap().join(WAD_HASHTABLES_DIR))?;

    Ok(())
}
