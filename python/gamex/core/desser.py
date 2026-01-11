from __future__ import annotations
import os, io, json
from enum import Enum, Flag, IntFlag
from numpy import ndarray, array
from quaternion import quaternion
from gamex.core.globalx import Color3, Color4

def jsonKey(s: str) -> str: # CamelCase
    if not s: return s
    cs = [c for c in s]; clen = len(cs)
    for i in range(clen):
        if i == 1 and not cs[i].isupper(): break
        if i > 0 and i + 1 < clen and not cs[i + 1].isupper(): # stop when next char is already lowercase.
            if cs[i + 1] == ' ': cs[i] = cs[i].lower() # if the next char is a space, lowercase current char before exiting.
            break
        cs[i] = cs[i].lower()
    s = ''.join(cs)
    return s
def jsonValue(s: object) -> object: return s #return s.replace('\'', TICK) if isinstance(s, str) else s

class FloatEncoder(float):
    def __repr__(self): return format(self, '.9g')

class CustomEncoder(json.JSONEncoder):
    def default(self, s):
        match s:
            # case float(): return f'{s:.9g}' #float
            case Enum(): return s.value #enum
            case quaternion(): return f'{s.x:.9g} {s.y:.9g} {s.z:.9g} {s.w:.9g}' #quaternion
            case ndarray():
                match (len(s.shape), s.shape[0]):
                    case (1, 2): return f'{s[0]:.9g} {s[1]:.9g}' #vector2
                    case (1, 3): return f'{s[0]:.9g} {s[1]:.9g} {s[2]:.9g}' #vector3
                    case (1, 4): return f'{s[0]:.9g} {s[1]:.9g} {s[2]:.9g} {s[3]:.9g}' #vector4
                    case (2, 2): return [
                        f'{s[0][0]:.9g} {s[0][1]:.9g}',
                        f'{s[1][0]:.9g} {s[1][1]:.9g}']  #matrix2x2
                    case (2, 3): return [
                        f'{s[0][0]:.9g} {s[0][1]:.9g} {s[0][2]:.9g}',
                        f'{s[1][0]:.9g} {s[1][1]:.9g} {s[1][2]:.9g}',
                        f'{s[2][0]:.9g} {s[2][1]:.9g} {s[2][2]:.9g}']  #matrix3x3
                    # case (2, 3): return [
                    #     f'{s[0][0]:.9g} {s[0][1]:.9g} {s[0][2]:.9g}',
                    #     f'{s[1][0]:.9g} {s[1][1]:.9g} {s[1][2]:.9g}',
                    #     f'{s[2][0]:.9g} {s[2][1]:.9g} {s[2][2]:.9g}']  #matrix3x4
                    case (2, 4): return [
                        f'{s[0][0]:.9g} {s[0][1]:.9g} {s[0][2]:.9g} {s[0][3]:.9g}',
                        f'{s[1][0]:.9g} {s[1][1]:.9g} {s[1][2]:.9g} {s[1][3]:.9g}',
                        f'{s[2][0]:.9g} {s[2][1]:.9g} {s[2][2]:.9g} {s[2][3]:.9g}',
                        f'{s[3][0]:.9g} {s[3][1]:.9g} {s[3][2]:.9g} {s[3][3]:.9g}']  #matrix4x4
                    case _: raise Exception('Unknown mapping')
            case Color3(): return f'{s.r:.9g} {s.g:.9g} {s.b:.9g}' #color3
            case Color4(): return f'{s.r:.9g} {s.g:.9g} {s.b:.9g} {s.a:.9g}' #color4
        n = type(s).__name__; c = {jsonKey(k):jsonValue(getattr(s, k)) for k in dir(s) if not k.startswith('_')}; d = {k:v for k,v in c.items() if not callable(v)}
        match n:
            case 'mappingproxy': return None
            case 'Binary_Nif': return {jsonKey(k):jsonValue(v) for k,v in d.items() if not k in ['f', 'length']}
        if n in DesSer.adds: return DesSer.adds[n](s)
        return d #super().default(s)

class DesSer:
    adds: dict = {}
    @staticmethod
    def add(s: dict) -> None: DesSer.adds = s
    @staticmethod
    def serialize(obj: object, w: io.BufferedWriter=None) -> str:
        if hasattr(json.encoder, 'c_make_encoder'): json.encoder.c_make_encoder = None
        json.encoder.float = FloatEncoder
        if w: return json.dump(obj, w, cls=CustomEncoder, sort_keys=True, indent=2)
        else: return json.dumps(obj, cls=CustomEncoder, sort_keys=True, indent=2)