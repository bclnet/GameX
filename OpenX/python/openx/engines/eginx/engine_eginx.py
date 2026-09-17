from __future__ import annotations
import traceback
from openx.core import ISource, Engine
from openx.engines.system import SystemSfx
from openx.gfx import IOpenGfxApi, IOpenGfxSprite, IOpenGfxModel, ObjectModelBuilder, ObjectModelManager, MaterialBuilder, MaterialManager, ShaderBuilder, ShaderManager, TextureManager, TextureBuilder
from openx.client import IClientHost
from openx.gfx.egin import Game, GraphicsDeviceManager

#region Client

# EginXClientHost
class EginXClientHost(Game, IClientHost):
    def __init__(self, client: callable):
        super().__init__()
        # self.deviceManager = GraphicsDeviceManager(self)
        self.client: ClientBase = client()
        self.scene: SceneBase = None
        self.pluginHost: IPluginHost = None

    def getScene[T](self) -> T: return self.scene

    def setScene(self, scene: SceneBase) -> None:
        if self.scene: self.scene.dispose()
        self.scene = scene
        self.scene.load() 

    def loadContent(self) -> None:
        super().loadContent()
        self.client.loadContent()

    def unloadContent(self) -> None:
        self.client.unloadContent()
        super().unloadContent()

#endregion

#region Engine

# EginXObjectBuilder
# MISSING

# EginXGfxApi
class EginXGfxApi(IOpenGfxApi):
    def __init__(self): pass
    def attach(self, method: GfxAttach, src: object, args: list[object]) -> object: raise NotImplementedError()

# EginXGfxSprite2D
class EginXGfxSprite2D(IOpenGfxSprite):
    def __init__(self):
        self.spriteManager: SpriteManager = SpriteManager(OpenGLTextureBuilder())
    def preload(self, source: ISource, path: object) -> None: self.spriteManager.preload(path)
    def create(self, source: ISource, path: object, level: range = None) -> int: return self.spriteManager.create(path)[0]

# EginXEngine
class EginXEngine(Engine):
    def __init__(self):
        super().__init__('EX', 'EginX')
        self.gfxFactory = staticmethod(lambda: [EginXGfxApi(), EginXGfxSprite2D(), None, None, None, None])
        self.sfxFactory = staticmethod(lambda: [SystemSfx()])
EginXEngine.this = EginXEngine()

#endregion