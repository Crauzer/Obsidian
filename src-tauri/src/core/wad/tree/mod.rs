use std::{
    collections::HashMap,
    io::{Read, Seek},
    path::{self, Path, PathBuf},
    sync::Arc,
};

use itertools::Itertools;
use league_toolkit::core::wad::{Wad, WadChunk, WadDecoder, WadError};
use thiserror::Error;
use tracing::info;
use uuid::Uuid;

mod item;

pub use item::*;

use crate::{
    core::league_file::{
        get_extension_from_league_file_kind, identify_league_file, LeagueFileKind,
    },
    state::WadHashtable,
};

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

        // create tree
        let mut tree = WadTree {
            wad_id,
            wad_path: wad_path.into(),
            items: Vec::default(),
            item_storage: HashMap::default(),
            chunk_item_ids: HashMap::default(),
        };

        let (mut decoder, chunks) = wad.decode();

        // create all items recursively
        for (chunk_path_hash, chunk) in chunks.iter() {
            let path = match hashtable.items().get(chunk_path_hash) {
                Some(path) => path.clone(),
                None => Self::guess_chunk_path(*chunk_path_hash, chunk, &mut decoder)?,
            };

            tree.create_item_from_chunk(chunk, Path::new(path.as_ref()))?;
        }

        // sort everything
        tree.sort();

        Ok(tree)
    }

    fn guess_chunk_path<TSource: Read + Seek>(
        chunk_path_hash: u64,
        chunk: &WadChunk,
        decoder: &mut WadDecoder<TSource>,
    ) -> Result<Arc<str>, WadError> {
        let data = decoder.load_chunk_decompressed(chunk)?;
        let file_kind = identify_league_file(&data);

        match file_kind {
            LeagueFileKind::Unknown => Ok(format!("{:#0x}", chunk_path_hash).into()),
            _ => Ok(format!(
                "{:#0x}.{}",
                chunk_path_hash,
                get_extension_from_league_file_kind(file_kind)
            )
            .into()),
        }
    }

    pub fn sort(&mut self) {
        self.items.sort_unstable_by(|a, b| {
            let a = self.item_storage.get(a);
            let b = self.item_storage.get(b);

            a.cmp(&b)
        });

        let item_ids = self.item_storage.keys().map(|x| *x).collect_vec();
        for item_id in item_ids.iter() {
            let Some(item) = self.item_storage.remove(item_id) else {
                continue;
            };

            if let WadTreeItem::Directory(mut directory) = item {
                directory.sort(&self);
                self.item_storage
                    .insert(directory.id(), WadTreeItem::Directory(directory));
            } else {
                self.item_storage.insert(item.id(), item);
            }
        }
    }

    pub fn create_item_from_chunk(
        &mut self,
        chunk: &WadChunk,
        path: impl AsRef<Path>,
    ) -> Result<(), WadTreeError> {
        let mut path_components = path.as_ref().components().peekable();

        let mut current_parent_id: Option<Uuid> = None;
        let mut current_parent_path = Some(PathBuf::new());
        while let Some(path_component) = path_components.next() {
            let path::Component::Normal(path_component) = path_component else {
                return Err(WadTreeError::InvalidItemPath {
                    item_path: path.as_ref().to_str().unwrap().to_string(),
                });
            };

            let current_path = match &current_parent_path {
                Some(current_parent_path) => current_parent_path.join(path_component),
                None => path_component.to_os_string().into(),
            };

            if path_components.peek().is_none() {
                let file = WadTreeFile::new(
                    path_component.to_str().unwrap().into(),
                    path.as_ref().to_str().unwrap().into(),
                    current_parent_id,
                    chunk,
                );

                match current_parent_id {
                    Some(current_parent_id) => {
                        if let WadTreeItem::Directory(parent) =
                            self.item_storage.get_mut(&current_parent_id).unwrap()
                        {
                            parent.store_item(file.id(), &current_path);
                            self.store_item(
                                current_path.to_string_lossy().to_string().into(),
                                WadTreeItem::File(file),
                            );
                        }
                    }
                    None => {
                        self.add_item(
                            current_path.to_string_lossy().to_string().into(),
                            WadTreeItem::File(file),
                        );
                    }
                }

                return Ok(());
            }

            match self.resolve_item_by_path_mut(
                WadTreeItemKind::Directory,
                current_path.to_string_lossy().as_ref().into(),
            ) {
                Some(item) => {
                    let WadTreeItem::Directory(directory) = item else {
                        tracing::error!("{:#?}", item);

                        return Err(WadTreeError::NotADirectory { item_id: item.id() });
                    };

                    current_parent_id = Some(directory.id());
                    current_parent_path = Some(current_path);
                }
                None => {
                    let directory = WadTreeDirectory::new(
                        path_component.to_str().unwrap().into(),
                        current_path.to_string_lossy().into(),
                        current_parent_id,
                    );

                    match current_parent_id {
                        Some(id) => {
                            if let WadTreeItem::Directory(parent) =
                                self.item_storage.get_mut(&id).unwrap()
                            {
                                parent.store_item(directory.id(), &current_path);
                            }

                            current_parent_id = Some(directory.id());
                            self.store_item(
                                current_path.to_string_lossy().to_string().into(),
                                WadTreeItem::Directory(directory),
                            );
                            current_parent_path = Some(current_path);
                        }
                        None => {
                            current_parent_id = Some(directory.id());
                            self.add_item(
                                current_path.to_string_lossy().to_string().into(),
                                WadTreeItem::Directory(directory),
                            );
                            current_parent_path = Some(current_path);
                        }
                    }
                }
            };
        }

        Ok(())
    }

    fn resolve_path(path_hash: u64, hashtable: &WadHashtable) -> Arc<str> {
        match hashtable.items().get(&path_hash) {
            Some(path) => path.clone(),
            None => format!("{:#0x}", path_hash).into(),
        }
    }

    fn resolve_item_by_path(&self, kind: WadTreeItemKind, path: Arc<str>) -> Option<&WadTreeItem> {
        if let Some(item_id) = self.chunk_item_ids.get(&WadItemKindPath {
            kind,
            path: path.clone(),
        }) {
            self.item_storage.get(item_id)
        } else {
            None
        }
    }
    fn resolve_item_by_path_mut(
        &mut self,
        kind: WadTreeItemKind,
        path: Arc<str>,
    ) -> Option<&mut WadTreeItem> {
        if let Some(item_id) = self.chunk_item_ids.get_mut(&WadItemKindPath {
            kind,
            path: path.clone(),
        }) {
            self.item_storage.get_mut(item_id)
        } else {
            None
        }
    }

    pub fn store_item(&mut self, path: Arc<str>, item: WadTreeItem) {
        self.chunk_item_ids.insert(
            WadItemKindPath {
                kind: item.kind(),
                path: path.as_ref().into(),
            },
            item.id(),
        );
        self.item_storage.insert(item.id(), item);
    }

    pub fn add_item(&mut self, path: Arc<str>, item: WadTreeItem) {
        self.items.push(item.id());
        self.store_item(path, item);
    }

    pub fn wad_id(&self) -> Uuid {
        self.wad_id
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

    pub fn chunk_item_ids(&self) -> &HashMap<WadItemKindPath, Uuid> {
        &self.chunk_item_ids
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
