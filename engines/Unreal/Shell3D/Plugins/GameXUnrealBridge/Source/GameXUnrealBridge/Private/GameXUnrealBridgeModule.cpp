#include "GameXUnrealBridge.h"
#include "Interfaces/IPluginManager.h"
#include "Misc/Paths.h"

// ISO C++ loader from OpenX/dotnet/Engines/Unreal/CliBridge/unreal_load.cpp.
// One include. Do not also add that file to this module.
THIRD_PARTY_INCLUDES_START
#include "unreal_load.cpp"
THIRD_PARTY_INCLUDES_END

DEFINE_LOG_CATEGORY_STATIC(LogGameXUnrealBridge, Log, All);

void FGameXUnrealBridgeModule::StartupModule()
{
	TArray<FString> Candidates;
	if (const TSharedPtr<IPlugin> Plugin = IPluginManager::Get().FindPlugin(TEXT("GameXUnrealBridge")))
	{
		Candidates.Add(FPaths::Combine(Plugin->GetBaseDir(), TEXT("Binaries/Win64/GameX.Unreal.CliBridge.dll")));
	}
	Candidates.Add(FPaths::Combine(FPaths::ProjectPluginsDir(), TEXT("GameXUnrealBridge/Binaries/Win64/GameX.Unreal.CliBridge.dll")));
	Candidates.Add(FPaths::Combine(FPaths::ProjectDir(), TEXT("Binaries/Win64/GameX.Unreal.CliBridge.dll")));
	Candidates.Add(TEXT("GameX.Unreal.CliBridge.dll"));

	char Buf[64];
	LoadCode = -2;
	Ping.Reset();
	for (const FString& Candidate : Candidates)
	{
		FString Full = FPaths::ConvertRelativePathToFull(Candidate);
		Full.ReplaceInline(TEXT("/"), TEXT("\\"));
		const int Code = GameX_Unreal_LoadBridge(*Full, Buf, (int)sizeof Buf);
		if (Code >= 0)
		{
			LoadCode = Code;
			Ping = UTF8_TO_TCHAR(Buf);
			UE_LOG(LogGameXUnrealBridge, Display, TEXT("Loaded %s from %s"), *Ping, *Full);
			return;
		}
		UE_LOG(LogGameXUnrealBridge, Warning, TEXT("Bridge %s failed (%d)"), *Full, Code);
		LoadCode = Code;
	}
}

void FGameXUnrealBridgeModule::ShutdownModule()
{
}

IMPLEMENT_MODULE(FGameXUnrealBridgeModule, GameXUnrealBridge)
