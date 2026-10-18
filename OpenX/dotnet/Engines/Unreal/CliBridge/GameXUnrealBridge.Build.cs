using UnrealBuildTool;

// Win64 module. ISO C++ only: do not pass /clr or /clr:netcore.
// GameX.Unreal.CliBridge.dll is built by MSBuild, not UnrealBuildTool.
public class GameXUnrealBridge : ModuleRules {
	public GameXUnrealBridge(ReadOnlyTargetRules Target) : base(Target) {
		PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;

		PublicDependencyModuleNames.AddRange(new string[] {
			"Core",
			"CoreUObject",
			"Engine"
		});

		if (Target.Platform != UnrealTargetPlatform.Win64)
			throw new System.Exception("GameXUnrealBridge is Windows-only.");
	}
}
