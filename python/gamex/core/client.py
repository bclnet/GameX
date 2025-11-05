from __future__ import annotations
import os

# IPluginHost
class IPluginHost: pass

# Game
class Game:
    def __init__(self, f): pass
    def __enter__(self): return self
    def __exit__(self, type, value, traceback): pass
    def run(self): pass

# IPluginHost
class GameController(Game):
    def __init__(self, pluginHost: IPluginHost): self.pluginHost = pluginHost
    
