# GameX

GameX loads game assets (textures, models, levels, audio) out of the archive formats of ~36 studios/engines ("families") and hands them to a rendering engine. The C# tree is the hand-written original; Python is a partial hand port; Rust is an in-progress AI-assisted port.

**What's next is in [TODO.md](TODO.md).** Read it at the start of a session, and update it in the same commit as the work.

## Layout

| Path | What it is |
| -- | -- |
| `dotnet/` | GameX in C# — the reference implementation. `Core/GameX` (Family → Game → Archive model, store detection), `Families/GameX.*` (one project per family), `Engines/GameX.Engine.*`, `GameX.Cli`, `Apps/` (WPF/MAUI explorers, Windows-only) |
| `python/gamex/` | Python port. Same structure, fewer families implemented |
| `rust/` | Rust port. The core model (`core/gamex`), Uncore and Arkane, and the egui Explorer (`apps/app-explorer`, which opens Arx Fatalis and draws textures through OpenX's OpenGL engine) work; most family files are still `PORT-STATUS: todo` stubs. See `rust/_/PORTING.md` and `PORT_MAP.tsv` |
| `OpenX/` | The engine-abstraction library GameX sits on (`OpenX.Poly`, `Gfx`, `Sfx`, `Vfx`, `Engines/OpenX.Engine.*`), vendored in-tree in all three languages. Formerly named OpenStack |
| `Mirage/` | Headless test engine (scene graph with a text form). Mirrors github.com/bclnet/Mirage |
| `python/gamex/specs/*.json` | Family/game definitions. **Single source of truth** — the C# build embeds these same files |
| `third_party/` | Proprietary DLLs (Oodle, XCompress), excluded from the MIT license. Don't copy them elsewhere |
| `book/`, `docs/`, `resources/`, `posters/` | Documentation and data; rarely relevant to code changes |

Each engine exists at two layers: `OpenX.Engine.X` (implements `Engine` and the `IOpenGfx*` slots) and `GameX.Engine.X` (`XRenderer.CreateRenderer`, family object builders). Engine codes: `GL` OpenGL, `UN` Unity, `UR` Unreal, `GD` Godot, `SD` SDL, `ST` Stride, `OG` Ogre, `MG` MonoGame, `EX` EginX, `MR` Mirage, `O3` O3DE, `VK` Vulkan, `RL` Raylib, `BX` bgfx, `BV` Bevy, `WG` wgpu, `DN` Diligent, `TT` Test, `UK` Unknown. Register an id only in a language that can host that engine. `BV` is Rust only: no C# project and no Python module. `RL`, `BX`, `WG`, and `DN` are registration stubs in all three. The Explorer does not list the stubs.

## Build and test

Build individual projects, not the solution — `GameX.sln` includes WPF/MAUI/Unity projects that only build on Windows.

```bash
dotnet build dotnet/GameX.Cli/GameX.Cli.csproj
dotnet build dotnet/Core/GameXTests/GameXTests.csproj      # builds every family
```

On Windows, `GameX.Resource`'s post-build `xcopy` needs `$(SolutionDir)`, which a single-project build leaves undefined; add `-p:SolutionDir=<repo>\dotnet\` (it copies into the gitignored `engines/Plugins/GameX.Resource/`).

Tests that run anywhere (no game files needed):

```bash
dotnet test Mirage/dotnet/Mirage.sln
dotnet test OpenX/dotnet/Engines/OpenX.EngineTests/OpenX.EngineTests.csproj --filter "FullyQualifiedName~MirageEngineTests"
cargo test -p openx-gfx --manifest-path OpenX/rust/Cargo.toml
```

Everything else is an integration test against installed games and is expected to fail on a machine without them: `GameXTests` (most of its ~350 tests), `python/tests`, and the OpenGL tests in `OpenX.EngineTests` (need a GL context). Many of those tests are also stale (old family ids like `AC`, `Tes`, `Cry`). Don't treat their failures as regressions; don't "fix" them unless asked.

To test GameX code without a GPU or game install, use the Mirage engine: `MirageEngine.Reset()`, render through `MirageRenderer.CreateRenderer(...)`, assert on `MirageEngine.Tree.ToText()`. See `docs/Engines/Mirage/Readme.md`.

Rust: both workspaces build with zero errors and warnings (`cargo check --workspace` in `OpenX/rust` and in `rust/`), and so does `cargo check -p openx --features full` in `OpenX/rust`. Some crates' own `#[cfg(test)]` modules are stale and don't compile (e.g. `openx-vfx`'s `disc_nrg.rs`), so `cargo test` works per crate, not everywhere; TODO.md lists the known test failures. `cargo test -p gamex` (in `rust/`) loads every family spec and opens archives over local files. `cargo run -p gamex-app-explorer` opens the Rust Explorer.

Python: put `python/` and `OpenX/python/` on `sys.path`, then `import gamex` (which runs `init()`) and `gamex.getFamily('Arkane').getArchive('game:/#AF')`. A game's `.pak`s are sub-archives on `FileSource.arc`: call `open()`, walk its `files`, and read each with `await archive.getAsset(object, file)` (formats) or `getData` (bytes). That's the quickest check of a Python format against real files.

### Seeing the Rust Explorer

- It opens the config's family and game (`rust/core/gamex/src/config.rs`, the twin of `_Config.cs`) and selects `forcePath`: `sample:N` is entry N of the family's `python/gamex/specs/*Family.Sample.json` (Arkane's `sample:1` is textures). These are the owner's settings; change them only for a run and put them back.
- Screenshots taken from outside (GDI `CopyFromScreen`, `PrintWindow`) come out white for eframe's GL window. To see it, patch `MainPage::ui` for the run: `request_repaint()` (egui only repaints on input otherwise), send `egui::ViewportCommand::Screenshot` after a few hundred frames, save the `egui::Event::Screenshot` image with the `image` crate, then close. Revert the patch afterwards.
- egui's resizable side panel grows to fit any row wider than it, a few pixels every frame, until it fills the window; size rows to the space actually left (`file_explorer_xaml.rs`'s filter row).

## Things that will bite you

- **The CLIs have hardcoded debug args.** `dotnet/GameX.Cli/Program.cs` and `python/gamex/cli/_cli.py` contain commented `args = [...]` / `sys.argv = [...]` lines the owner toggles for testing. If one is uncommented, the CLI ignores its real arguments, and on macOS/Linux it creates literal `D:\...` directories in the cwd. Leave these lines as they are; clean up any such directory you create.
- **`dotnet/Core/GameX/_Config.cs` and `python/gamex/_config.py`** select a debug family/game via `#define` / options. Also owner-toggled; not a bug.
- **`import gamex` has side effects** — it runs `init()`, which opens archives.
- **Store paths are personal** (`E:\AbandonLibrary`, `G:\...`). Game detection in `Core/GameX/Store.cs` only fully works on Windows.
- **Every change goes to every language** ([ADR 0001](docs/ADR/0001-ports-stay-one-to-one.md)). A change to C# is made to its Python twin (`python/gamex/...`, `OpenX/python/...`) and its Rust twin (listed in `PORT_MAP.tsv`) in the same change, with the same names and shape. Where a twin doesn't exist yet, say so; list each language as updated, not applicable, or still to do. The MAUI explorer (`dotnet/Apps/GameX.App.ExplorerM`) is C#-only: it has no Python or Rust port and doesn't need one. The Ultima Online client port is also C# only until it is merged into GameX (see Known, accepted exceptions). UI: C# WPF, Python PyQt6, Rust egui (eframe), with each XAML view and its code-behind one `.rs` file (`views/file_explorer_xaml.rs`) ([ADR 0005](docs/ADR/0005-rust-ui-egui.md)).
- **Porting a file to Rust: follow the `port-to-rust` skill** (`.claude/skills/port-to-rust/SKILL.md`). It covers the `PORT-SOURCE` / `PORT-SHA` / `PORT-STATUS` headers (keep them when editing, and don't trust a `done` header), `PORT_MAP.tsv`, and registering spec types, since Rust has no `Type.GetType`. An unregistered family fails with "unknown ArchiveType".
- **Stale tests exist in all three ports.** A test module referring to names that no longer exist is stale, not a signal the code is wrong.
- **Line endings on Windows.** `.rs`, `.md` and `.tsv` files are LF; `.cs` are CRLF + BOM (see Conventions); `.py` files are mixed (about 60% CRLF), so keep each file's endings. Python's text mode writes CRLF on Windows (open with `newline="\n"`), Git Bash's `sed -i` drops the CRs of a CRLF file, and PowerShell's `Set-Content -Encoding utf8` adds a BOM; check with `file` after a scripted edit.
- **No reflection in Rust.** Archive types, engines and apps are found by registration (the `port-to-rust` skill, step 3); a Rust Explorer that can't open a family or list an engine is usually missing a `register()` or `EngineX::EnginesUnionWith` call.

## Conventions

- C#: file-scoped namespaces, K&R braces (opening brace on the same line), primary constructors, 4-space indent, `#region` blocks, `///` summaries. Files are **CRLF with a UTF-8 BOM** — preserve both when editing `.cs`, `.csproj`, `.sln`. (`dotnet sln add` rewrites the `.sln` with LF; convert it back.)
- Python: camelCase method names mirroring the C# (`getFamily`, `parseHash`), one-line bodies are common. Needs Python ≥ 3.10 (`match`), despite what `setup.cfg` says.
- Rust: C# names in camelCase, exactly as Python does: `decodePath`, `getArchive`, `fileSize`; types and traits keep C#'s PascalCase (`ISource`, `I*` traits). **Never write snake_case names**, and convert existing snake_case names to camelCase in any file you touch. Naming and unused-code warnings are allowed once per workspace (`[workspace.lints.rust]` in `OpenX/rust/Cargo.toml` and `rust/Cargo.toml`); every crate's `Cargo.toml` has `[lints]` / `workspace = true`, so add that to any new crate. One `.rs` per `.cs`.
- A C# sub-project `GameX.X.Sub` (e.g. `GameX.WB.Props`, `GameX.Bethesda.Platform`) is the Rust crate `rust/families/x-sub` (`gamex-x-sub`) and the Python module `python/gamex/families/GameX_X_Sub.py` with its files under `X_Sub/`. Keep the three in step when a project is split or added.
- Where things live (OpenX only; GameX stays one-to-one with C#, file for file): C# polyfills for .NET libraries map to the matching Python/Rust library, not to copies of C#'s namespace folders. The table of homes is [`OpenX/Polyfills.md`](OpenX/Polyfills.md); the rules are [ADR 0002](docs/ADR/0002-library-homes-for-polyfills.md). E.g. `System.Numerics` → glam, `Int3` → `openx_poly::poly` alias of `glam::IVec3`. Update the table when something moves.
- C# value types ([ADR 0003](docs/ADR/0003-value-types.md)): ports match C#'s behaviour, not its lines. Python creates C#'s implicit defaults and copies explicitly (marked `# C# value type`); Rust structs derive `Clone`/`Default` (and `Copy`), `default(T)` is `Default::default()`, nullable classes are `Option<T>`.
- Rust slim builds: a module that doesn't compile yet stays declared and gated with `#[cfg(feature = "full")]`; `cargo build` compiles the working set, `cargo check --features full` shows what's left. Don't move files to make a crate compile. Folders named `_/` (e.g. `OpenX/rust/core/poly/_/`) hold code the owner parked on purpose. Move individual features, functions or types back out of `_/` when something needs them; don't un-park whole folders.
- C# caches keyed on `(ISource source, object path)` rely on reference identity; the Rust equivalent is `openx_poly::core::SourceRef`.
- Match the surrounding file's density and idiom over general style preferences.
- Prefer lean code. Don't add defensive checks, validation or hardening the code doesn't already have unless asked; fix the bug in front of you and stop.

## Known, accepted exceptions

Don't fix or report these:

- **Archive paths aren't sanitized.** `ExportManager`/`ImportManager` (C#) and `exportManager.py` combine archive entry names with the output folder unchecked, so a `../` or rooted entry name can write (or, on import, read) outside that folder. This is deliberate: GameX trusts the archives it opens.
- **Ultima Online is C# until it is merged.** The client port (`dotnet/Families/GameX.Origin.UO`, `dotnet/Families/GameX.Origin/Clients/UO`, and MonoGame host changes that exist only to run that client) does not get a Python or Rust twin yet. Do not add `gamex-origin-uo` to the Rust workspace or keep `GameX_Origin_UO.py` in step. ADR 0001 applies again when the owner says the client is merged. Origin resource parsers that already have twins stay in all three languages.

## Git

- The owner commits as plain `updates` and works from more than one machine. Pull before starting; never force-push `master`.
- Work in the main checkout on `master`, even when the session starts in a `.claude/worktrees/` worktree: the owner watches the changes there as they happen. Move any work already in a worktree across first (`git diff --binary` | `git apply --binary`), then use paths into the main checkout. A hook blocks the Edit and Write tools outside the session's worktree, so edits there go through scripts (keep each file's line endings); starting the session in the main checkout avoids that.
- Work lands on `master` directly (no PRs): when asked to commit, commit on `master` and push it; delete any branch you made.
- Commit or push only when asked.
- `*.orig` / `*.rej` are ignored; don't commit merge leftovers.
