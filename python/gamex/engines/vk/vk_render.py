from __future__ import annotations
from openx.core import ISource, IHaveSource
from openx.engines.vk.gfx.vk_render import TestTriRenderer, TextureRenderer

# typedefs
class IOpenGfx: pass
class Renderer: pass

# VkRenderer
class VkRenderer:
    @staticmethod
    def createRenderer(parent: object, gfx: list[IOpenGfx], source: ISource, obj: object, type: str) -> Renderer:
        if isinstance(obj, IHaveSource): source = obj.source
        match type:
            case 'TestTri': return TestTriRenderer(gfx, source, obj)
            case 'Texture': return TextureRenderer(gfx, source, obj, slice(0, None))
            # case 'Object': return ObjectRenderer(gfx, source, obj)
            # case 'Cell': return CellRenderer(gfx, source, obj)
            # case 'Engine': return EngineRenderer(gfx, source, obj)
            case _: return None
