// PORT-SOURCE: Families/GameX.Bioware/Formats/Binary.cs
// PORT-SHA: unstamped
// PORT-STATUS: partial
//
// Ported: Binary_Aurora (KEY/BIFF/BIFC file list), Binary_Myp (versions 5 and 6,
// packed copy, zlib, zstd), Binary_Gff, Binary_Key (Infinity 8-byte resref).
// Not ported: TOR.HashLookup and WAR.HashLookup. gamex-resource leaves both as
// stubs, so the lookup is an empty map and every row is named {hash:02X}.bin.
// No hash dictionary is invented. Fixed BIFF rows (tilesets) are not read.

use std::collections::HashMap;
use std::io::{self, Cursor, Error, ErrorKind, Read, Seek, SeekFrom};
use std::sync::atomic::Ordering;
use std::sync::{Arc, LazyLock, Mutex};
use gamex::binary::{ArcBinary, Archive, BinaryArchive, Stream, Task};
use gamex::meta::{directoryName, FileSource, IHaveMetaInfo, MetaInfo, MetaManager, Tag};
use gamex_uncore::formats::compression::Compression;
use openx_vfx::vfx::VfsRead;

const KEY_MAGIC: u32 = 0x2059454B; // "KEY "
const KEY_VERSION: u32 = 0x20203156; // "V1  "
const BIFF_MAGIC: u32 = 0x46464942; // "BIFF"
const BIFF_VERSION: u32 = 0x20203156; // "V1  "
const BIFC_MAGIC: u32 = 0x43464942; // "BIFC"
const BIFC_VERSION: u32 = 0x302E3156; // "V1.0"

fn unknownHex(magic: u32) -> Error { Error::new(ErrorKind::InvalidData, format!("Unknown File Type {magic:08X}")) }
fn unknownDec(magic: u32) -> Error { Error::new(ErrorKind::InvalidData, format!("Unknown File Type {magic}")) }
fn bad(msg: &str) -> Error { Error::new(ErrorKind::InvalidData, msg) }

// #region Binary_Aurora

// 16-byte resref. Resource index is id & 0xFFFFF. Bif index is (id >> 20) & 0xFFF.
// The BIFF fixed-resource count is skipped. There is no tileset bitfield.

pub struct Binary_Aurora;

impl Binary_Aurora {
    pub fn current() -> Arc<dyn ArcBinary> {
        static CURRENT: LazyLock<Arc<dyn ArcBinary>> = LazyLock::new(|| Arc::new(Binary_Aurora));
        CURRENT.clone()
    }
}

impl ArcBinary for Binary_Aurora {
    fn read<'a>(&'a self, source: &'a BinaryArchive, r: Option<&'a mut dyn VfsRead>, _tag: &'a Tag) -> Task<'a, ()> {
        Box::pin(async move {
            let r = r.ok_or_else(|| Error::new(ErrorKind::InvalidInput, "NullReferenceException: r"))?;
            let mut files = Vec::new();
            let magic = readU32(r)?;
            source.magic.store(magic, Ordering::Relaxed);
            if magic == KEY_MAGIC { auroraReadKey(source, r, &mut files)?; }
            else if magic == BIFF_MAGIC { auroraReadBiff(source, r, &mut files, None, None, None, true)?; }
            else if magic == BIFC_MAGIC { readBifc(source, r, &mut files, None, None, None, true, true)?; }
            else { return Err(unknownHex(magic)); }
            source.setFiles(Some(files));
            Ok(())
        })
    }

    fn readData<'a>(&'a self, source: &'a BinaryArchive, r: Option<&'a mut dyn VfsRead>, file: &'a Arc<FileSource>, _option: &'a Tag) -> Task<'a, Option<Stream>> {
        Box::pin(async move { readStored(source, r, file, true) })
    }
}

fn auroraReadKey(source: &BinaryArchive, r: &mut dyn VfsRead, files: &mut Vec<Arc<FileSource>>) -> io::Result<()> {
    let version = readU32(r)?;
    if version != KEY_VERSION { return Err(bad("BAD MAGIC")); }
    source.version.store(version, Ordering::Relaxed);
    let bifCount = readU32(r)?;
    let keyCount = readU32(r)?;
    let filesOffset = readU32(r)?;
    let keysOffset = readU32(r)?;
    _ = readU32(r)?;
    _ = readU32(r)?;
    _ = readExact(r, 32)?;

    let mut raw = Vec::new();
    seekTo(r, filesOffset as u64)?;
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
    seekTo(r, keysOffset as u64)?;
    for _ in 0..keyCount {
        let resref = resRef(&readExact(r, 16)?);
        let resType = readU16(r)?;
        let id = readU32(r)?;
        let bif = (id >> 20) & 0xFFF;
        let index = id & 0xFFFFF;
        let map = byBif.entry(bif).or_default();
        if !map.contains_key(&index) { map.insert(index, (resref, resType)); }
    }

    for (i, rel) in bifs.iter().enumerate() {
        let full = bifPath(&source.base.binPath, rel);
        if !std::path::Path::new(&full).is_file() { continue; }
        let empty = HashMap::new();
        let map = byBif.get(&(i as u32)).unwrap_or(&empty);
        auroraOpenBif(source, files, &full, rel, map)?;
    }
    Ok(())
}

