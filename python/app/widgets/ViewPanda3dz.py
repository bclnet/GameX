import sys, os, pygame, numpy as np
# from pygame.locals import *
from typing import TypeVar
from PyQt6.QtCore import Qt, QTimer
from PyQt6.QtGui import QWindow
from PyQt6.QtWidgets import QWidget, QVBoxLayout, QHBoxLayout, QPushButton
# from openstk.gfx.gfx_render import IRenderer
# from openstk.gfx.gfx_texture import ITexture
from gamex.platform_pygame_views import createView
# from panda3d.core import loadPrcFileData, Loader, WindowProperties, FrameBufferProperties, GraphicsEngine, GraphicsWindow, GraphicsPipe, GraphicsPipeSelection
# from panda3d.core import ClockObject, NodePath, Camera, PerspectiveLens
from panda3d.core import WindowProperties
from panda3d.core import loadPrcFileData
from direct.showbase.ShowBase import ShowBase

# https://www.google.com/search?q=panda3d+in+pyqt&rlz=1C1CHZN_enUS1038US1038&oq=panda3d+in+pyqt&gs_lcrp=EgZjaHJvbWUyBggAEEUYOTIICAEQABgWGB4yDQgCEAAYhgMYgAQYigUyDQgDEAAYhgMYgAQYigUyDQgEEAAYhgMYgAQYigUyBwgFEAAY7wUyBwgGEAAY7wUyCggHEAAYgAQYogQyCggIEAAYgAQYogTSAQg1NzUyajBqNKgCALACAQ&sourceid=chrome&ie=UTF-8

# typedefs
class IPanda3dGfx: pass

loadPrcFileData('', 'window-type none')

class ViewPanda3d(QWidget):
    view: object = None

    def __init__(self, parent: object, tab: object):
        super().__init__(parent)
        self.base = ShowBase()
        wp = WindowProperties()
        wp.setParentWindow(int(self.winId()))
        wp.setSize(self.width(), self.height())
        self.base.openMainWindow(props=wp)
        self.base.disableMouse()
        self.base.camera.setPos(0, -10, 0)
        self.base.camera.lookAt(0, 0, 0)
        self.model = self.base.loader.loadModel('models/box')
        self.model.reparentTo(self.base.render)


    # def tick(self):
    #     self.engine.render_frame()
    #     self.clock.tick()

    def resizeEvent(self, event):
        wp = WindowProperties()
        wp.setParentWindow(int(self.winId()))
        wp.setSize(self.width(), self.height())
        self.base.openMainWindow(props=wp)

    def closeEvent(self, event):
        self.base.closeWindow()
        self.base.destroy()