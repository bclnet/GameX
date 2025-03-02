from __future__ import annotations
import sys, os, re, time, itertools
from enum import Enum, Flag
from io import BytesIO
from openstk.poly import Reader, GenericPool, SinglePool, StaticPool
from gamex.meta import FileSource, MetaManager, MetaItem, MetaInfo
from gamex.util import _throw

# FileOption
class FileOption(Flag):
    Default = 0x0
    Supress = 0x10

# ITransformFileObject
class ITransformFileObject:
    def canTransformFileObject(self, transformTo: PakFile, source: object) -> bool: pass
    def transformFileObject(self, transformTo: PakFile, source: object) -> object: pass

# PakState
class PakState:
    def __init__(self, fileSystem: IFileSystem, game: FamilyGame, edition: Edition = None, path: str = None, tag: object = None):
        self.fileSystem = fileSystem
        self.game = game
        self.edition = edition
        self.path = path or ''
        self.tag = tag

# tag::PakFile[]
class PakFile:
    class FuncObjectFactoryFactory: pass
    class PakStatus(Enum):
        Opening = 1
        Opened = 2
        Closing = 3
        Closed = 4

    def __init__(self, state: PakState):
        z = None
        self.status = self.PakStatus.Closed
        self.fileSystem = state.fileSystem
        self.family = state.game.family
        self.game = state.game
        self.edition = state.edition
        self.pakPath = state.path
        self.name = z if not state.path or (z := os.path.basename(state.path)) else os.path.basename(os.path.dirname(state.path))
        self.tag = state.tag
        self.objectFactoryFunc = None
        self.gfx = None
        self.sfx = None
    def __enter__(self): return self
    def __exit__(self, type, value, traceback): self.close()
    def __repr__(self): return f'{self.name}#{self.game.id}'
    def valid(self) -> bool: return True
    def close(self) -> None:
        self.status = self.PakStatus.Closing
        self.closing()
        self.status = self.PakStatus.Closed
        return self
    def closing(self) -> None: pass
    def open(self, items: list[MetaItem] = None, manager: MetaManager = None) -> None:
        if self.status != self.PakStatus.Closed: return self
        self.status = self.PakStatus.Opening
        start = time.time()
        self.opening()
        end = time.time()
        self.status = self.PakStatus.Opened
        elapsed = round(end - start, 4)
        if items != None:
            for item in self.getMetaItems(manager): items.append(item)
        print(f'Opened: {self.name} @ {elapsed}ms')
        return self
    def opening(self) -> None: pass
    def contains(self, path: FileSource | str | int) -> bool: pass
    def getFileSource(self, path: FileSource | str | int, throwOnError: bool = True) -> (PakFile, FileSource): pass
    def loadFileData(self, path: FileSource | str | int, option: FileOption = FileOption.Default, throwOnError: bool = True) -> bytes: pass
    def loadFileObject(self, path: FileSource | str | int, option: FileOption = FileOption.Default, throwOnError: bool = True) -> object: pass
    def openPakFile(self, res: object, throwOnError: bool = True) -> PakFile:
        raise Exception('TODO')
    #region Transform
    def loadFileObject2(self, transformTo: object, source: object): pass
    def transformFileObject(self, transformTo: object, source: object): pass
    #endregion
    #region Metadata
    def getMetaFilters(self, manager: MetaManager) -> list[MetaItem.Filter]:
        return [MetaItem.Filter(name = k, description = v) for k,v in self.game.filters.items()] \
            if self.game.filters else None
    def getMetaInfos(self, manager: MetaManager, item: MetaItem) -> list[MetaItem]: raise NotImplementedError()
    def getMetaItems(self, manager: MetaManager) -> list[MetaInfo]: raise NotImplementedError()
    #endregion
# end::PakFile[]

