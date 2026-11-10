// PORT-SOURCE: Families/GameX.IW/Formats/FastFile.cs
// PORT-SHA: f36f11a8651477bb
// PORT-STATUS: partial
//
// PARTIAL PORT: `ListFastFile` and the retail readers it calls. COD4, WaW,
// and Black Ops share `list32`. MW2, MW3, and Ghosts share `list64`. BO2 and
// BO3 share `listBo`. The unused members live in `FastFileUnused.cs` and stay
// commented. `GetZoneFile` writes a sibling `.ff~`. Salsa20 here is
// `Compression.Salsa20`.

use std::io::{self, Error, ErrorKind, Read, Seek, SeekFrom};
use flate2::read::DeflateDecoder;
use openx_poly::prelude::BinaryReaderExt;
use openx_vfx::vfx::VfsRead;
use sha1::{Digest, Sha1};
use crate::formats::fast_file_asset_type::{IW3XAssetType, IW4XAssetType, IW5XAssetType, Oth_AssetType};

pub const IWff: u32 = 0x66665749;
pub const S1ff: u32 = 0x66663153;
pub const TAff: u32 = 0x66664154;
pub const U100: u32 = 0x30303175;
pub const A100: u32 = 0x30303161;
pub const F100: u32 = 0x30303130;
pub const F000: u32 = 0x30303030;

const CO4_WWII: u32 = 0x0005;
const WaW: u32 = 0x0183;
const MW2: u32 = 0x0114;
const BO: u32 = 0x01d9;
const MW3: u32 = 0x0001;
const BO2: u32 = 0x0093;
const Ghosts: u32 = 0x0235;
const BO3: u32 = 0x0251;

const GhostsNil: i64 = i64::from_le_bytes([0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFD, 0xFD, 0xFD]);
const Bo2Key: [u8; 32] = [
    0x64, 0x1D, 0x8A, 0x2F, 0xE3, 0x1D, 0x3A, 0xA6, 0x36, 0x22, 0xBB, 0xC9, 0xCE, 0x85, 0x87, 0x22,
    0x9D, 0x42, 0xB0, 0xF8, 0xED, 0x9B, 0x92, 0x41, 0x30, 0xBF, 0x88, 0xB6, 0x5E, 0xDC, 0x50, 0xBE,
];

pub struct FileHeader {
    pub id: i32,
    pub path: String,
    pub position: i64,
    pub fileSize: i64,
    pub data: Option<Vec<u8>>,
}

fn i32At(z: &[u8], o: usize) -> i32 { i32::from_le_bytes(z[o..o + 4].try_into().unwrap()) }
fn i64At(z: &[u8], o: usize) -> i64 { i64::from_le_bytes(z[o..o + 8].try_into().unwrap()) }

fn inflateRaw(data: &[u8]) -> io::Result<Vec<u8>> {
    let mut dec = DeflateDecoder::new(data);
    let mut out = Vec::new();
    dec.read_to_end(&mut out).map_err(|e| Error::new(ErrorKind::InvalidData, e))?;
    Ok(out)
}

fn readRest(r: &mut dyn VfsRead) -> io::Result<Vec<u8>> {
    let mut v = Vec::new();
    r.read_to_end(&mut v)?;
    Ok(v)
}

fn inflateAt(r: &mut dyn VfsRead, offset: u64) -> io::Result<Vec<u8>> {
    r.seek(SeekFrom::Start(offset))?;
    let cmf = r.readByte()?;
    let flg = r.readByte()?;
    if cmf != 0x78 || (flg != 0x01 && flg != 0x5E && flg != 0x9C && flg != 0xDA) {
        return Err(Error::new(ErrorKind::InvalidData, format!("Fastfile is not zlib ({cmf:02X}{flg:02X})")));
    }
    inflateRaw(&readRest(r)?)
}

fn inflateAuthed(r: &mut dyn VfsRead) -> io::Result<Vec<u8>> {
    r.seek(SeekFrom::Start(0))?;
    let mut file = Vec::new();
    r.read_to_end(&mut file)?;
    if file.len() < 0x4017 || file[0x4015] != 0x78 { return Err(Error::new(ErrorKind::InvalidData, "Signed fastfile has no zlib stream")); }
    let mut packed = Vec::new();
    let mut pos = 0x4015usize;
    let mut dataChunks = 0i32;
    while pos < file.len() {
        if dataChunks == 256 { pos += 0x2000; dataChunks = 0; continue; }
        let n = (file.len() - pos).min(0x2000);
        packed.extend_from_slice(&file[pos..pos + n]);
        pos += n;
        dataChunks += 1;
    }
    inflateRaw(&packed[2..])
}

