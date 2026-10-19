using UnrealBuildTool;
using System.IO;

// Win64 module. ISO C++ only: do not pass /clr or /clr:netcore.
// GameX.Unreal.CliBridge.dll is built by MSBuild, not UnrealBuildTool.
public class GameXUnrealBridge : ModuleRules
{
	public GameXUnrealBridge(ReadOnlyTargetRules Target) : base(Target)
	{
		PCHUsage = PCHUsageMode.UseExplicitOrSharedPCHs;

		PublicDependencyModuleNames.AddRange(new string[]
		{
			"Core",
			"CoreUObject",
			"Engine"
		});
		PrivateDependencyModuleNames.Add("Projects");

		if (Target.Platform != UnrealTargetPlatform.Win64)
			throw new System.Exception("GameXUnrealBridge is Windows-only.");

		// engines/Unreal/Shell3D/Plugins/GameXUnrealBridge/Source/GameXUnrealBridge
		// seven levels up is the repo root.
		string BridgeDir = Path.GetFullPath(Path.Combine(ModuleDirectory, "..", "..", "..", "..", "..", "..", "..", "OpenX", "dotnet", "Engines", "Unreal", "CliBridge"));
		PrivateIncludePaths.Add(BridgeDir);

		string Win64 = Path.Combine(PluginDirectory, "Binaries", "Win64");
		string[] Staged =
		{
			"GameX.Unreal.CliBridge.dll",
			"ijwhost.dll",
			"GameX.Unreal.CliBridge.runtimeconfig.json"
		};
		foreach (string Name in Staged)
		{
			string PathOnDisk = Path.Combine(Win64, Name);
			if (System.IO.File.Exists(PathOnDisk))
				RuntimeDependencies.Add(PathOnDisk);
		}
	}
}
