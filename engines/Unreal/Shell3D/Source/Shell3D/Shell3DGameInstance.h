#pragma once

#include "CoreMinimal.h"
#include "Engine/GameInstance.h"
#include "Shell3DGameInstance.generated.h"

// Shows the C++/CLI bridge ping. A map cannot replace the game instance
// the way it can replace the game mode, so this runs on the engine template map.
UCLASS()
class UShell3DGameInstance : public UGameInstance
{
	GENERATED_BODY()

	public:
		virtual void OnStart() override;
};
