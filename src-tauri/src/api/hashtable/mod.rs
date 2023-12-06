mod commands;

pub use commands::*;
use serde::{Deserialize, Serialize};

#[derive(Debug, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct WadHashtableStatus {
    pub is_loaded: bool,
}
