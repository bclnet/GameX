#include "Shell3DGameInstance.h"
#include "Engine/Engine.h"
#include "GameXUnrealBridge.h"

void UShell3DGameInstance::OnStart()
{
	Super::OnStart();

	FGameXUnrealBridgeModule& Bridge = FGameXUnrealBridgeModule::Get();
	const bool bOk = Bridge.GetLoadCode() >= 0 && !Bridge.GetPing().IsEmpty();
	const FString Line = bOk
		? Bridge.GetPing()
		: FString::Printf(TEXT("GameX bridge load failed (%d)"), Bridge.GetLoadCode());

	UE_LOG(LogTemp, Display, TEXT("%s"), *Line);
	if (GEngine)
	{
		GEngine->AddOnScreenDebugMessage(-1, 12.f, bOk ? FColor::Green : FColor::Red, Line);
	}
}