# BinaryPakFile
class BinaryPakFile(PakFile):
    def __init__(self, state: PakState, pakBinary: PakBinary):
        super().__init__(state)
        self.pakBinary = pakBinary
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

    def valid(self) -> bool: return self.files != None

    readers: dict[str, GenericPool] = {}
    
    def getReader(self, path: str = None, pooled: bool = True) -> Reader:
        path = path or self.pakPath
        return self.readers.get(path) or self.readers.setdefault(path, GenericPool[Reader](lambda: self.fileSystem.openReader(path), lambda r: r.seek(0)) if self.fileSystem.fileExists(path) else None) if pooled else \
            SinglePool[Reader](self.fileSystem.openReader(path) if self.fileSystem.fileExists(path) else None) 
    
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
                pak, s2 = self._findPath(s)
                return pak.contains(s2) if pak else self.filesByPath and s.replace('\\', '/') in self.filesByPath
            case i if isinstance(path, int):
                return self.filesById and i in self.filesById
            case _: raise Exception(f'Unknown: {path}')

    def getFileSource(self, path: FileSource | str | int, throwOnError: bool = True) -> (PakFile, FileSource):
        match path:
            case None: raise Exception('Null')
            case f if isinstance(path, FileSource): return (self, f)
            case s if isinstance(path, str):
                pak, s2 = self._findPath(s)
                if pak: return pak.getFileSource(s2)
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

    def loadFileData(self, path: FileSource | str | int, option: FileOption = FileOption.Default, throwOnError: bool = True) -> bytes:
        if not path: return None
        elif not isinstance(path, FileSource):
            (p, f2) = self.getFileSource(path, throwOnError)
            return p.loadFileData(f2, option, throwOnError) if p else None
        f = path
        return self.readData(f, option)

    def loadFileObject(self, type: type, path: FileSource | str | int, option: FileOption = FileOption.Default, throwOnError: bool = True) -> object:
        if not path: return None
        elif not isinstance(path, FileSource):
            (p, f2) = self.getFileSource(path, throwOnError)
            return p.loadFileObject(type, f2, option, throwOnError) if p else None
        f = path
        if self.game.isPakFile(f.path): return None
        data = self.loadFileData(f, option, throwOnError)
        if not data: return None
        objectFactory = self._ensureCachedObjectFactory(f)
        if objectFactory != FileSource.emptyObjectFactory:
            r = Reader(data)
            try:
                task = objectFactory(r, f, self)
                if task:
                    value = task
                    return value
            except: print(sys.exc_info()[1]); raise
        return data if type == BytesIO or type == object else \
            _throw(f'Stream not returned for {f.path} with {type}')

    def _ensureCachedObjectFactory(self, file: FileSource) -> callable:
        if not self.objectFactoryFunc: return FileSource.emptyObjectFactory
        if file.cachedObjectFactory: return file.cachedObjectFactory
        option, factory = self.objectFactoryFunc(file, self.game)
        file.cachedObjectOption = option
        file.cachedObjectFactory = factory or FileSource.emptyObjectFactory
        return file.cachedObjectFactory

    def process(self) -> None:
        if self.useFileId and self.files: self.filesById = { x.id:x for x in self.files if x }
        if self.files: self.filesByPath = { k:list(g) for k,g in itertools.groupby(self.files, lambda x: x.path) }
        if self.pakBinary: self.pakBinary.process(self)

    def _findPath(self, path: str) -> (object, str):
        paths = path.split(':', 2)
        p = paths[0].replace('\\', '/')
        first = next(iter(self.filesByPath[p]), None) if self.filesByPath and p in self.filesByPath else None
        pak = first.pak if first else None
        if pak: pak.open()
        return pak, (paths[1] if pak and len(paths) > 1 else None)

    #region PakBinary
    def read(self, tag: object = None) -> None: return \
        self.readerT(lambda r: self.pakBinary.read(self, r, tag)) if self.useReader else \
        self.pakBinary.read(self, None, tag)

    def readData(self, file: FileSource, option: FileOption = None) -> bytes: return \
        self.readerT(lambda r: self.pakBinary.readData(self, r, file, option)) if self.useReader else \
        self.pakBinary.readData(self, None, file, option)
    #endregion

    #region Metadata
    def getMetaInfos(self, manager: MetaManager, item: MetaItem) -> list[MetaInfo]:
        return MetaManager.getMetaInfos(manager, self, item.source if isinstance(item.source, FileSource) else None) if self.valid() else None

    def getMetaItems(self, manager: MetaManager) -> list[MetaItem]:
        return MetaManager.getMetaItems(manager, self) if self.valid() else None
    #endregion

# ManyPakFile
class ManyPakFile(BinaryPakFile):
    def __init__(self, basis: PakFile, state: PakState, name: str, paths: list[str], pathSkip: int = 0):
        super().__init__(state, None)
        self.objectFactoryFunc = basis.objectFactoryFunc
        self.name = name
        self.paths = paths
        self.pathSkip = pathSkip
        self.useReader = False

    #region PakBinary
    def read(self, tag: object = None) -> None:
        self.files = [FileSource(
            path = s.replace('\\', '/'),
            pak = self.game.createPakFileType(PakState(self.fileSystem, self.game, self.edition, s)) if self.game.isPakFile(s) else None,
            fileSize = self.fileSystem.fileInfo(s)[1])
            for s in self.paths]

    def readData(self, file: FileSource, option: FileOption = None) -> BytesIO:
        return file.pak.readData(file, option) if file.pak else \
            BytesIO(self.fileSystem.openReader(file.path).readBytes(file.fileSize))
    #endregion

