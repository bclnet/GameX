from __future__ import annotations
import os
from openstk.core.reflect import *
from gamex.families.Xbox.formats.xna import TypeReader

@RAssembly('BmFont.XmlSourceReader')
@RType('BmFont')
class XmlSourceReader(TypeReader[str]):
    def __init__(self): super().__init__()
    def read(self, r: ContentReader, o: str) -> str: return r.readString()