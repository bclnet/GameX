from __future__ import annotations
import os
from openstk.gfx import GfX
from openstk.platforms.opengl.gfx import TestTriRenderer, TextureRenderer, ObjectRenderer, MaterialRenderer, ParticleRenderer, EngineRenderer, WorldRenderer

# typedefs
class IOpenGfx: pass
class Renderer: pass

# OpenGLRenderer
class OpenGLRenderer:
    @staticmethod
    def createRenderer(parent: object, gfx: list[IOpenGfx], obj: object, type: str) -> Renderer:
        match type:
            case 'TestTri': return TestTriRenderer(gfx, obj)
            case 'Texture' | 'VideoTexture': return TextureRenderer(gfx, obj, None, False)
            case 'Object': return ObjectRenderer(gfx, obj)
            case 'Material': return MaterialRenderer(gfx, obj)
            case 'Particle': return ParticleRenderer(gfx, obj)
            case 'Engine': return EngineRenderer(gfx, obj)
            case 'World': return WorldRenderer(gfx, obj)
            case _: return None
