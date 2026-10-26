from __future__ import annotations
from openx.core import ISource, IHaveSource
from openx.engines.panda3d.gfx import Panda3dX, TestTriRenderer, TextureRenderer, ObjectRenderer, EngineRenderer
from gamex.engines.panda3d.panda3d_gamebryo import Panda3dNifObjectBuilder
from gamex.engines.panda3d.panda3d_arkane import Panda3dFtlObjectBuilder
from gamex.engines.panda3d.panda3d_arkane import Panda3dFtsObjectBuilder
from gamex.families.Arkane.formats.danae.binary import Binary_Ftl, Binary_Fts
from gamex.families.Gamebryo.formats.binary import Binary_Nif

# typedefs
class IOpenGfx: pass
class Renderer: pass

# Panda3dRenderer
Panda3dX.buildersByType['Binary_Nif'] = Panda3dNifObjectBuilder.buildObject
Panda3dX.buildersByType[Binary_Nif] = Panda3dNifObjectBuilder.buildObject
Panda3dX.buildersByType['Binary_Ftl'] = Panda3dFtlObjectBuilder.buildObject
Panda3dX.buildersByType[Binary_Ftl] = Panda3dFtlObjectBuilder.buildObject
Panda3dX.buildersByType['Binary_Fts'] = Panda3dFtsObjectBuilder.buildObject
Panda3dX.buildersByType[Binary_Fts] = Panda3dFtsObjectBuilder.buildObject

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
