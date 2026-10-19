from numpy import array, zeros
from openx.core.manager import CellBuilder, CellManager
from openx.core.poly.system import changeRange, getExtrema
from openx.gfx.gfx import GfxTerrainLayer
from openx.sys.drawing import Color

# Morrowind LAND rules. Twin of GameX.Bethesda.TesCells.
class TesCells:
    side: int = 65
    heightScale: int = 8
    textureRows: int = 16
    textureColumns: int = 16
    sampleDivisor: int = side - 1
    defaultTexturePath: str = 'textures/_land_default.dds'

    @staticmethod
    def textureIndexs(land: CellManager.ILand) -> list[int]:
        return land.vtex if land.vtex is not None else [0] * 256

    @staticmethod
    def decodeHeights(land: CellManager.ILand, dest) -> None:
        heights = land.heights
        rowOffset = land.heightOffset
        side = TesCells.side
        for y in range(side):
            rowOffset += heights[y * side]
            dest[y, 0] = rowOffset * TesCells.heightScale
            colOffset = rowOffset
            for x in range(1, side):
                colOffset += heights[(y * side) + x]
                dest[y, x] = colOffset * TesCells.heightScale

    @staticmethod
    def layerKey(index: int) -> int: return int(index) - 1

    @staticmethod
    def alphaKey(indexs: list[int], x: int, y: int) -> int:
        yMajor = y // 4; yMinor = y - (yMajor * 4)
        xMajor = x // 4; xMinor = x - (xMajor * 4)
        return int(indexs[(yMajor * 64) + (xMajor * 16) + (yMinor * 4) + xMinor]) - 1

    @staticmethod
    def texturePath(query: CellManager.IQuery, key: int) -> str:
        return query.findLtex(key).path if key >= 0 else TesCells.defaultTexturePath

class Texture: pass

# Draws a Morrowind LAND through an engine's terrain slot.
class BethesdaCellBuilder(CellBuilder):
    terrainLayers: dict = {}

    async def createLand(self, land: CellManager.ILand, parent: object):
        heights = land.heights
        if not heights: return
        side = TesCells.side
        newHeights = zeros((side, side), dtype=float)
        TesCells.decodeHeights(land, newHeights)
        minHeight, maxHeight = getExtrema(newHeights)
        for y in range(side):
            for x in range(side):
                newHeights[y, x] = changeRange(newHeights[y, x], minHeight, maxHeight, 0, 1)
        textureManager = self.gfxModel.textureManager
        indexs = TesCells.textureIndexs(land); layers = []; layerIndexs = {}
        for i in range(len(indexs)):
            index = TesCells.layerKey(indexs[i])
            if index in layerIndexs: continue
            path = TesCells.texturePath(self.query, index)
            tex, _ = await self.gfxModel.createTexture(self.source, path)
            layer = BethesdaCellBuilder.terrainLayers.get(tex)
            if not layer:
                layer = GfxTerrainLayer[Texture](
                    texture=tex,
                    smoothness=.3,
                    metallic=.2,
                    specular=Color.black,
                    tileSize=array([6, 6]))
                layer.maskMapTexture = textureManager.createSolidTexture(1, 1, [layer.metallic, .0, .0, layer.smoothness])
                layer.normalMapTexture = textureManager.createNormalMapTexture(tex)
                BethesdaCellBuilder.terrainLayers[tex] = layer
            layerIndex = len(layers); layers.append(layer); layerIndexs[index] = layerIndex
        alphaMap = zeros((TesCells.textureRows, TesCells.textureColumns, len(layers)))
        for y in range(TesCells.textureRows):
            for x in range(TesCells.textureColumns):
                texIndex = TesCells.alphaKey(indexs, x, y)
                alphaMap[y, x, layerIndexs[texIndex] if texIndex >= 0 else 0] = 1
        heightRange = (maxHeight - minHeight) / self.meterInUnits
        position = array([land.gridId.y * self.cellLengthInMeters, land.gridId.y * self.cellLengthInMeters, minHeight / self.meterInUnits])
        sampleDistance = self.cellLengthInMeters / TesCells.sampleDivisor
        data = self.gfxTerrain.createTerrainData(-1, newHeights, heightRange, sampleDistance, layers, alphaMap)
        self.gfxTerrain.createTerrain('terrain', position, data, parent)
