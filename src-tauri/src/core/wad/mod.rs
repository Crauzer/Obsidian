mod extractor;

pub mod tree;

pub use extractor::*;
use serde::{Deserialize, Serialize};

#[derive(Serialize, Deserialize)]
#[serde(rename_all = "snake_case")]
pub enum WadChunkPreviewType {
    Image,
}
