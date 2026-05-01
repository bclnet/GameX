from __future__ import annotations
import os
from openstk.gfx import GfX
from openstk.platforms.pygame.gfx import TestTriRenderer, TextureRenderer, TestAnimRenderer 

# typedefs
class IOpenGfx: pass
class Renderer: pass

# PygameRenderer
class PygameRenderer:
    @staticmethod
    def createRenderer(parent: object, gfx: list[IOpenGfx], obj: object, type: str) -> Renderer:
        surf = parent.surface
        match type:
            case 'TestTri': return TestTriRenderer(gfx, obj, surf)
            case 'Texture' | 'VideoTexture': return TextureRenderer(gfx, obj, surf, None)
            case 'TestAnim': return TestAnimRenderer(gfx, obj, surf)
            case _: return None
