using GameX.Origin.Clients.UO;
using GameX.Origin.Formats.UO;
using System.Threading.Tasks;

namespace GameX.Origin.UO;

/// <summary>
/// Ultima Online asset slot. ClassicUO's UltimaOnline, without loading mul/uop
/// files itself and without constructing FNA Texture2D objects.
/// Not the process entry. UOGameClient.LoadContent is the bootstrap.
/// </summary>
public class UltimaOnline {
    /// <summary>
    /// The slot filled when UOGameClient boots. Not a second client.
    /// </summary>
    public static UltimaOnline Current { get; internal set; }

    /// <summary>
    /// Asset loaders. ClassicUO's FileManager slot. Not a second mul reader.
    /// </summary>
    public Assets Assets { get; set; }

    // ClassicUO renderer slots. Not constructed here (no Texture2D):
    // Animations, Arts, Gumps, Texmaps, Lights, MultiMaps, Sounds.
    // World in this project is only the login scene's entity lists. GameCursor is not in yet.
    // Protocol is ClassicUO ClientFlags. GameX's ClientFlags is internal.

    /// <summary>
    /// Client version. UOGame.Version is internal, so this stays unset.
    /// Assets.Load reads the version inside Origin.
    /// </summary>
    public ClientVersion Version { get; set; }

    /// <summary>
    /// Install directory. Set from FamilyGame.Found.Root, which is public.
    /// </summary>
    public string ClientPath { get; set; }

    /// <summary>
    /// ClassicUO folders and the GameX home for each. Not a port of those trees.
    /// </summary>
    public static readonly string[] Map = [
        "ClassicUO.Assets and ClassicUO.IO -> stay in GameX.Origin formats (already the resource parsers). Do not re-port them here.",
        "ClassicUO.Client Game/ scenes and UI -> GameX.Origin.UO. LoginScene is the first scene. The gumps are not in yet.",
        "ClassicUO.Renderer -> GameX.Engine.Mg, later.",
        "ClassicUO.Utility -> only if a later slice needs a specific helper; not copied now.",
    ];

    /// <summary>
    /// Not a port of LoadUOFiles, and not the bootstrap. UOGameClient.LoadContent already calls Assets.Load
    /// and then MgClientHost.SetScene. Call this only when a caller wants the slot filled without that boot.
    /// Renderer objects (hue sampler 512x1024, lights sampler 32x63) are still not constructed here.
    /// </summary>
    public async Task Load(Archive archive, UOGameClient ctx) {
        var game = (UOGame)archive.Game;
        ClientPath = game.Found?.Root;
        var assets = new Assets(game);
        Assets = assets;
        await assets.Load(archive, ctx);
    }
}
