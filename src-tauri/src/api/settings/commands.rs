use crate::{
    api::error::ApiError,
    paths::SETTINGS_FILE,
    state::{Settings, SettingsState},
};
use tauri::Manager;

#[tauri::command]
pub async fn get_settings(settings: tauri::State<'_, SettingsState>) -> Result<Settings, String> {
    let settings = settings.0.read();

    Ok(settings.clone())
}

#[tauri::command]
pub async fn update_settings(
    app: tauri::AppHandle,
    settings: Settings,
    settings_state: tauri::State<'_, SettingsState>,
) -> Result<(), ApiError> {
    let mut settings_state = settings_state.0.write();

    tracing::info!("updating settings: {:?}", settings);

    *settings_state = settings;

    // Save to the app config directory (not the project directory)
    let settings_path = app.path().app_config_dir().unwrap().join(SETTINGS_FILE);
    settings_state.save(settings_path)?;

    Ok(())
}
