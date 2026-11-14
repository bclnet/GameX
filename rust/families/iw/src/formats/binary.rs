// PORT-SOURCE: Families/GameX.IW/Formats/Binary.cs
// PORT-SHA: f59ba82df59b0e9c
// PORT-STATUS: partial
//
// PARTIAL PORT: the `.ff` `Read` and `ReadData` for Magic.FF. FastFile
// readers live in this file. Each asset extension has a factory stub.
// `Binary_Iwi`'s real decoder stays in C#. Not ported: CASC, `.iwd` zip,
// `.pak`, `.ipak`, `.xpak`, and the asset bodies.
// BO4, BOCW, and Vanguard set magic CASC and list nothing.

use std::sync::atomic::Ordering;
use std::sync::{Arc, LazyLock};
use openx_poly::prelude::BinaryReaderExt;
use openx_vfx::vfx::VfsRead;
use std::io::{self, Cursor, Error, ErrorKind, Read, Seek, SeekFrom};
use flate2::read::DeflateDecoder;
use sha1::{Digest, Sha1};
use gamex::binary::{ArcBinary, Archive, BinaryArchive, Stream, Task};
use gamex::meta::{extension, fileName, FileSource, IHaveMetaInfo, MetaContent, MetaInfo, MetaManager, Tag};
fn reader<'a>(r: Option<&'a mut dyn VfsRead>) -> io::Result<&'a mut dyn VfsRead> { r.ok_or_else(|| Error::new(ErrorKind::InvalidInput, "r")) }
fn fromResult<'a>(value: io::Result<Tag>) -> Task<'a, Tag> { Box::pin(std::future::ready(value)) }

pub struct Binary_IW;

impl Binary_IW {
    pub fn current() -> Arc<dyn ArcBinary> {
        static CURRENT: LazyLock<Arc<dyn ArcBinary>> = LazyLock::new(|| Arc::new(Binary_IW));
        CURRENT.clone()
    }
}

impl ArcBinary for Binary_IW {
    fn read<'a>(&'a self, source: &'a BinaryArchive, r: Option<&'a mut dyn VfsRead>, _tag: &'a Tag) -> Task<'a, ()> {
        Box::pin(async move {
            let r = reader(r)?;
            source.setFiles(Some(Vec::new()));
            let id = source.base.game.id.as_str();
            if id == "BO4" || id == "BOCW" || id == "Vanguard" {
                source.magic.store(0, Ordering::Relaxed);
                return Ok(());
            }
            if !extension(&source.base.binPath).eq_ignore_ascii_case(".ff") { return Ok(()); }
            source.magic.store(2, Ordering::Relaxed);
            let magic = r.readUInt32()?;
            let format_ = r.readUInt32()?;
            let version = r.readUInt32()?;
            if magic != IWff && magic != S1ff && magic != TAff { return Err(Error::new(ErrorKind::InvalidData, format!("Bad magic {magic}"))); }
            if format_ != U100 && format_ != F100 && format_ != A100 && format_ != F000 { return Err(Error::new(ErrorKind::InvalidData, format!("Bad format {format_}"))); }
            let Some((zone, headers)) = Self::listFastFile(r, magic, format_, version)? else { return Ok(()); };
            *source.base.tag.lock().unwrap_or_else(|p| p.into_inner()) = match zone {
                Some(z) => Tag::Bytes(z),
                None => Tag::None,
            };
            let files = headers.into_iter().map(|asset| Arc::new(FileSource {
                id: asset.id,
                path: asset.path,
                offset: asset.position,
                fileSize: asset.fileSize,
                data: asset.data,
                ..Default::default()
            })).collect();
            source.setFiles(Some(files));
            Ok(())
        })
    }

    fn readData<'a>(&'a self, source: &'a BinaryArchive, _r: Option<&'a mut dyn VfsRead>, file: &'a Arc<FileSource>, _option: &'a Tag) -> Task<'a, Option<Stream>> {
        Box::pin(async move {
            if let Some(data) = &file.data { return Ok(Some(Box::new(Cursor::new(data.clone())) as Stream)); }
            let tag = source.base.tag.lock().unwrap_or_else(|p| p.into_inner()).clone();
            if let Tag::Bytes(zone) = tag {
                if file.fileSize > 0 {
                    if file.offset < 0 { return Err(Error::new(ErrorKind::InvalidInput, "offset")); }
                    let start = file.offset as usize;
                    let end = start + file.fileSize as usize;
                    if end < start || end > zone.len() { return Err(Error::new(ErrorKind::InvalidInput, "offset")); }
                    return Ok(Some(Box::new(Cursor::new(zone[start..end].to_vec())) as Stream));
                }
            }
            Ok(Some(Box::new(Cursor::new(Vec::new())) as Stream))
        })
    }
}

// #region FastFile

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

