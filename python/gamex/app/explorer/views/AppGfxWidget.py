from openstk.platforms.ex.gfx.qt_widget import ExWidget
from openstk.platforms.opengl.gfx.qt_widget import OpenGLWidget
from openstk.platforms.panda3d.gfx.qt_widget import Panda3dWidget
from openstk.platforms.pyengine3d.gfx.qt_widget import PyEngine3dWidget
from openstk.platforms.pygame.gfx.qt_widget import PygameWidget
from gamex.platforms.ex import ExRenderer
from gamex.platforms.opengl import OpenGLRenderer
from gamex.platforms.panda3d import Panda3dRenderer
from gamex.platforms.pyengine3d import PyEngine3dRenderer
from gamex.platforms.pygame import PygameRenderer

# typedefs
class Renderer: pass

# AppExWidget
class AppExWidget(ExWidget):
    def __init__(self, parent: object, tab: object): super().__init__(parent, tab)
    def createRenderer(self) -> Renderer: return ExRenderer.createRenderer(self, self.gfx, self.value, self.type)

# AppOpenGLWidget
class AppOpenGLWidget(OpenGLWidget):
    def __init__(self, parent: object, tab: object): super().__init__(parent, tab)
    def createRenderer(self) -> Renderer: return OpenGLRenderer.createRenderer(self, self.gfx, self.value, self.type)

# AppPanda3dWidget
class AppPanda3dWidget(Panda3dWidget):
    def __init__(self, parent: object, tab: object): super().__init__(parent, tab)
    def createRenderer(self) -> Renderer: return Panda3dRenderer.createRenderer(self, self.gfx, self.value, self.type)

# AppPyEngine3dWidget
class AppPyEngine3dWidget(PyEngine3dWidget):
    def __init__(self, parent: object, tab: object): super().__init__(parent, tab)
    def createRenderer(self) -> Renderer: return PyEngine3dRenderer.createRenderer(self, self.gfx, self.value, self.type)

# AppPygameWidget
class AppPygameWidget(PygameWidget):
    def __init__(self, parent: object, tab: object): super().__init__(parent, tab)
    def createRenderer(self) -> Renderer: return PygameRenderer.createRenderer(self, self.gfx, self.value, self.type)
