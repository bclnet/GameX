from __future__ import annotations
import os, io, numpy as np
import json

class CustomEncoder(json.JSONEncoder):
    def default(self, s):
        if isinstance(s, np.ndarray):
            match (len(s.shape), s.shape[0]):
                case (1, 2): return f'{s[0]} {s[1]}'
                case (1, 3): return f'{s[0]} {s[1]} {s[2]}'
                case (1, 4): return f'{s[0]} {s[1]} {s[2]} {s[3]}'
                case (2, 3): return [
                    f'{s[0][0]} {s[0][1]} {s[0][2]}',
                    f'{s[1][0]} {s[1][1]} {s[1][2]}',
                    f'{s[2][0]} {s[2][1]} {s[2][2]}']
                case (2, 4): return [
                    f'{s[0][0]} {s[0][1]} {s[0][2]} {s[0][3]}',
                    f'{s[1][0]} {s[1][1]} {s[1][2]} {s[1][3]}',
                    f'{s[2][0]} {s[2][1]} {s[2][2]} {s[2][3]}',
                    f'{s[3][0]} {s[3][1]} {s[3][2]} {s[3][3]}']
                case _: raise Exception('Unknown mapping')
        return s.__dict__
        # if isinstance(s, complex):
        #     return [s.real, obj.imag]
        # return super().default(s)

class DesSer:
    @staticmethod
    def serialize(obj: object, w: io.BufferedWriter=None) -> str:
        if w: return json.dump(obj, w, cls=CustomEncoder, indent=2)
        else: return json.dumps(obj, cls=CustomEncoder, indent=2)