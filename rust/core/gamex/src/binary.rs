// PORT-SOURCE: Core/GameX/Binary.cs
// PORT-SHA: e46d1fce4ffca728
// PORT-STATUS: done
//
// C#'s class tree maps to:
//   * `Binary` + `Archive`'s fields: the `Binary` struct, reached through `Archive::base()`.
//   * `abstract class Archive`: the `Archive` trait (its virtual members) plus
//     `ArchiveExt` (its non-virtual ones, and `Binary.Open`/`Close`).
//   * `BinaryArchive`: a struct. Its subclasses that override `Read`, `ReadData`,
//     `Opening`, ... (`ManyArchive`, `SubArchive`, `StreamArchive`) plug in a
//     `BinaryArchiveVirtual`; the ones that only set things up in a constructor
//     are plain constructor functions returning `Arc<BinaryArchive>`.
//   * `Task<T>`: a boxed local future. `Open`/`Close` wait on `Opening`/`Closing`
//     with `getResult`, as C# does with `GetAwaiter().GetResult()`.
//   * C# `object` (paths, options, tags, assets): `meta::Tag`.
//   * `GetAsset<T>` returns the asset as a `Tag`; callers match on it.
//
// `GenericPoolX.ActionAsync` / `FuncAsync` release the reader when the task is
// created, not when it finishes. A checkout here is held until drop, including
// across `.await`. `SinglePool` is not used: its `Release` disposes the only
// instance. A missing writer file is `NotFound` (C# stores a null pool and
// throws later). A filesystem with no `openWrite` is still `Unsupported`.
// A reader that cannot be owned falls back to `open()`.
// `tag` is a `Mutex` and `useReader` is an `AtomicBool`: C# assigns both
// through the shared archive. `baseOpening` clones `tag` out of the lock
// before `read`.

use std::any::{Any, TypeId};
use std::collections::{HashMap, HashSet};
use std::future::Future;
use std::io::{self, Cursor, Error, ErrorKind, Read, Seek, SeekFrom, Write};
use std::pin::Pin;
use std::sync::atomic::{AtomicBool, AtomicI32, AtomicU32, Ordering};
use std::sync::{Arc, LazyLock, Mutex, MutexGuard, OnceLock, RwLock, Weak};
use std::task::{Context, Poll, Wake, Waker};

use openx_poly::log::Log;
use openx_poly::pool::GenericPool;
use openx_vfx::vfx::{DirectoryFileSystem, FileSystem, VfsRead, VfsWrite};

use crate::family::{Family, FamilyGame, FamilyGameEdition};
use crate::formats::stream::PakBinaryCanStream;
use crate::meta::{directoryName, fileName, FileSource, MetaInfo, MetaItem, MetaItemFilter, MetaManager, Tag};

/// C# `Task<T>`.
pub type Task<'a, T> = Pin<Box<dyn Future<Output = io::Result<T>> + 'a>>;
/// C# `Stream`.
pub type Stream = Box<dyn VfsRead + Send>;
/// C# `FileSystem` as GameX holds it.
pub type Vfx = Arc<dyn FileSystem>;
/// C# `Func<BinaryReader, FileSource, Archive, Task<object>>`.
pub type AssetFactory = for<'a> fn(&'a mut Stream, Arc<FileSource>, &'a dyn Archive) -> Task<'a, Tag>;
/// C# `Archive.FuncAssetFactory`.
pub type FuncAssetFactory = fn(&FileSource, &FamilyGame) -> (Tag, Option<AssetFactory>);
/// C# `Func<object, object>`, a path finder.
pub type PathFinder = fn(Tag) -> Tag;
/// C# `Func<MetaManager, BinaryArchive, FileSource, Task<List<MetaInfo>>>`.
pub type MetaInfoFactory = for<'a> fn(&'a dyn MetaManager, &'a BinaryArchive, &'a Arc<FileSource>) -> Task<'a, Vec<MetaInfo>>;

/// C# `task.GetAwaiter().GetResult()`.
pub fn getResult<F: Future>(f: F) -> F::Output {
    struct Unpark(std::thread::Thread);
    impl Wake for Unpark { fn wake(self: Arc<Self>) { self.0.unpark(); } }
    let waker = Waker::from(Arc::new(Unpark(std::thread::current())));
    let mut cx = Context::from_waker(&waker);
    let mut f = std::pin::pin!(f);
    loop {
        if let Poll::Ready(v) = f.as_mut().poll(&mut cx) { return v; }
        std::thread::park();
    }
}

pub(crate) fn ok<'a, T: 'a>(v: T) -> Task<'a, T> { Box::pin(async move { Ok(v) }) }
pub(crate) fn err<'a, T: 'a>(kind: ErrorKind, msg: String) -> Task<'a, T> { Box::pin(async move { Err(Error::new(kind, msg)) }) }

// #region FileOption

bitflags::bitflags! {
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct FileOption: i32 {
        const Default = 0x0;
        const Raw = 0x1;
        const Marker = 0x2;
        const Object = 0x4;
        const BinaryObject = Self::Object.bits() | 0x8;
        const StreamObject = Self::Object.bits() | 0x10;
        const UnknownFileModel = 0x100;
        const Hosting = Self::Raw.bits() | Self::Marker.bits();
    }
}

// #region BinaryState

#[derive(Clone)]
pub struct BinaryState {
    pub vfx: Vfx,
    pub game: Arc<FamilyGame>,
    pub edition: Option<Arc<FamilyGameEdition>>,
    pub path: String,
    pub tag: Tag,
}

impl BinaryState {
    pub fn new(vfx: Vfx, game: Arc<FamilyGame>, edition: Option<Arc<FamilyGameEdition>>, path: Option<&str>, tag: Tag) -> Self {
        Self { vfx, game, edition, path: path.unwrap_or("").to_string(), tag }
    }
}

// #region Binary

