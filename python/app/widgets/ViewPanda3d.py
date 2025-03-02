import sys, os, pygame, numpy as np
# from pygame.locals import *
from typing import TypeVar
from PyQt6.QtCore import Qt, QTimer
from PyQt6.QtGui import QWindow
from PyQt6.QtWidgets import QWidget, QVBoxLayout, QHBoxLayout, QPushButton
# from openstk.gfx.gfx_render import IRenderer
# from openstk.gfx.gfx_texture import ITexture
from gamex.platform_pygame_views import createView
# from panda3d.core import loadPrcFileData, ClockObject, WindowProperties, GraphicsEngine, GraphicsPipe, GraphicsOutput
# from panda3d.core import Filename, Texture, TextureStage, NodePath, Camera, PerspectiveLens, AmbientLight, DirectionalLight
# from panda3d.core import Vec3, Vec4, Point3, Point4, Quat, LQuaternion

from panda3d.core import loadPrcFileData, GraphicsWindow, WindowProperties, GraphicsEngine
from panda3d.core import ClockObject, NodePath, Camera, PerspectiveLens


# https://www.google.com/search?q=panda3d+in+pyqt&rlz=1C1CHZN_enUS1038US1038&oq=panda3d+in+pyqt&gs_lcrp=EgZjaHJvbWUyBggAEEUYOTIICAEQABgWGB4yDQgCEAAYhgMYgAQYigUyDQgDEAAYhgMYgAQYigUyDQgEEAAYhgMYgAQYigUyBwgFEAAY7wUyBwgGEAAY7wUyCggHEAAYgAQYogQyCggIEAAYgAQYogTSAQg1NzUyajBqNKgCALACAQ&sourceid=chrome&ie=UTF-8

# typedefs
class IPanda3dGfx: pass

TObj = TypeVar('TObj')

class ViewPanda3d(QWidget):
    def __init__(self, parent: object, tab: object):
        super().__init__(parent)
        loadPrcFileData('', 'window-type offscreen')
        self.engine = GraphicsEngine()
        self.clock = ClockObject.get_global_clock()

        wp = WindowProperties()
        wp.set_size(self.width(), self.height())
        wp.set_parent_window(int(self.winId()))
        self.window = self.engine.make_output(None, 'Panda', 0, wp, flags=GraphicsWindow.UH_parent | GraphicsWindow.UH_no_decorate | GraphicsWindow.UH_fixed_size)
        self.engine.open_windows()

        self.scene = NodePath('scene')
        self.camera = Camera('camera')
        self.camera_node_path = self.scene.attach_new_node(self.camera)
        self.lens = PerspectiveLens()
        self.camera.set_lens(self.lens)
        self.render_target = self.engine.make_camera(self.window, self.camera)
        self.render_target.set_scene(self.scene)

        self.model = loader.load_model('models/environment')
        self.model.reparent_to(self.scene)

        self.timer = QTimer(self)
        self.timer.timeout.connect(self.update_frame)
        self.timer.start(16)

    def update_frame(self):
        self.engine.render_frame()
        self.clock.tick()

    def resizeEvent(self, event):
        wp = WindowProperties()
        wp.set_size(event.size().width(), event.size().height())
        self.window.request_properties(wp)
        self.lens.set_film_size(event.size().width(), event.size().height())

    def closeEvent(self, event):
        self.engine.remove_all()
        self.clock = None