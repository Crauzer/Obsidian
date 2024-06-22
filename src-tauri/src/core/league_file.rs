use serde::{Deserialize, Serialize};

static LEAGUE_FILE_MAGIC_BYTES: &[LeagueFileMagicBytes] = &[
    LeagueFileMagicBytes::from_bytes(b"r3d2Mesh", LeagueFileKind::StaticMeshBinary),
    LeagueFileMagicBytes::from_bytes(b"r3d2sklt", LeagueFileKind::Skeleton),
    LeagueFileMagicBytes::from_bytes(b"r3d2ammd", LeagueFileKind::Animation),
    LeagueFileMagicBytes::from_bytes(b"r3d2canm", LeagueFileKind::Animation),
    LeagueFileMagicBytes::from_fn(
        |data| u32::from_le_bytes(data[4..8].try_into().unwrap()) == 1,
        8,
        LeagueFileKind::WwisePackage,
    ),
    LeagueFileMagicBytes::from_fn(|data| &data[1..4] == b"PNG", 4, LeagueFileKind::Png),
    LeagueFileMagicBytes::from_bytes(b"DDS ", LeagueFileKind::TextureDds),
    LeagueFileMagicBytes::from_bytes(&[0x33, 0x22, 0x11, 0x00], LeagueFileKind::SimpleSkin),
    LeagueFileMagicBytes::from_bytes(b"PROP", LeagueFileKind::PropertyBin),
    LeagueFileMagicBytes::from_bytes(b"BKHD", LeagueFileKind::WwiseBank),
    LeagueFileMagicBytes::from_bytes(b"WGEO", LeagueFileKind::WorldGeometry),
    LeagueFileMagicBytes::from_bytes(b"OEGM", LeagueFileKind::MapGeometry),
    LeagueFileMagicBytes::from_bytes(b"[Obj", LeagueFileKind::StaticMeshAscii),
    LeagueFileMagicBytes::from_fn(|data| &data[1..5] == b"LuaQ", 5, LeagueFileKind::LuaObj),
    LeagueFileMagicBytes::from_bytes(b"PreLoad", LeagueFileKind::Preload),
    LeagueFileMagicBytes::from_fn(
        |data| u32::from_le_bytes(data[..4].try_into().unwrap()) == 3,
        4,
        LeagueFileKind::LightGrid,
    ),
    LeagueFileMagicBytes::from_bytes(b"RST", LeagueFileKind::RiotStringTable),
    LeagueFileMagicBytes::from_bytes(b"PTCH", LeagueFileKind::PropertyBinOverride),
    LeagueFileMagicBytes::from_fn(
        |data| ((u32::from_le_bytes(data[..4].try_into().unwrap()) & 0x00FFFFFF) == 0x00FFD8FF),
        3,
        LeagueFileKind::Jpeg,
    ),
    LeagueFileMagicBytes::from_fn(
        |data| u32::from_le_bytes(data[4..8].try_into().unwrap()) == 0x22FD4FC3,
        8,
        LeagueFileKind::Skeleton,
    ),
    LeagueFileMagicBytes::from_bytes(b"TEX\0", LeagueFileKind::Texture),
];

#[derive(Debug, Serialize, Deserialize, Clone, Copy, PartialEq, Eq, PartialOrd, Ord, Hash)]
#[serde(rename_all = "snake_case")]
pub enum LeagueFileKind {
    Animation,
    Jpeg,
    LightGrid,
    LuaObj,
    MapGeometry,
    Png,
    Preload,
    PropertyBin,
    PropertyBinOverride,
    RiotStringTable,
    SimpleSkin,
    Skeleton,
    StaticMeshAscii,
    StaticMeshBinary,
    Texture,
    TextureDds,
    Unknown,
    WorldGeometry,
    WwiseBank,
    WwisePackage,
}

