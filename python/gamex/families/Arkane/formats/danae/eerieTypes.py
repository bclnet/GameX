from numpy import ndarray, array
from enum import Enum, Flag
from quaternion import quaternion

# types
type Vector2 = ndarray
type Vector3 = ndarray
type Quaternion = ndarray

#struct
#{
#    Vector3 v[3];
#} EERIE_TRI; # Aligned 1 2 4

#struct
#{
#    EERIE_2D min;
#EERIE_2D max;
#} EERIE_2D_BBOX; # Aligned 1 2 4 8

#struct
#{
#    Vector3 min;
#Vector3 max;
#} EERIE_3D_BBOX; # Aligned 1 2 4

#struct
#{
#    char exist;
#char type;
#char treat;
#char selected;
#short extras;
#short status; # on/off 1/0
#Vector3 pos;
#float fallstart;
#float fallend;
#float falldiff;
#float falldiffmul;
#float precalc;
#EERIE_RGB rgb255;
#float intensity;
#EERIE_RGB rgb;
#float i;
#Vector3 mins;
#Vector3 maxs;
#float temp;
#long ltemp;
#EERIE_RGB ex_flicker;
#float ex_radius;
#float ex_frequency;
#float ex_size;
#float ex_speed;
#float ex_flaresize;
#long tl;
#unsigned long time_creation;
#long duration; # will start to fade before the end of duration...
#long sample;
#} EERIE_LIGHT; # Aligned 1 2 4

#enum EERIE_TYPES_EXTRAS_MODE
#{
#    EXTRAS_SEMIDYNAMIC = 0x00000001,
#    EXTRAS_EXTINGUISHABLE = 0x00000002,
#    EXTRAS_STARTEXTINGUISHED = 0x00000004,
#    EXTRAS_SPAWNFIRE = 0x00000008,
#    EXTRAS_SPAWNSMOKE = 0x00000010,
#    EXTRAS_OFF = 0x00000020,
#    EXTRAS_COLORLEGACY = 0x00000040,
#    EXTRAS_NOCASTED = 0x00000080,
#    EXTRAS_FIXFLARESIZE = 0x00000100,
#    EXTRAS_FIREPLACE = 0x00000200,
#    EXTRAS_NO_IGNIT = 0x00000400,
#    EXTRAS_FLARE = 0x00000800
#};

##define TYP_SPECIAL1 1


##*************************************************************************************
## EERIE Types
##*************************************************************************************

#struct E_MATRIX

class MATERIAL(Enum):
    NONE_ = 0
    WEAPON = 1
    FLESH = 2
    METAL = 3
    GLASS = 4
    CLOTH = 5
    WOOD = 6
    EARTH = 7
    WATER = 8
    ICE = 9
    GRAVEL = 10
    STONE = 11
    FOOT_LARGE = 12
    FOOT_BARE = 13
    FOOT_SHOE = 14
    FOOT_METAL = 15
    FOOT_STEALTH = 16

class POLY(Flag):
    NONE_ = 0
    NO_SHADOW = 1
    DOUBLESIDED = 1 << 1
    TRANS = 1 << 2
    WATER = 1 << 3
    GLOW = 1 << 4
    #
    IGNORE = 1 << 5
    QUAD = 1 << 6
    TILED = 1 << 7
    METAL = 1 << 8
    HIDE = 1 << 9
    #
    STONE = 1 << 10
    WOOD = 1 << 11
    GRAVEL = 1 << 12
    EARTH = 1 << 13
    NOCOL = 1 << 14
    LAVA = 1 << 15
    CLIMB = 1 << 16
    FALL = 1 << 17
    NOPATH = 1 << 18
    NODRAW = 1 << 19
    PRECISE_PATH = 1 << 20
    NO_CLIMB = 1 << 21
    ANGULAR = 1 << 22
    ANGULAR_IDX0 = 1 << 23
    ANGULAR_IDX1 = 1 << 24
    ANGULAR_IDX2 = 1 << 25
    ANGULAR_IDX3 = 1 << 26
    LATE_MIP = 1 << 27

class TLVERTEX:
    _struct = ('<4f2I2f', 32)
    s: Vector3           # Screen coordinates
    rhw: float           # Reciprocal of homogeneous w
    color: int          # Vertex color
    specular: int       # Specular component of vertex
    t: Vector2           # Texture coordinates
    def __init__(self, t):
        s = self.s = array([None]*3)
        t_ = self.t = array([None]*2)
        (s[0], s[1], s[2],
        self.rhw,
        self.color,
        self.specular,
        t_[0], t_[1]) = t

