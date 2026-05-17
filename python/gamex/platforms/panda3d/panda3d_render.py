from __future__ import annotations
from openstk.core import ISource, IHaveSource
from openstk.platforms.panda3d import Panda3dPlatform
from openstk.platforms.panda3d.gfx import TestTriRenderer, TextureRenderer, ObjectRenderer, EngineRenderer
from gamex.platforms.panda3d.panda3dnifobjectbuilder import Panda3dNifObjectBuilder

# typedefs
class IOpenGfx: pass
class Renderer: pass

# Panda3dRenderer
Panda3dPlatform.buildersByType['Binary_Nif'] = Panda3dNifObjectBuilder.buildObject
class Panda3dRenderer:
    @staticmethod
    def createRenderer(parent: object, gfx: list[IOpenGfx], source: ISource, obj: object, type: str) -> Renderer:
        if isinstance(obj, IHaveSource): source = obj.source
        match type:
            case 'TestTri': return TestTriRenderer(gfx, source, obj)
            case 'Texture' | 'VideoTexture': return TextureRenderer(gfx, source, obj)
            case 'Object': return ObjectRenderer(gfx, source, obj)
            # case 'Material': return MaterialRenderer(gfx, source, obj)
            # case 'Particle': return ParticleRenderer(gfx, source, obj)
            case 'Engine': return EngineRenderer(gfx, source, obj)
            # case 'World': return WorldRenderer(gfx, source, obj)
            case _: return None
