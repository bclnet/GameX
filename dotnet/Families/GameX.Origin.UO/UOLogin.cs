using GameX.Origin.Clients.UO;
using GameX.Origin.Formats.UO;
using OpenX.Client;
using System.IO;

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
        return new LoginScene(new World());
    }
}
