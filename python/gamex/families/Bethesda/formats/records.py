import os
from io import BytesIO
from enum import Enum, Flag, IntEnum, IntFlag
from openstk import log 
from gamex import FileSource, BinaryReader, ArcBinaryT, IRecord
from gamex.families.Uncore.formats.compression import decompressLz4, decompressZlib

class LTEXRecord: pass

#region Enums

class FormType(Enum):
    APPA = 0x41505041
    ARMA = 0x414D5241
    AACT = 0x54434141
    ASPC = 0x43505341
    ACTI = 0x49544341
    ARMO = 0x4F4D5241
    AMMO = 0x4F4D4D41
    ASTP = 0x50545341
    ARTO = 0x4F545241
    AMDL = 0x4C444D41
    AECH = 0x48434541
    ACRE = 0x45524341
    AORU = 0x55524F41
    ALCH = 0x48434C41
    ACHR = 0x52484341
    ANIO = 0x4F494E41
    AVIF = 0x46495641
    ADDN = 0x4E444441
    BOOK = 0x4B4F4F42
    BSGN = 0x4E475342
    BODY = 0x59444F42
    BNDS = 0x53444E42
    BPTD = 0x44545042
    CMPO = 0x4F504D43
    CLAS = 0x53414C43
    CSTY = 0x59545343
    CONT = 0x544E4F43
    CLMT = 0x544D4C43
    CELL = 0x4C4C4543
    CAMS = 0x534D4143
    CPTH = 0x48545043
    CREA = 0x41455243
    CLFM = 0x4D464C43
    COLL = 0x4C4C4F43
    CLOT = 0x544F4C43
    COBJ = 0x4A424F43
    DMGT = 0x54474D44
    DOOR = 0x524F4F44
    DOBJ = 0x4A424F44
    DFOB = 0x424F4644
    DUAL = 0x4C415544
    DLBR = 0x52424C44
    DEBR = 0x52424544
    DLVW = 0x57564C44
    DIAL = 0x4C414944
    EQUP = 0x50555145
    EYES = 0x53455945
    EFSH = 0x48534645
    ECZN = 0x4E5A4345
    EXPL = 0x4C505845
    ENCH = 0x48434E45
    FACT = 0x54434146
    FURN = 0x4E525546
    FLOR = 0x524F4C46
    FSTP = 0x50545346
    FSTS = 0x53545346
    FLST = 0x54534C46
    GRUP = 0x50555247
    GMST = 0x54534D47
    GLOB = 0x424F4C47
    GRAS = 0x53415247
    GDRY = 0x59524447
    HAZD = 0x445A4148
    HDPT = 0x54504448
    HAIR = 0x52494148
    INGR = 0x52474E49
    IDLM = 0x4D4C4449
    INFO = 0x4F464E49
    IDLE = 0x454C4449
    IPCT = 0x54435049
    IPDS = 0x53445049
    IMGS = 0x53474D49
    IMAD = 0x44414D49
    INNR = 0x524E4E49
    KYWD = 0x4457594B
    KEYM = 0x4D59454B
    KSSM = 0x4D53534B
    LIGH = 0x4847494C
    LCTN = 0x4E54434C
    LCRT = 0x5452434C
    LTEX = 0x5845544C
    LVLN = 0x4E4C564C
    LVLI = 0x494C564C
    LAND = 0x444E414C
    LSCR = 0x5243534C
    LVSP = 0x5053564C
    LGTM = 0x4D54474C
    LVLC = 0x434C564C
    LEVC = 0x4356454C
    LOCK = 0x4B434F4C
    LENS = 0x534E454C
    LSPR = 0x5250534C
    LEVI = 0x4956454C
    LAYR = 0x5259414C
    MATT = 0x5454414D
    MSTT = 0x5454534D
    MGEF = 0x4645474D
    MICN = 0x4E43494D
    MISC = 0x4353494D
    MESG = 0x4753454D
    MUSC = 0x4353554D
    MUST = 0x5453554D
    MOVT = 0x54564F4D
    MATO = 0x4F54414D
    MSWP = 0x5057534D
    NONE = 0x454E4F4E
    NPC_ = 0x5F43504E
    NOTE = 0x45544F4E
    NAVI = 0x4956414E
    NAVM = 0x4D56414E
    NOCM = 0x4D434F4E
    OTFT = 0x5446544F
    OMOD = 0x444F4D4F
    OVIS = 0x5349564F
    PROJ = 0x4A4F5250
    PMIS = 0x53494D50
    PARW = 0x57524150
    PGRE = 0x45524750
    PBEA = 0x41454250
    PFLA = 0x414C4650
    PCON = 0x4E4F4350
    PBAR = 0x52414250
    PHZD = 0x445A4850
    PACK = 0x4B434150
    PERK = 0x4B524550
    PKIN = 0x4E494B50
    PROB = 0x424F5250
    PGRD = 0x44524750
    QUST = 0x54535551
    RFCT = 0x54434652
    REGN = 0x4E474552
    RACE = 0x45434152
    REFR = 0x52464552
    RGDL = 0x4C444752
    REVB = 0x42564552
    ROAD = 0x44414F52
    REPA = 0x41504552
    RFGP = 0x50474652
    RELA = 0x414C4552
    SPGD = 0x44475053
    SLGM = 0x4D474C53
    STAT = 0x54415453
    SCOL = 0x4C4F4353
    SOUN = 0x4E554F53
    SKIL = 0x4C494B53
    SCPT = 0x54504353
    SPEL = 0x4C455053
    SCRL = 0x4C524353
    SMBN = 0x4E424D53
    SMQN = 0x4E514D53
    SMEN = 0x4E454D53
    SHOU = 0x554F4853
    SBSP = 0x50534253
    SNDR = 0x52444E53
    SNCT = 0x54434E53
    SOPM = 0x4D504F53
    SCCO = 0x4F434353
    SCSN = 0x4E534353
    STAG = 0x47415453
    SGST = 0x54534753
    SSCR = 0x52435353
    SCEN = 0x4E454353
    SNDG = 0x47444E53
    TOFT = 0x54464F54
    TERM = 0x4D524554
    TREE = 0x45455254
    TLOD = 0x444F4C54
    TES3 = 0x33534554
    TES4 = 0x34534554
    TES5 = 0x35534554
    TES6 = 0x36534554
    TRNS = 0x534E5254
    TXST = 0x54535854
    TACT = 0x54434154
    VTYP = 0x50595456
    WRLD = 0x444C5257
    WEAP = 0x50414557
    WTHR = 0x52485457
    WATR = 0x52544157
    WOOP = 0x504F4F57
    ZOOM = 0x4D4F4F5A

    # @classmethod
    # def _missing_(cls, value):
    #     s = object.__new__(cls); s._value_ = value; s._name_ = f'_{hex(value)}'
    #     print(f'_missing_: {s}')
    #     return s

class FieldType(Enum):
    ANAM = 0x4D414E41
    AVFX = 0x58465641
    ASND = 0x444E5341
    AADT = 0x54444141
    AODT = 0x54444F41
    ALDT = 0x54444C41
    AMBI = 0x49424D41
    ATXT = 0x54585441
    ATTR = 0x52545441
    AIDT = 0x54444941
    AI_W = 0x575F4941
    AI_T = 0x545F4941
    AI_F = 0x465F4941
    AI_E = 0x455F4941
    AI_A = 0x415F4941
    BTXT = 0x54585442
    BVFX = 0x58465642
    BSND = 0x444E5342
    BMDT = 0x54444D42
    BKDT = 0x54444B42
    BNAM = 0x4D414E42
    BYDT = 0x54445942
    CSTD = 0x44545343
    CSAD = 0x44415343
    CNAM = 0x4D414E43
    CTDA = 0x41445443
    CTDT = 0x54445443
    CVFX = 0x58465643
    CSND = 0x444E5343
    CNDT = 0x54444E43
    CLDT = 0x54444C43
    CNTO = 0x4F544E43
    DATA = 0x41544144
    DNAM = 0x4D414E44
    DESC = 0x43534544
    DELE = 0x454C4544
    DODT = 0x54444F44
    ENDT = 0x54444E45
    ESCE = 0x45435345
    ENIT = 0x54494E45
    EFID = 0x44494645
    EFIT = 0x54494645
    EDID = 0x44494445
    ENAM = 0x4D414E45
    FADT = 0x54444146
    FGGS = 0x53474746
    FGGA = 0x41474746
    FGTS = 0x53544746
    FRMR = 0x524D5246
    FLAG = 0x47414C46
    FLTV = 0x56544C46
    FULL = 0x4C4C5546
    FNAM = 0x4D414E46
    GNAM = 0x4D414E47
    HEDR = 0x52444548
    HSND = 0x444E5348
    HVFX = 0x58465648
    HNAM = 0x4D414E48
    ICON = 0x4E4F4349
    ICO2 = 0x324F4349
    INDX = 0x58444E49
    INTV = 0x56544E49
    INCC = 0x43434E49
    INAM = 0x4D414E49
    ITEX = 0x58455449
    IRDT = 0x54445249
    JNAM = 0x4D414E4A
    KNAM = 0x4D414E4B
    LVLD = 0x444C564C
    LVLF = 0x464C564C
    LVLO = 0x4F4C564C
    LNAM = 0x4D414E4C
    LKDT = 0x54444B4C
    LHDT = 0x5444484C
    MODL = 0x4C444F4D
    MODB = 0x42444F4D
    MODT = 0x54444F4D
    MNAM = 0x4D414E4D
    MAST = 0x5453414D
    MEDT = 0x5444454D
    MOD2 = 0x32444F4D
    MO2B = 0x42324F4D
    MO2T = 0x54324F4D
    MOD3 = 0x33444F4D
    MO3B = 0x42334F4D
    MO3T = 0x54334F4D
    MOD4 = 0x34444F4D
    MO4B = 0x42344F4D
    MO4T = 0x54344F4D
    MCDT = 0x5444434D
    NPCS = 0x5343504E
    NAM1 = 0x314D414E
    NAME = 0x454D414E
    NAM2 = 0x324D414E
    NAM0 = 0x304D414E
    NAM9 = 0x394D414E
    NNAM = 0x4D414E4E
    NAM5 = 0x354D414E
    NPCO = 0x4F43504E
    NPDT = 0x5444504E
    OFST = 0x5453464F
    ONAM = 0x4D414E4F
    PGRP = 0x50524750
    PGRR = 0x52524750
    PFIG = 0x47494650
    PFPC = 0x43504650
    PKDT = 0x54444B50
    PLDT = 0x54444C50
    PSDT = 0x54445350
    PTDT = 0x54445450
    PBDT = 0x54444250
    PTEX = 0x58455450
    PGRC = 0x43524750
    PGAG = 0x47414750
    PGRL = 0x4C524750
    PGRI = 0x49524750
    PNAM = 0x4D414E50
    QSDT = 0x54445351
    QSTA = 0x41545351
    QSTI = 0x49545351
    QSTR = 0x52545351
    QSTN = 0x4E545351
    QSTF = 0x46545351
    QNAM = 0x4D414E51
    RIDT = 0x54444952
    RNAM = 0x4D414E52
    RCLR = 0x524C4352
    RPLI = 0x494C5052
    RPLD = 0x444C5052
    RDAT = 0x54414452
    RDOT = 0x544F4452
    RDMP = 0x504D4452
    RDGS = 0x53474452
    RDMD = 0x444D4452
    RDSD = 0x44534452
    RDWT = 0x54574452
    RADT = 0x54444152
    RGNN = 0x4E4E4752
    SCIT = 0x54494353
    SCRI = 0x49524353
    SCHR = 0x52484353
    SCDA = 0x41444353
    SCTX = 0x58544353
    SCRO = 0x4F524353
    SOUL = 0x4C554F53
    SLCP = 0x50434C53
    SNAM = 0x4D414E53
    SPLO = 0x4F4C5053
    SCHD = 0x44484353
    SCVR = 0x52564353
    SCDT = 0x54444353
    SLSD = 0x44534C53
    SCRV = 0x56524353
    SCPT = 0x54504353
    STRV = 0x56525453
    SKDT = 0x54444B53
    SNDX = 0x58444E53
    SNDD = 0x44444E53
    SPIT = 0x54495053
    SPDT = 0x54445053
    TNAM = 0x4D414E54
    TPIC = 0x43495054
    TRDT = 0x54445254
    TCLT = 0x544C4354
    TCLF = 0x464C4354
    TEXT = 0x54584554
    UNAM = 0x4D414E55
    VNAM = 0x4D414E56
    VTXT = 0x54585456
    VNML = 0x4C4D4E56
    VHGT = 0x54474856
    VCLR = 0x524C4356
    VTEX = 0x58455456
    WLST = 0x54534C57
    WNAM = 0x4D414E57
    WHGT = 0x54474857
    WPDT = 0x54445057
    WEAT = 0x54414557
    XTEL = 0x4C455458
    XLOC = 0x434F4C58
    XTRG = 0x47525458
    XSED = 0x44455358
    XCHG = 0x47484358
    XHLT = 0x544C4858
    XLCM = 0x4D434C58
    XRTM = 0x4D545258
    XACT = 0x54434158
    XCNT = 0x544E4358
    XMRK = 0x4B524D58
    XXXX = 0x58585858
    XOWN = 0x4E574F58
    XRNK = 0x4B4E5258
    XGLB = 0x424C4758
    XESP = 0x50534558
    XSCL = 0x4C435358
    XRGD = 0x44475258
    XPCI = 0x49435058
    XLOD = 0x444F4C58
    XMRC = 0x43524D58
    XHRS = 0x53524858
    XSOL = 0x4C4F5358
    XCLC = 0x434C4358
    XCLL = 0x4C4C4358
    XCLW = 0x574C4358
    XCLR = 0x524C4358
    XCMT = 0x544D4358
    XCCM = 0x4D434358
    XCWT = 0x54574358
    XNAM = 0x4D414E58

    @classmethod
    def _missing_(cls, value):
        s = object.__new__(cls); s._value_ = value; s._name_ = f'_{hex(value)}'
        print(f'_missing_: {s}')
        return s

#endregion

#region Header

# IHaveMODL
class IHaveMODL:
    # MODL: MODLGroup
    pass

# Header
class Header(BinaryReader):
    # EsmFlags
    class EsmFlags(Flag):
        EsmFile = 0x00000001                # ESM file. (TES4.HEDR record only.)
        Deleted = 0x00000020                # Deleted
        R00 = 0x00000040                    # Constant / (REFR) Hidden From Local Map (Needs Confirmation: Related to shields)
        R01 = 0x00000100                    # Must Update Anims / (REFR) Inaccessible
        R02 = 0x00000200                    # (REFR) Hidden from local map / (ACHR) Starts dead / (REFR) MotionBlurCastsShadows
        R03 = 0x00000400                    # Quest item / Persistent reference / (LSCR) Displays in Main Menu
        InitiallyDisabled = 0x00000800      # Initially disabled
        Ignored = 0x00001000                # Ignored
        VisibleWhenDistant = 0x00008000     # Visible when distant
        R04 = 0x00010000                    # (ACTI) Random Animation Start
        R05 = 0x00020000                    # (ACTI) Dangerous / Off limits (Interior cell) Dangerous Can't be set withough Ignore Object Interaction
        Compressed = 0x00040000             # Data is compressed
        CantWait = 0x00080000               # Can't wait
        # tes5
        R06 = 0x00100000                    # (ACTI) Ignore Object Interaction Ignore Object Interaction Sets Dangerous Automatically
        IsMarker = 0x00800000               # Is Marker
        R07 = 0x02000000                    # (ACTI) Obstacle / (REFR) No AI Acquire
        NavMesh01 = 0x04000000              # NavMesh Gen - Filter
        NavMesh02 = 0x08000000              # NavMesh Gen - Bounding Box
        R08 = 0x10000000                    # (FURN) Must Exit to Talk / (REFR) Reflected By Auto Water
        R09 = 0x20000000                    # (FURN/IDLM) Child Can Use / (REFR) Don't Havok Settle
        R10 = 0x40000000                    # NavMesh Gen - Ground / (REFR) NoRespawn
        R11 = 0x80000000                    # (REFR) MultiBound

    def __repr__(self) -> str: return f'{self.type}:{self.groupType}'
    binPath: str
    format: FormType
    parent: 'Header'
    type: FormType
    dataSize: int 
    flags: EsmFlags 
    @property
    def compressed(self) -> bool: return Header.EsmFlags.Compressed in self.flags
    id: int = 0
    group: 'GroupHeader' = None
    position: int

    def __init__(self, b: BinaryReader, binPath: str, format: FormType, parent: 'Header' = None):
        super().__init__(b.f); r = self
        self.binPath = binPath
        self.format = format
        self.parent = parent
        self.type = FormType(r.readUInt32())
        if type == FormType.GRUP:
            self.dataSize = (b.readUInt32() - (20 if format == FormType.TES4 else 24))
            self.group = GroupHeader(
                header = self,
                label = FormType(b.readUInt32()),
                type = GroupHeader.GroupType(r.readInt32()),
                dataSize = self.dataSize)
            r.readUInt32() # stamp | stamp + unknown
            if format != FormType.TES4: r.readUInt32() # version + unknown
            self.position = self.group.position = r.tell()
            return
        self.dataSize = r.readUInt32()
        if format == FormType.TES3: r.readUInt32() # unknown
        while True:
            self.flags = Header.EsmFlags(r.readUInt32())
            if format == FormType.TES3: break
            self.id = r.readUInt32()
            r.readUInt32()
            if format == FormType.TES4: break
            r.readUInt32()
            if format == FormType.TES5: break
        self.position = r.tell()

# GroupHeader
class GroupHeader:
    # GroupType
    class GroupType(Enum):
        Top = 0,                         # Label: Record type
        WorldChildren = 1,               # Label: Parent (WRLD)
        InteriorCellBlock = 2,           # Label: Block number
        InteriorCellSubBlock = 3,        # Label: Sub-block number
        ExteriorCellBlock = 4,           # Label: Grid Y, X (Note the reverse order)
        ExteriorCellSubBlock = 5,        # Label: Grid Y, X (Note the reverse order)
        CellChildren = 6,                # Label: Parent (CELL)
        TopicChildren = 7,               # Label: Parent (DIAL)
        CellPersistentChilden = 8,       # Label: Parent (CELL)
        CellTemporaryChildren = 9,       # Label: Parent (CELL)
        CellVisibleDistantChildren = 10  # Label: Parent (CELL)

    def __repr__(self) -> str: return f'{self.label}'
    def __init__(self, header: Header, label: FormType, type: GroupType = 0, dataSize: int = 0, position: int = 0):
        self.header: Header = header
        self.label: FormType = label
        self.type: GroupType = type
        self.dataSize: int = dataSize
        self.position: int = position

# FieldHeader
class FieldHeader:
    def __repr__(self) -> str: return f'{self.type}'
    def __init__(self, r: Header):
        self.type: FieldType = FieldType(r.readUInt32())
        self.dataSize: int = r.readUInt32() if r.format == FormType.TES3 else r.readUInt16()

#endregion

#region Fields

class STRVField:
    def __repr__(self) -> str: return f'{self.value}'
    value: str
class FILEField:
    def __repr__(self) -> str: return f'{self.value}'
    value: str
class DATVField:
    def __repr__(self) -> str: return f'DATV'
    b: bool; i: int; f: float; s: str
class FLTVField:
    _struct = ('<f', 4)
    def __repr__(self) -> str: return f'{self.value}'
    def __init__(self, tuple): self.value = tuple
class BYTEField: 
    _struct = ('<c', 1)
    def __repr__(self) -> str: return f'{self.value}'
    def __init__(self, tuple): self.value = tuple
class IN16Field: 
    _struct = ('<h', 2)
    def __repr__(self) -> str: return f'{self.value}'
    def __init__(self, tuple): self.value = tuple
class UI16Field: 
    def __repr__(self) -> str: return f'{self.value}'
    _struct = ('<H', 2)
    def __init__(self, tuple): self.value = tuple
class IN32Field: 
    _struct = ('<i', 4)
    def __repr__(self) -> str: return f'{self.value}'
    def __init__(self, tuple): self.value = tuple
class UI32Field: 
    _struct = ('<I', 4)
    def __repr__(self) -> str: return f'{self.value}'
    def __init__(self, tuple): self.value = tuple
class INTVField:
    def __repr__(self) -> str: return f'{self.value}'
    _struct = ('<q', 8)
    def __init__(self, tuple): self.value = tuple
    def asUI16Field(self) -> UI16Field: return UI16Field(self.value)
class CREFField:
    _struct = ('<4c', 4)
    def __repr__(self) -> str: return f'{self.color}'
    def __init__(self, tuple): self.color = ByteColor4(tuple)
class CNTOField:
    def __repr__(self) -> str: return f'{self.item}'
    itemCount: int # Number of the item
    item: 'Ref[Record]' # The ID of the item
    def __init__(self, r: Header, dataSize: int):
        if r.format == FormType.TES3: self.itemCount = r.readUInt32(); self.item = Ref(r.readFAString(32)); return
        self.item = Ref(r.readUInt32()); self.itemCount = r.readUInt32()
class BYTVField:
    def __repr__(self) -> str: return f'BYTS'
    value: bytes
class UNKNField: 
    def __repr__(self) -> str: return f'UNKN'
    value: bytes
class MODLGroup:
    def __repr__(self) -> str: return f'{self.value}'
    def __init__(self, r: Header, dataSize: int): self.value: str = r.readFUString(dataSize)
    bound: float
    textures: bytes # Texture Files Hashes
    def MODBField(self, r: Header, dataSize: int) -> object: return setattr(self, bound, r.readSingle())
    def MODTField(self, r: Header, dataSize: int) -> object: return setattr(self, textures, r.readBytes(dataSize))

#endregion

#region Record

class Record(IRecord):
    def __repr__(self) -> str: return f'{self.__name__}:{self.EDID.value}'
    mapx: dict[FormType, (callable, callable)] = {
        FormType.TES3: (lambda: TES3Record(), lambda x: True),
        FormType.TES4: (lambda: TES4Record(), lambda x: True),
        # 0                                 
        FormType.LTEX: (lambda: LTEXRecord(), lambda x: x > 0),
        FormType.STAT: (lambda: STATRecord(), lambda x: x > 0),
        FormType.CELL: (lambda: CELLRecord(), lambda x: x > 0),
        FormType.LAND: (lambda: LANDRecord(), lambda x: x > 0),
        # 1                                 
        FormType.DOOR: (lambda: DOORRecord(), lambda x: x > 1),
        FormType.MISC: (lambda: MISCRecord(), lambda x: x > 1),
        FormType.WEAP: (lambda: WEAPRecord(), lambda x: x > 1),
        FormType.CONT: (lambda: CONTRecord(), lambda x: x > 1),
        FormType.LIGH: (lambda: LIGHRecord(), lambda x: x > 1),
        FormType.ARMO: (lambda: ARMORecord(), lambda x: x > 1),
        FormType.CLOT: (lambda: CLOTRecord(), lambda x: x > 1),
        FormType.REPA: (lambda: REPARecord(), lambda x: x > 1),
        FormType.ACTI: (lambda: ACTIRecord(), lambda x: x > 1),
        FormType.APPA: (lambda: APPARecord(), lambda x: x > 1),
        FormType.LOCK: (lambda: LOCKRecord(), lambda x: x > 1),
        FormType.PROB: (lambda: PROBRecord(), lambda x: x > 1),
        FormType.INGR: (lambda: INGRRecord(), lambda x: x > 1),
        FormType.BOOK: (lambda: BOOKRecord(), lambda x: x > 1),
        FormType.ALCH: (lambda: ALCHRecord(), lambda x: x > 1),
        FormType.CREA: (lambda: CREARecord(), lambda x: x > 1 and True),
        FormType.NPC_: (lambda: NPC_Record(), lambda x: x > 1 and True),
        # 2                                 
        FormType.GMST: (lambda: GMSTRecord(), lambda x: x > 2),
        FormType.GLOB: (lambda: GLOBRecord(), lambda x: x > 2),
        FormType.SOUN: (lambda: SOUNRecord(), lambda x: x > 2),
        FormType.REGN: (lambda: REGNRecord(), lambda x: x > 2),
        # 3                                 
        FormType.CLAS: (lambda: CLASRecord(), lambda x: x > 3),
        FormType.SPEL: (lambda: SPELRecord(), lambda x: x > 3),
        FormType.BODY: (lambda: BODYRecord(), lambda x: x > 3),
        FormType.PGRD: (lambda: PGRDRecord(), lambda x: x > 3),
        FormType.INFO: (lambda: INFORecord(), lambda x: x > 3),
        FormType.DIAL: (lambda: DIALRecord(), lambda x: x > 3),
        FormType.SNDG: (lambda: SNDGRecord(), lambda x: x > 3),
        FormType.ENCH: (lambda: ENCHRecord(), lambda x: x > 3),
        FormType.SCPT: (lambda: SCPTRecord(), lambda x: x > 3),
        FormType.SKIL: (lambda: SKILRecord(), lambda x: x > 3),
        FormType.RACE: (lambda: RACERecord(), lambda x: x > 3),
        FormType.MGEF: (lambda: MGEFRecord(), lambda x: x > 3),
        FormType.LEVI: (lambda: LEVIRecord(), lambda x: x > 3),
        FormType.LEVC: (lambda: LEVCRecord(), lambda x: x > 3),
        FormType.BSGN: (lambda: BSGNRecord(), lambda x: x > 3),
        FormType.FACT: (lambda: FACTRecord(), lambda x: x > 3),
        FormType.SSCR: (lambda: SSCRRecord(), lambda x: x > 3),
        # 4 - Oblivion                      
        FormType.WRLD: (lambda: WRLDRecord(), lambda x: x > 0),
        FormType.ACRE: (lambda: ACRERecord(), lambda x: x > 1),
        FormType.ACHR: (lambda: ACHRRecord(), lambda x: x > 1),
        FormType.REFR: (lambda: REFRRecord(), lambda x: x > 1),
        #                                   
        FormType.AMMO: (lambda: AMMORecord(), lambda x: x > 4),
        FormType.ANIO: (lambda: ANIORecord(), lambda x: x > 4),
        FormType.CLMT: (lambda: CLMTRecord(), lambda x: x > 4),
        FormType.CSTY: (lambda: CSTYRecord(), lambda x: x > 4),
        FormType.EFSH: (lambda: EFSHRecord(), lambda x: x > 4),
        FormType.EYES: (lambda: EYESRecord(), lambda x: x > 4),
        FormType.FLOR: (lambda: FLORRecord(), lambda x: x > 4),
        FormType.FURN: (lambda: FURNRecord(), lambda x: x > 4),
        FormType.GRAS: (lambda: GRASRecord(), lambda x: x > 4),
        FormType.HAIR: (lambda: HAIRRecord(), lambda x: x > 4),
        FormType.IDLE: (lambda: IDLERecord(), lambda x: x > 4),
        FormType.KEYM: (lambda: KEYMRecord(), lambda x: x > 4),
        FormType.LSCR: (lambda: LSCRRecord(), lambda x: x > 4),
        FormType.LVLC: (lambda: LVLCRecord(), lambda x: x > 4),
        FormType.LVLI: (lambda: LVLIRecord(), lambda x: x > 4),
        FormType.LVSP: (lambda: LVSPRecord(), lambda x: x > 4),
        FormType.PACK: (lambda: PACKRecord(), lambda x: x > 4),
        FormType.QUST: (lambda: QUSTRecord(), lambda x: x > 4),
        FormType.ROAD: (lambda: ROADRecord(), lambda x: x > 4),
        FormType.SBSP: (lambda: SBSPRecord(), lambda x: x > 4),
        FormType.SGST: (lambda: SGSTRecord(), lambda x: x > 4),
        FormType.SLGM: (lambda: SLGMRecord(), lambda x: x > 4),
        FormType.TREE: (lambda: TREERecord(), lambda x: x > 4),
        FormType.WATR: (lambda: WATRRecord(), lambda x: x > 4),
        FormType.WTHR: (lambda: WTHRRecord(), lambda x: x > 4),
        # 5 - Skyrim                        
        FormType.AACT: (lambda: AACTRecord(), lambda x: x > 5),
        FormType.ADDN: (lambda: ADDNRecord(), lambda x: x > 5),
        FormType.ARMA: (lambda: ARMARecord(), lambda x: x > 5),
        FormType.ARTO: (lambda: ARTORecord(), lambda x: x > 5),
        FormType.ASPC: (lambda: ASPCRecord(), lambda x: x > 5),
        FormType.ASTP: (lambda: ASTPRecord(), lambda x: x > 5),
        FormType.AVIF: (lambda: AVIFRecord(), lambda x: x > 5),
        FormType.DLBR: (lambda: DLBRRecord(), lambda x: x > 5),
        FormType.DLVW: (lambda: DLVWRecord(), lambda x: x > 5),
        FormType.SNDR: (lambda: SNDRRecord(), lambda x: x > 5)
    }
    empty: 'Record'
    header: Header
    @property
    def id(self) -> int: return self.header.formId
    EDID: STRVField # Editor ID

    # Return an uninitialized subrecord to deserialize, or null to skip.
    def createField(self, r: Header, type: FieldType, dataSize: int) -> object: return Record.empty

    def read(self, r: Header) -> None:
        start = r.tell(); end = start + self.header.dataSize
        while not r.atEnd(end):
            field = FieldHeader(r)
            if field.type == FieldType.XXXX:
                if field.dataSize != 4: raise Exception()
                field.dataSize = r.readUInt32()
                continue
            elif field.type == FieldType.OFST and self.header.Type == FormType.WRLD: r.seek(end); continue
            tell = r.tell()
            if self.createField(r, field.type, field.dataSize) == Record.empty: print(f'Unsupported ESM record type: {self.header.type}:{field.type}'); r.skip(field.dataSize); continue
            r.ensureAtEnd(tell + field.dataSize, f'Failed reading {self.header.type}:{field.type} field data at offset {tell} in {r.binPath} of {r.tell() - tell - field.dataSize}')
        r.ensureAtEnd(end, f'Failed reading {self.header.type} record data at offset {start} in {r.binPath}')

    @staticmethod
    def factory(r: Header, level: int) -> 'Record':
        if not (z := Record.mapx.get(r.type)): print(f'Unsupported ESM record type: {r.type}'); return None
        if not z[1](level): return None
        record = z[0]()
        record.header = r
        return record
