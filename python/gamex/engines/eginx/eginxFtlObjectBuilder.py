from gamex.families.Arkane.engines.ftlObjectBuilder import getTexturePaths, groupPositions, toGeometry, toMaterialProp
from gamex.engines.eginx.eginxObjectHolder import EginXObjectHolder

# An FTL kept as the shared mesh. Danae is Y-up, so positions stay as stored.
# materialManager may be null. Texture paths are not uploaded.

class EginXFtlObjectBuilder:
    @staticmethod
    def buildObject(path: object, isStatic: bool, materialManager: object) -> EginXObjectHolder:
        obj = path.obj
        name = obj.file if obj.file else 'ftl'
        for texturePath in getTexturePaths(obj):
            if materialManager is not None: _ = texturePath
        mesh = toGeometry(obj)
        materials = []
        for sub in mesh.submeshes:
            tex = None
            if obj.textures and sub.texId >= 0 and sub.texId < len(obj.textures): tex = obj.textures[sub.texId]
            materials.append(toMaterialProp(tex))
        return EginXObjectHolder(name, isStatic, mesh, materials, groupPositions(obj))
