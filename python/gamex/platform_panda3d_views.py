from __future__ import annotations
import os
from openstk.gfx.gfx_texture import ITexture, ITextureFrames

# typedefs
class IPanda3dGfx: pass

# ViewBase
class ViewBase:
    gfx: IPanda3dGfx = None
    obj: object = None
    def __init__(self, gfx: IPanda3dGfx, obj: object):
        self.gfx = gfx
        self.obj = obj
    def start(self) -> None: pass
    def update(self) -> None: pass

# ViewCell
class ViewCell(ViewBase):
    def __init__(self, gfx: IPanda3dGfx, obj: object):
        super().__init__(gfx, obj)

# ViewParticle
class ViewParticle(ViewBase):
    def __init__(self, gfx: IPanda3dGfx, obj: object):
        super().__init__(gfx, obj)

# ViewEngine
class ViewEngine(ViewBase):
    def __init__(self, gfx: IPanda3dGfx, obj: object):
        super().__init__(gfx, obj)

# ViewObject
class ViewObject(ViewBase):
    def __init__(self, gfx: IPanda3dGfx, obj: object):
        super().__init__(gfx, obj)

# ViewMaterial
class ViewMaterial(ViewBase):
    def __init__(self, gfx: IPanda3dGfx, obj: object):
        super().__init__(gfx, obj)

# ViewTexture
class ViewTexture(ViewBase):
    def __init__(self, gfx: IPanda3dGfx, obj: object):
        super().__init__(gfx, obj)

# ViewTexture
class ViewTexture(ViewBase):
    def __init__(self, gfx: IPanda3dGfx, obj: object):
        super().__init__(gfx, obj)

# ViewVideoTexture
class ViewVideoTexture(ViewBase):
    frameDelay: int = 0
    def __init__(self, gfx: IPanda3dGfx, obj: object):
        super().__init__(gfx, obj)

# ViewTestTri
class ViewTestTri(ViewBase):
    def __init__(self, gfx: IPanda3dGfx, obj: object):
        super().__init__(gfx, obj)

@staticmethod
def createView(parent: object, gfx: IPanda3dGfx, obj: object, type: str) -> ViewBase:
    match type:
        case 'Material': return ViewMaterial(gfx, obj)
        case 'Particle': return ViewParticle(gfx, obj)
        case 'TestTri': return ViewTestTri(gfx, obj)
        case 'Texture': return ViewTexture(gfx, obj)
        case 'VideoTexture': return ViewVideoTexture(gfx, obj)
        case 'Object': return ViewObject(gfx, obj)
        case 'Cell': return ViewCell(gfx, obj)
        case 'World': return None
        case 'Engine': return ViewEngine(gfx, obj)
        case _: return None