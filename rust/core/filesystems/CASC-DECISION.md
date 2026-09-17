# `GameX.FileSystems` is entirely CASC — the crate question

All 35 files in this project are under `Casc/`. Despite the name, there is no
general filesystem layer here: it is one implementation of Blizzard's Content
Addressable Storage Container, 8,938 live lines.

## Where the lines are

| file | live | what |
|---|---|---|
| `Casc/RootHandlers.cs` | **3,817** | per-game root manifests — 43% of the project |
| `Casc/KeyService.cs` | 862 | encryption key management |
| `Casc/CascConfig.cs` | 328 | build/CDN config parsing |
| `Casc/BLTEStream.cs` | 282 | BLTE block decompression |
| `Casc/CascHandler.cs` + `Base` + `Lite` | 590 | storage handlers |
| `Casc/Extensions.cs` | 262 | reader helpers |
| `Casc/Salsa20.cs` | 235 | **a second Salsa20 — see below** |
| `Casc/Jenkins96.cs` + `Jenkins96Old.cs` | 290 | filename hashing (two versions) |
| `Casc/CdnCache.cs`, `CdnIndexHandler.cs`, `RibbitClient.cs` | 435 | CDN fetch |
| the other 22 files | ~1,800 | indices, encoding, install/download manifests, utils |

## The Rust options

Three exist, and they are not equivalent:

* **`cascette-rs`** (wowemulation-dev) — the most complete: TACT (CDN delivery)
  and CASC (local storage) together, with a Python prototyping harness for
  format verification. Actively developed, MIT/Apache-2.0. Explicitly a
  "nights-and-weekends effort by one person", which is worth weighing for a
  dependency this central.
* **`casc-storage`** — local CASC only: `.idx` and data archives, plus TACT
  install and download manifests. Narrower and correspondingly simpler.
* **`casc-rs`** (echo000) — pure Rust, but **only supports TVFS root format.**

## Recommendation: split the project, do not adopt or port wholesale

The crates cover the *container* layer — indices, BLTE, keys, CDN. That is
roughly 5,100 lines here and it is generic: it is the same for every Blizzard
game, it is what the crates already implement, and it is exactly the kind of
code where an independent implementation earns nothing.

What the crates do **not** cover is `RootHandlers.cs` — 3,817 lines of
per-game root manifest parsing. `casc-rs` handles only TVFS; the others handle
some subset. This is where GameX's actual value sits, and it has to be ported
regardless.

So:

1. **Adopt a crate for the container layer.** `cascette-rs` if TACT/CDN support
   is wanted, `casc-storage` if only local storage is. Either removes ~5,100
   lines from the port surface.
2. **Port `RootHandlers.cs` against whichever crate's key/index types.** This is
   the real work and it needs test data — a root manifest per supported game.
3. **Do not port either Salsa20.** See below.

Deciding this before porting matters more than usual: whichever crate is chosen
dictates the key and index types that `RootHandlers` is written against, so
doing the root handlers first means rewriting them afterwards.

## Two things worth acting on regardless of the crate choice

**There are two different Salsa20 implementations in GameX.**

    Core/GameX/_LIB/Compression/Salsa20.cs      168 live lines
    Core/GameX.FileSystems/Casc/Salsa20.cs      235 live lines

Not copies — different content. Three call sites pick between them by which
namespace they imported (`Casc/KeyService.cs`,
`Families/GameX.IW/Formats/FastFile.cs`, and CASC's BLTE 'E' block path).
Neither has tests. If they disagree anywhere, whether a file decrypts correctly
depends on which file the caller happened to `using`. **Diff them before
deleting either** — a disagreement means one has been producing wrong plaintext.

**`Jenkins96` derives from `HashAlgorithm` and ignores its contract.**
`HashCore(byte[] array, int ibStart, int cbSize)` reads neither `ibStart` nor
`cbSize` — it hashes the whole array from index 0. So the class is correct only
through its own `ComputeHash(string)` helper and silently wrong through the
streaming API it inherits (`TransformBlock`,
`ComputeHash(buffer, offset, count)`). Its `hashBytes` field is also `static` on
a class with per-instance state.

Ported as a plain function in `casc/jenkins96.rs`, which makes both
unrepresentable. The empty-input case is verified from the algorithm's
definition (`a = b = c = 0xdeadbeef`, packed `(c << 32) | b` =
`0xdeadbeef_deadbeef`), and the no-allocation block reading was checked
equivalent to the C#'s explicit zero-padding across 43 input lengths.
