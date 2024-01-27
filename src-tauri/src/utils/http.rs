use std::{cmp::min, fs::File, io::Write, path::Path};

use color_eyre::eyre::{self, eyre};
use futures_util::StreamExt;
use reqwest::IntoUrl;

pub async fn download_file(
    url: impl IntoUrl,
    path: impl AsRef<Path>,
    mut on_progress: impl FnMut(usize, usize),
) -> eyre::Result<()> {
    let response = reqwest::get(url).await?;

    let total_size = response
        .content_length()
        .ok_or(eyre!("failed to get content length"))? as usize;

    let mut file = File::create(path)?;
    let mut response_stream = response.bytes_stream();
    let mut downloaded_bytes = 0usize;

    while let Some(item) = response_stream.next().await {
        let chunk = item?;

        file.write_all(&chunk)?;

        downloaded_bytes = min(downloaded_bytes + (chunk.len()), total_size);
        on_progress(downloaded_bytes, total_size);
    }

    Ok(())
}
