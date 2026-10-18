#include "CoreMinimal.h"
#include "Modules/ModuleManager.h"

// Defined in unreal_load.cpp. Add that file to the module when this source
// is copied into a UE project. Do not #include "unreal_load.cpp"; compiling
// it twice would duplicate the definition.
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
