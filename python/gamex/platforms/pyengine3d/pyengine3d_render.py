from __future__ import annotations
import os
from openstk.gfx import IHaveOpenGfx
from openstk.platforms.pyengine3d.gfx import TestTriRenderer

# typedefs
class IOpenGfx: pass
class Renderer: pass

# PyEngine3dRenderer
class PyEngine3dRenderer:
    @staticmethod
    def createRenderer(parent: object, gfx: list[IOpenGfx], obj: object, type: str) -> Renderer:
        if isinstance(obj, IHaveOpenGfx): gfx = obj.gfx
        surf = parent.surface
        match type:
            case 'TestTri': return TestTriRenderer(gfx[GfX.XModel], obj, surf)
            case _: return None
