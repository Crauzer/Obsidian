mod extractor;

pub mod tree;

pub use extractor::*;
use serde::{Deserialize, Serialize};

#[derive(Serialize, Deserialize)]
pub enum WadChunkPreviewType {
    Image,
}
