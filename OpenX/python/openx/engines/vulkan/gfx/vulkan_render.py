from __future__ import annotations
from openx.core import ISource, log
from openx.gfx import GfX, Renderer, ITextureFrames

#region TestTriRenderer

# TestTriRenderer
class TestTriRenderer(Renderer):
    def __init__(self, gfx: list, source: ISource, obj: object):
        self.gfxModel = gfx[GfX.XModel]

#endregion

#region TextureRenderer

# TextureRenderer
class TextureRenderer(Renderer):
    def __init__(self, gfx: list, source: ISource, obj: object, level):
        self.gfxModel = gfx[GfX.XModel]
        self.source = source
        self.obj = obj
        self.level = level
        self.gfxModel.textureManager.delete(source, obj)
        self.texture = self.gfxModel.textureManager.create(source, obj, level)[0]
        self.frameDelay = 0

    def start(self) -> None:
        log.info('MakeTexture')
        log.info('Done')

    def update(self, deltaTime: float) -> None:
        obj = self.obj
        if not isinstance(obj, ITextureFrames) or self.gfxModel is None or not obj.hasFrames: return
        self.frameDelay += deltaTime
        if self.frameDelay <= obj.fps or not obj.nextFrame(): return
        self.frameDelay = 0 # reset delay between frames
        self.gfxModel.textureManager.reload(self.source, obj, self.level)

#endregion
