# Engines

An engine is registered in a language only when that language can host it. There is no C++ port of GameX. C++ appears as the native library, or as a Rust build shim.

## Type

| Type | Meaning | Engines |
| -- | -- | -- |
| Game engine | A full runtime. | Unity, Unreal, Godot, Stride, O3DE, Bevy, Panda3D |
| Render engine | Draws a scene. Not a full game. | Ogre, PyEngine3D |
| Display | A graphics API, or a thin window and draw library. | OpenGL, Vulkan, wgpu, bgfx, Diligent, SDL, Raylib, Pygame |
| Game framework | A game loop and a graphics device. | MonoGame, EginX |
| Test | A stand-in used to check rendering. | Mirage, Test |
| Fallback | A stand-in used when no engine is selected. | Unknown |

A language cell names the library that project links. `stub` means the project exists and does not link the engine. `—` means that language has no project. `C# host` says which process runs the C# for that engine.

| Id | Engine | Type | Native | C# | C# host | Python | Rust | Description |
| -- | -- | -- | -- | -- | -- | -- | -- | -- |
| GL | OpenGL | Display | C | OpenTK 5.0.0-pre.16 | GameX process | PyOpenGL | glow 0.17 | Graphics API. The Explorers that draw use this. |
| UN | Unity | Game engine | C++ | UnityEngine 6000.4.5f1 | Inside Unity | — | stub | Full game runtime. The C# project runs inside the editor. |
| UR | Unreal | Game engine | C++ | stub | Inside Unreal: UnrealSharp, UnrealCLR, or a C++/CLI DLL | — | C++ shim, not linked | Full game runtime. No port links an Unreal install. |
| GD | Godot | Game engine | C++ | GodotSharp 4.6.2 | Inside Godot | stub | stub | Full game runtime. The Python folder still calls Panda3D. |
| SD | SDL | Display | C | ppy.SDL3-CS | GameX process | — | stub | Window, input, and a simple draw layer. |
| ST | Stride | Game engine | C# | Stride 4.3.0.2507 | Inside Stride | — | stub | Full game runtime, formerly Xenko. |
| OG | Ogre | Render engine | C++ | stub | — | — | stub | Scene-graph renderer. Not a full game runtime. |
| MG | MonoGame | Game framework | C# | MonoGame.Framework.DesktopGL 3.8.5-preview.6 | GameX process | — | stub | XNA-style game loop and graphics device. |
| EX | EginX | Game framework | in-tree | in-tree | GameX process | in-tree | in-tree | In-tree XNA-style game loop. The graphics device is empty. |
| MR | Mirage | Test | C# | Mirage | GameX process | — | — | Headless scene tree. Tests assert on its text form. |
| O3 | O3DE | Game engine | C++ | stub | — | — | stub | Open 3D Engine, the Lumberyard successor. |
| VK | Vulkan | Display | C | Silk.NET.Vulkan 2.23.0 | GameX process | vulkan | ash 0.38 | Graphics API. Each port opens an instance. |
| RL | Raylib | Display | C | Raylib-cs 8.1.0 | GameX process | pyray | stub | Window and immediate-mode drawing. |
| BX | bgfx | Display | C++ | stub | — | stub | stub | Cross-API renderer. You bring your own engine. |
| BV | Bevy | Game engine | Rust | — | — | — | stub | ECS game engine. Rust registers the id only. |
| WG | wgpu | Display | Rust | Ahjo.Wgpu 0.7.0 | GameX process | wgpu | wgpu 22.1.0 | WebGPU API. Each port opens an instance and an adapter. |
| DN | Diligent | Display | C++ | stub | — | stub | C shim, not linked | Graphics API over D3D, Vulkan, OpenGL, and WebGPU. |
| PD | Panda3D | Game engine | C++ | — | — | panda3d | — | Full game runtime. Python only. |
| PG | Pygame | Display | Python | — | — | pygame | — | Python wrapper over SDL for 2D windows and drawing. |
| P3 | PyEngine3D | Render engine | Python | — | — | stub | — | Python 3D renderer. The project imports no package. |
| TT | Test | Test | in-tree | core | core | core | core | Used when the process is a test host. |
| UK | Unknown | Fallback | in-tree | core | core | core | core | Used when no engine is selected. |

`TT` and `UK` live in core and are the startup engines. The others are `OpenX.Engine.*` / `openx.engines` / `openx-engine-*`, with a matching `GameX.Engine.*` or `gamex-engine-*` where that language has object builders.

## C# inside Unreal

Unreal is the process that starts. `OpenX/dotnet/Engines/Unreal/5.2/UnrealEngine.Framework.dll` is pure C#: its CLI header has `ILONLY` set, so it is not a C++/CLI mixed assembly. It is UnrealCLR's managed API (.NET 7, 32-bit). The native plugin that hosts the CLR is not in this repo. UnrealBuildTool does not compile `/clr`.

| Option | What loads the C# | Status here |
| -- | -- | -- |
| UnrealSharp | A UE 5.6–5.8 plugin generates the C# API from reflected C++. .NET 10.0.5 or newer. | Sketch only, in `OpenX/dotnet/Engines/Unreal/Sharp/`. The plugin is cloned into a UE project's `Plugins` folder, not this repo. |
| UnrealCLR | A native UE plugin hosts the CLR, binds `UnrealEngine.Runtime.dll`, and that runtime fills function pointers in `UnrealEngine.Framework.dll`. | The Framework DLL is in `Unreal/5.2` (pure IL, .NET 7, 169 types, no `DllImport`). Loading it and calling `Engine.IsEditor` crashes because those pointers are still null. A standalone host is not possible. See `Unreal/ClrHost/FINDINGS.md`. |
| C++/CLI DLL | MSBuild builds a mixed DLL outside UnrealBuildTool. A small Unreal C++ file loads it with `LoadLibrary`. The DLL exports a C ABI and does not include Unreal headers. Windows only. | Built and loaded. `OpenX/dotnet/Engines/Unreal/CliBridge/`. The native loader prints `GameX C++/CLI`. No Unreal Engine is installed (the registry entries for 4.27 and 5.1–5.3 point at folders that are gone), so `GameXUnrealBridgeModule.cpp` is not compiled by UnrealBuildTool. |

## What each Explorer lists

| App | Ids |
| -- | -- |
| C# WPF (`GameX.App.Explorer`) | GD, O3, OG, GL, SD, ST, UN, UR |
| C# MAUI (`GameX.App.ExplorerM`) | GL |
| Python (`python/gamex/app/explorer`) | EX, GL, PD, P3, PG |
| Rust (`gamex-app-explorer`) | GD, O3, OG, GL, SD, ST, UN, UR |

MG, MR, VK, RL, BX, BV, WG, and DN are not in those lists. Of the Rust Explorer set, OpenGL is the one that draws.

## Notes

- Python `openx/engines/godot` is a copy of the Panda3D engine. Its modules import `panda3d.core`.
- Rust Raylib has no `raylib` crate. The crates.io crate needs a newer rustc than this workspace (1.75).
- Rust Bevy registers `BV` and does not depend on the `bevy` crate.
- Rust Unreal and Diligent compile a C++ shim. Both features stay off, and both report not linked. The C# Unreal engine throws. C# and Python Diligent have no package.
- C# and Python bgfx have no package. Rust bgfx has no bgfx crate.
- wgpu in all three languages creates an instance and an adapter. It does not draw a surface yet.
- Vulkan in all three languages creates an instance. The render path is still open.
- Raylib in C# and Python calls a version check. It does not open a window.
