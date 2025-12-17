from __future__ import annotations
import os, json, re, random, platform
from enum import Enum
from datetime import datetime
from urllib.parse import urlparse
from importlib import resources
from openstk import findType, _throw, YamlDict
from openstk.vfx import FileSystem, AggregateFileSystem, NetworkFileSystem, DirectoryFileSystem, VirtualFileSystem
from openstk.platforms import PlatformX
from gamex import option, familyKeys 
from gamex.core.binary import ArchiveState, ManyArchive, MultiArchive
from gamex.core.store import getPathByKey as Store_getPathByKey
from .util import _valueF, _value, _list, _related, _dictTrim

#region Factory

# tag::SearchBy[]
class SearchBy(Enum):
    Default = 1
    Arc = 2
    TopDir = 3
    TwoDir = 4
    DirDown = 5
    AllDir = 6
# tag::SearchBy[]

# tag::FoundPath[]
class SystemPath:
    root: str
    type: str
    paths: list[str]
    def __init__(self, root: str, type: str, paths: list[str]):
        self.root = root
        self.type = type
        self.paths = paths
# tag::FoundPath[]

# TODO: HostFactory = HttpHost.Factory

# tag::parseKey[]
# parse key
@staticmethod
def parseKey(value: str) -> object:
    if not value: return None
    elif value.startswith('b64:'): return base64.b64decode(value[4:].encode('ascii'))
    elif value.startswith('hex:'): return bytes.fromhex(value[4:].replace('/x', ''))
    elif value.startswith('asc:'): return value[4:].encode('ascii')
    else: return value
# end::parseKey[]

# tag::parseEngine[]
# parse engine
@staticmethod
def parseEngine(value: str) -> (str, str):
    if not value: return (None, None)
    p = value.split(':', 1)
    return (p[0], None if len(p) < 2 else p[1])
# end::parseEngine[]

# create FamilySample
@staticmethod
def createFamilySample(path: str, loader: callable) -> FamilySample:
    elem = loader(path)
    return FamilySample(elem)

# create Family
@staticmethod
def createFamily(any: str, loader: callable = None, loadSamples: bool = False) -> Family:
    elem = loader(any) if loader else any
    familyType = _value(elem, 'familyType')
    family = findType(familyType)(elem) if familyType else Family(elem)
    if family.specSamples and loadSamples:
        for sample in family.specSamples:
            family.mergeSample(createFamilySample(sample, loader))
    if family.specs:
        for spec in family.specs:
            family.merge(createFamily(spec, loader, loadSamples))
    return family

# create FamilyEngine
@staticmethod
def createFamilyEngine(family: Family, id: str, elem: dict[str, object]) -> FamilyEngine:
    engineType = _value(elem, 'engineType')
    return findType(engineType)(family, id, elem) if engineType else FamilyEngine(family, id, elem)

# create FamilyGame
@staticmethod
def createFamilyGame(family: Family, id: str, elem: dict[str, object], dgame: FamilyGame) -> FamilyGame:
    gameType = _value(elem, 'gameType', dgame.gameType)
    game = findType(gameType)(family, id, elem, dgame) if gameType else FamilyGame(family, id, elem, dgame)
    game.gameType = gameType
    return game

# create FamilyApp
@staticmethod
def createFamilyApp(family: Family, id: str, elem: dict[str, object]) -> FamilyApp:
    appType = _value(elem, 'appType')
    return findType(appType)(family, id, elem) if appType else FamilyApp(family, id, elem)

# create FileSystem
@staticmethod
def createFileSystem(vfxType: str, path: SystemPath, subPath: str, host: str = None) -> FileSystem:
    vfx = NetworkFileSystem(host) if host else \
        findType(vfxType)(path) if vfxType else \
        None
    if vfx: return vfx.next()
    baseRoot = path.root if not subPath else os.path.join(path.root, subPath)
    if baseRoot.endswith('/') or baseRoot.endswith('\\'): baseRoot = baseRoot[:-1]
    basePath = next(iter(path.paths), None) if path else None
    vfx = DirectoryFileSystem(baseRoot, basePath)
    return vfx.next()

