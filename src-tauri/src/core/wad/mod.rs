use byteorder::{ReadBytesExt, LE};
use flate2::read::GzDecoder;
use memchr::memmem;
use num_enum::{IntoPrimitive, TryFromPrimitive};
use std::{
    collections::HashMap,
    io::{BufReader, Read, Seek, SeekFrom},
    vec,
};

mod error;
mod extractor;
pub mod tree;

pub use error::*;
pub use extractor::*;

#[derive(Debug)]
pub struct Wad<TSource: Read + Seek> {
    chunks: HashMap<u64, WadChunk>,
    source: TSource,
}

#[derive(Clone, Copy, Debug, PartialEq, Eq)]
pub struct WadChunk {
    path_hash: u64,
    data_offset: usize,
    compressed_size: usize,
    uncompressed_size: usize,
    compression_type: WadChunkCompression,
    is_duplicated: bool,
    frame_count: u8,
    start_frame: u16,
    checksum: u64,
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, TryFromPrimitive, IntoPrimitive)]
#[repr(u8)]
pub enum WadChunkCompression {
    None = 0,
    GZip = 1,
    Satellite = 2,
    Zstd = 3,
    ZstdMulti = 4,
}

const ZSTD_MAGIC: [u8; 4] = [0x28, 0xB5, 0x2F, 0xFD];

impl<TSource: Read + Seek> Wad<TSource> {
    pub fn chunks(&self) -> &HashMap<u64, WadChunk> {
        &self.chunks
    }

    pub fn mount(mut source: TSource) -> Result<Wad<TSource>, WadError> {
        let mut reader = BufReader::new(&mut source);

        // 0x5752 = "RW"
        let magic = reader.read_u16::<LE>()?;
        if magic != 0x5752 {
            return Err(WadError::InvalidHeader {
                expected: String::from("RW"),
                actual: format!("0x{:x}", magic),
            });
        }

        let major = reader.read_u8()?;
        let minor = reader.read_u8()?;
        if major > 3 {
            return Err(WadError::InvalidVersion { major, minor });
        }

        if major == 2 {
            let _ecdsa_length = reader.seek(SeekFrom::Current(1))?;
            let _ecdsa_signature = reader.seek(SeekFrom::Current(83))?;
            let _data_checksum = reader.seek(SeekFrom::Current(8))?;
        } else if major == 3 {
            let _ecdsa_signature = reader.seek(SeekFrom::Current(256))?;
            let _data_checksum = reader.seek(SeekFrom::Current(8))?;
        }

        if major == 1 || major == 2 {
            let _toc_start_offset = reader.seek(SeekFrom::Current(2))?;
            let _toc_chunk_size = reader.seek(SeekFrom::Current(2))?;
        }

        let chunk_count = reader.read_i32::<LE>()? as usize;
        let mut chunks = HashMap::<u64, WadChunk>::with_capacity(chunk_count);
        for _ in 0..chunk_count {
            let chunk = WadChunk::read(&mut reader).expect("failed to read chunk");
            chunks
                .insert(chunk.path_hash(), chunk)
                .map_or(Ok(()), |chunk| {
                    Err(WadError::DuplicateChunk {
                        path_hash: chunk.path_hash(),
                    })
                })?;
        }

        Ok(Wad { chunks, source })
    }

    pub fn decode<'wad>(&'wad mut self) -> (WadDecoder<'wad, TSource>, &HashMap<u64, WadChunk>) {
        (
            WadDecoder {
                source: &mut self.source,
            },
            &self.chunks,
        )
    }
}

impl WadChunk {
    fn read<R: Read>(reader: &mut BufReader<R>) -> Result<WadChunk, WadError> {
        let path_hash = reader.read_u64::<LE>()?;
        let data_offset = reader.read_u32::<LE>()? as usize;
        let compressed_size = reader.read_i32::<LE>()? as usize;
        let uncompressed_size = reader.read_i32::<LE>()? as usize;

        let type_frame_count = reader.read_u8()?;
        let frame_count = type_frame_count >> 4;
        let compression_type = WadChunkCompression::try_from_primitive(type_frame_count & 0xF)
            .expect("failed to read chunk compression");

        let is_duplicated = reader.read_u8()? == 1;
        let start_frame = reader.read_u16::<LE>()?;
        let checksum = reader.read_u64::<LE>()?;

        Ok(WadChunk {
            path_hash,
            data_offset,
            compressed_size,
            uncompressed_size,
            compression_type,
            is_duplicated,
            frame_count,
            start_frame,
            checksum,
        })
    }

