import os, copy
from io import BytesIO
from numpy import linalg, ndarray, array, zeros
from openx.core import log, IWriteToStream, BinaryReader, unsafe
from gamex import ArcBinary, FileSource, MetaInfo, MetaContent, IHaveMetaInfo, DesSer
from gamex.families.Arkane.formats.danae.eerieTypes import POLY, TLVERTEX, E_VERTEX, E_TEXTURE, E_POLY, E_FACE, E_SPRINGS, CLOTHESVERTEX, CLOTHES_DATA, COLLISION_SPHERE, COLLISION_SPHERES_DATA, E_GROUPLIST, E_ACTIONLIST, E_SELECTIONS, E_3DOBJ, E_SAVE_PORTALS, E_PORTALS, E_ROOM_DATA, E_SAVE_ROOM_DATA, E_PORTAL_DATA
from gamex.families.Uncore.formats.compression import decompressBlast

# typedefs
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
            self.vert = TLVERTEX(t[:8])
            v = self.v = array([None]*3)
            norm = self.norm = array([None]*3)
            (v[0], v[1], v[2],
            norm[0], norm[1], norm[2]) = t[8:]
        def to(s) -> E_VERTEX:
            s.vert.color = 0xFF000000
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
            def _groupZ(t) -> E_GROUPLIST: z = t.to(); z.indexes = r.readPArray(None, 'i', z.numIndex) if z.numIndex > 0 else None; return z
            def _selectionsZ(t) -> E_SELECTIONS: z = t.to(); z.selected = r.readPArray(None, 'i', z.numSelected); return z
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
            obj.vertexs = [z.to() for z in r.readSArray(Binary_Ftl.FTL_VERTEX, s.numVertex)] if s.numVertex > 0 else None; obj.point0 = obj.vertexs[obj.origin].v if s.numVertex > 0 else None
            obj.faces = [z.to() for z in r.readSArray(Binary_Ftl.FTL_FACE, s.numFaces)] if s.numFaces > 0 else None
            obj.textures = [z.to() for z in r.readSEach(Binary_Ftl.FTL_TEXTURE, s.numMaps)] if s.numMaps > 0 else None
            obj.groups = [_groupZ(z) for z in r.readSEach(Binary_Ftl.FTL_GROUPLIST, s.numGroups)] if s.numGroups > 0 else None
            obj.actions = [z.to() for z in r.readSEach(Binary_Ftl.FTL_ACTIONLIST, s.numAction)] if s.numAction > 0 else None
            obj.selections = [_selectionsZ(z) for z in r.readSEach(Binary_Ftl.FTL_SELECTIONS, s.numSelections)] if s.numSelections > 0 else None

        # collision spheres
        if header.offsetCollisionSpheres != -1:
            r.seek(header.offsetCollisionSpheres)
            obj.spheres = r.readL32SArray(COLLISION_SPHERE)

        # progressive data
        if header.offsetProgressiveData != -1:
            r.seek(header.offsetProgressiveData)
            numVertex = r.readInt32()
            r.skip(PROGRESSIVE_DATA.SIZEOF * numVertex)

        # clothes data
        if header.offsetClothesData != -1:
            r.seek(header.offsetClothesData)
            numCvert = r.readInt32(); numSprings = r.readInt32()
            obj.cdata = CLOTHES_DATA(
                cvert = r.readSArray(CLOTHESVERTEX, numCvert),
                springs = r.readSArray(E_SPRINGS, numSprings))
        
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

