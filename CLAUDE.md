# GameX

GameX loads game assets (textures, models, levels, audio) out of the archive formats of ~36 studios/engines ("families") and hands them to a rendering engine. The C# tree is the hand-written original; Python is a partial hand port; Rust is an in-progress AI-assisted port.

## Layout

| Path | What it is |
| -- | -- |
| `dotnet/` | GameX in C# — the reference implementation. `Core/GameX` (Family → Game → Archive model, store detection), `Families/GameX.*` (one project per family), `Engines/GameX.Engine.*`, `GameX.Cli`, `Apps/` (WPF/MAUI explorers, Windows-only) |
| `python/gamex/` | Python port. Same structure, fewer families implemented |
| `rust/` | Rust port. Most files are `PORT-STATUS: todo` stubs; see `rust/_/PORTING.md` and `PORT_MAP.tsv` |
| `OpenX/` | The engine-abstraction library GameX sits on (`OpenX.Poly`, `Gfx`, `Sfx`, `Vfx`, `Engines/OpenX.Engine.*`), vendored in-tree in all three languages. Formerly named OpenStack |
| `Mirage/` | Headless test engine (scene graph with a text form). Mirrors github.com/bclnet/Mirage |
| `python/gamex/specs/*.json` | Family/game definitions. **Single source of truth** — the C# build embeds these same files |
| `third_party/` | Proprietary DLLs (Oodle, XCompress), excluded from the MIT license. Don't copy them elsewhere |
| `book/`, `docs/`, `resources/`, `posters/` | Documentation and data; rarely relevant to code changes |

Each engine exists at two layers: `OpenX.Engine.X` (implements `Engine` and the `IOpenGfx*` slots) and `GameX.Engine.X` (`XRenderer.CreateRenderer`, family object builders). Engine codes: `GL` OpenGL, `UN` Unity, `UR` Unreal, `GD` Godot, `SD` SDL, `ST` Stride, `OG` Ogre, `MG` MonoGame, `EX` EginX, `MR` Mirage, `TT` Test, `UK` Unknown.

## Build and test

Build individual projects, not the solution — `GameX.sln` includes WPF/MAUI/Unity projects that only build on Windows.

```bash
dotnet build dotnet/GameX.Cli/GameX.Cli.csproj
dotnet build dotnet/Core/GameXTests/GameXTests.csproj      # builds every family
```

Tests that run anywhere (no game files needed):

```bash
dotnet test Mirage/dotnet/Mirage.sln
dotnet test OpenX/dotnet/Engines/OpenX.EngineTests/OpenX.EngineTests.csproj --filter "FullyQualifiedName~MirageEngineTests"
cargo test -p openx-gfx --manifest-path OpenX/rust/Cargo.toml
```

Everything else is an integration test against installed games and is expected to fail on a machine without them: `GameXTests` (most of its ~350 tests), `python/tests`, and the OpenGL tests in `OpenX.EngineTests` (need a GL context). Many of those tests are also stale (old family ids like `AC`, `Tes`, `Cry`). Don't treat their failures as regressions; don't "fix" them unless asked.

To test GameX code without a GPU or game install, use the Mirage engine: `MirageEngine.Reset()`, render through `MirageRenderer.CreateRenderer(...)`, assert on `MirageEngine.Tree.ToText()`. See `docs/Engines/Mirage/Readme.md`.

Rust: both workspaces resolve, but check per crate (`cargo check -p <crate>`), not `--workspace`. Currently compiling in `OpenX/rust`: `openx-poly`, `openx-gfx`, `openx-gfx-other`, `openx-phy`, `openx-aix`, `openx-sfx*`. Blocked: `openx-vfx` (`disc_mds.rs`) and `openx-gfx-egin` (needs `KV` from `core/poly/_/`), which block `openx` and everything in `rust/`.

## Things that will bite you

- **The CLIs have hardcoded debug args.** `dotnet/GameX.Cli/Program.cs` and `python/gamex/cli/_cli.py` contain commented `args = [...]` / `sys.argv = [...]` lines the owner toggles for testing. If one is uncommented, the CLI ignores its real arguments, and on macOS/Linux it creates literal `D:\...` directories in the cwd. Leave these lines as they are; clean up any such directory you create.
- **`dotnet/Core/GameX/_Config.cs` and `python/gamex/_config.py`** select a debug family/game via `#define` / options. Also owner-toggled; not a bug.
- **`import gamex` has side effects** — it runs `init()`, which opens archives.
- **Store paths are personal** (`E:\AbandonLibrary`, `G:\...`). Game detection in `Core/GameX/Store.cs` only fully works on Windows.
- **Ports must stay in step.** A change to a format or to core behaviour in C# usually has a Python twin (`python/gamex/families/<Family>/...`) and a Rust file listed in `PORT_MAP.tsv`. Say which ports you did and didn't update.
- **Rust port files carry `PORT-SOURCE` / `PORT-SHA` / `PORT-STATUS` headers**; keep them when editing.
- **Stale tests exist in all three ports.** A test module referring to names that no longer exist is stale, not a signal the code is wrong.

## Conventions

- C#: file-scoped namespaces, K&R braces (opening brace on the same line), primary constructors, 4-space indent, `#region` blocks, `///` summaries. Files are **CRLF with a UTF-8 BOM** — preserve both when editing `.cs`, `.csproj`, `.sln`. (`dotnet sln add` rewrites the `.sln` with LF; convert it back.)
- Python: camelCase method names mirroring the C# (`getFamily`, `parseHash`), one-line bodies are common. Needs Python ≥ 3.10 (`match`), despite what `setup.cfg` says.
- Rust: mirrors C# names too (camelCase methods, `ISource`, `I*` traits) with `non_snake_case` allowed; one `.rs` per `.cs`.
- C# caches keyed on `(ISource source, object path)` rely on reference identity; the Rust equivalent is `openx_poly::core::SourceRef`.
- Match the surrounding file's density and idiom over general style preferences.

## Git

- The owner commits as plain `updates` and works from more than one machine. Pull before starting; never force-push `master`.
- Commit or push only when asked.
- `*.orig` / `*.rej` are ignored; don't commit merge leftovers.