fn enumName<T: std::fmt::Debug>(type_: i32, max: i32, fromRaw: fn(i32) -> Option<T>) -> String {
    if type_ >= 0 && type_ < max {
        if let Some(v) = fromRaw(type_) { return format!("{v:?}"); }
    }
    format!("type{type_}")
}

fn iw3Name(type_: i32) -> String { enumName(type_, IW3XAssetType::max as i32, IW3XAssetType::fromRaw) }
fn iw4Name(type_: i32) -> String { enumName(type_, IW4XAssetType::max as i32, IW4XAssetType::fromRaw) }
fn iw5Name(type_: i32) -> String { enumName(type_, IW5XAssetType::max as i32, IW5XAssetType::fromRaw) }
fn othName(type_: i32) -> String {
    if type_ >= 0 && type_ < Oth_AssetType::Max as i32 {
        if let Some(v) = Oth_AssetType::fromRaw(type_) { return format!("{v:?}"); }
    }
    format!("type{type_}")
}

fn list32(r: &mut dyn VfsRead, version: u32) -> io::Result<(Vec<u8>, Vec<FileHeader>)> {
    if version == CO4_WWII {
    let cmf = r.readByte()?;
    let flg = r.readByte()?;
    if cmf != 0x78 || (flg != 0x01 && flg != 0x5E && flg != 0x9C && flg != 0xDA) {
        return Err(Error::new(ErrorKind::InvalidData, format!("COD4 fastfile is not zlib ({cmf:02X}{flg:02X})")));
    }
    let zone = inflateRaw(&readRest(r)?)?;
    if zone.len() < 0x3C { return Err(Error::new(ErrorKind::InvalidData, format!("COD4 zone is {} bytes", zone.len()))); }
    let stringCount = i32At(&zone, 0x2C);
    let stringPtr = i32At(&zone, 0x30);
    let assetCount = i32At(&zone, 0x34);
    let assetPtr = i32At(&zone, 0x38);
    if stringCount < 0 || assetCount < 0 || stringCount > 1_000_000 || assetCount > 1_000_000 {
        return Err(Error::new(ErrorKind::InvalidData, format!("COD4 index counts {stringCount}/{assetCount}")));
    }
    let mut files = Vec::new();
    let mut o = 0x3Cusize;
    if stringCount > 0 {
        if stringPtr != -1 { return Err(Error::new(ErrorKind::InvalidData, format!("COD4 script strings are not inline ({stringPtr:08X})"))); }
        let mut ptrs = Vec::with_capacity(stringCount as usize);
        for i in 0..stringCount as usize { ptrs.push(i32At(&zone, o + i * 4)); }
        o += stringCount as usize * 4;
        let mut text = String::new();
        for (i, ptr) in ptrs.iter().copied().enumerate() {
            if ptr == -1 {
                let s = o;
                while o < zone.len() && zone[o] != 0 { o += 1; }
                let name = String::from_utf8_lossy(&zone[s..o]).replace('\u{FFFD}', "?");
                if o < zone.len() { o += 1; }
                if !name.is_empty() { text.push_str(&format!("{i}\t{name}\n")); }
            } else if ptr != 0 {
                return Err(Error::new(ErrorKind::InvalidData, format!("COD4 script string pointer {ptr:08X}")));
            }
        }
        if !text.is_empty() {
            let data = text.into_bytes();
            files.push(FileHeader { id: -1, path: "scriptstrings.txt".into(), position: 0, fileSize: data.len() as i64, data: Some(data) });
        }
    }
    if assetCount > 0 {
        if assetPtr != -1 { return Err(Error::new(ErrorKind::InvalidData, format!("COD4 asset list is not inline ({assetPtr:08X})"))); }
        for i in 0..assetCount as usize {
            let pos = o;
            let type_ = i32At(&zone, o);
            o += 8;
            files.push(FileHeader { id: i as i32, path: format!("{}/{:04}", iw3Name(type_), i), position: pos as i64, fileSize: 8, data: None });
        }
    }
    return Ok((zone, files));
    }
    let (nameOf, maxType): (fn(i32) -> String, i32) = if version == WaW { (iw3Name, IW3XAssetType::max as i32) } else { (othName, Oth_AssetType::Max as i32) };
    let zone = inflateAt(r, 12)?;
    if zone.len() < 0x30 { return Ok((zone, Vec::new())); }
    let count = i32At(&zone, 0x2C);
    if count <= 0 || count > 1_000_000 || 0x30 + (count as i64) * 8 > zone.len() as i64 { return Ok((zone, Vec::new())); }
    let mut files = Vec::with_capacity(count as usize);
    for i in 0..count as usize {
        let pos = 0x30 + i * 8;
        let type_ = i32At(&zone, pos + 4);
        if (type_ as u32) >= (maxType as u32) { return Ok((zone, Vec::new())); }
        files.push(FileHeader { id: i as i32, path: format!("{}/{:04}", nameOf(type_), i), position: pos as i64, fileSize: 8, data: None });
    }
    Ok((zone, files))
}

