// PORT-SOURCE: Core/GameX/Family.cs
// PORT-SHA: efdc9e92dc086d9d
// PORT-STATUS: done
//
// Families are shared as `Arc`s and live for the process, as C#'s static
// `Families` dictionary keeps them. Back references (`FamilyGame.Family`,
// `FamilyApp.Family`, `FamilyEngine.Family`, `Detector.Game`) are set once the
// owner's `Arc` exists, so they sit in a `OnceLock`; read them with `family()` /
// `game()`. A game merged in from a spec keeps the spec's family, as in C#.
//
// C# `Uri` fields are strings; `ParseResource` reads them with a small `Uri`
// parser below. `Type` fields are type names (strings).
//
// `Type.GetType` + `Activator.CreateInstance`: Rust has no reflection, so
// family crates register factories by the C# type name with `registerType`, and
// GameX looks them up with `findType` (Python's `findType`). Archives
// (`archiveType`), file systems (`vfxType`), clients (`clientType`) and explorer
// apps are created this way. `familyType`, `gameType`, `engineType`, `appType`
// and `detectorType` are recorded but build the base type: no family crate
// derives from them in Rust yet.
//
// Not in openx's Rust port yet, so done differently here: `NetworkFileSystem`
// and `NetworkHost` (an http resource fails with `Unsupported`), and the store
// maps of `Store.cs` (only `Direct:` keys resolve).

use std::any::Any;
use std::collections::{HashMap, HashSet};
use std::fmt;
use std::io::{self, Error, ErrorKind, Read, Seek, SeekFrom};
use std::sync::atomic::{AtomicBool, Ordering};
use std::sync::{Arc, LazyLock, Mutex, OnceLock, RwLock, RwLockReadGuard, Weak};
use std::time::{Duration, SystemTime, UNIX_EPOCH};

use indexmap::IndexMap;
use md5::{Digest, Md5};
use openx::util::{decodePath, YamlDict};
use openx_vfx::vfx::{AggregateFileSystem, DirectoryFileSystem, FileSystem, VfsRead, VirtualFileSystem};
use serde_json::Value;

use crate::binary::{Archive, ArchiveExt, BinaryState, ManyArchive, MultiArchive, Vfx};
use crate::meta::{fileName, MetaManager, Tag};
use crate::store::{self, StoreKind};
use crate::util::{_dictTrim, _list, _listF, _related, _relatedA, _value, _valueF, _valueV};

fn argumentOutOfRange(name: &str, msg: impl fmt::Display) -> Error { Error::new(ErrorKind::InvalidInput, format!("{name}: {msg}")) }

// #region Factory

/// C# `FamilyManager.SearchBy`.
#[derive(Debug, Clone, Copy, PartialEq, Eq, Default)]
pub enum SearchBy {
    #[default]
    Default,
    Arc,
    TopDir,
    TwoDir,
    DirDown,
    AllDir,
}

impl SearchBy {
    /// C# `Enum.TryParse<SearchBy>(s, ignoreCase: true, ...)`.
    pub fn parse(s: &str) -> Option<Self> {
        [Self::Default, Self::Arc, Self::TopDir, Self::TwoDir, Self::DirDown, Self::AllDir].into_iter().find(|z| format!("{z:?}").eq_ignore_ascii_case(s))
    }
}

/// C# `FamilyManager.SystemPath`.
#[derive(Debug, Clone, Default, PartialEq, Eq)]
pub struct SystemPath {
    pub root: String,
    pub type_: Option<String>,
    pub paths: Option<Vec<String>>,
}

static TYPES: LazyLock<RwLock<HashMap<String, Arc<dyn Any + Send + Sync>>>> = LazyLock::new(Default::default);

/// Registers what C# `Type.GetType(name)` + `Activator.CreateInstance` would build, under the C# type name.
pub fn registerType<T: Any + Send + Sync>(name: &str, value: T) { TYPES.write().unwrap().insert(name.to_string(), Arc::new(value)); }

/// C# `Type.GetType(name)`, for a type registered with `registerType` as a `T`.
pub fn findType<T: Any + Clone>(name: &str) -> Option<T> { TYPES.read().unwrap().get(name).and_then(|s| s.downcast_ref::<T>()).cloned() }

/// An `archiveType`: C# `Archive(Archive parent, BinaryState state)`.
pub type ArchiveFactory = fn(Option<Arc<dyn Archive>>, BinaryState) -> io::Result<Arc<dyn Archive>>;
/// A `vfxType`: C# `FileSystem(SystemPath path)`, before `.Next()`.
pub type FileSystemFactory = fn(&SystemPath) -> io::Result<Vfx>;
/// A `clientType`: C# `ClientBase(ClientState state)`.
pub type ClientFactory = fn(ClientState) -> io::Result<Box<dyn Any + Send>>;
/// An `explorerAppType`: C# `Application_Startup` on a new instance.
pub type AppStartup = fn(&FamilyApp, Option<&dyn MetaManager>) -> io::Result<()>;

/// Parse Key.
pub fn parseKey(elem: &Value) -> io::Result<Tag> {
    let str = match elem { Value::String(z) => z.clone(), Value::Null => String::new(), z => z.to_string() };
    let hex = |s: &str, i: usize| s.get(i..i + 2).and_then(|z| u8::from_str_radix(z, 16).ok()).ok_or_else(|| Error::new(ErrorKind::InvalidData, format!("bad hex key: {str}")));
    if str.is_empty() { Ok(Tag::None) }
    else if let Some(z) = str.strip_prefix("b64:") { Ok(Tag::Bytes(fromBase64String(z)?)) }
    else if let Some(z) = str.strip_prefix("hex:") {
        Ok(Tag::Bytes(if z.starts_with('/') { (0..z.len() >> 2).map(|x| hex(z, (x << 2) + 2)).collect::<io::Result<_>>()? }
            else { (0..z.len() >> 1).map(|x| hex(z, x << 1)).collect::<io::Result<_>>()? }))
    }
    else if let Some(z) = str.strip_prefix("asc:") { Ok(Tag::Bytes(z.chars().map(|c| if (c as u32) < 0x80 { c as u8 } else { b'?' }).collect())) }
    else { Ok(Tag::Str(str)) }
}

/// C# `Convert.FromBase64String`.
fn fromBase64String(s: &str) -> io::Result<Vec<u8>> {
    const T: &[u8] = b"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
    let (mut acc, mut bits, mut out) = (0u32, 0u32, Vec::new());
    for c in s.bytes().filter(|c| !c.is_ascii_whitespace() && *c != b'=') {
        let v = T.iter().position(|&x| x == c).ok_or_else(|| Error::new(ErrorKind::InvalidData, format!("invalid base64: {s}")))?;
        acc = (acc << 6) | v as u32;
        bits += 6;
        if bits >= 8 { bits -= 8; out.push((acc >> bits) as u8); }
    }
    Ok(out)
}

/// Parse Engine.
pub fn parseEngine(elem: &Value) -> (Option<String>, Option<String>) {
    let str = elem.as_str().unwrap_or_default();
    if str.is_empty() { return (None, None); }
    match str.split_once(':') { Some((n, v)) => (Some(n.to_string()), Some(v.to_string())), None => (Some(str.to_string()), None) }
}

/// C# `JsonDocument.Parse(json, CommentHandling = Skip)`.
pub fn parseJson(json: &str) -> io::Result<Value> {
    let json = json.trim_start_matches('\u{feff}');
    let (b, mut out, mut i, mut inStr) = (json.as_bytes(), Vec::with_capacity(json.len()), 0, false);
    while i < b.len() {
        let c = b[i];
        if inStr {
            out.push(c);
            if c == b'\\' && i + 1 < b.len() { out.push(b[i + 1]); i += 1; } else if c == b'"' { inStr = false; }
        }
        else if c == b'/' && b.get(i + 1) == Some(&b'/') { while i < b.len() && b[i] != b'\n' { i += 1; } continue; }
        else if c == b'/' && b.get(i + 1) == Some(&b'*') { i += 2; while i + 1 < b.len() && !(b[i] == b'*' && b[i + 1] == b'/') { i += 1; } i += 2; continue; }
        else { if c == b'"' { inStr = true; } out.push(c); }
        i += 1;
    }
    serde_json::from_slice(&out).map_err(|e| Error::new(ErrorKind::InvalidData, e))
}

