use indexmap::IndexMap;
use league_toolkit::wad::Wad;
use parking_lot::Mutex;
use std::{
    collections::HashMap,
    fs::File,
    path::{Path, PathBuf},
    sync::Arc,
};
use tracing::info;
use uuid::Uuid;
use walkdir::WalkDir;

use crate::core::wad::tree::{WadTree, WadTreeError, WadTreeItem, WadTreeParent, WadTreePathable};

use super::WadHashtable;

/// Represents a file in the unified game explorer tree
#[derive(Debug, Clone)]
pub struct GameExplorerFile {
    pub id: Uuid,
    pub name: Arc<str>,
    pub path: Arc<str>,
    pub wad_id: Uuid,
    pub wad_name: Arc<str>,
    pub item_id: Uuid, // The original item ID in the source WAD tree
}

/// Represents a directory in the unified game explorer tree
#[derive(Debug, Clone)]
pub struct GameExplorerDirectory {
    pub id: Uuid,
    pub name: Arc<str>,
    pub path: Arc<str>,
    pub items: Vec<Uuid>,
}

/// An item in the unified game explorer tree
#[derive(Debug, Clone)]
pub enum GameExplorerItem {
    File(GameExplorerFile),
    Directory(GameExplorerDirectory),
}

impl GameExplorerItem {
    pub fn id(&self) -> Uuid {
        match self {
            GameExplorerItem::File(f) => f.id,
            GameExplorerItem::Directory(d) => d.id,
        }
    }

    pub fn name(&self) -> &str {
        match self {
            GameExplorerItem::File(f) => &f.name,
            GameExplorerItem::Directory(d) => &d.name,
        }
    }

    pub fn path(&self) -> &str {
        match self {
            GameExplorerItem::File(f) => &f.path,
            GameExplorerItem::Directory(d) => &d.path,
        }
    }

    pub fn kind(&self) -> &'static str {
        match self {
            GameExplorerItem::File(_) => "file",
            GameExplorerItem::Directory(_) => "directory",
        }
    }
}

/// The unified game explorer tree that combines all WAD files
#[derive(Debug)]
pub struct GameExplorerTree {
    /// Root level items
    root_items: Vec<Uuid>,
    /// All items in the tree
    item_storage: HashMap<Uuid, GameExplorerItem>,
    /// Path to directory ID lookup (for merging)
    directory_lookup: HashMap<Arc<str>, Uuid>,
}

impl GameExplorerTree {
    pub fn new() -> Self {
        Self {
            root_items: Vec::new(),
            item_storage: HashMap::new(),
            directory_lookup: HashMap::new(),
        }
    }

    /// Add items from a WAD tree into the unified tree
    pub fn add_wad_tree(&mut self, wad_tree: &WadTree, wad_id: Uuid, wad_name: Arc<str>) {
        // Process root items of the WAD tree
        for item_id in wad_tree.items().iter() {
            if let Some(item) = wad_tree.item_storage().get(item_id) {
                self.add_item_recursive(item, wad_tree, wad_id, wad_name.clone(), None);
            }
        }

    }

    fn add_item_recursive(
        &mut self,
        item: &WadTreeItem,
        wad_tree: &WadTree,
        wad_id: Uuid,
        wad_name: Arc<str>,
        parent_id: Option<Uuid>,
    ) {
        match item {
            WadTreeItem::File(file) => {
                let explorer_file = GameExplorerFile {
                    id: Uuid::new_v4(),
                    name: file.name(),
                    path: file.path(),
                    wad_id,
                    wad_name,
                    item_id: file.id(),
                };

                let file_id = explorer_file.id;
                self.item_storage
                    .insert(file_id, GameExplorerItem::File(explorer_file));

                // Add to parent or root
                if let Some(parent_id) = parent_id {
                    if let Some(GameExplorerItem::Directory(dir)) =
                        self.item_storage.get_mut(&parent_id)
                    {
                        dir.items.push(file_id);
                    }
                } else {
                    self.root_items.push(file_id);
                }
            }
            WadTreeItem::Directory(dir) => {
                let path: Arc<str> = dir.path();

                // Check if this directory already exists (merge)
                let dir_id = if let Some(&existing_id) = self.directory_lookup.get(&path) {
                    existing_id
                } else {
                    // Create new directory
                    let explorer_dir = GameExplorerDirectory {
                        id: Uuid::new_v4(),
                        name: dir.name(),
                        path: path.clone(),
                        items: Vec::new(),
                    };

                    let dir_id = explorer_dir.id;
                    self.directory_lookup.insert(path, dir_id);
                    self.item_storage
                        .insert(dir_id, GameExplorerItem::Directory(explorer_dir));

                    // Add to parent or root
                    if let Some(parent_id) = parent_id {
                        if let Some(GameExplorerItem::Directory(parent_dir)) =
                            self.item_storage.get_mut(&parent_id)
                        {
                            parent_dir.items.push(dir_id);
                        }
                    } else {
                        self.root_items.push(dir_id);
                    }

                    dir_id
                };

                // Process children
                for child_id in dir.items().iter() {
                    if let Some(child_item) = wad_tree.item_storage().get(child_id) {
                        self.add_item_recursive(
                            child_item,
                            wad_tree,
                            wad_id,
                            wad_name.clone(),
                            Some(dir_id),
                        );
                    }
                }
            }
        }
    }

