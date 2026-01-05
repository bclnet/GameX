from __future__ import annotations
import os

class Color3:
    _struct = ('<3f', 12)
    def __repr__(self) -> str: return f'{self.r}:{self.g}:{self.b}'
    def __init__(self, *args):
        match len(args):
            case 1 if isinstance(args[0], tuple): r = args[0]; self.r: float = r[0]; self.g: float = r[1]; self.b: float = r[2]
            case 1: r = args[0]; self.r: float = r.readSingle(); self.g: float = r.readSingle(); self.b: float = r.readSingle()
            case 3: self.r: float = args[0]; self.g: float = args[1]; self.b: float = args[2]
    @property
    def asColor(self) -> Color: raise NotImplementedError() # return Color.fromArgb(r * 255.0, g * 255.0, b * 255.0)

class ByteColor3:
    _struct = ('<3c', 3)
    def __repr__(self) -> str: return f'{self.r}:{self.g}:{self.b}'
    def __init__(self, *args):
        match len(args):
            case 1 if isinstance(args[0], tuple): r = args[0]; self.r: int = r[0]; self.g: int = r[1]; self.b: int = r[2]
            case 1: r = args[0]; self.r: int = r.readByte(); self.g: int = r.readByte(); self.b: int = r.readByte()
            case 3: self.r: int = args[0]; self.g: int = args[1]; self.b: int = args[2]
    @property
    def asColor(self) -> Color: raise NotImplementedError()

class Color4:
    _struct = ('<4f', 16)
    def __repr__(self) -> str: return f'{self.r}:{self.g}:{self.b}:{self.a}'
    def __init__(self, *args):
        match len(args):
            case 1 if isinstance(args[0], tuple): r = args[0]; self.r: float = r[0]; self.g: float = r[1]; self.b: float = r[2]; self.a: float = r[3]
            case 1: r = args[0]; self.r: float = r.readSingle(); self.g: float = r.readSingle(); self.b: float = r.readSingle(); self.a: float = r.readSingle()
            case 4: self.r: float = args[0]; self.g: float = args[1]; self.b: float = args[2]; self.a: float = args[3]
    @property
    def asColor(self) -> Color: raise NotImplementedError()

class ByteColor4:
    _struct = ('<4c', 4)
    def __repr__(self) -> str: return f'{self.r}:{self.g}:{self.b}:{self.a}'
    def __init__(self, *args):
        match len(args):
            case 1 if isinstance(args[0], tuple): r = args[0]; self.r: int = r[0]; self.g: int = r[1]; self.b: int = r[2]; self.a: int = r[3]
            case 1: r = args[0]; self.r: int = r.readByte(); self.g: int = r.readByte(); self.b: int = r.readByte(); self.a: int = r.readByte()
            case 4: self.r: int = args[0]; self.g: int = args[1]; self.b: int = args[2]; self.a: int = args[3]
    @property
    def asColor(self) -> Color: raise NotImplementedError() # return GXColor32(self.r, self.g, self.b, self.a)
