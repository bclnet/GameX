from openx.core import ISource
from openx.gfx.gfx import MaterialManager
from gamex.engines.opengl.openglFtlObjectBuilder import drawMesh
from gamex.families.Arkane.engines.ftsObjectBuilder import getTexturePaths, scenePositions, toGeometry, toMaterialProp

# An FTS kept as the shared mesh. Danae is Y-up, so positions stay as stored.
# OpenGL's material builder only accepts MaterialShaderVProp, so the props stay MaterialStd2Prop.

class OpenGLFtsObject:
    def __init__(self, name: str, isStatic: bool, data, materials, groups):
        self.name = name
        self.isStatic = isStatic
        self.data = data
        self.materials = materials
        self.groups = groups

    def drawGl(self) -> None:
        drawMesh(self.data)

class OpenGLFtsObjectBuilder:
    @staticmethod
    async def buildObject(source: ISource, path: object, isStatic: bool, materialManager: MaterialManager) -> OpenGLFtsObject:
        level = path.level
        name = 'fts'
        textureManager = materialManager._textureManager
        for texturePath in getTexturePaths(level): textureManager.preload(source, texturePath)
        data = toGeometry(level)
        materials = []
        for sub in data.submeshes:
            tex = None
            if level.textures and sub.texId >= 0 and sub.texId < len(level.textures): tex = level.textures[sub.texId]
            materials.append(toMaterialProp(tex, sub.type))
        return OpenGLFtsObject(name, isStatic, data, materials, scenePositions(level))
