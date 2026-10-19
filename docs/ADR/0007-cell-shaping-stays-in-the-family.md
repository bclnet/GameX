ADR 0007: Cell shaping stays in the family
===

* **Status:** Superseded in part by [ADR 0008](0008-create-land-stays-in-the-family.md). Rules in points 3 and 4 still hold. `CreateLand` no longer stays in OpenX.
* **Date:** 2026-10-18

## Context

`CellManager` loads a window of cells and asks a `CellBuilder` to build them. That builder's `CreateLand` was written for Morrowind: 65 cumulative height samples, scaled by 8, with a 16 by 16 texture grid and a texture index stored one higher than the real id.

Ultima Online land is a different shape. A cell is 64 by 64 tiles, the heights are absolute, and the texture id is used as stored. Putting both shapes behind one `ICellEngine` on `IQuery`, and teaching `CreateLand` to ask that engine which rule to apply, made the shared OpenX manager a switch for two families.

OpenX cannot call Bethesda or Origin. Those projects sit above it. An engine project such as `GameX.Engine.OpenGL` does not reference either family, and a family project cannot subclass `OpenGLCellBuilder` without taking a dependency on that engine.

## Decision

1. **OpenX `CellManager` is not a Tes or Ultima strategy.** There is no `ICellEngine`, and `IQuery` does not name one. Engines keep constructing `OpenGLCellBuilder`, `UnityCellBuilder`, and `GodotCellBuilder`.
2. **`CellBuilder.CreateLand` stays the Morrowind path** it had before the strategy interface: side 65, cumulative heights times 8, 16 by 16 textures, index minus 1, default texture `textures/_land_default.dds`.
3. **Tes land rules live in Bethesda.** `GameX.Bethesda.TesCells` in `Families/GameX.Bethesda/Managers.cs` holds those numbers and the height, layer, alpha, and path functions. OpenX does not call it.
4. **Ultima Online land rules live in Origin.** Origin Systems is the family that contains Ultima. `GameX.Origin.UltimaCells` in `Families/GameX.Origin/Managers.cs` holds side 64, absolute heights, raw texture ids, and the path `tex{id}`. That file is the family manager, not `Clients/UO/Managers.cs`.
5. **An Ultima land is not passed to `CreateLand`.** That method reads 65 cumulative samples. Ultima terrain has to be drawn by Origin code that uses `UltimaCells`.

## Consequences

* The Morrowind numbers are in `CreateLand` and again on `TesCells`. The copy in OpenX is the renderer the engines already call. The copy in Bethesda is the family's own statement of the rules.
* `UltimaCellQuery` reads map and static streams. It does not register itself through an OpenX engine property.
* Python has `families/Bethesda/managers.py` and `families/Origin/managers.py`. Rust has the Bethesda manager. Origin has no family-root `managers` module yet, so that port is still to do.
* Rust `CreateLand` is not ported. Bethesda's Rust `TesCells` is the family rule, not a second renderer.
