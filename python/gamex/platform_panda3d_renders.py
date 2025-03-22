from __future__ import annotations
import os
from panda3d.core import *
from openstk.gfx.gfx_render import IRenderer

# typedefs
class IPanda3dGfx: pass

# TextureRenderer
class TextureRenderer(IRenderer):
    gfx: IPanda3dGfx
    texture: Texture
    background: bool

    def __init__(self, gfx: IPanda3dGfx, texture: Texture, background: bool = False):
        self.gfx = gfx
        self.texture = texture
        self.background = background

    def render(self, camera: Camera, renderPass: RenderPass) -> None:
        pass

    def update(self, deltaTime: float) -> None: pass
