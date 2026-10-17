# TODO

What's next, in order. Pick from the top, and update this file in the same commit as the work: remove what's done and add what you find. Rules and conventions are in [CLAUDE.md](CLAUDE.md); decisions are in [docs/ADR](docs/ADR/Readme.md).

Last updated 2026-10-17, after `Binary_Zip` (section 3). `Binary_Dds` and `Binary_Plist` are in.

## Where things stand

- **Both Rust workspaces build clean**: `cargo check --workspace` in `OpenX/rust` and `rust/`, and `cargo check -p openx --features full`, give zero errors and warnings, with rustc under 1 GB. (`gamex-wb` needed over 20 GB until its big enums became `ConstEnum`s; see PORTING.md.)
- Tests: `gamex` 51, `gamex-arkane` 4, `openx-gfx` 27, `openx-gfx-egin` 65, `openx-engine-opengl` 23, `openx-sfx` 5, `openx-poly` 35 (34 pass), `openx-vfx` 175 (174 pass), `gamex-app-explorer` 7, `gamex-cig-explorer` 1. The failures are in section 5.
- **Rust port: 100 files marked `done`, 301 still `todo` in `PORT_MAP.tsv`** (partial files count as `todo`). 29 of the 100 are placeholders whose header says "NOT PORTED" (section 5), so 71 are really ported. The core model is done, including `store.rs` with every store but Blizzard.
- **Arx Fatalis opens in the Rust Explorer** through GOG, and every file in its seven `.pak`s reads: text, scripts, `.wav`, `.tga`, `.bmp` / `.jpg`, `.ftl` / `.fts` / `.tea`.
- **OpenX's engines are C#'s `Engine` / `EngineX`** (`openx::engine`), and the Explorer lists C#'s engines: OpenGL draws; the others are registered without a backend.
- **The Explorer shows Arx's textures.** `OpenGLEngine` is in the engine list (the config picks `GL`), and a "Texture" tab hosts `AppOpenGLControl`: OpenX's `OpenGLControl` in an egui paint callback on eframe's GL context. Checked on `data.pak:GRAPH/particles/Boom.bmp` with the config's `forcePath` set to `sample:1` for the run. See PORTING.md, "OpenX's OpenGL engine in the egui Explorer".

## Remaining work at a glance

| Work | Language | Size | Section |
| -- | -- | -- | -- |
| `disc_mds` mount (`LoadMdsToDisc` still needs `Disc` / `Blob_Raw`) | Rust | small | 2 |
| Uncore formats: `Binary_Engine` | Rust | medium | 3 |
| Arkane's other games (needs Crytek, Epic, Valve and ID archive binaries) | Rust | large | 3 |
| The other families: 301 `todo` files, each family registered as it lands | Rust | very large | 3 |
| Uncore formats: `Binary_Iif`, `Pal`, `Plist`, `Raw`, `Xga`, BMP headers (9 Arx `.bmp`s fail) | Python | medium | 4 |
| OpenX graphics: `Colorf` / `Color32`, `RenderPrimitiveType`, `OpenGLObjectBuilder`, `MeshSceneNode` | Python | small | 4 |
| Core: `EmptyArchive`, `IUnknown.cs`, `Stream.cs`, `FamilyManager` / `ShellState`, `Util.cs` | Python | medium | 4 |
| Python always activates `TestEngine` | Python | small | 4 |
| 29 `done` rows that are empty placeholders (Collada, compression, `DesSer`, `Manager`, ...) | Rust | large | 5 |
| 11 STALE `done` files and 13 missing rows from `sync-check.sh` | Rust | medium | 5 |
| Enum files still on `from_raw`; enum-only `done` headers | Rust | small | 5 |
| `ConstEnum` names as one string plus offsets (smaller, faster builds) | Rust | small | 5 |
| WB's `Properties.cs` | Rust, Python | medium | 5 |
| C# OpenGL bugs the ports kept | all three, owner decides | small | 5 |
| Blast fails on `.bmp` exports | C# | small | 5 |
| Bullfrog and Beamdog readers; `%20` in file URIs | Python | medium | 5 |
| Failing and stale tests; scripts with dead paths | Rust, scripts | small | 5 |

## 1. Finish the Rust Explorer's views

