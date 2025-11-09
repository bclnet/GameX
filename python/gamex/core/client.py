from __future__ import annotations
import os
from gamex import PakFile
from gamex.core.eng.eng import Game

# IPluginHost
class IPluginHost: pass

# IPluginHost
class GameController(Game):
    def __init__(self, game: PakFile, pluginHost: IPluginHost):
        super().__init__()
        self.game = game
        self.pluginHost = pluginHost
    