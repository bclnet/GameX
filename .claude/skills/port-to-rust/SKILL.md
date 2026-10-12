---
name: port-to-rust
description: Checklist for porting one C# (or Python) file of GameX or OpenX to Rust, such as filling a `PORT-STATUS: todo` stub under rust/ or OpenX/rust/, finishing a partial port, or re-syncing a Rust file whose C# source changed. Use whenever a task in this repo writes or updates Rust from its C# or Python twin, e.g. "port X to Rust", "fill in the Rust stub for X", "do the Rust side of this change", or a TODO.md item that needs a Rust file written.
---

# Port one C# file to Rust

The rules are in [CLAUDE.md](../../../CLAUDE.md) (naming, value types, `full` gating, `_/` folders, lean code) and the ADRs it links. This is the order to apply them in, one file per pass. Run every command from the repo root.

## 1. Find the source, its map row and its twins

- [ ] `git pull` first. The owner works from more than one machine.
- [ ] Read the whole C# file under `dotnet/` (OpenX: `OpenX/dotnet/`). C# is the reference.
- [ ] Find its row in `rust/_/PORT_MAP.tsv`:
  ```bash
  grep -F 'GameX.Arkane/Arkane.cs' rust/_/PORT_MAP.tsv
  ```
  The columns are `status  cs_loc  cs_sha256_16  cs_path  rs_path  crate`. `cs_path` is relative to `dotnet/` and `rs_path` is relative to `rust/`. OpenX has no map; there, the `.rs` file's `PORT-SOURCE` header is the link, and polyfills go where [OpenX/Polyfills.md](../../../OpenX/Polyfills.md) says ([ADR 0002](../../../docs/ADR/0002-library-homes-for-polyfills.md)).
- [ ] **No row?** Add one, named by the stub generator's own rules. This prints `cs_loc`, the hash and the path inside the crate's `src/`:
  ```bash
  python3 -c 'import sys; sys.path.insert(0, "rust/_"); from pathlib import Path; from gen_gamex import rust_path_for, live_loc, sha; p = Path(sys.argv[1]); print(live_loc(p), sha(p), rust_path_for(p.relative_to(sys.argv[2])))' dotnet/Families/GameX.Uncore/Formats/Binary.cs dotnet/Families/GameX.Uncore
  ```
  Copy the crate directory and crate name from a row of the same project (`grep -m1 'GameX.Uncore/' rust/_/PORT_MAP.tsv`). Write the row as `todo`, create the `.rs` file with the three headers, and declare it with `pub mod` in its parent.
- [ ] Find the Python twin. It already has the camelCase shape, so start from it when it exists:
  - `Core/GameX/Foo.cs` → `python/gamex/core/foo.py`
  - `Families/GameX.Bar/Bar.cs` → `python/gamex/families/GameX_Bar.py`
  - `Families/GameX.Bar/Formats/...` → `python/gamex/families/Bar/formats/...`
  - `OpenX/dotnet/...` → `OpenX/python/openx/...`

  If there's no twin, say so in the change description ([ADR 0001](../../../docs/ADR/0001-ports-stay-one-to-one.md)).
- [ ] Read the current `.rs` file and **check it against the C#, member by member**, even if it says `done`. `family.rs` and `meta.rs` were marked `done` while missing most of their types, and the generated `families/bethesda/src/formats/records.rs` is `done` with only the enums of a 9,000-line file.
- [ ] Check what it uses. If a dependency is still a `todo` stub, port that first, or gate this one (step 4).

## 2. Write it

- [ ] Use C#'s names: members in camelCase, types in PascalCase, never snake_case (CLAUDE.md, Conventions). File and module names come from `PORT_MAP`'s `rs_path`.
- [ ] Use one `.rs` per `.cs`, with the same members in the same order and the same control flow (ADR 0001). Value types follow [ADR 0003](../../../docs/ADR/0003-value-types.md).
- [ ] Reuse the shapes the core already settled, and don't invent new ones. The header of `rust/core/gamex/src/binary.rs` lists them: `Archive` trait + `ArchiveExt`, C# `object` → `meta::Tag`, `Task<T>` → boxed future waited on with `getResult`, `Stream`, `AssetFactory`. Compare it with `dotnet/Core/GameX/Binary.cs` for a finished example.
- [ ] Where Rust or OpenX's Rust port can't do what C# does, use the nearest equivalent and say so in the header (binary.rs's "Not yet in openx's Rust port" list).
- [ ] Keep C#'s behaviour, bugs included. Record what you find in `rust/_/PORTING.md` instead of fixing it in one language only.
- [ ] **Enum tables are generated, not transcribed:**
  ```bash
  python3 rust/_/gen_enums.py dotnet/<cs_path> rust/<rs_path> <cs_path>
  python3 rust/_/verify_enums.py dotnet/<cs_path>:rust/<rs_path>
  ```
  The verifier must print `OK ... missing= 0 mismatched= 0`. The generator writes `PORT-SHA: PLACEHOLDER` and `PORT-STATUS: done` even when the C# file has more than enums, so set both by hand (step 5). It also emits `from_raw`. That snake_case name is the generator's; fix it in `gen_enums.py`, not in the output.

