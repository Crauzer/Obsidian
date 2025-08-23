mod commands;

pub use commands::*;
use league_toolkit::{file::LeagueFileKind, wad::WadChunkCompression};

use crate::core::wad::tree::{WadTreeDirectory, WadTreeFile, WadTreeItem, WadTreePathable};
use serde::{self, Deserialize, Serialize};
use std::path::Path;
use uuid::Uuid;

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
pub struct SearchWadResponse {
    pub items: Vec<SearchWadResponseItem>,
}

#[derive(Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct SearchWadResponseItem {
    pub id: Uuid,
    pub parent_id: Option<Uuid>,
    pub name: String,
    pub path: String,
    pub extension_kind: LeagueFileKind,
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
    pub item: usize,
    pub is_selected: bool,
}

#[derive(Debug, Serialize, Deserialize)]
#[serde(rename_all = "snake_case")]
pub enum WadChunkCompressionDto {
    None,
    GZip,
    Satellite,
    Zstd,
    ZstdMulti,
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

        compression_kind: WadChunkCompressionDto,
        compressed_size: usize,
        uncompressed_size: usize,

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
            compression_kind: value.chunk().compression_type().into(),
            compressed_size: value.chunk().compressed_size(),
            uncompressed_size: value.chunk().uncompressed_size(),
            extension_kind: guess_file_kind(value.name()),
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

impl From<WadChunkCompression> for WadChunkCompressionDto {
    fn from(value: WadChunkCompression) -> Self {
        match value {
            WadChunkCompression::None => Self::None,
            WadChunkCompression::GZip => Self::GZip,
            WadChunkCompression::Satellite => Self::Satellite,
            WadChunkCompression::Zstd => Self::Zstd,
            WadChunkCompression::ZstdMulti => Self::ZstdMulti,
        }
    }
}

fn guess_file_kind(file_name: impl AsRef<str>) -> LeagueFileKind {
    if let Some(extension) = Path::new(file_name.as_ref()).extension() {
        if let Some(extension) = extension.to_str() {
            return LeagueFileKind::from_extension(extension);
        }
    }

    LeagueFileKind::Unknown
}
