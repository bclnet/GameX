from __future__ import annotations
import os, pygame
# from pygame.locals import *
from openstk.gfx import ITexture

# typedefs
class IPygameGfx: pass

# ViewBase
class ViewBase:
    gfx: IOpenGLGfx = None
    obj: object = None
    def __init__(self, gfx: IPygameGfx, obj: object, surf: object):
        self.gfx = gfx
        self.obj = obj
        self.surf = surf
    def start(self) -> None: pass
    def update(self) -> None: pass

# ViewTexture
class ViewTexture(ViewBase):
    def __init__(self, gfx: IOpenGLGfx, obj: object, surf: object): super().__init__(gfx, obj, surf)

# ViewTexture
class TestAnim(ViewBase):
    def __init__(self, gfx: IOpenGLGfx, obj: object, surf: object): super().__init__(gfx, obj, surf)

    def start(self) -> None:
        self.x = 320 # Initial x position of the moving object
        self.dx = 5 # Speed of the moving object

    def update(self) -> None:
        w = self.surf.get_width()
        # Draw the moving object
        pygame.draw.circle(self.surf, (0, 0, 0), (self.x, 240), 30)  # Draw a black circle at the current
        # position
        self.x += self.dx  # Update the position of the moving object
        if self.x + 30 > w or self.x - 30 < 0:  # Check if the moving object has reached the edge of the surface
            self.dx = -self.dx  # Reverse the direction of the moving object

def createView(parent: object, gfx: IPygameGfx, obj: object, type: str) -> ViewBase:
    surf = parent.surface
    match type:
        # case 'Texture': return ViewTexture(gfx, obj, surf)
        case _: return TestAnim(gfx, obj, surf)
