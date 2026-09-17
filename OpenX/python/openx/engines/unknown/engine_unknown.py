from __future__ import annotations
from openx.core.engine import Engine

#region Engine

# UnknownEngine
class UnknownEngine(Engine):
    # buildersByType: dict[type, callable] = {}
    def __init__(self):
        super().__init__('UK', 'Unknown')
        self.gfxFactory = staticmethod(lambda: [None, None, None, None, None, None])
        self.sfxFactory = staticmethod(lambda: [None])
UnknownEngine.this = UnknownEngine()

#endregion