pub fn listFastFile(r: &mut dyn VfsRead, magic: u32, format_: u32, version: u32) -> io::Result<Option<(Option<Vec<u8>>, Vec<FileHeader>)>> {
    if magic == IWff && format_ == U100 && (version == CO4_WWII || version == WaW || version == BO) {
        let (z, f) = list32(r, version)?;
        return Ok(Some((Some(z), f)));
    }
    if magic == IWff && (version == MW2 || version == MW3 || (format_ == U100 && version == Ghosts)) { return Ok(Some(list64(r, format_, version)?)); }
    if magic == TAff && format_ == F100 && version == BO2 { return Ok(Some(listBo(r, false)?)); }
    if magic == TAff && format_ == F000 && version == BO3 { return Ok(Some(listBo(r, true)?)); }
    Ok(None)
}

fn list64(r: &mut dyn VfsRead, format_: u32, version: u32) -> io::Result<(Option<Vec<u8>>, Vec<FileHeader>)> {
    if version == Ghosts {
        let zone = inflateAt(r, 0x2C)?;
        let files = scanRaw(&zone, GhostsNil)?;
        return Ok((Some(zone), files));
    }
    let (countAt, recAt, rawType, maxType, nameOf): (usize, usize, i32, i32, fn(i32) -> String) = if version == MW2 {
        (0x38, 0x40, IW4XAssetType::rawfile as i32, IW4XAssetType::max as i32, iw4Name)
    } else {
        (0x3C, 0x44, IW5XAssetType::rawfile as i32, IW5XAssetType::max as i32, iw5Name)
    };
    let zone = if format_ == U100 { inflateAt(r, 0x15)? }
        else if format_ == F100 { inflateAuthed(r)? }
        else { return Ok((None, Vec::new())); };
    if zone.len() < countAt + 4 { return Ok((Some(zone), Vec::new())); }
    let count = i32At(&zone, countAt);
    if count <= 0 || count > 1_000_000 || recAt as i64 + (count as i64) * 16 > zone.len() as i64 { return Ok((Some(zone), Vec::new())); }
    let mut files = Vec::with_capacity(count as usize);
    let mut allRaw = true;
    for i in 0..count as usize {
        let pos = recAt + i * 16;
        let type_ = i64At(&zone, pos + 8);
        if type_ < 0 || type_ >= maxType as i64 { return Ok((Some(zone), Vec::new())); }
        if type_ != rawType as i64 { allRaw = false; }
        files.push(FileHeader { id: i as i32, path: format!("{}/{:04}", nameOf(type_ as i32), i), position: pos as i64, fileSize: 16, data: None });
    }
    if allRaw {
        let mut body = recAt + count as usize * 16;
        if body + 8 <= zone.len() && i64At(&zone, body) == -1 { body += 8; }
        if let Some(named) = tryWalkAll(&zone, body, count as usize, -1)? { return Ok((Some(zone), named)); }
    }
    Ok((Some(zone), files))
}

