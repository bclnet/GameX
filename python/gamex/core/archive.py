from __future__ import annotations
import sys, os, re, time, itertools
from enum import Enum, Flag
from io import BytesIO
from openstk import _throw, Reader, GenericPool, SinglePool, StaticPool
from gamex.core.meta import FileSource, MetaManager, MetaItem, MetaInfo

# FileOption
class FileOption(Flag):
    Default = 0x0
    Raw = 0x1
    Marker = 0x2
    Object = 0x4
    BinaryObject = Object | 0x8
    StreamObject = Object | 0x10
    # Supress = 0x10
    UnknownFileModel = 0x100

# ITransformAsset
class ITransformAsset:
    def canTransformAsset(self, transformTo: Archive, source: object) -> bool: pass
    def transformAsset(self, transformTo: Archive, source: object) -> object: pass

# ArcState
class ArcState:
    def __init__(self, vfx: FileSystem, game: FamilyGame, edition: Edition = None, path: str = None, tag: object = None):
        self.vfx = vfx
        self.game = game
        self.edition = edition
        self.path = path or ''
        self.tag = tag

# tag::Archive[]
class Archive:
    class FuncObjectFactoryFactory: pass
    class ArcStatus(Enum):
        Opening = 1
        Opened = 2
        Closing = 3
        Closed = 4

    def __init__(self, state: ArcState):
        z = None
        self.status = self.ArcStatus.Closed
        self.vfx = state.vfx
        self.family = state.game.family
        self.game = state.game
        self.edition = state.edition
        self.arcPath = state.path
        self.name = z if not state.path or (z := os.path.basename(state.path)) else os.path.basename(os.path.dirname(state.path))
        self.tag = state.tag
        self.assetFactoryFunc = None
        self.gfx = None
        self.sfx = None
    def __enter__(self): return self
    def __exit__(self, type, value, traceback): self.close()
    def __repr__(self): return f'{self.name}#{self.game.id}'
    def valid(self) -> bool: return True
    def close(self) -> None:
        self.status = self.ArcStatus.Closing
        self.closing()
        self.status = self.ArcStatus.Closed
        return self
    def closing(self) -> None: pass
    def open(self, items: list[MetaItem] = None, manager: MetaManager = None) -> None:
        if self.status != self.ArcStatus.Closed: return self
        self.status = self.ArcStatus.Opening
        start = time.time()
        self.opening()
        end = time.time()
        self.status = self.ArcStatus.Opened
        elapsed = round(end - start, 4)
        if items != None:
            for item in self.getMetaItems(manager): items.append(item)
        print(f'Opened[{self.game.id}]: {self.name} @ {elapsed}ms')
        return self
    def opening(self) -> None: pass
    def setPlatform(self, platform: Platform) -> Archive:
        self.gfx = platform.gfxFactory(self) if platform and platform.gfxFactory else None
        self.sfx = platform.sfxFactory(self) if platform and platform.sfxFactory else None
        return self
    def contains(self, path: FileSource | str | int) -> bool: pass
    def getSource(self, path: FileSource | str | int, throwOnError: bool = True) -> (Archive, FileSource): pass
    def getData(self, path: FileSource | str | int, option: object = None, throwOnError: bool = True) -> bytes: pass
    def getAsset(self, path: FileSource | str | int, option: object = None, throwOnError: bool = True) -> object: pass
    def openArchive(self, res: object, throwOnError: bool = True) -> Archive:
        raise Exception('TODO')
    #region Transform
    def loadAsset2(self, transformTo: object, source: object): pass
    def transformAsset(self, transformTo: object, source: object): pass
    #endregion
    #region Metadata
    def getMetaFilters(self, manager: MetaManager) -> list[MetaItem.Filter]:
        return [MetaItem.Filter(name = k, description = v) for k,v in self.game.filters.items()] \
            if self.game.filters else None
    def getMetaInfos(self, manager: MetaManager, item: MetaItem) -> list[MetaItem]: raise NotImplementedError()
    def getMetaItems(self, manager: MetaManager) -> list[MetaInfo]: raise NotImplementedError()
    #endregion
# end::Archive[]