#endregion

#region Detector

# tag::Detector[]
class Detector:
    def __init__(self, game: FamilyGame, id: str, elem: dict[str, object]):
        self.cache: dict[str, object] = {}
        self.hashs: dict[str, dict[str, object]] = {}
        self.id = self.name = id
        self.game = game
        def data_(k,v):
            match k:
                case 'name': self.name = name; return v
                case 'type': return v
                case 'key': return _valueF(elem, 'key', parseKey)
                # case 'hashs': self.hashs = _related(elem, 'hashs', k => k.GetProperty("hash").GetString(), v => parseHash(game, v)); return v
                case _: return v
        self.data = { k:data_(k,v) for k,v in elem.items() }
    def parseHash(self, game: FamilyGame, elem: dict[str, object]) -> dict[str, object]:
        def data_(k,v):
            match k:
                case 'edition': return v
                case 'locale': return v
                case _: return v
        return { k:data_(k,v) for k,v in elem.items() }
    def __repr__(self): return f'detector#{self.game}'
    def getHash(self, r: Reader) -> str:
        pass
    #TODO:Needsmore
# end::Detector[]

#endregion

#region Resource

# tag::Resource[]
class Resource:
    def __init__(self, vfx: FileSystem, game: FamilyGame, edition: Edition, searchPattern: str):
        self.vfx = vfx
        self.game = game
        self.edition = edition
        self.searchPattern = searchPattern
    def __repr__(self): return f'res:/{self.searchPattern}#{self.game}'
# end::Resource[]

#endregion

#region Family

# tag::Family[]
class Family:
    def __init__(self, elem: dict[str, object]):
        self.id = _value(elem, 'id')
        self.name = _value(elem, 'name')
        self.studio = _value(elem, 'studio')
        self.description = _value(elem, 'description')
        self.urls = _list(elem, 'url')
        self.specSamples = _list(elem, 'samples')
        self.specs = _list(elem, 'specs')
        # related
        dgame = FamilyGame(self, None, None, None)
        def gameMethod(k, v):
            nonlocal dgame
            game = createFamilyGame(self, k, v, dgame)
            if k.startswith('*'): dgame = game; return None
            return game
        self.apps = _related(elem, 'apps', lambda k,v: createFamilyApp(self, k, v))
        self.engines = _related(elem, 'engines', lambda k,v: createFamilyEngine(self, k, v))
        self.games = _dictTrim(_related(elem, 'games', gameMethod))
        self.samples = {}
    def __repr__(self): return f'''
{self.id}: {self.name}
engines: {[x for x in self.engines.values()]}
games: {[x for x in self.games.values()]}'''

    # merge
    def merge(self, source: Family) -> None:
        if not source: return
        self.engines.update(source.engines)
        self.games.update(source.games)
        self.apps.update(source.apps)

    # merge Sample
    def mergeSample(self, source: FamilySample) -> None:
        if not source: return
        self.samples.update(source.samples)

    # get Game
    def getGame(self, id: str, throwOnError: bool = True) -> FamilyGame:
        ids = id.rsplit('.', 1)
        gid = ids[0]; eid = ids[1] if len(ids) > 1 else ''
        game = _value(self.games, gid) or (throwOnError and _throw(f'Unknown game: {id}'))
        edition = _value(game.editions, eid)
        return (game, edition)

    # to Game
    def toGame(self, game: FamilyGame, edition: FamilyGame.Edition) -> str: return f'{game.id}.{edition.id}' if edition else game.id

    # tag::Family.parseResource[]
    # parse Resource
    def parseResource(self, uri: str, throwOnError: bool = True) -> Resource:
        if uri is None or not (uri := urlparse(uri)).fragment:
            return Resource(Game = FamilyGame(self, None, None, None))
        game, edition = self.getGame(uri.fragment)
        searchPattern = '' if uri.scheme == 'file' else uri.path[1:]
        virtuals = game.virtuals
        found = game.found
        subPath = edition.path if edition else None
        vfxType = game.vfxType
        vfx = \
            (createFileSystem(vfxType, found, subPath) if found else None) if uri.scheme == 'game' else \
            (createFileSystem(vfxType, SystemPath(uri.path, None), subPath) if uri.path else None) if uri.scheme == 'file' else \
            (createFileSystem(vfxType, found, subPath, uri) if uri.netloc else None) if uri.scheme.startswith('http') else None
        if not vfx:
            if throwOnError: raise Exception(f'{game.id}: unable to find game')
            else: return None
        if virtuals: vfx = AggregateFileSystem([vfx, VirtualFileSystem(virtuals)])
        return Resource(
            vfx = vfx,
            game = game,
            edition = edition,
            searchPattern = searchPattern)
    # end::Family.parseResource[]

    # to Game
    def toResource(self, res: Resource) -> str:
        return ''

    # open Archive
    def getArchive(self, res: Resource | str, throwOnError: bool = True) -> Archive:
        r = None
        match res:
            case s if isinstance(res, Resource): r = s
            case u if isinstance(res, str): r = self.parseResource(u)
            case _: raise Exception(f'Unknown: {res}')
        if not r.game.hasLoaded: r.game.hasLoaded = True; r.game.loaded()
        return (arc := r.game._getArchive(r.vfx, r.edition, r.searchPattern, throwOnError)) and arc.open() if r.game else \
            _throw(f'Undefined Game')