/// The family json loader: C# `Func<string, string>`.
pub type Loader<'a> = &'a dyn Fn(&str) -> io::Result<String>;

/// Create family sample.
pub fn createFamilySample(path: &str, loader: Loader) -> io::Result<FamilySample> {
    let json = loader(path)?;
    if json.is_empty() { return Err(Error::new(ErrorKind::InvalidInput, "json")); }
    FamilySample::new(&parseJson(&json)?)
}

/// Create family.
pub fn createFamily(any: &str, loader: Option<Loader>, loadSamples: bool) -> io::Result<Arc<Family>> {
    let json = match loader { Some(loader) => loader(any)?, None => any.to_string() };
    if json.is_empty() { return Err(Error::new(ErrorKind::InvalidInput, "json")); }
    let elem = parseJson(&json)?;
    let mut family = Family::new(&elem)?;
    if let (Some(specSamples), Some(loader), true) = (family.specSamples.clone(), loader, loadSamples) {
        for sample in specSamples { family.mergeSample(createFamilySample(&sample, loader)?)?; }
    }
    if let Some(specs) = family.specs.clone() {
        for spec in specs { family.merge(&*createFamily(&spec, loader, loadSamples)?)?; }
    }
    let family = Arc::new(family);
    for s in family.games.values() { let _ = s.family.set(family.clone()); }
    for s in family.apps.values() { let _ = s.family.set(family.clone()); }
    for s in family.engines.values() { let _ = s.family.set(family.clone()); }
    Ok(family)
}

/// Create family engine.
pub fn createFamilyEngine(familyId: &str, id: &str, elem: &Value) -> io::Result<Arc<FamilyEngine>> {
    Ok(Arc::new(FamilyEngine::new(id, elem)?))
}

/// Create family game. A `*` id becomes the default `dgame` and returns `None`.
pub fn createFamilyGame(familyId: &str, id: &str, elem: &Value, dgame: &mut Arc<FamilyGame>) -> Option<Arc<FamilyGame>> {
    let gameType = _value(elem, "gameType").or_else(|| dgame.gameType.clone());
    let mut game = FamilyGame::new(familyId, id, elem, dgame);
    game.gameType = gameType;
    let game = Arc::new(game);
    for s in game.detectors.values() { let _ = s.game.set(Arc::downgrade(&game)); }
    if id.starts_with('*') { *dgame = game; return None; }
    Some(game)
}

/// Create family app.
pub fn createFamilyApp(familyId: &str, id: &str, elem: &Value) -> io::Result<Arc<FamilyApp>> {
    Ok(Arc::new(FamilyApp::new(id, elem)?))
}

/// Creates the file system.
pub fn createFileSystem(vfxType: Option<&str>, path: &SystemPath, subPath: Option<&str>, host: Option<&Uri>) -> io::Result<Vfx> {
    if host.is_some() { return Err(Error::new(ErrorKind::Unsupported, "NetworkFileSystem")); }
    if let Some(vfx) = vfxType.and_then(findType::<FileSystemFactory>) { return vfx(path); }
    let mut baseRoot = match subPath { Some(s) if !s.is_empty() => pathCombine(&path.root, s), _ => path.root.clone() };
    if baseRoot.ends_with('/') || baseRoot.ends_with('\\') { baseRoot.pop(); }
    Ok(match &path.paths {
        Some(basePaths) if basePaths.len() > 1 => Arc::new(AggregateFileSystem::new(basePaths.iter().map(|s| Box::new(DirectoryFileSystem::new(pathCombine(&baseRoot, s))) as Box<dyn FileSystem>).collect())),
        basePaths => Arc::new(DirectoryFileSystem::new(match basePaths.as_ref().and_then(|s| s.first()) { Some(s) => pathCombine(&baseRoot, s), None => baseRoot })),
    })
}

/// C# `Path.Combine`.
fn pathCombine(a: &str, b: &str) -> String {
    if a.is_empty() || b.starts_with('/') || b.starts_with('\\') { b.to_string() }
    else if a.ends_with('/') || a.ends_with('\\') { format!("{a}{b}") }
    else { format!("{a}/{b}") }
}

/// The parts of C# `System.Uri` GameX reads.
#[derive(Debug, Clone, Default, PartialEq, Eq)]
pub struct Uri {
    pub scheme: String,
    pub host: String,
    pub localPath: String,
    /// With its `#`, or empty.
    pub fragment: String,
    pub isFile: bool,
}

impl Uri {
    pub fn parse(s: &str) -> io::Result<Uri> {
        let (s, fragment) = match s.find('#') { Some(i) => (&s[..i], s[i..].to_string()), None => (s, String::new()) };
        let scheme = s.find(':').map(|i| &s[..i]).filter(|z| z.len() > 1 && z.chars().all(|c| c.is_ascii_alphanumeric() || "+-.".contains(c)));
        let Some(scheme) = scheme else {
            // a local path
            if s.starts_with('/') || s.get(1..2) == Some(":") { return Ok(Uri { scheme: "file".into(), host: String::new(), localPath: s.to_string(), fragment, isFile: true }); }
            return Err(argumentOutOfRange("uri", s));
        };
        let rest = &s[scheme.len() + 1..];
        let (host, path) = match rest.strip_prefix("//") { Some(z) => match z.find('/') { Some(i) => (&z[..i], &z[i..]), None => (z, "/") }, None => ("", rest) };
        let scheme = scheme.to_lowercase();
        let isFile = scheme == "file";
        let localPath = if isFile && path.get(2..3) == Some(":") { path[1..].to_string() } else { path.to_string() };
        Ok(Uri { scheme, host: host.to_string(), localPath, fragment, isFile })
    }
}

// #region Detector

pub struct Detector {
    cache: Mutex<HashMap<String, Tag>>,
    pub hashs: Option<IndexMap<String, IndexMap<String, Tag>>>,
    /// Gets the identifier
    pub id: String,
    /// Gets the Game.
    pub game: OnceLock<Weak<FamilyGame>>,
    /// Gets the Data
    pub data: IndexMap<String, Tag>,
    /// Gets the game name.
    pub name: Option<String>,
}

impl Detector {
    pub fn new(game: &FamilyGame, id: &str, elem: &Value) -> io::Result<Self> {
        let mut s = Self { cache: Mutex::new(HashMap::new()), hashs: None, id: id.to_string(), game: OnceLock::new(), data: IndexMap::new(), name: Some(id.to_string()) };
        for (k, v) in elem.as_object().into_iter().flatten() {
            let z = match k.as_str() {
                "name" => { s.name = v.as_str().map(str::to_string); _valueV(v) }
                "type" => _valueV(v),
                "key" => parseKey(v)?,
                "hashs" => { let z = _relatedA(elem, "hashs", |k| k["hash"].as_str().unwrap_or_default().to_string(), |v| s.parseHash(game, v)); s.hashs = Some(z); _valueV(v) }
                _ => _valueV(v),
            };
            s.data.insert(k.clone(), z);
        }
        Ok(s)
    }

    pub fn game(&self) -> Option<Arc<FamilyGame>> { self.game.get().and_then(Weak::upgrade) }

    pub fn parseHash(&self, game: &FamilyGame, elem: &Value) -> IndexMap<String, Tag> {
        elem.as_object().into_iter().flatten().map(|(k, v)| (k.clone(), match k.as_str() {
            "edition" => match v.as_str().and_then(|z| game.editions.get(z)) { Some(a) => Tag::Any(a.clone()), None => _valueV(v) },
            "locale" => match v.as_str().and_then(|z| game.locales.get(z)) { Some(a) => Tag::Any(a.clone()), None => _valueV(v) },
            _ => _valueV(v),
        })).collect()
    }

