import sys, os
from PyQt6.QtWidgets import QApplication
from PyQt6.QtCore import Qt
from PyQt6.QtGui import QSurfaceFormat
from gamex import PlatformX
from openstk.platforms.platform_opengl import OpenGLPlatform
from openstk.platforms.platform_panda3d import Panda3dPlatform
from openstk.platforms.platform_pygame import PygamePlatform

PlatformX.platforms = PlatformX.platforms.union({OpenGLPlatform.This, Panda3dPlatform.This, PygamePlatform.This})
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