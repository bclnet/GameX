ADR 0008: CreateLand stays in the family
===

* **Status:** Accepted
* **Date:** 2026-10-18
* **Supersedes:** the CreateLand clause of [ADR 0007](0007-cell-shaping-stays-in-the-family.md)

## Context

ADR 0007 put the Tes and Ultima land numbers in the family managers and left `CellBuilder.CreateLand` in OpenX, because that method also loads textures and builds a terrain mesh. The height loop and the mesh were then two copies of one Morrowind path.

OpenX still cannot reference Bethesda or Origin. A family project can reference OpenX and subclass the generic `CellBuilder`, because that type is not tied to OpenGL or Unity.

## Decision

1. **`CreateLand` moves to the family manager.** `BethesdaCellBuilder.CreateLand` in `Families/GameX.Bethesda/Managers.cs` is the Morrowind method. `UltimaCellBuilder.CreateLand` in `Families/GameX.Origin/Managers.cs` is the Ultima Online method: 64 absolute samples, raw texture ids, path `tex{id}`.
2. **OpenX `CellBuilder.CreateLand` draws nothing.** Engines still own containers, objects, and lights.
3. **The query supplies the builder.** `IQuery.CreateCellBuilder` defaults to the empty OpenX builder. `Tes3CellQuery`, `ElseCellQuery`, and `UltimaCellQuery` return their family builder. Engine renderers call that instead of `new OpenGLCellBuilder` or `new UnityCellBuilder`.

## Consequences

* Morrowind terrain is drawn only when the query is a Bethesda query. An Ultima query draws through `UltimaCellBuilder`.
* The generic engine cell-builder types remain, and no longer carry land shaping.
* Python has the same builders and the same query methods. Rust has no terrain `CreateLand`; `families/bethesda/src/managers.rs` stays a partial port of the Tes rules, and Origin still has no family-root `managers` module.
