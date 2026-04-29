from __future__ import annotations
import sys, os, re, time
from itertools import groupby
from enum import Enum, Flag
from io import BytesIO
from openstk.core import _throw, ISourceWithPlatform, BinaryReader, GenericPool, SinglePool, StaticPool, IDatabase
from gamex.core.meta import FileSource, MetaManager, MetaItem, MetaInfo

# FileOption
class FileOption(Flag):
    Default = 0x0
    Raw = 0x1
    Marker = 0x2
    Object = 0x4
    BinaryObject = Object | 0x8
    StreamObject = Object | 0x10
    UnknownFileModel = 0x100
    Hosting = Raw | Marker

# BinaryState
class BinaryState:
    def __init__(self, vfx: FileSystem, game: FamilyGame, edition: Edition = None, path: str = None, tag: object = None):
        self.vfx = vfx
        self.game = game
        self.edition = edition
        self.path = path or ''
        self.tag = tag

# tag::Binary[]
class Binary:
    class Stat(Enum): Opening = 0; Opened = 1; Closing = 2; Closed = 3
    def __init__(self, state: BinaryState):
        z = None
        self.status = self.Stat.Closed
        self.vfx = state.vfx
        self.family = state.game.family
        self.game = state.game
        self.edition = state.edition
        self.binPath = state.path
        self.name = z if not state.path or (z := os.path.basename(state.path)) else os.path.basename(os.path.dirname(state.path))
        self.tag = state.tag
        self.valid = True
    def __enter__(self): return self
    def __exit__(self, type, value, traceback): self.close()
    def __repr__(self): return f'{self.name}#{self.game.id}'
    def close(self) -> None:
        self.status = self.Stat.Closing
        self.closing()
        self.status = self.Stat.Closed
        return self
    def closing(self) -> None: pass
    def open(self) -> None:
        if self.status != self.Stat.Closed: return self
        self.status = self.Stat.Opening
        start = time.time()
        self.opening()
        end = time.time()
        self.status = self.Stat.Opened
        elapsed = round(end - start, 4)
        print(f'Opened[{self.game.id}]: {self.name} @ {elapsed}ms')
        return self
    def opening(self) -> None: pass
# end::Binary[]

# tag::Archive[]
class Archive(Binary, ISourceWithPlatform):
    # class FuncObjectFactoryFactory: pass
    def __init__(self, state: BinaryState):
        super().__init__(state)
        self.pathFinders = {}
        self.assetFactoryFunc = None
        self.gfx = None
        self.sfx = None
        self.count = 0
    def open(self, items: list[MetaItem] = None, manager: MetaManager = None) -> None:
        if self.status != self.Stat.Closed: return self
        super().open()
        if items != None: items.extend(self.getMetaItems(manager))
        return self
    def contains(self, path: FileSource | str | int) -> bool: pass
    def findPath(self, t: type, path: object) -> object:
        if len(self.pathFinders) != 1: z = self.pathFinders.get(t); return z(path) if z else path
        first = next(iter(self.pathFinders.items()), None)
        return first[1](path) if first[0] == t or first[0] == None else path
    def setPlatform(self, platform: Platform) -> Archive:
        self.gfx = platform.gfxFactory(self) if platform and platform.gfxFactory else None
        self.sfx = platform.sfxFactory(self) if platform and platform.sfxFactory else None
        return self
    def getSource(self, path: FileSource | str | int, throwOnError: bool = True) -> tuple[Archive, FileSource]: pass
    async def getData(self, path: FileSource | str | int, option: object = None, throwOnError: bool = True) -> bytes: pass
    async def getAsset(self, path: FileSource | str | int, option: object = None, throwOnError: bool = True) -> object: pass
    def getArchive(self, path: object, throwOnError: bool = True) -> Archive:
        match path:
            case str(): z = self.game._getArchive(self.vfx, self.edition, path, throwOnError); return z.open() if z else None
            case _: raise Exception('path')
    #region Transform
    async def getAsset2(self, path: object, transformTo: object): await self.transformAsset(await self.getAsset(path), transformTo)
    def _transformAsset(self, path: object, transformTo: object): raise NotImplementedError()
    #endregion
    #region Metadata
    def getMetaFilters(self, manager: MetaManager) -> list[MetaItem.Filter]: return [MetaItem.Filter(name = k, description = v) for k,v in self.game.filters.items()] if self.game.filters else None
    async def getMetaInfos(self, manager: MetaManager, item: MetaItem) -> list[MetaItem]: raise NotImplementedError('getMetaInfos')
    def getMetaItems(self, manager: MetaManager) -> list[MetaInfo]: raise NotImplementedError('getMetaItems')
    #endregion
