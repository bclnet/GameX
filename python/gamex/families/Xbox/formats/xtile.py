from __future__ import annotations
import os
from openstk.core.typex import *
from gamex.families.Xbox.formats.xna import TypeReader

class Map:
    pass

@RType('xTile.Pipeline.TideReader')
@RAssembly('xTile')
class TideReader(TypeReader[Map]):
    def __init__(self): super().__init__()
    def read(self, r: ContentReader, o: Map) -> Map:
        data = r.readL32Bytes()
        return None