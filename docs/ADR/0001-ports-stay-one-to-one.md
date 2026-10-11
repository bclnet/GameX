ADR 0001: Every change is applied to every language
===

* **Status:** Accepted
* **Date:** 2026-10-11

## Context

GameX and OpenX each exist in three languages:

| Language | Role | Where |
| -- | -- | -- |
| C# | the reference implementation | `dotnet/`, `OpenX/dotnet/` |
| Python | hand port | `python/gamex/`, `OpenX/python/` |
| Rust | in-progress port | `rust/`, `OpenX/rust/` |

The ports are meant to be read side by side: the same files, the same types, the same method names, the same behaviour. A change made in one language and not the others starts a drift that is invisible until someone compares them, and every later port of that code has to rediscover what changed and why.

## Decision

A change is not done until it has been made in every language that has the code.

1. **Same change, same commit.** A behaviour or API change to C# is made to the Python and Rust twins in the same change, not in a follow-up.
2. **Same shape and names.** Keep the same types, members, method names and control flow. Don't add language-specific naming such as an `Async` suffix in one port only; follow the casing each port already uses (`Opening`, `opening`).
3. **Nearest equivalent where a language differs.** When a language can't express the change directly, use its closest equivalent and say so at the site. Example: C# `Opening().GetAwaiter().GetResult()` is Python `runSync(self.opening())`.
4. **Missing twin, record it.** If a port doesn't have the code yet (a Rust `PORT-STATUS: todo` stub, or a family Python hasn't ported), don't invent it as part of the change. Note it in the change description so the port picks up the current shape when it's written; for Rust, `PORT_MAP.tsv` and `sync-check.sh` track the C# file it must follow.
5. **Say what was updated.** Every change description lists each language as updated, not applicable (and why), or still to do.

## Consequences

* Changes take longer and touch more files, and reviewers check all three trees.
* The ports stay comparable line by line, so a fix found in one language can be applied to the others mechanically.
* A change that is hard to express in one language is a signal to reconsider the design before it lands, rather than after the ports have diverged.

## Example

Making archive opening and closing asynchronous:

| Language | Change |
| -- | -- |
| C# | `Binary.Opening()`/`Closing()` return `Task`; `Open()`/`Close()` keep their signatures and wait with `GetAwaiter().GetResult()`. `BinaryArchive`, `MultiArchive`, `SubArchive`, `UncoreArchive` and the test archive follow. |
| Python | `opening`/`closing` become `async def`; `open()`/`close()` keep their signatures and wait with `runSync`, added to OpenX's `pool.py` next to `parallelFor`. `BinaryArchive`, `MultiArchive` and `SubArchive` follow. |
| Rust | Not applicable yet: `Core/GameX/Binary.cs` has no Rust port. When it is ported, it starts from the async shape. |
