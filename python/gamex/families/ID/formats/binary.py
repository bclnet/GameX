from __future__ import annotations
import os, numpy as np
from io import BytesIO
from enum import Enum, Flag
from openx.core import _throw, _pathExtension, unsafe, BinaryReader
from openx.gfx import Raster, TextureAsBytes, ITexture, ITextureSelect, ITextureFrames, TextureFlags, TextureFormat, TexturePixel
from gamex import Archive, BinaryArchive, ArcBinary, ArcBinaryT, FileSource, MetaInfo, MetaManager, MetaContent, IHaveMetaInfo
from gamex.families.Uncore.formats.compression import decompressBlast
from hashlib import md5
from cryptography.hazmat.primitives import hashes
from cryptography.hazmat.backends import default_backend
from cryptography.hazmat.primitives.asymmetric import padding
from cryptography.hazmat.primitives import serialization

# lumps
class X_LumpON:
     offset: int
     num: int

class X_LumpNO:
    num: int
    offset: int

class X_LumpNO2:
    num: int
    offset: int
    offset2: int

class X_Lump2NO:
    num: int
    offset: int
    offset2: int

def _fillLumps(owner, values, names) -> None:
    for i, name in enumerate(names):
        lump = X_LumpON(); lump.offset = values[i * 2]; lump.num = values[i * 2 + 1]; setattr(owner, name, lump)

# C# Encoding.ASCII.GetString. Bytes above 127 are '?'. None throws.
def _asciiGetString(data: bytes) -> str: return ''.join(chr(b) if b < 0x80 else '?' for b in data)

#region Binary_BspY

# Binary_BspY
class Binary_BspY(ArcBinaryT):
    #region Headers

    class D_Model:
        _struct = ('<9f7i', 64)
        def __init__(self, t):
            self.boundMin, self.boundMax, self.origin = t[0:3], t[3:6], t[6:9]
            (self.nodeId0, self.nodeId1, self.nodeId2, self.nodeId3, self.numLeafs, self.faceId, self.faceNum) = t[9:16]

    class D_TexInfo:
        _struct = ('<8f2I', 40)
        def __init__(self, t):
            self.vectorS, self.distS, self.vectorT, self.distT = t[0:3], t[3], t[4:7], t[7]
            (self.textureId, self.animated) = t[8:10]

    class D_Face:
        _struct = ('<HHi2H4Bi', 20)
        def __init__(self, t):
            (self.planeId, self.side, self.ledgeId, self.ledgeNum, self.texinfoId, self.typeLight, self.baseLight, light0, light1, self.lightMap) = t
            self.light = (light0, light1)

    class D_Node:
        _struct = ('<q2H2h2H', 20) # PlaneId is a long. Box is one Vector2<short>
        def __init__(self, t):
            (self.planeId, self.front, self.back, box0, box1, self.faceId, self.faceNum) = t
            self.box = (box0, box1)

    class D_Leaf:
        _struct = ('<2i2h2H4B', 20) # Bound is a Vector2<short>
        def __init__(self, t):
            (self.type, self.visList, bound0, bound1, self.lFaceId, self.lFaceNum, self.sndWater, self.sndSky, self.sndSlime, self.sndLava) = t
            self.bound = (bound0, bound1)

    class D_Plane:
        _struct = ('<4fi', 20)
        def __init__(self, t):
            self.normal = t[0:3]
            (self.dist, self.type) = t[3:5]

    class D_ClipNode:
        _struct = ('<I2h', 8)
        def __init__(self, t):
            (self.planeNum, self.front, self.back) = t

    class X_Header:
        _struct = ('<31i', 124)
        def __init__(self, t):
            self.version = t[0]
            _fillLumps(self, t[1:], ('entities', 'planes', 'textures', 'vertices', 'visibility', 'nodes', 'texInfo', 'faces', 'lighting', 'clipNodes', 'leaves', 'markSurfaces', 'edges', 'surfEdges', 'models'))
        def forGameId(self, id: str) -> None:
            pass

    class X_Texture:
        _struct = ('<16s6I', 40)
        def __init__(self, t):
            (self.name, self.width, self.height, o0, o1, o2, o3) = t
            self.offsets = (o0, o1, o2, o3)

    #endregion

    def read(self, source: BinaryArchive, r: BinaryReader, tag: object = None) -> None:
        files = source.files = []
        header = r.readS(self.X_Header)
        # if header.version != 30: raise Exception('BAD VERSION')
        header.forGameId(source.game.id)
        files.append(FileSource(path = 'entities.txt', offset = header.entities.offset, fileSize = header.entities.num))
        files.append(FileSource(path = 'planes.dat', offset = header.planes.offset, fileSize = header.planes.num))
        r.seek(start := header.textures.offset)
        for o in r.readL32PArray(None, 'I'):
            r.seek(start + o)
            tex = r.readS(self.X_Texture)
            files.append(FileSource(path = f'textures/{unsafe.fixedAString(tex.name, 16)}.tex', tag = tex))
        files.append(FileSource(path = 'vertices.dat', offset = header.vertices.offset, fileSize = header.vertices.num))
        files.append(FileSource(path = 'visibility.dat', offset = header.visibility.offset, fileSize = header.visibility.num))
        files.append(FileSource(path = 'nodes.dat', offset = header.nodes.offset, fileSize = header.nodes.num))
        files.append(FileSource(path = 'texInfo.dat', offset = header.texInfo.offset, fileSize = header.texInfo.num))
        files.append(FileSource(path = 'faces.dat', offset = header.faces.offset, fileSize = header.faces.num))
        files.append(FileSource(path = 'lighting.dat', offset = header.lighting.offset, fileSize = header.lighting.num))
        files.append(FileSource(path = 'clipNodes.dat', offset = header.clipNodes.offset, fileSize = header.clipNodes.num))
        files.append(FileSource(path = 'leaves.dat', offset = header.leaves.offset, fileSize = header.leaves.num))
        files.append(FileSource(path = 'markSurfaces.dat', offset = header.markSurfaces.offset, fileSize = header.markSurfaces.num))
        files.append(FileSource(path = 'edges.dat', offset = header.edges.offset, fileSize = header.edges.num))
        files.append(FileSource(path = 'surfEdges.dat', offset = header.surfEdges.offset, fileSize = header.surfEdges.num))
        start = header.models.offset; stop = start + header.models.num; stride = 33 + (4 << 2)
        for o in range(start, stop, stride): files.append(FileSource(path = f'models/model{o}.dat', offset = o, fileSize = stride))

    def readData(self, source: BinaryArchive, r: BinaryReader, file: FileSource, option: object = None) -> BytesIO:
        r.seek(file.offset)
        return BytesIO(r.readBytes(file.fileSize))

