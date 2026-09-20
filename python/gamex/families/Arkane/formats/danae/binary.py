import os
from io import BytesIO
from numpy import ndarray, array
from openx.core import IWriteToStream, unsafe
from gamex import ArcBinary, FileSource, MetaInfo, MetaContent, IHaveMetaInfo, DesSer
from gamex.families.Arkane.formats.danae.eerieTypes import POLY, TLVERTEX, E_VERTEX, E_TEXTURE, E_FACE, E_GROUPLIST, E_ACTIONLIST, E_SELECTIONS, E_3DOBJ, E_SPRINGS, CLOTHESVERTEX, CLOTHES_DATA, COLLISION_SPHERE, COLLISION_SPHERES_DATA

# typedefs
class BinaryReader: pass
class BinaryArchive: pass
class Archive: pass
class MetaManager: pass

# types
type Vector3 = ndarray

#region Binary_Ftl

# Binary_Ftl
class Binary_Ftl(IHaveMetaInfo, IWriteToStream):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Ftl(r)

    #region Headers

    _FTL_MAGIC = 0x004c5446
    _FTL_VERSION = 0.83257

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
            self.name = unsafe.fixedAStringScan(self.name, 256)

    class FTL_VERTEX:
        _struct = (f'{TLVERTEX._struct[0]}6f', 32 + 24)
        def __init__(self, t):
            vert = self.vert = TLVERTEX(t[:8])
            v = self.v = array([None]*3)
            norm = self.norm = array([None]*3)
            (v[0], v[1], v[2],
            norm[0], norm[1], norm[2]) = t[8:]
        def to(s) -> E_VERTEX:
            return E_VERTEX(
                vert = s.vert,
                v = s.v,
                norm = s.norm,
                vworld = None)

    class FTL_TEXTURE:
        _struct = ('<256s', 256)
        def __init__(self, t):
            (self.name) = t
            self.name = unsafe.fixedAStringScan(self.name, 256)
        def to(s) -> E_TEXTURE:
            name: str  = s.name
            poly: POLY = POLY.NONE_
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
        _struct = ('<4i3Hh6f6h14f', 116)
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
            self.name = unsafe.fixedAStringScan(self.name, 256)
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
            self.name = unsafe.fixedAStringScan(self.name, 256)
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
            self.name = unsafe.fixedAStringScan(self.name, 64)
        def to(s) -> E_SELECTIONS:
            return E_SELECTIONS(
                name = s.name,
                numSelected = s.numSelected)

    #endregion

    obj: E_3DOBJ

    def __init__(self, r: BinaryReader):
        obj = self.obj = E_3DOBJ()
        magic = r.readUInt32()
        if magic != Binary_Ftl._FTL_MAGIC: raise Exception(f"Invalid FTL magic: '{magic}'.")
        version = r.readSingle()
        if version != Binary_Ftl._FTL_VERSION: raise Exception(f"Invalid FLT version: '{version}'.")
        r.skip(512) # skip checksum
        header = r.readS(Binary_Ftl.FTL_HEADER)

        # Check For & Load 3D Data
        if header.offset3Ddata != -1:
            r.seek(header.offset3Ddata)
            s = r.readS(Binary_Ftl.FTL_3DHEADER)
            obj.numVertex = s.numVertex
            obj.numFaces = s.numFaces
            obj.numMaps = s.numMaps
            obj.numGroups = s.numGroups
            obj.numAction = s.numAction
            obj.numSelections = s.numSelections
            obj.origin = s.origin
            obj.file = s.name

            # Alloc'n'Copy vertices
            if s.numVertex > 0:
                vertexList = r.readSArray(Binary_Ftl.FTL_VERTEX, s.numVertex)
                obj.vertexList = [None]*s.numVertex
                for i in range(s.numVertex):
                    obj.vertexList[i] = vertexList[i].to()
                    obj.vertexList[i].vert.color = 0xFF000000
                obj.point0 = obj.vertexList[obj.origin].v

            # Alloc'n'Copy faces
            if s.numFaces > 0:
                faceList = r.readSArray(Binary_Ftl.FTL_FACE, s.numFaces)
                obj.faceList = [None]*s.numFaces
                for i in range(s.numFaces):
                    obj.faceList[i] = faceList[i].to()

            # Alloc'n'Copy textures
            if s.numMaps > 0:
                textures = r.readSEach(Binary_Ftl.FTL_TEXTURE, s.numMaps)
                obj.textures = [None]*s.numMaps
                for i in range(s.numMaps):
                    obj.textures[i] = textures[i].to()

            # Alloc'n'Copy groups
            if s.numGroups > 0:
                groupList = r.readSEach(Binary_Ftl.FTL_GROUPLIST, s.numGroups)
                obj.groupList = [None]*s.numGroups
                for i in range(s.numGroups):
                    obj.groupList[i] = groupList[i].to()
                    if obj.groupList[i].numIndex > 0: obj.groupList[i].indexes = r.readPArray(None, 'i', obj.groupList[i].numIndex)
            # Alloc'n'Copy action points
            if s.numAction > 0:
                actionList = r.readSEach(Binary_Ftl.FTL_ACTIONLIST, s.numAction)
                obj.actionList = [None]*s.numAction
                for i in range(s.numAction):
                    obj.actionList[i] = actionList[i].to()

            # Alloc'n'Copy selections
            if s.numSelections > 0:
                selections = r.readSEach(Binary_Ftl.FTL_SELECTIONS, s.numSelections)
                obj.selections = [None]*s.numSelections
                for i in range(s.numSelections):
                    obj.selections[i] = selections[i].to()
                    obj.selections[i].selected = r.readPArray(None, 'i', obj.selections[i].numSelected)

        # Alloc'n'Copy Collision Spheres Data
        if header.offsetCollisionSpheres != -1:
            r.seek(header.offsetCollisionSpheres)
            csh = r.readS(Binary_Ftl.FTL_COLLISIONSPHERESHEADER)
            obj.sdata = COLLISION_SPHERES_DATA(
                numSpheres = csh.numSpheres,
                spheres = r.readSArray(COLLISION_SPHERE, csh.numSpheres))

        # Alloc'n'Copy Progressive DATA
        if header.offsetProgressiveData != -1:
            r.seek(header.offsetProgressiveData)
            ph = r.readS(Binary_Ftl.FTL_PROGRESSIVEHEADER)
            r.skip(PROGRESSIVE_DATA.SIZEOF * ph.numVertex)

        # Alloc'n'Copy Clothes DATA
        if header.offsetClothesData != -1:
            r.seek(header.offsetClothesData)
            ch = r.readS(Binary_Ftl.FTL_CLOTHESHEADER)
            obj.cdata = CLOTHES_DATA(
                numCvert = ch.numCvert,
                numSprings = ch.numSprings,
                cvert = r.readSArray(CLOTHESVERTEX, ch.numCvert),
                springs = r.readSArray(E_SPRINGS, ch.numSprings))
        
        # process
        obj._centerObjectCoordinates()
        obj._createCedricData()
        obj._precomputeFastAccess()


    def writeToStream(self, stream: object): return DesSer.serialize(self, stream)
    def __repr__(self): return DesSer.serialize(self)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self)),
        MetaInfo('FTL', items = [
            MetaInfo(f'Obj: {self.obj}')
            ])
        ]

