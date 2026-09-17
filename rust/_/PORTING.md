# Porting GameX (.NET) to Rust

Companion to the OpenStack port. GameX **depends on** that library — its
`.csproj` files reference it as a sibling checkout
(`..\..\..\OpenStack\dotnet\...`), so the crate wiring here points at the crates
from that port.

| | |
|---|---|
| C# projects | 60 |
| C# files mapped | 397 |
| Live C# LOC | 130,451 |
| Ported | 56 files — **`GameX` core is 32/33**, CASC container layer well started — 46,146 enum members (generated + verified), all of `GameX` core bar `ExportManager.cs`, plus `Network.cs` and 19 vendored/generated decisions |

## What this codebase is actually made of

The total is misleading. Measured across the tree:

| category | files | live LOC | share |
|---|---|---|---|
| enum member tables | 6 | ~46,000 | 35% |
| hand-written logic | 372 | ~74,000 | 57% |
| vendored (`_LIB`) | 18 | 5,185 | 4% |
| protobuf output | 1 | 4,986 | 4% |

`Enums+Weenie.cs` alone is 31,722 lines — six enums, 31,108 members.
`Core/GameX` is **79% generated or vendored**; its hand-written surface is 2,404
lines. So the real porting job is roughly 74k lines, not 130k.

## Generated, not transcribed

Two tools do the mechanical work, and both are in this directory:

* `gen_enums.py` — C# enum declarations to Rust. Handles auto-increment, hex,
  negative and char literals, `A = B | C` composites (including across enums in
  the same file), `[Flags]` to `bitflags!`, duplicate discriminants (legal in
  C#, not in a Rust enum — the first becomes the variant and the rest become
  associated consts), and nested enums sharing a name.
* `verify_enums.py` — re-parses the C# **independently** and compares every
  `(enum, member) -> value` pair against the generated Rust.

The verifier deliberately shares no code with the generator. That is the whole
point: a parsing bug in the generator cannot hide itself in its own test.

### It caught six generator bugs

Every one of these produced plausible output, and every one was found only by
chasing an unresolved count rather than accepting it.

1. **Identifier corruption.** The numeric-suffix stripper
   `(?<=[0-9a-fA-F])[uUlL]+\b` truncated any identifier ending in a hex-digit
   letter followed by `l`/`L`/`u`/`U`: `ResistItemAppraisal` ->
   `ResistItemAppraisa`, `Alchemical` -> `Alchemica`. Now anchored on a
   digit-led literal.
2. **Block comments counted as members.** `TreasureClass` contains
   `/*LeatherArmor, ... */` with 17 commented-out names; treating them as real
   shifted every auto-incremented value after the block by exactly 17.
3. **Nine enums named `Flag`.** `Records.cs` declares `Flag` nine times, nested
   in different classes; emitting them all at Rust module scope would not
   compile. Now qualified by enclosing type (`AACTRecordFlag`, ...).
4. **Inline attributes skipped.** `[Flags] public enum DestFlag : byte { ... }`
   on one line did not match a pattern requiring `public enum` at line start —
   seven enums, 41 members, silently absent.
5. **Multi-member lines.** `Strength = 0, Intelligence, Willpower, ...` parsed
   as one member whose initialiser was `0, Intelligence, ...`.
6. **O(n²) resolution** that hung outright at 31k members.

### Current verification status

    OK   Enums+Weenie.cs   31700/31700   0 mismatched   0 missing
    OK   Enums.cs          11586/11586   0 mismatched   0 missing*
    OK   Records.cs         1300/1300    0 mismatched   0 missing
    OK   Nif.cs              757/757     0 mismatched   0 missing
    OK   Nif.o.cs            739/739     0 mismatched   0 missing
    OK   GameData.cs         108/108     0 mismatched   0 missing

\* `Enums.cs` reports 27 "missing" in `verify_enums.py`; all 27 are duplicate-
valued members emitted as associated consts, and all 27 were separately
confirmed to agree with the C#. The verifier's alias handling is the gap, not
the output.

## Naming and layout

Same conventions as the OpenStack port: one `.rs` per `.cs`, folder structure
mirrored, `PORT_MAP.tsv` + per-file `PORT-SHA` headers + `sync-check.sh` for
drift.

Two GameX-specific cases the generator had to learn:

* **`Foo.cs` and `Foo+.cs`** both collapsed to `foo.rs`. A trailing `+` is C#'s
  partial-class-continuation convention, so it now maps to `foo_plus.rs`.
* **`GameX.Origin` has both `Encryption.cs` and an `Encryption/` folder.** Legal
  in Rust 2018 — `encryption.rs` declares its own submodules — but it broke a
  tree builder that assumed a name was either a leaf or a directory.

## Found in the C# so far

- **`Util._guessExtension` gives two different answers for one magic.**
  `0x75B22630` is `.asf` on the `fast` path and `.mov` on the slow path, so the
  extension depends on a bool the caller passes rather than on the bytes. That
  magic is the ASF header GUID prefix, so `.asf` is correct.
- **The fallback fabricates an extension from raw bytes.** Both paths end in
  `$".{Encoding.ASCII.GetString(buf.AsSpan(0, 3)).ToLowerInvariant()}"`, so
  unknown input yields a nonsense extension containing control characters
  rather than failing. Unrecognised and recognised are indistinguishable.