#endregion

#region Binary_BspX

class BspFace:
    def __init__(self, planeNum, side, firstEdge, numEdges, texinfo):
        self.planeNum, self.side, self.firstEdge, self.numEdges, self.texinfo = planeNum, side, firstEdge, numEdges, texinfo

class BspTexinfo:
    def __init__(self, vecs0, vecs1, miptex):
        self.vecs0, self.vecs1, self.miptex = vecs0, vecs1, miptex

class BspPlane:
    def __init__(self, normal, dist):
        self.normal, self.dist = normal, dist

class BspModel:
    def __init__(self, firstFace, numFaces):
        self.firstFace, self.numFaces = firstFace, numFaces

class BspTexture:
    def __init__(self, name, width, height):
        self.name, self.width, self.height = name, width, height

class BspLevel:
    def __init__(self):
        self.vertices = []
        self.edges = []
        self.surfEdges = []
        self.faces = []
        self.texinfo = []
        self.planes = []
        self.models = []
        self.textures = []
        self.entities = ''

# int32 count, then count int32 offsets from the start of the lump. Offset <= 0 keeps the miptex index.
def _bspTextures(data) -> list:
    if not data or len(data) < 4: return []
    count = int.from_bytes(data[0:4], 'little', signed=True)
    textures = []
    for i in range(count if count > 0 else 0):
        at = 4 + i * 4
        offset = int.from_bytes(data[at:at + 4], 'little', signed=True) if at + 4 <= len(data) else 0
        if offset <= 0 or offset + 24 > len(data): textures.append(BspTexture('', 0, 0)); continue
        raw = data[offset:offset + 16]
        z = raw.find(0)
        name = _asciiGetString(raw[:z] if z >= 0 else raw)
        width = int.from_bytes(data[offset + 16:offset + 20], 'little', signed=False)
        height = int.from_bytes(data[offset + 20:offset + 24], 'little', signed=False)
        textures.append(BspTexture(name, width, height))
    return textures