# end::Family[]

#endregion

#region FamilyApp

# tag::FamilyApp[]
class FamilyApp:
    def __init__(self, family: Family, id: str, elem: dict[str, object]):
        self.family = family
        self.id = self.name = id
        def switch(k,v):
            match k:
                case 'name': self.name = v; return v
                case 'explorerAppType': self.explorerType = v; return v
                case 'explorer2AppType': self.explorer2Type = v; return v
                case 'key': return _valueF(elem, 'key', parseKey)
                case _: return v
        self.data = { k:switch(k,v) for k,v in elem.items() }
    def __repr__(self): return f'\n  {self.id}: {self.name}'
    def openAsync(self, explorerType: Type, manager: MetaManager):
        #TODO:
        raise Exception('Not Implemented')
# end::FamilyApp[]

#endregion

#region FamilyEngine

# tag::FamilyEngine[]
class FamilyEngine:
    def __init__(self, family: Family, id: str, elem: dict[str, object]):
        self.family = family
        self.id = self.name = id
        def switch(k,v):
            match k:
                case 'name': self.name = v; return v
                case 'key': return _valueF(elem, 'key', parseKey)
                case _: return v
        self.data = { k:switch(k,v) for k,v in elem.items() }
    def __repr__(self): return f'\n  {self.id}: {self.name}'
# end::FamilyEngine[]

#endregion

#region FamilySample

# tag::FamilySample[]
class FamilySample:
    samples: dict[str, list[object]] = {}
    class File:
        def __init__(self, elem: dict[str, object]):
            def switch(k,v):
                match k:
                    case 'path': v = (v if isinstance(v, list) else [v]); self.paths = v; return v
                    case 'size': return v
                    case _: return v
            self.data = { k:switch(k,v) for k,v in elem.items() }
        def __repr__(self): return f'{self.paths}'

    def __init__(self, elem: dict[str, object]):
        for k,v in elem.items():
            self.samples[k] = [FamilySample.File(x) for x in v['files']]
# end::FamilySample[]

#endregion

#region FamilyGame

# The client state.
class ClientState:
    def __init__(self, archive: Archive, args: list[str] = None, tag: object = None):
        self.archive = archive
        self.args = args or []
        self.tag = tag