fn auroraOpenBif(source: &BinaryArchive, files: &mut Vec<Arc<FileSource>>, full: &str, rel: &str, map: &HashMap<u32, (String, u16)>) -> io::Result<()> {
    let mut br = std::fs::File::open(full)?;
    let magic = readU32(&mut br)?;
    if magic == BIFF_MAGIC { auroraReadBiff(source, &mut br, files, Some(rel), Some(full), Some(map), false) }
    else if magic == BIFC_MAGIC { readBifc(source, &mut br, files, Some(rel), Some(full), Some(map), false, true) }
    else { Err(unknownHex(magic)) }
}

fn auroraReadBiff(source: &BinaryArchive, r: &mut dyn VfsRead, files: &mut Vec<Arc<FileSource>>, rel: Option<&str>, tagPath: Option<&str>, map: Option<&HashMap<u32, (String, u16)>>, setVersion: bool) -> io::Result<()> {
    let version = readU32(r)?;
    if version != BIFF_VERSION { return Err(bad("BAD MAGIC")); }
    if setVersion { source.version.store(version, Ordering::Relaxed); }
    let fileCount = readU32(r)?;
    _ = readU32(r)?;
    let fileTableOffset = readU32(r)?;
    seekTo(r, fileTableOffset as u64)?;
    for i in 0..fileCount {
        let fileId = readU32(r)?;
        let offset = readU32(r)?;
        let size = readU32(r)?;
        let resType = readU32(r)?;
        if let Some(map) = map {
            let Some((resref, resType)) = map.get(&(fileId & 0xFFFFF)) else { continue; };
            files.push(Arc::new(FileSource {
                path: format!("{}/{resref}{}", rel.unwrap_or(""), auroraExt(*resType as u32)),
                offset: offset as i64,
                fileSize: size as i64,
                tag: tagPath.map(|p| Tag::Str(p.to_string())).unwrap_or(Tag::None),
                ..Default::default()
            }));
        }
        else {
            files.push(Arc::new(FileSource {
                path: format!("{i:04}{}", auroraExt(resType)),
                offset: offset as i64,
                fileSize: size as i64,
                ..Default::default()
            }));
        }
    }
    Ok(())
}

