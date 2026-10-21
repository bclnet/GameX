ADR 0012: Game networking lives in a Netcode sibling
===

* **Status:** Accepted
* **Date:** 2026-10-21

## Context

Family projects such as `GameX.Origin` are `netstandard2.1`. They hold archives, formats, and game identity. `GameX.All` references them.

Sockets, login crypto, and server lists are not format code. Putting them in the family project couples every format consumer to the protocol. Putting them in an engine project couples the protocol to one renderer. OpenX is the generic engine back end and does not own a game's protocol.

## Decision

A game's protocol is its own project, `GameX.<Family>.Netcode`, under `dotnet/Families`, next to `GameX.<Family>`.

1. **References point one way.** The Netcode project references the family project. The family project does not reference Netcode. The client project references Netcode.
2. **One game, one namespace.** Protocol types for one game go in a subfolder and namespace for that game. The first case is Ultima Online: project `GameX.Origin.Netcode`, folder `UO`, namespace `GameX.Origin.Netcode.UO`. It references `GameX.Origin` only.
3. **A later game gets its own Netcode sibling.** Not a shared network library, and not a second target framework on the family project.
4. **Ultima Online stays C# until the owner says it is merged.** This Netcode project gets no Python twin, no Rust twin, and no `PORT_MAP.tsv` row. [ADR 0001](0001-ports-stay-one-to-one.md) still applies to Origin resource parsers that already have twins.

## Consequences

* `GameX.All` stays free of sockets.
* Engine projects do not own login.
* `GameX.Origin` stays `netstandard2.1`.
