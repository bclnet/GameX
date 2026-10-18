// C ABI for the openx-engine-unreal native shim.
// The default build does not include or link Unreal Engine.

#pragma once

#ifdef __cplusplus
extern "C" {
#endif

// 0 unless this binary was built against a real Unreal library.
int unreal_shim_linked(void);

// Static version string. The default build returns "unreal-shim-stub".
const char *unreal_shim_version(void);

#ifdef __cplusplus
}
#endif
