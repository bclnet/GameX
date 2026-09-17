# CryLevelImporter — runtime CryEngine 3 level loader for Unity

A minimal runtime importer that reconstructs a Crysis 2-era CryEngine level
inside Unity: terrain, object layers, entities, brush placements, and a
time-of-day lighting snapshot. Same shape as a Quake `.map` importer —
parse → build world → spawn entities via a class registry — but fanned out
across the multiple files a CryEngine level actually consists of.

## Setup

1. Copy `Runtime/` into your Unity project (2021.3+; works in Built-in and URP).
2. Extract the level: a shipped level folder contains `level.pak` (a zip).
   Unzip it in place so the XML files sit next to it:

```
MyLevel/
├── LevelData.xml          ← terrain dims + layer list
├── Mission_MyMission.xml  ← entities & brushes
├── TimeOfDay.xml          ← lighting curves
└── terrain/
    └── heightmap.raw      ← 16-bit raw (export from Sandbox, or omit)
```

3. Empty GameObject → add `CryLevelLoader` → set **Level Folder** to that
   path → Play.

## What maps to what

| CryEngine                  | Unity (this importer)                          |
|----------------------------|------------------------------------------------|
| Level                      | Root GameObject (Scene-per-level in a real port) |
| Object layer               | Child container GameObject, toggleable via `SetLayerActive` (stand-in for additive scenes) |
| Terrain heightmap          | Runtime-built `TerrainData` + `Terrain`        |
| Entity (`Mission_*.xml`)   | Prefab/handler via `CryEntitySpawner` registry |
| Brush (`.cgf` placement)   | Placeholder cube, or your prefab via `CryBrushResolver.PrefabLookup` |
| TimeOfDay.xml              | Directional sun + fog + ambient snapshot       |

## Extending

**Custom entity classes** — register a handler, exactly like mapping a Quake
classname to a spawn function:

```csharp
var loader = GetComponent<CryLevelLoader>();
loader.EntitySpawner.Register("AmmoCrate", (entity, parent) => {
    var go = Instantiate(ammoCratePrefab, parent);
    if (entity.TryGetFloat("Capacity", out var cap))
        go.GetComponent<AmmoCrate>().capacity = (int)cap;
    return go;
});
```

**Real brush geometry** — pre-convert `.cgf` meshes (e.g. via CGF → FBX
tooling offline) into prefabs, then:

```csharp
loader.BrushResolver.PrefabLookup = cgfPath =>
    myPrefabTable.TryGetValue(cgfPath, out var p) ? p : null;
```

**Layer streaming** — `SetLayerActive(name, bool)` toggles a layer container;
swap the container implementation for `SceneManager.LoadSceneAsync(...,
LoadSceneMode.Additive)` to get true streaming parity.

## Known limitations

- `.cgf`/`.cga` binary meshes are not parsed — placeholders + hook only.
- Terrain splat/material layers, vegetation instances, and voxel terrain
  objects are not imported.
- TimeOfDay is sampled at one keyframe, not imported as animated curves.
- FlowGraph logic is not translated (entity properties are preserved on
  `CryEntityInfo` so gameplay code can act on them).
- XML shapes vary between CryEngine builds; parsers are defensive but you
  may need to adjust XPath queries for a specific game build.