fn tryRaw(z: &[u8], pos: usize, nil: i64) -> io::Result<Option<(String, Vec<u8>, usize)>> {
    if pos + 24 > z.len() { return Ok(None); }
    let p1 = i64At(z, pos);
    let cLen = i32At(z, pos + 8);
    let len = i32At(z, pos + 12);
    let p2 = i64At(z, pos + 16);
    if p1 != nil || p2 != nil || cLen < 0 || len < 0 || cLen as usize > z.len() - pos { return Ok(None); }
    let s = pos + 24;
    let mut e = s;
    while e < z.len() && z[e] != 0 {
        if z[e] < 32 || z[e] > 126 { return Ok(None); }
        e += 1;
        if e - s > 120 { return Ok(None); }
    }
    if e >= z.len() || e == s { return Ok(None); }
    let name = String::from_utf8_lossy(&z[s..e]).into_owned();
    let mut p = e + 1;
    if (p & 1) != 0 && p < z.len() && z[p] == 0 { p += 1; }
    else if cLen == 0 && p + 1 < z.len() && z[p] == 0 && z[p + 1] == 0xFF { p += 1; }
    if p > z.len() || (z.len() - p) < cLen as usize { return Ok(None); }
    if cLen > 0 && z[p] != 0x78 { return Ok(None); }
    let data = if cLen == 0 { Vec::new() } else {
        match inflateRaw(&z[p + 2..p + cLen as usize]) {
            Ok(b) => b,
            Err(e) if e.kind() == ErrorKind::InvalidData => return Ok(None),
            Err(e) => return Err(e),
        }
    };
    Ok(Some((name, data, p + cLen as usize)))
}

fn tryWalkAll(z: &[u8], mut pos: usize, count: usize, nil: i64) -> io::Result<Option<Vec<FileHeader>>> {
    let mut files = Vec::with_capacity(count);
    for i in 0..count {
        let Some((name, data, next)) = tryRaw(z, pos, nil)? else { return Ok(None); };
        if next < pos { return Ok(None); }
        let fileSize = data.len() as i64;
        files.push(FileHeader { id: i as i32, path: name, position: 0, fileSize, data: Some(data) });
        pos = next;
    }
    Ok(Some(files))
}

fn scanRaw(z: &[u8], nil: i64) -> io::Result<Vec<FileHeader>> {
    let mut files = Vec::new();
    let mut pos = 0x40usize;
    let low = (nil as u8);
    while pos + 24 < z.len() {
        if z[pos] != low || i64At(z, pos) != nil { pos += 1; continue; }
        if tryRaw(z, pos, nil)?.is_none() { pos += 1; continue; }
        let mut run = Vec::new();
        let mut p = pos;
        let mut guard = 0i32;
        while guard < 100_000 {
            guard += 1;
            let Some((name, data, next)) = tryRaw(z, p, nil)? else { break; };
            if next < p { break; }
            let fileSize = data.len() as i64;
            run.push(FileHeader { id: (files.len() + run.len()) as i32, path: name, position: 0, fileSize, data: Some(data) });
            if next == p { break; }
            p = next;
        }
        if run.len() >= 2 || (run.len() == 1 && p >= z.len() - 16) {
            files.append(&mut run);
            if p > pos { pos = p - 1; }
        }
        pos += 1;
    }
    Ok(files)
}

fn rotl(v: u32, c: u32) -> u32 { v.rotate_left(c) }
fn loadU32(b: &[u8], o: usize) -> u32 { u32::from_le_bytes(b[o..o + 4].try_into().unwrap()) }

