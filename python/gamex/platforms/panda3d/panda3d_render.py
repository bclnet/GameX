from __future__ import annotations
import os
from openstk.gfx import GfX
from openstk.platforms.panda3d.gfx import TestTriRenderer, TextureRenderer, ObjectRenderer, MaterialRenderer, ParticleRenderer, CellRenderer, EngineRenderer, WorldRenderer

# typedefs
class IOpenGfx: pass
class Renderer: pass

# Panda3dRenderer
class Panda3dRenderer:
    @staticmethod
    def createRenderer(parent: object, gfx: list[IOpenGfx], obj: object, type: str) -> Renderer:
        match type:
            case 'TestTri': return TestTriRenderer(gfx[GfX.XModel], obj)
            case 'Texture' | 'VideoTexture': return TextureRenderer(gfx[GfX.XModel], obj)
            case 'Object': return ObjectRenderer(gfx[GfX.XModel], obj)
            case 'Material': return MaterialRenderer(gfx[GfX.XModel], obj)
            case 'Particle': return ParticleRenderer(gfx[GfX.XModel], obj)
            case 'Cell': return CellRenderer(gfx[GfX.XModel], obj)
            case 'Engine': return EngineRenderer(gfx[GfX.XModel], obj)
            case 'World': return WorldRenderer(gfx[GfX.XModel], obj)
            case _: return None
