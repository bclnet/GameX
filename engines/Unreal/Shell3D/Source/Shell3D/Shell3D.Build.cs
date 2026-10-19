using UnrealBuildTool;

public class Shell3D : ModuleRules
{
	public Shell3D(ReadOnlyTargetRules Target) : base(Target)
	{
		PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;

		PublicDependencyModuleNames.AddRange(new string[]
		{
			"Core",
			"CoreUObject",
			"Engine",
			"InputCore"
		});

		// Plugin module. Win64 only. See Plugins/GameXUnrealBridge.
		PrivateDependencyModuleNames.Add("GameXUnrealBridge");
	}
}