# Binary_BspX
class Binary_BspX(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_BspX(r, s)

    @staticmethod
    def copyLump(r: BinaryReader, fileLength: int, lump: X_LumpON, pat: str, size: int, maxSize: int, cls: type = None):
        length = lump.num; ofs = lump.offset
        if length % size != 0: raise Exception('LoadBSPFile: odd lump size')
        if length // size > maxSize:
            print('WARNING: exceeded max size for lump %d size %d > maxSize %d\n')
            length = maxSize * size
        if ofs + length > fileLength:
            print('WARNING: exceeded file length for lump GameX.ID.Formats.X_LumpON\n')
            length = fileLength - ofs
            if length <= 0: return None
        r.seek(ofs)
        count = length // size
        if pat == None: return r.readSArray(cls, count) if size > 1 else r.readBytes(count)
        return r.readPArray(None, pat, count)

    #region Headers

    class Ent:
        MAX_MAP_ENTITIES = 2048
        class epair:
            next: object
            key: object
            value: object
        class Entity:
            origin: object
            firstBrush: int
            numBrushes: int
            epairs: object
            areaPortalNum: int
            portalAreas: int # [2]
            modelNum: int
            wasDetail: bool

    class X_DHeader_H1:
        _struct = ('<31i', 124)
        def __init__(self, t):
            self.version = t[0]
            _fillLumps(self, t[1:], ('entities', 'planes', 'textures', 'vertexes', 'visibility', 'nodes', 'texInfo', 'faces', 'lighting', 'clipNodes', 'leafs', 'markSurfaces', 'edges', 'surfEdges', 'models'))
        def read(self, r: BinaryReader, id: str, h1: bool):
            if id == 'HL:BS': self.entities, self.planes = self.planes, self.entities
            if self.version != (30 if h1 else 29): raise Exception('BAD VERSION')
            fileLength = r.length; B = Binary_BspX; copy = B.copyLump
            dmodels = copy(r, fileLength, self.models, None, B.X_DModel_H1._struct[1], 400 if h1 else 256, B.X_DModel_H1)
            dvertexes = copy(r, fileLength, self.vertexes, '3f', 12, 65535)
            dplanes = copy(r, fileLength, self.planes, None, B.X_DPlane_H1S2._struct[1], 32767 if h1 else 8192, B.X_DPlane_H1S2)
            dleafs = copy(r, fileLength, self.leafs, None, B.X_DLeaf_H1._struct[1], 8192 if h1 else 32767, B.X_DLeaf_H1)
            dnodes = copy(r, fileLength, self.nodes, None, B.X_DNode_H1S2._struct[1], 32767, B.X_DNode_H1S2)
            texinfo = copy(r, fileLength, self.texInfo, None, B.X_Texinfo_H1._struct[1], 8192 if h1 else 4096, B.X_Texinfo_H1)
            dclipnodes = copy(r, fileLength, self.clipNodes, None, B.X_DClipNode_H1._struct[1], 32767, B.X_DClipNode_H1)
            dfaces = copy(r, fileLength, self.faces, None, B.X_DFace_H12._struct[1], 65535, B.X_DFace_H12)
            dmarksurfaces = copy(r, fileLength, self.markSurfaces, 'H', 2, 65535)
            dsurfedges = copy(r, fileLength, self.surfEdges, 'i', 4, 512000)
            dedges = copy(r, fileLength, self.edges, None, B.X_DEdge_H1S2._struct[1], 256000, B.X_DEdge_H1S2)
            dtexdata = copy(r, fileLength, self.textures, None, 1, 0x200000)
            dvisdata = copy(r, fileLength, self.visibility, None, 1, 0x200000 if h1 else 0x100000)
            dlightdata = copy(r, fileLength, self.lighting, None, 1, 0x200000 if h1 else 0x100000)
            dentdata = copy(r, fileLength, self.entities, None, 1, 131072 if h1 else 65536)
            level = BspLevel()
            level.vertices = dvertexes or []
            level.edges = [e.v for e in dedges] if dedges else []
            level.surfEdges = dsurfedges or []
            level.faces = [BspFace(f.planeNum, f.side, f.firstEdge, f.numEdges, f.texinfo) for f in dfaces] if dfaces else []
            level.texinfo = [BspTexinfo(tuple(t.vecs0), tuple(t.vecs1), t.miptex) for t in texinfo] if texinfo else []
            level.planes = [BspPlane(tuple(p.normal), p.dist) for p in dplanes] if dplanes else []
            level.models = [BspModel(m.firstFace, m.numFaces) for m in dmodels] if dmodels else []
            level.textures = _bspTextures(dtexdata)
            level.entities = _asciiGetString(dentdata) if dentdata else ''
            return level

    class X_DHeader_S2:
        _struct = ('<44i', 176) # C# "<42i" is 168; sizeof is 176
        def __init__(self, t):
            self.magic, self.version = t[0], t[1]
            _fillLumps(self, t[2:], ('entities', 'planes', 'vertexes', 'textures', 'visibility', 'nodes', 'texInfo', 'faces', 'lighting', 'leafs', 'leafFaces', 'leafBrushes', 'edges', 'surfEdges', 'models', 'brushes', 'brushSides', 'pop', 'areas', 'areaPortals', 'lightInfoS'))
        def read(self, r: BinaryReader, id: str, sin: bool):
            if self.magic != (0x0 if sin else 0x0): raise Exception('BAD MAGIC')
            if self.version != (30 if sin else 38): raise Exception('BAD VERSION')
            fileLength = r.length; B = Binary_BspX; copy = B.copyLump
            # C# counts with sizeof(X_DModel_S2) and reads X_DModel_H1
            dmodels = copy(r, fileLength, self.models, None, B.X_DModel_S2._struct[1], 1024, B.X_DModel_H1)
            dvertexes = copy(r, fileLength, self.vertexes, '3f', 12, 65535)
            dplanes = copy(r, fileLength, self.planes, None, B.X_DPlane_H1S2._struct[1], 65535, B.X_DPlane_H1S2)
            dleafs = copy(r, fileLength, self.leafs, None, B.X_DLeaf_S2._struct[1], 65535, B.X_DLeaf_S2)
            dnodes = copy(r, fileLength, self.nodes, None, B.X_DNode_H1S2._struct[1], 65535, B.X_DNode_H1S2)
            texinfo = copy(r, fileLength, self.texInfo, None, B.X_Texinfo_S._struct[1], 8192, B.X_Texinfo_S) if sin else copy(r, fileLength, self.texInfo, None, B.X_Texinfo_2._struct[1], 8192, B.X_Texinfo_2)
            dfaces = copy(r, fileLength, self.faces, None, B.X_DFace_S._struct[1], 65535, B.X_DFace_S) if sin else copy(r, fileLength, self.faces, None, B.X_DFace_H12._struct[1], 65535, B.X_DFace_H12)
            dleaffaces = copy(r, fileLength, self.leafFaces, 'H', 2, 65535)
            dleafbrushes = copy(r, fileLength, self.leafBrushes, 'H', 2, 65535)
            dsurfedges = copy(r, fileLength, self.surfEdges, 'i', 4, 256000)
            dedges = copy(r, fileLength, self.edges, None, B.X_DEdge_H1S2._struct[1], 128000, B.X_DEdge_H1S2)
            dbrushes = copy(r, fileLength, self.brushes, None, B.X_DBrush_S23._struct[1], 8192, B.X_DBrush_S23)
            dbrushsides = copy(r, fileLength, self.brushSides, None, B.X_DBrushside_S._struct[1], 65536, B.X_DBrushside_S) if sin else copy(r, fileLength, self.brushSides, None, B.X_DBrushside_2._struct[1], 65536, B.X_DBrushside_2)
            dareas = copy(r, fileLength, self.areas, None, B.X_DArea_S2._struct[1], 256, B.X_DArea_S2)
            dareaportals = copy(r, fileLength, self.areaPortals, None, B.X_DAreaportal_S2._struct[1], 1024, B.X_DAreaportal_S2)
            lightinfo = copy(r, fileLength, self.lightInfoS, None, B.X_Lightinfo_S._struct[1], 1024, B.X_Lightinfo_S) if sin else None
            dvisdata = copy(r, fileLength, self.visibility, None, 1, 0x280000)
            dlightdata = copy(r, fileLength, self.lighting, None, 1, 0x300000 if sin else 0x320000)
            dentdata = _asciiGetString(copy(r, fileLength, self.entities, None, 1, 0x40000))
            return None

    class X_DHeader_3:
        _struct = ('<36i', 144)
        def __init__(self, t):
            self.magic, self.version = t[0], t[1]
            _fillLumps(self, t[2:], ('entities', 'shaders', 'planes', 'nodes', 'leafs', 'leafSurfaces', 'leafBrushes', 'models', 'brushes', 'brushSides', 'drawVerts', 'drawIndexes', 'fogs', 'surfaces', 'lightmaps', 'lightGrid', 'visibility'))
        def read(self, r: BinaryReader, id: str):
            if self.magic != 0x0: raise Exception('BAD MAGIC')
            if self.version != 46: raise Exception('BAD VERSION')
            fileLength = r.length; B = Binary_BspX; copy = B.copyLump
            dshaders = copy(r, fileLength, self.shaders, None, B.X_DShader_3._struct[1], 0x400, B.X_DShader_3)
            dmodels = copy(r, fileLength, self.models, None, B.X_DModel_3._struct[1], 0x400, B.X_DModel_3)
            dplanes = copy(r, fileLength, self.planes, None, B.X_DPlane_3._struct[1], 0x20000, B.X_DPlane_3)
            dleafs = copy(r, fileLength, self.leafs, None, B.X_DLeaf_3._struct[1], 0x20000, B.X_DLeaf_3)
            dnodes = copy(r, fileLength, self.nodes, None, B.X_DNode_3._struct[1], 0x20000, B.X_DNode_3)
            dleafsurfaces = copy(r, fileLength, self.leafSurfaces, 'i', 4, 0x20000)
            dleafbrushes = copy(r, fileLength, self.leafBrushes, 'i', 4, 0x40000)
            dbrushes = copy(r, fileLength, self.brushes, None, B.X_DBrush_S23._struct[1], 0x8000, B.X_DBrush_S23)
            dbrushsides = copy(r, fileLength, self.brushSides, None, B.X_DBrushside_3._struct[1], 0x20000, B.X_DBrushside_3)
            drawVerts = copy(r, fileLength, self.drawVerts, None, B.X_DrawVert_3._struct[1], 0x80000, B.X_DrawVert_3)
            drawSurfaces = copy(r, fileLength, self.surfaces, None, B.X_DSurface_3._struct[1], 0x20000, B.X_DSurface_3)
            dfogs = copy(r, fileLength, self.fogs, None, B.X_DFog_3._struct[1], 0x100, B.X_DFog_3)
            drawIndexes = copy(r, fileLength, self.drawIndexes, 'i', 4, 0x80000)
            dvisData = copy(r, fileLength, self.visibility, None, 1, 0x200000)
            dlightData = copy(r, fileLength, self.lightmaps, None, 1, 0x800000)
            dentData = _asciiGetString(copy(r, fileLength, self.entities, None, 1, 0x10000))
            gridData = copy(r, fileLength, self.lightGrid, None, 1, 0x800000)
            return None

    class X_DModel_H1:
        _struct = ('<9f7i', 64)
        def __init__(self, t):
            self.mins, self.maxs, self.origin = t[0:3], t[3:6], t[6:9]
            self.headNodes = t[9:13]
            (self.visleafs, self.firstFace, self.numFaces) = t[13:16]

    class X_DModel_S2:
        _struct = ('<9f3i', 48)
        def __init__(self, t):
            self.mins, self.maxs, self.origin = t[0:3], t[3:6], t[6:9]
            (self.headNode, self.firstFace, self.numFaces) = t[9:12]

    class X_DModel_3:
        _struct = ('<6f4i', 40)
        def __init__(self, t):
            self.mins, self.maxs = t[0:3], t[3:6]
            (self.firstSurface, self.numSurfaces, self.firstBrush, self.numBrushes) = t[6:10]

    class X_DMiptexLump_H1:
        _struct = ('<5i', 20)
        def __init__(self, t):
            self.numMiptex = t[0]
            self.dataofs = t[1:5]

    class X_Miptex_H1:
        _struct = ('<16s6I', 40)
        def __init__(self, t):
            (self.name, self.width, self.height, o0, o1, o2, o3) = t
            self.offsets = (o0, o1, o2, o3)

    class PLANE(Enum):
        X = 0
        Y = 1
        Z = 2
        ANYX = 3
        ANYY = 4
        ANYZ = 5

    class X_DPlane_H1S2:
        _struct = ('<4fi', 20)
        def __init__(self, t):
            self.normal = t[0:3]
            (self.dist, self.type) = t[3:5]

    class X_DPlane_3:
        _struct = ('<4f', 16)
        def __init__(self, t):
            self.normal = t[0:3]
            self.dist = t[3]

    class X_DNode_H1S2:
        _struct = ('<i8h2H', 24)
        def __init__(self, t):
            self.planeNum = t[0]
            self.children = t[1:3]
            self.mins, self.maxs = t[3:6], t[6:9]
            (self.firstFace, self.numFaces) = t[9:11]

    class X_DNode_3:
        _struct = ('<9i', 36)
        def __init__(self, t):
            self.planeNum = t[0]
            self.children = t[1:3]
            self.mins, self.maxs = t[3:6], t[6:9]

    class X_DClipNode_H1:
        _struct = ('<I2h', 8)
        def __init__(self, t):
            self.planeNum = t[0]
            self.children = t[1:3]

    class X_Texinfo_H1:
        _struct = ('<8f2i', 40)
        def __init__(self, t):
            self.vecs0, self.vecs1 = t[0:4], t[4:8]
            (self.miptex, self.flags) = t[8:10]

    class X_Lightinfo_S:
        _struct = ('<i6f32s', 60) # C# "<i6fs32"; Python struct counts before s
        def __init__(self, t):
            self.value = t[0]
            self.color = t[1:4]
            (self.direct, self.directangle, self.directstyle) = t[4:7]
            self.directstyleName = t[7]

    class X_Texinfo_S:
        _struct = ('<8fi64sif2i8f32s', 180) # C# "<8f2i" is 40; sizeof is 180
        def __init__(self, t):
            self.vecs0, self.vecs1 = t[0:4], t[4:8]
            self.flags, self.texture, self.nextTexinfo = t[8], t[9], t[10]
            self.transMag, self.transAngle, self.baseAngle = t[11], t[12], t[13]
            (self.animtime, self.nonlit, self.translucence, self.friction, self.restitution) = t[14:19]
            self.color = t[19:22]
            self.groupName = t[22]

    class X_Texinfo_2:
        _struct = ('<8f2i32si', 76) # C# "<8f2i" is 40; sizeof is 76
        def __init__(self, t):
            self.vecs0, self.vecs1 = t[0:4], t[4:8]
            (self.flags, self.value) = t[8:10]
            self.texture, self.nextTexinfo = t[10], t[11]

    class X_DEdge_H1S2:
        _struct = ('<2H', 4)
        def __init__(self, t):
            self.v = t

    class X_DFace_H12:
        _struct = ('<Hhi2h4Bi', 20)
        def __init__(self, t):
            (self.planeNum, self.side, self.firstEdge, self.numEdges, self.texinfo, s0, s1, s2, s3, self.lightOfs) = t
            self.styles = (s0, s1, s2, s3)

    class X_DFace_S:
        _struct = ('<Hhi2h4B2i', 24)
        def __init__(self, t):
            (self.planeNum, self.side, self.firstEdge, self.numEdges, self.texinfo, s0, s1, s2, s3, self.lightOfs, self.lightInfo) = t
            self.styles = (s0, s1, s2, s3)

    class X_DSurface_3:
        _struct = ('<12i12f2i', 104) # C# "<12i6f2i" is 80; sizeof is 104
        def __init__(self, t):
            (self.shaderNum, self.fogNum, self.surfaceType, self.firstVert, self.numVerts, self.firstIndex, self.numIndexes, self.lightmapNum, self.lightmapX, self.lightmapY, self.lightmapWidth, self.lightmapHeight) = t[0:12]
            self.lightmapOrigin, self.lightmapVecs0, self.lightmapVecs1, self.lightmapVecs2 = t[12:15], t[15:18], t[18:21], t[21:24]
            (self.patchWidth, self.patchHeight) = t[24:26]

    class AMBIENT_H1(Enum):
        WATER = 0
        SKY = 1
        SLIME = 2
        LAVA = 3

    class X_DLeaf_H1:
        _struct = ('<2i6h2H4B', 28)
        def __init__(self, t):
            (self.contents, self.visOfs) = t[0:2]
            self.mins, self.maxs = t[2:5], t[5:8]
            (self.firstMarkSurface, self.numMarkSurface) = t[8:10]
            self.ambientLevel = t[10:14]

    class X_DLeaf_S2:
        _struct = ('<i8h4H', 28) # C# "<i8h8H" is 36; sizeof is 28
        def __init__(self, t):
            self.contents = t[0]
            (self.cluster, self.area) = t[1:3]
            self.mins, self.maxs = t[3:6], t[6:9]
            (self.firstLeafFace, self.numLeafFaces, self.firstLeafBrush, self.numLeafBrushes) = t[9:13]

    class X_DLeaf_3:
        _struct = ('<12i', 48)
        def __init__(self, t):
            (self.cluster, self.area) = t[0:2]
            self.mins, self.maxs = t[2:5], t[5:8]
            (self.firstLeafSurface, self.numLeafSurfaces, self.firstLeafBrush, self.numLeafBrushes) = t[8:12]

    class X_DBrushside_2:
        _struct = ('<Hh', 4)
        def __init__(self, t):
            (self.planeNum, self.texinfo) = t

    class X_DBrushside_S:
        _struct = ('<Hhi', 8)
        def __init__(self, t):
            (self.planeNum, self.texinfo, self.lightinfo) = t

    class X_DBrushside_3:
        _struct = ('<2i', 8)
        def __init__(self, t):
            (self.planeNum, self.shaderNum) = t

    class X_DBrush_S23:
        _struct = ('<3i', 12)
        def __init__(self, t):
            (self.firstSide, self.numSides, self.contents_ShaderNum) = t

    class X_DShader_3:
        _struct = ('<64s2i', 72)
        def __init__(self, t):
            (self.shader, self.surfaceFlags, self.contentFlags) = t

    class X_DAreaportal_S2:
        _struct = ('<2i', 8)
        def __init__(self, t):
            (self.portalNum, self.otherArea) = t

    class X_DArea_S2:
        def __init__(self, t):
            (self.numAreaportals, self.firstAreaportal) = t

    class X_DFog_3:
        _struct = ('<64s2i', 72) # C# "<64s3i" is 76; sizeof is 72
        def __init__(self, t):
            (self.shader, self.brushNum, self.visibleSide) = t

    class X_DrawVert_3:
        def __init__(self, t):
            (self.firstSide, self.numSides, self.contents_ShaderNum) = t

    class CONTENTS_H1(Enum):
        EMPTY = -1
        SOLID = -2
        WATER = -3
        SLIME = -4
        LAVA = -5
        SKY = -6
        ORIGIN = -7
        CLIP = -8
        CURRENT_0 = -9
        CURRENT_90 = -10
        CURRENT_180 = -11
        CURRENT_270 = -12
        CURRENT_UP = -13
        CURRENT_DOWN = -14
        TRANSLUCENT = -15

    class CONTENTS_S2(Flag):
        SOLID = 1
        WINDOW = 2
        AUX = 4
        LAVA = 8
        SLIME = 16
        WATER = 32
        MIST = 64
        LAST_VISIBLE = 64
        AREAPORTAL = 0x8000
        PLAYERCLIP = 0x10000
        MONSTERCLIP = 0x20000
        CURRENT_0 = 0x40000
        CURRENT_90 = 0x80000
        CURRENT_180 = 0x100000
        CURRENT_270 = 0x200000
        CURRENT_UP = 0x400000
        CURRENT_DOWN = 0x800000
        ORIGIN = 0x1000000
        MONSTER = 0x2000000
        DEADMONSTER = 0x4000000
        DETAIL = 0x8000000
        Q2TRANSLUCENT = 0x10000000
        LADDER = 0x20000000

    #endregion

    # C# Struct size is sizeof of the other type
    X_DArea_S2._struct = ('<2i', X_DAreaportal_S2._struct[1])
    X_DrawVert_3._struct = ('<3i', X_DBrush_S23._struct[1])

    def __init__(self, r: BinaryReader, s: Archive):
        flag = False
        gameId = s.game.id; engineN, engineV = s.game.engine
        # C# || does not assign flag when the left side is already true
        self.h1 = r.readS(self.X_DHeader_H1).read(r, gameId, flag) if engineV == '2' or (flag := engineN == 'GoldSrc') else None
        self.s2 = r.readS(self.X_DHeader_S2).read(r, gameId, flag) if engineV == '3' or (flag := gameId == 'Sin') else None
        self.x3 = r.readS(self.X_DHeader_3).read(r, gameId) if engineV == '3' else None

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self)),
        MetaInfo('BSP', items = [
            ])
        ]

