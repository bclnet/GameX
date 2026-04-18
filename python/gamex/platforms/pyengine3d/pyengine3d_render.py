from __future__ import annotations
import os
from openstk.gfx import GfX
from openstk.platforms.pyengine3d.gfx import PyEngine3dTestAnimRenderer, PyEngine3dTestTriRenderer

# typedefs
class IOpenGfx: pass
class Renderer: pass

# PyEngine3dRenderer
class PyEngine3dRenderer:
    @staticmethod
    def createRenderer(parent: object, gfx: list[IOpenGfx], obj: object, type: str) -> Renderer:
        surf = parent.surface
        match type:
            case 'TestAnim' | 'TestTri': return PyEngine3dTestAnimRenderer(gfx[GfX.XModel], obj, surf)
            # case 'TestTri': return PyEngine3dTestTriRenderer(gfx[GfX.XModel], obj, surf)
            case _: return None
