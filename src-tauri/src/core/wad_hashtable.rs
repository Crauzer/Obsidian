use std::{
    collections::HashMap,
    fs::File,
    io::{BufRead, BufReader},
    path::{Path, PathBuf},
    sync::Arc,
};

pub struct WadHashtable {
    items: HashMap<u64, Arc<str>>,
}

impl WadHashtable {
    pub fn new() -> Self {
        WadHashtable {
            items: HashMap::default(),
        }
    }

    pub fn items(&self) -> &HashMap<u64, Arc<str>> {
        &self.items
    }
    pub fn items_mut(&mut self) -> &mut HashMap<u64, Arc<str>> {
        &mut self.items
    }

    pub fn initialize(&mut self, files: &[impl AsRef<Path>]) -> Result<(), String> {
        for file in files {
            self.add_from_file(&mut File::open(file).map_err(|e| e.to_string())?)?;
        }

        Ok(())
    }

    pub fn add_from_file(&mut self, file: &mut File) -> Result<(), String> {
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
}