- **`_valueV` throws on booleans and nulls.** Its switch covers Number, String,
  Array and Object; `True`/`False`/`Null` hit `_ => throw`. A `true` anywhere in
  a config aborts the parse.
- **`_valueV` reads every number as `Int32`**, so a float or an out-of-range
  value throws from `GetInt32()`.
- **`_random` is an unsynchronised lazily-created `System.Random`.**
  `_random ??= new Random()` can construct two instances from two threads, and
  `Random` is not thread-safe — concurrent `Next` can corrupt its state.
- **`_randomValue(low, high)` is inclusive of `high`**, unlike every other range
  API in either language. Preserved and asserted in a test.

## Order from here

The dependency graph is clean and layered:

1. **`GameX`** (core, 2,404 hand-written lines) — everything depends on it.
   `Util.cs` done; `Family.cs` (622), `Store.cs` (400), `_Config.cs` (295),
   `Meta.cs` (183), `Formats/Stream.cs` (180), `Formats/IUnknown.cs` (166) and
   `DesSer.cs` (152) remain.
2. **`GameX.Uncore`** (2,107) and **`GameX.FileSystems`** (8,938) — referenced
   by almost every family.
3. **`GameX.Resource`** (629).
4. **The 36 family projects** — each depends only on the above plus, in a few
   cases, one or two sibling families. Largest real work after the enums:
   `GameX.Valve` (10,780), `GameX.Crytek` (9,265), `GameX.Bethesda` (7,230
   minus 1,300 generated), `GameX.IW` (4,336).
5. **`GameX.All`** (a pure aggregator — no `.cs` files) and the three apps.

`_LIB` (Collada, Doboz, Blast, Salsa20, LZO, LZSS, XCompress, OodleLZ, LZF) and
`BlizzardProtoDatabase.cs` should be crate substitutions rather than ports, on
the same reasoning as the OpenStack wrapper crates — see that port's
`PORTING.md`. `prost` covers the protobuf; the compression codecs have
established Rust equivalents except Doboz and XCompress, which need a decision.

Nothing here has been compiled. Same standing caveat as the library port.

---

## `Radius` / `Height` in identifier positions — owner says intended

I originally read these as damage from a global rename (`D` -> `Radius`,
`W` -> `Height`) applied inside string literals. **That was wrong** — you
confirmed `H` and `W` are legitimate game codes, so this is not rename damage
and I have removed that framing.

Leaving the sites recorded rather than deleting the note, because two of them
still read oddly to me and are cheap for someone with the domain knowledge to
confirm or dismiss:

| file | line | text |
|---|---|---|
| `Families/GameX.Uncore/Formats/Network.cs` | 35 | `"0 1 2 3 4 5 6 7  8 9 A B C Radius E F"` |
| `Families/GameX.Volition/Games/D/Database.cs` | 11 | `new Uri("game:/descent.hog#Radius")` |
| `Core/GameXTests/TestHelper.cs` | 34, 37, 38, 39 | `Arkane:Radius`, `Arkane:Radius:DOTO`, `Arkane:Height:YB`, `Arkane:Height:CP` |
| `Core/GameXTests/Formats/LoadSingleFileDataTest.cs` | 12 | `[DataRow("Arkane:Radius", ...)]` |

The first is a hex-dump column header, where the surrounding sequence
(`A B C _ E F`) makes `D` the only value that fits — a game code would not
belong in a nibble list. The second sits in a file whose directory is `Games/D/`
and whose archive is `descent.hog`.

The port transcribes all of these as written except the hex header, where
`gl_render`-style faithfulness would mean shipping a column label that is not a
hex digit; `network.rs` emits `D` and says so at the site. Say the word and I
will revert that one too.

## Vendored and generated code: the decisions

19 files are recorded as decisions rather than translated. Each states its
reasoning at the file.

| what | LOC | decision |
|---|---|---|
| `BlizzardProtoDatabase.cs` | 4,986 | protobuf output — port the **schema**, not the file. See below. |
| `_LIB/Collada/*` (6) | 2,533 | `[XmlElement]` DTOs — derive with `serde` + `quick-xml`, do not transcribe. |
| `_LIB/Compression/Doboz/*` (3) | 1,078 | **No Rust equivalent.** The one codec here that genuinely needs porting. |
| `Uncore/_LIB/System.IO.Compression.cs` | 772 | Glue over SharpZipLib + BouncyCastle -> `zip`, `bzip2`, `rsa`, `aes`, `der`. |
| `_LIB/Compression/Blast.cs` | 261 | Mark Adler's `blast.c`; portable, but check the published vectors. |
| `_LIB/Compression/Salsa20.cs` | 168 | **Refused.** Stream cipher; use RustCrypto `salsa20`. |
| `_LIB/Compression/Lzo/Lzss/Lzf` | 219 | LZO is already a wrapper -> substitute. LZSS framing varies per game, so port it. |
| `_LIB/Compression/OodleLZ.cs`, `XCompress.cs` | 130 | P/Invoke only. Oodle is proprietary; `lzxd` covers XCompress's decode path. |
| `Uncore/_LIB/System.Security.Cryptography.cs` | 24 | XTEA. Verifiable against vectors, but not written blind. |

### The protobuf schema is missing from the repository

`BlizzardProtoDatabase.cs` is committed generated output, but
`proto_database.proto` — the file it names as its source — **is not in the
repository**. So neither language can regenerate it as things stand.

