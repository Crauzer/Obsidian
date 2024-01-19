use crate::{
    core::wad::{WadChunk, WadDecoder},
    state::WadHashtable,
};
use color_eyre::eyre::{self};
use eyre::Context;
use std::{
    collections::HashMap,
    fs::{self, DirBuilder},
    io::{self, Read, Seek},
    path::{Path, PathBuf},
};
use tracing::{error, info, warn};

pub fn prepare_extraction_directories<'chunks>(
    chunks: impl Iterator<Item = (&'chunks u64, &'chunks WadChunk)>,
    wad_hashtable: &WadHashtable,
    extraction_directory: impl AsRef<Path>,
) -> eyre::Result<()> {
    info!("preparing extraction directories");

    // collect all chunk directories
    let chunk_directories = chunks.map(|(_, chunk)| {
        Path::new(wad_hashtable.resolve_path(chunk.path_hash()).as_ref())
            .parent()
            .map(|path| path.to_path_buf())
    });

    // create all chunk directories
    // this wont error if the directory already exists since recursive mode is enabled
    for chunk_directory in chunk_directories {
        if let Some(chunk_directory) = chunk_directory {
            DirBuilder::new()
                .recursive(true)
                .create(extraction_directory.as_ref().join(chunk_directory))?;
        }
    }

    Ok(())
}

pub fn extract_wad_chunks<'chunks, TSource: Read + Seek>(
    decoder: &mut WadDecoder<TSource>,
    chunks: &HashMap<u64, WadChunk>,
    wad_hashtable: &WadHashtable,
    extract_directory: PathBuf,
    report_progress: impl Fn(f64, Option<&str>) -> eyre::Result<()>,
) -> eyre::Result<()> {
    info!("extracting chunks");

    let mut i = 0;
    for (_, chunk) in chunks {
        let chunk_path = wad_hashtable.resolve_path(chunk.path_hash());
        let chunk_path = Path::new(chunk_path.as_ref());

        report_progress(i as f64 / chunks.len() as f64, chunk_path.to_str())?;

        extract_wad_chunk(decoder, &chunk, &chunk_path, &extract_directory)?;

        i = i + 1;
    }

    Ok(())
}

pub fn extract_wad_chunk<'wad, TSource: Read + Seek>(
    decoder: &mut WadDecoder<'wad, TSource>,
    chunk: &WadChunk,
    chunk_path: impl AsRef<Path>,
    extract_directory: impl AsRef<Path>,
) -> eyre::Result<()> {
    let chunk_data = decoder.load_chunk_decompressed(chunk).wrap_err(format!(
        "failed to decompress chunk (chunk_path: {})",
        chunk_path.as_ref().display()
    ))?;

    if let Err(error) = fs::write(&extract_directory.as_ref().join(&chunk_path), &chunk_data) {
        if error.kind() == io::ErrorKind::InvalidFilename {
            let hashed_path = format!("{:#0x}", chunk.path_hash());
            let hashed_path = Path::new(&hashed_path);

            warn!(
                "invalid chunk filename, writing as {}",
                hashed_path.display()
            );

            fs::write(&extract_directory.as_ref().join(hashed_path), &chunk_data)?;
        } else {
            error!("{:#?}", error);
        }
    }

    Ok(())
}