/// C# `Binary.Stat`.
#[derive(Debug, Clone, Copy, PartialEq, Eq, Default)]
pub enum BinaryStat { Opening, Opened, Closing, #[default] Closed }

/// C# `Binary`'s fields, and `Archive`'s.
pub struct Binary {
    pub status: Mutex<BinaryStat>,
    pub vfx: Vfx,
    pub family: Arc<Family>,
    pub game: Arc<FamilyGame>,
    pub edition: Option<Arc<FamilyGameEdition>>,
    pub binPath: String,
    pub name: String,
    /// C# `object Tag`, assigned through the shared archive (`Binary_Zip`).
    pub tag: Mutex<Tag>,
    pub valid: AtomicBool,
    // Archive
    pub pathFinders: HashMap<TypeId, PathFinder>,
    pub assetFactoryFunc: Option<FuncAssetFactory>,
    pub count: AtomicI32,
    me: OnceLock<Weak<dyn Archive>>,
}

impl Binary {
    pub fn new(state: BinaryState) -> Self {
        let name = if state.path.is_empty() { String::new() }
            else { let z = fileName(&state.path); if !z.is_empty() { z } else { fileName(directoryName(&state.path)) } };
        Self {
            status: Mutex::new(BinaryStat::Closed),
            vfx: state.vfx,
            family: state.game.family(),
            game: state.game,
            edition: state.edition,
            binPath: state.path,
            name,
            tag: Mutex::new(state.tag),
            valid: AtomicBool::new(true),
            pathFinders: HashMap::new(),
            assetFactoryFunc: None,
            count: AtomicI32::new(0),
            me: OnceLock::new(),
        }
    }

    pub fn status(&self) -> BinaryStat { *self.status.lock().unwrap() }
    pub fn setStatus(&self, value: BinaryStat) { *self.status.lock().unwrap() = value; }
    pub fn valid(&self) -> bool { self.valid.load(Ordering::Relaxed) }
    pub fn count(&self) -> i32 { self.count.load(Ordering::Relaxed) }

    /// C# `this`, for a method that returns it.
    pub fn this(&self) -> Arc<dyn Archive> { self.me.get().and_then(Weak::upgrade).expect("archive not created with newArchive") }
}

/// Wraps a new archive in an `Arc` so it can hand out `this`. Every archive is created through it.
pub fn newArchive<A: Archive>(archive: A) -> Arc<A> {
    let archive = Arc::new(archive);
    let me: Weak<dyn Archive> = Arc::downgrade(&archive) as Weak<A>;
    let _ = archive.base().me.set(me);
    archive
}

// #region Archive

/// C# `abstract class Archive`: its virtual members.
pub trait Archive: openx_poly::core::ISource + Any + Send + Sync {
    fn base(&self) -> &Binary;
    /// C# `this is BinaryArchive`.
    fn asBinaryArchive(&self) -> Option<&BinaryArchive> { None }
    /// C# `this is ITransformAsset<T>`.
    fn asTransformAsset(&self) -> Option<&dyn ITransformAsset> { None }

    /// Opens this instance.
    fn opening(&self) -> Task<'_, ()> { ok(()) }
    /// Closes this instance.
    fn closing(&self) -> Task<'_, ()> { ok(()) }

    /// Determines whether this instance contains the item.
    fn contains(&self, path: &Tag) -> io::Result<bool>;
    /// Gets the file source.
    fn getSource(&self, path: &Tag, throwOnError: bool) -> io::Result<(Option<Arc<dyn Archive>>, Option<Arc<FileSource>>)>;
    /// Loads the file data asynchronous.
    fn getData<'a>(&'a self, path: &'a Tag, option: &'a Tag, throwOnError: bool) -> Task<'a, Option<Stream>>;
    /// Loads the object asynchronous.
    fn getAsset<'a>(&'a self, path: &'a Tag, option: &'a Tag, throwOnError: bool) -> Task<'a, Tag>;

    // #region Metadata

    /// Gets the metadata item filters.
    fn getMetaFilters(&self, manager: &dyn MetaManager) -> Option<Vec<MetaItemFilter>> {
        Some(self.base().game.filters.iter().map(|(k, v)| MetaItemFilter::new(k, v)).collect())
    }
    /// Gets the metadata infos.
    fn getMetaInfos<'a>(&'a self, manager: &'a dyn MetaManager, item: &'a MetaItem) -> Task<'a, Option<Vec<MetaInfo>>> { err(ErrorKind::Unsupported, "NotImplemented".into()) }
    /// Gets the metadata items.
    fn getMetaItems(&self, manager: &dyn MetaManager) -> io::Result<Vec<MetaItem>> { Err(Error::new(ErrorKind::Unsupported, "NotImplemented")) }
}

/// C# `Archive`'s non-virtual members, and `Binary.Open`/`Close`.
pub trait ArchiveExt: Archive {
    /// Closes this instance.
    fn close(&self) -> io::Result<()> {
        let base = self.base();
        base.setStatus(BinaryStat::Closing);
        getResult(self.closing())?;
        base.setStatus(BinaryStat::Closed);
        Ok(())
    }

    /// Opens this instance: `Binary.Open`, then `Archive.Open` adds the meta items.
    fn open(&self, items: Option<&mut Vec<MetaItem>>, manager: Option<&dyn MetaManager>) -> io::Result<()> {
        let base = self.base();
        if base.status() != BinaryStat::Closed { return Ok(()); }
        base.setStatus(BinaryStat::Opening);
        let watch = std::time::Instant::now();
        getResult(self.opening())?;
        base.setStatus(BinaryStat::Opened);
        Log::info(&format!("Opened[{}]: {} @ {}ms", base.game.id, base.name, watch.elapsed().as_millis()));
        if let Some(items) = items {
            let manager = manager.ok_or_else(|| Error::new(ErrorKind::InvalidInput, "manager"))?;
            items.extend(self.getMetaItems(manager)?);
        }
        Ok(())
    }

    /// Finds the path.
    fn findPath<T: 'static>(&self, path: Tag) -> Tag {
        let pathFinders = &self.base().pathFinders;
        if pathFinders.len() != 1 { return match pathFinders.get(&TypeId::of::<T>()) { Some(z) => z(path), None => path }; }
        let (key, value) = pathFinders.iter().next().unwrap();
        if *key == TypeId::of::<T>() || *key == TypeId::of::<Tag>() { value(path) } else { path }
    }

    /// Opens the family arc file.
    fn getArchive(&self, path: &Tag, throwOnError: bool) -> io::Result<Option<Arc<dyn Archive>>> {
        let base = self.base();
        match path {
            Tag::Str(s) => {
                let Some(arc) = base.game.getArchive(base.vfx.clone(), base.edition.clone(), Some(s), throwOnError)? else { return Ok(None) };
                arc.open(None, None)?;
                Ok(Some(arc))
            }
            _ => Err(Error::new(ErrorKind::InvalidInput, "path")),
        }
    }

    // #region Transform

    /// Loads the object transformed asynchronous: C# `GetAsset<T>(path, transformTo)`.
    fn getAssetTransform<'a>(&'a self, path: &'a Tag, transformTo: &'a dyn Archive) -> Task<'a, Tag> where Self: Sized {
        Box::pin(async move {
            let source = self.getAsset(path, &Tag::None, true).await?;
            transformAsset(self, source, transformTo).await
        })
    }
}