class E_CYLINDER:
    _struct = ('<5f', 20)
    origin: Vector3
    radius: float
    height: float

class E_SPHERE:
    _struct = ('<4f', 16)
    origin: Vector3
    radius: float

class E_TEXTURE:
    id: int
    path: str
    poly: POLY
    def __init__(self, id: int=None, path: str=None, poly: POLY=None):
        self.id = id
        self.path = path
        self.poly = poly

class E_POLY:
    type: POLY # at least 16 bits
    min: Vector3
    max: Vector3
    norm: Vector3
    norm2: Vector3
    v: list[TLVERTEX] # new TLVERTEX[4];
    tv: list[TLVERTEX] # new TLVERTEX[4];
    nrml: list[Vector3] # new Vector3[4];
    tex: E_TEXTURE
    center: Vector3
    transVal: float
    area: float
    room: int
    misc: int
    #distBump: float
    #uslInd: list[int] # new ushort[4];
    def memset(self):
        self.misc = 0

class E_VERTEX:
    vert: TLVERTEX
    v: Vector3
    norm: Vector3
    vworld: Vector3
    def __init__(self, vert: TLVERTEX=None, v: Vector3=None, norm: Vector3=None, vworld: Vector3=None):
        self.vert = vert
        self.v = v
        self.norm = norm
        self.vworld = vworld

class E_FACE:
    faceType: int;  # 0 = flat, 1 = text, 2 = Double-Side
    texId: int
    vid: Vector3
    u: Vector3
    v: Vector3
    transVal: float
    norm: Vector3
    nrmls: list[Vector3]
    temp: float
    ou: Vector3
    ov: Vector3
    color: list[Vector2]
    def __init__(self, faceType: int=None, texId: int=None, vid: Vector3=None, u: Vector3=None, v: Vector3=None, transVal: float=None, norm: Vector3=None, nrmls: list[Vector3]=None, temp: float=None, ou: Vector3=None, ov: Vector3=None, color: list[Vector2]=None):
        self.faceType = faceType
        self.texId = texId
        self.vid = vid
        self.u = u
        self.v = v
        self.transVal = transVal
        self.norm = norm
        self.nrmls = nrmls
        self.temp = temp
        self.ou = ou
        self.ov = ov
        self.color = color

##define MAX_PFACE 16
#struct E_PFACE
#{
#    #short faceidx[MAX_PFACE];
#    #int facetype;
#    #short texid;  #long
#    #short nbvert;
#    #float transval;
#    #ushort vid[MAX_PFACE];
#    #float u[MAX_PFACE];
#    #float v[MAX_PFACE];
#    #D3DCOLOR color[MAX_PFACE];
#}

##***********************************************************************
##*		BEGIN EERIE OBJECT STRUCTURES									*
##***********************************************************************
#struct
#{
#    short nb_Nvertex;
#short nb_Nfaces;
#short* Nvertex;
#short* Nfaces;
#} NEIGHBOURS_DATA; # Aligned 1 2 4

class PROGRESSIVE_DATA: # Aligned 1 2 4
    SIZEOF = 16
    # ingame data
    actualCollapse: int # -1 = no collapse
    needComputing: int
    collapseRatio: float
    # static data
    collapseCost: float
    collapseCandidate: int
    padd: int

class E_SPRINGS:
    _struct = ('<2h3fi', 20)
    def __init__(self, t):
        (self.startidx,
        self.endidx,
        self.restlength,
        self.constant,
        self.damping,
        self.type) = t
    startidx: int
    endidx: int
    restlength: float
    constant: float # spring constant
    damping: float # spring damping
    type: int

##define CLOTHES_FLAG_NORMAL	0
##define CLOTHES_FLAG_FIX	1
##define CLOTHES_FLAG_NOCOL	2

