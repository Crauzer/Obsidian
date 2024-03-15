use std::path::PathBuf;
use std::sync::Arc;
use std::vec;
use std::{collections::HashMap, path::Path};

use serde::{Deserialize, Serialize};
use uuid::Uuid;
use xxhash_rust::xxh3::xxh3_64;

use super::{WadChunk, WadTree};

#[derive(Debug, Clone, Copy, Eq, PartialEq, PartialOrd, Ord, Hash, Serialize, Deserialize)]
pub enum WadTreeItemKind {
    #[serde(rename = "file")]
    File,
    #[serde(rename = "directory")]
    Directory,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash)]
pub struct WadTreeItemKey {
    pub path_hash: u64,
    pub kind: WadTreeItemKind,
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub enum WadTreeItem {
    File(WadTreeFile),
    Directory(WadTreeDirectory),
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct WadTreeFile {
    pub(super) id: Uuid,
    pub(super) parent_id: Option<Uuid>,
    pub(super) name: Arc<str>,
    pub(super) path: Arc<str>,
    pub(super) name_hash: u64,
    pub(super) path_hash: u64,
    pub(super) is_selected: bool,
    pub(super) is_checked: bool,
    pub(super) chunk: WadChunk,
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct WadTreeDirectory {
    pub(super) id: Uuid,
    pub(super) parent_id: Option<Uuid>,
    pub(super) name: Arc<str>,
    pub(super) path: Arc<str>,
    pub(super) name_hash: u64,
    pub(super) path_hash: u64,
    pub(super) is_selected: bool,
    pub(super) is_checked: bool,
    pub(super) is_expanded: bool,
    pub(super) items: Vec<Uuid>,
    pub(super) item_path_lookup: HashMap<PathBuf, Uuid>,
}

pub trait WadTreePathable {
    fn id(&self) -> Uuid;
    fn parent_id(&self) -> Option<Uuid>;
    fn name(&self) -> Arc<str>;
    fn path(&self) -> Arc<str>;
    fn name_hash(&self) -> u64;
    fn path_hash(&self) -> u64;
}

pub trait WadTreeParent {
    fn is_root(&self) -> bool;
    fn items(&self) -> &[Uuid];
    fn items_mut(&mut self) -> &mut Vec<Uuid>;
}

pub trait WadTreeSelectable {
    fn set_is_selected(&mut self, is_selected: bool);
}

pub trait WadTreeCheckable {
    fn set_is_checked(&mut self, is_checked: bool);
}

pub trait WadTreeExpandable {
    fn set_is_expanded(&mut self, is_expanded: bool);
}

impl WadTreeItem {
    pub fn kind(&self) -> WadTreeItemKind {
        match self {
            WadTreeItem::File(_) => WadTreeItemKind::File,
            WadTreeItem::Directory(_) => WadTreeItemKind::Directory,
        }
    }

    pub fn id(&self) -> Uuid {
        match self {
            WadTreeItem::File(file) => file.id(),
            WadTreeItem::Directory(directory) => directory.id(),
        }
    }
}

impl WadTreeFile {
    pub fn new(name: Arc<str>, path: Arc<str>, parent_id: Option<Uuid>, chunk: &WadChunk) -> Self {
        let name_hash = xxh3_64(name.as_bytes());
        let path_hash = xxh3_64(path.as_bytes());

        Self {
            id: Uuid::new_v4(),
            parent_id,
            name,
            path,
            name_hash,
            path_hash,
            is_selected: false,
            is_checked: false,
            chunk: *chunk,
        }
    }

    pub fn id(&self) -> Uuid {
        self.id
    }
    pub fn is_selected(&self) -> bool {
        self.is_selected
    }
    pub fn is_checked(&self) -> bool {
        self.is_checked
    }
    pub fn chunk(&self) -> &WadChunk {
        &self.chunk
    }
}

impl WadTreeDirectory {
    pub fn new(name: Arc<str>, path: Arc<str>, parent_id: Option<Uuid>) -> Self {
        let name_hash = xxh3_64(name.as_bytes());
        let path_hash = xxh3_64(path.as_bytes());

        Self {
            id: Uuid::new_v4(),
            parent_id,
            name,
            path,
            name_hash,
            path_hash,
            is_selected: false,
            is_expanded: false,
            is_checked: false,
            items: vec![],
            item_path_lookup: HashMap::new(),
        }
    }

    pub fn id(&self) -> Uuid {
        self.id
    }
    pub fn is_selected(&self) -> bool {
        self.is_selected
    }
    pub fn is_checked(&self) -> bool {
        self.is_checked
    }
    pub fn is_expanded(&self) -> bool {
        self.is_expanded
    }
    pub fn item_path_lookup(&self) -> &HashMap<PathBuf, Uuid> {
        &self.item_path_lookup
    }

    pub fn store_item(&mut self, item_id: Uuid, path: impl AsRef<Path>) {
        self.item_path_lookup
            .insert(path.as_ref().to_path_buf(), item_id);
        self.items.push(item_id);
    }

    pub fn sort(&mut self, wad_tree: &WadTree) {
        self.items.sort_by(|a, b| {
            let a = wad_tree.item_storage().get(a);
            let b = wad_tree.item_storage().get(b);

            a.cmp(&b)
        });
    }
}

impl WadTreeParent for WadTreeDirectory {
    fn is_root(&self) -> bool {
        false
    }

    fn items(&self) -> &[Uuid] {
        &self.items
    }

    fn items_mut(&mut self) -> &mut Vec<Uuid> {
        &mut self.items
    }
}

impl WadTreePathable for WadTreeItem {
    fn id(&self) -> Uuid {
        match self {
            WadTreeItem::File(file) => file.id(),
            WadTreeItem::Directory(directory) => directory.id(),
        }
    }

    fn parent_id(&self) -> Option<Uuid> {
        match self {
            WadTreeItem::File(file) => file.parent_id(),
            WadTreeItem::Directory(directory) => directory.parent_id(),
        }
    }

    fn name(&self) -> Arc<str> {
        match self {
            WadTreeItem::File(file) => file.name(),
            WadTreeItem::Directory(directory) => directory.name(),
        }
    }

    fn path(&self) -> Arc<str> {
        match self {
            WadTreeItem::File(file) => file.path(),
            WadTreeItem::Directory(directory) => directory.path(),
        }
    }

    fn name_hash(&self) -> u64 {
        match self {
            WadTreeItem::File(file) => file.name_hash(),
            WadTreeItem::Directory(directory) => directory.name_hash(),
        }
    }

    fn path_hash(&self) -> u64 {
        match self {
            WadTreeItem::File(file) => file.path_hash(),
            WadTreeItem::Directory(directory) => directory.path_hash(),
        }
    }
}

impl WadTreePathable for WadTreeDirectory {
    fn id(&self) -> Uuid {
        self.id
    }

    fn parent_id(&self) -> Option<Uuid> {
        self.parent_id
    }

    fn name(&self) -> Arc<str> {
        self.name.clone()
    }

    fn path(&self) -> Arc<str> {
        self.path.clone()
    }

    fn name_hash(&self) -> u64 {
        self.name_hash
    }

    fn path_hash(&self) -> u64 {
        self.path_hash
    }
}

impl WadTreePathable for WadTreeFile {
    fn id(&self) -> Uuid {
        self.id
    }

    fn parent_id(&self) -> Option<Uuid> {
        self.parent_id
    }

    fn name(&self) -> Arc<str> {
        self.name.clone()
    }

    fn path(&self) -> Arc<str> {
        self.path.clone()
    }

    fn name_hash(&self) -> u64 {
        self.name_hash
    }

    fn path_hash(&self) -> u64 {
        self.path_hash
    }
}

impl WadTreeSelectable for WadTreeItem {
    fn set_is_selected(&mut self, is_selected: bool) {
        match self {
            WadTreeItem::File(file) => file.set_is_selected(is_selected),
            WadTreeItem::Directory(directory) => directory.set_is_selected(is_selected),
        }
    }
}

impl WadTreeSelectable for WadTreeFile {
    fn set_is_selected(&mut self, is_selected: bool) {
        self.is_selected = is_selected
    }
}

impl WadTreeSelectable for WadTreeDirectory {
    fn set_is_selected(&mut self, is_selected: bool) {
        self.is_selected = is_selected
    }
}

impl WadTreeExpandable for WadTreeDirectory {
    fn set_is_expanded(&mut self, is_expanded: bool) {
        self.is_expanded = is_expanded;
    }
}

impl Ord for WadTreeItem {
    fn cmp(&self, other: &Self) -> std::cmp::Ordering {
        match (self, other) {
            (WadTreeItem::File(_), WadTreeItem::Directory(_)) => std::cmp::Ordering::Greater,
            (WadTreeItem::Directory(_), WadTreeItem::File(_)) => std::cmp::Ordering::Less,
            _ => self.name().cmp(&other.name()),
        }
    }
}

impl PartialOrd for WadTreeItem {
    fn partial_cmp(&self, other: &Self) -> Option<std::cmp::Ordering> {
        Some(self.cmp(other))
    }
}