# BinaryArchive
class BinaryArchive(Archive):
    def __init__(self, state: ArcState, arcBinary: ArcBinary):
        super().__init__(state)
        self.arcBinary = arcBinary
        # options
        self.retainInPool = 10
        self.useReader = True
        self.useFileId = False
        # state
        self.fileMask = None
        self.params = {}
        self.magic = None
        self.version = None
        # metadata/factory
        self.metadataInfos = {}
        # binary
        self.files = None
        self.filesById = None
        self.filesByPath = None
        self.pathSkip = 0
        self.atEnd = False

    def valid(self) -> bool: return self.files != None

    readers: dict[str, GenericPool] = {}
    
    def getReader(self, path: str = None, pooled: bool = True) -> Reader:
        path = path or self.arcPath
        return self.readers.get(path) or self.readers.setdefault(path, GenericPool[Reader](lambda: Reader(self.vfx.open(path)), lambda r: r.seek(0)) if self.vfx.fileExists(path) else None) if pooled else \
            SinglePool[Reader](Reader(self.vfx.open(path)) if self.vfx.fileExists(path) else None)
    
    def reader(self, func: callable, path: str = None, pooled: bool = False): self.getReader(path, pooled).action(func)

    def readerT(self, func: callable, path: str = None, pooled: bool = False): return self.getReader(path, pooled).func(func)

    def opening(self) -> None:
        self.read()
        self.process()

    def closing(self) -> None: pass

    def contains(self, path: FileSource | str | int) -> bool:
        match path:
            case None: raise Exception('Null')
            case s if isinstance(path, str):
                arc, next_ = self._findPath(s)
                return arc.contains(next_) if arc else self.filesByPath and s.replace('\\', '/') in self.filesByPath
            case i if isinstance(path, int):
                return self.filesById and i in self.filesById
            case _: raise Exception(f'Unknown: {path}')

    def getSource(self, path: FileSource | str | int, throwOnError: bool = True) -> (Archive, FileSource):
        match path:
            case None: raise Exception('Null')
            case f if isinstance(path, FileSource): return (self, f)
            case s if isinstance(path, str):
                arc, next_ = self._findPath(s)
                if arc: return arc.getSource(next_) if next_ else (arc, next_)
                files = self.filesByPath[s] if self.filesByPath and (s := s.replace('\\', '/')) in self.filesByPath else []
                if len(files) == 1: return (self, files[0])
                print(f'ERROR.LoadFileData: {s} @ {len(files)}')
                if throwOnError: raise Exception(f'File not found: {s}' if len(files) == 0 else f'More then one file found: {s}')
                return (None, None)
            case i if isinstance(path, int):
                files = self.filesById[i] if self.filesById and i in self.filesById else []
                if len(files) == 1: return (self, files[0])
                print(f'ERROR.LoadFileData: {i} @ {len(files)}')
                if throwOnError: raise Exception(f'File not found: {i}' if len(files) == 0 else f'More then one file found: {i}')
                return (None, None)
            case _: raise Exception(f'Unknown: {path}')

    def getData(self, path: FileSource | str | int, option: object = None, throwOnError: bool = True) -> bytes:
        if not path: return None
        elif not isinstance(path, FileSource):
            (p, next_) = self.getSource(path, throwOnError)
            return p.getData(next_, option, throwOnError) if p else None
        f = path
        return self.readData(f.fix(), option)

    def getAsset(self, type: type, path: FileSource | str | int, option: object = None, throwOnError: bool = True) -> object:
        if not path: return None
        elif not isinstance(path, FileSource):
            (p, next_) = self.getSource(path, throwOnError)
            return p.getAsset(type, next_, option, throwOnError) if p else None
        f = path
        if self.game.isArcPath(f.path): return None
        data = self.getData(f, option, throwOnError)
        if not data: return None
        assetFactory = self.ensureCachedObjectFactory(f)
        if assetFactory != FileSource.emptyObjectFactory:
            r = Reader(data)
            try:
                task = assetFactory(r, f, self)
                if task: return (value := task)
            except: print(sys.exc_info()[1]); raise
            finally:
                self.atEnd = r.atEnd()
                # if task and not (value and isinstance(value, IDisposable)): r.dispose()
        return data if type == BytesIO or type == object else \
            _throw(f'Stream not returned for {f.path} with {type}')

    def ensureCachedObjectFactory(self, file: FileSource) -> callable:
        if not self.assetFactoryFunc: return FileSource.emptyObjectFactory
        if file.cachedObjectFactory: return file.cachedObjectFactory
        option, factory = self.assetFactoryFunc(file, self.game)
        file.cachedObjectOption = option
        file.cachedObjectFactory = factory or FileSource.emptyObjectFactory
        return file.cachedObjectFactory

    def process(self) -> None:
        if self.useFileId and self.files: self.filesById = { x.id:x for x in self.files if x }
        if self.files: self.filesByPath = { k:list(g) for k,g in itertools.groupby(self.files, lambda x: x.path) }
        if self.arcBinary: self.arcBinary.process(self)

    def _findPath(self, path: str) -> (object, str):
        paths = path.split(':', 1)
        p = paths[0].replace('\\', '/')
        first = next(iter(self.filesByPath[p]), None) if self.filesByPath and p in self.filesByPath else None
        arc = first.arc if first else None
        if arc: arc.open()
        return arc, (paths[1] if arc and len(paths) > 1 else None)

    #region ArcBinary
    def read(self, tag: object = None) -> None: return \
        self.readerT(lambda r: self.arcBinary.read(self, r, tag)) if self.useReader else \
        self.arcBinary.read(self, None, tag)

    def readData(self, file: FileSource, option: object = None) -> bytes: return \
        self.readerT(lambda r: self.arcBinary.readData(self, r, file, option)) if self.useReader else \
        self.arcBinary.readData(self, None, file, option)
    #endregion

    #region Metadata
    def getMetaInfos(self, manager: MetaManager, item: MetaItem) -> list[MetaInfo]:
        return MetaManager.getMetaInfos(manager, self, item.source if isinstance(item.source, FileSource) else None) if self.valid() else None

    def getMetaItems(self, manager: MetaManager) -> list[MetaItem]:
        return MetaManager.getMetaItems(manager, self) if self.valid() else None
    #endregion