#endregion

#region Binary_Lmp - tag::Binary_Lmp[]

# Binary_Lmp
class Binary_Lmp(ITexture, IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Lmp(r, f, s)

    palette: 'Binary_Lmp' = None
    colormap: 'Binary_Lmp' = None

    @staticmethod
    def toLightPixel(light: int, pixel: int) -> bytes:
        return Binary_Lmp.palette.paletteRecords[Binary_Lmp.colormap.colormapRecords[(light >> 3) & 0x1F][pixel]]

    def __init__(self, r: BinaryReader, f: FileSource, s: Archive):
        self.paletteRecords = None
        self.colormapRecords = None
        self.pixels = None
        match os.path.splitext(os.path.basename(f.path))[0]:
            case 'palette':
                self.paletteRecords = r.readFArray(lambda s: bytearray(s.readBytes(3)) + b'\x00', 256)
                Binary_Lmp.palette = self
                return
            case 'colormap':
                self.colormapRecords = r.readFArray(lambda s: s.readBytes(256), 32)
                Binary_Lmp.colormap = self
                return
            case _:
                if Binary_Lmp.palette is None or Binary_Lmp.palette.paletteRecords is None: raise NotImplementedError()
                self.width = r.readInt32()
                self.height = r.readInt32()
                indices = r.readBytes(self.width * self.height)
                pixels = bytearray()
                for index in indices: pixels += Binary_Lmp.toLightPixel(32 << 3, index)
                self.pixels = pixels
                return

    #region ITexture

    format: tuple = (TextureFormat.BGRA32, TexturePixel.Unknown)
    width: int = 0
    height: int = 0
    depth: int = 0
    mipMaps: int = 1
    texFlags: TextureFlags = 0
    def create(self, platform: str, func: callable): return func(TextureAsBytes(self.pixels, self.format, None))

    #endregion

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'TextureName', name = os.path.basename(file.path), value = self)),
        MetaInfo('TextureName', items = [
            MetaInfo(f'Width: {self.width}'),
            MetaInfo(f'Height: {self.height}')
            ])
        ]