_MAX_BKGX = 160; _MAX_BKGZ = 160; _BKG_SIZX = 100; _BKG_SIZZ = 100

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

    class E_BACKGROUND:
        fastdata: 'F_BKG_DATA'
        exist: int = 1
        xsize: int
        zsize: int
        xdiv: int
        zdiv: int
        xmul: float
        zmul: float
        backg: list['E_BKG_INFO']
        ambient: Vector3
        ambient255: Vector3
        minMax: list['E_SMINMAX']
        numAnchors: int
        anchors: list['ANCHOR_DATA']
        name: str
        def __init__(self, sx: int=_MAX_BKGX, sz: int=_MAX_BKGZ, xdiv: int=_BKG_SIZX, zdiv: int=_BKG_SIZZ):
            self.fastdata = zeros((_MAX_BKGX, _MAX_BKGZ))
            self.xsize = sx
            self.zsize = sz
            if xdiv < 0: xdiv = 1
            if zdiv < 0: zdiv = 1
            self.xdiv = xdiv
            self.zdiv = zdiv
            self.xmul = 1. / xdiv
            self.zmul = 1. / zdiv
            self.backg = [Binary_Fts.E_BKG_INFO()]*(sx * sz)
            for i in range(len(self.backg)): self.backg[i].nothing = True
            self.minMax = [Binary_Fts.E_SMINMAX()]*sz
            for i in range(len(self.minMax)): self.minMax[i].Min = 9999; self.minMax[i].Max = -1

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
            self.version = round(self.version, 7)

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
            v0 = self.v0 = Binary_Fts.F_VERTEX(); v1 = self.v1 = Binary_Fts.F_VERTEX(); v2 = self.v2 = Binary_Fts.F_VERTEX(); v3 = self.v3 = Binary_Fts.F_VERTEX()
            norm = self.norm = array([None]*3); norm2 = self.norm2 = array([None]*3)
            nrml0 = self.nrml0 = array([None]*3); nrml1 = self.nrml1 = array([None]*3); nrml2 = self.nrml2 = array([None]*3); nrml3 = self.nrml3 = array([None]*3)
            (v0.sy, v0.ssx, v0.ssz, v0.stu, v0.stv, v1.sy, v1.ssx, v1.ssz, v1.stu, v1.stv, v2.sy, v2.ssx, v2.ssz, v2.stu, v2.stv, v3.sy, v3.ssx, v3.ssz, v3.stu, v3.stv,
            self.texPtr,
            norm[0], norm[1], norm[2], norm2[0], norm2[1], norm2[2],
            nrml0[0], nrml0[1], nrml0[2], nrml1[0], nrml1[1], nrml1[2], nrml2[0], nrml2[1], nrml2[2], nrml3[0], nrml3[1], nrml3[2],
            self.transVal,
            self.area,
            self.type,
            self.room,
            self.paddy) = t
            self.type = POLY(self.type)
        def to(s, textures: list[E_TEXTURE], bkg: Binary_Fts.E_BACKGROUND) -> E_POLY:
            @staticmethod
            def declareEGInfo(bkg: Binary_Fts.E_BACKGROUND, x: float, y: float, z: float) -> None:
                posx = int(x * bkg.xmul)
                if posx < 0: return
                elif posx >= bkg.xsize: return
                posz = int(z * bkg.zmul)
                if posz < 0: return
                elif posz >= bkg.zsize: return
                eg = bkg.backg[posx + posz * bkg.xsize]
                eg.nothing = False

            texPtr = s.texPtr
            t = E_POLY(
                room = s.room
                area = s.area
                norm = s.norm
                norm2 = s.norm2
                nrml = [s.nrml0, s.nrml1, s.nrml2, s.nrml3]
                tex = next((x for x in textures if x.id == texPtr), None) if texPtr != 0 else None
                transVal = s.transval
                type = s.type
                v = [
                    TLVERTEX(color = 0xFFFFFFFF, rhw = 1., specular = 1, s = array([s.v0.ssx, s.v0.sy, s.v0.ssz]), t = array([s.v0.stu, s.v0.stv])),
                    TLVERTEX(color = 0xFFFFFFFF, rhw = 1., specular = 1, s = array([s.v1.ssx, s.v1.sy, s.v1.ssz]), t = array([s.v1.stu, s.v1.stv])),
                    TLVERTEX(color = 0xFFFFFFFF, rhw = 1., specular = 1, s = array([s.v2.ssx, s.v2.sy, s.v2.ssz]), t = array([s.v2.stu, s.v2.stv])),
                    TLVERTEX(color = 0xFFFFFFFF, rhw = 1., specular = 1, s = array([s.v3.ssx, s.v3.sy, s.v3.ssz]), t = array([s.v3.stu, s.v3.stv]))]
            )

            # clone v
            t.tv = copy.deepcopy(t.v)
            for kk in range(4): t.tv[kk].color = 0xFF000000

            # re-center
            if (s.type & POLY.QUAD) != 0: to = 4; div = 0.25
            else: to = 3; div = 0.333333333333
            t.center = array([0., 0., 0.])
            for h in range(to):
                t.center += t.v[h].s
                if h != 0:
                    t.max[0] = max(t.max[0], t.v[h].s[0]); t.min[0] = min(t.min[0], t.v[h].s[0])
                    t.max[1] = max(t.max[1], t.v[h].s[1]); t.min[1] = min(t.min[1], t.v[h].s[1])
                    t.max[2] = max(t.max[2], t.v[h].s[2]); t.min[2] = min(t.min[2], t.v[h].s[2])
                else: t.min = t.max = t.v[0].s
            t.center *= div

            # distance
            dist = 0.
            for h in range(to): dist = max(dist, linalg.norm(t.v[h].s - t.center))
            t.v[0].rhw = dist

            # declare
            declareEGInfo(bkg, t.center[0], t.center[1], t.center[2])
            declareEGInfo(bkg, t.v[0].s[0], t.v[0].s[1], t.v[0].s[2])
            declareEGInfo(bkg, t.v[1].s[0], t.v[1].s[1], t.v[1].s[2])
            declareEGInfo(bkg, t.v[2].s[0], t.v[2].s[1], t.v[2].s[2])
            if (s.type & POLY.QUAD) != 0: declareEGInfo(bkg, t.v[3].s[0], t.v[3].s[1], t.v[3].s[2])
            return t

    class F_SCENE_HEADER:
        _struct = ('<f5i6f2i', 56)
        def __init__(self, t):
            playerPos = self.playerPos = array([None]*3)
            mscenePos = self.mscenePos = array([None]*3)
            (self.version,
            self.sizeX,
            self.sizeZ,
            self.numTextures,
            self.numPolys,
            self.numAnchors,
            playerPos[0], playerPos[1], playerPos[2],
            mscenePos[0], mscenePos[1], mscenePos[2],
            self.numPortals,
            self.numRooms) = t
            self.version = round(self.version, 7)

    class F_TEXTURE_CONTAINER:
        _struct = ('<2i256s', 8 + 256)
        def __init__(self, t):
            (self.tcPtr,
            self.tempPtr,
            self.fic) = t
            self.fic = unsafe.fixedAStringScan(self.fic, 256)
        def to(s) -> E_TEXTURE:
            return E_TEXTURE(
                id = s.tcPtr,
                path = s.fic)

    class F_ANCHOR_DATA:
        _struct = ('<5f2h', 24)
        def __init__(self, t):
            pos = self.pos = array([None]*3)
            (pos[0], pos[1], pos[2],
            self.radius,
            self.height,
            self.numLinked,
            self.flags) = t

    class F_SCENE_INFO:
        _struct = ('<2I', 8)
        def __init__(self, t):
            (self.numPoly,
            self.numIAnchors) = t

    class ROOM_DIST_DATA:
        _struct = ('<7f', 28)
        def __init__(self, t):
            startPos = self.startPos = array([None]*3)
            endPos = self.endPos = array([None]*3)
            (self.distance,
            startPos[0], startPos[1], startPos[2],
            endPos[0], endPos[1], endPos[2]) = t

    class F_LEVEL:
        playerPos: Vector3 
        mscenePos: Vector3
        textures: list[E_TEXTURE]
        backg: list['E_BKG_INFO']
        portals: E_PORTAL_DATA
        numRoomDistance: int
        roomDistance: list['ROOM_DIST_DATA']

    #endregion

    level: F_LEVEL
    bkg: E_BACKGROUND

    def __init__(self, r: BinaryReader):
        @staticmethod
        def setRoomDistance(level: Binary_Fts.F_LEVEL, i: int, j: int, rd: ROOM_DIST_DATA) -> None:
            if i < 0 or j < 0 or i >= level.numRoomDistance or j >= level.numRoomDistance or level.roomDistance == None: return
            level.roomDistance[i + j * level.numRoomDistance] = rd

        header = r.readS(Binary_Fts.FTS_HEADER)
        if header.version != Binary_Fts._FTS_VERSION: raise Exception('BAD MAGIC')
        if header.count > 0:
            count = 0
            while count < header.count:
                r.readS(Binary_Fts.FTS_HEADER2)
                r.skip(512) # skip check
                count += 1
                if count > 60: raise Exception('BAD HEADER')
        self.level = Binary_Fts.F_LEVEL()
        self.bkg = Binary_Fts.E_BACKGROUND()
        s = BytesIO(decompressBlast(r, r.length - r.tell(), header.compressedsize))
        with BinaryReader(s) as r2:
            # read
            fsh = r2.readS(Binary_Fts.F_SCENE_HEADER)
            if fsh.version != Binary_Fts._FTS_VERSION: raise Exception('BAD MAGIC')
            if fsh.sizeX != self.bkg.xsize: raise Exception('BAD HEADER')
            if fsh.sizeZ != self.bkg.zsize: raise Exception('BAD HEADER')
            self.level.playerPos = fsh.playerPos
            self.level.mscenePos = fsh.mscenePos
            # log.info(f'Header2: {r2.tell()}, 24')
            
            # textures
            textures = self.level.textures = [z.to() for z in r.readSArray(Binary_Ftl.F_TEXTURE_CONTAINER, fsh.numTextures)]
            # log.info(f'Texture: {r2.tell()}')

            # backg
            backg = self.bkg.backg
            for j in range(fsh.sizeZ):
                for i in range(fsh.sizeX):
                    bi = backg[(i + j * fsh.sizeX)]
                    fsi = r2.readS(Binary_Fts.F_SCENE_INFO)
                    #if fsi.numPoly > 0: log.info(f"F[{j},{i}]: {r2.tell()}, {fsi.numPoly}, {fsi.numIAnchors}')
                    bi.numIAnchors = fsi.numIAnchors
                    bi.numPoly = fsi.numPoly
                    bi.polydata = [z.to(textures, self.bkg) for z in r2.readSArray(Binary_Fts.F_POLY, fsi.numPoly)] if fsi.numPoly > 0 else None
                    bi.treat = 0
                    bi.nothing = fsi.numPoly == 0
                    bi.frustrumMaxY = -99999999.
                    bi.frustrumMinY = 99999999.
                    bi.ianchors = None if fsi.numIAnchors <= 0 else r2.readPArray(None, 'i', fsi.numIAnchors)
            #log.info(f'Background: {r2.Tell():x}')

            # anchors
            self.bkg.numAnchors = fsh.numAnchors
            anchors = self.bkg.anchors = [Binary_Fts.ANCHOR_DATA()]*fsh.numAnchors if fsh.numAnchors > 0 else None
            for i in range(fsh.numAnchors):
                a = anchors[i]
                fad = r2.readS(Binary_Fts.F_ANCHOR_DATA)
                a.flags = fad.flags
                a.pos = fad.pos
                a.numLinked = fad.numLinked
                a.height = fad.height
                a.radius = fad.radius
                a.linked = r2.readPArray(None, 'i', fad.numLinked) if fad.numLinked > 0 else None
            #log.info(f'Anchors: {r2.tell()}')

            # rooms
            portals: E_PORTAL_DATA = None
            if fsh.numRooms > 0:
                portals = self.level.portals = E_PORTAL_DATA(
                    numRooms = fsh.numRooms
                    room = [E_ROOM_DATA()]*(fsh.numRooms + 1)
                    numTotal = fsh.numPortals
                    portals = [z.to() for z in r2.readSArray(E_SAVE_PORTALS, fsh.numPortals)])
                for i in range(portals.numRooms + 1):
                    x = r2.readS(E_SAVE_ROOM_DATA)
                    portals.room[i] = E_ROOM_DATA(
                        numPortals = x.numPortals,
                        numPolys = x.numPolys,
                        portals = r2.readPArray(None, 'i', x.numPortals) if x.numPortals > 0 else None,
                        rpData = r2.readSArray(EP_DATA, x.numPolys) if x.numPolys > 0 else None)
            #log.info(f'Portals: {r2.tell()}')

            if portals:
                numRoomDistance = self.level.numRoomDistance = portals.numRooms + 1
                self.level.roomDistance = [ROOM_DIST_DATA()]*(numRoomDistance * numRoomDistance)
                for n in range(numRoomDistance):
                    for m in range(numRoomDistance):
                        setRoomDistance(self.level, m, n, r2.readS(ROOM_DIST_DATA))
            else: self.level.numRoomDistance = 0; self.level.roomDistance = None
            #log.info(f'RoomDistance: {r2.tell()}')

    def writeToStream(self, stream: object): return DesSer.serialize(self, stream)
    def __repr__(self): return DesSer.serialize(self)

    def getInfoNodes(self, resource: MetaManager = None, file: FileSource = None, tag: object = None) -> list[MetaInfo]: return [
        MetaInfo(None, MetaContent(type = 'Text', name = os.path.basename(file.path), value = self))
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
