# Gfx sprite decisions

Living notes. The type map is [gfx.svg](gfx.svg). The engine slot grid is [abstractions.svg](abstractions.svg). Sfx is [../Sfx/sfx.svg](../Sfx/sfx.svg). This file is the decision. The slot names in code are `Sprite` and `SpriteAlt`.

Date started: 2026-11-27.

## Decisions

### Sprite and SpriteAlt stay separate

`Sprite` is the majority slot (`GfX.XSprite`, slot 1): Unity canvas, Godot `Sprite2D`, MonoGame, EginX, and SDL.

`SpriteAlt` is the other slot (`GfX.XSpriteAlt`, slot 2): a 3D billboard or a quad. Godot `Sprite3D`, and the 3D engines that only register that slot.

### The two slots stay separate

`GfX.XSprite` is slot 1 and `GfX.XSpriteAlt` is slot 2 of `Engine.GfxFactory`. Both slots carry one `ISprite` through `IOpenGfxSprite` and `SpriteManager`. The split is where that sprite is attached.

- Godot has two host nodes: `Sprite2D` (canvas) and `Sprite3D` (a quad in the 3D scene). Those host names stay. `GodotGfxSprite` and `GodotGfxSpriteAlt` match those nodes. Their `SpriteManager` lines are commented out.
- Unity has one host type, `UnityEngine.Sprite`. `UnityGfxSprite` and `UnityGfxSpriteAlt` are two slots, both `IOpenGfxSprite<GameObject, Sprite>`. The Sprite slot is the canvas placement. The SpriteAlt slot is a billboard in the world. Neither constructs `UnitySpriteBuilder`.
- Mirage's picture resource is already `Sprite`, so the component that hangs it on a node stays `Sprite2D`. `SpriteAlt` is an empty subclass of that component. Both slots construct `MirageSpriteBuilder`. Mirage is the only engine that does.
- MonoGame and EginX register only the Sprite slot. OpenGL, Vulkan, Stride, Unreal, Ogre, bgfx, wgpu, Diligent, Raylib, and O3DE register only the SpriteAlt slot. SDL registers `SdlGfxSprite` in the Sprite slot.

`Sprite` and `SpriteAlt` each place one sprite. `GfxSpriteSheet` is the atlas those slots play. `GfxUnknown` is the old scene-node loader, parked until its job is clear.

### TextureSequences stays in OpenX.Gfx

Valve's `GetSpriteSheetData` fills it. The OpenGL particle renderer reads `info["sequences"]` as this type. Rust has the same type. Moving it into `GameX.Valve` would make `OpenX.Engine.OpenGL` depend on a family. The type stays in `OpenX.Gfx`. The producer stays in Valve.

### TextureSequences is not a sprite sheet

`TextureSequences` in `OpenX.Gfx` is the Source 2 particle sheet. Valve's `GetSpriteSheetData` fills it. A sequence has a name, frames per second, and frames. A frame has one or more images. An image is a pair of UV rectangles (`CroppedMin` / `CroppedMax`, `UncroppedMin` / `UncroppedMax`), plus particle flags (`Clamp`, `AlphaCrop`, `NoColor`, `NoAlpha`, `FloatParams`). The OpenGL particle renderer reads `info["sequences"]` as this type.

It has no pivot, no palette, and no single pixel rectangle. It stays under its own name. A later adapter may copy one sequence into a `GfxSpriteSheet`. It is not renamed and not used as the sheet type.

### GfxModel is the mesh path

`IGfxModel`, `GfxModelBuilder`, and `GfxModelManager` are the mesh path. `Object` was prefixed because `Model` is a common word. The name is `GfxModel`, in the same family as `GfxShader`. The factory slot stays Model. That slot is the engine object, not this type.

### GfxUnknown is the parked loader

`GfxUnknownBuilder` and `GfxUnknownManager` were `ObjectSpriteBuilder` and `ObjectSpriteManager`. `Create` loads an archive object and returns a scene node (`B_Object`). `Instance` parents that node. `B_Sprite` is a type parameter and the methods never read it. No engine constructs the manager. The only mention outside `OpenX.Gfx` is a comment in `O3deGfxSpriteAlt`. The name stays `GfxUnknown` until that loader has a job.