    fn sort_items(&mut self) {
        // First, collect all directory IDs that need sorting
        let dir_ids: Vec<Uuid> = self
            .item_storage
            .iter()
            .filter_map(|(id, item)| {
                if matches!(item, GameExplorerItem::Directory(_)) {
                    Some(*id)
                } else {
                    None
                }
            })
            .collect();

        // Sort root items
        let item_storage = &self.item_storage;
        self.root_items.sort_by(|a, b| {
            Self::compare_items(item_storage.get(a), item_storage.get(b))
        });

        // Sort each directory's children
        for dir_id in dir_ids {
            if let Some(GameExplorerItem::Directory(dir)) = self.item_storage.get(&dir_id).cloned()
            {
                let mut sorted_items = dir.items.clone();
                let item_storage = &self.item_storage;
                sorted_items.sort_by(|a, b| {
                    Self::compare_items(item_storage.get(a), item_storage.get(b))
                });

                if let Some(GameExplorerItem::Directory(dir)) =
                    self.item_storage.get_mut(&dir_id)
                {
                    dir.items = sorted_items;
                }
            }
        }
    }

    fn compare_items(
        item_a: Option<&GameExplorerItem>,
        item_b: Option<&GameExplorerItem>,
    ) -> std::cmp::Ordering {
        match (item_a, item_b) {
            (Some(a), Some(b)) => {
                // Directories first, then by name
                let a_is_dir = matches!(a, GameExplorerItem::Directory(_));
                let b_is_dir = matches!(b, GameExplorerItem::Directory(_));

                match (a_is_dir, b_is_dir) {
                    (true, false) => std::cmp::Ordering::Less,
                    (false, true) => std::cmp::Ordering::Greater,
                    _ => a.name().cmp(b.name()),
                }
            }
            _ => std::cmp::Ordering::Equal,
        }
    }

    pub fn root_items(&self) -> &[Uuid] {
        &self.root_items
    }

    pub fn item_storage(&self) -> &HashMap<Uuid, GameExplorerItem> {
        &self.item_storage
    }

    pub fn get_item(&self, id: &Uuid) -> Option<&GameExplorerItem> {
        self.item_storage.get(id)
    }

    pub fn get_directory_items(&self, dir_id: &Uuid) -> Vec<&GameExplorerItem> {
        if let Some(GameExplorerItem::Directory(dir)) = self.item_storage.get(dir_id) {
            dir.items
                .iter()
                .filter_map(|id| self.item_storage.get(id))
                .collect()
        } else {
            Vec::new()
        }
    }
}

/// State for the game explorer feature
pub struct GameExplorer {
    /// Whether the game explorer is initialized
    is_initialized: bool,
    /// The base path of the league directory
    base_path: Option<PathBuf>,
    /// All mounted WAD trees (wad_id -> tree)
    wad_trees: IndexMap<Uuid, WadTree>,
    /// All mounted WADs (wad_id -> wad)
    wads: HashMap<Uuid, Wad<File>>,
    /// WAD metadata (wad_id -> (path, name))
    wad_metadata: HashMap<Uuid, (PathBuf, Arc<str>)>,
    /// The unified game explorer tree
    unified_tree: Option<GameExplorerTree>,
}

