from dataclasses import dataclass, field
from numpy import ndarray, array, zeros
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
    s: Vector3          # Screen coordinates
    rhw: float          # Reciprocal of homogeneous w
    color: int          # Vertex color
    specular: int       # Specular component of vertex
    t: Vector2          # Texture coordinates
    def __init__(self, *args, **kwargs):
        if len(args) == 1:
            s = self.s = array([None]*3)
            t = self.t = array([None]*2)
            (s[0], s[1], s[2],
            self.rhw,
            self.color,
            self.specular,
            t[0], t[1]) = args[0]
        elif kwargs:
            self.s = kwargs.get('s')
            self.rhw = kwargs.get('rhw')
            self.color = kwargs.get('color')
            self.specular = kwargs.get('specular')
            self.t = kwargs.get('t')
        else: raise NotImplementedError('TLVERTEX')

class E_CYLINDER:
    _struct = ('<5f', 20)
    origin: Vector3
    radius: float
    height: float

class E_SPHERE:
    _struct = ('<4f', 16)
    origin: Vector3
    radius: float

@dataclass
class E_TEXTURE:
    id: int
    path: str
    poly: POLY

@dataclass
class E_POLY:
    type: POLY # at least 16 bits
    min: Vector3; max: Vector3
    norm: Vector3; norm2: Vector3
    v: list[TLVERTEX]; tv: list[TLVERTEX]
    nrml: list[Vector3]
    tex: E_TEXTURE
    center: Vector3
    transVal: float
    area: float
    room: int
    misc: int = 0
    #distBump: float
    #uslInd: list[int]

@dataclass
class E_VERTEX:
    vert: TLVERTEX
    v: Vector3
    norm: Vector3
    vworld: Vector3

@dataclass
class E_FACE:
    faceType: int # 0 = flat, 1 = text, 2 = Double-Side
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
    
##define MAX_PFACE 16
#@dataclass
#class E_PFACE:
#    #faceidx: list[short] #[MAX_PFACE]
#    #facetype: int
#    #texid: int #long
#    #nbvert: int
#    #transVal: float
#    #vid: list[int] #[MAX_PFACE]
#    #u: list[float] #[MAX_PFACE]
#    #v: list[float] #[MAX_PFACE]
#    #color: list[D3DCOLOR] #[MAX_PFACE]

##***********************************************************************
##*		BEGIN EERIE OBJECT STRUCTURES									*
##***********************************************************************
#class NEIGHBOURS_DATA:
#   numNvertex: int
#   numNfaces: int
#   nvertex: list[int]
#   nfaces: list[int]

class PROGRESSIVE_DATA:
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
    startidx: int
    endidx: int
    restlength: float
    constant: float # spring constant
    damping: float # spring damping
    type: int
    _struct = ('<2h3fi', 20)
    def __init__(self, t):
        (self.startidx,
        self.endidx,
        self.restlength,
        self.constant,
        self.damping,
        self.type) = t

##define CLOTHES_FLAG_NORMAL	0
##define CLOTHES_FLAG_FIX	1
##define CLOTHES_FLAG_NOCOL	2

class CLOTHESVERTEX:
    idx: int
    flags: int
    coll: int
    pos: Vector3
    velocity: Vector3
    force: Vector3
    mass: float # 1.f/mass
    t_pos: Vector3
    t_velocity: Vector3
    t_force: Vector3
    lastpos: Vector3
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

@dataclass
class CLOTHES_DATA:
    cvert: list[CLOTHESVERTEX]
    springs: list[E_SPRINGS]

class COLLISION_SPHERE:
    idx: int
    flags: int
    radius: float
    _struct = ('<2hf', 8)
    def __init__(self, t):
        (self.idx,
        self.flags,
        self.radius) = t

#class PHYSVERT:
#   initpos: Vector3
#   temp: Vector3
#   pos: Vector3
#   velocity: Vector3
#   force: Vector3
#   inertia: Vector3
#   mass: float

#class PHYSICS_BOX_DATA:
#   vert: list[PHYSVERT]
#   numPhysvert: int;
#   active: int
#   stopcount: int
#   radius: float #radius around vert[0].pos for spherical collision
#   storedtiming: float

#class EERIE_MAP:
#   sx: int
#   sy: int
#   bpp: int
#   bmpdata: bytes

@dataclass
class E_GROUPLIST:
    name: str
    origin: int
    numIndex: int
    indexes: list[int]
    size: float

@dataclass
class E_ACTIONLIST:
    name: str
    idx: int #index vertex;
    act: int #action
    sfx: int #sfx

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

@dataclass
class E_SELECTIONS:
    name: str
    numSelected: int
    selected: list[int]

##define DRAWFLAG_HIGHLIGHT	1

@dataclass
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
    _padd: int = 0

