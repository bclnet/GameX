using GameX.Origin.Clients.UO;
using GameX.Origin.Formats.UO;
using GameX.Origin.Netcode.UO;
using OpenX.Client;
using System;
using System.IO;
using System.Reflection;

namespace GameX.Origin.UO;

/// <summary>
/// Builds the login scene for UOGameClient.LoadContent. Not a host and not a second client.
/// File path is UOGame.Found.Root, not ClassicUO settings.json.
/// </summary>
public static class UOLogin {
    public static SceneBase Create(UOGameClient client) {
        var game = client.Archive.Game as UOGame;
        var root = game?.Found?.Root;
        var versionText = game?.Options != null && game.Options.TryGetValue("version", out var z) ? z as string : null;
        if (string.IsNullOrWhiteSpace(versionText) && !string.IsNullOrEmpty(root))
            versionText = ClientVersionHelper.ParseFromFile(Path.Combine(root, "client.exe"));
        versionText = string.IsNullOrWhiteSpace(versionText) ? null : versionText.Trim();
        UltimaOnline.Current = new UltimaOnline {
            Assets = client.Assets,
            ClientPath = root,
            VersionText = versionText,
            Version = ClientVersionHelper.ValidateClientVersion(versionText) ?? default,
        };
        var assets = client.Assets;
        UOLoginNet.StepHeight = (x, y, z) => assets != null ? assets.StepZ(x, y, z) : z;
        UOLoginNet.FacetSwitch = map => assets?.UseFacet(map);
        TryAttachMusic();
        return new LoginScene(new World());
    }

    // Origin.UO cannot reference GameX.Origin.Mg because that project is net10.0. Music lives there because this project has no MediaPlayer.
    static void TryAttachMusic() {
        try {
            Assembly asm = null;
            foreach (var loaded in AppDomain.CurrentDomain.GetAssemblies())
                if (loaded.GetName().Name == "GameX.Origin.Mg") { asm = loaded; break; }
            if (asm == null) {
                var file = Path.Combine(AppContext.BaseDirectory, "GameX.Origin.Mg.dll");
                if (!File.Exists(file)) return;
                asm = Assembly.LoadFrom(file);
            }
            asm.GetType("GameX.Origin.Mg.UO.UoMusic")?.GetMethod("Attach")?.Invoke(null, null);
        }
        catch { }
    }
}
