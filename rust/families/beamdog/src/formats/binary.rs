// PORT-SOURCE: Families/GameX.Beamdog/Formats/Binary.cs
// PORT-SHA: 39d9e56345f6a781
// PORT-STATUS: done
//
// Binary_Key, Binary_Dat, and Binary_Da2 are regions of this file.
// Left out of the C# reader too: tileset payloads, KEY V1.1, "BIF " V1.0
// (.cbf), and encrypted bifs. Binary_Dat throws unless the magic is a zip
// local header. Binary_Abc and Binary_XXX stay the unused stubs.

use std::any::Any;
use std::collections::HashMap;
use std::io::{self, Cursor, Error, ErrorKind, Read, Seek, SeekFrom};
use std::sync::atomic::Ordering;
use std::sync::{Arc, LazyLock};
use gamex::binary::{ArcBinary, BinaryArchive, Stream, Task};
use gamex::meta::{directoryName, extension, FileSource, IHaveMetaInfo, MetaInfo, MetaManager, Tag};
use gamex_uncore::formats::binary::Binary_Zip;
use gamex_uncore::formats::compression::Compression;
use openx_vfx::vfx::VfsRead;

// #region Binary_Abc

pub struct Binary_Abc;

impl Binary_Abc {
    pub fn new(_r: &mut dyn VfsRead) -> Self { Self }
}

impl IHaveMetaInfo for Binary_Abc {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, _file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> {
        vec![MetaInfo::new(Some("BinaryPak".into()), Tag::None, vec![], false)]
    }
}

// #endregion

// #region Binary_Key

// Left out of the C# reader too: tileset payloads, KEY V1.1, "BIF " V1.0
// (.cbf: a filename length, the filename, then one zlib stream), and encrypted bifs.
// BIFC is IESDP V1.0 blocks: version, uncompressed size, then declen/complen/zlib.
// https://gibberlings3.github.io/iesdp/file_formats/ie_formats/key_v1.htm
// https://gibberlings3.github.io/iesdp/file_formats/ie_formats/bif_v1.htm

const KEY_MAGIC: u32 = 0x2059454B; // "KEY "
const KEY_VERSION: u32 = 0x20203156; // "V1  "
const BIFF_MAGIC: u32 = 0x46464942; // "BIFF"
const BIFF_VERSION: u32 = 0x20203156; // "V1  "
const BIFC_MAGIC: u32 = 0x43464942; // "BIFC"
const BIFC_VERSION: u32 = 0x302E3156; // "V1.0"

pub struct Binary_Key;

impl Binary_Key {
    pub fn current() -> Arc<dyn ArcBinary> {
        static CURRENT: LazyLock<Arc<dyn ArcBinary>> = LazyLock::new(|| Arc::new(Binary_Key));
        CURRENT.clone()
    }
}

impl ArcBinary for Binary_Key {
    fn read<'a>(&'a self, source: &'a BinaryArchive, r: Option<&'a mut dyn VfsRead>, _tag: &'a Tag) -> Task<'a, ()> {
        Box::pin(async move {
            let r = r.ok_or_else(|| Error::new(ErrorKind::InvalidInput, "NullReferenceException: r"))?;
            let mut files = Vec::new();
            let magic = readU32(r)?;
            source.magic.store(magic, Ordering::Relaxed);
            if magic == KEY_MAGIC { readKey(source, r, &mut files)?; }
            else if magic == BIFF_MAGIC { readBiff(source, r, &mut files, None, None, None, true)?; }
            else if magic == BIFC_MAGIC { readBifc(source, r, &mut files, None, None, None, true)?; }
            else { return Err(Error::new(ErrorKind::InvalidData, format!("Unknown File Type {magic}"))); }
            source.setFiles(Some(files));
            Ok(())
        })
    }