#endregion - end::Binary_Lmp[]

#region Binary_Mdl - tag::Binary_Mdl[]

# Binary_Mdl
class Binary_Mdl(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Mdl(r)

    def __init__(self, r: BinaryReader):
        pass

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = 'UnknownFileModel File')),
        MetaInfo('UnknownFileModel', items = [
            MetaInfo(f'Records: {1}')
            ])
        ]

#endregion - end::Binary_Mdl[]

#region Binary_Spr - tag::Binary_Spr[]

# Binary_Spr
class Binary_Spr(ITextureFrames, IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Spr(r)

    #region Headers

    S_MAGIC = 0x50534449 #: IDSP

    class SprType(Enum):
        VP_PARALLEL_UPRIGHT = 0
        FACING_UPRIGHT = 1
        VP_PARALLEL = 2
        ORIENTED = 3
        VP_PARALLEL_ORIENTED = 4

    class SprTextFormat(Enum):
        SPR_NORMAL = 0
        SPR_ADDITIVE = 1
        SPR_INDEXALPHA = 2
        SPR_ALPHTEST = 3

    class SprSynchType(Enum):
        Synchronized = 0
        Random = 1

    class S_Header:
        _struct = ('<I3if3ifi', 40)
        def __init__(self, t):
            (self.magic,
            self.version,
            self.type,
            self.textFormat,
            self.boundingRadius,
            self.maxWidth,
            self.maxHeight,
            self.numFrames,
            self.beamLen,
            self.synchType) = t

    class S_Frame:
        _struct = ('<5i', 20)
        def __init__(self, t):
            (self.group,
            self.originX,
            self.originY,
            self.width,
            self.height) = t

    #endregion

    def __init__(self, r: BinaryReader):
        # read file
        header = r.readS(self.S_Header)
        if header.magic != self.S_MAGIC: raise Exception('BAD MAGIC')

        # load palette
        self.palette = r.readBytes(r.readUInt16() * 3)

        # load frames
        frames = self.frames = [self.S_Frame] * header.numFrames
        pixels = self.pixels = [bytearray] * header.numFrames
        for i in range(header.numFrames):
            frame = frames[i] = r.readS(self.S_Frame)
            pixels[i] = r.readBytes(frame.width * frame.height)
        self.width = frames[0].width
        self.height = frames[0].height
        self.bytes = bytearray(self.width * self.height << 2)
        self.frame = 0

    #region ITexture

    format: tuple = (TextureFormat.RGBA32, TexturePixel.Unknown)
    width: int = 0
    height: int = 0
    depth: int = 0
    mipMaps: int = 1
    texFlags: TextureFlags = 0
    fps: int = 60
    def create(self, platform: str, func: callable): return func(TextureAsBytes(self.bytes, self.format, None))

    def hasFrames(self) -> bool: return self.frame < len(self.frames)

    def nextFrame(self) -> bool:
        p = self.pixels[self.frame]
        Raster.blitByPalette(self.bytes, 4, p, self.palette, 3)
        self.frame += 1
        return True

    #endregion

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'VideoTexture', name = os.path.basename(file.path), value = self)),
        MetaInfo('Sprite', items = [
            MetaInfo(f'Frames: {len(self.frames)}'),
            MetaInfo(f'Width: {self.width}'),
            MetaInfo(f'Height: {self.height}'),
            MetaInfo(f'Mipmaps: {self.mipMaps}')
            ])
        ]

