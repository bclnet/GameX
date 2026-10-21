from __future__ import annotations
from openx.core import ISource, IHaveSource
from openx.engines.diligent.gfx.diligent_render import TestTriRenderer, TextureRenderer
from openx.engines.diligent.gfx.diligent import DiligentX
from openx.engines.diligent.diligent_shim import DiligentShim
from gamex.families.Arkane.formats.danae.binary import Binary_Ftl, Binary_Fts
from gamex.families.Arkane.engines.ftlObjectBuilder import getTexturePaths as ftlPaths, groupPositions, toGeometry as ftlGeometry, toMaterialProp as ftlMaterial
from gamex.families.Arkane.engines.ftsObjectBuilder import getTexturePaths as ftsPaths, scenePositions, toGeometry as ftsGeometry, toMaterialProp as ftsMaterial
from gamex.families.Gamebryo.formats.binary import Binary_Nif
from gamex.families.Gamebryo.formats.nif import NiNode, NiTriShape, NiTriShapeData, RootCollisionNode
from gamex.families.Gamebryo.engines.nifobjectbuilder import NifObjectBuilder

class IOpenGfx: pass
class Renderer: pass

# Holder. Danae is Y-up, so FTL and FTS positions stay as stored. No GPU upload.
class DiligentObjectMaterials:
    def __init__(self, paths, props):
        self.paths = paths
        self.props = props

class DiligentNifShape:
    def __init__(self, name, positions, indices):
        self.name = name
        self.positions = positions
        self.indices = indices

class DiligentObject:
    def __init__(self, name, isStatic, mesh, materials, groups):
        self.name = name
        self.isStatic = isStatic
        self.mesh = mesh
        self.materials = materials
        self.groups = groups

def _copyPositions(verts):
    if verts is None: return []
    copy = getattr(verts, 'copy', None)
    return copy() if copy is not None else list(verts)

def buildFtl(path, isStatic, materialManager):
    obj = path.obj
    name = obj.file if obj.file else 'ftl'
    mesh = ftlGeometry(obj)
    paths = list(ftlPaths(obj))
    props = []
    textures = obj.textures
    for sub in mesh.submeshes:
        tex = None
        if textures is not None and sub.texId is not None and 0 <= sub.texId < len(textures): tex = textures[sub.texId]
        props.append(ftlMaterial(tex))
    return DiligentObject(name, isStatic, mesh, DiligentObjectMaterials(paths, props), groupPositions(obj))

def buildFts(path, isStatic, materialManager):
    level = path.level
    mesh = ftsGeometry(level)
    paths = list(ftsPaths(level))
    props = []
    textures = getattr(level, 'textures', None)
    for sub in mesh.submeshes:
        texId = -1 if sub.texId is None else int(sub.texId)
        tex = None
        if textures is not None and 0 <= texId < len(textures): tex = textures[texId]
        props.append(ftsMaterial(tex, sub.type))
    return DiligentObject('fts', isStatic, mesh, DiligentObjectMaterials(paths, props), scenePositions(level))

def _walkNif(node, shapes, props):
    if node is None or isinstance(node, RootCollisionNode): return
    if isinstance(node, NiTriShape):
        if node.skinInstance is not None: return
        data = node.data.value if node.data is not None else None
        if not isinstance(data, NiTriShapeData): return
        positions = _copyPositions(getattr(data, 'vertices', None))
        indices = []
        tris = getattr(data, 'triangles', None)
        if tris:
            for t in tris: indices.extend((int(t.v1), int(t.v2), int(t.v3)))
        shapes.append(DiligentNifShape(node.name, positions, indices))
        props.append(NifObjectBuilder.toMaterialProp(node))
        return
    if isinstance(node, NiNode) and node.children:
        for child in node.children:
            if child is not None: _walkNif(child.value, shapes, props)

def buildNif(path, isStatic, materialManager):
    shapes = []
    props = []
    roots = getattr(path, 'roots', None)
    if roots:
        for root in roots:
            if root is not None: _walkNif(root.value, shapes, props)
    name = path.name if getattr(path, 'name', None) else 'nif'
    return DiligentObject(name, isStatic, shapes, DiligentObjectMaterials([], props), [])

DiligentX.buildersByType['Binary_Nif'] = buildNif
DiligentX.buildersByType[Binary_Nif] = buildNif
DiligentX.buildersByType['Binary_Ftl'] = buildFtl
DiligentX.buildersByType[Binary_Ftl] = buildFtl
DiligentX.buildersByType['Binary_Fts'] = buildFts
DiligentX.buildersByType[Binary_Fts] = buildFts

# ObjectRenderer. start looks up the builder for obj's type. An unknown type does nothing.
class ObjectRenderer:
    def __init__(self, gfx, source, obj):
        self.gfx = gfx
        self.source = source
        self.obj = obj
        self.built = None

    def start(self) -> None:
        obj = self.obj
        if obj is None: return
        table = DiligentX.buildersByType
        build = table.get(type(obj))
        if build is None: build = table.get(type(obj).__name__)
        if build is None: return
        self.built = build(obj, True, None)

    def update(self, deltaTime: float) -> None:
        return

# DiligentRenderer
class DiligentRenderer:
    @staticmethod
    def diligentLinked() -> bool: return DiligentShim.linked()
    @staticmethod
    def diligentVersion() -> str: return DiligentShim.version()
    @staticmethod
    def createRenderer(parent: object, gfx: list[IOpenGfx], source: ISource, obj: object, type: str) -> Renderer:
        if isinstance(obj, IHaveSource): source = obj.source
        match type:
            case 'TestTri': return TestTriRenderer(gfx, source, obj)
            case 'Texture': return TextureRenderer(gfx, source, obj, slice(0, None))
            case 'Object': return ObjectRenderer(gfx, source, obj)
            case _: return None
