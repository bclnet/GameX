Third Party
===

The files in this folder are **not** part of GameX and are **excluded from the MIT license** in the root [LICENSE](../LICENSE).
They are proprietary binaries owned by their respective rights holders, kept here only so the loaders that call them can be built and tested. No license to use, copy, modify, or redistribute them is granted by this repository.

| Path | Component | Rights holder | Used by
| -- | -- | -- | --
| `oodle/oo2ext_7_win64.dll` | Oodle data compression | Epic Games Tools (RAD Game Tools) | `dotnet/Core/GameX/_LIB/Compression/OodleLZ.cs`
| `xcompress/xcompress64.dll` | XCompress (XMem LZX) | Microsoft | `dotnet/Core/GameX/_LIB/Compression/XCompress.cs`, `python/gamex/_LIB/compression/xcompress.py`

### Notes
* Both binaries are Windows x64 only.
* `dotnet/Core/GameX/GameX.csproj` copies them to the build output (`oo2ext_7_win64.dll` and `x64/xcompress64.dll`).
* `oodle/oo2ext_7_win64.dll.meta` keeps the Unity asset guid. Unity projects need the file inside their `Plugins` folder; `engines/Plugins/Readme.md` has the `mklink /D Oodle ...` line that links this folder in.
* If you redistribute GameX, obtain these binaries under your own license or remove this folder.
