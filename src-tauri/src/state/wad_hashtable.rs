use crate::api::hashtable::WadHashtableStatus;
use color_eyre::eyre::{self, eyre, Result};
use parking_lot::Mutex;
use serde::{Deserialize, Serialize};
use std::{
    collections::HashMap,
    fs::File,
    io::{BufRead, BufReader},
    path::Path,
    sync::Arc,
};
use tracing::info;
use walkdir::WalkDir;

#[derive(Debug, Clone, Default)]
pub struct WadHashtable {
    is_loaded: bool,
    items: HashMap<u64, Arc<str>>,
}

impl WadHashtable {
    pub fn new() -> Result<Self> {
        Ok(WadHashtable {
            is_loaded: false,
            items: HashMap::default(),
        })
    }

    pub fn resolve_path(&self, path_hash: u64) -> Arc<str> {
        self.items
            .get(&path_hash)
            .map(|x| x.clone())
            .unwrap_or_else(|| format!("{:#0x}", path_hash).into())
    }

    pub fn add_from_dir(&mut self, dir: impl AsRef<Path>) -> eyre::Result<()> {
        info!("loading wad hasthables from dir: {:?}", dir.as_ref());

        for wad_hashtable_entry in WalkDir::new(dir).into_iter().filter_map(|x| x.ok()) {
            if !wad_hashtable_entry.file_type().is_file() {
                continue;
            }

            info!("loading wad hasthable: {:?}", wad_hashtable_entry.path());
            self.add_from_file(&mut File::open(wad_hashtable_entry.path())?)?;
        }

        self.is_loaded = true;

        Ok(())
    }

    pub fn add_from_file(&mut self, file: &mut File) -> eyre::Result<()> {
        let reader = BufReader::new(file);
        let mut lines = reader.lines();

        while let Some(Ok(line)) = lines.next() {
            let mut components = line.split(' ');

            let hash = components.next().ok_or(eyre!("failed to read hash"))?;
            let hash = u64::from_str_radix(hash, 16).expect("failed to convert hash");
            let path = itertools::join(components, " ");

            self.items.insert(hash, path.into());
        }

        Ok(())
    }

    pub fn items(&self) -> &HashMap<u64, Arc<str>> {
        &self.items
    }
    pub fn items_mut(&mut self) -> &mut HashMap<u64, Arc<str>> {
        &mut self.items
    }

    pub fn status(&self) -> WadHashtableStatus {
        WadHashtableStatus {
            is_loaded: self.is_loaded,
        }
    }
}

pub struct WadHashtableState(pub Mutex<WadHashtable>);
