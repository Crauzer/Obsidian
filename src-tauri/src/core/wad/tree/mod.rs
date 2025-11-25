use std::{
    collections::HashMap,
    io::{Read, Seek},
    sync::Arc,
};

use camino::{Utf8Component, Utf8Path};
use league_toolkit::{
    file::LeagueFileKind,
    wad::{Wad, WadChunk, WadDecoder, WadError},
};
use thiserror::Error;
use tracing::info;
use uuid::Uuid;

mod item;

pub use item::*;

use crate::state::WadHashtable;

#[derive(Error, Debug)]
pub enum WadTreeError {
    #[error("invalid item name (chunk_path: {chunk_path:#0x})")]
    InvalidItemName { chunk_path: u64 },

    #[error("invalid item path (item_path: {item_path})")]
    InvalidItemPath { item_path: String },

    #[error("failed to create item (item_path: {item_path})")]
    ItemCreationFailure { item_path: String },

    #[error("existing file: (file_path: {file_path})")]
    ExistingFile { file_path: String },

    #[error("parent does not exist: (parent_path: {parent_path})")]
    ParentDoesNotExist { parent_path: String },

    #[error("item does not exist: (item_id: {item_id})")]
    ItemDoesNotExist { item_id: Uuid },

    #[error("not a directory: (item_id: {item_id})")]
    NotADirectory { item_id: Uuid },

    #[error("wad error: {0}")]
    WadError(#[from] WadError),

    #[error("{message}")]
    Other { message: String },
}

#[derive(Debug, PartialEq, Eq, PartialOrd, Ord, Clone, Hash)]
pub struct WadItemKindPath {
    pub kind: WadTreeItemKind,
    pub path: Arc<str>,
}

#[derive(Debug)]
pub struct WadTree {
    wad_id: Uuid,
    wad_path: Arc<str>,
    items: Vec<Uuid>,
    item_storage: HashMap<Uuid, WadTreeItem>,
    chunk_item_ids: HashMap<WadItemKindPath, Uuid>,
}

impl WadTree {
    /// Create a WadTree from a WAD file
    /// This version attempts to identify unknown file types by reading chunk data
    pub fn from_wad<TSource>(
        wad: &mut Wad<TSource>,
        wad_id: Uuid,
        wad_path: impl Into<Arc<str>>,
        hashtable: &WadHashtable,
    ) -> Result<WadTree, WadTreeError>
    where
        TSource: Read + Seek,
    {
        info!("creating wad tree for wad (wad_id: {})", wad_id);

        let (mut decoder, chunks) = wad.decode();
        let chunk_count = chunks.len();

        // Pre-allocate storage
        let mut tree = WadTree {
            wad_id,
            wad_path: wad_path.into(),
            items: Vec::with_capacity(64), // Root items are usually fewer
            item_storage: HashMap::with_capacity(chunk_count * 2), // Files + directories
            chunk_item_ids: HashMap::with_capacity(chunk_count * 2),
        };

        // Create all items
        for (chunk_path_hash, chunk) in chunks.iter() {
            let path = match hashtable.items().get(chunk_path_hash) {
                Some(path) => path.clone(),
                None => Self::guess_chunk_path_with_decompress(*chunk_path_hash, chunk, &mut decoder)?,
            };

            tree.create_item_from_chunk(chunk, path)?;
        }

        // Sort everything
        tree.sort();

        Ok(tree)
    }

    /// Create a WadTree without decompressing unknown chunks (faster)
    /// Unknown paths will just be shown as hex hashes without file extensions
    pub fn from_wad_fast<TSource>(
        wad: &mut Wad<TSource>,
        wad_id: Uuid,
        wad_path: impl Into<Arc<str>>,
        hashtable: &WadHashtable,
    ) -> Result<WadTree, WadTreeError>
    where
        TSource: Read + Seek,
    {
        info!("creating wad tree (fast) for wad (wad_id: {})", wad_id);

        let (_, chunks) = wad.decode();
        let chunk_count = chunks.len();

        // Pre-allocate storage
        let mut tree = WadTree {
            wad_id,
            wad_path: wad_path.into(),
            items: Vec::with_capacity(64),
            item_storage: HashMap::with_capacity(chunk_count * 2),
            chunk_item_ids: HashMap::with_capacity(chunk_count * 2),
        };

        // Create all items - use hashtable only, no decompression
        for (chunk_path_hash, chunk) in chunks.iter() {
            let path: Arc<str> = match hashtable.items().get(chunk_path_hash) {
                Some(path) => path.clone(),
                None => format!("{:#0x}", chunk_path_hash).into(),
            };

            tree.create_item_from_chunk(chunk, path)?;
        }

        // Sort everything
        tree.sort();

        Ok(tree)
    }

    /// Guess chunk path by decompressing and identifying file type from magic bytes
    fn guess_chunk_path_with_decompress<TSource: Read + Seek>(
        chunk_path_hash: u64,
        chunk: &WadChunk,
        decoder: &mut WadDecoder<TSource>,
    ) -> Result<Arc<str>, WadError> {
        let data = decoder.load_chunk_decompressed(chunk)?;
        let file_kind = LeagueFileKind::identify_from_bytes(&data);

        match file_kind {
            LeagueFileKind::Unknown => Ok(format!("{:#0x}", chunk_path_hash).into()),
            _ => Ok(format!("{:#0x}.{}", chunk_path_hash, file_kind.extension().unwrap()).into()),
        }
    }

