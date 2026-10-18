using GameX.Origin.Clients.UO;
using OpenX.Client;

namespace GameX.Origin.UO;

/// <summary>
/// Builds the login scene for UOGameClient.LoadContent. Not a host and not a second client.
/// File path is UOGame.Found.Root, not ClassicUO settings.json.
/// </summary>
public static class UOLogin {
    public static SceneBase Create(UOGameClient client) {
        var game = client.Archive.Game as UOGame;
        UltimaOnline.Current = new UltimaOnline {
            Assets = client.Assets,
            ClientPath = game?.Found?.Root,
        };
        return new LoginScene(new World());
    }
}