### GfxSpriteSheet is the atlas

`GfxSpriteSheet` is in `OpenX.Gfx`, in C#, Python, and Rust. `Texture` is the packed image. `Frame` is a nested cell: `X`, `Y`, `Width`, `Height` in pixels from the top left, `Pivot` in 0..1, `Duration` in seconds. `Anim` is nested the same way: a name plus indexes into `Frames`. Rust has no nested class, so those two are `GfxSpriteSheetFrame` and `GfxSpriteSheetAnim`.

`IOpenGfxSprite.Frame(object texture, GfxSpriteSheet sheet, int index)` returns `(B_Sprite sprite, GfxSpriteSheet.Frame frame)`. `texture` is the host texture `TextureManager` already uploaded. The sprite slot does not upload it. That is why the argument is `object`: `B_Sprite` is not the texture type. Unity's `B_Sprite` is `Sprite`, OpenGL's is a texture id (`int`), and a MonoGame slot's is `object`.

Stride 4.3 already has `Stride.Graphics.SpriteSheet` and `Stride.Graphics.SpriteFrame`. The `Gfx` prefix keeps our atlas off those names. Stride's `Frame` still returns the texture and the cell. It does not construct `Stride.Graphics.Sprite`.

What each engine does with that call:

| Engine | `Frame` returns |
| --- | --- |
| Unity | `Sprite.Create(texture, rect, pivot)` on both slots |
| Godot | `AtlasTexture` region on `Sprite2D`, and the same region on `Sprite3D` |
| Mirage | a picture resource of the cell size. The resource has no source rect, so the rect comes back beside it |
| OpenGL | the same texture id, plus the cell. Sampling the rect is still the caller's job |
| MonoGame, EginX, SDL, Vulkan, Unreal, Ogre, Stride, bgfx, wgpu, Diligent, Raylib, O3DE | the same texture, plus the cell |
| Test | throws |

EginX is our engine. Its `Frame` is the texture-and-rect form, same as MonoGame.

Python has no Unity, Godot, Mirage, SDL, MonoGame, Stride, Ogre, O3DE, or Unreal engine module. The Python sprite classes that exist (OpenGL, EginX, Vulkan, bgfx, wgpu, Diligent, Raylib, Test) have the same `frame` method. Rust's `IOpenGfxSprite::frame` is on the trait. No Rust engine implements that trait yet.

## Existing Gfx objects

| Name | Where | What it is |
| --- | --- | --- |
| `ITexture` | `OpenX.Gfx` | One image. Width, height, depth, mips, `Create`. |
| `ITextureFrames` | `OpenX.Gfx` | A flipbook on a texture. `Fps`, `HasFrames`, `NextFrame`. No rectangles. |
| `GfxSpriteSheet` | `OpenX.Gfx` | One texture. `Frame` and `Anim` are nested on it. `IOpenGfxSprite.Frame` plays one cell. |
| `ISprite` | `OpenX.Gfx` | One sprite image. Width, height, `Create`. |
| `SpriteManager` / `SpriteBuilder` | `OpenX.Gfx` | Load an `ISprite` and upload it. The engine supplies the builder. |
| `TextureSequences` | `OpenX.Gfx` | Valve particle sequences. See the decision above. |
| `IOpenGfxSprite` | `OpenX.Gfx` | The engine slot that owns a `SpriteManager`. |

`GfxFactory` order is API, Sprite, SpriteAlt, Model, Light, Terrain.

## Engine slots

Live means the class constructs its manager. Shell means the class is registered and the manager is not constructed. Empty means a null slot.

`yes` means the class is registered. Sprite `live` and `shell` mean the same as on the slot grid. This table is the C# `GfxFactory`.