Record.empty = Record()

class RefId[T: Record]:
    _struct = ('<I', 4)
    def __repr__(self) -> str: return f'{self.type}:{self.id}'
    def __init__(self, tuple): self.id = tuple
    @property
    def type(self) -> str: return 'TBD'

class Ref[T: Record]:
    def __repr__(self) -> str: return f'{self.type}:{self.name}{self.id}'
    def __init__(self, *args): self.id = None; self.name = None
    def setName(self, name: str) -> 'Ref': return Ref(self.id, name)
    @property
    def type(self) -> str: return 'TBD'

class RefField[T: Record]:
    def __repr__(self) -> str: return f'{self.value}'
    def __init__(self, r: Header, dataSize: int): self.value = Ref(r.readUInt32()) if dataSize == 4 else Ref(r.readFAString(dataSize))
    def setName(self, name: str) -> Ref: self.value = self.value.setName(name); return self.value

class Ref2Field[T: Record]:
    def __repr__(self) -> str: return f'{self.value1}x{self.value2}'
    def __init__(self, r: Header, dataSize: int): self.value1 = Ref(r.readUInt32()); self.value2 = Ref(r.readUInt32())

#endregion

#region Record Group

class RecordGroup:
    cellsLoaded: int = 0
    @property
    def label(self) -> str: return next(self.headers, None).value.label
    def __repr__(self) -> str: return f'{next(self.headers, None).value}'
    def __init__(self, level: int):
        self.headers: list[GroupHeader] = []
        self.records: list[Record] = []
        self.groups: list[RecordGroup] = None
        self.groupsByLabel: dict[int, list[RecordGroup]] = None
        self.level: int = level
        self.skip: int = 0

    def addHeader(self, h: GroupHeader, load: bool = True) -> None:
        self.headers.append(h)
        if load and h.label != 0 and h.groupType == GroupHeader.GroupType.Top:
            match h.label:
                case FormType.CELL | FormType.WRLD: self.load() # or FormType.DIAL

    def load(self, loadAll: bool = False) -> list[Record]:
        if self.skip == len(self.headers): return self.records
        for h in self.headers[self.skip:]: self.readGroup(h, loadAll)
        self.skip = len(self.headers)
        return self.records

    def readGroup(self, h: GroupHeader, loadAll: bool) -> None:
        r = h.header
        r.seek(h.position)
        end = h.position + h.dataSize
        while not r.atEnd(end):
            r2 = Header(r, r.binPath, r.format)
            if r2.type == FormType.GRUP:
                group = ReadGRUP(r, r2.group)
                if loadAll: group.load(loadAll)
                continue
            # HACK to limit cells loading
            if r2.type == FormType.CELL and cellsLoaded > 1000: r.skip(r2.dataSize); continue
            record = Record.factory(r2, self.level)
            if not record: r.skip(r2.dataSize); continue
            self.readRecord(r, record, r2.compressed)
            self.records.append(record)
            if r2.type == FormType.CELL: cellsLoaded += 1
            self.groupsByLabel = { s.key:list(g) for s, g in groupby(self.groups, lambda s: s.label) }

    def readGRUP(self, r: Header, h: GroupHeader) -> 'RecordGroup':
        nextPosition = r.tell() + h.dataSize
        self.groups = self.groups or []
        group = RecordGroup(self.level)
        group.addHeader(h)
        self.groups.append(group)
        r.seek(nextPosition)
        # print header path
        # headerPath = string.Join("/", [.. GetHeaderPath([], r)]);
        # log.info(f'Grup: {headerPath} {r.groupType}')
        return group

    # @staticmethod
    # def getHeaderPath(b: list[str], h: Header):
    #     if (header.Parent != null) GetHeaderPath(b, header.Parent);
    #     b.Add(header.GroupType != Header.HeaderGroupType.Top ? BitConverter.ToString(header.Label).Replace("-", string.Empty) : Encoding.ASCII.GetString(header.Label));
    #     return b

    def readRecord(self, r: Header, record: Record, compressed: bool) -> None:
        # log.info(f'Recd: {record.header.type}')
        if not compressed: record.read(r); return
        newDataSize = r.readUInt32()
        newData = decompressZlib2(r, record.header.dataSize - 4, newDataSize)
        # read record
        record.header.position = 0
        record.header.dataSize = newDataSize
        with Header(BinaryReader(newData), r.binPath, r.format) as r2: record.read(r2)


#endregion

#region Extensions

def then[T, TResult](s: Record, value: T, then: callable) -> TResult : return then(value)
def addX[T](s: list[T], value: T) -> T: s.append(value); return value
def addRangeX[T](s: list[T], value: iter) -> iter: s.extend(value); return value
def readINTV(r: Header, length: int) -> INTVField:
    match length:
        case 1: return INTVField(value=r.readByte()),
        case 2: return INTVField(value=r.readInt16()),
        case 4: return INTVField(value=r.readInt32()),
        case 8: return INTVField(value=r.readInt64()),
        case _: raise Exception(f'Tried to read an INTV subrecord with an unsupported size ({length})')
def readDATV(r: Header, length: int, type: chr) -> DATVField:
    match type:
        case 'b': return DATVField(b=r.readInt32() != 0)
        case 'i': return DATVField(i=r.readInt32())
        case 'f': return DATVField(f=r.readSingle())
        case 's': return DATVField(s=r.ReadFUString(length))
        case _: raise Exception(f'{type}')
def readSTRV(r: Header, length: int) -> STRVField: return STRVField(value=r.readFUString(length))
def readSTRV_ZPad(r: Header, length: int) -> STRVField: return STRVField(value=r.readFAString(length))
def readFILE(r: Header, length: int) -> FILEField: return FILEField(value=r.readFUString(length))
def readBYTV(r: Header, length: int) -> BYTVField: return BYTVField(value=r.readBytes(length))
def readUNKN(r: Header, length: int) -> UNKNField: return UNKNField(value=r.readBytes(length))

#endregion

#region Records

# AACT.Action - 0050 - tag::AACT[]
class AACTRecord(Record):
    CNAM: CREFField # RGB Color

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.CNAM: z = self.CNAM = r.readS(CREFField, dataSize)
            case _: z = Record.empty
        return z
# end::AACT[]

# ACRE.Placed creature - 0400 - tag::ACRE[]
class ACRERecord(Record):
    NAME: RefField[Record] # Base
    DATA: REFRRecord.DATAField # Position/Rotation
    XOWNs: list[CELLRecord.XOWNGroup] # Ownership (optional)
    XESP: REFRRecord.XESPField # Enable Parent (optional)
    XSCL: FLTVField # Scale (optional)
    XRGD: BYTVField # Ragdoll Data (optional)

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.NAME: z = self.NAME = RefField[Record](r, dataSize)
            case FieldType.DATA: z = self.DATA = REFRRecord.DATAField(r, dataSize)
            case FieldType.XOWN: z = self.XOWNs = (self.XOWNs or []).addX(CELLRecord.XOWNGroup(XOWN = RefField[Record](r, dataSize)))
            case FieldType.XRNK: z = self.XOWNs.last().XRNK = r.readS(IN32Field, dataSize)
            case FieldType.XGLB: z = self.XOWNs.last().XGLB = RefField[Record](r, dataSize)
            case FieldType.XESP: z = self.XESP = REFRRecord.XESPField(r, dataSize)
            case FieldType.XSCL: z = self.XSCL = r.readS(FLTVField, dataSize)
            case FieldType.XRGD: z = self.XRGD = r.readBYTV(dataSize)
            case _: z = Record.empty
        return z
# end::ACRE[]

# ACHR.Actor Reference - 0450 - tag::ACHR[]
class ACHRRecord(Record):
    NAME: RefField[Record] # Base
    DATA: REFRRecord.DATAField # Position/Rotation
    XPCI: RefField[CELLRecord] # Unused (optional)
    XLOD: BYTVField # Distant LOD Data (optional)
    XESP: REFRRecord.XESPField # Enable Parent (optional)
    XMRC: RefField[REFRRecord] # Merchant Container (optional)
    XHRS: RefField[ACRERecord] # Horse (optional)
    XSCL: FLTVField # Scale (optional)
    XRGD: BYTVField # Ragdoll Data (optional)

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.NAME: z = self.NAME = RefField[Record](r, dataSize)
            case FieldType.DATA: z = self.DATA = REFRRecord.DATAField(r, dataSize)
            case FieldType.XPCI: z = self.XPCI = RefField[CELLRecord](r, dataSize)
            case FieldType.FULL: z = self.XPCI.value.setName(r.readFAString(dataSize))
            case FieldType.XLOD: z = self.XLOD = r.readBYTV(dataSize)
            case FieldType.XESP: z = self.XESP = REFRRecord.XESPField(r, dataSize)
            case FieldType.XMRC: z = self.XMRC = RefField[REFRRecord](r, dataSize)
            case FieldType.XHRS: z = self.XHRS = RefField[ACRERecord](r, dataSize)
            case FieldType.XSCL: z = self.XSCL = r.readS(FLTVField, dataSize)
            case FieldType.XRGD: z = self.XRGD = r.readBYTV(dataSize)
            case _: z = Record.empty
        return z
# end::ACHR[]

# ACTI.Activator - 3450 - tag::ACTI[]
class ACTIRecord(Record, IHaveMODL):
    MODL: MODLGroup # Model Name
    FULL: STRVField # Item Name
    SCRI: RefField[SCPTRecord] # Script (Optional)
    # TES4
    SNAM: RefField[SOUNRecord] # Sound (Optional)

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID or FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize)
            case FieldType.FULL or FieldType.FNAM: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.SCRI: z = self.SCRI = RefField[SCPTRecord](r, dataSize)
            # TES4
            case FieldType.SNAM: z = self.SNAM = RefField[SOUNRecord](r, dataSize)
            case _: z = Record.empty
        return z
# end::ACTI[]

# ADDN-Addon Node - 0050 - tag::ADDN[]
class ADDNRecord(Record):
    CNAM: CREFField # RGB Color

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.CNAM: z = self.CNAM = r.readS(CREFField, dataSize)
            case _: z = Record.empty
        return z
# end::ADDN[]

# ALCH.Potion - 3450 - tag::ALCH[]
class ALCHRecord(Record, IHaveMODL):
    # TESX
    class DATAField:
        Weight: float
        Value: int
        Flags: int # AutoCalc

        def __init__(self, r: Header, dataSize: int):
            self.weight = r.readSingle()
            if r.format == FormType.TES3:
                self.value = r.readInt32()
                self.flags = r.readInt32()
        def ENITField(self, r: Header, dataSize: int) -> object:
            self.value = r.readInt32()
            self.flags = r.readByte()
            r.skip(3) # Unknown
            return True
    # TES3
    class ENAMField:
        def __init__(self, r: Header, dataSize: int):
            self.effectId: int = r.readInt16()
            self.skillId: int = r.readByte() # for skill related effects, -1/0 otherwise
            self.attributeId: int = r.readByte() # for attribute related effects, -1/0 otherwise
            self.unknown1: int = r.readInt32()
            self.unknown2: int = r.readInt32()
            self.duration: int = r.readInt32()
            self.magnitude: int = r.readInt32()
            self.unknown4: int = r.readInt32()

    MODL: MODLGroup # Model
    FULL: STRVField  # Item Name
    DATA: DATAField  # Alchemy Data
    ENAM: ENAMField # Enchantment
    ICON: FILEField  # Icon
    SCRI: RefField[SCPTRecord] # Script (optional)
    # TES4
    EFITs: list[ENCHRecord.EFITField] = [] # Effect Data
    SCITs: list[ENCHRecord.SCITField] = [] # Script Effect Data

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID or FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize)
            case FieldType.FULL: z = FULL = r.readSTRV(dataSize) if len(self.SCITs) == 0 else SCITs.last().FULLField(r, dataSize)
            case FieldType.FNAM: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.DATA or FieldType.ALDT: z = self.DATA = DATAField(r, dataSize)
            case FieldType.ENAM: z = self.ENAM = ENAMField(r, dataSize)
            case FieldType.ICON or FieldType.TEXT: z = self.ICON = r.readFILE(dataSize)
            case FieldType.SCRI: z = self.SCRI = RefField[SCPTRecord](r, dataSize)
            # TES4
            case FieldType.ENIT: z = self.DATA.ENITField(r, dataSize)
            case FieldType.EFID: r.skip(dataSize)
            case FieldType.EFIT: z = self.EFITs.addX(ENCHRecord.EFITField(r, dataSize))
            case FieldType.SCIT: z = self.SCITs.addX(ENCHRecord.SCITField(r, dataSize))
            case _: z = Record.empty
        return z
# end::ALCH[]

# AMMO.Ammo - 0450 - tag::AMMO[]
class AMMORecord(Record, IHaveMODL):
    class DATAField:
        def __init__(self r: Header, dataSize: int):
            self.speed: float = r.readSingle()
            self.flags: int = r.readUInt32()
            self.value: int = r.readUInt32()
            self.weight: float = r.readSingle()
            self.damage: int = r.readUInt16()

    MODL: MODLGroup # Model
    FULL: STRVField # Item Name
    ICON: FILEField  # Male Icon (optional)
    ENAM: RefField[ENCHRecord] # Enchantment ID (optional)
    ANAM: IN16Field # Enchantment Points (optional)
    DATA: DATAField # Ammo Data

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize)
            case FieldType.FULL: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.ICON: z = self.ICON = r.readFILE(dataSize)
            case FieldType.ENAM: z = self.ENAM = RefField[ENCHRecord](r, dataSize)
            case FieldType.ANAM: z = self.ANAM = r.readS(IN16Field, dataSize)
            case FieldType.DATA: z = self.DATA = DATAField(r, dataSize)
            case _: z = Record.empty
        return z
# end::AMMO[]

# ANIO.Animated Object - 0450 - tag::ANIO[]
class ANIORecord(Record, IHaveMODL):
    MODL: MODLGroup # Model
    DATA: RefField[IDLERecord] # IDLE Animation

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.DATA: z = self.DATA = RefField[IDLERecord](r, dataSize)
            case _: z = Record.empty
        return z
# end::ANIO[]

# APPA.Alchem Apparatus - 3450 - tag::APPA[]
class APPARecord(Record, IHaveMODL):
    # TESX
    class DATAField:
        type: int # 0 = Mortar and Pestle, 1 = Albemic, 2 = Calcinator, 3 = Retort
        value: int
        weight: float
        quality: float

        def __init__(self, r: Header, dataSize: int):
            if r.format == FormType.TES3:
                self.type = r.readInt32() & 0xFF
                self.quality = r.readSingle()
                self.weight = r.readSingle()
                self.value = r.readInt32()
                return
            self.type = r.readByte()
            self.value = r.readInt32()
            self.weight = r.readSingle()
            self.quality = r.readSingle()

    MODL: MODLGroup # Model
    FULL: STRVField # Item Name
    DATA: DATAField # Alchemy Data
    ICON: FILEField # Inventory Icon
    SCRI: RefField[SCPTRecord] # Script Name

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID or FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize)
            case FieldType.FULL or FieldType.FNAM: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.DATA or FieldType.AADT: z = self.DATA = DATAField(r, dataSize)
            case FieldType.ICON or FieldType.ITEX: z = self.ICON = r.readFILE(dataSize)
            case FieldType.SCRI: z = self.SCRI = RefField[SCPTRecord](r, dataSize)
            case _: z = Record.empty
        return z
# end::APPA[]

# ARMA.Armature (Model) - 0050 - tag::ARMA[]
class ARMARecord(Record):
    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case _: z = Record.empty
        return z
# end::ARMA[]

# ARMO.Armor - 3450 - tag::ARMA[]
class ARMORecord(Record, IHaveMODL):
    # TESX
    class DATAField:
        class ARMOType(Enum): Helmet = 0; Cuirass = 2; L_Pauldron = 3; R_Pauldron = 4; Greaves = 5; Boots = 6; L_Gauntlet = 7; R_Gauntlet = 8; Shield = 9; L_Bracer = 10; R_Bracer = 11
        armour: int
        value: int
        health: int
        weight: float
        # TES3
        type: int
        enchantPts: int

        def __init__(self, r: Header, dataSize: int):
            if r.format == FormType.TES3:
                self.type = r.readInt32()
                self.weight = r.readSingle()
                self.value = r.readInt32()
                self.health = r.readInt32()
                # TES3
                self.enchantPts = r.readInt32()
                self.armour = r.readInt32() & 0xFFFF
                return
            self.armour = r.readInt16()
            self.value = r.readInt32()
            self.health = r.readInt32()
            self.weight = r.readSingle()
            # TES3
            self.type = 0
            self.enchantPts = 0

    MODL: MODLGroup # Male Biped Model
    FULL: STRVField # Item Name
    ICON: FILEField # Male Icon
    DATA: DATAField # Armour Data
    SCRI: RefField[SCPTRecord] # Script Name (optional)
    ENAM: RefField[ENCHRecord] # Enchantment FormId (optional)
    # TES3
    INDXs: List[CLOTRecord.INDXFieldGroup] = [] # Body Part Index
    # TES4
    BMDT: UI32Field # Flags
    MOD2: MODLGroup # Male World Model (optional)
    MOD3: MODLGroup # Female Biped Model (optional)
    MOD4: MODLGroup # Female World Model (optional)
    ICO2: FILEField # Female Icon (optional)
    ANAM: IN16Field # Enchantment Points (optional)

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID or FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize)
            case FieldType.FULL or FieldType.FNAM: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.DATA or FieldType.AODT: z = self.DATA = DATAField(r, dataSize)
            case FieldType.ICON or FieldType.ITEX: z = self.ICON = r.readFILE(dataSize)
            case FieldType.SCRI: z = self.SCRI = RefField[SCPTRecord](r, dataSize)
            case FieldType.ENAM: z = self.ENAM = RefField[ENCHRecord](r, dataSize)
            # TES3
            case FieldType.INDX: z = self.INDXs.addX(CLOTRecord.INDXFieldGroup(INDX = r.readINTV(dataSize)))
            case FieldType.BNAM: z = self.INDXs.last().BNAM = r.readSTRV(dataSize)
            case FieldType.CNAM: z = self.INDXs.last().CNAM = r.readSTRV(dataSize)
            # TES4
            case FieldType.BMDT: z = self.BMDT = r.readS(UI32Field, dataSize)
            case FieldType.MOD2: z = self.MOD2 = MODLGroup(r, dataSize)
            case FieldType.MO2B: z = self.MOD2.MODBField(r, dataSize)
            case FieldType.MO2T: z = self.MOD2.MODTField(r, dataSize)
            case FieldType.MOD3: z = self.MOD3 = MODLGroup(r, dataSize)
            case FieldType.MO3B: z = self.MOD3.MODBField(r, dataSize)
            case FieldType.MO3T: z = self.MOD3.MODTField(r, dataSize)
            case FieldType.MOD4: z = self.MOD4 = MODLGroup(r, dataSize)
            case FieldType.MO4B: z = self.MOD4.MODBField(r, dataSize)
            case FieldType.MO4T: z = self.MOD4.MODTField(r, dataSize)
            case FieldType.ICO2: z = self.ICO2 = r.readFILE(dataSize)
            case FieldType.ANAM: z = self.ANAM = r.readS(IN16Field, dataSize)
            case _: z = Record.empty
        return z
# end::ARMO[]

# ARTO.Art Object - 0050 - tag::ARTO[]
class ARTORecord(Record):
    CNAM: CREFField # RGB Color

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.CNAM: z = self.CNAM = r.readS(CREFField, dataSize)
            case _: z = Record.empty
        return z
# end::ARTO[]

# ASPC.Acoustic Space - 0050 - tag::ASPC[]
class ASPCRecord(Record):
    CNAM: CREFField # RGB Color

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.CNAM: z = self.CNAM = r.readS(CREFField, dataSize)
            case _: z = Record.empty
        return z
# end::ASPC[]

# ASTP.Association Type - 0050 - tag::ASTP[]
class ASTPRecord(Record):
    CNAM: CREFField # RGB Color

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize),
            case FieldType.CNAM: z = self.CNAM = r.readS(CREFField, dataSize),
            case _: z = Record.empty
        return z
# end::ASTP[]

# AVIF.Actor Values_Perk Tree Graphics - 0050 - tag::ASTP[]
class AVIFRecord(Record):
    CNAM: CREFField # RGB Color

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.DID = r.readSTRV(dataSize)
            case FieldType.CNAM: z = self.CNAM = r.readS(CREFField, dataSize)
            case _: z = Record.empty
        return z
# end::AVIF[]

# BODY.Body - 3000 - tag::ASTP[]
class BODYRecord(Record, IHaveMODL):
    class BYDTField:
        def __init__(self, r: Header, dataSize: int):
            self.part: int = r.readByte()
            self.vampire: int = r.readByte()
            self.flags: int = r.readByte()
            self.partType: int = r.readByte()

    MODL: MODLGroup # NIF Model
    FNAM: STRVField # Body Name
    BYDT: BYDTField

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        if r.format == TES3:
            match type:
                case FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
                case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
                case FieldType.FNAM: z = self.FNAM = r.readSTRV(dataSize)
                case FieldType.BYDT: z = self.BYDT = BYDTField(r, dataSize)
                case _: z = Record.empty
            return z
        return None
# end::BODY[]

# BOOK.Book - 3450 - tag::BOOK[]
class BOOKRecord(Record, IHaveMODL):
    class DATAField:
        flags: int # Scroll - (1 is scroll, 0 not)
        teaches: int # SkillId - (-1 is no skill)
        value: int
        weight: float
        # TES3
        enchantPts: int

        def __init__(self, r: Header, dataSize: int):
            if r.format == FormType.TES3:
                self.weight = r.readSingle()
                self.value = r.readInt32()
                self.flags = r.readInt32() & 0xFF
                self.teaches = r.readInt32() & 0xFF
                self.enchantPts = r.readInt32()
                return
            self.flags = r.readByte()
            self.teaches = r.readByte()
            self.value = r.readInt32()
            self.weight = r.readSingle()
            self.enchantPts = 0

    MODL: MODLGroup # Model (optional)
    FULL: STRVField # Item Name
    DATA: DATAField # Book Data
    DESC: STRVField # Book Text
    ICON: FILEField # Inventory Icon (optional)
    SCRI: RefField[SCPTRecord] # Script Name (optional)
    ENAM: RefField[ENCHRecord] # Enchantment FormId (optional)
    # TES4
    ANAM: IN16Field # Enchantment points (optional)

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID or FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize)
            case FieldType.FULL or FieldType.FNAM: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.DATA or FieldType.BKDT: z = self.DATA = DATAField(r, dataSize)
            case FieldType.ICON or FieldType.ITEX: z = self.ICON = r.readFILE(dataSize)
            case FieldType.SCRI: z = self.SCRI = RefField[SCPTRecord](r, dataSize)
            case FieldType.DESC or FieldType.TEXT: z = self.DESC = r.readSTRV(dataSize)
            case FieldType.ENAM: z = self.ENAM = RefField[ENCHRecord](r, dataSize)
            # TES4
            case FieldType.ANAM: z = self.ANAM = r.readS(IN16Field, dataSize)
            case _: z = Record.empty
        return z
