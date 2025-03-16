import sys, os, numpy as np
from PyQt6.QtCore import Qt, QEvent, QTimer
from PyQt6.QtGui import QWindow
from PyQt6.QtWidgets import QWidget, QVBoxLayout, QHBoxLayout, QPushButton
from openstk.gfx.gfx_texture import ITextureSelect
from openstk.gfx.gfx_ui import MouseState, KeyboardState
# opengl
from OpenGL.GL import *
from openstk.gfx.gl_view import OpenGLView
from gamex.platform_opengl import OpenGLPlatform
from gamex.platform_opengl_views import createView as openGLCreateView
# panda3d
from panda3d.core import loadPrcFileData, WindowProperties, FrameBufferProperties
from direct.showbase.ShowBase import ShowBase
from gamex.platform_panda3d_views import createView as panda3dCreateView
# pygame
import pygame
# from pygame.locals import *
from gamex.platform_pygame import PygamePlatform
from gamex.platform_pygame_views import createView as pygameCreateView

# typedefs
class GLCamera: pass
class IOpenGfx: pass
class IOpenGLGfx: pass
class IPanda3dGfx: pass
class IPygameGfx: pass

#region OpenGLWidget

# OpenGLWidget
class OpenGLWidget(OpenGLView):
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
        self.view = openGLCreateView(self, self.gfx, self.source, self.type)
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

#endregion

#region Panda3dWidget
# https://discourse.panda3d.org/t/panda-in-pyqt/3964/35?page=2

class Panda3dWidget(QWidget, ShowBase):
    view: object = None
    id: int = 0

    # Binding

    def __init__(self, parent: object, tab: object):
        loadPrcFileData('', """
        allow-parent 1
        window-title GameX
        show-frame-rate-meter #t
        """)
        super(QWidget, self).__init__(parent)
        super(ShowBase, self).__init__()
        self.gfx: IOpenGfx = parent.gfx
        self.source: object = tab.value
        self.type: str = tab.type
        # print('win: %s' % base.win.getProperties())

        # self.disableMouse()
        # self.camera.setPos(0, -10, 0)
        # self.camera.lookAt(0, 0, 0)
        self.onSourceChanged()

    def onSourceChanged(self) -> None:
        if not self.gfx or not self.source or not self.type: return
        self.view = panda3dCreateView(self, self.gfx, self.source, self.type)
        self.view.start()
        if isinstance(self.source, ITextureSelect): self.source.select(self.id)

    def closeEvent(self, event):
        self.taskMgr.stop()
        self.closeWindow()
        self.destroy()

    # Render

    def showEvent(self, event: QEvent) -> None:
        super().showEvent(event)
        wp = WindowProperties().getDefault()
        # wp.setForeground(False)
        # wp.setOrigin(0, 0)
        wp.setSize(self.width(), self.height())
        # wp.setParentWindow(int(self.winId()))
        self.openDefaultWindow(props=wp)
        self.run()
        
    # def tick(self):
    #     self.engine.render_frame()
    #     self.clock.tick()

    def resizeEvent(self, event):
        wp = WindowProperties()
        wp.setParentWindow(int(self.winId()))
        wp.setSize(self.width(), self.height())
        # self.win.requestProperties(wp)
        # self.openDefaultWindow(props=wp)

#endregion

#region PygameWidget
# https://stackoverflow.com/questions/38280057/how-to-integrate-pygame-and-pyqt4
# https://gist.github.com/martinnovaak/aa3a905980a1f6484e1ffd721080dd78
# https://stackoverflow.com/questions/12828825/how-to-assign-callback-when-the-user-resizes-a-qmainwindow
# https://stackoverflow.com/questions/34910086/pygame-how-do-i-resize-a-surface-and-keep-all-objects-within-proportionate-to-t

# PygameWidget
class PygameWidget(QWidget):
    id: int = 0
    view: object = None

    # Embedding

    def __init__(self, parent: object, tab: object):
        super().__init__()
        self.gfx: IOpenGfx = parent.gfx
        self.source: object = tab.value
        self.type: str = tab.type

        # create a Pygame surface and pass it to a QWindow
        pygame.init()
        pygame.display.set_caption('Pygame')
        self.surface = pygame.display.set_mode((640, 480), pygame.NOFRAME)
        handle = pygame.display.get_wm_info()['window'] # get the handle to the Pygame window
        self.window = QWindow.fromWinId(handle) # create a QWindow from the handle
        self.widget = QWidget.createWindowContainer(self.window, self) # create a QWidget using the QWindow
        self.widget.setFocusPolicy(Qt.FocusPolicy.StrongFocus) # set the focus policy of the QWidget
        self.widget.setGeometry(0, 0, 640, 480)  # set the size and position of the QWidget
        self.timer = QTimer(self) # create a timer to control the animation
        self.timer.timeout.connect(self.tick) # connect the timeout signal of the timer to the

        # add the Pygame widget to the main window
        layout = QVBoxLayout()
        layout.addWidget(self.widget)
        self.setLayout(layout)
        # Add the start and stop buttons to the main window
        # button_layout = QHBoxLayout()
        # self.start_button = QPushButton('Start Animation', self)
        # self.start_button.clicked.connect(self.start_animation)
        # button_layout.addWidget(self.start_button)
        # self.stop_button = QPushButton('Stop Animation', self)
        # self.stop_button.clicked.connect(self.stop_animation)
        # button_layout.addWidget(self.stop_button)
        # layout.addLayout(button_layout)

        # start / update
        if not self.timer.isActive(): self.timer.start(1000 // 60) # start the timer with an interval of 1000 / 60 milliseconds to update the Pygame surface at 60 FPS
        else: self.timer.start()
        self.onSourceChanged()

    def unload(self):
        self.timer.stop()

    def onSourceChanged(self) -> None:
        if not self.gfx or not self.source or not self.type: return
        self.view = pygameCreateView(self, self.gfx, self.source, self.type)
        self.view.start()
        if isinstance(self.source, ITextureSelect): self.source.select(self.id)

    # Render

    def resizeEvent(self, event):
        # print(self.size())
        pass

    def tick(self):
        self.surface.fill((220, 220, 220))
        self.view.update()
        pygame.display.update()

#endregion