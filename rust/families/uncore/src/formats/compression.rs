// PORT-SOURCE: Families/GameX.Uncore/Formats/Compression.cs
// PORT-SHA: 7a4f65ff5733ab7c
// PORT-STATUS: partial
//
// C#'s `static class Compression` of `BinaryReader` extension methods is the `Compression`
// trait, blanket-implemented for every `Read + Seek`. SharpZipLib's `Inflater`/`Deflater`
// are flate2's `Decompress`/`Compress` (miniz_oxide backend); `noHeader` is raw deflate.
// `CompressZlib` isn't an extension method, so it's a free function after the trait.
//
// Ported: BufferSize, LZMAPropsSize, the Zlib region (DecompressZlibStream, DecompressZlib,
// DecompressZlib2, CompressZlib) and DecompressBlast.
//
// Not ported:
// - The `(byte[] source, byte[] target)` overloads of every region, including
//   `DecompressZlib(byte[], byte[])` and `DecompressBlast(byte[], byte[])` (the latter throws).
// - Doboz, Lzf, Lzss, Oodle and Xmem: their decompressors in `gamex::vendor::compression`
//   are still placeholders.
// - Lz4 (K4os), Lzo (lzo.net), Zstd (ZstdNet), Lzma (SevenZip) and Snappy (throws
//   NotSupportedException): no Rust dependency for them yet.

use flate2::{Compress, Decompress, FlushCompress, FlushDecompress, Status};
use gamex::vendor::compression::blast::Blast;
use openx_poly::io::reader::BinaryReaderExt;
use std::io::{self, Read, Seek, SeekFrom};

const BufferSize: usize = 4096 * 10;
pub const LZMAPropsSize: i32 = 5;

pub trait Compression: Read + Seek {
    //#region Zlib

    fn decompressZlibStream(&mut self, noHeader: bool) -> io::Result<Vec<u8>> {
        let (mut b, mut res) = (vec![0u8; 1024], vec![0u8; 1024]); let (mut outputUsed, mut count) = (0usize, 0usize);
        let mut z = Decompress::new(!noHeader);
        let mut input = 0usize; let mut isFinished = false; // Inflater's input window is b[input..count]
        while !isFinished {
            while input == count { count = self.read(&mut b)?; if count == 0 { return Err(io::Error::new(io::ErrorKind::UnexpectedEof, "Unexpected End Of File")); } input = 0; }
            if outputUsed == res.len() { res.resize(res.len() * 2, 0); }
            let (totalIn, totalOut) = (z.total_in(), z.total_out());
            let status = z.decompress(&b[input..count], &mut res[outputUsed..], FlushDecompress::None).map_err(|e| io::Error::new(io::ErrorKind::Other, e.to_string()))?;
            input += (z.total_in() - totalIn) as usize; outputUsed += (z.total_out() - totalOut) as usize;
            isFinished = status == Status::StreamEnd;
        }
        // Adjust reader to point to the next stream correctly.
        let position = self.stream_position()?; self.seek(SeekFrom::Start(position - (count - input) as u64))?;
        z.reset(!noHeader);
        // rebuild output
        res.truncate(outputUsed);
        Ok(res)
    }

    fn decompressZlib(&mut self, length: i32, newLength: i32, noHeader: bool) -> io::Result<Vec<u8>> {
        let data = self.readBytes(length as usize)?;
        let mut z = Decompress::new(!noHeader);
        let mut count: usize;
        let mut b = vec![0u8; BufferSize];
        let mut s = Vec::new();
        loop {
            let (totalIn, totalOut) = (z.total_in() as usize, z.total_out());
            z.decompress(&data[totalIn..], &mut b, FlushDecompress::None).map_err(|e| io::Error::new(io::ErrorKind::InvalidData, e.to_string()))?;
            count = (z.total_out() - totalOut) as usize;
            if count == 0 { break; }
            s.extend_from_slice(&b[..count]);
        }
        Ok(s)
    }

    fn decompressZlib2(&mut self, length: i32, newLength: i32) -> io::Result<Vec<u8>> {
        let data = self.readBytes(length as usize)?;
        let s = flate2::read::ZlibDecoder::new_with_buf(&data[..], vec![0u8; 4096]);
        let mut res = Vec::with_capacity(newLength as usize);
        s.take(newLength as u64).read_to_end(&mut res)?; res.resize(newLength as usize, 0);
        Ok(res)
    }

    //#endregion

    //#region Blast

    fn decompressBlast(&mut self, length: i32, newLength: i32) -> io::Result<Vec<u8>> {
        let mut z = Blast::new();
        let data = self.readBytes(length as usize)?;
        //var os = new byte[newLength];
        let mut os = Vec::with_capacity(newLength as usize);
        z.decompressStream(&data, &mut os)?;
        Ok(os)
    }

    //#endregion
}

impl<T: Read + Seek + ?Sized> Compression for T {}

// Zlib
pub fn compressZlib(source: &[u8], length: i32) -> io::Result<Vec<u8>> {
    let mut z = Compress::new(flate2::Compression::best(), true);
    let input = &source[..length as usize];
    let mut count: usize;
    let mut b = vec![0u8; BufferSize];
    let mut s = Vec::new();
    loop {
        let (totalIn, totalOut) = (z.total_in() as usize, z.total_out());
        z.compress(&input[totalIn..], &mut b, FlushCompress::None).map_err(|e| io::Error::new(io::ErrorKind::Other, e.to_string()))?;
        count = (z.total_out() - totalOut) as usize;
        if count == 0 { break; }
        s.extend_from_slice(&b[..count]);
    }
    Ok(s)
}
