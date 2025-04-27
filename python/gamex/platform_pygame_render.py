from __future__ import annotations
import os
from openstk.gfx import GfX
from openstk.gfx.pygame import PygameTestAnimRenderer, PygameTestTriRenderer

# typedefs
class IOpenGfx: pass
class Renderer: pass

# PygameRenderer
class PygameRenderer:
    @staticmethod
    def createRenderer(parent: object, gfx: list[IOpenGfx], obj: object, type: str) -> Renderer:
        surf = parent.surface
        match type:
            case 'TestAnim' | 'TestTri': return PygameTestAnimRenderer(gfx[GfX.XModel], obj, surf)
            # case 'TestTri': return PygameTestTriRenderer(gfx[GfX.XModel], obj, surf)
            case _: return None