## 3. Register types a spec names

Rust has no `Type.GetType`. If the C# type appears in a spec (`grep -l 'GameX.Arkane.ArkaneArchive' python/gamex/specs/*.json`), register it under the exact spec string:

- [ ] Add a `register()` to the family's main file, as `rust/families/uncore/src/uncore.rs` does:
  ```rust
  pub fn register() {
      registerType::<ArchiveFactory>("GameX.Arkane.ArkaneArchive, GameX.Arkane", |parent, state| Ok(ArkaneArchive::new(parent, state)));
  }
  ```
  The factory type follows the spec key (`rust/core/gamex/src/family.rs`): `archiveType` → `ArchiveFactory`, `vfxType` → `FileSystemFactory`, `clientType` → `ClientFactory`, `explorerAppType` → `AppStartup`.
- [ ] Call it at startup in `rust/apps/app-explorer/src/app_xaml.rs`, next to `gamex_uncore::uncore::register();`. Until then, opening that family fails with "unknown ArchiveType".

## 4. Build

- [ ] GameX:
  ```bash
  cd rust && cargo check --workspace
  ```
- [ ] OpenX, when you touched it:
  ```bash
  cd OpenX/rust && cargo check --workspace && cargo check -p openx --features full
  ```
- [ ] Tests for the crate you changed (`gamex` passes 60):
  ```bash
  cd rust && cargo test -p gamex
  ```
  Some crates' own `#[cfg(test)]` modules are stale (CLAUDE.md), so test per crate.
- [ ] Aim for zero errors and no new warnings.
- [ ] **If it can't compile yet** because it needs unported code, keep it declared and gate it in the parent with `#[cfg(feature = "full")] pub mod foo;`. Give the crate `[features] full = []` if it lacks one (`OpenX/rust/vfx/vfx/Cargo.toml`). Don't move files to make a crate build, and don't un-park whole `_/` folders.
- [ ] A new crate needs `[lints]` / `workspace = true`.

## 5. Header and map: only when complete

- [ ] **Complete** means every member of the C# file is ported and it builds without `full`. Then hash the C# file as checked out (raw bytes, BOM and CRLF included):
  ```bash
  sha256sum dotnet/<cs_path> | cut -c1-16
  ```
  Set `// PORT-SHA: <hash>` and `// PORT-STATUS: done`, and keep `PORT-SOURCE`. In the PORT_MAP row, set `status` to `done` and `cs_sha256_16` to the same hash.
- [ ] Complete, but it calls into an unported file: still `done`, plus a `// PARTIAL PORT: <what's missing and where>` line (`uncore.rs`).
- [ ] **Not complete:** leave the hash alone. Use `// PORT-STATUS: partial` with a header listing what isn't ported (`OpenX/rust/vfx/vfx/src/disc_mds.rs`), or leave it `todo`.
- [ ] Don't run `rust/_/stamp.py`. It points at `/home/claude/...` and re-stamps every `done` file, which would hide real drift.
- [ ] Check drift for your file. This should print nothing once the file is `done` and current:
  ```bash
  bash rust/_/sync-check.sh | grep -F -A2 '<cs_path>'
  ```
  The script exits non-zero today because of rows that were already STALE or MISSING. Only your file's line matters.

## 6. Other languages, TODO.md, commit

- [ ] If the port needs a change to C# or Python, make it in all three in the same change. List each language as updated, not applicable or still to do (ADR 0001).
- [ ] Update [TODO.md](../../../TODO.md) in the same change: remove what's done, add what you found, and refresh the counts in "Where things stand":
  ```bash
  cut -f1 rust/_/PORT_MAP.tsv | sort | uniq -c
  ```
- [ ] Commit or push only when asked (CLAUDE.md, Git).
