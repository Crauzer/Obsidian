use std::io;

use thiserror::Error;

#[derive(Error, Debug)]
pub enum WadError {
    #[error("invalid header")]
    InvalidHeader { expected: String, actual: String },

    #[error("invalid version {major:?}.{minor:?}")]
    InvalidVersion { major: u8, minor: u8 },

    #[error("invalid chunk compression: {compression:?}")]
    InvalidChunkCompression { compression: u8 },

    #[error("duplicate chunk: {path_hash:#08x}")]
    DuplicateChunk { path_hash: u64 },

    #[error("failed to decompress chunk (path: {path_hash:#08x}, reason: {reason:?})")]
    DecompressionFailure { path_hash: u64, reason: String },

    #[error("io error")]
    IoError(#[from] io::Error),

    #[error("error: `{0}`")]
    Other(String),
}