    /// C# formats the crc with `{crc:s}`, which `uint` doesn't support; here it is decimal.
    pub fn getHash(&self, r: &mut dyn VfsRead) -> io::Result<String> {
        let type_ = self.data.get("type").and_then(Tag::asStr).unwrap_or("md5");
        match type_ {
            "crc" => {
                // create table
                let seed = 0xEDB88320u32;
                let mut table = [0u32; 256];
                for i in 0..256u32 {
                    let mut n = i;
                    for _ in 0..8 { n = if n & 1 != 0 { (n >> 1) ^ seed } else { n >> 1 }; }
                    table[i as usize] = n;
                }
                // generate crc
                let mut crc = 0xFFFFFFFFu32;
                let mut data = Vec::new();
                r.read_to_end(&mut data)?;
                for b in data { crc = (crc >> 8) ^ table[((crc ^ b as u32) & 0xFF) as usize]; }
                crc ^= 0xFFFFFFFF;
                Ok(format!("{crc}"))
            }
            "md5" => {
                let mut data = Vec::new();
                r.take(1024 * 1024).read_to_end(&mut data)?;
                Ok(Md5::digest(&data).iter().map(|b| format!("{b:02x}")).collect())
            }
            _ => Err(argumentOutOfRange("Type", format!("Unknown Type {type_}"))),
        }
    }

    pub fn get(&self, key: &str, value: &mut dyn VfsRead, func: Option<fn(&Detector, Tag) -> Tag>) -> io::Result<Tag> {
        if let Some(z) = self.cache.lock().unwrap().get(key) { return Ok(z.clone()); }
        let s = self.detect(key, value)?;
        let s = match func { Some(func) if !s.isNone() => func(self, s), _ => s };
        Ok(self.cache.lock().unwrap().entry(key.to_string()).or_insert(s).clone())
    }

    pub fn detect(&self, key: &str, r: &mut dyn VfsRead) -> io::Result<Tag> {
        let Some(hashs) = &self.hashs else { return Err(Error::new(ErrorKind::InvalidData, "Hashs")) };
        r.seek(SeekFrom::Start(0))?;
        let hash = self.getHash(r)?;
        r.seek(SeekFrom::Start(0))?;
        Ok(hashs.get(&hash).map(|z| Tag::Map(z.clone())).unwrap_or_default())
    }
}

// #region Resource

#[derive(Clone, Default)]
pub struct Resource {
    /// The filesystem.
    pub vfx: Option<Vfx>,
    /// The game.
    pub game: Option<Arc<FamilyGame>>,
    /// The game edition.
    pub edition: Option<Arc<FamilyGameEdition>>,
    /// The search pattern.
    pub searchPattern: Option<String>,
}

// #region Family

#[derive(Default)]
pub struct Family {
    /// Gets or sets the family identifier.
    pub id: String,
    /// Gets or sets the family name.
    pub name: Option<String>,
    /// Gets or sets the family studio.
    pub studio: Option<String>,
    /// Gets or sets the family description.
    pub description: Option<String>,
    /// Gets or sets the game urls.
    pub urls: Option<Vec<String>>,
    /// Gets the families spec samples.
    pub specSamples: Option<Vec<String>>,
    /// Gets the families specs.
    pub specs: Option<Vec<String>>,
    /// Gets the family apps.
    pub apps: IndexMap<String, Arc<FamilyApp>>,
    /// Gets the family engines.
    pub engines: IndexMap<String, Arc<FamilyEngine>>,
    /// Gets the family games.
    pub games: IndexMap<String, Arc<FamilyGame>>,
    /// Gets the family samples.
    pub samples: IndexMap<String, Vec<Arc<FamilySampleFile>>>,
}

impl Family {
    /// An empty family.
    pub fn empty() -> Arc<Family> {
        static EMPTY: LazyLock<Arc<Family>> = LazyLock::new(|| Arc::new(Family { id: String::new(), name: Some("Empty".into()), ..Default::default() }));
        EMPTY.clone()
    }

    pub fn new(elem: &Value) -> io::Result<Family> {
        let id = _value(elem, "id").ok_or_else(|| Error::new(ErrorKind::InvalidInput, "id"))?;
        let s = (|| -> io::Result<Family> {
            // related
            let mut dgame = Arc::new(FamilyGame { searchBy: SearchBy::Default, arcs: Some(vec!["game:/".into()]), ..Default::default() });
            Ok(Family {
                name: _value(elem, "name"),
                studio: _value(elem, "studio"),
                description: _value(elem, "description"),
                urls: _list(elem, "url"),
                specSamples: _list(elem, "samples"),
                specs: _list(elem, "specs"),
                apps: _related(elem, "apps", |k, v| createFamilyApp(&id, k, v)).into_iter().map(|(k, v)| v.map(|v| (k, v))).collect::<io::Result<_>>()?,
                engines: _related(elem, "engines", |k, v| createFamilyEngine(&id, k, v)).into_iter().map(|(k, v)| v.map(|v| (k, v))).collect::<io::Result<_>>()?,
                games: _dictTrim(_related(elem, "games", |k, v| createFamilyGame(&id, k, v, &mut dgame))),
                samples: IndexMap::new(),
                id: id.clone(),
            })
        })();
        s.inspect_err(|e| eprintln!("ERROR: {id}: {e}"))
    }

    /// Touches this instance.
    pub fn touch() {}

    /// Merges the family. C# `Dictionary.Add` throws on a duplicate key.
    pub fn merge(&mut self, source: &Family) -> io::Result<()> {
        fn add<T: Clone>(z: &mut IndexMap<String, T>, k: &str, v: &T) -> io::Result<()> {
            if z.contains_key(k) { return Err(Error::new(ErrorKind::AlreadyExists, format!("An item with the same key has already been added. Key: {k}"))); }
            z.insert(k.to_string(), v.clone());
            Ok(())
        }
        for (k, v) in &source.engines { add(&mut self.engines, k, v)?; }
        for (k, v) in &source.games { add(&mut self.games, k, v)?; }
        for (k, v) in &source.apps { add(&mut self.apps, k, v)?; }
        Ok(())
    }

    /// Merges the family sample.
    pub fn mergeSample(&mut self, source: FamilySample) -> io::Result<()> {
        for (k, v) in source.samples {
            if self.samples.contains_key(&k) { return Err(Error::new(ErrorKind::AlreadyExists, format!("An item with the same key has already been added. Key: {k}"))); }
            self.samples.insert(k, v);
        }
        Ok(())
    }

    /// Gets the specified family game.
    pub fn getGame(&self, id: &str, throwOnError: bool) -> io::Result<(Option<Arc<FamilyGame>>, Option<Arc<FamilyGameEdition>>)> {
        let (gid, eid) = id.split_once('.').unwrap_or((id, ""));
        let Some(game) = self.games.get(gid).cloned() else {
            return if throwOnError { Err(argumentOutOfRange("id", id)) } else { Ok((None, None)) };
        };
        let edition = game.editions.get(eid).cloned();
        Ok((Some(game), edition))
    }

    /// Generates the specified family game string.
    pub fn toGame(&self, game: &FamilyGame, edition: Option<&FamilyGameEdition>) -> String {
        match edition { Some(edition) => format!("{}.{}", game.id, edition.id), None => game.id.clone() }
    }

    /// Parses the family resource uri.
    pub fn parseResource(&self, uri: Option<&Uri>, throwOnError: bool) -> io::Result<Resource> {
        let Some(uri) = uri.filter(|s| !s.fragment.is_empty()) else { return Ok(Resource { game: Some(Arc::new(FamilyGame::default())), ..Default::default() }) };
        let (Some(game), edition) = self.getGame(&uri.fragment[1..], true)? else { unreachable!() };
        let searchPattern = if uri.isFile { None } else { Some(uri.localPath.get(1..).unwrap_or_default().to_string()) };
        let virtuals = &game.virtuals;
        let found = &game.found;
        let subPath = edition.as_ref().and_then(|s| s.path.as_deref());
        let vfxType = game.vfxType.as_deref();
        let vfx = if uri.scheme.eq_ignore_ascii_case("game") { match found { Some(found) => Some(createFileSystem(vfxType, found, subPath, None)?), None => None } }
            else if uri.isFile { if !uri.localPath.is_empty() { Some(createFileSystem(vfxType, &SystemPath { root: uri.localPath.clone(), ..Default::default() }, subPath, None)?) } else { None } }
            else if uri.scheme.to_lowercase().starts_with("http") { if !uri.host.is_empty() { Some(createFileSystem(vfxType, &found.clone().unwrap_or_default(), subPath, Some(uri))?) } else { None } }
            else { None };
        let Some(mut vfx) = vfx else {
            return if throwOnError { Err(argumentOutOfRange("uri", format!("{}: unable to find archive", game.id))) } else { Ok(Resource::default()) };
        };
        {
            let virtuals = virtuals.iter().map(|(k, v)| (k.clone(), v.clone().unwrap_or_default())).collect();
            vfx = Arc::new(AggregateFileSystem::new(vec![Box::new(ArcFileSystem(vfx)), Box::new(VirtualFileSystem::new(virtuals))]));
        }
        Ok(Resource { vfx: Some(vfx), game: Some(game), edition, searchPattern })
    }

