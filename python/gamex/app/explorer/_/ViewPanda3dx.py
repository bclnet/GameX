import sys, os, pygame, numpy as np
# from pygame.locals import *
from typing import TypeVar
from PyQt6.QtCore import Qt, QTimer
from PyQt6.QtGui import QWindow
from PyQt6.QtWidgets import QWidget, QVBoxLayout, QHBoxLayout, QPushButton
# from openstk.gfx import ITexture, IRenderer
from gamex.platform_pygame_views import createView
# from panda3d.core import loadPrcFileData, ClockObject, WindowProperties, GraphicsEngine, GraphicsPipe, GraphicsOutput
# from panda3d.core import Filename, Texture, TextureStage, NodePath, Camera, PerspectiveLens, AmbientLight, DirectionalLight
# from panda3d.core import Vec3, Vec4, Point3, Point4, Quat, LQuaternion

from panda3d.core import loadPrcFileData, Loader, WindowProperties, FrameBufferProperties, GraphicsEngine, GraphicsWindow, GraphicsPipe, GraphicsPipeSelection
from panda3d.core import ClockObject, NodePath, Camera, PerspectiveLens

# https://www.google.com/search?q=panda3d+in+pyqt&rlz=1C1CHZN_enUS1038US1038&oq=panda3d+in+pyqt&gs_lcrp=EgZjaHJvbWUyBggAEEUYOTIICAEQABgWGB4yDQgCEAAYhgMYgAQYigUyDQgDEAAYhgMYgAQYigUyDQgEEAAYhgMYgAQYigUyBwgFEAAY7wUyBwgGEAAY7wUyCggHEAAYgAQYogQyCggIEAAYgAQYogTSAQg1NzUyajBqNKgCALACAQ&sourceid=chrome&ie=UTF-8

# typedefs
class IPanda3dGfx: pass

class ViewPanda3d(QWidget):
    view: object = None

    def __init__(self, parent: object, tab: object):
        super().__init__(parent)
        loadPrcFileData('', 'window-type none')
        self.clock = ClockObject.get_global_clock()
        self.engine = GraphicsEngine.get_global_ptr()
        self.pipe = GraphicsPipeSelection.get_global_ptr().make_default_pipe()
        wp = WindowProperties()
        wp.set_parent_window(int(self.winId()))
        wp.set_size(self.width(), self.height())
        fb = FrameBufferProperties()
        # fb.rgb_color = 1
        # fb.color_bits = 3 * 8
        # fb.depth_bits = 24
        # fb.back_buffers = 1
        self.window = self.engine.make_output(self.pipe, 'Panda', 0, fb, wp, GraphicsPipe.BF_refuse_window) #GraphicsPipe.UH_parent | GraphicsPipe.UH_no_decorate | GraphicsPipe.UH_fixed_size
        self.engine.open_windows()

        # Set a grey background color
        # self.window.set_clear_color_active(True)
        # self.window.set_clear_color((0.5, 0.5, 0.5, 1))

        # create scene
        self.scene = NodePath('scene')
        self.camera = Camera('camera')
        self.camera_node = self.scene.attach_new_node(self.camera)
        self.lens = PerspectiveLens()
        self.camera.set_lens(self.lens)
        # self.render_target = self.engine.make_camera(self.window, self.camera)
        # self.render_target.set_scene(self.scene)

        # Load a model into the scene
        self.loader = Loader.get_global_ptr()
        node = self.loader.load_model('models/environment')
        print(node)
        model = self.scene.attach_new_node(self.loader.load_sync('model.egg'))
        # model.set_pos(0, 20, 0)

        # self.model = loader.load_model('models/environment')
        # self.model.reparent_to(self.scene)

        self.timer = QTimer(self)
        self.timer.timeout.connect(self.tick)
        self.timer.start(16)

    def tick(self):
        self.engine.render_frame()
        self.clock.tick()

    def resizeEvent(self, event):
        wp = WindowProperties()
        wp.set_size(event.size().width(), event.size().height())
        # self.window.request_properties(wp)
        # self.render_target.modify_size(self.width(), self.height())
        self.lens.set_film_size(event.size().width(), event.size().height())

    def closeEvent(self, event):
        self.engine.remove_all()
        self.clock = None