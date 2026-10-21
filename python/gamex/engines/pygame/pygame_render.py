from __future__ import annotations
import os, asyncio
from openx.core import ISource, IHaveSource
from openx.engines.pygame.gfx import PygameX, TestTriRenderer, TextureRenderer, TestAnimRenderer
from gamex.families.Arkane.formats.danae.binary import Binary_Ftl, Binary_Fts
from gamex.families.Gamebryo.formats.binary import Binary_Nif
from gamex.engines.pygame.pygameftlobjectbuilder import PygameFtlObjectBuilder
from gamex.engines.pygame.pygameftsobjectbuilder import PygameFtsObjectBuilder
from gamex.engines.pygame.pygamenifobjectbuilder import PygameNifObjectBuilder

PygameX.buildersByType['Binary_Nif'] = PygameNifObjectBuilder.buildObject
PygameX.buildersByType[Binary_Nif] = PygameNifObjectBuilder.buildObject
PygameX.buildersByType['Binary_Ftl'] = PygameFtlObjectBuilder.buildObject
PygameX.buildersByType[Binary_Ftl] = PygameFtlObjectBuilder.buildObject
PygameX.buildersByType['Binary_Fts'] = PygameFtsObjectBuilder.buildObject
PygameX.buildersByType[Binary_Fts] = PygameFtsObjectBuilder.buildObject

# typedefs
class IOpenGfx: pass
class Renderer: pass

# PygameRenderer
class PygameRenderer:
    @staticmethod
    def createRenderer(parent: object, gfx: list[IOpenGfx], source: ISource, obj: object, type: str) -> Renderer:
        surf = parent.surface
        if isinstance(obj, IHaveSource): source = obj.source
        match type:
            case 'TestTri': return TestTriRenderer(gfx, source, obj, surf)
            case 'Texture' | 'VideoTexture': return TextureRenderer(gfx, source, obj, surf, None)
            case 'TestAnim': return TestAnimRenderer(gfx, source, obj, surf)
            case 'Object': return ObjectRenderer(gfx, source, obj, surf)
            case _: return None

# ObjectRenderer
class ObjectRenderer(Renderer):
    def __init__(self, gfx: list[IOpenGfx], source: ISource, obj: object, surf: object = None):
        self.gfx = gfx
        self.source = source
        self.obj = obj
        self.surf = surf
        self.result = None

    def start(self) -> None:
        builder = _builderFor(self.obj)
        if builder is None: return
        materialManager = None
        model = self.gfx[3] if self.gfx is not None and len(self.gfx) > 3 else None
        if model is not None: materialManager = getattr(model, 'materialManager', None)
        self.result = asyncio.run(builder(self.source, self.obj, True, materialManager))

    def update(self, deltaTime: float) -> None: pass

def _builderFor(obj: object):
    if obj is None: return None
    builder = PygameX.buildersByType.get(obj.__class__.__name__)
    if builder is None: builder = PygameX.buildersByType.get(type(obj))
    return builder
