#include "CoreMinimal.h"
#include "Modules/ModuleManager.h"

// Defined in unreal_load.cpp. The sample project includes that file once,
// from engines/Unreal/Shell3D. Do not #include "unreal_load.cpp" here as well.
int GameX_Unreal_LoadBridge(const wchar_t* dllPath, char* buf, int cap);

class FGameXUnrealBridgeModule : public IModuleInterface {
public:
	virtual void StartupModule() override {
		char buf[64];
		GameX_Unreal_LoadBridge(L"GameX.Unreal.CliBridge.dll", buf, (int)sizeof buf);
	}

	virtual void ShutdownModule() override {
	}
};

IMPLEMENT_MODULE(FGameXUnrealBridgeModule, GameXUnrealBridge)
