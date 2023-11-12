use std::{iter::Peekable, sync::Arc};

use indexmap::IndexMap;
use uuid::Uuid;

use crate::core::wad_hashtable::WadHashtable;

use super::utils::{
    add_item_to_parent, find_parent_item, traverse_parent_items, traverse_parent_items_mut,
};
use super::{WadChunk, WadTreeError};

#[derive(Debug, Clone, Copy, Eq, PartialEq, PartialOrd, Ord, Hash)]
pub enum WadTreeItemKind {
    File,
    Directory,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash)]
pub struct WadTreeItemKey {
    pub(super) path_hash: u64,
    pub(super) kind: WadTreeItemKind,
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub enum WadTreeItem {
    File(WadTreeFile),
    Directory(WadTreeDirectory),
}

#[derive(Debug, Clone, PartialEq, Eq)]
pub struct WadTreeFile {
    pub(super) id: Uuid,
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
    pub(super) name: Arc<str>,
    pub(super) path: Arc<str>,
    pub(super) name_hash: u64,
    pub(super) path_hash: u64,
    pub(super) is_selected: bool,
    pub(super) is_checked: bool,
    pub(super) is_expanded: bool,
    pub(super) items: IndexMap<WadTreeItemKey, WadTreeItem>,
}

pub trait WadTreePathable {
    fn id(&self) -> Uuid;
    fn name(&self) -> &str;
    fn path(&self) -> &str;
    fn name_hash(&self) -> u64;
    fn path_hash(&self) -> u64;
}

pub trait WadTreeParent {
    fn is_root(&self) -> bool;
    fn items(&self) -> &IndexMap<WadTreeItemKey, WadTreeItem>;
    fn items_mut(&mut self) -> &mut IndexMap<WadTreeItemKey, WadTreeItem>;

    fn traverse_items(&self, cb: &mut impl FnMut(&WadTreeItem));
    fn traverse_items_mut(&mut self, cb: &mut impl FnMut(&mut WadTreeItem));

    fn find_item<'p>(&'p self, condition: impl Fn(&WadTreeItem) -> bool)
        -> Option<&'p WadTreeItem>;
}

pub(super) trait WadTreeParentInternal: WadTreeParent {
    fn add_item(
        &mut self,
        path_components: &mut Peekable<std::str::Split<char>>,
        chunk: &WadChunk,
        hashtable: &WadHashtable,
    ) -> Result<(), WadTreeError>;
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
    pub fn id(&self) -> Uuid {
        match self {
            WadTreeItem::File(file) => file.id(),
            WadTreeItem::Directory(directory) => directory.id(),
        }
    }
}

impl WadTreeFile {
    pub fn id(&self) -> Uuid {
        self.id
    }
    pub fn is_selected(&self) -> bool {
        self.is_selected
    }
    pub fn is_checked(&self) -> bool {
        self.is_checked
    }
}

impl WadTreeDirectory {
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
}

impl WadTreeParent for WadTreeDirectory {
    fn is_root(&self) -> bool {
        false
    }

    fn items(&self) -> &IndexMap<WadTreeItemKey, WadTreeItem> {
        &self.items
    }

    fn items_mut(&mut self) -> &mut IndexMap<WadTreeItemKey, WadTreeItem> {
        &mut self.items
    }

    fn traverse_items(&self, mut cb: &mut impl FnMut(&WadTreeItem)) {
        traverse_parent_items(self, &mut cb)
    }
    fn traverse_items_mut(&mut self, mut cb: &mut impl FnMut(&mut WadTreeItem)) {
        traverse_parent_items_mut(self, &mut cb)
    }

    fn find_item<'p>(
        &'p self,
        condition: impl Fn(&WadTreeItem) -> bool,
    ) -> Option<&'p WadTreeItem> {
        find_parent_item(self, &condition)
    }
}

impl WadTreeParentInternal for WadTreeDirectory {
    fn add_item(
        &mut self,
        path_components: &mut Peekable<std::str::Split<char>>,
        chunk: &WadChunk,
        hashtable: &WadHashtable,
    ) -> Result<(), WadTreeError> {
        add_item_to_parent(self, path_components, chunk, &hashtable)
    }
}

impl WadTreePathable for WadTreeItem {
    fn id(&self) -> Uuid {
        match self {
            WadTreeItem::File(file) => file.id(),
            WadTreeItem::Directory(directory) => directory.id(),
        }
    }

    fn name(&self) -> &str {
        match self {
            WadTreeItem::File(file) => file.name(),
            WadTreeItem::Directory(directory) => directory.name(),
        }
    }

    fn path(&self) -> &str {
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

    fn name(&self) -> &str {
        &self.name
    }

    fn path(&self) -> &str {
        &self.path
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

    fn name(&self) -> &str {
        &self.name
    }

    fn path(&self) -> &str {
        &self.path
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
