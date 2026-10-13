# TODO

What's next, in order. Pick from the top, and update this file in the same commit as the work: remove what's done and add what you find. Rules and conventions are in [CLAUDE.md](CLAUDE.md); decisions are in [docs/ADR](docs/ADR/Readme.md).

Last updated 2026-10-12, after the OpenGL engine view (on top of d6111c2).

## Where things stand

- **OpenX's Rust workspace builds clean**: `cargo check --workspace` and `cargo check -p openx --features full` give zero errors and warnings. **GameX's builds clean except `gamex-wb` and the three apps that depend on it through `gamex-all`** (`app-explorer`, `app-client`, `cli`); see section 1.
- Tests: `gamex` 49, `gamex-arkane` 4, `openx-gfx` 24, `openx-gfx-egin` 62, `openx-engine-opengl` 15. Three older tests fail; see section 5.
- **Rust port: 75 files done, 326 still `todo` in `PORT_MAP.tsv`** (partial files count as `todo`). The core model is done, including `store.rs` with every store but Blizzard.
- **Arx Fatalis opens in the Rust Explorer** through GOG, and every file in its seven `.pak`s reads: text, scripts, `.wav`, `.tga`, `.bmp` / `.jpg`, `.ftl` / `.fts` / `.tea`.
- **OpenX's OpenGL engine runs in egui.** `OpenGLEngine` is in the Explorer's engine list (the config picks `GL`), and a "Texture" tab hosts `AppOpenGLControl`: OpenX's `OpenGLControl` in an egui paint callback on eframe's GL context. A scratch harness drew Arx's `.bmp` and `.jpg` textures through it. See PORTING.md, "OpenX's OpenGL engine in the egui Explorer".

## 1. Unblock the GameX build

