import sys, os
from PyQt6.QtWidgets import QApplication
from PyQt6.QtCore import Qt
from PyQt6.QtGui import QSurfaceFormat
from gamex import EngineX
from openx.engines.eginx import EginXEngine
from openx.engines.opengl import OpenGLEngine
from openx.engines.panda3d import Panda3dEngine
from openx.engines.pyengine3d import PyEngine3dEngine
from openx.engines.pygame import PygameEngine

EngineX.engines = EngineX.engines.union({EginXEngine.this, OpenGLEngine.this, Panda3dEngine.this, PyEngine3dEngine.this, PygameEngine.this})
from gamex.app.explorer.views.MainPage import MainPage

if __name__ == '__main__':
    # QApplication.setAttribute(Qt.ApplicationAttribute.AA_UseDesktopOpenGL)
    # fmt = QSurfaceFormat()
    # fmt.setVersion(4, 6)
    # fmt.setProfile(QSurfaceFormat.OpenGLContextProfile.CoreProfile)
    # fmt.setOption(QSurfaceFormat.FormatOption.DebugContext)
    # QSurfaceFormat.setDefaultFormat(fmt)

    app = QApplication(sys.argv)
    p = MainPage()
    p.startup()
    sys.exit(app.exec())