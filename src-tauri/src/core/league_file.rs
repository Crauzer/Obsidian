use serde::{Deserialize, Serialize};

static LEAGUE_FILE_MAGIC_BYTES: &[LeagueFilePattern] = &[
    LeagueFilePattern::from_bytes(b"r3d2Mesh", LeagueFileKind::StaticMeshBinary),
    LeagueFilePattern::from_bytes(b"r3d2sklt", LeagueFileKind::Skeleton),
    LeagueFilePattern::from_bytes(b"r3d2ammd", LeagueFileKind::Animation),
    LeagueFilePattern::from_bytes(b"r3d2canm", LeagueFileKind::Animation),
    LeagueFilePattern::from_fn(
        |data| u32::from_le_bytes(data[4..8].try_into().unwrap()) == 1,
        8,
        LeagueFileKind::WwisePackage,
    ),
    LeagueFilePattern::from_fn(|data| &data[1..4] == b"PNG", 4, LeagueFileKind::Png),
    LeagueFilePattern::from_bytes(b"DDS ", LeagueFileKind::TextureDds),
    LeagueFilePattern::from_bytes(&[0x33, 0x22, 0x11, 0x00], LeagueFileKind::SimpleSkin),
    LeagueFilePattern::from_bytes(b"PROP", LeagueFileKind::PropertyBin),
    LeagueFilePattern::from_bytes(b"BKHD", LeagueFileKind::WwiseBank),
    LeagueFilePattern::from_bytes(b"WGEO", LeagueFileKind::WorldGeometry),
    LeagueFilePattern::from_bytes(b"OEGM", LeagueFileKind::MapGeometry),
    LeagueFilePattern::from_bytes(b"[Obj", LeagueFileKind::StaticMeshAscii),
    LeagueFilePattern::from_fn(|data| &data[1..5] == b"LuaQ", 5, LeagueFileKind::LuaObj),
    LeagueFilePattern::from_bytes(b"PreLoad", LeagueFileKind::Preload),
    LeagueFilePattern::from_fn(
        |data| u32::from_le_bytes(data[..4].try_into().unwrap()) == 3,
        4,
        LeagueFileKind::LightGrid,
    ),
    LeagueFilePattern::from_bytes(b"RST", LeagueFileKind::RiotStringTable),
    LeagueFilePattern::from_bytes(b"PTCH", LeagueFileKind::PropertyBinOverride),
    LeagueFilePattern::from_fn(
        |data| ((u32::from_le_bytes(data[..4].try_into().unwrap()) & 0x00FFFFFF) == 0x00FFD8FF),
        3,
        LeagueFileKind::Jpeg,
    ),
    LeagueFilePattern::from_fn(
        |data| u32::from_le_bytes(data[4..8].try_into().unwrap()) == 0x22FD4FC3,
        8,
        LeagueFileKind::Skeleton,
    ),
    LeagueFilePattern::from_bytes(b"TEX\0", LeagueFileKind::Texture),
    LeagueFilePattern::from_bytes(b"<svg", LeagueFileKind::Svg),
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
    Svg,
    Texture,
    TextureDds,
    Unknown,
    WorldGeometry,
    WwiseBank,
    WwisePackage,
}

enum LeagueFilePatternKind {
    Bytes(&'static [u8]),
    Fn(fn(&[u8]) -> bool),
}

struct LeagueFilePattern {
    pattern: LeagueFilePatternKind,
    min_length: usize,
    kind: LeagueFileKind,
}

impl LeagueFilePattern {
    const fn from_bytes(bytes: &'static [u8], kind: LeagueFileKind) -> Self {
        Self {
            pattern: LeagueFilePatternKind::Bytes(bytes),
            min_length: bytes.len(),
            kind,
        }
    }

    const fn from_fn(f: fn(&[u8]) -> bool, min_length: usize, kind: LeagueFileKind) -> Self {
        Self {
            pattern: LeagueFilePatternKind::Fn(f),
            min_length,
            kind,
        }
    }

    fn matches(&self, data: &[u8]) -> bool {
        data.len() >= self.min_length
            && match self.pattern {
                LeagueFilePatternKind::Bytes(bytes) => &data[..bytes.len()] == bytes,
                LeagueFilePatternKind::Fn(f) => f(data),
            }
    }
}

pub fn get_league_file_kind_from_extension(extension: impl AsRef<str>) -> LeagueFileKind {
    if extension.as_ref().len() == 0 {
        return LeagueFileKind::Unknown;
    }

    match match extension.as_ref().starts_with('.') {
        true => &extension.as_ref()[1..],
        false => extension.as_ref(),
    } {
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
        "svg" => LeagueFileKind::Svg,
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
        LeagueFileKind::Svg => "svg",
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