impl<T: Archive + ?Sized> ArchiveExt for T {}

/// Transforms the file object asynchronous.
fn transformAsset<'a>(this: &'a dyn Archive, source: Tag, transformTo: &'a dyn Archive) -> Task<'a, Tag> {
    if let Some(left) = this.asTransformAsset().filter(|s| s.canTransformAsset(&source, transformTo)) { left.transformAsset(source, transformTo) }
    else if let Some(right) = transformTo.asTransformAsset().filter(|s| s.canTransformAsset(&source, transformTo)) { right.transformAsset(source, transformTo) }
    else { err(ErrorKind::InvalidInput, "transformTo".into()) }
}

impl dyn Archive {
    /// An empty archive.
    pub fn empty() -> Arc<dyn Archive> {
        static EMPTY: LazyLock<Arc<dyn Archive>> = LazyLock::new(|| {
            let mut base = Binary::new(BinaryState::new(Arc::new(DirectoryFileSystem::new("", None)), FamilyGame::empty(), None, None, Tag::None));
            base.name = "Empty".into();
            newArchive(EmptyArchive { base })
        });
        EMPTY.clone()
    }

    /// C# `(BinaryArchive)archive`.
    pub fn downcastBinaryArchive(self: Arc<Self>) -> Option<Arc<BinaryArchive>> {
        let any: Arc<dyn Any + Send + Sync> = self;
        any.downcast::<BinaryArchive>().ok()
    }
}

/// Implements `ISource` for an archive type; `GetAsset` returns its `Tag`.
#[macro_export]
macro_rules! archiveSource {
    ($t:ty) => {
        impl openx_poly::core::ISource for $t {
            fn getAssetAny<'a>(&'a self, path: &'a openx_poly::core::SourcePath, option: Option<&'a (dyn std::any::Any + Sync)>) -> openx_poly::core::BoxFuture<'a, Result<Box<dyn std::any::Any + Send + Sync>, std::io::Error>> {
                // C# returns the object when the path already is that object.
                // `ObjectModelManager` does not use this arm: boxing the `Arc` here
                // would hide the concrete `TypeId`. See `SourcePath::Obj`.
                if let openx_poly::core::SourcePath::Obj(obj) = path {
                    let obj = obj.clone();
                    return Box::pin(async move { Ok(Box::new(obj) as Box<dyn std::any::Any + Send + Sync>) });
                }
                Box::pin(async move {
                    let path = match path {
                        openx_poly::core::SourcePath::Str(s) => $crate::meta::Tag::Str(s.clone()),
                        openx_poly::core::SourcePath::Int(i) => $crate::meta::Tag::Int(*i as i64),
                        openx_poly::core::SourcePath::Obj(_) => unreachable!("Obj is returned above"),
                    };
                    let asset = $crate::binary::getResult($crate::binary::Archive::getAsset(self, &path, &$crate::meta::Tag::None, true))?;
                    Ok(Box::new(asset) as Box<dyn std::any::Any + Send + Sync>)
                })
            }
        }
    };
}

struct EmptyArchive { base: Binary }
archiveSource!(EmptyArchive);

impl Archive for EmptyArchive {
    fn base(&self) -> &Binary { &self.base }
    fn contains(&self, path: &Tag) -> io::Result<bool> { Ok(false) }
    fn getSource(&self, path: &Tag, throwOnError: bool) -> io::Result<(Option<Arc<dyn Archive>>, Option<Arc<FileSource>>)> { Err(Error::new(ErrorKind::Unsupported, "NotImplemented")) }
    fn getData<'a>(&'a self, path: &'a Tag, option: &'a Tag, throwOnError: bool) -> Task<'a, Option<Stream>> { err(ErrorKind::Unsupported, "NotImplemented".into()) }
    fn getAsset<'a>(&'a self, path: &'a Tag, option: &'a Tag, throwOnError: bool) -> Task<'a, Tag> { err(ErrorKind::Unsupported, "NotImplemented".into()) }
}

// #region BinaryArchive

/// The members a `BinaryArchive` subclass overrides. Defaults are `BinaryArchive`'s own.
pub trait BinaryArchiveVirtual: Send + Sync {
    fn opening<'a>(&'a self, s: &'a BinaryArchive) -> Task<'a, ()> { s.baseOpening() }
    fn closing<'a>(&'a self, s: &'a BinaryArchive) -> Task<'a, ()> { s.baseClosing() }
    fn getReader<'a>(&'a self, s: &'a BinaryArchive, path: Option<&str>) -> io::Result<Option<Box<dyn VfsRead + 'a>>> { s.baseGetReader(path) }
    fn read<'a>(&'a self, s: &'a BinaryArchive, tag: &'a Tag) -> Task<'a, ()> { s.baseRead(tag) }
    fn readData<'a>(&'a self, s: &'a BinaryArchive, file: &'a Arc<FileSource>, option: &'a Tag) -> Task<'a, Option<Stream>> { s.baseReadData(file, option) }
    fn write<'a>(&'a self, s: &'a BinaryArchive, tag: &'a Tag) -> Task<'a, ()> { s.baseWrite(tag) }
    fn writeData<'a>(&'a self, s: &'a BinaryArchive, file: &'a Arc<FileSource>, data: Stream, option: &'a Tag) -> Task<'a, ()> { s.baseWriteData(file, data, option) }
}

pub struct BinaryArchive {
    pub base: Binary,
    pub arcBinary: Option<Arc<dyn ArcBinary>>,
    // options
    pub retainInPool: i32,
    /// C# `bool`, assigned through the shared archive (`Binary_Zip` sets it false in `Read`).
    pub useReader: AtomicBool,
    pub useWriter: bool,
    pub useFileId: bool,
    // state
    pub fileMask: Option<fn(&str) -> String>,
    pub params: Mutex<HashMap<String, String>>,
    pub magic: AtomicU32,
    pub version: AtomicU32,
    // metadata/factory
    pub metaInfos: HashMap<String, MetaInfoFactory>,
    // binary
    pub files: RwLock<Option<Vec<Arc<FileSource>>>>,
    pub filesRawSet: RwLock<Option<HashSet<String>>>,
    pub filesById: RwLock<Option<HashMap<i32, Vec<Arc<FileSource>>>>>,
    /// Keyed by lower-cased path: C# `StringComparer.OrdinalIgnoreCase`.
    pub filesByPath: RwLock<Option<HashMap<String, Vec<Arc<FileSource>>>>>,
    pub pathSkip: usize,
    pub atEnd: AtomicBool,
    pub virt: Option<Box<dyn BinaryArchiveVirtual>>,
    readers: Mutex<HashMap<String, Arc<GenericPool<Box<dyn VfsRead + Send>>>>>,
    writers: Mutex<HashMap<String, Arc<GenericPool<Box<dyn VfsWrite + Send>>>>>,
}
archiveSource!(BinaryArchive);

