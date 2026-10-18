from __future__ import annotations
from openx.core import ISource, IHaveSource
from openx.engines.diligent.gfx.diligent_render import TestTriRenderer, TextureRenderer
from openx.engines.diligent.diligent_shim import DiligentShim

class IOpenGfx: pass
class Renderer: pass

# DiligentRenderer
class DiligentRenderer:
    @staticmethod
    def diligentLinked() -> bool: return DiligentShim.linked()
    @staticmethod
    def diligentVersion() -> str: return DiligentShim.version()
    @staticmethod
    def createRenderer(parent: object, gfx: list[IOpenGfx], source: ISource, obj: object, type: str) -> Renderer:
        if isinstance(obj, IHaveSource): source = obj.source
        match type:
            case 'TestTri': return TestTriRenderer(gfx, source, obj)
            case 'Texture': return TextureRenderer(gfx, source, obj, slice(0, None))
            case _: return None
