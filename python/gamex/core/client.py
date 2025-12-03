from __future__ import annotations
import os
from gamex import Archive
from gamex.core.eng.eng import Game

# IPluginHost
class IPluginHost: pass

# IPluginHost
class GameController(Game):
    def __init__(self, game: Archive, pluginHost: IPluginHost):
        super().__init__()
        self.game = game
        self.pluginHost = pluginHost
    