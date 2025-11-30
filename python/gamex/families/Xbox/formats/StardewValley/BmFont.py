from __future__ import annotations
import os
from openstk.core.typex import *
from gamex.families.Xbox.formats.xna import TypeReader

@RType('BmFont.XmlSourceReader')
@RAssembly('BmFont')
class XmlSourceReader(TypeReader[str]):
    def __init__(self, t: type): super().__init__(t)
    def read(self, r: ContentReader, o: str) -> str: return r.readString()