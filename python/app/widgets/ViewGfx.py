from gamex.platform_controls_pyqt6 import OpenGLWidget, Panda3dWidget, PygameWidget

# ViewOpenGL
class ViewOpenGL(OpenGLWidget):
    def __init__(self, parent: object, tab: object):
        super().__init__(parent, tab)

# ViewGLBase
class ViewPanda3d(Panda3dWidget):
    def __init__(self, parent: object, tab: object):
        super().__init__(parent, tab)

# ViewPygame
class ViewPygame(PygameWidget):
    def __init__(self, parent: object, tab: object):
        super().__init__(parent, tab)