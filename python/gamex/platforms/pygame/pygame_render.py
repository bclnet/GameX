from __future__ import annotations
import os
from openstk.gfx import GfX
from openstk.platforms.pygame.gfx import TestTriRenderer, TestAnimRenderer 

# typedefs
class IOpenGfx: pass
class Renderer: pass

# PygameRenderer
class PygameRenderer:
    @staticmethod
    def createRenderer(parent: object, gfx: list[IOpenGfx], obj: object, type: str) -> Renderer:
        surf = parent.surface
        match type:
            case 'TestTri': return TestTriRenderer(gfx[GfX.XModel], obj, surf)
            case 'TestAnim': return TestAnimRenderer(gfx[GfX.XModel], obj, surf)
            case _: return None