// Index names. Same strings as the C# enums, which stay in ZoneUnused and are not ported.
const IW3_NAMES: &[&str] = &[
    "xmodelpieces", "physpreset", "xanim", "xmodel", "material", "techset", "image", "sound",
    "sndcurve", "loaded_sound", "col_map_sp", "col_map_mp", "com_map", "game_map_sp", "game_map_mp",
    "map_ents", "gfx_map", "lightdef", "ui_map", "font", "menufile", "menu", "localize", "weapon",
    "snddriverglobals", "fx", "impactfx", "aitype", "mptype", "character", "xmodelalias", "rawfile",
    "stringtable",
];
const IW4_NAMES: &[&str] = &[
    "physpreset", "phys_collmap", "xanim", "xmodelsurfs", "xmodel", "material", "pixelshader",
    "vertexshader", "vertexdecl", "techset", "image", "sound", "sndcurve", "loaded_sound",
    "col_map_sp", "col_map_mp", "com_map", "game_map_sp", "game_map_mp", "map_ents", "fx_map",
    "gfx_map", "lightdef", "ui_map", "font", "menufile", "menu", "localize", "weapon",
    "snddriverglobals", "fx", "impactfx", "aitype", "mptype", "character", "xmodelalias", "rawfile",
    "stringtable", "leaderboarddef", "structureddatadef", "tracer", "vehicle", "addon_map_ents",
];
const IW5_NAMES: &[&str] = &[
    "physpreset", "phys_collmap", "xanim", "xmodelsurfs", "xmodel", "material", "pixelshader",
    "vertexshader", "vertexdecl", "techset", "image", "sound", "sndcurve", "loaded_sound",
    "col_map_mp", "com_map", "glass_map", "ai_paths", "vehicle_tracks", "map_ents", "fx_map",
    "gfx_map", "lightdef", "ui_map", "font", "menufile", "menu", "localize", "attachment", "weapon",
    "snddriverglobals", "fx", "impactfx", "surfacefx", "aitype", "mptype", "character", "xmodelalias",
    "rawfile", "scriptfile", "stringtable", "leaderboarddef", "structureddatadef", "tracer", "vehicle",
    "addon_map_ents",
];
const OTH_NAMES: &[&str] = &[
    "PhysPreset", "PhysColMap", "XAnim", "XModelSurfs", "XModel", "Material", "PixelShader",
    "VertexShader", "VertexDecl", "TechSet", "Image", "Sound", "SndCurve", "LoadedSound",
    "ColMapSP", "ColMapMP", "ComMap", "GameMapSP", "GameMapMP", "MapEnts", "FXMap", "GfxMap",
    "LightDef", "UIMap", "Font", "MenuFile", "Menu", "Localize", "Weapon", "SndDriverGlobals",
    "FX", "ImpactFX", "AIType", "MPType", "Character", "XModelAlias", "RawFile", "StringTable",
    "LeaderBoardDef", "StructuredDataDef", "Tracer", "Vehical", "AddONMapEnts",
];

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

