# Grok context

Evaluation of this checkout on 2026-10-15. The Rust Explorer's engine view is wired for textures, objects, particles and engines. `SourcePath::Obj` carries a loaded object by identity. No `BuildersByType` entry exists yet, so an Object tab stays empty. The Particle tab builds the outer `ParticleRenderer`, which draws nothing. `Binary_Engine` produces the `CellDatabase` arc when the query archive implements `IQueryFunc`. Bethesda's `Binary_Esm` is that implementor and is still unported, so a real Engine tab still fails the cast. Texture frame methods are on `ITexture`; current textures have no frames. Frame delay is a float in C#, Python, and Rust. An Audio tab has a Play / Pause button that calls `SystemSfx`; `SystemAudioBuilder` throws, so Play logs `NotImplementedException` and no samples come out. A DataGrid tab is a table: a map is Key and Value, and a list of maps is one row per map. Anything else stays the debug text. Python still shows that tab as text. Cig's DataForge, StarWords and Subsumption apps register `Application_Startup` and open an egui window. The trees are empty until `DataForgeApp`, the databases and `CigArchive` are ported. Python's `openAsync` still raises `NotImplementedError`. Rules and conventions stay in [CLAUDE.md](../../CLAUDE.md). The work queue stays in [TODO.md](../../TODO.md). This file is what those two do not say: what is real, what only looks finished, and which documents lag the tree.

`cargo check --workspace` was run for `OpenX/rust` and `rust/` after `SourcePath::Obj`. Build and test counts below are TODO.md's, dated the same day. Game-file tests were not re-run.

## What is real

One path works end to end. Arx Fatalis, through GOG: locate the install, open its seven `.pak`s, decode text, scripts, audio, and images, and draw a texture in the Rust Explorer. The Explorer's startup in `rust/apps/app-explorer/src/app_xaml.rs` registers two families, `gamex_uncore` and `gamex_arkane`, and eight engines. OpenGL is the one that draws. The others are names in the list.

