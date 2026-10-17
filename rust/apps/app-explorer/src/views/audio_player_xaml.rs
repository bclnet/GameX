// PORT-SOURCE: Apps/GameX.App.Explorer/Views/AudioPlayer.xaml.cs
// PORT-SHA: 0ff23c031b49053d
// PORT-STATUS: partial
//
// The button is C#'s Play / Pause. Playback goes through the current engine's
// `SystemSfx` (`AudioManager.create(source, path)`), not NAudio. `SystemAudioBuilder`
// throws `NotImplementedException`, so Play logs that and the label stays "Play".
// Not ported: `WaveOutEvent`, `WaveFileReader`, `Mp3FileReader`,
// `StreamMediaFoundationReader`, `PlaybackStopped`, and disposing those objects.
// `Unloaded` is Drop, and nothing here owns a device.

use std::future::Future;
use std::io::{Error, ErrorKind};
use std::task::{Context, Poll, Wake, Waker};
use std::sync::{Arc, Mutex};
use eframe::egui;
use gamex::meta::{MetaItem, Tag};
use openx::engine::EngineX;
use openx::engine_system::platform_system::SystemSfx;
use openx_poly::core::{SourcePath, SourceRef};

/// C# `task.Result`.
fn result<F: Future>(f: F) -> F::Output {
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

/// AudioPlayer
pub struct AudioPlayer {
    source: Option<SourceRef>,
    path: Option<SourcePath>,
    label: String,
    playing: bool,
}

impl AudioPlayer {
    pub fn new(source: Option<SourceRef>, item: Option<&MetaItem>, value: &Tag, format: &Tag) -> Self {
        // C# `Load`: a format and a stream, then a reader for .wav / .mp3 / .aac.
        if let (Some(format), false) = (format.asStr(), value.isNone()) {
            if !matches!(format.to_lowercase().as_str(), ".wav" | ".mp3" | ".aac") {
                eprintln!("ArgumentOutOfRangeException: Format: {format}");
            }
        }
        let path = item.and_then(|i| i.source.asFileSource().map(|f| SourcePath::from(f.path.as_str())));
        Self { source, path, label: "Play".into(), playing: false }
    }

    /// C# `Play_Click`. `WaveOut.Play` is `SystemSfx.createAudio`.
    fn playClick(&mut self) {
        if self.playing {
            self.playing = false;
            self.label = "Play".into();
            return;
        }
        if let Err(e) = self.playThroughSfx() {
            eprintln!("{e}");
            return;
        }
        self.playing = true;
        self.label = "Pause".into();
    }

    fn playThroughSfx(&mut self) -> Result<(), Error> {
        let Some(source) = &self.source else { return Err(Error::new(ErrorKind::NotFound, "no source")); };
        let Some(path) = &self.path else { return Err(Error::new(ErrorKind::NotFound, "no path")); };
        let Some(slot) = EngineX::Sfx().as_deref().and_then(|s| s.first().and_then(|z| z.clone())) else {
            return Err(Error::new(ErrorKind::InvalidData, "InvalidCastException: Sfx[0]"));
        };
        let sfx = slot.downcast::<Mutex<SystemSfx>>().map_err(|_| Error::new(ErrorKind::InvalidData, "InvalidCastException: Sfx[0]"))?;
        let mut sfx = sfx.lock().unwrap_or_else(|p| p.into_inner());
        result(sfx.createAudio(source, path))?;
        Ok(())
    }

    pub fn ui(&mut self, ui: &mut egui::Ui) {
        if ui.add_sized(egui::vec2(100.0, 25.0), egui::Button::new(&self.label)).clicked() {
            self.playClick();
        }
    }
}