class CLOTHESVERTEX:
    _struct = ('<h2b22f', 92)
    def __init__(self, t):
        pos = self.pos = array([None]*3)
        velocity = self.velocity = array([None]*3)
        force = self.force = array([None]*3)
        t_pos = self.t_pos = array([None]*3)
        t_velocity = self.t_velocity = array([None]*3)
        t_force = self.t_force = array([None]*3)
        lastpos = self.lastpos = array([None]*3)
        (self.idx,
        self.flags,
        self.coll,
        pos[0], pos[1], pos[2],
        velocity[0], velocity[1], velocity[2],
        force[0], force[1], force[2],
        self.mass,
        t_pos[0], t_pos[1], t_pos[2],
        t_velocity[0], t_velocity[1], t_velocity[2],
        t_force[0], t_force[1], t_force[2],
        lastpos[0], lastpos[1], lastpos[2]) = t
    idx: int
    flags: int
    coll: int
    pos: Vector3
    velocity: Vector3
    force: Vector3
    mass: float # 1.f/mass
    #
    t_pos: Vector3
    t_velocity: Vector3
    t_force: Vector3
    #
    lastpos: Vector3

class CLOTHES_DATA:
    cvert: list[CLOTHESVERTEX]
    #backup: list[CLOTHESVERTEX]
    numCvert: int
    numSprings: int
    springs: list[E_SPRINGS]
    def __init__(self, cvert: list[CLOTHESVERTEX]=None, numCvert: int=None, numSprings: int=None, springs: list[E_SPRINGS]=None):
        self.cvert = cvert
        self.numCvert = numCvert
        self.numSprings = numSprings
        self.springs = springs

class COLLISION_SPHERE:
    _struct = ('<2hf', 8)
    def __init__(self, t):
        (self.idx,
        self.flags,
        self.radius) = t
    idx: int
    flags: int
    radius: float

class COLLISION_SPHERES_DATA:
    numSpheres: int
    spheres: list[COLLISION_SPHERE]
    def __init__(self, numSpheres: int=None, spheres: list[COLLISION_SPHERE]=None):
        self.numSpheres = numSpheres
        self.spheres = spheres

#struct
#{
#    Vector3 initpos;
#Vector3 temp;
#Vector3 pos;
#Vector3 velocity;
#Vector3 force;
#Vector3 inertia;
#float mass;
#} PHYSVERT; # Aligned 1 2 4

#struct
#{
#    PHYSVERT* vert;
#long nb_physvert;
#short active;
#short stopcount;
#float radius; #radius around vert[0].pos for spherical collision
#float storedtiming;
#} PHYSICS_BOX_DATA; # Aligned 1 2 4

#struct
#{
#    long sx;
#long sy;
#unsigned long bpp;
#unsigned char* bmpdata;
#} EERIE_MAP; # Aligned 1 2 4

class E_GROUPLIST:
    name: str
    origin: int
    numIndex: int
    indexes: list[int]
    size: float
    def __init__(self, name: str=None, origin: int=None, numIndex: int=None, indexes: list[int]=None, size: float=None):
        self.name = name
        self.origin = origin
        self.numIndex = numIndex
        self.indexes = indexes
        self.size = size

class E_ACTIONLIST:
    name: str
    idx: int #index vertex;
    act: int #action
    sfx: int #sfx
    def __init__(self, name: str=None, idx: int=None, act: int=None, sfx: int=None):
        self.name = name
        self.idx = idx
        self.act = act
        self.sfx = sfx

#struct
#{
#    float xmin;
#float xmax;
#float ymin;
#float ymax;
#float zmin;
#float zmax;
#} CUB3D; # Aligned 1 2 4

#struct
#{
#    long link_origin;
#Vector3 link_position;
#Vector3 scale;
#Vector3 rot;
#unsigned long flags;
#} EERIE_MOD_INFO; # Aligned 1 2 4

#struct
#{
#    long lgroup; #linked to group n� if lgroup=-1 NOLINK
#long lidx;
#long lidx2;
#void* obj;
#EERIE_MOD_INFO modinfo;
#void* io;
#} EERIE_LINKED; # Aligned 1 2 4

class E_SELECTIONS:
    name: str
    numSelected: int
    selected: list[int]
    def __init__(self, name: str=None, numSelected: int=None, selected: list[int]=None):
        self.name = name
        self.numSelected = numSelected
        self.selected = selected

##define DRAWFLAG_HIGHLIGHT	1