# MultiPakFile
class MultiPakFile(PakFile):
    def __init__(self, state: PakState, name: str, pakFiles: list[PakFile]):
        super().__init__(state)
        self.name = name
        self.pakFiles = pakFiles or _throw('Empty pakFiles')

    def closing(self):
        for pakFile in self.pakFiles: pakFile.close()

    def opening(self):
        for pakFile in self.pakFiles: pakFile.open()

    def contains(path: object) -> bool:
        match path:
            case None: raise Exception('Null')
            case s if isinstance(path, str):
                paks, s2 = self._findPakFiles(s)
                return any(x.valid() and x.contains(s2) for x in paks)
            case i if isinstance(path, int): return any(x.valid() and x.contains(i) in self.pakFiles)
            case _: raise Exception(f'Unknown: {path}')

    def _findPakFiles(self, path: str) -> (list[PakFile], str):
        paths = re.split('\\\\|/|:', path, 1)
        if len(paths) == 1: return self.pakFiles, path
        path, nextPath = paths
        pakFiles = [x for x in self.pakFiles if x.name.startswith(path)]
        for pakFile in pakFiles: pakFile.open()
        return pakFiles, nextPath

    def getFileSource(self, path: FileSource | str | int, throwOnError: bool = True) -> (PakFile, FileSource):
        match path:
            case None: raise Exception('Null')
            case s if isinstance(path, str):
                pakFiles, s2 = self._findPakFiles(s)
                value = next(iter([x for x in pakFiles if x.valid() and x.contains(s2)]), None)
                if not value: raise Exception(f'Could not find file {path}')
                return value.getFileSource(s2, throwOnError)
            case i if isinstance(path, int):
                value = next(iter([x for x in self.pakFiles if x.valid() and x.contains(i)]), None)
                if not value: raise Exception(f'Could not find file {path}')
                return value.getFileSource(i, throwOnError)
            case _: raise Exception(f'Unknown: {path}')

    def loadFileData(self, path: FileSource | str | int, option: FileOption = FileOption.Default) -> bytes:
        match path:
            case None: raise Exception('Null')
            case s if isinstance(path, str):
                pakFiles, s2 = self._findPakFiles(s)
                value = next(iter([x for x in pakFiles if x.valid() and x.contains(s2)]), None)
                if not value: raise Exception(f'Could not find file {path}')
                return value.loadFileData(s2, option)
            case i if isinstance(path, int):
                value = next(iter([x for x in self.pakFiles if x.valid() and x.contains(i)]), None)
                if not value: raise Exception(f'Could not find file {path}')
                return value.loadFileData(i, option)
            case _: raise Exception(f'Unknown: {path}')

    def loadFileObject(self, type: type, path: FileSource | str | int) -> object:
        match path:
            case None: raise Exception('Null')
            case s if isinstance(path, str):
                pakFiles, s2 = self._findPakFiles(s)
                value = next(iter([x for x in pakFiles if x.valid() and x.contains(s2)]), None)
                if not value: raise Exception(f'Could not find file {path}')
                return value.loadFileObject(type, s2, option)
            case i if isinstance(path, int):
                value = next(iter([x for x in self.pakFiles if x.valid() and x.contains(i)]), None)
                if not value: raise Exception(f'Could not find file {path}')
                return value.loadFileObject(type, i, option)
            case _: raise Exception(f'Unknown: {path}')

    #region Metadata
    def getMetaItems(self, manager: MetaManager) -> list[MetaInfo]:
        root = []
        for pakFile in [x for x in self.pakFiles if x.valid()]:
            root.append(MetaItem(pakFile, pakFile.name, manager.packageIcon, pakFile = pakFile, items = pakFile.getMetaItems(manager)))
        return root
    #endregion

# tag::PakBinary[]
# PakBinary
class PakBinary:
    def read(self, source: BinaryPakFile, r: Reader, tag: object = None) -> None: pass
    def readData(self, source: BinaryPakFile, r: Reader, file: FileSource, option: FileOption = None): pass
    def process(self, source: BinaryPakFile): pass
    def handleException(self, source: object, option: FileOption, message: str):
        print(message)
        if (option & FileOption.Supress) != 0: raise Exception(message)

# PakBinaryT
class PakBinaryT(PakBinary):
    _instance = None
    def __new__(cls):
        if cls._instance is None: cls._instance = super().__new__(cls)
        return cls._instance

    class SubPakFile(BinaryPakFile):
        def __init__(self, parent: PakBinary, state: PakState, file: FileSource, source: BinaryPakFile):
            super().__init__(state, parent._instance)
            self.file = file
            self.source = source
            self.objectFactoryFunc = source.objectFactoryFunc
            # self.open()

        def opening(self) -> None: self.r = Reader(self.source.readData(file)); self.pool = StaticPool[Reader](self.r); super().opening()
        def closing(self) -> None: self.r.__exit__(); super().closing()
        def getReader(path: str, pooled: bool) -> IGenericPool[Reader]: return self.pool

        # def read(self, r: Reader, tag: object = None):
        #     if self.useReader: super().read(r, tag); return
        #     with Reader(self.readData(self.source.getReader(), self.file)) as r2:
        #         self.pakBinary.read(self, r2, tag)
# end::PakBinary[]
