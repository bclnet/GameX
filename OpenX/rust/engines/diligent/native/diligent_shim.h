// C ABI for the openx-engine-diligent native shim.
// The default build does not include or link Diligent Engine.

#pragma once

#ifdef __cplusplus
extern "C" {
#endif

// 0 unless this binary was built against a real Diligent library.
int diligentShimLinked(void);

// Static version string. The default build returns "not linked".
const char *diligentShimVersion(void);

#ifdef __cplusplus
}
#endif
