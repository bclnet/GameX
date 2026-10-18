from numpy import array
from openx.gfx.gfx import MaterialStd2Prop
from gamex.families.Arkane.formats.danae.eerieTypes import E_3DOBJ, E_TEXTURE, POLY

# Shared mesh for an FTL. Danae is Y-up, so positions stay as stored.

class FtlSubmesh:
    def __init__(self, texId: int, triangles: list[int]):
        self.texId = texId
        self.triangles = triangles

class FtlMesh:
    def __init__(self):
        self.vertices = []
        self.normals = []
        self.uvs = []
        self.submeshes = []

def _isZero(v) -> bool:
    if v is None: return True
    return float(v[0]) == 0 and float(v[1]) == 0 and float(v[2]) == 0

def getTexturePaths(obj: E_3DOBJ):
    if not obj.textures: return
    for t in obj.textures:
        if t is not None and t.path: yield t.path

def toMaterialProp(tex: E_TEXTURE) -> MaterialStd2Prop:
    mp = MaterialStd2Prop()
    mp.textures = {}
    mp.alphaBlended = False
    mp.alpha = 0
    if tex is None: return mp
    if tex.path: mp.textures['Main'] = tex.path
    if tex.poly & (POLY.TRANS | POLY.WATER):
        mp.alphaBlended = True
        mp.alpha = 1
    return mp

def groupPositions(obj: E_3DOBJ):
    if not obj.groups or not obj.vertexs: return []
    return [(g.name, obj.vertexs[g.origin].v) for g in obj.groups]

def toGeometry(obj: E_3DOBJ) -> FtlMesh:
    vertices = []
    normals = []
    uvs = []
    order = []
    tris = {}
    if obj.faces and obj.vertexs:
        for face in obj.faces:
            if face is None: continue
            tex = None
            if obj.textures and face.texId >= 0 and face.texId < len(obj.textures): tex = obj.textures[face.texId]
            if tex is not None and (tex.poly & (POLY.HIDE | POLY.NODRAW)): continue
            if face.texId not in tris:
                tris[face.texId] = []
                order.append(face.texId)
            lst = tris[face.texId]
            ids = [int(face.vid[0]), int(face.vid[1]), int(face.vid[2])]
            us = [float(face.u[0]), float(face.u[1]), float(face.u[2])]
            vs = [float(face.v[0]), float(face.v[1]), float(face.v[2])]
            base = len(vertices)
            for c in range(3):
                src = obj.vertexs[ids[c]]
                vertices.append(src.v)
                if face.nrmls is not None and c < len(face.nrmls): n = face.nrmls[c]
                elif not _isZero(face.norm): n = face.norm
                else: n = src.norm
                normals.append(n)
                uvs.append(array([us[c], vs[c]], dtype='float32'))
            lst.extend((base, base + 1, base + 2))
            if face.faceType == 2 or (tex is not None and (tex.poly & POLY.DOUBLESIDED)): lst.extend((base, base + 2, base + 1))
    mesh = FtlMesh()
    mesh.vertices = vertices
    mesh.normals = normals
    mesh.uvs = uvs
    mesh.submeshes = [FtlSubmesh(texId, tris[texId]) for texId in order]
    return mesh
