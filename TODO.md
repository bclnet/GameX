# TODO

What's next, in order. Pick from the top, and update this file in the same commit as the work: remove what's done and add what you find. Rules and conventions are in [CLAUDE.md](CLAUDE.md); decisions are in [docs/ADR](docs/ADR/Readme.md).

Last updated 2026-10-17. Next is the open Arkane bullet in section 3 (the FTS map loader, then `.bsp`, Crytek models, `ZipArchiveX`, and `UPackage`). `Binary_Dds`, `Binary_Plist`, `Binary_Zip`, `Binary_Engine`, Danae `.tea`, and the FTL object builder are in.

## Where things stand

- **Both Rust workspaces build clean**: `cargo check --workspace` in `OpenX/rust` and `rust/`, and `cargo check -p openx --features full`, give zero errors and warnings, with rustc under 1 GB. (`gamex-wb` needed over 20 GB until its big enums became `ConstEnum`s; see PORTING.md.)
- Tests: `gamex` 51, `gamex-arkane` 7, `gamex-id` 8, `gamex-crytek` 1, `gamex-epic` 1, `gamex-valve` 8, `openx-gfx` 27, `openx-gfx-egin` 65, `openx-engine-opengl` 23, `openx-sfx` 5, `openx-poly` 36 (35 pass), `openx-vfx` 175 (174 pass), `gamex-app-explorer` 7, `gamex-cig-explorer` 1. The failures are in section 5.
- **Rust port: 102 files marked `done`, 305 still `todo` in `PORT_MAP.tsv`** (partial files count as `todo`). 29 of the 102 are placeholders whose header says "NOT PORTED" (section 5), so 73 are really ported. The core model is done, including `store.rs` with every store but Blizzard. The four `Formats/Binary.cs` rows (Crytek, Epic, ID, Valve) are `todo`.
- **Arx Fatalis opens in the Rust Explorer** through GOG, and every file in its seven `.pak`s reads: text, scripts, `.wav`, `.tga`, `.bmp` / `.jpg`, `.ftl` / `.fts`. `.tea` decodes through `TheaToEerie`. The FTL object builder is in for Unity and Mirage. The FTS map loader is not started.
- **OpenX's engines are C#'s `Engine` / `EngineX`** (`openx::engine`), and the Explorer lists C#'s engines: OpenGL draws; the others are registered without a backend.
- **The Explorer shows Arx's textures.** `OpenGLEngine` is in the engine list (the config picks `GL`), and a "Texture" tab hosts `AppOpenGLControl`: OpenX's `OpenGLControl` in an egui paint callback on eframe's GL context. Checked on `data.pak:GRAPH/particles/Boom.bmp` with the config's `forcePath` set to `sample:1` for the run. See PORTING.md, "OpenX's OpenGL engine in the egui Explorer".

## Remaining work at a glance

| Work | Language | Size | Section |
| -- | -- | -- | -- |
| `disc_mds` mount (`LoadMdsToDisc` still needs `Disc` / `Blob_Raw`) | Rust | small | 2 |
| Arkane: FTS map, `.bsp`, Crytek models, `ZipArchiveX`, `UPackage`, RSA, and registering Crytek / Epic / Valve / ID | Rust | large | 3 |
| The other families: 305 `todo` files, each family registered as it lands | Rust | very large | 3 |
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

Done. Each line keeps the hole that is still open.

- [x] **Engine view.** `ObjectRenderer` passes the loaded object as `SourcePath::Obj`. The Rust Explorer still has no `BuildersByType` entry, so an Object tab logs the missing builder. Frame delay is a float. The Particle tab builds `ParticleRenderer`, which draws nothing. Sprite and trail shaders are still not embedded (section 5). `Binary_Esm` is still unported, so a real Engine file fails the `IQueryFunc` cast.
- [x] **Audio player.** Play / Pause loads through `getAsset`. `SystemAudioBuilder` still throws, so no samples come out. NAudio is not ported.
- [x] **Data grid.** A map is Key and Value. A list of maps is one row per map. Python still shows the tab as text.
- [x] **Family apps.** DataForge, StarWords, and Subsumption each open an empty egui window. `DataForgeApp`, the databases, and `CigArchive` are unported. Python's `openAsync` still raises `NotImplementedError`.

