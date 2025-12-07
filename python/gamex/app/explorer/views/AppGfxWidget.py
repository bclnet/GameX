from openstk.platforms.platform_qt_widget import OpenGLWidget, Panda3dWidget, PygameWidget
from gamex.platform_opengl_render import OpenGLRenderer
from gamex.platform_panda3d_render import Panda3dRenderer
from gamex.platform_pygame_render import PygameRenderer

# typedefs
class Renderer: pass

# AppOpenGLWidget
class AppOpenGLWidget(OpenGLWidget):
    def __init__(self, parent: object, tab: object):
        super().__init__(parent, tab)
    def createRenderer(self) -> Renderer: return OpenGLRenderer.createRenderer(self, self.gfx, self.source, self.type)

# AppPanda3dWidget
class AppPanda3dWidget(Panda3dWidget):
    def __init__(self, parent: object, tab: object):
        super().__init__(parent, tab)
    def createRenderer(self) -> Renderer: return Panda3dRenderer.createRenderer(self, self.gfx, self.source, self.type)

# AppPygameWidget
class AppPygameWidget(PygameWidget):
    def __init__(self, parent: object, tab: object):
        super().__init__(parent, tab)
    def createRenderer(self) -> Renderer: return PygameRenderer.createRenderer(self, self.gfx, self.source, self.type)
