from __future__ import annotations
import os
from gamex.core.eng.eng import Game

# IPluginHost
class IPluginHost: pass

# IPluginHost
class GameController(Game):
    def __init__(self, pluginHost: IPluginHost):
        super().__init__()
        self.pluginHost = pluginHost
    