    /// Generates the family resource uri.
    pub fn toResource(&self, res: &Resource) -> Option<Uri> { None }

    /// Opens the family archive from a uri: C# `GetArchive(string or Uri)`.
    pub fn getArchive(&self, res: &str, throwOnError: bool) -> io::Result<Option<Arc<dyn Archive>>> {
        let r = self.parseResource(Some(&Uri::parse(res)?), throwOnError)?;
        self.getArchiveFor(r, throwOnError)
    }

    /// Opens the family archive: C# `GetArchive(Resource)`.
    pub fn getArchiveFor(&self, r: Resource, throwOnError: bool) -> io::Result<Option<Arc<dyn Archive>>> {
        let game = r.game.ok_or_else(|| Error::new(ErrorKind::InvalidInput, "Game"))?;
        if !game.hasLoaded.swap(true, Ordering::Relaxed) { game.loaded(); }
        let Some(vfx) = r.vfx else { return Err(Error::new(ErrorKind::InvalidInput, "Vfx")) };
        let Some(arc) = game.getArchive(vfx, r.edition, r.searchPattern.as_deref(), throwOnError)? else { return Ok(None) };
        arc.open(None, None)?;
        Ok(Some(arc))
    }
}

/// Lets an `Arc`'d file system sit in an `AggregateFileSystem`.
struct ArcFileSystem(Vfx);
impl FileSystem for ArcFileSystem {
    fn glob(&self, path: &str, searchPattern: &str) -> Vec<String> { self.0.glob(path, searchPattern) }
    fn fileExists(&self, path: &str) -> bool { self.0.fileExists(path) }
    fn fileInfo(&self, path: &str) -> Option<openx_vfx::vfx::FileInfo> { self.0.fileInfo(path) }
    fn open(&self, path: &str) -> Option<Box<dyn VfsRead + '_>> { self.0.open(path) }
}

impl fmt::Display for Family {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result { write!(f, "{}", self.name.as_deref().unwrap_or_default()) }
}

// #region FamilyApp

pub struct FamilyApp {
    /// Gets the family.
    pub family: OnceLock<Arc<Family>>,
    /// Gets the identifier.
    pub id: String,
    /// Gets the data.
    pub data: IndexMap<String, Tag>,
    /// Gets the name.
    pub name: Option<String>,
    /// Gets the explorer type.
    pub explorerType: Option<String>,
    /// Gets the explorer2 type.
    pub explorer2Type: Option<String>,
}

impl FamilyApp {
    pub fn new(id: &str, elem: &Value) -> io::Result<Self> {
        let mut s = Self { family: OnceLock::new(), id: id.to_string(), data: IndexMap::new(), name: Some(id.to_string()), explorerType: None, explorer2Type: None };
        for (k, v) in elem.as_object().into_iter().flatten() {
            let z = match k.as_str() {
                "name" => { s.name = v.as_str().map(str::to_string); _valueV(v) }
                "explorerAppType" => { s.explorerType = v.as_str().map(str::to_string); _valueV(v) }
                "explorerApp2Type" => { s.explorer2Type = v.as_str().map(str::to_string); _valueV(v) }
                "key" => parseKey(v)?,
                _ => _valueV(v),
            };
            s.data.insert(k.clone(), z);
        }
        Ok(s)
    }

    pub fn family(&self) -> Arc<Family> { self.family.get().cloned().unwrap_or_else(Family::empty) }

    /// Starts the app's explorer.
    pub fn openAsync(&self, explorerType: &str, manager: Option<&dyn MetaManager>) -> io::Result<()> {
        let startup = findType::<AppStartup>(explorerType).ok_or_else(|| argumentOutOfRange("explorerType", "No Application_Startup found"))?;
        startup(self, manager)
    }
}

impl fmt::Display for FamilyApp {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result { write!(f, "{}", self.name.as_deref().unwrap_or_default()) }
}

// #region FamilyEngine

pub struct FamilyEngine {
    /// Gets the family.
    pub family: OnceLock<Arc<Family>>,
    /// Gets the identifier.
    pub id: String,
    /// Gets the data.
    pub data: IndexMap<String, Tag>,
    /// Gets the name.
    pub name: Option<String>,
}

impl FamilyEngine {
    pub fn new(id: &str, elem: &Value) -> io::Result<Self> {
        let mut s = Self { family: OnceLock::new(), id: id.to_string(), data: IndexMap::new(), name: Some(id.to_string()) };
        for (k, v) in elem.as_object().into_iter().flatten() {
            let z = match k.as_str() {
                "name" => { s.name = v.as_str().map(str::to_string); _valueV(v) }
                "key" => parseKey(v)?,
                _ => _valueV(v),
            };
            s.data.insert(k.clone(), z);
        }
        Ok(s)
    }

    pub fn family(&self) -> Arc<Family> { self.family.get().cloned().unwrap_or_else(Family::empty) }
}

impl fmt::Display for FamilyEngine {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result { write!(f, "{}", self.name.as_deref().unwrap_or_default()) }
}

// #region FamilySample

#[derive(Default)]
pub struct FamilySample {
    pub samples: IndexMap<String, Vec<Arc<FamilySampleFile>>>,
}

impl FamilySample {
    pub fn new(elem: &Value) -> io::Result<Self> {
        let mut samples = IndexMap::new();
        for (k, v) in elem.as_object().into_iter().flatten() {
            let files = v.get("files").and_then(Value::as_array).ok_or_else(|| Error::new(ErrorKind::InvalidData, "files"))?;
            samples.insert(k.clone(), files.iter().map(|x| Arc::new(FamilySampleFile::new(x))).collect());
        }
        Ok(Self { samples })
    }
}

/// C# `FamilySample.File`: the sample file.
#[derive(Debug, Clone, Default)]
pub struct FamilySampleFile {
    /// The path
    pub paths: Option<Vec<String>>,
    /// The Data
    pub data: IndexMap<String, Tag>,
}

impl FamilySampleFile {
    pub fn new(elem: &Value) -> Self {
        let mut s = Self::default();
        for (k, v) in elem.as_object().into_iter().flatten() {
            let z = match k.as_str() {
                "path" => { let p = match v { Value::String(z) => vec![z.clone()], _ => v.as_array().into_iter().flatten().map(|s| s.as_str().unwrap_or_default().to_string()).collect() }; s.paths = Some(p.clone()); Tag::Strs(p) }
                "size" => Tag::Int(v.as_i64().unwrap_or_default()),
                _ => _valueV(v),
            };
            s.data.insert(k.clone(), z);
        }
        s
    }
}

// #region FamilyGame

/// ClientState
pub struct ClientState {
    /// Gets the arc family game.
    pub archive: Arc<dyn Archive>,
    /// Gets the args.
    pub args: Vec<String>,
    /// Gets the tag.
    pub tag: Tag,
}

impl ClientState {
    pub fn new(archive: Arc<dyn Archive>, args: Option<Vec<String>>, tag: Tag) -> Self { Self { archive, args: args.unwrap_or_default(), tag } }
}

/// C# `FamilyGame.Edition`: the game edition.
#[derive(Debug, Clone, Default)]
pub struct FamilyGameEdition {
    /// Gets the identifier.
    pub id: String,
    /// Gets the data.
    pub data: IndexMap<String, Tag>,
    /// Gets the name.
    pub name: Option<String>,
    /// Gets the paths.
    pub path: Option<String>,
    /// Gets the ignores.
    pub ignores: Option<Vec<String>>,
}