impl Binary_IW {
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
        Self::inflateRaw(&Self::readRest(r)?)
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
        Self::inflateRaw(&packed[2..])
    }

    fn typeName(type_: i32, names: &[&str]) -> String {
        if type_ >= 0 {
            let i = type_ as usize;
            if i < names.len() { return names[i].to_string(); }
        }
        format!("type{type_}")
    }
    fn nameIndex(names: &[&str], name: &str) -> i32 { names.iter().position(|s| *s == name).expect(name) as i32 }
    fn iw3Name(type_: i32) -> String { Self::typeName(type_, IW3_NAMES) }
    fn iw4Name(type_: i32) -> String { Self::typeName(type_, IW4_NAMES) }
    fn iw5Name(type_: i32) -> String { Self::typeName(type_, IW5_NAMES) }
    fn othName(type_: i32) -> String { Self::typeName(type_, OTH_NAMES) }

    fn assetExt(typeName: &str) -> String {
        let key = typeName.to_ascii_lowercase().replace('_', "");
        match key.as_str() {
            "image" => ".iwi".into(),
            "scriptfile" => ".gsc".into(),
            "xmodelpieces" => ".xmp".into(),
            "physpreset" => ".phy".into(),
            "xanim" => ".xan".into(),
            "xmodel" => ".xmd".into(),
            "material" => ".mat".into(),
            "techset" => ".tec".into(),
            "sound" => ".snd".into(),
            "sndcurve" => ".crv".into(),
            "loadedsound" => ".lds".into(),
            "colmapsp" => ".csp".into(),
            "colmapmp" => ".cmp".into(),
            "commap" => ".cmm".into(),
            "gamemapsp" => ".gsp".into(),
            "gamemapmp" => ".gmp".into(),
            "mapents" => ".ent".into(),
            "gfxmap" => ".gfx".into(),
            "lightdef" => ".lgt".into(),
            "uimap" => ".uim".into(),
            "font" => ".fnt".into(),
            "menufile" => ".mnf".into(),
            "menu" => ".mnu".into(),
            "localize" => ".loc".into(),
            "weapon" => ".wep".into(),
            "snddriverglobals" => ".sdg".into(),
            "fx" => ".fx".into(),
            "impactfx" => ".ifx".into(),
            "aitype" => ".ait".into(),
            "mptype" => ".mpt".into(),
            "character" => ".chr".into(),
            "xmodelalias" => ".xma".into(),
            "rawfile" => ".raw".into(),
            "stringtable" => ".tbl".into(),
            "physcolmap" | "physcollmap" => ".pcm".into(),
            "xmodelsurfs" => ".xms".into(),
            "pixelshader" => ".psh".into(),
            "vertexshader" => ".vsh".into(),
            "vertexdecl" => ".vdl".into(),
            "fxmap" => ".fxm".into(),
            "leaderboarddef" => ".lbd".into(),
            "structureddatadef" => ".sdd".into(),
            "tracer" => ".trc".into(),
            "vehicle" | "vehical" => ".veh".into(),
            "addonmapents" => ".ame".into(),
            "glassmap" => ".gls".into(),
            "aipaths" => ".aip".into(),
            "vehicletracks" => ".vtr".into(),
            "attachment" => ".att".into(),
            "surfacefx" => ".sfx".into(),
            _ => {
                let n = if key.len() <= 3 { key.len() } else { 3 };
                format!(".{}", &key[..n])
            }
        }
    }

    fn indexPath(typeName: &str, i: usize) -> String {
        format!("{typeName}/{:04}{}", i, Self::assetExt(typeName))
    }

    fn list32(r: &mut dyn VfsRead, version: u32) -> io::Result<(Vec<u8>, Vec<FileHeader>)> {
        if version == CO4_WWII {
        let cmf = r.readByte()?;
        let flg = r.readByte()?;
        if cmf != 0x78 || (flg != 0x01 && flg != 0x5E && flg != 0x9C && flg != 0xDA) {
            return Err(Error::new(ErrorKind::InvalidData, format!("COD4 fastfile is not zlib ({cmf:02X}{flg:02X})")));
        }
        let zone = Self::inflateRaw(&Self::readRest(r)?)?;
        if zone.len() < 0x3C { return Err(Error::new(ErrorKind::InvalidData, format!("COD4 zone is {} bytes", zone.len()))); }
        let stringCount = Self::i32At(&zone, 0x2C);
        let stringPtr = Self::i32At(&zone, 0x30);
        let assetCount = Self::i32At(&zone, 0x34);
        let assetPtr = Self::i32At(&zone, 0x38);
        if stringCount < 0 || assetCount < 0 || stringCount > 1_000_000 || assetCount > 1_000_000 {
            return Err(Error::new(ErrorKind::InvalidData, format!("COD4 index counts {stringCount}/{assetCount}")));
        }
        let mut files = Vec::new();
        let mut o = 0x3Cusize;
        if stringCount > 0 {
            if stringPtr != -1 { return Err(Error::new(ErrorKind::InvalidData, format!("COD4 script strings are not inline ({stringPtr:08X})"))); }
            let mut ptrs = Vec::with_capacity(stringCount as usize);
            for i in 0..stringCount as usize { ptrs.push(Self::i32At(&zone, o + i * 4)); }
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
                let type_ = Self::i32At(&zone, o);
                o += 8;
                files.push(FileHeader { id: i as i32, path: Self::indexPath(&Self::iw3Name(type_), i), position: pos as i64, fileSize: 8, data: None });
            }
        }
        return Ok((zone, files));
        }
        let (nameOf, maxType): (fn(i32) -> String, i32) = if version == WaW { (Self::iw3Name, IW3_NAMES.len() as i32) } else { (Self::othName, OTH_NAMES.len() as i32) };
        let zone = Self::inflateAt(r, 12)?;
        if zone.len() < 0x30 { return Ok((zone, Vec::new())); }
        let count = Self::i32At(&zone, 0x2C);
        if count <= 0 || count > 1_000_000 || 0x30 + (count as i64) * 8 > zone.len() as i64 { return Ok((zone, Vec::new())); }
        let mut files = Vec::with_capacity(count as usize);
        for i in 0..count as usize {
            let pos = 0x30 + i * 8;
            let type_ = Self::i32At(&zone, pos + 4);
            if (type_ as u32) >= (maxType as u32) { return Ok((zone, Vec::new())); }
            files.push(FileHeader { id: i as i32, path: Self::indexPath(&nameOf(type_), i), position: pos as i64, fileSize: 8, data: None });
        }
        Ok((zone, files))
    }

    pub fn listFastFile(r: &mut dyn VfsRead, magic: u32, format_: u32, version: u32) -> io::Result<Option<(Option<Vec<u8>>, Vec<FileHeader>)>> {
        if magic == IWff && format_ == U100 && (version == CO4_WWII || version == WaW || version == BO) {
            let (z, f) = Self::list32(r, version)?;
            return Ok(Some((Some(z), f)));
        }
        if magic == IWff && (version == MW2 || version == MW3 || (format_ == U100 && version == Ghosts)) { return Ok(Some(Self::list64(r, format_, version)?)); }
        if magic == TAff && format_ == F100 && version == BO2 { return Ok(Some(Self::listBo(r, false)?)); }
        if magic == TAff && format_ == F000 && version == BO3 { return Ok(Some(Self::listBo(r, true)?)); }
        Ok(None)
    }

    fn list64(r: &mut dyn VfsRead, format_: u32, version: u32) -> io::Result<(Option<Vec<u8>>, Vec<FileHeader>)> {
        if version == Ghosts {
            let zone = Self::inflateAt(r, 0x2C)?;
            let files = Self::scanRaw(&zone, GhostsNil)?;
            return Ok((Some(zone), files));
        }
        let (countAt, recAt, rawType, maxType, nameOf): (usize, usize, i32, i32, fn(i32) -> String) = if version == MW2 {
            (0x38, 0x40, Self::nameIndex(IW4_NAMES, "rawfile"), IW4_NAMES.len() as i32, Self::iw4Name)
        } else {
            (0x3C, 0x44, Self::nameIndex(IW5_NAMES, "rawfile"), IW5_NAMES.len() as i32, Self::iw5Name)
        };
        let zone = if format_ == U100 { Self::inflateAt(r, 0x15)? }
            else if format_ == F100 { Self::inflateAuthed(r)? }
            else { return Ok((None, Vec::new())); };
        if zone.len() < countAt + 4 { return Ok((Some(zone), Vec::new())); }
        let count = Self::i32At(&zone, countAt);
        if count <= 0 || count > 1_000_000 || recAt as i64 + (count as i64) * 16 > zone.len() as i64 { return Ok((Some(zone), Vec::new())); }
        let mut files = Vec::with_capacity(count as usize);
        let mut allRaw = true;
        for i in 0..count as usize {
            let pos = recAt + i * 16;
            let type_ = Self::i64At(&zone, pos + 8);
            if type_ < 0 || type_ >= maxType as i64 { return Ok((Some(zone), Vec::new())); }
            if type_ != rawType as i64 { allRaw = false; }
            files.push(FileHeader { id: i as i32, path: Self::indexPath(&nameOf(type_ as i32), i), position: pos as i64, fileSize: 16, data: None });
        }
        if allRaw {
            let mut body = recAt + count as usize * 16;
            if body + 8 <= zone.len() && Self::i64At(&zone, body) == -1 { body += 8; }
            if let Some(named) = Self::tryWalkAll(&zone, body, count as usize, -1)? { return Ok((Some(zone), named)); }
        }
        Ok((Some(zone), files))
    }

    fn tryRaw(z: &[u8], pos: usize, nil: i64) -> io::Result<Option<(String, Vec<u8>, usize)>> {
        if pos + 24 > z.len() { return Ok(None); }
        let p1 = Self::i64At(z, pos);
        let cLen = Self::i32At(z, pos + 8);
        let len = Self::i32At(z, pos + 12);
        let p2 = Self::i64At(z, pos + 16);
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
            match Self::inflateRaw(&z[p + 2..p + cLen as usize]) {
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
            let Some((name, data, next)) = Self::tryRaw(z, pos, nil)? else { return Ok(None); };
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
            if z[pos] != low || Self::i64At(z, pos) != nil { pos += 1; continue; }
            if Self::tryRaw(z, pos, nil)?.is_none() { pos += 1; continue; }
            let mut run = Vec::new();
            let mut p = pos;
            let mut guard = 0i32;
            while guard < 100_000 {
                guard += 1;
                let Some((name, data, next)) = Self::tryRaw(z, p, nil)? else { break; };
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
            state[4] ^= Self::rotl(state[0].wrapping_add(state[12]), 7);
            state[8] ^= Self::rotl(state[4].wrapping_add(state[0]), 9);
            state[12] ^= Self::rotl(state[8].wrapping_add(state[4]), 13);
            state[0] ^= Self::rotl(state[12].wrapping_add(state[8]), 18);
            state[9] ^= Self::rotl(state[5].wrapping_add(state[1]), 7);
            state[13] ^= Self::rotl(state[9].wrapping_add(state[5]), 9);
            state[1] ^= Self::rotl(state[13].wrapping_add(state[9]), 13);
            state[5] ^= Self::rotl(state[1].wrapping_add(state[13]), 18);
            state[14] ^= Self::rotl(state[10].wrapping_add(state[6]), 7);
            state[2] ^= Self::rotl(state[14].wrapping_add(state[10]), 9);
            state[6] ^= Self::rotl(state[2].wrapping_add(state[14]), 13);
            state[10] ^= Self::rotl(state[6].wrapping_add(state[2]), 18);
            state[3] ^= Self::rotl(state[15].wrapping_add(state[11]), 7);
            state[7] ^= Self::rotl(state[3].wrapping_add(state[15]), 9);
            state[11] ^= Self::rotl(state[7].wrapping_add(state[3]), 13);
            state[15] ^= Self::rotl(state[11].wrapping_add(state[7]), 18);
            state[1] ^= Self::rotl(state[0].wrapping_add(state[3]), 7);
            state[2] ^= Self::rotl(state[1].wrapping_add(state[0]), 9);
            state[3] ^= Self::rotl(state[2].wrapping_add(state[1]), 13);
            state[0] ^= Self::rotl(state[3].wrapping_add(state[2]), 18);
            state[6] ^= Self::rotl(state[5].wrapping_add(state[4]), 7);
            state[7] ^= Self::rotl(state[6].wrapping_add(state[5]), 9);
            state[4] ^= Self::rotl(state[7].wrapping_add(state[6]), 13);
            state[5] ^= Self::rotl(state[4].wrapping_add(state[7]), 18);
            state[11] ^= Self::rotl(state[10].wrapping_add(state[9]), 7);
            state[8] ^= Self::rotl(state[11].wrapping_add(state[10]), 9);
            state[9] ^= Self::rotl(state[8].wrapping_add(state[11]), 13);
            state[10] ^= Self::rotl(state[9].wrapping_add(state[8]), 18);
            state[12] ^= Self::rotl(state[15].wrapping_add(state[14]), 7);
            state[13] ^= Self::rotl(state[12].wrapping_add(state[15]), 9);
            state[14] ^= Self::rotl(state[13].wrapping_add(state[12]), 13);
            state[15] ^= Self::rotl(state[14].wrapping_add(state[13]), 18);
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
        state[1] = Self::loadU32(key, 0);
        state[2] = Self::loadU32(key, 4);
        state[3] = Self::loadU32(key, 8);
        state[4] = Self::loadU32(key, 12);
        let constants: &[u8] = if key.len() == 32 { b"expand 32-byte k" } else { b"expand 16-byte k" };
        let keyIndex = key.len() - 16;
        state[11] = Self::loadU32(key, keyIndex);
        state[12] = Self::loadU32(key, keyIndex + 4);
        state[13] = Self::loadU32(key, keyIndex + 8);
        state[14] = Self::loadU32(key, keyIndex + 12);
        state[0] = Self::loadU32(constants, 0);
        state[5] = Self::loadU32(constants, 4);
        state[10] = Self::loadU32(constants, 8);
        state[15] = Self::loadU32(constants, 12);
        state[6] = Self::loadU32(iv, 0);
        state[7] = Self::loadU32(iv, 4);
        let mut output = vec![0u8; data.len()];
        let mut offset = 0usize;
        while offset < data.len() {
            let block = Self::salsaHash(&state);
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
            let mut ivTable = Self::createIvTable(&name);
            let pos = r.stream_position()?;
            r.seek(SeekFrom::Start(pos + 0x100))?;
            let length = Self::streamLen(r)?;
            let mut zone = Vec::new();
            let mut section = 0i32;
            while r.stream_position()? + 4 <= length && section < 10_000 {
                let size = r.readInt32()?;
                if size == 0 { break; }
                if size < 0 || r.stream_position()? + size as u64 > length { return Err(Error::new(ErrorKind::InvalidData, format!("BO2 section {size}"))); }
                let cipher = r.readBytes(size as usize)?;
                let iv = Self::ivAt((section % 4) as usize, &ivTable, &ivCount);
                let decrypted = Self::salsa20(&Bo2Key, &iv, &cipher);
                let hash = Sha1::digest(&decrypted);
                Self::updateIvTable((section % 4) as usize, &hash, &mut ivTable, &mut ivCount);
                zone.extend(Self::inflateRaw(&decrypted)?);
                section += 1;
            }
            let files = Self::harvestNames(&zone, false);
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
        let length = Self::streamLen(r)?;
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
            zone.extend(Self::inflateRaw(&compressed)?);
            consumed += decompressedSize as i64;
            let target = (position as i64) + 16 + (blockSize as i64);
            r.seek(SeekFrom::Start(target as u64))?;
        }
        let files = Self::harvestNames(&zone, true);
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
            if Self::isAssetName(&name) && seen.insert(name.clone()) {
                files.push(FileHeader { id: files.len() as i32, path: name, position: start as i64, fileSize: (e - start) as i64, data: None });
            }
            i += 1;
        }
        files
    }

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
        match Binary_IW::listFastFile(&mut f, magic, format_, version).unwrap() {
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
    fn assetExtNames() {
        assert_eq!(Binary_IW::indexPath("image", 1), "image/0001.iwi");
        assert_eq!(Binary_IW::indexPath("Image", 1), "Image/0001.iwi");
        assert_eq!(Binary_IW::indexPath("PhysColMap", 0), "PhysColMap/0000.pcm");
        assert_eq!(Binary_IW::indexPath("phys_collmap", 0), "phys_collmap/0000.pcm");
        assert_eq!(Binary_IW::indexPath("LoadedSound", 3), "LoadedSound/0003.lds");
        assert_eq!(Binary_IW::indexPath("loaded_sound", 3), "loaded_sound/0003.lds");
        assert_eq!(Binary_IW::indexPath("scriptfile", 4), "scriptfile/0004.gsc");
        assert_eq!(Binary_IW::indexPath("ColMapSP", 5), "ColMapSP/0005.csp");
        assert_eq!(Binary_IW::indexPath("col_map_mp", 6), "col_map_mp/0006.cmp");
        assert_eq!(Binary_IW::indexPath("ComMap", 7), "ComMap/0007.cmm");
        assert_eq!(Binary_IW::indexPath("GameMapSP", 8), "GameMapSP/0008.gsp");
        assert_eq!(Binary_IW::indexPath("GameMapMP", 9), "GameMapMP/0009.gmp");
        assert_eq!(Binary_IW::indexPath("MapEnts", 10), "MapEnts/0010.ent");
        assert_eq!(Binary_IW::indexPath("FXMap", 11), "FXMap/0011.fxm");
        assert_eq!(Binary_IW::indexPath("GfxMap", 12), "GfxMap/0012.gfx");
        assert_eq!(Binary_IW::indexPath("UIMap", 13), "UIMap/0013.uim");
        assert_eq!(Binary_IW::indexPath("Vehical", 2), "Vehical/0002.veh");
        assert_eq!(Binary_IW::indexPath("vehicle", 2), "vehicle/0002.veh");
        assert_eq!(Binary_IW::indexPath("AddONMapEnts", 14), "AddONMapEnts/0014.ame");
        assert_eq!(Binary_IW::indexPath("VertexShader", 0), "VertexShader/0000.vsh");
        assert_eq!(Binary_IW::indexPath("material", 2), "material/0002.mat");
        assert_eq!(Binary_IW::indexPath("techset", 0), "techset/0000.tec");
        assert_eq!(Binary_IW::indexPath("fx", 0), "fx/0000.fx");
        assert_eq!(Binary_IW::indexPath("xmodel", 0), "xmodel/0000.xmd");
        assert_eq!(Binary_IW::indexPath("xmodelpieces", 0), "xmodelpieces/0000.xmp");
        assert_eq!(Binary_IW::iw3Name(7), "sound");
        assert_eq!(Binary_IW::iw3Name(31), "rawfile");
        assert_eq!(Binary_IW::iw3Name(32), "stringtable");
        assert_eq!(Binary_IW::iw3Name(33), "type33");
        assert_eq!(Binary_IW::iw4Name(36), "rawfile");
        assert_eq!(Binary_IW::iw4Name(43), "type43");
        assert_eq!(Binary_IW::iw5Name(14), "col_map_mp");
        assert_eq!(Binary_IW::iw5Name(38), "rawfile");
        assert_eq!(Binary_IW::iw5Name(46), "type46");
        assert_eq!(Binary_IW::othName(41), "Vehical");
        assert_eq!(Binary_IW::othName(42), "AddONMapEnts");
        assert_eq!(Binary_IW::othName(43), "type43");
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
        assert_eq!(files.iter().map(|f| f.path.as_str()).collect::<Vec<_>>(), ["techset/0000.tec", "techset/0001.tec", "material/0002.mat", "material/0003.mat", "material/0004.mat", "rawfile/0005.raw"]);
        assert_eq!(body(&z, &files[2]), vec![4, 0, 0, 0, 0xFF, 0xFF, 0xFF, 0xFF]);

        let (_, files) = load("Call of Duty World at War", "zone/english/mp_makin_day_load.ff");
        assert_eq!(files.len(), 9);
        assert_eq!(files[0].path, "sound/0000.snd");
        assert_eq!(files[1].path, "image/0001.iwi");
        assert_eq!(files[8].path, "stringtable/0008.tbl");
        assert_eq!(load("Call of Duty World at War", "zone/english/code_post_gfx.ff").1.len(), 0);

        let (_, files) = load("Call of Duty Modern Warfare 2", "zone/english/mp_underpass_load.ff");
        assert_eq!(files[0].path, "techset/0000.tec");
        assert_eq!(files[4].path, "rawfile/0004.raw");
        let (z, files) = load("Call of Duty Modern Warfare 2", "zone/english/code_pre_gfx.ff");
        assert_eq!(files.len(), 8);
        assert_eq!(files[0].path, "codescripts/$default");
        assert_eq!(files[1].path, "configure.csv");
        assert_eq!(files[7].path, "code_pre_gfx");
        assert!(body(&z, &files[1]).len() > 100);
        assert_eq!(load("Call of Duty Modern Warfare 2", "zone/english/code_post_gfx.ff").1.len(), 0);

        let (_, files) = load("Call of Duty Black Ops", "zone/Common/ui_mp.ff");
        assert_eq!(files.len(), 376);
        assert_eq!(files[0].path, "VertexShader/0000.vsh");
        let (_, files) = load("Call of Duty Black Ops", "zone/Common/code_pre_gfx.ff");
        assert_eq!(files.len(), 37);
        assert_eq!(files[0].path, "StringTable/0000.tbl");
        assert_eq!(files[1].path, "RawFile/0001.raw");
        assert_eq!(load("Call of Duty Black Ops", "zone/Common/code_post_gfx.ff").1.len(), 0);

        let (_, files) = load("Call of Duty Modern Warfare 3", "zone/english/mp_alpha_load.ff");
        assert_eq!(files[0].path, "techset/0000.tec");
        assert_eq!(files[4].path, "rawfile/0004.raw");
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

// #endregion

// Nameless fastfile rows are the index record. These factories do not read it.

// C# Binary_Iwi reads the IWi header. This stub does not.
// #region Binary_Iwi

pub struct Binary_Iwi;

impl Binary_Iwi {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Iwi))))
    }
}

