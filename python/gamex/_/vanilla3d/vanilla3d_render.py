from __future__ import annotations
import os
from openx.gfx import GfX
from openx.engines.vanilla3d.gfx import TestTriRenderer

# typedefs
class IOpenGfx: pass
class Renderer: pass

# Vanilla3dRenderer
class Vanilla3dRenderer:
    @staticmethod
    def createRenderer(parent: object, gfx: list[IOpenGfx], obj: object, type: str) -> Renderer:
        surf = parent.surface
        match type:
            case 'TestTri': return TestTriRenderer(gfx[GfX.XModel], obj, surf)
            case _: return None
