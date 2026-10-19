Unreal Plugin
===

The plugin is `engines/Unreal/Shell3D/Plugins/GameXUnrealBridge`. It is a Win64 runtime module, loading phase PostEngineInit. It includes `unreal_load.cpp` from the CliBridge folder. It searches for `GameX.Unreal.CliBridge.dll` under the plugin `Binaries/Win64` folder, then the project `Binaries/Win64` folder, then the process DLL search path. `build-bridge.cmd` fills the plugin Binaries folder. The plugin is not a `GameX.sln` project.
