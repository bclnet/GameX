// PORT-SOURCE: Core/OpenX/Engine_System/Platform_System.cs
// PORT-SHA: 7f060198da5dd7c1
// PORT-STATUS: done
//
// `SystemAudioBuilder` throws `NotImplementedException` in C#. `create` returns
// that as `Err` so the Explorer can log it the way `AudioPlayer.Load` catches;
// `delete` panics, as the other throw-only members do. `CreateAudio` is
// `(_audioManager.Create(source, path), null)`: the manager's pair is boxed
// into the audio slot and the tag is null. The manager's `create` is async
// instead of `.Result` (see `openx_sfx::sfx`).

use std::any::Any;
use std::io::{Error, ErrorKind};
use std::sync::Arc;
use openx_poly::core::{BoxFuture, SourcePath, SourceRef};
use openx_sfx::sfx::{AnyArc, AudioBuilder, AudioManager, IOpenSfx, IOpenSfxAudio, SfxBackend};

// #region Platform

/// C# `AudioBuilder<object>`.
pub struct SystemBackend;
impl SfxBackend for SystemBackend { type Audio = AnyArc; }

/// SystemAudioBuilder
pub struct SystemAudioBuilder;

impl AudioBuilder<SystemBackend> for SystemAudioBuilder {
    fn create(&mut self, _source: &SourceRef, _path: &(dyn Any + Send + Sync)) -> Result<AnyArc, Error> {
        Err(Error::new(ErrorKind::Unsupported, "NotImplementedException"))
    }
    fn delete(&mut self, _source: &SourceRef, _audio: &AnyArc) { unimplemented!() }
}

/// SystemSfx
pub struct SystemSfx {
    audioManager: AudioManager<SystemBackend, SystemAudioBuilder>,
}

impl SystemSfx {
    pub fn new() -> Self { Self { audioManager: AudioManager::new(SystemAudioBuilder) } }

    /// C# `(_audioManager.Create(source, path), null)`.
    pub fn createAudio<'a>(&'a mut self, source: &'a SourceRef, path: &'a SourcePath) -> BoxFuture<'a, Result<(AnyArc, Option<AnyArc>), Error>> {
        Box::pin(async move {
            let pair = self.audioManager.create(source, path).await?;
            // C# boxes the pair to `object`. The cast is the boxing; inference will not do it.
            let aud: AnyArc = Arc::new(pair);
            Ok((aud, None))
        })
    }
}

impl IOpenSfx for SystemSfx {}

impl IOpenSfxAudio<SystemBackend> for SystemSfx {
    type Builder = SystemAudioBuilder;
    fn audioManager(&mut self) -> &mut AudioManager<SystemBackend, SystemAudioBuilder> { &mut self.audioManager }
    fn createAudio<'a>(&'a mut self, source: &'a SourceRef, path: &'a SourcePath) -> BoxFuture<'a, Result<(AnyArc, Option<AnyArc>), Error>> {
        SystemSfx::createAudio(self, source, path)
    }
}

// #endregion

#[cfg(test)]
mod tests {
    use super::*;
    use std::future::Future;
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

    struct OkSource;
    impl ISource for OkSource {
        fn getAssetAny<'a>(&'a self, _path: &'a SourcePath, _option: Option<&'a (dyn Any + Sync)>) -> BoxFuture<'a, Result<Box<dyn Any + Send + Sync>, Error>> {
            Box::pin(async { Ok(Box::new(()) as Box<dyn Any + Send + Sync>) })
        }
    }

    #[test]
    fn createAudioThrowsNotImplemented() {
        let src = SourceRef::new(OkSource);
        let mut sfx = SystemSfx::new();
        let err = block_on(sfx.createAudio(&src, &"a.wav".into())).unwrap_err();
        assert!(err.to_string().contains("NotImplementedException"));
        // The builder throws before the cache write, so a second call throws again.
        assert!(block_on(IOpenSfxAudio::createAudio(&mut sfx, &src, &"a.wav".into())).is_err());
    }
}