## 2. Fill the gaps in OpenX's Rust port

These limit the core model. Behavior that was matched on purpose stays in the line.

- [x] **Reader pooling.** `BinaryArchive` keeps a pool per path and returns a checkout on drop, including across `.await`. `SinglePool` is not ported. C# releases when the task is created; that timing is not copied.
- [x] **FileSystem write mode.** `openWrite` opens an existing file and does not create or truncate. A missing file is `NotFound`. Virtual write-back is not ported.
- [x] **Network file systems.** `NetworkFileSystem` does not fetch. `getArchive` returns `NotImplementedException: NetworkFileSystem`. `NetworkHost` is blocking `ureq` with a 30-minute timeout.
- [x] **Logging.** Core `Log.Info` / `Log.Error` call `openx_poly::log::Log`. The sink starts as stderr. Rust errors have no stack trace.
- [x] **TypeRegistry** lives in `openx_poly::type_x`. `GetAllProperties`, `GetAllFields`, and `GetDefaultConstructor` stay parked.
- [x] **`DirectoryFileSystem(baseRoot, basePath)` and `Next()`.** One `Next` per directory. A missing directory leaves the root. Zip, 7z, n64, and 3ds `Advance` return nothing.
- [ ] **`openx-vfx`'s `disc_mds.rs`** stays `PORT-STATUS: partial`. `LoadMdsPath` and `LoadResults` are in. `MountBlobs`, `EmitRawTOCEntry` and `LoadMdsToDisc` still need `Disc`, `Blob_Raw`, `RawTOCEntry`, `SubchannelQ` and `SS_Gap`, which are not in the Rust tree.

## 3. Port the GameX families to Rust