# end::BODY[]

# BSGN.Birthsign - 3400 - tag::BSGN[]
class BSGNRecord(Record):
    FULL: STRVField # Sign Name
    ICON: FILEField # Texture
    DESC: STRVField # Description
    NPCSs: list[STRVField] = [] # TES3: Spell/ability
    SPLOs: list[RefField[Record]] = [] # TES4: (points to a SPEL or LVSP record)

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID or FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.FULL or FieldType.FNAM: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.ICON or FieldType.TNAM: z = self.ICON = r.readFILE(dataSize)
            case FieldType.DESC: z = self.DESC = r.readSTRV(dataSize)
            case FieldType.SPLO: z = (self.SPLOs or []).addX(RefField[Record](r, dataSize))
            case FieldType.NPCS: z = (self.NPCSs or []).addX(r.readSTRV(dataSize))
            case _: z = Record.empty
        return z
# end::BSGN[]

# CELL.Cell - 3450 - tag::CELL[]
class CELLRecord(Record, ICellRecord):
    class CELLFlags(Flag):
        Interior = 0x0001
        HasWater = 0x0002
        InvertFastTravel = 0x0004 # IllegalToSleepHere
        BehaveLikeExterior = 0x0008 # BehaveLikeExterior (Tribunal), Force hide land (exterior cell) / Oblivion interior (interior cell)
        Unknown1 = 0x0010
        PublicArea = 0x0020 # Public place
        HandChanged = 0x0040
        ShowSky = 0x0080 # Behave like exterior
        UseSkyLighting = 0x0100

    class XCLCField:
        def __repr__(self): return f'{self.gridX}x{self.gridY}'
        _struct = { 8 => '<2i', 12 => '<2iI' }
        def __init__(self, tuple):
            match len(tuple):
                case 8:
                    self.gridX, \
                    self.gridY = tuple
                case 12:
                    self.gridX, \
                    self.gridY, \
                    self.flags = tuple

    class XCLLField:
        _struct = { 16 => '<4c4c4cf', 36 => '<4c4c4c2f2i2f', 40 => '<4c4c4c2f2i3f' }
        def __init__(self, tuple):
            match len(tuple):
                case 16:
                    self.ambientColor, \
                    self.directionalColor, '''SunlightColor''' \
                    self.fogColor, \
                    self.fogNear, '''FogDensity''' = tuple
                case 36:
                    self.ambientColor, \
                    self.directionalColor, '''SunlightColor''' \
                    self.fogColor, \
                    self.fogNear, '''FogDensity''' \
                    '''TES4''' \
                    self.fogFar,
                    self.directionalRotationXY,
                    self.directionalRotationZ,
                    self.directionalFade,
                    self.fogClipDist = tuple
                case 40:
                    self.ambientColor, \
                    self.directionalColor, '''SunlightColor''' \
                    self.fogColor, \
                    self.fogNear, '''FogDensity''' \
                    '''TES4''' \
                    self.fogFar,
                    self.directionalRotationXY,
                    self.directionalRotationZ,
                    self.directionalFade,
                    self.fogClipDist,
                    '''TES5''' \
                    self.fogPow = tuple
            self.ambientColor = ByteColor4(self.ambientColor)
            self.directionalColor = ByteColor4(self.directionalColor)
            self.fogColor = ByteColor4(self.fogColor)

    class XOWNGroup:
        XOWN: RefField[Record]
        XRNK: IN32Field # Faction rank
        XGLB: RefField[Record]

    class XYZAField:
        _struct = ('<3f3f', 24)
        def __init__(self, tuple):
            self.position, \
            self.eulerAngles = tuple
            self.position = Float3(self.position)
            self.eulerAngles = Float3(self.eulerAngles)

    class RefObj:
        def __repr__(self): return f'CREF: {self.EDID.value}'
        FRMR: UI32Field # Object Index (starts at 1)
        # This is used to uniquely identify objects in the cell. For files the index starts at 1 and is incremented for each object added. For modified objects the index is kept the same.
        EDID: STRVField # Object ID
        XSCL: FLTVField # Scale (Static)
        DELE: IN32Field # Indicates that the reference is deleted.
        DODT: XYZAField # XYZ Pos, XYZ Rotation of exit
        DNAM: STRVField # Door exit name (Door objects)
        FLTV: FLTVField # Follows the DNAM optionally, lock level
        KNAM: STRVField # Door key
        TNAM: STRVField # Trap name
        UNAM: BYTEField # Reference Blocked (only occurs once in MORROWIND.ESM)
        ANAM: STRVField # Owner ID string
        BNAM: STRVField # Global variable/rank ID
        INTV: IN32Field # Number of uses, occurs even for objects that don't use it
        NAM9: UI32Field # Unknown
        XSOL: STRVField # Soul Extra Data (ID string of creature)
        DATA: XYZAField # Ref Position Data
        # TES?
        CNAM: STRVField # Unknown
        NAM0: UI32Field # Unknown
        XCHG: IN32Field # Unknown
        INDX: IN32Field # Unknown

    FULL: STRVField # Full Name / TES3:RGNN - Region name
    DATA: UI16Field # Flags
    XCLC: XCLCField # Cell Data (only used for exterior cells)
    XCLL: XCLLField # Lighting (only used for interior cells)
    XCLW: FLTVField # Water Height
    # TES3
    NAM0: UI32Field # Number of objects in cell in current file (Optional)
    INTV: INTVField # Unknown
    NAM5: CREFField # Map Color (COLORREF)
    # TES4
    XCLRs: RefField[REGNRecord][] # Regions
    XCMT: BYTEField # Music (optional)
    XCCM: RefField[CLMTRecord] # Climate
    XCWT: RefField[WATRRecord] # Water
    XOWNs: list[XOWNGroup] = [] # Ownership

    # Referenced Object Data Grouping
    inFRMR: bool = False
    refObjs: list[RefObj] = []
    _lastRef: RefObj

    @property
    def isInterior(self) -> bool: return (self.DATA.value & 0x01) == 0x01
    gridId: Int3 # => Int3(XCLC.Value.GridX, XCLC.Value.GridY, !IsInterior ? 0 : -1);
    @property
    def ambientLight(self) -> Colorf: return Colorf(self.XCLL.value.ambientColor.asColor32) if self.XCLL else None

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        #log.info(f'   {type}')
        if not self.inFRMR and type == FieldType.FRMR: self.inFRMR = True
        if not self.inFRMR:
            match type:
                case FieldType.EDID or FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
                case FieldType.FULL or FieldType.RGNN: z = self.FULL = r.readSTRV(dataSize)
                case FieldType.DATA: z = self.DATA = r.readINTV(4 if r.format == FormType.TES3 else dataSize).asUI16Field; if r.format == FormType.TES3: self.XCLC = r.readS<XCLCField>(8 if r.format == FormType.TES3 else dataSize)
                case FieldType.XCLC: z = self.XCLC = r.readS(XCLCField, 8 if r.format == FormType.TES3 else dataSize)
                case FieldType.XCLL or FieldType.AMBI: z = self.XCLL = r.readS(XCLLField, dataSize)
                case FieldType.XCLW or FieldType.WHGT: z = self.XCLW = r.readS(FLTVField, dataSize)
                # TES3
                case FieldType.NAM0: z = self.NAM0 = r.readS(UI32Field, dataSize)
                case FieldType.INTV: z = self.INTV = r.readINTV(dataSize)
                case FieldType.NAM5: z = self.NAM5 = r.readS<CREFField>(dataSize)
                # TES4
                case FieldType.XCLR: z = self.XCLRs = [.. Enumerable.Range(0, dataSize >> 2).Select(x => RefField<REGNRecord>(r, 4))]
                case FieldType.XCMT: z = self.XCMT = r.readS<BYTEField>(dataSize)
                case FieldType.XCCM: z = self.XCCM = RefField[CLMTRecord](r, dataSize)
                case FieldType.XCWT: z = self.XCWT = RefField[WATRRecord](r, dataSize)
                case FieldType.XOWN: z = self.XOWNs.addX(XOWNGroup(XOWN = RefField[Record](r, dataSize)))
                case FieldType.XRNK: z = self.XOWNs.last().XRNK = r.readS(IN32Field, dataSize)
                case FieldType.XGLB: z = self.XOWNs.last().XGLB = RefField[Record](r, dataSize)
                case _: z = Record.empty
        # Referenced Object Data Grouping
        match type:
        # RefObjDataGroup sub-records
            case FieldType.FRMR: z = self.RefObjs.addX(_lastRef = RefObj()).FRMR = r.readS<UI32Field>(dataSize)
            case FieldType.NAME: z = self._lastRef.EDID = r.readSTRV(dataSize)
            case FieldType.XSCL: z = self._lastRef.XSCL = r.readS<FLTVField>(dataSize)
            case FieldType.DODT: z = self._lastRef.DODT = r.readS<XYZAField>(dataSize)
            case FieldType.DNAM: z = self._lastRef.DNAM = r.readSTRV(dataSize)
            case FieldType.FLTV: z = self._lastRef.FLTV = r.readS<FLTVField>(dataSize)
            case FieldType.KNAM: z = self._lastRef.KNAM = r.readSTRV(dataSize)
            case FieldType.TNAM: z = self._lastRef.TNAM = r.readSTRV(dataSize)
            case FieldType.UNAM: z = self._lastRef.UNAM = r.readS<BYTEField>(dataSize)
            case FieldType.ANAM: z = self._lastRef.ANAM = r.readSTRV(dataSize)
            case FieldType.BNAM: z = self._lastRef.BNAM = r.readSTRV(dataSize)
            case FieldType.INTV: z = self._lastRef.INTV = r.readS<IN32Field>(dataSize)
            case FieldType.NAM9: z = self._lastRef.NAM9 = r.readS<UI32Field>(dataSize)
            case FieldType.XSOL: z = self._lastRef.XSOL = r.readSTRV(dataSize)
            case FieldType.DATA: z = self._lastRef.DATA = r.readS<XYZAField>(dataSize)
            case # TES
            case FieldType.CNAM: z = self._lastRef.CNAM = r.readSTRV(dataSize)
            case FieldType.NAM0: z = self._lastRef.NAM0 = r.readS<UI32Field>(dataSize)
            case FieldType.XCHG: z = self._lastRef.XCHG = r.readS<IN32Field>(dataSize)
            case FieldType.INDX: z = self._lastRef.INDX = r.readS<IN32Field>(dataSize)
            case _: z = Record.empty
        return z
# end::CELL[]

# CLAS.Class - 3450 - tag::CLAS[]
class CLASRecord(Record):
    class DATAField:
        #wbArrayS('Primary Attributes', wbInteger('Primary Attribute', itS32, wbActorValueEnum), 2),
        #wbInteger('Specialization', itU32, wbSpecializationEnum),
        #wbArrayS('Major Skills', wbInteger('Major Skill', itS32, wbActorValueEnum), 7),
        #wbInteger('Flags', itU32, wbFlags(['Playable', 'Guard'])),
        #wbInteger('Buys/Sells and Services', itU32, wbServiceFlags),
        #wbInteger('Teaches', itS8, wbSkillEnum),
        #wbInteger('Maximum training level', itU8),
        #wbInteger('Unused', itU16)
        def __init(self, r: Header, dataSize: int):
            r.skip(dataSize)

    FULL: STRVField # Name
    DESC: STRVField # Description
    # TES4
    ICON: STRVField # Icon (Optional)
    DATA: DATAField # Data

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID or FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.FULL or FieldType.FNAM: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.CLDT: z = self.r.skip(dataSize) # TES3
            case FieldType.DESC: z = self.DESC = r.readSTRV(dataSize)
            # TES4
            case FieldType.ICON: z = self.ICON = r.readSTRV(dataSize)
            case FieldType.DATA: z = self.DATA = DATAField(r, dataSize)
            case _: z = Record.empty
        return z
# end::CLAS[]

# CLOT.Clothing - 3450 - tag::CLOT[]
class CLOTRecord(Record, IHaveMODL):
    # TESX
    class DATAField:
        class CLOTType(Enum): Pants = 0; Shoes = 1; Shirt = 2; Belt = 3; Robe = 4; R_Glove = 5; L_Glove = 6; Skirt = 7; Ring = 8; Amulet = 9
        value: int
        weight: float
        # TES3
        type: int
        enchantPts: int

        def __init__(self, r: Header, dataSize: int):
            if r.format == FormType.TES3:
                self.type = r.readInt32()
                self.weight = r.readSingle()
                self.value = r.readInt16()
                self.enchantPts = r.readInt16()
                return
            self.value = r.readInt32()
            self.weight = r.readSingle()
            self.type = 0
            self.enchantPts = 0

    class INDXFieldGroup:
        def __repr__(self): return f'{self.INDX.value}: {self.BNAM.value}'
        INDX: INTVField 
        BNAM: STRVField
        CNAM: STRVField

    MODL: MODLGroup # Model Name
    FULL: STRVField # Item Name
    DATA: DATAField # Clothing Data
    ICON: FILEField # Male Icon
    ENAM: STRVField # Enchantment Name
    SCRI: RefField[SCPTRecord] # Script Name
    # TES3
    INDXs list[INDXFieldGroup] = [] # Body Part Index (Moved to Race)
    # TES4
    BMDT: UI32Field # Clothing Flags
    MOD2: MODLGroup # Male world model (optional)
    MOD3: MODLGroup # Female biped (optional)
    MOD4: MODLGroup # Female world model (optional)
    ICO2: FILEField # Female icon (optional)
    ANAM: IN16Field # Enchantment points (optional)

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID or FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize)
            case FieldType.FULL or FieldType.FNAM: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.DATA or FieldType.CTDT: z = self.DATA = DATAField(r, dataSize)
            case FieldType.ICON or FieldType.ITEX: z = self.ICON = r.readFILE(dataSize)
            case FieldType.INDX: z = self.INDXs.addX(INDXFieldGroup { INDX = r.readINTV(dataSize) })
            case FieldType.BNAM: z = self.INDXs.last().BNAM = r.readSTRV(dataSize)
            case FieldType.CNAM: z = self.INDXs.last().CNAM = r.readSTRV(dataSize)
            case FieldType.ENAM: z = self.ENAM = r.readSTRV(dataSize)
            case FieldType.SCRI: z = self.SCRI = RefField<SCPTRecord>(r, dataSize)
            case FieldType.BMDT: z = self.BMDT = r.readS<UI32Field>(dataSize)
            case FieldType.MOD2: z = self.MOD2 = MODLGroup(r, dataSize)
            case FieldType.MO2B: z = self.MOD2.MODBField(r, dataSize)
            case FieldType.MO2T: z = self.MOD2.MODTField(r, dataSize)
            case FieldType.MOD3: z = self.MOD3 = MODLGroup(r, dataSize)
            case FieldType.MO3B: z = self.MOD3.MODBField(r, dataSize)
            case FieldType.MO3T: z = self.MOD3.MODTField(r, dataSize)
            case FieldType.MOD4: z = self.MOD4 = MODLGroup(r, dataSize)
            case FieldType.MO4B: z = self.MOD4.MODBField(r, dataSize)
            case FieldType.MO4T: z = self.MOD4.MODTField(r, dataSize)
            case FieldType.ICO2: z = self.ICO2 = r.readFILE(dataSize)
            case FieldType.ANAM: z = self.ANAM = r.readS<IN16Field>(dataSize)
            case _: z = Record.empty
        return z
# end::CLOT[]

# CLMT.Climate - 0450 - tag::CLMT[]
class CLMTRecord(Record, IHaveMODL):
    class WLSTField:
        def __init__(self, r: Header, dataSize: int):
            self.weather: Ref[WTHRRecord] = Ref(r.readUInt32())
            self.chance: int = r.readInt32()

    class TNAMField:
        def __init__(self, r: Header, dataSize: int):
            self.sunriseBegin: int = r.readByte()
            self.sunriseEnd: int = r.readByte()
            self.sunsetBegin: int = r.readByte()
            self.sunsetEnd: int = r.readByte()
            self.volatility: int = r.readByte()
            self.moonsPhaseLength: int = r.readByte()

    MODL: MODLGroup # Model
    FNAM: FILEField # Sun Texture
    GNAM: FILEField # Sun Glare Texture
    WLSTs: list[WLSTField] = [] # Climate
    TNAM: TNAMField # Timing

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.FNAM: z = self.FNAM = r.readFILE(dataSize)
            case FieldType.GNAM: z = self.GNAM = r.readFILE(dataSize)
            case FieldType.WLST: z = self.WLSTs.AddRangeX(Enumerable.Range(0, dataSize >> 3).Select(x => WLSTField(r, dataSize)))
            case FieldType.TNAM: z = self.TNAM = TNAMField(r, dataSize)
            case _: z = Record.empty
        return z
# end::CLMT[]

# CONT.Container - 3450 - tag::CONT[]
class CONTRecord(Record, IHaveMODL):
    # TESX
    class DATAField:
        flags: int # flags 0x0001 = Organic, 0x0002 = Respawns, organic only, 0x0008 = Default, unknown
        weight: float

        def __init__(self, r: Header, dataSize: int):
            if r.format == self.TES3:
                self.weight = r.readSingle()
                return
            self.flags = r.readByte()
            self.weight = r.readSingle()
        def FLAGField(self, r: Header, dataSize: int) -> object: z = self.flags = r.readUInt32() & 0xFF; return z

    MODL: MODLGroup # Model
    FULL: STRVField # Container Name
    DATA: DATAField # Container Data
    SCRI: RefField[SCPTRecord]
    CNTOs: list[CNTOField] = []
    # TES4
    SNAM: RefField[SOUNRecord] # Open sound
    QNAM: RefField[SOUNRecord] # Close sound

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID or FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize)
            case FieldType.FULL or FieldType.FNAM: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.DATA or FieldType.CNDT: z = self.DATA = DATAField(r, dataSize)
            case FieldType.FLAG: z = self.DATA.FLAGField(r, dataSize)
            case FieldType.CNTO or FieldType.NPCO: z = self.CNTOs.addX(CNTOField(r, dataSize))
            case FieldType.SCRI: z = self.SCRI = RefField<SCPTRecord>(r, dataSize)
            case FieldType.SNAM: z = self.SNAM = RefField<SOUNRecord>(r, dataSize)
            case FieldType.QNAM: z = self.QNAM = RefField<SOUNRecord>(r, dataSize)
            case _: z = Record.empty
        return z
# end::CONT[]

# CREA.Creature - 3450 - tag::CREA[]
class CREARecord(Record, IHaveMODL):
    class CREAFlags(Flag):
        Biped = 0x0001
        Respawn = 0x0002
        WeaponAndShield = 0x0004
        None = 0x0008
        Swims = 0x0010
        Flies = 0x0020
        Walks = 0x0040
        DefaultFlags = 0x0048
        Essential = 0x0080
        SkeletonBlood = 0x0400
        MetalBlood = 0x0800

    class NPDTField:
        def __init__(self, r: Header, dataSize: int):
            self.type: int  = r.readInt32() # 0 = Creature, 1 = Daedra, 2 = Undead, 3 = Humanoid
            self.level: int  = r.readInt32()
            self.strength: int  = r.readInt32()
            self.intelligence: int  = r.readInt32()
            self.willpower: int  = r.readInt32()
            self.agility: int  = r.readInt32()
            self.speed: int  = r.readInt32()
            self.endurance: int  = r.readInt32()
            self.personality: int  = r.readInt32()
            self.luck: int  = r.readInt32()
            self.health: int  = r.readInt32()
            self.spellPts: int  = r.readInt32()
            self.fatigue: int  = r.readInt32()
            self.soul: int  = r.readInt32()
            self.combat: int  = r.readInt32()
            self.magic: int  = r.readInt32()
            self.stealth: int  = r.readInt32()
            self.attackMin1: int  = r.readInt32()
            self.attackMax1: int  = r.readInt32()
            self.attackMin2: int  = r.readInt32()
            self.attackMax2: int  = r.readInt32()
            self.attackMin3: int  = r.readInt32()
            self.attackMax3: int  = r.readInt32()
            self.gold: int  = r.readInt32()

    class AIDTField:
        class AIFlags(Flag):
            Weapon = 0x00001
            Armor = 0x00002
            Clothing = 0x00004
            Books = 0x00008
            Ingrediant = 0x00010
            Picks = 0x00020
            Probes = 0x00040
            Lights = 0x00080
            Apparatus = 0x00100
            Repair = 0x00200
            Misc = 0x00400
            Spells = 0x00800
            MagicItems = 0x01000
            Potions = 0x02000
            Training = 0x04000
            Spellmaking = 0x08000
            Enchanting = 0x10000
            RepairItem = 0x20000
        def __init__(self, r: Header, dataSize: int):
            self.hello: int = r.readByte()
            self.unknown1: int = r.readByte()
            self.fight: int = r.readByte()
            self.flee: int = r.readByte()
            self.alarm: int = r.readByte()
            self.unknown2: int = r.readByte()
            self.unknown3: int = r.readByte()
            self.unknown4: int = r.readByte()
            self.flags: int = r.readUInt32()

    class AI_WField:
        def __init__(self, r:Header, dataSize: int):
            self.distance: int = r.readInt16()
            self.duration: int = r.readInt16()
            self.timeOfDay: int = r.readByte()
            self.idle: bytes = r.readBytes(8)
            self.unknown: int = r.readByte()

    class AI_TField:
        def __init__(self, r:Header, dataSize: int):
            self.x: float = r.readSingle()
            self.y: float = r.readSingle()
            self.z: float = r.readSingle()
            self.unknown: float = r.readSingle()

    class AI_FField:
        def __init__(self, r:Header, dataSize: int):
            self.x: float = r.readSingle()
            self.y: float = r.readSingle()
            self.z: float = r.readSingle()
            self.duration: int = r.readInt16()
            self.id: str = r.readFAString(32)
            self.unknown: int = r.readInt16()

    class AI_AField:
        def __init__(self, r:Header, dataSize: int):
            self.name: str = r.readFAString(32)
            self.unknown: int = r.readByte()

    MODL: MODLGroup # NIF Model
    FNAM: STRVField # Creature name
    NPDT: NPDTField # Creature data
    FLAG: IN32Field # Creature Flags
    SCRI: RefField[SCPTRecord] # Script
    NPCO: CNTOField # Item record
    AIDT: AIDTField # AI data
    AI_W: AI_WField # AI Wander
    AI_T: AI_TField # AI Travel
    AI_F: AI_FField # AI Follow
    AI_E: AI_FField # AI Escort
    AI_A: AI_AField # AI Activate
    XSCL: FLTVField # Scale (optional), Only present if the scale is not 1.0
    CNAM: STRVField
    NPCSs: list[STRVField] = []

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        if r.format == FormType.TES3:
            match type:
                case FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
                case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
                case FieldType.FNAM: z = self.FNAM = r.readSTRV(dataSize)
                case FieldType.NPDT: z = self.NPDT = NPDTField(r, dataSize)
                case FieldType.FLAG: z = self.FLAG = r.readS<IN32Field>(dataSize)
                case FieldType.SCRI: z = self.SCRI = RefField<SCPTRecord>(r, dataSize)
                case FieldType.NPCO: z = self.NPCO = CNTOField(r, dataSize)
                case FieldType.AIDT: z = self.AIDT = AIDTField(r, dataSize)
                case FieldType.AI_W: z = self.AI_W = AI_WField(r, dataSize)
                case FieldType.AI_T: z = self.AI_T = AI_TField(r, dataSize)
                case FieldType.AI_F: z = self.AI_F = AI_FField(r, dataSize)
                case FieldType.AI_E: z = self.AI_E = AI_FField(r, dataSize)
                case FieldType.AI_A: z = self.AI_A = AI_AField(r, dataSize)
                case FieldType.XSCL: z = self.XSCL = r.readS<FLTVField>(dataSize)
                case FieldType.CNAM: z = self.CNAM = r.readSTRV(dataSize)
                case FieldType.NPCS: z = self.NPCSs.addX(r.ReadSTRV_ZPad(dataSize))
                case _: z = Record.empty
            return z
        return None
# end::CREA[]

