from numpy import array
from openx.gfx.gfx import MaterialStd2Prop

# Shared mesh for a Quake BSP. Quake is Z-up: a point is (x, z, y) * WorldScale, a direction is (x, z, y).

WorldScale = 1 / 32
_SKIP = ('skip', 'hint', 'origin', 'trigger', 'clip')

class BspSubmesh:
    def __init__(self, texId: int, triangles: list[int]):
        self.texId = texId
        self.triangles = triangles

class BspMesh:
    def __init__(self):
        self.vertices = []
        self.normals = []
        self.uvs = []
        self.submeshes = []

def getTexturePaths(bsp):
    for tex in bsp.h1.textures:
        if tex is not None and tex.name: yield tex.name

def toMaterialProp(tex) -> MaterialStd2Prop:
    mp = MaterialStd2Prop()
    mp.textures = {}
    if tex is not None and tex.name: mp.textures['Main'] = tex.name
    return mp

def _point(q): return (q[0] * WorldScale, q[2] * WorldScale, q[1] * WorldScale)

def _direction(q): return (q[0], q[2], q[1])

def toGeometry(bsp) -> BspMesh:
    h1 = bsp.h1
    vertices = []
    normals = []
    uvs = []
    order = []
    tris = {}
    faces = h1.faces
    if h1.models:
        model = h1.models[0]
        faces = faces[model.firstFace:model.firstFace + model.numFaces]
    for face in faces:
        info = h1.texinfo[face.texinfo]
        tex = h1.textures[info.miptex] if 0 <= info.miptex < len(h1.textures) else None
        if tex is not None and tex.name and tex.name.lower() in _SKIP: continue
        normal = h1.planes[face.planeNum].normal
        if face.side != 0: normal = (-normal[0], -normal[1], -normal[2])
        normal = _direction(normal)
        width = tex.width if tex is not None and tex.width else 256
        height = tex.height if tex is not None and tex.height else 256
        v0, v1 = info.vecs0, info.vecs1
        mip = info.miptex
        if mip not in tris:
            tris[mip] = []
            order.append(mip)
        lst = tris[mip]
        base = len(vertices)
        for i in range(face.numEdges):
            se = h1.surfEdges[face.firstEdge + i]
            vi = h1.edges[se][0] if se >= 0 else h1.edges[-se][1]
            q = h1.vertices[vi]
            vertices.append(_point(q))
            normals.append(normal)
            s = q[0] * v0[0] + q[1] * v0[1] + q[2] * v0[2] + v0[3]
            t = q[0] * v1[0] + q[1] * v1[1] + q[2] * v1[2] + v1[3]
            uvs.append(array([s / width, t / height], dtype='float32'))
        for k in range(1, face.numEdges - 1): lst.extend((base, base + k, base + k + 1))
    mesh = BspMesh()
    mesh.vertices = vertices
    mesh.normals = normals
    mesh.uvs = uvs
    mesh.submeshes = [BspSubmesh(texId, tris[texId]) for texId in order]
    return mesh
