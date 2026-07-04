# Quake .map Runtime Importer for Unity

Parses a standard id-software **.map** text file at runtime (no editor step required) and
builds the level straight into a running Unity scene: brush geometry as meshes, entities as
GameObjects with behavior components.

## What's in here

```
Assets/
  Scripts/QuakeImporter/
    Data/QuakeTypes.cs              - plane/face/brush/entity data model + coord conversion
    Parsing/QuakeMapParser.cs       - hand-rolled .map text parser
    Geometry/QuakeBrushMeshBuilder.cs - turns brush planes into actual mesh vertices/UVs
    Geometry/QuakeGeometryBuilder.cs  - spawns one GameObject per brush
    Utility/QuakeMaterialUtil.cs    - texture-name -> Material lookup/placeholder generation
    Entities/QuakeEntitySpawner.cs  - classname -> spawn function registry
    Entities/QuakeEntityComponents.cs - func_door, trigger_multiple, player start, IQuakeTriggerable
    Runtime/QuakeRuntimeMapLoader.cs  - the MonoBehaviour you actually drop into a scene
    Runtime/QuakeQuickTestPlayer.cs   - bare WASD+mouselook capsule, for verifying the import
  StreamingAssets/Maps/sample_room.map - a small generated test level (room, doorway, sliding door)
```

## Setup

1. Copy the `Assets/` folder contents into your Unity project's `Assets/` folder.
2. Create an empty scene, add an empty GameObject, attach `QuakeRuntimeMapLoader`.
3. Leave `streamingAssetsMapPath` as `Maps/sample_room.map` (or drag a `.map` file in as a
   `TextAsset` and assign it to `mapTextAsset`, which takes priority if set).
4. Press Play. You should land in a small room facing a doorway; walk toward it and the door
   slides open via its trigger volume, then auto-closes after 3 seconds. You can also walk
   directly into the door itself to open it.

No Quake assets (WADs, textures, BSPs) are required for this to render — see "Textures" below.

## What's implemented vs. not

**Implemented:** the standard 3-point-per-face `.map` text format, brush→mesh conversion via
plane-triple intersection (the same fundamental technique real Quake tools use, since brushes
store no vertices, only bounding planes), id software's classic per-face texture-axis UV
projection, `worldspawn`/`func_wall`/`func_door`/`trigger_multiple`/`light`/`info_player_start`,
and `clip`/`skip`/`hint`/`origin`/`trigger` surface semantics (invisible and/or non-solid as
appropriate).

**Not implemented**, and worth knowing about if you keep going: the compiled binary `.bsp`
format (this only reads the original text `.map`, which is the actual source format — `.bsp`
also bakes in PVS/lightmaps/etc. and is a separate, much larger parsing job); the **Valve 220**
UV format (maps exported with explicit `[ux uy uz off]` axis vectors instead of bare numbers —
the parser detects this and throws a clear error telling you to re-export as "Standard" format
in TrenchBroom); monsters, weapons, pickups, and most other gameplay entities (only the handful
listed above have spawners — anything else with brushes still gets its geometry built, anything
else without brushes is skipped with a warning); real WAD texture import (see below); and any
performance batching (every brush is its own GameObject/mesh — fine for small-to-medium maps,
but a large id1 level would benefit from merging brushes per-material before shipping).

## Coordinate conversion

Quake is Z-up/right-handed; Unity is Y-up. Conversion is a straight Y/Z swap plus a uniform
scale (`worldScale`, default `1/32`, i.e. "32 Quake units per meter"). If you load a real map
and geometry looks inside-out (you see through walls from outside but they vanish from inside,
or vice versa), toggle **Flip Winding** on the loader rather than touching the math — the mesh
builder already self-corrects each face's triangle winding to match its own plane normal, so a
global flip should only ever be needed if a real-world map's plane convention doesn't match the
one implemented here (id software's `Cross(p0-p1, p2-p1)`, anchored at p1).

## Textures

Real `.map` files reference WAD texture names, not Unity materials. This importer doesn't parse
WADs. Instead, for each distinct texture name it first looks for a hand-made material at
`Resources/QuakeMaterials/<textureName>.mat` (so dropping in your own converted textures "just
works" — name the material file after the texture name used in the `.map`); if none exists, it
generates a flat, deterministically-colored placeholder material so different surfaces are at
least visually distinguishable out of the box.

UV tiling assumes a 256×256 texture (a common original Quake texture size) since the actual
texture dimensions aren't known without loading the WAD — if your real textures are a different
size, tweak `defaultTextureSize` in `QuakeBrushMeshBuilder.ComputeUV`, or extend
`QuakeMaterialUtil` to look up real dimensions per material.

## Loading a real map

Export a `.map` from TrenchBroom (or similar) in **Standard** format (not Valve 220), then either:
- drag the `.map` file into Unity, change its import extension recognition if needed (Unity may
  treat it as a generic text asset already), and assign it to `mapTextAsset`, or
- drop it under `Assets/StreamingAssets/Maps/` and point `streamingAssetsMapPath` at it.

If parsing throws, the exception message tells you why (Valve220 format, malformed brush, etc.)
rather than failing silently.

## Extending it

- New entity types: add an entry to the `Spawners` dictionary in `QuakeEntitySpawner.cs`.
- New trigger-able behaviors (plats, buttons): implement `IQuakeTriggerable.Activate(...)`
  exactly like `QuakeFuncDoor` does — `QuakeRuntimeMapLoader` already resolves `target` →
  `targetname` links generically via `QuakeTriggerRelay`.
- Mesh batching: combine all `BuiltBrush.RenderMesh` instances sharing a material into one
  `Mesh` per material per entity (or per level) using `Mesh.CombineMeshes`, instead of one
  GameObject per brush.