    fn readData<'a>(&'a self, source: &'a BinaryArchive, r: Option<&'a mut dyn VfsRead>, file: &'a Arc<FileSource>, _option: &'a Tag) -> Task<'a, Option<Stream>> {
        Box::pin(async move {
            if let Tag::Str(path) = &file.tag {
                let bytes = readPath(path, file.offset, file.fileSize)?;
                return Ok(Some(Box::new(Cursor::new(bytes)) as Stream));
            }
            let fromTag = {
                let tag = source.base.tag.lock().unwrap_or_else(|p| p.into_inner());
                if let Tag::Bytes(b) = &*tag { Some(sliceAt(b, file.offset, file.fileSize)) } else { None }
            };
            if let Some(bytes) = fromTag { return Ok(Some(Box::new(Cursor::new(bytes?)) as Stream)); }
            if source.magic.load(Ordering::Relaxed) == BIFC_MAGIC {
                let r = r.ok_or_else(|| Error::new(ErrorKind::InvalidInput, "NullReferenceException: r"))?;
                seekTo(r, 0)?;
                let magic = readU32(r)?;
                if magic != BIFC_MAGIC { return Err(Error::new(ErrorKind::InvalidData, format!("Unknown File Type {magic}"))); }
                let inflated = inflateBifc(r)?;
                return Ok(Some(Box::new(Cursor::new(sliceAt(&inflated, file.offset, file.fileSize)?)) as Stream));
            }
            let r = r.ok_or_else(|| Error::new(ErrorKind::InvalidInput, "NullReferenceException: r"))?;
            seekTo(r, file.offset as u64)?;
            Ok(Some(Box::new(Cursor::new(readSome(r, file.fileSize as usize)?)) as Stream))
        })
    }
}

fn readKey(source: &BinaryArchive, r: &mut dyn VfsRead, files: &mut Vec<Arc<FileSource>>) -> io::Result<()> {
    let version = readU32(r)?;
    if version != KEY_VERSION { return Err(Error::new(ErrorKind::InvalidData, "BAD MAGIC")); }
    source.version.store(version, Ordering::Relaxed);
    let bifCount = readU32(r)?;
    let resourceCount = readU32(r)?;
    let bifOffset = readU32(r)?;
    let resourceOffset = readU32(r)?;

    let mut raw = Vec::new();
    seekTo(r, bifOffset as u64)?;
    for _ in 0..bifCount {
        _ = readU32(r)?;
        let nameOfs = readU32(r)?;
        let nameLen = readU16(r)?;
        _ = readU16(r)?;
        raw.push((nameOfs, nameLen));
    }
    let mut bifs = Vec::with_capacity(raw.len());
    for (nameOfs, nameLen) in raw { bifs.push(readName(r, nameOfs, nameLen)?); }

    let mut byBif: HashMap<u32, HashMap<u32, (String, u16)>> = HashMap::new();
    seekTo(r, resourceOffset as u64)?;
    for _ in 0..resourceCount {
        let resref = resRef(&readExact(r, 8)?);
        let resType = readU16(r)?;
        let locator = readU32(r)?;
        if ((locator >> 14) & 0x3F) != 0 { continue; }
        let bif = (locator >> 20) & 0xFFF;
        let index = locator & 0x3FFF;
        let map = byBif.entry(bif).or_default();
        if !map.contains_key(&index) { map.insert(index, (resref, resType)); }
    }

    for (i, rel) in bifs.iter().enumerate() {
        let full = fullPath(&source.base.binPath, rel);
        if !std::path::Path::new(&full).is_file() { continue; }
        let empty = HashMap::new();
        let map = byBif.get(&(i as u32)).unwrap_or(&empty);
        openBif(source, files, &full, rel, map)?;
    }
    Ok(())
}

fn openBif(source: &BinaryArchive, files: &mut Vec<Arc<FileSource>>, full: &str, rel: &str, map: &HashMap<u32, (String, u16)>) -> io::Result<()> {
    let mut br = std::fs::File::open(full)?;
    let magic = readU32(&mut br)?;
    if magic == BIFF_MAGIC { readBiff(source, &mut br, files, Some(rel), Some(full), Some(map), false) }
    else if magic == BIFC_MAGIC { readBifc(source, &mut br, files, Some(rel), Some(full), Some(map), false) }
    else { Err(Error::new(ErrorKind::InvalidData, format!("Unknown File Type {magic}"))) }
}

