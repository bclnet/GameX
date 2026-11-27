from __future__ import annotations
from openx.gfx.gfx import GfxSpriteSheet

# Binary_Nwx. cells are (width, height, flip, data). data is column-major indexes.
# The WAXF reader is C# only. SCUMM costumes and XGA do not produce pixel buffers.
class Binary_Nwx:
    def __init__(self, cells, palette=None):
        self.cells = list(cells)
        self.palette = palette

    # flip writes top-down; otherwise the same walk is stored backwards. Index 0 is transparent.
    def _rgba(self, width, height, flip, data):
        raw = bytearray(width * height * 4)
        i = 0 if flip else len(raw) - 4
        step = 4 if flip else -4
        for row in range(height):
            for col in range(width):
                pixel = data[col * height + row]
                if self.palette is None: raw[i] = raw[i + 1] = raw[i + 2] = pixel
                else:
                    r, g, b = self.palette[pixel]
                    raw[i] = r; raw[i + 1] = g; raw[i + 2] = b
                raw[i + 3] = 0 if pixel == 0 else 255
                i += step
        return bytes(raw)

    # One frame per cell, duration 0.1s.
    def toSheet(self):
        packed = [(self._rgba(w, h, flip, data), w, h, 0.1) for w, h, flip, data in self.cells]
        return GfxSpriteSheet.pack(packed)

def testTwoCells():
    pal = [(0, 0, 0), (10, 20, 30), (40, 50, 60), (70, 80, 90)]
    sheet = Binary_Nwx([
        (2, 1, True, bytes([1, 2])),
        (1, 2, False, bytes([3, 0])),
    ], pal).toSheet()
    assert len(sheet.frames) == 2
    assert sheet.frames[0].duration == 0.1 and sheet.frames[1].duration == 0.1
    assert (sheet.frames[0].x, sheet.frames[0].width, sheet.frames[0].height) == (0, 2, 1)
    assert (sheet.frames[1].x, sheet.frames[1].width, sheet.frames[1].height) == (2, 1, 2)
    assert len(sheet.anims) == 1 and sheet.anims[0].name == "all" and list(sheet.anims[0].frames) == [0, 1]
    assert (sheet.texture.width, sheet.texture.height) == (3, 2)
    expect = bytes([
        10, 20, 30, 255, 40, 50, 60, 255, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 70, 80, 90, 255,
    ])
    assert sheet.texture.rgba == expect
    gray = Binary_Nwx([(1, 1, True, bytes([7]))], None).toSheet()
    assert gray.texture.rgba == bytes([7, 7, 7, 255])

if __name__ == "__main__":
    testTwoCells()
