mod commands;

pub use commands::*;
use serde::{Deserialize, Serialize};
use uuid::Uuid;

#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(rename_all = "camelCase", tag = "kind")]
pub enum ActionProgress {
    #[serde(rename_all = "camelCase")]
    Progress {
        id: Uuid,
        value: f64,
        message: Option<String>,
    },
    Finished {
        id: Uuid,
    },
}

#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct ActionProgressEvent {
    pub progress: f64,
    pub message: Option<String>,
}
