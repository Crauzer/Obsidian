use color_eyre::eyre::Context;
use tauri::{Manager as _, Runtime};
use tauri_plugin_dialog::DialogExt as _;

use crate::api::error::ApiError;

use super::{AppDirectoryResponse, PickDirectoryResponse, PickFileResponse};

#[tauri::command]
pub async fn pick_file<R: Runtime>(app: tauri::AppHandle<R>) -> Result<PickFileResponse, ApiError> {
    let file = app.dialog().file().blocking_pick_file();

    file.map(|path| PickFileResponse {
        path: path.to_string(),
    })
    .ok_or(ApiError::from_message("failed to pick file"))
}

#[tauri::command]
pub async fn pick_directory(
    app: tauri::AppHandle,
    initial_directory: Option<String>,
) -> Result<PickDirectoryResponse, ApiError> {
    let mut dialog = app.dialog().file();

    if let Some(initial_directory) = initial_directory {
        dialog = dialog.set_directory(initial_directory);
    }

    dialog
        .blocking_pick_folder()
        .map(|path| PickDirectoryResponse {
            path: path.to_string(),
        })
        .ok_or(ApiError::from_message("failed to pick directory"))
}

#[tauri::command]
pub async fn get_app_directory(app: tauri::AppHandle) -> Result<AppDirectoryResponse, ApiError> {
    if let Ok(app_config_dir) = app.path().app_config_dir() {
        if let Some(app_config_dir) = app_config_dir.to_str() {
            return Ok(AppDirectoryResponse {
                app_directory: app_config_dir.to_string(),
            });
        }
    }

    Err(ApiError::from_message("failed to get app directory"))
}

#[tauri::command]
pub async fn open_path(path: String) -> Result<(), ApiError> {
    open::that(path).wrap_err("failed to open path")?;

    Ok(())
}
