from gamex.families.Arkane.engines.ftsObjectBuilder import getTexturePaths, scenePositions, toGeometry, toMaterialProp
from gamex.engines.eginx.eginxObjectHolder import EginXObjectHolder

# An FTS map kept as one mesh. Danae is Y-up, so positions stay as stored.
# materialManager may be null. Texture paths are not uploaded.

class EginXFtsObjectBuilder:
    @staticmethod
    def buildObject(path: object, isStatic: bool, materialManager: object) -> EginXObjectHolder:
        level = path.level
        name = 'fts'
        for texturePath in getTexturePaths(level):
            if materialManager is not None: _ = texturePath
        mesh = toGeometry(level)
        materials = []
        for sub in mesh.submeshes:
            tex = None
            textures = getattr(level, 'textures', None)
            if textures and sub.texId >= 0 and sub.texId < len(textures): tex = textures[sub.texId]
            materials.append(toMaterialProp(tex, sub.type))
        return EginXObjectHolder(name, isStatic, mesh, materials, scenePositions(level))
