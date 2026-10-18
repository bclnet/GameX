// Stand-in for an Unreal module. It calls the same loader the module would.

#include "unreal_load.h"
#include <cstdio>
#include <cstring>

int main() {
    char buf[64];
    int n = GameX_Unreal_LoadBridge(L"GameX.Unreal.CliBridge.dll", buf, (int)sizeof buf);
    if (n < 0) {
        std::printf("load failed %d\n", n);
        return 1;
    }
    std::printf("%s\n", buf);
    return std::strcmp(buf, "GameX C++/CLI") == 0 ? 0 : 2;
}
