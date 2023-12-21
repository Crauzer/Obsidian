mod commands;

pub use commands::*;
use serde::{Deserialize, Serialize};

#[derive(Debug, Serialize, Deserialize, Clone)]
pub struct PickFileResponse {
    path: String,
}

#[derive(Debug, Serialize, Deserialize, Clone)]
pub struct PickDirectoryResponse {
    path: String,
}

#[derive(Debug, Serialize, Deserialize, Clone)]
pub struct AppDirectoryResponse {
    app_directory: String,
}
