// C++/CLI bridge. MSBuild compiles this with /clr:netcore.
// UnrealBuildTool does not. A UE module loads the resulting DLL.

#include <string.h>
#include <vcclr.h>

using namespace System;
using namespace System::Runtime::InteropServices;

ref class Bridge {
public:
    static String^ Version() { return "GameX C++/CLI"; }
};

extern "C" __declspec(dllexport) int GameX_Unreal_Ping(char* buf, int cap) {
    if (!buf || cap < 1) return -1;
    String^ text = Bridge::Version();
    IntPtr raw = Marshal::StringToHGlobalAnsi(text);
    const char* src = static_cast<const char*>(raw.ToPointer());
    int n = (int)strlen(src);
    if (n >= cap) n = cap - 1;
    memcpy(buf, src, n);
    buf[n] = 0;
    Marshal::FreeHGlobal(raw);
    return n;
}
