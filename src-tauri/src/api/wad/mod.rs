use std::path::Path;

use serde::{self, Deserialize, Serialize};
use uuid::Uuid;

use crate::core::{
    league_file::{get_league_file_kind_from_extension, LeagueFileKind},
    wad::tree::{WadTreeDirectory, WadTreeFile, WadTreeItem, WadTreeItemKind, WadTreePathable},
};

mod commands;

pub use commands::*;

#[derive(Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct MountedWadsResponse {
    pub wads: Vec<MountedWadDto>,
}

#[derive(Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct MountWadResponse {
    pub wad_ids: Vec<Uuid>,
}

#[derive(Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct MountedWadDto {
    pub id: Uuid,
    pub name: String,
    pub wad_path: String,
}

#[derive(Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct WadItemPathComponentDto {
    pub item_id: Uuid,
    pub name: String,
    pub path: String,
}

#[derive(Debug, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct WadItemSelectionUpdate {
    pub index: usize,
    pub is_selected: bool,
}

#[derive(Serialize, Deserialize)]
#[serde(rename_all = "camelCase", tag = "kind")]
pub enum WadItemDto {
    #[serde(rename_all = "camelCase")]
    File {
        id: Uuid,
        name: String,
        path: String,
        name_hash: u64,
        path_hash: u64,

        extension_kind: LeagueFileKind,
        is_selected: bool,
        is_checked: bool,
    },
    #[serde(rename_all = "camelCase")]
    Directory {
        id: Uuid,
        name: String,
        path: String,
        name_hash: u64,
        path_hash: u64,

        is_selected: bool,
        is_checked: bool,
        is_expanded: bool,
    },
}

impl From<&WadTreeItem> for WadItemDto {
    fn from(value: &WadTreeItem) -> Self {
        match value {
            WadTreeItem::File(file) => Self::from(file),
            WadTreeItem::Directory(directory) => Self::from(directory),
        }
    }
}

impl From<&WadTreeFile> for WadItemDto {
    fn from(value: &WadTreeFile) -> Self {
        Self::File {
            id: value.id(),
            name: value.name().to_string(),
            path: value.path().to_string(),
            name_hash: value.name_hash(),
            path_hash: value.path_hash(),
            extension_kind: guess_file_kind(value),
            is_selected: value.is_selected(),
            is_checked: value.is_checked(),
        }
    }
}

impl From<&WadTreeDirectory> for WadItemDto {
    fn from(value: &WadTreeDirectory) -> Self {
        Self::Directory {
            id: value.id(),
            name: value.name().to_string(),
            path: value.path().to_string(),
            name_hash: value.name_hash(),
            path_hash: value.path_hash(),
            is_selected: value.is_selected(),
            is_checked: value.is_checked(),
            is_expanded: value.is_expanded(),
        }
    }
}

fn guess_file_kind(file: &WadTreeFile) -> LeagueFileKind {
    if let Some(extension) = Path::new(file.name()).extension() {
        if let Some(extension) = extension.to_str() {
            return get_league_file_kind_from_extension(extension);
        }
    }

    LeagueFileKind::Unknown
}