# end::Archive[]

# BinaryArchive
class BinaryArchive(Archive):
    def __init__(self, state: BinaryState, arcBinary: ArcBinary):
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
        self.metaInfos = {}
        # binary
        self.files = None
        self.filesRawSet = None
        self.filesById = None
        self.filesByPath = None
        self.pathSkip = 0
        self.atEnd = False
        # pool
        self.readers: dict[str, GenericPool] = {}
    
    #region Pool

    def getReader(self, path: str = None, pooled: bool = True) -> BinaryReader:
        path = path or self.binPath
        return self.readers.get(path) or self.readers.setdefault(path, GenericPool[BinaryReader](lambda: BinaryReader(self.vfx.open(path)), lambda r: r.seek(0)) if self.vfx.fileExists(path) else None) if pooled else \
            SinglePool[BinaryReader](BinaryReader(self.vfx.open(path)) if self.vfx.fileExists(path) else None)
    
    def reader(self, func: callable, path: str = None, pooled: bool = False): self.getReader(path, pooled).action(func)

    def readerT(self, func: callable, path: str = None, pooled: bool = False): return self.getReader(path, pooled).func(func)
    
    #endregion

    def opening(self) -> None:
        self.read()
        self.process()
        self.valid = self.files != None
        self.count = len(self.filesByPath)

    def closing(self) -> None:
        self.valid = False
        self.count = 0
        self.files = None
        self.filesRawSet = None
        self.filesById = None
        self.filesByPath = None
        # for r in self.readers.values(): r.dispose()
        self.readers.clear()

    def contains(self, path: FileSource | str | int) -> bool:
        match path:
            case None: raise Exception('Null')
            case s if isinstance(path, str): arc, next_ = self._findPath(s); return arc.contains(next_) if arc else self.filesByPath and s.replace('\\', '/').lower() in self.filesByPath
            case i if isinstance(path, int): return self.filesById and i in self.filesById
            case _: raise Exception(f'Unknown: {path}')

    def getSource(self, path: FileSource | str | int, throwOnError: bool = True) -> tuple[Archive, FileSource]:
        match path:
            case None: raise Exception('Null')
            case f if isinstance(path, FileSource): return (self, f)
            case s if isinstance(path, str):
                arc, next_ = self._findPath(s)
                if arc: return arc.getSource(next_) if next_ else (arc, next_)
                files = self.filesByPath[s] if self.filesByPath and (s := s.replace('\\', '/').lower()) in self.filesByPath else []
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

    async def getData(self, path: FileSource | str | int, option: object = None, throwOnError: bool = True) -> bytes:
        if not path: return None
        elif not isinstance(path, FileSource):
            (arc, next_) = self.getSource(path, throwOnError)
            return arc.getData(next_, option, throwOnError) if arc else None
        f = path
        return self.readData(f.fix(), option)

    async def getAsset(self, type: type, path: FileSource | str | int, option: object = None, throwOnError: bool = True) -> object:
        if not path: return None
        elif not isinstance(path, FileSource):
            (arc, next_) = self.getSource(path, throwOnError)
            return await arc.getAsset(type, next_, option, throwOnError)
        f = path
        if self.game._isArcPath(f.path): return None
        if isinstance(self.arcBinary, IDatabase) and (s := self.arcBinary):
            res = s.query(f)
            if res: return res
        data = await self.getData(f, option, throwOnError)
        if not data: return None
        assetFactory = self.ensureCachedAssetFactory(f)
        if assetFactory != FileSource.emptyAssetFactory:
            r = BinaryReader(data)
            try:
                task = assetFactory(r, f, self)
                if task:
                    value = await task
                    return value
            except: print(sys.exc_info()[1]); raise
            finally:
                self.atEnd = r.atEnd()
                # if task and not (value and isinstance(value, IDisposable)): r.dispose()
        return data if type == BytesIO or type == object else \
            _throw(f'Stream not returned for {f.path} with {type}')

    def ensureCachedAssetFactory(self, file: FileSource) -> callable:
        if not self.assetFactoryFunc: return FileSource.emptyAssetFactory
        if file.cachedObjectFactory: return file.cachedObjectFactory
        option, factory = self.assetFactoryFunc(file, self.game)
        file.cachedObjectOption = option
        file.cachedObjectFactory = factory or FileSource.emptyAssetsFactory
        return file.cachedObjectFactory

    def process(self) -> None:
        if self.useFileId and self.files: self.filesById = { x.id:x for x in self.files if x }
        if self.files: self.filesByPath = { k.lower():list(g) for k,g in groupby(sorted(self.files, key=lambda s: s.path), lambda s: s.path) }
        if self.arcBinary: self.arcBinary.process(self)

    def _findPath(self, path: str) -> tuple[object, str]:
        paths = path.split(':', 1)
        p = paths[0].replace('\\', '/').lower()
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
        return MetaManager.getMetaInfos(manager, self, item.source if isinstance(item.source, FileSource) else None) if self.valid else None

    def getMetaItems(self, manager: MetaManager) -> list[MetaItem]:
        return MetaManager.getMetaItems(manager, self) if self.valid else None
    #endregion

