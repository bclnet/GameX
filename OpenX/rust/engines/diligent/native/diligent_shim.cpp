// Native shim for openx-engine-diligent.
//
// Cargo feature `diligent` (off by default) defines GAMEX_WITH_DILIGENT for this
// file. The default build does not include Diligent headers and does not link
// Diligent Engine: diligentShimLinked returns 0 and diligentShimVersion returns
// "not linked". Feature `diligent` is for DiligentCore's C API already built
// as a normal native library (ISO C++, not /clr), with this shim outside that
// build. This repository does not ship Diligent, so that build is expected to fail.

#include "diligent_shim.h"

extern "C" {

int diligentShimLinked(void) {
    return 0;
}

const char *diligentShimVersion(void) {
    return "not linked";
}

}

#if defined(GAMEX_WITH_DILIGENT)
// Compiled out of the default build. A real host would call, and not stub:
//   Diligent_GetEngineFactoryVk()                 EngineFactoryVk.h
//   IEngineFactoryVk_CreateDeviceAndContextsVk
//   IEngineFactoryVk_CreateSwapChainVk
// Those headers are not included on any path in this tree.
#error GAMEX_WITH_DILIGENT needs a Diligent Engine install. This repository does not vendor Diligent.
#endif
