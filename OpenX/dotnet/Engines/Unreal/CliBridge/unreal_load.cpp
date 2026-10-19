// The Unreal side of the bridge.
//
// Included once by engines/Unreal/Shell3D. The module stays ISO C++.
// The mixed DLL beside it is built by MSBuild
// (GameX.Unreal.CliBridge.vcxproj), not by UnrealBuildTool.
//
// On Windows an absolute path uses LoadLibraryEx so ijwhost.dll beside the
// mixed DLL is found inside Unreal. A relative path still uses LoadLibrary.
// This file does not include Unreal headers.

#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include "unreal_load.h"

typedef int (*GameX_Unreal_PingFn)(char*, int);

// Unreal calls SetDefaultDllDirectories, so a plain LoadLibrary does not
// search the mixed DLL's folder for ijwhost.dll. An absolute path plus
// LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR puts that folder back on the search.
static HMODULE LoadBridgeLibrary(const wchar_t* dllPath) {
    const bool absolute = dllPath[0] == L'\\' || (dllPath[0] != 0 && dllPath[1] == L':');
    if (!absolute) return LoadLibraryW(dllPath);
    return LoadLibraryExW(
        dllPath,
        nullptr,
        LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR | LOAD_LIBRARY_SEARCH_DEFAULT_DIRS);
}

int GameX_Unreal_LoadBridge(const wchar_t* dllPath, char* buf, int cap) {
    if (!dllPath || !buf || cap < 1) return -1;
    HMODULE lib = LoadBridgeLibrary(dllPath);
    if (!lib) {
        int err = (int)GetLastError();
        return err > 0 ? -err : -2;
    }
#pragma warning(push)
#pragma warning(disable: 4191)
    auto ping = (GameX_Unreal_PingFn)GetProcAddress(lib, "GameX_Unreal_Ping");
#pragma warning(pop)
    if (!ping) return -3;
    return ping(buf, cap);
}