class E_FASTACCESS:
    viewAttach: int
    primaryAttach: int
    leftAttach: int
    weaponAttach: int
    secondaryAttach: int
    mouthGroup: int
    jawGroup: int
    headGroupOrigin: int
    headGroup: int
    mouthGroupOrigin: int
    vright: int
    uright: int
    fire: int
    selHead: int
    selChest: int
    selLeggings: int
    carryAttach: int
    _padd: int
    def __init__(self, viewAttach: int=None, primaryAttach: int=None, leftAttach: int=None, weaponAttach: int=None, secondaryAttach: int=None, mouthGroup: int=None, jawGroup: int=None, headGroupOrigin: int=None, headGroup: int=None, mouthGroupOrigin: int=None, vright: int=None, uright: int=None, fire: int=None, selHead: int=None, selChest: int=None, selLeggings: int=None, carryAttach: int=None):
        self.viewAttach = viewAttach
        self.primaryAttach = primaryAttach
        self.leftAttach = leftAttach
        self.weaponAttach = weaponAttach
        self.secondaryAttach = secondaryAttach
        self.mouthGroup = mouthGroup
        self.jawGroup = jawGroup
        self.headGroupOrigin = headGroupOrigin
        self.headGroup = headGroup
        self.mouthGroupOrigin = mouthGroupOrigin
        self.vright = vright
        self.uright = uright
        self.fire = fire
        self.selHead = selHead
        self.selChest = selChest
        self.selLeggings = selLeggings
        self.carryAttach = carryAttach

class E_BONE:
    numIdxVertices: int; idxVertices: list[int]
    originalGroup: E_GROUPLIST
    father: int
    quatAnim: quaternion; transAnim: Vector3; scaleAnim: Vector3
    quatLast: quaternion; transLast: Vector3; scaleLast: Vector3
    quatInit: quaternion; transInit: Vector3; scaleInit: Vector3
    transInitGlobal: Vector3 = None
    def __init__(self):
        self.numIdxVertices = 0; self.idxVertices = []
        self.quatInit = quaternion(); self.quatAnim = quaternion()
        self.scaleInit = array([0]*3); self.scaleAnim = array([0]*3)
    def addIdxToBone(self, idx: int) -> None: self.idxVertices.append(idx); self.numIdxVertices += 1

class E_CDATA:
    bones: list[E_BONE]; numBones: int

##########################################
#struct
#{
#    float x;
#float y;
#float z;
#float w;
#} EERIE_3DPAD;