fn salsaHash(input: &[u32; 16]) -> [u8; 64] {
    let mut state = *input;
    let mut round = 20i32;
    while round > 0 {
        state[4] ^= rotl(state[0].wrapping_add(state[12]), 7);
        state[8] ^= rotl(state[4].wrapping_add(state[0]), 9);
        state[12] ^= rotl(state[8].wrapping_add(state[4]), 13);
        state[0] ^= rotl(state[12].wrapping_add(state[8]), 18);
        state[9] ^= rotl(state[5].wrapping_add(state[1]), 7);
        state[13] ^= rotl(state[9].wrapping_add(state[5]), 9);
        state[1] ^= rotl(state[13].wrapping_add(state[9]), 13);
        state[5] ^= rotl(state[1].wrapping_add(state[13]), 18);
        state[14] ^= rotl(state[10].wrapping_add(state[6]), 7);
        state[2] ^= rotl(state[14].wrapping_add(state[10]), 9);
        state[6] ^= rotl(state[2].wrapping_add(state[14]), 13);
        state[10] ^= rotl(state[6].wrapping_add(state[2]), 18);
        state[3] ^= rotl(state[15].wrapping_add(state[11]), 7);
        state[7] ^= rotl(state[3].wrapping_add(state[15]), 9);
        state[11] ^= rotl(state[7].wrapping_add(state[3]), 13);
        state[15] ^= rotl(state[11].wrapping_add(state[7]), 18);
        state[1] ^= rotl(state[0].wrapping_add(state[3]), 7);
        state[2] ^= rotl(state[1].wrapping_add(state[0]), 9);
        state[3] ^= rotl(state[2].wrapping_add(state[1]), 13);
        state[0] ^= rotl(state[3].wrapping_add(state[2]), 18);
        state[6] ^= rotl(state[5].wrapping_add(state[4]), 7);
        state[7] ^= rotl(state[6].wrapping_add(state[5]), 9);
        state[4] ^= rotl(state[7].wrapping_add(state[6]), 13);
        state[5] ^= rotl(state[4].wrapping_add(state[7]), 18);
        state[11] ^= rotl(state[10].wrapping_add(state[9]), 7);
        state[8] ^= rotl(state[11].wrapping_add(state[10]), 9);
        state[9] ^= rotl(state[8].wrapping_add(state[11]), 13);
        state[10] ^= rotl(state[9].wrapping_add(state[8]), 18);
        state[12] ^= rotl(state[15].wrapping_add(state[14]), 7);
        state[13] ^= rotl(state[12].wrapping_add(state[15]), 9);
        state[14] ^= rotl(state[13].wrapping_add(state[12]), 13);
        state[15] ^= rotl(state[14].wrapping_add(state[13]), 18);
        round -= 2;
    }
    let mut output = [0u8; 64];
    for index in 0..16 {
        let w = state[index].wrapping_add(input[index]);
        output[4 * index..4 * index + 4].copy_from_slice(&w.to_le_bytes());
    }
    output
}

fn salsa20(key: &[u8], iv: &[u8], data: &[u8]) -> Vec<u8> {
    let mut state = [0u32; 16];
    state[1] = loadU32(key, 0);
    state[2] = loadU32(key, 4);
    state[3] = loadU32(key, 8);
    state[4] = loadU32(key, 12);
    let constants: &[u8] = if key.len() == 32 { b"expand 32-byte k" } else { b"expand 16-byte k" };
    let keyIndex = key.len() - 16;
    state[11] = loadU32(key, keyIndex);
    state[12] = loadU32(key, keyIndex + 4);
    state[13] = loadU32(key, keyIndex + 8);
    state[14] = loadU32(key, keyIndex + 12);
    state[0] = loadU32(constants, 0);
    state[5] = loadU32(constants, 4);
    state[10] = loadU32(constants, 8);
    state[15] = loadU32(constants, 12);
    state[6] = loadU32(iv, 0);
    state[7] = loadU32(iv, 4);
    let mut output = vec![0u8; data.len()];
    let mut offset = 0usize;
    while offset < data.len() {
        let block = salsaHash(&state);
        state[8] = state[8].wrapping_add(1);
        if state[8] == 0 { state[9] = state[9].wrapping_add(1); }
        let n = (data.len() - offset).min(64);
        for i in 0..n { output[offset + i] = data[offset + i] ^ block[i]; }
        offset += n;
    }
    output
}

fn createIvTable(source: &[u8]) -> Vec<u8> {
    let mut ivTable = vec![0u8; 16000];
    let mut addDiv = 0usize;
    let nameKeyLength = source.iter().position(|b| *b == 0).unwrap_or(usize::MAX);
    if nameKeyLength == 0 || nameKeyLength == usize::MAX { return ivTable; }
    let mut i = 0usize;
    while i < ivTable.len() {
        let mut x = 0usize;
        while x < nameKeyLength * 4 {
            if i + addDiv >= ivTable.len() || i + x >= ivTable.len() { return ivTable; }
            addDiv = if x > 0 { x / 4 } else { 0 };
            for y in 0..4 { ivTable[i + x + y] = source[addDiv]; }
            x += 4;
        }
        i += nameKeyLength * 4;
    }
    ivTable
}

