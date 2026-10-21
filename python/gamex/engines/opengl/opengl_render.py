from __future__ import annotations
import os
from openx.core import ISource, IHaveSource
from openx.engines.opengl.gfx import OpenGLX, TestTriRenderer, TextureRenderer, ObjectRenderer, EngineRenderer
from gamex.engines.opengl.openglFtlObjectBuilder import OpenGLFtlObjectBuilder
from gamex.engines.opengl.openglFtsObjectBuilder import OpenGLFtsObjectBuilder
from gamex.engines.opengl.openglNifObjectBuilder import OpenGLNifObjectBuilder
from gamex.families.Arkane.formats.danae.binary import Binary_Ftl, Binary_Fts
from gamex.families.Gamebryo.formats.binary import Binary_Nif

OpenGLX.buildersByType['Binary_Ftl'] = OpenGLFtlObjectBuilder.buildObject
OpenGLX.buildersByType[Binary_Ftl] = OpenGLFtlObjectBuilder.buildObject
OpenGLX.buildersByType['Binary_Fts'] = OpenGLFtsObjectBuilder.buildObject
OpenGLX.buildersByType[Binary_Fts] = OpenGLFtsObjectBuilder.buildObject
OpenGLX.buildersByType['Binary_Nif'] = OpenGLNifObjectBuilder.buildObject
OpenGLX.buildersByType[Binary_Nif] = OpenGLNifObjectBuilder.buildObject

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
