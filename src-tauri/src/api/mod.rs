use serde::Serialize;
use uuid::Uuid;

pub mod actions;
pub mod error;
pub mod hashtable;
pub mod settings;
pub mod wad;

#[derive(Debug, Serialize)]
pub struct OpenWadResponse {
    wad_id: Uuid,
}

impl OpenWadResponse {
    pub fn new(wad_id: Uuid) -> Self {
        Self { wad_id }
    }
}
