from __future__ import annotations
from openx.core import ISource, Engine, EngineX
from openx.gfx import IOpenGfxApi, IOpenGfxSprite, IOpenGfxModel, ITexture, TextureAsBytes, TextureBuilder, TextureFlags, TextureFormat, TextureManager, TexturePixel
import struct
from openx.engines.system import SystemSfx
from openx.client import IClientHost
from openx.engines.vulkan.gfx.vulkan import VulkanInstance

#region Client

# VulkanClientHost
class VulkanClientHost(IClientHost):
    def dispose(self) -> None: raise NotImplementedError()
    def run(self) -> None: raise NotImplementedError()

#endregion

#region Engine

# VulkanGfxApi
class VulkanGfxApi(IOpenGfxApi):
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

# VulkanGfxSprite3D
class VulkanGfxSprite3D(IOpenGfxSprite):
    def __init__(self):
        self.spriteManager = None
    def preload(self, source: ISource, path: object) -> None: self.spriteManager.preload(source, path)
    def create(self, source: ISource, path: object, parent: object = None) -> tuple[object, object]: return self.spriteManager.create(source, path, parent)

# The pixels VulkanGfxModel.textureManager keeps. No device image yet.
class VulkanTexture:
    def __init__(self):
        self.width = 0
        self.height = 0
        self.mipMaps = 1
        self.format = None
        self.bytes = None
        self.spans = None
        self.levelStart = 0
        self.levelStop = 1
        self.clampS = False
        self.clampT = False
        self.deleted = False

# VulkanTextureBuilder. Accepts the same TextureAsBytes path as OpenGL and keeps the bytes.
class VulkanTextureBuilder(TextureBuilder):
    def __init__(self):
        self._default = None

    @property
    def default(self) -> VulkanTexture:
        if self._default is None: self._default = self.createSolid(4, 4, [
            0.9, 0.2, 0.8, 1.0, 0.0, 0.9, 0.0, 1.0, 0.9, 0.2, 0.8, 1.0, 0.0, 0.9, 0.0, 1.0,
            0.0, 0.9, 0.0, 1.0, 0.9, 0.2, 0.8, 1.0, 0.0, 0.9, 0.0, 1.0, 0.9, 0.2, 0.8, 1.0,
            0.9, 0.2, 0.8, 1.0, 0.0, 0.9, 0.0, 1.0, 0.9, 0.2, 0.8, 1.0, 0.0, 0.9, 0.0, 1.0,
            0.0, 0.9, 0.0, 1.0, 0.9, 0.2, 0.8, 1.0, 0.0, 0.9, 0.0, 1.0, 0.9, 0.2, 0.8, 1.0,
        ])
        return self._default

    def createNormalMap(self, src: VulkanTexture, strength: float) -> VulkanTexture: return None

    def createSolid(self, width: int, height: int, rgbas: list[float]) -> VulkanTexture:
        tex = VulkanTexture()
        tex.width = width
        tex.height = height
        tex.format = (TextureFormat.RGBA32, TexturePixel.Float)
        tex.bytes = struct.pack('<' + 'f' * len(rgbas), *rgbas)
        return tex

    def create(self, reuse: VulkanTexture, src: ITexture, level: range = None) -> VulkanTexture:
        def build(x: object) -> VulkanTexture:
            match x:
                case TextureAsBytes():
                    if not x.bytes: return self.default
                    fmt = x.format
                    if not isinstance(fmt, tuple): raise Exception(f'Unknown format: {fmt}')
                    formatx, _pixel = fmt
                    if not _known(formatx): raise Exception(f'Unknown format: {formatx}')
                    numMipMaps = max(1, src.mipMaps)
                    start = level.start if level else 0
                    if _badSpan(x.spans, start, numMipMaps): return self.default
                    tex = reuse if reuse is not None else VulkanTexture()
                    tex.width = src.width
                    tex.height = src.height
                    tex.mipMaps = numMipMaps
                    tex.format = fmt
                    tex.bytes = x.bytes
                    tex.spans = x.spans
                    tex.levelStart = start
                    tex.levelStop = numMipMaps
                    flags = src.texFlags
                    tex.clampS = (flags & TextureFlags.SUGGEST_CLAMPS) != 0
                    tex.clampT = (flags & TextureFlags.SUGGEST_CLAMPT) != 0
                    tex.deleted = False
                    return tex
                case _: raise Exception(f'Unknown x: {x}')
        return src.create('VK', build)

    def delete(self, src: VulkanTexture) -> None:
        if src is None: return
        src.deleted = True
        src.bytes = None

def _known(formatx: TextureFormat) -> bool:
    return formatx in (
        TextureFormat.I8, TextureFormat.L8, TextureFormat.R8, TextureFormat.R16, TextureFormat.RG16,
        TextureFormat.RGB24, TextureFormat.RGB565, TextureFormat.RGBA32, TextureFormat.ARGB32,
        TextureFormat.BGRA32, TextureFormat.BGRA1555, TextureFormat.DXT1, TextureFormat.DXT1A,
        TextureFormat.DXT3, TextureFormat.DXT5, TextureFormat.BC4, TextureFormat.BC5,
        TextureFormat.BC6H, TextureFormat.BC7, TextureFormat.ETC2, TextureFormat.ETC2_EAC,
    )

def _badSpan(spans: list, start: int, stop: int) -> bool:
    if not spans: return False
    for l in range(start, stop):
        span = spans[l]
        if span is not None and span.start < 0: return True
    return False

# VulkanGfxModel
class VulkanGfxModel(IOpenGfxModel):
    def __init__(self):
        self.materialManager = None
        self.objectManager = None
        self.shaderManager = None
        self.textureManager = TextureManager(VulkanTextureBuilder())
    def preload(self, source: ISource, path: object) -> None: raise NotImplementedError()
    def preloadTexture(self, source: ISource, path: object) -> None: self.textureManager.preload(source, path)
    def create(self, source: ISource, path: object, isStatic: bool, parent: object = None) -> tuple[object, object]: raise NotImplementedError()
    def createShader(self, source: ISource, path: object, args: dict[str, bool] = None) -> tuple[object, object]: raise NotImplementedError()
    def createTexture(self, source: ISource, path: object, level: object = None) -> tuple[object, object]: return self.textureManager.create(source, path, level)
    def post(self, src: object, position: object, eulerAngles: object, scale: float, parent: object = None) -> None: raise NotImplementedError()

# VulkanSfx
class VulkanSfx(SystemSfx): pass

# VulkanEngine
class VulkanEngine(Engine):
    def __init__(self):
        super().__init__('VK', 'Vulkan')
        self.caps = EngineX.Caps.Drawing
        self.gfxFactory = staticmethod(lambda: [VulkanGfxApi(), None, VulkanGfxSprite3D(), VulkanGfxModel(), None, None])
        self.sfxFactory = staticmethod(lambda: [VulkanSfx()])
    @staticmethod
    def createInstance(applicationName: str = 'GameX') -> VulkanInstance: return VulkanInstance.createInstance(applicationName)
VulkanEngine.this = VulkanEngine()

# VulkanShellEngine
class VulkanShellEngine(Engine):
    def __init__(self):
        super().__init__('VK', 'Vulkan')
VulkanShellEngine.this = VulkanShellEngine()

#endregion
