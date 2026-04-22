from __future__ import annotations
import os
from openstk.platforms.panda3d.gfx import TestTriRenderer, TextureRenderer, ObjectRenderer, MaterialRenderer, ParticleRenderer, CellRenderer, EngineRenderer, WorldRenderer

# typedefs
class IOpenGfx: pass
class Renderer: pass

# Panda3dRenderer
class Panda3dRenderer:
    @staticmethod
    def createRenderer(parent: object, gfx: list[IOpenGfx], obj: object, type: str) -> Renderer:
        match type:
            case 'TestTri': return TestTriRenderer(gfx, obj)
            case 'Texture' | 'VideoTexture': return TextureRenderer(gfx, obj)
            case 'Object': return ObjectRenderer(gfx, obj)
            case 'Material': return MaterialRenderer(gfx, obj)
            case 'Particle': return ParticleRenderer(gfx, obj)
            case 'Cell': return CellRenderer(gfx, obj)
            case 'Engine': return EngineRenderer(gfx, obj)
            case 'World': return WorldRenderer(gfx, obj)
            case _: return None