fn readBifc(source: &BinaryArchive, r: &mut dyn VfsRead, files: &mut Vec<Arc<FileSource>>, rel: Option<&str>, tagPath: Option<&str>, map: Option<&HashMap<u32, (String, u16)>>, setVersion: bool) -> io::Result<()> {
    let inflated = inflateBifc(r)?;
    if setVersion {
        let mut tag = source.base.tag.lock().unwrap_or_else(|p| p.into_inner());
        if !matches!(*tag, Tag::Map(_)) { *tag = Tag::Bytes(inflated.clone()); }
    }
    let mut inner = Cursor::new(inflated);
    let magic = readU32(&mut inner)?;
    if magic != BIFF_MAGIC { return Err(Error::new(ErrorKind::InvalidData, format!("Unknown File Type {magic}"))); }
    readBiff(source, &mut inner, files, rel, tagPath, map, setVersion)
}

fn readBiff(source: &BinaryArchive, r: &mut dyn VfsRead, files: &mut Vec<Arc<FileSource>>, rel: Option<&str>, tagPath: Option<&str>, map: Option<&HashMap<u32, (String, u16)>>, setVersion: bool) -> io::Result<()> {
    let version = readU32(r)?;
    if version != BIFF_VERSION { return Err(Error::new(ErrorKind::InvalidData, "BAD MAGIC")); }
    if setVersion { source.version.store(version, Ordering::Relaxed); }
    let fileCount = readU32(r)?;
    _ = readU32(r)?;
    let fileTableOffset = readU32(r)?;
    seekTo(r, fileTableOffset as u64)?;
    for i in 0..fileCount {
        let locator = readU32(r)?;
        let offset = readU32(r)?;
        let size = readU32(r)?;
        let resType = readU16(r)?;
        _ = readU16(r)?;
        if let Some(map) = map {
            let index = locator & 0x3FFF;
            let Some((resref, resType)) = map.get(&index) else { continue; };
            files.push(Arc::new(FileSource {
                path: format!("{}/{resref}{}", rel.unwrap_or(""), ext(*resType)),
                offset: offset as i64,
                fileSize: size as i64,
                tag: tagPath.map(|p| Tag::Str(p.to_string())).unwrap_or(Tag::None),
                ..Default::default()
            }));
        }
        else {
            files.push(Arc::new(FileSource {
                path: format!("{i:04}{}", ext(resType)),
                offset: offset as i64,
                fileSize: size as i64,
                ..Default::default()
            }));
        }
    }
    Ok(())
}

// IESDP BIFC V1.0. Cap the copy at the stated uncompressed size.
fn inflateBifc(r: &mut dyn VfsRead) -> io::Result<Vec<u8>> {
    let version = readU32(r)?;
    if version != BIFC_VERSION { return Err(Error::new(ErrorKind::InvalidData, "BAD MAGIC")); }
    let uncompressed = readU32(r)? as usize;
    let mut output = Vec::new();
    while output.len() < uncompressed {
        let declen = readU32(r)? as usize;
        let complen = readU32(r)?;
        if declen == 0 || complen == 0 || complen > i32::MAX as u32 { return Err(Error::new(ErrorKind::InvalidData, "zlib")); }
        let chunk = r.decompressZlib(complen as i32, declen as i32, false)?;
        if chunk.len() < declen { return Err(Error::new(ErrorKind::InvalidData, "zlib")); }
        let room = uncompressed - output.len();
        let take = declen.min(room);
        output.extend_from_slice(&chunk[..take]);
    }
    Ok(output)
}

fn readPath(path: &str, offset: i64, size: i64) -> io::Result<Vec<u8>> {
    let mut br = std::fs::File::open(path)?;
    let magic = readU32(&mut br)?;
    if magic == BIFC_MAGIC {
        let inflated = inflateBifc(&mut br)?;
        return sliceAt(&inflated, offset, size);
    }
    if offset < 0 || size < 0 { return Err(Error::new(ErrorKind::InvalidInput, "ArgumentOutOfRangeException")); }
    seekTo(&mut br, offset as u64)?;
    readSome(&mut br, size as usize)
}

