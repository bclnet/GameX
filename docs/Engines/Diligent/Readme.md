Diligent
========

Engine id: `DN`. The id is `DN` because `DL` is Deathloop.

This slice registers the engine (caps: Drawing) and links a C shim that does not include Diligent headers. `diligentShimLinked` returns 0. `diligentShimVersion` returns `not linked`. Cargo feature `diligent` is off. Turning it on refuses to compile: this repository does not vendor Diligent. C# reports the same result in managed code and does not use C++/CLI. Python reports the same result and does not import a native module. There is no device and no swap chain. Host, sprite, and model methods throw. The Explorer does not list it.

DiligentCore C API (`Diligent_GetEngineFactoryVk`, `CreateDeviceAndContextsVk`) and a .NET sample. Apache-2.0. Neither is in this tree.
