use color_eyre::eyre::Context;

use crate::{
    api::error::ApiError,
    paths::SETTINGS_FILE,
    state::{Settings, SettingsState},
};

#[tauri::command]
pub async fn get_settings(settings: tauri::State<'_, SettingsState>) -> Result<Settings, String> {
    let settings = settings.0.read();

    Ok(settings.clone())
}

#[tauri::command]
pub async fn update_settings(
    settings: Settings,
    settings_state: tauri::State<'_, SettingsState>,
) -> Result<(), ApiError> {
    let mut settings_state = settings_state.0.write();

    tracing::info!("updating settings: {:?}", settings);

    *settings_state = settings;

    Ok(())
}
