use serde::{Deserialize, Serialize};

#[derive(Debug, Serialize, Deserialize)]
pub enum LeagueFileKind {
    Animation,
    Jpeg,
    LuaObj,
    MapGeometry,
    Png,
    Preload,
    PropertyBin,
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
