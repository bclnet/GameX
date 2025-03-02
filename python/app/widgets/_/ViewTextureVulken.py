import sys, os, numpy as np
from Vulken.GL import *
from PyQt6.QtWidgets import QWidget
from PyQt6.QtCore import Qt
from PyQt6.QtVulkenWidgets import QVulkanWindow
from PyQt6.QtVulken import QOpenGLBuffer, QOpenGLShader, QOpenGLShaderProgram, QOpenGLTexture
from openstk.gfx_render import RenderPass
from openstk.gfx_texture import ITexture, ITextureSelect
from openstk.vk_view import VulkenView
from openstk.vk_renderer import TextureRenderer
from openstk.gfx_ui import MouseState, KeyboardState

FACTOR: int = 1

# typedefs
class VulkenCamera: pass
class IVulkenGfx: pass

# TextureView
class TextureView(VulkenView):
    background: bool = False
    level: range = None
    renderers: list[TextureRenderer] = []
    obj: obj = None
    # ui
    id: int = 0

    def __init__(self, parent, tab):
        super().__init__()
        self.parent = parent
        self.gfx: IOpenGLGfx = parent.gfx
        self.source: ITexture = tab.value
        
    def initializeGL(self):
        super().initializeGL()
        self.onProperty()

    def setViewportSize(self, x: int, y: int, width: int, height: int) -> None:
        if not self.obj: return
        if self.obj.width > 1024 or self.obj.height > 1024 or False: super().setViewportSize(x, y, width, height)
        else: super().setViewportSize(x, y, self.obj.width << FACTOR, self.obj.height << FACTOR)

    def onProperty(self):
        if not self.gfx or not self.source: return
        self.gl = self.gfx
        self.obj = self.source if isinstance(self.source, ITexture) else None
        if not self.obj: return
        if isinstance(self.source, ITextureSelect): self.source.select(self.id)

        # self.camera.setLocation(np.array([200., 200., 200.]))
        # self.camera.lookAt(np.zeros(3))

        self.gl.textureManager.deleteTexture(self.obj)
        texture, _ = self.gl.textureManager.createTexture(self.obj, self.level)
        self.renderers.clear()
        self.renderers.append(TextureRenderer(self.gl, texture, self.background))

    def render(self, camera: GLCamera, frameTime: float):
        for renderer in self.renderers: renderer.render(camera, RenderPass.Both)

    def handleInput(self, mouseState: MouseState, keyboardState: KeyboardState):
        pass
    #     # if key == "r":
    #     #     glColor3f(1.0, 0.0, 0.0)
    #     #     print "Presionaste",key
    #     # elif key == "g":
    #     #     glColor3f(0.0, 1.0, 0.0)
    #     #     print "Presionaste g"
    #     # elif key ==   "b":
    #     #     glColor3f(0.0, 0.0, 1.0)
    #     #     print "Presionaste b"