# ManyArchive
class ManyArchive(BinaryArchive):
    def __init__(self, basis: Archive, state: ArcState, name: str, paths: list[str], pathSkip: int = 0):
        super().__init__(state, None)
        self.assetFactoryFunc = basis.assetFactoryFunc
        self.name = name
        self.paths = paths
        self.pathSkip = pathSkip
        self.useReader = False

    #region ArcBinary
    def read(self, tag: object = None) -> None:
        def lambdax(x, s): x.fileSize = self.vfx.fileInfo(s)[1]; x.lazy = None
        self.files = [FileSource(
            path = s.replace('\\', '/'),
            arc = self.game.createArchiveType(ArcState(self.vfx, self.game, self.edition, s)) if self.game.isArcPath(s) else None,
            fileSize = 0,
            lazy = lambda x, _s=s: lambdax(x, _s))
            for s in self.paths]

    def readData(self, file: FileSource, option: object = None) -> BytesIO:
        return file.arc.readData(file, option) if file.arc else \
            BytesIO(Reader(self.vfx.open(file.path)).readBytes(file.fileSize))
    #endregion

# MultiArchive
class MultiArchive(Archive):
    def __init__(self, state: ArcState, name: str, archives: list[Archive]):
        super().__init__(state)
        self.name = name
        self.archives = archives or _throw('Empty archives')

    def closing(self):
        for archive in self.archives: archive.close()

    def opening(self):
        for archive in self.archives: archive.open()

    def contains(path: object) -> bool:
        match path:
            case None: raise Exception('Null')
            case s if isinstance(path, str):
                arcs, next_ = self._findArchives(s)
                return any(x.valid() and x.contains(next_) for x in arcs)
            case i if isinstance(path, int): return any(x.valid() and x.contains(i) in self.archives)
            case _: raise Exception(f'Unknown: {path}')

    def _findArchives(self, path: str) -> (list[Archive], str):
        paths = re.split('\\\\|/|:', path, 1)
        if len(paths) == 1: return self.archives, path
        path, nextPath = paths
        archives = [x for x in self.archives if x.name.startswith(path)]
        for archive in archives: archive.open()
        return archives, nextPath

    def getSource(self, path: FileSource | str | int, throwOnError: bool = True) -> (Archive, FileSource):
        match path:
            case None: raise Exception('Null')
            case s if isinstance(path, str):
                archives, next_ = self._findArchives(s)
                value = next(iter([x for x in archives if x.valid() and x.contains(next_)]), None)
                if not value: raise Exception(f'Could not find file {path}')
                return value.getSource(next_, throwOnError)
            case i if isinstance(path, int):
                value = next(iter([x for x in self.archives if x.valid() and x.contains(i)]), None)
                if not value: raise Exception(f'Could not find file {path}')
                return value.getSource(i, throwOnError)
            case _: raise Exception(f'Unknown: {path}')

    def getData(self, path: FileSource | str | int, option: object = None) -> bytes:
        match path:
            case None: raise Exception('Null')
            case s if isinstance(path, str):
                archives, next_ = self._findArchives(s)
                value = next(iter([x for x in archives if x.valid() and x.contains(next_)]), None)
                if not value: raise Exception(f'Could not find file {path}')
                return value.getData(next_, option)
            case i if isinstance(path, int):
                value = next(iter([x for x in self.archives if x.valid() and x.contains(i)]), None)
                if not value: raise Exception(f'Could not find file {path}')
                return value.getData(i, option)
            case _: raise Exception(f'Unknown: {path}')

    def getAsset(self, type: type, path: FileSource | str | int) -> object:
        match path:
            case None: raise Exception('Null')
            case s if isinstance(path, str):
                archives, next_ = self._findArchives(s)
                value = next(iter([x for x in archives if x.valid() and x.contains(next_)]), None)
                if not value: raise Exception(f'Could not find file {path}')
                return value.getAsset(type, next_, option)
            case i if isinstance(path, int):
                value = next(iter([x for x in self.archives if x.valid() and x.contains(i)]), None)
                if not value: raise Exception(f'Could not find file {path}')
                return value.getAsset(type, i, option)
            case _: raise Exception(f'Unknown: {path}')

    #region Metadata
    def getMetaItems(self, manager: MetaManager) -> list[MetaInfo]:
        root = []
        for archive in [x for x in self.archives if x.valid()]:
            root.append(MetaItem(archive, archive.name, manager.packageIcon, archive = archive, items = archive.getMetaItems(manager)))
        return root
    #endregion