# CSTY.Combat Style - 0450 - tag::CSTY[]
class CSTYRecord(Record):
    class CSTDField:
        dodgePercentChance: int
        leftRightPercentChance: int
        dodgeLeftRightTimer_Min: float
        dodgeLeftRightTimer_Max: float
        dodgeForwardTimer_Min: float
        dodgeForwardTimer_Max: float
        dodgeBackTimer_Min: float
        dodgeBackTimer_Max: float
        idleTimer_Min: float
        idleTimer_Max: float
        blockPercentChance: int
        attackPercentChance: int
        recoilStaggerBonusToAttack: float
        unconsciousBonusToAttack: float
        handToHandBonusToAttack: float
        powerAttackPercentChance: int
        recoilStaggerBonusToPower: float
        unconsciousBonusToPowerAttack: float
        powerAttack_Normal: int
        powerAttack_Forward: int
        powerAttack_Back: int
        powerAttack_Left: int
        powerAttack_Right: int
        holdTimer_Min: float
        holdTimer_Maxublic: float
        flags1: int
        acrobaticDodgePercentChance: int
        rangeMult_Optimal: float
        rangeMult_Max: float
        switchDistance_Melee: float
        switchDistance_Ranged: float
        buffStandoffDistance: float
        rangedStandoffDistance: float
        groupStandoffDistance: float
        rushingAttackPercentChance: int
        rushingAttackDistanceMult: float
        flags2: int

        def __init__(self, r: Header, dataSize: int):
            #if (dataSize != 124 && dataSize != 120 && dataSize != 112 && dataSize != 104 && dataSize != 92 && dataSize != 84) self.dodgePercentChance = 0;
            self.dodgePercentChance = r.readByte()
            self.leftRightPercentChance = r.readByte()
            r.skip(2) # Unused
            self.dodgeLeftRightTimer_Min = r.readSingle()
            self.dodgeLeftRightTimer_Max = r.readSingle()
            self.dodgeForwardTimer_Min = r.readSingle()
            self.dodgeForwardTimer_Max = r.readSingle()
            self.dodgeBackTimer_Min = r.readSingle()
            self.dodgeBackTimer_Max = r.readSingle()
            self.idleTimer_Min = r.readSingle()
            self.idleTimer_Max = r.readSingle()
            self.blockPercentChance = r.readByte()
            self.attackPercentChance = r.readByte()
            r.skip(2) # Unused
            self.recoilStaggerBonusToAttack = r.readSingle()
            self.unconsciousBonusToAttack = r.readSingle()
            self.handToHandBonusToAttack = r.readSingle()
            self.powerAttackPercentChance = r.readByte()
            r.skip(3) # Unused
            self.recoilStaggerBonusToPower = r.readSingle()
            self.unconsciousBonusToPowerAttack = r.readSingle()
            self.powerAttack_Normal = r.readByte()
            self.powerAttack_Forward = r.readByte()
            self.powerAttack_Back = r.readByte()
            self.powerAttack_Left = r.readByte()
            self.powerAttack_Right = r.readByte()
            r.skip(3) # Unused
            self.holdTimer_Min = r.readSingle()
            self.holdTimer_Max = r.readSingle()
            self.flags1 = r.readByte()
            self.acrobaticDodgePercentChance = r.readByte()
            r.skip(2) # Unused
            if dataSize == 84: return; self.rangeMult_Optimal = r.readSingle()
            self.RangeMult_Max = r.readSingle();
            if dataSize == 92: return; self.switchDistance_Melee = r.readSingle();
            self.switchDistance_Ranged = r.readSingle()
            self.buffStandoffDistance = r.readSingle()
            if dataSize == 104: return; self.rangedStandoffDistance = r.readSingle()
            self.GroupStandoffDistance = r.readSingle()
            if dataSize == 112: return; self.rushingAttackPercentChance = r.readByte()
            r.skip(3) # Unused
            self.rushingAttackDistanceMult = r.readSingle()
            if dataSize == 120: return; self.flags2 = r.readUInt32()

    class CSADField:
        def __init__(self, r: Header, dataSize: int):
            self.dodgeFatigueModMult: float = r.readSingle()
            self.dodgeFatigueModBase: float = r.readSingle()
            self.encumbSpeedModBase: float = r.readSingle()
            self.encumbSpeedModMult: float = r.readSingle()
            self.dodgeWhileUnderAttackMult: float = r.readSingle()
            self.dodgeNotUnderAttackMult: float = r.readSingle()
            self.dodgeBackWhileUnderAttackMult: float = r.readSingle()
            self.dodgeBackNotUnderAttackMult: float = r.readSingle()
            self.dodgeForwardWhileAttackingMult: float = r.readSingle()
            self.dodgeForwardNotAttackingMult: float = r.readSingle()
            self.blockSkillModifierMult: float = r.readSingle()
            self.blockSkillModifierBase: float = r.readSingle()
            self.blockWhileUnderAttackMult: float = r.readSingle()
            self.blockNotUnderAttackMult: float = r.readSingle()
            self.attackSkillModifierMult: float = r.readSingle()
            self.attackSkillModifierBase: float = r.readSingle()
            self.attackWhileUnderAttackMult: float = r.readSingle()
            self.attackNotUnderAttackMult: float = r.readSingle()
            self.attackDuringBlockMult: float = r.readSingle()
            self.powerAttFatigueModBase: float = r.readSingle()
            self.powerAttFatigueModMult: float = r.readSingle()

    CSTD: CSTDField # Standard
    CSAD: CSADField # Advanced

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.CSTD: z = self.CSTD = CSTDField(r, dataSize)
            case FieldType.CSAD: z = self.CSAD = CSADField(r, dataSize)
            case _: z = Record.empty
        return z
# end::CSTY[]

# DIAL.Dialog Topic - 3450 - tag::DIAL[]
class DIALRecord(Record):
    LastRecord: DIALRecord

    class DIALType(Enum): RegularTopic = 0; Voice = 1; Greeting = 2; Persuasion = 3; Journal = 4

    FULL: STRVField # Dialogue Name
    DATA: BYTEField # Dialogue Type
    QSTIs: List[RefField[QUSTRecord]] # Quests (optional)
    INFOs: List[INFORecord] = [] # Info Records

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID or FieldType.NAME: z = self.(LastRecord = this, EDID = r.readSTRV(dataSize))
            case FieldType.FULL: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.DATA: z = self.DATA = r.readS<BYTEField>(dataSize)
            case FieldType.QSTI or FieldType.QSTR: z = self.(QSTIs ??= []).addX(RefField<QUSTRecord>(r, dataSize))
            case _: z = Record.empty
        return z
# end::DIAL[]

# DLBR.Dialog Branch - 0050 - tag::DIAL[]
class DLBRRecord(Record):
    CNAM: CREFField # RGB color

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.CNAM: z = self.CNAM = r.readS<CREFField>(dataSize)
            case _: z = Record.empty
        return z
# end::DLBR[]

# DLVW.Dialog View - 0050 - tag::DLVW[]
class DLVWRecord(Record):
    CNAM: CREFField # RGB color

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.CNAM: z = self.CNAM = r.readS<CREFField>(dataSize)
            case _: z = Record.empty
        return z
# end::DLVW[]

# DOOR.Door - 3450 - tag::DOOR[]
class DOORRecord(Record, IHaveMODL):
    FULL: STRVField # Door name
    MODL: MODLGroup # NIF model filename
    SCRI: RefField[SCPTRecord] # Script (optional)
    SNAM: RefField[SOUNRecord] # Open Sound
    ANAM: RefField[SOUNRecord] # Close Sound
    # TES4
    BNAM: RefField[SOUNRecord] # Loop Sound
    FNAM: BYTEField # Flags
    TNAM: RefField[Record] # Random teleport destination

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID or FieldType.NAME: z = self.DID = r.readSTRV(dataSize)
            case FieldType.FULL: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.FNAM: z = self.r.Format != TES3 ? FNAM = r.readS<BYTEField>(dataSize) : FULL = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize)
            case FieldType.SCRI: z = self.SCRI = RefField<SCPTRecord>(r, dataSize)
            case FieldType.SNAM: z = self.SNAM = RefField<SOUNRecord>(r, dataSize)
            case FieldType.ANAM: z = self.ANAM = RefField<SOUNRecord>(r, dataSize)
            case FieldType.BNAM: z = self.ANAM = RefField<SOUNRecord>(r, dataSize)
            case FieldType.TNAM: z = self.TNAM = RefField[Record](r, dataSize)
            case _: z = Record.empty
        return z
# end::DOOR[]

# EFSH.Effect Shader - 0450 - tag::EFSH[]
class EFSHRecord(Record):
    class DATAField:
        flags: int
        membraneShader_SourceBlendMode: int
        membraneShader_BlendOperation: int
        membraneShader_ZTestFunction: int
        fillTextureEffect_Color: ByteColor4
        fillTextureEffect_AlphaFadeInTime: float
        fillTextureEffect_FullAlphaTime: float
        fillTextureEffect_AlphaFadeOutTime: float
        fillTextureEffect_PresistentAlphaRatio: float
        fillTextureEffect_AlphaPulseAmplitude: float
        fillTextureEffect_AlphaPulseFrequency: float
        fillTextureEffect_TextureAnimationSpeed_U: float
        fillTextureEffect_TextureAnimationSpeed_V: float
        edgeEffect_FallOff: float
        edgeEffect_Color: ByteColor4
        edgeEffect_AlphaFadeInTime: float
        edgeEffect_FullAlphaTime: float
        edgeEffect_AlphaFadeOutTime: float
        edgeEffect_PresistentAlphaRatio: float
        edgeEffect_AlphaPulseAmplitude: float
        edgeEffect_AlphaPulseFrequency: float
        fillTextureEffect_FullAlphaRatio: float
        edgeEffect_FullAlphaRatio: float
        membraneShader_DestBlendMode: int
        particleShader_SourceBlendMode: int
        particleShader_BlendOperation: int
        particleShader_ZTestFunction: int
        particleShader_DestBlendMode: int
        particleShader_ParticleBirthRampUpTime: float
        particleShader_FullParticleBirthTime: float
        particleShader_ParticleBirthRampDownTime: float
        particleShader_FullParticleBirthRatio: float
        particleShader_PersistantParticleBirthRatio: float
        particleShader_ParticleLifetime: float
        particleShader_ParticleLifetime_Delta: float
        particleShader_InitialSpeedAlongNormal: float
        particleShader_AccelerationAlongNormal: float
        particleShader_InitialVelocity1: float
        particleShader_InitialVelocity2: float
        particleShader_InitialVelocity3: float
        particleShader_Acceleration1: float
        particleShader_Acceleration2: float
        particleShader_Acceleration3: float
        particleShader_ScaleKey1: float
        particleShader_ScaleKey2: float
        particleShader_ScaleKey1Time: float
        particleShader_ScaleKey2Time: float
        colorKey1_Color: ByteColor4
        colorKey2_Color: ByteColor4
        colorKey3_Color: ByteColor4
        colorKey1_ColorAlpha: float
        colorKey2_ColorAlpha: float
        colorKey3_ColorAlpha: float
        colorKey1_ColorKeyTime: float
        colorKey2_ColorKeyTime: float
        colorKey3_ColorKeyTime: float

        def __init__(self, r: Header, dataSize: int):
            if dataSize != 224 and dataSize != 96: self.flags = 0
            self.flags = r.readByte()
            r.skip(3) # Unused
            self.membraneShader_SourceBlendMode = r.readUInt32()
            self.membraneShader_BlendOperation = r.readUInt32()
            self.membraneShader_ZTestFunction = r.readUInt32()
            self.fillTextureEffect_Color = r.readS(ByteColor4, dataSize)
            self.fillTextureEffect_AlphaFadeInTime = r.readSingle()
            self.fillTextureEffect_FullAlphaTime = r.readSingle()
            self.fillTextureEffect_AlphaFadeOutTime = r.readSingle()
            self.fillTextureEffect_PresistentAlphaRatio = r.readSingle()
            self.fillTextureEffect_AlphaPulseAmplitude = r.readSingle()
            self.fillTextureEffect_AlphaPulseFrequency = r.readSingle()
            self.fillTextureEffect_TextureAnimationSpeed_U = r.readSingle()
            self.fillTextureEffect_TextureAnimationSpeed_V = r.readSingle()
            self.edgeEffect_FallOff = r.readSingle()
            self.edgeEffect_Color = r.readS(ByteColor4, dataSize)
            self.edgeEffect_AlphaFadeInTime = r.readSingle()
            self.edgeEffect_FullAlphaTime = r.readSingle()
            self.edgeEffect_AlphaFadeOutTime = r.readSingle()
            self.edgeEffect_PresistentAlphaRatio = r.readSingle()
            self.edgeEffect_AlphaPulseAmplitude = r.readSingle()
            self.edgeEffect_AlphaPulseFrequency = r.readSingle()
            self.fillTextureEffect_FullAlphaRatio = r.readSingle()
            self.edgeEffect_FullAlphaRatio = r.readSingle()
            self.membraneShader_DestBlendMode = r.readUInt32()
            if dataSize == 96: return
            self.particleShader_SourceBlendMode = r.readUInt32()
            self.particleShader_BlendOperation = r.readUInt32()
            self.particleShader_ZTestFunction = r.readUInt32()
            self.particleShader_DestBlendMode = r.readUInt32()
            self.particleShader_ParticleBirthRampUpTime = r.readSingle()
            self.particleShader_FullParticleBirthTime = r.readSingle()
            self.particleShader_ParticleBirthRampDownTime = r.readSingle()
            self.particleShader_FullParticleBirthRatio = r.readSingle()
            self.particleShader_PersistantParticleBirthRatio = r.readSingle()
            self.particleShader_ParticleLifetime = r.readSingle()
            self.particleShader_ParticleLifetime_Delta = r.readSingle()
            self.particleShader_InitialSpeedAlongNormal = r.readSingle()
            self.particleShader_AccelerationAlongNormal = r.readSingle()
            self.particleShader_InitialVelocity1 = r.readSingle()
            self.particleShader_InitialVelocity2 = r.readSingle()
            self.particleShader_InitialVelocity3 = r.readSingle()
            self.particleShader_Acceleration1 = r.readSingle()
            self.particleShader_Acceleration2 = r.readSingle()
            self.particleShader_Acceleration3 = r.readSingle()
            self.particleShader_ScaleKey1 = r.readSingle()
            self.particleShader_ScaleKey2 = r.readSingle()
            self.particleShader_ScaleKey1Time = r.readSingle()
            self.particleShader_ScaleKey2Time = r.readSingle()
            self.colorKey1_Color = r.readS(ByteColor4, dataSize)
            self.colorKey2_Color = r.readS(ByteColor4, dataSize)
            self.colorKey3_Color = r.readS(ByteColor4, dataSize)
            self.colorKey1_ColorAlpha = r.readSingle()
            self.colorKey2_ColorAlpha = r.readSingle()
            self.colorKey3_ColorAlpha = r.readSingle()
            self.colorKey1_ColorKeyTime = r.readSingle()
            self.colorKey2_ColorKeyTime = r.readSingle()
            self.colorKey3_ColorKeyTime = r.readSingle()

    ICON: FILEField # Fill Texture
    ICO2: FILEField # Particle Shader Texture
    DATA: DATAField # Data

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.ICON: z = self.ICON = r.readFILE(dataSize)
            case FieldType.ICO2: z = self.ICO2 = r.readFILE(dataSize)
            case FieldType.DATA: z = self.DATA = DATAField(r, dataSize)
            case _: z = Record.empty
        return z
# end::EFSH[]

# ENCH.Enchantment - 3450 - tag::ENCH[]
class ENCHRecord(Record):
    # TESX
    class ENITField:
        # TES3: 0 = Cast Once, 1 = Cast Strikes, 2 = Cast when Used, 3 = Constant Effect
        # TES4: 0 = Scroll, 1 = Staff, 2 = Weapon, 3 = Apparel
        type: int
        enchantCost: int
        chargeAmount: int # Charge
        flags: int # AutoCalc

        def __init__(self, r: Header, dataSize: int):
            self.type = r.readInt32()
            if r.format == FormType.TES3:
                self.enchantCost = r.readInt32()
                self.chargeAmount = r.readInt32()
            else:
                self.chargeAmount = r.readInt32()
                self.enchantCost = r.readInt32()
            self.flags = r.readInt32()

    class EFITField:
        effectId: str
        type: int # RangeType - 0 = Self, 1 = Touch, 2 = Target
        area: int
        duration: int
        magnitudeMin: int
        # TES3
        skillId: int # (-1 if NA)
        attributeId: int # (-1 if NA)
        magnitudeMax: int
        # TES4
        actorValue: int

        def __init__(self, r: Header, dataSize: int):
            if r.format == FormType.TES3:
                self.effectId = r.readFAString(2)
                self.skillId = r.readByte()
                self.attributeId = r.readByte()
                self.type = r.readInt32()
                self.area = r.readInt32()
                self.duration = r.readInt32()
                self.magnitudeMin = r.readInt32()
                self.magnitudeMax = r.readInt32()
                return
            self.effectId = r.readFAString(4)
            self.magnitudeMin = r.readInt32()
            self.area = r.readInt32()
            self.duration = r.readInt32()
            self.type = r.readInt32()
            self.actorValue = r.readInt32()

    # TES4
    class SCITField:
        name: str
        scriptFormId int
        school: int # 0 = Alteration, 1 = Conjuration, 2 = Destruction, 3 = Illusion, 4 = Mysticism, 5 = Restoration
        visualEffect: str
        flags: int

        def __init__(self, r: Header, dataSize: int):
            self.name = 'Script Effect'
            self.scriptFormId = r.readInt32()
            if dataSize == 4: return
            self.school = r.readInt32()
            self.visualEffect = r.readFAString(4)
            self.flags = r.readUInt32() if dataSize > 12 else 0
        def FULLField(self, r: Header, dataSize: int) -> object: z = self.name = r.readFUString(dataSize); return z

    FULL: STRVField # Enchant name
    ENIT: ENITField # Enchant Data
    EFITs: list[EFITField] = [] # Effect Data
    # TES4
    SCITs: list[SCITField] = [] # Script effect data

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID or FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.FULL: z = self.SCITs.Count == 0 ? FULL = r.readSTRV(dataSize) : SCITs.last().FULLField(r, dataSize)
            case FieldType.ENIT or FieldType.ENDT: z = self.ENIT = ENITField(r, dataSize)
            case FieldType.EFID: z = self.r.Skip(dataSize)
            case FieldType.EFIT or FieldType.ENAM: z = self.EFITs.addX(EFITField(r, dataSize))
            case FieldType.SCIT: z = self.SCITs.addX(SCITField(r, dataSize))
            case _: z = Record.empty
        return z
# end::ENCH[]

# EYES.Eyes - 0450 - tag::XXXX[]
class EYESRecord(Record)
    public STRVField FULL;
    public FILEField ICON;
    public BYTEField DATA; # Playable

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.FULL: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.ICON: z = self.ICON = r.readFILE(dataSize)
            case FieldType.DATA: z = self.DATA = r.readS<BYTEField>(dataSize)
            case _: z = Record.empty
        return z
# end::EYES[]

# FACT.Faction - 3450 - tag::XXXX[]
class FACTRecord(Record):
    # TESX
    class RNAMGroup:
        public override string ToString() => $"{RNAM.Value}:{MNAM.Value}";
        public IN32Field RNAM; # rank
        public STRVField MNAM; # male
        public STRVField FNAM; # female
        public STRVField INAM; # insignia

    # TES3
    public struct FADTField {
        public FADTField(Header r, int dataSize) => r.Skip(dataSize);
    }

    # TES4
    public struct XNAMField(Header r, int dataSize) {
        public override string ToString() => $"{FormId}";
        public int FormId = r.readInt32();
        public int Mod = r.readInt32();
        public int Combat = r.Format > TES4 ? r.readInt32() : 0;
    }

    public STRVField FNAM; # Faction name
    public List<RNAMGroup> RNAMs = []; # Rank Name
    public FADTField FADT; # Faction data
    public List<STRVField> ANAMs = []; # Faction name
    public List<INTVField> INTVs = []; # Faction reaction
                                       # TES4
    public XNAMField XNAM; # Interfaction Relations
    public INTVField DATA; # Flags (byte, uint32)
    public UI32Field CNAM;

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        if r.format == FormType.TES3:
            match type:
            case FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.FNAM: z = self.FNAM = r.readSTRV(dataSize)
            case FieldType.RNAM: z = self.RNAMs.addX(RNAMGroup { MNAM = r.readSTRV(dataSize) })
            case FieldType.FADT: z = self.FADT = FADTField(r, dataSize)
            case FieldType.ANAM: z = self.ANAMs.addX(r.readSTRV(dataSize))
            case FieldType.INTV: z = self.INTVs.addX(r.readINTV(dataSize))
            case _: z = Record.empty
            return z
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.FULL: z = self.FNAM = r.readSTRV(dataSize)
            case FieldType.XNAM: z = self.XNAM = XNAMField(r, dataSize)
            case FieldType.DATA: z = self.DATA = r.readINTV(dataSize)
            case FieldType.CNAM: z = self.CNAM = r.readS<UI32Field>(dataSize)
            case FieldType.RNAM: z = self.RNAMs.addX(RNAMGroup(RNAM = r.readS<IN32Field>(dataSize)))
            case FieldType.MNAM: z = self.RNAMs.last().MNAM = r.readSTRV(dataSize)
            case FieldType.FNAM: z = self.RNAMs.last().FNAM = r.readSTRV(dataSize)
            case FieldType.INAM: z = self.RNAMs.last().INAM = r.readSTRV(dataSize)
            case _: z = Record.empty
        return z
# end::FACT[]

# FLOR.Flora - 0450 - tag::XXXX[]
class FLORRecord(Record, IHaveMODL):
    public MODLGroup MODL { get; set; } # Model
    public STRVField FULL; # Plant Name
    public RefField<SCPTRecord> SCRI; # Script (optional)
    public RefField<INGRRecord> PFIG; # The ingredient the plant produces (optional)
    public BYTVField PFPC; # Spring, Summer, Fall, Winter Ingredient Production (byte)

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize)
            case FieldType.FULL: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.SCRI: z = self.SCRI = RefField<SCPTRecord>(r, dataSize)
            case FieldType.PFIG: z = self.PFIG = RefField<INGRRecord>(r, dataSize)
            case FieldType.PFPC: z = self.PFPC = r.readBYTV(dataSize)
            case _: z = Record.empty
        return z
# end::FLOR[]

# FURN.Furniture - 0450 - tag::XXXX[]
class FURNRecord(Record, IHaveMODL):
    public MODLGroup MODL { get; set; } # Model
    public STRVField FULL; # Furniture Name
    public RefField<SCPTRecord> SCRI; # Script (optional)
    public IN32Field MNAM; # Active marker flags, required. A bit field with a bit value of 1 indicating that the matching marker position in the NIF file is active.

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize)
            case FieldType.FULL: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.SCRI: z = self.SCRI = RefField<SCPTRecord>(r, dataSize)
            case FieldType.MNAM: z = self.MNAM = r.readS<IN32Field>(dataSize)
            case _: z = Record.empty
        return z
# end::FURN[]

# GLOB.Global - 3450 - tag::XXXX[]
class GLOBRecord(Record):
    public BYTEField? FNAM; # Type of global (s, l, f)
    public FLTVField? FLTV; # Float data

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID or FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.FNAM: z = self.FNAM = r.readS<BYTEField>(dataSize)
            case FieldType.FLTV: z = self.FLTV = r.readS<FLTVField>(dataSize)
            case _: z = Record.empty
        return z
# end::GLOB[]

# GMST.Game Setting - 3450 - tag::XXXX[]
public class GMSTRecord : Record {
    public DATVField DATA; # Data

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        if r.format == FormType.TES3:
            match type:
                case FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
                case FieldType.STRV: z = self.DATA = r.ReadDATV(dataSize, 's')
                case FieldType.INTV: z = self.DATA = r.ReadDATV(dataSize, 'i')
                case FieldType.FLTV: z = self.DATA = r.ReadDATV(dataSize, 'f')
                case _: z = Record.empty
            return z
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.DATA: z = self.DATA = r.ReadDATV(dataSize, EDID.Value[0])
            case _: z = Record.empty
        return z
# end::GMST[]

# GRAS.Grass - 0450 - tag::XXXX[]
class GRASRecord(Record):
    public struct DATAField {
        public byte Density;
        public byte MinSlope;
        public byte MaxSlope;
        public ushort UnitFromWaterAmount;
        public uint UnitFromWaterType;
        #Above - At Least,
        #Above - At Most,
        #Below - At Least,
        #Below - At Most,
        #Either - At Least,
        #Either - At Most,
        #Either - At Most Above,
        #Either - At Most Below
        public float PositionRange;
        public float HeightRange;
        public float ColorRange;
        public float WavePeriod;
        public byte Flags;

        public DATAField(Header r, int dataSize) {
            Density = r.ReadByte();
            MinSlope = r.ReadByte();
            MaxSlope = r.ReadByte();
            r.ReadByte();
            UnitFromWaterAmount = r.ReadUInt16();
            r.Skip(2);
            UnitFromWaterType = r.readUInt32();
            PositionRange = r.readSingle();
            HeightRange = r.readSingle();
            ColorRange = r.readSingle();
            WavePeriod = r.readSingle();
            Flags = r.ReadByte();
            r.Skip(3);
        }
    }

    public MODLGroup MODL;
    public DATAField DATA;

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize)
            case FieldType.DATA: z = self.DATA = DATAField(r, dataSize)
            case _: z = Record.empty
        return z
# end::GRAS[]

# HAIR.Hair - 0400 - tag::XXXX[]
class HAIRRecord(Record, IHaveMODL):
    public STRVField FULL;
    public MODLGroup MODL { get; set; }
    public FILEField ICON;
    public BYTEField DATA; # Playable, Not Male, Not Female, Fixed

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.FULL: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.ICON: z = self.ICON = r.readFILE(dataSize)
            case FieldType.DATA: z = self.DATA = r.readS<BYTEField>(dataSize)
            case _: z = Record.empty
        return z
# end::HAIR[]

# IDLE.Idle Animations - 0450 - tag::XXXX[]
class IDLERecord(Record, IHaveMODL):
    public MODLGroup MODL { get; set; }
    public List<SCPTRecord.CTDAField> CTDAs = []; # Conditions
    public BYTEField ANAM;
    public RefField<IDLERecord>[] DATAs;

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.CTDA or FieldType.CTDT: z = self.CTDAs.addX(SCPTRecord.CTDAField(r, dataSize))
            case FieldType.ANAM: z = self.ANAM = r.readS<BYTEField>(dataSize)
            case FieldType.DATA: z = self.DATAs = [.. Enumerable.Range(0, dataSize >> 2).Select(x => RefField<IDLERecord>(r, 4))]
            case _: z = Record.empty
        return z
# end::IDLE[]

