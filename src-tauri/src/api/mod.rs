use serde::Serialize;
use uuid::Uuid;

pub mod actions;
pub mod error;
pub mod fs;
pub mod hashtable;
pub mod settings;
pub mod wad;

pub type ApiResult<T> = Result<T, error::ApiError>;