fn updateIvTable(index: usize, hash: &[u8], ivTable: &mut [u8], ivCounter: &mut [i32]) {
    let mut i = 0usize;
    while i < 20 {
        let value = (index + 4 * ivCounter[index] as usize) % 800 * 5;
        for x in 0..5 { ivTable[4 * value + x + i] ^= hash[i + x]; }
        i += 5;
    }
    ivCounter[index] += 1;
}

fn ivAt(index: usize, ivTable: &[u8], ivCounter: &[i32]) -> [u8; 8] {
    let arrayIndex = (index + 4 * (ivCounter[index] - 1) as usize) % 800 * 20;
    let mut iv = [0u8; 8];
    iv.copy_from_slice(&ivTable[arrayIndex..arrayIndex + 8]);
    iv
}

fn streamLen(r: &mut dyn VfsRead) -> io::Result<u64> {
    let pos = r.stream_position()?;
    let len = r.seek(SeekFrom::End(0))?;
    r.seek(SeekFrom::Start(pos))?;
    Ok(len)
}

fn listBo(r: &mut dyn VfsRead, bo3: bool) -> io::Result<(Option<Vec<u8>>, Vec<FileHeader>)> {
    if !bo3 {
        r.seek(SeekFrom::Start(0x18))?;
        let mut ivCount = [1i32, 1, 1, 1];
        let name = r.readBytes(0x20)?;
        let mut ivTable = createIvTable(&name);
        let pos = r.stream_position()?;
        r.seek(SeekFrom::Start(pos + 0x100))?;
        let length = streamLen(r)?;
        let mut zone = Vec::new();
        let mut section = 0i32;
        while r.stream_position()? + 4 <= length && section < 10_000 {
            let size = r.readInt32()?;
            if size == 0 { break; }
            if size < 0 || r.stream_position()? + size as u64 > length { return Err(Error::new(ErrorKind::InvalidData, format!("BO2 section {size}"))); }
            let cipher = r.readBytes(size as usize)?;
            let iv = ivAt((section % 4) as usize, &ivTable, &ivCount);
            let decrypted = salsa20(&Bo2Key, &iv, &cipher);
            let hash = Sha1::digest(&decrypted);
            updateIvTable((section % 4) as usize, &hash, &mut ivTable, &mut ivCount);
            zone.extend(inflateRaw(&decrypted)?);
            section += 1;
        }
        let files = harvestNames(&zone, false);
        return Ok((Some(zone), files));
    }
    r.seek(SeekFrom::Start(12))?;
    let unknown = r.readByte()?;
    let flagsZ = r.readByte()?;
    let flagsPc = r.readByte()?;
    let flagsEncrypted = r.readByte()?;
    if unknown != 0 || flagsZ != 1 || flagsPc != 0 || flagsEncrypted != 0 { return Ok((None, Vec::new())); }
    r.seek(SeekFrom::Start(0x90))?;
    let size = r.readInt64()?;
    if size <= 0 || size > 512 * 1024 * 1024 { return Err(Error::new(ErrorKind::InvalidData, format!("BO3 zone size {size}"))); }
    r.seek(SeekFrom::Start(0x248))?;
    let length = streamLen(r)?;
    let mut zone = Vec::new();
    let mut consumed = 0i64;
    while consumed < size {
        let pos = r.stream_position()?;
        if pos + 16 > length { break; }
        let compressedSize = r.readInt32()?;
        let decompressedSize = r.readInt32()?;
        let blockSize = r.readInt32()?;
        let position = r.readInt32()?;
        if position as i64 != pos as i64 { return Err(Error::new(ErrorKind::InvalidData, format!("BO3 block position {position:X}"))); }
        if decompressedSize == 0 {
            let cur = r.stream_position()?;
            r.seek(SeekFrom::Start((cur + 0x7FFFFF) & !0x7FFFFF))?;
            continue;
        }
        if compressedSize < 2 { return Err(Error::new(ErrorKind::InvalidData, format!("BO3 block compressed size {compressedSize}"))); }
        let cur = r.stream_position()?;
        r.seek(SeekFrom::Start(cur + 2))?;
        let compressed = r.readBytes((compressedSize - 2) as usize)?;
        zone.extend(inflateRaw(&compressed)?);
        consumed += decompressedSize as i64;
        let target = (position as i64) + 16 + (blockSize as i64);
        r.seek(SeekFrom::Start(target as u64))?;
    }
    let files = harvestNames(&zone, true);
    Ok((Some(zone), files))
}

