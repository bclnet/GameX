import sys, os, pygame, numpy as np
# from pygame.locals import *
from typing import TypeVar
from PyQt6.QtCore import Qt, QTimer
from PyQt6.QtGui import QWindow
from PyQt6.QtWidgets import QWidget, QVBoxLayout, QHBoxLayout, QPushButton
from openstk.gfx.gfx_texture import ITextureSelect
from gamex.platform_pygame import PygamePlatform
from gamex.platform_pygame_views import createView

# https://stackoverflow.com/questions/38280057/how-to-integrate-pygame-and-pyqt4
# https://gist.github.com/martinnovaak/aa3a905980a1f6484e1ffd721080dd78
# https://stackoverflow.com/questions/12828825/how-to-assign-callback-when-the-user-resizes-a-qmainwindow
# https://stackoverflow.com/questions/34910086/pygame-how-do-i-resize-a-surface-and-keep-all-objects-within-proportionate-to-t

# typedefs
class IOpenGfx: pass

# ViewPygame
class ViewPygame(QWidget):
    id: int = 0
    view: object = None

    #region Embedding

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
        self.view = createView(self, self.gfx, self.source, self.type)
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