fn auroraExt(resType: u32) -> &'static str {
    match resType {
        0x0000 => ".res", 0x0001 => ".bmp", 0x0002 => ".mve", 0x0003 => ".tga", 0x0004 => ".wav",
        0x0006 => ".plt", 0x0007 => ".ini", 0x0008 => ".mp3", 0x0009 => ".mpg", 0x000A => ".txt", 0x000B => ".xml",
        0x07D0 => ".plh", 0x07D1 => ".tex", 0x07D2 => ".mdl", 0x07D3 => ".thg", 0x07D5 => ".fnt",
        0x07D7 => ".lua", 0x07D8 => ".slt", 0x07D9 => ".nss", 0x07DA => ".ncs", 0x07DB => ".mod",
        0x07DC => ".are", 0x07DD => ".set", 0x07DE => ".ifo", 0x07DF => ".bic", 0x07E0 => ".wok",
        0x07E1 => ".2da", 0x07E2 => ".tlk", 0x07E6 => ".txi", 0x07E7 => ".git", 0x07E8 => ".bti",
        0x07E9 => ".uti", 0x07EA => ".btc", 0x07EB => ".utc", 0x07ED => ".dlg", 0x07EE => ".itp",
        0x07EF => ".btt", 0x07F0 => ".utt", 0x07F1 => ".dds", 0x07F2 => ".bts", 0x07F3 => ".uts",
        0x07F4 => ".ltr", 0x07F5 => ".gff", 0x07F6 => ".fac", 0x07F7 => ".bte", 0x07F8 => ".ute",
        0x07F9 => ".btd", 0x07FA => ".utd", 0x07FB => ".btp", 0x07FC => ".utp", 0x07FD => ".dft",
        0x07FE => ".gic", 0x07FF => ".gui", 0x0800 => ".css", 0x0801 => ".ccs", 0x0802 => ".btm",
        0x0803 => ".utm", 0x0804 => ".dwk", 0x0805 => ".pwk", 0x0806 => ".btg", 0x0808 => ".jrl",
        0x0809 => ".sav", 0x080A => ".utw", 0x080B => ".4pc", 0x080C => ".ssf", 0x080F => ".bik",
        0x0810 => ".ndb", 0x0811 => ".ptm", 0x0812 => ".ptt", 0x0813 => ".ncm", 0x0814 => ".mfx",
        0x0815 => ".mat", 0x0816 => ".mdb", 0x0817 => ".say", 0x0818 => ".ttf", 0x0819 => ".ttc",
        0x081A => ".cut", 0x081B => ".ka", 0x081C => ".jpg", 0x081D => ".ico", 0x081E => ".ogg",
        0x081F => ".spt", 0x0820 => ".spw", 0x0821 => ".wfx", 0x0822 => ".ugm", 0x0823 => ".qdb",
        0x0824 => ".qst", 0x0825 => ".npc", 0x0826 => ".spn", 0x0827 => ".utx", 0x0828 => ".mmd",
        0x0829 => ".smm", 0x082A => ".uta", 0x082B => ".mde", 0x082C => ".mdv", 0x082D => ".mda",
        0x082E => ".mba", 0x082F => ".oct", 0x0830 => ".bfx", 0x0831 => ".pdb", 0x0832 => ".TheWitcherSave",
        0x0833 => ".pvs", 0x0834 => ".cfx", 0x0835 => ".luc", 0x0837 => ".prb", 0x0838 => ".cam",
        0x0839 => ".vds", 0x083A => ".bin", 0x083B => ".wob", 0x083C => ".api", 0x083D => ".properties",
        0x083E => ".png", 0x270B => ".big", 0x270D => ".erf", 0x270E => ".bif", 0x270F => ".key",
        _ => "",
    }
}

// #endregion

// #region Binary_Myp

pub struct Binary_Myp;

impl Binary_Myp {
    pub fn current() -> Arc<dyn ArcBinary> {
        static CURRENT: LazyLock<Arc<dyn ArcBinary>> = LazyLock::new(|| Arc::new(Binary_Myp));
        CURRENT.clone()
    }
}

impl ArcBinary for Binary_Myp {
    fn read<'a>(&'a self, source: &'a BinaryArchive, r: Option<&'a mut dyn VfsRead>, _tag: &'a Tag) -> Task<'a, ()> {
        Box::pin(async move {
            let r = r.ok_or_else(|| Error::new(ErrorKind::InvalidInput, "NullReferenceException: r"))?;
            let mut files = Vec::new();
            source.setFiles(Some(Vec::new()));
            let hashLookup = mypHashes(&source.base.game.id);
            let magic = readU32(r)?;
            let version = readU32(r)?;
            let bom = readU32(r)?;
            let mut tableOffset = readU64(r)? as i64;
            _ = readU32(r)?;
            _ = readU32(r)?;
            _ = readU32(r)?;
            _ = readU32(r)?;
            if magic != 0x0050594d { return Err(bad("Not a .tor file (Wrong file header)")); }
            if version != 5 && version != 6 { return Err(bad(&format!("Only versions 5 and 6 are supported, file has {version}"))); }
            if bom != 0xfd23ec43 { return Err(bad("Unexpected byte order")); }
            if tableOffset == 0 { return Err(bad("File is empty")); }
            source.version.store(version, Ordering::Relaxed);
            while tableOffset != 0 {
                seekToI(r, tableOffset)?;
                let numFiles = readI32(r)?;
                if numFiles == 0 { break; }
                if numFiles < 0 { return Err(bad("numFiles")); }
                tableOffset = readI64(r)?;
                for i in 0..numFiles {
                    let offset = readU64(r)?;
                    let headerSize = readU32(r)?;
                    let packedSize = readU32(r)?;
                    let fileSize = readU32(r)?;
                    let hash = readU64(r)?;
                    _ = readU32(r)?;
                    let compressed = readU16(r)?;
                    if offset == 0 { continue; }
                    let path = if let Some(z) = hashLookup.get(&hash) { z.replace('\\', "/") } else { format!("{hash:02X}.bin") };
                    let path = if path.starts_with('/') { path[1..].to_string() } else { path };
                    files.push(Arc::new(FileSource {
                        id: i,
                        path,
                        fileSize: fileSize as i64,
                        packedSize: packedSize as i64,
                        offset: offset.wrapping_add(headerSize as u64) as i64,
                        hash,
                        compressed: compressed as i32,
                        ..Default::default()
                    }));
                }
            }
            source.setFiles(Some(files));
            Ok(())
        })
    }