impl GameExplorer {
    pub fn new() -> Self {
        Self {
            is_initialized: false,
            base_path: None,
            wad_trees: IndexMap::new(),
            wads: HashMap::new(),
            wad_metadata: HashMap::new(),
            unified_tree: None,
        }
    }

    /// Mount all WAD files from the league directory
    pub fn mount_from_directory(
        &mut self,
        league_directory: &Path,
        hashtable: &WadHashtable,
    ) -> Result<(), WadTreeError> {
        info!(
            "Mounting game explorer from directory: {}",
            league_directory.display()
        );

        // Clear existing state
        self.wad_trees.clear();
        self.wads.clear();
        self.wad_metadata.clear();
        self.unified_tree = None;
        self.base_path = Some(league_directory.to_path_buf());

        // Find all .wad.client files
        let wad_paths: Vec<PathBuf> = WalkDir::new(league_directory)
            .follow_links(true)
            .into_iter()
            .filter_map(|e| e.ok())
            .filter(|e| {
                e.path().is_file()
                    && e.path()
                        .file_name()
                        .and_then(|n| n.to_str())
                        .map(|n| n.ends_with(".wad.client"))
                        .unwrap_or(false)
            })
            .map(|e| e.path().to_path_buf())
            .collect();

        info!("Found {} WAD files", wad_paths.len());

        // Mount each WAD
        for wad_path in wad_paths {
            let wad_id = Uuid::new_v4();
            let wad_name: Arc<str> = wad_path
                .file_name()
                .and_then(|n| n.to_str())
                .unwrap_or("unknown")
                .into();

            match File::open(&wad_path) {
                Ok(file) => match Wad::mount(file) {
                    Ok(mut wad) => {
                        match WadTree::from_wad(
                            &mut wad,
                            wad_id,
                            wad_path.to_string_lossy().as_ref(),
                            hashtable,
                        ) {
                            Ok(tree) => {
                                self.wad_trees.insert(wad_id, tree);
                                self.wads.insert(wad_id, wad);
                                self.wad_metadata
                                    .insert(wad_id, (wad_path.clone(), wad_name));
                            }
                            Err(e) => {
                                tracing::warn!(
                                    "Failed to create tree for {}: {}",
                                    wad_path.display(),
                                    e
                                );
                            }
                        }
                    }
                    Err(e) => {
                        tracing::warn!("Failed to mount {}: {}", wad_path.display(), e);
                    }
                },
                Err(e) => {
                    tracing::warn!("Failed to open {}: {}", wad_path.display(), e);
                }
            }
        }

        // Build unified tree
        self.build_unified_tree();
        self.is_initialized = true;

        info!(
            "Game explorer initialized with {} WADs",
            self.wad_trees.len()
        );

        Ok(())
    }

    fn build_unified_tree(&mut self) {
        let mut unified = GameExplorerTree::new();

        for (wad_id, tree) in self.wad_trees.iter() {
            let wad_name = self
                .wad_metadata
                .get(wad_id)
                .map(|(_, name)| name.clone())
                .unwrap_or_else(|| "unknown".into());

            unified.add_wad_tree(tree, *wad_id, wad_name);
        }

        // Sort the unified tree
        unified.sort_items();

        self.unified_tree = Some(unified);
    }

    pub fn is_initialized(&self) -> bool {
        self.is_initialized
    }

    pub fn base_path(&self) -> Option<&Path> {
        self.base_path.as_deref()
    }

    pub fn unified_tree(&self) -> Option<&GameExplorerTree> {
        self.unified_tree.as_ref()
    }

    pub fn wad_count(&self) -> usize {
        self.wad_trees.len()
    }

    pub fn wad_trees(&self) -> &IndexMap<Uuid, WadTree> {
        &self.wad_trees
    }

    pub fn wads(&self) -> &HashMap<Uuid, Wad<File>> {
        &self.wads
    }

    pub fn wads_mut(&mut self) -> &mut HashMap<Uuid, Wad<File>> {
        &mut self.wads
    }

    pub fn wad_metadata(&self) -> &HashMap<Uuid, (PathBuf, Arc<str>)> {
        &self.wad_metadata
    }
}

pub struct GameExplorerState(pub Mutex<GameExplorer>);

