from __future__ import annotations
import os, asyncio
from openx.core import ISource, IHaveSource
from openx.engines.pyengine3d.gfx import PyEngine3dX, TestTriRenderer
from gamex.families.Arkane.formats.danae.binary import Binary_Ftl, Binary_Fts
from gamex.families.Gamebryo.formats.binary import Binary_Nif
from gamex.engines.pyengine3d.pyengine3dftlobjectbuilder import PyEngine3dFtlObjectBuilder
from gamex.engines.pyengine3d.pyengine3dftsobjectbuilder import PyEngine3dFtsObjectBuilder
from gamex.engines.pyengine3d.pyengine3dnifobjectbuilder import PyEngine3dNifObjectBuilder

PyEngine3dX.buildersByType['Binary_Nif'] = PyEngine3dNifObjectBuilder.buildObject
PyEngine3dX.buildersByType[Binary_Nif] = PyEngine3dNifObjectBuilder.buildObject
PyEngine3dX.buildersByType['Binary_Ftl'] = PyEngine3dFtlObjectBuilder.buildObject
PyEngine3dX.buildersByType[Binary_Ftl] = PyEngine3dFtlObjectBuilder.buildObject
PyEngine3dX.buildersByType['Binary_Fts'] = PyEngine3dFtsObjectBuilder.buildObject
PyEngine3dX.buildersByType[Binary_Fts] = PyEngine3dFtsObjectBuilder.buildObject

# typedefs
class IOpenGfx: pass
class Renderer: pass

# PyEngine3dRenderer
class PyEngine3dRenderer:
    @staticmethod
    def createRenderer(parent: object, gfx: list[IOpenGfx], source: ISource, obj: object, type: str) -> Renderer:
        # surf = parent.surface
        if isinstance(obj, IHaveSource): source = obj.source
        match type:
            case 'TestTri': return TestTriRenderer(gfx, source, obj, surf)
            case 'Object': return ObjectRenderer(gfx, source, obj)
            case _: return None

# ObjectRenderer
class ObjectRenderer(Renderer):
    def __init__(self, gfx: list[IOpenGfx], source: ISource, obj: object):
        self.gfx = gfx
        self.source = source
        self.obj = obj
        self.result = None

    def start(self) -> None:
        builder = _builderFor(self.obj)
        if builder is None: return
        materialManager = None
        model = self.gfx[3] if self.gfx is not None and len(self.gfx) > 3 else None
        if model is not None: materialManager = getattr(model, 'materialManager', None)
        self.result = asyncio.run(builder(self.source, self.obj, True, materialManager))

    def update(self, deltaTime: float) -> None: pass

def _builderFor(obj: object):
    if obj is None: return None
    builder = PyEngine3dX.buildersByType.get(obj.__class__.__name__)
    if builder is None: builder = PyEngine3dX.buildersByType.get(type(obj))
    return builder