fn sliceAt(data: &[u8], offset: i64, size: i64) -> io::Result<Vec<u8>> {
    if offset < 0 || size < 0 { return Err(Error::new(ErrorKind::InvalidInput, "ArgumentOutOfRangeException")); }
    let o = offset as usize;
    let n = size as usize;
    let end = o.checked_add(n).ok_or_else(|| Error::new(ErrorKind::InvalidInput, "OverflowException"))?;
    if end > data.len() { return Err(Error::new(ErrorKind::InvalidInput, "Offset and length were out of bounds.")); }
    Ok(data[o..end].to_vec())
}

fn readName(r: &mut dyn VfsRead, offset: u32, length: u16) -> io::Result<String> {
    if length == 0 { return Ok(String::new()); }
    seekTo(r, offset as u64)?;
    Ok(bifName(&readSome(r, length as usize)?))
}

fn bifName(b: &[u8]) -> String { ascii(b).replace('\\', "/") }

fn resRef(b: &[u8]) -> String { ascii(b).trim().to_string() }

fn ascii(b: &[u8]) -> String {
    let n = b.iter().position(|c| *c == 0).unwrap_or(b.len());
    b[..n].iter().map(|c| if *c < 128 { *c as char } else { '?' }).collect()
}

fn fullPath(binPath: &str, name: &str) -> String {
    let rel = name.replace('/', std::path::MAIN_SEPARATOR_STR).replace('\\', std::path::MAIN_SEPARATOR_STR);
    let dir = directoryName(binPath);
    let sep = std::path::MAIN_SEPARATOR;
    let joined = if dir.is_empty() { rel } else { format!("{dir}{sep}{rel}") };
    let path = std::path::PathBuf::from(&joined);
    let abs = if path.is_absolute() { path } else { std::env::current_dir().unwrap_or_default().join(path) };
    abs.to_string_lossy().into_owned()
}

fn ext(resType: u16) -> &'static str {
    match resType {
        0x0001 => ".bmp",
        0x0002 => ".mve",
        0x0004 => ".wav",
        0x0005 => ".wfx",
        0x0006 => ".plt",
        0x03e8 => ".bam",
        0x03e9 => ".wed",
        0x03ea => ".chu",
        0x03eb => ".tis",
        0x03ec => ".mos",
        0x03ed => ".itm",
        0x03ee => ".spl",
        0x03ef => ".bcs",
        0x03f0 => ".ids",
        0x03f1 => ".cre",
        0x03f2 => ".are",
        0x03f3 => ".dlg",
        0x03f4 => ".2da",
        0x03f5 => ".gam",
        0x03f6 => ".sto",
        0x03f7 => ".wmp",
        0x03f8 => ".eff",
        0x03f9 => ".bs",
        0x03fa => ".chr",
        0x03fb => ".vvc",
        0x03fc => ".vef",
        0x03fd => ".pro",
        0x03fe => ".bio",
        0x03ff => ".wbm",
        0x0400 => ".fnt",
        0x0402 => ".gui",
        0x0403 => ".sql",
        0x0404 => ".pvrz",
        0x0405 => ".glsl",
        0x0406 => ".tot",
        0x0407 => ".toh",
        0x0408 => ".menu",
        0x0409 => ".lua",
        0x040a => ".ttf",
        0x040b => ".png",
        0x044c => ".bah",
        0x0802 => ".ini",
        0x0803 => ".src",
        0x0804 => ".maze",
        _ => "",
    }
}

fn readU32(r: &mut (impl Read + ?Sized)) -> io::Result<u32> { let mut b = [0u8; 4]; r.read_exact(&mut b)?; Ok(u32::from_le_bytes(b)) }
fn readU16(r: &mut (impl Read + ?Sized)) -> io::Result<u16> { let mut b = [0u8; 2]; r.read_exact(&mut b)?; Ok(u16::from_le_bytes(b)) }
fn readExact(r: &mut (impl Read + ?Sized), n: usize) -> io::Result<Vec<u8>> { let mut b = vec![0u8; n]; r.read_exact(&mut b)?; Ok(b) }
fn readSome(r: &mut (impl Read + ?Sized), n: usize) -> io::Result<Vec<u8>> {
    let mut b = vec![0u8; n];
    let mut filled = 0;
    while filled < n {
        let k = r.read(&mut b[filled..])?;
        if k == 0 { break; }
        filled += k;
    }
    b.truncate(filled);
    Ok(b)
}
fn seekTo(r: &mut (impl Seek + ?Sized), offset: u64) -> io::Result<()> { r.seek(SeekFrom::Start(offset))?; Ok(()) }

