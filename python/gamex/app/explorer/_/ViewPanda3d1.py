import sys
from PyQt5 import QtWidgets
from panda3d.core import *
from panda3d.core import loadPrcFileData
from direct.showbase.ShowBase import ShowBase

loadPrcFileData("", "window-type none")

class PandaWindow(QtWidgets.QWidget):
    def __init__(self, parent=None):
        super().__init__(parent)
        self.setLayout(QtWidgets.QVBoxLayout())
        self.panda_app = ShowBase()
        wp = WindowProperties()
        wp.setParentWindow(int(self.winId()))
        wp.setSize(self.width(), self.height())
        self.panda_app.openMainWindow(props=wp)
        self.panda_app.disableMouse()
        self.panda_app.camera.setPos(0, -10, 0)
        self.panda_app.camera.lookAt(0, 0, 0)
        self.model = self.panda_app.loader.loadModel("models/box")
        self.model.reparentTo(self.panda_app.render)
        self.task = self.panda_app.taskMgr.add(self.rotate_model, "rotate_model")
        
    def rotate_model(self, task):
        self.model.setH(self.model.getH() + 1)
        return task.cont

    def resizeEvent(self, event):
         wp = WindowProperties()
         wp.setParentWindow(int(self.winId()))
         wp.setSize(self.width(), self.height())
         self.panda_app.openMainWindow(props=wp)

    def closeEvent(self, event):
        self.panda_app.closeWindow()
        self.panda_app.destroy()

class MainWindow(QtWidgets.QMainWindow):
    def __init__(self):
        super().__init__()
        self.panda_window = PandaWindow(self)
        self.setCentralWidget(self.panda_window)

if __name__ == '__main__':
    app = QtWidgets.QApplication(sys.argv)
    main_window = MainWindow()
    main_window.show()
    sys.exit(app.exec_())