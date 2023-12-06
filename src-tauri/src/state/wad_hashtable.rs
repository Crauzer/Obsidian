use crate::{api::hashtable::WadHashtableStatus, error::Result};
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

    pub fn items(&self) -> &HashMap<u64, Arc<str>> {
        &self.items
    }
    pub fn items_mut(&mut self) -> &mut HashMap<u64, Arc<str>> {
        &mut self.items
    }

    pub fn load_from_dir(&mut self, dir: impl AsRef<Path>) -> Result<()> {
        info!("loading wad hasthables from dir: {:?}", dir.as_ref());

        for wad_hashtable_entry in WalkDir::new(dir)
            .into_iter()
            .filter_entry(|x| x.file_type().is_file())
        {
            let wad_hashtable_entry = wad_hashtable_entry?;

            info!("loading wad hasthable: {:?}", wad_hashtable_entry.path());
            self.add_from_file(&mut File::open(wad_hashtable_entry.path())?)?;
        }

        self.is_loaded = true;

        Ok(())
    }

    pub fn add_from_file(&mut self, file: &mut File) -> Result<()> {
        let reader = BufReader::new(file);
        let mut lines = reader.lines();

        while let Some(Ok(line)) = lines.next() {
            let mut components = line.split(' ');

            let hash = components.next().expect("failed to read hash");
            let hash = u64::from_str_radix(hash, 16).expect("failed to convert hash");
            let path = itertools::join(components, " ");

            self.items.insert(hash, path.into());
        }

        Ok(())
    }

    pub fn status(&self) -> WadHashtableStatus {
        WadHashtableStatus {
            is_loaded: self.is_loaded,
        }
    }
}

pub struct WadHashtableState(pub Mutex<WadHashtable>);