fn isAssetName(name: &str) -> bool {
    let b = name.as_bytes();
    if b.len() < 3 || b.len() > 180 { return false; }
    if b.iter().any(|c| *c < 32 || *c > 126) { return false; }
    if name.contains('/') || name.contains('\\') || b[0] == b'$' { return true; }
    name.ends_with(".cfg") || name.ends_with(".csv") || name.ends_with(".gsc") || name.ends_with(".csc")
        || name.ends_with(".ddl") || name.ends_with(".txt") || name.ends_with(".vision") || name.ends_with(".menu")
}

fn harvestNames(zone: &[u8], commaPrefix: bool) -> Vec<FileHeader> {
    let mut files = Vec::new();
    let mut seen = std::collections::HashSet::new();
    let mut i = 0usize;
    while i < zone.len() {
        let mut start = i;
        if commaPrefix {
            if zone[i] != b',' || (i > 0 && zone[i - 1] >= 32) { i += 1; continue; }
            start = i + 1;
        } else if i > 0 && zone[i - 1] >= 32 && zone[i - 1] < 127 {
            i += 1;
            continue;
        }
        let mut e = start;
        while e < zone.len() && zone[e] >= 32 && zone[e] < 127 { e += 1; }
        if e >= zone.len() || zone[e] != 0 || e == start { i += 1; continue; }
        let name = String::from_utf8_lossy(&zone[start..e]).into_owned();
        if isAssetName(&name) && seen.insert(name.clone()) {
            files.push(FileHeader { id: files.len() as i32, path: name, position: start as i64, fileSize: (e - start) as i64, data: None });
        }
        i += 1;
    }
    files
}

#[cfg(test)]
mod tests {
    use std::fs::File;
    use std::io::Read;
    use std::path::PathBuf;
    use openx_poly::prelude::BinaryReaderExt;
    use openx_vfx::vfx::VfsRead;
    use super::*;

    fn ff(game: &str, rel: &str) -> PathBuf {
        let mut p = PathBuf::from(r"G:\SteamLibrary\steamapps\common");
        p.push(game);
        for part in rel.split('/') { p.push(part); }
        assert!(p.is_file(), "missing {}", p.display());
        p
    }

    fn load(game: &str, rel: &str) -> (Option<Vec<u8>>, Vec<FileHeader>) {
        let mut f = File::open(ff(game, rel)).unwrap();
        let magic = f.readUInt32().unwrap();
        let format_ = f.readUInt32().unwrap();
        let version = f.readUInt32().unwrap();
        assert!(magic == IWff || magic == S1ff || magic == TAff);
        assert!(format_ == U100 || format_ == F100 || format_ == A100 || format_ == F000);
        match listFastFile(&mut f, magic, format_, version).unwrap() {
            Some(z) => z,
            None => (None, Vec::new()),
        }
    }

    fn body(zone: &Option<Vec<u8>>, file: &FileHeader) -> Vec<u8> {
        if let Some(data) = &file.data { return data.clone(); }
        if let Some(zone) = zone {
            if file.fileSize > 0 {
                let s = file.position as usize;
                return zone[s..s + file.fileSize as usize].to_vec();
            }
        }
        Vec::new()
    }