# INFO.Dialog Topic Info - 3450 - tag::XXXX[]
class INFORecord(Record):
    # TES3
    public struct DATA3Field(Header r, int dataSize) {
        public int Unknown1 = r.readInt32();
        public int Disposition = r.readInt32();
        public byte Rank = r.ReadByte(); # (0-10)
        public byte Gender = r.ReadByte(); # 0xFF = None, 0x00 = Male, 0x01 = Female
        public byte PCRank = r.ReadByte(); # (0-10)
        public byte Unknown2 = r.ReadByte();
    }

    public class TES3Group {
        public STRVField NNAM; # Next info ID (form a linked list of INFOs for the DIAL). First INFO has an empty PNAM, last has an empty NNAM.
        public DATA3Field DATA; # Info data
        public STRVField ONAM; # Actor
        public STRVField RNAM; # Race
        public STRVField CNAM; # Class
        public STRVField FNAM; # Faction 
        public STRVField ANAM; # Cell
        public STRVField DNAM; # PC Faction
        public STRVField NAME; # The info response string (512 max)
        public FILEField SNAM; # Sound
        public BYTEField QSTN; # Journal Name
        public BYTEField QSTF; # Journal Finished
        public BYTEField QSTR; # Journal Restart
        public SCPTRecord.CTDAField SCVR; # String for the function/variable choice
        public UNKNField INTV; #
        public UNKNField FLTV; # The function/variable result for the previous SCVR
        public STRVField BNAM; # Result text (not compiled)
    }

    # TES4
    public struct DATA4Field(Header r, int dataSize) {
        public byte Type = r.ReadByte();
        public byte NextSpeaker = r.ReadByte();
        public byte Flags = dataSize == 3 ? r.ReadByte() : (byte)0;
    }

    public class TRDTField {
        public uint EmotionType;
        public int EmotionValue;
        public byte ResponseNumber;
        public string ResponseText;
        public string ActorNotes;

        public TRDTField(Header r, int dataSize) {
            EmotionType = r.readUInt32();
            EmotionValue = r.readInt32();
            r.Skip(4); # Unused
            ResponseNumber = r.ReadByte();
            r.Skip(3); # Unused
        }
        public object NAM1Field(Header r, int dataSize) => ResponseText = r.ReadFUString(dataSize);
        public object NAM2Field(Header r, int dataSize) => ActorNotes = r.ReadFUString(dataSize);
    }

    public class TES4Group {
        public DATA4Field DATA; # Info data
        public RefField<QUSTRecord> QSTI; # Quest
        public RefField[DIALRecord] TPIC; # Topic
        public List<RefField[DIALRecord]> NAMEs = []; # Topics
        public List<TRDTField> TRDTs = []; # Responses
        public List<SCPTRecord.CTDAField> CTDAs = []; # Conditions
        public List<RefField[DIALRecord]> TCLTs = []; # Choices
        public List<RefField[DIALRecord]> TCLFs = []; # Link From Topics
        public SCPTRecord.SCHRField SCHR; # Script Data
        public BYTVField SCDA; # Compiled Script
        public STRVField SCTX; # Script Source
        public List<RefField[Record]> SCROs = []; # Global variable reference
    }

    public RefField<INFORecord> PNAM; # Previous info ID
    public TES3Group TES3 = new();
    public TES4Group TES4 = new();

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        if r.format == FormType.TES3:
            match type:
                case FieldType.INAM: z = self.(DIALRecord.LastRecord?.INFOs.addX(this), EDID = r.readSTRV(dataSize))
                case FieldType.PNAM: z = self.PNAM = RefField<INFORecord>(r, dataSize)
                case FieldType.NNAM: z = self.TES3.NNAM = r.readSTRV(dataSize)
                case FieldType.DATA: z = self.TES3.DATA = DATA3Field(r, dataSize)
                case FieldType.ONAM: z = self.TES3.ONAM = r.readSTRV(dataSize)
                case FieldType.RNAM: z = self.TES3.RNAM = r.readSTRV(dataSize)
                case FieldType.CNAM: z = self.TES3.CNAM = r.readSTRV(dataSize)
                case FieldType.FNAM: z = self.TES3.FNAM = r.readSTRV(dataSize)
                case FieldType.ANAM: z = self.TES3.ANAM = r.readSTRV(dataSize)
                case FieldType.DNAM: z = self.TES3.DNAM = r.readSTRV(dataSize)
                case FieldType.NAME: z = self.TES3.NAME = r.readSTRV(dataSize)
                case FieldType.SNAM: z = self.TES3.SNAM = r.readFILE(dataSize)
                case FieldType.QSTN: z = self.TES3.QSTN = r.readS<BYTEField>(dataSize)
                case FieldType.QSTF: z = self.TES3.QSTF = r.readS<BYTEField>(dataSize)
                case FieldType.QSTR: z = self.TES3.QSTR = r.readS<BYTEField>(dataSize)
                case FieldType.SCVR: z = self.TES3.SCVR = SCPTRecord.CTDAField(r, dataSize)
                case FieldType.INTV: z = self.TES3.INTV = r.ReadUNKN(dataSize)
                case FieldType.FLTV: z = self.TES3.FLTV = r.ReadUNKN(dataSize)
                case FieldType.BNAM: z = self.TES3.BNAM = r.readSTRV(dataSize)
                case _: z = Record.empty
            return z
        }
        match type:
            case FieldType.DATA: z = self.TES4.DATA = DATA4Field(r, dataSize)
            case FieldType.QSTI: z = self.TES4.QSTI = RefField<QUSTRecord>(r, dataSize)
            case FieldType.TPIC: z = self.TES4.TPIC = RefField[DIALRecord](r, dataSize)
            case FieldType.NAME: z = self.TES4.NAMEs.addX(RefField[DIALRecord](r, dataSize))
            case FieldType.TRDT: z = self.TES4.TRDTs.addX(TRDTField(r, dataSize))
            case FieldType.NAM1: z = self.TES4.TRDTs.last().NAM1Field(r, dataSize)
            case FieldType.NAM2: z = self.TES4.TRDTs.last().NAM2Field(r, dataSize)
            case FieldType.CTDA or FieldType.CTDT: z = self.TES4.CTDAs.addX(SCPTRecord.CTDAField(r, dataSize))
            case FieldType.TCLT: z = self.TES4.TCLTs.addX(RefField[DIALRecord](r, dataSize))
            case FieldType.TCLF: z = self.TES4.TCLFs.addX(RefField[DIALRecord](r, dataSize))
            case FieldType.SCHR or FieldType.SCHD: z = self.TES4.SCHR = SCPTRecord.SCHRField(r, dataSize)
            case FieldType.SCDA: z = self.TES4.SCDA = r.readBYTV(dataSize)
            case FieldType.SCTX: z = self.TES4.SCTX = r.readSTRV(dataSize)
            case FieldType.SCRO: z = self.TES4.SCROs.addX(RefField[Record](r, dataSize))
            case _: z = Record.empty
        return z
# end::INFO[]

# INGR.Ingredient - 3450 - tag::XXXX[]
class INGRRecord(Record, IHaveMODL):
    # TES3
    public struct IRDTField {
        public float Weight;
        public int Value;
        public int[] EffectId; # 0 or -1 means no effect
        public int[] SkillId; # only for Skill related effects, 0 or -1 otherwise
        public int[] AttributeId; # only for Attribute related effects, 0 or -1 otherwise

        public IRDTField(Header r, int dataSize) {
            Weight = r.readSingle();
            Value = r.readInt32();
            EffectId = int[4];
            for (var i = 0; i < EffectId.Length; i++) EffectId[i] = r.readInt32();
            SkillId = int[4];
            for (var i = 0; i < SkillId.Length; i++) SkillId[i] = r.readInt32();
            AttributeId = int[4];
            for (var i = 0; i < AttributeId.Length; i++) AttributeId[i] = r.readInt32();
        }
    }

    # TES4
    public class DATAField(Header r, int dataSize) {
        public float Weight = r.readSingle();
        public int Value;
        public uint Flags;

        public object ENITField(Header r, int dataSize) {
            Value = r.readInt32();
            Flags = r.readUInt32();
            return Value;
        }
    }

    public MODLGroup MODL { get; set; } # Model Name
    public STRVField FULL; # Item Name
    public IRDTField IRDT; # Ingrediant Data #: TES3
    public DATAField DATA; # Ingrediant Data #: TES4
    public FILEField ICON; # Inventory Icon
    public RefField<SCPTRecord> SCRI; # Script Name
    # TES4
    public List<ENCHRecord.EFITField> EFITs = []; # Effect Data
    public List<ENCHRecord.SCITField> SCITs = []; # Script effect data

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID or FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize)
            case FieldType.FULL: z = self.SCITs.Count == 0 ? FULL = r.readSTRV(dataSize) : SCITs.last().FULLField(r, dataSize)
            case FieldType.FNAM: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.DATA: z = self.DATA = DATAField(r, dataSize)
            case FieldType.IRDT: z = self.IRDT = IRDTField(r, dataSize)
            case FieldType.ICON or FieldType.ITEX: z = self.ICON = r.readFILE(dataSize)
            case FieldType.SCRI: z = self.SCRI = RefField<SCPTRecord>(r, dataSize)
            #
            case FieldType.ENIT: z = self.DATA.ENITField(r, dataSize)
            case FieldType.EFID: z = self.r.Skip(dataSize)
            case FieldType.EFIT: z = self.EFITs.addX(ENCHRecord.EFITField(r, dataSize))
            case FieldType.SCIT: z = self.SCITs.addX(ENCHRecord.SCITField(r, dataSize))
            case _: z = Record.empty
        return z
# end::INGR[]

# KEYM.Key - 0400 - tag::XXXX[]
class KEYMRecord(Record, IHaveMODL):
    public struct DATAField(Header r, int dataSize) {
        public int Value = r.readInt32();
        public float Weight = r.readSingle();
    }

    public MODLGroup MODL { get; set; } # Model
    public STRVField FULL; # Item Name
    public RefField<SCPTRecord> SCRI; # Script (optional)
    public DATAField DATA; # Type of soul contained in the gem
    public FILEField ICON; # Icon (optional)

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize)
            case FieldType.FULL: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.SCRI: z = self.SCRI = RefField<SCPTRecord>(r, dataSize)
            case FieldType.DATA: z = self.DATA = DATAField(r, dataSize)
            case FieldType.ICON: z = self.ICON = r.readFILE(dataSize)
            case _: z = Record.empty
        return z
# end::KEYM[]

# LAND.Land - 3450 - tag::XXXX[]
class LANDRecord(Record):
    # TESX
    public struct VNMLField(Header r, int dataSize) {
        public Byte3[] Vertexs = r.ReadPArray<Byte3>("3B", dataSize / 3); # XYZ 8 bit floats
    }

    public struct VHGTField {
        public float ReferenceHeight; # A height offset for the entire cell. Decreasing this value will shift the entire cell land down.
        public sbyte[] HeightData; # HeightData

        public VHGTField(Header r, int dataSize) {
            ReferenceHeight = r.readSingle();
            var count = dataSize - 4 - 3;
            HeightData = r.ReadPArray<sbyte>("B", count);
            r.Skip(3); # Unused
        }
    }

    public struct VCLRField(Header r, int dataSize) {
        public ByteColor3[] Colors = r.ReadSArray<ByteColor3>(dataSize / 24); # 24-bit RGB
    }

    public struct VTEXField {
        public ushort[] TextureIndicesT3;
        public uint[] TextureIndicesT4;

        public VTEXField(Header r, int dataSize) {
            if (r.format == FormType.TES3) {
                TextureIndicesT3 = r.ReadPArray<ushort>("H", dataSize >> 1);
                TextureIndicesT4 = null;
                return;
            }
            TextureIndicesT3 = null;
            TextureIndicesT4 = r.ReadPArray<uint>("I", dataSize >> 2);
        }
    }

    # TES3
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CORDField {
        public static (string, int) Struct = ("<2i", 8);
        public int CellX;
        public int CellY;
        public override readonly string ToString() => $"{CellX},{CellY}";
    }

    public struct WNAMField {
        # Low-LOD heightmap (signed chars)
        public WNAMField(Header r, int dataSize) {
            r.Skip(dataSize);
            #var heightCount = dataSize;
            #for (var i = 0; i < heightCount; i++) { var height = r.ReadByte(); }
        }
    }

    # TES4
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BTXTField {
        public static (string, int) Struct = ("<I2ch", 8);
        public uint Texture;
        public byte Quadrant;
        public byte Pad01;
        public short Layer;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VTXTField {
        public ushort Position;
        public ushort Pad01;
        public float Opacity;
    }

    public class ATXTGroup {
        public BTXTField ATXT;
        public VTXTField[] VTXTs;
    }

    public override string ToString() => $"LAND: {INTV}";
    public IN32Field DATA; # Unknown (default of 0x09) Changing this value makes the land 'disappear' in the editor.
    # A RGB color map 65x65 pixels in size representing the land normal vectors.
    # The signed value of the 'color' represents the vector's component. Blue
    # is vertical(Z), Red the X direction and Green the Y direction.Note that
    # the y-direction of the data is from the bottom up.
    public VNMLField VNML;
    public VHGTField VHGT; # Height data
    public VNMLField? VCLR; # Vertex color array, looks like another RBG image 65x65 pixels in size. (Optional)
    public VTEXField? VTEX; # A 16x16 array of short texture indices. (Optional)
    # TES3
    public CORDField INTV; # The cell coordinates of the cell
    public WNAMField WNAM; # Unknown byte data.
    # TES4
    public BTXTField[] BTXTs = BTXTField[4]; # Base Layer
    public ATXTGroup[] ATXTs; # Alpha Layer
    ATXTGroup _lastATXT;

    public Int3 GridId; # => Int3(INTV.CellX, INTV.CellY, 0);

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.DATA: z = self.DATA = r.readS<IN32Field>(dataSize)
            case FieldType.VNML: z = self.VNML = VNMLField(r, dataSize)
            case FieldType.VHGT: z = self.VHGT = VHGTField(r, dataSize)
            case FieldType.VCLR: z = self.VCLR = VNMLField(r, dataSize)
            case FieldType.VTEX: z = self.VTEX = VTEXField(r, dataSize)
            # TES3
            case FieldType.INTV: z = self.INTV = r.readS<CORDField>(dataSize)
            case FieldType.WNAM: z = self.WNAM = WNAMField(r, dataSize)
            # TES4
            case FieldType.BTXT: z = self.this.Then(r.readS<BTXTField>(dataSize), btxt => BTXTs[btxt.Quadrant] = btxt)
            case FieldType.ATXT: z = self.(ATXTs ??= ATXTGroup[4], this.Then(r.readS<BTXTField>(dataSize), atxt => _lastATXT = ATXTs[atxt.Quadrant] = ATXTGroup { ATXT = atxt }))
            case FieldType.VTXT: z = self._lastATXT.VTXTs = r.ReadSArray<VTXTField>(dataSize >> 3)
            case _: z = Record.empty
        return z
# end::LAND[]

# LEVC.Leveled Creature - 3000 - tag::XXXX[]
class LEVCRecord(Record):
    public IN32Field DATA; # List data - 1 = Calc from all levels <= PC level
    public BYTEField NNAM; # Chance None?
    public IN32Field INDX; # Number of items in list
    public List<STRVField> CNAMs = []; # ID string of list item
    public List<IN16Field> INTVs = []; # PC level for previous CNAM
    # The CNAM/INTV can occur many times in pairs

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        if r.format == FormType.TES3:
            match type:
                case FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
                case FieldType.DATA: z = self.DATA = r.readS<IN32Field>(dataSize)
                case FieldType.NNAM: z = self.NNAM = r.readS<BYTEField>(dataSize)
                case FieldType.INDX: z = self.INDX = r.readS<IN32Field>(dataSize)
                case FieldType.CNAM: z = self.CNAMs.addX(r.readSTRV(dataSize))
                case FieldType.INTV: z = self.INTVs.addX(r.readS<IN16Field>(dataSize))
                case _: z = Record.empty
            return z
        return None
# end::LEVC[]

# LEVI.Leveled item - 3000 - tag::XXXX[]
class LEVIRecord(Record):
    public IN32Field DATA; # List data - 1 = Calc from all levels <= PC level, 2 = Calc for each item
    public BYTEField NNAM; # Chance None?
    public IN32Field INDX; # Number of items in list
    public List<STRVField> INAMs = []; # ID string of list item
    public List<IN16Field> INTVs = []; # PC level for previous INAM
    # The CNAM/INTV can occur many times in pairs

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        if r.format == FormType.TES3:
            match type:
                case FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
                case FieldType.DATA: z = self.DATA = r.readS<IN32Field>(dataSize)
                case FieldType.NNAM: z = self.NNAM = r.readS<BYTEField>(dataSize)
                case FieldType.INDX: z = self.INDX = r.readS<IN32Field>(dataSize)
                case FieldType.INAM: z = self.INAMs.addX(r.readSTRV(dataSize))
                case FieldType.INTV: z = self.INTVs.addX(r.readS<IN16Field>(dataSize))
                case _: z = Record.empty
            return z
        return None
# end::LEVI[]

# LIGH.Light - 3450 - tag::XXXX[]
class LIGHRecord(Record, IHaveMODL):
    # TESX
    public struct DATAField {
        public enum ColorFlags {
            Dynamic = 0x0001,
            CanCarry = 0x0002,
            Negative = 0x0004,
            Flicker = 0x0008,
            Fire = 0x0010,
            OffDefault = 0x0020,
            FlickerSlow = 0x0040,
            Pulse = 0x0080,
            PulseSlow = 0x0100
        }

        public float Weight;
        public int Value;
        public int Time;
        public int Radius;
        public ByteColor4 LightColor;
        public int Flags;
        # TES4
        public float FalloffExponent;
        public float FOV;

        public DATAField(Header r, int dataSize) {
            if (r.format == FormType.TES3) {
                Weight = r.readSingle();
                Value = r.readInt32();
                Time = r.readInt32();
                Radius = r.readInt32();
                LightColor = r.readS<ByteColor4>(4);
                Flags = r.readInt32();
                FalloffExponent = 1;
                FOV = 90;
                return;
            }
            Time = r.readInt32();
            Radius = r.readInt32();
            LightColor = r.readS<ByteColor4>(4);
            Flags = r.readInt32();
            if (dataSize == 32) { FalloffExponent = r.readSingle(); FOV = r.readSingle(); }
            else { FalloffExponent = 1; FOV = 90; }
            Value = r.readInt32();
            Weight = r.readSingle();
        }
    }

    public MODLGroup MODL { get; set; } # Model
    public STRVField? FULL; # Item Name (optional)
    public DATAField DATA; # Light Data
    public STRVField? SCPT; # Script Name (optional)??
    public RefField<SCPTRecord>? SCRI; # Script FormId (optional)
    public FILEField? ICON; # Male Icon (optional)
    public FLTVField FNAM; # Fade Value
    public RefField<SOUNRecord> SNAM; # Sound FormId (optional)

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID or FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.FULL: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.FNAM: z = self.r.Format != TES3 ? FNAM = r.readS<FLTVField>(dataSize) : FULL = r.readSTRV(dataSize)
            case FieldType.DATA or FieldType.LHDT: z = self.DATA = DATAField(r, dataSize)
            case FieldType.SCPT: z = self.SCPT = r.readSTRV(dataSize)
            case FieldType.SCRI: z = self.SCRI = RefField<SCPTRecord>(r, dataSize)
            case FieldType.ICON or FieldType.ITEX: z = self.ICON = r.readFILE(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize)
            case FieldType.SNAM: z = self.SNAM = RefField<SOUNRecord>(r, dataSize)
            case _: z = Record.empty
        return z
# end::LIGH[]

# LOCK.Lock - 3450 - tag::XXXX[]
class LOCKRecord(Record, IHaveMODL):
    public struct LKDTField(Header r, int dataSize) {
        public float Weight = r.readSingle();
        public int Value = r.readInt32();
        public float Quality = r.readSingle();
        public int Uses = r.readInt32();
    }

    public MODLGroup MODL { get; set; } # Model Name
    public STRVField FNAM; # Item Name
    public LKDTField LKDT; # Lock Data
    public FILEField ICON; # Inventory Icon
    public RefField<SCPTRecord> SCRI; # Script Name

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        if r.format == FormType.TES3:
            match type:
                case FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
                case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
                case FieldType.FNAM: z = self.FNAM = r.readSTRV(dataSize)
                case FieldType.LKDT: z = self.LKDT = LKDTField(r, dataSize)
                case FieldType.ITEX: z = self.ICON = r.readFILE(dataSize)
                case FieldType.SCRI: z = self.SCRI = RefField<SCPTRecord>(r, dataSize)
                case _: z = Record.empty
            return z
        return None
# end::LOCK[]

# LSCR.Load Screen - 0450 - tag::XXXX[]
class LSCRRecord(Record):
    public struct LNAMField(Header r, int dataSize) {
        public Ref[Record] Direct = new(r.readUInt32());
        public Ref<WRLDRecord> IndirectWorld = new(r.readUInt32());
        public short IndirectGridX = r.readInt16();
        public short IndirectGridY = r.readInt16();
    }

    public FILEField ICON; # Icon
    public STRVField DESC; # Description
    public List<LNAMField> LNAMs; # LoadForm

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.ICON: z = self.ICON = r.readFILE(dataSize)
            case FieldType.DESC: z = self.DESC = r.readSTRV(dataSize)
            case FieldType.LNAM: z = self.(LNAMs ??= []).addX(LNAMField(r, dataSize))
            case _: z = Record.empty
        return z
# end::LSCR[]

# LTEX.Land Texture - 3450 - tag::XXXX[]
class LTEXRecord(Record):
    public struct HNAMField(Header r, int dataSize) {
        public byte MaterialType = r.ReadByte();
        public byte Friction = r.ReadByte();
        public byte Restitution = r.ReadByte();
    }

    public FILEField ICON; # Texture
    # TES3
    public INTVField INTV;
    # TES4
    public HNAMField HNAM; # Havok data
    public BYTEField SNAM; # Texture specular exponent
    public List<RefField<GRASRecord>> GNAMs = []; # Potential grass

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID or FieldType.NAME: z = self.EDID = r.readSTRV(dataSize),
            case FieldType.INTV: z = self.INTV = r.readINTV(dataSize),
            case FieldType.ICON or FieldType.DATA: z = self.ICON = r.readFILE(dataSize),
            # TES4
            case FieldType.HNAM: z = self.HNAM = HNAMField(r, dataSize),
            case FieldType.SNAM: z = self.SNAM = r.readS<BYTEField>(dataSize),
            case FieldType.GNAM: z = self.GNAMs.addX(RefField<GRASRecord>(r, dataSize)),
            case _: z = Record.empty
        return z
# end::LTEX[]

# LVLC.Leveled Creature - 0400 - tag::XXXX[]
public class LVLCRecord : Record {
    public BYTEField LVLD; # Chance
    public BYTEField LVLF; # Flags - 0x01 = Calculate from all levels <= player's level, 0x02 = Calculate for each item in count
    public RefField<SCPTRecord> SCRI; # Script (optional)
    public RefField<CREARecord> TNAM; # Creature Template (optional)
    public List<LVLIRecord.LVLOField> LVLOs = [];

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize),
            case FieldType.LVLD: z = self.LVLD = r.readS<BYTEField>(dataSize),
            case FieldType.LVLF: z = self.LVLF = r.readS<BYTEField>(dataSize),
            case FieldType.SCRI: z = self.SCRI = RefField<SCPTRecord>(r, dataSize),
            case FieldType.TNAM: z = self.TNAM = RefField<CREARecord>(r, dataSize),
            case FieldType.LVLO: z = self.LVLOs.addX(LVLIRecord.LVLOField(r, dataSize)),
            case _: z = Record.empty
        return z
# end::LVLC[]

# LVLI.Leveled Item - 0400 - tag::XXXX[]
public class LVLIRecord : Record {
    public struct LVLOField {
        public short Level;
        public Ref[Record] ItemFormId;
        public int Count;

        public LVLOField(Header r, int dataSize) {
            Level = r.readInt16();
            r.Skip(2); # Unused
            ItemFormId = Ref[Record](r.readUInt32());
            if (dataSize == 12) {
                Count = r.readInt16();
                r.Skip(2); # Unused
            }
            else Count = 0;
        }
    }

    public BYTEField LVLD; # Chance
    public BYTEField LVLF; # Flags - 0x01 = Calculate from all levels <= player's level, 0x02 = Calculate for each item in count
    public BYTEField? DATA; # Data (optional)
    public List<LVLOField> LVLOs = [];

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize),
            case FieldType.LVLD: z = self.LVLD = r.readS<BYTEField>(dataSize),
            case FieldType.LVLF: z = self.LVLF = r.readS<BYTEField>(dataSize),
            case FieldType.DATA: z = self.DATA = r.readS<BYTEField>(dataSize),
            case FieldType.LVLO: z = self.LVLOs.addX(LVLOField(r, dataSize)),
            case _: z = Record.empty
        return z
# end::LVLI[]

# LVSP.Leveled Spell - 0400 - tag::XXXX[]
public class LVSPRecord : Record {
    public BYTEField LVLD; # Chance
    public BYTEField LVLF; # Flags
    public List<LVLIRecord.LVLOField> LVLOs = []; # Number of items in list

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize),
            case FieldType.LVLD: z = self.LVLD = r.readS<BYTEField>(dataSize),
            case FieldType.LVLF: z = self.LVLF = r.readS<BYTEField>(dataSize),
            case FieldType.LVLO: z = self.LVLOs.addX(LVLIRecord.LVLOField(r, dataSize)),
            case _: z = Record.empty
        return z
# end::LVSP[]

# MGEF.Magic Effect - 3400 - tag::XXXX[]
public class MGEFRecord : Record {
    # TES3
    public struct MEDTField(Header r, int dataSize) {
        public int SpellSchool = r.readInt32(); # 0 = Alteration, 1 = Conjuration, 2 = Destruction, 3 = Illusion, 4 = Mysticism, 5 = Restoration
        public float BaseCost = r.readSingle();
        public int Flags = r.readInt32(); # 0x0200 = Spellmaking, 0x0400 = Enchanting, 0x0800 = Negative
        public ByteColor4 Color = new((byte)r.readInt32(), (byte)r.readInt32(), (byte)r.readInt32(), 255);
        public float SpeedX = r.readSingle();
        public float SizeX = r.readSingle();
        public float SizeCap = r.readSingle();
    }

    # TES4
    [Flags]
    public enum MFEGFlag : uint {
        Hostile = 0x00000001,
        Recover = 0x00000002,
        Detrimental = 0x00000004,
        MagnitudePercent = 0x00000008,
        Self = 0x00000010,
        Touch = 0x00000020,
        Target = 0x00000040,
        NoDuration = 0x00000080,
        NoMagnitude = 0x00000100,
        NoArea = 0x00000200,
        FXPersist = 0x00000400,
        Spellmaking = 0x00000800,
        Enchanting = 0x00001000,
        NoIngredient = 0x00002000,
        Unknown14 = 0x00004000,
        Unknown15 = 0x00008000,
        UseWeapon = 0x00010000,
        UseArmor = 0x00020000,
        UseCreature = 0x00040000,
        UseSkill = 0x00080000,
        UseAttribute = 0x00100000,
        Unknown21 = 0x00200000,
        Unknown22 = 0x00400000,
        Unknown23 = 0x00800000,
        UseActorValue = 0x01000000,
        SprayProjectileType = 0x02000000, # (Ball if Spray, Bolt or Fog is not specified)
        BoltProjectileType = 0x04000000,
        NoHitEffect = 0x08000000,
        Unknown28 = 0x10000000,
        Unknown29 = 0x20000000,
        Unknown30 = 0x40000000,
        Unknown31 = 0x80000000,
    }

    public class DATAField {
        public uint Flags;
        public float BaseCost;
        public int AssocItem;
        public int MagicSchool;
        public int ResistValue;
        public uint CounterEffectCount; # Must be updated automatically when ESCE length changes!
        public Ref<LIGHRecord> Light;
        public float ProjectileSpeed;
        public Ref<EFSHRecord> EffectShader;
        public Ref<EFSHRecord> EnchantEffect;
        public Ref<SOUNRecord> CastingSound;
        public Ref<SOUNRecord> BoltSound;
        public Ref<SOUNRecord> HitSound;
        public Ref<SOUNRecord> AreaSound;
        public float ConstantEffectEnchantmentFactor;
        public float ConstantEffectBarterFactor;

        public DATAField(Header r, int dataSize) {
            Flags = r.readUInt32();
            BaseCost = r.readSingle();
            AssocItem = r.readInt32();
            #wbUnion('Assoc. Item', wbMGEFFAssocItemDecider, [
            #  wbFormIDCk('Unused', [NULL]),
            #  wbFormIDCk('Assoc. Weapon', [WEAP]),
            #  wbFormIDCk('Assoc. Armor', [ARMO, NULL{?}]),
            #  wbFormIDCk('Assoc. Creature', [CREA, LVLC, NPC_]),
            #  wbInteger('Assoc. Actor Value', itS32, wbActorValueEnum)
            MagicSchool = r.readInt32();
            ResistValue = r.readInt32();
            CounterEffectCount = r.ReadUInt16();
            r.Skip(2); # Unused
            Light = Ref<LIGHRecord>(r.readUInt32());
            ProjectileSpeed = r.readSingle();
            EffectShader = Ref<EFSHRecord>(r.readUInt32());
            if (dataSize == 36)
                return;
            EnchantEffect = Ref<EFSHRecord>(r.readUInt32());
            CastingSound = Ref<SOUNRecord>(r.readUInt32());
            BoltSound = Ref<SOUNRecord>(r.readUInt32());
            HitSound = Ref<SOUNRecord>(r.readUInt32());
            AreaSound = Ref<SOUNRecord>(r.readUInt32());
            ConstantEffectEnchantmentFactor = r.readSingle();
            ConstantEffectBarterFactor = r.readSingle();
        }
    }

    public override string ToString() => $"MGEF: {INDX.Value}:{EDID.Value}";
    public STRVField DESC; # Description
                           # TES3
    public INTVField INDX; # The Effect ID (0 to 137)
    public MEDTField MEDT; # Effect Data
    public FILEField ICON; # Effect Icon
    public STRVField PTEX; # Particle texture
    public STRVField CVFX; # Casting visual
    public STRVField BVFX; # Bolt visual
    public STRVField HVFX; # Hit visual
    public STRVField AVFX; # Area visual
    public STRVField? CSND; # Cast sound (optional)
    public STRVField? BSND; # Bolt sound (optional)
    public STRVField? HSND; # Hit sound (optional)
    public STRVField? ASND; # Area sound (optional)
                            # TES4
    public STRVField FULL;
    public MODLGroup MODL;
    public DATAField DATA;
    public STRVField[] ESCEs;

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        if r.format == FormType.TES3:
            match type:
                case FieldType.INDX: z = self.INDX = r.readINTV(dataSize),
                case FieldType.MEDT: z = self.MEDT = MEDTField(r, dataSize),
                case FieldType.ITEX: z = self.ICON = r.readFILE(dataSize),
                case FieldType.PTEX: z = self.PTEX = r.readSTRV(dataSize),
                case FieldType.CVFX: z = self.CVFX = r.readSTRV(dataSize),
                case FieldType.BVFX: z = self.BVFX = r.readSTRV(dataSize),
                case FieldType.HVFX: z = self.HVFX = r.readSTRV(dataSize),
                case FieldType.AVFX: z = self.AVFX = r.readSTRV(dataSize),
                case FieldType.DESC: z = self.DESC = r.readSTRV(dataSize),
                case FieldType.CSND: z = self.CSND = r.readSTRV(dataSize),
                case FieldType.BSND: z = self.BSND = r.readSTRV(dataSize),
                case FieldType.HSND: z = self.HSND = r.readSTRV(dataSize),
                case FieldType.ASND: z = self.ASND = r.readSTRV(dataSize),
                case _: z = Record.empty
            return z
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize),
            case FieldType.FULL: z = self.FULL = r.readSTRV(dataSize),
            case FieldType.DESC: z = self.DESC = r.readSTRV(dataSize),
            case FieldType.ICON: z = self.ICON = r.readFILE(dataSize),
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize),
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize),
            case FieldType.DATA: z = self.DATA = DATAField(r, dataSize),
            case FieldType.ESCE: z = self.ESCEs = [.. Enumerable.Range(0, dataSize >> 2).Select(x => r.readSTRV(4))],
            case _: z = Record.empty
        return z
