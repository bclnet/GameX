from gamex.families.Gamebryo.formats.nif import NiNode, NiTriShape, NiTriShapeData
from gamex.families.Gamebryo.engines.nifobjectbuilder import NifObjectBuilder
from gamex.engines.eginx.eginxObjectHolder import EginXObjectHolder

# Copies NiTriShape positions and indices. No colliders, no skinning, no NIF reparse.
# materialManager may be null.

class EginXNifShape:
    def __init__(self, name, positions, indices):
        self.name = name
        self.positions = positions
        self.indices = indices

class EginXNifObjectBuilder:
    @staticmethod
    def buildObject(path: object, isStatic: bool, materialManager: object) -> EginXObjectHolder:
        _ = materialManager
        name = path.name if getattr(path, 'name', None) else 'nif'
        shapes = []
        materials = []
        roots = getattr(path, 'roots', None)
        if roots:
            for root in roots:
                if root is not None: EginXNifObjectBuilder.walk(root.value, shapes, materials)
        return EginXObjectHolder(name, isStatic, shapes, materials, [])

    @staticmethod
    def walk(node, shapes, materials):
        if node is None: return
        if isinstance(node, NiNode):
            children = getattr(node, 'children', None)
            if not children: return
            for child in children:
                if child is not None: EginXNifObjectBuilder.walk(child.value, shapes, materials)
            return
        if isinstance(node, NiTriShape): EginXNifObjectBuilder.copyShape(node, shapes, materials)

    @staticmethod
    def copyShape(shape, shapes, materials):
        data = shape.data.value if getattr(shape, 'data', None) is not None else None
        if not isinstance(data, NiTriShapeData): return
        verts = getattr(data, 'vertices', None)
        positions = [v.copy() if hasattr(v, 'copy') else v for v in verts] if verts is not None else []
        indices = []
        triangles = getattr(data, 'triangles', None)
        if triangles:
            for tri in triangles:
                indices.extend((int(tri.v1), int(tri.v2), int(tri.v3)))
        shapes.append(EginXNifShape(getattr(shape, 'name', None), positions, indices))
        props = getattr(shape, 'properties', None)
        materials.append(NifObjectBuilder.toMaterialProp(shape) if props is not None else None)