- [ ] **`gamex-wb` needs over 20 GB of memory to check.** `families/wb/src/formats/ac/props/enums_weenie.rs` is generated: `WeenieClassName` has 31,108 variants with `#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]`. Removing `fromRaw` doesn't help, so it's the enum itself. On a 32 GB machine with other programs running, rustc runs out of memory, and nothing that depends on `gamex-all` builds, including the Explorer. Change what `gen_enums.py` emits for an enum this size (e.g. a `u16` newtype with associated consts and a name table), regenerate, and check the Explorer runs with the Texture view (it hasn't been run since the engine work; the harness has).

## 2. Finish the Rust Explorer's views

- [ ] **Engine view, the rest:** OpenX's `ObjectRenderer` (C# passes the loaded object as the manager's `path`; Rust's `SourcePath` can't carry one), `ParticleRenderer` (needs `Egin_Particle.cs` and egin's scene types), `EngineRenderer` (needs `OpenGLOpenEngine`, which needs `CellManager`), and the other engines' controls in `AppGfxControl.cs`. `ITextureSelect` / `ITextureFrames` aren't reachable through `Tag::Texture`, so frame stepping and `Select` aren't wired.
- [ ] **OpenX's Rust `platform.rs` is still C#'s old `Platform.cs`.** C# renamed it `Engine.cs` (`Engine`, `EngineX`), with `GfxFactory` / `SfxFactory` and `EngineX.Gfx`. Port it, so `OpenGLEngine::gfx()` (the stand-in) goes, and camelCase its snake_case names (`platform_os`, `current_caps`).
- [ ] **Audio player** (`TAudioPlayer`): buttons in egui, playback through OpenX's sfx.
- [ ] **Data grid** (`TDataGrid`): a table with `egui_extras::TableBuilder`.
- [ ] **Family apps:** nothing registers an `AppStartup` yet, so `FamilyApp::openAsync` finds nothing.

## 3. Fill the gaps in OpenX's Rust port

These limit the core model; each is noted in the file headers.

- [ ] **Reader pooling** (C# `GenericPoolX`): archives open a new reader each time.
- [ ] **FileSystem write mode:** `getWriter` returns `Unsupported`.
- [ ] **Network file systems** (`NetworkFileSystem`, `NetworkHost`): http resources return `Unsupported`.
- [ ] **Logging:** `eprintln!` stands in for C#'s `Log`.
- [ ] **TypeRegistry** is parked in `OpenX/rust/core/poly/_/`. Move just that type out and point `registerType` / `findType` at it.
- [ ] **Remaining `DirectoryFileSystem(baseRoot, basePath)` argument and `FileSystem.Next()`.**
- [ ] **`openx-vfx`'s `disc_mds.rs`** is ported but still `PORT-STATUS: partial`.

## 4. Port the GameX families to Rust

- [ ] **Port `Binary_Dds`.** Needs `DDS_HEADER.Read`, commented out in `OpenX/rust/gfx/gfx/src/gfx_texture.rs`. Uncore's other missing types: `Binary_Plist` (needs `formats/apple/plist_reader.rs`), `Binary_Zip` (needs a zip crate), `Binary_Engine` (needs `ICellDatabase`).
- [ ] **Arkane's other games** (Dishonored, Prey, Dark Messiah, Wolfenstein) need Crytek's, Epic's, Valve's and ID's archive binaries and asset factories; `ArkaneArchive::getArcBinary` fails for them.
- [ ] **Register the other families** the same way (`register()` in the family's main file, called from `rust/apps/app-explorer/src/app_xaml.rs`) as each one's archive type is ported.

Families with the most stub files: IW 31, Cig Explorer 25, Uncore 22, Crytek 19, WB 17, Origin 13, Blizzard 13, Epic 11. Most others have 1 to 8. Families that Python already has are the easiest start, because Python shows the camelCase shape.

## 5. Smaller known issues

- [ ] **Python doesn't decode `%20` in file URIs.**
- [ ] **C# OpenGL bugs the port kept** (PORTING.md, "OpenX's OpenGL engine in the egui Explorer"): `ARGB32` uploads as `Rgb` + `UnsignedInt8888Rev`, so RGBA images don't upload; anisotropy is set with the query enum 0x84FF; `TestTriRenderer` draws 6 of 3 vertices; only `plane` / `testtri` shaders are embedded. Owner to decide; fix all three languages together.
- [ ] **Arkane's blast decompression** fails on `.bmp` exports in C# (143 errors). The Rust port of Blast.cs reads all 1,314 of Arx's `.bmp`s, so compare the two.
- [ ] **Python's Bullfrog and Beamdog readers** need porting properly from C#.
- [ ] **6 Rust files still name `Applications/` in their `PORT-SOURCE` header**, as do 6 rows of `rust/_/PORT_MAP.tsv`. C#'s folder is now `dotnet/Apps/`.
- [ ] **`bash rust/_/sync-check.sh` reports 12 STALE `done` files**, where the C# changed after the port: `ImportManager`, `Client`, `DesSer`, `Globalx`, `_Config`, `Collada`, `RootHandlers`, Bethesda `Records`, Gamebryo `Nif`, Uncore `Network`, and WB `Enums` / `Enums+Weenie`. Port each change, then bump its PORT-SHA. It also reports 13 MISSING rows: the 6 `Applications/` ones, `Gamebryo/Formats/Nif.o.cs`, and 6 `GameX.Valve.Platform/OpenGL` files. And it reports 94 C# files with no row, 22 of them in the C#-only `Apps/GameX.App.ExplorerM`.
- [ ] **Some `done` headers cover only enums.** `families/bethesda/src/formats/records.rs` holds just the generated enums of `Records.cs`, which also has about 350 classes. Check the other `gen_enums.py` outputs the same way.
- [ ] **Regenerate the enum files.** `gen_enums.py` now emits `fromRaw` / `toRaw`, but 8 files generated before still have `from_raw` / `to_raw` (`grep -rln from_raw --include=*.rs rust`).
- [ ] **Porting scripts with dead paths:** `rust/_/stamp.py` and `gen_gamex.py` point at `/home/claude/...`. `OpenX/rust/_/sync-check.sh` has no `PORT_MAP.tsv` to read and still defaults to `../src/dotnet`.
- [ ] **Stale `#[cfg(test)]` modules** don't compile, e.g. `openx-vfx`'s `disc_nrg.rs`. Fix or remove them when you touch those files. (`openx-poly`'s `reader.rs` is fixed.)
- [ ] **Three tests fail:** `openx-poly`'s `pool::tests::numeric_strings_convert_like_the_c_sharp` (it never ran before, because `reader.rs`'s tests didn't compile), and `gamex-uncore`'s `network.rs` tests `credential_opcodes_are_redacted` and `non_printable_bytes_become_dots`.
- [ ] **Store.cs looks renamed in three places** (see `rust/_/PORTING.md`, Store.cs): it switches on `"Query"` where the specs say `Archive:` (57 keys, which throw), reads registry values `"ModelPath"` / `"Installed ModelPath"`, and looks in `Ubisoft Query Launcher`. Python has `Archive`, `Path` and `Ubisoft Game Launcher`. Rust follows C#. Owner to decide; fix all three languages together.
- [ ] **`Store_Blizzard` can't read `product.db`** in Rust: `BlizzardProtoDatabase.cs` isn't ported (its `.proto` schema isn't in the repo).
- [ ] **Uncore's `Compression.cs`** has only Blast and Zlib in Rust. Doboz, Lzf, Lzss, Oodle and Xmem wait on `rust/core/gamex/src/vendor/compression/`'s placeholders; Lz4, Lzo, Zstd, Lzma need crates.

## Dates

- **2027-10-11: recheck the Rust UI choice** ([ADR 0006](docs/ADR/0006-review-rust-ui-2027.md)), including whether eframe has moved past glow 0.17.