class E_3DOBJ:
    #name: str
    file: str
    #pos: Vector3
    point0: Vector3
    #angle: Vector3
    origin: int
    #ident: int
    numVertex: int
    #trueNumVertex: int
    numFaces: int
    numPfaces: int
    numMaps: int
    numGroups: int
    numAction: int
    numSelections: int
    #drawFlags: int
    #VertexLocal: EERIE_3DPAD
    vertexList: list[E_VERTEX]
    #vertexList3: list[E_VERTEX]

    faceList: list[E_FACE]
    #pfaceList: list[EERIE_PFACE];
    #mapList: list[EERIE_MAP]
    groupList: list[E_GROUPLIST]
    actionList: list[E_ACTIONLIST]
    selections: list[E_SELECTIONS]
    textures: list[E_TEXTURE]

    #originalTextures: bytes
    #cub: CUB3D
    #quat: EERIE_QUAT
    #linked: EERIE_LINKED
    #numLinked: int

    #pbox: PHYSICS_BOX_DATA
    #pdata: PROGRESSIVE_DATA
    #ndata: NEIGHBOURS_DATA
    cdata: CLOTHES_DATA
    sdata: COLLISION_SPHERES_DATA
    fastAccess: E_FASTACCESS
    #c_data: EERIE_C_DATA

    def _centerObjectCoordinates(self) -> None:
        offset = self.vertexList[self.origin].v
        if offset[0] == 0 and offset[1] == 0 and offset[2] == 0: return
        log.info(f'NOT CENTERED {self.file}\n')
        for i in range(self.numVertex): self.vertexList[i].v -= offset; self.vertexList[i].vert.s -= offset
        self.point0 -= offset
    def _createCedricData(self) -> None:
        def getFather(origin: int, startGroup: int) -> int:
            for i in range(startGroup, -1, -1):
                for j in range(self.groupList[i].numIndex):
                    if self.groupList[i].indexes[j] == origin: return i
            return -1

        self.cdata = E_CDATA()
        if self.numGroups <= 0:
            self.cdata.numBones = 1; self.cdata.bones = [E_BONE()]*1
            s = self.cdata.bones[0]
            for i in range(self.numVertex): s.addIdxToBone(i)
            s.transInitGlobal = s.TransInit
            s.originalGroup = None
            s.father = -1
        else:
            self.cdata.numBones = self.numGroups; self.cdata.bones = [E_BONE()]*self.cdata.numBones
            temp = [False]*self.numVertex
            for i in range(self.numGroups - 1, -1, -1):
                s = self.cdata.bones[i]
                vorigin = self.vertexList[self.groupList[i].origin]
                for j in range(self.groupList[i].numIndex):
                    if not temp[self.groupList[i].indexes[j]]: temp[self.groupList[i].indexes[j]] = True; s.addIdxToBone(self.groupList[i].indexes[j])
                s.transInit = vorigin.v.copy()
                s.transInitGlobal = s.transInit
                s.originalGroup = self.groupList[i];
                s.father = getFather(self.groupList[i].origin, i - 1)

            # Try to correct lonely vertex
            for i in range(self.numVertex):
                ok = False
                for j in range(self.numGroups):
                    for k in range(self.groupList[j].numIndex):
                        if self.groupList[j].indexes[k] == i: ok = True; break
                    if ok: break
                if not ok: self.cdata.bones[0].addIdxToBone(i)

            for i in range(self.numGroups - 1, -1, -1):
                s = self.cdata.bones[i]
                if s.father >= 0: s.transInit -= self.cdata.bones[s.father].transInit
                s.transInitGlobal = s.transInit

        # Build proper mesh
        obj = self.cdata
        for i in range(obj.numBones):
            s = obj.bones[i];
            if s.father >= 0:
                f = obj.bones[s.father]
                s.quatAnim = f.quatAnim * s.quatInit # Rotation
                E_3DOBJ._transformVertexQuat(f.quatAnim, s.transInit, s.transAnim) # Translation
                s.transAnim = f.transAnim + s.transAnim
                s.ScaleAnim = array([1.]*3) # Scale
            else:
                s.quatAnim = s.quatInit # Rotation
                s.transAnim = s.transInit # Translation
                s.scaleAnim = array([1.]*3) # Scale
        self.vertexLocal = [array([None]*4)]*self.numVertex
        for i in range(obj.numBones):
            s = obj.bones[i]
            vec = s.transAnim
            for v in range(s.numIdxVertices):
                t = self.vertexList[s.idxVertices[v]].v - vec
                E_3DOBJ._transformInverseVertexQuat(s.quatAnim, t, t)
                self.vertexLocal[s.idxVertices[v]] = array([t[0], t[1], t[2], 0.])
    
    def _precomputeFastAccess(self) -> None:
        def getSelection(selName: str) -> int:
            selName = selName.casefold()
            for i in range(self.numSelections):
                if self.selections[i].name.casefold() == selName: return i
            return -1
        def getGroup(groupName: str) -> None:
            groupName = groupName.casefold()
            for i in range(self.numGroups):
                if self.groupList[i].name.casefold() == groupName: return i
            return -1
        def getActionPointIdx(text: str) -> None:
            text = text.casefold()
            for i in range(self.numAction):
                if self.actionList[i].name.casefold() == text: return self.actionList[i].idx
            return -1
        self.fastAccess = E_FASTACCESS(
            vright = getActionPointIdx('V_RIGHT'),
            uright = getActionPointIdx('U_RIGHT'),
            viewAttach = getActionPointIdx('View_attach'),
            primaryAttach = getActionPointIdx('PRIMARY_ATTACH'),
            leftAttach = getActionPointIdx('LEFT_ATTACH'),
            weaponAttach = getActionPointIdx('WEAPON_ATTACH'),
            secondaryAttach = getActionPointIdx('SECONDARY_ATTACH'),
            jawGroup = getGroup('jaw'),
            mouthGroup = (mouthGroup := getGroup('mouth all')),
            mouthGroupOrigin = -1 if mouthGroup == -1 else self.groupList[mouthGroup].origin,
            headGroup = (headGroup := getGroup('head')),
            headGroupOrigin = -1 if headGroup == -1 else self.groupList[headGroup].origin,
            fire = getActionPointIdx('FIRE'),
            carryAttach = getActionPointIdx('CARRY_ATTACH'),
            selHead = getSelection('head'),
            selChest = getSelection('chest'),
            selLeggings = getSelection('leggings'))

    @staticmethod
    def _transformVertexQuat(q: quaternion, s: Vector3, t: Vector3) -> None:
        rx = s[0] * q.w - s[1] * q.z + s[2] * q.y; ry = s[1] * q.w - s[2] * q.x + s[0] * q.z; rz = s[2] * q.w - s[0] * q.y + s[1] * q.x; rw = s[0] * q.x + s[1] * q.y + s[2] * q.z
        t[0] = q.w * rx + q.x * rw + q.y * rz - q.z * ry; t[1] = q.w * ry + q.y * rw + q.z * rx - q.x * rz; t[2] = q.w * rz + q.z * rw + q.x * ry - q.y * rx

    @staticmethod
    def _transformInverseVertexQuat(q: quaternion, s: Vector3, t: Vector3) -> None:
        p = quaternion.inverse(q)
        x = s[0]; y = s[1]; z = s[2]
        qx = p.x; qy = p.y; qz = p.z; qw = p.w
        rx = x * qw - y * qz + z * qy; ry = y * qw - z * qx + x * qz; rz = z * qw - x * qy + y * qx; rw = x * qx + y * qy + z * qz
        t[0] = qw * rx + qx * rw + qy * rz - qz * ry; t[1] = qw * ry + qy * rw + qz * rx - qx * rz; t[2] = qw * rz + qz * rw + qx * ry - qy * rx

