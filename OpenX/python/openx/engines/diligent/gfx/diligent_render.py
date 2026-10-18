# TestTriRenderer
class TestTriRenderer:
    def __init__(self, gfx, source, obj):
        self.gfxModel = gfx[3] if gfx is not None and len(gfx) > 3 else None

# TextureRenderer
class TextureRenderer:
    def __init__(self, gfx, source, obj, level):
        self.gfxModel = gfx[3] if gfx is not None and len(gfx) > 3 else None
        self.source = source
        self.obj = obj
        self.level = level