/// A pooled reader. Drop seeks to 0 and releases it. A failed seek drops it.
struct PooledReader {
    inner: Option<Box<dyn VfsRead + Send>>,
    pool: Arc<GenericPool<Box<dyn VfsRead + Send>>>,
}

impl Read for PooledReader {
    fn read(&mut self, buf: &mut [u8]) -> io::Result<usize> { self.inner.as_mut().unwrap().read(buf) }
}
impl Seek for PooledReader {
    fn seek(&mut self, pos: SeekFrom) -> io::Result<u64> { self.inner.as_mut().unwrap().seek(pos) }
}
impl Drop for PooledReader {
    fn drop(&mut self) {
        let Some(mut r) = self.inner.take() else { return };
        if r.seek(SeekFrom::Start(0)).is_ok() { self.pool.release(r); }
    }
}

/// A pooled writer. `Seek` runs on drop, which is where C# resets.
struct PooledWriter {
    inner: Option<Box<dyn VfsWrite + Send>>,
    pool: Arc<GenericPool<Box<dyn VfsWrite + Send>>>,
}

impl Write for PooledWriter {
    fn write(&mut self, buf: &[u8]) -> io::Result<usize> { self.inner.as_mut().unwrap().write(buf) }
    fn flush(&mut self) -> io::Result<()> { self.inner.as_mut().unwrap().flush() }
}
impl Drop for PooledWriter {
    fn drop(&mut self) {
        let Some(mut w) = self.inner.take() else { return };
        if w.seek(SeekFrom::Start(0)).is_ok() { self.pool.release(w); }
    }
}

impl BinaryArchive {
    pub fn new(parent: Option<Arc<dyn Archive>>, state: BinaryState, arcBinary: Option<Arc<dyn ArcBinary>>) -> Self {
        Self {
            base: Binary::new(state),
            arcBinary,
            retainInPool: 10,
            useReader: AtomicBool::new(true),
            useWriter: true,
            useFileId: false,
            fileMask: None,
            params: Mutex::new(HashMap::new()),
            magic: AtomicU32::new(0),
            version: AtomicU32::new(0),
            metaInfos: HashMap::new(),
            files: RwLock::new(None),
            filesRawSet: RwLock::new(None),
            filesById: RwLock::new(None),
            filesByPath: RwLock::new(None),
            pathSkip: 0,
            atEnd: AtomicBool::new(false),
            virt: None,
            readers: Mutex::new(HashMap::new()),
            writers: Mutex::new(HashMap::new()),
        }
    }

    pub fn files(&self) -> Option<Vec<Arc<FileSource>>> { self.files.read().unwrap().clone() }
    pub fn setFiles(&self, files: Option<Vec<Arc<FileSource>>>) { *self.files.write().unwrap() = files; }
    pub fn atEnd(&self) -> bool { self.atEnd.load(Ordering::Relaxed) }

    fn arcBinary(&self) -> io::Result<&Arc<dyn ArcBinary>> { self.arcBinary.as_ref().ok_or_else(|| Error::new(ErrorKind::InvalidData, "ArcBinary")) }
    fn byPath(&self, path: &str) -> Vec<Arc<FileSource>> {
        self.filesByPath.read().unwrap().as_ref().and_then(|s| s.get(&path.to_lowercase()).cloned()).unwrap_or_default()
    }

    // #region Pool

