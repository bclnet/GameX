//! `openx-sfx-ogg` — 1:1 mapping of .NET project `OpenX.Sfx.Ogg`.
//!
//! Not ported: the C# is 148 `DllImport`s over native libogg/libvorbis, not a
//! decoder. Use `lewton` or `symphonia` inside an `AudioBuilder::create`.
//! `AudioManager::create` loads from `(ISource, path)`. See the module for the full rationale.

pub mod sfx_ogg_vorbis;