    fn readData<'a>(&'a self, source: &'a BinaryArchive, r: Option<&'a mut dyn VfsRead>, file: &'a Arc<FileSource>, _option: &'a Tag) -> Task<'a, Option<Stream>> {
        Box::pin(async move {
            if file.fileSize == 0 { return Ok(Some(Box::new(Cursor::new(Vec::new())) as Stream)); }
            let r = r.ok_or_else(|| Error::new(ErrorKind::InvalidInput, "NullReferenceException: r"))?;
            seekToI(r, file.offset)?;
            if file.packedSize < 0 || file.fileSize < 0 { return Err(Error::new(ErrorKind::InvalidInput, "ArgumentOutOfRangeException")); }
            let bytes = if file.compressed == 0 {
                readSome(r, file.packedSize as usize)?
            }
            else if source.version.load(Ordering::Relaxed) == 6 {
                let packed = readSome(r, file.packedSize as usize)?;
                mypZstd(&packed, file.fileSize)?
            }
            else { r.decompressZlib(file.packedSize as i32, file.fileSize as i32, false)? };
            Ok(Some(Box::new(Cursor::new(bytes)) as Stream))
        })
    }
}

// TOR.HashLookup and WAR.HashLookup are not ported.
fn mypHashes(gameId: &str) -> HashMap<u64, String> {
    match gameId { "SWTOR" | "WAR" | _ => HashMap::new() }
}

fn mypZstd(packed: &[u8], fileSize: i64) -> io::Result<Vec<u8>> {
    let mut decoder = ruzstd::StreamingDecoder::new(Cursor::new(packed))
        .map_err(|e| bad(&format!("zstd {e:?}")))?;
    let mut out = Vec::new();
    let mut left = fileSize as usize;
    let mut buf = [0u8; 8192];
    let cap = buf.len();
    while left > 0 {
        let n = Read::read(&mut decoder, &mut buf[..left.min(cap)])?;
        if n == 0 { break; }
        out.extend_from_slice(&buf[..n]);
        left -= n;
    }
    Ok(out)
}

// #endregion

// #region Binary_Gff

type GffMap = Arc<Mutex<HashMap<String, GffValue>>>;

#[derive(Clone)]
enum GffValue {
    Byte(u8),
    Char(u16),
    Word(u16),
    Short(i16),
    DWord(u32),
    Int(i32),
    DWord64(u64),
    Int64(i64),
    Float(f32),
    Double(f64),
    Str(Option<String>),
    ResRef(Option<String>),
    Loc { dialogID: u32, values: Vec<(u32, Option<String>)> },
    Bytes(Vec<u8>),
    Struct(GffMap),
    List(Vec<GffMap>),
}

pub struct Binary_Gff {
    type_: u32,
    root: GffMap,
    index: HashMap<u32, GffMap>,
}

impl Binary_Gff {
    pub fn factory<'a>(r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        Box::pin(async move {
            let g = parseGff(r)?;
            Ok(Tag::Info(Arc::new(g) as Arc<dyn IHaveMetaInfo>))
        })
    }

    fn typeName(&self) -> String {
        match self.type_ {
            0x20474c44 => "DLG".into(),
            0x20424451 => "QDB".into(),
            0x20545351 => "QST".into(),
            n => n.to_string(),
        }
    }
}

impl IHaveMetaInfo for Binary_Gff {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, _file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> {
        vec![MetaInfo::new(Some("BinaryGFF".into()), Tag::None, vec![
            MetaInfo::new(Some(format!("Type: {}", self.typeName())), Tag::None, vec![], false),
        ], false)]
    }
}

