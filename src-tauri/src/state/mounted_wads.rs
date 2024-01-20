use indexmap::IndexMap;
use parking_lot::Mutex;
use std::{collections::HashMap, fs::File, sync::Arc};

use tracing::info;
use uuid::Uuid;

use crate::core::wad::{
    tree::{WadTree, WadTreeError},
    Wad,
};

use super::WadHashtable;

pub struct MountedWads {
    wad_trees: IndexMap<Uuid, WadTree>,
    wads: HashMap<Uuid, Wad<File>>,
}

impl MountedWads {
    pub fn new() -> Self {
        Self {
            wad_trees: IndexMap::default(),
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

    pub fn get_wad(&self, id: Uuid) -> Option<(&WadTree, &Wad<File>)> {
        match (self.wad_trees.get(&id), self.wads.get(&id)) {
            (Some(wad_tree), Some(wad)) => Some((wad_tree, wad)),
            _ => None,
        }
    }
    pub fn get_wad_mut(&mut self, id: Uuid) -> Option<(&mut WadTree, &mut Wad<File>)> {
        match (self.wad_trees.get_mut(&id), self.wads.get_mut(&id)) {
            (Some(wad_tree), Some(wad)) => Some((wad_tree, wad)),
            _ => None,
        }
    }

    pub fn wad_trees(&self) -> &IndexMap<Uuid, WadTree> {
        &self.wad_trees
    }
    pub fn wad_trees_mut(&mut self) -> &mut IndexMap<Uuid, WadTree> {
        &mut self.wad_trees
    }

    pub fn wads(&self) -> &HashMap<Uuid, Wad<File>> {
        &self.wads
    }
    pub fn wads_mut(&mut self) -> &mut HashMap<Uuid, Wad<File>> {
        &mut self.wads
    }
}

pub struct MountedWadsState(pub Mutex<MountedWads>);