- [x] **Engine view, the rest.** `ObjectRenderer` passes the loaded object as `SourcePath::Obj`. Nothing is registered in `BuildersByType` (`Binary_Nif` / `OpenGLNifObjectBuilder`), so an Object tab opens and `Start` logs the missing builder. Texture frames step through `ITexture`'s default methods (`Tag::Texture` can't see `ITextureFrames`); `Select(0)` runs after the upload. Frame delay is a float, so each `deltaTime` accumulates. The Particle tab builds the outer `ParticleRenderer`, which has no draw in C# either. `ChildRenderer`, the factory, sprites and trails are ported; sprite and trail shaders are still not embedded (section 5), and texture sequences stay unused because the texture tag is an `ITexture`. `EngineRenderer` needs an `Arc<dyn CellDatabase>` inside the `Any`. Nothing produces one until `Binary_Engine` (section 3). `OpenGLOpenEngine` steps cells with `stepCells` rather than the coroutine queue, and `OpenGLCellBuilder::step` returns immediately, so a cell is empty containers. The other `AppGfx` controls stay unwired: their hosts are still the WPF placeholders.
- [x] **Audio player** (`TAudioPlayer`): a Play / Pause button. `AudioManager` / `AudioBuilder` take `(ISource, path)` and load with `getAsset` (`create` is async instead of `.Result`). `SystemAudioBuilder` still throws `NotImplementedException`, so Play logs that and no samples come out. `SystemSfx.CreateAudio` boxes the manager's pair as the audio and returns null for the tag, as the C# expression does. `OpenGLEngine`'s `sfxFactory` is `SystemSfx`. NAudio's readers and `WaveOut` are not ported. Python already had the throwing `SystemSfx`.
- [x] **Data grid** (`TDataGrid`): a table with `egui_extras::TableBuilder` (resizable auto columns, no stripe, virtualized rows). Rust has no property reflection. A `Tag::Map` is Key and Value. `Tag::Any` of `Vec<IndexMap<String, Tag>>` is one row per map, columns in first-seen order, a missing key an empty cell. A `Vec<Tag>` of non-maps is one Value column; a mixed list takes columns from the maps and leaves the other rows blank. An empty map keeps the headers; an empty list is blank. Anything else stays the debug text. Python still shows the tab as text. No working family emits `DataGrid` yet (Valve's lists are unported).
- [x] **Family apps:** DataForge, StarWords and Subsumption register `Application_Startup`, and each opens its window (an egui window in the explorer). The trees are empty. `appType` still builds the base `FamilyApp`, and `DataForgeApp`, `StarWordsApp`, the databases and `CigArchive` are unported, so `Db.OpenAsync` does not run. `ResourceManagerProvider` is still a stub. Python's `openAsync` still raises `NotImplementedError`.

## 2. Fill the gaps in OpenX's Rust port

These limit the core model; each is noted in the file headers.

- [x] **Reader pooling** (C# `GenericPoolX`): `BinaryArchive` keeps a pool per path. A checkout is returned on drop, including across `.await`. C# `ActionAsync` releases when the task is created; that is not copied. `SinglePool` is not ported.
- [x] **FileSystem write mode:** `DirectoryFileSystem.openWrite` opens an existing file for write and does not create or truncate. A missing file is `NotFound` (C# stores a null pool and throws later). A filesystem with no writer is still `Unsupported`. Virtual write-back is not ported.
- [x] **Network file systems:** `NetworkFileSystem` matches C#, which does not fetch. `getArchive` returns `NotImplementedException: NetworkFileSystem`. `NetworkHost` is blocking `ureq` with a 30-minute timeout. `GetArchive` still does not build a `StreamArchive` for http, as that C# line is commented out.
- [x] **Logging:** core `Log.Info` / `Log.Error` call `openx_poly::log::Log`. `Engine.activate` sets the sink. The sink starts as stderr, so a log before activate still prints. Rust errors have no stack trace.
- [x] **TypeRegistry** lives in `openx_poly::type_x`. `family::registerType` / `findType` store the factories there. The historical copy stays at `OpenX/rust/_/core/polyio/type_x.rs`. `GetAllProperties`, `GetAllFields` and `GetDefaultConstructor` stay parked.
- [x] **`DirectoryFileSystem(baseRoot, basePath)` and `Next()`.** One `Next` per directory. A file root moves to its parent. A `.bin` / `.cue` whose `Advance` is not ported stays put and does not join `basePath`. A missing directory leaves the root (C# throws `DirectoryNotFoundException`). Zip, 7z, n64 and 3ds `Advance` return nothing.
- [ ] **`openx-vfx`'s `disc_mds.rs`** stays `PORT-STATUS: partial`. `LoadMdsPath` and `LoadResults` are in. `MountBlobs`, `EmitRawTOCEntry` and `LoadMdsToDisc` still need `Disc`, `Blob_Raw`, `RawTOCEntry`, `SubchannelQ` and `SS_Gap`, which are not in the Rust tree.

## 3. Port the GameX families to Rust

- [x] **Port `Binary_Dds`.** `DDS_HEADER.read` / `write` match the C# vectors in `OpenX.GfxTests`. `MakeFormat` stays Raw / RGBA32, and BC7 still maps to `TextureFormat.BC5`. An empty mip hits C# `-1..`, which `System.Index` rejects, so that path errors. `TextureConvert.Dxt3ToDtx5` is not ported. `.dds` uses `Binary_Dds.factory`.
- [x] **`Binary_Plist`.** `PlistReader` reads `bplist00`. The root is the last object-table entry, and `ReadObject<T>(string)` still calls itself. Uncore does not register `.plist`. Python has no Uncore twin. Checked against the C# reader on the same bytes.
- [x] **`Binary_Zip`.** A null key keeps names that do not end in `/` and leaves flags at 0. Any other key skips names ending in `/` or `\` and sets flags to 1 on an encrypted entry. A string key is the password. A `byte[]` key is SharpZipLib's private `Key`, which `zip` 4.2.0 cannot set, so that encrypted entry returns `NotSupportedException: ZipFile.Key`. `UseReader` is cleared and the bytes are copied; the borrowed reader is not closed. Uncore does not register `.zip`. `Binary.cs` stays partial. Python's twin is the smaller `zipfile` reader.
- [ ] **`Binary_Engine`:** `CellDatabase` is in `openx::manager`; `IQueryFunc` and the format itself are still unported.
- [ ] **Arkane's other games** (Dishonored, Prey, Dark Messiah, Wolfenstein) need Crytek's, Epic's, Valve's and ID's archive binaries and asset factories; `ArkaneArchive::getArcBinary` fails for them.
- [ ] **Register the other families** the same way (`register()` in the family's main file, called from `rust/apps/app-explorer/src/app_xaml.rs`) as each one's archive type is ported.

Families with the most stub files: IW 31, Uncore 22, Crytek 19, WB 17, Origin 13, Blizzard 13, Epic 11. Most others have 1 to 8. Cig Explorer has `ResourceManagerProvider` left. Families that Python already has are the easiest start, because Python shows the camelCase shape.

## 4. Bring Python up to what Rust has

C# files Rust has ported whose Python twin is missing or behind (ADR 0001). Python's Arkane, `Engine.cs` and `Store.cs` are done: every Arx file Python reads parses, except the nine `.bmp`s below.

- [ ] **Uncore `Formats/Binary.cs`:** Python lacks `Binary_Iif`, `Binary_Pal`, `Binary_Plist`, `Binary_Raw` and `Binary_Xga`, and `Binary_Img` lacks C#'s `BmpHeader` / `BmpInfoHeader`. Nine Arx `.bmp`s (e.g. `data.pak:GRAPH/particles/_Flare.bmp`) fail in `Raster.blitByPalette` with "list index out of range". `Compression.cs` is a few free functions in Python. Python's `Binary_Zip` always uses `zipfile`: no UseSystem split, no flags, a `bytes` key raises `NotImplementedError`, and the `match` guard reads `key` instead of `self.key`.
- [ ] **OpenX graphics:** `Gfx_Render.cs`'s `Colorf` / `Color32`, `OpenGL.cs`'s `RenderPrimitiveType` / `ToOpenTK`, `Engine_OpenGL.cs`'s `OpenGLObjectBuilder`, and `Gl_Render.cs`'s `MeshSceneNode` / `ParticleControllerFactory`, plus `OpenGL_Render.cs`'s sprite, trail and engine renderers. Rust has those renderers.
- [ ] **Core:** `Binary.cs`'s `EmptyArchive`, `Formats/IUnknown.cs` (6 lines of 166), `Formats/Stream.cs` (no twin), `Family.cs`'s `FamilyManager` / `ShellState`, most of `Util.cs`. (`DesSer.cs`'s JSON converters, Collada, the compression libraries and Casc's small files aren't in Rust either: their `done` rows are placeholders, section 5.)
- [ ] **Python always activates `TestEngine`:** `EngineX.inTestHost` is `'unittest' in sys.modules`, and `openx.core.core` imports `unittest`, so it is always true.
- [ ] **Store.cs's per-OS roots differ:** Python guesses Linux/macOS paths for Ubisoft and the Abandon/Archive roots, where C# has `["??"]` and `E:\...` on every OS.

## 5. Smaller known issues

- [ ] **Python doesn't decode `%20` in file URIs.**
- [ ] **C# OpenGL bugs the port kept** (PORTING.md, "OpenX's OpenGL engine in the egui Explorer"): `ARGB32` uploads as `Rgb` + `UnsignedInt8888Rev`, so RGBA images don't upload; anisotropy is set with the query enum 0x84FF; `TestTriRenderer` draws 6 of 3 vertices; only `plane` / `testtri` shaders are embedded; `TrailsRenderer` uploads `uModelMatrix` with a count of 0. Owner to decide; fix all three languages together.
- [ ] **Arkane's blast decompression** fails on `.bmp` exports in C# (143 errors). The Rust port of Blast.cs reads all 1,314 of Arx's `.bmp`s, so compare the two.
- [ ] **Python's Bullfrog and Beamdog readers** need porting properly from C#.
- [ ] **6 Rust files still name `Applications/` in their `PORT-SOURCE` header**, as do 6 rows of `rust/_/PORT_MAP.tsv`. C#'s folder is now `dotnet/Apps/`.
- [ ] **`bash rust/_/sync-check.sh` reports 11 STALE `done` files**, where the C# changed after the port: `ImportManager`, `Client`, `DesSer`, `Globalx`, `_Config`, `Collada`, `RootHandlers`, Bethesda `Records`, Gamebryo `Nif`, Uncore `Network`, and WB `Enums` (its 18 classes aren't ported; its enums are current). Port each change, then bump its PORT-SHA. It also reports 13 MISSING rows: the 6 `Applications/` ones, `Gamebryo/Formats/Nif.o.cs`, and 6 `GameX.Valve.Platform/OpenGL` files. And it reports 94 C# files with no row, 22 of them in the C#-only `Apps/GameX.App.ExplorerM`.
- [ ] **29 `done` rows are empty placeholders** whose header says "NOT PORTED": `App/ImportManager`, `App/ManifestManager`, `Assembly`, `BlizzardProtoDatabase`, `DesSer`, `Manager`, `Transforms/UnknownTransform`, the six `_LIB/Collada` files, `_LIB/Compression`'s Doboz (3), `Lzf`, `Lzo`, `Lzss`, `OodleLZ`, `Salsa20` and `XCompress`, Casc's `BackgroundWorkerEx`, `CdnCacheStats`, `Logger`, `PerfCounter` and `Salsa20`, and Uncore's `_LIB/System.IO.Compression` and `System.Security.Cryptography`. Mark them `todo` in `PORT_MAP.tsv`, or port them.
- [ ] **Some `done` headers cover only enums.** `families/bethesda/src/formats/records.rs` holds just the generated enums of `Records.cs`, which also has about 350 classes. Check the other `gen_enums.py` outputs the same way.
- [ ] **`ConstEnum`'s name table as one string plus offsets.** Each `(value, &str)` entry is a pointer with a relocation; one string plus `(value, offset)` pairs cut `WeenieClassName`'s `.rlib` from 19 to 12 MB, its build from 1.7 to 0.8 s, and a program using it from 1.8 to 1.2 MB (PORTING.md, "WB's Props are their own project"). Change `openx_poly::system::const_enum` and `gen_enums.py`, then regenerate.
- [ ] **WB's `Properties.cs`** (`GameX.WB.Props`) is a `todo` stub in Rust and has no Python twin.
- [ ] **Regenerate the other enum files.** WB's two are regenerated (camelCase `fromRaw` / `toRaw`, LF, `ConstEnum` for the big ones). `casc/root_handlers.rs`, Bethesda `records.rs`, Gamebryo `nif.rs` / `nif_o.rs` and Stardew Valley's `game_data.rs` still have `from_raw` / `to_raw`; regenerate them and set their headers by hand (the generator writes `PORT-SHA: PLACEHOLDER`), keeping the old hash where the C# file has more than enums.
- [ ] **`Graph/interface/misc/quit1.bmp` doesn't decode**: the `image` crate says "The image format could not be determined" (it's Arx sample 1's first path, outside the `.pak`s). Compare with what System.Drawing makes of it.
- [ ] **Porting scripts with dead paths:** `rust/_/stamp.py` and `gen_gamex.py` point at `/home/claude/...`. `OpenX/rust/_/sync-check.sh` has no `PORT_MAP.tsv` to read and still defaults to `../src/dotnet`.
- [ ] **Stale `#[cfg(test)]` modules.** `openx-vfx`'s `disc_nrg.rs` needed `Vec::<u8>` so the crate's tests would build. (`openx-poly`'s `reader.rs` is fixed.)
- [ ] **Four tests fail:** `openx-poly`'s `pool::tests::numeric_strings_convert_like_the_c_sharp`, `openx-vfx`'s `n64::tests::byteswapped_and_wordswapped_normalise_to_the_same_rom` (`NotAValidRom` on the word-swapped image; the suite could not run before `disc_nrg` type-checked), and `gamex-uncore`'s `network.rs` tests `credential_opcodes_are_redacted` and `non_printable_bytes_become_dots`.
- [ ] **`Store_Blizzard` can't read `product.db`** in Rust: `BlizzardProtoDatabase.cs` isn't ported (its `.proto` schema isn't in the repo).
- [ ] **Uncore's `Compression.cs`** has only Blast and Zlib in Rust. Doboz, Lzf, Lzss, Oodle and Xmem wait on `rust/core/gamex/src/vendor/compression/`'s placeholders; Lz4, Lzo, Zstd, Lzma need crates.

## Dates

- **2027-10-11: recheck the Rust UI choice** ([ADR 0006](docs/ADR/0006-review-rust-ui-2027.md)), including whether eframe has moved past glow 0.17.