enum LeagueFileMagicBytesPattern {
    Bytes(&'static [u8]),
    Fn(fn(&[u8]) -> bool),
}

struct LeagueFileMagicBytes {
    pattern: LeagueFileMagicBytesPattern,
    min_length: usize,
    kind: LeagueFileKind,
}

impl LeagueFileMagicBytes {
    const fn from_bytes(bytes: &'static [u8], kind: LeagueFileKind) -> Self {
        Self {
            pattern: LeagueFileMagicBytesPattern::Bytes(bytes),
            min_length: bytes.len(),
            kind,
        }
    }

    const fn from_fn(f: fn(&[u8]) -> bool, min_length: usize, kind: LeagueFileKind) -> Self {
        Self {
            pattern: LeagueFileMagicBytesPattern::Fn(f),
            min_length,
            kind,
        }
    }

    fn matches(&self, data: &[u8]) -> bool {
        data.len() >= self.min_length
            && match self.pattern {
                LeagueFileMagicBytesPattern::Bytes(bytes) => &data[..bytes.len()] == bytes,
                LeagueFileMagicBytesPattern::Fn(f) => f(data),
            }
    }
}

pub fn get_league_file_kind_from_extension(mut extension: &str) -> LeagueFileKind {
    if extension.len() == 0 {
        return LeagueFileKind::Unknown;
    }

    if extension.starts_with('.') {
        extension = &extension[1..];
    }

    match extension {
        "anm" => LeagueFileKind::Animation,
        "bin" => LeagueFileKind::PropertyBin,
        "bnk" => LeagueFileKind::WwiseBank,
        "dds" => LeagueFileKind::TextureDds,
        "jpg" => LeagueFileKind::Jpeg,
        "luaobj" => LeagueFileKind::LuaObj,
        "mapgeo" => LeagueFileKind::MapGeometry,
        "png" => LeagueFileKind::Png,
        "preload" => LeagueFileKind::Preload,
        "scb" => LeagueFileKind::StaticMeshBinary,
        "sco" => LeagueFileKind::StaticMeshAscii,
        "skl" => LeagueFileKind::Skeleton,
        "skn" => LeagueFileKind::SimpleSkin,
        "stringtable" => LeagueFileKind::RiotStringTable,
        "tex" => LeagueFileKind::Texture,
        "wgeo" => LeagueFileKind::WorldGeometry,
        "wpk" => LeagueFileKind::WwisePackage,
        _ => LeagueFileKind::Unknown,
    }
}

pub fn get_extension_from_league_file_kind(kind: LeagueFileKind) -> &'static str {
    match kind {
        LeagueFileKind::Animation => "anm",
        LeagueFileKind::Jpeg => "jpg",
        LeagueFileKind::LightGrid => "lightgrid",
        LeagueFileKind::LuaObj => "luaobj",
        LeagueFileKind::MapGeometry => "mapgeo",
        LeagueFileKind::Png => "png",
        LeagueFileKind::Preload => "preload",
        LeagueFileKind::PropertyBin => "bin",
        LeagueFileKind::PropertyBinOverride => "bin",
        LeagueFileKind::RiotStringTable => "stringtable",
        LeagueFileKind::SimpleSkin => "skn",
        LeagueFileKind::Skeleton => "skl",
        LeagueFileKind::StaticMeshAscii => "sco",
        LeagueFileKind::StaticMeshBinary => "scb",
        LeagueFileKind::Texture => "tex",
        LeagueFileKind::TextureDds => "dds",
        LeagueFileKind::Unknown => "",
        LeagueFileKind::WorldGeometry => "wgeo",
        LeagueFileKind::WwiseBank => "bnk",
        LeagueFileKind::WwisePackage => "wpk",
    }
}

pub fn identify_league_file(data: &Box<[u8]>) -> LeagueFileKind {
    for magic_byte in LEAGUE_FILE_MAGIC_BYTES.iter() {
        if magic_byte.matches(data) {
            return magic_byte.kind;
        }
    }

    return LeagueFileKind::Unknown;
}
