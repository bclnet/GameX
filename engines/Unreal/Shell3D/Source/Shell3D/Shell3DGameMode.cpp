#include "Shell3DGameMode.h"
#include "GameFramework/DefaultPawn.h"

AShell3DGameMode::AShell3DGameMode()
{
	DefaultPawnClass = ADefaultPawn::StaticClass();
}