# ManyArchive
class ManyArchive(BinaryArchive):
    def __init__(self, basis: Archive, state: BinaryState, name: str, paths: list[str], pathSkip: int = 0):
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
            arc = self.game.createArchive(BinaryState(self.vfx, self.game, self.edition, s)) if self.game._isArcPath(s) else None,
            fileSize = 0,
            lazy = lambda x, _s=s: lambdax(x, _s))
            for s in self.paths]

    def readData(self, file: FileSource, option: object = None) -> BytesIO:
        if file.arc: file.arc.readData(file, option)
        data = self.vfx.open(file.path)
        return data if isinstance(data, BytesIO) else BytesIO(data.readall()) # .readBytes(file.fileSize))
    #endregion

# MultiArchive
class MultiArchive(Archive):
    def __init__(self, state: BinaryState, name: str, archives: list[Archive]):
        super().__init__(state)
        self.name = name
        self.archives = archives or _throw('Empty archives')

    def opening(self):
        count = 0
        for s in self.archives: s.open(); count += s.count
        self.valid = True
        self.count = count

    def closing(self):
        for s in self.archives: s.close()
        self.valid = False
        self.count = 0

    def contains(self, path: object) -> bool:
        match path:
            case None: raise Exception('Null')
            case s if isinstance(path, str):
                arcs, next_ = self._findArchives(s)
                return any(x.valid and x.contains(next_) for x in arcs)
            case i if isinstance(path, int): return any(x.valid and x.contains(i) for x in self.archives)
            case _: raise Exception(f'Unknown: {path}')

    def _findArchives(self, path: str) -> tuple[list[Archive], str]:
        paths = re.split('\\\\|/|:', path, 1)
        if len(paths) == 1: return self.archives, path
        path, nextPath = paths
        archives = [s for s in self.archives if s.name.startswith(path)]
        for archive in archives: archive.open()
        return archives, nextPath

    def getSource(self, path: FileSource | str | int, throwOnError: bool = True) -> tuple[Archive, FileSource]:
        match path:
            case None: raise Exception('Null')
            case s if isinstance(path, str):
                archives, next_ = self._findArchives(s)
                value = next(iter([x for x in archives if x.valid and x.contains(next_)]), None)
                if not value: raise Exception(f'Could not find file {path}')
                return value.getSource(next_, throwOnError)
            case i if isinstance(path, int):
                value = next(iter([x for x in self.archives if x.valid and x.contains(i)]), None)
                if not value: raise Exception(f'Could not find file {path}')
                return value.getSource(i, throwOnError)
            case _: raise Exception(f'Unknown: {path}')

    def getData(self, path: FileSource | str | int, option: object = None, throwOnError: bool = True) -> bytes:
        match path:
            case None: raise Exception('Null')
            case s if isinstance(path, str):
                arcs, next_ = self._findArchives(s)
                z = next(iter([s for s in arcs if s.valid and s.contains(next_)]), None)
                if not z: raise Exception(f'Could not find file {path}')
                return z.getData(next_, option, throwOnError)
            case i if isinstance(path, int):
                z = next(iter([s for s in self.archives if s.valid and s.contains(i)]), None)
                if not z: raise Exception(f'Could not find file {path}')
                return z.getData(i, option, throwOnError)
            case _: raise Exception(f'Unknown: {path}')

    def getAsset(self, t: type, path: FileSource | str | int, option: object = None, throwOnError: bool = True) -> object:
        match path:
            case None: raise Exception('Null')
            case s if isinstance(path, str):
                arcs, next_ = self._findArchives(s)
                z = next(iter([s for s in arcs if s.valid and s.contains(next_)]), None)
                if not z: raise Exception(f'Could not find file {path}')
                return z.getAsset(t, next_, option, throwOnError)
            case i if isinstance(path, int):
                z = next(iter([s for s in self.archives if s.valid and s.contains(i)]), None)
                if not z: raise Exception(f'Could not find file {path}')
                return z.getAsset(t, i, option, throwOnError)
            case _: raise Exception(f'Unknown: {path}')

    #region Metadata
    def getMetaItems(self, manager: MetaManager) -> list[MetaInfo]: return [MetaItem(s, s.name, manager.packageIcon, archive=s, items=s.getMetaItems(manager)) for s in self.archives if s.valid]
    #endregion