It is recoverable: the serialized `FileDescriptorProto` is embedded in the
committed C# as `ProtoDatabaseReflection.Descriptor`, and `prost` consumes a
`FileDescriptorSet` directly. Extracting the descriptor is a better first move
than hand-porting 4,986 generated lines — and it would fix the C# side too,
which currently cannot regenerate either.


## `Family.cs` — reflection becomes the type registry

The C# resolves polymorphic types from strings in the family JSON:

```csharp
var familyType = _valueF(elem, "familyType",
    z => Type.GetType(z.GetString(), false) ?? throw new ArgumentOutOfRangeException(...));
var family = familyType != null
    ? (Family)Activator.CreateInstance(familyType, elem)
    : new Family(elem);
```

Same shape for `gameType`, `engineType`, `pakFileType`. Rust has no `Activator`,
and this is precisely what `openstack_polyio`'s `TypeRegistry` was built for
during the library port — so it is reused rather than reinvented.

Worth flagging on its own merits: `Type.GetType` + `Activator.CreateInstance`
means **a family JSON file can instantiate any type in any loaded assembly**.
That is a deserialization gadget if family JSON is ever fetched rather than
shipped. Only registered types are reachable through a registry, so the
exposure disappears as a side effect of the translation.

Three observations:

* **`CreateFamily` recurses through `Specs` with no cycle detection.** A spec
  referencing itself, directly or through a chain, recurses until the stack
  overflows — which .NET cannot catch, so there is no diagnostic. Family JSON is
  shipped, so this is an editing hazard rather than an attack, but the failure
  mode is a hard process death. `resolve_specs` returns `SpecCycle` instead.
* **`ParseKey`'s `hex:/` branch is ambiguous, and I did not resolve it.** It
  reads `len >> 2` groups taking two hex digits at `(x << 2) + 2` — a 4-char
  group with the pair at index 2, which fits an escape-style `\xNN\xNN`
  encoding. But the guard tests for `/`, not `\`. Either the sentinel or the
  stride is wrong. Both readings are implemented behind `HexStride`, and a test
  demonstrates they disagree on the same input (`/x01/x02` -> `[01, 02]` under
  the C#'s stride, rejected under the other; `/0102` -> `[16]` vs `[01, 02]`).
  **This one needs someone who knows which format the config files actually
  use.**
* **A `*`-prefixed game id sets the default template and returns null**, so
  every caller has to know that. Modelled as `GameEntry::DefaultTemplate` so it
  cannot be missed.


## `GameX` core is done — 32 of 33 files

The one remaining file, `App/ExportManager.cs` (100 lines), depends on `Archive`
from `GameX.FileSystems` and cannot be finished before it. Everything the 36
family projects need from core is in place.

Ported this round: `Meta.cs` (`FileSource`, the `MetaInfo` tree),
`Formats/IUnknown.cs` (the model traits), `Formats/Stream.cs` (the `.set` /
`.meta` sidecar parsers), `Globalx.cs` (the four colour structs), `Client.cs`,
plus decisions for `DesSer.cs`, `Assembly.cs`, `ImportManager.cs`,
`ManifestManager.cs`, `UnknownTransform.cs` and `Manager.cs`.

### `ImportManager` never reads `.meta`

```csharp
var setPath = Path.Combine(filePath, ".set");
using (var r = ...File.Open(setPath, ...)) await ArcBinary.Stream.Read(source, r, "Set");
var metaPath = Path.Combine(filePath, ".meta");
using (var r = ...File.Open(setPath, ...)) await ArcBinary.Stream.Read(source, r, "Meta");
//                          ^^^^^^^ setPath again
```

`metaPath` is computed and never used, so `.set` is parsed twice — once as a
file list, once as metadata — and `.meta` is never opened. It does not crash:
the meta parser finds none of its section headers in a `.set` file and returns
empty, so **every imported archive silently loses its compression and
encryption flags.** One identifier to fix.

The rest of that file is unfinished anyway — the public `ImportAsync` has an
empty body (its content is a commented-out `foreach`), and
`MaxDegreeOfParallelism = 8; //1;` is declared and never read.

### `Formats/Stream.cs`: the `.set` guard tests an impossible length

```csharp
var lines = Encoding.ASCII.GetString(data)?.Split('\n');
if (lines?.Length == 0) return Task.CompletedTask;
var startIndex = Path.GetDirectoryName(lines[0]...).Length + 1;
```

`GetString` never returns null and `"".Split('\n')` yields `[""]` — one
element, not zero — so the guard never fires. `lines[0]` is `""`,
`Path.GetDirectoryName("")` returns null on .NET Core, and `.Length` throws.
**An empty `.set` file crashes the import.**

Worse in practice: `startIndex` is the directory-name length of the *first* line
and is applied to every line. A `.set` listing two directory depths mis-cuts
every path outside the first one's depth, and the `line.Length >= startIndex`
guard makes those lines **vanish from the archive** rather than error.

### `Globalx.cs`: bytes assigned to normalised floats

`public Color3(byte[] s) { R = s[0]; G = s[1]; B = s[2]; }` — `R`/`G`/`B` are
0..1 floats everywhere else in the type (`AsColor` multiplies by 255), so a byte
of 255 becomes 255.0 and `AsColor` computes 255 × 255, which
`Color.FromArgb` rejects. Identical to the `Colorf(uint, Format.ARGB32)` defect
in the OpenStack port, and `AsColor` also casts without clamping.

