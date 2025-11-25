use std::{fs::File, path::Path};

use color_eyre::eyre;
use parking_lot::RwLock;
use serde::{Deserialize, Serialize};
use tracing::info;

#[derive(Debug, Serialize, Deserialize, Clone)]
#[serde(rename_all = "camelCase")]
pub struct Settings {
    pub open_directory_after_extraction: bool,
    pub default_mount_directory: Option<String>,
    pub default_extraction_directory: Option<String>,
    pub league_directory: Option<String>,
}

impl Settings {
    pub fn load_or_default(location: impl AsRef<Path>) -> Self {
        info!("loading settings...");

        let Ok(file) = File::open(location) else {
            tracing::error!("failed to open settings file, using default settings...");
            return Self::default();
        };

        match serde_json::from_reader::<File, Self>(file) {
            Ok(settings) => settings,
            Err(error) => {
                tracing::error!(
                    "failed to parse settings file, using default settings | error: {}",
                    error
                );
                Self::default()
            }
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
            open_directory_after_extraction: true,
            default_mount_directory: None,
            default_extraction_directory: None,
            league_directory: None,
        }
    }
}

pub struct SettingsState(pub RwLock<Settings>);
