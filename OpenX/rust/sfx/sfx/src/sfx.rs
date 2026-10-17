// PORT-SOURCE: Sfx/OpenX.Sfx/Sfx.cs
// PORT-SHA: 661f48a68e400f2d
// PORT-STATUS: done
//
// Same shape as `TextureManager`: `create(source, path)` loads the asset and
// the builder turns that tag into a backend handle. C# `Create` blocks on
// `Load(...).Result`, which deadlocks a thread with a `SynchronizationContext`
// (WPF, Unity). `create` is async instead, as `TextureManager::create` is, and
// a failed load is the original error rather than an `AggregateException`.
//
// Kept from C#: a failed load leaves its task, so the next `create` fails the
// same way without loading again. `delete` removes the cache entry and leaves
// an in-flight preload. `Debug.Assert` is `debug_assert`.

use std::any::Any;
use std::collections::HashMap;
use std::io::Error;
use std::sync::Arc;
use openx_poly::core::{BoxFuture, SourceKey, SourcePath, SourceRef};

/// C# `interface IOpenSfx`.
pub trait IOpenSfx {}

/// C# `IOpenSfx[]`: each slot is the backend's object as `Any`, as `openx_gfx::gfx::IOpenGfxs`.
pub type IOpenSfxs = Vec<Option<Arc<dyn Any + Send + Sync>>>;

/// C# `object` when the audio handle or the loaded tag is an opaque reference.
pub type AnyArc = Arc<dyn Any + Send + Sync>;

/// The backend's handle type. C# threads it through as the `Audio` type parameter.
pub trait SfxBackend {
    type Audio: Clone;
}

/// C# `abstract class AudioBuilder<B_Audio>`.
pub trait AudioBuilder<B: SfxBackend> {
    /// C# `Create(ISource source, object path)`. `path` is the loaded tag, not the cache key.
    ///
    /// `Result` is C#'s throw: `SystemAudioBuilder` throws `NotImplementedException`,
    /// and the Explorer logs that the way `AudioPlayer.Load` catches.
    fn create(&mut self, source: &SourceRef, path: &(dyn Any + Send + Sync)) -> Result<B::Audio, Error>;

    /// C# `Delete(ISource source, B_Audio audio)`.
    fn delete(&mut self, source: &SourceRef, audio: &B::Audio);
}

type LoadTask = BoxFuture<'static, Result<AnyArc, Error>>;

fn load_any(source: &SourceRef, path: &SourcePath) -> LoadTask {
    let source = source.clone();
    let path = path.clone();
    Box::pin(async move {
        let obj = source.getAssetAny(&path, None).await?;
        Ok(Arc::from(obj))
    })
}

fn key(source: &SourceRef, path: &SourcePath) -> SourceKey { (source.clone(), path.clone()) }

/// C# `class AudioManager<Audio>`.
pub struct AudioManager<B: SfxBackend, AB: AudioBuilder<B>> {
    builder: AB,
    cached: HashMap<SourceKey, (B::Audio, AnyArc)>,
    tasks: HashMap<SourceKey, LoadTask>,
}

impl<B: SfxBackend, AB: AudioBuilder<B>> AudioManager<B, AB> {
    pub fn new(builder: AB) -> Self {
        Self { builder, cached: HashMap::new(), tasks: HashMap::new() }
    }

    /// C# `Create(ISource source, object path)`.
    pub async fn create(&mut self, source: &SourceRef, path: &SourcePath) -> Result<(B::Audio, AnyArc), Error> {
        let key = key(source, path);
        if let Some(c) = self.cached.get(&key) { return Ok(c.clone()); }
        let tag = self.load(&key).await?;
        let obj = self.builder.create(source, &*tag)?;
        self.cached.insert(key, (obj.clone(), tag.clone()));
        Ok((obj, tag))
    }

    /// C# `Preload(ISource source, object path)`.
    pub fn preload(&mut self, source: &SourceRef, path: &SourcePath) {
        let key = key(source, path);
        if self.cached.contains_key(&key) { return; }
        self.tasks.entry(key).or_insert_with(|| load_any(source, path));
    }

    /// C# `Delete(ISource source, object path)`. An in-flight preload is left in `tasks`.
    pub fn delete(&mut self, source: &SourceRef, path: &SourcePath) {
        let key = key(source, path);
        let Some((aud, _)) = self.cached.remove(&key) else { return; };
        self.builder.delete(source, &aud);
    }

    async fn load(&mut self, key: &SourceKey) -> Result<AnyArc, Error> {
        debug_assert!(!self.cached.contains_key(key));
        self.preload(&key.0, &key.1);
        let task = self.tasks.remove(key).expect("preload registers the task");
        match task.await {
            Ok(obj) => Ok(obj),
            Err(e) => {
                // C# removes the task only after `await` returns. A fault stays, and the next load awaits it again.
                let (kind, msg) = (e.kind(), e.to_string());
                self.tasks.insert(key.clone(), Box::pin(std::future::ready(Err(Error::new(kind, msg.clone())))));
                Err(Error::new(kind, msg))
            }
        }
    }
}