#endregion - end::Binary_Spr[]


# Binary_Wad3X
class Binary_Wad3X(IHaveMetaInfo, ITexture):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Wad3X(r, f)

    #region Headers

    class CharInfo:
        _struct = ('<2H', 4)
        def __init__(self, t):
            (self.startOffset,
            self.charWidth) = t

    class Formats(Enum): Nonex = 0; Tex2 = 0x40; Pic = 0x42; Tex = 0x43; Fnt = 0x46

    #endregion

    def __init__(self, r: BinaryReader, f: FileSource):
        match _pathExtension(f.path):
            case '.pic': type = self.Formats.Pic
            case '.tex': type = self.Formats.Tex
            case '.tex2': type = self.Formats.Tex2
            case '.fnt': type = self.Formats.Fnt
            case _: type = self.Formats.Nonex
        self.transparent = os.path.basename(f.path).startswith('{')
        self.format = (type, (TextureFormat.RGBA32, TexturePixel.Unknown)) if self.transparent \
            else (type, (TextureFormat.RGB24, TexturePixel.Unknown))
        self.name = r.readFWString(16) if type == self.Formats.Tex2 or type == self.Formats.Tex else None
        self.width = r.readUInt32()
        self.height = r.readUInt32()

        # validate
        if self.width > 0x1000 or self.height > 0x1000: raise Exception('Texture width or height exceeds maximum size!')
        elif self.width == 0 or self.height == 0: raise Exception('Texture width and height must be larger than 0!')

        # read pixel offsets
        if type == self.Formats.Tex2 or type == self.Formats.Tex:
            offsets = [r.readUInt32(), r.readUInt32(), r.readUInt32(), r.readUInt32()]
            if r.tell() != offsets[0]: raise Exception('BAD OFFSET')
        elif type == self.Formats.Fnt:
            self.width = 0x100
            rowCount = r.readUInt32()
            rowHeight = r.readUInt32()
            charInfos = r.readSArray(self.CharInfo, 0x100)

        # read pixels
        pixelSize = self.width * self.height
        pixels = self.pixels = [r.readBytes(pixelSize), r.readBytes(pixelSize >> 2), r.readBytes(pixelSize >> 4), r.readBytes(pixelSize >> 6)] if type == self.Formats.Tex2 or type == self.Formats.Tex \
            else [r.readBytes(pixelSize)]
        self.mipMaps = len(pixels)

        # read pallet
        r.skip(2)
        p = self.palette = r.readBytes(0x100 * 3); j = 0
        if type == self.Formats.Tex2:
            for i in range(0x100):
                p[j + 0] = i
                p[j + 1] = i
                p[j + 2] = i
                j += 3

    #region ITexture

    width: int = 0
    height: int = 0
    depth: int = 0
    mipMaps: int = 1
    texFlags: TextureFlags = 0

    def _lambdax(self) -> TextureAsBytes:
        bbp = 4 if self.transparent else 3
        buf = bytearray(sum([len(x) for x in self.pixels]) * bbp); mv = memoryview(buf)
        spans = [range(0, 0)] * len(self.pixels); offset = 0
        for i, p in enumerate(self.pixels):
            size = len(p) * bbp; span = spans[i] = range(offset, offset + size); offset += size
            Raster.blitByPalette(mv[span.start:span.stop], bbp, p, self.palette, 3, 0xFF if self.transparent else None)
        return TextureAsBytes(buf, self.format[1], spans)
    def create(self, platform: str, func: callable): return func(_lambdax)

    #endregion

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Texture', name = os.path.basename(file.path), value = self)),
        MetaInfo('Texture', items = [
            MetaInfo(f'Format: {self.format[0]}'),
            MetaInfo(f'Width: {self.width}'),
            MetaInfo(f'Height: {self.height}'),
            MetaInfo(f'Mipmaps: {self.mipMaps}')
            ])
        ]

