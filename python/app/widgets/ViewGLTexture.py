import sys, os, numpy as np
from OpenGL.GL import *
from openstk.gfx.gfx_render import IRenderer
from openstk.gfx.gfx_texture import ITexture
from openstk.gfx.gl_renderer import TextureRenderer
from .ViewGLBase import ViewGLBase

# ViewGLTexture
class ViewGLTexture(ViewGLBase):
    def __init__(self, parent, tab):
        super().__init__(parent, tab)

    # def setViewportx(self, x: int, y: int, width: int, height: int) -> None:
    #     if not self.obj: return
    #     if self.obj.width > 1024 or self.obj.height > 1024 or False: super().setViewport(x, y, width, height)
    #     else: super().setViewport(x, y, self.obj.width << self.FACTOR, self.obj.height << self.FACTOR)

    def getObj(self, source: object) -> (ITexture, list[IRenderer]):
        obj: ITexture = source
        self.g.textureManager.deleteTexture(obj)
        texture, _ = self.g.textureManager.createTexture(obj, self.level)
        return (obj, [TextureRenderer(self.g, texture, self.toggleValue)])