fn parseGff(r: &mut (impl Read + Seek)) -> io::Result<Binary_Gff> {
    let type_ = readU32(r)?;
    let version = readU32(r)?;
    let structOffset = readU32(r)?;
    let structCount = readU32(r)?;
    let fieldOffset = readU32(r)?;
    let fieldCount = readU32(r)?;
    let labelOffset = readU32(r)?;
    let labelCount = readU32(r)?;
    let fieldDataOffset = readU32(r)?;
    _ = readU32(r)?;
    let fieldIndicesOffset = readU32(r)?;
    _ = readU32(r)?;
    let listIndicesOffset = readU32(r)?;
    _ = readU32(r)?;
    if version != 0x322e3356 && version != 0x332e3356 { return Err(bad("BAD MAGIC")); }

    seekTo(r, structOffset as u64)?;
    let mut structs: Vec<GffMap> = Vec::new();
    let mut counts = Vec::new();
    let mut dataOffs = Vec::new();
    let mut index = HashMap::new();
    for _ in 0..structCount {
        let id = readU32(r)?;
        let data = readU32(r)?;
        let count = readU32(r)?;
        let map = Arc::new(Mutex::new(HashMap::new()));
        if id != 0 {
            map.lock().unwrap_or_else(|p| p.into_inner()).insert("_".into(), GffValue::DWord(id));
            if index.contains_key(&id) { return Err(bad("An item with the same key has already been added.")); }
            index.insert(id, map.clone());
        }
        structs.push(map);
        counts.push(count);
        dataOffs.push(data);
    }

    seekTo(r, fieldOffset as u64)?;
    let mut fields = Vec::new();
    for _ in 0..fieldCount {
        let typ = readU32(r)?;
        let label = readU32(r)?;
        let data = readU32(r)?;
        let value = gffValue(r, typ, data, &structs, fieldDataOffset, listIndicesOffset)?;
        fields.push((label, value));
    }

    seekTo(r, labelOffset as u64)?;
    let mut labels = Vec::new();
    for _ in 0..labelCount { labels.push(fixedAString(&readExact(r, 16)?)); }

    for i in 0..structCount as usize {
        let count = counts[i];
        let data = dataOffs[i];
        if count == 1 {
            let (label, value) = fields.get(data as usize).cloned().ok_or_else(|| bad("IndexOutOfRangeException"))?;
            let name = labels.get(label as usize).cloned().ok_or_else(|| bad("IndexOutOfRangeException"))?;
            gffAdd(&structs[i], name, value)?;
            continue;
        }
        seekTo(r, fieldIndicesOffset.wrapping_add(data) as u64)?;
        let mut idxs = Vec::new();
        for _ in 0..count { idxs.push(readU32(r)?); }
        for idx in idxs {
            let (label, value) = fields.get(idx as usize).cloned().ok_or_else(|| bad("IndexOutOfRangeException"))?;
            let name = labels.get(label as usize).cloned().ok_or_else(|| bad("IndexOutOfRangeException"))?;
            gffAdd(&structs[i], name, value)?;
        }
    }
    if structs.is_empty() { return Err(bad("IndexOutOfRangeException")); }
    Ok(Binary_Gff { type_, root: structs[0].clone(), index })
}

fn gffAdd(map: &GffMap, name: String, value: GffValue) -> io::Result<()> {
    let mut guard = map.lock().unwrap_or_else(|p| p.into_inner());
    if guard.contains_key(&name) { return Err(bad("An item with the same key has already been added.")); }
    guard.insert(name, value);
    Ok(())
}

fn gffValue(r: &mut (impl Read + Seek + ?Sized), typ: u32, data: u32, structs: &[GffMap], fieldDataOffset: u32, listIndicesOffset: u32) -> io::Result<GffValue> {
    match typ {
        0 => return Ok(GffValue::Byte(data as u8)),
        1 => return Ok(GffValue::Char(data as u16)),
        2 => return Ok(GffValue::Word(data as u16)),
        3 => return Ok(GffValue::Short(data as i16)),
        4 => return Ok(GffValue::DWord(data)),
        5 => return Ok(GffValue::Int(data as i32)),
        8 => return Ok(GffValue::Float(f32::from_bits(data))),
        14 => {
            let s = structs.get(data as usize).cloned().ok_or_else(|| bad("IndexOutOfRangeException"))?;
            return Ok(GffValue::Struct(s));
        }
        15 => {
            seekTo(r, listIndicesOffset.wrapping_add(data) as u64)?;
            let n = readU32(r)?;
            let mut list = Vec::new();
            for _ in 0..n {
                let idx = readU32(r)? as usize;
                if idx >= structs.len() { return Err(bad("Exception")); }
                list.push(structs[idx].clone());
            }
            return Ok(GffValue::List(list));
        }
        _ => {}
    }
    seekTo(r, fieldDataOffset.wrapping_add(data) as u64)?;
    match typ {
        6 => Ok(GffValue::DWord64(readU64(r)?)),
        7 => Ok(GffValue::Int64(readI64(r)?)),
        9 => Ok(GffValue::Double(readF64(r)?)),
        10 => Ok(GffValue::Str(readL32UString(r)?)),
        11 => Ok(GffValue::ResRef(readL8UString(r)?)),
        12 => {
            skip(r, 4)?;
            let dialogID = readU32(r)?;
            let n = readU32(r)?;
            let mut values = Vec::new();
            for _ in 0..n { values.push((readU32(r)?, readL32UString(r)?)); }
            Ok(GffValue::Loc { dialogID, values })
        }
        13 => {
            let n = readU32(r)? as usize;
            Ok(GffValue::Bytes(readExact(r, n)?))
        }
        _ => Err(bad(&typ.to_string())),
    }
}

