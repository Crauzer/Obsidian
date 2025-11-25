use camino::{Utf8Path, Utf8PathBuf};
use indexmap::IndexMap;
use league_toolkit::wad::Wad;
use parking_lot::Mutex;
use rayon::prelude::*;
use std::{collections::HashMap, fs::File, sync::Arc};
use tracing::info;
use uuid::Uuid;
use walkdir::WalkDir;

use crate::core::wad::tree::{WadTree, WadTreeError, WadTreeItem, WadTreeParent, WadTreePathable};

use super::WadHashtable;

// ============================================================================
// Merged Index - Lightweight references to underlying WadTree items
// ============================================================================

/// Reference to an item in a WAD tree (no data duplication)
#[derive(Debug, Clone)]
pub enum MergedItemRef {
    /// Reference to a file in a specific WAD
    File { wad_id: Uuid, item_id: Uuid },
    /// Reference to a merged directory (virtual)
    Directory(Uuid),
}

/// A merged directory that combines items from multiple WADs
#[derive(Debug, Clone)]
pub struct MergedDirectory {
    pub id: Uuid,
    pub name: Arc<str>,
    pub path: Arc<str>,
    pub items: Vec<MergedItemRef>,
}

/// Lightweight index for merged view of multiple WAD trees
#[derive(Debug)]
pub struct MergedIndex {
    /// Root level items
    root_items: Vec<MergedItemRef>,
    /// Virtual directories (merged by path)
    directories: HashMap<Uuid, MergedDirectory>,
    /// Path -> directory ID lookup (for merging)
    path_to_dir: HashMap<Arc<str>, Uuid>,
}

impl MergedIndex {
    pub fn new() -> Self {
        Self {
            root_items: Vec::new(),
            directories: HashMap::new(),
            path_to_dir: HashMap::new(),
        }
    }

    /// Build merged index from multiple WAD trees
    pub fn build(wad_trees: &IndexMap<Uuid, WadTree>) -> Self {
        let mut index = Self::new();

        for (wad_id, tree) in wad_trees.iter() {
            index.add_wad_tree(*wad_id, tree);
        }

        index.sort_all();
        index
    }

    fn add_wad_tree(&mut self, wad_id: Uuid, tree: &WadTree) {
        for item_id in tree.items().iter() {
            if let Some(item) = tree.item_storage().get(item_id) {
                self.add_item_recursive(wad_id, tree, item, None);
            }
        }
    }

    fn add_item_recursive(
        &mut self,
        wad_id: Uuid,
        tree: &WadTree,
        item: &WadTreeItem,
        parent_dir_id: Option<Uuid>,
    ) {
        match item {
            WadTreeItem::File(file) => {
                let file_ref = MergedItemRef::File {
                    wad_id,
                    item_id: file.id(),
                };

                if let Some(parent_id) = parent_dir_id {
                    if let Some(dir) = self.directories.get_mut(&parent_id) {
                        dir.items.push(file_ref);
                    }
                } else {
                    self.root_items.push(file_ref);
                }
            }
            WadTreeItem::Directory(dir) => {
                let path = dir.path();

                // Check if directory already exists (merge)
                let dir_id = if let Some(&existing_id) = self.path_to_dir.get(&path) {
                    existing_id
                } else {
                    // Create new merged directory
                    let new_dir = MergedDirectory {
                        id: Uuid::new_v4(),
                        name: dir.name(),
                        path: path.clone(),
                        items: Vec::new(),
                    };
                    let dir_id = new_dir.id;

                    self.path_to_dir.insert(path, dir_id);
                    self.directories.insert(dir_id, new_dir);

                    // Add to parent or root
                    let dir_ref = MergedItemRef::Directory(dir_id);
                    if let Some(parent_id) = parent_dir_id {
                        if let Some(parent_dir) = self.directories.get_mut(&parent_id) {
                            parent_dir.items.push(dir_ref);
                        }
                    } else {
                        self.root_items.push(dir_ref);
                    }

                    dir_id
                };

                // Process children
                for child_id in dir.items().iter() {
                    if let Some(child) = tree.item_storage().get(child_id) {
                        self.add_item_recursive(wad_id, tree, child, Some(dir_id));
                    }
                }
            }
        }
    }

    fn sort_all(&mut self) {
        // Build a name lookup for sorting
        let dir_names: HashMap<Uuid, Arc<str>> = self
            .directories
            .iter()
            .map(|(id, dir)| (*id, dir.name.clone()))
            .collect();

        // Sort root items
        self.root_items
            .sort_by(|a, b| Self::compare_refs(a, b, &dir_names));

        // Sort each directory's items
        let dir_ids: Vec<Uuid> = self.directories.keys().copied().collect();
        for dir_id in dir_ids {
            if let Some(dir) = self.directories.get_mut(&dir_id) {
                dir.items
                    .sort_by(|a, b| Self::compare_refs(a, b, &dir_names));
            }
        }
    }

    fn compare_refs(
        a: &MergedItemRef,
        b: &MergedItemRef,
        dir_names: &HashMap<Uuid, Arc<str>>,
    ) -> std::cmp::Ordering {
        // Directories first
        match (a, b) {
            (MergedItemRef::Directory(_), MergedItemRef::File { .. }) => std::cmp::Ordering::Less,
            (MergedItemRef::File { .. }, MergedItemRef::Directory(_)) => std::cmp::Ordering::Greater,
            (MergedItemRef::Directory(a_id), MergedItemRef::Directory(b_id)) => {
                let a_name = dir_names.get(a_id).map(|n| n.as_ref()).unwrap_or("");
                let b_name = dir_names.get(b_id).map(|n| n.as_ref()).unwrap_or("");
                a_name.cmp(b_name)
            }
            // Files are sorted later when we have access to WadTrees
            (MergedItemRef::File { .. }, MergedItemRef::File { .. }) => std::cmp::Ordering::Equal,
        }
    }