    #[test]
    fn retailFastfiles() {
        let gate = PathBuf::from(r"G:\SteamLibrary\steamapps\common\Call of Duty 4\zone\english\code_post_gfx.ff");
        if !gate.is_file() { return; }
        let (z, files) = load("Call of Duty 4", "zone/english/code_post_gfx.ff");
        assert_eq!(files.len(), 1640);
        assert_eq!(files[0].path, "scriptstrings.txt");
        assert!(body(&z, &files[0]).starts_with(b"1\tend\n2\ttag_origin\n"));

        let (z, files) = load("Call of Duty 4", "zone/english/mp_backlot_load.ff");
        assert_eq!(files.iter().map(|f| f.path.as_str()).collect::<Vec<_>>(), ["techset/0000", "techset/0001", "material/0002", "material/0003", "material/0004", "rawfile/0005"]);
        assert_eq!(body(&z, &files[2]), vec![4, 0, 0, 0, 0xFF, 0xFF, 0xFF, 0xFF]);

        let (_, files) = load("Call of Duty World at War", "zone/english/mp_makin_day_load.ff");
        assert_eq!(files.len(), 9);
        assert_eq!(files[0].path, "sound/0000");
        assert_eq!(files[8].path, "stringtable/0008");
        assert_eq!(load("Call of Duty World at War", "zone/english/code_post_gfx.ff").1.len(), 0);

        let (_, files) = load("Call of Duty Modern Warfare 2", "zone/english/mp_underpass_load.ff");
        assert_eq!(files[0].path, "techset/0000");
        assert_eq!(files[4].path, "rawfile/0004");
        let (z, files) = load("Call of Duty Modern Warfare 2", "zone/english/code_pre_gfx.ff");
        assert_eq!(files.len(), 8);
        assert_eq!(files[0].path, "codescripts/$default");
        assert_eq!(files[1].path, "configure.csv");
        assert_eq!(files[7].path, "code_pre_gfx");
        assert!(body(&z, &files[1]).len() > 100);
        assert_eq!(load("Call of Duty Modern Warfare 2", "zone/english/code_post_gfx.ff").1.len(), 0);

        let (_, files) = load("Call of Duty Black Ops", "zone/Common/ui_mp.ff");
        assert_eq!(files.len(), 376);
        assert_eq!(files[0].path, "VertexShader/0000");
        let (_, files) = load("Call of Duty Black Ops", "zone/Common/code_pre_gfx.ff");
        assert_eq!(files.len(), 37);
        assert_eq!(files[0].path, "StringTable/0000");
        assert_eq!(files[1].path, "RawFile/0001");
        assert_eq!(load("Call of Duty Black Ops", "zone/Common/code_post_gfx.ff").1.len(), 0);

        let (_, files) = load("Call of Duty Modern Warfare 3", "zone/english/mp_alpha_load.ff");
        assert_eq!(files[0].path, "techset/0000");
        assert_eq!(files[4].path, "rawfile/0004");
        let (z, files) = load("Call of Duty Modern Warfare 3", "zone/english/code_pre_gfx.ff");
        assert_eq!(files.len(), 7);
        assert_eq!(files[6].path, "code_pre_gfx");
        assert!(body(&z, &files[1]).len() > 100);
        assert_eq!(load("Call of Duty Modern Warfare 3", "zone/english/code_post_gfx.ff").1.len(), 0);

        let (_, files) = load("Call of Duty Black Ops II", "zone/all/code_pre_gfx.ff");
        assert!(files.iter().any(|f| f.path == "configure.cfg"));
        let _ = load("Call of Duty Black Ops II", "zone/all/yemen_gump_outro.ff");
        let _ = load("Call of Duty Black Ops II", "zone/all/code_post_gfx.ff");

        let (_, files) = load("Call of Duty Ghosts", "ui_install.ff");
        assert_eq!(files.len(), 1);
        assert_eq!(files[0].path, "ui_install");
        let (z, files) = load("Call of Duty Ghosts", "code_pre_gfx.ff");
        assert_eq!(files.len(), 6);
        assert_eq!(files[4].path, "ragdoll.cfg");
        assert!(body(&z, &files[1]).len() > 100);
        let (_, files) = load("Call of Duty Ghosts", "code_post_gfx.ff");
        assert_eq!(files.len(), 101);
        assert_eq!(files[0].path, "options_graphics.cfg");
        assert_eq!(files[100].path, "code_post_gfx");

        assert_eq!(load("Call of Duty Advanced Warfare", "patch_sanfran.ff").1.len(), 0);
        assert_eq!(load("Call of Duty Advanced Warfare", "code_post_gfx.ff").1.len(), 0);
        assert!(load("Call of Duty Black Ops III", "zone/core_post_gfx.ff").1.iter().any(|f| f.path == "language.cfg"));
        assert_eq!(load("Call of Duty - Infinite Warfare", "code_pre_gfx.ff").1.len(), 0);
        assert_eq!(load("Call of Duty WWII", "code_post_gfx.ff").1.len(), 0);
        assert_eq!(load("Call of Duty Modern Warfare Remastered", "code_post_gfx.ff").1.len(), 0);
    }
}
