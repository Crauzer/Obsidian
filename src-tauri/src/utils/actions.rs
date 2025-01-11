use color_eyre::eyre::{self, Context};
use tauri::{Emitter, Manager};
use uuid::Uuid;

use crate::api::actions::ActionProgressEvent;

pub fn emit_action_progress(
    app_handle: &tauri::AppHandle,
    action_id: Uuid,
    progress: f64,
    message: Option<String>,
) -> eyre::Result<()> {
    app_handle
        .emit(
            &action_id.to_string(),
            ActionProgressEvent { progress, message },
        )
        .wrap_err(format!(
            "failed to emit action progress (action_id = {})",
            action_id
        ))
}
