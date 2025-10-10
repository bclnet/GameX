from __future__ import annotations
import os

class Color3:
    def __init__(self, *args):
        if len(args) == 1:
            r = args[0]
            self.r: float = r.readSingle()
            self.g: float = r.readSingle()
            self.b: float = r.readSingle()
        elif len(args) == 3:
            self.r: float = args[0]
            self.g: float = args[1]
            self.b: float = args[2]
    def toColor(self) -> Color: raise NotImplementedError() # return Color.fromArgb(r * 255.0, g * 255.0, b * 255.0)

class ByteColor3:
    def __init__(self, *args):
        if len(args) == 1:
            r = args[0]
            self.r: float = r.readByte()
            self.g: float = r.readByte()
            self.b: float = r.readByte()
        elif len(args) == 3:
            self.r: float = args[0]
            self.g: float = args[1]
            self.b: float = args[2]
    def toColor(self) -> Color: raise NotImplementedError()

class Color4:
    def __init__(self, *args):
        if len(args) == 1:
            r = args[0]
            self.r: float = r.readSingle()
            self.g: float = r.readSingle()
            self.b: float = r.readSingle()
            self.a: float = r.readSingle()
        elif len(args) == 4:
            self.r: float = args[0]
            self.g: float = args[1]
            self.b: float = args[2]
            self.a: float = args[3]

class ByteColor4:
    def __init__(self, *args):
        if len(args) == 1:
            r = args[0]
            self.r: float = r.readByte()
            self.g: float = r.readByte()
            self.b: float = r.readByte()
            self.a: float = r.readByte()
        elif len(args) == 4:
            self.r: float = args[0]
            self.g: float = args[1]
            self.b: float = args[2]
            self.a: float = args[3]