    pub fn sort(&mut self) {
        // Sort root items in-place
        let item_storage = &self.item_storage;
        self.items.sort_unstable_by(|a, b| {
            let a = item_storage.get(a);
            let b = item_storage.get(b);
            compare_items(a, b)
        });

        // Collect directory IDs to sort
        let dir_ids: Vec<Uuid> = self
            .item_storage
            .iter()
            .filter_map(|(id, item)| {
                if matches!(item, WadTreeItem::Directory(_)) {
                    Some(*id)
                } else {
                    None
                }
            })
            .collect();

        // Sort each directory's children
        for dir_id in dir_ids {
            // Extract, sort, reinsert
            if let Some(WadTreeItem::Directory(mut dir)) = self.item_storage.remove(&dir_id) {
                let item_storage = &self.item_storage;
                dir.items.sort_unstable_by(|a, b| {
                    let a = item_storage.get(a);
                    let b = item_storage.get(b);
                    compare_items(a, b)
                });
                self.item_storage.insert(dir_id, WadTreeItem::Directory(dir));
            }
        }
    }

    pub fn create_item_from_chunk(
        &mut self,
        chunk: &WadChunk,
        path: Arc<str>,
    ) -> Result<(), WadTreeError> {
        let path_ref = Utf8Path::new(path.as_ref());
        let mut path_components = path_ref.components().peekable();

        let mut current_parent_id: Option<Uuid> = None;
        let mut current_path = String::new();

        while let Some(path_component) = path_components.next() {
            let Utf8Component::Normal(component_str) = path_component else {
                return Err(WadTreeError::InvalidItemPath {
                    item_path: path.to_string(),
                });
            };

            // Build current path efficiently
            if !current_path.is_empty() {
                current_path.push('/');
            }
            current_path.push_str(component_str);

            let is_file = path_components.peek().is_none();

            if is_file {
                // Create file
                let file = WadTreeFile::new(
                    component_str.into(),
                    path.clone(),
                    current_parent_id,
                    chunk,
                );

                let file_id = file.id();

                match current_parent_id {
                    Some(parent_id) => {
                        if let Some(WadTreeItem::Directory(parent)) =
                            self.item_storage.get_mut(&parent_id)
                        {
                            parent.items.push(file_id);
                        }
                        self.store_item(path.clone(), WadTreeItem::File(file));
                    }
                    None => {
                        self.add_item(path.clone(), WadTreeItem::File(file));
                    }
                }

                return Ok(());
            }

            // Check if directory exists
            let current_path_arc: Arc<str> = current_path.clone().into();
            let existing = self.resolve_item_by_path_mut(
                WadTreeItemKind::Directory,
                current_path_arc.clone(),
            );

            match existing {
                Some(WadTreeItem::Directory(dir)) => {
                    current_parent_id = Some(dir.id());
                }
                Some(item) => {
                    return Err(WadTreeError::NotADirectory { item_id: item.id() });
                }
                None => {
                    // Create directory
                    let directory = WadTreeDirectory::new(
                        component_str.into(),
                        current_path_arc.clone(),
                        current_parent_id,
                    );

                    let dir_id = directory.id();

                    match current_parent_id {
                        Some(parent_id) => {
                            if let Some(WadTreeItem::Directory(parent)) =
                                self.item_storage.get_mut(&parent_id)
                            {
                                parent.items.push(dir_id);
                            }
                            self.store_item(current_path_arc, WadTreeItem::Directory(directory));
                        }
                        None => {
                            self.add_item(current_path_arc, WadTreeItem::Directory(directory));
                        }
                    }

                    current_parent_id = Some(dir_id);
                }
            }
        }

        Ok(())
    }

    fn resolve_item_by_path_mut(
        &mut self,
        kind: WadTreeItemKind,
        path: Arc<str>,
    ) -> Option<&mut WadTreeItem> {
        let key = WadItemKindPath { kind, path };
        if let Some(item_id) = self.chunk_item_ids.get(&key) {
            let id = *item_id;
            self.item_storage.get_mut(&id)
        } else {
            None
        }
    }

    pub fn store_item(&mut self, path: Arc<str>, item: WadTreeItem) {
        let key = WadItemKindPath {
            kind: item.kind(),
            path,
        };
        self.chunk_item_ids.insert(key, item.id());
        self.item_storage.insert(item.id(), item);
    }

    pub fn add_item(&mut self, path: Arc<str>, item: WadTreeItem) {
        self.items.push(item.id());
        self.store_item(path, item);
    }

    pub fn wad_path(&self) -> &str {
        &self.wad_path
    }

    pub fn item_storage(&self) -> &HashMap<Uuid, WadTreeItem> {
        &self.item_storage
    }

    pub fn item_storage_mut(&mut self) -> &mut HashMap<Uuid, WadTreeItem> {
        &mut self.item_storage
    }
}

/// Compare two items for sorting (directories first, then by name)
fn compare_items(a: Option<&WadTreeItem>, b: Option<&WadTreeItem>) -> std::cmp::Ordering {
    match (a, b) {
        (Some(a), Some(b)) => a.cmp(b),
        _ => std::cmp::Ordering::Equal,
    }
}

impl WadTreeParent for WadTree {
    fn is_root(&self) -> bool {
        true
    }

    fn items(&self) -> &[Uuid] {
        &self.items
    }

    fn items_mut(&mut self) -> &mut Vec<Uuid> {
        &mut self.items
    }
}

impl WadTreePathable for WadTree {
    fn id(&self) -> Uuid {
        uuid::uuid!("00000000-0000-0000-0000-000000000000")
    }

    fn parent_id(&self) -> Option<Uuid> {
        None
    }

    fn name(&self) -> Arc<str> {
        "".into()
    }

    fn path(&self) -> Arc<str> {
        "".into()
    }

    fn name_hash(&self) -> u64 {
        0
    }

    fn path_hash(&self) -> u64 {
        0
    }
}
