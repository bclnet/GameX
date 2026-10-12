# TODO

What's next, in order. Pick from the top, and update this file in the same commit as the work: remove what's done and add what you find. Rules and conventions are in [CLAUDE.md](CLAUDE.md); decisions are in [docs/ADR](docs/ADR/Readme.md).

Last updated 2026-10-12, after commit 585c66a.

## Where things stand

- **Both Rust workspaces build clean.** `cargo check --workspace` gives zero errors and warnings in `OpenX/rust` and `rust/`, and so does `cargo check -p openx --features full`. `cargo test -p gamex` passes 60 tests.
- **Rust port: 70 files done, 328 still `PORT-STATUS: todo` stubs.** The core model is done: `rust/core/gamex` has `family.rs` (Family.cs), `binary.rs` (Binary.cs), `meta.rs`, `util.rs`, `config.rs`, `store.rs` and `formats/stream.rs`. The specs are embedded by `rust/core/gamex/build.rs`.
- **The Rust Explorer runs on egui** (ADR 0005): `cargo run -p gamex-app-explorer` from `rust/`. The Open dialog lists families and games. Opening an Uncore `file://<dir>#APP` folder shows its file tree, info tree, and text or hex content.

## 1. Make a real game open in the Rust Explorer

The Explorer preselects Arkane / Arx Fatalis from the config, so Arkane is the first target. Today a real game fails with "unable to find archive" or "unknown ArchiveType".

- [ ] **Port Store.cs's store maps** into `rust/core/gamex/src/store.rs`. Only `Direct:` keys resolve today, so only Uncore's games are found. Steam, GOG, Epic and the rest need their paths. Store detection only fully works on Windows (CLAUDE.md).
- [ ] **Register archive types from each family crate.** Rust has no reflection: C#'s `Type.GetType` is `registerType` / `findType` in `family.rs`, keyed by the C# type name in the spec. Copy `rust/families/uncore/src/uncore.rs`'s `register()`, and call it from the Explorer's `App` at startup. Start with Arkane (2 stub files).
- [ ] **Port Uncore's `Formats/Binary.cs`** (`Binary_Txt`, `Binary_Img` and the rest). Until then images show as hex. It has no PORT_MAP row yet; add one.

## 2. Finish the Rust Explorer's views

- [ ] **Engine view** (`TViewGfx`): host OpenX's GL engine in an egui paint callback. OpenX's glow is pinned to 0.17 to match eframe (ADR 0005). The engine list currently shows only Unknown.
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

Families with the most stub files: IW 31, Cig Explorer 25, Uncore 22, Crytek 19, WB 17, Origin 13, Blizzard 13, Epic 11. Most others have 1 to 8. Families that Python already has are the easiest start, because Python shows the camelCase shape.

## 5. Smaller known issues

- [ ] **Python doesn't decode `%20` in file URIs.**
- [ ] **Arkane's blast decompression** fails on `.bmp` exports (143 errors). This predates the Rust work.
- [ ] **Python's Bullfrog and Beamdog readers** need porting properly from C#.
- [ ] **6 Rust files still name `Applications/` in their `PORT-SOURCE` header**, as do 6 rows of `rust/_/PORT_MAP.tsv`. C#'s folder is now `dotnet/Apps/`.
- [ ] **`bash rust/_/sync-check.sh` reports 13 STALE `done` files**, where the C# changed after the port: `ImportManager`, `Client`, `DesSer`, `Globalx`, `Store`, `_Config`, `Collada`, `RootHandlers`, Bethesda `Records`, Gamebryo `Nif`, Uncore `Network`, and WB `Enums` / `Enums+Weenie`. Port each change, then bump its PORT-SHA. It also reports 13 MISSING rows: the 6 `Applications/` ones, `Gamebryo/Formats/Nif.o.cs`, and 6 `GameX.Valve.Platform/OpenGL` files. And it reports 97 C# files with no row, 22 of them in the C#-only `Apps/GameX.App.ExplorerM`.
- [ ] **Some `done` headers cover only enums.** `families/bethesda/src/formats/records.rs` holds just the generated enums of `Records.cs`, which also has about 350 classes. Check the other `gen_enums.py` outputs the same way.
- [ ] **`gen_enums.py` emits `from_raw`.** That's snake_case, so rename it to `fromRaw` in the generator and regenerate.
- [ ] **Porting scripts with dead paths:** `rust/_/stamp.py` and `gen_gamex.py` point at `/home/claude/...`. `OpenX/rust/_/sync-check.sh` has no `PORT_MAP.tsv` to read and still defaults to `../src/dotnet`.
- [ ] **Stale `#[cfg(test)]` modules** don't compile, e.g. `openx-poly`'s `reader.rs` and `openx-vfx`'s `disc_nrg.rs`. Fix or remove them when you touch those files.

## Dates

- **2027-10-11: recheck the Rust UI choice** ([ADR 0006](docs/ADR/0006-review-rust-ui-2027.md)), including whether eframe has moved past glow 0.17.
