using UnrealBuildTool;
using System.Collections.Generic;

public class Shell3DTarget : TargetRules
{
	public Shell3DTarget(TargetInfo Target) : base(Target)
	{
		Type = TargetType.Game;
		DefaultBuildSettings = BuildSettingsVersion.V7;
		IncludeOrderVersion = EngineIncludeOrderVersion.Unreal5_8;
		ExtraModuleNames.Add("Shell3D");
	}
}
