from numpy import array, zeros
from openx.core.manager import CellBuilder, CellManager
from openx.core.poly.system import changeRange, getExtrema
from openx.gfx.gfx import GfxTerrainLayer
from openx.sys.drawing import Color

# Ultima Online land rules. Twin of GameX.Origin.UltimaCells.
class UltimaCells:
    side: int = 64
    textureRows: int = 64
    textureColumns: int = 64
    sampleDivisor: int = 64

    @staticmethod
    def textureIndexs(land: CellManager.ILand) -> list[int]:
        return land.vtex if land.vtex is not None else [0] * (UltimaCells.side * UltimaCells.side)

    @staticmethod
    def decodeHeights(land: CellManager.ILand, dest) -> None:
        heights = land.heights
        side = UltimaCells.side
        for y in range(side):
            for x in range(side):
                dest[y, x] = heights[(y * side) + x]

    @staticmethod
    def layerKey(index: int) -> int: return int(index)

    @staticmethod
    def alphaKey(indexs: list[int], x: int, y: int) -> int:
        return int(indexs[(y * UltimaCells.side) + x])

    @staticmethod
    def texturePath(key: int) -> str: return f'tex{key}'

class Texture: pass

# Draws an Ultima Online land cell through an engine's terrain slot.
class UltimaCellBuilder(CellBuilder):
    terrainLayers: dict = {}

    async def createLand(self, land: CellManager.ILand, parent: object):
        heights = land.heights
        if not heights: return
        side = UltimaCells.side
        newHeights = zeros((side, side), dtype=float)
        UltimaCells.decodeHeights(land, newHeights)
        minHeight, maxHeight = getExtrema(newHeights)
        for y in range(side):
            for x in range(side):
                newHeights[y, x] = changeRange(newHeights[y, x], minHeight, maxHeight, 0, 1)
        textureManager = self.gfxModel.textureManager
        indexs = UltimaCells.textureIndexs(land); layers = []; layerIndexs = {}
        for i in range(len(indexs)):
            index = UltimaCells.layerKey(indexs[i])
            if index in layerIndexs: continue
            path = UltimaCells.texturePath(index)
            tex, _ = await self.gfxModel.createTexture(self.source, path)
            layer = UltimaCellBuilder.terrainLayers.get(tex)
            if not layer:
                layer = GfxTerrainLayer[Texture](
                    texture=tex,
                    smoothness=.3,
                    metallic=.2,
                    specular=Color.black,
                    tileSize=array([6, 6]))
                layer.maskMapTexture = textureManager.createSolidTexture(1, 1, [layer.metallic, .0, .0, layer.smoothness])
                layer.normalMapTexture = textureManager.createNormalMapTexture(tex)
                UltimaCellBuilder.terrainLayers[tex] = layer
            layerIndex = len(layers); layers.append(layer); layerIndexs[index] = layerIndex
        alphaMap = zeros((UltimaCells.textureRows, UltimaCells.textureColumns, len(layers)))
        for y in range(UltimaCells.textureRows):
            for x in range(UltimaCells.textureColumns):
                texIndex = UltimaCells.alphaKey(indexs, x, y)
                alphaMap[y, x, layerIndexs[texIndex]] = 1
        heightRange = (maxHeight - minHeight) / self.meterInUnits
        position = array([land.gridId.x * self.cellLengthInMeters, land.gridId.y * self.cellLengthInMeters, minHeight / self.meterInUnits])
        sampleDistance = self.cellLengthInMeters / UltimaCells.sampleDivisor
        data = self.gfxTerrain.createTerrainData(-1, newHeights, heightRange, sampleDistance, layers, alphaMap)
        self.gfxTerrain.createTerrain('terrain', position, data, parent)
