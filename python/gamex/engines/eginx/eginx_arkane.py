from gamex.families.Arkane.engines.ftlObjectBuilder import getTexturePaths as ftlGetTexturePaths, groupPositions, toGeometry as ftlToGeometry, toMaterialProp as ftlToMaterialProp
from gamex.engines.eginx.eginxObjectHolder import EginXObjectHolder
from gamex.families.Arkane.engines.ftsObjectBuilder import getTexturePaths as ftsGetTexturePaths, scenePositions, toGeometry as ftsToGeometry, toMaterialProp as ftsToMaterialProp

# An FTL kept as the shared mesh. Danae is Y-up, so positions stay as stored.
# materialManager may be null. Texture paths are not uploaded.

class EginXFtlObjectBuilder:
    @staticmethod
    def buildObject(path: object, isStatic: bool, materialManager: object) -> EginXObjectHolder:
        obj = path.obj
        name = obj.file if obj.file else 'ftl'
        for texturePath in ftlGetTexturePaths(obj):
            if materialManager is not None: _ = texturePath
        mesh = ftlToGeometry(obj)
        materials = []
        for sub in mesh.submeshes:
            tex = None
            if obj.textures and sub.texId >= 0 and sub.texId < len(obj.textures): tex = obj.textures[sub.texId]
            materials.append(ftlToMaterialProp(tex))
        return EginXObjectHolder(name, isStatic, mesh, materials, groupPositions(obj))

# An FTS map kept as one mesh. Danae is Y-up, so positions stay as stored.
# materialManager may be null. Texture paths are not uploaded.

class EginXFtsObjectBuilder:
    @staticmethod
    def buildObject(path: object, isStatic: bool, materialManager: object) -> EginXObjectHolder:
        level = path.level
        name = 'fts'
        for texturePath in ftsGetTexturePaths(level):
            if materialManager is not None: _ = texturePath
        mesh = ftsToGeometry(level)
        materials = []
        for sub in mesh.submeshes:
            tex = None
            textures = getattr(level, 'textures', None)
            if textures and sub.texId >= 0 and sub.texId < len(textures): tex = textures[sub.texId]
            materials.append(ftsToMaterialProp(tex, sub.type))
        return EginXObjectHolder(name, isStatic, mesh, materials, scenePositions(level))