### Two open questions for someone with domain knowledge

* **`IUnknownSkin.BoneMap.Weight` is `int[]`, commented `// Byte / 256?`** — a
  question mark in shipped code. Nothing says whether these are 0..255 or 0..1
  scaled, and the two differ by 256× in every skinned vertex.
  `IntVertex::weights_normalised` makes the ambiguity visible; it needs
  resolving from the formats, not the code.
* **`ParseKey`'s `hex:/` stride** (see the `Family.cs` section) — reads 4-char
  groups with the pair at index 2, which fits `\xNN\xNN`, but guards on `/`.

### Smaller notes

* `IUnknownBone` declares two `Matrix4x4` properties both commented "4x3
  matrix", so the fourth row is padding every implementor must know to ignore.
* `IntVertex` has fields literally named `Obsolete0` and `Obsolete2` — still
  read and stored, consumed by nothing: 24 bytes per skinned vertex.
* `UnknownTransform.CanTransformAsset` returns constant `false` in front of a
  `TransformAsset` that always throws — the same arrangement as
  `Platform_Test` in the OpenStack port.
* `Assembly.cs` grants `InternalsVisibleTo("GameX.Uncore")`, a *runtime*
  dependency rather than a test project, so core is exposing internals across a
  real boundary.
* `MetaInfo` trees are walked recursively in the C# with no depth bound; the
  port is iterative and tested to 50,000 levels.
* `DesSer.cs` has no Rust counterpart by design — it is `JsonSerializerOptions`
  plus 14 converters, which become serde derives on the types themselves. Two
  settings must be carried across deliberately:
  `AllowNamedFloatingPointLiterals` (serde writes `null` for non-finite floats,
  which would corrupt model data) and `AlphabetizeProperties` (serde emits
  declaration order, so any golden file over serialized output will not match).


## `GameX.FileSystems` is entirely CASC

All 35 files sit under `Casc/`. Despite the name there is no general filesystem
layer: it is one implementation of Blizzard's Content Addressable Storage
Container, 8,938 live lines, of which `RootHandlers.cs` alone is **3,817 (43%)**.

**The crate decision comes before the porting**, and it is written up in
`CASC-DECISION.md`. Summary: three Rust crates exist and they are not
equivalent — `cascette-rs` (TACT + CASC, most complete, single maintainer),
`casc-storage` (local storage only), `casc-rs` (**TVFS root format only**). The
crates cover the *container* layer — indices, BLTE, keys, CDN, roughly 5,100
lines here — which is generic across every Blizzard game. None covers
`RootHandlers.cs`, the per-game root manifests, which is where GameX's value
sits and has to be ported regardless.

Whichever crate is chosen dictates the key and index types the root handlers get
written against, so doing the root handlers first means rewriting them.

Ported so far are the crate-independent utilities, useful either way:
`Jenkins96`, `HexConverter`, `WildcardRegex`, `MultiDictionary`.

### There are two different Salsa20 implementations in GameX

    Core/GameX/_LIB/Compression/Salsa20.cs      168 live lines
    Core/GameX.FileSystems/Casc/Salsa20.cs      235 live lines

Not copies — different content. Three call sites choose between them by which
namespace they imported: `Casc/KeyService.cs`,
`Families/GameX.IW/Formats/FastFile.cs`, and CASC's BLTE 'E' block path. Neither
has tests.

Same pattern as the three disagreeing binary16 implementations in the OpenStack
port, except this is cryptography: if the two differ anywhere — a rotation
constant, a counter width, block-boundary handling — then whether a file
decrypts correctly depends on an import. **Diff them before deleting either.** A
disagreement means one has been producing wrong plaintext for whichever games
route through it. Both should become RustCrypto's `salsa20`.

### `Jenkins96` ignores its own base-class contract

`HashCore(byte[] array, int ibStart, int cbSize)` reads **neither** `ibStart`
nor `cbSize` — it hashes the whole array from index 0. `HashAlgorithm`'s
contract is to hash `cbSize` bytes from `ibStart`, which is what
`TransformBlock` and `ComputeHash(buffer, offset, count)` rely on. So the class
is correct only through its own `ComputeHash(string)` helper and silently wrong
through the streaming API it inherits. Its `hashBytes` field is also `static` on
a class with per-instance state, so every instance's `Hash` property returns the
same array.

Ported as a plain function, which makes both unrepresentable. **Verified** two
ways: the empty-input case pins the seed and packing from the algorithm's own
definition (`a = b = c = 0xdeadbeef`, packed `(c << 32) | b` =
`0xdeadbeef_deadbeef`), and the no-allocation block reading was checked
equivalent to the C#'s explicit zero-padding across 43 input lengths.

### `HexConverter` is vendored .NET runtime code

It is `System.HexConverter` copied verbatim, branchless nibble trick included.
I checked that trick against plain formatting for **all 256 byte values in both
casings** — identical output. So it is an optimisation, not a different
algorithm, and the port uses a lookup table rather than reproducing
unsafe-adjacent arithmetic for no behavioural gain.

Two notes: `Casing` is an enum whose values are *pre-shifted ASCII case bits*
(`Upper = 0`, `Lower = 0x2020`) ORed into packed characters, so any third value
emits garbage and nothing prevents one; and `ToCharsBuffer` writes
`buffer[startingIndex + 1]` before `buffer[startingIndex]`, both unchecked, so a
bad index corrupts the array before throwing.

