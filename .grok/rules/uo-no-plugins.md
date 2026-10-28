# Do not port ClassicUO's plugin system

ClassicUO's assistant host is not part of the GameX client. Do not port it, do not implement it, and do not extend the empty stubs that already exist.

This covers `src/ClassicUO.Client/PluginHost.cs` (`IPluginHost`, `HostBindings`, `ClientBindings`, `UnmanagedAssistantHost`), `src/ClassicUO.Client/Network/Plugin.cs`, and `src/ClassicUO.Bootstrap` `Plugin.cs`. Do not add Razor, ClassicAssist, UOS, or any other assistant DLL. Do not add a plugin directory, a plugin path in settings, or packet, hotkey, mouse, or SDL hooks whose only job is to call a plugin.

`OpenX.Client.Plugin` and `IPluginHost` in `OpenX/dotnet/Core/OpenX/Client.cs` are empty stubs. `MgClientHost` calls them. Leave those calls as no-ops. Do not fill them in.

`engines/Plugins` is GameX's engine-player assemblies (Unity, Unreal). That is not ClassicUO's plugin system. Leave it alone.
