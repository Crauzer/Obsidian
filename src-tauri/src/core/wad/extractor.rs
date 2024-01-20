use crate::{
    core::wad::{WadChunk, WadDecoder},
    state::WadHashtable,
};
use color_eyre::eyre::{self, Ok};
use eyre::Context;
use std::{
    collections::HashMap,
    fs::{self, DirBuilder},
    io::{self, Read, Seek},
    path::{Path, PathBuf},
};

pub fn prepare_extraction_directories_absolute<'chunks>(
    chunks: impl Iterator<Item = (&'chunks u64, &'chunks WadChunk)>,
    wad_hashtable: &WadHashtable,
    extraction_directory: impl AsRef<Path>,
) -> eyre::Result<()> {
    tracing::info!("preparing absolute extraction directories");

    // collect all chunk directories
    let chunk_directories = chunks.filter_map(|(_, chunk)| {
        Path::new(wad_hashtable.resolve_path(chunk.path_hash()).as_ref())
            .parent()
            .map(|path| path.to_path_buf())
    });

    create_extraction_directories(chunk_directories, wad_hashtable, extraction_directory)?;

    Ok(())
}

pub fn prepare_extraction_directories_relative<'chunks>(
    chunks: impl Iterator<Item = &'chunks WadChunk>,
    parent_path: &Path,
    wad_hashtable: &WadHashtable,
    extraction_directory: impl AsRef<Path>,
) -> eyre::Result<()> {
    tracing::info!("preparing relative extraction directories");

    // collect all chunk directories
    let chunk_directories = chunks.filter_map(|chunk| {
        Path::new(wad_hashtable.resolve_path(chunk.path_hash()).as_ref())
            .parent()
            .unwrap()
            .strip_prefix(parent_path)
            .map(|x| x.to_path_buf())
            .ok()
    });

    create_extraction_directories(chunk_directories, wad_hashtable, extraction_directory)?;

    Ok(())
}

fn create_extraction_directories(
    chunk_directories: impl Iterator<Item = impl AsRef<Path>>,
    wad_hashtable: &WadHashtable,
    extraction_directory: impl AsRef<Path>,
) -> eyre::Result<()> {
    // this wont error if the directory already exists since recursive mode is enabled
    for chunk_directory in chunk_directories {
        DirBuilder::new()
            .recursive(true)
            .create(extraction_directory.as_ref().join(chunk_directory))?;
    }

    Ok(())
}

pub fn extract_wad_chunks<TSource: Read + Seek>(
    decoder: &mut WadDecoder<TSource>,
    chunks: &HashMap<u64, WadChunk>,
    wad_hashtable: &WadHashtable,
    extract_directory: PathBuf,
    report_progress: impl Fn(f64, Option<&str>) -> eyre::Result<()>,
) -> eyre::Result<()> {
    tracing::info!("extracting chunks");

    let mut i = 0;
    for (_, chunk) in chunks {
        let chunk_path = wad_hashtable.resolve_path(chunk.path_hash());
        let chunk_path = Path::new(chunk_path.as_ref());

        report_progress(i as f64 / chunks.len() as f64, chunk_path.to_str())?;

        extract_wad_chunk_absolute(decoder, &chunk, &chunk_path, &extract_directory)?;

        i = i + 1;
    }

    Ok(())
}

pub fn extract_wad_chunks_relative<TSource: Read + Seek>(
    decoder: &mut WadDecoder<TSource>,
    chunks: &Vec<WadChunk>,
    base_directory: &Path,
    wad_hashtable: &WadHashtable,
    extract_directory: PathBuf,
    report_progress: impl Fn(f64, Option<&str>) -> eyre::Result<()>,
) -> eyre::Result<()> {
    tracing::info!("extracting chunks");

    let mut i = 0;
    for chunk in chunks {
        let chunk_path = wad_hashtable.resolve_path(chunk.path_hash());
        let chunk_path = Path::new(chunk_path.as_ref());

        report_progress(i as f64 / chunks.len() as f64, chunk_path.to_str())?;

        extract_wad_chunk_absolute(
            decoder,
            &chunk,
            &chunk_path.strip_prefix(base_directory)?,
            &extract_directory,
        )?;

        i = i + 1;
    }

    Ok(())
}

pub fn extract_wad_chunk_absolute<'wad, TSource: Read + Seek>(
    decoder: &mut WadDecoder<'wad, TSource>,
    chunk: &WadChunk,
    chunk_path: impl AsRef<Path>,
    extract_directory: impl AsRef<Path>,
) -> eyre::Result<()> {
    let chunk_data = decoder.load_chunk_decompressed(chunk).wrap_err(format!(
        "failed to decompress chunk (chunk_path: {})",
        chunk_path.as_ref().display()
    ))?;

    let Err(error) = fs::write(&extract_directory.as_ref().join(&chunk_path), &chunk_data) else {
        return Ok(());
    };
    if error.kind() != io::ErrorKind::InvalidFilename {
        return Err(error).wrap_err(format!(
            "failed to write chunk (chunk_path: {})",
            chunk_path.as_ref().display()
        ));
    }

    let hashed_path = format!("{:#0x}", chunk.path_hash());
    let hashed_path = Path::new(&hashed_path);

    tracing::warn!(
        "invalid chunk filename, writing as {}",
        hashed_path.display()
    );
    fs::write(&extract_directory.as_ref().join(hashed_path), &chunk_data)?;

    Ok(())
}

pub fn extract_wad_chunk_relative<'wad, TSource: Read + Seek>(
    decoder: &mut WadDecoder<'wad, TSource>,
    chunk: &WadChunk,
    chunk_path: impl AsRef<Path>,
    extract_directory: impl AsRef<Path>,
) -> eyre::Result<()> {
    let chunk_data = decoder.load_chunk_decompressed(chunk).wrap_err(format!(
        "failed to decompress chunk (chunk_path: {})",
        chunk_path.as_ref().display()
    ))?;
    let chunk_name = chunk_path.as_ref().file_name().unwrap();

    if let Err(error) = fs::write(&extract_directory.as_ref().join(chunk_name), &chunk_data) {
        if error.kind() != io::ErrorKind::InvalidFilename {
            return Err(error).wrap_err(format!(
                "failed to write chunk (chunk_path: {})",
                chunk_path.as_ref().display()
            ));
        }

        let hashed_path = format!("{:#0x}", chunk.path_hash());
        let hashed_path = Path::new(&hashed_path);

        tracing::warn!(
            "invalid chunk filename, writing as {}",
            hashed_path.display()
        );

        fs::write(&extract_directory.as_ref().join(chunk_name), &chunk_data)?;
    }

    Ok(())
}
