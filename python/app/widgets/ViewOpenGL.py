import sys, os, numpy as np
from typing import TypeVar
from OpenGL.GL import *
from openstk.gfx.gfx_texture import ITextureSelect
from openstk.gfx.gl_view import OpenGLView
from openstk.gfx.gfx_ui import MouseState, KeyboardState
from gamex.platform_opengl import OpenGLPlatform
from gamex.platform_opengl_views import createView

# typedefs
class GLCamera: pass
class IOpenGfx: pass
class IOpenGLGfx: pass

# ViewGLBase
class ViewOpenGL(OpenGLView):
    view: object = None
    id: int = 0

    # Binding

    def __init__(self, parent: object, tab: object, interval: float = 1.0):
        super().__init__(interval)
        self.gfx: IOpenGfx = parent.gfx
        self.source: object = tab.value
        self.type: str = tab.type
        
    def onSourceChanged(self) -> None:
        if not self.gfx or not self.source or not self.type: return
        self.view = createView(self, self.gfx, self.source, self.type)
        self.view.start()
        if isinstance(self.source, ITextureSelect): self.source.select(self.id)

    # Render

    def initializeGL(self) -> None:
        super().initializeGL()
        self.onSourceChanged()

    def setViewport(self, x: int, y: int, width: int, height: int) -> None:
        p = self.view.getViewport((width, height)) or (width, height) if self.view else (width, height)
        super().setViewport(x, y, p[0], p[1])

    def render(self, camera: GLCamera, frameTime: float) -> None:
        if self.view: self.view.render(camera, frameTime)

    def tick(self) -> None:
        super().tick()
        if self.view: self.view.update(self.deltaTime)
        self.render(self.camera, 0.0)

    # HandleInput

    def handleInput(self, mouseState: MouseState, keyboardState: KeyboardState) -> None: pass