# tag::FamilyGame[]
class FamilyGame:
    # The game edition.
    class Edition:
        def __init__(self, id: str, elem: dict[str, object]):
            self.id = self.name = id
            self.path = None
            self.ignores = None
            def switch(k,v):
                match k:
                    case 'name': self.name = v; return v
                    case 'path': self.path = v; return v
                    case 'ignore': self.ignores = _list(elem, 'ignore'); return v
                    case 'key': return _valueF(elem, 'key', parseKey)
                    case _: return v
            self.data = { k:switch(k,v) for k,v in elem.items() }
        def __repr__(self): return f'{self.id}: {self.name}'

    # The game DLC.    
    class DownloadableContent:
        def __init__(self, id: str, elem: dict[str, object]):
            self.id = self.name = id
            self.path = None
            def switch(k,v):
                match k:
                    case 'name': self.name = v; return v
                    case 'path': self.path = v; return v
                    case 'key': return _valueF(elem, 'key', parseKey)
                    case _: return v
            self.data = { k:switch(k,v) for k,v in elem.items() }
        def __repr__(self): return f'{self.id}: {self.name}'

    # The game locale.
    class Locale:
        def __init__(self, id: str, elem: dict[str, object]):
            self.id = self.name = id
            def switch(k,v):
                match k:
                    case 'name': self.name = v; return v
                    case 'key': return _valueF(elem, 'key', parseKey)
                    case _: return v
            self.data = { k:switch(k,v) for k,v in elem.items() }
        def __repr__(self): return f'{self.id}: {self.name}'

    # The game files.
    class FileSet:
        keys: list[str]
        paths: list[str]
        def __init__(self, elem: dict[str, object]):
            self.keys = _list(elem, 'key')
            self.paths = _list(elem, 'path', [])

    # create Detector
    def createDetector(self, id: str, elem: dict[str, object]) -> Detector:
        detectorType = _value(elem, 'detectorType')
        return findType(detectorType)(self, id, elem) if detectorType else Detector(self, id, elem)

    # create Archive
    def createArchive(self, state: ArchiveState) -> Archive: return findType(self.archiveType)(state) if self.archiveType else _throw(f'{self.id} missing ArchiveType')

    # create Client
    def createClient(self, state: ClientState) -> Archive: return findType(self.clientType)(state) if self.clientType else _throw(f'{self.id} missing ClientType')

    def __init__(self, family: Family, id: str, elem: dict[str, object], dgame: FamilyGame):
        self.hasLoaded = False
        self.family = family
        self.id = id
        if not dgame:
            self.searchBy = SearchBy.Default; self.arcs = ['game:/']
            self.gameType = self.engine = self.resource = \
            self.paths = self.key = self.detector = self.clientType = self.vfxType = \
            self.archiveType = self.arcExts = None
            return
        self.name = _value(elem, 'name')
        self.engine = _valueF(elem, 'engine', parseEngine, dgame.engine)
        self.resource = _value(elem, 'resource', dgame.resource)
        self.urls = _list(elem, 'url')
        try:
            self.date = _valueF(elem, 'date', lambda s: datetime.strptime(s, '%Y-%m-%d' if s[0].isdigit() else '%b %d, %Y' if s.find(' ') == 3 else '%B %d, %Y').date() if s and s != 'XXX' else None)
        except Exception as e: print(f'{self.id} - {e}')
        #self.option = _list(elem, 'option', dgame.option)
        self.arcs = _list(elem, 'arc', dgame.arcs)
        self.paths = _list(elem, 'path', dgame.paths)
        self.key = _valueF(elem, 'key', parseKey, dgame.key); self.keyorig = _value(elem, 'key')
        self.status = _value(elem, 'status')
        self.tags = _value(elem, 'tags', '').split(' ')
        # interface
        self.clientType = _value(elem, 'clientType', dgame.clientType)
        self.vfxType = _value(elem, 'vfxType', dgame.vfxType)
        self.searchBy = _value(elem, 'searchBy', dgame.searchBy)
        self.archiveType = _value(elem, 'archiveType', dgame.archiveType)
        self.arcExts = _list(elem, 'arcExt', dgame.arcExts) 
        # related
        self.editions = _related(elem, 'editions', lambda k,v: FamilyGame.Edition(k, v))
        self.dlcs = _related(elem, 'dlcs', lambda k,v: FamilyGame.DownloadableContent(k, v))
        self.locales = _related(elem, 'locales', lambda k,v: FamilyGame.Locale(k, v))
        self.detectors = _related(elem, 'detectors', lambda k,v: self.createDetector(k, v))
        # files
        self.files: dict[str, PathItem] = _valueF(elem, 'files', lambda x: FamilyGame.FileSet(x))
        self.ignores: set = set(_list(elem, 'ignores', []))
        self.virtuals: dict[str, object] = _related(elem, 'virtuals', lambda k,v: parseKey(v))
        self.filters: dict[str, object] = _related(elem, 'filters', lambda k,v: v)
        # find
        self.found = self._getSystemPath(option.FindKey, family.id, elem)
        self.options = None
        
    def __repr__(self): return f'''
   {self.id}: {self.name} - {self.found}'''