impl FamilyGameEdition {
    pub fn new(id: &str, elem: &Value) -> io::Result<Self> {
        let mut s = Self { id: id.to_string(), name: Some(id.to_string()), ..Default::default() };
        for (k, v) in elem.as_object().into_iter().flatten() {
            let z = match k.as_str() {
                "name" => { s.name = v.as_str().map(str::to_string); _valueV(v) }
                "path" => { s.path = v.as_str().map(str::to_string); _valueV(v) }
                "ignore" => { s.ignores = _list(elem, "ignore"); _valueV(v) }
                "key" => parseKey(v)?,
                _ => _valueV(v),
            };
            s.data.insert(k.clone(), z);
        }
        Ok(s)
    }
}

/// C# `FamilyGame.DownloadableContent`: the game DLC.
#[derive(Debug, Clone, Default)]
pub struct FamilyGameDownloadableContent {
    /// Gets the identifier.
    pub id: String,
    /// Gets the data.
    pub data: IndexMap<String, Tag>,
    /// Gets the name.
    pub name: Option<String>,
    /// Gets the path.
    pub path: Option<String>,
}

impl FamilyGameDownloadableContent {
    pub fn new(id: &str, elem: &Value) -> io::Result<Self> {
        let mut s = Self { id: id.to_string(), name: Some(id.to_string()), ..Default::default() };
        for (k, v) in elem.as_object().into_iter().flatten() {
            let z = match k.as_str() {
                "name" => { s.name = v.as_str().map(str::to_string); _valueV(v) }
                "path" => { s.path = v.as_str().map(str::to_string); _valueV(v) }
                "key" => parseKey(v)?,
                _ => _valueV(v),
            };
            s.data.insert(k.clone(), z);
        }
        Ok(s)
    }
}

/// C# `FamilyGame.Locale`: the game locale.
#[derive(Debug, Clone, Default)]
pub struct FamilyGameLocale {
    /// Gets the identifier.
    pub id: String,
    /// Gets the data.
    pub data: IndexMap<String, Tag>,
    /// Gets the name.
    pub name: Option<String>,
}

impl FamilyGameLocale {
    pub fn new(id: &str, elem: &Value) -> io::Result<Self> {
        let mut s = Self { id: id.to_string(), name: Some(id.to_string()), ..Default::default() };
        for (k, v) in elem.as_object().into_iter().flatten() {
            let z = match k.as_str() {
                "name" => { s.name = v.as_str().map(str::to_string); _valueV(v) }
                "key" => parseKey(v)?,
                _ => _valueV(v),
            };
            s.data.insert(k.clone(), z);
        }
        Ok(s)
    }
}

/// C# `FamilyGame.FileSet`: the game files.
#[derive(Debug, Clone, Default)]
pub struct FamilyGameFileSet {
    pub elem: Value,
    pub keys: Option<Vec<String>>,
    pub paths: Vec<String>,
}

impl FamilyGameFileSet {
    pub fn new(elem: &Value) -> Self { Self { elem: elem.clone(), keys: _list(elem, "key"), paths: _list(elem, "path").unwrap_or_default() } }
}

#[derive(Default)]
pub struct FamilyGame {
    /// Has loaded
    pub hasLoaded: AtomicBool,
    /// Gets or sets the game type.
    pub gameType: Option<String>,
    /// Gets or sets the family.
    pub family: OnceLock<Arc<Family>>,
    /// Gets or sets the game identifier.
    pub id: String,
    /// Gets or sets the game name.
    pub name: Option<String>,
    /// Gets or sets the game engine.
    pub engine: (Option<String>, Option<String>),
    /// Gets or sets the game resource.
    pub resource: Option<String>,
    /// Gets or sets the game urls.
    pub urls: Option<Vec<String>>,
    /// Gets or sets the game date: C# `DateTime`, `None` is `default(DateTime)`.
    pub date: Option<SystemTime>,
    /// Gets or sets the search by.
    pub searchBy: SearchBy,
    /// Gets or sets the archive type.
    pub archiveType: Option<String>,
    /// Gets or sets the arc exts.
    pub arcExts: Option<Vec<String>>,
    /// Gets or sets the paks.
    pub arcs: Option<Vec<String>>,
    /// Gets or sets the Paths.
    pub paths: Option<Vec<String>>,
    /// Gets or sets the key.
    pub key: Tag,
    /// Gets or sets the Status.
    pub status: Option<String>,
    /// Gets or sets the Tags.
    pub tags: Vec<String>,
    /// Gets or sets the type of the file system.
    pub vfxType: Option<String>,
    /// Gets or sets the type of the client.
    pub clientType: Option<String>,
    /// Gets or sets the game editions.
    pub editions: IndexMap<String, Arc<FamilyGameEdition>>,
    /// Gets or sets the game dlcs.
    pub dlcs: IndexMap<String, Arc<FamilyGameDownloadableContent>>,
    /// Gets or sets the game locales.
    pub locales: IndexMap<String, Arc<FamilyGameLocale>>,
    /// Gets or sets the detectorss.
    pub detectors: IndexMap<String, Detector>,
    /// Determines if the game has been found.
    pub found: Option<SystemPath>,
    /// Gets or sets the files.
    pub files: Option<FamilyGameFileSet>,
    /// Gets or sets the ignores.
    pub ignores: HashSet<String>,
    /// Gets or sets the virtuals.
    pub virtuals: IndexMap<String, Option<Vec<u8>>>,
    /// Gets or sets the filters.
    pub filters: IndexMap<String, String>,
    // Options
    pub options: Mutex<Option<YamlDict>>,
}

impl FamilyGame {
    /// An empty family game.
    pub fn empty() -> Arc<FamilyGame> {
        static EMPTY: LazyLock<Arc<FamilyGame>> = LazyLock::new(|| {
            let s = FamilyGame { id: "Empty".into(), name: Some("Empty".into()), ..Default::default() };
            let _ = s.family.set(Family::empty());
            Arc::new(s)
        });
        EMPTY.clone()
    }

    /// Create Detector.
    fn createDetector(&self, id: &str, elem: &Value) -> io::Result<Detector> { Detector::new(self, id, elem) }

    /// Create archive
    pub fn createArchive(&self, parent: Option<Arc<dyn Archive>>, state: BinaryState) -> io::Result<Arc<dyn Archive>> {
        let archiveType = self.archiveType.as_deref().ok_or_else(|| Error::new(ErrorKind::InvalidData, format!("{} missing ArchiveType", self.id)))?;
        let factory = findType::<ArchiveFactory>(archiveType).ok_or_else(|| Error::new(ErrorKind::InvalidData, format!("{}: unknown ArchiveType {archiveType}", self.id)))?;
        factory(parent, state)
    }

    /// Create client
    fn createClient(&self, state: ClientState) -> io::Result<Box<dyn Any + Send>> {
        let clientType = self.clientType.as_deref().ok_or_else(|| Error::new(ErrorKind::InvalidData, format!("{} missing ArchiveType", self.id)))?;
        let factory = findType::<ClientFactory>(clientType).ok_or_else(|| Error::new(ErrorKind::InvalidData, format!("{}: unknown ClientType {clientType}", self.id)))?;
        factory(state)
    }

    /// FamilyGame. Like C#, an error part way through is logged and leaves the rest unset.
    pub fn new(familyId: &str, id: &str, elem: &Value, dgame: &FamilyGame) -> FamilyGame {
        let mut s = FamilyGame { id: id.to_string(), ..Default::default() };
        if let Err(e) = s.init(familyId, elem, dgame) { eprintln!("ERROR: {familyId}.{id}: {e}"); }
        s
    }