### `WildcardRegex` cannot express a literal `*`

`Escape(pattern).Replace("\\*", ".*")` — `Regex.Escape` turns `*` into `\*`,
and the replace turns every `\*` back into `.*`. So escaping is exactly what
gets undone, and a pattern meaning "filename containing an asterisk" is
indistinguishable from a wildcard. CASC virtual paths can contain both `*` and
`?`.

Also `matchStartEnd: false` produces an unanchored regex, so pattern `foo`
matches `barfoobaz` — both call styles exist in the tree and the difference is a
bool at the call site. Ported as a direct glob matcher (iterative, so a
pathological pattern cannot go exponential), with `to_regex` retained to
reproduce the C#'s string output including the bug.

### `MultiDictionary` uses `new` hiding rather than `override`

It derives from `Dictionary<K, List<V>>` and declares `public new void Clear()`.
`Dictionary.Clear` is not virtual, so the custom `Clear` is reachable only
through a `MultiDictionary`-typed reference — a base-class or `IDictionary`
reference gets the base behaviour. Three call styles, two behaviours. Ported as
its own type wrapping a `HashMap`, so there is no base class to route around.

Small note worth keeping: the C#'s local variable inside `Add` is named `hset`,
suggesting a `HashSet` was once intended, but it is a `List<V>` — so duplicate
values under one key are kept. Preserved, and asserted in a test.


### CDN fetches are plaintext HTTP

    public static string MakeCDNUrl(string cdnHost, string cdnPath)
        => $"http://{cdnHost}/{cdnPath}";

Every CDN download goes over unencrypted HTTP. Three more sites do the same:
`CascConfig.cs:341` and `:343` build `http://{host}/{path}` from the CDN config,
and two commented-out lines reach `http://us.patch.battle.net:1119`. **There is
no `https` anywhere in the project.**

Worth being precise about how bad this is: **CASC verifies content by hash**, so
a tampered payload fails its content-key check rather than being executed. This
is not a code-execution hole. What it leaks is *which* files a user downloads —
the CDN path contains the content key — and it leaves fetches open to disruption
rather than substitution.

Blizzard's CDN hosts serve HTTPS. It is a one-word change per site.
`utils.rs::make_cdn_url` takes the scheme as a parameter with `Https` as the
default, so neither choice is silent.

### `NestedStream` tracks how much it read, not where it started

`Position` returns `length - remainingBytes` — the count of bytes *this view*
consumed, which is correct only while nothing else moves the underlying stream.
Two `NestedStream`s over one source silently interleave, and each reports a
plausible `Position` throughout. Nothing in the type prevents constructing them.

Ported with the same decision as `PartialInputStream` in the OpenStack port:
ownership is explicit in the type. `NestedStream` owns its source;
`SharedSource` is the borrowing variant, so the sequencing that the C#'s
`leaveOpen` bool leaves implicit becomes a borrow the compiler checks. An
`anchored` constructor records the start offset and `is_consistent()` reports
whether the source is still where the view expects — the check the C# cannot
make.

Three more in that file: `offset + count > buffer.Length` can overflow `int`
and pass; the `Memory<byte>` and array read paths use *different*
end-of-stream conditions (`remainingBytes < 0`, which its own arithmetic cannot
reach, versus `count <= 0`); and a short read from the source ends the window
early and silently, leaving the view claiming data remains — the same shape as
`Util.CopyFile` in the OpenStack port.

### Smaller CASC notes

* **`Utils.MakeCDNPath` slices unchecked** — `Substring(0, 2)` and
  `Substring(2, 2)` throw for any name under four characters.
* **`Utils.HttpWebResponse` retries by recursion**, so a 5-deep stack holds five
  live `HttpWebRequest` objects, and it retries on *any* exception including
  404 — a missing file costs five round trips.
* **`BackgroundWorkerEx.ReportProgress` throws `OperationCanceledException`**
  when cancellation is pending, making cancellation a side effect of reporting
  progress. It is also `new`-hiding over a non-virtual method, like
  `MultiDictionary.Clear`.
* **`PerfCounter.Dispose` writes to both `Console` and `Logger`**, so every
  measurement prints twice, unconditionally, in release builds.
* **`CDNCacheStats` is three unsynchronised mutable statics** incremented from
  concurrent downloads; `TimeSpan +=` is not atomic.
* **`Logger` holds three static fields with no synchronisation**, and `Init`
  opens with `FileMode.Create` — calling it twice truncates and leaks the first
  `FileStream`.

## A bug in my own tooling

`balance-check.py` reported a phantom imbalance in `utils.rs`. The cause was the
checker, not the code: it stripped `//` comments *before* string literals, so
the `//` inside `"{}://{cdn_host}"` was read as a comment start and swallowed
the rest of the line including its closing delimiters. **Any file containing a
URL literal would have been misreported** — and this port is full of them.

Rewritten as a single-pass tokenizer handling line comments, nested block
comments (Rust allows nesting), raw strings (`r#"..."#`), byte strings, char
literals and lifetimes. Both ports re-verify clean: 52 GameX files and all 224
OpenStack files.