fn readL32UString(r: &mut (impl Read + Seek + ?Sized)) -> io::Result<Option<String>> {
    let n = readU32(r)? as usize;
    if n == 0 { return Ok(None); }
    Ok(Some(utf8Trim(&readExact(r, n)?)))
}

fn readL8UString(r: &mut (impl Read + ?Sized)) -> io::Result<Option<String>> {
    let mut b = [0u8; 1];
    r.read_exact(&mut b)?;
    let n = b[0] as usize;
    if n == 0 { return Ok(None); }
    Ok(Some(utf8Trim(&readExact(r, n)?)))
}

fn utf8Trim(b: &[u8]) -> String {
    let mut n = b.len();
    while n > 0 && b[n - 1] == 0 { n -= 1; }
    String::from_utf8_lossy(&b[..n]).into_owned()
}

// #endregion

// #region Binary_Key

// Infinity chitin.key. 8-byte resref. Low 14 bits are the file index.
// Bits 19-14 are tileset rows and are skipped. Bits 31-20 are the bif index.

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
            if magic == KEY_MAGIC { keyReadKey(source, r, &mut files)?; }
            else if magic == BIFF_MAGIC { keyReadBiff(source, r, &mut files, None, None, None, true)?; }
            else if magic == BIFC_MAGIC { readBifc(source, r, &mut files, None, None, None, true, false)?; }
            else { return Err(unknownDec(magic)); }
            source.setFiles(Some(files));
            Ok(())
        })
    }

    fn readData<'a>(&'a self, source: &'a BinaryArchive, r: Option<&'a mut dyn VfsRead>, file: &'a Arc<FileSource>, _option: &'a Tag) -> Task<'a, Option<Stream>> {
        Box::pin(async move { readStored(source, r, file, false) })
    }
}

fn keyReadKey(source: &BinaryArchive, r: &mut dyn VfsRead, files: &mut Vec<Arc<FileSource>>) -> io::Result<()> {
    let version = readU32(r)?;
    if version != KEY_VERSION { return Err(bad("BAD MAGIC")); }
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
        let full = bifPath(&source.base.binPath, rel);
        if !std::path::Path::new(&full).is_file() { continue; }
        let empty = HashMap::new();
        let map = byBif.get(&(i as u32)).unwrap_or(&empty);
        keyOpenBif(source, files, &full, rel, map)?;
    }
    Ok(())
}

fn keyOpenBif(source: &BinaryArchive, files: &mut Vec<Arc<FileSource>>, full: &str, rel: &str, map: &HashMap<u32, (String, u16)>) -> io::Result<()> {
    let mut br = std::fs::File::open(full)?;
    let magic = readU32(&mut br)?;
    if magic == BIFF_MAGIC { keyReadBiff(source, &mut br, files, Some(rel), Some(full), Some(map), false) }
    else if magic == BIFC_MAGIC { readBifc(source, &mut br, files, Some(rel), Some(full), Some(map), false, false) }
    else { Err(unknownDec(magic)) }
}

fn keyReadBiff(source: &BinaryArchive, r: &mut dyn VfsRead, files: &mut Vec<Arc<FileSource>>, rel: Option<&str>, tagPath: Option<&str>, map: Option<&HashMap<u32, (String, u16)>>, setVersion: bool) -> io::Result<()> {
    let version = readU32(r)?;
    if version != BIFF_VERSION { return Err(bad("BAD MAGIC")); }
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
            let Some((resref, resType)) = map.get(&(locator & 0x3FFF)) else { continue; };
            files.push(Arc::new(FileSource {
                path: format!("{}/{resref}{}", rel.unwrap_or(""), keyExt(*resType)),
                offset: offset as i64,
                fileSize: size as i64,
                tag: tagPath.map(|p| Tag::Str(p.to_string())).unwrap_or(Tag::None),
                ..Default::default()
            }));
        }
        else {
            files.push(Arc::new(FileSource {
                path: format!("{i:04}{}", keyExt(resType)),
                offset: offset as i64,
                fileSize: size as i64,
                ..Default::default()
            }));
        }
    }
    Ok(())
}

