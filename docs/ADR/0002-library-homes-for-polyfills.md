ADR 0002: Polyfills go to the library they extend
===

* **Status:** Proposed
* **Date:** 2026-10-11
* **Scope:** OpenX only. GameX's ports stay one-to-one with C#, file for file and type for type.

## Context

Much of C#'s `OpenX.Poly` is polyfills: code that fills gaps in .NET libraries, filed in folders named after the .NET namespace it extends (`System.Numerics/`, `System.IO/`, `System.Collections.Generic/`, ...). Python and Rust don't have those .NET libraries. They have their own: Python uses `numpy` and `quaternion` where C# uses `System.Numerics`; Rust has `glam`, `half` and `std::io`.

Copying C#'s namespace folders into Rust gives `system_numerics/polyfill.rs` files that wrap types Rust already has, and puts extensions to `glam` somewhere nobody would look for them. Python already files most of OpenX by what each piece is, under `openx.core.poly.*`.

## Decision

1. **Use the matching library.** Where C# polyfills a .NET library, Python and Rust use their own equivalent library instead of re-creating the .NET type.
2. **Extensions live with what they extend.** Code that adds to an outside library goes in a module named for that library. In Rust, that means extension traits, because Rust only allows extending another crate's types through a trait.
3. **C# names stay.** A C#-specific type keeps its C# name. Where the target library already has the type, that name is an alias of it (`pub type Int3 = glam::IVec3;`).
4. **Python's layout names the modules.** Rust module names follow Python's `openx.core.poly.*` packages, not C#'s namespace folders.
5. **Nothing for code with no caller.** A polyfill nothing uses yet isn't ported until something needs it.

## Homes

The table of where each piece lives in each language is [`OpenX/Polyfills.md`](../../OpenX/Polyfills.md), at the OpenX root next to the three language trees. When something moves, update it there.

## Consequences

* Rust code reads like Rust: `Vec3` is glam's, and a reader is a `std::io::Read` extension.
* Each language's layout differs from C#'s folders, so finding a port means looking up its row here rather than mirroring the C# path.
* ADR 0001 still holds: the same types, members and behaviour exist in every language; only where they're filed differs.
