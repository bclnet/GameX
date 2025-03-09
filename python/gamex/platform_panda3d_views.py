from __future__ import annotations
import os
from openstk.gfx.gfx_texture import ITexture, ITextureFrames

# typedefs
class IPanda3dGfx: pass

# ViewBase
class ViewBase:
    def __init__(self, base: object, gfx: IPanda3dGfx, obj: object):
        self.base = base
        self.gfx = gfx
        self.obj = obj
    def start(self) -> None: pass
    def update(self) -> None: pass

# ViewCell
class ViewCell(ViewBase):
    def __init__(self, base: object, gfx: IPanda3dGfx, obj: object):
        super().__init__(base, gfx, obj)

# ViewParticle
class ViewParticle(ViewBase):
    def __init__(self, base: object, gfx: IPanda3dGfx, obj: object):
        super().__init__(base, gfx, obj)

# ViewEngine
class ViewEngine(ViewBase):
    def __init__(self, base: object, gfx: IPanda3dGfx, obj: object):
        super().__init__(base, gfx, obj)

# ViewObject
class ViewObject(ViewBase):
    def __init__(self, base: object, gfx: IPanda3dGfx, obj: object):
        super().__init__(base, gfx, obj)

# ViewMaterial
class ViewMaterial(ViewBase):
    def __init__(self, base: object, gfx: IPanda3dGfx, obj: object):
        super().__init__(base, gfx, obj)

# ViewTexture
class ViewTexture(ViewBase):
    def __init__(self, base: object, gfx: IPanda3dGfx, obj: object):
        super().__init__(base, gfx, obj)

# ViewTexture
class ViewTexture(ViewBase):
    def __init__(self, base: object, gfx: IPanda3dGfx, obj: object):
        super().__init__(base, gfx, obj)
    def start(self):
        self.scene = self.base.loader.loadModel('models/environment')
        self.scene.reparentTo(self.base.render)
        self.scene.setScale(0.25, 0.25, 0.25)
        self.scene.setPos(-8, 42, 0)

# ViewVideoTexture
class ViewVideoTexture(ViewBase):
    frameDelay: int = 0
    def __init__(self, gfx: IPanda3dGfx, obj: object):
        super().__init__(gfx, obj)

# ViewTestTri
class ViewTestTri(ViewBase):
    def __init__(self, gfx: IPanda3dGfx, obj: object):
        super().__init__(gfx, obj)
    def start(self):
        self.scene = self.base.loader.loadModel('models/environment')
        self.scene.reparentTo(self.base.render)
        self.scene.setScale(0.25, 0.25, 0.25)
        self.scene.setPos(-8, 42, 0)
        # self.scene = self.loader.loadModel('teapot')
        # self.scene.reparentTo(self.render)

@staticmethod
def createView(parent: object, gfx: IPanda3dGfx, obj: object, type: str) -> ViewBase:
    base = parent
    match type:
        case 'Material': return ViewMaterial(base, gfx, obj)
        case 'Particle': return ViewParticle(base, gfx, obj)
        case 'TestTri': return ViewTestTri(base, gfx, obj)
        case 'Texture': return ViewTexture(base, gfx, obj)
        case 'VideoTexture': return ViewVideoTexture(base, gfx, obj)
        case 'Object': return ViewObject(base, gfx, obj)
        case 'Cell': return ViewCell(base, gfx, obj)
        case 'World': return None
        case 'Engine': return ViewEngine(base, gfx, obj)
        case _: return None