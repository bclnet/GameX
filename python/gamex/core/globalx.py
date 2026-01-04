from __future__ import annotations
import os

class Color3:
    _struct = ('<4c', 4)
    def __repr__(self) -> str: return f'{self.r}:{self.g}:{self.b}'
    def __init__(self, *args):
        match len(args):
            case 1: r = args[0]; self.r: float = r.readSingle(); self.g: float = r.readSingle(); self.b: float = r.readSingle()
            case 3: self.r: float = args[0]; self.g: float = args[1]; self.b: float = args[2]
    @property
    def asColor(self) -> Color: raise NotImplementedError() # return Color.fromArgb(r * 255.0, g * 255.0, b * 255.0)

class ByteColor3:
    _struct = ('<3c', 3)
    def __repr__(self) -> str: return f'{self.r}:{self.g}:{self.b}'
    def __init__(self, *args):
        match len(args):
            case 1: r = args[0]; self.r: int = r.readByte(); self.g: int = r.readByte(); self.b: int = r.readByte()
            case 3: self.r: int = args[0]; self.g: int = args[1]; self.b: int = args[2]
    @property
    def asColor(self) -> Color: raise NotImplementedError()

class Color4:
    _struct = ('<4c', 4)
    def __repr__(self) -> str: return f'{self.r}:{self.g}:{self.b}'
    def __init__(self, *args):
        match len(args):
            case 1: r = args[0]; self.r: float = r.readSingle(); self.g: float = r.readSingle(); self.b: float = r.readSingle(); self.a: float = r.readSingle()
            case 4: self.r: float = args[0]; self.g: float = args[1]; self.b: float = args[2]; self.a: float = args[3]
    @property
    def asColor(self) -> Color: raise NotImplementedError()

class ByteColor4:
    _struct = ('<4c', 4)
    def __repr__(self) -> str: return f'{self.r}:{self.g}:{self.b}'
    def __init__(self, *args):
        match len(args):
            case 1: r = args[0]; self.r: int = r.readByte(); self.g: int = r.readByte(); self.b: int = r.readByte(); self.a: int = r.readByte()
            case 4: self.r: int = args[0]; self.g: int = args[1]; self.b: int = args[2]; self.a: int = args[3]
    @property
    def asColor(self) -> Color: raise NotImplementedError() # return GXColor32(self.r, self.g, self.b, self.a)