impl IHaveMetaInfo for Binary_Iwi {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "IWI".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("IWI".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Gsc

pub struct Binary_Gsc;

impl Binary_Gsc {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Gsc))))
    }
}

impl IHaveMetaInfo for Binary_Gsc {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "GSC".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("GSC".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Xmp

pub struct Binary_Xmp;

impl Binary_Xmp {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Xmp))))
    }
}

impl IHaveMetaInfo for Binary_Xmp {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "XMP".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("XMP".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Phy

pub struct Binary_Phy;

impl Binary_Phy {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Phy))))
    }
}

impl IHaveMetaInfo for Binary_Phy {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "PHY".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("PHY".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Xan

pub struct Binary_Xan;

impl Binary_Xan {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Xan))))
    }
}

impl IHaveMetaInfo for Binary_Xan {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "XAN".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("XAN".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Xmd

pub struct Binary_Xmd;

impl Binary_Xmd {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Xmd))))
    }
}

impl IHaveMetaInfo for Binary_Xmd {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "XMD".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("XMD".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Mat

pub struct Binary_Mat;

impl Binary_Mat {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Mat))))
    }
}

impl IHaveMetaInfo for Binary_Mat {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "MAT".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("MAT".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Tec

pub struct Binary_Tec;

impl Binary_Tec {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Tec))))
    }
}