# end::MGEF[]

# MISC.Misc Item - 3450 - tag::XXXX[]
public class MISCRecord : Record, IHaveMODL {
    # TESX
    public struct DATAField {
        public float Weight;
        public uint Value;
        public uint Unknown;

        public DATAField(Header r, int dataSize) {
            if (r.format == FormType.TES3) {
                Weight = r.readSingle();
                Value = r.readUInt32();
                Unknown = r.readUInt32();
                return;
            }
            Value = r.readUInt32();
            Weight = r.readSingle();
            Unknown = 0;
        }
    }

    public MODLGroup MODL { get; set; } # Model
    public STRVField FULL; # Item Name
    public DATAField DATA; # Misc Item Data
    public FILEField ICON; # Icon (optional)
    public RefField<SCPTRecord> SCRI; # Script FormID (optional)
    # TES3
    public RefField<ENCHRecord> ENAM; # enchantment ID

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID or FieldType.NAME: z = self.EDID = r.readSTRV(dataSize),
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize),
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize),
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize),
            case FieldType.FULL or FieldType.FNAM: z = self.FULL = r.readSTRV(dataSize),
            case FieldType.DATA or FieldType.MCDT: z = self.DATA = DATAField(r, dataSize),
            case FieldType.ICON or FieldType.ITEX: z = self.ICON = r.readFILE(dataSize),
            case FieldType.ENAM: z = self.ENAM = RefField<ENCHRecord>(r, dataSize),
            case FieldType.SCRI: z = self.SCRI = RefField<SCPTRecord>(r, dataSize),
            case _: z = Record.empty
        return z
# end::MISC[]

# NPC_.Non-Player Character - 3450 - tag::XXXX[]
public class NPC_Record : Record, IHaveMODL {
    [Flags]
    public enum NPC_Flags : uint {
        Female = 0x0001,
        Essential = 0x0002,
        Respawn = 0x0004,
        None = 0x0008,
        Autocalc = 0x0010,
        BloodSkel = 0x0400,
        BloodMetal = 0x0800,
    }

    public class NPDTField {
        public short Level;
        public byte Strength;
        public byte Intelligence;
        public byte Willpower;
        public byte Agility;
        public byte Speed;
        public byte Endurance;
        public byte Personality;
        public byte Luck;
        public byte[] Skills;
        public byte Reputation;
        public short Health;
        public short SpellPts;
        public short Fatigue;
        public byte Disposition;
        public byte FactionId;
        public byte Rank;
        public byte Unknown1;
        public int Gold;

        # 12 byte version
        #public short Level;
        #public byte Disposition;
        #public byte FactionId;
        #public byte Rank;
        #public byte Unknown1;
        public byte Unknown2;
        public byte Unknown3;
        #public int Gold;

        public NPDTField(Header r, int dataSize) {
            if (dataSize == 52) {
                Level = r.readInt16();
                Strength = r.ReadByte();
                Intelligence = r.ReadByte();
                Willpower = r.ReadByte();
                Agility = r.ReadByte();
                Speed = r.ReadByte();
                Endurance = r.ReadByte();
                Personality = r.ReadByte();
                Luck = r.ReadByte();
                Skills = r.ReadBytes(27);
                Reputation = r.ReadByte();
                Health = r.readInt16();
                SpellPts = r.readInt16();
                Fatigue = r.readInt16();
                Disposition = r.ReadByte();
                FactionId = r.ReadByte();
                Rank = r.ReadByte();
                Unknown1 = r.ReadByte();
                Gold = r.readInt32();
            }
            else {
                Level = r.readInt16();
                Disposition = r.ReadByte();
                FactionId = r.ReadByte();
                Rank = r.ReadByte();
                Unknown1 = r.ReadByte();
                Unknown2 = r.ReadByte();
                Unknown3 = r.ReadByte();
                Gold = r.readInt32();
            }
        }
    }

    public struct DODTField(Header r, int dataSize) {
        public float XPos = r.readSingle();
        public float YPos = r.readSingle();
        public float ZPos = r.readSingle();
        public float XRot = r.readSingle();
        public float YRot = r.readSingle();
        public float ZRot = r.readSingle();
    }

    public STRVField FULL; # NPC name
    public MODLGroup MODL { get; set; } # Animation
    public STRVField RNAM; # Race Name
    public STRVField ANAM; # Faction name
    public STRVField BNAM; # Head model
    public STRVField CNAM; # Class name
    public STRVField KNAM; # Hair model
    public NPDTField NPDT; # NPC Data
    public INTVField FLAG; # NPC Flags
    public List<CNTOField> NPCOs = List<CNTOField>(); # NPC item
    public List<STRVField> NPCSs = List<STRVField>(); # NPC spell
    public CREARecord.AIDTField AIDT; # AI data
    public CREARecord.AI_WField? AI_W; # AI
    public CREARecord.AI_TField? AI_T; # AI Travel
    public CREARecord.AI_FField? AI_F; # AI Follow
    public CREARecord.AI_FField? AI_E; # AI Escort
    public STRVField? CNDT; # Cell escort/follow to string (optional)
    public CREARecord.AI_AField? AI_A; # AI Activate
    public DODTField DODT; # Cell Travel Destination
    public STRVField DNAM; # Cell name for previous DODT, if interior
    public FLTVField? XSCL; # Scale (optional) Only present if the scale is not 1.0
    public RefField<SCPTRecord>? SCRI; # Unknown

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID or FieldType.NAME: z = self.EDID = r.readSTRV(dataSize),
            case FieldType.FULL or FieldType.FNAM: z = self.FULL = r.readSTRV(dataSize),
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize),
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize),
            case FieldType.RNAM: z = self.RNAM = r.readSTRV(dataSize),
            case FieldType.ANAM: z = self.ANAM = r.readSTRV(dataSize),
            case FieldType.BNAM: z = self.BNAM = r.readSTRV(dataSize),
            case FieldType.CNAM: z = self.CNAM = r.readSTRV(dataSize),
            case FieldType.KNAM: z = self.KNAM = r.readSTRV(dataSize),
            case FieldType.NPDT: z = self.NPDT = NPDTField(r, dataSize),
            case FieldType.FLAG: z = self.FLAG = r.readINTV(dataSize),
            case FieldType.NPCO: z = self.NPCOs.addX(CNTOField(r, dataSize)),
            case FieldType.NPCS: z = self.NPCSs.addX(r.ReadSTRV_ZPad(dataSize)),
            case FieldType.AIDT: z = self.AIDT = CREARecord.AIDTField(r, dataSize),
            case FieldType.AI_W: z = self.AI_W = CREARecord.AI_WField(r, dataSize),
            case FieldType.AI_T: z = self.AI_T = CREARecord.AI_TField(r, dataSize),
            case FieldType.AI_F: z = self.AI_F = CREARecord.AI_FField(r, dataSize),
            case FieldType.AI_E: z = self.AI_E = CREARecord.AI_FField(r, dataSize),
            case FieldType.CNDT: z = self.CNDT = r.readSTRV(dataSize),
            case FieldType.AI_A: z = self.AI_A = CREARecord.AI_AField(r, dataSize),
            case FieldType.DODT: z = self.DODT = DODTField(r, dataSize),
            case FieldType.DNAM: z = self.DNAM = r.readSTRV(dataSize),
            case FieldType.XSCL: z = self.XSCL = r.readS<FLTVField>(dataSize),
            case FieldType.SCRI: z = self.SCRI = RefField<SCPTRecord>(r, dataSize),
            case _: z = Record.empty
        return z
# end::NPC_[]

# PACK.AI Package - 0450 - tag::XXXX[]
public class PACKRecord : Record {
    public struct PKDTField {
        public ushort Flags;
        public byte Type;

        public PKDTField(Header r, int dataSize) {
            Flags = r.ReadUInt16();
            Type = r.ReadByte();
            r.Skip(dataSize - 3); # Unused
        }
    }

    public struct PLDTField(Header r, int dataSize) {
        public int Type = r.readInt32();
        public uint Target = r.readUInt32();
        public int Radius = r.readInt32();
    }

    public struct PSDTField(Header r, int dataSize) {
        public byte Month = r.ReadByte();
        public byte DayOfWeek = r.ReadByte();
        public byte Date = r.ReadByte();
        public sbyte Time = (sbyte)r.ReadByte();
        public int Duration = r.readInt32();
    }

    public struct PTDTField(Header r, int dataSize) {
        public int Type = r.readInt32();
        public uint Target = r.readUInt32();
        public int Count = r.readInt32();
    }

    public PKDTField PKDT; # General
    public PLDTField PLDT; # Location
    public PSDTField PSDT; # Schedule
    public PTDTField PTDT; # Target
    public List<SCPTRecord.CTDAField> CTDAs = []; # Conditions

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize),
            case FieldType.PKDT: z = self.PKDT = PKDTField(r, dataSize),
            case FieldType.PLDT: z = self.PLDT = PLDTField(r, dataSize),
            case FieldType.PSDT: z = self.PSDT = PSDTField(r, dataSize),
            case FieldType.PTDT: z = self.PTDT = PTDTField(r, dataSize),
            case FieldType.CTDA or FieldType.CTDT: z = self.CTDAs.addX(SCPTRecord.CTDAField(r, dataSize)),
            case _: z = Record.empty
        return z
# end::PACK[]

# PGRD.Path grid - 3400 - tag::XXXX[]
public class PGRDRecord : Record {
    public struct DATAField {
        public int X;
        public int Y;
        public short Granularity;
        public short PointCount;

        public DATAField(Header r, int dataSize) {
            if (r.Format != TES3) {
                X = Y = Granularity = 0;
                PointCount = r.readInt16();
                return;
            }
            X = r.readInt32();
            Y = r.readInt32();
            Granularity = r.readInt16();
            PointCount = r.readInt16();
        }
    }

    public struct PGRPField {
        public Vector3 Point;
        public byte Connections;

        public PGRPField(Header r, int dataSize) {
            Point = Vector3(r.readSingle(), r.readSingle(), r.readSingle());
            Connections = r.ReadByte();
            r.Skip(3); # Unused
        }
    }

    public struct PGRRField(Header r, int dataSize) {
        public short StartPointId = r.readInt16();
        public short EndPointId = r.readInt16();
    }

    public struct PGRIField {
        public short PointId;
        public Vector3 ForeignNode;

        public PGRIField(Header r, int dataSize) {
            PointId = r.readInt16();
            r.Skip(2); # Unused (can merge back)
            ForeignNode = Vector3(r.readSingle(), r.readSingle(), r.readSingle());
        }
    }

    public struct PGRLField {
        public Ref<REFRRecord> Reference;
        public short[] PointIds;

        public PGRLField(Header r, int dataSize) {
            Reference = Ref<REFRRecord>(r.readUInt32());
            PointIds = short[(dataSize - 4) >> 2];
            for (var i = 0; i < PointIds.Length; i++) {
                PointIds[i] = r.readInt16();
                r.Skip(2); # Unused (can merge back)
            }
        }
    }

    public DATAField DATA; # Number of nodes
    public PGRPField[] PGRPs;
    public UNKNField PGRC;
    public UNKNField PGAG;
    public PGRRField[] PGRRs; # Point-to-Point Connections
    public List<PGRLField> PGRLs; # Point-to-Reference Mappings
    public PGRIField[] PGRIs; # Inter-Cell Connections

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID or FieldType.NAME: z = self.EDID = r.readSTRV(dataSize),
            case FieldType.DATA: z = self.DATA = DATAField(r, dataSize),
            case FieldType.PGRP: z = self.PGRPs = [.. Enumerable.Range(0, dataSize >> 4).Select(x => PGRPField(r, 16))],
            case FieldType.PGRC: z = self.PGRC = r.ReadUNKN(dataSize),
            case FieldType.PGAG: z = self.PGAG = r.ReadUNKN(dataSize),
            case FieldType.PGRR: z = self.(PGRRs = [.. Enumerable.Range(0, dataSize >> 2).Select(x => PGRRField(r, 4))], r.Skip(dataSize % 4)),
            case FieldType.PGRL: z = self.(PGRLs ??= []).addX(PGRLField(r, dataSize)),
            case FieldType.PGRI: z = self.PGRIs = [.. Enumerable.Range(0, dataSize >> 4).Select(x => PGRIField(r, 16))],
            case _: z = Record.empty
        return z
# end::PGRD[]

# PROB.Probe - 3000 - tag::XXXX[]
public class PROBRecord : Record, IHaveMODL {
    public struct PBDTField(Header r, int dataSize) {
        public float Weight = r.readSingle();
        public int Value = r.readInt32();
        public float Quality = r.readSingle();
        public int Uses = r.readInt32();
    }

    public MODLGroup MODL { get; set; } # Model Name
    public STRVField FNAM; # Item Name
    public PBDTField PBDT; # Probe Data
    public FILEField ICON; # Inventory Icon
    public RefField<SCPTRecord> SCRI; # Script Name

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        if r.format == FormType.TES3:
            match type:
                case FieldType.NAME: z = self.EDID = r.readSTRV(dataSize),
                case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize),
                case FieldType.FNAM: z = self.FNAM = r.readSTRV(dataSize),
                case FieldType.PBDT: z = self.PBDT = PBDTField(r, dataSize),
                case FieldType.ITEX: z = self.ICON = r.readFILE(dataSize),
                case FieldType.SCRI: z = self.SCRI = RefField<SCPTRecord>(r, dataSize),
                case _: z = Record.empty
            return z
        return None
# end::PROB[]

# QUST.Quest - 0450 - tag::XXXX[]
public class QUSTRecord : Record {
    public struct DATAField(Header r, int dataSize) {
        public byte Flags = r.ReadByte();
        public byte Priority = r.ReadByte();
    }

    public STRVField FULL; # Item Name
    public FILEField ICON; # Icon
    public DATAField DATA; # Icon
    public RefField<SCPTRecord> SCRI; # Script Name
    public SCPTRecord.SCHRField SCHR; # Script Data
    public BYTVField SCDA; # Compiled Script
    public STRVField SCTX; # Script Source
    public List<RefField[Record]> SCROs = []; # Global variable reference

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize),
            case FieldType.FULL: z = self.FULL = r.readSTRV(dataSize),
            case FieldType.ICON: z = self.ICON = r.readFILE(dataSize),
            case FieldType.DATA: z = self.DATA = DATAField(r, dataSize),
            case FieldType.SCRI: z = self.SCRI = RefField<SCPTRecord>(r, dataSize),
            case FieldType.CTDA: z = self.r.Skip(dataSize),
            case FieldType.INDX: z = self.r.Skip(dataSize),
            case FieldType.QSDT: z = self.r.Skip(dataSize),
            case FieldType.CNAM: z = self.r.Skip(dataSize),
            case FieldType.QSTA: z = self.r.Skip(dataSize),
            case FieldType.SCHR: z = self.SCHR = SCPTRecord.SCHRField(r, dataSize),
            case FieldType.SCDA: z = self.SCDA = r.readBYTV(dataSize),
            case FieldType.SCTX: z = self.SCTX = r.readSTRV(dataSize),
            case FieldType.SCRO: z = self.SCROs.addX(RefField[Record](r, dataSize)),
            case _: z = Record.empty
        return z
# end::QUST[]

# RACE.Race_Creature type - 3450 - tag::XXXX[]
public class RACERecord : Record {
    # TESX
    public class DATAField {
        public enum RaceFlag : uint {
            Playable = 0x00000001,
            FaceGenHead = 0x00000002,
            Child = 0x00000004,
            TiltFrontBack = 0x00000008,
            TiltLeftRight = 0x00000010,
            NoShadow = 0x00000020,
            Swims = 0x00000040,
            Flies = 0x00000080,
            Walks = 0x00000100,
            Immobile = 0x00000200,
            NotPushable = 0x00000400,
            NoCombatInWater = 0x00000800,
            NoRotatingToHeadTrack = 0x00001000,
            DontShowBloodSpray = 0x00002000,
            DontShowBloodDecal = 0x00004000,
            UsesHeadTrackAnims = 0x00008000,
            SpellsAlignWMagicNode = 0x00010000,
            UseWorldRaycastsForFootIK = 0x00020000,
            AllowRagdollCollision = 0x00040000,
            RegenHPInCombat = 0x00080000,
            CantOpenDoors = 0x00100000,
            AllowPCDialogue = 0x00200000,
            NoKnockdowns = 0x00400000,
            AllowPickpocket = 0x00800000,
            AlwaysUseProxyController = 0x01000000,
            DontShowWeaponBlood = 0x02000000,
            OverlayHeadPartList = 0x04000000, #{> Only one can be active <}
            OverrideHeadPartList = 0x08000000, #{> Only one can be active <}
            CanPickupItems = 0x10000000,
            AllowMultipleMembraneShaders = 0x20000000,
            CanDualWield = 0x40000000,
            AvoidsRoads = 0x80000000,
        }

        public struct SkillBoost {
            public byte SkillId;
            public sbyte Bonus;

            public SkillBoost(Header r, int dataSize) {
                if (r.format == FormType.TES3) {
                    SkillId = (byte)r.readInt32();
                    Bonus = (sbyte)r.readInt32();
                    return;
                }
                SkillId = r.ReadByte();
                Bonus = r.ReadSByte();
            }
        }

        public struct RaceStats {
            public float Height;
            public float Weight;
            # Attributes;
            public byte Strength;
            public byte Intelligence;
            public byte Willpower;
            public byte Agility;
            public byte Speed;
            public byte Endurance;
            public byte Personality;
            public byte Luck;
        }

        public SkillBoost[] SkillBoosts = SkillBoost[7]; # Skill Boosts
        public RaceStats Male = new();
        public RaceStats Female = new();
        public uint Flags; # 1 = Playable 2 = Beast Race

        public DATAField(Header r, int dataSize) {
            if (r.format == FormType.TES3) {
                for (var i = 0; i < SkillBoosts.Length; i++) SkillBoosts[i] = SkillBoost(r, 8);
                Male.Strength = (byte)r.readInt32(); Female.Strength = (byte)r.readInt32();
                Male.Intelligence = (byte)r.readInt32(); Female.Intelligence = (byte)r.readInt32();
                Male.Willpower = (byte)r.readInt32(); Female.Willpower = (byte)r.readInt32();
                Male.Agility = (byte)r.readInt32(); Female.Agility = (byte)r.readInt32();
                Male.Speed = (byte)r.readInt32(); Female.Speed = (byte)r.readInt32();
                Male.Endurance = (byte)r.readInt32(); Female.Endurance = (byte)r.readInt32();
                Male.Personality = (byte)r.readInt32(); Female.Personality = (byte)r.readInt32();
                Male.Luck = (byte)r.readInt32(); Female.Luck = (byte)r.readInt32();
                Male.Height = r.readSingle(); Female.Height = r.readSingle();
                Male.Weight = r.readSingle(); Female.Weight = r.readSingle();
                Flags = r.readUInt32();
                return;
            }
            for (var i = 0; i < SkillBoosts.Length; i++) SkillBoosts[i] = SkillBoost(r, 2);
            r.readInt16(); # padding
            Male.Height = r.readSingle(); Female.Height = r.readSingle();
            Male.Weight = r.readSingle(); Female.Weight = r.readSingle();
            Flags = r.readUInt32();
        }

        public object ATTRField(Header r, int dataSize) {
            Male.Strength = r.ReadByte();
            Male.Intelligence = r.ReadByte();
            Male.Willpower = r.ReadByte();
            Male.Agility = r.ReadByte();
            Male.Speed = r.ReadByte();
            Male.Endurance = r.ReadByte();
            Male.Personality = r.ReadByte();
            Male.Luck = r.ReadByte();
            Female.Strength = r.ReadByte();
            Female.Intelligence = r.ReadByte();
            Female.Willpower = r.ReadByte();
            Female.Agility = r.ReadByte();
            Female.Speed = r.ReadByte();
            Female.Endurance = r.ReadByte();
            Female.Personality = r.ReadByte();
            Female.Luck = r.ReadByte();
            return this;
        }
    }

    # TES4
    public class FacePartGroup {
        public enum Indx : uint { Head, Ear_Male, Ear_Female, Mouth, Teeth_Lower, Teeth_Upper, Tongue, Eye_Left, Eye_Right, }
        public UI32Field INDX;
        public MODLGroup MODL;
        public FILEField ICON;
    }

    public class BodyPartGroup {
        public enum Indx : uint { UpperBody, LowerBody, Hand, Foot, Tail }
        public UI32Field INDX;
        public FILEField ICON;
    }

    public class BodyGroup {
        public FILEField MODL;
        public FLTVField MODB;
        public List<BodyPartGroup> BodyParts = [];
    }

    public STRVField FULL; # Race name
    public STRVField DESC; # Race description
    public List<STRVField> SPLOs = []; # NPCs: Special power/ability name
    # TESX
    public DATAField DATA; # RADT:DATA/ATTR: Race data/Base Attributes
    # TES4
    public Ref2Field<RACERecord> VNAM; # Voice
    public Ref2Field<HAIRRecord> DNAM; # Default Hair
    public BYTEField CNAM; # Default Hair Color
    public FLTVField PNAM; # FaceGen - Main clamp
    public FLTVField UNAM; # FaceGen - Face clamp
    public UNKNField XNAM; # Unknown
    #
    public List<RefField<HAIRRecord>> HNAMs = [];
    public List<RefField<EYESRecord>> ENAMs = [];
    public BYTVField FGGS; # FaceGen Geometry-Symmetric
    public BYTVField FGGA; # FaceGen Geometry-Asymmetric
    public BYTVField FGTS; # FaceGen Texture-Symmetric
    public UNKNField SNAM; # Unknown

