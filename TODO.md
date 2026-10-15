# TODO

What's next, in order. Pick from the top, and update this file in the same commit as the work: remove what's done and add what you find. Rules and conventions are in [CLAUDE.md](CLAUDE.md); decisions are in [docs/ADR](docs/ADR/Readme.md).

Last updated 2026-10-15, after bringing Python's Arkane, Engine.cs and Store.cs up to C# (on top of 89189fb).

## Where things stand

- **Both Rust workspaces build clean**: `cargo check --workspace` in `OpenX/rust` and `rust/`, and `cargo check -p openx --features full`, give zero errors and warnings, with rustc under 1 GB. (`gamex-wb` needed over 20 GB until its big enums became `ConstEnum`s; see PORTING.md.)
- Tests: `gamex` 49, `gamex-arkane` 4, `openx-gfx` 24, `openx-gfx-egin` 62, `openx-engine-opengl` 15, `openx-poly` 18. Three older tests fail; see section 5.
- **Rust port: 75 files marked `done`, 326 still `todo` in `PORT_MAP.tsv`** (partial files count as `todo`). 29 of the 75 are placeholders whose header says "NOT PORTED" (section 5), so 46 are really ported. The core model is done, including `store.rs` with every store but Blizzard.
- **Arx Fatalis opens in the Rust Explorer** through GOG, and every file in its seven `.pak`s reads: text, scripts, `.wav`, `.tga`, `.bmp` / `.jpg`, `.ftl` / `.fts` / `.tea`.
- **OpenX's engines are C#'s `Engine` / `EngineX`** (`openx::engine`), and the Explorer lists C#'s engines: OpenGL draws; the others are registered without a backend.
- **The Explorer shows Arx's textures.** `OpenGLEngine` is in the engine list (the config picks `GL`), and a "Texture" tab hosts `AppOpenGLControl`: OpenX's `OpenGLControl` in an egui paint callback on eframe's GL context. Checked on `data.pak:GRAPH/particles/Boom.bmp` with the config's `forcePath` set to `sample:1` for the run. See PORTING.md, "OpenX's OpenGL engine in the egui Explorer".

## Remaining work at a glance

| Work | Language | Size | Section |
| -- | -- | -- | -- |
| Explorer: rest of the engine view (objects, particles, engines), audio player, data grid, family apps | Rust | large | 1 |
| OpenX gaps: reader pooling, file system writes, network file systems, logging, `TypeRegistry`, `disc_mds` | Rust | medium | 2 |
| Uncore formats: `Binary_Dds`, `Binary_Plist`, `Binary_Zip`, `Binary_Engine` | Rust | medium | 3 |
| Arkane's other games (needs Crytek, Epic, Valve and ID archive binaries) | Rust | large | 3 |
| The other families: 326 `todo` files, each family registered as it lands | Rust | very large | 3 |
| Uncore formats: `Binary_Iif`, `Pal`, `Plist`, `Raw`, `Xga`, BMP headers (9 Arx `.bmp`s fail) | Python | medium | 4 |
| OpenX graphics: `Colorf` / `Color32`, `RenderPrimitiveType`, `OpenGLObjectBuilder`, `MeshSceneNode` | Python | small | 4 |
| Core: `EmptyArchive`, `IUnknown.cs`, `Stream.cs`, `FamilyManager` / `ShellState`, `Util.cs` | Python | medium | 4 |
| Python always activates `TestEngine` | Python | small | 4 |
| 29 `done` rows that are empty placeholders (Collada, compression, `DesSer`, `Manager`, ...) | Rust | large | 5 |
| 11 STALE `done` files and 13 missing rows from `sync-check.sh` | Rust | medium | 5 |
| Enum files still on `from_raw`; enum-only `done` headers | Rust | small | 5 |
| C# OpenGL bugs the ports kept | all three, owner decides | small | 5 |
| Blast fails on `.bmp` exports | C# | small | 5 |
| Bullfrog and Beamdog readers; `%20` in file URIs | Python | medium | 5 |
| Failing and stale tests; scripts with dead paths | Rust, scripts | small | 5 |

## 1. Finish the Rust Explorer's views

