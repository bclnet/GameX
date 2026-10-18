from __future__ import annotations
import math, numpy as np
from enum import Enum

#region Color

def _clamp01(value: float) -> float:
    # MathX.Clamp(float): one argument, 0..1.
    return value if 0. <= value <= 1. else (1. if value > 1. else 0.)

def _clamp(value: float, min_: float, max_: float) -> float:
    return min_ if value < min_ else max_ if value > max_ else value

def _u8(value: float) -> int:
    # unchecked (byte) of a float: truncate toward 0, then the low 8 bits.
    if value != value or value == float('inf') or value == float('-inf'): return 0
    return int(value) & 0xFF

def _netFloat(value: float, format: str) -> str:
    if format and format[0] in 'Ff' and format[1:].isdigit(): return f'{value:.{int(format[1:])}f}'
    return str(value)

class _FreshColor:
    def __init__(self, r: float, g: float, b: float, a: float): self._v = (r, g, b, a)
    def __get__(self, obj, owner): return owner(*self._v)

# Colorf. Equals is always false: Equals(other.G) boxes a float.
class Colorf:
    class Format(Enum):
        ARGB32 = 0

    def __init__(self, r: float, g=None, b: float = None, a: float = 1.):
        if isinstance(g, Colorf.Format):
            color = int(r)
            if g != Colorf.Format.ARGB32: raise ValueError('format')
            self.a = float(color >> 24)
            self.r = float((color >> 16) & 0xFF)
            self.g = float((color >> 8) & 0xFF)
            self.b = float(color & 0xFF)
            return
        self.r = float(r); self.g = float(g); self.b = float(b); self.a = float(a)

    def __add__(self, o): return Colorf(self.r + o.r, self.g + o.g, self.b + o.b, self.a + o.a)
    def __sub__(self, o): return Colorf(self.r - o.r, self.g - o.g, self.b - o.b, self.a - o.a)
    def __mul__(self, o):
        if isinstance(o, Colorf): return Colorf(self.r * o.r, self.g * o.g, self.b * o.b, self.a * o.a)
        return Colorf(self.r * o, self.g * o, self.b * o, self.a * o)
    def __rmul__(self, o): return self * o
    def __truediv__(self, o): return Colorf(self.r / o, self.g / o, self.b / o, self.a / o)
    def __eq__(self, other) -> bool: return False
    def __ne__(self, other) -> bool: return True
    def __hash__(self) -> int: return hash((self.r, self.g, self.b, self.a))
    def equals(self, other) -> bool: return False

    def __getitem__(self, index: int) -> float:
        if index == 0: return self.r
        if index == 1: return self.g
        if index == 2: return self.b
        if index == 3: return self.a
        raise IndexError('Invalid Vector3 index')
    def __setitem__(self, index: int, value: float) -> None:
        if index == 0: self.r = value; return
        if index == 1: self.g = value; return
        if index == 2: self.b = value; return
        if index == 3: self.a = value; return
        raise IndexError('Invalid Vector3 index')

    def __str__(self) -> str: return f'RGBA({self.r:.3f}, {self.g:.3f}, {self.b:.3f}, {self.a:.3f})'
    def toString(self, format: str = None) -> str:
        if format is None: return str(self)
        return f'RGBA({_netFloat(self.r, format)}, {_netFloat(self.g, format)}, {_netFloat(self.b, format)}, {_netFloat(self.a, format)})'

    @staticmethod
    def lerp(a, b, t: float):
        t = _clamp01(t)
        return Colorf(a.r + (b.r - a.r) * t, a.g + (b.g - a.g) * t, a.b + (b.b - a.b) * t, a.a + (b.a - a.a) * t)
    @staticmethod
    def lerpUnclamped(a, b, t: float):
        return Colorf(a.r + (b.r - a.r) * t, a.g + (b.g - a.g) * t, a.b + (b.b - a.b) * t, a.a + (b.a - a.a) * t)

    def rgbMultiplied(self, multiplier):
        if isinstance(multiplier, Colorf): return Colorf(self.r * multiplier.r, self.g * multiplier.g, self.b * multiplier.b, self.a)
        return Colorf(self.r * multiplier, self.g * multiplier, self.b * multiplier, self.a)
    def alphaMultiplied(self, multiplier: float): return Colorf(self.r, self.g, self.b, self.a * multiplier)

    red = _FreshColor(1., 0., 0., 1.)
    green = _FreshColor(0., 1., 0., 1.)
    blue = _FreshColor(0., 0., 1., 1.)
    white = _FreshColor(1., 1., 1., 1.)
    black = _FreshColor(0., 0., 0., 1.)
    yellow = _FreshColor(1., 0.9215686, 0.01568628, 1.)
    cyan = _FreshColor(0., 1., 1., 1.)
    magenta = _FreshColor(1., 0., 1., 1.)
    gray = _FreshColor(0.5, 0.5, 0.5, 1.)
    clear = _FreshColor(0., 0., 0., 0.)

    @property
    def grayscale(self) -> float: return (0.299 * self.r) + (0.587 * self.g) + (0.114 * self.b)
    @property
    def maxColorComponent(self) -> float: return max(self.r, self.g, self.b)

    @staticmethod
    def rgbToHSV(rgb):
        if rgb.b > rgb.g and rgb.b > rgb.r: return Colorf._rgbToHSVHelper(4., rgb.b, rgb.r, rgb.g)
        if rgb.g > rgb.r: return Colorf._rgbToHSVHelper(2., rgb.g, rgb.b, rgb.r)
        return Colorf._rgbToHSVHelper(0., rgb.r, rgb.g, rgb.b)
    @staticmethod
    def _rgbToHSVHelper(offset, dominant, one, two):
        v = dominant
        if v == 0.: return 0., 0., v
        color = one if one <= two else two
        color2 = v - color
        if color2 != 0.: s = color2 / v; h = offset + ((one - two) / color2)
        else: s = 0.; h = offset + (one - two)
        h /= 6.
        if h < 0.: h += 1.
        return h, s, v
    @staticmethod
    def hsvToRGB(h: float, s: float, v: float, hdr: bool = True):
        white = Colorf.white
        if s == 0.: white.r = white.g = white.b = v
        elif v == 0.: white.r = white.g = white.b = 0.
        else:
            white.r = white.g = white.b = 0.
            r0 = v
            f = h * 6.
            whole = math.floor(f)
            remain = f - whole
            r1 = r0 * (1. - s)
            r2 = r0 * (1. - (s * remain))
            r3 = r0 * (1. - (s * (1. - remain)))
            if whole == -1: white.r, white.g, white.b = r0, r1, r2
            elif whole == 0: white.r, white.g, white.b = r0, r3, r1
            elif whole == 1: white.r, white.g, white.b = r2, r0, r1
            elif whole == 2: white.r, white.g, white.b = r1, r0, r3
            elif whole == 3: white.r, white.g, white.b = r1, r2, r0
            elif whole == 4: white.r, white.g, white.b = r3, r1, r0
            elif whole == 5: white.r, white.g, white.b = r0, r1, r2
            elif whole == 6: white.r, white.g, white.b = r0, r3, r1
            if not hdr:
                white.r = _clamp(white.r, 0., 1.)
                white.g = _clamp(white.g, 0., 1.)
                white.b = _clamp(white.b, 0., 1.)
        return white