class E_BONE:
    numIdxVertices: int = 0; idxVertices: list[int] = []
    originalGroup: E_GROUPLIST = None
    father: int = 0
    quatAnim: quaternion = quaternion(); transAnim: Vector3 = array([0]*3); scaleAnim: Vector3 = array([0]*3)
    quatLast: quaternion = quaternion(); transLast: Vector3 = array([0]*3); scaleLast: Vector3 = array([0]*3)
    quatInit: quaternion = quaternion(); transInit: Vector3 = array([0]*3); scaleInit: Vector3 = array([0]*3)
    transInitGlobal: Vector3 = array([0]*3)
    def __init__(self): self.idxVertices = []
    def addIdxToBone(self, idx: int) -> None: self.idxVertices.append(idx); self.numIdxVertices += 1

##########################################
#struct
#{
#    float x;
#float y;
#float z;
#float w;
#} EERIE_3DPAD;

@dataclass
class E_3DOBJ:
    #name: str = 0
    file: str = 0
    #pos: Vector3 = field(default_factory=list)
    point0: Vector3 = field(default_factory=list)
    #angle: Vector3 = field(default_factory=list)
    origin: int = 0
    #ident: int = 0
    numVertex: int = 0
    #trueNumVertex: int = 0
    numFaces: int = 0
    numPfaces: int = 0
    numMaps: int = 0
    numGroups: int = 0
    numAction: int = 0
    numSelections: int = 0
    #drawFlags: int = 0
    #VertexLocal: EERIE_3DPAD = None
    vertexs: list[E_VERTEX] = None
    #vertexs3: list[E_VERTEX] = None

    faces: list[E_FACE] = None
    #pfaces: list[EERIE_PFACE] = None
    #maps: list[EERIE_MAP] = None
    groups: list[E_GROUPLIST] = None
    actions: list[E_ACTIONLIST] = None
    selections: list[E_SELECTIONS] = None
    textures: list[E_TEXTURE] = None

    #originalTextures: bytes = None
    #cub: CUB3D = None
    #quat: EERIE_QUAT = None
    #linked: EERIE_LINKED = None
    #numLinked: int = 0

    #pbox: PHYSICS_BOX_DATA = None
    #pdata: PROGRESSIVE_DATA = None
    #ndata: NEIGHBOURS_DATA = None
    cdata: CLOTHES_DATA = None
    spheres: list[COLLISION_SPHERE] = None
    fastAccess: E_FASTACCESS = None
    bones: list[E_BONE] = None

    def _centerObjectCoordinates(self) -> None:
        offset = self.vertexs[self.origin].v
        if offset[0] == 0 and offset[1] == 0 and offset[2] == 0: return
        # log.info(f'NOT CENTERED {self.file}\n')
        for i in range(self.numVertex): self.vertexs[i].v -= offset; self.vertexs[i].vert.s -= offset
        self.point0 -= offset
    def _createCedricData(self) -> None:
        def getFather(origin: int, startGroup: int) -> int:
            for i in range(startGroup, -1, -1):
                for j in range(self.groups[i].numIndex):
                    if self.groups[i].indexes[j] == origin: return i
            return -1

        if self.numGroups <= 0:
            self.bones = [E_BONE()]*1
            s = self.bones[0]
            for i in range(self.numVertex): s.addIdxToBone(i)
            s.transInitGlobal = s.transInit
            s.originalGroup = None
            s.father = -1
        else:
            self.bones = [E_BONE()]*self.numGroups
            temp = [False]*self.numVertex
            for i in range(self.numGroups - 1, -1, -1):
                s = self.bones[i]
                vorigin = self.vertexs[self.groups[i].origin]
                for j in range(self.groups[i].numIndex):
                    if not temp[self.groups[i].indexes[j]]: temp[self.groups[i].indexes[j]] = True; s.addIdxToBone(self.groups[i].indexes[j])
                s.transInit = vorigin.v.copy()
                s.transInitGlobal = s.transInit
                s.originalGroup = self.groups[i]
                s.father = getFather(self.groups[i].origin, i - 1)

            # Try to correct lonely vertex
            for i in range(self.numVertex):
                ok = False
                for j in range(self.numGroups):
                    for k in range(self.groups[j].numIndex):
                        if self.groups[j].indexes[k] == i: ok = True; break
                    if ok: break
                if not ok: self.bones[0].addIdxToBone(i)

            for i in range(self.numGroups - 1, -1, -1):
                s = self.bones[i]
                if s.father >= 0: s.transInit -= self.bones[s.father].transInit
                s.transInitGlobal = s.transInit

        # Build proper mesh
        for i in range(len(self.bones)):
            s = self.bones[i]
            if s.father >= 0:
                f = self.bones[s.father]
                s.quatAnim = f.quatAnim * s.quatInit # Rotation
                E_3DOBJ._transformVertexQuat(f.quatAnim, s.transInit, s.transAnim) # Translation
                s.transAnim = f.transAnim + s.transAnim
                s.scaleAnim = array([1.]*3) # Scale
            else:
                s.quatAnim = s.quatInit # Rotation
                s.transAnim = s.transInit # Translation
                s.scaleAnim = array([1.]*3) # Scale
        self.vertexLocal = [array([None]*4)]*self.numVertex
        for i in range(len(self.bones)):
            s = self.bones[i]
            vec = s.transAnim
            for v in range(s.numIdxVertices):
                t = self.vertexs[s.idxVertices[v]].v - vec
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
                if self.groups[i].name.casefold() == groupName: return i
            return -1
        def getActionPointIdx(text: str) -> None:
            text = text.casefold()
            for i in range(self.numAction):
                if self.actions[i].name.casefold() == text: return self.actions[i].idx
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
            mouthGroupOrigin = -1 if mouthGroup == -1 else self.groups[mouthGroup].origin,
            headGroup = (headGroup := getGroup('head')),
            headGroupOrigin = -1 if headGroup == -1 else self.groups[headGroup].origin,
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
        if q == quaternion(0., 0., 0.): t[0] = t[1] = t[2] = 0.; return
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

