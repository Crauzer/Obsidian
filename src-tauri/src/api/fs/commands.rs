use crate::api::error::ApiError;

use super::PickFileResponse;

#[tauri::command]
async fn pick_file() -> Result<PickFileResponse, ApiError> {
    let file = tauri::api::dialog::blocking::FileDialogBuilder::new().pick_file();

    file.map(|path| PickFileResponse {
        path: path.to_string_lossy().to_string(),
    })
    .ok_or(ApiError::from_message("failed to pick file"))
}