#     def __repr__(self): return f'''
#   {self.id}: {self.name} - {self.status}
#   - editions: {self.editions if self.editions else None}
#   - dlcs: {self.dlcs if self.dlcs else None}
#   - locales: {self.locales if self.locales else None}'''

    # detect
    def detect(self, id: str, key: str, value: object, func: callable) -> object: return self.detectors[id].get(key, value, func) if id in self.detectors else None

    # Ensures this instance.
    def loaded(self) -> None: self.options = YamlDict(f'~/.gamex.{self.family.id}_{self.id}.yaml')

    # Converts the game to uris.
    def toUris(self, edition: str) -> list[str]: return [FamilyGame.toUri(self.id, edition, s) for s in self.arcs] # if self.arcs else []

    # Converts the game to a uri
    @staticmethod
    def toUri(id: str, edition: str = None, prefix: str = None) -> list[str]: return f'{prefix or 'game:/'}#{id}{'.' + edition if edition else ''}'

    # gets a client
    def getClient(self, state: ClientState) -> object: return self.createClient(state)

    # gets a game sample
    def getSample(self, id: str) -> FamilySample.File:
        if not self.id in self.family.samples or not (samples := self.family.samples[self.id]): return None
        random.seed()
        idx = random.randrange(len(samples)) if id == '*' else int(id)
        return samples[idx] if len(samples) > idx else None

    # gets a game system path
    def _getSystemPath(self, startsWith: str, family: str, elem: map[str, object]) -> SystemPath:
        if not self.files or not self.files.keys: return None
        for key in [x for x in self.files.keys if x.startsWith(startsWith)] if startsWith else self.files.keys:
            p = key.split('#', 1)
            k = p[0]; v = None if len(p) < 2 else p[1]
            path = Store_getPathByKey(k, family, elem)
            if not path: continue
            path = os.path.abspath(PlatformX.decodePath(path))
            if not os.path.exists(path): continue
            return SystemPath(root = path, type = None, paths = self.files.paths)
        return None

    # create SearchPatterns
    def _createSearchPatterns(self, searchPattern: str) -> str:
        if searchPattern: return searchPattern
        elif self.searchBy == SearchBy.Default: return ''
        elif self.searchBy == SearchBy.Arc: return '' if not self.arcExts else \
            f'*{self.arcExts[0]}' if len(self.arcExts) == 1 else f'(*{":*".join(self.arcExts)})'
        elif self.searchBy == SearchBy.TopDir: return '*'
        elif self.searchBy == SearchBy.TwoDir: return '*/*'
        elif self.searchBy == SearchBy.DirDown: return '**/*'
        elif self.searchBy == SearchBy.AllDir: return '**/*'
        else: raise Exception(f'Unknown searchBy: {self.searchBy}')

    # is a Archive
    def _isArcPath(self, path: str) -> bool: return self.arcExts and any([x for x in self.arcExts if path.endswith(x)])

    # get Archive
    def _getArchive(self, vfx: FileSystem, edition: Edition, searchPattern: str, throwOnError: bool) -> Archive:
        if isinstance(vfx, NetworkFileSystem): raise Exception('NetworkFileSystem not supported')
        searchPattern = self._createSearchPatterns(searchPattern)
        archives = []
        dlcKeys = [x[0] for x in self.dlcs.items() if x[1].path]
        slash = '\\'
        for key in [None] + dlcKeys:
            for p in self._findPaths(vfx, edition, self.dlcs[key] if key else None, searchPattern):
                if self.searchBy == SearchBy.Arc:
                    for path in p[1]:
                        if self._isArcPath(path): archives.append(self._getArchiveObj(vfx, edition, path))
                else:
                    archives.append(self._getArchiveObj(vfx, edition,
                        (p[0], [x for x in p[1] if x.find(slash) >= 0]) if self.searchBy == SearchBy.DirDown else p))
        return (archives[0] if len(archives) == 1 else self._getArchiveObj(vfx, edition, archives)).setPlatform(PlatformX.current)

    # get ArchiveObj
    def _getArchiveObj(self, vfx: FileSystem, edition: Edition, value: object, tag: object = None) -> Archive:
        arcState = ArchiveState(vfx, self, edition, value if isinstance(value, str) else None, tag)
        match value:
            case s if isinstance(value, str): return self.createArchive(arcState) if self._isArcPath(s) else _throw(f'{self.id} missing {s}')
            case p, l if isinstance(value, tuple):
                return self._getArchiveObj(vfx, edition, l[0], tag) if len(l) == 1 and self._isArcPath(l[0]) \
                    else ManyArchive(
                        self.createArchive(arcState), arcState,
                        p if len(p) > 0 else 'Many', l,
                        pathSkip = len(p) + 1 if len(p) > 0 else 0)
            case s if isinstance(value, list): return s[0] if len(s) == 1 else MultiArchive(arcState, 'Multi', s)
            case None: return None
            case _: raise Exception(f'Unknown: {value}')

    # find Paths
    def _findPaths(self, vfx: FileSystem, edition: Edition, dlc: DownloadableContent, searchPattern: str):
        ignores = self.ignores
        for path in self.paths or ['']:
            searchPath = os.path.join(path, dlc.path) if dlc and dlc.path else path
            fileSearch = vfx.findPaths(searchPath, searchPattern)
            if ignores: fileSearch = [s for s in fileSearch if not os.path.basename(s) in ignores]
            yield (path, list(fileSearch))

