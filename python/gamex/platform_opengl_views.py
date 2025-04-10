from __future__ import annotations
import os, pygame
# from pygame.locals import *
from openstk.gfx.gfx_render import IRenderer, RenderPass
from openstk.gfx.gfx_texture import ITexture, ITextureFrames
from openstk.gfx.gl_renderer import TextureRenderer, TestTriRenderer

# typedefs
class IOpenGLGfx: pass

# ViewBase
class ViewBase:
    renderers: list[IRenderer] = []
    toggleValue: bool = False
    level: range = None
    FACTOR: int = 0
    def __init__(self, gfx: IOpenGLGfx, obj: object):
        self.gfx = gfx
        self.obj = obj
    def getViewport(self, size: tuple) -> tuple: return None
    def dispose(self) -> None: pass
    def start(self) -> None: pass
    def update(self, deltaTime: int) -> None: pass
    def render(self, camera: GLCamera, frameTime: float) -> None:
        if not self.renderers: return
        for renderer in self.renderers: renderer.render(camera, RenderPass.Both)

# ViewCell
class ViewCell(ViewBase):
    def __init__(self, gfx: IOpenGLGfx, obj: object): super().__init__(gfx, obj)

# ViewParticle
class ViewParticle(ViewBase):
    def __init__(self, gfx: IOpenGLGfx, obj: object): super().__init__(gfx, obj)

# ViewEngine
class ViewEngine(ViewBase):
    def __init__(self, gfx: IOpenGLGfx, obj: object): super().__init__(gfx, obj)

# ViewObject
class ViewObject(ViewBase):
    def __init__(self, gfx: IOpenGLGfx, obj: object): super().__init__(gfx, obj)

# ViewMaterial
class ViewMaterial(ViewBase):
    def __init__(self, gfx: IOpenGLGfx, obj: object): super().__init__(gfx, obj)

# ViewTexture
class ViewTexture(ViewBase):
    def __init__(self, gfx: IOpenGLGfx, obj: object): super().__init__(gfx, obj)
    def getViewport(self, size: tuple) -> tuple:
        return None if not (o := self.obj) else \
        size if o.width > 1024 or o.height > 1024 or False else (o.width << self.FACTOR, o.height << self.FACTOR)
    def start(self) -> None:
        obj: ITexture = self.obj
        self.gfx.textureManager.deleteTexture(obj)
        texture, _ = self.gfx.textureManager.createTexture(obj, self.level)
        self.renderers = [TextureRenderer(self.gfx, texture, self.toggleValue)]

# ViewVideoTexture
class ViewVideoTexture(ViewBase):
    frameDelay: int = 0
    def __init__(self, gfx: IOpenGLGfx, obj: object):
        super().__init__(gfx, obj)
    def getViewport(self, size: tuple) -> tuple:
        return None if not (o := self.obj) else \
        size if o.width > 1024 or o.height > 1024 or False else (o.width << self.FACTOR, o.height << self.FACTOR)
    def start(self) -> None:
        obj: ITextureFrames = self.obj
        self.gfx.textureManager.deleteTexture(obj)
        texture, _ = self.gfx.textureManager.createTexture(obj, self.level)
        self.renderers = [TextureRenderer(self.gfx, texture, self.toggleValue)]
    def update(self, deltaTime: int) -> None:
        obj: ITextureFrames = self.obj
        if not self.gfx or not obj or not obj.hasFrames(): return
        self.frameDelay += deltaTime
        if self.frameDelay <= obj.fps or not obj.decodeFrame(): return
        self.frameDelay = 0 # reset delay between frames
        self.gfx.textureManager.reloadTexture(obj)

# ViewTestTri
class ViewTestTri(ViewBase):
    def __init__(self, gfx: IOpenGLGfx, obj: object): super().__init__(gfx, obj)
    def start(self) -> None:
        self.renderers = [TestTriRenderer(self.gfx)]

@staticmethod
def createView(parent: object, gfx: IOpenGLGfx, obj: object, type: str) -> ViewBase:
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