| Engine | API | Sprite | SpriteAlt | Model | Light | Terrain |
| --- | --- | --- | --- | --- | --- | --- |
| Mirage | yes | live | live | yes | yes | yes |
| Unity | yes | shell | shell | yes | yes | yes |
| Godot | yes | shell | shell | yes | empty | empty |
| MonoGame | empty | shell | empty | empty | empty | empty |
| EginX | empty | shell | empty | empty | empty | empty |
| SDL | empty | shell | empty | empty | empty | empty |
| OpenGL | yes | empty | shell | yes | empty | yes |
| Vulkan | yes | empty | shell | yes | empty | empty |
| Unreal | yes | empty | shell | yes | empty | empty |
| Ogre | empty | empty | shell | yes | empty | empty |
| Stride | empty | empty | shell | yes | empty | empty |
| bgfx | empty | empty | shell | yes | empty | empty |
| wgpu | empty | empty | shell | yes | empty | empty |
| Diligent | empty | empty | shell | yes | empty | empty |
| Raylib | empty | empty | shell | yes | empty | empty |
| O3DE | empty | empty | shell | yes | empty | empty |
| Test | yes | registered | registered | yes | Terrain sits here | no entry |
| Unknown | empty | empty | empty | empty | empty | empty |

Test's factory has five entries, so `TestGfxTerrain` occupies the Light index. Sprite and SpriteAlt are the same `TestGfxSprite` class. Unknown registers six nulls. Bevy is Rust only and sets no factory.

Python-only engines: Panda3D registers API, Model, Light, and Terrain. PyEngine3D registers API and Model. Pygame registers Model. Godot's Python factory leaves both sprite slots empty and registers Light and Terrain. OpenGL's Python factory registers Light. Those two do not match this table.

## How the engines are used

Rank is how much of GameX actually runs on that host. A lower number is a host something already opens. This is not a quality ranking. EginX is our engine.

| Rank | Engine | Id | Where it is used | Sheet |
| --- | --- | --- | --- | --- |
| 1 | OpenGL | GL | C# Explorer control and the Rust Explorer. The Rust Explorer draws Arx textures. `App.Client` `-e GL` still constructs the MonoGame host. | texture id + rect |
| 2 | MonoGame | MG | `App.Client` with no arguments plays Origin UO through this host. The Ultima draw path is `GumpArt`, not `MgGfxSprite`. | texture + rect |
| 3 | Mirage | MR | Headless tests. Both sprite slots construct `SpriteManager`. Not in the WPF Explorer list. | picture + rect beside it |
| 4 | Unity | UN | Explorer control. The renderer call is commented out. The shell player is the view the owner runs. Object builders are constructed. Sprite managers are not. | `Sprite.Create` |
| 5 | Vulkan | VK | Explorer control. The view presents. Sprite manager is not constructed. | texture + rect |
| 6 | EginX | EX | Ours. `App.Client -e EX`. The Python Explorer registers it. Not in the WPF Explorer list. | texture + rect |
| 7 | Godot | GD | Explorer control. The renderer call is commented out. Shell pipe code is in the engine project. Sprite managers are not constructed. | `AtlasTexture` |
| 8 | Stride | ST | Explorer control calls `StrideRenderer` for NIF, FTL, FTS, and BSP. Sprite manager is not constructed. `StrideClientHost.Run` throws. Nothing else opens it. | texture + rect |
| 9 | SDL | SD | Explorer control. Sprite manager is not constructed. The Rust backend is still a comment. | texture + rect |
| 10 | Ogre | OG | Explorer control calls `OgreRenderer`. Sprite manager is not constructed. | texture + rect |
| 11 | O3DE | O3 | Explorer control calls `O3deRenderer`. Sprite manager is not constructed. | texture + rect |
| 12 | Unreal | UR | Explorer control. The renderer call is commented out. The Rust shim reports `unreal-shim-stub`. | texture + rect |
| 13 | bgfx, wgpu, Diligent, Raylib | BX, WG, DN, RL | Registration stubs. The C# Explorer does not list them. | texture + rect |
| 14 | Bevy | BV | Rust only. No `GfxFactory`, no C# project, no Python module. The crate does not depend on Bevy. | no `Frame` |
| 15 | Test, Unknown | TT, UK | Test throws. Unknown is six empty slots. | Test throws |

Stride is the weakest host that the Explorer still lists. The view and the four object builders are real. The sprite slot and the client host are empty. Dropping it means removing that Explorer view. It is still in the tree.

Panda3D, PyEngine3D, and Pygame are Python only. They register no sprite slot, so they have no `Frame`.