That is the second bug in this checker (the first was a missing `DOTALL` that
desynced on multi-line strings). A tool that reports "0 imbalanced" is only
worth as much as its own correctness, so both fixes are noted here rather than
quietly applied.


## Converting CASC: the key types

Started on the container itself rather than waiting on the crate decision.
`RootHandlers.cs`'s six enums are generated and verified (279 members, 0
mismatches), and the key types are extracted into `casc_key.rs`.

### The core key type is buried at line 4196 of the largest file

`MD5Hash` — the 16-byte content key that every index, every encoding table and
every root entry is keyed on — is declared **inside `RootHandlers.cs`**, between
a flags enum and `RootEntry`, two-thirds of the way down a 3,817-line file.

That placement is a symptom rather than a bug, but it has consequences:

* **`MD5Hash` is two `ulong`s with no byte-order contract.** It is only ever
  produced by reinterpreting 16 bytes read from a file, and
  `MD5HashComparer.GetHashCode` reinterprets it *again* as four `uint`s via
  `Unsafe.As`. On a big-endian host both the comparison and the hash change
  meaning. There is no `FromBytes`/`ToBytes` anywhere. The port stores the
  bytes and derives the words, so on-disk order is explicit.
* **It has no constructor, no `Equals`, no `GetHashCode`.** Equality lives in a
  separate `MD5HashComparer` singleton, so `hash1 == hash2` uses the default
  field-wise struct comparison (which happens to be right) while a
  `Dictionary<MD5Hash, T>` built *without* passing the comparer silently falls
  back to reflection-based `ValueType.GetHashCode` — correct but slow, on the
  hottest lookup path in the system.
* **`MD5HashComparer.GetHashCode` assumes the struct is exactly 16 bytes with
  no padding**, via `Unsafe.As<MD5Hash, uint>` plus `Unsafe.Add`. Nothing
  enforces that; a field added to `MD5Hash` reads out of bounds.

### Two more from the same region

* **`FileDataHash.ComputeHash` mixes signed and unsigned.**
  `0x100000001B3L * (... ^ baseOffset)` multiplies a `long` literal by a `ulong`
  expression, and that pairing has no common type in C#. Either the literal is
  being coerced in a way worth checking or this does not compile as written. The
  algorithm is plain FNV-1a over the four little-endian bytes of the id, which
  the port implements and checks against an independent reimplementation.
* **`ContentFlagsFilter.Filter` enumerates its input three times** — two
  `temp.Any(...)` probes plus the final consumption, over a LINQ chain on the
  root file. For a WoW root with millions of entries that is three full passes
  where one would do.

### Also worth knowing about this project's layout

`_/Casc/` (two files, 323 lines) declares namespace `GameX.Blizzard.Formats.Casc`
— but that namespace is live, owned by `Families/GameX.Blizzard/Formats/Casc/`
(the WDB/WDC database readers). So two projects contribute to one namespace from
different assemblies, and the `_/` copy is a stale duplicate of on-disk struct
definitions under a name that is in use elsewhere.


## BLTE: a missing decryption key returns zeros

`BLTEStream.cs` is the block container every file in CASC is wrapped in, so
parsing it is the gateway to reading anything. **This is the most consequential
defect I have found in either codebase**, because it produces plausible data
rather than an error.

```csharp
byte[] key = KeyService.GetKey(keyName);
bool hasKey = key != null;
if (key == null) {
    key = new byte[16];                                  // all-zero key
    if (CascConfig.ThrowOnMissingDecryptionKey && index == 0)
        throw new BLTEDecoderException(3, ...);
}
...
MemoryStream ms = cs.CopyToMemoryStream();
return hasKey ? ms : null;
```

and at the call site:

```csharp
Stream decryptedData = Decrypt(data, index);
if (decryptedData != null) ... HandleDataBlock(decryptedData, index);
else _memStream.Write(new byte[_dataBlocks[index].DecompSize], 0, ...);
```

So when the key is unknown it runs the **entire Salsa20 decryption with an
all-zero key**, throws the result away, and the caller writes `DecompSize`
**zero bytes** into the output. The read then succeeds and reports the full
expected length.

Three things make this worse than a bare failure:

* The guard is `ThrowOnMissingDecryptionKey && index == 0`, so a missing key on
  **any block but the first** never throws, even with the flag on. A
  partially-keyed file yields real data followed by a run of zeros.
* `CascConfig.ValidateData` gates the MD5 block check separately, so with
  validation off nothing notices.
* **Zeros are a legal payload for most formats.** A texture reads as black, a
  model as degenerate, a database as empty rows. Nothing downstream can tell
  this from a real file.

The port returns `BlteError::MissingKey { key_name, block }`. Zero-filling has
to be asked for.

### Six more in the same file

1. **`Length` over-reports for a header-bearing stream.**
   `_length = _hasHeader ? _memStream.Capacity : _memStream.Length` — `Capacity`
   is the *declared* sum of `DecompSize`. A block that decompresses short still
   reports the declared total, and reads past the real data return zeros.
   Truncation presented as a complete file.
2. **Three dead conditions.** `if (size < 12)` after `if (size < 36)`;
   `keyNameSize == 0 || keyNameSize != 8` (the first is subsumed);
   `IVSize != 4 || IVSize > 0x10` (unreachable — if it is not 4 it already
   threw, and 4 is not > 16). Each reads as a real bound and checks nothing.