#endregion - end::Binary_Wad3X[]

#region Binary_Pak - tag::Binary_Pak[]

# Binary_Pak
class Binary_Pak(ArcBinaryT):
    #region Headers

    P_MAGIC = 0x4b434150 #: PACK

    class P_Header:
        _struct = ('<I2i', 12)
        def __init__(self, t):
            (self.magic,
            self.dirOffset,
            self.dirLength) = t

    class P_File:
        _struct = ('<56s2i', 64)
        def __init__(self, t):
            (self.path,
            self.offset,
            self.fileSize) = t

    #endregion

    def read(self, source: BinaryArchive, r: BinaryReader, tag: object = None) -> None:
        header = r.readS(self.P_Header)
        if header.magic != self.P_MAGIC: raise Exception('BAD MAGIC')
        numFiles = header.dirLength // self.P_File._struct[1]
        r.seek(header.dirOffset)
        files = []
        for s in r.readSArray(self.P_File, numFiles):
            path = unsafe.fixedAString(s.path, 56).replace('\\', '/')
            file = FileSource(path = path, offset = s.offset, fileSize = s.fileSize)
            if path.lower().endswith('.wad'): file.arc = Binary_Wad.SubArchive(source, file, path)
            files.append(file)
        source.files = files

    def readData(self, source: BinaryArchive, r: BinaryReader, file: FileSource, option: object = None) -> BytesIO:
        r.seek(file.offset)
        return BytesIO(r.readBytes(file.fileSize))