// #endregion

// #region Binary_Dat

// No MDK2 HD sample, so there is no custom layout. A PK local header
// (0x04034B50) rewinds and calls Binary_Zip::getArcBinary. Any other magic
// throws. MDK2:W has no second magic.

const ZipMagic: u32 = 0x04034B50;

pub struct Binary_Dat;

impl Binary_Dat {
    pub fn current() -> Arc<dyn ArcBinary> {
        static CURRENT: LazyLock<Arc<dyn ArcBinary>> = LazyLock::new(|| Arc::new(Binary_Dat));
        CURRENT.clone()
    }

    fn zipBinary(game: &gamex::family::FamilyGame) -> Arc<dyn ArcBinary> { Binary_Zip::getArcBinary(game) }
}

impl ArcBinary for Binary_Dat {
    fn read<'a>(&'a self, source: &'a BinaryArchive, r: Option<&'a mut dyn VfsRead>, tag: &'a Tag) -> Task<'a, ()> {
        Box::pin(async move {
            let r = r.ok_or_else(|| Error::new(ErrorKind::InvalidInput, "NullReferenceException: r"))?;
            let mut b = [0u8; 4];
            if let Err(e) = r.read_exact(&mut b) {
                if e.kind() == ErrorKind::UnexpectedEof { return Err(Error::new(ErrorKind::InvalidData, "Binary_Dat: no sample")); }
                return Err(e);
            }
            let magic = u32::from_le_bytes(b);
            if magic == ZipMagic {
                r.seek(SeekFrom::Start(0))?;
                let zip = Self::zipBinary(&source.base.game);
                return zip.read(source, Some(r), tag).await;
            }
            Err(Error::new(ErrorKind::InvalidData, format!("Binary_Dat: unknown magic {magic:08X}")))
        })
    }

    fn readData<'a>(&'a self, source: &'a BinaryArchive, r: Option<&'a mut dyn VfsRead>, file: &'a Arc<FileSource>, option: &'a Tag) -> Task<'a, Option<Stream>> {
        // `read` stored the zip on `source.Tag`. `Binary_Zip::readData` does not use `self`, and a local `Arc` cannot satisfy the `'a` borrow.
        static ZIP: LazyLock<Arc<dyn ArcBinary>> = LazyLock::new(Binary_Zip::instance);
        ArcBinary::readData(&**ZIP, source, r, file, option)
    }
}

// #endregion

// #region Binary_Da2

// GOB and LMP from jbgda GobExtractor / Lmp. Dark Alliance stores the name
// in the 0x40 LMP row. The other Snowblind games store a name offset in a
// 0x0C row. The inline row is tried first. No 2022 PC sample was on disk.

const GobStride: usize = 0x28;
const LmpDaStride: usize = 0x40;
const LmpRefStride: usize = 0x0C;
const MaxFiles: u32 = 100000;

pub struct Binary_Da2;

impl Binary_Da2 {
    pub fn current() -> Arc<dyn ArcBinary> {
        static CURRENT: LazyLock<Arc<dyn ArcBinary>> = LazyLock::new(|| Arc::new(Binary_Da2));
        CURRENT.clone()
    }
}

impl ArcBinary for Binary_Da2 {
    fn read<'a>(&'a self, source: &'a BinaryArchive, r: Option<&'a mut dyn VfsRead>, _tag: &'a Tag) -> Task<'a, ()> {
        Box::pin(async move {
            let r = r.ok_or_else(|| Error::new(ErrorKind::InvalidInput, "NullReferenceException: r"))?;
            let length = streamLen(r)?;
            if length < 4 { return Err(Error::new(ErrorKind::InvalidData, "Binary_Da2: no sample")); }
            let ext = extension(&source.base.binPath).to_lowercase();
            let files = if ext == ".gob" { readGob(r, length)? }
            else if ext == ".lmp" { readLmp(r, length)? }
            else {
                let mut b = [0u8; 4];
                r.read_exact(&mut b)?;
                let magic = u32::from_le_bytes(b);
                return Err(Error::new(ErrorKind::InvalidData, format!("Binary_Da2: unknown magic {magic:08X}")));
            };
            source.setFiles(Some(files));
            Ok(())
        })
    }