fn keyExt(resType: u16) -> &'static str {
    match resType {
        0x0001 => ".bmp", 0x0002 => ".mve", 0x0004 => ".wav", 0x0005 => ".wfx", 0x0006 => ".plt",
        0x03e8 => ".bam", 0x03e9 => ".wed", 0x03ea => ".chu", 0x03eb => ".tis", 0x03ec => ".mos",
        0x03ed => ".itm", 0x03ee => ".spl", 0x03ef => ".bcs", 0x03f0 => ".ids", 0x03f1 => ".cre",
        0x03f2 => ".are", 0x03f3 => ".dlg", 0x03f4 => ".2da", 0x03f5 => ".gam", 0x03f6 => ".sto",
        0x03f7 => ".wmp", 0x03f8 => ".eff", 0x03f9 => ".bs", 0x03fa => ".chr", 0x03fb => ".vvc",
        0x03fc => ".vef", 0x03fd => ".pro", 0x03fe => ".bio", 0x03ff => ".wbm", 0x0400 => ".fnt",
        0x0402 => ".gui", 0x0403 => ".sql", 0x0404 => ".pvrz", 0x0405 => ".glsl", 0x0406 => ".tot",
        0x0407 => ".toh", 0x0408 => ".menu", 0x0409 => ".lua", 0x040a => ".ttf", 0x040b => ".png",
        0x044c => ".bah", 0x0802 => ".ini", 0x0803 => ".src", 0x0804 => ".maze",
        _ => "",
    }
}

// #endregion

fn readBifc(source: &BinaryArchive, r: &mut dyn VfsRead, files: &mut Vec<Arc<FileSource>>, rel: Option<&str>, tagPath: Option<&str>, map: Option<&HashMap<u32, (String, u16)>>, setVersion: bool, aurora: bool) -> io::Result<()> {
    let inflated = inflateBifc(r)?;
    if setVersion {
        let mut tag = source.base.tag.lock().unwrap_or_else(|p| p.into_inner());
        if !matches!(*tag, Tag::Map(_)) { *tag = Tag::Bytes(inflated.clone()); }
    }
    let mut inner = Cursor::new(inflated);
    let magic = readU32(&mut inner)?;
    if magic != BIFF_MAGIC { return Err(if aurora { unknownHex(magic) } else { unknownDec(magic) }); }
    if aurora { auroraReadBiff(source, &mut inner, files, rel, tagPath, map, setVersion) }
    else { keyReadBiff(source, &mut inner, files, rel, tagPath, map, setVersion) }
}

fn readStored(source: &BinaryArchive, r: Option<&mut dyn VfsRead>, file: &Arc<FileSource>, aurora: bool) -> io::Result<Option<Stream>> {
    if let Tag::Str(path) = &file.tag {
        return Ok(Some(Box::new(Cursor::new(readPath(path, file.offset, file.fileSize, aurora)?)) as Stream));
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
        if magic != BIFC_MAGIC { return Err(if aurora { unknownHex(magic) } else { unknownDec(magic) }); }
        let inflated = inflateBifc(r)?;
        return Ok(Some(Box::new(Cursor::new(sliceAt(&inflated, file.offset, file.fileSize)?)) as Stream));
    }
    let r = r.ok_or_else(|| Error::new(ErrorKind::InvalidInput, "NullReferenceException: r"))?;
    if file.offset < 0 || file.fileSize < 0 { return Err(Error::new(ErrorKind::InvalidInput, "ArgumentOutOfRangeException")); }
    seekTo(r, file.offset as u64)?;
    Ok(Some(Box::new(Cursor::new(readSome(r, file.fileSize as usize)?)) as Stream))
}

fn inflateBifc(r: &mut dyn VfsRead) -> io::Result<Vec<u8>> {
    let version = readU32(r)?;
    if version != BIFC_VERSION { return Err(bad("BAD MAGIC")); }
    let uncompressed = readU32(r)?;
    if uncompressed > i32::MAX as u32 { return Err(bad("zlib")); }
    let uncompressed = uncompressed as usize;
    let mut output = Vec::new();
    while output.len() < uncompressed {
        let declen = readU32(r)? as usize;
        let complen = readU32(r)?;
        if declen == 0 || complen == 0 || declen > i32::MAX as usize || complen > i32::MAX as u32 { return Err(bad("zlib")); }
        let chunk = r.decompressZlib(complen as i32, declen as i32, false)?;
        if chunk.len() < declen { return Err(bad("zlib")); }
        let room = uncompressed - output.len();
        let take = declen.min(room);
        output.extend_from_slice(&chunk[..take]);
    }
    Ok(output)
}

