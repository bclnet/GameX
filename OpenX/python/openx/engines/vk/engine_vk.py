from __future__ import annotations
from openx.core import ISource, Engine, EngineX
from openx.gfx import IOpenGfxApi, IOpenGfxSprite, IOpenGfxModel
from openx.engines.system import SystemSfx
from openx.client import IClientHost
from openx.engines.vk.gfx.vk import VkInstance

#region Client

# VkClientHost
class VkClientHost(IClientHost):
    def dispose(self) -> None: raise NotImplementedError()
    def run(self) -> None: raise NotImplementedError()

#endregion

#region Engine

# VkGfxApi
class VkGfxApi(IOpenGfxApi):
    def createObject(self, name: str, tag: str = None, parent: object = None) -> object: raise NotImplementedError()
    def createMesh(self, mesh: object) -> object: raise NotImplementedError()
    def addMeshRenderer(self, src: object, mesh: object, material: object, enabled: bool, isStatic: bool) -> None: raise NotImplementedError()
    def addMeshCollider(self, src: object, mesh: object, isKinematic: bool, isStatic: bool) -> None: raise NotImplementedError()
    def attach(self, method: object, src: object, *args: object) -> None: raise NotImplementedError()
    def parent(self, src: object, parent: object) -> None: raise NotImplementedError()
    def transform(self, src: object, position: object, rotation: object, localScale: object) -> None: raise NotImplementedError()
    def addMissingMeshCollidersRecursively(self, src: object, isStatic: bool) -> None: raise NotImplementedError()
    def setLayerRecursively(self, src: object, layer: int) -> None: raise NotImplementedError()
    def setVisible(self, src: object, visible: bool) -> None: raise NotImplementedError()
    def destroy(self, src: object) -> None: raise NotImplementedError()

# VkGfxSprite3D
class VkGfxSprite3D(IOpenGfxSprite):
    def __init__(self):
        self.spriteManager = None
    def preload(self, source: ISource, path: object) -> None: self.spriteManager.preload(source, path)
    def create(self, source: ISource, path: object, parent: object = None) -> tuple[object, object]: return self.spriteManager.create(source, path, parent)

# VkGfxModel
class VkGfxModel(IOpenGfxModel):
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

# VkSfx
class VkSfx(SystemSfx): pass

# VkEngine
class VkEngine(Engine):
    def __init__(self):
        super().__init__('VK', 'Vulkan')
        self.caps = EngineX.Caps.Drawing
        self.gfxFactory = staticmethod(lambda: [VkGfxApi(), None, VkGfxSprite3D(), VkGfxModel(), None, None])
        self.sfxFactory = staticmethod(lambda: [VkSfx()])
    @staticmethod
    def createInstance(applicationName: str = 'GameX') -> VkInstance: return VkInstance.createInstance(applicationName)
VkEngine.this = VkEngine()

# VkShellEngine
class VkShellEngine(Engine):
    def __init__(self):
        super().__init__('VK', 'Vulkan')
VkShellEngine.this = VkShellEngine()

#endregion