#endregion - end::Binary_Pak[]

#region Binary_Wad - tag::Binary_Wad[]

# Binary_Wad
class Binary_Wad(ArcBinaryT):
    #region Headers

    W_MAGIC = 0x32444157 #: WAD2

    class W_Header:
        _struct = ('<I2i', 12)
        def __init__(self, t):
            (self.magic,
            self.lumpCount,
            self.lumpOffset) = t

    class W_Lump:
        _struct = ('<3i4b16s', 32)
        def __init__(self, t):
            (self.offset,
            self.packedSize,
            self.fileSize,
            self.type,
            self.compression,
            self.padding0,
            self.padding1,
            self.path) = t

    #endregion

    def read(self, source: BinaryArchive, r: BinaryReader, tag: object = None) -> None:
        header = r.readS(self.W_Header)
        if header.magic != self.W_MAGIC: raise Exception('BAD MAGIC')
        r.seek(header.lumpOffset)
        source.files = [FileSource(
            path = f'{unsafe.fixedAString(s.path, 16).replace("\\", "/")}.tex',
            hash = s.type,
            offset = s.offset,
            packedSize = s.packedSize,
            fileSize = s.fileSize,
            compressed = s.compression) for s in r.readSArray(self.W_Lump, header.lumpCount)]

    def readData(self, source: BinaryArchive, r: BinaryReader, file: FileSource, option: object = None) -> BytesIO:
        r.seek(file.offset)
        return BytesIO(r.readBytes(file.fileSize))

#endregion - end::Binary_Wad[]