3. **`Decrypt`'s length check runs after the reads it guards.**
   `if (data.Length < keyNameSize + IVSize + 4)` sits *below* `ReadUInt64()` and
   `ReadBytes(IVSize)`, so a short block throws `EndOfStreamException` from the
   read before reaching the check that exists to prevent exactly that.
4. **`Position`'s setter decodes forward then gives up silently.**
   `while (value > _memStream.Length) if (!ProcessNextBlock()) break;` — seeking
   past the real end leaves the position wherever decoding stopped, with no
   indication the seek did not land.
5. **`using (BinaryReader)` then `using (CryptoStream)` over the same stream**
   disposes the underlying stream twice, so `Decrypt` consumes a stream its
   signature suggests it only reads.
6. **`_md5` is one shared `MD5` instance** for the header hash and every block
   hash. Fine while single-threaded; breaks quietly if a block handler ever
   recurses — which is precisely what the `F` type is for.

`F` (recursive frame) and ARC4 both throw `NotImplementedException` in the C#,
so there was nothing to port. The Salsa20 call is behind a `StreamCipher` trait
and the deflate step behind the caller's decompressor, so this crate still
contains no hand-written cipher — see `casc/salsa20.rs`.


## `LocalIndexHandler`: "latest index" is whatever the OS lists last

The 16 local `.idx` files map a truncated 9-byte encoded key to its location in
the `data.NNN` archives, so this is the lookup every local file read goes
through. `.idx` files are versioned — `0000000042.idx`, `0000000043.idx` — and
the newest must win, because an older index points at archive offsets that have
since been rewritten.

```csharp
var files = Directory.EnumerateFiles(dir, $"{i:x2}*.idx");
if (files.Any()) latestIdx.Add(files.Last());
```

`EnumerateFiles` returns **filesystem order, not sorted order**. NTFS happens to
return B-tree order, which is usually lexicographic; ext4 returns hash order. So
this picks the newest index on Windows by accident and an arbitrary one on Linux
— and a stale index resolves to offsets that now hold different data. One
`OrderBy` fixes it. (`files.Any()` also enumerates the directory a second time.)

Four more in the same file:

* **The alignment masks are 32-bit literals on 64-bit values.**
  `(8 + HeaderHashSize + 0x0F) & 0xFFFFFFF0` and
  `(EntriesSize + 0x0FFF) & 0xFFFFF000` clear every bit above 32, so a position
  at or past 4 GiB wraps to near zero and the parse silently restarts mid-file.
  `.idx` files are small so it does not bite today; the mask is still wrong.
* **`HeaderHashSize` is read from the file and passed straight to
  `ReadBytes`**, so a corrupt index allocates whatever it claims.
* **Both checksums are read and never verified.** `HeaderHash` and
  `EntriesHash` are assigned to locals nothing reads — the integrity fields are
  parsed out and ignored.
* **`ContainsKey` then `Add` hashes every key twice** on the hottest parse loop
  in the system (hundreds of thousands of entries across 16 files).

One thing that looks wrong and is not: the 18-byte entry mixes endianness —
`indexLow` is big-endian, `Size` little-endian, in the same record. That is the
format, and it is preserved.

## `EncodingHandler`: seven bugs, and one I would not guess at

The encoding table maps content keys (what a file *is*) to encoded keys (what is
stored), with each file's decoded size and its ESpec compression/encryption
descriptor.

**The magic is skipped, not checked** — `stream.Skip(2); // EN` — so a file that
is not an encoding table parses as garbage and the first thing that fails is
something far downstream. **Three fields carry "must be" comments and none is
validated**: `Version` ("must be 1"), `unk1` ("must be 0"), and
`CKeyLength`/`EKeyLength`, which are read but never compared against the 16 and
9 the rest of the file assumes.

**Page sizes are read signed then scaled.** `ReadInt16BE() * 1024` means a page
size above 32 KB comes back negative and is used as a chunk stride.

**`Add` throws on duplicate keys, twice.** `EKeyToCKey.Add(eKey, cKey)` and
`EncodingData.Add(cKey, entry)` both raise `ArgumentException` on a repeat — but
a repeated e-key is *legitimate*: one stored blob backing two content keys is
the entire point of content-addressable storage. So this aborts the load of a
valid table. The port counts collisions instead and reports them.

**`strings[eSpecIndex]` is unchecked.** The index comes from the file as a
big-endian `int32`; only `-1` is special-cased, so any other out-of-range value
throws mid-parse.

### The magic number I preserved rather than fixed

```csharp
long remaining = CHUNK_SIZE - ((pos - chunkStart) % CHUNK_SIZE);
if (remaining == 0xFFF) { pos -= 1; i++; continue; }
```

`remaining` is in `1..=4096` by construction, so `0xFFF` (4095) means **exactly
one byte into a chunk**. The response is to rewind that byte and then `i++` *in
addition to* the `for` loop's own increment — advancing the page counter by two
and silently dropping a page.

This is a workaround for an off-by-one somewhere else, and I could not tell
where from the code. Reproduced faithfully with the arithmetic spelled out, and
the port counts how often it fires (`skipped_pages`) so the condition is
observable rather than invisible. A test pins that `0xFFF` means offset 1 and
nothing else. **Changing this needs a real encoding file.**

### And one I did not port