    fn init(&mut self, familyId: &str, elem: &Value, dgame: &FamilyGame) -> io::Result<()> {
        let typeName = |key: &str, default_: &Option<String>| _value(elem, key).or_else(|| default_.clone());
        self.hasLoaded = AtomicBool::new(false);
        self.name = _value(elem, "name");
        self.engine = _valueF(elem, "engine", parseEngine, dgame.engine.clone());
        self.resource = _value(elem, "resource").or_else(|| dgame.resource.clone());
        self.urls = _list(elem, "url");
        self.date = _valueF(elem, "date", |z| parseDate(z.as_str().unwrap_or_default()).map(Some), Ok(None))?;
        self.arcs = _list(elem, "arc").or_else(|| dgame.arcs.clone());
        self.paths = _list(elem, "path").or_else(|| dgame.paths.clone());
        self.key = _valueF(elem, "key", parseKey, Ok(dgame.key.clone()))?;
        self.status = _value(elem, "status");
        self.tags = _value(elem, "tags").unwrap_or_default().split(' ').map(str::to_string).collect();
        // interface
        self.clientType = typeName("clientType", &dgame.clientType);
        self.vfxType = typeName("vfxType", &dgame.vfxType);
        self.searchBy = _valueF(elem, "searchBy", |z| { let z = z.as_str().unwrap_or_default(); SearchBy::parse(z).ok_or_else(|| argumentOutOfRange("searchBy", format!("Unknown option: {z}"))) }, Ok(dgame.searchBy))?;
        self.archiveType = typeName("archiveType", &dgame.archiveType);
        self.arcExts = _list(elem, "arcExt").or_else(|| dgame.arcExts.clone());
        // related
        self.editions = _related(elem, "editions", |k, v| FamilyGameEdition::new(k, v).map(Arc::new)).into_iter().map(|(k, v)| v.map(|v| (k, v))).collect::<io::Result<_>>()?;
        self.dlcs = _related(elem, "dlcs", |k, v| FamilyGameDownloadableContent::new(k, v).map(Arc::new)).into_iter().map(|(k, v)| v.map(|v| (k, v))).collect::<io::Result<_>>()?;
        self.locales = _related(elem, "locals", |k, v| FamilyGameLocale::new(k, v).map(Arc::new)).into_iter().map(|(k, v)| v.map(|v| (k, v))).collect::<io::Result<_>>()?;
        self.detectors = _related(elem, "detectors", |k, v| self.createDetector(k, v)).into_iter().map(|(k, v)| v.map(|v| (k, v))).collect::<io::Result<_>>()?;
        // files
        self.files = elem.get("files").map(FamilyGameFileSet::new);
        self.ignores = _list(elem, "ignores").unwrap_or_default().into_iter().collect();
        self.virtuals = _related(elem, "virtuals", |k, v| parseKey(v).map(|z| match z { Tag::Bytes(z) => Some(z), _ => None })).into_iter().map(|(k, v)| v.map(|v| (k, v))).collect::<io::Result<_>>()?;
        self.filters = _related(elem, "filters", |k, v| match _valueV(v) { Tag::Str(z) => z, z => format!("{z:?}") });
        // find
        self.found = self.getSystemPath(crate::config::option().findKey, familyId, elem)?;
        *self.options.get_mut().unwrap() = None;
        Ok(())
    }

    pub fn family(&self) -> Arc<Family> { self.family.get().cloned().unwrap_or_else(Family::empty) }

    /// Gets the displayed game name.
    pub fn displayedName(&self) -> String { format!("{}{}", self.name.as_deref().unwrap_or_default(), if self.found.is_some() { " - found" } else { "" }) }

    /// Detect
    pub fn detect(&self, id: &str, key: &str, value: &mut dyn VfsRead, func: Option<fn(&Detector, Tag) -> Tag>) -> io::Result<Tag> {
        match self.detectors.get(id) { Some(z) => z.get(key, value, func), None => Ok(Tag::None) }
    }

    /// Ensures this instance.
    pub fn loaded(&self) { *self.options.lock().unwrap() = Some(YamlDict::new(format!("~/.gamex.{}_{}.yaml", self.family().id, self.id))); }

    /// Converts the game to uris.
    pub fn toUris(&self, edition: Option<&str>) -> Vec<String> {
        self.arcs.iter().flatten().map(|s| Self::toUri(&self.id, edition, Some(s))).collect()
    }

    /// Converts the game to a uri
    pub fn toUri(id: &str, edition: Option<&str>, prefix: Option<&str>) -> String { format!("{}#{}", prefix.unwrap_or("game:/"), Self::toId(id, edition)) }

    /// Converts the game to an id
    pub fn toId(id: &str, edition: Option<&str>) -> String {
        match edition { Some(edition) if !edition.is_empty() => format!("{id}.{edition}"), _ => id.to_string() }
    }

    /// Gets a client
    pub fn getClient(&self, state: ClientState) -> io::Result<Box<dyn Any + Send>> { self.createClient(state) }

    /// Gets a game sample
    pub fn getSample(&self, id: &str) -> io::Result<Option<Arc<FamilySampleFile>>> {
        let family = self.family();
        let Some(samples) = family.samples.get(&self.id).filter(|s| !s.is_empty()) else { return Ok(None) };
        let idx = if id == "*" { SystemTime::now().duration_since(UNIX_EPOCH).unwrap_or_default().subsec_nanos() as usize % samples.len() }
            else { id.parse::<usize>().map_err(|e| Error::new(ErrorKind::InvalidInput, e))? };
        Ok(samples.get(idx).cloned())
    }

    /// Gets a game system path
    fn getSystemPath(&self, startsWith: Option<&str>, family: &str, elem: &Value) -> io::Result<Option<SystemPath>> {
        let Some(files) = &self.files else { return Ok(None) };
        let Some(keys) = &files.keys else { return Ok(None) };
        for key in keys.iter().filter(|k| startsWith.map_or(true, |s| s.starts_with(k.as_str()))) {
            let (k, v) = match key.split_once('#') { Some((k, v)) => (k, Some(v.to_string())), None => (key.as_str(), None) };
            let Some(path) = getPathByKey(k, family)? else { continue };
            if path.is_empty() { continue; }
            let mut path = getFullPath(&decodePath(&applicationPath(), &path, None));
            if !std::path::Path::new(&path).exists() { continue; }
            if let Some(subPath) = _valueF(&files.elem, k, |s| _list(s, "path"), None) { path = getFullPath(&subPath[0].replace("%path%", &path)); }
            return Ok(Some(SystemPath { root: path, type_: v, paths: Some(files.paths.clone()) }));
        }
        Ok(None)
    }

    // #region Arc

    /// Creates the search patterns.
    fn createSearchPatterns(&self, searchPattern: Option<&str>) -> String {
        if let Some(s) = searchPattern.filter(|s| !s.is_empty()) { return s.to_string(); }
        match self.searchBy {
            SearchBy::Default => String::new(),
            SearchBy::Arc => match self.arcExts.as_deref() {
                None | Some([]) => String::new(),
                Some([z]) => format!("*{z}"),
                Some(z) => format!("(*{})", z.join(":*")),
            },
            SearchBy::TopDir => "*".into(),
            SearchBy::TwoDir => "*/*".into(),
            SearchBy::DirDown | SearchBy::AllDir => "**/*".into(),
        }
    }

    /// Is arc file.
    pub fn isArcPath(&self, path: &str) -> bool {
        self.arcExts.iter().flatten().any(|x| path.len() >= x.len() && path.is_char_boundary(path.len() - x.len()) && path[path.len() - x.len()..].eq_ignore_ascii_case(x))
    }

    /// Create the archive.
    pub fn getArchive(self: &Arc<Self>, vfx: Vfx, edition: Option<Arc<FamilyGameEdition>>, searchPattern: Option<&str>, throwOnError: bool) -> io::Result<Option<Arc<dyn Archive>>> {
        let searchPattern = self.createSearchPatterns(searchPattern);
        let mut archives: Vec<Arc<dyn Archive>> = Vec::new();
        let dlcKeys: Vec<&String> = self.dlcs.iter().filter(|x| x.1.path.as_deref().is_some_and(|s| !s.is_empty())).map(|x| x.0).collect();
        let slash = '\\';
        for key in std::iter::once(None).chain(dlcKeys.into_iter().map(Some)) {
            for (root, paths) in self.findPaths(&vfx, edition.as_deref(), key.map(|k| &*self.dlcs[k]), &searchPattern) {
                match self.searchBy {
                    SearchBy::Arc => for path in paths {
                        if self.isArcPath(&path) { archives.extend(self.getArchiveRecursive(None, &vfx, &edition, ArchiveValue::Path(path), Tag::None)?); }
                    },
                    _ => {
                        let paths = if self.searchBy == SearchBy::DirDown { paths.into_iter().filter(|x| x.contains(slash)).collect() } else { paths };
                        archives.extend(self.getArchiveRecursive(None, &vfx, &edition, ArchiveValue::Paths(root, paths), Tag::None)?);
                    }
                }
            }
        }
        if archives.len() == 1 { Ok(archives.pop()) } else { self.getArchiveRecursive(None, &vfx, &edition, ArchiveValue::Archives(archives), Tag::None) }
    }

