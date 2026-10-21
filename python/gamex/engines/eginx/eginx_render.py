from __future__ import annotations
import os
from openx.core import ISource, IHaveSource
from openx.engines.eginx.gfx import EginX, TestTriRenderer
from gamex.families.Arkane.formats.danae.binary import Binary_Ftl, Binary_Fts
from gamex.families.Gamebryo.formats.binary import Binary_Nif
from gamex.engines.eginx.eginxFtlObjectBuilder import EginXFtlObjectBuilder
from gamex.engines.eginx.eginxFtsObjectBuilder import EginXFtsObjectBuilder
from gamex.engines.eginx.eginxNifObjectBuilder import EginXNifObjectBuilder

# typedefs
class IOpenGfx: pass
class Renderer: pass

EginX.buildersByType['Binary_Nif'] = EginXNifObjectBuilder.buildObject
EginX.buildersByType[Binary_Nif] = EginXNifObjectBuilder.buildObject
EginX.buildersByType['Binary_Ftl'] = EginXFtlObjectBuilder.buildObject
EginX.buildersByType[Binary_Ftl] = EginXFtlObjectBuilder.buildObject
EginX.buildersByType['Binary_Fts'] = EginXFtsObjectBuilder.buildObject
EginX.buildersByType[Binary_Fts] = EginXFtsObjectBuilder.buildObject

class ObjectRenderer:
    def __init__(self, gfx: list[IOpenGfx], source: ISource, obj: object):
        self.obj = obj
        self.holder = None

    def start(self):
        obj = self.obj
        if obj is None: return
        build = EginX.buildersByType.get(type(obj))
        if build is None: build = EginX.buildersByType.get(type(obj).__name__)
        if build is None: return
        self.holder = build(obj, True, None)

    def update(self, deltaTime: float): pass

# EginXRenderer
class EginXRenderer:
    @staticmethod
    def createRenderer(parent: object, gfx: list[IOpenGfx], source: ISource, obj: object, type: str) -> Renderer:
        surf = parent.surface
        if isinstance(obj, IHaveSource): source = obj.source
        match type:
            case 'TestTri': return TestTriRenderer(gfx, source, obj, surf)
            case 'Object': return ObjectRenderer(gfx, source, obj)
            case _: return None
