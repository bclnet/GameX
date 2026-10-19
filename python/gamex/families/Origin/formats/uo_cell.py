import importlib.util
import math
import threading
from pathlib import Path
from openx.core.manager import CellManager
from openx.core.poly.poly import Float3, Int3

# Family rules live in managers.py. Loaded by path so this module does not import gamex.
_spec = importlib.util.spec_from_file_location('origin_managers', Path(__file__).resolve().parents[1] / 'managers.py')
_managers = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(_managers)
UltimaCells = _managers.UltimaCells
UltimaCellBuilder = _managers.UltimaCellBuilder

# Ultima map cells. A cell is an 8x8 block of 8x8 land chunks (64x64 tiles).
# The caller supplies the map stream, mul or UOP.

class UltimaLtex(CellManager.ILtex):
    def __init__(self, index: int):
        self.intv = index
        self.path = f'tex{index}'

class UltimaModel(CellManager.ICellXrefModel):
    def __init__(self, name: str): self.modelPath = name

class UltimaXref(CellManager.ICellXref):
    def __init__(self, name: str, position: Float3):
        self.name = name
        self.scale = None
        self.position = position
        self.eulerAngles = Float3()

class UltimaLand(CellManager.ILand):
    def __init__(self, gridId: Int3, vtex: list[int], heights: list[int]):
        self.gridId = gridId
        self.vtex = vtex
        self.heightOffset = 0
        self.heights = heights

class UltimaCell(CellManager.ICell):
    def __init__(self, id: int, gridId: Int3, name: str, xrefs: list):
        self.id = id
        self.isInterior = False
        self.gridId = gridId
        self.name = name
        self.ambientLight = None
        self.Xrefs = xrefs