# end::FamilyGame[]

#endregion

#region Loader

Families = {}
unknown = None
unknownArchive = None

@staticmethod
def init(loadSamples: bool = True):
    def commentRemover(text: str) -> str:
        def replacer(match): s = match.group(0); return ' ' if s.startswith('/') else s
        pattern = re.compile(r'//.*?$|/\*.*?\*/|\'(?:\\.|[^\\\'])*\'|"(?:\\.|[^\\"])*"', re.DOTALL | re.MULTILINE)
        return re.sub(pattern, replacer, text)
    def familyJsonLoader(path: str) -> dict[str, object]:
        body = resources.files().joinpath('../specs', path).read_text(encoding='utf-8')
        return json.loads(commentRemover(body).encode().decode('utf-8-sig'))

    # load Families
    for path in [f'{s}Family.json' for s in familyKeys]:
        try:
            family = createFamily(path, familyJsonLoader, loadSamples)
            Families[family.id] = family
        except Exception as e:
            print(f'{path} - {type(e).__name__}')
            print(e)

    # load unknown
    unknown = getFamily('Unknown')
    unknownArchive = unknown.getArchive('game:/#APP', False)

@staticmethod
def getFamily(id: str, throwOnError: bool = True) -> Family:
    family = _value(Families, id)
    if not family and throwOnError: raise Exception(f'Unknown family: {id}')
    return family

#endregion