    /// Create the archive.
    fn getArchiveRecursive(self: &Arc<Self>, parent: Option<Arc<dyn Archive>>, vfx: &Vfx, edition: &Option<Arc<FamilyGameEdition>>, value: ArchiveValue, tag: Tag) -> io::Result<Option<Arc<dyn Archive>>> {
        let path = if let ArchiveValue::Path(s) = &value { Some(s.as_str()) } else { None };
        let state = BinaryState::new(vfx.clone(), self.clone(), edition.clone(), path, tag.clone());
        match value {
            ArchiveValue::Path(s) => if self.isArcPath(&s) { self.createArchive(parent, state).map(Some) } else { Err(Error::new(ErrorKind::InvalidData, format!("{} missing {s}", self.id))) },
            ArchiveValue::Paths(root, paths) => if paths.len() == 1 && self.isArcPath(&paths[0]) {
                self.getArchiveRecursive(parent, vfx, edition, ArchiveValue::Path(paths[0].clone()), tag)
            } else {
                let basis = self.createArchive(None, state.clone())?;
                let pathSkip = if !root.is_empty() { root.len() + 1 } else { 0 };
                Ok(Some(ManyArchive::new(&*basis, parent, state, if !root.is_empty() { &root } else { "Many" }, paths, pathSkip)))
            },
            ArchiveValue::Archives(mut s) => Ok(Some(if s.len() == 1 { s.pop().unwrap() } else { MultiArchive::new(parent, state, "Multi", s) })),
        }
    }

    /// Find the games paths.
    fn findPaths(&self, vfx: &Vfx, edition: Option<&FamilyGameEdition>, dlc: Option<&FamilyGameDownloadableContent>, searchPattern: &str) -> Vec<(String, Vec<String>)> {
        let ignores = &self.ignores;
        let paths = self.paths.clone().unwrap_or_else(|| vec![String::new()]);
        paths.into_iter().map(|path| {
            let searchPath = match dlc.and_then(|s| s.path.as_deref()) { Some(z) if !path.is_empty() => format!("{path}/{z}"), Some(z) => z.to_string(), None => path.clone() };
            let fileSearch = vfx.findPaths(&searchPath, searchPattern).into_iter().filter(|s| !ignores.contains(&fileName(s))).collect();
            (path, fileSearch)
        }).collect()
    }
}

impl fmt::Display for FamilyGame {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result { write!(f, "{}", self.name.as_deref().unwrap_or_default()) }
}

/// C# `GetArchiveRecursive`'s `object value`.
enum ArchiveValue {
    Path(String),
    Paths(String, Vec<String>),
    Archives(Vec<Arc<dyn Archive>>),
}

/// C# `DateTime.Parse` for the specs' forms: `yyyy-MM-dd`, `MMMM d, yyyy`, `MMM d, yyyy` and `d MMMM yyyy`.
fn parseDate(s: &str) -> io::Result<SystemTime> {
    const MONTHS: [&str; 12] = ["jan", "feb", "mar", "apr", "may", "jun", "jul", "aug", "sep", "oct", "nov", "dec"];
    let bad = || Error::new(ErrorKind::InvalidData, format!("String '{s}' was not recognized as a valid DateTime."));
    let num = |z: &str| z.parse::<i64>().map_err(|_| bad());
    let month = |z: &str| MONTHS.iter().position(|m| z.len() >= 3 && z.to_lowercase().starts_with(m)).map(|i| i as i64 + 1).ok_or_else(bad);
    let p: Vec<&str> = s.split(['-', ' ', ',']).filter(|z| !z.is_empty()).collect();
    let (y, m, d) = match p[..] {
        [y, m, d] if y.len() == 4 && y.bytes().all(|c| c.is_ascii_digit()) => (num(y)?, num(m)?, num(d)?),
        [m, d, y] if m.starts_with(|c: char| c.is_alphabetic()) => (num(y)?, month(m)?, num(d)?),
        [d, m, y] => (num(y)?, month(m)?, num(d)?),
        _ => return Err(bad()),
    };
    // days from civil
    let y = if m <= 2 { y - 1 } else { y };
    let era = y.div_euclid(400);
    let yoe = y - era * 400;
    let doy = (153 * (m + if m > 2 { -3 } else { 9 }) + 2) / 5 + d - 1;
    let doe = yoe * 365 + yoe / 4 - yoe / 100 + doy;
    let days = era * 146097 + doe - 719468;
    Ok(if days >= 0 { UNIX_EPOCH + Duration::from_secs(days as u64 * 86400) } else { UNIX_EPOCH - Duration::from_secs((-days) as u64 * 86400) })
}

/// C# `EngineX.ApplicationPath`: the executable's folder.
fn applicationPath() -> String {
    std::env::current_exe().ok().and_then(|s| s.parent().map(|s| s.to_string_lossy().into_owned())).unwrap_or_default()
}

/// C# `Path.GetFullPath`.
fn getFullPath(path: &str) -> String { std::path::absolute(path).map(|s| s.to_string_lossy().into_owned()).unwrap_or_else(|_| path.to_string()) }

/// C# `Store.GetPathByKey(key, family, elem)`. Only `Direct:` resolves until `Store.cs`'s store maps are ported.
fn getPathByKey(key: &str, family: &str) -> io::Result<Option<String>> {
    let (k, v) = key.split_once(':').unwrap_or((key, ""));
    match StoreKind::parse(k) {
        Some(StoreKind::Direct) => Ok(Some(store::direct(v).to_string_lossy().into_owned())),
        Some(_) => Ok(store::getPathByKey(key, family, &HashMap::new()).map_err(|e| argumentOutOfRange("key", e))?.map(|s| s.to_string_lossy().into_owned())),
        None => Err(argumentOutOfRange("key", key)),
    }
}

// #region Loader

mod specs { include!(concat!(env!("OUT_DIR"), "/specs.rs")); }

fn familyJsonLoader(path: &str) -> io::Result<String> {
    specs::SPECS.iter().find(|s| s.0 == path).map(|s| s.1.to_string()).ok_or_else(|| Error::new(ErrorKind::NotFound, format!("Unable to spec: GameX.Specs.{path}")))
}

/// Load samples.
pub static LoadSamples: AtomicBool = AtomicBool::new(true);

/// The families: C# `Dictionary(StringComparer.OrdinalIgnoreCase)`.
static FAMILIES: LazyLock<RwLock<IndexMap<String, Arc<Family>>>> = LazyLock::new(Default::default);

/// The families.
pub fn families() -> RwLockReadGuard<'static, IndexMap<String, Arc<Family>>> { FAMILIES.read().unwrap() }

/// The Unknown family.
pub fn uncore() -> Option<Arc<Family>> {
    static UNCORE: LazyLock<Option<Arc<Family>>> = LazyLock::new(|| { Family::touch(); getFamily("Uncore", false).ok().flatten() });
    UNCORE.clone()
}

/// The Unknown arc file.
pub fn uncoreArchive() -> Option<Arc<dyn Archive>> {
    static UNCORE_ARCHIVE: LazyLock<Option<Arc<dyn Archive>>> = LazyLock::new(|| uncore()?.getArchive("game:/#APP", false).inspect_err(|e| eprintln!("ERROR: {e}")).ok().flatten());
    UNCORE_ARCHIVE.clone()
}

/// Gets the specified family.
pub fn getFamily(id: &str, throwOnError: bool) -> io::Result<Option<Arc<Family>>> {
    if let Some(family) = FAMILIES.read().unwrap().iter().find(|s| s.0.eq_ignore_ascii_case(id)).map(|s| s.1.clone()) { return Ok(Some(family)); }
    match createFamily(&format!("{id}Family.json"), Some(&familyJsonLoader), LoadSamples.load(Ordering::Relaxed)) {
        Ok(family) => {
            FAMILIES.write().unwrap().insert(family.id.clone(), family.clone());
            return Ok(Some(family));
        }
        Err(e) => eprintln!("ERROR: {e}"),
    }
    if throwOnError { Err(argumentOutOfRange("id", id)) } else { Ok(None) }
}