    pub fn path_hash(&self) -> u64 {
        self.path_hash
    }
    pub fn data_offset(&self) -> usize {
        self.data_offset
    }
    pub fn compressed_size(&self) -> usize {
        self.compressed_size
    }
    pub fn uncompressed_size(&self) -> usize {
        self.uncompressed_size
    }
    pub fn compression_type(&self) -> WadChunkCompression {
        self.compression_type
    }
    pub fn checksum(&self) -> u64 {
        self.checksum
    }
}

pub struct WadDecoder<'wad, TSource: Read + Seek> {
    source: &'wad mut TSource,
}

impl<'wad, TSource> WadDecoder<'wad, TSource>
where
    TSource: Read + Seek,
{
    pub fn load_chunk_raw(&mut self, chunk: &WadChunk) -> Result<Box<[u8]>, WadError> {
        let mut data = vec![0; chunk.compressed_size];

        self.source
            .seek(SeekFrom::Start(chunk.data_offset as u64))?;
        self.source.read_exact(&mut data)?;

        Ok(data.into_boxed_slice())
    }
    pub fn load_chunk_decompressed(&mut self, chunk: &WadChunk) -> Result<Box<[u8]>, WadError> {
        match chunk.compression_type {
            WadChunkCompression::None => self.load_chunk_raw(chunk),
            WadChunkCompression::GZip => self.decode_gzip_chunk(chunk),
            WadChunkCompression::Satellite => Err(WadError::Other(String::from(
                "satellite chunks are not supported",
            ))),
            WadChunkCompression::Zstd => self.decode_zstd_chunk(chunk),
            WadChunkCompression::ZstdMulti => self.decode_zstd_multi_chunk(chunk),
        }
    }

    fn decode_gzip_chunk(&mut self, chunk: &WadChunk) -> Result<Box<[u8]>, WadError> {
        self.source
            .seek(SeekFrom::Start(chunk.data_offset as u64))?;

        let mut data = vec![0; chunk.uncompressed_size];
        GzDecoder::new(&mut self.source).read_exact(&mut data)?;

        Ok(data.into_boxed_slice())
    }
    fn decode_zstd_chunk(&mut self, chunk: &WadChunk) -> Result<Box<[u8]>, WadError> {
        self.source
            .seek(SeekFrom::Start(chunk.data_offset as u64))?;

        let mut data: Vec<u8> = vec![0; chunk.uncompressed_size];
        zstd::Decoder::new(&mut self.source)
            .expect("failed to create zstd decoder")
            .read_exact(&mut data)?;

        Ok(data.into_boxed_slice())
    }
    fn decode_zstd_multi_chunk(&mut self, chunk: &WadChunk) -> Result<Box<[u8]>, WadError> {
        let raw_data = self.load_chunk_raw(chunk)?;
        let mut data: Vec<u8> = vec![0; chunk.uncompressed_size];

        let zstd_magic_offset =
            memmem::find(&raw_data, &ZSTD_MAGIC).ok_or(WadError::DecompressionFailure {
                path_hash: chunk.path_hash,
                reason: String::from("failed to find zstd magic"),
            })?;

        // copy raw uncompressed data which exists before first zstd frame
        for (i, value) in raw_data[0..zstd_magic_offset].iter().enumerate() {
            data[i] = *value;
        }

        // seek to start of first zstd frame
        self.source.seek(SeekFrom::Start(
            (chunk.data_offset + zstd_magic_offset) as u64,
        ))?;

        // decode zstd data
        zstd::Decoder::new(&mut self.source)
            .expect("failed to create zstd decoder")
            .read_exact(&mut data[zstd_magic_offset..])?;

        Ok(data.into_boxed_slice())
    }
}