# tag::ArcBinary[]
# ArcBinary
class ArcBinary:
    def read(self, source: BinaryArchive, r: Reader, tag: object = None) -> None: pass
    def readData(self, source: BinaryArchive, r: Reader, file: FileSource, option: object = None): pass
    def process(self, source: BinaryArchive): pass
    def handleException(self, source: object, option: object, message: str):
        print(message)
        # if (option & FileOption.Supress) != 0: raise Exception(message)

# ArcBinaryT
class ArcBinaryT(ArcBinary):
    _instance = None
    def __new__(cls):
        if cls._instance is None: cls._instance = super().__new__(cls)
        return cls._instance

    class SubArchive(BinaryArchive):
        def __init__(self, parent: ArcBinary, state: ArcState, file: FileSource, source: BinaryArchive):
            super().__init__(state, parent._instance)
            self.file = file
            self.source = source
            self.assetFactoryFunc = source.assetFactoryFunc
            # self.open()

        def opening(self) -> None: self.r = Reader(self.source.readData(file)); self.pool = StaticPool[Reader](self.r); super().opening()
        def closing(self) -> None: self.r.__exit__(); super().closing()
        def getReader(path: str, pooled: bool) -> IGenericPool[Reader]: return self.pool

        # def read(self, r: Reader, tag: object = None):
        #     if self.useReader: super().read(r, tag); return
        #     with Reader(self.readData(self.source.getReader(), self.file)) as r2:
        #         self.arcBinary.read(self, r2, tag)
# end::ArcBinary[]
