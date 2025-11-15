import sys, os, pygame, numpy as np
# from pygame.locals import *
from PyQt6.QtCore import Qt, QTimer
from PyQt6.QtGui import QWindow
from PyQt6.QtWidgets import QWidget, QVBoxLayout, QHBoxLayout, QPushButton
# from openstk.gfx import ITexture, IRenderer
from gamex.platform_pygame_views import createView
from panda3d.core import loadPrcFileData, ClockObject, WindowProperties, GraphicsEngine, GraphicsPipe, GraphicsOutput
from panda3d.core import Filename, Texture, TextureStage, NodePath, Camera, PerspectiveLens, AmbientLight, DirectionalLight
from panda3d.core import Vec3, Vec4, Point3, Point4, Quat, LQuaternion

# https://www.google.com/search?q=panda3d+in+pyqt&rlz=1C1CHZN_enUS1038US1038&oq=panda3d+in+pyqt&gs_lcrp=EgZjaHJvbWUyBggAEEUYOTIICAEQABgWGB4yDQgCEAAYhgMYgAQYigUyDQgDEAAYhgMYgAQYigUyDQgEEAAYhgMYgAQYigUyBwgFEAAY7wUyBwgGEAAY7wUyCggHEAAYgAQYogQyCggIEAAYgAQYogTSAQg1NzUyajBqNKgCALACAQ&sourceid=chrome&ie=UTF-8

# typedefs
class IPanda3dGfx: pass

class ViewPanda3d(QWidget):
    def __init__(self, parent: object, tab: object):
        super().__init__(parent)
        loadPrcFileData('', 'window-type none')
        self.engine = GraphicsEngine()
        self.pipe = GraphicsPipe.open_pipe()
        props = WindowProperties()
        props.set_parent_window(int(self.winId()))
        props.set_size(self.width(), self.height())
        self.window = self.engine.make_output(self.pipe, 'window', 0, props, GraphicsPipe.BF_refuse_window, None)
        self.engine.open_windows()

        self.scene = NodePath('scene')
        self.camera = Camera('camera')
        self.camera_node = self.scene.attach_new_node(self.camera)
        self.lens = PerspectiveLens()
        self.camera.set_lens(self.lens)
        
        self.render_target = self.window.make_texture_buffer('render_target', self.width(), self.height())
        self.render_target.set_clear_color(Vec4(0, 0, 0, 1))
        self.render_target_node = self.render_target.get_texture()
        self.camera_node.reparent_to(self.scene)
        self.render_target_cam_node = self.render_target.make_camera(self.camera)
        self.render_target_cam_node.reparent_to(self.camera_node)
        self.scene.set_shader_auto()
        self.model = loader.load_model('models/environment')
        self.model.reparent_to(self.scene)
        self.model.set_scale(0.25, 0.25, 0.25)
        self.model.set_pos(-8, 42, 0)
        self.ambient_light = AmbientLight('ambient_light')
        self.ambient_light.set_color(Vec4(0.2, 0.2, 0.2, 1))
        self.ambient_light_node = self.scene.attach_new_node(self.ambient_light)
        self.scene.set_light(self.ambient_light_node)
        self.directional_light = DirectionalLight('directional_light')
        self.directional_light.set_color(Vec4(0.8, 0.8, 0.8, 1))
        self.directional_light_node = self.scene.attach_new_node(self.directional_light)
        self.directional_light_node.set_hpr(-10, -10, -10)
        self.scene.set_light(self.directional_light_node)
        self.camera_node.set_pos(0, -20, 3)
        self.camera_node.look_at(0, 0, 0)
        self.clock = ClockObject.get_global_clock()
        self.last_time = self.clock.get_real_time()
        self.timer = self.startTimer(16)

    def timerEvent(self, event):
        time = self.clock.get_real_time()
        dt = time - self.last_time
        self.last_time = time
        self.engine.step_frame()

    def resizeEvent(self, event):
        props = WindowProperties()
        props.set_parent_window(int(self.winId()))
        props.set_size(self.width(), self.height())
        self.window.modify_properties(props)
        self.render_target.modify_size(self.width(), self.height())
        self.lens.set_film_size(self.width(), self.height())

# class MainWindow(QtWidgets.QMainWindow):
#     def __init__(self):
#         super().__init__()
#         self.panda_widget = PandaQWidget(self)
#         self.setCentralWidget(self.panda_widget)
#         self.setWindowTitle("Panda3D in PyQt6")
#         self.resize(800, 600)
