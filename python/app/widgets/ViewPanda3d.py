import sys, os, pygame, numpy as np
# from pygame.locals import *
from typing import TypeVar
from PyQt6.QtCore import Qt, QTimer
from PyQt6.QtGui import QWindow
from PyQt6.QtWidgets import QWidget, QVBoxLayout, QHBoxLayout, QPushButton
from gamex.platform_pygame_views import createView
from panda3d.core import loadPrcFileData, WindowProperties, FrameBufferProperties
from direct.showbase.ShowBase import ShowBase

# typedefs
class IPanda3dGfx: pass

loadPrcFileData('', 'allow-parent 1') #window-type offscreen')

class ViewPanda3d(QWidget, ShowBase):
    view: object = None

    def __init__(self, parent: object, tab: object):
        super(QWidget, self).__init__(parent)
        self.initializePanda3d()

    def initializePanda3d(self) -> None:
        super(ShowBase, self).__init__()
        wp = WindowProperties()
        # wp.setParentWindow(int(self.winId()))
        wp.setSize(self.width(), self.height())
        self.openMainWindow(name='Panda3d', props=wp)
        # self.openWindow(name='Panda3d', props=wp)
        self.disableMouse()
        self.camera.setPos(0, -10, 0)
        self.camera.lookAt(0, 0, 0)
        self.model = self.loader.loadModel('teapot')
        self.model.reparentTo(self.render)

    # def tick(self):
    #     self.engine.render_frame()
    #     self.clock.tick()

    def resizeEvent(self, event):
        wp = WindowProperties()
        # wp.setParentWindow(int(self.winId()))
        wp.setSize(self.width(), self.height())
        self.win.requestProperties(wp)
        # self.openMainWindow(name='Panda3d', props=wp)
        pass

    def closeEvent(self, event):
        self.closeWindow()
        self.destroy()