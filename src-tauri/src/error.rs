#[derive(thiserror::Error, Debug)]
pub enum Error {
    #[error("walkdir error: {0}")]
    Walkdir(#[from] walkdir::Error),
    #[error("io error: {0}")]
    Io(#[from] std::io::Error),
    #[error("serialization error: {0}")]
    Serde(#[from] serde_json::Error),
    #[error("github error: {0}")]
    Github(#[from] octocrab::Error),
    #[error("http error: {0}")]
    Http(#[from] reqwest::Error),
    #[error("wad error: {0}")]
    Wad(#[from] crate::core::wad::WadError),
    #[error("error: {0}")]
    Other(#[from] Box<dyn std::error::Error + Send + Sync + 'static>),
}
