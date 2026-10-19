Unreal
===============

The sample project is `engines/Unreal/Shell3D`.

Unreal starts. The plugin loads the C++/CLI DLL with `LoadLibrary`. The DLL exports a C function and does not include Unreal headers.

The MSBuild projects are in `OpenX/dotnet/Engines/Unreal/CliBridge`. How to build and open the sample is in `engines/Unreal/Shell3D/README.md`.

Unreal Engine 5.8 compiled `Shell3DEditor`. A null-RHI game launch logged `GameX C++/CLI`.

UnrealSharp is only the sketch in `OpenX/dotnet/Engines/Unreal/Sharp`. This sample does not use it. UnrealCLR's Framework DLL in `OpenX/dotnet/Engines/Unreal/5.2` is pure IL and is not the host.
