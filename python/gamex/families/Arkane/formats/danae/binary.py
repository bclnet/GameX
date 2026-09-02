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
        obj = E_3DOBJ()
        magic = r.readUInt32()
        if magic != Binary_Ftl.FTL_MAGIC: raise Exception(f"Invalid FTL magic: '{magic}'.")
        version = r.readSingle()
        if version != Binary_Ftl.FTL_VERSION: raise Exception(f"Invalid FLT version: '{version}'.")
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
                print('FTL_GROUPLIST')
                groupList = r.readSEach(Binary_Ftl.FTL_GROUPLIST, s.numGroups)
b                obj.groupList = [None]*s.numGroups
                for i in range(s.numGroups):
                    obj.groupList[i] = groupList[i].to()
                    if obj.groupList[i].numIndex > 0: obj.groupList[i].indexes = r.readPArray(None, 'i', obj.groupList[i].numIndex)
            print('HERE')
            # Alloc'n'Copy action points
            if s.numAction > 0:
                print('FTL_ACTIONLIST')
                actionList = r.readSEach(Binary_Ftl.FTL_ACTIONLIST, s.numAction)
                obj.actionList = [None]*s.numAction
                for i in range(s.numAction):
                    obj.actionList[i] = actionList[i].to()

            # Alloc'n'Copy selections
            if s.numSelections > 0:
                print('FTL_SELECTIONS')
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
        #obj.CenterObjectCoordinates()
        #obj.CreateCedricData()
        #obj.CreatePFaces()
        #obj.PrecomputeFastAccess()

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