# One Ultima facet. textureIds is land tile id to texture id, indexed by tileId & 0x3FFF.
class UltimaCellQuery(CellManager.IQuery):
    meterInUnitsValue: float = 10
    cellLengthValue: float = 64
    cellPack: int = 8
    landChunkSize: int = 196
    landChunkData: int = 192
    mapHeights: list[int] = [512, 512, 200, 256, 181]
    radiusValue: list[int] = [2, 2]

    @classmethod
    def openUop(cls, source, statics, staidx, textureIds: list[int], mapIndex: int = 0):
        from gamex.families.Origin.formats.binary import Binary_UO
        return cls(Binary_UO().openMap(source), statics, staidx, textureIds, mapIndex)

    def __init__(self, map, statics, staidx, textureIds: list[int], mapIndex: int = 0):
        self.map = map
        self.statics = statics
        self.staidx = staidx
        self.textureIds = textureIds or []
        self.world = mapIndex
        self.meterInUnits = UltimaCellQuery.meterInUnitsValue
        self.cellLengthInMeters = UltimaCellQuery.cellLengthValue
        self.radius = UltimaCellQuery.radiusValue
        self.cells: dict[Int3, UltimaCell] = {}
        self.lands: dict[Int3, UltimaLand] = {}
        self._lock = threading.RLock()
        height = UltimaCellQuery.mapHeights[0 if mapIndex < 0 else len(UltimaCellQuery.mapHeights) - 1 if mapIndex >= len(UltimaCellQuery.mapHeights) else mapIndex]
        self.chunkHeight = height
        length = _length(map)
        self.chunkWidth = 0 if height == 0 else length // (height * UltimaCellQuery.landChunkSize)

    def setWorld(self, world: int) -> None: pass
    def createCellBuilder(self, source, gfx): return UltimaCellBuilder(source, self, gfx)
    def getCellId(self, point) -> Int3: return Int3(int(math.floor(point[0] / self.cellLengthInMeters)), int(math.floor(point[2] / self.cellLengthInMeters)), self.world)
    def findCellByName(self, name: str): return None
    def findLtex(self, index: int): return UltimaLtex(index)
    def findAnyByName(self, name: str):
        if name is None or len(name) < 6: return None
        if name.startswith('sta') or name.startswith('lnd'):
            try: value = int(name[3:])
            except ValueError: return None
            if -32768 <= value <= 32767: return UltimaModel(name)
        return None
    def findLand(self, cell: Int3):
        self._ensure(cell)
        return self.lands.get(cell)
    def findCell(self, cell: Int3):
        self._ensure(cell)
        return self.cells.get(cell)

    def _ensure(self, cell: Int3) -> None:
        with self._lock:
            if cell in self.cells or self.chunkWidth == 0: return
            side = UltimaCells.side
            heights = [0] * (side * side)
            vtex = [0] * (side * side)
            xrefs = []
            for cy in range(UltimaCellQuery.cellPack):
                for cx in range(UltimaCellQuery.cellPack):
                    self._readChunk(heights, vtex, xrefs, cell.x * UltimaCellQuery.cellPack + cx, cell.y * UltimaCellQuery.cellPack + cy)
            self.lands[cell] = UltimaLand(cell, vtex, heights)
            self.cells[cell] = UltimaCell(cell.x | (cell.y << 16), cell, f'{cell.x}x{cell.y}', xrefs)

    def _readChunk(self, heights, vtex, xrefs, chunkX: int, chunkY: int) -> None:
        ground = self._readLand(chunkX, chunkY)
        i = 0
        side = UltimaCells.side
        for x in range(8):
            for y in range(8):
                tileId = int.from_bytes(ground[i:i + 2], 'little', signed=True); i += 2
                z = ground[i] - 256 if ground[i] >= 128 else ground[i]; i += 1
                idx = (((chunkY & 7) * 8 + y) * side) + ((chunkX & 7) * 8) + x
                heights[idx] = z
                tid = tileId & 0x3FFF
                vtex[idx] = self.textureIds[tid] if tid < len(self.textureIds) else 0
        statics, length = self._readStatic(chunkX, chunkY)
        count = length // 7
        s = 0
        for _n in range(count):
            tileId = int.from_bytes(statics[s:s + 2], 'little', signed=True); s += 2
            x = statics[s] - 256 if statics[s] >= 128 else statics[s]; s += 1
            y = statics[s] - 256 if statics[s] >= 128 else statics[s]; s += 1
            z = statics[s] - 256 if statics[s] >= 128 else statics[s]; s += 1
            s += 2
            if 0x0C45 <= tileId <= 0x0DAF: continue
            xrefs.append(UltimaXref(f'sta{tileId:03d}', Float3(chunkX * 8 + x, chunkY * 8 + y, z / UltimaCellQuery.meterInUnitsValue)))

    def _readLand(self, chunkX: int, chunkY: int) -> bytes:
        data = bytearray(UltimaCellQuery.landChunkData)
        if self.map is None or self.chunkWidth == 0: return data
        chunkX %= self.chunkWidth
        chunkY %= self.chunkHeight
        ptr = ((chunkX * self.chunkHeight) + chunkY) * UltimaCellQuery.landChunkSize + 4
        with self._lock:
            length = _length(self.map)
            if ptr < 0 or ptr + UltimaCellQuery.landChunkData > length: return data
            self.map.seek(ptr)
            got = self.map.read(UltimaCellQuery.landChunkData)
            data[:len(got)] = got
        return data

    def _readStatic(self, chunkX: int, chunkY: int) -> tuple[bytes, int]:
        if self.statics is None or self.staidx is None or self.chunkWidth == 0: return b'', 0
        chunkX %= self.chunkWidth
        chunkY %= self.chunkHeight
        pos = ((chunkX * self.chunkHeight) + chunkY) * 12
        with self._lock:
            if pos < 0 or pos + 8 > _length(self.staidx): return b'', 0
            self.staidx.seek(pos)
            idx = self.staidx.read(8)
            if len(idx) < 8: return b'', 0
            lookup = int.from_bytes(idx[0:4], 'little', signed=True)
            length = int.from_bytes(idx[4:8], 'little', signed=True)
            if lookup < 0 or length <= 0: return b'', 0
            if lookup + length > _length(self.statics): return b'', 0
            self.statics.seek(lookup)
            buf = self.statics.read(length)
        return buf, len(buf)

def _length(stream) -> int:
    if stream is None: return 0
    pos = stream.tell()
    stream.seek(0, 2)
    n = stream.tell()
    stream.seek(pos)
    return n
