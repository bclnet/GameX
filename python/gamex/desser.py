from __future__ import annotations
import os, io, json
from enum import Enum, Flag, IntFlag
from numpy import ndarray, array
from quaternion import quaternion
from gamex.globalx import Color3, Color4

class FloatEncoder(float):
    def __repr__(self): return format(self, '.4f')

class CustomEncoder(json.JSONEncoder):
    def default(self, s):
        match s:
            # case float(): return f'{s:.4f}' #float
            case Enum(): return s.value #enum
            case quaternion(): return f'{s.x:.4f} {s.y:.4f} {s.z:.4f} {s.w:.4f}' #quaternion
            case ndarray():
                match (len(s.shape), s.shape[0]):
                    case (1, 2): return f'{s[0]:.4f} {s[1]:.4f}' #vector2
                    case (1, 3): return f'{s[0]:.4f} {s[1]:.4f} {s[2]:.4f}' #vector3
                    case (1, 4): return f'{s[0]:.4f} {s[1]:.4f} {s[2]:.4f} {s[3]:.4f}' #vector4
                    case (2, 2): return [
                        f'{s[0][0]:.4f} {s[0][1]:.4f}',
                        f'{s[1][0]:.4f} {s[1][1]:.4f}']  #matrix2x2
                    case (2, 3): return [
                        f'{s[0][0]:.4f} {s[0][1]:.4f} {s[0][2]:.4f}',
                        f'{s[1][0]:.4f} {s[1][1]:.4f} {s[1][2]:.4f}',
                        f'{s[2][0]:.4f} {s[2][1]:.4f} {s[2][2]:.4f}']  #matrix3x3
                    # case (2, 3): return [
                    #     f'{s[0][0]:.4f} {s[0][1]:.4f} {s[0][2]:.4f}',
                    #     f'{s[1][0]:.4f} {s[1][1]:.4f} {s[1][2]:.4f}',
                    #     f'{s[2][0]:.4f} {s[2][1]:.4f} {s[2][2]:.4f}']  #matrix3x4
                    case (2, 4): return [
                        f'{s[0][0]:.4f} {s[0][1]:.4f} {s[0][2]:.4f} {s[0][3]:.4f}',
                        f'{s[1][0]:.4f} {s[1][1]:.4f} {s[1][2]:.4f} {s[1][3]:.4f}',
                        f'{s[2][0]:.4f} {s[2][1]:.4f} {s[2][2]:.4f} {s[2][3]:.4f}',
                        f'{s[3][0]:.4f} {s[3][1]:.4f} {s[3][2]:.4f} {s[3][3]:.4f}']  #matrix4x4
                    case _: raise Exception('Unknown mapping')
            case Color3(): return f'{s.r:.4f} {s.g:.4f} {s.b:.4f}' #color3
            case Color4(): return f'{s.r:.4f} {s.g:.4f} {s.b:.4f} {s.a:.4f}' #color4
        n = type(s).__name__; c = {k:getattr(s, k) for k in dir(s) if not k.startswith('_')}; d = {k:v for k,v in c.items() if not callable(v)}
        match n:
            case 'mappingproxy': return None
            case 'Binary_Nif': return {k:v for k,v in d.items() if not k in ['f', 'length']}
        if n in DesSer.adds: return DesSer.adds[n](s)
        return d #super().default(s)

class DesSer:
    adds: dict
    @staticmethod
    def add(s: dict) -> None: DesSer.adds = s
    @staticmethod
    def serialize(obj: object, w: io.BufferedWriter=None) -> str:
        if hasattr(json.encoder, 'c_make_encoder'): json.encoder.c_make_encoder = None
        json.encoder.float = FloatEncoder
        if w: return json.dump(obj, w, cls=CustomEncoder, sort_keys=True, indent=2)
        else: return json.dumps(obj, cls=CustomEncoder, sort_keys=True, indent=2)