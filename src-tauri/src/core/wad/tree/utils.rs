use std::{iter::Peekable, sync::Arc};

use indexmap::IndexMap;
use uuid::Uuid;
use xxhash_rust::xxh3::xxh3_64;

use crate::{
    core::wad::{tree::WadTreeParent, WadChunk},
    state::WadHashtable,
};

use super::{
    WadTree, WadTreeDirectory, WadTreeError, WadTreeFile, WadTreeItem, WadTreeItemKey,
    WadTreeItemKind, WadTreeParentInternal, WadTreePathable,
};

pub(super) fn add_item_to_parent(
    parent: &mut (impl WadTreeParentInternal + WadTreePathable),
    path_components: &mut Peekable<std::str::Split<char>>,
    chunk: &WadChunk,
    hashtable: &WadHashtable,
) -> Result<(), WadTreeError> {
    let name: Arc<str> = path_components
        .next()
        .ok_or(WadTreeError::InvalidItemName {
            chunk_path: chunk.path_hash(),
        })?
        .into();

    // we've depleted the components, add as file
    if let None = path_components.peek() {
        return add_file_item(parent, name.clone(), chunk, &hashtable);
    }

    match add_or_resolve_directory_item(parent, name.clone())? {
        WadTreeItem::File(_) => Err(WadTreeError::ItemCreationFailure {
            item_path: WadTree::resolve_chunk_path(chunk.path_hash, &hashtable).to_string(),
        }),
        WadTreeItem::Directory(directory) => {
            add_item_to_parent(directory, path_components, chunk, hashtable)
        }
    }
}

pub(super) fn add_file_item(
    parent: &mut impl WadTreeParentInternal,
    name: Arc<str>,
    chunk: &WadChunk,
    hashtable: &WadHashtable,
) -> Result<(), WadTreeError> {
    let path = WadTree::resolve_chunk_path(chunk.path_hash, &hashtable);
    let path_hash = xxh3_64(path.as_bytes());
    let name_hash = xxh3_64(name.as_bytes());

    if let Some(existing_item) = parent.items_mut().insert(
        WadTreeItemKey {
            path_hash: chunk.path_hash,
            kind: WadTreeItemKind::File,
        },
        WadTreeItem::File(WadTreeFile {
            id: Uuid::new_v4(),
            name,
            path,
            name_hash,
            path_hash,
            is_selected: false,
            is_checked: false,
            chunk: *chunk,
        }),
    ) {
        return Err(WadTreeError::ExistingFile {
            file_path: existing_item.path().to_string(),
        });
    }

    return Ok(());
}

pub(super) fn add_or_resolve_directory_item(
    parent: &mut (impl WadTreeParentInternal + WadTreePathable),
    name: Arc<str>,
) -> Result<&mut WadTreeItem, WadTreeError> {
    let path: Arc<str> = match parent.is_root() {
        true => name.clone(),
        false => format!("{}/{}", parent.path(), name).into(),
    };
    let path_hash = xxh3_64(path.as_bytes());

    match parent.items_mut().entry(WadTreeItemKey {
        path_hash,
        kind: WadTreeItemKind::Directory,
    }) {
        indexmap::map::Entry::Occupied(entry) => Ok(entry.into_mut()),
        indexmap::map::Entry::Vacant(entry) => {
            Ok(entry.insert(WadTreeItem::Directory(WadTreeDirectory {
                id: Uuid::new_v4(),
                name: name.clone(),
                path,
                name_hash: xxh3_64(name.as_bytes()),
                path_hash,
                is_selected: false,
                is_checked: false,
                is_expanded: false,
                items: IndexMap::default(),
            })))
        }
    }
}

pub(super) fn sort_parent_items(parent: &mut impl WadTreeParentInternal) {
    parent
        .items_mut()
        .sort_by(|_, a_value, _, b_value| a_value.cmp(b_value));

    for (_, item) in parent.items_mut() {
        if let WadTreeItem::Directory(directory) = item {
            sort_parent_items(directory)
        }
    }
}

pub(super) fn traverse_parent_items(parent: &impl WadTreeParent, mut cb: impl FnMut(&WadTreeItem)) {
    fn traverse_parent_items_internal(
        parent: &impl WadTreeParent,
        cb: &mut dyn FnMut(&WadTreeItem),
    ) {
        for (_, item) in parent.items() {
            match item {
                WadTreeItem::Directory(directory) => {
                    cb(item);
                    traverse_parent_items_internal(directory, cb);
                }
                WadTreeItem::File(_) => cb(item),
            }
        }
    }

    traverse_parent_items_internal(parent, &mut cb)
}

pub(super) fn traverse_parent_items_mut(
    parent: &mut impl WadTreeParent,
    mut cb: impl FnMut(&mut WadTreeItem),
) {
    fn traverse_parent_items_internal(
        parent: &mut impl WadTreeParent,
        cb: &mut dyn FnMut(&mut WadTreeItem),
    ) {
        for (_, item) in parent.items_mut() {
            if let WadTreeItem::Directory(_) = item {
                cb(item);
            }

            match item {
                WadTreeItem::Directory(directory) => {
                    traverse_parent_items_internal(directory, cb);
                }
                WadTreeItem::File(_) => cb(item),
            }
        }
    }

    traverse_parent_items_internal(parent, &mut cb)
}

pub(super) fn find_parent_item<'p>(
    parent: &'p impl WadTreeParent,
    condition: impl Fn(&WadTreeItem) -> bool,
) -> Option<&'p WadTreeItem> {
    fn search_parent<'p>(
        parent: &'p impl WadTreeParent,
        condition: &dyn Fn(&WadTreeItem) -> bool,
    ) -> Option<&'p WadTreeItem> {
        for (_, item) in parent.items() {
            if condition(&item) {
                return Some(item);
            }

            if let Some(found_item) = match item {
                WadTreeItem::Directory(directory) => search_parent(directory, condition),
                _ => None,
            } {
                return Some(found_item);
            }
        }

        None
    }

    search_parent(parent, &condition)
}

pub(super) fn find_parent_item_mut<'p>(
    parent: &'p mut impl WadTreeParent,
    condition: impl Fn(&WadTreeItem) -> bool,
) -> Option<&'p mut WadTreeItem> {
    fn search_parent<'p>(
        parent: &'p mut impl WadTreeParent,
        condition: &dyn Fn(&WadTreeItem) -> bool,
    ) -> Option<&'p mut WadTreeItem> {
        for (_, item) in parent.items_mut() {
            if condition(&item) {
                return Some(item);
            }

            if let Some(found_item) = match item {
                WadTreeItem::Directory(directory) => search_parent(directory, condition),
                _ => None,
            } {
                return Some(found_item);
            }
        }

        None
    }

    search_parent(parent, &condition)
}
