from __future__ import annotations
import os
from openstk.core import ISource, IHaveSource
from openstk.platforms.opengl.gfx import TestTriRenderer, TextureRenderer, ObjectRenderer, EngineRenderer

# typedefs
class IOpenGfx: pass
class Renderer: pass

# OpenGLRenderer
class OpenGLRenderer:
    @staticmethod
    def createRenderer(parent: object, gfx: list[IOpenGfx], source: ISource, obj: object, type: str) -> Renderer:
        if isinstance(obj, IHaveSource): source = obj.source
        match type:
            case 'TestTri': return TestTriRenderer(gfx, source, obj)
            case 'Texture' | 'VideoTexture': return TextureRenderer(gfx, source, obj, None, False)
            case 'Object': return ObjectRenderer(gfx, source, obj)
            # case 'Material': return MaterialRenderer(gfx, source, obj)
            # case 'Particle': return ParticleRenderer(gfx, source, obj)
            case 'Engine': return EngineRenderer(gfx, source, obj)
            # case 'World': return WorldRenderer(gfx, source, obj)
            case _: return None
