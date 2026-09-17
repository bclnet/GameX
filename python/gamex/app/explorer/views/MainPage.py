import sys, os
from typing import Any
from PyQt6.QtWidgets import QMainWindow, QApplication, QWidget, QProgressBar, QScrollArea, QTableView, QTableWidget, QTableWidgetItem, QGridLayout, QHeaderView, QAbstractItemView, QLabel, QComboBox, QTextEdit, QVBoxLayout, QHBoxLayout, QMenu, QFileDialog, QSplitter, QTabWidget
from PyQt6.QtGui import QIcon, QFont, QDrag, QPixmap, QPainter, QColor, QBrush, QAction
from PyQt6.QtCore import Qt, QBuffer, QByteArray, QUrl, QMimeData, QPoint, pyqtSignal
from PyQt6.QtMultimedia import QMediaPlayer
from PyQt6.QtMultimediaWidgets import QVideoWidget
from PyQt6 import QtCore, QtMultimedia
from gamex import EngineX, Family, option
from gamex.core.util import _find
from .SaveFileWidget import SaveFileWidget
from .OpenWidget import OpenWidget
from .FileContent import FileContent
from .FileExplorer import FileExplorer
from ..resourcemgr import ResourceManager

engineValues = sorted([s for s in EngineX.engines if s and s.enabled], key=lambda s: s.name)
engineIndex = max(_find([s.id for s in engineValues], option.Engine), 0)

# ExplorerMainTab
class ExplorerMainTab:
    def __init__(self, name: str=None, archive: Any=None, appList: list[Any]=None, text: str=None):
        self.name = name
        self.archive = archive
        self.appList = appList
        self.text = text

# TextBlock
class TextBlock(QWidget):
    def __init__(self, parent, tab):
        super().__init__()
        mainWidget = QScrollArea(self)
        mainWidget.setStyleSheet('border:0px;')
        label = QLabel(mainWidget)
        label.setText(tab.text)
        label.setWordWrap(True)
        label.setTextInteractionFlags(Qt.TextInteractionFlag.TextSelectableByMouse)

# LogBar
class LogBar(QLabel):
    def __init__(self, parent):
        super().__init__(parent)

    def contextMenuEvent(self, e):
        context = QMenu(self)
        clearAction = QAction('Clear', self)
        clearAction.triggered.connect(lambda:self.setText(''))
        quitAction = QAction('Quit', self)
        quitAction.triggered.connect(lambda:exit(0))
        context.addAction(clearAction)
        context.addAction(quitAction)
        context.exec(e.globalPos())

# AppList
class AppList(QWidget):
    def __init__(self, parent, tab):
        super().__init__()

# MainPage
class MainPage(QMainWindow):
    def __init__(self, parent=None):
        super().__init__(parent)
        self.resource = ResourceManager()
        self.title = 'Explorer'
        self.width = 800
        self.height = 600
        self.archives = []
        self.openWidgets = []
        self.mainTabs = []
        self.initUI()

    def closeWidget(self, w):
        if w in self.openWidgets: self.openWidgets.remove(w)
        w.deleteLater()

    def closeEvent(self, e):
        for h in self.openWidgets: h.closeEvent(None)
        self.openWidgets = None
        if os.path.exists('tmp') and len(os.listdir('tmp')) == 0: os.rmdir('tmp')

    def initUI(self):
        self.setWindowTitle(self.title)
        self.resize(self.width, self.height)
        
        # main tab
        mainTab = self.mainTab = QTabWidget(self)
        # mainTab.setMinimumWidth(300) # remove
        mainTab.setMaximumWidth(500)
        mainTab.setMaximumWidth(300) # remove
        self.updateTabs()

        # contentBlock
        contentBlock = self.contentBlock = FileContent(self)
        contentBlock.setContentsMargins(50, 50, 50, 50)
        # contentBlock.setAttribute(Qt.WidgetAttribute.WA_StyledBackground, True)
        # contentBlock.setStyleSheet('background-color: darkgreen;')

        # splitter
        splitter = QSplitter(self)
        splitter.addWidget(mainTab)
        splitter.addWidget(contentBlock)

        # logBar
        logBar = self.logBar = LogBar(self)
        logBar.setAlignment(Qt.AlignmentFlag.AlignTop)
        logBar.setAttribute(Qt.WidgetAttribute.WA_StyledBackground, True)
        logBar.setStyleSheet('background-color: lightgray;')
        engineInput = self.engineInput = QComboBox(logBar)
        for x in engineValues: engineInput.addItem(x.name, x)
        engineInput.currentIndexChanged.connect(self.engine_change)
        engineInput.setCurrentIndex(engineIndex); self.setEngine(engineInput.itemData(engineInput.currentIndex()))
        p = QPoint(300, 0)
        engineInput.move(p)

        # add to layout
        mainWidget = self.mainWidget = QWidget(self)
        mainWidgetLayout = QVBoxLayout(mainWidget)
        mainWidgetLayout.addWidget(splitter, 9)
        mainWidgetLayout.addWidget(logBar, 1)
        mainWidget.setLayout(mainWidgetLayout)
        self.setCentralWidget(mainWidget)

        # mainMenu
        mainMenu = self.menuBar()
        fileMenu = mainMenu.addMenu('&File')
        fileMenu.addAction('&Open', self.openPage_click)
        self.show()

    def setEngine(self, engine):
        EngineX.activate(engine)
        self.contentBlock.setEngine(engine)

    def engine_change(self, index):
        selected = self.platformSelected = engineValues[index] if index >= 0 else None
        self.setEngine(selected)

    def startup(self):
        if option.ForcePath and option.ForcePath.startswith('app:') and self.familyApps and option.ForcePath[:4] in self.familyApps:
            app = self.familyApps[option.ForcePath[:4]]
        self.openPage_click()

    def updateTabs(self):
        self.mainTab.clear()
        for tab in self.mainTabs:
            control = FileExplorer(self, tab) if tab.archive else \
                AppList(self, tab) if tab.appList else \
                TextBlock(self, tab)
            self.mainTab.addTab(control, tab.name)

    def openPage_click(self):
        w = OpenWidget(self, lambda s: self.open(s.familySelected, OpenWidget.pakUris.__get__(s)))
        self.openWidgets.append(w)
        w.loaded()

    def log(self, value):
        logBar = self.logBar
        text = logBar.text()
        logBar.setText(text + value + '\n')

    def open(self, family: Family, uris: list[str], path: str = None):
        self.archives.clear()
        if not family: return
        self.familyApps = family.apps
        for s in uris:
            self.log(f'Opening {s}')
            arc = family.getArchive(s)
            if arc: self.archives.append(arc)
        self.log('Done')
        self.onOpened(family, path)

    def onOpened(self, family, path):
        tabs = [ExplorerMainTab(
            name = archive.name,
            archive = archive
        ) for archive in self.archives]
        if family.description:
            tabs.append(ExplorerMainTab(
                name = 'Information',
                text = family.description
            ))
        self.mainTabs = tabs
        self.updateTabs()