# tag::ArcBinary[]
# ArcBinary
class ArcBinary:
    def read(self, source: BinaryArchive, r: BinaryReader, tag: object = None) -> None: pass
    def readData(self, source: BinaryArchive, r: BinaryReader, file: FileSource, option: object = None): pass
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
        def __init__(self, parent: ArcBinary, state: BinaryState, file: FileSource, source: BinaryArchive):
            super().__init__(state, parent._instance)
            self.file = file
            self.source = source
            self.assetFactoryFunc = source.assetFactoryFunc
            # self.open()

        def opening(self) -> None: self.r = BinaryReader(self.source.readData(file)); self.pool = StaticPool[BinaryReader](self.r); super().opening()
        def closing(self) -> None: self.r.__exit__(); super().closing()
        def getReader(path: str, pooled: bool) -> IGenericPool[BinaryReader]: return self.pool

        # def read(self, r: BinaryReader, tag: object = None):
        #     if self.useReader: super().read(r, tag); return
        #     with BinaryReader(self.readData(self.source.getReader(), self.file)) as r2:
        #         self.arcBinary.read(self, r2, tag)
# end::ArcBinary[]

# ITransformAsset
class ITransformAsset:
    def canTransformAsset(self, src: object, transformTo: Archive) -> bool: pass
    def transformAsset(self, src: object, transformTo: Archive) -> object: pass
