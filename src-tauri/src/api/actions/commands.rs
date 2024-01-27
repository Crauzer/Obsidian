use crate::{api::error::ApiError, state::ActionsState};

use super::ActionProgress;

#[tauri::command]
pub async fn get_action_progress(
    action_id: String,
    actions: tauri::State<'_, ActionsState>,
) -> Result<ActionProgress, ApiError> {
    let action_id = uuid::Uuid::parse_str(&action_id)
        .map_err(|_| ApiError::from_message("failed to parse action_id"))?;

    actions.0.read().get(&action_id).map_or(
        Err(ApiError::from_message("failed to find action")),
        |action| Ok(action.progress().clone()),
    )
}
