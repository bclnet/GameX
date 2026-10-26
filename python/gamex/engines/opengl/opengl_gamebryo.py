from openx.core import ISource
from openx.gfx.gfx import MaterialManager
from gamex.families.Gamebryo.formats.binary import Binary_Nif
from gamex.families.Gamebryo.formats.nif import NiNode, NiTriShape, NiTriShapeData, RootCollisionNode
from gamex.families.Gamebryo.engines.nifobjectbuilder import NifObjectBuilder

# Thin OpenGL NIF. Walks Binary_Nif roots and copies NiTriShape vertex and triangle
# data. Colliders (RootCollisionNode) and skinned shapes are skipped.
# Props stay MaterialStd2Prop from NifObjectBuilder.toMaterialProp. create is not called.

class OpenGLNifMesh:
    def __init__(self, name, vertices, normals, uvs, triangles, material):
        self.name = name
        self.vertices = vertices
        self.normals = normals
        self.uvs = uvs
        self.triangles = triangles
        self.material = material

class _NifSub:
    def __init__(self, triangles):
        self.triangles = triangles

class _NifDraw:
    def __init__(self, vertices, triangles):
        self.vertices = vertices
        self.submeshes = [_NifSub(triangles)]

class OpenGLNifObject:
    def __init__(self, name, isStatic, rootCount, meshes):
        self.name = name
        self.isStatic = isStatic
        self.rootCount = rootCount
        self.meshes = meshes

    def drawGl(self) -> None:
        from gamex.engines.opengl.opengl_arkane import drawMesh
        for mesh in self.meshes or []:
            tris = []
            for tri in mesh.triangles or []: tris.extend(tri)
            if mesh.vertices and tris: drawMesh(_NifDraw(mesh.vertices, tris))

def _walk(node, meshes: list) -> None:
    if node is None: return
    if isinstance(node, RootCollisionNode): return
    if isinstance(node, NiTriShape):
        if getattr(node, 'skin', None) or getattr(node, 'skinInstance', None): return
        dataRef = node.data
        data = dataRef.value if dataRef else None
        if not isinstance(data, NiTriShapeData) or not data.vertices: return
        scale = NifObjectBuilder.MeterInUnits
        vertices = [v / scale for v in data.vertices]
        normals = list(data.normals) if data.normals else []
        uvs = [(t.u, t.v) for t in data.uvSets[0]] if data.uvSets else []
        # C# OpenGLNifObjectBuilder.ToGeometry stores v1, v3, v2.
        triangles = [(t.v1, t.v3, t.v2) for t in data.triangles] if data.triangles else []
        material = NifObjectBuilder.toMaterialProp(node) if getattr(node, 'properties', None) else None
        meshes.append(OpenGLNifMesh(node.name, vertices, normals, uvs, triangles, material))
        return
    if isinstance(node, NiNode):
        for child in node.children or []:
            if child: _walk(child.value, meshes)

class OpenGLNifObjectBuilder:
    @staticmethod
    async def buildObject(source: ISource, path: object, isStatic: bool, materialManager: MaterialManager) -> OpenGLNifObject:
        o: Binary_Nif = path
        name = o.name
        textureManager = materialManager._textureManager
        for texturePath in o.getTexturePaths(): textureManager.preload(source, texturePath)
        meshes = []
        roots = o.roots or []
        for root in roots:
            if root: _walk(root.value, meshes)
        return OpenGLNifObject(name, isStatic, len(roots), meshes)
