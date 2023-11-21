use std::{fs::File, path::Path};

use parking_lot::RwLock;
use serde::{Deserialize, Serialize};

use crate::error::Result;

#[derive(Debug, Serialize, Deserialize, Clone)]
pub struct Settings {
    pub wad_hashtable_urls: Vec<String>,
}

impl Settings {
    pub fn load_or_default(location: impl AsRef<Path>) -> Result<Self> {
        match File::open(location) {
            Ok(file) => Ok(serde_json::from_reader::<File, Self>(file)?),
            Err(_) => Ok(Self::default()),
        }
    }

    pub fn save(&self, path: impl AsRef<Path>) -> Result<()> {
        serde_json::to_writer_pretty(File::create(path)?, self)?;
        Ok(())
    }
}

impl Default for Settings {
    fn default() -> Self {
        Self {
            wad_hashtable_urls: vec![
                "https://raw.githubusercontent.com/CommunityDragon/CDTB/master/cdragontoolbox/hashes.game.txt"
                    .into(),
                "https://raw.githubusercontent.com/CommunityDragon/CDTB/master/cdragontoolbox/hashes.lcu.txt"
                    .into(),
            ],
        }
    }
}

pub struct SettingsState(pub RwLock<Settings>);
