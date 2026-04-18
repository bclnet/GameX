from __future__ import annotations
import os
from openstk.gfx import GfX
from openstk.platforms.Tiny3d.gfx import Tiny3dTestAnimRenderer, Tiny3dTestTriRenderer

# typedefs
class IOpenGfx: pass
class Renderer: pass

# Tiny3dRenderer
class Tiny3dRenderer:
    @staticmethod
    def createRenderer(parent: object, gfx: list[IOpenGfx], obj: object, type: str) -> Renderer:
        surf = parent.surface
        match type:
            case 'TestAnim' | 'TestTri': return Tiny3dTestAnimRenderer(gfx[GfX.XModel], obj, surf)
            # case 'TestTri': return Tiny3dTestTriRenderer(gfx[GfX.XModel], obj, surf)
            case _: return None
