from __future__ import annotations
import os
from openstk.core import ISource, IHaveSource
from openstk.platforms.eginx.gfx import TestTriRenderer

# typedefs
class IOpenGfx: pass
class Renderer: pass

# EginXRenderer
class EginXRenderer:
    @staticmethod
    def createRenderer(parent: object, gfx: list[IOpenGfx], source: ISource, obj: object, type: str) -> Renderer:
        surf = parent.surface
        if isinstance(obj, IHaveSource): source = obj.source
        match type:
            case 'TestTri': return TestTriRenderer(gfx, source, obj, surf)
            case _: return None
