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
        # C# UOGameClient.LoadContent loads Assets from UOGame.Found.Root, then
        # host.setScene(LoginScene) in GameX.Origin.UO. This is not that bootstrap.
        Fonts.load(self.game, self.device)

    def unloadContent(self) -> None: pass