Encryption key names are extracted from the ESpec with a regex:
`(?<=e:\{)([0-9a-fA-F]{16})(?=,)` — a lookbehind for `e:{`, a lookahead for a
comma. ESpec is a structured grammar (`b:{164=z,16K*=z,1656K=z}`, `e:{...}`), so
a nested block or a different field order silently yields **no keys**, and a
file that needed decrypting is then read undecrypted — which, combined with the
BLTE zero-fill above, means it comes back as zeros. This wants a small ESpec
parser, not a pattern, so I left it out rather than reproduce a fragile regex.

## Found in the C# (running list)

Beyond the rename damage above:

- **`Util._guessExtension` returns two different answers for one magic.**
  `0x75B22630` is `.asf` on the `fast` path and `.mov` on the slow path, so the
  result depends on a bool the caller passes rather than on the bytes. That
  magic is the ASF header GUID prefix, so `.asf` is correct.
- **Its fallback fabricates an extension from raw bytes**, so unknown input
  yields something like `".\u{1}\u{2}"` rather than failing — unrecognised and
  recognised are indistinguishable.
- **`_valueV` throws on booleans and nulls** (`True`/`False`/`Null` hit
  `_ => throw`), so one `true` in a config aborts the parse; and it reads every
  number as `Int32`, so a float or out-of-range value throws.
- **`_random` is an unsynchronised lazily-created `System.Random`** —
  `??= new Random()` can construct two instances from two threads, and `Random`
  is not thread-safe.
- **`_randomValue(low, high)` is inclusive of `high`**, unlike every other range
  API in either language. Preserved, and asserted.
- **`PacketLogger` hard-codes `Ticks: 0`**, so every log line reports tick zero
  despite having a field for it.
- **`PacketLogger.CreateFile` disposes `_logFile` then reassigns it** while
  `Log` reads the same field unsynchronised — a concurrent `Log` can write to a
  disposed file. It also indexes `message[0]` before checking the length.
- **XTEA silently skips a trailing partial block** (`i + 8 <= offset + count`),
  leaving 1..7 bytes in plaintext with no indication.

## `Store.cs` — ported as-is

Game-store install-path discovery. **The path data is transcribed exactly**,
including the parts that look wrong; an earlier draft of this port "corrected"
them and substituted paths I had invented, which was worse than porting yours,
since invented paths carry no evidence at all. Observations are comments;
behaviour matches the C#.

Recorded, not changed:

* Every macOS branch does
  `search.SelectMany(x => search, (s, h) => Path.Join(h, s, tail))`, which
  crosses `search` with itself and never reads the `home` array declared on the
  line above. Five sites: lines 125, 189, 233, 371, 417.
* `Store_Blizzard`'s Linux branch searches `.steam`, `.steam/steam`,
  `.steam/root`, `.local/share/Steam` plus `appcache` — the same paths as
  `Store_Steam`.
* `Store_Epic` joins `"Sbi"` on all three platforms; the constructor then looks
  for `Manifests` beneath it. `"Sbi"` appears only in this store.
* Three Linux `search` arrays are the literal `["??"]`.
* `Store_Abandon` / `Store_Archive` roots are `E:\AbandonLibrary` and
  `E:\ArchiveLibrary`, preserved as `ABANDON_ROOT` / `ARCHIVE_ROOT`.
* `Store_Blizzard`'s protobuf fallback re-parses the same `Stream` after a
  failed parse has advanced it to the end.
* Every store walks the filesystem from a **static constructor** using
  `paths.Add` / `ToDictionary`, which throw on duplicate keys — surfacing as
  `TypeInitializationException` and poisoning the type process-wide.

Two things do differ, only because a 1:1 translation is impossible:

* **Plan-building is split from filesystem probing.** `SearchPlan` holds
  candidates; `first_existing` takes an `exists` predicate. The C# interleaves
  them in one expression, which cannot be unit-tested. The candidates produced
  are identical, and the tests assert the C#'s actual output — including the
  self-crossed macOS paths.
* **Static constructors become explicit builders**, since Rust has no static
  initialiser that runs I/O. `library_paths` uses `insert` (last-wins) where the
  C# uses `Add`; panicking inside a lazy static would be worse than the C#'s
  exception. That is the file's one behavioural difference and it is noted at
  the function.

## `_Config.cs` — 36 `#if` branches to a runtime table

`#define Arkane` at the top of the file selects one of 36 mutually exclusive
branches, each assigning the same `static GlobalOption Option`.

Ported as a lookup table rather than `#[cfg(feature)]` blocks, deliberately:
cargo features are additive and not mutually exclusive, so 36 of them assigning
one static would either fail to compile with two enabled or silently pick one.
A table with a default is the same data, selectable without editing source.

**An extraction bug worth recording**, since it is the same shape as one from
the enum generator: each branch carries several *commented-out*
`Game = "..."` alternatives, and a regex over the raw block reads the last one.
That made `Arkane` come out as `RF` — from a trailing
`//Missing: Game = "RF"` — instead of the active `AF`. Comments are stripped
first now, and a test pins `AF`.

Those commented alternatives turned out to be the most valuable thing in the
file: **421 game ids with human descriptions across all families**, which is the
only such registry in the repository. Preserved as `config::ALTERNATIVES`, and
it is what settled the `Height` rename above.

Two observations, unchanged: `ForceOpen = true` and `ForcePath = "sample:N"` are
set in **every** branch — debug overrides committed enabled, so a release build
inherits them — and changing family requires a recompile.


