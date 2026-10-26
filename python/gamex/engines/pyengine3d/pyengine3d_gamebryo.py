from openx.core import ISource
from openx.gfx.gfx import MaterialManager
from gamex.families.Gamebryo.formats.binary import Binary_Nif
from gamex.families.Gamebryo.engines.nifobjectbuilder import NifObjectBuilder

# Copied NiTriShape vertices and indices. Parsing stays in Binary_Nif.

class PyEngine3dNifShape:
    def __init__(self, vertices, indices, material):
        self.vertices = vertices
        self.indices = indices
        self.material = material

class PyEngine3dNifObject:
    def __init__(self, name: str, isStatic: bool, rootCount: int, shapes: list):
        self.name = name
        self.isStatic = isStatic
        self.rootCount = rootCount
        self.shapes = shapes

def _deref(node):
    if node is None: return None
    return node.value if hasattr(node, 'val') else node

def _indices(triangles) -> list[int]:
    if not triangles: return []
    indices = []
    for t in triangles:
        if hasattr(t, 'v1'): indices.extend((int(t.v1), int(t.v2), int(t.v3)))
        else:
            for i in t: indices.append(int(i))
    return indices

def _walk(node, shapes: list, seen: set) -> None:
    s = _deref(node)
    if s is None: return
    mark = id(s)
    if mark in seen: return
    seen.add(mark)
    data = _deref(getattr(s, 'data', None))
    vertices = getattr(data, 'vertices', None) if data is not None else None
    triangles = getattr(data, 'triangles', None) if data is not None else None
    if vertices is not None or triangles is not None:
        material = None
        toMaterial = getattr(NifObjectBuilder, 'toMaterialProp', None)
        if toMaterial is not None and getattr(s, 'properties', None): material = toMaterial(s)
        shapes.append(PyEngine3dNifShape(list(vertices) if vertices is not None else [], _indices(triangles), material))
    children = getattr(s, 'children', None)
    if children:
        for child in children: _walk(child, shapes, seen)

class PyEngine3dNifObjectBuilder:
    @staticmethod
    async def buildObject(source: ISource, path: object, isStatic: bool, materialManager: MaterialManager) -> PyEngine3dNifObject:
        nif: Binary_Nif = path
        name = getattr(nif, 'name', None) or 'nif'
        roots = getattr(nif, 'roots', None) or []
        if materialManager is not None and hasattr(nif, 'getTexturePaths'):
            textureManager = materialManager._textureManager
            for texturePath in nif.getTexturePaths(): textureManager.preload(source, texturePath)
        shapes = []
        seen = set()
        for root in roots: _walk(root, shapes, seen)
        return PyEngine3dNifObject(name, isStatic, len(roots), shapes)
