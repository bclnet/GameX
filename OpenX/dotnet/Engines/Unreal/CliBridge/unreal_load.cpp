// The Unreal side of the bridge.
//
// Drop this into a UE module and call it from StartupModule. The module
// stays ISO C++. The mixed DLL beside it is built by MSBuild
// (GameX.Unreal.CliBridge.vcxproj), not by UnrealBuildTool.
//
// On Windows, FPlatformProcess::GetDllHandle is LoadLibrary. This file calls
// LoadLibrary so the same function builds here, where Unreal headers are
// not vendored.

#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include "unreal_load.h"

typedef int (*GameX_Unreal_PingFn)(char*, int);

int GameX_Unreal_LoadBridge(const wchar_t* dllPath, char* buf, int cap) {
    if (!dllPath || !buf || cap < 1) return -1;
    HMODULE lib = LoadLibraryW(dllPath);
    if (!lib) return -2;
    auto ping = (GameX_Unreal_PingFn)GetProcAddress(lib, "GameX_Unreal_Ping");
    if (!ping) return -3;
    return ping(buf, cap);
}
