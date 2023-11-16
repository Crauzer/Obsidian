use tauri::Runtime;
use tauri_plugin_store::{with_store, StoreCollection};

use crate::api::error::ApiError;

#[tauri::command]
async fn load_hashtables<R: Runtime>(
    app: tauri::AppHandle<R>,
    stores: tauri::State<'_, StoreCollection<R>>,
) -> Result<(), ApiError> {
    Ok(())
}
