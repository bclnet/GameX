ADR 0003: C# value types are ported by behaviour
===

* **Status:** Proposed
* **Date:** 2026-10-11

## Context

C# has value types (`struct`, and the built-in numeric types): a value-type variable is never null, a field starts as `default(T)` (all zero), and assigning or passing one copies it. A lot of GameX and OpenX relies on that without saying so: a struct field that was never assigned reads as zeros, `x = y` takes a copy that later changes to `y` don't affect, and binary readers fill structs field by field.

The other two languages differ:

| | C# | Python | Rust |
| -- | -- | -- | -- |
| value types | `struct`, numerics | none: every object is a reference | structs and enums are values: moved, or copied when `Copy` |
| unset field | `default(T)`, all zero | must be given a value; usually `None` | must be given a value; `Default::default()` is `default(T)` |
| null for a struct | impossible | possible (`None`) | impossible (`Option<T>` only when asked for) |
| assignment of a struct | copies | shares the same object | moves, or copies when `Copy` |
| C# `null` for a class | `null` | `None` | `Option<T>::None` |

Porting file by file, these differences show up as output that doesn't match C#: a field Python leaves as `None` where C# had a zeroed struct, or two Python names that share an object where C# had two independent copies. To get the same output, the ports sometimes have to create a default or a copy that C# made implicitly. That makes those lines differ from the C# even though the behaviour matches.

## Decision

1. **Behaviour follows C#.** A port reproduces C#'s value-type behaviour: unset struct fields read as their default, assignments of structs are independent copies, and struct-typed values are never null.
2. **Python** creates what C# does implicitly:
   * a value-type field or local starts as an explicit default instance (`Int3()`, `0`, `0.0`), not `None`;
   * where C# copies a struct and the copy is later changed, Python copies explicitly (`copy(x)`, or a new instance);
   * mark the spot with `# C# value type` so it reads as deliberate.
3. **Rust** uses its own value semantics:
   * a C# `struct` is a Rust struct deriving `Clone`, `Default`, and `Copy` when all its fields are `Copy`;
   * `default(T)` is `Default::default()`;
   * a C# class that can be `null` is `Option<T>`; a C# struct is never wrapped in `Option` unless C# used `T?`.
4. **This is an allowed difference from ADR 0001.** Lines may differ from C# where a port has to make a C# value-type default or copy explicit. Everything else still matches file for file.

## Consequences

* GameX's ports, the base classes especially, won't be exact line-for-line mirrors of C#. The differences are limited to value-type defaults and copies, and are marked.
* Output matches across languages, which is what file-by-file comparison of a model's output checks.
* Rust ports need fewer of these than Python, because Rust structs already behave like C# structs.
