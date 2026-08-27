import os
from io import BytesIO
from numpy import ndarray, array
from gamex import ArcBinary, FileSource, MetaInfo, MetaContent, IHaveMetaInfo
from gamex.families.Arkane.formats.danae.eerieTypes import TLVERTEX, E_VERTEX, E_TEXTURE, E_FACE, E_GROUPLIST, E_ACTIONLIST, E_SELECTIONS, E_3DOBJ

# typedefs
class BinaryReader: pass
class BinaryArchive: pass
class Archive: pass
class MetaManager: pass

#region Binary_Ftl

# Binary_Ftl
class Binary_Ftl(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Ftl(r)

    #region Headers

    FTL_MAGIC = 0x004c5446
    FTL_VERSION = 0.83257

    class FTL_HEADER:
        _struct = ('<6i', 24)
        def __init__(self, t):
            (self.offset3Ddata,           # -1 = no
            self.offsetCylinder,          # -1 = no
            self.offsetProgressiveData,   # -1 = no
            self.offsetClothesData,       # -1 = no
            self.offsetCollisionSpheres,  # -1 = no
            self.offsetPhysicsBox) = t    # -1 = no

    class FTL_PROGRESSIVEHEADER:
        _struct = ('<i', 4)
        def __init__(self, t):
            (self.numVertex) = t

    class FTL_CLOTHESHEADER:
        _struct = ('<2i', 8)
        def __init__(self, t):
            (self.numCvert,
            self.numSprings) = t

    class FTL_COLLISIONSPHERESHEADER:
        _struct = ('<i', 4)
        def __init__(self, t):
            (self.numSpheres) = t

    class FTL_3DHEADER:
        _struct = ('<7i256s', 28 + 256)
        def __init__(self, t):
            (self.numVertex,
            self.numFaces,
            self.numMaps,
            self.numGroups,
            self.numAction,
            self.numSelections,
            self.origin,
            self.name) = t
            self.name = self.name.decode('ascii')

    class FTL_VERTEX:
        _struct = (f'<{TLVERTEX._struct[0]}6f', 0)
        def __init__(self, t):
            v = self.v = array([None]*3)
            norm = self.norm = array([None]*3)
            (self.vert,
            v[0], v[1], v[2],
            norm[0], norm[1], norm[2]) = t
        def to(s) -> E_VERTEX:
            return E_VERTEX(
                Vert = s.vert,
                V = s.v,
                Norm = s.norm,
                VWorld = None)

    class FTL_TEXTURE:
        _struct = ('<256s', 256)
        def __init__(self, t):
            (self.name) = t
            self.name = self.name.decode('ascii')
        def to(s) -> E_TEXTURE:
            name: str  = s.name
            poly: POLY = 0
            if 'NPC_' in name: poly |= POLY.LATE_MIP
            if 'nocol' in name: poly |= POLY.NOCOL
            if 'climb' in name: poly |= POLY.CLIMB # change string depending on GFX guys
            if 'fall' in name: poly |= POLY.FALL
            if 'lava' in name: poly |= POLY.LAVA
            if 'water' in name: poly |= POLY.WATER | POLY.TRANS
            elif 'spider_web' in name: poly |= POLY.WATER | POLY.TRANS
            elif '[metal]' in name: poly |= POLY.METAL
            return E_TEXTURE(
                path = s.name,
                poly = poly)

    class FTL_FACE:
        _struct = ('<4i3Hh6f6h14f', -1)
        def __init__(self, t):
            rgb = self.rbg = array([None]*3)
            vid = self.vid = array([None]*3)
            u = self.u = array([None]*3)
            v = self.v = array([None]*3)
            ou = self.ou = array([None]*3)
            ov = self.ov = array([None]*3)
            norm = self.norm = array([None]*3)
            nrmls0 = self.nrmls0 = array([None]*3)
            nrmls1 = self.nrmls1 = array([None]*3)
            nrmls2 = self.nrmls2 = array([None]*3)
            (self.faceType, # 0 = flat, 1 = text, 2 = Double-Side
            rgb[0], rgb[1], rgb[2],
            vid[0], vid[1], vid[2],
            self.texId,
            u[0], u[1], u[2],
            v[0], v[1], v[2],
            ou[0], ou[1], ou[2],
            ov[0], ov[1], ov[2],
            self.transVal,
            norm[0], norm[1], norm[2],
            nrmls0[0], nrmls0[1], nrmls0[2], nrmls1[0], nrmls1[1], nrmls1[2], nrmls2[0], nrmls2[1], nrmls2[2],
            self.temp) = t
        def to(s) -> E_FACE:
            return E_FACE(
                faceType = s.faceType,
                texId = s.texId,
                u = s.u,
                v = s.v,
                ou = s.ou,
                ov = s.ov,
                transVal = s.transVal,
                norm = s.norm,
                nrmls = [s.nrmls0, s.nrmls1, s.nrmls2],
                temp = s.temp)

    class FTL_GROUPLIST:
        _struct = ('<256s3if', 256 + 16)
        def __init__(self, t):
            (self.name,
            self.origin,
            self.numIndex,
            self.trash, #indexes
            self.size) = t
            self.name = self.name.decode('ascii')
        def to(s) -> E_GROUPLIST:
            return E_GROUPLIST(
                name = s.name,
                origin = s.origin,
                numIndex = s.numIndex,
                size = s.size)

    class FTL_ACTIONLIST:
        _struct = ('<256s3i', 256 + 12)
        def __init__(self, t):
            (self.name,
            self.idx, #index vertex
            self.act, #action
            self.sfx) = t #sfx
            self.name = self.name.decode('ascii')
        def to(s) -> E_ACTIONLIST:
            return E_ACTIONLIST(
                name = s.name,
                idx = s.idx,
                act = s.act,
                sfx = s.sfx)

    class FTL_SELECTIONS:
        _struct = ('<64s2i', 64 + 8)
        def __init__(self, t):
            (self.name,
            self.numSelected,
            self.trash) = t #selected
            self.name = self.name.decode('ascii')
        def to(s) -> E_SELECTIONS:
            return E_SELECTIONS(
                name = s.name,
                numSelected = s.numSelected)

    #endregion

    obj: E_3DOBJ

    def __init__(self, r: BinaryReader):
        print(r.readBytes(10))
        exit(0)
        obj = E_3DOBJ()
        magic = r.readUInt32()
        if magic != Binary_Ftl.FTL_MAGIC: raise Exception(f"Invalid FTL magic: '{magic}'.")
        version = r.readSingle()
        if version != Binary_Ftl.FTL_VERSION: raise Exception(f"Invalid FLT version: '{version}'.")
        r.skip(512) # skip checksum
        header = r.readS(Binary_Ftl.FTL_HEADER)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        # MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self.data))
        ]

#endregion

#region Binary_Fts

# Binary_Fts
class Binary_Fts(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Fts(r)

    #region Headers

    #endregion

    def __init__(self, r: BinaryReader):
        pass

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        # MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self.data))
        ]

#endregion

#region Binary_Tea

# Binary_Tea
class Binary_Tea(IHaveMetaInfo):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Tea(r)

    def __init__(self, r: BinaryReader):
        pass

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        # MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self.data))
        ]

#endregion