fn readPath(path: &str, offset: i64, size: i64, aurora: bool) -> io::Result<Vec<u8>> {
    let mut br = std::fs::File::open(path)?;
    let magic = readU32(&mut br)?;
    if magic == BIFC_MAGIC {
        let inflated = inflateBifc(&mut br)?;
        return sliceAt(&inflated, offset, size);
    }
    let _ = aurora;
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

fn bifName(b: &[u8]) -> String { ascii(b, true, false).replace('\\', "/") }
fn resRef(b: &[u8]) -> String { ascii(b, true, true) }
fn fixedAString(b: &[u8]) -> String { ascii(b, false, false) }

fn ascii(b: &[u8], stop: bool, trim: bool) -> String {
    let n = if stop {
        b.iter().position(|c| *c == 0).unwrap_or(b.len())
    }
    else {
        let mut n = b.len();
        while n > 0 && b[n - 1] == 0 { n -= 1; }
        n
    };
    let s: String = b[..n].iter().map(|c| if *c < 128 { *c as char } else { '?' }).collect();
    if trim { s.trim().to_string() } else { s }
}

fn bifPath(binPath: &str, name: &str) -> String {
    let full = fullPath(binPath, name);
    if std::path::Path::new(&full).is_file() { return full; }
    let dir = directoryName(binPath);
    let parent = directoryName(&dir);
    if parent.is_empty() { return full; }
    let folder = dir.rsplit(['\\', '/']).next().unwrap_or("");
    if folder.is_empty() { return full; }
    let slash = name.replace('\\', "/");
    let prefix = format!("{}/", folder.to_ascii_lowercase());
    if slash.to_ascii_lowercase().starts_with(&prefix) {
        let sep = std::path::MAIN_SEPARATOR;
        return fullPath(&format!("{parent}{sep}key"), name);
    }
    full
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

fn readU32(r: &mut (impl Read + ?Sized)) -> io::Result<u32> { let mut b = [0u8; 4]; r.read_exact(&mut b)?; Ok(u32::from_le_bytes(b)) }
fn readU16(r: &mut (impl Read + ?Sized)) -> io::Result<u16> { let mut b = [0u8; 2]; r.read_exact(&mut b)?; Ok(u16::from_le_bytes(b)) }
fn readU64(r: &mut (impl Read + ?Sized)) -> io::Result<u64> { let mut b = [0u8; 8]; r.read_exact(&mut b)?; Ok(u64::from_le_bytes(b)) }
fn readI32(r: &mut (impl Read + ?Sized)) -> io::Result<i32> { let mut b = [0u8; 4]; r.read_exact(&mut b)?; Ok(i32::from_le_bytes(b)) }
fn readI64(r: &mut (impl Read + ?Sized)) -> io::Result<i64> { let mut b = [0u8; 8]; r.read_exact(&mut b)?; Ok(i64::from_le_bytes(b)) }
fn readF64(r: &mut (impl Read + ?Sized)) -> io::Result<f64> { let mut b = [0u8; 8]; r.read_exact(&mut b)?; Ok(f64::from_le_bytes(b)) }
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
fn seekToI(r: &mut (impl Seek + ?Sized), offset: i64) -> io::Result<()> {
    if offset < 0 { return Err(Error::new(ErrorKind::InvalidInput, "ArgumentOutOfRangeException")); }
    seekTo(r, offset as u64)
}
fn skip(r: &mut (impl Seek + ?Sized), n: i64) -> io::Result<()> { r.seek(SeekFrom::Current(n))?; Ok(()) }

#[cfg(test)]
mod keyTests {
    use super::*;
    use gamex::binary::{getResult, newArchive, BinaryArchive, BinaryState};
    use gamex::family::{createFileSystem, FamilyGame, SystemPath};
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
mod auroraTests {
    use super::*;
    use gamex::binary::{getResult, newArchive, BinaryArchive, BinaryState};
    use gamex::family::{createFileSystem, FamilyGame, SystemPath};
    use std::io::Cursor;

    #[test]
    fn unknownMagicNamesTheBytes() {
        let vfx = createFileSystem(None, &SystemPath { root: ".".into(), ..Default::default() }, None, None).unwrap();
        let state = BinaryState::new(vfx, FamilyGame::empty(), None, Some("nwn.key"), Tag::None);
        let binary = Binary_Aurora::current();
        let arc = newArchive(BinaryArchive::new(None, state, Some(binary.clone())));
        let mut cursor = Cursor::new(vec![0xFFu8; 8]);
        let err = getResult(binary.read(&arc, Some(&mut cursor), &Tag::None)).unwrap_err();
        let text = err.to_string();
        assert!(text.contains("FFFFFFFF"), "{text}");
    }
}