#struct
#{
#    long nbobj;
#EERIE_3DOBJ** objs;
#Vector3 pos;
#Vector3 point0;
#long nbtex;
#TextureContainer** texturecontainer;
#long nblight;
#EERIE_LIGHT** light;
#float ambient_r;
#float ambient_g;
#float ambient_b;
#CUB3D cub;
#} EERIE_3DSCENE; # Aligned 1 2 4

##define MAX_SCENES 64
#struct
#{
#    long nb_scenes;
#EERIE_3DSCENE* scenes[MAX_SCENES];
#CUB3D cub;
#Vector3 pos;
#Vector3 point0;
#} EERIE_MULTI3DSCENE; # Aligned 1 2 4

#struct
#{
#    long num_frame;
#long flag;
#int master_key_frame;
#short f_translate; #int
#short f_rotate; #int
#float time;
#Vector3 translate;
#EERIE_QUAT quat;
#long sample;
#} EERIE_FRAME; # Aligned 1 2 4

#struct
#{
#    int key;
#Vector3 translate;
#EERIE_QUAT quat;
#Vector3 zoom;
#} EERIE_GROUP; # Aligned 1 2 4

## Animation playing flags
##define EA_LOOP			1	# Must be looped at end (indefinitely...)
##define EA_REVERSE		2	# Is played reversed (from end to start)
##define EA_PAUSED		4	# Is paused
##define EA_ANIMEND		8	# Has just finished
##define	EA_STATICANIM	16	# Is a static Anim (no movement offset returned).
##define	EA_STOPEND		32	# Must Be Stopped at end.
##define EA_FORCEPLAY	64	# User controlled... MUST be played...
##define EA_EXCONTROL	128	# ctime externally set, no update.
#struct
#{
#    float anim_time;
#unsigned long flag;
#long nb_groups;
#long nb_key_frames;
#EERIE_FRAME* frames;
#EERIE_GROUP* groups;
#unsigned char* voidgroups;
#} EERIE_ANIM; # Aligned 1 2 4

##-------------------------------------------------------------------------
#Portal Data;

class SAVE_EERIEPOLY:
    _struct = ('<?', -1)
    type: POLY # at least 16 bits
    min: Vector3; max: Vector3
    norm: Vector3; norm2: Vector3
    v0: TLVERTEX; v1: TLVERTEX; v2: TLVERTEX; v3: TLVERTEX
    tv0: TLVERTEX; tv1: TLVERTEX; tv2: TLVERTEX; tv3: TLVERTEX
    nrml0: Vector3; nrml1: Vector3; nrml2: Vector3; nrml3: Vector3
    texPtr: int
    center: Vector3
    transVal: float
    area: float
    room: int
    misc: int

class E_SAVE_PORTALS:
    _struct = ('<?', -1)
    poly: SAVE_EERIEPOLY
    room1: int # facing normal
    room2: int
    usePortal: int
    paddy: int

class E_PORTALS:
    _struct = ('<?', -1)
    poly: E_POLY
    room1: int # facing normal
    room2: int
    usePortal: int
    paddy: int

    def memset(): pass

class EP_DATA:
    px: int
    py: int
    idx: int
    padd: int