    # Parts
    public List<FacePartGroup> FaceParts = [];
    public BodyGroup[] Bodys = [BodyGroup(), BodyGroup()];
    sbyte _nameState;
    sbyte _genderState;

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        if r.format == FormType.TES3:
            match type:
                case FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
                case FieldType.FNAM: z = self.FULL = r.readSTRV(dataSize)
                case FieldType.RADT: z = self.DATA = DATAField(r, dataSize)
                case FieldType.NPCS: z = self.SPLOs.addX(r.readSTRV(dataSize))
                case FieldType.DESC: z = self.DESC = r.readSTRV(dataSize)
                case _: z = Record.empty
        }
        : r.Format == TES4 ? _nameState switch {
            # preamble
            0 => type switch {
                FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
                FieldType.FULL: z = self.FULL = r.readSTRV(dataSize)
                FieldType.DESC: z = self.DESC = r.readSTRV(dataSize)
                FieldType.DATA: z = self.DATA = DATAField(r, dataSize)
                FieldType.SPLO: z = self.SPLOs.addX(r.readSTRV(dataSize))
                FieldType.VNAM: z = self.VNAM = Ref2Field<RACERecord>(r, dataSize)
                FieldType.DNAM: z = self.DNAM = Ref2Field<HAIRRecord>(r, dataSize)
                FieldType.CNAM: z = self.CNAM = r.readS<BYTEField>(dataSize)
                FieldType.PNAM: z = self.PNAM = r.readS<FLTVField>(dataSize)
                FieldType.UNAM: z = self.UNAM = r.readS<FLTVField>(dataSize)
                FieldType.XNAM: z = self.XNAM = r.ReadUNKN(dataSize)
                FieldType.ATTR: z = self.DATA.ATTRField(r, dataSize)
                FieldType.NAM0: z = self._nameState++
                z = Record.empty
            },
            # face data
            1 => type switch {
                FieldType.INDX: z = self.FaceParts.addX(FacePartGroup { INDX = r.readS<UI32Field>(dataSize) }),
                FieldType.MODL: z = self.FaceParts.last().MODL = MODLGroup(r, dataSize),
                FieldType.ICON: z = self.FaceParts.last().ICON = r.readFILE(dataSize),
                FieldType.MODB: z = self.FaceParts.last().MODL.MODBField(r, dataSize),
                FieldType.NAM1: z = self._nameState++,
                _ => Empty,
            },
            # body data
            2 => type switch {
                FieldType.MNAM: z = self._genderState = 0,
                FieldType.FNAM: z = self._genderState = 1,
                FieldType.MODL: z = self.Bodys[_genderState].MODL = r.readFILE(dataSize),
                FieldType.MODB: z = self.Bodys[_genderState].MODB = r.readS<FLTVField>(dataSize),
                FieldType.INDX: z = self.Bodys[_genderState].BodyParts.addX(BodyPartGroup { INDX = r.readS<UI32Field>(dataSize) }),
                FieldType.ICON: z = self.Bodys[_genderState].BodyParts.last().ICON = r.readFILE(dataSize),
                FieldType.HNAM: z = self.(_nameState++, HNAMs.AddRangeX(Enumerable.Range(0, dataSize >> 2).Select(x => RefField<HAIRRecord>(r, 4)))),
                _ => Empty,
            },
            # postamble
            3 => type switch {
                    FieldType.HNAM: z = self.HNAMs.AddRangeX(Enumerable.Range(0, dataSize >> 2).Select(x => RefField<HAIRRecord>(r, 4))),
                    FieldType.ENAM: z = self.ENAMs.AddRangeX(Enumerable.Range(0, dataSize >> 2).Select(x => RefField<EYESRecord>(r, 4))),
                    FieldType.FGGS: z = self.FGGS = r.readBYTV(dataSize),
                    FieldType.FGGA: z = self.FGGA = r.readBYTV(dataSize),
                    FieldType.FGTS: z = self.FGTS = r.readBYTV(dataSize),
                    FieldType.SNAM: z = self.SNAM = r.ReadUNKN(dataSize),
                    case _: z = Record.empty
                return z
            },
            _ => Empty,
        }
        : null;
# end::RACE[]

# REPA.Repair Item - 3000 - tag::XXXX[]
public class REPARecord : Record, IHaveMODL {
    public struct RIDTField(Header r, int dataSize) {
        public float Weight = r.readSingle();
        public int Value = r.readInt32();
        public int Uses = r.readInt32();
        public float Quality = r.readSingle();
    }

    public MODLGroup MODL { get; set; } # Model Name
    public STRVField FNAM; # Item Name
    public RIDTField RIDT; # Repair Data
    public FILEField ICON; # Inventory Icon
    public RefField<SCPTRecord> SCRI; # Script Name

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        if r.format == FormType.TES3:
            match type:
                case FieldType.NAME: z = self.EDID = r.readSTRV(dataSize),
                case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize),
                case FieldType.FNAM: z = self.FNAM = r.readSTRV(dataSize),
                case FieldType.RIDT: z = self.RIDT = RIDTField(r, dataSize),
                case FieldType.ITEX: z = self.ICON = r.readFILE(dataSize),
                case FieldType.SCRI: z = self.SCRI = RefField<SCPTRecord>(r, dataSize),
                case _: z = Record.empty
            return z
        return None
# end::REPA[]

# REFR.Placed Object - 0450 - tag::XXXX[]
public class REFRRecord : Record {
    public struct XTELField(Header r, int dataSize) {
        public Ref<REFRRecord> Door = new(r.readUInt32());
        public Vector3 Position = new(r.readSingle(), r.readSingle(), r.readSingle());
        public Vector3 Rotation = new(r.readSingle(), r.readSingle(), r.readSingle());
    }

    public struct DATAField(Header r, int dataSize) {
        public Vector3 Position = new(r.readSingle(), r.readSingle(), r.readSingle());
        public Vector3 Rotation = new(r.readSingle(), r.readSingle(), r.readSingle());
    }

    public struct XLOCField {
        public override readonly string ToString() => $"{Key}";
        public byte LockLevel;
        public Ref<KEYMRecord> Key;
        public byte Flags;

        public XLOCField(Header r, int dataSize) {
            LockLevel = r.ReadByte();
            r.Skip(3); # Unused
            Key = Ref<KEYMRecord>(r.readUInt32());
            if (dataSize == 16) r.Skip(4); # Unused
            Flags = r.ReadByte();
            r.Skip(3); # Unused
        }
    }

    public struct XESPField {
        public override readonly string ToString() => $"{Reference}";
        public Ref[Record] Reference;
        public byte Flags;

        public XESPField(Header r, int dataSize) {
            Reference = Ref[Record](r.readUInt32());
            Flags = r.ReadByte();
            r.Skip(3); # Unused
        }
    }

    public struct XSEDField {
        public override readonly string ToString() => $"{Seed}";
        public byte Seed;

        public XSEDField(Header r, int dataSize) {
            Seed = r.ReadByte();
            if (dataSize == 4) r.Skip(3); # Unused
        }
    }

    public class XMRKGroup {
        public override string ToString() => $"{FULL.Value}";
        public BYTEField FNAM; # Map Flags
        public STRVField FULL; # Name
        public BYTEField TNAM; # Type
    }

    public RefField[Record] NAME; # Base
    public XTELField? XTEL; # Teleport Destination (optional)
    public DATAField DATA; # Position/Rotation
    public XLOCField? XLOC; # Lock information (optional)
    public List<CELLRecord.XOWNGroup> XOWNs; # Ownership (optional)
    public XESPField? XESP; # Enable Parent (optional)
    public RefField[Record]? XTRG; # Target (optional)
    public XSEDField? XSED; # SpeedTree (optional)
    public BYTVField? XLOD; # Distant LOD Data (optional)
    public FLTVField? XCHG; # Charge (optional)
    public FLTVField? XHLT; # Health (optional)
    public RefField<CELLRecord>? XPCI; # Unused (optional)
    public IN32Field? XLCM; # Level Modifier (optional)
    public RefField<REFRRecord>? XRTM; # Unknown (optional)
    public UI32Field? XACT; # Action Flag (optional)
    public IN32Field? XCNT; # Count (optional)
    public List<XMRKGroup> XMRKs; # Ownership (optional)
    #public bool? ONAM; # Open by Default
    public BYTVField? XRGD; # Ragdoll Data (optional)
    public FLTVField? XSCL; # Scale (optional)
    public BYTEField? XSOL; # Contained Soul (optional)
    int _nextFull;

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize),
            case FieldType.NAME: z = self.NAME = RefField[Record](r, dataSize),
            case FieldType.XTEL: z = self.XTEL = XTELField(r, dataSize),
            case FieldType.DATA: z = self.DATA = DATAField(r, dataSize),
            case FieldType.XLOC: z = self.XLOC = XLOCField(r, dataSize),
            case FieldType.XOWN: z = self.(XOWNs ??= []).addX(CELLRecord.XOWNGroup { XOWN = RefField[Record](r, dataSize) }),
            case FieldType.XRNK: z = self.XOWNs.last().XRNK = r.readS<IN32Field>(dataSize),
            case FieldType.XGLB: z = self.XOWNs.last().XGLB = RefField[Record](r, dataSize),
            case FieldType.XESP: z = self.XESP = XESPField(r, dataSize),
            case FieldType.XTRG: z = self.XTRG = RefField[Record](r, dataSize),
            case FieldType.XSED: z = self.XSED = XSEDField(r, dataSize),
            case FieldType.XLOD: z = self.XLOD = r.readBYTV(dataSize),
            case FieldType.XCHG: z = self.XCHG = r.readS<FLTVField>(dataSize),
            case FieldType.XHLT: z = self.XCHG = r.readS<FLTVField>(dataSize),
            case FieldType.XPCI: z = self.(_nextFull = 1, XPCI = RefField<CELLRecord>(r, dataSize)),
            case FieldType.FULL: z = self._nextFull == 1 ? XPCI.Value.SetName(r.ReadFAString(dataSize)) : _nextFull == 2 ? XMRKs.last().FULL = r.readSTRV(dataSize) : _nextFull = 0,
            case FieldType.XLCM: z = self.XLCM = r.readS<IN32Field>(dataSize),
            case FieldType.XRTM: z = self.XRTM = RefField<REFRRecord>(r, dataSize),
            case FieldType.XACT: z = self.XACT = r.readS<UI32Field>(dataSize),
            case FieldType.XCNT: z = self.XCNT = r.readS<IN32Field>(dataSize),
            case FieldType.XMRK: z = self.(_nextFull = 2, (XMRKs ??= []).addX(XMRKGroup())),
            case FieldType.FNAM: z = self.XMRKs.last().FNAM = r.readS<BYTEField>(dataSize),
            case FieldType.TNAM: z = self.XMRKs.last().TNAM = r.readS<BYTEField>(dataSize),
            case FieldType.ONAM: z = self.true,
            case FieldType.XRGD: z = self.XRGD = r.readBYTV(dataSize),
            case FieldType.XSCL: z = self.XSCL = r.readS<FLTVField>(dataSize),
            case FieldType.XSOL: z = self.XSOL = r.readS<BYTEField>(dataSize),
            case _: z = Record.empty
        return z
# end::REFR[]

# REGN.Region - 3450 - tag::XXXX[]
public class REGNRecord : Record {
    # TESX
    public class RDATField {
        public enum REGNType : byte { Objects = 2, Weather, Map, Landscape, Grass, Sound }

        public uint Type;
        public REGNType Flags;
        public byte Priority;
        # groups
        public RDOTField[] RDOTs; # Objects
        public STRVField RDMP; # MapName
        public RDGSField[] RDGSs; # Grasses
        public UI32Field RDMD; # Music Type
        public RDSDField[] RDSDs; # Sounds
        public RDWTField[] RDWTs; # Weather Types

        public RDATField() { }
        public RDATField(Header r, int dataSize) {
            Type = r.readUInt32();
            Flags = (REGNType)r.ReadByte();
            Priority = r.ReadByte();
            r.Skip(2); # Unused
        }
    }

    public struct RDOTField {
        public override readonly string ToString() => $"{Object}";
        public Ref[Record] Object;
        public ushort ParentIdx;
        public float Density;
        public byte Clustering;
        public byte MinSlope; # (degrees)
        public byte MaxSlope; # (degrees)
        public byte Flags;
        public ushort RadiusWrtParent;
        public ushort Radius;
        public float MinHeight;
        public float MaxHeight;
        public float Sink;
        public float SinkVariance;
        public float SizeVariance;
        public Int3 AngleVariance;
        public ByteColor4 VertexShading; # RGB + Shading radius (0 - 200) %

        public RDOTField(Header r, int dataSize) {
            Object = Ref[Record](r.readUInt32());
            ParentIdx = r.ReadUInt16();
            r.Skip(2); # Unused
            Density = r.readSingle();
            Clustering = r.ReadByte();
            MinSlope = r.ReadByte();
            MaxSlope = r.ReadByte();
            Flags = r.ReadByte();
            RadiusWrtParent = r.ReadUInt16();
            Radius = r.ReadUInt16();
            MinHeight = r.readSingle();
            MaxHeight = r.readSingle();
            Sink = r.readSingle();
            SinkVariance = r.readSingle();
            SizeVariance = r.readSingle();
            AngleVariance = Int3(r.ReadUInt16(), r.ReadUInt16(), r.ReadUInt16());
            r.Skip(2); # Unused
            VertexShading = r.readS<ByteColor4>(dataSize);
        }
    }

    public struct RDGSField {
        public override readonly string ToString() => $"{Grass}";
        public Ref<GRASRecord> Grass;

        public RDGSField(Header r, int dataSize) {
            Grass = Ref<GRASRecord>(r.readUInt32());
            r.Skip(4); # Unused
        }
    }

    public struct RDSDField {
        public override readonly string ToString() => $"{Sound}";
        public Ref<SOUNRecord> Sound;
        public uint Flags;
        public uint Chance;

        public RDSDField(Header r, int dataSize) {
            if (r.format == FormType.TES3) {
                Sound = Ref<SOUNRecord>(r.ReadFAString(32));
                Flags = 0;
                Chance = r.ReadByte();
                return;
            }
            Sound = Ref<SOUNRecord>(r.readUInt32());
            Flags = r.readUInt32();
            Chance = r.readUInt32(); #: float with TES5
        }
    }

    public struct RDWTField(Header r, int dataSize) {
        public override readonly string ToString() => $"{Weather}";
        public static byte SizeOf(FormType format) => format == TES4 ? (byte)8 : (byte)12;
        public Ref<WTHRRecord> Weather = new(r.readUInt32());
        public uint Chance = r.readUInt32();
        public Ref<GLOBRecord> Global = r.Format == TES5 ? Ref<GLOBRecord>(r.readUInt32()) : Ref<GLOBRecord>();
    }

    # TES3
    public struct WEATField {
        public byte Clear;
        public byte Cloudy;
        public byte Foggy;
        public byte Overcast;
        public byte Rain;
        public byte Thunder;
        public byte Ash;
        public byte Blight;

        public WEATField(Header r, int dataSize) {
            Clear = r.ReadByte();
            Cloudy = r.ReadByte();
            Foggy = r.ReadByte();
            Overcast = r.ReadByte();
            Rain = r.ReadByte();
            Thunder = r.ReadByte();
            Ash = r.ReadByte();
            Blight = r.ReadByte();
            # v1.3 ESM files add 2 bytes to WEAT subrecords.
            if (dataSize == 10)
                r.Skip(2);
        }
    }

    # TES4
    public class RPLIField(Header r, int dataSize) {
        public uint EdgeFalloff = r.readUInt32(); # (World Units)
        public Vector2[] Points; # Region Point List Data

        public object RPLDField(Header r, int dataSize) {
            Points = Vector2[dataSize >> 3];
            for (var i = 0; i < Points.Length; i++) Points[i] = Vector2(r.readSingle(), r.readSingle());
            return Points;
        }
    }

    public STRVField ICON; # Icon / Sleep creature
    public RefField<WRLDRecord> WNAM; # Worldspace - Region name
    public CREFField RCLR; # Map Color (COLORREF)
    public List<RDATField> RDATs = []; # Region Data Entries / TES3: Sound Record (order determines the sound priority)
    # TES3
    public WEATField? WEAT; # Weather Data
    # TES4
    public List<RPLIField> RPLIs = []; # Region Areas

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID or FieldType.NAME: z = self.EDID = r.readSTRV(dataSize),
            case FieldType.WNAM or FieldType.FNAM: z = self.WNAM = RefField<WRLDRecord>(r, dataSize),
            case FieldType.WEAT: z = self.WEAT = WEATField(r, dataSize),#: TES3
            case FieldType.ICON or FieldType.BNAM: z = self.ICON = r.readSTRV(dataSize),
            case FieldType.RCLR or FieldType.CNAM: z = self.RCLR = r.readS<CREFField>(dataSize),
            case FieldType.SNAM: z = self.RDATs.addX(RDATField { RDSDs = [RDSDField(r, dataSize)] }),
            case FieldType.RPLI: z = self.RPLIs.addX(RPLIField(r, dataSize)),
            case FieldType.RPLD: z = self.RPLIs.last().RPLDField(r, dataSize),
            case FieldType.RDAT: z = self.RDATs.addX(RDATField(r, dataSize)),
            case FieldType.RDOT: z = self.RDATs.last().RDOTs = [.. Enumerable.Range(0, dataSize / 52).Select(x => RDOTField(r, dataSize))],
            case FieldType.RDMP: z = self.RDATs.last().RDMP = r.readSTRV(dataSize),
            case FieldType.RDGS: z = self.RDATs.last().RDGSs = [.. Enumerable.Range(0, dataSize / 8).Select(x => RDGSField(r, dataSize))],
            case FieldType.RDMD: z = self.RDATs.last().RDMD = r.readS<UI32Field>(dataSize),
            case FieldType.RDSD: z = self.RDATs.last().RDSDs = [.. Enumerable.Range(0, dataSize / 12).Select(x => RDSDField(r, dataSize))],
            case FieldType.RDWT: z = self.RDATs.last().RDWTs = [.. Enumerable.Range(0, dataSize / RDWTField.SizeOf(r.Format)).Select(x => RDWTField(r, dataSize))],
            case _: z = Record.empty
        return z
# end::REGN[]

# ROAD.Road - 0400 - tag::XXXX[]
public class ROADRecord : Record {
    public PGRDRecord.PGRPField[] PGRPs;
    public UNKNField PGRR;

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.PGRP: z = self.PGRPs = [.. Enumerable.Range(0, dataSize >> 4).Select(x => PGRDRecord.PGRPField(r, dataSize))],
            case FieldType.PGRR: z = self.PGRR = r.ReadUNKN(dataSize),
            case _: z = Record.empty
        return z
# end::ROAD[]


# SBSP.Subspace - 0400 - tag::XXXX[]
public class SBSPRecord : Record {
    public struct DNAMField(Header r, int dataSize) {
        public float X = r.readSingle(); # X dimension
        public float Y = r.readSingle(); # Y dimension
        public float Z = r.readSingle(); # Z dimension
    }

    public DNAMField DNAM;

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize),
            case FieldType.DNAM: z = self.DNAM = DNAMField(r, dataSize),
            case _: z = Record.empty
        return z
# end::SBSP[]

# SCPT.Script - 3400 - tag::XXXX[]
class SCPTRecord(Record):
    # TESX
    class CTDAField:
        class INFOType(Enum): Nothing = 0; Function = 1; Global = 2; Local = 3; Journal = 4; Item = 5; Dead = 6; NotId = 7; NotFaction = 8; NotClass = 9; NotRace = 10; NotCell = 11; NotLocal = 12

        # TES3: 0 = [=], 1 = [!=], 2 = [>], 3 = [>=], 4 = [<], 5 = [<=]
        # TES4: 0 = [=], 2 = [!=], 4 = [>], 6 = [>=], 8 = [<], 10 = [<=]
        public byte CompareOp;
        # (00-71) - sX = Global/Local/Not Local types, JX = Journal type, IX = Item Type, DX = Dead Type, XX = Not ID Type, FX = Not Faction, CX = Not Class, RX = Not Race, LX = Not Cell
        public string FunctionId;
        # TES3
        public byte Index; # (0-5)
        public byte Type;
        # Except for the function type, this is the ID for the global/local/etc. Is not nessecarily NULL terminated.The function type SCVR sub-record has
        public string Name;
        # TES4
        public float ComparisonValue;
        public int Parameter1; # Parameter #1
        public int Parameter2; # Parameter #2

        public CTDAField(Header r, int dataSize) {
            if (r.format == FormType.TES3) {
                Index = r.ReadByte();
                Type = r.ReadByte();
                FunctionId = r.ReadFAString(2);
                CompareOp = (byte)(r.ReadByte() << 1);
                Name = r.ReadFAString(dataSize - 5);
                ComparisonValue = Parameter1 = Parameter2 = 0;
                return;
            }
            CompareOp = r.ReadByte();
            r.Skip(3); # Unused
            ComparisonValue = r.readSingle();
            FunctionId = r.ReadFAString(4);
            Parameter1 = r.readInt32();
            Parameter2 = r.readInt32();
            if (dataSize != 24) r.Skip(4); # Unused
            Index = Type = 0;
            Name = null;
        }
    }

    # TES3
    public class SCHDField(Header r, int dataSize) {
        public override string ToString() => $"{Name}";
        public string Name = r.ReadFAString(32);
        public int NumShorts = r.readInt32();
        public int NumLongs = r.readInt32();
        public int NumFloats = r.readInt32();
        public int ScriptDataSize = r.readInt32();
        public int LocalVarSize = r.readInt32();
        public string[] Variables = null;
        public object SCVRField(Header r, int dataSize) => Variables = r.ReadZAStringList(dataSize).ToArray();
    }

    # TES4
    public struct SCHRField {
        public override readonly string ToString() => $"{RefCount}";
        public uint RefCount;
        public uint CompiledSize;
        public uint VariableCount;
        public uint Type; # 0x000 = Object, 0x001 = Quest, 0x100 = Magic Effect

        public SCHRField(Header r, int dataSize) {
            r.Skip(4); # Unused
            RefCount = r.readUInt32();
            CompiledSize = r.readUInt32();
            VariableCount = r.readUInt32();
            Type = r.readUInt32();
            if (dataSize == 20) return;
            r.Skip(dataSize - 20);
        }
    }

    public class SLSDField {
        public override string ToString() => $"{Idx}:{VariableName}";
        public uint Idx;
        public uint Type;
        public string VariableName;

        public SLSDField(Header r, int dataSize) {
            Idx = r.readUInt32();
            r.readUInt32(); # Unknown
            r.readUInt32(); # Unknown
            r.readUInt32(); # Unknown
            Type = r.readUInt32();
            r.readUInt32(); # Unknown
                            # SCVRField
            VariableName = null;
        }
        public object SCVRField(Header r, int dataSize) => VariableName = r.ReadFUString(dataSize);
    }

    public override string ToString() => $"SCPT: {EDID.Value ?? SCHD.Name}";
    public BYTVField SCDA; # Compiled Script
    public STRVField SCTX; # Script Source
                           # TES3
    public SCHDField SCHD; # Script Data
                           # TES4
    public SCHRField SCHR; # Script Data
    public List<SLSDField> SLSDs = []; # Variable data
    public List<SLSDField> SCRVs = []; # Ref variable data (one for each ref declared)
    public List<RefField[Record]> SCROs = []; # Global variable reference

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize),
            case FieldType.SCHD: z = self.SCHD = SCHDField(r, dataSize),
            case FieldType.SCVR: z = self.r.Format != TES3 ? SLSDs.last().SCVRField(r, dataSize) : SCHD.SCVRField(r, dataSize),
            case FieldType.SCDA or FieldType.SCDT: z = self.SCDA = r.readBYTV(dataSize),
            case FieldType.SCTX: z = self.SCTX = r.readSTRV(dataSize),
            # TES4
            case FieldType.SCHR: z = self.SCHR = SCHRField(r, dataSize),
            case FieldType.SLSD: z = self.SLSDs.addX(SLSDField(r, dataSize)),
            case FieldType.SCRO: z = self.SCROs.addX(RefField[Record](r, dataSize)),
            case FieldType.SCRV: z = self.SCRVs.addX(this.Then(r.readUInt32(), idx => SLSDs.Single(x => x.Idx == idx))),
            case _: z = Record.empty
        return z
    }
# end::SCPT[]

# SGST.Sigil Stone - 0400 - tag::XXXX[]
public class SGSTRecord : Record, IHaveMODL {
    public struct DATAField(Header r, int dataSize) {
        public byte Uses = r.ReadByte();
        public int Value = r.readInt32();
        public float Weight = r.readSingle();
    }

    public MODLGroup MODL { get; set; } # Model
    public STRVField FULL; # Item Name
    public DATAField DATA; # Sigil Stone Data
    public FILEField ICON; # Icon
    public RefField<SCPTRecord>? SCRI; # Script (optional)
    public List<ENCHRecord.EFITField> EFITs = []; # Effect Data
    public List<ENCHRecord.SCITField> SCITs = []; # Script Effect Data

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize),
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize),
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize),
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize),
            case FieldType.FULL: z = self.SCITs.Count == 0 ? FULL = r.readSTRV(dataSize) : SCITs.last().FULLField(r, dataSize),
            case FieldType.DATA: z = self.DATA = DATAField(r, dataSize),
            case FieldType.ICON: z = self.ICON = r.readFILE(dataSize),
            case FieldType.SCRI: z = self.SCRI = RefField<SCPTRecord>(r, dataSize),
            case FieldType.EFID: z = self.r.Skip(dataSize),
            case FieldType.EFIT: z = self.EFITs.addX(ENCHRecord.EFITField(r, dataSize)),
            case FieldType.SCIT: z = self.SCITs.addX(ENCHRecord.SCITField(r, dataSize)),
            case _: z = Record.empty
        return z
# end::SGST[]

# SKIL.Skill - 3450 - tag::XXXX[]
public class SKILRecord : Record {
    # TESX
    public struct DATAField {
        public int Action;
        public int Attribute;
        public uint Specialization; # 0 = Combat, 1 = Magic, 2 = Stealth
        public float[] UseValue; # The use types for each skill are hard-coded.

        public DATAField(Header r, int dataSize) {
            Action = r.format == FormType.TES3 ? 0 : r.readInt32();
            Attribute = r.readInt32();
            Specialization = r.readUInt32();
            UseValue = float[r.format == FormType.TES3 ? 4 : 2];
            for (var i = 0; i < UseValue.Length; i++) UseValue[i] = r.readSingle();
        }
    }

    public override string ToString() => $"SKIL: {INDX.Value}:{EDID.Value}";
    public IN32Field INDX; # Skill ID
    public DATAField DATA; # Skill Data
    public STRVField DESC; # Skill description
    # TES4
    public FILEField ICON; # Icon
    public STRVField ANAM; # Apprentice Text
    public STRVField JNAM; # Journeyman Text
    public STRVField ENAM; # Expert Text
    public STRVField MNAM; # Master Text

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize),
            case FieldType.INDX: z = self.INDX = r.readS<IN32Field>(dataSize),
            case FieldType.DATA or FieldType.SKDT: z = self.DATA = DATAField(r, dataSize),
            case FieldType.DESC: z = self.DESC = r.readSTRV(dataSize),
            case FieldType.ICON: z = self.ICON = r.readFILE(dataSize),
            case FieldType.ANAM: z = self.ANAM = r.readSTRV(dataSize),
            case FieldType.JNAM: z = self.JNAM = r.readSTRV(dataSize),
            case FieldType.ENAM: z = self.ENAM = r.readSTRV(dataSize),
            case FieldType.MNAM: z = self.MNAM = r.readSTRV(dataSize),
            case _: z = Record.empty
        return z