/// Loads all families
pub fn loadFamilies(ids: Option<Vec<String>>, throwOnError: bool) -> io::Result<()> {
    uncore(); // C#'s static FamilyManager() has loaded Uncore by now
    let ids = ids.unwrap_or_else(|| crate::config::FamilyIds.iter().map(|s| s.to_string()).collect());
    for s in ids { getFamily(&s, throwOnError)?; }
    Ok(())
}

// #region ShellState

#[derive(Debug, Clone, Default, PartialEq)]
pub struct ShellState {
    pub familyId: Option<String>,
    pub arcUri: Option<String>,
    pub path: Option<String>,
    pub type_: Option<String>,
}

impl fmt::Display for ShellState {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        let z = |s: &Option<String>| s.clone().unwrap_or_default();
        write!(f, "ST|{}|{}|{}|{}", z(&self.familyId), z(&self.arcUri), z(&self.path), z(&self.type_))
    }
}

impl ShellState {
    pub fn create(source: &Tag, path: &crate::meta::MetaItem, value: &Tag, type_: &str) -> ShellState {
        let a = path.archive.as_ref().map(|s| s.base());
        ShellState {
            familyId: a.map(|a| a.family.id.clone()),
            arcUri: a.and_then(|a| a.game.toUris(a.edition.as_ref().map(|s| s.id.as_str())).into_iter().next()),
            path: path.path(),
            type_: Some(type_.to_string()),
        }
    }

    pub fn parse(state: Option<&str>) -> Option<ShellState> {
        let p: Vec<&str> = state?.split('|').collect();
        if p[0] != "ST" { return None; }
        let z = |i: usize| p.get(i).map(|s| s.to_string());
        Some(ShellState { familyId: z(1), arcUri: z(2), path: z(3), type_: z(4) })
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::binary::{newArchive, BinaryArchive};
    use crate::meta::MetaItem;

    #[test]
    fn keysParseAsTheCSharpDoes() {
        assert_eq!(parseKey(&Value::Null).unwrap(), Tag::None);
        assert_eq!(parseKey(&"plain".into()).unwrap(), Tag::Str("plain".into()));
        assert_eq!(parseKey(&"b64:AQID".into()).unwrap(), Tag::Bytes(vec![1, 2, 3]));
        assert_eq!(parseKey(&"hex:00FF10".into()).unwrap(), Tag::Bytes(vec![0x00, 0xFF, 0x10]));
        assert_eq!(parseKey(&"hex:/x01/x02".into()).unwrap(), Tag::Bytes(vec![0x01, 0x02]));
        assert_eq!(parseKey(&"asc:A\u{e9}B".into()).unwrap(), Tag::Bytes(vec![b'A', b'?', b'B']));
        assert!(parseKey(&"hex:ZZ".into()).is_err());
    }

    #[test]
    fn enginesSplitOnTheFirstColon() {
        assert_eq!(parseEngine(&"idTech:6".into()), (Some("idTech".into()), Some("6".into())));
        assert_eq!(parseEngine(&"a:b:c".into()), (Some("a".into()), Some("b:c".into())));
        assert_eq!(parseEngine(&"".into()), (None, None));
    }

    #[test]
    fn urisParse() {
        let u = Uri::parse("game:/main.yaml#CAT").unwrap();
        assert_eq!((u.scheme.as_str(), u.localPath.as_str(), u.fragment.as_str(), u.isFile), ("game", "/main.yaml", "#CAT", false));
        let u = Uri::parse("file:///tmp/x#APP").unwrap();
        assert_eq!((u.localPath.as_str(), u.isFile), ("/tmp/x", true));
        assert_eq!(FamilyGame::toUri("APP", Some("e"), None), "game:/#APP.e");
    }

    #[test]
    fn datesParse() {
        assert_eq!(parseDate("May 9, 2011").unwrap(), parseDate("2011-05-09").unwrap());
        assert_eq!(parseDate("Sep 9, 2011").unwrap(), parseDate("9 September 2011").unwrap());
        assert_eq!(parseDate("June 21, 2001").unwrap(), parseDate("2001-06-21").unwrap());
        assert_eq!(parseDate("1970-01-02").unwrap(), UNIX_EPOCH + Duration::from_secs(86400));
        assert_eq!(parseDate("2000-03-01").unwrap(), UNIX_EPOCH + Duration::from_secs(951868800));
    }

    #[test]
    fn everyFamilyLoadsWithItsGames() {
        loadFamilies(None, true).unwrap();
        let families = families();
        assert_eq!(families.len(), crate::config::FamilyIds.len());
        let mut games = 0;
        for (id, family) in families.iter() {
            assert_eq!(id, &family.id);
            for (gid, game) in &family.games {
                assert_eq!(gid, &game.id);
                assert!(!gid.starts_with('*'));
                assert!(game.family.get().is_some(), "{id}.{gid} has no family");
                println!("{id}.{gid}: {}", game.displayedName());
                games += 1;
            }
        }
        println!("{} families, {games} games", families.len());
    }

    #[test]
    fn defaultsFlowFromTheStarGame() {
        let family = getFamily("Uncore", true).unwrap().unwrap();
        let app = &family.games["APP"];
        assert_eq!(app.archiveType.as_deref(), Some("GameX.Uncore.UncoreArchive, GameX.Uncore"));
        assert_eq!(app.arcs.as_deref(), Some(&["game:/".to_string()][..]));
        assert_eq!(app.virtuals.get("testtri.gfx"), Some(&None));
        assert_eq!(app.toUris(None), vec!["game:/#APP"]);
        assert!(app.found.is_some(), "Direct:%AppPath% exists");
        assert_eq!(family.games["CAT"].toUris(None), vec!["game:/main.yaml#CAT"]);
        assert_eq!(app.getSample("0").unwrap().unwrap().paths.as_deref(), Some(&["testtri.gfx".to_string()][..]));
    }

    #[test]
    fn specFilesMergeIntoTheirFamily() {
        let family = getFamily("Capcom", true).unwrap().unwrap();
        let sub = family.games.values().find(|s| s.family().id != family.id).expect("a game from a spec file");
        assert!(sub.family().id.starts_with("Capcom+"));
    }

    #[test]
    fn anArchiveTypeOpensThroughTheFamily() {
        // A registered archiveType, as a family crate would register UncoreArchive.
        fn factory(parent: Option<Arc<dyn Archive>>, state: BinaryState) -> io::Result<Arc<dyn Archive>> {
            Ok(newArchive(BinaryArchive::new(parent, state, Some(<dyn crate::binary::ArcBinary>::stream()))))
        }
        registerType::<ArchiveFactory>("GameX.Uncore.UncoreArchive, GameX.Uncore", factory);
        let dir = std::env::temp_dir().join(format!("gamex-family-{}", std::process::id()));
        std::fs::create_dir_all(dir.join("sub")).unwrap();
        std::fs::write(dir.join("a.txt"), "a").unwrap();
        std::fs::write(dir.join("sub/b.txt"), "b").unwrap();
        let family = getFamily("Uncore", true).unwrap().unwrap();
        let uri = format!("file://{}#APP", dir.display());
        let arc = family.getArchive(&uri, true).unwrap().unwrap();
        let arc = arc.downcastBinaryArchive().expect("a ManyArchive");
        let mut paths: Vec<String> = arc.files().unwrap().iter().map(|s| s.path.clone()).collect();
        paths.sort();
        assert_eq!(paths, vec!["a.txt", "sub/b.txt", "testtri.gfx"]);
        let data = getResultData(&arc, "sub/b.txt");
        assert_eq!(data, b"b");
        std::fs::remove_dir_all(&dir).unwrap();
    }

    fn getResultData(arc: &BinaryArchive, path: &str) -> Vec<u8> {
        let mut s = crate::binary::getResult(arc.getData(&Tag::Str(path.into()), &Tag::None, true)).unwrap().unwrap();
        let mut z = Vec::new();
        s.read_to_end(&mut z).unwrap();
        z
    }

    #[test]
    fn shellStatesRoundTrip() {
        let s = ShellState { familyId: Some("F".into()), arcUri: Some("game:/#G".into()), path: Some("p".into()), type_: Some("t".into()) };
        assert_eq!(ShellState::parse(Some(&s.to_string())), Some(s));
        assert_eq!(ShellState::parse(Some("XX|a")), None);
    }
}
