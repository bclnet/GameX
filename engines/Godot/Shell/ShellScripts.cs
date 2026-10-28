using System.Runtime.CompilerServices;

// Godot registers scripts in the game assembly only. This loads the engine class.
static class ShellScripts {
#pragma warning disable CA2255
	[ModuleInitializer]
	internal static void Init() {
		Godot.Bridge.ScriptManagerBridge.LookupScriptsInAssembly(typeof(OpenX.ViewInfoX).Assembly);
		GameX.Engines.Godot.ShellBind.Init();
	}
}
