import sys, os, re, traceback
from PyQt6.QtWidgets import QMainWindow, QApplication, QWidget, QProgressBar, QTreeView, QTableView, QTableWidget, QTableWidgetItem, QGridLayout, QHeaderView, QAbstractItemView, QLabel, QComboBox, QTextEdit, QHBoxLayout, QMenu, QFileDialog, QSplitter, QTabWidget
from PyQt6.QtGui import QIcon, QFont, QDrag, QPixmap, QPainter, QColor, QBrush, QAction, QStandardItem, QStandardItemModel
from PyQt6.QtCore import Qt, QObject, QBuffer, QByteArray, QUrl, QMimeData, pyqtSignal, QItemSelectionModel
from PyQt6.QtMultimedia import QMediaPlayer
from PyQt6.QtMultimediaWidgets import QVideoWidget
from PyQt6 import QtCore, QtMultimedia
from gamex import option
from gamex.core.binary import Binary
from gamex.core.meta import FileSource, MetaItem, MetaInfo

# https://doc.qt.io/qt-6/qtreeview.html
# https://gist.github.com/skriticos/5415869

# typedefs
class MetaManager: pass

# MetaItemToViewModel
class MetaItemToViewModel:
    @staticmethod
    def toTreeNodes(model: object, modelMap: dict[object, object], source: list[MetaItem]) -> None:
        if not source: return
        for s in source:
            if not s: continue
            item = QStandardItem(s.icon, s.name) if isinstance(s.icon, QIcon) else QStandardItem(s.name)
            item.setData(s, Qt.ItemDataRole.UserRole)
            modelMap[s] = item
            model.appendRow(item)
            if s.items: MetaItemToViewModel.toTreeNodes(item, modelMap, s.items)

# MetaInfoToViewModel
class MetaInfoToViewModel:
    @staticmethod
    def toTreeNodes(model: object, source: list[MetaInfo]) -> None:
        if not source: return
        for s in source:
            if not s: continue
            item = QStandardItem(s.name)
            item.setData(s, Qt.ItemDataRole.UserRole)
            model.appendRow(item)
            if s.items: MetaInfoToViewModel.toTreeNodes(item, s.items)

# FileExplorer
class FileExplorer(QWidget):
    def __init__(self, parent, tab):
        super().__init__()
        self.parent = parent
        self.resource = parent.resource
        self._nodes = []
        self._infos = []
        self._selectedItem = None
        self.initUI()
        # ready
        self.setArchive(tab.archive)

    def setArchive(self, archive):
        self.archive = archive
        self.filters = archive.getMetaFilters(self.resource)
        self.nodes = self.pakNodes = archive.getMetaItems(self.resource)
        self.ready(archive)

    def initUI(self):
        filterLabel = QLabel(self); filterLabel.setText('File Filter:')
        filterInput = self.filterInput = QComboBox(self)
        filterInput.currentIndexChanged.connect(self.filter_change)

        # nodeModel
        self.nodeModelMap = {}
        nodeModel = self.nodeModel = QStandardItemModel()
        nodeModel.setHorizontalHeaderLabels(['path'])
        
        # nodeView
        nodeView = self.nodeView = QTreeView(self)
        nodeView.setHeaderHidden(True)
        nodeView.setUniformRowHeights(True)
        nodeView.setModel(nodeModel)
        nodeView.selectionModel().selectionChanged.connect(self.node_change)
        
        # infoModel
        infoModel = self.infoModel = QStandardItemModel()
        infoModel.setHorizontalHeaderLabels(['path'])

        # infoView
        infoView = self.infoView = QTreeView(self)
        infoView.setHeaderHidden(True)
        infoView.setUniformRowHeights(True)
        infoView.setModel(infoModel)
        
        # layout
        layout = QGridLayout()
        layout.addWidget(filterLabel, 0, 0)
        layout.addWidget(filterInput, 1, 0)
        layout.addWidget(nodeView, 2, 0); layout.setRowStretch(2, 70)
        layout.addWidget(infoView, 3, 0); layout.setRowStretch(3, 30)
        self.setLayout(layout)

    @property
    def nodes(self): return self._nodes
    @nodes.setter
    def nodes(self, value):
        self._nodes = value
        self.updateNodes()
        
    def updateNodes(self):
        self.nodeModel.clear()
        self.nodeModelMap.clear()
        MetaItemToViewModel.toTreeNodes(self.nodeModel, self.nodeModelMap, self._nodes)

    @property
    def infos(self): return self._infos
    @nodes.setter
    def infos(self, value):
        self._infos = value
        self.infoModel.clear()
        MetaInfoToViewModel.toTreeNodes(self.infoModel, value)
        self.infoView.expandAll()

    @property
    def selectedItem(self): return self._selectedItem
    @nodes.setter
    def selectedItem(self, value):
        if self._selectedItem == value: return
        self._selectedItem = value
        if not value: self.onInfo(); return
        src = value.source.fix() if isinstance(value.source, FileSource) else None
        arc = src.arc if src else None
        try:
            if arc:
                if arc.status == Binary.Stat.Opened: return
                arc.open(value.items, self.resource)
                self.updateNodes()
                self.onFilterKeyUp(None, None)
            self.onInfo(value.archive.getMetaInfos(self.resource, value) if value.archive else None)
        except:
            print(traceback.format_exc())
            self.onInfo([
                MetaInfo(f'EXCEPTION: {sys.exc_info()[1]}'),
                MetaInfo(traceback.format_exc())])

    def setSelectedItem(self, node):
        if not node: return
        self.updateNodes()
        index = self.nodeModel.indexFromItem(self.nodeModelMap[node])
        self.nodeView.selectionModel().select(index, QItemSelectionModel.SelectionFlag.SelectCurrent)

    def filter_change(self, index):
        pass

    def node_change(self, newSelection, oldSelection):
        index = next(iter(newSelection.indexes()), None)
        self.selectedItem = index.data(Qt.ItemDataRole.UserRole)
        
    def onFilterKeyUp(self, a, b):
        pass

    def onInfo(self, infos: list[MetaInfo] = None):
        self.parent.contentBlock.onInfo(self.archive, [s for s in infos if not s.name] if infos else None)
        self.infos = [s for s in infos if s.name] if infos else None

    def ready(self, archive):
        if not option.ForcePath or option.ForcePath.startswith('app:'): return
        sample = archive.game.getSample(option.ForcePath[7:]) if option.ForcePath.startswith('sample:') else None
        paths = sample.paths if sample else [option.ForcePath]
        if not paths: return
        # abc = MetaItem.findByPathForNodes(self.pakNodes, paths, self.resource)
        for path in paths: self.setSelectedItem(MetaItem.findByPathForNodes(self.pakNodes, path, self.resource))