- [ ] **Engine view, the rest:** OpenX's `ObjectRenderer` (C# passes the loaded object as the manager's `path`; Rust's `SourcePath` can't carry one), `ParticleRenderer` (needs `Egin_Particle.cs` and egin's scene types), `EngineRenderer` (needs `OpenGLOpenEngine`, which needs `CellManager`), and the other engines' controls in `AppGfxControl.cs`. `ITextureSelect` / `ITextureFrames` aren't reachable through `Tag::Texture`, so frame stepping and `Select` aren't wired.
- [ ] **Audio player** (`TAudioPlayer`): buttons in egui, playback through OpenX's sfx. Port `Engine_System`'s `SystemSfx` / `SystemAudioBuilder` first (`OpenX/rust/core/openx/src/engine_system/`), with openx-sfx's `AudioManager` / `AudioBuilder` brought to C#'s shape (built from `(ISource, path)`), and give `OpenGLEngine`'s `sfxFactory` its `SystemSfx`.
- [ ] **Data grid** (`TDataGrid`): a table with `egui_extras::TableBuilder`.
- [ ] **Family apps:** nothing registers an `AppStartup` yet, so `FamilyApp::openAsync` finds nothing.

## 2. Fill the gaps in OpenX's Rust port

These limit the core model; each is noted in the file headers.

- [ ] **Reader pooling** (C# `GenericPoolX`): archives open a new reader each time.
- [ ] **FileSystem write mode:** `getWriter` returns `Unsupported`.
- [ ] **Network file systems** (`NetworkFileSystem`, `NetworkHost`): http resources return `Unsupported`.
- [ ] **Logging:** `eprintln!` stands in for C#'s `Log`.
- [ ] **TypeRegistry** is parked in `OpenX/rust/core/poly/_/`. Move just that type out and point `registerType` / `findType` at it.
- [ ] **Remaining `DirectoryFileSystem(baseRoot, basePath)` argument and `FileSystem.Next()`.**
- [ ] **`openx-vfx`'s `disc_mds.rs`** is ported but still `PORT-STATUS: partial`.

## 3. Port the GameX families to Rust

- [ ] **Port `Binary_Dds`.** Needs `DDS_HEADER.Read`, commented out in `OpenX/rust/gfx/gfx/src/gfx_texture.rs`. Uncore's other missing types: `Binary_Plist` (needs `formats/apple/plist_reader.rs`), `Binary_Zip` (needs a zip crate), `Binary_Engine` (needs `ICellDatabase`).
- [ ] **Arkane's other games** (Dishonored, Prey, Dark Messiah, Wolfenstein) need Crytek's, Epic's, Valve's and ID's archive binaries and asset factories; `ArkaneArchive::getArcBinary` fails for them.
- [ ] **Register the other families** the same way (`register()` in the family's main file, called from `rust/apps/app-explorer/src/app_xaml.rs`) as each one's archive type is ported.

Families with the most stub files: IW 31, Cig Explorer 25, Uncore 22, Crytek 19, WB 17, Origin 13, Blizzard 13, Epic 11. Most others have 1 to 8. Families that Python already has are the easiest start, because Python shows the camelCase shape.

## 4. Bring Python up to what Rust has

C# files Rust has ported whose Python twin is missing or behind (ADR 0001). Python's Arkane, `Engine.cs` and `Store.cs` are done: every Arx file Python reads parses, except the nine `.bmp`s below.

- [ ] **Uncore `Formats/Binary.cs`:** Python lacks `Binary_Iif`, `Binary_Pal`, `Binary_Plist`, `Binary_Raw` and `Binary_Xga`, and `Binary_Img` lacks C#'s `BmpHeader` / `BmpInfoHeader`. Nine Arx `.bmp`s (e.g. `data.pak:GRAPH/particles/_Flare.bmp`) fail in `Raster.blitByPalette` with "list index out of range". `Compression.cs` is a few free functions in Python.
- [ ] **OpenX graphics:** `Gfx_Render.cs`'s `Colorf` / `Color32`, `OpenGL.cs`'s `RenderPrimitiveType` / `ToOpenTK`, `Engine_OpenGL.cs`'s `OpenGLObjectBuilder`, and `Gl_Render.cs`'s `MeshSceneNode` / `ParticleControllerFactory`. (`OpenGL_Render.cs`'s particle, sprite and trail renderers are missing in Rust too.)
- [ ] **Core:** `Binary.cs`'s `EmptyArchive`, `Formats/IUnknown.cs` (6 lines of 166), `Formats/Stream.cs` (no twin), `Family.cs`'s `FamilyManager` / `ShellState`, most of `Util.cs`. (`DesSer.cs`'s JSON converters, Collada, the compression libraries and Casc's small files aren't in Rust either: their `done` rows are placeholders, section 5.)
- [ ] **Python always activates `TestEngine`:** `EngineX.inTestHost` is `'unittest' in sys.modules`, and `openx.core.core` imports `unittest`, so it is always true.
- [ ] **Store.cs's per-OS roots differ:** Python guesses Linux/macOS paths for Ubisoft and the Abandon/Archive roots, where C# has `["??"]` and `E:\...` on every OS.

## 5. Smaller known issues

- [ ] **Python doesn't decode `%20` in file URIs.**
- [ ] **C# OpenGL bugs the port kept** (PORTING.md, "OpenX's OpenGL engine in the egui Explorer"): `ARGB32` uploads as `Rgb` + `UnsignedInt8888Rev`, so RGBA images don't upload; anisotropy is set with the query enum 0x84FF; `TestTriRenderer` draws 6 of 3 vertices; only `plane` / `testtri` shaders are embedded. Owner to decide; fix all three languages together.
- [ ] **Arkane's blast decompression** fails on `.bmp` exports in C# (143 errors). The Rust port of Blast.cs reads all 1,314 of Arx's `.bmp`s, so compare the two.
- [ ] **Python's Bullfrog and Beamdog readers** need porting properly from C#.
- [ ] **6 Rust files still name `Applications/` in their `PORT-SOURCE` header**, as do 6 rows of `rust/_/PORT_MAP.tsv`. C#'s folder is now `dotnet/Apps/`.
- [ ] **`bash rust/_/sync-check.sh` reports 11 STALE `done` files**, where the C# changed after the port: `ImportManager`, `Client`, `DesSer`, `Globalx`, `_Config`, `Collada`, `RootHandlers`, Bethesda `Records`, Gamebryo `Nif`, Uncore `Network`, and WB `Enums` (its 18 classes aren't ported; its enums are current). Port each change, then bump its PORT-SHA. It also reports 13 MISSING rows: the 6 `Applications/` ones, `Gamebryo/Formats/Nif.o.cs`, and 6 `GameX.Valve.Platform/OpenGL` files. And it reports 94 C# files with no row, 22 of them in the C#-only `Apps/GameX.App.ExplorerM`.
- [ ] **29 `done` rows are empty placeholders** whose header says "NOT PORTED": `App/ImportManager`, `App/ManifestManager`, `Assembly`, `BlizzardProtoDatabase`, `DesSer`, `Manager`, `Transforms/UnknownTransform`, the six `_LIB/Collada` files, `_LIB/Compression`'s Doboz (3), `Lzf`, `Lzo`, `Lzss`, `OodleLZ`, `Salsa20` and `XCompress`, Casc's `BackgroundWorkerEx`, `CdnCacheStats`, `Logger`, `PerfCounter` and `Salsa20`, and Uncore's `_LIB/System.IO.Compression` and `System.Security.Cryptography`. Mark them `todo` in `PORT_MAP.tsv`, or port them.
- [ ] **Some `done` headers cover only enums.** `families/bethesda/src/formats/records.rs` holds just the generated enums of `Records.cs`, which also has about 350 classes. Check the other `gen_enums.py` outputs the same way.
- [ ] **Regenerate the other enum files.** WB's two are regenerated (camelCase `fromRaw` / `toRaw`, LF, `ConstEnum` for the big ones). `casc/root_handlers.rs`, Bethesda `records.rs`, Gamebryo `nif.rs` / `nif_o.rs` and Stardew Valley's `game_data.rs` still have `from_raw` / `to_raw`; regenerate them and set their headers by hand (the generator writes `PORT-SHA: PLACEHOLDER`), keeping the old hash where the C# file has more than enums.
- [ ] **`Graph/interface/misc/quit1.bmp` doesn't decode**: the `image` crate says "The image format could not be determined" (it's Arx sample 1's first path, outside the `.pak`s). Compare with what System.Drawing makes of it.
- [ ] **Porting scripts with dead paths:** `rust/_/stamp.py` and `gen_gamex.py` point at `/home/claude/...`. `OpenX/rust/_/sync-check.sh` has no `PORT_MAP.tsv` to read and still defaults to `../src/dotnet`.
- [ ] **Stale `#[cfg(test)]` modules** don't compile, e.g. `openx-vfx`'s `disc_nrg.rs`. Fix or remove them when you touch those files. (`openx-poly`'s `reader.rs` is fixed.)
- [ ] **Three tests fail:** `openx-poly`'s `pool::tests::numeric_strings_convert_like_the_c_sharp` (it never ran before, because `reader.rs`'s tests didn't compile), and `gamex-uncore`'s `network.rs` tests `credential_opcodes_are_redacted` and `non_printable_bytes_become_dots`.
- [ ] **`Store_Blizzard` can't read `product.db`** in Rust: `BlizzardProtoDatabase.cs` isn't ported (its `.proto` schema isn't in the repo).
- [ ] **Uncore's `Compression.cs`** has only Blast and Zlib in Rust. Doboz, Lzf, Lzss, Oodle and Xmem wait on `rust/core/gamex/src/vendor/compression/`'s placeholders; Lz4, Lzo, Zstd, Lzma need crates.

## Dates

- **2027-10-11: recheck the Rust UI choice** ([ADR 0006](docs/ADR/0006-review-rust-ui-2027.md)), including whether eframe has moved past glow 0.17.
