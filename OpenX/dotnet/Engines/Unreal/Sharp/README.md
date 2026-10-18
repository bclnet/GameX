# GameX C# inside UnrealSharp

Unreal is the host process. This folder is a sketch of a script that would run there. It is not a GameX engine, and nothing here is referenced by a csproj.

Confirmed from the [UnrealSharp README](https://github.com/UnrealSharp/UnrealSharp) and the current [setup](https://www.unrealsharp.com/getting-started-and-fundamentals/quickstart) page (not the older github.io notes, which still say UE 5.3 and .NET 8).

## Prerequisites

- Unreal Engine 5.6–5.8
- .NET SDK 10.0.5 or newer
- A C++ UE project (recommended; a Blueprint-only project is harder to support)
- The usual UE C++ build tools

The plugin is MIT licensed.

## Where the plugin is cloned

Clone [UnrealSharp/UnrealSharp](https://github.com/UnrealSharp/UnrealSharp) into that UE project's `Plugins/UnrealSharp` folder (the directory that contains `UnrealSharp.uplugin`). Do not clone it into this repo.

Compile the UE project from its solution. Avoid building by opening the `.uproject`. On first launch, accept the prompt and create the C# project. By default that project is `ProjectOrPluginRoot/Script` (`ScriptDirectoryName` in `Config/UnrealSharp.Settings.json` can change the folder). Further types are added from the U# toolbar (**Create New C# type**) or from the IDE: pick a name, a path under Script, and a parent class.

An actor is a `partial` class marked `[UClass]` (`UnrealSharp.Attributes`) that derives from `AActor` (`UnrealSharp.Engine`). `ActorSketch.cs` is that shape: `BeginPlay` calls `FamilyManager.GetFamily(...).GetArchive(...)`, then `AStaticMeshActor.Spawn(...)`.

## Why this cannot be a PackageReference

`OpenX.Engine.Unreal` is a normal `net10.0` library that GameX's process references. UnrealSharp is the other way around: the plugin hosts .NET inside the Unreal editor and game.

`AActor`, `[UClass]`, and `AStaticMeshActor.Spawn` are not a NuGet package. The plugin generates them from the reflected C++ of one engine version plus that project (the Script project and its regenerated ProjectGlue). Those assemblies appear only after that UE build, under the plugin's `Binaries/Managed`. A `PackageReference` on `OpenX.Engine.Unreal` would make GameX.sln depend on a UE install, a C++ project, and bindings that change per engine and per game. The weaver and source generator also run as part of the Script project, not as a dependency of OpenX.