    /// Gets the binary reader.
    pub fn getReader(&self, path: Option<&str>) -> io::Result<Option<Box<dyn VfsRead + '_>>> {
        match &self.virt { Some(v) => v.getReader(self, path), None => self.baseGetReader(path) }
    }
    pub fn baseGetReader(&self, path: Option<&str>) -> io::Result<Option<Box<dyn VfsRead + '_>>> {
        let path = path.unwrap_or(&self.base.binPath).to_string();
        let pool = {
            let mut map = self.readers.lock().unwrap_or_else(|p| p.into_inner());
            if let Some(pool) = map.get(&path).cloned() { Some(pool) }
            else if self.base.vfx.fileExists(&path) {
                let pool = Arc::new(self.readerPool(&path));
                map.insert(path.clone(), pool.clone());
                Some(pool)
            }
            else { None }
        };
        let Some(pool) = pool else { return Ok(None); };
        if let Some(r) = pool.tryTake() { return Ok(Some(Box::new(PooledReader { inner: Some(r), pool }))); }
        if let Some(r) = self.base.vfx.openOwned(&path) { return Ok(Some(Box::new(PooledReader { inner: Some(r), pool }))); }
        Ok(self.base.vfx.open(&path))
    }

    fn readerPool(&self, path: &str) -> GenericPool<Box<dyn VfsRead + Send>> {
        let vfx = self.base.vfx.clone();
        let path = path.to_string();
        // Checkout calls `openOwned` itself. `get` is the C# factory, used when the pool is asked directly.
        GenericPool::new(move || vfx.openOwned(&path).unwrap_or_else(|| Box::new(Cursor::new(Vec::new())) as Box<dyn VfsRead + Send>))
            .withReset(|r| { let _ = r.seek(SeekFrom::Start(0)); })
            .withRetain(self.retainInPool.max(0) as usize)
    }

    /// Gets the binary writer.
    pub fn getWriter(&self, path: Option<&str>) -> io::Result<Box<dyn Write + '_>> {
        let path = path.unwrap_or(&self.base.binPath).to_string();
        if let Some(pool) = self.writers.lock().unwrap_or_else(|p| p.into_inner()).get(&path).cloned() {
            if let Some(w) = pool.tryTake() { return Ok(Box::new(PooledWriter { inner: Some(w), pool })); }
            if let Some(w) = self.base.vfx.openWrite(&path) { return Ok(Box::new(PooledWriter { inner: Some(w), pool })); }
        }
        if !self.base.vfx.fileExists(&path) { return Err(Error::new(ErrorKind::NotFound, path)); }
        let Some(w) = self.base.vfx.openWrite(&path) else {
            return Err(Error::new(ErrorKind::Unsupported, "openx_vfx FileSystem has no write mode"));
        };
        let pool = Arc::new(self.writerPool(&path));
        self.writers.lock().unwrap_or_else(|p| p.into_inner()).insert(path, pool.clone());
        Ok(Box::new(PooledWriter { inner: Some(w), pool }))
    }

    fn writerPool(&self, path: &str) -> GenericPool<Box<dyn VfsWrite + Send>> {
        let vfx = self.base.vfx.clone();
        let path = path.to_string();
        GenericPool::new(move || vfx.openWrite(&path).unwrap_or_else(|| Box::new(Cursor::new(Vec::new())) as Box<dyn VfsWrite + Send>))
            .withReset(|w| { let _ = w.seek(SeekFrom::Start(0)); })
            .withRetain(self.retainInPool.max(0) as usize)
    }

    // #endregion

    pub fn baseOpening(&self) -> Task<'_, ()> {
        Box::pin(async move {
            let tag = self.base.tag.lock().unwrap_or_else(|p| p.into_inner()).clone();
            self.read(&tag).await?;
            self.process().await?;
            self.base.valid.store(self.files.read().unwrap().is_some(), Ordering::Relaxed);
            self.base.count.store(self.filesByPath.read().unwrap().as_ref().map_or(0, |s| s.len()) as i32, Ordering::Relaxed);
            Ok(())
        })
    }

    pub fn baseClosing(&self) -> Task<'_, ()> {
        self.base.valid.store(false, Ordering::Relaxed);
        self.base.count.store(0, Ordering::Relaxed);
        self.setFiles(None);
        *self.filesRawSet.write().unwrap() = None;
        *self.filesById.write().unwrap() = None;
        *self.filesByPath.write().unwrap() = None;
        // C# disposes `Readers` only. `Writers` stays.
        self.readers.lock().unwrap_or_else(|p| p.into_inner()).clear();
        ok(())
    }

    /// Ensures the file object factory.
    pub fn ensureCachedAssetFactory(&self, file: &FileSource) -> Option<AssetFactory> {
        let assetFactoryFunc = self.base.assetFactoryFunc?;
        *file.cachedObjectFactory.get_or_init(|| {
            let (option, factory) = assetFactoryFunc(file, &self.base.game);
            let _ = file.cachedObjectOption.set(option);
            factory
        })
    }

    /// Processes this instance.
    pub fn process(&self) -> Task<'_, ()> {
        Box::pin(async move {
            let files = self.files();
            if self.useFileId {
                let mut z: HashMap<i32, Vec<Arc<FileSource>>> = HashMap::new();
                for s in files.iter().flatten() { z.entry(s.id).or_default().push(s.clone()); }
                *self.filesById.write().unwrap() = Some(z);
            }
            if let Some(files) = &files {
                let mut z: HashMap<String, Vec<Arc<FileSource>>> = HashMap::new();
                for s in files { z.entry(s.path.to_lowercase()).or_default().push(s.clone()); }
                *self.filesByPath.write().unwrap() = Some(z);
            }
            if let Some(arcBinary) = &self.arcBinary { arcBinary.process(self).await?; }
            Ok(())
        })
    }

    /// FindPath.
    fn findPathArc(&self, path: &str) -> io::Result<(Option<Arc<BinaryArchive>>, Option<String>)> {
        let paths: Vec<&str> = path.splitn(2, ':').collect();
        let p = paths[0].replace('\\', "/");
        let arc = self.byPath(&p).first().and_then(|s| s.arc.clone());
        if let Some(arc) = &arc { arc.open(None, None)?; }
        let next = if arc.is_some() && paths.len() > 1 { Some(paths[1].to_string()) } else { None };
        Ok((arc, next))
    }

    // #region ArcBinary

    /// Reads the asynchronous.
    pub fn read<'a>(&'a self, tag: &'a Tag) -> Task<'a, ()> {
        match &self.virt { Some(v) => v.read(self, tag), None => self.baseRead(tag) }
    }
    pub fn baseRead<'a>(&'a self, tag: &'a Tag) -> Task<'a, ()> {
        Box::pin(async move {
            let arcBinary = self.arcBinary()?;
            if !self.useReader.load(Ordering::Relaxed) { return arcBinary.read(self, None, tag).await; }
            let mut r = self.getReader(None)?.ok_or_else(|| Error::new(ErrorKind::NotFound, self.base.binPath.clone()))?;
            arcBinary.read(self, Some(&mut *r), tag).await
        })
    }

    /// Reads the file data asynchronous.
    pub fn readData<'a>(&'a self, file: &'a Arc<FileSource>, option: &'a Tag) -> Task<'a, Option<Stream>> {
        match &self.virt { Some(v) => v.readData(self, file, option), None => self.baseReadData(file, option) }
    }
    pub fn baseReadData<'a>(&'a self, file: &'a Arc<FileSource>, option: &'a Tag) -> Task<'a, Option<Stream>> {
        Box::pin(async move {
            let arcBinary = self.arcBinary()?;
            if !self.useReader.load(Ordering::Relaxed) { return arcBinary.readData(self, None, file, option).await; }
            let mut r = self.getReader(None)?.ok_or_else(|| Error::new(ErrorKind::NotFound, self.base.binPath.clone()))?;
            arcBinary.readData(self, Some(&mut *r), file, option).await
        })
    }

    /// Writes the asynchronous.
    pub fn write<'a>(&'a self, tag: &'a Tag) -> Task<'a, ()> {
        match &self.virt { Some(v) => v.write(self, tag), None => self.baseWrite(tag) }
    }
    pub fn baseWrite<'a>(&'a self, tag: &'a Tag) -> Task<'a, ()> {
        Box::pin(async move {
            let arcBinary = self.arcBinary()?;
            if !self.useWriter { return arcBinary.write(self, None, tag).await; }
            let mut w = self.getWriter(None)?;
            arcBinary.write(self, Some(&mut *w), tag).await
        })
    }

    /// Writes the file data asynchronous.
    pub fn writeData<'a>(&'a self, file: &'a Arc<FileSource>, data: Stream, option: &'a Tag) -> Task<'a, ()> {
        match &self.virt { Some(v) => v.writeData(self, file, data, option), None => self.baseWriteData(file, data, option) }
    }
    pub fn baseWriteData<'a>(&'a self, file: &'a Arc<FileSource>, data: Stream, option: &'a Tag) -> Task<'a, ()> {
        Box::pin(async move {
            let arcBinary = self.arcBinary()?;
            if !self.useWriter { return arcBinary.writeData(self, None, file, data, option).await; }
            let mut w = self.getWriter(None)?;
            arcBinary.writeData(self, Some(&mut *w), file, data, option).await
        })
    }
}

impl Archive for BinaryArchive {
    fn base(&self) -> &Binary { &self.base }
    fn asBinaryArchive(&self) -> Option<&BinaryArchive> { Some(self) }

