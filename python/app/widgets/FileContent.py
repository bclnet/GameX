import sys, os
from PyQt6.QtWidgets import QMainWindow, QApplication, QWidget, QSizePolicy, QProgressBar, QScrollArea, QTableView, QTableWidget, QTableWidgetItem, QGridLayout, QHeaderView, QAbstractItemView, QLabel, QTextEdit, QHBoxLayout, QMenu, QFileDialog, QSplitter, QTabWidget, QPlainTextEdit
from PyQt6.QtGui import QIcon, QFont, QDrag, QPixmap, QPainter, QColor, QBrush, QAction
from PyQt6.QtCore import Qt, QBuffer, QByteArray, QUrl, QMimeData, pyqtSignal
from PyQt6.QtMultimedia import QMediaPlayer
from PyQt6.QtMultimediaWidgets import QVideoWidget
from PyQt6 import QtCore, QtMultimedia
from gamex.pak import PakFile
from gamex.meta import MetaContent, MetaInfo
from .AppHexWidget import AppHexWidget
from .AppGfxWidget import AppOpenGLWidget, AppPanda3dWidget, AppPygameWidget

# AppTextWidget
class AppTextWidget(QWidget):
    def __init__(self, parent, tab):
        super().__init__()
        self.parent = parent
        self.initUI(tab.value)
    def initUI(self, value: str):
        self.text = QPlainTextEdit(self)
        self.text.setPlainText(value)
        self.text.setFont(QFont('Courier New', 10))
        self.text.setReadOnly(True)
        self.text.setTextInteractionFlags(Qt.TextInteractionFlag.TextSelectableByMouse)
        self.layout = QHBoxLayout()
        self.layout.addWidget(self.text)
        self.setLayout(self.layout)

# AppNullWidget
class AppNullWidget(QWidget):
    def __init__(self, parent, tab):
        super().__init__()

# FileContent
class FileContent(QTabWidget):
    def __init__(self, parent):
        super().__init__()
        self.parent = parent
        self._gfx = []
        self._contentTabs = []
        self.initUI()
    
    # def closeEvent(self, e):
    #     for h in self.openWidgets:
    #         if isinstance(h, ViewHex) and h.tmp_file is not None: os.unlink(h.tmp_file)
    #         h.closeEvent(None)
    #     self.openWidgets = None

    def initUI(self):
        # self.setAttribute(Qt.WidgetAttribute.WA_StyledBackground, True)
        # self.setStyleSheet('background-color: darkgreen;')
        # content tab
        contentTab = self.contentTab = self #QTabWidget(self)
        # contentTab.setMinimumWidth(300)
        # contentTab.setMinimumHeight(300)

    def setPlatform(self, platform: object):
        plat = platform.id if platform else 'UK'
        self.gfxWidget = \
            AppOpenGLWidget if plat == 'GL' else \
            AppPanda3dWidget if plat == 'PD' else \
            AppPygameWidget if plat == 'PG' else \
            AppNullWidget
        if len(self.contentTab) > 0: self.updateTabs()

    def updateTabs(self):
        self.contentTab.clear()
        if not self.contentTabs: return
        for tab in self.contentTabs:
            key = \
                'THex' if tab.type == 'Hex' else \
                'TText' if tab.type == 'Text' else \
                'TDataGrid' if tab.type == 'DataGrid' else \
                'TAudioPlayer' if tab.type == 'AudioPlayer' else \
                'TViewGfx' if tab.type == 'Material' else \
                'TViewGfx' if tab.type == 'Particle' else \
                'TViewGfx' if tab.type == 'TestTri' else \
                'TViewGfx' if tab.type == 'Texture' else \
                'TViewGfx' if tab.type == 'VideoTexture' else \
                'TViewGfx' if tab.type == 'Object' else \
                'TViewGfx' if tab.type == 'World' else \
                'TNull' if tab.type == None else \
                'TText'
            control = \
                AppHexWidget(self, tab) if key == 'THex' else \
                AppTextWidget(self, tab) if key == 'TText' else \
                AppTextWidget(self, tab) if key == 'TDataGrid' else \
                AppTextWidget(self, tab) if key == 'TAudioPlayer' else \
                self.gfxWidget(self, tab) if key == 'TViewGfx' else \
                AppNullWidget(self, tab) if key == 'TNull' else \
                None
            self.contentTab.addTab(control, tab.name)

    @property
    def gfx(self): return self._gfx
    @gfx.setter
    def gfx(self, value): self._gfx = value

    @property
    def contentTabs(self) -> list[MetaContent]: return self._contentTabs
    @contentTabs.setter
    def contentTabs(self, value: list[MetaContent]):
        self._contentTabs = value
        self.updateTabs()

    def onInfo(self, pakFile: PakFile, infos: list[MetaInfo] = None):
        self.gfx = pakFile.gfx
        self.contentTabs = [x.tag for x in infos if isinstance(x.tag, MetaContent)] if infos else None
        self.contentTab.selectedIndex = 0 if infos else -1
