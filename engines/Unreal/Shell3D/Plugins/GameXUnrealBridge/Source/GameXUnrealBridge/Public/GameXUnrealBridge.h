#pragma once

#include "CoreMinimal.h"
#include "Modules/ModuleManager.h"

class FGameXUnrealBridgeModule : public IModuleInterface
{
public:
	static FGameXUnrealBridgeModule& Get()
	{
		return FModuleManager::LoadModuleChecked<FGameXUnrealBridgeModule>(TEXT("GameXUnrealBridge"));
	}

	virtual void StartupModule() override;
	virtual void ShutdownModule() override;

	const FString& GetPing() const { return Ping; }
	int GetLoadCode() const { return LoadCode; }

private:
	FString Ping;
	int LoadCode = -1;
};