/// C# `interface IOpenSfx<Audio> : IOpenSfx`.
pub trait IOpenSfxAudio<B: SfxBackend>: IOpenSfx {
    type Builder: AudioBuilder<B>;
    fn audioManager(&mut self) -> &mut AudioManager<B, Self::Builder>;
    fn createAudio<'a>(&'a mut self, source: &'a SourceRef, path: &'a SourcePath) -> BoxFuture<'a, Result<(B::Audio, Option<AnyArc>), Error>>;
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::future::Future;
    use std::io::ErrorKind;
    use std::sync::atomic::{AtomicU32, Ordering};
    use std::task::{Context, Poll, RawWaker, RawWakerVTable, Waker};
    use openx_poly::core::ISource;

    fn block_on<F: Future>(f: F) -> F::Output {
        fn raw() -> RawWaker { RawWaker::new(std::ptr::null(), &VTABLE) }
        static VTABLE: RawWakerVTable = RawWakerVTable::new(|_| raw(), |_| {}, |_| {}, |_| {});
        let waker = unsafe { Waker::from_raw(raw()) };
        let mut cx = Context::from_waker(&waker);
        let mut f = std::pin::pin!(f);
        loop {
            if let Poll::Ready(v) = f.as_mut().poll(&mut cx) { return v; }
        }
    }

    struct Test;
    impl SfxBackend for Test { type Audio = u32; }

    #[derive(Default)]
    struct Counting { next: u32, creates: u32, deletes: u32 }

    impl AudioBuilder<Test> for Counting {
        fn create(&mut self, _source: &SourceRef, path: &(dyn Any + Send + Sync)) -> Result<u32, Error> {
            assert!(path.downcast_ref::<String>().is_some(), "the builder receives the loaded tag");
            self.next += 1;
            self.creates += 1;
            Ok(self.next)
        }
        fn delete(&mut self, _source: &SourceRef, _audio: &u32) { self.deletes += 1; }
    }

    struct Boom;
    impl AudioBuilder<Test> for Boom {
        fn create(&mut self, _source: &SourceRef, _path: &(dyn Any + Send + Sync)) -> Result<u32, Error> {
            Err(Error::new(ErrorKind::Unsupported, "NotImplementedException"))
        }
        fn delete(&mut self, _source: &SourceRef, _audio: &u32) {}
    }

    struct FakeSource { loads: Arc<AtomicU32> }
    impl ISource for FakeSource {
        fn getAssetAny<'a>(&'a self, path: &'a SourcePath, _option: Option<&'a (dyn Any + Sync)>) -> BoxFuture<'a, Result<Box<dyn Any + Send + Sync>, Error>> {
            self.loads.fetch_add(1, Ordering::Relaxed);
            let path = path.clone();
            Box::pin(async move {
                match &path {
                    SourcePath::Str(s) if s.ends_with(".wav") => Ok(Box::new(s.clone()) as Box<dyn Any + Send + Sync>),
                    _ => Err(Error::new(ErrorKind::NotFound, format!("{path:?}"))),
                }
            })
        }
    }

    fn source() -> (SourceRef, Arc<AtomicU32>) {
        let loads = Arc::new(AtomicU32::new(0));
        (SourceRef::new(FakeSource { loads: loads.clone() }), loads)
    }

    #[test]
    fn cacheHitsBySourceAndPath() {
        let (src, loads) = source();
        let (other, _) = source();
        let mut m = AudioManager::<Test, _>::new(Counting::default());
        let (a, tag) = block_on(m.create(&src, &"shot.wav".into())).unwrap();
        let (b, _) = block_on(m.create(&src, &"shot.wav".into())).unwrap();
        assert_eq!(a, b);
        assert_eq!(tag.downcast_ref::<String>().unwrap(), "shot.wav");
        assert_eq!(m.builder.creates, 1);
        assert_eq!(loads.load(Ordering::Relaxed), 1);
        block_on(m.create(&other, &"shot.wav".into())).unwrap();
        assert_eq!(m.builder.creates, 2, "the key includes the source");
    }

    #[test]
    fn deleteReleasesTheHandle() {
        let (src, _) = source();
        let mut m = AudioManager::<Test, _>::new(Counting::default());
        block_on(m.create(&src, &"a.wav".into())).unwrap();
        m.delete(&src, &"a.wav".into());
        assert_eq!(m.builder.deletes, 1);
        m.delete(&src, &"a.wav".into());
        assert_eq!(m.builder.deletes, 1);
        block_on(m.create(&src, &"a.wav".into())).unwrap();
        assert_eq!(m.builder.creates, 2);
    }

    #[test]
    fn preloadDoesNotReloadACachedPath() {
        let (src, loads) = source();
        let mut m = AudioManager::<Test, _>::new(Counting::default());
        block_on(m.create(&src, &"a.wav".into())).unwrap();
        m.preload(&src, &"a.wav".into());
        assert_eq!(loads.load(Ordering::Relaxed), 1);
    }

    #[test]
    fn failedLoadStaysWedged() {
        let (src, loads) = source();
        let mut m = AudioManager::<Test, _>::new(Counting::default());
        assert!(block_on(m.create(&src, &"missing.ogg".into())).is_err());
        assert!(block_on(m.create(&src, &"missing.ogg".into())).is_err());
        assert_eq!(loads.load(Ordering::Relaxed), 1, "the faulted task is awaited again");
        assert_eq!(m.builder.creates, 0);
    }

    #[test]
    fn builderErrorIsNotCached() {
        let (src, loads) = source();
        let mut m = AudioManager::<Test, _>::new(Boom);
        let err = block_on(m.create(&src, &"a.wav".into())).unwrap_err();
        assert!(err.to_string().contains("NotImplementedException"));
        assert!(block_on(m.create(&src, &"a.wav".into())).is_err());
        assert_eq!(loads.load(Ordering::Relaxed), 2, "Create throws before the cache write, so the next call loads again");
    }
}
