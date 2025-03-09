import sys, os, pygame, numpy as np
# from pygame.locals import *
from typing import TypeVar
from PyQt6.QtCore import Qt, QEvent, QTimer
from PyQt6.QtGui import QWindow
from PyQt6.QtWidgets import QWidget, QVBoxLayout, QHBoxLayout, QPushButton
from openstk.gfx.gfx_texture import ITextureSelect
from gamex.platform_panda3d_views import createView
from panda3d.core import loadPrcFileData, WindowProperties, FrameBufferProperties
from direct.showbase.ShowBase import ShowBase

# https://discourse.panda3d.org/t/panda-in-pyqt/3964/35?page=2

loadPrcFileData('', """
allow-parent 1
window-title GameX
show-frame-rate-meter #t
""")

# typedefs
class IPanda3dGfx: pass

class ViewPanda3d(QWidget, ShowBase):
    view: object = None
    id: int = 0

    # Binding

    def __init__(self, parent: object, tab: object):
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
        self.view = createView(self, self.gfx, self.source, self.type)
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

 