    fn readData<'a>(&'a self, _source: &'a BinaryArchive, r: Option<&'a mut dyn VfsRead>, file: &'a Arc<FileSource>, _option: &'a Tag) -> Task<'a, Option<Stream>> {
        Box::pin(async move {
            let r = r.ok_or_else(|| Error::new(ErrorKind::InvalidInput, "NullReferenceException: r"))?;
            r.seek(SeekFrom::Start(file.offset as u64))?;
            let mut buf = vec![0u8; file.fileSize as usize];
            r.read_exact(&mut buf)?;
            Ok(Some(Box::new(Cursor::new(buf)) as Stream))
        })
    }
}

fn streamLen(r: &mut dyn VfsRead) -> std::io::Result<u64> {
    let pos = r.stream_position()?;
    let end = r.seek(SeekFrom::End(0))?;
    r.seek(SeekFrom::Start(pos))?;
    Ok(end)
}

fn readAt(r: &mut dyn VfsRead, pos: u64, n: usize) -> std::io::Result<Vec<u8>> {
    r.seek(SeekFrom::Start(pos))?;
    let mut buf = vec![0u8; n];
    r.read_exact(&mut buf)?;
    Ok(buf)
}

fn readGob(r: &mut dyn VfsRead, length: u64) -> std::io::Result<Vec<Arc<FileSource>>> {
    let mut rows: Vec<(String, u64, u64)> = Vec::new();
    let mut pos = 0u64;
    while pos + GobStride as u64 <= length {
        let rec = readAt(r, pos, GobStride)?;
        let name = cString(&rec, 0, 0x20);
        if name.is_empty() { break; }
        if rec[0] < 0x20 || rec[0] > 0x7E {
            let magic = u32::from_le_bytes(rec[0..4].try_into().unwrap());
            return Err(Error::new(ErrorKind::InvalidData, format!("Binary_Da2: unknown magic {magic:08X}")));
        }
        let offset = u32::from_le_bytes(rec[0x20..0x24].try_into().unwrap()) as u64;
        let sizeField = u32::from_le_bytes(rec[0x24..0x28].try_into().unwrap()) as u64;
        rows.push((name.replace('\\', "/"), offset, sizeField));
        pos += GobStride as u64;
    }
    let mut files = Vec::new();
    for i in 0..rows.len() {
        let (name, offset, sizeField) = &rows[i];
        let size = if *sizeField > 0 && offset + sizeField <= length { *sizeField }
        else { (if i + 1 < rows.len() { rows[i + 1].1 } else { length }) - offset };
        if offset + size > length { return Err(Error::new(ErrorKind::InvalidData, "Binary_Da2: bad gob entry")); }
        files.push(Arc::new(FileSource { path: name.clone(), offset: *offset as i64, fileSize: size as i64, ..Default::default() }));
    }
    Ok(files)
}

fn readLmp(r: &mut dyn VfsRead, length: u64) -> std::io::Result<Vec<Arc<FileSource>>> {
    let count = u32::from_le_bytes(readAt(r, 0, 4)?.try_into().unwrap());
    if count == 0 { return Ok(Vec::new()); }
    if count > MaxFiles { return Err(Error::new(ErrorKind::InvalidData, format!("Binary_Da2: unknown magic {count:08X}"))); }
    if let Some(files) = tryDa(r, length, count)? { return Ok(files); }
    if let Some(files) = tryRef(r, length, count)? { return Ok(files); }
    Err(Error::new(ErrorKind::InvalidData, format!("Binary_Da2: unknown magic {count:08X}")))
}

