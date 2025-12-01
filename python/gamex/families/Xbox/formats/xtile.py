from __future__ import annotations
import os
from openstk.core.typex import *
from gamex.families.Xbox.formats.xna import TypeReader

class Map:
    pass

@RType('xTile.Pipeline.TideReader')
@RAssembly('xTile')
class TideReader(TypeReader[Map]):
    def __init__(self, t: type): super().__init__(Map)
    def read(self, r: ContentReader, o: Map) -> Map:
        data = r.readL32Bytes()
        return Map()