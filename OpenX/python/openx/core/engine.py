from __future__ import annotations
import os, sys
from enum import Enum, Flag
from numpy import float32
from openx.core.core import ISource
from openx.core.util import decodePath, YamlDict

def _inTestHost() -> bool:
    # C#: AppDomain assemblies whose FullName starts with "testhost,".
    # A library import of unittest (numpy.testing, via quaternion) is not the host.
    argv0 = os.path.basename(sys.argv[0]).lower() if sys.argv and sys.argv[0] else ''
    stem = os.path.splitext(argv0)[0]
    if stem in ('pytest', 'py.test', 'testhost') or stem.startswith('pytest') or stem.startswith('testhost'): return True
    main = sys.modules.get('__main__')
    spec = getattr(main, '__spec__', None) if main is not None else None
    name = getattr(spec, 'name', None) or ''
    return name == 'unittest.__main__' or name.startswith('unittest.') or name == 'pytest' or name.startswith('pytest.') or name.startswith('_pytest.')

#region Engine

# Engine
class Engine:
    def __init__(self, id: str, name: str):
        self.enabled: bool = True
        self.caps: EngineX.Caps = EngineX.Caps.None_
        self.id: str = id
        self.name: str = name
        self.tag: str = None
        self.gfxFactory: callable = None
        self.sfxFactory: callable = None
        self.logFunc: callable = lambda a: print(a)
    @property
    def displayName(self) -> str: return self.name
    def activate(self) -> None: pass
    def deactivate(self) -> None: pass

# EngineX
class EngineX:
    @staticmethod
    def getEpsilon() -> float:
        epsilon = float32(1.)
        while True:
            epsilon *= float32(.5); comparison = float32(1.) + epsilon
            if not comparison > 1.: break
        return float(epsilon)
    epsilon: float = getEpsilon()

    # The Caps.
    class Caps(Flag):
        None_ = 0x0
        ReadDds = 0x1
        Drawing = 0x2

    # The OS.
    class OS(Enum):
        Unknown = 0
        Windows = 1
        OSX = 2
        Linux = 3
        Android = 4

    @staticmethod
    def activate(engine: Engine) -> None:
        if not engine or not engine.enabled: engine = UnknownEngine.this
        EngineX.engines.add(engine)
        current = EngineX.current
        if current != engine:
            if current: current.deactivate()
            if engine: engine.activate()
            EngineX.gfx = engine.gfxFactory() if engine and engine.gfxFactory else None
            EngineX.sfx = engine.sfxFactory() if engine and engine.sfxFactory else None
            EngineX.current = engine
        return engine

    @staticmethod
    def createMatcher(searchPattern: str) -> callable:
        if not searchPattern: return lambda x: True
        wildcardCount = searchPattern.count('*')
        if wildcardCount <= 0: return lambda x: x.casefold() == searchPattern.casefold()
        elif wildcardCount == 1:
            newPattern = searchPattern.replace('*', '')
            if searchPattern.startswith('*'): return lambda x: x.casefold().endswith(newPattern)
            elif searchPattern.endswith('*'): return lambda x: x.casefold().startswith(newPattern)
        regexPattern = f'^{re.escape(searchPattern).replace('\\*', '.*')}$'
        @staticmethod
        def _lambdax(x: str):
            try: return re.match(x, regexPattern)
            except: return False
        return _lambdax

    @staticmethod
    def decodePath(path: str, rootPath: str = None) -> str: return decodePath(EngineX.applicationPath, path, rootPath)

    platformOS: OS = OS.Windows if sys.platform == 'win32' else \
        OS.OSX if sys.platform == 'darwin' else \
        OS.Linux if sys.platform.startswith('linux') else \
        OS.Unknown
    engines: set[object] = {}
    inTestHost: bool = _inTestHost()
    applicationPath = os.getcwd()
    options = YamlDict('~/.gamex.yaml')
    current: Engine = None
    gfx: list[IOpenGfx] = None
    sfx: list[IOpenSfx] = None

#endregion

from openx.engines.test import TestEngine
from openx.engines.unknown import UnknownEngine

EngineX.engines = { UnknownEngine.this }
EngineX.current = EngineX.activate(TestEngine.this if EngineX.inTestHost else UnknownEngine.this)
