from __future__ import annotations
import os


class Color3:
    def __init__(self, r: Reader):
        self.r: float = r.readSingle() #:M
        self.g: float = r.readSingle() #:M
        self.b: float = r.readSingle() #:M
    def toColor(self) -> Color: raise NotImplementedError() # return Color.fromArgb(r * 255.0, g * 255.0, b * 255.0)

class Color4:
    def __init__(self, r: Reader):
        self.r: float = r.readSingle() #:M
        self.g: float = r.readSingle() #:M
        self.b: float = r.readSingle() #:M
        self.a: float = r.readSingle() #:M
