mod commands;

pub use commands::*;
use serde::{Deserialize, Serialize};
use uuid::Uuid;

#[derive(Debug, Clone, Serialize, Deserialize, Copy)]
#[serde(rename_all = "camelCase", tag = "kind")]
pub enum ActionProgress {
    #[serde(rename_all = "camelCase")]
    Progress {
        id: Uuid,
        value: f64,
    },
    Finished {
        id: Uuid,
    },
}
