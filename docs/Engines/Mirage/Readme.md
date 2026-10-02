Mirage
===============

Engine code `MR`. A headless test engine: everything GameX would draw is built into a `Mirage.SceneTree` instead, and the tree has a canonical text form you can assert against.

```csharp
EngineX.Activate(MirageEngine.This);          // or MirageEngine.Reset() for a fresh tree
MirageRenderer.Register();                    // registers the family object builders (NIF, ...)

var family = FamilyManager.GetFamily("Bethesda");
var source = family.GetArchive(new Uri("game:/Morrowind.bsa#Morrowind"));
var value = source.GetAsset<object>("meshes/l/light_com_candle_04.nif").Result;
var renderer = MirageRenderer.CreateRenderer(null, EngineX.Gfx, source, value, "Object");
renderer.Start();

Console.WriteLine(MirageEngine.Tree.ToText());
// Root
//   _Prefabs hidden
//     light_com_candle_04
//       ...
//   Object
//     light_com_candle_04
//       Candle pos=(...) rot=(...)
//         @MeshRenderer mesh=Candle material=std2#...
```

| What | Where
| -- | --
| The engine itself | [Mirage/dotnet/Mirage](../../../Mirage/dotnet/Mirage) (`SceneTree`, `Node`, `Component`, `SceneText`)
| OpenX engine (`MirageEngine`, Gfx/Sfx, renderers) | `OpenX/dotnet/Engines/OpenX.Engine.Mirage`
| GameX engine (family builders, `MirageRenderer.CreateRenderer`) | `dotnet/Engines/GameX.Engine.Mirage`
| Tests | `Mirage/dotnet/MirageTests`, `OpenX/dotnet/Engines/OpenX.EngineTests/Mirage.cs`

Renderer types: `TestTri`, `Texture`/`VideoTexture`, `Sprite`, `Object`, `Material`, `Audio`.

`MirageEngine.Tree.Resources` lists every mesh, texture, material, shader, sprite and audio the engine was handed, one line each; `MirageEngine.Tree.Journal` records node adds, component readies, frames and log lines in order.
