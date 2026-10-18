from openx.core import ISource
from openx.gfx.gfx import MaterialManager
from gamex.families.Arkane.engines.ftlObjectBuilder import getTexturePaths, groupPositions, toGeometry, toMaterialProp

# An FTL kept as the shared mesh. Danae is Y-up, so positions stay as stored.
# OpenGL's material builder only accepts MaterialShaderVProp, so the props stay MaterialStd2Prop.

class OpenGLFtlObject:
    def __init__(self, name: str, isStatic: bool, data, materials, groups):
        self.name = name
        self.isStatic = isStatic
        self.data = data
        self.materials = materials
        self.groups = groups

class OpenGLFtlObjectBuilder:
    @staticmethod
    async def buildObject(source: ISource, path: object, isStatic: bool, materialManager: MaterialManager) -> OpenGLFtlObject:
        obj = path.obj
        name = obj.file if obj.file else 'ftl'
        textureManager = materialManager._textureManager
        for texturePath in getTexturePaths(obj): textureManager.preload(source, texturePath)
        data = toGeometry(obj)
        materials = []
        for sub in data.submeshes:
            tex = None
            if obj.textures and sub.texId >= 0 and sub.texId < len(obj.textures): tex = obj.textures[sub.texId]
            materials.append(toMaterialProp(tex))
        return OpenGLFtlObject(name, isStatic, data, materials, groupPositions(obj))