fn tryDa(r: &mut dyn VfsRead, length: u64, count: u32) -> std::io::Result<Option<Vec<Arc<FileSource>>>> {
    let mut header = 4u64;
    if header + count as u64 * LmpDaStride as u64 > length { return Ok(None); }
    let mut files = Vec::new();
    for _ in 0..count {
        let rec = readAt(r, header, LmpDaStride)?;
        if !hasCString(&rec, 0, 0x38) { return Ok(None); }
        let name = cString(&rec, 0, 0x38).replace('\\', "/");
        let offset = u32::from_le_bytes(rec[0x38..0x3C].try_into().unwrap()) as u64;
        let size = u32::from_le_bytes(rec[0x3C..0x40].try_into().unwrap()) as u64;
        if name.is_empty() || offset + size > length { return Ok(None); }
        files.push(Arc::new(FileSource { path: name, offset: offset as i64, fileSize: size as i64, ..Default::default() }));
        header += LmpDaStride as u64;
    }
    Ok(Some(files))
}

fn tryRef(r: &mut dyn VfsRead, length: u64, count: u32) -> std::io::Result<Option<Vec<Arc<FileSource>>>> {
    let mut header = 4u64;
    if header + count as u64 * LmpRefStride as u64 > length { return Ok(None); }
    let mut rows = Vec::new();
    for _ in 0..count {
        let rec = readAt(r, header, LmpRefStride)?;
        let strOff = u32::from_le_bytes(rec[0..4].try_into().unwrap()) as u64;
        let offset = u32::from_le_bytes(rec[4..8].try_into().unwrap()) as u64;
        let size = u32::from_le_bytes(rec[8..12].try_into().unwrap()) as u64;
        if strOff >= length || offset + size > length { return Ok(None); }
        rows.push((strOff, offset, size));
        header += LmpRefStride as u64;
    }
    let mut files = Vec::new();
    for (strOff, offset, size) in rows {
        let raw = readAt(r, strOff, (length - strOff) as usize)?;
        let name = cString(&raw, 0, raw.len()).replace('\\', "/");
        if name.is_empty() { return Ok(None); }
        files.push(Arc::new(FileSource { path: name, offset: offset as i64, fileSize: size as i64, ..Default::default() }));
    }
    Ok(Some(files))
}

fn hasCString(rec: &[u8], start: usize, max: usize) -> bool {
    for i in 0..max {
        let b = rec[start + i];
        if b == 0 { return true; }
        if b < 0x20 || b > 0x7E { return false; }
    }
    false
}

fn cString(rec: &[u8], start: usize, max: usize) -> String {
    let mut n = 0;
    while n < max && rec[start + n] != 0 { n += 1; }
    String::from_utf8_lossy(&rec[start..start + n]).into_owned()
}

// #endregion

// #region Binary_XXX

pub struct Binary_XXX;

impl Binary_XXX {
    pub fn current() -> Arc<dyn ArcBinary> {
        static CURRENT: LazyLock<Arc<dyn ArcBinary>> = LazyLock::new(|| Arc::new(Binary_XXX));
        CURRENT.clone()
    }
}

impl ArcBinary for Binary_XXX {
    fn read<'a>(&'a self, source: &'a BinaryArchive, _r: Option<&'a mut dyn VfsRead>, _tag: &'a Tag) -> Task<'a, ()> {
        Box::pin(async move {
            source.setFiles(Some(Vec::new()));
            Ok(())
        })
    }

    fn readData<'a>(&'a self, _source: &'a BinaryArchive, _r: Option<&'a mut dyn VfsRead>, _file: &'a Arc<FileSource>, _option: &'a Tag) -> Task<'a, Option<Stream>> {
        Box::pin(async { Err(Error::new(ErrorKind::Unsupported, "NotImplementedException")) })
    }
}

// #endregion

#[cfg(test)]
mod keyTests {
    use super::*;
    use gamex::binary::{getResult, newArchive, BinaryArchive, BinaryState};
    use gamex::family::{createFileSystem, FamilyGame, SystemPath};
    use openx_vfx::vfx::VfsRead;
    use std::io::Cursor;