impl IHaveMetaInfo for Binary_Tec {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "TEC".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("TEC".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Snd

pub struct Binary_Snd;

impl Binary_Snd {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Snd))))
    }
}

impl IHaveMetaInfo for Binary_Snd {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "SND".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("SND".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Crv

pub struct Binary_Crv;

impl Binary_Crv {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Crv))))
    }
}

impl IHaveMetaInfo for Binary_Crv {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "CRV".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("CRV".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Lds

pub struct Binary_Lds;

impl Binary_Lds {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Lds))))
    }
}

impl IHaveMetaInfo for Binary_Lds {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "LDS".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("LDS".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Csp

pub struct Binary_Csp;

impl Binary_Csp {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Csp))))
    }
}

impl IHaveMetaInfo for Binary_Csp {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "CSP".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("CSP".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Cmp

pub struct Binary_Cmp;

impl Binary_Cmp {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Cmp))))
    }
}

impl IHaveMetaInfo for Binary_Cmp {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "CMP".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("CMP".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Cmm

pub struct Binary_Cmm;

impl Binary_Cmm {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Cmm))))
    }
}

impl IHaveMetaInfo for Binary_Cmm {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "CMM".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("CMM".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Gsp

pub struct Binary_Gsp;

impl Binary_Gsp {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Gsp))))
    }
}

