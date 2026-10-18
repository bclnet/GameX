# TODO

What's next. The tracks under "Parallel tracks" can run at the same time. Inside a track, pick from the top, and update this file in the same commit as the work: remove what's done and add what you find. Rules and conventions are in [CLAUDE.md](CLAUDE.md); decisions are in [docs/ADR](docs/ADR/Readme.md).

Last updated 2026-10-17. The Arkane bullet in section 3 is still open (`.bsp`, Crytek models, `ZipArchiveX`, and `UPackage`). The FTS map loader and the OpenGL FTL builder are in. `Binary_Dds`, `Binary_Plist`, `Binary_Zip`, `Binary_Engine`, Danae `.tea`, and the FTL object builder are in. The Vulkan provider (`VK`) registers and can create an instance. Raylib (`RL`) calls `IsWindowReady` from C# and Python; Rust cannot take the raylib 6 crate on rustc 1.75. bgfx (`BX`) is still a registration stub. wgpu (`WG`) creates an instance and one adapter and does not draw. Diligent (`DN`) registers in all three and its shim reports not linked. Bevy (`BV`) is Rust only. Host, sprite, and model methods still throw, and the Explorer does not list these ids.

## Where things stand

- **Both Rust workspaces build clean**: `cargo check --workspace` in `OpenX/rust` and `rust/`, and `cargo check -p openx --features full`, give zero errors and warnings, with rustc under 1 GB. (`gamex-wb` needed over 20 GB until its big enums became `ConstEnum`s; see PORTING.md.)
- Tests: `gamex` 51, `gamex-arkane` 8, `gamex-id` 8, `gamex-crytek` 1, `gamex-epic` 1, `gamex-valve` 8, `openx-gfx` 27, `openx-gfx-egin` 65, `openx-engine-opengl` 23, `openx-sfx` 5, `openx-poly` 36 (35 pass), `openx-vfx` 175 (174 pass, plus 12 `disc_mds` tests), `gamex-app-explorer` 7, `gamex-cig-explorer` 1. The failures are in section 5.
- **Rust port: 76 files marked `done`, 339 still `todo` in `PORT_MAP.tsv`** (partial files count as `todo`). The 29 empty `NOT PORTED` placeholders are `todo`, and their PORT-SHA values were not changed. The core model is done, including `store.rs` with every store but Blizzard. The four `Formats/Binary.cs` rows (Crytek, Epic, ID, Valve) are `todo`.
- **Arx Fatalis opens in the Rust Explorer** through GOG, and every file in its seven `.pak`s reads: text, scripts, `.wav`, `.tga`, `.bmp` / `.jpg`, `.ftl` / `.fts`. `.tea` decodes through `TheaToEerie`. The FTL object builder is in for Unity, Mirage, and OpenGL. The FTS map loader copies Danae Y-up and is registered for Unity and Mirage. Rust Unity does not call UnityEngine.
- **OpenX's engines are C#'s `Engine` / `EngineX`** (`openx::engine`), and the Explorer lists C#'s engines: OpenGL draws; the others are registered without a backend.
- **The Explorer shows Arx's textures.** `OpenGLEngine` is in the engine list (the config picks `GL`), and a "Texture" tab hosts `AppOpenGLControl`: OpenX's `OpenGLControl` in an egui paint callback on eframe's GL context. Checked on `data.pak:GRAPH/particles/Boom.bmp` with the config's `forcePath` set to `sample:1` for the run. See PORTING.md, "OpenX's OpenGL engine in the egui Explorer".

## Remaining work at a glance

