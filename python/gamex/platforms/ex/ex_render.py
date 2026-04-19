from __future__ import annotations
import os
from openstk.gfx import GfX
from openstk.platforms.ex.gfx import TestTriRenderer

# typedefs
class IOpenGfx: pass
class Renderer: pass

# ExRenderer
class ExRenderer:
    @staticmethod
    def createRenderer(parent: object, gfx: list[IOpenGfx], obj: object, type: str) -> Renderer:
        surf = parent.surface
        match type:
            case 'TestTri': return TestTriRenderer(gfx[GfX.XModel], obj, surf)
            case _: return None
