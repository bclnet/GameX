from __future__ import annotations
import os
from openstk.gfx import GfX
from openstk.platforms.Ex.gfx import ExTestAnimRenderer, ExTestTriRenderer

# typedefs
class IOpenGfx: pass
class Renderer: pass

# ExRenderer
class ExRenderer:
    @staticmethod
    def createRenderer(parent: object, gfx: list[IOpenGfx], obj: object, type: str) -> Renderer:
        surf = parent.surface
        match type:
            case 'TestAnim' | 'TestTri': return ExTestAnimRenderer(gfx[GfX.XModel], obj, surf)
            # case 'TestTri': return ExTestTriRenderer(gfx[GfX.XModel], obj, surf)
            case _: return None
