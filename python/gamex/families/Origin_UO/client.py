# UltimaOnline asset slot. Not the process entry.
# C# UOGameClient.LoadContent is the bootstrap: it loads the install and
# host.setScene(LoginScene). This module does not do that.
# Renderer home later is GameX.Engine.Mg (OpenX engine code MG), not FNA.

# ClassicUO UltimaOnline shell. Resource parsers stay in GameX.Origin.
class UltimaOnline:
    # ClassicUO folders and the GameX home for each. Not a port of those trees.
    Map = [
        "ClassicUO.Assets and ClassicUO.IO -> stay in GameX.Origin formats (already the resource parsers). Do not re-port them here.",
        "ClassicUO.Client Game/ scenes and UI -> GameX.Origin.UO. LoginScene is the first scene. The gumps are not in yet.",
        "ClassicUO.Renderer -> GameX.Engine.Mg, later.",
        "ClassicUO.Utility -> only if a later slice needs a specific helper; not copied now.",
    ]

    def __init__(self):
        # ClassicUO FileManager slot. C# type is GameX.Origin.Clients.UO.Assets.
        self.assets = None
        # ClientVersion. UOGame.Version is internal, so this stays unset.
        self.version = None
        self.clientPath = None
        # Renderer slots, not constructed: Animations, Arts, Gumps, Texmaps, Lights, MultiMaps, Sounds.
        # World: C# login scene holds the entity lists only. GameCursor is not in yet.
        # Protocol: ClientFlags is internal.

    def load(self, archive, ctx):
        # Not a port of LoadUOFiles, and not the bootstrap.
        # C# UOGameClient.LoadContent calls Assets.Load, then MgClientHost.SetScene(LoginScene).
        # Python Assets has no load. ctx is that client. Renderer objects are not constructed here.
        game = archive.game
        found = game.found
        self.clientPath = found.root if found else None
