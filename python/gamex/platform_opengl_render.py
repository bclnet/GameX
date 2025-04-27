from __future__ import annotations
import os
from openstk.gfx import GfX
from openstk.gfx.opengl import OpenGLTextureRenderer, OpenGLObjectRenderer, OpenGLMaterialRenderer, OpenGLParticleRenderer, OpenGLCellRenderer, OpenGLWorldRenderer, OpenGLTestTriRenderer

# typedefs
class IOpenGfx: pass
class Renderer: pass

# OpenGLRenderer
class OpenGLRenderer:
    @staticmethod
    def createRenderer(parent: object, gfx: list[IOpenGfx], obj: object, type: str) -> Renderer:
        match type:
            case 'Texture' | 'VideoTexture': return OpenGLTextureRenderer(gfx[GfX.XModel], obj, None, False)
            case 'Object': return OpenGLObjectRenderer(gfx[GfX.XModel], obj)
            case 'Material': return OpenGLMaterialRenderer(gfx[GfX.XModel], obj)
            case 'Particle': return OpenGLParticleRenderer(gfx[GfX.XModel], obj)
            case 'Cell': return OpenGLCellRenderer(gfx[GfX.XModel], obj)
            case 'World': return OpenGLWorldRenderer(gfx[GfX.XModel], obj)
            case 'TestTri': return OpenGLTestTriRenderer(gfx[GfX.XModel], obj)
            case _: return None
