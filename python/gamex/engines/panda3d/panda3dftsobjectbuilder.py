from openx.core import ISource
from openx.gfx.gfx import MaterialManager
from gamex.families.Arkane.formats.danae.binary import Binary_Fts
from gamex.families.Arkane.engines.ftsObjectBuilder import getTexturePaths, scenePositions, toGeometry, toMaterialProp

# An FTS level kept as the shared mesh. Danae is Y-up, so positions stay as stored.

class Panda3dFtsObject:
    def __init__(self, name: str, isStatic: bool, data, materials, scenes):
        self.name = name
        self.isStatic = isStatic
        self.data = data
        self.materials = materials
        self.scenes = scenes

class Panda3dFtsObjectBuilder:
    @staticmethod
    async def buildObject(source: ISource, path: object, isStatic: bool, materialManager: MaterialManager) -> Panda3dFtsObject:
        fts: Binary_Fts = path
        level = fts.level
        name = 'fts'
        paths = getTexturePaths(level)
        if materialManager is not None and paths is not None:
            textureManager = materialManager._textureManager
            for texturePath in paths: textureManager.preload(source, texturePath)
        data = toGeometry(level)
        materials = []
        textures = getattr(level, 'textures', None)
        for sub in data.submeshes:
            tex = None
            if textures and sub.texId >= 0 and sub.texId < len(textures): tex = textures[sub.texId]
            materials.append(toMaterialProp(tex, sub.type))
        return Panda3dFtsObject(name, isStatic, data, materials, scenePositions(level))
