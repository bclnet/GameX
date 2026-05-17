from __future__ import annotations
import os
from openstk.core import ISource, IHaveSource
from openstk.platforms.pygame.gfx import TestTriRenderer, TextureRenderer, TestAnimRenderer 

# typedefs
class IOpenGfx: pass
class Renderer: pass

# PygameRenderer
class PygameRenderer:
    @staticmethod
    def createRenderer(parent: object, gfx: list[IOpenGfx], source: ISource, obj: object, type: str) -> Renderer:
        surf = parent.surface
        if isinstance(obj, IHaveSource): source = obj.source
        match type:
            case 'TestTri': return TestTriRenderer(gfx, source, obj, surf)
            case 'Texture' | 'VideoTexture': return TextureRenderer(gfx, source, obj, surf, None)
            case 'TestAnim': return TestAnimRenderer(gfx, source, obj, surf)
            case _: return None
