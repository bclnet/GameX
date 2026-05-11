from __future__ import annotations
from openstk.gfx import IHaveOpenGfx
from openstk.platforms.panda3d import Panda3dPlatform
from openstk.platforms.panda3d.gfx import TestTriRenderer, TextureRenderer, ObjectRenderer, MaterialRenderer, ParticleRenderer, EngineRenderer, WorldRenderer
from gamex.platforms.panda3d.panda3dnifobjectbuilder import Panda3dNifObjectBuilder

# typedefs
class IOpenGfx: pass
class Renderer: pass

# Panda3dRenderer
Panda3dPlatform.buildersByType['Binary_Nif'] = Panda3dNifObjectBuilder.buildObject
class Panda3dRenderer:
    @staticmethod
    def createRenderer(parent: object, gfx: list[IOpenGfx], obj: object, type: str) -> Renderer:
        if isinstance(obj, IHaveOpenGfx): gfx = obj.gfx
        match type:
            case 'TestTri': return TestTriRenderer(gfx, obj)
            case 'Texture' | 'VideoTexture': return TextureRenderer(gfx, obj)
            case 'Object': return ObjectRenderer(gfx, obj)
            case 'Material': return MaterialRenderer(gfx, obj)
            case 'Particle': return ParticleRenderer(gfx, obj)
            case 'Engine': return EngineRenderer(gfx, obj)
            case 'World': return WorldRenderer(gfx, obj)
            case _: return None
