from gamex import Archive
from gamex.core.client import ClientState, GameClient

# MorrowindGameClient
class MorrowindGameClient(GameClient):
    def __init__(self, state: ClientState): super().__init__(state)
    def loadContent(self) -> None:
        super().loadContent()
        print('HERE')
    def unloadContent(self) -> None: super().unloadContent()
