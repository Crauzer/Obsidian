use parking_lot::Mutex;
use std::{collections::HashMap, fs::File, sync::Arc};

use tracing::info;
use uuid::Uuid;

use crate::core::{
    wad::{
        tree::{WadTree, WadTreeError},
        Wad,
    },
    wad_hashtable::WadHashtable,
};

pub struct MountedWads {
    wad_trees: HashMap<Uuid, WadTree>,
    wads: HashMap<Uuid, Wad<File>>,
}

impl MountedWads {
    pub fn new() -> Self {
        Self {
            wad_trees: HashMap::default(),
            wads: HashMap::default(),
        }
    }

    pub fn mount_wad(
        &mut self,
        wad: Wad<File>,
        wad_path: Arc<str>,
        hashtable: &WadHashtable,
    ) -> Result<Uuid, WadTreeError> {
        let id = Uuid::new_v4();

        info!("mounting wad (id: {}, path: {})", id, wad_path);
        info!("creating wad tree (id: {})", id);

        self.wad_trees
            .insert(id, WadTree::from_wad(&wad, id, wad_path, hashtable)?);
        self.wads.insert(id, wad);

        Ok(id)
    }

    pub fn unmount_wad(&mut self, id: Uuid) {
        info!("unmounting wad (id: {})", id);

        let _ = self.wad_trees.remove(&id);
        let _ = self.wads.remove(&id);
    }

    pub fn wad_trees(&self) -> &HashMap<Uuid, WadTree> {
        &self.wad_trees
    }
    pub fn wad_trees_mut(&mut self) -> &mut HashMap<Uuid, WadTree> {
        &mut self.wad_trees
    }
}

pub struct MountedWadsState(pub Mutex<MountedWads>);
