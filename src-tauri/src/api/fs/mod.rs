mod commands;

pub use commands::*;
use serde::{Deserialize, Serialize};

#[derive(Debug, Serialize, Deserialize, Clone)]
pub struct PickFileResponse {
    path: String,
}
