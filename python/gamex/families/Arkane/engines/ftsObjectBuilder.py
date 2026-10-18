from numpy import array
from openx.gfx.gfx import MaterialStd2Prop
from gamex.families.Arkane.formats.danae.eerieTypes import E_TEXTURE, POLY

# Shared mesh for an FTS map. Danae is Y-up, so positions stay as stored.
# A quad is a triangle strip: (0, 1, 2) then (1, 3, 2).
# level is Binary_Fts.F_LEVEL.

class FtsSubmesh:
    def __init__(self, texId: int, type: POLY, triangles: list[int]):
        self.texId = texId
        self.type = type
        self.triangles = triangles

class FtsMesh:
    def __init__(self):
        self.vertices = []
        self.normals = []
        self.uvs = []
        self.submeshes = []

def _isZero(v) -> bool:
    if v is None: return True
    return float(v[0]) == 0 and float(v[1]) == 0 and float(v[2]) == 0

def _texIndex(tex: E_TEXTURE, textures) -> int:
    if tex is None or not textures or not tex.id: return -1
    for i, t in enumerate(textures):
        if t is not None and t.id == tex.id: return i
    return -1

def getTexturePaths(level):
    textures = getattr(level, 'textures', None)
    if not textures: return
    for t in textures:
        if t is not None and t.path: yield t.path

def toMaterialProp(tex: E_TEXTURE, type: POLY = POLY.NONE_) -> MaterialStd2Prop:
    mp = MaterialStd2Prop()
    mp.textures = {}
    mp.alphaBlended = False
    mp.alpha = 0
    flags = type
    if tex is not None:
        if tex.path: mp.textures['Main'] = tex.path
        flags = type | tex.poly
    if flags & (POLY.TRANS | POLY.WATER):
        mp.alphaBlended = True
        mp.alpha = 1
    return mp

def scenePositions(level):
    return [('player', level.playerPos), ('mscene', level.mscenePos)]

def toGeometry(level) -> FtsMesh:
    vertices = []
    normals = []
    uvs = []
    order = []
    tris = {}
    types = {}
    cells = getattr(level, 'backg', None)
    if cells:
        for cell in cells:
            polydata = getattr(cell, 'polydata', None)
            if not polydata: continue
            for poly in polydata:
                if poly is None: continue
                tex = poly.tex
                if poly.type & (POLY.HIDE | POLY.NODRAW): continue
                if tex is not None and (tex.poly & (POLY.HIDE | POLY.NODRAW)): continue
                quad = bool(poly.type & POLY.QUAD)
                count = 4 if quad else 3
                if not poly.v or len(poly.v) < count: continue
                texId = _texIndex(tex, getattr(level, 'textures', None))
                if texId not in tris:
                    tris[texId] = []
                    order.append(texId)
                    types[texId] = POLY.NONE_
                lst = tris[texId]
                types[texId] |= poly.type & (POLY.TRANS | POLY.WATER)
                base = len(vertices)
                for c in range(count):
                    src = poly.v[c]
                    vertices.append(src.s)
                    if poly.nrml is not None and c < len(poly.nrml): n = poly.nrml[c]
                    elif not _isZero(poly.norm): n = poly.norm
                    else: n = array([0., 0., 0.], dtype='float32')
                    normals.append(n)
                    uvs.append(array([float(src.t[0]), float(src.t[1])], dtype='float32'))
                lst.extend((base, base + 1, base + 2))
                if quad: lst.extend((base + 1, base + 3, base + 2))
                if poly.type & POLY.DOUBLESIDED:
                    lst.extend((base, base + 2, base + 1))
                    if quad: lst.extend((base + 1, base + 2, base + 3))
    mesh = FtsMesh()
    mesh.vertices = vertices
    mesh.normals = normals
    mesh.uvs = uvs
    mesh.submeshes = [FtsSubmesh(texId, types[texId], tris[texId]) for texId in order]
    return mesh