class SAVE_EPOLY:
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
    _v = '4f2I2f'
    _struct = (f'i12f{_v}{_v}{_v}{_v}{_v}{_v}{_v}{_v}12fi5f2h', 384)
    def __init__(self, t):
        min = self.min = array([None]*3); max = self.max = array([None]*3)
        norm = self.norm = array([None]*3); norm2 = self.norm2 = array([None]*3)
        (self.type,
        min[0], min[1], min[2], max[0], max[1], max[2],
        norm[0], norm[1], norm[2], norm2[0], norm2[1], norm2[2]) = t[:13]
        self.v0 = TLVERTEX(t[13:21]); self.v1 = TLVERTEX(t[21:29]); self.v2 = TLVERTEX(t[29:37]); self.v3 = TLVERTEX(t[37:45])
        self.tv0 = TLVERTEX(t[45:53]); self.tv1 = TLVERTEX(t[53:61]); self.tv2 = TLVERTEX(t[61:69]); self.tv3 = TLVERTEX(t[69:77])
        nrml0 = self.nrml0 = array([None]*3); nrml1 = self.nrml1 = array([None]*3); nrml2 = self.nrml2 = array([None]*3); nrml3 = self.nrml3 = array([None]*3)
        center = self.center = array([None]*3)
        (nrml0[0], nrml0[1], nrml0[2], nrml1[0], nrml1[1], nrml1[2], nrml2[0], nrml2[1], nrml2[2], nrml3[0], nrml3[1], nrml3[2],
        self.texPtr,
        center[0], center[1], center[2],
        self.transVal,
        self.area,
        self.room,
        self.misc) = t[77:]
    def to(s) -> E_POLY:
        return E_POLY(
            area = s.area,
            type = s.type,
            transVal = s.transVal,
            room = s.room,
            misc = s.misc,
            center = s.center,
            max = s.max,
            min = s.min,
            norm = s.norm,
            norm2 = s.norm2,
            nrml = [s.nrml0, s.nrml1, s.nrml2, s.nrml3],
            v = [s.v0, s.v1, s.v2, s.v3],
            tv = [s.tv0, s.tv1, s.tv2, s.tv3],
            tex = None)

class E_SAVE_PORTALS:
    poly: SAVE_EPOLY
    room1: int # facing normal
    room2: int
    usePortal: int
    paddy: int
    _struct = (f'<{SAVE_EPOLY._struct[0]}2i2h', 384 + 12)
    def __init__(self, t):
        self.poly = SAVE_EPOLY(t[:97])
        (self.room1,
        self.room2,
        self.usePortal,
        self.paddy) = t[97:]
    def to(s) -> 'E_PORTALS':
        return E_PORTALS(
            poly = SAVE_EPOLY.to(s.poly),
            room1 = s.room1,
            room2 = s.room2,
            usePortal = s.usePortal,
            paddy = s.paddy)

@dataclass
class E_PORTALS:
    poly: E_POLY
    room1: int # facing normal
    room2: int
    usePortal: int
    paddy: int

class EP_DATA:
    _struct = ('<4h', 8)
    def __init__(self, t):
        (self.px,
        self.py,
        self.idx,
        self.padd) = t

@dataclass
class E_ROOM_DATA:
    numPortals: int
    portals: list[int]
    numPolys: int
    epData: list[EP_DATA]
    center: Vector3 = None
    radius: float = 0.
    pussIndice: list[int] = None
    #vertexBuffer: LPDIRECT3DVERTEXBUFFER7
    numTextures: int = 0
    textureContainer: E_TEXTURE = None

class E_SAVE_ROOM_DATA:
    _struct = ('<8i', 32)
    def __init__(self, t):
        padd = self.padd = [0]*6
        (self.numPolys,
        self.numPortals,
        padd[0], padd[1], padd[2], padd[3], padd[4], padd[5]) = t

@dataclass
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