    fn opening(&self) -> Task<'_, ()> {
        match &self.virt { Some(v) => v.opening(self), None => self.baseOpening() }
    }

    fn closing(&self) -> Task<'_, ()> {
        match &self.virt { Some(v) => v.closing(self), None => self.baseClosing() }
    }

    /// Determines whether the arc contains the specified file path.
    fn contains(&self, path: &Tag) -> io::Result<bool> {
        match path {
            Tag::None => Err(Error::new(ErrorKind::InvalidInput, "path")),
            Tag::Str(s) => {
                let (arc, next) = self.findPathArc(s)?;
                match arc {
                    Some(arc) => arc.contains(&next.map(Tag::Str).unwrap_or_default()),
                    None => Ok(self.filesByPath.read().unwrap().as_ref().is_some_and(|z| z.contains_key(&s.replace('\\', "/").to_lowercase()))),
                }
            }
            Tag::Int(i) => Ok(self.filesById.read().unwrap().as_ref().is_some_and(|z| z.contains_key(&(*i as i32)))),
            _ => Err(Error::new(ErrorKind::InvalidInput, "path")),
        }
    }

    /// Gets the file source.
    fn getSource(&self, path: &Tag, throwOnError: bool) -> io::Result<(Option<Arc<dyn Archive>>, Option<Arc<FileSource>>)> {
        match path {
            Tag::None => Err(Error::new(ErrorKind::InvalidInput, "path")),
            Tag::File(f) => Ok((Some(self.base.this()), Some(f.clone()))),
            Tag::Str(s) => {
                let (arc, next) = self.findPathArc(s)?;
                if let Some(arc) = arc { return match next { Some(next) => arc.getSource(&Tag::Str(next), true), None => Ok((Some(arc), None)) }; }
                let files = self.byPath(&s.replace('\\', "/"));
                if files.len() == 1 { return Ok((Some(self.base.this()), Some(files[0].clone()))); }
                Log::info(&format!("ERROR.GetSource: {s} @ {}", files.len()));
                if throwOnError { return Err(Error::new(ErrorKind::NotFound, if files.is_empty() { format!("File not found: {s}") } else { format!("More then one file found: {s}") })); }
                Ok((None, None))
            }
            Tag::Int(i) => {
                let files = self.filesById.read().unwrap().as_ref().and_then(|z| z.get(&(*i as i32)).cloned()).unwrap_or_default();
                if files.len() == 1 { return Ok((Some(self.base.this()), Some(files[0].clone()))); }
                Log::info(&format!("ERROR.GetSource: {i} @ {}", files.len()));
                if throwOnError { return Err(Error::new(ErrorKind::NotFound, if files.is_empty() { format!("File not found: {i}") } else { format!("More then one file found: {i}") })); }
                Ok((None, None))
            }
            _ => Ok((Some(self.base.this()), None)),
        }
    }

    /// Loads the file data asynchronous.
    fn getData<'a>(&'a self, path: &'a Tag, option: &'a Tag, throwOnError: bool) -> Task<'a, Option<Stream>> {
        Box::pin(async move {
            match path {
                Tag::None => Ok(None),
                Tag::File(f) => self.readData(&f.fix(), option).await,
                _ => {
                    let path = self.findPath::<Stream>(path.clone());
                    let (arc, next) = self.getSource(&path, throwOnError)?;
                    match arc { Some(arc) => arc.getData(&next.map(Tag::File).unwrap_or_default(), option, throwOnError).await, None => Ok(None) }
                }
            }
        })
    }

    /// Loads the file object asynchronous.
    fn getAsset<'a>(&'a self, path: &'a Tag, option: &'a Tag, throwOnError: bool) -> Task<'a, Tag> {
        Box::pin(async move {
            let f = match path {
                Tag::None => return Ok(Tag::None),
                Tag::File(f) => f.clone(),
                _ => {
                    let path = self.findPath::<Tag>(path.clone());
                    let (arc, next) = self.getSource(&path, throwOnError)?;
                    return match (arc, next) { (Some(arc), Some(next)) => arc.getAsset(&Tag::File(next), option, throwOnError).await, _ => Ok(path) };
                }
            };
            if self.base.game.isArcPath(&f.path) { return Ok(Tag::None); }
            let Some(mut data) = self.getData(&Tag::File(f.clone()), option, throwOnError).await? else { return Ok(Tag::None) };
            if let Some(assetFactory) = self.ensureCachedAssetFactory(&f) {
                let value = assetFactory(&mut data, f.clone(), self).await;
                let atEnd = (|| -> io::Result<bool> { let pos = data.stream_position()?; Ok(pos == data.seek(SeekFrom::End(0))?) })();
                self.atEnd.store(atEnd.unwrap_or(false), Ordering::Relaxed);
                return value.inspect_err(|e| Log::error(&e.to_string()));
            }
            Ok(Tag::stream(data))
        })
    }

    // #region Metadata

    /// Gets the explorer information nodes.
    fn getMetaInfos<'a>(&'a self, manager: &'a dyn MetaManager, item: &'a MetaItem) -> Task<'a, Option<Vec<MetaInfo>>> {
        match (self.base.valid(), item.source.asFileSource()) {
            (true, Some(file)) => <dyn MetaManager>::getMetaInfos(manager, self, file),
            _ => ok(Some(vec![])),
        }
    }

    /// Gets the explorer item nodes.
    fn getMetaItems(&self, manager: &dyn MetaManager) -> io::Result<Vec<MetaItem>> {
        Ok(if self.base.valid() { <dyn MetaManager>::getMetaItems(manager, self) } else { vec![] })
    }
}

// #region ManyArchive

pub struct ManyArchive {
    /// The paths
    pub paths: Vec<String>,
}

impl ManyArchive {
    pub fn new(basis: &dyn Archive, parent: Option<Arc<dyn Archive>>, state: BinaryState, name: &str, paths: Vec<String>, pathSkip: usize) -> Arc<BinaryArchive> {
        let mut s = BinaryArchive::new(parent, state, None);
        s.base.assetFactoryFunc = basis.base().assetFactoryFunc;
        s.base.name = name.to_string();
        s.pathSkip = pathSkip;
        s.useReader.store(false, Ordering::Relaxed);
        s.virt = Some(Box::new(ManyArchive { paths }));
        newArchive(s)
    }
}

