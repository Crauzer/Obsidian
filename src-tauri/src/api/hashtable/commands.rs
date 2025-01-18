use crate::api::error::{ApiErrorBuilder, ApiErrorExtension};
use crate::api::ApiResult;
use crate::{api::error::ApiError, state::WadHashtableState};
use color_eyre::eyre::{self, Context};
use octocrab::models::repos::ContentItems;
use tauri::Manager as _;
use tracing::info;
use walkdir::WalkDir;

use super::WadHashtableStatus;

#[tauri::command]
pub async fn get_wad_hashtable_status(
    wad_hashtable: tauri::State<'_, WadHashtableState>,
) -> ApiResult<WadHashtableStatus> {
    Ok(wad_hashtable.0.lock().status())
}

#[tauri::command]
pub async fn load_wad_hashtables(
    app: tauri::AppHandle,
    wad_hashtable: tauri::State<'_, WadHashtableState>,
) -> std::result::Result<(), ApiError> {
    info!("loading wad hashtables");

    let wad_hashtables_dir = app
        .path()
        .app_data_dir()
        .map_err(|_| ApiError::from_message("failed to get app data dir"))?
        .join("wad_hashtables");

    if wad_hashtables_dir
        .read_dir()
        .wrap_err("failed to read dir")?
        .next()
        .is_none()
    {
        return Err(ApiErrorBuilder::new()
            .message("Wad hashtables missing")
            .extend(ApiErrorExtension::WadHashtablesMissing)
            .build());
    }

    wad_hashtable.0.lock().items_mut().clear();
    wad_hashtable.0.lock().add_from_dir(wad_hashtables_dir)?;

    Ok(())
}

async fn get_file_content(path: impl AsRef<str>) -> eyre::Result<ContentItems> {
    let path = path.as_ref();
    info!("getting github file content: {}", path);

    Ok(octocrab::instance()
        .repos("CommunityDragon", "CDTB")
        .get_content()
        .path(path)
        .send()
        .await?)
}