impl IHaveMetaInfo for Binary_Gsp {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "GSP".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("GSP".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Gmp

pub struct Binary_Gmp;

impl Binary_Gmp {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Gmp))))
    }
}

impl IHaveMetaInfo for Binary_Gmp {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "GMP".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("GMP".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Ent

pub struct Binary_Ent;

impl Binary_Ent {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Ent))))
    }
}

impl IHaveMetaInfo for Binary_Ent {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "ENT".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("ENT".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Gfx

pub struct Binary_Gfx;

impl Binary_Gfx {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Gfx))))
    }
}

impl IHaveMetaInfo for Binary_Gfx {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "GFX".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("GFX".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Lgt

pub struct Binary_Lgt;

impl Binary_Lgt {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Lgt))))
    }
}

impl IHaveMetaInfo for Binary_Lgt {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "LGT".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("LGT".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Uim

pub struct Binary_Uim;

impl Binary_Uim {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Uim))))
    }
}

impl IHaveMetaInfo for Binary_Uim {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "UIM".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("UIM".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Fnt

pub struct Binary_Fnt;

impl Binary_Fnt {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Fnt))))
    }
}

impl IHaveMetaInfo for Binary_Fnt {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "FNT".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("FNT".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Mnf

pub struct Binary_Mnf;

impl Binary_Mnf {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Mnf))))
    }
}

