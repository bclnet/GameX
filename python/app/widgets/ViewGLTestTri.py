import sys, os, numpy as np
from OpenGL.GL import *
from openstk.gfx.gfx_render import IRenderer
from openstk.gfx.gl_renderer import TestTriRenderer
from .ViewGLBase import ViewGLBase

# ViewGLTestTri
class ViewGLTestTri(ViewGLBase):
    def __init__(self, parent, tab):
        super().__init__(parent, tab)

    def getObj(self, source: object) -> (object, list[IRenderer]):
        obj: object = self.obj
        return (obj, [TestTriRenderer(self.g)])
