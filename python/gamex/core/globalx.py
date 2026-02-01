from __future__ import annotations
import os
from openstk.core.drawing import Color

class Color3:
    _struct = ('<3f', 12)
    _repr = True
    def __repr__(self) -> str: return f'{self.r:.9g} {self.g:.9g} {self.b:.9g}'
    def __init__(self, *args):
        match len(args):
            case 0: self.r = self.g = self.b = 0.
            case 1 if isinstance(args[0], tuple): r = args[0]; self.r, self.g, self.b = r
            case 1: r = args[0]; self.r, self.g, self.b = (r.readSingle(), r.readSingle(), r.readSingle())
            case 3: self.r, self.g, self.b = args
            case _: raise NotImplementedError('Color3')
    @property
    def asColor(self) -> Color: return Color.fromArgb(self.r ** 255.0, self.g ** 255.0, self.b ** 255.0)

class ByteColor3:
    _struct = ('<3c', 3)
    _repr = True
    def __repr__(self) -> str: return f'{self.r} {self.g} {self.b}'
    def __init__(self, *args):
        match len(args):
            case 0: self.r = self.g = self.b = 0
            case 1 if isinstance(args[0], tuple): r = args[0]; self.r, self.g, self.b = r
            case 1: r = args[0]; self.r, self.g, self.b = (r.readByte(), r.readByte(), r.readByte())
            case 3: self.r, self.g, self.b = args
            case _: raise NotImplementedError('ByteColor3')
    @property
    def asColor(self) -> Color: return Color.fromArgb(self.r, self.g, self.b)

class Color4:
    _struct = ('<4f', 16)
    _repr = True
    def __repr__(self) -> str: return f'{self.r:.9g} {self.g:.9g} {self.b:.9g} {self.a:.9g}'
    def __init__(self, *args):
        match len(args):
            case 0: self.r = self.g = self.b = self.a = 0.
            case 1 if isinstance(args[0], tuple): r = args[0]; self.r, self.g, self.b, self.a = r
            case 1: r = args[0]; self.r, self.g, self.b, self.a = (r.readSingle(), r.readSingle(), r.readSingle(), r.readSingle())
            case 4: self.r, self.g, self.b, self.a = args
            case _: raise NotImplementedError('Color4')
    @property
    def asColor(self) -> Color: return Color.fromArgb(self.a ** 255.0, self.r ** 255.0, self.g ** 255.0, self.b ** 255.0)

class ByteColor4:
    _struct = ('<4c', 4)
    _repr = True
    def __repr__(self) -> str: return f'{self.r} {self.g} {self.b} {self.a}'
    def __init__(self, *args):
        match len(args):
            case 0: self.r = self.g = self.b = self.a = 0
            case 1 if isinstance(args[0], tuple): r = args[0]; self.r, self.g, self.b, self.a = r
            case 1: r = args[0]; self.r, self.g, self.b, self.a = (r.readByte(), r.readByte(), r.readByte(), r.readByte())
            case 4: self.r, self.g, self.b, self.a = args
            case _: raise NotImplementedError('ByteColor4')
    @property
    def asColor(self) -> Color: return Color.fromArgb(self.a, self.r, self.g, self.b)