impl IHaveMetaInfo for Binary_Mnf {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "MNF".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("MNF".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Mnu

pub struct Binary_Mnu;

impl Binary_Mnu {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Mnu))))
    }
}

impl IHaveMetaInfo for Binary_Mnu {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "MNU".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("MNU".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Loc

pub struct Binary_Loc;

impl Binary_Loc {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Loc))))
    }
}

impl IHaveMetaInfo for Binary_Loc {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "LOC".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("LOC".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Wep

pub struct Binary_Wep;

impl Binary_Wep {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Wep))))
    }
}

impl IHaveMetaInfo for Binary_Wep {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "WEP".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("WEP".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Sdg

pub struct Binary_Sdg;

impl Binary_Sdg {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Sdg))))
    }
}

impl IHaveMetaInfo for Binary_Sdg {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "SDG".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("SDG".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Fx

pub struct Binary_Fx;

impl Binary_Fx {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Fx))))
    }
}

impl IHaveMetaInfo for Binary_Fx {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "FX".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("FX".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Ifx

pub struct Binary_Ifx;

impl Binary_Ifx {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Ifx))))
    }
}

impl IHaveMetaInfo for Binary_Ifx {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "IFX".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("IFX".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Ait

pub struct Binary_Ait;

impl Binary_Ait {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Ait))))
    }
}

