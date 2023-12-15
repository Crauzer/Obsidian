use crate::api::error::ApiError;

use super::{PickDirectoryResponse, PickFileResponse};

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