| Work | Language | Size | Section |
| -- | -- | -- | -- |
| Arkane: `.bsp`, Crytek models, `ZipArchiveX`, `UPackage`, RSA, and registering Crytek / Epic / Valve / ID | Rust | large | 3 |
| The other families: 339 `todo` files, each family registered as it lands | Rust | very large | 3 |
| Uncore leftovers: nine Arx `.bmp`s, `Compression`, `Binary_Zip`, `Binary_Engine` | Python | medium | 4 |
| 11 STALE `done` files and the remaining missing rows from `sync-check.sh` | Rust | medium | 5 |
| Enum files still on `from_raw`; enum-only `done` headers | Rust | small | 5 |
| `ConstEnum` names as one string plus offsets (smaller, faster builds) | Rust | small | 5 |
| C# OpenGL bugs the ports kept | all three, owner decides | small | 5 |
| Blast fails on `.bmp` exports | C# | small | 5 |
| Failing and stale tests | Rust | small | 5 |
| Raylib (C#/Python version call; Rust blocked on rustc 1.75), bgfx, wgpu; Diligent shim reports not linked; Bevy is Rust only | per language that can host it | medium each | Parallel |

## Parallel tracks

These do not block each other. One session takes one track. The five engines are five tracks.

- [ ] **Arkane leftovers** (section 3). FTS map and OpenGL FTL are in. Next is `.bsp`, then Crytek models. Leave `ZipArchiveX` and `UPackage`.
- [ ] **Python catch-up** (section 4). The missing Uncore readers, OpenX graphics, core types, `TestEngine`, `%20`, and Store's Abandon/Archive/Ubisoft roots are in. Still open: the nine Arx `.bmp`s, `Compression`, `Binary_Zip`, and `Binary_Engine`.
- [x] **`disc_mds`** (section 2). `LoadMdsToDisc` mounts a disc. Twelve tests passed.
- [ ] **Ultima Online client.** C# only until it is merged. No Python or Rust twin for that port. `LoginScene` builds `LoginBackground` and `LoginGump` (pre-7.0.64 or Endless Journey from the client version). `Draw` counts gumps and does not paint them. There is no socket, no music, and no text entry.
- [ ] **Raylib (`RL`).** C# Raylib-cs 8.1.0 calls `IsWindowReady` and returns `6.0`. Python imports `pyray` only from `version`. Rust cannot depend on crates.io `raylib` 6.0.0 (edition 2024, rustc 1.88) while this workspace is rust-version 1.75. Nothing draws. The Explorer does not list it.
- [ ] **bgfx (`BX`).** The stub registers and does not draw. Next is the C API and the official C# bindings. This overlaps Vulkan and wgpu, so fill one shared backend before a second.
- [ ] **Bevy (`BV`).** Rust only. C# and Python do not register it. `openx-engine-bevy` records the id and does not depend on the `bevy` crate, because rust-version 1.75 is older than current Bevy.
- [ ] **wgpu (`WG`).** `createInstance` builds an instance and one adapter, with no window and no surface. C# uses Ahjo.Wgpu 0.7.0. Rust uses wgpu 22.1.0 (newer wgpu needs a newer rustc). Python imports `wgpu` only from that call. Host, sprite, and model methods still throw. bgfx stays a stub until this draws.
- [ ] **Diligent (`DN`).** The shim compiles without Diligent headers and reports not linked (`diligentShimLinked` is 0). There is no device. A real backend needs those headers, and this repo does not vendor them. The id is `DN` because `DL` is Deathloop.
- **Unreal C# stays a stub.** C++/CLI cannot host the engine: UnrealBuildTool does not support `/clr`. UnrealSharp (MIT, UE 5.6–5.8, .NET 10) and the vendored UnrealCLR (`UnrealEngine.Framework.dll`) are plugins that run inside Unreal. They do not let GameX start Unreal. The Rust side keeps the C++ shim, feature `unreal` off.

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
- [x] **`openx-vfx`'s `disc_mds.rs`.** `MountBlobs`, `EmitRawTOCEntry`, and `LoadMdsToDisc` are in, with `Disc`, `Blob_Raw`, `RawTOCEntry`, `SubchannelQ`, and `SS_Gap`. `cargo test -p openx-vfx disc_mds` is 12 passed. Still absent from that path: `SectorSynthProvider`, `DiscSectorReader`, `SS_2364_DeinterleavedQ`, `SS_PatchQ`, and `DiscTrack.NextTrack`. There is no Python twin. The n64 failure was not run.

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
  - FTS map loader is in. `FtsObjectBuilder` copies positions as stored (Danae Y-up). A quad is the strip `(0, 1, 2)` then `(1, 3, 2)`. Unity and Mirage register `Binary_Fts`. Rust Unity stays partial. There is no Rust Mirage crate. OpenGL's FTL builder is in and registers `Binary_Ftl`. `gamex-arkane` tests are 8.
  - `.bsp`, and Crytek `.xml` / models, fail when opened.
  - CryEngine's `Binary_Cry3` fails in `ZipArchiveX` (`ZipKind.Cry3`). Unreal's `Binary_Pck` fails in `UPackage`, and `WriteData` throws `NotImplementedException`. Leave both.
  - Crytek, Epic, Valve, and ID stay unregistered (`QGame`, Dunia, Valve `GetArcBinary`, and the transforms are still out).
  - Python was not changed. There is no `Binary_Lmp`. `Binary_Spr` sizes its buffer with `<< 4` (C# is `<< 2`). `Binary_Mdl40` names the info group `Model` (C# is `UnknownFileModel`), stores the string `self`, turns a byte above 127 into U+FFFD, and gives `M_Header2` / `M_Texture` the sizes 244 and 80. ID's `assetFactory` still always returns Uncore. Valve's VPK decodes `readVWString` as UTF-32, always runs `verifyHashes`, and does not check the CRC.
- [ ] **Register the other families** the same way (`register()` in the family's main file, called from `rust/apps/app-explorer/src/app_xaml.rs`) as each one's archive type is ported.

Families with the most stub files: IW 31, Uncore 21, Crytek 20, WB 17, Origin 13, Blizzard 13, Epic 12. Most others have 1 to 8. Cig Explorer has `ResourceManagerProvider` left. Families that Python already has are the easiest start, because Python shows the camelCase shape.

## 4. Bring Python up to what Rust has

C# files Rust has ported whose Python twin is missing or behind (ADR 0001). Python's Arkane, `Engine.cs` and `Store.cs` are done: every Arx file Python reads parses, except the nine `.bmp`s below.

- [ ] **Uncore `Formats/Binary.cs` leftovers.** `Binary_Iif`, `Binary_Pal`, `Binary_Plist`, `Binary_Raw`, `Binary_Xga`, and `BmpHeader` / `BmpInfoHeader` are in. `BmpHeader._struct` is `('<H3i', 54)`. Nine Arx `.bmp`s (e.g. `data.pak:GRAPH/particles/_Flare.bmp`) still fail in `Raster.blitByPalette` with "list index out of range". `Compression.cs` is still a few free functions. Python's `Binary_Zip` still always uses `zipfile`. Python's `Binary_Engine` still treats `bool("False")` as true, uses `len(pr) == 1` for identity (C# uses `<= 3`), and calls `getQuery` with no `IQueryFunc` cast.
- [x] **OpenX graphics.** Python has `Colorf` / `Color32`, `RenderPrimitiveType` / `toOpenTK`, `OpenGLObjectBuilder`, `MeshSceneNode` / `ParticleControllerFactory`, and the sprite, trail, and engine renderers. `Colorf` equality stays false. `Color32` still scales by `0xfff` and keeps the low 8 bits. Trail `uModelMatrix` is still uploaded with count 0.
- [x] **Core.** Python has `EmptyArchive`, `IUnknown`, `Stream` (`PakBinaryCanStream`), `FamilyManager` / `ShellState`, and `decodeUri` on `Util`. `Archive.Empty` is built at the end of `family.py`. (`DesSer`, Collada, the compression libraries, and Casc's small files stay `todo` placeholders, section 5.)
- [x] **`TestEngine` is no longer always on.** `EngineX.inTestHost` checks the process (`unittest`, `pytest`, `testhost`), not whether `unittest` was imported. `openx.core.core` does not import `unittest`. The agent reported `inTestHost` false under `python -c` and true under `python -m unittest`.
- [x] **Store roots.** Python `store_abandon` and `store_archive` return `E:\AbandonLibrary` and `E:\ArchiveLibrary` on every OS. Ubisoft's Linux and macOS search is `["??"]`, matching C# and Rust.

## 5. Smaller known issues

- [x] **`%20` in file URIs.** `decodeUri` matches `Uri.UnescapeDataString` in C#, Python, and Rust. `+` stays `+`. A bare `C:/foo%20bar` is not decoded. `game:/main%20.yaml` becomes `/main .yaml`. `Util.cs` PORT-SHA is `25073859b079bfe0`. The agent reported `cargo test -p openx` 42 passed and `cargo test -p gamex --lib` 51 passed.
- [ ] **C# OpenGL bugs the port kept** (PORTING.md, "OpenX's OpenGL engine in the egui Explorer"): `ARGB32` uploads as `Rgb` + `UnsignedInt8888Rev`, so RGBA images don't upload; anisotropy is set with the query enum 0x84FF; `TestTriRenderer` draws 6 of 3 vertices; only `plane` / `testtri` shaders are embedded; `TrailsRenderer` uploads `uModelMatrix` with a count of 0. Owner to decide; fix all three languages together.
- [ ] **Arkane's blast decompression** fails on `.bmp` exports in C# (143 errors). The byte-array writer no longer copies past the buffer: a chunk that does not fit returns `blast error: 1` in C# and Python. Rust already panics on that slice. The 143 `.bmp` exports were not re-run.
- [x] **Python's Bullfrog and Beamdog readers** follow C#. Bullfrog has `Binary_Bullfrog`, `Binary_Fli`, `Binary_Populus`, `Binary_Syndicate`, and `Binary_SyndicateX`. Beamdog is `Binary_Abc` and `Binary_XXX` (the old `Binary_Hogg` copy is gone). `py_compile` passed. Those Uncore types are in Python now. The nine-bmp `blitByPalette` crash is still open. Rust was not changed.
- [x] **6 Rust files named `Applications/`.** `PORT-SOURCE` and `PORT_MAP.tsv` now say `GameX.Cli/` (crate `gamex-cli`) and `Apps/GameX.App.Client/` (crate `gamex-app-client`). The CLI project lives at `dotnet/GameX.Cli`, not under `Apps/`.
- [ ] **`bash rust/_/sync-check.sh` reports 11 STALE `done` files**, where the C# changed after the port: `ImportManager`, `Client`, `DesSer`, `Globalx`, `_Config`, `Collada`, `RootHandlers`, Bethesda `Records`, Gamebryo `Nif`, Uncore `Network`, and WB `Enums` (its 18 classes aren't ported; its enums are current). Port each change, then bump its PORT-SHA. `ImportManager`, `DesSer`, and `Collada` are `todo` placeholders now, so they are no longer `done`. The script was not re-run. It also reported `Gamebryo/Formats/Nif.o.cs` and 6 `GameX.Valve.Platform/OpenGL` files missing, and 94 C# files with no row, 22 of them in the C#-only `Apps/GameX.App.ExplorerM`.
- [x] **29 empty placeholders are `todo`.** Headers still say "NOT PORTED", and the PORT-SHA values were not changed: `App/ImportManager`, `App/ManifestManager`, `Assembly`, `BlizzardProtoDatabase`, `DesSer`, `Manager`, `Transforms/UnknownTransform`, the six `_LIB/Collada` files, `_LIB/Compression`'s Doboz (3), `Lzf`, `Lzo`, `Lzss`, `OodleLZ`, `Salsa20` and `XCompress`, Casc's `BackgroundWorkerEx`, `CdnCacheStats`, `Logger`, `PerfCounter` and `Salsa20`, and Uncore's `_LIB/System.IO.Compression` and `System.Security.Cryptography`. `PORT_MAP.tsv` is 73 `done` / 339 `todo`. They were not ported.
- [ ] **Some `done` headers cover only enums.** `families/bethesda/src/formats/records.rs` holds just the generated enums of `Records.cs`, which also has about 350 classes. Check the other `gen_enums.py` outputs the same way.
- [ ] **`ConstEnum`'s name table as one string plus offsets.** Each `(value, &str)` entry is a pointer with a relocation; one string plus `(value, offset)` pairs cut `WeenieClassName`'s `.rlib` from 19 to 12 MB, its build from 1.7 to 0.8 s, and a program using it from 1.8 to 1.2 MB (PORTING.md, "WB's Props are their own project"). Change `openx_poly::system::const_enum` and `gen_enums.py`, then regenerate.
- [x] **WB's `Properties.cs`** is ported. Rust `properties.rs` is `done` at `bf4ea84fcfecfe68`. Python `properties.py` is imported from `GameX_WB_Props.py`. `ClientProperties` and `ServerOnlyProperties` share one set, matching `GenericPropertiesId<ServerOnlyAttribute>`. `cargo check -p gamex-wb-props` was reported clean. `py_compile` passed. The C# file is CRLF and has no BOM.
- [ ] **Regenerate the other enum files.** WB's two are regenerated (camelCase `fromRaw` / `toRaw`, LF, `ConstEnum` for the big ones). `casc/root_handlers.rs`, Bethesda `records.rs`, Gamebryo `nif.rs` / `nif_o.rs` and Stardew Valley's `game_data.rs` still have `from_raw` / `to_raw`; regenerate them and set their headers by hand (the generator writes `PORT-SHA: PLACEHOLDER`), keeping the old hash where the C# file has more than enums.
- [ ] **`Graph/interface/misc/quit1.bmp` doesn't decode**: the `image` crate says "The image format could not be determined" (it's Arx sample 1's first path, outside the `.pak`s). Compare with what System.Drawing makes of it.
- [x] **Porting scripts pointed at `/home/claude`.** `rust/_/stamp.py`, `rust/_/gen_gamex.py`, `OpenX/rust/_/stamp.py`, `gen_port.py`, and `gen_platform_stubs.py` now resolve the repo from `__file__`. They were not run. `stamp.py` rewrites PORT-SHA values and `PORT_MAP.tsv`. `OpenX/rust/_/sync-check.sh` defaults to `OpenX/dotnet`. OpenX still has no `PORT_MAP.tsv`, so that script exits 2.
- [ ] **Stale `#[cfg(test)]` modules.** `openx-vfx`'s `disc_nrg.rs` needed `Vec::<u8>` so the crate's tests would build. (`openx-poly`'s `reader.rs` is fixed.)
- [ ] **Four tests fail:** `openx-poly`'s `pool::tests::numeric_strings_convert_like_the_c_sharp`, `openx-vfx`'s `n64::tests::byteswapped_and_wordswapped_normalise_to_the_same_rom` (`NotAValidRom` on the word-swapped image; the suite could not run before `disc_nrg` type-checked), and `gamex-uncore`'s `network.rs` tests `credential_opcodes_are_redacted` and `non_printable_bytes_become_dots`.
- [ ] **`Store_Blizzard` can't read `product.db`** in Rust: `BlizzardProtoDatabase.cs` isn't ported (its `.proto` schema isn't in the repo).
- [ ] **Uncore's `Compression.cs`** has only Blast and Zlib in Rust. Doboz, Lzf, Lzss, Oodle and Xmem wait on `rust/core/gamex/src/vendor/compression/`'s placeholders; Lz4, Lzo, Zstd, Lzma need crates.

## Dates

- **2027-10-11: recheck the Rust UI choice** ([ADR 0006](docs/ADR/0006-review-rust-ui-2027.md)), including whether eframe has moved past glow 0.17.
