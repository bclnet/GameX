from __future__ import annotations
from openx.core import ISource, Engine, EngineX
from openx.gfx import IOpenGfxSprite, IOpenGfxModel
from openx.engines.system import SystemSfx
from openx.client import IClientHost

#region Client

# BgfxClientHost
class BgfxClientHost(IClientHost):
    def dispose(self) -> None: raise NotImplementedError()
    def run(self) -> None: raise NotImplementedError()

#endregion

#region Engine

# BgfxGfxSprite3D
class BgfxGfxSprite3D(IOpenGfxSprite):
    def __init__(self):
        self.spriteManager = None
    def preload(self, source: ISource, path: object) -> None: self.spriteManager.preload(source, path)
    def create(self, source: ISource, path: object, parent: object = None) -> tuple[object, object]: return self.spriteManager.create(source, path, parent)

# BgfxGfxModel
class BgfxGfxModel(IOpenGfxModel):
    def __init__(self):
        self.materialManager = None
        self.objectManager = None
        self.shaderManager = None
        self.textureManager = None
    def preload(self, source: ISource, path: object) -> None: raise NotImplementedError()
    def preloadTexture(self, source: ISource, path: object) -> None: self.textureManager.preload(source, path)
    def create(self, source: ISource, path: object, isStatic: bool, parent: object = None) -> tuple[object, object]: raise NotImplementedError()
    def createShader(self, source: ISource, path: object, args: dict[str, bool] = None) -> tuple[object, object]: raise NotImplementedError()
    def createTexture(self, source: ISource, path: object, level: object = None) -> tuple[object, object]: return self.textureManager.create(source, path, level)
    def post(self, src: object, position: object, eulerAngles: object, scale: float, parent: object = None) -> None: raise NotImplementedError()

# BgfxSfx
class BgfxSfx(SystemSfx): pass

# BgfxEngine
class BgfxEngine(Engine):
    def __init__(self):
        super().__init__('BX', 'Bgfx')
        self.caps = EngineX.Caps.Drawing
        self.gfxFactory = staticmethod(lambda: [None, None, BgfxGfxSprite3D(), BgfxGfxModel(), None, None])
        self.sfxFactory = staticmethod(lambda: [BgfxSfx()])
BgfxEngine.this = BgfxEngine()

# BgfxShellEngine
class BgfxShellEngine(Engine):
    def __init__(self):
        super().__init__('BX', 'Bgfx')
BgfxShellEngine.this = BgfxShellEngine()

#endregion
