// PORT-SOURCE: Core/OpenX.PolyIO/ISource.cs
// PORT-SHA: 4bb05d80da4f6254
// PORT-STATUS: done

use std::any::Any;
use std::fmt;
use std::future::Future;
use std::hash::{Hash, Hasher};
use std::ops::Deref;
use std::pin::Pin;
use std::io::{Error, ErrorKind};
use std::sync::Arc;

/// Boxed future alias — `Task<T>` with no external futures crate.
pub type BoxFuture<'a, T> = Pin<Box<dyn Future<Output = T> + Send + 'a>>;

/// C# `object path`. `Str` and `Int` compare by value. `Obj` is the loaded
/// object itself (the key `ObjectRenderer` passes), and compares by identity,
/// as a C# reference does.
#[derive(Clone)]
pub enum SourcePath {
    Str(String),
    Int(u64),
    Obj(Arc<dyn Any + Send + Sync>),
}
impl From<String> for SourcePath {
    fn from(s: String) -> Self { SourcePath::Str(s) }
}
impl From<&str> for SourcePath {
    fn from(s: &str) -> Self { SourcePath::Str(s.to_string()) }
}
impl From<u64> for SourcePath {
    fn from(v: u64) -> Self { SourcePath::Int(v) }
}
impl PartialEq for SourcePath {
    fn eq(&self, other: &Self) -> bool {
        match (self, other) {
            (SourcePath::Str(a), SourcePath::Str(b)) => a == b,
            (SourcePath::Int(a), SourcePath::Int(b)) => a == b,
            (SourcePath::Obj(a), SourcePath::Obj(b)) => Arc::ptr_eq(a, b),
            _ => false,
        }
    }
}
impl Eq for SourcePath {}
impl Hash for SourcePath {
    fn hash<H: Hasher>(&self, state: &mut H) {
        std::mem::discriminant(self).hash(state);
        match self {
            SourcePath::Str(s) => s.hash(state),
            SourcePath::Int(i) => i.hash(state),
            SourcePath::Obj(o) => (Arc::as_ptr(o) as *const () as usize).hash(state),
        }
    }
}
impl fmt::Debug for SourcePath {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result {
        match self {
            SourcePath::Str(s) => write!(f, "Str({s:?})"),
            SourcePath::Int(i) => write!(f, "Int({i})"),
            SourcePath::Obj(o) => write!(f, "Obj({:p})", Arc::as_ptr(o)),
        }
    }
}

#[derive(Debug, Clone, PartialEq, Eq, Hash)]
pub enum SourceTag { Str(String), Int(u64) }
impl From<String> for SourceTag {
    fn from(s: String) -> Self { SourceTag::Str(s) }
}
impl From<&str> for SourceTag {
    fn from(s: &str) -> Self { SourceTag::Str(s.to_string()) }
}
impl From<u64> for SourceTag {
    fn from(v: u64) -> Self { SourceTag::Int(v) }
}

/// Assets are `Send + Sync` so the managers that cache them can share the
/// loaded value (the C# `tag`) between callers.
pub trait ISource: Send + Sync {
    fn getAssetAny<'a>(&'a self, path: &'a SourcePath, option: Option<&'a (dyn Any + Sync)>) -> BoxFuture<'a, Result<Box<dyn Any + Send + Sync>, Error>>;
}

/// The generic front door — C# `GetAsset<T>`. Blanket-implemented, so every
/// `Source` gets it and it stays out of the object-safe trait.
pub trait ISourceExt: ISource {
    fn getAsset<'a, T: Any + Send + Sync>(&'a self, path: &'a SourcePath, option: Option<&'a (dyn Any + Sync)>) -> BoxFuture<'a, Result<T, Error>> {
        Box::pin(async move {
            let any = self.getAssetAny(path, option).await?;
            any.downcast::<T>().map(|b| *b).map_err(|_| Error::new(ErrorKind::Other, std::any::type_name::<T>()))
        })
    }
}

impl<T: ISource + ?Sized> ISourceExt for T {}

/// A shared handle to an `ISource` that compares and hashes by identity.
///
/// The C# managers key their caches on `(ISource source, object path)`, and a
/// C# reference compares by identity, not by value. `Arc<dyn ISource>` gives
/// the same thing here: two `SourceRef`s are equal when they point at the same
/// source object. Owning the `Arc` also lets a preload future keep its source
/// alive, so the future can be stored without borrowing the caller's lifetime.
#[derive(Clone)]
pub struct SourceRef(pub Arc<dyn ISource>);

impl SourceRef {
    pub fn new<S: ISource + 'static>(source: S) -> Self { Self(Arc::new(source)) }
    fn addr(&self) -> *const () { Arc::as_ptr(&self.0) as *const () }
}
impl From<Arc<dyn ISource>> for SourceRef {
    fn from(source: Arc<dyn ISource>) -> Self { Self(source) }
}
impl Deref for SourceRef {
    type Target = dyn ISource;
    fn deref(&self) -> &Self::Target { &*self.0 }
}
impl PartialEq for SourceRef {
    fn eq(&self, other: &Self) -> bool { self.addr() == other.addr() }
}
impl Eq for SourceRef {}
impl Hash for SourceRef {
    fn hash<H: Hasher>(&self, state: &mut H) { self.addr().hash(state) }
}
impl fmt::Debug for SourceRef {
    fn fmt(&self, f: &mut fmt::Formatter<'_>) -> fmt::Result { write!(f, "SourceRef({:p})", self.addr()) }
}

/// The cache key every manager uses — C# `var key = (source, path)`.
pub type SourceKey = (SourceRef, SourcePath);

pub trait IHaveSource {
    fn source(&self) -> &dyn ISource;
}
