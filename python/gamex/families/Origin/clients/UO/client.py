from enum import IntFlag
from gamex import Archive
from gamex.core.client import ClientState, GameClient
from gamex.families.Origin.renderers.UO.sprites import Fonts
# from gamex.families.Uncore.formats.network import PacketLogger

# Assets
class Assets:
    pass

# UOGameClient
class UOGameClient(GameClient):
    def __init__(self, state: ClientState): super().__init__(state)
    def loadContent(self) -> None:
        super().loadContent()
        Fonts.load(self.game, self.device)
        # SolidColorTextureCache.Initialize(GraphicsDevice);
        # Audio = new AudioManager();
#if false
        #SetScene(new MainScene(this));
#else
        #UO.Load(this);
        #Audio.Initialize();
        # SetScene(new LoginScene(UO.World));
#endif

    def unloadContent(self) -> None: pass