# end::SKIL[]

# SLGM.Soul Gem - 0450 - tag::XXXX[]
public class SLGMRecord : Record, IHaveMODL {
    public struct DATAField(Header r, int dataSize) {
        public int Value = r.readInt32();
        public float Weight = r.readSingle();
    }

    public MODLGroup MODL { get; set; } # Model
    public STRVField FULL; # Item Name
    public RefField<SCPTRecord> SCRI; # Script (optional)
    public DATAField DATA; # Type of soul contained in the gem
    public FILEField ICON; # Icon (optional)
    public BYTEField SOUL; # Type of soul contained in the gem
    public BYTEField SLCP; # Soul gem maximum capacity

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize),
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize),
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize),
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize),
            case FieldType.FULL: z = self.FULL = r.readSTRV(dataSize),
            case FieldType.SCRI: z = self.SCRI = RefField<SCPTRecord>(r, dataSize),
            case FieldType.DATA: z = self.DATA = DATAField(r, dataSize),
            case FieldType.ICON: z = self.ICON = r.readFILE(dataSize),
            case FieldType.SOUL: z = self.SOUL = r.readS<BYTEField>(dataSize),
            case FieldType.SLCP: z = self.SLCP = r.readS<BYTEField>(dataSize),
            case _: z = Record.empty
        return z
# end::SLGM[]

# SNDG.Sound Generator - 3000 - tag::XXXX[]
public class SNDGRecord : Record {
    public enum SNDGType : uint {
        LeftFoot = 0,
        RightFoot = 1,
        SwimLeft = 2,
        SwimRight = 3,
        Moan = 4,
        Roar = 5,
        Scream = 6,
        Land = 7,
    }

    public IN32Field DATA; # Sound Type Data
    public STRVField SNAM; # Sound ID
    public STRVField? CNAM; # Creature name (optional)

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        if r.format == FormType.TES3:
            match type:
                case FieldType.NAME: z = self.EDID = r.readSTRV(dataSize),
                case FieldType.DATA: z = self.DATA = r.readS<IN32Field>(dataSize),
                case FieldType.SNAM: z = self.SNAM = r.readSTRV(dataSize),
                case FieldType.CNAM: z = self.CNAM = r.readSTRV(dataSize),
                case _: z = Record.empty
            return z
        return None
# end::SNDG[]

# SNDR.Sound Reference - 0050 - tag::XXXX[]
public class SNDRRecord : Record {
    public CREFField CNAM; # RGB color

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize),
            case FieldType.CNAM: z = self.CNAM = r.readS<CREFField>(dataSize),
            case _: z = Record.empty
        return z
# end::SNDR[]

# SOUN.Sound - 3450 - tag::XXXX[]
public class SOUNRecord : Record {
    [Flags]
    public enum SOUNFlags : ushort {
        RandomFrequencyShift = 0x0001,
        PlayAtRandom = 0x0002,
        EnvironmentIgnored = 0x0004,
        RandomLocation = 0x0008,
        Loop = 0x0010,
        MenuSound = 0x0020,
        _2D = 0x0040,
        _360LFE = 0x0080,
    }

    # TESX
    public class DATAField {
        public byte Volume; # (0=0.00, 255=1.00)
        public byte MinRange; # Minimum attenuation distance
        public byte MaxRange; # Maximum attenuation distance
        # Bethesda4
        public sbyte FrequencyAdjustment; # Frequency adjustment %
        public ushort Flags; # Flags
        public ushort StaticAttenuation; # Static Attenuation (db)
        public byte StopTime; # Stop time
        public byte StartTime; # Start time

        public DATAField(Header r, int dataSize) {
            Volume = r.format == FormType.TES3 ? r.ReadByte() : (byte)0;
            MinRange = r.ReadByte();
            MaxRange = r.ReadByte();
            if (r.format == FormType.TES3) return;
            FrequencyAdjustment = r.ReadSByte();
            r.ReadByte(); # Unused
            Flags = r.ReadUInt16();
            r.ReadUInt16(); # Unused
            if (dataSize == 8) return;
            StaticAttenuation = r.ReadUInt16();
            StopTime = r.ReadByte();
            StartTime = r.ReadByte();
        }
    }

    public FILEField FNAM; # Sound Filename (relative to Sounds\)
    public DATAField DATA; # Sound Data

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID or FieldType.NAME: z = self.EDID = r.readSTRV(dataSize),
            case FieldType.FNAM: z = self.FNAM = r.readFILE(dataSize),
            case FieldType.SNDX: z = self.DATA = DATAField(r, dataSize),
            case FieldType.SNDD: z = self.DATA = DATAField(r, dataSize),
            case FieldType.DATA: z = self.DATA = DATAField(r, dataSize),
            case _: z = Record.empty
        return z
# end::SOUN[]

# SPEL.Spell - 3450 - tag::XXXX[]
public class SPELRecord : Record {
    # TESX
    public struct SPITField(Header r, int dataSize) {
        public override readonly string ToString() => $"{Type}";
        # TES3: 0 = Spell, 1 = Ability, 2 = Blight, 3 = Disease, 4 = Curse, 5 = Power
        # TES4: 0 = Spell, 1 = Disease, 2 = Power, 3 = Lesser Power, 4 = Ability, 5 = Poison
        public uint Type = r.readUInt32();
        public int SpellCost = r.readInt32();
        public uint Flags = r.readUInt32(); # 0x0001 = AutoCalc, 0x0002 = PC Start, 0x0004 = Always Succeeds
        # TES4
        public int SpellLevel = r.Format != TES3 ? r.readInt32() : 0;
    }

    public STRVField FULL; # Spell name
    public SPITField SPIT; # Spell data
    public List<ENCHRecord.EFITField> EFITs = []; # Effect Data
    # TES4
    public List<ENCHRecord.SCITField> SCITs = []; # Script effect data

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID or FieldType.NAME: z = self.EDID = r.readSTRV(dataSize),
            case FieldType.FULL: z = self.SCITs.Count == 0 ? FULL = r.readSTRV(dataSize) : SCITs.last().FULLField(r, dataSize),
            case FieldType.FNAM: z = self.FULL = r.readSTRV(dataSize),
            case FieldType.SPIT or FieldType.SPDT: z = self.SPIT = SPITField(r, dataSize),
            case FieldType.EFID: z = self.r.Skip(dataSize),
            case FieldType.EFIT or FieldType.ENAM: z = self.EFITs.addX(ENCHRecord.EFITField(r, dataSize)),
            case FieldType.SCIT: z = self.SCITs.addX(ENCHRecord.SCITField(r, dataSize)),
            case _: z = Record.empty
        return z
# end::SPEL[]

# SSCR.Start Script - 3000 - tag::XXXX[]
class SSCRRecord(Record)
    public STRVField DATA; # Digits

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        if r.format == FormType.TES3:
            match type:
                case FieldType.NAME: z = self.EDID = r.readSTRV(dataSize),
                case FieldType.DATA: z = self.DATA = r.readSTRV(dataSize),
                case _: z = Record.empty
            return z
        return None
# end::SSCR[]

# STAT.Static - 3450 - tag::STAT[]
class STATRecord(Record, IHaveMODL):
    public MODLGroup MODL { get; set; } # Model

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID or FieldType.NAME: z = self.EDID = r.readSTRV(dataSize),
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize),
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize),
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize),
            case _: z = Record.empty
        return z
# end::STAT[]

# TES3.Plugin Info - 3000 - tag::TES3[]
class TES3Record(Record):
    public struct HEDRField(Header r, int dataSize) {
        public float Version = r.readSingle();
        public uint FileType = r.readUInt32();
        public string CompanyName = r.ReadFAString(32);
        public string FileDescription = r.ReadFAString(256);
        public uint NumRecords = r.readUInt32();
    }

    public HEDRField HEDR;
    public List<STRVField> MASTs;
    public List<INTVField> DATAs;

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.HEDR: z = self.HEDR = HEDRField(r, dataSize),
            case FieldType.MAST: z = self.(MASTs ??= []).addX(r.readSTRV(dataSize)),
            case FieldType.DATA: z = self.(DATAs ??= []).addX(r.readINTV(dataSize)),
            case _: z = Record.empty
        return z
# end::TES3[]

# TES4.Plugin Info - 0450 - tag::TES4[]
public unsafe class TES4Record : Record {
    public struct HEDRField {
        public static (string, int) Struct = ("<fiI", 12);
        public float Version;
        public int NumRecords; # Number of records and groups (not including TES4 record itself).
        public uint NextObjectId; # Next available object ID.
    }

    public HEDRField HEDR;
    public STRVField? CNAM; # author (Optional)
    public STRVField? SNAM; # description (Optional)
    public List<STRVField> MASTs; # master
    public List<INTVField> DATAs; # fileSize
    public UNKNField? ONAM; # overrides (Optional)
    public IN32Field INTV; # unknown
    public IN32Field? INCC; # unknown (Optional)
    # TES5
    public UNKNField? TNAM; # overrides (Optional)

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.HEDR: z = self.HEDR = r.readS<HEDRField>(dataSize),
            case FieldType.OFST: z = self.r.Skip(dataSize),
            case FieldType.DELE: z = self.r.Skip(dataSize),
            case FieldType.CNAM: z = self.CNAM = r.readSTRV(dataSize),
            case FieldType.SNAM: z = self.SNAM = r.readSTRV(dataSize),
            case FieldType.MAST: z = self.(MASTs ??= []).addX(r.readSTRV(dataSize)),
            case FieldType.DATA: z = self.(DATAs ??= []).addX(r.readINTV(dataSize)),
            case FieldType.ONAM: z = self.ONAM = r.ReadUNKN(dataSize),
            case FieldType.INTV: z = self.INTV = r.readS<IN32Field>(dataSize),
            case FieldType.INCC: z = self.INCC = r.readS<IN32Field>(dataSize),
            # TES5
            case FieldType.TNAM: z = self.TNAM = r.ReadUNKN(dataSize),
            case _: z = Record.empty
        return z
# end::TES4[]

# TREE.Tree - 0450 - tag::TREE[]
public class TREERecord : Record, IHaveMODL {
    public struct SNAMField {
        public int[] Values;

        public SNAMField(Header r, int dataSize) {
            Values = int[dataSize >> 2];
            for (var i = 0; i < Values.Length; i++)
                Values[i] = r.readInt32();
        }
    }

    public struct CNAMField(Header r, int dataSize) {
        public float LeafCurvature = r.readSingle();
        public float MinimumLeafAngle = r.readSingle();
        public float MaximumLeafAngle = r.readSingle();
        public float BranchDimmingValue = r.readSingle();
        public float LeafDimmingValue = r.readSingle();
        public int ShadowRadius = r.readInt32();
        public float RockSpeed = r.readSingle();
        public float RustleSpeed = r.readSingle();
    }

    public struct BNAMField(Header r, int dataSize) {
        public float Width = r.readSingle();
        public float Height = r.readSingle();
    }

    public MODLGroup MODL { get; set; } # Model
    public FILEField ICON; # Leaf Texture
    public SNAMField SNAM; # SpeedTree Seeds, array of ints
    public CNAMField CNAM; # Tree Parameters
    public BNAMField BNAM; # Billboard Dimensions

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize),
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize),
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize),
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize),
            case FieldType.ICON: z = self.ICON = r.readFILE(dataSize),
            case FieldType.SNAM: z = self.SNAM = SNAMField(r, dataSize),
            case FieldType.CNAM: z = self.CNAM = CNAMField(r, dataSize),
            case FieldType.BNAM: z = self.BNAM = BNAMField(r, dataSize),
            case _: z = Record.empty
        return z
# end::TREE[]

# WATR.Water Type - 0450 - tag::WATR[]
public class WATRRecord : Record {
    public class DATAField {
        public float WindVelocity;
        public float WindDirection;
        public float WaveAmplitude;
        public float WaveFrequency;
        public float SunPower;
        public float ReflectivityAmount;
        public float FresnelAmount;
        public float ScrollXSpeed;
        public float ScrollYSpeed;
        public float FogDistance_NearPlane;
        public float FogDistance_FarPlane;
        public ByteColor4 ShallowColor;
        public ByteColor4 DeepColor;
        public ByteColor4 ReflectionColor;
        public byte TextureBlend;
        public float RainSimulator_Force;
        public float RainSimulator_Velocity;
        public float RainSimulator_Falloff;
        public float RainSimulator_Dampner;
        public float RainSimulator_StartingSize;
        public float DisplacementSimulator_Force;
        public float DisplacementSimulator_Velocity;
        public float DisplacementSimulator_Falloff;
        public float DisplacementSimulator_Dampner;
        public float DisplacementSimulator_StartingSize;
        public ushort Damage;

        public DATAField(Header r, int dataSize) {
            if (dataSize != 102 && dataSize != 86 && dataSize != 62 && dataSize != 42 && dataSize != 2) WindVelocity = 1;
            if (dataSize == 2) { Damage = r.ReadUInt16(); return; }
            WindVelocity = r.readSingle();
            WindDirection = r.readSingle();
            WaveAmplitude = r.readSingle();
            WaveFrequency = r.readSingle();
            SunPower = r.readSingle();
            ReflectivityAmount = r.readSingle();
            FresnelAmount = r.readSingle();
            ScrollXSpeed = r.readSingle();
            ScrollYSpeed = r.readSingle();
            FogDistance_NearPlane = r.readSingle();
            if (dataSize == 42) { Damage = r.ReadUInt16(); return; }
            FogDistance_FarPlane = r.readSingle();
            ShallowColor = r.readS<ByteColor4>(dataSize);
            DeepColor = r.readS<ByteColor4>(dataSize);
            ReflectionColor = r.readS<ByteColor4>(dataSize);
            TextureBlend = r.ReadByte();
            r.Skip(3); # Unused
            if (dataSize == 62) { Damage = r.ReadUInt16(); return; }
            RainSimulator_Force = r.readSingle();
            RainSimulator_Velocity = r.readSingle();
            RainSimulator_Falloff = r.readSingle();
            RainSimulator_Dampner = r.readSingle();
            RainSimulator_StartingSize = r.readSingle();
            DisplacementSimulator_Force = r.readSingle();
            if (dataSize == 86) {
                #DisplacementSimulator_Velocity = DisplacementSimulator_Falloff = DisplacementSimulator_Dampner = DisplacementSimulator_StartingSize = 0F;
                Damage = r.ReadUInt16();
                return;
            }
            DisplacementSimulator_Velocity = r.readSingle();
            DisplacementSimulator_Falloff = r.readSingle();
            DisplacementSimulator_Dampner = r.readSingle();
            DisplacementSimulator_StartingSize = r.readSingle();
            Damage = r.ReadUInt16();
        }
    }

    class GNAMField:
        def __init__(self, r: Header, dataSize: int):
        self.daytime: Ref[WATRRecord] = Ref(r.readUInt32());
        self.nighttime: Ref[WATRRecord] = Ref(r.readUInt32());
        self.underwater: Ref[WATRRecord] = Ref(r.readUInt32());
    }

    public STRVField TNAM; # Texture
    public BYTEField ANAM; # Opacity
    public BYTEField FNAM; # Flags
    public STRVField MNAM; # Material ID
    public RefField<SOUNRecord> SNAM; # Sound
    public DATAField DATA; # DATA
    public GNAMField GNAM; # GNAM

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize),
            case FieldType.TNAM: z = self.TNAM = r.readSTRV(dataSize),
            case FieldType.ANAM: z = self.ANAM = r.readS<BYTEField>(dataSize),
            case FieldType.FNAM: z = self.FNAM = r.readS<BYTEField>(dataSize),
            case FieldType.MNAM: z = self.MNAM = r.readSTRV(dataSize),
            case FieldType.SNAM: z = self.SNAM = RefField<SOUNRecord>(r, dataSize),
            case FieldType.DATA: z = self.DATA = DATAField(r, dataSize),
            case FieldType.GNAM: z = self.GNAM = GNAMField(r, dataSize),
            case _: z = Record.empty
        return z
# end::WATR[]

# WEAP.Weapon - 3450 - tag::WEAP[]
public class WEAPRecord : Record, IHaveMODL {
    public struct DATAField {
        public enum WEAPType { ShortBladeOneHand = 0, LongBladeOneHand, LongBladeTwoClose, BluntOneHand, BluntTwoClose, BluntTwoWide, SpearTwoWide, AxeOneHand, AxeTwoHand, MarksmanBow, MarksmanCrossbow, MarksmanThrown, Arrow, Bolt, }

        public float Weight;
        public int Value;
        public ushort Type;
        public short Health;
        public float Speed;
        public float Reach;
        public short Damage; #: EnchantPts;
        public byte ChopMin;
        public byte ChopMax;
        public byte SlashMin;
        public byte SlashMax;
        public byte ThrustMin;
        public byte ThrustMax;
        public int Flags; # 0 = ?, 1 = Ignore Normal Weapon Resistance?

        public DATAField(Header r, int dataSize) {
            if (r.format == FormType.TES3) {
                Weight = r.readSingle();
                Value = r.readInt32();
                Type = r.ReadUInt16();
                Health = r.readInt16();
                Speed = r.readSingle();
                Reach = r.readSingle();
                Damage = r.readInt16();
                ChopMin = r.ReadByte();
                ChopMax = r.ReadByte();
                SlashMin = r.ReadByte();
                SlashMax = r.ReadByte();
                ThrustMin = r.ReadByte();
                ThrustMax = r.ReadByte();
                Flags = r.readInt32();
                return;
            }
            Type = (ushort)r.readUInt32();
            Speed = r.readSingle();
            Reach = r.readSingle();
            Flags = r.readInt32();
            Value = r.readInt32();
            Health = (short)r.readInt32();
            Weight = r.readSingle();
            Damage = r.readInt16();
            ChopMin = ChopMax = SlashMin = SlashMax = ThrustMin = ThrustMax = 0;
        }
    }

    public MODLGroup MODL { get; set; } # Model
    public STRVField FULL; # Item Name
    public DATAField DATA; # Weapon Data
    public FILEField ICON; # Male Icon (optional)
    public RefField<ENCHRecord> ENAM; # Enchantment ID
    public RefField<SCPTRecord> SCRI; # Script (optional)
                                      # TES4
    public IN16Field? ANAM; # Enchantment points (optional)

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID or FieldType.NAME: z = self.EDID = r.readSTRV(dataSize),
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize),
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize),
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize),
            case FieldType.FULL or FieldType.FNAM: z = self.FULL = r.readSTRV(dataSize),
            case FieldType.DATA or FieldType.WPDT: z = self.DATA = DATAField(r, dataSize),
            case FieldType.ICON or FieldType.ITEX: z = self.ICON = r.readFILE(dataSize),
            case FieldType.ENAM: z = self.ENAM = RefField<ENCHRecord>(r, dataSize),
            case FieldType.SCRI: z = self.SCRI = RefField<SCPTRecord>(r, dataSize),
            case FieldType.ANAM: z = self.ANAM = r.readS<IN16Field>(dataSize),
            case _: z = Record.empty
        return z
# end::WEAP[]

# WRLD.Worldspace - 0450 - tag::WRLD[]
public unsafe class WRLDRecord : Record {
    public struct MNAMField {
        public static (string, int) Struct = ($"<2i4h", 16);
        public Int2 UsableDimensions;
        # Cell Coordinates
        public short NWCell_X;
        public short NWCell_Y;
        public short SECell_X;
        public short SECell_Y;
    }

    public struct NAM0Field(Header r, int dataSize) {
        #public static (string, int) Struct = ("<2f", 8);
        #public static (string, int) Struct = ("<4f", 16);
        public Vector2 Min = new(r.readSingle(), r.readSingle());
        public Vector2 Max = Vector2.Zero;
        public object NAM9Field(Header r, int dataSize) => Max = Vector2(r.readSingle(), r.readSingle());
    }

    # TES5
    public struct RNAMField {
        public struct Reference {
            public RefId<REFRRecord> Ref;
            public short X;
            public short Y;
        }
        public short GridX;
        public short GridY;
        public Reference[] GridReferences;

        public RNAMField(Header r, int dataSize) {
            GridX = r.readInt16();
            GridY = r.readInt16();
            var referenceCount = r.readUInt32();
            var referenceSize = dataSize - 8;
            Log.Assert(referenceSize >> 3 == referenceCount);
            GridReferences = r.ReadSArray<Reference>(referenceSize >> 3);
        }
    }

    public STRVField FULL;
    public RefField<WRLDRecord>? WNAM; # Parent Worldspace
    public RefField[CLMTRecord]? CNAM; # Climate
    public RefField[WATRRecord]? NAM2; # Water
    public FILEField? ICON; # Icon
    public MNAMField? MNAM; # Map Data
    public BYTEField? DATA; # Flags
    public NAM0Field NAM0; # Object Bounds
    public UI32Field? SNAM; # Music
    # TES5
    public List<RNAMField> RNAMs = []; # Large References

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize),
            case FieldType.FULL: z = self.FULL = r.readSTRV(dataSize),
            case FieldType.WNAM: z = self.WNAM = RefField<WRLDRecord>(r, dataSize),
            case FieldType.CNAM: z = self.CNAM = RefField[CLMTRecord](r, dataSize),
            case FieldType.NAM2: z = self.NAM2 = RefField[WATRRecord](r, dataSize),
            case FieldType.ICON: z = self.ICON = r.readFILE(dataSize),
            case FieldType.MNAM: z = self.MNAM = r.readS<MNAMField>(dataSize),
            case FieldType.DATA: z = self.DATA = r.readS<BYTEField>(dataSize),
            case FieldType.NAM0: z = self.NAM0 = NAM0Field(r, dataSize),
            case FieldType.NAM9: z = self.NAM0.NAM9Field(r, dataSize),
            case FieldType.SNAM: z = self.SNAM = r.readS<UI32Field>(dataSize),
            case FieldType.OFST: z = self.r.Skip(dataSize),
            # TES5
            case FieldType.RNAM: z = self.RNAMs.addX(RNAMField(r, dataSize)),
            case _: z = Record.empty
        return z
# end::WRLD[]

# WTHR.Weather - 0450 - tag::WTHR[]
public class WTHRRecord : Record, IHaveMODL {
    public struct FNAMField(Header r, int dataSize) {
        public float DayNear = r.readSingle();
        public float DayFar = r.readSingle();
        public float NightNear = r.readSingle();
        public float NightFar = r.readSingle();
    }

    public struct HNAMField(Header r, int dataSize) {
        public float EyeAdaptSpeed = r.readSingle();
        public float BlurRadius = r.readSingle();
        public float BlurPasses = r.readSingle();
        public float EmissiveMult = r.readSingle();
        public float TargetLUM = r.readSingle();
        public float UpperLUMClamp = r.readSingle();
        public float BrightScale = r.readSingle();
        public float BrightClamp = r.readSingle();
        public float LUMRampNoTex = r.readSingle();
        public float LUMRampMin = r.readSingle();
        public float LUMRampMax = r.readSingle();
        public float SunlightDimmer = r.readSingle();
        public float GrassDimmer = r.readSingle();
        public float TreeDimmer = r.readSingle();
    }

    public struct DATAField(Header r, int dataSize) {
        public byte WindSpeed = r.ReadByte();
        public byte CloudSpeed_Lower = r.ReadByte();
        public byte CloudSpeed_Upper = r.ReadByte();
        public byte TransDelta = r.ReadByte();
        public byte SunGlare = r.ReadByte();
        public byte SunDamage = r.ReadByte();
        public byte Precipitation_BeginFadeIn = r.ReadByte();
        public byte Precipitation_EndFadeOut = r.ReadByte();
        public byte ThunderLightning_BeginFadeIn = r.ReadByte();
        public byte ThunderLightning_EndFadeOut = r.ReadByte();
        public byte ThunderLightning_Frequency = r.ReadByte();
        public byte WeatherClassification = r.ReadByte();
        public ByteColor4 LightningColor = new(r.ReadByte(), r.ReadByte(), r.ReadByte(), 255);
    }

    public struct SNAMField(Header r, int dataSize) {
        public Ref<SOUNRecord> Sound = new(r.readUInt32()); # Sound FormId
        public uint Type = r.readUInt32(); # Sound Type - 0=Default, 1=Precipitation, 2=Wind, 3=Thunder
    }

    public MODLGroup MODL { get; set; } # Model
    public FILEField CNAM; # Lower Cloud Layer
    public FILEField DNAM; # Upper Cloud Layer
    public BYTVField NAM0; # Colors by Types/Times
    public FNAMField FNAM; # Fog Distance
    public HNAMField HNAM; # HDR Data
    public DATAField DATA; # Weather Data
    public List<SNAMField> SNAMs = []; # Sounds

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize),
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize),
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize),
            case FieldType.CNAM: z = self.CNAM = r.readFILE(dataSize),
            case FieldType.DNAM: z = self.DNAM = r.readFILE(dataSize),
            case FieldType.NAM0: z = self.NAM0 = r.readBYTV(dataSize),
            case FieldType.FNAM: z = self.FNAM = FNAMField(r, dataSize),
            case FieldType.HNAM: z = self.HNAM = HNAMField(r, dataSize),
            case FieldType.DATA: z = self.DATA = DATAField(r, dataSize),
            case FieldType.SNAM: z = self.SNAMs.addX(SNAMField(r, dataSize)),
            case _: z = Record.empty
        return z
# end::WTHR[]











# TES3.Plugin info - 3000
class TES3Record(Record):
    class HEDRField:
        def __init__(self, r: Header, dataSize: int):
            self.version: float = r.readSingle()
            self.fileType: int = r.readUInt32()
            self.companyName: str = r.readFAString(32)
            self.fileDescription: str = r.readFAString(256)
            self.numRecords: int = r.readUInt32()

    HEDR: any #HEDRField 
    MASTs: list[STRVField]
    DATAs: list[INTVField]

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.HEDR: z = self.HEDR = self.HEDRField(r, dataSize)
            case FieldType.MAST: z = self.MASTs = (self.MASTs or []).addX(r.readSTRV(dataSize))
            case FieldType.DATA: z = self.DATAs = (self.DATAs or []).addX(r.readINTV(dataSize))
            case _: z = Record.empty
        return z

#endregion
