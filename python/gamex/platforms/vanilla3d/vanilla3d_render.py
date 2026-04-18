from __future__ import annotations
import os
from openstk.gfx import GfX
from openstk.platforms.Vanilla3d.gfx import Vanilla3dTestAnimRenderer, Vanilla3dTestTriRenderer

# typedefs
class IOpenGfx: pass
class Renderer: pass

# Vanilla3dRenderer
class Vanilla3dRenderer:
    @staticmethod
    def createRenderer(parent: object, gfx: list[IOpenGfx], obj: object, type: str) -> Renderer:
        surf = parent.surface
        match type:
            case 'TestAnim' | 'TestTri': return Vanilla3dTestAnimRenderer(gfx[GfX.XModel], obj, surf)
            # case 'TestTri': return Vanilla3dTestTriRenderer(gfx[GfX.XModel], obj, surf)
            case _: return None
