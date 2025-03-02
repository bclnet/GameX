from __future__ import annotations
import os, pygame
# from pygame.locals import *
from openstk.gfx.gfx_texture import ITexture

# typedefs
class IPygameGfx: pass

# ViewBase
class ViewBase:
    def __init__(self, gfx: IPygameGfx, surface: object, obj: object):
        self.gfx = gfx
        self.surface = surface
        self.obj = obj
    def start(self) -> None: pass
    def update(self) -> None: pass

# ViewCell
class ViewCell(ViewBase):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)
    pass

# ViewEngine
class ViewEngine(ViewBase):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)

# ViewInfo
class ViewInfo:
    pass

# ViewObject
class ViewObject(ViewBase):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)
    pass

# ViewTexture
class ViewTexture(ViewBase):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)
    pass

# ViewTexture
class TestAnim(ViewBase):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, **kwargs)

    def start(self) -> None:
        self.x = 320 # Initial x position of the moving object
        self.dx = 5 # Speed of the moving object

    def update(self) -> None:
        w = self.surface.get_width()
        # Draw the moving object
        pygame.draw.circle(self.surface, (0, 0, 0), (self.x, 240), 30)  # Draw a black circle at the current
        # position
        self.x += self.dx  # Update the position of the moving object
        if self.x + 30 > w or self.x - 30 < 0:  # Check if the moving object has reached the edge of the surface
            self.dx = -self.dx  # Reverse the direction of the moving object

@staticmethod
def createView(gfx: IPygameGfx, surface: object, obj: object) -> ViewBase:
    if isinstance(obj, ITexture): return ViewTexture(gfx, surface, obj)
    return TestAnim(gfx, surface, obj)