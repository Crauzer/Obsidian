use color_eyre::eyre::Context;

use crate::api::error::ApiError;

use super::{AppDirectoryResponse, PickDirectoryResponse, PickFileResponse};

#[tauri::command]
pub async fn pick_file() -> Result<PickFileResponse, ApiError> {
    let file = tauri::api::dialog::blocking::FileDialogBuilder::new().pick_file();

    file.map(|path| PickFileResponse {
        path: path.to_string_lossy().to_string(),
    })
    .ok_or(ApiError::from_message("failed to pick file"))
}

#[tauri::command]
pub async fn pick_directory() -> Result<PickDirectoryResponse, ApiError> {
    let directory = tauri::api::dialog::blocking::FileDialogBuilder::new().pick_folder();

    directory
        .map(|path| PickDirectoryResponse {
            path: path.to_string_lossy().to_string(),
        })
        .ok_or(ApiError::from_message("failed to pick directory"))
}

#[tauri::command]
pub async fn get_app_directory(app: tauri::AppHandle) -> Result<AppDirectoryResponse, ApiError> {
    if let Some(app_config_dir) = app.path_resolver().app_config_dir() {
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