impl BinaryArchiveVirtual for ManyArchive {
    fn read<'a>(&'a self, s: &'a BinaryArchive, tag: &'a Tag) -> Task<'a, ()> {
        Box::pin(async move {
            let game = &s.base.game;
            let mut files = Vec::new();
            for p in &self.paths {
                let arc = if game.isArcPath(p) {
                    let state = BinaryState::new(s.base.vfx.clone(), game.clone(), s.base.edition.clone(), Some(p), Tag::None);
                    game.createArchive(Some(s.base.this()), state)?.downcastBinaryArchive()
                } else { None };
                let (vfx, p2) = (s.base.vfx.clone(), p.clone());
                files.push(Arc::new(FileSource {
                    path: p.replace('\\', "/"),
                    arc,
                    lazy: Some(Arc::new(move |x: &mut FileSource| { x.fileSize = vfx.fileInfo(&p2).map_or(0, |z| z.length as i64); x.lazy = None; })),
                    ..Default::default()
                }));
            }
            s.setFiles(Some(files));
            Ok(())
        })
    }

    fn readData<'a>(&'a self, s: &'a BinaryArchive, file: &'a Arc<FileSource>, option: &'a Tag) -> Task<'a, Option<Stream>> {
        Box::pin(async move {
            if let Some(arc) = &file.arc { return arc.readData(file, option).await; }
            let Some(mut data) = s.base.vfx.open(&file.path) else { return Ok(None) };
            let mut z = Vec::new();
            data.read_to_end(&mut z)?;
            Ok(Some(Box::new(Cursor::new(z)) as Stream))
        })
    }
}

// #region MultiArchive

pub struct MultiArchive {
    pub base: Binary,
    /// The paks
    pub archives: Vec<Arc<dyn Archive>>,
}
archiveSource!(MultiArchive);

impl MultiArchive {
    pub fn new(parent: Option<Arc<dyn Archive>>, state: BinaryState, name: &str, archives: Vec<Arc<dyn Archive>>) -> Arc<MultiArchive> {
        let mut base = Binary::new(state);
        base.name = name.to_string();
        newArchive(MultiArchive { base, archives })
    }

    fn findArchives(&self, path: &str) -> io::Result<(Vec<Arc<dyn Archive>>, String)> {
        let paths: Vec<&str> = path.splitn(2, ['\\', '/', ':']).collect();
        if paths.len() == 1 { return Ok((self.archives.clone(), path.to_string())); }
        let (path, next) = (paths[0], paths[1].to_string());
        let archives: Vec<Arc<dyn Archive>> = self.archives.iter().filter(|s| s.base().name.starts_with(path)).cloned().collect();
        for s in &archives { s.open(None, None)?; }
        Ok((archives, next))
    }

    fn findArchive(&self, path: &Tag) -> io::Result<(Arc<dyn Archive>, Tag)> {
        let (archives, next) = match path {
            Tag::None => return Err(Error::new(ErrorKind::InvalidInput, "path")),
            Tag::Str(s) => { let (a, n) = self.findArchives(s)?; (a, Tag::Str(n)) }
            Tag::Int(_) => (self.archives.clone(), path.clone()),
            _ => return Err(Error::new(ErrorKind::InvalidInput, "path")),
        };
        for s in archives { if s.base().valid() && s.contains(&next)? { return Ok((s, next)); } }
        Err(Error::new(ErrorKind::NotFound, format!("Could not find file \"{path:?}\".")))
    }
}

impl Archive for MultiArchive {
    fn base(&self) -> &Binary { &self.base }

    /// Opens this instance.
    fn opening(&self) -> Task<'_, ()> {
        Box::pin(async move {
            let mut count = 0;
            for s in &self.archives { s.open(None, None)?; count += s.base().count(); }
            self.base.valid.store(true, Ordering::Relaxed);
            self.base.count.store(count, Ordering::Relaxed);
            Ok(())
        })
    }

    /// Closes this instance.
    fn closing(&self) -> Task<'_, ()> {
        Box::pin(async move {
            for s in &self.archives { s.close()?; }
            self.base.valid.store(false, Ordering::Relaxed);
            self.base.count.store(0, Ordering::Relaxed);
            Ok(())
        })
    }

    fn contains(&self, path: &Tag) -> io::Result<bool> {
        match path {
            Tag::None => Err(Error::new(ErrorKind::InvalidInput, "path")),
            Tag::Str(s) => { let (archives, next) = self.findArchives(s)?; let next = Tag::Str(next); for s in archives { if s.base().valid() && s.contains(&next)? { return Ok(true); } } Ok(false) }
            Tag::Int(_) => { for s in &self.archives { if s.base().valid() && s.contains(path)? { return Ok(true); } } Ok(false) }
            _ => Err(Error::new(ErrorKind::InvalidInput, "path")),
        }
    }

    fn getSource(&self, path: &Tag, throwOnError: bool) -> io::Result<(Option<Arc<dyn Archive>>, Option<Arc<FileSource>>)> {
        match path {
            Tag::None | Tag::Str(_) | Tag::Int(_) => { let (s, next) = self.findArchive(path)?; s.getSource(&next, throwOnError) }
            _ => Ok((Some(self.base.this()), None)),
        }
    }

    fn getData<'a>(&'a self, path: &'a Tag, option: &'a Tag, throwOnError: bool) -> Task<'a, Option<Stream>> {
        Box::pin(async move { let (s, next) = self.findArchive(path)?; s.getData(&next, option, throwOnError).await })
    }

    fn getAsset<'a>(&'a self, path: &'a Tag, option: &'a Tag, throwOnError: bool) -> Task<'a, Tag> {
        Box::pin(async move { let (s, next) = self.findArchive(path)?; s.getAsset(&next, option, throwOnError).await })
    }

    // #region Metadata

    /// Gets the metadata items.
    fn getMetaItems(&self, manager: &dyn MetaManager) -> io::Result<Vec<MetaItem>> {
        let mut items = Vec::new();
        for s in self.archives.iter().filter(|t| t.base().valid()) {
            items.push(MetaItem::new(Tag::Archive(s.clone()), &s.base().name, manager.packageIcon(), Tag::None, Some(s.clone()), Some(s.getMetaItems(manager)?)));
        }
        Ok(items)
    }
}

// #region ArcBinary

