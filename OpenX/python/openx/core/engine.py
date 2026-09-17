from __future__ import annotations
import os, sys
from enum import Enum, Flag
from openx.core.core import ISource
from openx.core.util import decodePath, YamlDict

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
    def activate(self) -> None: pass
    def deactivate(self) -> None: pass

# EngineX
class EngineX:
    # The Caps.
    class Caps(Flag):
        None_ = 0x0
        ReadDds = 0x1

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
    inTestHost: bool = 'unittest' in sys.modules.keys()
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