impl IHaveMetaInfo for Binary_Ait {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "AIT".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("AIT".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Mpt

pub struct Binary_Mpt;

impl Binary_Mpt {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Mpt))))
    }
}

impl IHaveMetaInfo for Binary_Mpt {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "MPT".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("MPT".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Chr

pub struct Binary_Chr;

impl Binary_Chr {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Chr))))
    }
}

impl IHaveMetaInfo for Binary_Chr {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "CHR".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("CHR".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Xma

pub struct Binary_Xma;

impl Binary_Xma {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Xma))))
    }
}

impl IHaveMetaInfo for Binary_Xma {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "XMA".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("XMA".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Raw

pub struct Binary_Raw;

impl Binary_Raw {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Raw))))
    }
}

impl IHaveMetaInfo for Binary_Raw {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "RAW".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("RAW".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Tbl

pub struct Binary_Tbl;

impl Binary_Tbl {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Tbl))))
    }
}

impl IHaveMetaInfo for Binary_Tbl {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "TBL".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("TBL".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Pcm

pub struct Binary_Pcm;

impl Binary_Pcm {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Pcm))))
    }
}

impl IHaveMetaInfo for Binary_Pcm {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "PCM".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("PCM".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Xms

pub struct Binary_Xms;

impl Binary_Xms {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Xms))))
    }
}

impl IHaveMetaInfo for Binary_Xms {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "XMS".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("XMS".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Psh

pub struct Binary_Psh;

impl Binary_Psh {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Psh))))
    }
}

impl IHaveMetaInfo for Binary_Psh {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "PSH".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("PSH".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Vsh

pub struct Binary_Vsh;

impl Binary_Vsh {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Vsh))))
    }
}

impl IHaveMetaInfo for Binary_Vsh {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "VSH".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("VSH".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Vdl

pub struct Binary_Vdl;

impl Binary_Vdl {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Vdl))))
    }
}

impl IHaveMetaInfo for Binary_Vdl {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "VDL".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("VDL".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Fxm

pub struct Binary_Fxm;

impl Binary_Fxm {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Fxm))))
    }
}

impl IHaveMetaInfo for Binary_Fxm {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "FXM".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("FXM".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Lbd

pub struct Binary_Lbd;

impl Binary_Lbd {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Lbd))))
    }
}

impl IHaveMetaInfo for Binary_Lbd {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "LBD".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("LBD".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Sdd

pub struct Binary_Sdd;

impl Binary_Sdd {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Sdd))))
    }
}

impl IHaveMetaInfo for Binary_Sdd {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "SDD".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("SDD".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Trc

pub struct Binary_Trc;

impl Binary_Trc {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Trc))))
    }
}

impl IHaveMetaInfo for Binary_Trc {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "TRC".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("TRC".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Veh

pub struct Binary_Veh;

impl Binary_Veh {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Veh))))
    }
}

impl IHaveMetaInfo for Binary_Veh {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "VEH".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("VEH".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Ame

pub struct Binary_Ame;

impl Binary_Ame {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Ame))))
    }
}

impl IHaveMetaInfo for Binary_Ame {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "AME".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("AME".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Gls

pub struct Binary_Gls;

impl Binary_Gls {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Gls))))
    }
}

impl IHaveMetaInfo for Binary_Gls {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "GLS".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("GLS".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Aip

pub struct Binary_Aip;

impl Binary_Aip {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Aip))))
    }
}

impl IHaveMetaInfo for Binary_Aip {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "AIP".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("AIP".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Vtr

pub struct Binary_Vtr;

impl Binary_Vtr {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Vtr))))
    }
}

impl IHaveMetaInfo for Binary_Vtr {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "VTR".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("VTR".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Att

pub struct Binary_Att;

impl Binary_Att {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Att))))
    }
}

impl IHaveMetaInfo for Binary_Att {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "ATT".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("ATT".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion

// #region Binary_Sfx

pub struct Binary_Sfx;

impl Binary_Sfx {
    pub fn factory<'a>(_r: &'a mut Stream, _f: Arc<FileSource>, _s: &'a dyn Archive) -> Task<'a, Tag> {
        fromResult(Ok(Tag::Info(Arc::new(Binary_Sfx))))
    }
}

impl IHaveMetaInfo for Binary_Sfx {
    fn getInfoNodes(&self, _resource: Option<&dyn MetaManager>, file: Option<&Arc<FileSource>>, _tag: &Tag) -> Vec<MetaInfo> { vec![
        MetaInfo::new(None, Tag::MetaContent(Arc::new(MetaContent {
            type_: "SFX".into(),
            name: file.map_or(String::new(), |s| fileName(&s.path)),
            value: Tag::Str("SFX".into()),
            ..Default::default()
        })), vec![], false),
    ] }
}

// #endregion
