# Shell3D

## What it is

A blank Win64 UE5 project. On startup the GameXUnrealBridge module loads `GameX.Unreal.CliBridge.dll` and keeps the ping string. `UShell3DGameInstance::OnStart` prints that string on screen and in the output log. The startup map is the engine template `Template_Default`, so the repo has no `.umap`.

## Build the bridge

Run `build-bridge.cmd` from this folder. That script builds the mixed DLL and the small native loader, runs the loader, and copies the DLL, `ijwhost.dll`, and the runtimeconfig json into `Plugins/GameXUnrealBridge/Binaries/Win64`. The loader prints `GameX C++/CLI` and exits 0. That does not compile the Unreal module. The script does not call UnrealBuildTool.

## Open it

This project is associated with Unreal Engine 5.8 (`EngineAssociation` is `5.8`). The target files use `BuildSettingsVersion.V7` and `EngineIncludeOrderVersion.Unreal5_8`, the same pair as the 5.8 blank template. Open `Shell3D.uproject` and compile `Shell3DEditor`. Press Play. The screen and the log show `GameX C++/CLI` when the DLL loaded. A negative code means the DLL was not next to `ijwhost`. Run the script again.

On this machine, Unreal Engine 5.8 compiled `Shell3DEditor`, and a `-nullrhi` game launch logged `Loaded GameX C++/CLI` and `LogTemp: Display: GameX C++/CLI`. The on-screen message was not looked at, because that launch has no renderer.

## Layout

- `Source` is the Shell3D game module.
- `Plugins/GameXUnrealBridge` is the plugin. It includes `unreal_load.cpp` from `OpenX/dotnet/Engines/Unreal/CliBridge` and does not duplicate that file.
- `Config` is the project ini. The startup map is `Template_Default`.
- The MSBuild projects are under `OpenX/dotnet/Engines/Unreal/CliBridge`: `GameX.Unreal.CliBridge.vcxproj` and `GameX.Unreal.CliLoader.vcxproj`. `bin/` and `obj/` there are build output and are already gitignored.

The plugin `Build.cs` refuses `/clr` and only adds the CliBridge include path, so `unreal_load.cpp` is compiled as ISO C++. `engines/Unreal/.gitignore` ignores `Binaries`, `Intermediate`, `Saved`, `DerivedDataCache`, and sln files.

## Limits

Windows x64 only. The bridge ping does not open a game archive. `GameX.Engine.Unreal` still throws if the Explorer starts Unreal itself. Do not add this project to `GameX.sln`.
