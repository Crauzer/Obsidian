use std::{fs::File, path::Path};

use color_eyre::eyre;
use parking_lot::RwLock;
use serde::{Deserialize, Serialize};
use tracing::info;

#[derive(Debug, Serialize, Deserialize, Clone)]
pub struct Settings {
    pub default_mount_directory: Option<String>,
    pub default_extraction_directory: Option<String>,
}

impl Settings {
    pub fn load_or_default(location: impl AsRef<Path>) -> eyre::Result<Self> {
        info!("loading settings...");

        match File::open(location) {
            Ok(file) => Ok(serde_json::from_reader::<File, Self>(file)?),
            Err(_) => Ok(Self::default()),
        }
    }

    pub fn save(&self, path: impl AsRef<Path>) -> eyre::Result<()> {
        serde_json::to_writer_pretty(File::create(path)?, self)?;
        Ok(())
    }
}

impl Default for Settings {
    fn default() -> Self {
        Self {
            default_mount_directory: None,
            default_extraction_directory: None,
        }
    }
}

pub struct SettingsState(pub RwLock<Settings>);