- [x] **`Binary_Dds`.** `MakeFormat` stays Raw / RGBA32, and BC7 still maps to `TextureFormat.BC5`. An empty mip errors. `TextureConvert.Dxt3ToDtx5` is not ported.
- [x] **`Binary_Plist`.** `PlistReader` reads `bplist00`. The root is the last object-table entry, and `ReadObject<T>(string)` still calls itself. Uncore does not register `.plist`. Python has no Uncore twin.
- [x] **`Binary_Zip`.** A null key keeps names that do not end in `/`. Any other key skips names ending in `/` or `\` and flags an encrypted entry. A `byte[]` key cannot be set on `zip` 4.2.0, so that entry returns `NotSupportedException: ZipFile.Key`. Uncore does not register `.zip`. Python's twin is the smaller `zipfile` reader.
- [x] **`Binary_Engine`.** Six ASCII fields. A byte above 127 becomes `?`. A rotation of length `<= 3` is identity. The value is `Arc<dyn CellDatabase>` inside `Tag::Any` when the archive implements `IQueryFunc`. Bethesda's `Binary_Esm` is still unported, so a real query archive fails that cast. Python treats `bool("False")` as true, uses `len(pr) == 1` for identity, and calls `getQuery` with no cast.
- [x] **idTech `Binary_Pak` and `Binary_Wad`.** A `.wad` entry opens a child archive.
- [x] **Source `Binary_Vpk`.** The tree is UTF-16 stopped on the first 0 byte. Version 2 skips MD5 when `Engine.v` contains `x`. A signature that must be checked fails closed (`RSA.VerifyData` is not ported).
- [x] **idTech `Binary_Lmp`, `Binary_Spr`, and `Binary_Mdl`.** `Palette` and `Colormap` are process-wide. A picture with no palette throws. Pixels are RGB plus a 0 byte labeled `BGRA32`. `.spr` is `IDSP` and `NextFrame` always returns true, then steps past the last frame. `.mdl` reads nothing (`UnknownFileModel`). `.bsp` still fails when opened.
- [x] **Source `Binary_Mdl40`.** Magic `IDST`, version at least 40, a 64-byte ASCII name (trailing NULs trimmed, a byte above 127 is `?`), and the six vectors plus flags. An empty name throws. Bone, sequence, and texture loads stay commented out. The factory casts to `BinaryArchive` before it reads.
- [x] **Danae `.tea`.** `TheaToEerie` in C#, Python, and Rust. Version below 2014 throws. A move, orient, or morph payload is read only when the flag is 1. `anim_time` is `(float)nb_frames * 1000 * DIV24` (minimum 1). The blend is component-wise with the weights swapped. `num_sfx` is discarded. A sample name is stored and `sample` stays -1.
- [x] **FTL object builder.** `FtlObjectBuilder` builds the shared mesh: three vertices per face, first-seen texture, skip `HIDE` / `NODRAW`, reversed winding when `FaceType == 2` or the texture is `DOUBLESIDED`. Danae stays Y-up. Unity and Mirage register `Binary_Ftl`. There is no Rust Mirage crate, and the Rust Unity file does not call UnityEngine. `gamex-arkane` tests are 7.
- [ ] **Arkane's other games** (Dishonored, Prey, Dark Messiah, Wolfenstein). `ArkaneArchive` dispatches on the engine name. The readers above are in. Still open:
  - FTS map loader. OpenGL has no FTL builder.
  - `.bsp`, and Crytek `.xml` / models, fail when opened.
  - CryEngine's `Binary_Cry3` fails in `ZipArchiveX` (`ZipKind.Cry3`). Unreal's `Binary_Pck` fails in `UPackage`, and `WriteData` throws `NotImplementedException`. Leave both.
  - Crytek, Epic, Valve, and ID stay unregistered (`QGame`, Dunia, Valve `GetArcBinary`, and the transforms are still out).
  - Python was not changed. There is no `Binary_Lmp`. `Binary_Spr` sizes its buffer with `<< 4` (C# is `<< 2`). `Binary_Mdl40` names the info group `Model` (C# is `UnknownFileModel`), stores the string `self`, turns a byte above 127 into U+FFFD, and gives `M_Header2` / `M_Texture` the sizes 244 and 80. ID's `assetFactory` still always returns Uncore. Valve's VPK decodes `readVWString` as UTF-32, always runs `verifyHashes`, and does not check the CRC.
- [ ] **Register the other families** the same way (`register()` in the family's main file, called from `rust/apps/app-explorer/src/app_xaml.rs`) as each one's archive type is ported.

Families with the most stub files: IW 31, Uncore 21, Crytek 20, WB 17, Origin 13, Blizzard 13, Epic 12. Most others have 1 to 8. Cig Explorer has `ResourceManagerProvider` left. Families that Python already has are the easiest start, because Python shows the camelCase shape.

## 4. Bring Python up to what Rust has

C# files Rust has ported whose Python twin is missing or behind (ADR 0001). Python's Arkane, `Engine.cs` and `Store.cs` are done: every Arx file Python reads parses, except the nine `.bmp`s below.

- [ ] **Uncore `Formats/Binary.cs`:** Python lacks `Binary_Iif`, `Binary_Pal`, `Binary_Plist`, `Binary_Raw` and `Binary_Xga`, and `Binary_Img` lacks C#'s `BmpHeader` / `BmpInfoHeader`. Nine Arx `.bmp`s (e.g. `data.pak:GRAPH/particles/_Flare.bmp`) fail in `Raster.blitByPalette` with "list index out of range". `Compression.cs` is a few free functions in Python. Python's `Binary_Zip` always uses `zipfile`: no UseSystem split, no flags, a `bytes` key raises `NotImplementedError`, and the `match` guard reads `key` instead of `self.key`. Python's `Binary_Engine` treats `bool("False")` as true, uses `len(pr) == 1` for identity (C# uses `<= 3`), and calls `getQuery` with no `IQueryFunc` cast.
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
