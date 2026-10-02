Mirage
===

Mirage is a lightweight, headless test engine that simulates a real-time game loop with pure text and console outputs. Designed entirely for decoupled unit testing, CI/CD validation, and logic benchmarking without a graphical overhead.

The interface is the familiar one - a `SceneTree` of `Node`s, each with a `Transform`, `Component`s and children, plus named `Resource`s (`Mesh`, `Material`, `Texture`, `Shader`, `Sprite`, `Audio`, `TerrainData`) - but nothing is drawn. Instead every scene has a canonical text form that tests can assert against, and every lifecycle event goes into a journal.

```
Root
  Player layer=1 pos=(1,0,0)
    @MeshRenderer mesh=Cube material=Std
    @MeshCollider mesh=Cube
    Weapon rot=(0,0.7071,0,0.7071) scale=(2,2,2) hidden
      @MeshRenderer mesh=Sword
```

### Using it

```csharp
var tree = new SceneTree();
var player = tree.Root.AddChild("Player");
player.Transform.Position = new Vector3(1, 0, 0);
player.AddComponent(new MeshRenderer { Mesh = tree.Resources.Get<Mesh>("Cube") });
player.AddComponent(new Script { OnProcess = delta => player.Transform.Position += new Vector3(delta, 0, 0) });

tree.Run(60, 1f / 60f);            // sixty deterministic frames
Assert.AreEqual(2f, player.Transform.Position.X, 1e-4f);
Assert.IsTrue(SceneText.AreEqual(@"
Root
  Player pos=(2,0,0)
    @MeshRenderer mesh=Cube
    @Script
", tree.ToText()));
```

* `SceneText.Write` / `SceneText.Parse` round-trip the text form; `SceneText.Normalize` gives the canonical form of hand-written text.
* `SceneTree.Step` advances one frame: `Ready()` once per component, then `Process(delta)` for every enabled one, depth first. `Run(frames)` and `RunUntil(predicate)` build on it.
* `SceneTree.Journal` records `add`, `remove`, `destroy`, `component`, `ready`, `frame` and `log` lines in order; `Trace = true` echoes them to `Out`.
* `SceneTree.Load(text)` builds a scene from text; `Print()` writes the scene to `Out` (the console by default).
* Custom components: derive from `Component`, override `Ready`/`Process`, and `WriteProps`/`ReadProp` if they should appear in text; `SceneText.Register<T>()` lets the parser create them.

### Layout

| Path | Contents
| -- | --
| `dotnet/Mirage` | the engine (`netstandard2.1`, no dependencies beyond `System.Drawing.Common`)
| `dotnet/MirageTests` | MSTest suite

GameX drives Mirage through `OpenX.Engine.Mirage` (engine code `MR`) and `GameX.Engine.Mirage`, so any asset GameX can load can be rendered into a Mirage scene and checked as text.