#endregion

#region Binary_Fts

# Binary_Fts
class Binary_Fts(IHaveMetaInfo, IWriteToStream):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Fts(r)

    #region Headers

    class ANCHOR_DATA:
        pos: Vector3
        numLinked: int
        flags: int
        linked: list[int]
        radius: float
        height: float

    class E_BKG_INFO:
        treat: int
        nothing: bool
        numPoly: int
        numIAnchors: int
        numPolyin: int
        frustrumMinY: float
        frustrumMaxY: float
        polydata: list[E_POLY]
        # polyin: list[list[E_POLY]]
        ianchors: list[int] # index on anchors list
        flags: int
        tileMinY: float
        tileMaxY: float

    class E_SMINMAX:
        min: int
        max: int

    class F_BKG_DATA:
        treat: int
        nothing: int
        numPoly: int
        numIAnchors: int
        numPolyin: int
        flags: int
        frustrumMinY: float
        frustrumMaxY: float
        polydata: list[E_POLY]
        polyin: list[list[E_POLY]]
        ianchors: list[int] # index on anchors list

    _MAX_BKGX = 160
    _MAX_BKGZ = 160
    _BKG_SIZX = 100
    _BKG_SIZZ = 100

    class E_BACKGROUND:
        fastdata: F_BKG_DATA #[,]  = new F_BKG_DATA[MAX_BKGX, MAX_BKGZ];
        exist: int = 1
        xsize: int
        zsize: int
        xdiv: int
        zdiv: int
        xmul: float
        zmul: float
        backg: list[E_BKG_INFO]
        ambient: Vector3
        ambient255: Vector3
        minMax: list[E_SMINMAX]
        numAnchors: int
        anchors: list[ANCHOR_DATA]
        name: str
        def __init__(self, sx: int=_MAX_BKGX, sz: int=_MAX_BKGZ, xdiv: int=_BKG_SIZX, zdiv: int=_BKG_SIZZ):
            self.xsize = sx
            self.zsize = sz
            if xdiv < 0: xdiv = 1
            if zdiv < 0: zdiv = 1
            self.xdiv = xdiv
            self.zdiv = zdiv
            self.xmul = 1. / Xdiv
            self.zmul = 1. / Zdiv
            self.backg = [E_BKG_INFO()]*(sx * sz)
            for i in range(len(self.backg)): self.backg[i].nothing = True
            self.minMax = [E_SMINMAX()]*sz
            for i in range(len(self.minMax)):
                self.minMax[i].Min = 9999
                self.minMax[i].Max = -1

    _FTS_VERSION = 0.141

    class FTS_HEADER:
        _struct = ('<256sifi3i', 256 + 24)
        def __init__(self, t):
            pad = self.pad = [0]*3
            (self.path,
            self.count,
            self.version,
            self.compressedsize,
            pad[0], pad[1], pad[2]) = t
            self.path = unsafe.fixedAStringScan(self.path, 256)

    class FTS_HEADER2:
        _struct = ('<256s', 256)
        def __init__(self, t):
            (self.path) = t
            self.path = unsafe.fixedAStringScan(self.path, 256)

    class F_VERTEX:
        def __init__(self):
            self.sy = 0
            self.ssx = 0
            self.ssz = 0
            self.stu = 0
            self.stv = 0

    class F_POLY:
        _struct = ('<20fi20fi2h', 172)
        def __init__(self, t):
            v0 = self.v0 = F_VERTEX()
            (v0,

    class F_LEVEL:
        playerPos: Vector3 
        mscenePos: Vector3
        textures: list[E_TEXTURE]
        backg: list[E_BKG_INFO]
        portals: E_PORTAL_DATA
        numRoomDistance: int
        roomDistance: list[ROOM_DIST_DATA]

    #endregion

    level: F_LEVEL
    bkg: E_BACKGROUND

    def __init__(self, r: BinaryReader):
        header = r.readS(Binary_Fts.FTS_HEADER)
        if header.version != Binary_Fts._FTS_VERSION: raise Exception('BAD MAGIC')
        if header.count > 0:
            count = 0
            while count < header.count:
                r.readS<FTS_HEADER2>()
                r.skip(512) # skip check
                count += 1
                if count > 60: raise Exception('BAD HEADER')
        self.level = F_LEVEL()
        self.bkg = E_BACKGROUND()

    def writeToStream(self, stream: object): return DesSer.serialize(self, stream)
    def __repr__(self): return DesSer.serialize(self)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self.data))
        ]

#endregion

#region Binary_Tea

# Binary_Tea
class Binary_Tea(IHaveMetaInfo, IWriteToStream):
    @staticmethod
    async def factory(r: BinaryReader, f: FileSource, s: Archive): return Binary_Tea(r)

    def __init__(self, r: BinaryReader):
        pass

    def writeToStream(self, stream: object): return DesSer.serialize(self, stream)
    def __repr__(self): return DesSer.serialize(self)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self.data))
        ]

#endregion
