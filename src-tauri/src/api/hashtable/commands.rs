use crate::api::ApiResult;
use crate::error::Result;
use crate::{
    api::{actions::ActionProgressEvent, error::ApiError},
    state::{WadHashtableChecksums, WadHashtableState},
    utils::http::download_file,
};
use octocrab::models::repos::ContentItems;
use tauri::Manager;
use tracing::info;

use super::WadHashtableStatus;

const GAME_HASHES_PATH: &str = "cdragontoolbox/hashes.game.txt";
const LCU_HASHES_PATH: &str = "cdragontoolbox/hashes.lcu.txt";

#[tauri::command]
pub async fn get_wad_hashtable_status(
    wad_hashtable: tauri::State<'_, WadHashtableState>,
) -> ApiResult<WadHashtableStatus> {
    Ok(wad_hashtable.0.lock().status())
}

#[tauri::command]
pub async fn load_wad_hashtables(
    app: tauri::AppHandle,
    action_id: String,
    wad_hashtable: tauri::State<'_, WadHashtableState>,
) -> std::result::Result<(), ApiError> {
    info!("loading wad hashtables");
    info!("{:#?}", app.path_resolver().app_data_dir());
    info!(
        "{:#?}",
        app.path_resolver()
            .app_data_dir()
            .unwrap()
            .join("wad_hashtables")
    );

    let wad_hashtables_dir = app
        .path_resolver()
        .app_data_dir()
        .ok_or(ApiError::from_message("failed to get app data dir"))?
        .join("wad_hashtables");

    let mut wad_hashtable = wad_hashtable.0.lock();
    wad_hashtable.load_from_dir(wad_hashtables_dir)?;
    wad_hashtable.is_update_available = check_for_update(wad_hashtable.checksums()).await?;

    Ok(())
}

#[tauri::command]
pub async fn update_wad_hashtables() {}

async fn download_wad_hashtables(app: &tauri::AppHandle, action_id: &str) -> Result<()> {
    let game_content = &get_file_content(GAME_HASHES_PATH).await?.items[0];
    let lcu_content = &get_file_content(LCU_HASHES_PATH).await?.items[0];

    download_file(
        game_content.download_url.as_ref().unwrap(),
        "C:/Users/Filip/hashes.game.txt",
        |downloaded_bytes, total_size| {
            let progress = downloaded_bytes as f64 / total_size as f64;
            app.emit_all(
                &action_id,
                ActionProgressEvent {
                    progress: progress * 100.0,
                    message: Some("C:/Users/Filip/hashes.game.txt".to_string()),
                },
            );
            info!("{}", downloaded_bytes);
        },
    )
    .await?;

    Ok(())
}

pub async fn check_for_update(checksums: &WadHashtableChecksums) -> Result<bool> {
    info!("checking for wad hashtable update");

    let game_content = &get_file_content(GAME_HASHES_PATH).await?.items[0];
    let lcu_content = &get_file_content(LCU_HASHES_PATH).await?.items[0];

    Ok(game_content.sha != checksums.game || lcu_content.sha != checksums.lcu)
}

async fn get_file_content(path: impl AsRef<str>) -> Result<ContentItems> {
    let path = path.as_ref();
    info!("getting github file content: {}", path);

    Ok(octocrab::instance()
        .repos("CommunityDragon", "CDTB")
        .get_content()
        .path(path)
        .send()
        .await?)
}