    fn emptyKey() -> Vec<u8> {
        let mut b = Vec::new();
        b.extend_from_slice(&KEY_MAGIC.to_le_bytes());
        b.extend_from_slice(&KEY_VERSION.to_le_bytes());
        b.extend_from_slice(&0u32.to_le_bytes());
        b.extend_from_slice(&0u32.to_le_bytes());
        b.extend_from_slice(&24u32.to_le_bytes());
        b.extend_from_slice(&24u32.to_le_bytes());
        b
    }

    #[test]
    fn emptyKeyListsNoFiles() {
        let vfx = createFileSystem(None, &SystemPath { root: ".".into(), ..Default::default() }, None, None).unwrap();
        let state = BinaryState::new(vfx, FamilyGame::empty(), None, Some("chitin.key"), Tag::None);
        let binary = Binary_Key::current();
        let arc = newArchive(BinaryArchive::new(None, state, Some(binary.clone())));
        let mut cursor = Cursor::new(emptyKey());
        getResult(binary.read(&arc, Some(&mut cursor), &Tag::None)).unwrap();
        let files = arc.files().unwrap();
        assert!(files.is_empty());
    }
}

#[cfg(test)]
mod datTests {
    use super::*;
    use gamex::binary::{getResult, newArchive, BinaryArchive, BinaryState};
    use gamex::family::{createFileSystem, FamilyGame, SystemPath};
    use std::io::Cursor;

    #[test]
    fn unknownMagicNamesTheBytes() {
        let vfx = createFileSystem(None, &SystemPath { root: ".".into(), ..Default::default() }, None, None).unwrap();
        let state = BinaryState::new(vfx, FamilyGame::empty(), None, Some("a.dat"), Tag::None);
        let binary = Binary_Dat::current();
        let arc = newArchive(BinaryArchive::new(None, state, Some(binary.clone())));
        let mut cursor = Cursor::new(vec![0u8; 8]);
        let err = getResult(binary.read(&arc, Some(&mut cursor), &Tag::None)).unwrap_err();
        let text = err.to_string();
        assert!(text.contains("00000000"), "{text}");
    }
}

#[cfg(test)]
mod da2Tests {
    use super::*;
    use gamex::binary::{getResult, newArchive, BinaryArchive, BinaryState};
    use gamex::family::{createFileSystem, FamilyGame, SystemPath};
    use std::io::Cursor;

    fn gobSample() -> Vec<u8> {
        let mut rec = vec![0u8; GobStride];
        rec[..5].copy_from_slice(b"a.lmp");
        rec[0x20..0x24].copy_from_slice(&0x50u32.to_le_bytes());
        rec[0x24..0x28].copy_from_slice(&4u32.to_le_bytes());
        let mut buf = rec;
        buf.extend_from_slice(&[0u8; GobStride]);
        buf.extend_from_slice(b"WXYZ");
        buf
    }

    fn readBuf(path: &str, bytes: Vec<u8>) -> std::io::Result<Vec<Arc<FileSource>>> {
        let vfx = createFileSystem(None, &SystemPath { root: ".".into(), ..Default::default() }, None, None).unwrap();
        let state = BinaryState::new(vfx, FamilyGame::empty(), None, Some(path), Tag::None);
        let binary = Binary_Da2::current();
        let arc = newArchive(BinaryArchive::new(None, state, Some(binary.clone())));
        let mut cursor = Cursor::new(bytes);
        getResult(binary.read(&arc, Some(&mut cursor), &Tag::None))?;
        let files = arc.files.read().unwrap().clone().unwrap_or_default();
        Ok(files)
    }

    #[test]
    fn gobListsTheNamedEntry() {
        let files = readBuf("a.gob", gobSample()).unwrap();
        assert_eq!(files.len(), 1);
        assert_eq!(files[0].path, "a.lmp");
        assert_eq!(files[0].offset, 0x50);
        assert_eq!(files[0].fileSize, 4);
    }

    #[test]
    fn unknownMagicNamesTheBytes() {
        let err = readBuf("a.gob", vec![0xFFu8; GobStride]).unwrap_err();
        assert!(err.to_string().contains("FFFFFFFF"), "{}", err);
    }
}

