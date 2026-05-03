from __future__ import annotations
import os
from openstk.gfx import IHaveOpenGfx
from openstk.platforms.ex.gfx import TestTriRenderer

# typedefs
class IOpenGfx: pass
class Renderer: pass

# ExRenderer
class ExRenderer:
    @staticmethod
    def createRenderer(parent: object, gfx: list[IOpenGfx], obj: object, type: str) -> Renderer:
        if isinstance(obj, IHaveOpenGfx): gfx = obj.gfx
        surf = parent.surface
        match type:
            case 'TestTri': return TestTriRenderer(gfx, obj, surf)
            case _: return None
