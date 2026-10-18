# UnrealEngine.Framework.dll

Inspected in place at `OpenX/dotnet/Engines/Unreal/5.2/UnrealEngine.Framework.dll`. The DLL was not copied.

## Image

PE32, machine I386, `CorFlags=ILOnly` (not `32BitRequired`, not a mixed-mode or `/clr` image). No managed entry point. `UnrealEngine.Framework.deps.json` targets `.NETCoreApp,Version=v7.0`. Assembly references are framework 7.0.0.0 only: System.Runtime, System.Collections, System.Reflection.Emit, System.Numerics.Vectors, System.Drawing.Primitives, System.Runtime.Serialization.Formatters, System.Runtime.InteropServices.

## Native imports

None. Metadata tables `ModuleRef` and `ImplMap` have 0 rows. There is no `DllImport` and no `UnmanagedCallersOnly`. The only PE import is `mscoree.dll`, the CLR bootstrap, not an Unreal library. The C# does not name a native DLL.

Native calls are 692 static `delegate* unmanaged cdecl` fields. Internal `UnrealEngine.Framework.Shared.Load(IntPtr* events, IntPtr functions, Assembly pluginAssembly)` copies those pointers out of tables the caller passes in. A literal `checksum` of `0x2F0` has to match. Until that runs, every pointer is null.

## Public surface

169 types. `Engine` and `World` are static facades (`IsEditor`, `Version`, `MaxFPS`, `SetTitle`, `GetViewportSize`, `ActorCount`, `DeltaTime`, actor lookup, traces, `OpenLevel`). `Actor` is an instance around a native `IntPtr` (spawn, destroy, components, tags). The other public types are that same gameplay API: `Pawn`, `Character`, lights, `*Component`, `StaticMesh`, `SkeletalMesh`, `Material`, and the input delegates. Each method is a `calli` through one of those fields.

## Load test

64-bit process, loaded from the original path. .NET 7.0.20 and .NET 10.0.13 both loaded `UnrealEngine.Framework, Version=1.0.0.0` and enumerated 169 types. `Engine.IsEditor` then aborted the process. .NET 7: fatal `System.AccessViolationException` in `Engine.get_IsEditor`. .NET 10: `0xC0000005` in the same method. The function pointer was still null. No host ran.

## Standalone host

No. The UnrealCLR plugin does not host this DLL. It loads `hostfxr.dll` and binds `UnrealEngine.Runtime.Core.ManagedCommand` on `UnrealEngine.Runtime.dll` with `UNMANAGEDCALLERSONLY_METHOD`. That runtime later reflects into `Shared.Load` and passes Unreal's function tables. Those tables are not in this assembly, and this assembly has no unmanaged entry a small native program can request. Loading the IL does not host Unreal.