class E_ROOM_DATA:
    numPortals: int
    portals: list[int]
    numPolys: int
    epData: list[EP_DATA]
    center: Vector3
    radius: float
    pussIndice: list[int]
    #vertexBuffer: LPDIRECT3DVERTEXBUFFER7
    numTextures: int
    textureContainer: E_TEXTURE

class E_SAVE_ROOM_DATA:
    _struct = ('<2i6i', -1)
    def __init___(self, t):
        (self.numPolys,
        self.numPortals,
        self.padd) = t

class E_PORTAL_DATA:
    numRooms: int
    room: list[E_ROOM_DATA]
    numTotal: int # of portals
    portals: list[E_PORTALS]

##define ARX_D3DVERTEX D3DTLVERTEX

#struct
#{
#    float x, y, z;
#int color;
#float tu, tv;
#} SMY_D3DVERTEX;

#struct
#{
#    float x, y, z;
#int color;
#float tu, tv;
#float tu2, tv2;
#float tu3, tv3;
#} SMY_D3DVERTEX3;

#struct
#{
#    float x, y, z;
#float rhw;
#int color;
#float tu, tv;
#float tu2, tv2;
#float tu3, tv3;
#} SMY_D3DVERTEX3_T;

#struct
#{
#    D3DTLVERTEX pD3DVertex[3];
#float uv[6];
#float color[3];
#} SMY_ZMAPPINFO;

#struct
#{
#    unsigned long uslStartVertex;
#unsigned long uslNbVertex;

#unsigned long uslStartCull;
#unsigned long uslNbIndiceCull;
#unsigned long uslStartNoCull;
#unsigned long uslNbIndiceNoCull;

#unsigned long uslStartCull_TNormalTrans;
#unsigned long uslNbIndiceCull_TNormalTrans;
#unsigned long uslStartNoCull_TNormalTrans;
#unsigned long uslNbIndiceNoCull_TNormalTrans;

#unsigned long uslStartCull_TMultiplicative;
#unsigned long uslNbIndiceCull_TMultiplicative;
#unsigned long uslStartNoCull_TMultiplicative;
#unsigned long uslNbIndiceNoCull_TMultiplicative;

#unsigned long uslStartCull_TAdditive;
#unsigned long uslNbIndiceCull_TAdditive;
#unsigned long uslStartNoCull_TAdditive;
#unsigned long uslNbIndiceNoCull_TAdditive;

#unsigned long uslStartCull_TSubstractive;
#unsigned long uslNbIndiceCull_TSubstractive;
#unsigned long uslStartNoCull_TSubstractive;
#unsigned long uslNbIndiceNoCull_TSubstractive;
#} SMY_ARXMAT;

#class CMY_DYNAMIC_VERTEXBUFFER
#{
#    public:
#		unsigned long uslFormat;
#    unsigned short ussMaxVertex;
#    unsigned short ussNbVertex;
#    unsigned short ussNbIndice;
#    LPDIRECT3DVERTEXBUFFER7 pVertexBuffer;
#    unsigned short* pussIndice;
#    public:
#		CMY_DYNAMIC_VERTEXBUFFER(unsigned short, unsigned long);
#    ~CMY_DYNAMIC_VERTEXBUFFER();

#    void* Lock(unsigned int);
#    bool UnLock();
#};

##define FVF_D3DVERTEX	(D3DFVF_XYZ|D3DFVF_DIFFUSE|D3DFVF_TEX1|D3DFVF_TEXTUREFORMAT2)
##define FVF_D3DVERTEX2	(D3DFVF_XYZ|D3DFVF_DIFFUSE|D3DFVF_TEX2|D3DFVF_TEXTUREFORMAT2)
##define FVF_D3DVERTEX3	(D3DFVF_XYZ|D3DFVF_DIFFUSE|D3DFVF_TEX3|D3DFVF_TEXTUREFORMAT2)

##define FVF_D3DVERTEX_T		(D3DFVF_XYZRHW|D3DFVF_DIFFUSE|D3DFVF_TEX1|D3DFVF_TEXTUREFORMAT2)
##define FVF_D3DVERTEX2_T	(D3DFVF_XYZRHW|D3DFVF_DIFFUSE|D3DFVF_TEX2|D3DFVF_TEXTUREFORMAT2)
##define FVF_D3DVERTEX3_T	(D3DFVF_XYZRHW|D3DFVF_DIFFUSE|D3DFVF_TEX3|D3DFVF_TEXTUREFORMAT2)

#extern long USE_PORTALS;
#extern EERIE_PORTAL_DATA* portals;