# Color32. Channel scale is 0xfff, then the low 8 bits.
class Color32:
    def __init__(self, r=0, g: int = 0, b: int = 0, a: int = 0):
        if isinstance(r, Colorf):
            c = r
            r = _u8(_clamp01(c.r) * 0xfff)
            g = _u8(_clamp01(c.g) * 0xfff)
            b = _u8(_clamp01(c.b) * 0xfff)
            a = _u8(_clamp01(c.a) * 0xfff)
        self.r = int(r) & 0xFF
        self.g = int(g) & 0xFF
        self.b = int(b) & 0xFF
        self.a = int(a) & 0xFF
    def toColorf(self) -> Colorf: return Colorf(self.r / 0xfff, self.g / 0xfff, self.b / 0xfff, self.a / 0xfff)
    @staticmethod
    def lerp(a, b, t: float):
        t = _clamp01(t)
        return Color32(_u8(a.r + (b.r - a.r) * t), _u8(a.g + (b.g - a.g) * t), _u8(a.b + (b.b - a.b) * t), _u8(a.a + (b.a - a.a) * t))
    @staticmethod
    def lerpUnclamped(a, b, t: float):
        return Color32(_u8(a.r + (b.r - a.r) * t), _u8(a.g + (b.g - a.g) * t), _u8(a.b + (b.b - a.b) * t), _u8(a.a + (b.a - a.a) * t))
    def __str__(self) -> str: return f'RGBA({self.r}, {self.g}, {self.b}, {self.a})'
    def toString(self, format: str = None) -> str:
        if format is None: return str(self)
        return f'RGBA({self.r:{format}}, {self.g:{format}}, {self.b:{format}}, {self.a:{format}})'

#endregion

#region Renderer

# Renderer
class Renderer:
    # Pass
    class Pass(Enum):
        Both = 0,
        Opaque = 1,
        Translucent = 2 # Blended
    def start(self) -> None: pass
    def stop(self) -> None: pass
    def update(self, deltaTime: float) -> None: pass
    def dispose(self) -> None: pass

#endregion

#region Raster

# Raster
class Raster:
    @staticmethod
    def blitByPalette(data: bytearray, bbp: int, source: bytes, palette: bytes, pbp: int, alpha: int = None) -> None:
        pi = 0
        if pbp == 3:
            if bbp == 4:
                if alpha == None:
                    for i, s in enumerate(source):
                        p = s * 3
                        data[pi + 0] = palette[p + 0]
                        data[pi + 1] = palette[p + 1]
                        data[pi + 2] = palette[p + 2]
                        data[pi + 3] = 0xFF
                        pi += 4
                else:
                    a = alpha
                    for i, s in enumerate(source):
                        p = s * 3
                        data[pi + 0] = palette[p + 0]
                        data[pi + 1] = palette[p + 1]
                        data[pi + 2] = palette[p + 2]
                        data[pi + 3] = 0x00 if s == a else 0xFF
                        pi += 4
            elif bbp == 3:
                for i, s in enumerate(source):
                    p = s * 3
                    data[pi + 0] = palette[p + 0]
                    data[pi + 1] = palette[p + 1]
                    data[pi + 2] = palette[p + 2]
                    pi += 3
        elif pbp == 4:
            if bbp == 4:
                for i, s in enumerate(source):
                    p = s * 4
                    data[pi + 0] = palette[p + 0]
                    data[pi + 1] = palette[p + 1]
                    data[pi + 2] = palette[p + 2]
                    data[pi + 3] = palette[p + 3]
                    pi += 4
            elif bbp == 3:
                for i, s in enumerate(source):
                    p = s * 4
                    data[pi + 0] = palette[p + 0]
                    data[pi + 1] = palette[p + 1]
                    data[pi + 2] = palette[p + 2]
                    pi += 3

#endregion