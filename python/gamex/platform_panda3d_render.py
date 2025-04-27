from __future__ import annotations
import os
from openstk.gfx import GfX
from openstk.gfx.panda3d import Panda3dTextureRenderer, Panda3dObjectRenderer, Panda3dMaterialRenderer, Panda3dParticleRenderer, Panda3dCellRenderer, Panda3dWorldRenderer, Panda3dTestTriRenderer

# typedefs
class IOpenGfx: pass
class Renderer: pass

# Panda3dRenderer
class Panda3dRenderer:
    @staticmethod
    def createRenderer(parent: object, gfx: list[IOpenGfx], obj: object, type: str) -> Renderer:
        match type:
            case 'Texture' | 'VideoTexture': return Panda3dTextureRenderer(gfx[GfX.XModel], obj)
            case 'Object': return Panda3dObjectRenderer(gfx[GfX.XModel], obj)
            case 'Material': return Panda3dMaterialRenderer(gfx[GfX.XModel], obj)
            case 'Particle': return Panda3dParticleRenderer(gfx[GfX.XModel], obj)
            case 'Cell': return Panda3dCellRenderer(gfx[GfX.XModel], obj)
            case 'World': return Panda3dWorldRenderer(gfx[GfX.XModel], obj)
            case 'TestTri': return Panda3dTestTriRenderer(gfx[GfX.XModel], obj)
            case _: return None
