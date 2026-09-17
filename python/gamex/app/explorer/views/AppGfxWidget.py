from openx.core import EngineX
from openx.engines.eginx.gfx.qt_widget import EginXWidget
from openx.engines.opengl.gfx.qt_widget import OpenGLWidget
from openx.engines.panda3d.gfx.qt_widget import Panda3dWidget
from openx.engines.pyengine3d.gfx.qt_widget import PyEngine3dWidget
from openx.engines.pygame.gfx.qt_widget import PygameWidget
from gamex.engines.eginx import EginXRenderer
from gamex.engines.opengl import OpenGLRenderer
from gamex.engines.panda3d import Panda3dRenderer
from gamex.engines.pyengine3d import PyEngine3dRenderer
from gamex.engines.pygame import PygameRenderer

# typedefs
class Renderer: pass

# AppEginXWidget
class AppEginXWidget(EginXWidget):
    def __init__(self, parent: object, tab: object): super().__init__(parent, tab)
    def createRenderer(self) -> Renderer: return EginXRenderer.createRenderer(self, EngineX.gfx, self.source, self.value, self.type)

# AppOpenGLWidget
class AppOpenGLWidget(OpenGLWidget):
    def __init__(self, parent: object, tab: object): super().__init__(parent, tab)
    def createRenderer(self) -> Renderer: return OpenGLRenderer.createRenderer(self, EngineX.gfx, self.source, self.value, self.type)

# AppPanda3dWidget
class AppPanda3dWidget(Panda3dWidget):
    def __init__(self, parent: object, tab: object): super().__init__(parent, tab)
    def createRenderer(self) -> Renderer: return Panda3dRenderer.createRenderer(self, EngineX.gfx, self.source, self.value, self.type)

# AppPyEngine3dWidget
class AppPyEngine3dWidget(PyEngine3dWidget):
    def __init__(self, parent: object, tab: object): super().__init__(parent, tab)
    def createRenderer(self) -> Renderer: return PyEngine3dRenderer.createRenderer(self, EngineX.gfx, self.source, self.value, self.type)

# AppPygameWidget
class AppPygameWidget(PygameWidget):
    def __init__(self, parent: object, tab: object): super().__init__(parent, tab)
    def createRenderer(self) -> Renderer: return PygameRenderer.createRenderer(self, EngineX.gfx, self.source, self.value, self.type)
