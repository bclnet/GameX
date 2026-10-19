using UnrealBuildTool;
using System.Collections.Generic;

public class Shell3DEditorTarget : TargetRules
{
	public Shell3DEditorTarget(TargetInfo Target) : base(Target)
	{
		Type = TargetType.Editor;
		DefaultBuildSettings = BuildSettingsVersion.V7;
		IncludeOrderVersion = EngineIncludeOrderVersion.Unreal5_8;
		ExtraModuleNames.Add("Shell3D");
	}
}
