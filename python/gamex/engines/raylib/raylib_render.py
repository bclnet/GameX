from __future__ import annotations
from openx.core import ISource, IHaveSource
from openx.engines.raylib.gfx.raylib_render import TestTriRenderer, TextureRenderer

class IOpenGfx: pass
class Renderer: pass

# RaylibRenderer
class RaylibRenderer:
    @staticmethod
    def createRenderer(parent: object, gfx: list[IOpenGfx], source: ISource, obj: object, type: str) -> Renderer:
        if isinstance(obj, IHaveSource): source = obj.source
        match type:
            case 'TestTri': return TestTriRenderer(gfx, source, obj)
            case 'Texture': return TextureRenderer(gfx, source, obj, slice(0, None))
            case _: return None
