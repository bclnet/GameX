import sys, os, pygame, numpy as np
# from pygame.locals import *
from typing import TypeVar
from PyQt6.QtCore import Qt, QTimer
from PyQt6.QtGui import QWindow
from PyQt6.QtWidgets import QWidget, QVBoxLayout, QHBoxLayout, QPushButton
from gamex.platform_pygame_views import createView
from panda3d.core import loadPrcFileData, Loader, WindowProperties, FrameBufferProperties, GraphicsEngine, GraphicsOutput, GraphicsWindow, GraphicsPipe, GraphicsPipeSelection
from panda3d.core import ClockObject, NodePath, Camera, PerspectiveLens
from direct.showbase.Loader import Loader

# https://github.com/panda3d/panda3d/blob/master/direct/src/showbase/ShowBase.py

# typedefs
class IPanda3dGfx: pass

class ViewPanda3d(QWidget):
    view: object = None

    def __init__(self, parent: object, tab: object):
        super().__init__(parent)
        loadPrcFileData('', 'window-type none')
        self.clock = ClockObject.getGlobalClock()
        self.engine = GraphicsEngine.getGlobalPtr()
        self.pipe = GraphicsPipeSelection.getGlobalPtr().makeDefaultPipe()
        if not self.pipe: raise Exception('No graphics pipe is available!')
        self.loader = Loader(None)
        self.engine.setDefaultLoader(self.loader.loader)

        # self.loader = Loader.getGlobalPtr()
        wp = WindowProperties()
        wp.setParentWindow(int(self.winId()))
        wp.setSize(self.width(), self.height())
        fb = FrameBufferProperties()
        self.window = self.engine.makeOutput(self.pipe, 'Panda', 0, fb, wp, GraphicsPipe.BF_refuse_window)
        self.engine.openWindows()

        # Set a grey background color
        # self.window.setClearColorActive(True)
        # self.window.setClearColor((0.5, 0.5, 0.5, 1))


        # Load the environment model.
        self.scene = self.loader.loadModel('teapot')
        # self.scene = self.loader.loadModel('models/environment')
        # self.scene.reparentTo(self.render)
        # Apply scale and position transforms on the model.
        # self.scene.setScale(0.25, 0.25, 0.25)
        # self.scene.setPos(-8, 42, 0)

        # create scene
        self.render = NodePath('render')
        self.camera = self.scene.attachNewNode(Camera('camera'))
        self.camera.node().setLens(PerspectiveLens())
        self.camera.reparent_to(self.render)
        # self.render_target = self.engine.makeCamera(self.window, self.camera)
        # self.render_target.set_scene(self.scene)

        # Load a model into the scene
        model = self.loader.loadModel('teapot')
        model.setPos(0, 20, 0)
        model.reparent_to(self.render)

        # self.model = loader.load_model('models/environment')
        # self.model.reparent_to(self.scene)

        self.timer = QTimer(self)
        self.timer.timeout.connect(self.tick)
        self.timer.start(16)

    def tick(self):
        self.engine.render_frame()
        self.clock.tick()

    def resizeEvent(self, event):
        width, height = (event.size().width(), event.size().height())
        wp = WindowProperties()
        wp.set_size(width, height)
        print(dir(self.window))
        self.window.request_properties(wp)
        # self.render_target.modify_size(self.width(), self.height())
        self.lens.setFilmSize(width, height)

    def closeEvent(self, event):
        self.engine.remove_all()
        self.clock = None