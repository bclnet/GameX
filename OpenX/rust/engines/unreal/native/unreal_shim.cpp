// Native shim for openx-engine-unreal.
//
// Cargo feature `unreal` (off by default) defines GAMEX_WITH_UNREAL for this
// file. The default build does not include Unreal headers and does not link
// Unreal Engine: unreal_shim_linked returns 0 and unreal_shim_version returns
// "unreal-shim-stub". Feature `unreal` is for a normal native library already
// produced by UnrealBuildTool (ISO C++, not /clr), with this shim outside UBT.
// This repository does not ship Unreal Engine, so that build is expected to fail.

#include "unreal_shim.h"

extern "C" {

int unreal_shim_linked(void) {
    return 0;
}

const char *unreal_shim_version(void) {
    return "unreal-shim-stub";
}

}

#if defined(GAMEX_WITH_UNREAL)
// Compiled out of the default build. A real host would call, and not stub:
//   UEngine *GEngine;                    Engine/Engine.h
//   FEngineLoop::PreInit / Init / Tick   Launch/LaunchEngineLoop.h
//   IModuleInterface::StartupModule      Modules/ModuleInterface.h
//   FModuleManager::LoadModuleChecked    Modules/ModuleManager.h
// Those headers are not included on any path in this tree.
#error GAMEX_WITH_UNREAL needs an Unreal Engine install. This repository does not vendor UE.
#endif