That path sits on a real core. In Rust, `Family`, `Binary`, `Store` (every store except Blizzard's `product.db`), `Meta`, `Util`, `Config`, `IUnknown`, and `Stream` are ports. Python's Arkane, `Engine`, and `Store` match that slice. Nine Arx `.bmp`s still fail in Python (`Raster.blitByPalette`).

Specs in `python/gamex/specs/*.json` are the family and game list. C# embeds those same files. A game that is not in a spec does not exist, whatever the README table says.

## What only looks finished

`rust/_/PORT_MAP.tsv` has 101 `done` rows and 300 `todo` rows. There is no `partial` row. `families/uncore/src/formats/binary.rs` is `done`. `compression.rs` still marks itself `partial`.

Of the 101 `done` rows, 29 are empty placeholders whose header says `NOT PORTED`: Collada, most compression codecs, `DesSer`, `Manager`, `ImportManager`, `ManifestManager`, `Assembly`, `BlizzardProtoDatabase`, `UnknownTransform`, several Casc helpers, and Uncore's two `_LIB` crypto and compression wrappers. `Blast.cs` is a real port. The other compression files are decisions, not codecs.

Several more `done` files are generated enums only. `records.rs` is the enums from Bethesda's `Records.cs`, which also has about 350 classes. Treat `nif.rs`, `nif_o.rs`, and Stardew Valley's `game_data.rs` the same way until the classes are there. WB's `enums.rs` and `enums_weenie.rs` are current generated enums (camelCase `fromRaw` / `toRaw`, `ConstEnum` for the big ones).

Every family has a Rust crate and a `python/gamex/families/GameX_*.py`. A crate or a module file is the slot, not the port. Most Python format folders are an empty `__init__.py`. The Rust Explorer cannot open a family whose `register()` is not called from `app_xaml.rs`; the failure is `unknown ArchiveType`.

`rust/_/PORTING.md` is a lab notebook. Its opening still says nothing has been compiled and that 56 files are ported. Later sections and TODO.md are the current picture. Read it from the bottom.

`README.md`'s game table is almost all dashes. It is not a status board.

`bash rust/_/sync-check.sh` is the drift check: 11 `done` files whose C# changed after the port, 13 missing rows, 6 `PORT-SOURCE` paths that still say `Applications/` (the folder is `dotnet/Apps/`). `rust/_/stamp.py` and `gen_gamex.py` still point at `/home/claude/...`.

## Newest structure

WB's Props left `GameX.WB` so the 44k lines of enums are their own project. The three twins:

| | Path |
| -- | -- |
| C# | `dotnet/Families/GameX.WB.Props` |
| Rust | `rust/families/wb-props` (`gamex-wb-props`); `gamex-wb` re-exports `formats::ac::props` |
| Python | `python/gamex/families/GameX_WB_Props.py` and `WB_Props/` |

`Properties.cs` is still a Rust `todo` stub and has no Python file. The Python package imports the two enum modules only.

A C# project `GameX.X.Sub` is the Rust crate `rust/families/x-sub` and the Python module `GameX_X_Sub.py`. Keep the three in step when a project is split.

## Where the languages differ

C# is the reference and the only complete family set. Its tests mostly need installed games and many still use old family ids (`AC`, `Tes`, `Cry`). A failure there is not a regression by itself.

Rust is ahead of Python on the core model and on the Arx decode path, and behind C# on every other family. OpenX's Rust port has no `PORT-STATUS` headers. Code the owner parked is under `OpenX/rust/core/poly/_/` and `OpenX/rust/_/`. Move a type out when something needs it. Do not un-park a folder to make a feature compile.

Python is the hand port and the spec source. It is behind Rust on `EmptyArchive`, `IUnknown`, `Stream`, `FamilyManager` / `ShellState`, and most of `Util`. It always activates `TestEngine` because `openx.core.core` imports `unittest`. Bullfrog and Beamdog readers are not real ports yet.

ADR 0001 still holds: one change, all three languages, same names. A missing twin is recorded, not invented.

## Known defects left on purpose

Do not fix these unless the owner asks, and then in all three languages:

- Archive entry names are combined with the output folder with no `..` check. GameX trusts its archives.
- C# OpenGL: `ARGB32` uploads as `Rgb` + `UnsignedInt8888Rev`, anisotropy uses the query enum `0x84FF`, `TestTriRenderer` draws 6 of 3 vertices, only the `plane` and `testtri` shaders are embedded. The Rust port kept them.
- C# Blast fails on 143 Arx `.bmp` exports. Rust's Blast reads all 1,314.
- The CLIs and `_Config` / `_config.py` contain commented debug args and family switches. Leave them. If one is uncommented, the CLI ignores its real arguments.
- Four tests fail and are already listed in TODO.md section 5. `openx-vfx`'s `disc_nrg.rs` tests compile. The n64 word-swap test in that crate fails.

## How to start a session

Read TODO.md and take the top open item. The queue is section 3: Arkane's other games, then the other families. Uncore's `Binary_Dds`, `Binary_Plist`, `Binary_Zip`, and `Binary_Engine` are in. A real `.start` still fails the `IQueryFunc` cast until Bethesda's `Binary_Esm`. `disc_mds` still has `LoadMdsToDisc` open; that waits on `Disc` and does not block the core model. IW, Cig Explorer, Crytek, WB, Origin, Blizzard, and Epic are the large stub piles. Python's families are the easier pattern for a Rust port, because the camelCase shape is already there.

When a Rust file is the work, follow `.claude/skills/port-to-rust/SKILL.md`. Keep the `PORT-SOURCE` / `PORT-SHA` / `PORT-STATUS` header. A `done` header is a claim. Open the file.

Update TODO.md in the same commit as the work. Commit only when asked. The owner commits as `updates` on `master` and works from more than one machine. Pull first. Do not force-push.