pub trait ArcBinary: Send + Sync {
    /// Reads the asynchronous.
    fn read<'a>(&'a self, source: &'a BinaryArchive, r: Option<&'a mut dyn VfsRead>, tag: &'a Tag) -> Task<'a, ()> { err(ErrorKind::Unsupported, "NotSupported".into()) }
    /// Reads the asynchronous.
    fn readData<'a>(&'a self, source: &'a BinaryArchive, r: Option<&'a mut dyn VfsRead>, file: &'a Arc<FileSource>, option: &'a Tag) -> Task<'a, Option<Stream>> { err(ErrorKind::Unsupported, "NotSupported".into()) }
    /// Writes the asynchronous.
    fn write<'a>(&'a self, source: &'a BinaryArchive, w: Option<&'a mut dyn Write>, tag: &'a Tag) -> Task<'a, ()> { err(ErrorKind::Unsupported, "NotSupported".into()) }
    /// Writes the asynchronous.
    fn writeData<'a>(&'a self, source: &'a BinaryArchive, w: Option<&'a mut dyn Write>, file: &'a Arc<FileSource>, data: Stream, option: &'a Tag) -> Task<'a, ()> { err(ErrorKind::Unsupported, "NotSupported".into()) }
    /// Processes this instance.
    fn process<'a>(&'a self, source: &'a BinaryArchive) -> Task<'a, ()> { ok(()) }
}

impl dyn ArcBinary {
    /// The file
    pub fn stream() -> Arc<dyn ArcBinary> {
        static STREAM: LazyLock<Arc<dyn ArcBinary>> = LazyLock::new(|| Arc::new(PakBinaryCanStream));
        STREAM.clone()
    }

    /// handles an exception.
    pub fn handleException(source: &Tag, option: &Tag, message: &str) {
        Log::info(message);
    }
}

// #region ArcBinary<T>

/// C# `ArcBinary<Self>.SubArchive`: an archive read out of a file of its parent.
pub struct SubArchive {
    file: Option<Arc<FileSource>>,
    parent: Arc<BinaryArchive>,
    r: Mutex<Option<Stream>>,
}

impl SubArchive {
    pub fn new(parent: &Arc<BinaryArchive>, file: Option<Arc<FileSource>>, path: &str, tag: Tag, source: Arc<dyn ArcBinary>) -> Arc<BinaryArchive> {
        let state = BinaryState::new(parent.base.vfx.clone(), parent.base.game.clone(), parent.base.edition.clone(), Some(path), tag);
        let mut s = BinaryArchive::new(Some(parent.base.this()), state, Some(source));
        s.base.assetFactoryFunc = parent.base.assetFactoryFunc;
        s.virt = Some(Box::new(SubArchive { file, parent: parent.clone(), r: Mutex::new(None) }));
        newArchive(s)
    }
}

/// `SubArchive`'s `StaticPool`: the one reader.
struct StaticReader<'a>(MutexGuard<'a, Option<Stream>>);
impl Read for StaticReader<'_> { fn read(&mut self, buf: &mut [u8]) -> io::Result<usize> { self.0.as_mut().unwrap().read(buf) } }
impl Seek for StaticReader<'_> { fn seek(&mut self, pos: SeekFrom) -> io::Result<u64> { self.0.as_mut().unwrap().seek(pos) } }

impl BinaryArchiveVirtual for SubArchive {
    fn opening<'a>(&'a self, s: &'a BinaryArchive) -> Task<'a, ()> {
        Box::pin(async move {
            let file = self.file.as_ref().ok_or_else(|| Error::new(ErrorKind::InvalidData, "NullReferenceException: File"))?;
            let r = self.parent.readData(file, &Tag::None).await?.ok_or_else(|| Error::new(ErrorKind::NotFound, file.path.clone()))?;
            *self.r.lock().unwrap() = Some(r);
            s.baseOpening().await
        })
    }

    fn closing<'a>(&'a self, s: &'a BinaryArchive) -> Task<'a, ()> {
        *self.r.lock().unwrap() = None;
        s.baseClosing()
    }

    fn getReader<'a>(&'a self, s: &'a BinaryArchive, path: Option<&str>) -> io::Result<Option<Box<dyn VfsRead + 'a>>> {
        let r = self.r.lock().unwrap();
        Ok(if r.is_some() { Some(Box::new(StaticReader(r))) } else { None })
    }
}

// #region ITransformAsset

pub trait ITransformAsset: Send + Sync {
    /// Determines whether this instance [can transform file object] the specified transform to.
    fn canTransformAsset(&self, src: &Tag, transformTo: &dyn Archive) -> bool;
    /// Transforms the file object asynchronous.
    fn transformAsset<'a>(&'a self, src: Tag, transformTo: &'a dyn Archive) -> Task<'a, Tag>;
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::sync::atomic::AtomicUsize;

    struct CountFs { opens: Arc<AtomicUsize>, writes: Arc<AtomicUsize> }
    impl FileSystem for CountFs {
        fn glob(&self, _: &str, _: &str) -> Vec<String> { vec![] }
        fn fileExists(&self, path: &str) -> bool { path == "a.bin" }
        fn fileInfo(&self, _: &str) -> Option<openx_vfx::vfx::FileInfo> { None }
        fn open(&self, _: &str) -> Option<Box<dyn VfsRead + '_>> { None }
        fn openOwned(&self, path: &str) -> Option<Box<dyn VfsRead + Send>> {
            if path != "a.bin" { return None; }
            self.opens.fetch_add(1, Ordering::SeqCst);
            Some(Box::new(Cursor::new(b"abc".to_vec())))
        }
        fn openWrite(&self, path: &str) -> Option<Box<dyn VfsWrite + Send>> {
            if path != "a.bin" { return None; }
            self.writes.fetch_add(1, Ordering::SeqCst);
            Some(Box::new(Cursor::new(Vec::<u8>::new())))
        }
    }

    #[test]
    fn readerAndWriterPoolsOpenOnce() {
        let opens = Arc::new(AtomicUsize::new(0));
        let writes = Arc::new(AtomicUsize::new(0));
        let arc = BinaryArchive::new(None, BinaryState::new(Arc::new(CountFs { opens: opens.clone(), writes: writes.clone() }), FamilyGame::empty(), None, Some("a.bin"), Tag::None), None);
        {
            let mut r = arc.getReader(None).unwrap().unwrap();
            let mut buf = [0u8; 3];
            r.read_exact(&mut buf).unwrap();
            assert_eq!(&buf, b"abc");
        }
        {
            let mut r = arc.getReader(None).unwrap().unwrap();
            let mut buf = [0u8; 3];
            r.read_exact(&mut buf).unwrap();
            assert_eq!(&buf, b"abc");
        }
        assert_eq!(opens.load(Ordering::SeqCst), 1);
        {
            arc.getWriter(None).unwrap().write_all(b"xy").unwrap();
        }
        {
            arc.getWriter(None).unwrap().write_all(b"z").unwrap();
        }
        assert_eq!(writes.load(Ordering::SeqCst), 1);
        assert!(arc.getWriter(Some("missing")).is_err());
    }
}