    pub fn root_items(&self) -> &[MergedItemRef] {
        &self.root_items
    }

    pub fn get_directory(&self, id: &Uuid) -> Option<&MergedDirectory> {
        self.directories.get(id)
    }

    pub fn directories(&self) -> &HashMap<Uuid, MergedDirectory> {
        &self.directories
    }
}

// ============================================================================
// Game Explorer State
// ============================================================================

/// State for the game explorer feature
pub struct GameExplorer {
    /// Whether the game explorer is initialized
    is_initialized: bool,
    /// The base path of the league directory
    base_path: Option<Utf8PathBuf>,
    /// All mounted WAD trees (wad_id -> tree)
    wad_trees: IndexMap<Uuid, WadTree>,
    /// All mounted WADs (wad_id -> wad)
    wads: HashMap<Uuid, Wad<File>>,
    /// WAD metadata (wad_id -> (path, name))
    wad_metadata: HashMap<Uuid, (Utf8PathBuf, Arc<str>)>,
    /// Merged index for unified view
    merged_index: Option<MergedIndex>,
}

impl GameExplorer {
    pub fn new() -> Self {
        Self {
            is_initialized: false,
            base_path: None,
            wad_trees: IndexMap::new(),
            wads: HashMap::new(),
            wad_metadata: HashMap::new(),
            merged_index: None,
        }
    }

    /// Mount all WAD files from the league directory
    pub fn mount_from_directory(
        &mut self,
        league_directory: &Utf8Path,
        hashtable: &WadHashtable,
    ) -> Result<(), WadTreeError> {
        info!(
            "Mounting game explorer from directory: {}",
            league_directory
        );

        let start_time = std::time::Instant::now();

        // Clear existing state
        self.wad_trees.clear();
        self.wads.clear();
        self.wad_metadata.clear();
        self.merged_index = None;
        self.base_path = Some(league_directory.to_path_buf());

        // Find all .wad.client files
        let wad_paths: Vec<Utf8PathBuf> = WalkDir::new(league_directory)
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
            .filter_map(|e| Utf8PathBuf::from_path_buf(e.into_path()).ok())
            .collect();

        info!(
            "Found {} WAD files in {:?}",
            wad_paths.len(),
            start_time.elapsed()
        );

        // Process WADs in parallel
        let mount_start = std::time::Instant::now();
        let results: Vec<_> = wad_paths
            .par_iter()
            .filter_map(|wad_path| {
                let wad_id = Uuid::new_v4();
                let wad_name: Arc<str> = wad_path.file_name().unwrap_or("unknown").into();

                let file = match File::open(wad_path) {
                    Ok(f) => f,
                    Err(e) => {
                        tracing::warn!("Failed to open {}: {}", wad_path, e);
                        return None;
                    }
                };

                let mut wad = match Wad::mount(file) {
                    Ok(w) => w,
                    Err(e) => {
                        tracing::warn!("Failed to mount {}: {}", wad_path, e);
                        return None;
                    }
                };

                // Create tree using fast method (no decompression of unknown chunks)
                let tree = match WadTree::from_wad_fast(&mut wad, wad_id, wad_path.as_str(), hashtable)
                {
                    Ok(t) => t,
                    Err(e) => {
                        tracing::warn!("Failed to create tree for {}: {}", wad_path, e);
                        return None;
                    }
                };

                Some((wad_id, tree, wad, wad_path.clone(), wad_name))
            })
            .collect();

        info!(
            "Mounted {} WADs in {:?}",
            results.len(),
            mount_start.elapsed()
        );

        // Collect results into state
        for (wad_id, tree, wad, wad_path, wad_name) in results {
            self.wad_trees.insert(wad_id, tree);
            self.wads.insert(wad_id, wad);
            self.wad_metadata.insert(wad_id, (wad_path, wad_name));
        }

        // Build merged index
        let index_start = std::time::Instant::now();
        self.merged_index = Some(MergedIndex::build(&self.wad_trees));
        info!("Built merged index in {:?}", index_start.elapsed());

        self.is_initialized = true;

        info!(
            "Game explorer initialized with {} WADs in {:?} total",
            self.wad_trees.len(),
            start_time.elapsed()
        );

        Ok(())
    }

    // Accessors

    pub fn is_initialized(&self) -> bool {
        self.is_initialized
    }

    pub fn base_path(&self) -> Option<&Utf8Path> {
        self.base_path.as_deref()
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

    pub fn wad_metadata(&self) -> &HashMap<Uuid, (Utf8PathBuf, Arc<str>)> {
        &self.wad_metadata
    }

    pub fn merged_index(&self) -> Option<&MergedIndex> {
        self.merged_index.as_ref()
    }

    /// Get the WAD name for a given WAD ID
    pub fn get_wad_name(&self, wad_id: &Uuid) -> Option<&str> {
        self.wad_metadata.get(wad_id).map(|(_, name)| name.as_ref())
    }
}

pub struct GameExplorerState(pub Mutex<GameExplorer>);
