import os
from io import BytesIO
from itertools import groupby
from typing import TypeVar, get_args
from enum import Enum, Flag, IntEnum, IntFlag
from numpy import ndarray, array
from openstk import log, Int3, Byte3, Float3
from gamex import FileSource, BinaryReader, ArcBinaryT
from gamex.core.globalx import ByteColor4
from gamex.families.Uncore.formats.compression import decompressLz4, decompressZlib

# types
type Vector3 = ndarray

class Colorf:
    pass

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
        None_ = 0x00000000                  # None
        EsmFile = 0x00000001                # ESM file. (TES4.HEDR record only.)
        Deleted = 0x00000020                # Deleted
        R00 = 0x00000040                    # Constant / (REFR) Hidden From Local Map (Needs Confirmation: Related to shields)
        R01 = 0x00000100                    # Must Update Anims / (REFR) Inaccessible
        R02 = 0x00000200                    # (REFR) Hidden from local map / (ACHR) Starts dead / (REFR) MotionBlurCastsShadows
        R03 = 0x00000400                    # Quest item / Persistent reference / (LSCR) Displays in Main Menu
        InitiallyDisabled = 0x00000800      # Initially disabled
        Ignored = 0x00001000                # Ignored
        Unknown1 = 0x00002000               # Unknown1
        Unknown2 = Unknown1 | R03           # Unknown2
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

        @classmethod
        def _missing_(cls, value):
            s = int.__new__(cls); s._value_ = value; s._name_ = f'_{hex(value)}'
            print(f'_missing_: {hex(value)}')
            return s

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
        self._header: Header = header
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
    def __init__(self, value: str = None): self.value: str = value
class FILEField:
    def __repr__(self) -> str: return f'{self.value}'
    def __init__(self, value: str = None): self.value: str = value
class DATVField:
    def __repr__(self) -> str: return f'DATV'
    def __init__(self, b: bool = None, i: int = None, f: float = None, s: str = None): self.b: bool = b; self.i: int = i; self.f: float = f; self.s: str = s
class FLTVField:
    _struct = ('<f', 4)
    def __repr__(self) -> str: return f'{self.value}'
    def __init__(self, tuple = None, value = None): self.value = tuple[0] if tuple else value
class BYTEField: 
    _struct = ('<c', 1)
    def __repr__(self) -> str: return f'{self.value}'
    def __init__(self, tuple = None, value = None): self.value = tuple[0] if tuple else value
class IN16Field: 
    _struct = ('<h', 2)
    def __repr__(self) -> str: return f'{self.value}'
    def __init__(self, tuple = None, value = None): self.value = tuple[0] if tuple else value
class UI16Field: 
    def __repr__(self) -> str: return f'{self.value}'
    _struct = ('<H', 2)
    def __init__(self, tuple = None, value = None): self.value = tuple[0] if tuple else value
class IN32Field: 
    _struct = ('<i', 4)
    def __repr__(self) -> str: return f'{self.value}'
    def __init__(self, tuple = None, value = None): self.value = tuple[0] if tuple else value
class UI32Field: 
    _struct = ('<I', 4)
    def __repr__(self) -> str: return f'{self.value}'
    def __init__(self, tuple = None, value = None): self.value = tuple[0] if tuple else value
class INTVField:
    def __repr__(self) -> str: return f'{self.value}'
    _struct = ('<q', 8)
    def __init__(self, tuple = None, value = None): self.value = tuple[0] if tuple else value
    def asUI16Field(self) -> UI16Field: return UI16Field(value=self.value)
class CREFField:
    _struct = ('<4c', 4)
    def __repr__(self) -> str: return f'{self.color}'
    def __init__(self, tuple = None, value = None): self.color = ByteColor4(tuple[0], tuple[1], tuple[2], tuple[3]) if tuple else value
class BYTVField:
    def __repr__(self) -> str: return f'BYTS'
    def __init__(self, value: bytes = None): self.value: bytes = value
class UNKNField: 
    def __repr__(self) -> str: return f'UNKN'
    def __init__(self, value: bytes = None): self.value: bytes = value
class CNTOField:
    def __repr__(self) -> str: return f'{self.item}'
    itemCount: int # Number of the item
    item: 'Ref[Record]' # The ID of the item
    def __init__(self, r: Header, dataSize: int):
        if r.format == FormType.TES3: self.itemCount = r.readUInt32(); self.item = Ref[Record](Record, r.readFAString(32)); return
        self.item = Ref[Record](Record, r.readUInt32()); self.itemCount = r.readUInt32()
class MODLGroup:
    def __repr__(self) -> str: return f'{self.value}'
    def __init__(self, r: Header, dataSize: int): self.value: str = r.readFUString(dataSize)
    bound: float = 0
    textures: bytes = None # Texture Files Hashes
    def MODBField(self, r: Header, dataSize: int) -> object: z = self.bound = r.readSingle(); return z
    def MODTField(self, r: Header, dataSize: int) -> object: z = self.textures = r.readBytes(dataSize); return z

#endregion

#region Record

class Record:
    def __repr__(self) -> str: return f'{self.__class__.__name__[:4]}:{self.EDID.value if self.EDID else None}'
    _mapx: dict[FormType, (callable, callable)] = {
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
    _empty: 'Record'
    _header: Header = None
    EDID: STRVField = None # Editor ID

    # Return an uninitialized subrecord to deserialize, or null to skip.
    def createField(self, r: Header, type: FieldType, dataSize: int) -> object: return Record._empty

    def read(self, r: Header) -> None:
        start = r.tell(); end = start + self._header.dataSize
        while not r.atEnd(end):
            field = FieldHeader(r)
            if field.type == FieldType.XXXX:
                if field.dataSize != 4: raise Exception()
                field.dataSize = r.readUInt32()
                continue
            elif field.type == FieldType.OFST and self._header.Type == FormType.WRLD: r.seek(end); continue
            tell = r.tell()
            if self.createField(r, field.type, field.dataSize) == Record._empty: print(f'Unsupported ESM record type: {self._header.type}:{field.type}'); r.skip(field.dataSize); continue
            r.ensureAtEnd(tell + field.dataSize, f'Failed reading {self._header.type}:{field.type} field data at offset {tell} in {r.binPath} of {r.tell() - tell - field.dataSize}')
        r.ensureAtEnd(end, f'Failed reading {self._header.type} record data at offset {start} in {r.binPath}')

    _factorySet = { FormType.TES3, FormType.APPA }
    # _factorySet = { FormType.TES3, FormType.ACTI }
    @staticmethod
    def factory(r: Header, type: FieldType, level: int = 1000) -> 'Record':
        if not (z := Record._mapx.get(type)): print(f'Unsupported ESM record type: {type}'); return None
        if type not in Record._factorySet: return None
        # if not z[1](level): return None
        record = z[0](); record._header = r
        return record
Record._empty = Record()

class RefId[T: Record]:
    _struct = ('<I', 4)
    def __repr__(self) -> str: return f'{self.type}:{self.id}'
    def __init__(self, t: type, tuple): self.type = (t if isinstance(t, str) else t.__name__)[:4]; self.id: int = tuple[0]

class Ref[T: Record]:
    def __repr__(self) -> str: return f'{self.type}:{self.name}{self.id}'
    def __init__(self, *args): 
        self.type = (args[0] if isinstance(args[0], str) else args[0].__name__)[:4]
        match len(args):
            case 1: self.id: int = 0; self.name: str = None
            case 2 if isinstance(args[1], int): self.id: int = args[1]; self.name: str = None
            case 2 if isinstance(args[1], str): self.id: int = 0; self.name: str = args[1]
            case 3: self.id: int = args[1]; self.name: str = args[2]
    def setName(self, name: str) -> 'Ref': z = self.name = name; return z

class RefField[T: Record]:
    def __repr__(self) -> str: return f'{self.value}'
    def __init__(self, t: type, r: Header = None, dataSize: int = 0): self.value = Ref[T](t, r.readUInt32()) if dataSize == 4 else Ref[T](t, r.readFAString(dataSize)) if r else Ref[T](t)
    def setName(self, name: str) -> str: z = self.value.name = name; return z

class Ref2Field[T: Record]:
    def __repr__(self) -> str: return f'{self.value1}x{self.value2}'
    def __init__(self, t: type, r: Header, dataSize: int): self.value1 = Ref[T](t, r.readUInt32()); self.value2 = Ref[T](t, r.readUInt32())

#endregion

#region Record Group

class RecordGroup:
    # def __repr__(self) -> str: return f'{next(iter(self.headers), None)}'
    cellsLoaded: int = 0
    @property
    def label(self) -> str: return next(iter(self.headers), None).label
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
        r = h._header
        r.seek(h.position)
        end = h.position + h.dataSize
        while not r.atEnd(end):
            r2 = Header(r, r.binPath, r.format)
            if r2.type == FormType.GRUP:
                group = ReadGRUP(r, r2.group)
                if loadAll: group.load(loadAll)
                continue
            # HACK to limit cells loading
            if r2.type == FormType.CELL and RecordGroup.cellsLoaded > 1000: r.skip(r2.dataSize); continue
            record = Record.factory(r2, r2.type, self.level)
            if not record: r.skip(r2.dataSize); continue
            self.readRecord(r, record, r2.compressed)
            self.records.append(record)
            if r2.type == FormType.CELL: RecordGroup.cellsLoaded += 1
            self.groupsByLabel = { s.key:list(g) for s, g in groupby(self.groups, lambda s: s.label) } if self.groups else None

    def readGRUP(self, r: Header, h: GroupHeader) -> 'RecordGroup':
        nextPosition = r.tell() + h.dataSize
        _nca(self, 'groups', [])
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
        # log.info(f'Recd: {record._header.type}')
        if not compressed: record.read(r); return
        newDataSize = r.readUInt32()
        newData = decompressZlib2(r, record._header.dataSize - 4, newDataSize)
        # read record
        record._header.position = 0
        record._header.dataSize = newDataSize
        with Header(BinaryReader(newData), r.binPath, r.format) as r2: record.read(r2)


#endregion

#region Extensions

def _nca(self, name, value): return getattr(self, name, None) or (setattr(self, name, value), getattr(self, name))[1]
def then[T, TResult](s: Record, value: T, then: callable) -> TResult: return then(value)
class EList[T](list[T]):
    def last[T](s: list[T]) -> T: return s[-1]
    def single[T](s: list[T], func: callable) -> T: return next(iter([x for x in s if func(x)]), None)
    def addX[T](s: list[T], value: T) -> T: s.append(value); return value
    def addRangeX[T](s: list[T], value: iter) -> iter: s.extend(value); return value
def listx(s: list = []): return EList(s)
def readINTV(r: Header, length: int) -> INTVField:
    match length:
        case 1: return INTVField(value=r.readByte())
        case 2: return INTVField(value=r.readInt16())
        case 4: return INTVField(value=r.readInt32())
        case 8: return INTVField(value=r.readInt64())
        case _: raise Exception(f'Tried to read an INTV subrecord with an unsupported size ({length})')
def readDATV(r: Header, length: int, type: chr) -> DATVField:
    match type:
        case 'b': return DATVField(b=r.readInt32() != 0)
        case 'i': return DATVField(i=r.readInt32())
        case 'f': return DATVField(f=r.readSingle())
        case 's': return DATVField(s=r.readFUString(length))
        case _: raise Exception(f'{type}')
def readSTRV(r: Header, length: int) -> STRVField: return STRVField(value=r.readFUString(length))
def readSTRV_ZPad(r: Header, length: int) -> STRVField: return STRVField(value=r.readFAString(length))
def readFILE(r: Header, length: int) -> FILEField: return FILEField(value=r.readFUString(length))
def readBYTV(r: Header, length: int) -> BYTVField: return BYTVField(value=r.readBytes(length))
def readUNKN(r: Header, length: int) -> UNKNField: return UNKNField(value=r.readBytes(length))

# monkey patch
Record.then = then
Header.readINTV = readINTV
Header.readDATV = readDATV
Header.readSTRV = readSTRV
Header.readSTRV_ZPad = readSTRV_ZPad
Header.readFILE = readFILE
Header.readBYTV = readBYTV
Header.readUNKN = readUNKN

#endregion

#region Records

# AACT.Action - 0050 - tag::AACT[]
# dep: None
class AACTRecord(Record):
    CNAM: CREFField # RGB Color

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.CNAM: z = self.CNAM = r.readS(CREFField, dataSize)
            case _: z = Record._empty
        return z
# end::AACT[]

# ACRE.Placed creature - 0400 - tag::ACRE[]
# dep: CELLRecord, REFRRecord
class ACRERecord(Record):
    NAME: RefField[Record] # Base
    DATA: 'REFRRecord.DATAField' # Position/Rotation
    XOWNs: list['CELLRecord.XOWNGroup'] # Ownership (optional)
    XESP: 'REFRRecord.XESPField' # Enable Parent (optional)
    XSCL: FLTVField # Scale (optional)
    XRGD: BYTVField # Ragdoll Data (optional)

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.NAME: z = self.NAME = RefField[Record](Record, r, dataSize)
            case FieldType.DATA: z = self.DATA = REFRRecord.DATAField(r, dataSize)
            case FieldType.XOWN: z = _nca(self, 'XOWNs', listx()).addX(CELLRecord.XOWNGroup(XOWN = RefField[Record](Record, r, dataSize)))
            case FieldType.XRNK: z = self.XOWNs.last().XRNK = r.readS(IN32Field, dataSize)
            case FieldType.XGLB: z = self.XOWNs.last().XGLB = RefField[Record](Record, r, dataSize)
            case FieldType.XESP: z = self.XESP = REFRRecord.XESPField(r, dataSize)
            case FieldType.XSCL: z = self.XSCL = r.readS(FLTVField, dataSize)
            case FieldType.XRGD: z = self.XRGD = r.readBYTV(dataSize)
            case _: z = Record._empty
        return z
# end::ACRE[]

# ACHR.Actor Reference - 0450 - tag::ACHR[]
# dep: ACRERecord, CELLRecord, REFRRecord, 
class ACHRRecord(Record):
    NAME: RefField[Record] # Base
    DATA: 'REFRRecord.DATAField' # Position/Rotation
    XPCI: RefField['CELLRecord'] # Unused (optional)
    XLOD: BYTVField # Distant LOD Data (optional)
    XESP: 'REFRRecord.XESPField' # Enable Parent (optional)
    XMRC: RefField['REFRRecord'] # Merchant Container (optional)
    XHRS: RefField[ACRERecord] # Horse (optional)
    XSCL: FLTVField # Scale (optional)
    XRGD: BYTVField # Ragdoll Data (optional)

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.NAME: z = self.NAME = RefField[Record](Record, r, dataSize)
            case FieldType.DATA: z = self.DATA = REFRRecord.DATAField(r, dataSize)
            case FieldType.XPCI: z = self.XPCI = RefField[CELLRecord](CELLRecord, r, dataSize)
            case FieldType.FULL: z = self.XPCI.value.setName(r.readFAString(dataSize))
            case FieldType.XLOD: z = self.XLOD = r.readBYTV(dataSize)
            case FieldType.XESP: z = self.XESP = REFRRecord.XESPField(r, dataSize)
            case FieldType.XMRC: z = self.XMRC = RefField[REFRRecord](REFRRecord, r, dataSize)
            case FieldType.XHRS: z = self.XHRS = RefField[ACRERecord](ACRERecord, r, dataSize)
            case FieldType.XSCL: z = self.XSCL = r.readS(FLTVField, dataSize)
            case FieldType.XRGD: z = self.XRGD = r.readBYTV(dataSize)
            case _: z = Record._empty
        return z
# end::ACHR[]

# ACTI.Activator - 3450 - tag::ACTI[]
# dep: SCPTRecord, SOUNRecord
class ACTIRecord(Record, IHaveMODL):
    MODL: MODLGroup # Model Name
    FULL: STRVField # Item Name
    SCRI: RefField['SCPTRecord'] = None # Script (Optional)
    # TES4
    SNAM: RefField['SOUNRecord'] = None # Sound (Optional)

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize)
            case FieldType.FULL | FieldType.FNAM: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.SCRI: z = self.SCRI = RefField[SCPTRecord](SCPTRecord, r, dataSize)
            # TES4
            case FieldType.SNAM: z = self.SNAM = RefField[SOUNRecord](SOUNRecord, r, dataSize)
            case _: z = Record._empty
        return z
# end::ACTI[]

# ADDN-Addon Node - 0050 - tag::ADDN[]
# dep: None
class ADDNRecord(Record):
    CNAM: CREFField # RGB Color

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.CNAM: z = self.CNAM = r.readS(CREFField, dataSize)
            case _: z = Record._empty
        return z
# end::ADDN[]

# ALCH.Potion - 3450 - tag::ALCH[]
# dep: ENCHRecord, SCPTRecord 
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
    SCRI: RefField['SCPTRecord'] = None # Script (optional)
    # TES4
    EFITs: list['ENCHRecord.EFITField'] = listx() # Effect Data
    SCITs: list['ENCHRecord.SCITField'] = listx() # Script Effect Data

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize)
            case FieldType.FULL: z = FULL = r.readSTRV(dataSize) if len(self.SCITs) == 0 else SCITs.last().FULLField(r, dataSize)
            case FieldType.FNAM: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.DATA | FieldType.ALDT: z = self.DATA = self.DATAField(r, dataSize)
            case FieldType.ENAM: z = self.ENAM = self.ENAMField(r, dataSize)
            case FieldType.ICON | FieldType.TEXT: z = self.ICON = r.readFILE(dataSize)
            case FieldType.SCRI: z = self.SCRI = RefField[SCPTRecord](SCPTRecord, r, dataSize)
            # TES4
            case FieldType.ENIT: z = self.DATA.ENITField(r, dataSize)
            case FieldType.EFID: z = r.skip(dataSize)
            case FieldType.EFIT: z = self.EFITs.addX(ENCHRecord.EFITField(r, dataSize))
            case FieldType.SCIT: z = self.SCITs.addX(ENCHRecord.SCITField(r, dataSize))
            case _: z = Record._empty
        return z
# end::ALCH[]

# AMMO.Ammo - 0450 - tag::AMMO[]
# dep: ENCHRecord
class AMMORecord(Record, IHaveMODL):
    class DATAField:
        def __init__(self, r: Header, dataSize: int):
            self.speed: float = r.readSingle()
            self.flags: int = r.readUInt32()
            self.value: int = r.readUInt32()
            self.weight: float = r.readSingle()
            self.damage: int = r.readUInt16()

    MODL: MODLGroup # Model
    FULL: STRVField # Item Name
    ICON: FILEField  # Male Icon (optional)
    ENAM: RefField['ENCHRecord'] # Enchantment ID (optional)
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
            case FieldType.ENAM: z = self.ENAM = RefField[ENCHRecord](ENCHRecord, r, dataSize)
            case FieldType.ANAM: z = self.ANAM = r.readS(IN16Field, dataSize)
            case FieldType.DATA: z = self.DATA = self.DATAField(r, dataSize)
            case _: z = Record._empty
        return z
# end::AMMO[]

# ANIO.Animated Object - 0450 - tag::ANIO[]
# dep: IDLERecord
class ANIORecord(Record, IHaveMODL):
    MODL: MODLGroup # Model
    DATA: RefField['IDLERecord'] # IDLE Animation

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.DATA: z = self.DATA = RefField[IDLERecord](IDLERecord, r, dataSize)
            case _: z = Record._empty
        return z
# end::ANIO[]

# APPA.Alchem Apparatus - 3450 - tag::APPA[]
# dep: SCPTRecord
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
    SCRI: RefField['SCPTRecord'] = None # Script Name

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize)
            case FieldType.FULL | FieldType.FNAM: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.DATA | FieldType.AADT: z = self.DATA = self.DATAField(r, dataSize)
            case FieldType.ICON | FieldType.ITEX: z = self.ICON = r.readFILE(dataSize)
            case FieldType.SCRI: z = self.SCRI = RefField[SCPTRecord](SCPTRecord, r, dataSize)
            case _: z = Record._empty
        return z
# end::APPA[]

# ARMA.Armature (Model) - 0050 - tag::ARMA[]
# dep: None
class ARMARecord(Record):
    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case _: z = Record._empty
        return z
# end::ARMA[]

# ARMO.Armor - 3450 - tag::ARMA[]
# dep: CLOTRecord, ENCHRecord, SCPTRecord
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
    SCRI: RefField['SCPTRecord'] # Script Name (optional)
    ENAM: RefField['ENCHRecord'] # Enchantment FormId (optional)
    # TES3
    INDXs: list['CLOTRecord.INDXFieldGroup'] = listx() # Body Part Index
    # TES4
    BMDT: UI32Field # Flags
    MOD2: MODLGroup # Male World Model (optional)
    MOD3: MODLGroup # Female Biped Model (optional)
    MOD4: MODLGroup # Female World Model (optional)
    ICO2: FILEField # Female Icon (optional)
    ANAM: IN16Field # Enchantment Points (optional)

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize)
            case FieldType.FULL | FieldType.FNAM: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.DATA | FieldType.AODT: z = self.DATA = self.DATAField(r, dataSize)
            case FieldType.ICON | FieldType.ITEX: z = self.ICON = r.readFILE(dataSize)
            case FieldType.SCRI: z = self.SCRI = RefField[SCPTRecord](SCPTRecord, r, dataSize)
            case FieldType.ENAM: z = self.ENAM = RefField[ENCHRecord](ENCHRecord, r, dataSize)
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
            case _: z = Record._empty
        return z
# end::ARMO[]

# ARTO.Art Object - 0050 - tag::ARTO[]
# dep: None
class ARTORecord(Record):
    CNAM: CREFField # RGB Color

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.CNAM: z = self.CNAM = r.readS(CREFField, dataSize)
            case _: z = Record._empty
        return z
# end::ARTO[]

# ASPC.Acoustic Space - 0050 - tag::ASPC[]
# dep: None
class ASPCRecord(Record):
    CNAM: CREFField # RGB Color

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.CNAM: z = self.CNAM = r.readS(CREFField, dataSize)
            case _: z = Record._empty
        return z
# end::ASPC[]

# ASTP.Association Type - 0050 - tag::ASTP[]
# dep: None
class ASTPRecord(Record):
    CNAM: CREFField # RGB Color

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize),
            case FieldType.CNAM: z = self.CNAM = r.readS(CREFField, dataSize),
            case _: z = Record._empty
        return z
# end::ASTP[]

# AVIF.Actor Values_Perk Tree Graphics - 0050 - tag::ASTP[]
# dep: None
class AVIFRecord(Record):
    CNAM: CREFField # RGB Color

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.DID = r.readSTRV(dataSize)
            case FieldType.CNAM: z = self.CNAM = r.readS(CREFField, dataSize)
            case _: z = Record._empty
        return z
# end::AVIF[]

# BODY.Body - 3000 - tag::ASTP[]
# dep: None
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
        if r.format == FormType.TES3:
            match type:
                case FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
                case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
                case FieldType.FNAM: z = self.FNAM = r.readSTRV(dataSize)
                case FieldType.BYDT: z = self.BYDT = self.BYDTField(r, dataSize)
                case _: z = Record._empty
            return z
        return None
# end::BODY[]

# BOOK.Book - 3450 - tag::BOOK[]
# dep: ENCHRecord, SCPTRecord
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
    SCRI: RefField['SCPTRecord'] # Script Name (optional)
    ENAM: RefField['ENCHRecord'] # Enchantment FormId (optional)
    # TES4
    ANAM: IN16Field # Enchantment points (optional)

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize)
            case FieldType.FULL | FieldType.FNAM: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.DATA | FieldType.BKDT: z = self.DATA = self.DATAField(r, dataSize)
            case FieldType.ICON | FieldType.ITEX: z = self.ICON = r.readFILE(dataSize)
            case FieldType.SCRI: z = self.SCRI = RefField[SCPTRecord](SCPTRecord, r, dataSize)
            case FieldType.DESC | FieldType.TEXT: z = self.DESC = r.readSTRV(dataSize)
            case FieldType.ENAM: z = self.ENAM = RefField[ENCHRecord](ENCHRecord, r, dataSize)
            # TES4
            case FieldType.ANAM: z = self.ANAM = r.readS(IN16Field, dataSize)
            case _: z = Record._empty
        return z
# end::BODY[]

# BSGN.Birthsign - 3400 - tag::BSGN[]
# dep: None
class BSGNRecord(Record):
    FULL: STRVField # Sign Name
    ICON: FILEField # Texture
    DESC: STRVField # Description
    NPCSs: list[STRVField] # TES3: Spell/ability
    SPLOs: list[RefField[Record]] # TES4: (points to a SPEL or LVSP record)

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.FULL | FieldType.FNAM: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.ICON | FieldType.TNAM: z = self.ICON = r.readFILE(dataSize)
            case FieldType.DESC: z = self.DESC = r.readSTRV(dataSize)
            case FieldType.SPLO: z = _nca(self, 'SPLOs', listx()).addX(RefField[Record](Record, r, dataSize))
            case FieldType.NPCS: z = _nca(self, 'NPCSs', listx()).addX(r.readSTRV(dataSize))
            case _: z = Record._empty
        return z
# end::BSGN[]

# CELL.Cell - 3450 - tag::CELL[]
# dep: REGNRecord, CLMTRecord, WATRRecord
class CELLRecord(Record): #ICellRecord
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
        _struct = { 8: '<2i', 12: '<2iI' }
        def __init__(self, tuple):
            match len(tuple):
                case 8:
                    (self.gridX,
                    self.gridY) = tuple
                case 12:
                    (self.gridX,
                    self.gridY,
                    self.flags) = tuple

    class XCLLField:
        _struct = { 16: '<12cf', 36: '<12c2f2i2f', 40: '<12c2f2i3f' }
        def __init__(self, tuple):
            ambientColor = self.ambientColor = ByteColor4()
            directionalColor = self.directionalColor = ByteColor4()
            fogColor = self.fogColor = ByteColor4()
            match len(tuple):
                case 16:
                    (ambientColor.r, ambientColor.g, ambientColor.b, ambientColor.a,
                    directionalColor.r, directionalColor.g, directionalColor.b, directionalColor.a, # SunlightColor
                    fogColor.r, fogColor.g, fogColor.b, fogColor.a,
                    self.fogNear) = tuple # FogDensity
                case 36:
                    (ambientColor.r, ambientColor.g, ambientColor.b, ambientColor.a,
                    directionalColor.r, directionalColor.g, directionalColor.b, directionalColor.a, # SunlightColor
                    fogColor.r, fogColor.g, fogColor.b, fogColor.a,
                    self.fogNear, # FogDensity
                    # TES4
                    self.fogFar,
                    self.directionalRotationXY,
                    self.directionalRotationZ,
                    self.directionalFade,
                    self.fogClipDist) = tuple
                case 40:
                    (ambientColor.r, ambientColor.g, ambientColor.b, ambientColor.a,
                    directionalColor.r, directionalColor.g, directionalColor.b, directionalColor.a, # SunlightColor
                    fogColor.r, fogColor.g, fogColor.b, fogColor.a,
                    self.fogNear, # FogDensity
                    # TES4
                    self.fogFar,
                    self.directionalRotationXY,
                    self.directionalRotationZ,
                    self.directionalFade,
                    self.fogClipDist,
                    # TES5
                    self.fogPow) = tuple

    class XOWNGroup:
        XOWN: RefField[Record]
        XRNK: IN32Field # Faction rank
        XGLB: RefField[Record]

    class XYZAField:
        _struct = ('<3f3f', 24)
        def __init__(self, tuple):
            position = self.position = Float3()
            eulerAngles = self.eulerAngles = Float3()
            (position.x, position.y, position.z,
            eulerAngles.x, eulerAngles.y, eulerAngles.z) = tuple
            
    class RefObj:
        def __repr__(self): return f'CREF: {self.EDID.value}'
        FRMR: UI32Field # Object Index (starts at 1)
        # This is used to uniquely identify objects in the cell. For files the index starts at 1 and is incremented for each object added. For modified objects the index is kept the same.
        EDID: STRVField # Object ID
        XSCL: FLTVField # Scale (Static)
        DELE: IN32Field # Indicates that the reference is deleted.
        DODT: 'XYZAField' # XYZ Pos, XYZ Rotation of exit
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
        DATA: 'XYZAField' # Ref Position Data
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
    XCLRs: list[RefField['REGNRecord']] # Regions
    XCMT: BYTEField # Music (optional)
    XCCM: RefField['CLMTRecord'] # Climate
    XCWT: RefField['WATRRecord'] # Water
    XOWNs: list[XOWNGroup] = listx() # Ownership

    # Referenced Object Data Grouping
    inFRMR: bool = False
    refObjs: list[RefObj] = listx()
    _lastRef: RefObj

    @property
    def isInterior(self) -> bool: return (self.DATA.value & 0x01) == 0x01
    gridId: Int3 # => Int3(XCLC.Value.GridX, XCLC.Value.GridY, !IsInterior ? 0 : -1);
    @property
    def ambientLight(self) -> Colorf: return Colorf(self.XCLL.value.ambientColor.asColor32) if self.XCLL else None

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        # print(f'   {type}')
        if not self.inFRMR and type == FieldType.FRMR: self.inFRMR = True
        if not self.inFRMR:
            match type:
                case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
                case FieldType.FULL | FieldType.RGNN: z = self.FULL = r.readSTRV(dataSize)
                case FieldType.DATA:
                    z = self.DATA = r.readINTV(4 if r.format == FormType.TES3 else dataSize).asUI16Field
                    if r.format == FormType.TES3: self.XCLC = r.readS(self.XCLCField, 8 if r.format == FormType.TES3 else dataSize)
                case FieldType.XCLC: z = self.XCLC = r.readS(self.XCLCField, 8 if r.format == FormType.TES3 else dataSize)
                case FieldType.XCLL | FieldType.AMBI: z = self.XCLL = r.readS(self.XCLLField, dataSize)
                case FieldType.XCLW | FieldType.WHGT: z = self.XCLW = r.readS(FLTVField, dataSize)
                # TES3
                case FieldType.NAM0: z = self.NAM0 = r.readS(UI32Field, dataSize)
                case FieldType.INTV: z = self.INTV = r.readINTV(dataSize)
                case FieldType.NAM5: z = self.NAM5 = r.readS(CREFField, dataSize)
                # TES4
                case FieldType.XCLR: z = self.XCLRs = r.readFArray(lambda z: RefField[REGNRecord](REGNRecord, r, 4), dataSize >> 2)
                case FieldType.XCMT: z = self.XCMT = r.readS(BYTEField, dataSize)
                case FieldType.XCCM: z = self.XCCM = RefField[CLMTRecord](CLMTRecord, r, dataSize)
                case FieldType.XCWT: z = self.XCWT = RefField[WATRRecord](WATRRecord, r, dataSize)
                case FieldType.XOWN: z = self.XOWNs.addX(self.XOWNGroup(XOWN = RefField[Record](Record, r, dataSize)))
                case FieldType.XRNK: z = self.XOWNs.last().XRNK = r.readS(IN32Field, dataSize)
                case FieldType.XGLB: z = self.XOWNs.last().XGLB = RefField[Record](Record, r, dataSize)
                case _: z = Record._empty
            return z
        # Referenced Object Data Grouping
        match type:
            # RefObjDataGroup sub-records
            case FieldType.FRMR: self._lastRef = self.refObjs.addX(self.RefObj()); z = self._lastRef.FRMR = r.readS(UI32Field, dataSize)
            case FieldType.NAME: z = self._lastRef.EDID = r.readSTRV(dataSize)
            case FieldType.XSCL: z = self._lastRef.XSCL = r.readS(FLTVField, dataSize)
            case FieldType.DODT: z = self._lastRef.DODT = r.readS(self.XYZAField, dataSize)
            case FieldType.DNAM: z = self._lastRef.DNAM = r.readSTRV(dataSize)
            case FieldType.FLTV: z = self._lastRef.FLTV = r.readS(FLTVField, dataSize)
            case FieldType.KNAM: z = self._lastRef.KNAM = r.readSTRV(dataSize)
            case FieldType.TNAM: z = self._lastRef.TNAM = r.readSTRV(dataSize)
            case FieldType.UNAM: z = self._lastRef.UNAM = r.readS(BYTEField, dataSize)
            case FieldType.ANAM: z = self._lastRef.ANAM = r.readSTRV(dataSize)
            case FieldType.BNAM: z = self._lastRef.BNAM = r.readSTRV(dataSize)
            case FieldType.INTV: z = self._lastRef.INTV = r.readS(IN32Field, dataSize)
            case FieldType.NAM9: z = self._lastRef.NAM9 = r.readS(UI32Field, dataSize)
            case FieldType.XSOL: z = self._lastRef.XSOL = r.readSTRV(dataSize)
            case FieldType.DATA: z = self._lastRef.DATA = r.readS(self.XYZAField, dataSize)
            # TES
            case FieldType.CNAM: z = self._lastRef.CNAM = r.readSTRV(dataSize)
            case FieldType.NAM0: z = self._lastRef.NAM0 = r.readS(UI32Field, dataSize)
            case FieldType.XCHG: z = self._lastRef.XCHG = r.readS(IN32Field, dataSize)
            case FieldType.INDX: z = self._lastRef.INDX = r.readS(IN32Field, dataSize)
            case _: z = Record._empty
        return z
# end::CELL[]

# CLAS.Class - 3450 - tag::CLAS[]
# dep: None
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
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.FULL | FieldType.FNAM: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.CLDT: z = r.skip(dataSize) # TES3
            case FieldType.DESC: z = self.DESC = r.readSTRV(dataSize)
            # TES4
            case FieldType.ICON: z = self.ICON = r.readSTRV(dataSize)
            case FieldType.DATA: z = self.DATA = self.DATAField(r, dataSize)
            case _: z = Record._empty
        return z
# end::CLAS[]

# CLOT.Clothing - 3450 - tag::CLOT[]
# dep: SCPTRecord
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
        def __init__(self, INDX: INTVField = None):
            self.INDX: INTVField = INDX
            self.BNAM: STRVField = None
            self.CNAM: STRVField = None

    MODL: MODLGroup # Model Name
    FULL: STRVField # Item Name
    DATA: DATAField # Clothing Data
    ICON: FILEField # Male Icon
    ENAM: STRVField # Enchantment Name
    SCRI: RefField['SCPTRecord'] # Script Name
    # TES3
    INDXs: list[INDXFieldGroup] = listx() # Body Part Index (Moved to Race)
    # TES4
    BMDT: UI32Field # Clothing Flags
    MOD2: MODLGroup # Male world model (optional)
    MOD3: MODLGroup # Female biped (optional)
    MOD4: MODLGroup # Female world model (optional)
    ICO2: FILEField # Female icon (optional)
    ANAM: IN16Field # Enchantment points (optional)

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize)
            case FieldType.FULL | FieldType.FNAM: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.DATA | FieldType.CTDT: z = self.DATA = self.DATAField(r, dataSize)
            case FieldType.ICON | FieldType.ITEX: z = self.ICON = r.readFILE(dataSize)
            case FieldType.INDX: z = self.INDXs.addX(self.INDXFieldGroup(INDX = r.readINTV(dataSize)))
            case FieldType.BNAM: z = self.INDXs.last().BNAM = r.readSTRV(dataSize)
            case FieldType.CNAM: z = self.INDXs.last().CNAM = r.readSTRV(dataSize)
            case FieldType.ENAM: z = self.ENAM = r.readSTRV(dataSize)
            case FieldType.SCRI: z = self.SCRI = RefField[SCPTRecord](SCPTRecord, r, dataSize)
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
            case _: z = Record._empty
        return z
# end::CLOT[]

# CLMT.Climate - 0450 - tag::CLMT[]
# dep: ^WTHRRecord
class CLMTRecord(Record, IHaveMODL):
    class WLSTField:
        def __init__(self, r: Header, dataSize: int):
            self.weather: Ref[WTHRRecord] = Ref[WTHRRecord](WTHRRecord, r.readUInt32())
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
    WLSTs: list[WLSTField] = listx() # Climate
    TNAM: TNAMField # Timing

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.FNAM: z = self.FNAM = r.readFILE(dataSize)
            case FieldType.GNAM: z = self.GNAM = r.readFILE(dataSize)
            case FieldType.WLST: z = self.WLSTs.addRangeX(r.readFArray(lambda z: self.WLSTField(r, dataSize), dataSize >> 3))
            case FieldType.TNAM: z = self.TNAM = self.TNAMField(r, dataSize)
            case _: z = Record._empty
        return z
# end::CLMT[]

# CONT.Container - 3450 - tag::CONT[]
# dep: SCPTRecord, SOUNRecord
class CONTRecord(Record, IHaveMODL):
    # TESX
    class DATAField:
        flags: int # flags 0x0001 = Organic, 0x0002 = Respawns, organic only, 0x0008 = Default, unknown
        weight: float

        def __init__(self, r: Header, dataSize: int):
            if r.format == FormType.TES3:
                self.weight = r.readSingle()
                return
            self.flags = r.readByte()
            self.weight = r.readSingle()
        def FLAGField(self, r: Header, dataSize: int) -> object: z = self.flags = r.readUInt32() & 0xFF; return z

    MODL: MODLGroup # Model
    FULL: STRVField # Container Name
    DATA: DATAField # Container Data
    SCRI: RefField['SCPTRecord']
    CNTOs: list[CNTOField] = listx()
    # TES4
    SNAM: RefField['SOUNRecord'] # Open sound
    QNAM: RefField['SOUNRecord'] # Close sound

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize)
            case FieldType.FULL | FieldType.FNAM: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.DATA | FieldType.CNDT: z = self.DATA = self.DATAField(r, dataSize)
            case FieldType.FLAG: z = self.DATA.FLAGField(r, dataSize)
            case FieldType.CNTO | FieldType.NPCO: z = self.CNTOs.addX(CNTOField(r, dataSize))
            case FieldType.SCRI: z = self.SCRI = RefField[SCPTRecord](SCPTRecord, r, dataSize)
            case FieldType.SNAM: z = self.SNAM = RefField[SOUNRecord](SOUNRecord, r, dataSize)
            case FieldType.QNAM: z = self.QNAM = RefField[SOUNRecord](SOUNRecord, r, dataSize)
            case _: z = Record._empty
        return z
# end::CONT[]

# CREA.Creature - 3450 - tag::CREA[]
# dep: SCPTRecord
class CREARecord(Record, IHaveMODL):
    class CREAFlags(Flag):
        Biped = 0x0001
        Respawn = 0x0002
        WeaponAndShield = 0x0004
        None_ = 0x0008
        Swims = 0x0010
        Flies = 0x0020
        Walks = 0x0040
        DefaultFlags = 0x0048
        Essential = 0x0080
        SkeletonBlood = 0x0400
        MetalBlood = 0x0800

    class NPDTField:
        def __init__(self, r: Header, dataSize: int):
            self.type: int = r.readInt32() # 0 = Creature, 1 = Daedra, 2 = Undead, 3 = Humanoid
            self.level: int = r.readInt32()
            self.strength: int = r.readInt32()
            self.intelligence: int = r.readInt32()
            self.willpower: int = r.readInt32()
            self.agility: int = r.readInt32()
            self.speed: int = r.readInt32()
            self.endurance: int = r.readInt32()
            self.personality: int = r.readInt32()
            self.luck: int = r.readInt32()
            self.health: int = r.readInt32()
            self.spellPts: int = r.readInt32()
            self.fatigue: int = r.readInt32()
            self.soul: int = r.readInt32()
            self.combat: int = r.readInt32()
            self.magic: int = r.readInt32()
            self.stealth: int = r.readInt32()
            self.attackMin1: int = r.readInt32()
            self.attackMax1: int = r.readInt32()
            self.attackMin2: int = r.readInt32()
            self.attackMax2: int = r.readInt32()
            self.attackMin3: int = r.readInt32()
            self.attackMax3: int = r.readInt32()
            self.gold: int = r.readInt32()

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
    SCRI: RefField['SCPTRecord'] # Script
    NPCO: CNTOField # Item record
    AIDT: AIDTField # AI data
    AI_W: AI_WField # AI Wander
    AI_T: AI_TField # AI Travel
    AI_F: AI_FField # AI Follow
    AI_E: AI_FField # AI Escort
    AI_A: AI_AField # AI Activate
    XSCL: FLTVField # Scale (optional), Only present if the scale is not 1.0
    CNAM: STRVField
    NPCSs: list[STRVField] = listx()

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        if r.format == FormType.TES3:
            match type:
                case FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
                case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
                case FieldType.FNAM: z = self.FNAM = r.readSTRV(dataSize)
                case FieldType.NPDT: z = self.NPDT = self.NPDTField(r, dataSize)
                case FieldType.FLAG: z = self.FLAG = r.readS(IN32Field, dataSize)
                case FieldType.SCRI: z = self.SCRI = RefField[SCPTRecord](SCPTRecord, r, dataSize)
                case FieldType.NPCO: z = self.NPCO = CNTOField(r, dataSize)
                case FieldType.AIDT: z = self.AIDT = self.AIDTField(r, dataSize)
                case FieldType.AI_W: z = self.AI_W = self.AI_WField(r, dataSize)
                case FieldType.AI_T: z = self.AI_T = self.AI_TField(r, dataSize)
                case FieldType.AI_F: z = self.AI_F = self.AI_FField(r, dataSize)
                case FieldType.AI_E: z = self.AI_E = self.AI_FField(r, dataSize)
                case FieldType.AI_A: z = self.AI_A = self.AI_AField(r, dataSize)
                case FieldType.XSCL: z = self.XSCL = r.readS(FLTVField, dataSize)
                case FieldType.CNAM: z = self.CNAM = r.readSTRV(dataSize)
                case FieldType.NPCS: z = self.NPCSs.addX(r.readSTRV_ZPad(dataSize))
                case _: z = Record._empty
            return z
        return None
# end::CREA[]

# CSTY.Combat Style - 0450 - tag::CSTY[]
# dep: None
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
            # if (dataSize != 124 && dataSize != 120 && dataSize != 112 && dataSize != 104 && dataSize != 92 && dataSize != 84) self.dodgePercentChance = 0;
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
            self.RangeMult_Max = r.readSingle()
            if dataSize == 92: return; self.switchDistance_Melee = r.readSingle()
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
            case FieldType.CSTD: z = self.CSTD = self.CSTDField(r, dataSize)
            case FieldType.CSAD: z = self.CSAD = self.CSADField(r, dataSize)
            case _: z = Record._empty
        return z
# end::CSTY[]

# DIAL.Dialog Topic - 3450 - tag::DIAL[]
# dep: INFORecord, QUSTRecord
class DIALRecord(Record):
    lastRecord: 'DIALRecord'
    class DIALType(Enum): RegularTopic = 0; Voice = 1; Greeting = 2; Persuasion = 3; Journal = 4
    FULL: STRVField # Dialogue Name
    DATA: BYTEField # Dialogue Type
    QSTIs: list[RefField['QUSTRecord']] # Quests (optional)
    INFOs: list['INFORecord'] = listx() # Info Records

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readSTRV(dataSize); DIALRecord.lastRecord = self
            case FieldType.FULL: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.DATA: z = self.DATA = r.readS(BYTEField, dataSize)
            case FieldType.QSTI | FieldType.QSTR: z = _nca(self, 'QSTIs', listx()).addX(RefField[QUSTRecord](QUSTRecord, r, dataSize))
            case _: z = Record._empty
        return z
# end::DIAL[]

# DLBR.Dialog Branch - 0050 - tag::DIAL[]
# dep: None
class DLBRRecord(Record):
    CNAM: CREFField # RGB color

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.CNAM: z = self.CNAM = r.readS(CREFField, dataSize)
            case _: z = Record._empty
        return z
# end::DLBR[]

# DLVW.Dialog View - 0050 - tag::DLVW[]
# dep: None
class DLVWRecord(Record):
    CNAM: CREFField # RGB color

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.CNAM: z = self.CNAM = r.readS(CREFField, dataSize)
            case _: z = Record._empty
        return z
# end::DLVW[]

# DOOR.Door - 3450 - tag::DOOR[]
# dep: SCPTRecord, SOUNRecord
class DOORRecord(Record, IHaveMODL):
    FULL: STRVField # Door name
    MODL: MODLGroup # NIF model filename
    SCRI: RefField['SCPTRecord'] # Script (optional)
    SNAM: RefField['SOUNRecord'] # Open Sound
    ANAM: RefField['SOUNRecord'] # Close Sound
    # TES4
    BNAM: RefField['SOUNRecord'] # Loop Sound
    FNAM: BYTEField # Flags
    TNAM: RefField[Record] # Random teleport destination

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.DID = r.readSTRV(dataSize)
            case FieldType.FULL: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.FNAM if r.format != FormType.TES3: z = self.FNAM = r.readS(BYTEField, dataSize) #:matchif
            case FieldType.FNAM if r.format == FormType.TES3: z = self.FULL = r.readSTRV(dataSize) #:matchif
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize)
            case FieldType.SCRI: z = self.SCRI = RefField[SCPTRecord](SCPTRecord, r, dataSize)
            case FieldType.SNAM: z = self.SNAM = RefField[SOUNRecord](SOUNRecord, r, dataSize)
            case FieldType.ANAM: z = self.ANAM = RefField[SOUNRecord](SOUNRecord, r, dataSize)
            case FieldType.BNAM: z = self.ANAM = RefField[SOUNRecord](SOUNRecord, r, dataSize)
            case FieldType.TNAM: z = self.TNAM = RefField[Record](Record, r, dataSize)
            case _: z = Record._empty
        return z
# end::DOOR[]

# EFSH.Effect Shader - 0450 - tag::EFSH[]
# dep: None
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
            case FieldType.DATA: z = self.DATA = self.DATAField(r, dataSize)
            case _: z = Record._empty
        return z
# end::EFSH[]

# ENCH.Enchantment - 3450 - tag::ENCH[]
# dep: None
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
        scriptFormId: int
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
    EFITs: list[EFITField] = listx() # Effect Data
    # TES4
    SCITs: list[SCITField] = listx() # Script effect data

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.FULL if len(self.SCITs) == 0: z = self.FULL = r.readSTRV(dataSize) #:matchif
            case FieldType.FULL if len(self.SCITs) != 0: z = self.SCITs.last().FULLField(r, dataSize) #:matchif
            case FieldType.ENIT | FieldType.ENDT: z = self.ENIT = self.ENITField(r, dataSize)
            case FieldType.EFID: z = r.skip(dataSize)
            case FieldType.EFIT | FieldType.ENAM: z = self.EFITs.addX(self.EFITField(r, dataSize))
            case FieldType.SCIT: z = self.SCITs.addX(self.SCITField(r, dataSize))
            case _: z = Record._empty
        return z
# end::ENCH[]

# EYES.Eyes - 0450 - tag::EYES[]
# dep: None
class EYESRecord(Record):
    FULL: STRVField
    ICON: FILEField
    DATA: BYTEField # Playable

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.FULL: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.ICON: z = self.ICON = r.readFILE(dataSize)
            case FieldType.DATA: z = self.DATA = r.readS(BYTEField, dataSize)
            case _: z = Record._empty
        return z
# end::EYES[]

# FACT.Faction - 3450 - tag::FACT[]
# dep: None
class FACTRecord(Record):
    # TESX
    class RNAMGroup:
        def __repr__(self): return f'{self.RNAM.value}:{self.MNAM.value}'
        def __init__(self, RNAM: IN32Field = None, MNAM: STRVField = None):
            self.RNAM: IN32Field = RNAM # rank
            self.MNAM: STRVField = MNAM # male
            self.FNAM: STRVField = None # female
            self.INAM: STRVField = None # insignia

    # TES3
    class FADTField:
        def __init__(self, r: Header, dataSize: int): r.skip(dataSize)

    # TES4
    class XNAMField:
        def __repr__(self): return f'{self.formId}'
        def __init__(self, r: Header, dataSize: int):
            self.formId: int = r.readInt32()
            self.mod: int = r.readInt32()
            self.combat: int = r.readInt32() if r.format > FormType.TES4 else 0

    FNAM: STRVField # Faction name
    RNAMs: list[RNAMGroup] = listx() # Rank Name
    FADT: FADTField # Faction data
    ANAMs: list[STRVField] = listx() # Faction name
    INTVs: list[INTVField] = listx() # Faction reaction
    # TES4
    XNAM: XNAMField # Interfaction Relations
    DATA: INTVField # Flags (byte, uint32)
    CNAM: UI32Field

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        if r.format == FormType.TES3:
            match type:
                case FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
                case FieldType.FNAM: z = self.FNAM = r.readSTRV(dataSize)
                case FieldType.RNAM: z = self.RNAMs.addX(self.RNAMGroup(MNAM = r.readSTRV(dataSize)))
                case FieldType.FADT: z = self.FADT = self.FADTField(r, dataSize)
                case FieldType.ANAM: z = self.ANAMs.addX(r.readSTRV(dataSize))
                case FieldType.INTV: z = self.INTVs.addX(r.readINTV(dataSize))
                case _: z = Record._empty
            return z
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.FULL: z = self.FNAM = r.readSTRV(dataSize)
            case FieldType.XNAM: z = self.XNAM = self.XNAMField(r, dataSize)
            case FieldType.DATA: z = self.DATA = r.readINTV(dataSize)
            case FieldType.CNAM: z = self.CNAM = r.readS(UI32Field, dataSize)
            case FieldType.RNAM: z = self.RNAMs.addX(self.RNAMGroup(RNAM = r.readS(IN32Field, dataSize)))
            case FieldType.MNAM: z = self.RNAMs.last().MNAM = r.readSTRV(dataSize)
            case FieldType.FNAM: z = self.RNAMs.last().FNAM = r.readSTRV(dataSize)
            case FieldType.INAM: z = self.RNAMs.last().INAM = r.readSTRV(dataSize)
            case _: z = Record._empty
        return z
# end::FACT[]

# FLOR.Flora - 0450 - tag::FLOR[]
# dep: INGRRecord, SCPTRecord
class FLORRecord(Record, IHaveMODL):
    MODL: MODLGroup # Model
    FULL: STRVField # Plant Name
    SCRI: RefField['SCPTRecord'] # Script (optional)
    PFIG: RefField['INGRRecord'] # The ingredient the plant produces (optional)
    PFPC: BYTVField # Spring, Summer, Fall, Winter Ingredient Production (byte)

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize)
            case FieldType.FULL: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.SCRI: z = self.SCRI = RefField[SCPTRecord](SCPTRecord, r, dataSize)
            case FieldType.PFIG: z = self.PFIG = RefField[INGRRecord](INGRRecord, r, dataSize)
            case FieldType.PFPC: z = self.PFPC = r.readBYTV(dataSize)
            case _: z = Record._empty
        return z
# end::FLOR[]

# FURN.Furniture - 0450 - tag::FURN[]
# dep: SCPTRecord
class FURNRecord(Record, IHaveMODL):
    MODL: MODLGroup # Model
    FULL: STRVField # Furniture Name
    SCRI: RefField['SCPTRecord'] # Script (optional)
    MNAM: IN32Field # Active marker flags, required. A bit field with a bit value of 1 indicating that the matching marker position in the NIF file is active.

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize)
            case FieldType.FULL: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.SCRI: z = self.SCRI = RefField[SCPTRecord](SCPTRecord, r, dataSize)
            case FieldType.MNAM: z = self.MNAM = r.readS(IN32Field, dataSize)
            case _: z = Record._empty
        return z
# end::FURN[]

# GLOB.Global - 3450 - tag::GLOB[]
# dep: None
class GLOBRecord(Record):
    FNAM: BYTEField # Type of global (s, l, f)
    FLTV: FLTVField # Float data

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.FNAM: z = self.FNAM = r.readS(BYTEField, dataSize)
            case FieldType.FLTV: z = self.FLTV = r.readS(FLTVField, dataSize)
            case _: z = Record._empty
        return z
# end::GLOB[]

# GMST.Game Setting - 3450 - tag::GMST[]
# dep: None
class GMSTRecord(Record):
    DATA: DATVField # Data

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        if r.format == FormType.TES3:
            match type:
                case FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
                case FieldType.STRV: z = self.DATA = r.readDATV(dataSize, 's')
                case FieldType.INTV: z = self.DATA = r.readDATV(dataSize, 'i')
                case FieldType.FLTV: z = self.DATA = r.readDATV(dataSize, 'f')
                case _: z = Record._empty
            return z
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.DATA: z = self.DATA = r.readDATV(dataSize, EDID.value[0])
            case _: z = Record._empty
        return z
# end::GMST[]

# GRAS.Grass - 0450 - tag::GRAS[]
# dep: None
class GRASRecord(Record):
    class DATAField:
        density: int
        minSlope: int
        maxSlope: int
        unitFromWaterAmount: int
        unitFromWaterType: int
        #Above - At Least,
        #Above - At Most,
        #Below - At Least,
        #Below - At Most,
        #Either - At Least,
        #Either - At Most,
        #Either - At Most Above,
        #Either - At Most Below
        positionRange: float
        heightRange: float
        colorRange: float
        wavePeriod: float
        flags: int

        def __init__(self, r: Header, dataSize: int):
            self.density = r.readByte()
            self.minSlope = r.readByte()
            self.maxSlope = r.readByte()
            r.readByte()
            self.unitFromWaterAmount = r.readUInt16()
            r.skip(2)
            self.unitFromWaterType = r.readUInt32()
            self.positionRange = r.readSingle()
            self.heightRange = r.readSingle()
            self.colorRange = r.readSingle()
            self.wavePeriod = r.readSingle()
            self.flags = r.readByte()
            r.skip(3)

    MODL: MODLGroup 
    DATA: DATAField 

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize)
            case FieldType.DATA: z = self.DATA = self.DATAField(r, dataSize)
            case _: z = Record._empty
        return z
# end::GRAS[]

# HAIR.Hair - 0400 - tag::HAIR[]
# dep: None
class HAIRRecord(Record, IHaveMODL):
    FULL: STRVField
    MODL: MODLGroup
    ICON: FILEField
    DATA: BYTEField # Playable, Not Male, Not Female, Fixed

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.FULL: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.ICON: z = self.ICON = r.readFILE(dataSize)
            case FieldType.DATA: z = self.DATA = r.readS(BYTEField, dataSize)
            case _: z = Record._empty
        return z
# end::HAIR[]

# IDLE.Idle Animations - 0450 - tag::IDLE[]
# dep: SCPTRecord
class IDLERecord(Record, IHaveMODL):
    MODL: MODLGroup
    CTDAs: list['SCPTRecord.CTDAField'] = [] # Conditions
    ANAM: BYTEField
    DATAs: list[RefField['IDLERecord']]

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.CTDA | FieldType.CTDT: z = self.CTDAs.addX(SCPTRecord.CTDAField(r, dataSize))
            case FieldType.ANAM: z = self.ANAM = r.readS(BYTEField, dataSize)
            case FieldType.DATA: z = self.DATAs = r.readFArray(lambda z: RefField[IDLERecord](IDLERecord, r, 4), dataSize >> 2)
            case _: z = Record._empty
        return z
# end::IDLE[]

# INFO.Dialog Topic Info - 3450 - tag::INFO[]
# dep: QUSTRecord, DIALRecord, SCPTRecord
class INFORecord(Record):
    # TES3
    class DATA3Field:
        def __init__(self, r: Header, dataSize: int):
            self.unknown1: int = r.readInt32()
            self.disposition: int = r.readInt32()
            self.rank: int = r.readByte() # (0-10)
            self.gender: int = r.readByte() # 0xFF = None, 0x00 = Male, 0x01 = Female
            self.pcRank: int = r.readByte() # (0-10)
            self.unknown2: int = r.readByte()

    class TES3Group:
        NNAM: STRVField # Next info ID (form a linked list of INFOs for the DIAL). First INFO has an empty PNAM, last has an empty NNAM.
        DATA: 'DATA3Field' # Info data
        ONAM: STRVField # Actor
        RNAM: STRVField # Race
        CNAM: STRVField # Class
        FNAM: STRVField # Faction 
        ANAM: STRVField # Cell
        DNAM: STRVField # PC Faction
        NAME: STRVField # The info response string (512 max)
        SNAM: FILEField # Sound
        QSTN: BYTEField # Journal Name
        QSTF: BYTEField # Journal Finished
        QSTR: BYTEField # Journal Restart
        SCVR: 'SCPTRecord.CTDAField' # String for the function/variable choice
        INTV: UNKNField #
        FLTV: UNKNField # The function/variable result for the previous SCVR
        BNAM: STRVField # Result text (not compiled)

    # TES4
    class DATA4Field:
        def __init__(self, r: Header, dataSize: int):
            self.type: int = r.readByte()
            self.nextSpeaker: int = r.readByte()
            self.flags: int = r.readByte() if dataSize == 3 else 0

    class TRDTField:
        emotionType: int
        emotionValue: int
        responseNumber: int
        responseText: str
        actorNotes: str

        def __init(self, r: Header, dataSize: int):
            self.emotionType = r.readUInt32()
            self.emotionValue = r.readInt32()
            r.skip(4) # Unused
            self.responseNumber = r.readByte()
            r.skip(3) # Unused
        def NAM1Field(self, r: Header, dataSize: int) -> object: z = self.responseText = r.readFUString(dataSize); return z
        def NAM2Field(self, r: Header, dataSize: int) -> object: z = self.actorNotes = r.readFUString(dataSize); return z

    class TES4Group:
        DATA: 'DATA4Field' # Info data
        QSTI: RefField['QUSTRecord'] # Quest
        TPIC: RefField[DIALRecord] # Topic
        NAMEs: list[RefField[DIALRecord]] = listx() # Topics
        TRDTs: list['TRDTField'] = listx() # Responses
        CTDAs: list['SCPTRecord.CTDAField'] = listx() # Conditions
        TCLTs: list[RefField[DIALRecord]] = listx() # Choices
        TCLFs: list[RefField[DIALRecord]] = listx() # Link From Topics
        SCHR: 'SCPTRecord.SCHRField' # Script Data
        SCDA: BYTVField # Compiled Script
        SCTX: STRVField # Script Source
        SCROs: list[RefField[Record]] = listx() # Global variable reference

    PNAM: RefField['INFORecord'] # Previous info ID
    TES3: TES3Group = TES3Group()
    TES4: TES4Group = TES4Group()

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        if r.format == FormType.TES3:
            match type:
                case FieldType.INAM: z = DIALRecord.lastRecord.INFOs.addX(self) if DIALRecord.lastRecord else None; self.EDID = r.readSTRV(dataSize)
                case FieldType.PNAM: z = self.PNAM = RefField[INFORecord](INFORecord, r, dataSize)
                case FieldType.NNAM: z = self.TES3.NNAM = r.readSTRV(dataSize)
                case FieldType.DATA: z = self.TES3.DATA = self.DATA3Field(r, dataSize)
                case FieldType.ONAM: z = self.TES3.ONAM = r.readSTRV(dataSize)
                case FieldType.RNAM: z = self.TES3.RNAM = r.readSTRV(dataSize)
                case FieldType.CNAM: z = self.TES3.CNAM = r.readSTRV(dataSize)
                case FieldType.FNAM: z = self.TES3.FNAM = r.readSTRV(dataSize)
                case FieldType.ANAM: z = self.TES3.ANAM = r.readSTRV(dataSize)
                case FieldType.DNAM: z = self.TES3.DNAM = r.readSTRV(dataSize)
                case FieldType.NAME: z = self.TES3.NAME = r.readSTRV(dataSize)
                case FieldType.SNAM: z = self.TES3.SNAM = r.readFILE(dataSize)
                case FieldType.QSTN: z = self.TES3.QSTN = r.readS(BYTEField, dataSize)
                case FieldType.QSTF: z = self.TES3.QSTF = r.readS(BYTEField, dataSize)
                case FieldType.QSTR: z = self.TES3.QSTR = r.readS(BYTEField, dataSize)
                case FieldType.SCVR: z = self.TES3.SCVR = SCPTRecord.CTDAField(r, dataSize)
                case FieldType.INTV: z = self.TES3.INTV = r.readUNKN(dataSize)
                case FieldType.FLTV: z = self.TES3.FLTV = r.readUNKN(dataSize)
                case FieldType.BNAM: z = self.TES3.BNAM = r.readSTRV(dataSize)
                case _: z = Record._empty
            return z
        match type:
            case FieldType.DATA: z = self.TES4.DATA = self.DATA4Field(r, dataSize)
            case FieldType.QSTI: z = self.TES4.QSTI = RefField[QUSTRecord](QUSTRecord, r, dataSize)
            case FieldType.TPIC: z = self.TES4.TPIC = RefField[DIALRecord](DIALRecord, r, dataSize)
            case FieldType.NAME: z = self.TES4.NAMEs.addX(RefField[DIALRecord](DIALRecord, r, dataSize))
            case FieldType.TRDT: z = self.TES4.TRDTs.addX(self.TRDTField(r, dataSize))
            case FieldType.NAM1: z = self.TES4.TRDTs.last().NAM1Field(r, dataSize)
            case FieldType.NAM2: z = self.TES4.TRDTs.last().NAM2Field(r, dataSize)
            case FieldType.CTDA | FieldType.CTDT: z = self.TES4.CTDAs.addX(SCPTRecord.CTDAField(r, dataSize))
            case FieldType.TCLT: z = self.TES4.TCLTs.addX(RefField[DIALRecord](DIALRecord, r, dataSize))
            case FieldType.TCLF: z = self.TES4.TCLFs.addX(RefField[DIALRecord](DIALRecord, r, dataSize))
            case FieldType.SCHR | FieldType.SCHD: z = self.TES4.SCHR = SCPTRecord.SCHRField(r, dataSize)
            case FieldType.SCDA: z = self.TES4.SCDA = r.readBYTV(dataSize)
            case FieldType.SCTX: z = self.TES4.SCTX = r.readSTRV(dataSize)
            case FieldType.SCRO: z = self.TES4.SCROs.addX(RefField[Record](Record, r, dataSize))
            case _: z = Record._empty
        return z
# end::INFO[]

# INGR.Ingredient - 3450 - tag::INGR[]
# dep: ENCHRecord, SCPTRecord
class INGRRecord(Record, IHaveMODL):
    # TES3
    class IRDTField:
        def __init__(self, r: Header, dataSize: int):
            self.weight: float = r.readSingle()
            self.value: int = r.readInt32()
            self.effectId: list[int] = [r.readInt32(), r.readInt32(), r.readInt32(), r.readInt32()] # 0 or -1 means no effect
            self.skillId: list[int] = [r.readInt32(), r.readInt32(), r.readInt32(), r.readInt32()] # only for Skill related effects, 0 or -1 otherwise
            self.attributeId: list[int] = [r.readInt32(), r.readInt32(), r.readInt32(), r.readInt32()] # only for Attribute related effects, 0 or -1 otherwise

    # TES4
    class DATAField:
        def __init__(self, r: Header, dataSize: int):
            self.weight: float = r.readSingle()
            self.value: int = 0
            self.flags: int = 0
        def ENITField(self, r: Header, dataSize: int) -> object: z = self.value = r.readInt32(); self.flags = r.readUInt32(); return z

    MODL: MODLGroup # Model Name
    FULL: STRVField # Item Name
    IRDT: IRDTField # Ingrediant Data # TES3
    DATA: DATAField # Ingrediant Data # TES4
    ICON: FILEField # Inventory Icon
    SCRI: RefField['SCPTRecord'] # Script Name
    # TES4
    EFITs: list[ENCHRecord.EFITField] = listx() # Effect Data
    SCITs: list[ENCHRecord.SCITField] = listx() # Script effect data

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize)
            case FieldType.FULL if len(self.SCITs) == 0: z = self.FULL = r.readSTRV(dataSize) #:matchif
            case FieldType.FULL if len(self.SCITs) != 0: z = self.SCITs.last().FULLField(r, dataSize) #:matchif
            case FieldType.FNAM: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.DATA: z = self.DATA = self.DATAField(r, dataSize)
            case FieldType.IRDT: z = self.IRDT = self.IRDTField(r, dataSize)
            case FieldType.ICON | FieldType.ITEX: z = self.ICON = r.readFILE(dataSize)
            case FieldType.SCRI: z = self.SCRI = RefField[SCPTRecord](SCPTRecord, r, dataSize)
            #
            case FieldType.ENIT: z = self.DATA.ENITField(r, dataSize)
            case FieldType.EFID: z = r.skip(dataSize)
            case FieldType.EFIT: z = self.EFITs.addX(ENCHRecord.EFITField(r, dataSize))
            case FieldType.SCIT: z = self.SCITs.addX(ENCHRecord.SCITField(r, dataSize))
            case _: z = Record._empty
        return z
# end::INGR[]

# KEYM.Key - 0400 - tag::KEYM[]
# dep: SCPTRecord
class KEYMRecord(Record, IHaveMODL):
    class DATAField:
        def __init__(self, r: Header, dataSize: int):
            self.value: int = r.readInt32()
            self.weight: float = r.readSingle()

    MODL: MODLGroup # Model
    FULL: STRVField # Item Name
    SCRI: RefField['SCPTRecord'] # Script (optional)
    DATA: DATAField # Type of soul contained in the gem
    ICON: FILEField # Icon (optional)

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize)
            case FieldType.FULL: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.SCRI: z = self.SCRI = RefField[SCPTRecord](SCPTRecord, r, dataSize)
            case FieldType.DATA: z = self.DATA = self.DATAField(r, dataSize)
            case FieldType.ICON: z = self.ICON = r.readFILE(dataSize)
            case _: z = Record._empty
        return z
# end::KEYM[]

# LAND.Land - 3450 - tag::LAND[]
# dep: None
class LANDRecord(Record):
    # TESX
    class VNMLField:
        def __init__(self, r: Header, dataSize: int):
            self.vertexs: list[Byte3] = r.readPArray(Byte3, '3B', dataSize // 3) # XYZ 8 bit floats

    class VHGTField:
        referenceHeight: float # A height offset for the entire cell. Decreasing this value will shift the entire cell land down.
        heightData: list[int] # HeightData

        def __init__(self, r: Header, dataSize: int):
            self.referenceHeight = r.readSingle()
            count = dataSize - 4 - 3
            self.heightData = r.readPArray(None, 'b', count)
            r.skip(3) # Unused

    class VCLRField:
        def __init__(self, r: Header, dataSize: int):
            self.colors: list[ByteColor3] = r.readSArray(ByteColor3, dataSize // 24) # 24-bit RGB

    class VTEXField:
        textureIndicesT3: list[int]
        textureIndicesT4: list[int]

        def __init__(self, r: Header, dataSize: int):
            if r.format == FormType.TES3:
                self.textureIndicesT3 = r.readPArray(None, 'H', dataSize >> 1)
                self.textureIndicesT4 = None
                return
            self.textureIndicesT3 = None
            self.textureIndicesT4 = r.readPArray(None, 'I', dataSize >> 2)

    # TES3
    class CORDField:
        def __repr__(self): return f'{self.cellX},{self.cellY}'
        _struct = ('<2i', 8)
        def __init__(self, tuple):
            (self.cellX,
            self.cellY) = tuple

    class WNAMField:
        # Low-LOD heightmap (signed chars)
        def __init__(self, r: Header, dataSize: int):
            r.skip(dataSize)
            #var heightCount = dataSize;
            #for (var i = 0; i < heightCount; i++) { var height = r.readByte(); }

    # TES4
    class BTXTField:
        _struct = ('<I2ch', 8)
        def __init__(self, tuple):
            (self.texture,
            self.quadrant,
            self.pad01,
            self.layer) = tuple

    class VTXTField:
        _struct = ('<2Hf', 8)
        def __init__(self, tuple):
            (self.position,
            self.pad01,
            self.opacity) = tuple

    class ATXTGroup:
        ATXT: 'BTXTField'
        VTXTs: list['VTXTField']

    def __repr__(self): return f'LAND: {self.INTV}'
    DATA: IN32Field # Unknown (default of 0x09) Changing this value makes the land 'disappear' in the editor.
    # A RGB color map 65x65 pixels in size representing the land normal vectors.
    # The signed value of the 'color' represents the vector's component. Blue
    # is vertical(Z), Red the X direction and Green the Y direction.Note that
    # the y-direction of the data is from the bottom up.
    VNML: VNMLField
    VHGT: VHGTField # Height data
    VCLR: VNMLField # Vertex color array, looks like another RBG image 65x65 pixels in size. (Optional)
    VTEX: VTEXField # A 16x16 array of short texture indices. (Optional)
    # TES3
    INTV: CORDField # The cell coordinates of the cell
    WNAM: WNAMField # Unknown byte data.
    # TES4
    BTXTs: list[BTXTField] = [None]*4 # Base Layer
    ATXTs: list[ATXTGroup] # Alpha Layer
    _lastATXT: ATXTGroup

    gridId: Int3 # => Int3(INTV.CellX, INTV.CellY, 0);

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.DATA: z = self.DATA = r.readS(IN32Field, dataSize)
            case FieldType.VNML: z = self.VNML = self.VNMLField(r, dataSize)
            case FieldType.VHGT: z = self.VHGT = self.VHGTField(r, dataSize)
            case FieldType.VCLR: z = self.VCLR = self.VNMLField(r, dataSize)
            case FieldType.VTEX: z = self.VTEX = self.VTEXField(r, dataSize)
            # TES3
            case FieldType.INTV: z = self.INTV = r.readS(self.CORDField, dataSize)
            case FieldType.WNAM: z = self.WNAM = self.WNAMField(r, dataSize)
            # TES4
            case FieldType.BTXT: z = self.then(r.readS(self.BTXTField, dataSize), lambda v: self.BTXTs.__setitem__(v.quadrant, v))
            case FieldType.ATXT: z = _nca(self, 'ATXTs', listx([None]*4)); self.then(r.readS(self.BTXTField, dataSize), lambda v: ((z := self.ATXTGroup(ATXT = v), setattr(self, '_lastATXT', z), self.ATXTs.__setitem__(v.quadrant, z))))
            case FieldType.VTXT: z = self._lastATXT.VTXTs = r.readSArray(self.VTXTField, dataSize >> 3)
            case _: z = Record._empty
        return z
# end::LAND[]

# LEVC.Leveled Creature - 3000 - tag::LEVC[]
# dep: None
class LEVCRecord(Record):
    DATA: IN32Field # List data - 1 = Calc from all levels <= PC level
    NNAM: BYTEField # Chance None?
    INDX: IN32Field # Number of items in list
    CNAMs: list[STRVField] = listx() # ID string of list item
    INTVs: list[IN16Field] = listx() # PC level for previous CNAM
    # The CNAM/INTV can occur many times in pairs

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        if r.format == FormType.TES3:
            match type:
                case FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
                case FieldType.DATA: z = self.DATA = r.readS(IN32Field, dataSize)
                case FieldType.NNAM: z = self.NNAM = r.readS(BYTEField, dataSize)
                case FieldType.INDX: z = self.INDX = r.readS(IN32Field, dataSize)
                case FieldType.CNAM: z = self.CNAMs.addX(r.readSTRV(dataSize))
                case FieldType.INTV: z = self.INTVs.addX(r.readS(IN16Field, dataSize))
                case _: z = Record._empty
            return z
        return None
# end::LEVC[]

# LEVI.Leveled item - 3000 - tag::LEVI[]
# dep: None
class LEVIRecord(Record):
    DATA: IN32Field # List data - 1 = Calc from all levels <= PC level, 2 = Calc for each item
    NNAM: BYTEField # Chance None?
    INDX: IN32Field # Number of items in list
    INAMs: list[STRVField] = listx() # ID string of list item
    INTVs: list[IN16Field] = listx() # PC level for previous INAM
    # The CNAM/INTV can occur many times in pairs

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        if r.format == FormType.TES3:
            match type:
                case FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
                case FieldType.DATA: z = self.DATA = r.readS(IN32Field, dataSize)
                case FieldType.NNAM: z = self.NNAM = r.readS(BYTEField, dataSize)
                case FieldType.INDX: z = self.INDX = r.readS(IN32Field, dataSize)
                case FieldType.INAM: z = self.INAMs.addX(r.readSTRV(dataSize))
                case FieldType.INTV: z = self.INTVs.addX(r.readS(IN16Field, dataSize))
                case _: z = Record._empty
            return z
        return None
# end::LEVI[]

# LIGH.Light - 3450 - tag::LIGH[]
# dep: SCPTRecord, SOUNRecord
class LIGHRecord(Record, IHaveMODL):
    # TESX
    class DATAField:
        class ColorFlags(Flag):
            Dynamic = 0x0001
            CanCarry = 0x0002
            Negative = 0x0004
            Flicker = 0x0008
            Fire = 0x0010
            OffDefault = 0x0020
            FlickerSlow = 0x0040
            Pulse = 0x0080
            PulseSlow = 0x0100

        weight: float
        value: int
        time: int
        radius: int
        lightColor: ByteColor4
        flags: int
        # TES4
        falloffExponent: float
        fov: float

        def __init__(self, r: Header, dataSize: int):
            if r.format == FormType.TES3:
                self.weight = r.readSingle()
                self.value = r.readInt32()
                self.time = r.readInt32()
                self.radius = r.readInt32()
                self.lightColor = r.readS(ByteColor4, 4)
                self.flags = r.readInt32()
                self.falloffExponent = 1
                self.fov = 90
                return
            self.time = r.readInt32()
            self.radius = r.readInt32()
            self.lightColor = r.readS(ByteColor4, 4)
            self.flags = r.readInt32()
            if dataSize == 32: self.falloffExponent = r.readSingle(); self.fov = r.readSingle()
            else: self.FalloffExponent = 1; self.fov = 90
            self.value = r.readInt32()
            self.weight = r.readSingle()

    MODL: MODLGroup # Model
    FULL: STRVField # Item Name (optional)
    DATA: DATAField # Light Data
    SCPT: STRVField # Script Name (optional)??
    SCRI: RefField['SCPTRecord'] # Script FormId (optional)
    ICON: FILEField # Male Icon (optional)
    FNAM: FLTVField # Fade Value
    SNAM: RefField['SOUNRecord'] # Sound FormId (optional)

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.FULL: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.FNAM if r.format != FormType.TES3: z = self.FNAM = r.readS(FLTVField, dataSize) #:matchif
            case FieldType.FNAM if r.format == FormType.TES3: z = self.FULL = r.readSTRV(dataSize) #:matchif
            case FieldType.DATA | FieldType.LHDT: z = self.DATA = self.DATAField(r, dataSize)
            case FieldType.SCPT: z = self.SCPT = r.readSTRV(dataSize)
            case FieldType.SCRI: z = self.SCRI = RefField[SCPTRecord](SCPTRecord, r, dataSize)
            case FieldType.ICON | FieldType.ITEX: z = self.ICON = r.readFILE(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize)
            case FieldType.SNAM: z = self.SNAM = RefField[SOUNRecord](SOUNRecord, r, dataSize)
            case _: z = Record._empty
        return z
# end::LIGH[]

# LOCK.Lock - 3450 - tag::LOCK[]
# dep: SCPTRecord
class LOCKRecord(Record, IHaveMODL):
    class LKDTField:
        def __init__(self, r: Header, dataSize: int):
            self.weight: float = r.readSingle()
            self.value: int = r.readInt32()
            self.quality: float = r.readSingle()
            self.uses: int = r.readInt32()

    MODL: MODLGroup # Model Name
    FNAM: STRVField # Item Name
    LKDT: LKDTField # Lock Data
    ICON: FILEField # Inventory Icon
    SCRI: RefField['SCPTRecord'] # Script Name

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        if r.format == FormType.TES3:
            match type:
                case FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
                case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
                case FieldType.FNAM: z = self.FNAM = r.readSTRV(dataSize)
                case FieldType.LKDT: z = self.LKDT = self.LKDTField(r, dataSize)
                case FieldType.ITEX: z = self.ICON = r.readFILE(dataSize)
                case FieldType.SCRI: z = self.SCRI = RefField[SCPTRecord](SCPTRecord, r, dataSize)
                case _: z = Record._empty
            return z
        return None
# end::LOCK[]

# LSCR.Load Screen - 0450 - tag::LSCR[]
# dep: WRLDRecord
class LSCRRecord(Record):
    class LNAMField:
        def __init__(self, r: Header, dataSize: int):
            self.direct: Ref[Record] = Ref[Record](Record, r.readUInt32())
            self.indirectWorld: Ref[WRLDRecord] = Ref[WRLDRecord](WRLDRecord, r.readUInt32())
            self.indirectGridX: int = r.readInt16()
            self.indirectGridY: int = r.readInt16()

    ICON: FILEField # Icon
    DESC: STRVField # Description
    LNAMs: list[LNAMField] # LoadForm

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.ICON: z = self.ICON = r.readFILE(dataSize)
            case FieldType.DESC: z = self.DESC = r.readSTRV(dataSize)
            case FieldType.LNAM: z = _nca(self, 'LNAMs', listx()).addX(self.LNAMField(r, dataSize))
            case _: z = Record._empty
        return z
# end::LSCR[]

# LTEX.Land Texture - 3450 - tag::LTEX[]
# dep: GRASRecord
class LTEXRecord(Record):
    class HNAMField:
        def __init__(self, r: Header, dataSize: int):
            self.materialType: byte = r.readByte()
            self.friction: byte = r.readByte()
            self.restitution: byte = r.readByte()

    ICON: FILEField # Texture
    # TES3
    INTV: INTVField
    # TES4
    HNAM: HNAMField # Havok data
    SNAM: BYTEField # Texture specular exponent
    GNAMs: list[RefField[GRASRecord]] = listx() # Potential grass

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readSTRV(dataSize),
            case FieldType.INTV: z = self.INTV = r.readINTV(dataSize),
            case FieldType.ICON | FieldType.DATA: z = self.ICON = r.readFILE(dataSize),
            # TES4
            case FieldType.HNAM: z = self.HNAM = self.HNAMField(r, dataSize),
            case FieldType.SNAM: z = self.SNAM = r.readS(BYTEField, dataSize),
            case FieldType.GNAM: z = self.GNAMs.addX(RefField[GRASRecord](GRASRecord, r, dataSize)),
            case _: z = Record._empty
        return z
# end::LTEX[]

# LVLC.Leveled Creature - 0400 - tag::LVLC[]
# dep: CREARecord, LVLIRecord, SCPTRecord
class LVLCRecord(Record):
    LVLD: BYTEField # Chance
    LVLF: BYTEField # Flags - 0x01 = Calculate from all levels <= player's level, 0x02 = Calculate for each item in count
    SCRI: RefField['SCPTRecord'] # Script (optional)
    TNAM: RefField[CREARecord] # Creature Template (optional)
    LVLOs: list['LVLIRecord.LVLOField'] = listx()

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.LVLD: z = self.LVLD = r.readS(BYTEField, dataSize)
            case FieldType.LVLF: z = self.LVLF = r.readS(BYTEField, dataSize)
            case FieldType.SCRI: z = self.SCRI = RefField[SCPTRecord](SCPTRecord, r, dataSize)
            case FieldType.TNAM: z = self.TNAM = RefField[CREARecord](CREARecord, r, dataSize)
            case FieldType.LVLO: z = self.LVLOs.addX(LVLIRecord.LVLOField(r, dataSize))
            case _: z = Record._empty
        return z
# end::LVLC[]

# LVLI.Leveled Item - 0400 - tag::LVLI[]
# dep: None
class LVLIRecord(Record):
    class LVLOField:
        level: int
        itemFormId: Ref[Record]
        count: int

        def __init__(self, r: Header, dataSize: int):
            self.level = r.readInt16()
            r.skip(2) # Unused
            self.itemFormId = Ref[Record](Record, r.readUInt32())
            if dataSize == 12:
                self.count = r.readInt16()
                r.skip(2) # Unused
            else: self.count = 0

    LVLD: BYTEField # Chance
    LVLF: BYTEField # Flags - 0x01 = Calculate from all levels <= player's level, 0x02 = Calculate for each item in count
    DATA: BYTEField # Data (optional)
    LVLOs: list[LVLOField] = listx()

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize),
            case FieldType.LVLD: z = self.LVLD = r.readS(BYTEField, dataSize),
            case FieldType.LVLF: z = self.LVLF = r.readS(BYTEField, dataSize),
            case FieldType.DATA: z = self.DATA = r.readS(BYTEField, dataSize),
            case FieldType.LVLO: z = self.LVLOs.addX(self.LVLOField(r, dataSize)),
            case _: z = Record._empty
        return z
# end::LVLI[]

# LVSP.Leveled Spell - 0400 - tag::LVSP[]
# dep: LVLIRecord
class LVSPRecord(Record):
    LVLD: BYTEField # Chance
    LVLF: BYTEField # Flags
    LVLOs: list[LVLIRecord.LVLOField] = listx() # Number of items in list

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize),
            case FieldType.LVLD: z = self.LVLD = r.readS(BYTEField, dataSize),
            case FieldType.LVLF: z = self.LVLF = r.readS(BYTEField, dataSize),
            case FieldType.LVLO: z = self.LVLOs.addX(LVLIRecord.LVLOField(r, dataSize)),
            case _: z = Record._empty
        return z
# end::LVSP[]

# MGEF.Magic Effect - 3400 - tag::MGEF[]
# dep: ^EFSHRecord, LIGHRecord, SOUNRecord
class MGEFRecord(Record):
    # TES3
    class MEDTField:
        def __init__(self, r: Header, dataSize: int):
            self.spellSchool: int = r.readInt32() # 0 = Alteration, 1 = Conjuration, 2 = Destruction, 3 = Illusion, 4 = Mysticism, 5 = Restoration
            self.baseCost: float = r.readSingle()
            self.flags: int = r.readInt32() # 0x0200 = Spellmaking, 0x0400 = Enchanting, 0x0800 = Negative
            self.color: ByteColor4 = ByteColor4(r.readInt32() & 0xFF, r.readInt32() & 0xFF, r.readInt32() & 0xFF, 255)
            self.speedX: float = r.readSingle()
            self.sizeX: float = r.readSingle()
            self.sizeCap: float = r.readSingle()

    # TES4
    class MFEGFlag(Flag):
        Hostile = 0x00000001
        Recover = 0x00000002
        Detrimental = 0x00000004
        MagnitudePercent = 0x00000008
        Self = 0x00000010
        Touch = 0x00000020
        Target = 0x00000040
        NoDuration = 0x00000080
        NoMagnitude = 0x00000100
        NoArea = 0x00000200
        FXPersist = 0x00000400
        Spellmaking = 0x00000800
        Enchanting = 0x00001000
        NoIngredient = 0x00002000
        Unknown14 = 0x00004000
        Unknown15 = 0x00008000
        UseWeapon = 0x00010000
        UseArmor = 0x00020000
        UseCreature = 0x00040000
        UseSkill = 0x00080000
        UseAttribute = 0x00100000
        Unknown21 = 0x00200000
        Unknown22 = 0x00400000
        Unknown23 = 0x00800000
        UseActorValue = 0x01000000
        SprayProjectileType = 0x02000000 # (Ball if Spray, Bolt or Fog is not specified)
        BoltProjectileType = 0x04000000
        NoHitEffect = 0x08000000
        Unknown28 = 0x10000000
        Unknown29 = 0x20000000
        Unknown30 = 0x40000000
        Unknown31 = 0x80000000

    class DATAField:
        flags: int
        baseCost: float
        assocItem: int
        magicSchool: int
        resistValue: int
        counterEffectCount: int # Must be updated automatically when ESCE length changes!
        light: Ref[LIGHRecord]
        projectileSpeed: float
        effectShader: Ref[EFSHRecord]
        enchantEffect: Ref[EFSHRecord]
        castingSound: Ref['SOUNRecord']
        boltSound: Ref['SOUNRecord']
        hitSound: Ref['SOUNRecord']
        areaSound: Ref['SOUNRecord']
        constantEffectEnchantmentFactor: float
        constantEffectBarterFactor: float

        def __init__(self, r: Header, dataSize: int):
            self.flags = r.readUInt32()
            self.baseCost = r.readSingle()
            self.assocItem = r.readInt32()
            #wbUnion('Assoc. Item', wbMGEFFAssocItemDecider, [
            #  wbFormIDCk('Unused', [NULL]),
            #  wbFormIDCk('Assoc. Weapon', [WEAP]),
            #  wbFormIDCk('Assoc. Armor', [ARMO, NULL{?}]),
            #  wbFormIDCk('Assoc. Creature', [CREA, LVLC, NPC_]),
            #  wbInteger('Assoc. Actor Value', itS32, wbActorValueEnum)
            self.magicSchool = r.readInt32()
            self.resistValue = r.readInt32()
            self.counterEffectCount = r.ReadUInt16()
            r.skip(2) # Unused
            self.light = Ref[LIGHRecord](LIGHRecord, r.readUInt32())
            self.projectileSpeed = r.readSingle()
            self.effectShader = Ref[EFSHRecord](EFSHRecord, r.readUInt32())
            if dataSize == 36: return
            self.enchantEffect = Ref[EFSHRecord](EFSHRecord, r.readUInt32())
            self.castingSound = Ref[SOUNRecord](SOUNRecord, r.readUInt32())
            self.boltSound = Ref[SOUNRecord](SOUNRecord, r.readUInt32())
            self.hitSound = Ref[SOUNRecord](SOUNRecord, r.readUInt32())
            self.areaSound = Ref[SOUNRecord](SOUNRecord, r.readUInt32())
            self.constantEffectEnchantmentFactor = r.readSingle()
            self.constantEffectBarterFactor = r.readSingle()

    def __repl__(self): return f'MGEF: {self.INDX.value}:{self.EDID.value}'
    DESC: STRVField # Description
    # TES3
    INDX: INTVField # The Effect ID (0 to 137)
    MEDT: MEDTField # Effect Data
    ICON: FILEField # Effect Icon
    PTEX: STRVField # Particle texture
    CVFX: STRVField # Casting visual
    BVFX: STRVField # Bolt visual
    HVFX: STRVField # Hit visual
    AVFX: STRVField # Area visual
    CSND: STRVField # Cast sound (optional)
    BSND: STRVField # Bolt sound (optional)
    HSND: STRVField # Hit sound (optional)
    ASND: STRVField # Area sound (optional)
    # TES4
    FULL: STRVField
    MODL: MODLGroup
    DATA: DATAField
    ESCEs: list[STRVField]

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        if r.format == FormType.TES3:
            match type:
                case FieldType.INDX: z = self.INDX = r.readINTV(dataSize)
                case FieldType.MEDT: z = self.MEDT = self.MEDTField(r, dataSize)
                case FieldType.ITEX: z = self.ICON = r.readFILE(dataSize)
                case FieldType.PTEX: z = self.PTEX = r.readSTRV(dataSize)
                case FieldType.CVFX: z = self.CVFX = r.readSTRV(dataSize)
                case FieldType.BVFX: z = self.BVFX = r.readSTRV(dataSize)
                case FieldType.HVFX: z = self.HVFX = r.readSTRV(dataSize)
                case FieldType.AVFX: z = self.AVFX = r.readSTRV(dataSize)
                case FieldType.DESC: z = self.DESC = r.readSTRV(dataSize)
                case FieldType.CSND: z = self.CSND = r.readSTRV(dataSize)
                case FieldType.BSND: z = self.BSND = r.readSTRV(dataSize)
                case FieldType.HSND: z = self.HSND = r.readSTRV(dataSize)
                case FieldType.ASND: z = self.ASND = r.readSTRV(dataSize)
                case _: z = Record._empty
            return z
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.FULL: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.DESC: z = self.DESC = r.readSTRV(dataSize)
            case FieldType.ICON: z = self.ICON = r.readFILE(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.DATA: z = self.DATA = DATAField(r, dataSize)
            case FieldType.ESCE: z = self.ESCEs = r.readFArray(lambda z: r.readSTRV(4), dataSize >> 2)
            case _: z = Record._empty
        return z
# end::MGEF[]

# MISC.Misc Item - 3450 - tag::MISC[]
# dep: ENCHRecord, SCPTRecord
class MISCRecord(Record, IHaveMODL):
    # TESX
    class DATAField:
        weight: float
        value: int
        unknown: int

        def __init__(self, r: Header, dataSize: int):
            if r.format == FormType.TES3:
                self.weight = r.readSingle()
                self.value = r.readUInt32()
                self.unknown = r.readUInt32()
                return
            self.value = r.readUInt32()
            self.weight = r.readSingle()
            self.unknown = 0

    MODL: MODLGroup # Model
    FULL: STRVField # Item Name
    DATA: DATAField # Misc Item Data
    ICON: FILEField # Icon (optional)
    SCRI: RefField['SCPTRecord'] # Script FormID (optional)
    # TES3
    ENAM: RefField[ENCHRecord] # enchantment ID

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readSTRV(dataSize),
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize),
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize),
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize),
            case FieldType.FULL | FieldType.FNAM: z = self.FULL = r.readSTRV(dataSize),
            case FieldType.DATA | FieldType.MCDT: z = self.DATA = self.DATAField(r, dataSize),
            case FieldType.ICON | FieldType.ITEX: z = self.ICON = r.readFILE(dataSize),
            case FieldType.ENAM: z = self.ENAM = RefField[ENCHRecord](ENCHRecord, r, dataSize),
            case FieldType.SCRI: z = self.SCRI = RefField[SCPTRecord](SCPTRecord, r, dataSize),
            case _: z = Record._empty
        return z
# end::MISC[]

# NPC_.Non-Player Character - 3450 - tag::NPC_[]
# dep: CREARecord, SCPTRecord
class NPC_Record(Record, IHaveMODL):
    class NPC_Flags(Flag):
        Female = 0x0001
        Essential = 0x0002
        Respawn = 0x0004
        None_ = 0x0008
        Autocalc = 0x0010
        BloodSkel = 0x0400
        BloodMetal = 0x0800

    class NPDTField:
        level: int
        strength: int
        intelligence: int
        willpower: int
        agility: int
        speed: int
        endurance: int
        personality: int
        luck: int
        skills: bytes
        reputation: int
        health: int
        spellPts: int
        fatigue: int
        disposition: int
        factionId: int
        rank: int
        unknown1: int
        gold: int
        # 12 byte version
        # level: int
        # disposition: int
        # factionId: int
        # rank: int
        # unknown1: int
        Unknown2: int
        Unknown3: int
        # gold: int

        def __init__(self, r: Header, dataSize: int):
            if dataSize == 52:
                self.level = r.readInt16()
                self.strength = r.readByte()
                self.intelligence = r.readByte()
                self.willpower = r.readByte()
                self.agility = r.readByte()
                self.speed = r.readByte()
                self.endurance = r.readByte()
                self.personality = r.readByte()
                self.luck = r.readByte()
                self.skills = r.readBytes(27)
                self.reputation = r.readByte()
                self.health = r.readInt16()
                self.spellPts = r.readInt16()
                self.fatigue = r.readInt16()
                self.disposition = r.readByte()
                self.factionId = r.readByte()
                self.rank = r.readByte()
                self.unknown1 = r.readByte()
                self.gold = r.readInt32()
            else:
                self.level = r.readInt16()
                self.disposition = r.readByte()
                self.factionId = r.readByte()
                self.rank = r.readByte()
                self.unknown1 = r.readByte()
                self.unknown2 = r.readByte()
                self.unknown3 = r.readByte()
                self.gold = r.readInt32()

    class DODTField:
        def __init__(self, r: Header, dataSize: int):
            self.xPos: float = r.readSingle()
            self.yPos: float = r.readSingle()
            self.zPos: float = r.readSingle()
            self.xRot: float = r.readSingle()
            self.yRot: float = r.readSingle()
            self.zRot: float = r.readSingle()

    FULL: STRVField # NPC name
    MODL: MODLGroup # Animation
    RNAM: STRVField # Race Name
    ANAM: STRVField # Faction name
    BNAM: STRVField # Head model
    CNAM: STRVField # Class name
    KNAM: STRVField # Hair model
    NPDT: NPDTField # NPC Data
    FLAG: INTVField # NPC Flags
    NPCOs: list[CNTOField] = listx() # NPC item
    NPCSs: list[STRVField] = listx() # NPC spell
    AIDT: CREARecord.AIDTField # AI data
    AI_W: CREARecord.AI_WField # AI
    AI_T: CREARecord.AI_TField # AI Travel
    AI_F: CREARecord.AI_FField # AI Follow
    AI_E: CREARecord.AI_FField # AI Escort
    CNDT: STRVField # Cell escort/follow to string (optional)
    AI_A: CREARecord.AI_AField # AI Activate
    DODT: DODTField # Cell Travel Destination
    DNAM: STRVField # Cell name for previous DODT, if interior
    XSCL: FLTVField # Scale (optional) Only present if the scale is not 1.0
    SCRI: RefField['SCPTRecord'] # Unknown

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.FULL | FieldType.FNAM: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.RNAM: z = self.RNAM = r.readSTRV(dataSize)
            case FieldType.ANAM: z = self.ANAM = r.readSTRV(dataSize)
            case FieldType.BNAM: z = self.BNAM = r.readSTRV(dataSize)
            case FieldType.CNAM: z = self.CNAM = r.readSTRV(dataSize)
            case FieldType.KNAM: z = self.KNAM = r.readSTRV(dataSize)
            case FieldType.NPDT: z = self.NPDT = self.NPDTField(r, dataSize)
            case FieldType.FLAG: z = self.FLAG = r.readINTV(dataSize)
            case FieldType.NPCO: z = self.NPCOs.addX(CNTOField(r, dataSize))
            case FieldType.NPCS: z = self.NPCSs.addX(r.readSTRV_ZPad(dataSize))
            case FieldType.AIDT: z = self.AIDT = CREARecord.AIDTField(r, dataSize)
            case FieldType.AI_W: z = self.AI_W = CREARecord.AI_WField(r, dataSize)
            case FieldType.AI_T: z = self.AI_T = CREARecord.AI_TField(r, dataSize)
            case FieldType.AI_F: z = self.AI_F = CREARecord.AI_FField(r, dataSize)
            case FieldType.AI_E: z = self.AI_E = CREARecord.AI_FField(r, dataSize)
            case FieldType.CNDT: z = self.CNDT = r.readSTRV(dataSize)
            case FieldType.AI_A: z = self.AI_A = CREARecord.AI_AField(r, dataSize)
            case FieldType.DODT: z = self.DODT = self.DODTField(r, dataSize)
            case FieldType.DNAM: z = self.DNAM = r.readSTRV(dataSize)
            case FieldType.XSCL: z = self.XSCL = r.readS(FLTVField, dataSize)
            case FieldType.SCRI: z = self.SCRI = RefField[SCPTRecord](SCPTRecord, r, dataSize)
            case _: z = Record._empty
        return z
# end::NPC_[]

# PACK.AI Package - 0450 - tag::PACK[]
# dep: SCPTRecord
class PACKRecord(Record):
    class PKDTField:
        flags: int
        type: int

        def __init__(self, r: Header, dataSize: int):
            self.flags = r.ReadUInt16()
            self.type = r.readByte()
            r.skip(dataSize - 3) # Unused

    class PLDTField:
        def __init__(self, r: Header, dataSize: int):
            self.type: int = r.readInt32()
            self.target: int = r.readUInt32()
            self.radius: int = r.readInt32()

    class PSDTField:
        def __init__(self, r: Header, dataSize: int):
            self.month: int = r.readByte()
            self.dayOfWeek: int = r.readByte()
            self.date: int = r.readByte()
            self.time: int = r.readSByte()
            self.duration: int = r.readInt32()

    class PTDTField:
        def __init__(self, r: Header, dataSize: int):
            self.type: int = r.readInt32()
            self.target: int = r.readUInt32()
            self.count: int = r.readInt32()

    PKDT: PKDTField # General
    PLDT: PLDTField # Location
    PSDT: PSDTField # Schedule
    PTDT: PTDTField # Target
    CTDAs: list['SCPTRecord.CTDAField'] = listx() # Conditions

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize),
            case FieldType.PKDT: z = self.PKDT = self.PKDTField(r, dataSize),
            case FieldType.PLDT: z = self.PLDT = self.PLDTField(r, dataSize),
            case FieldType.PSDT: z = self.PSDT = self.PSDTField(r, dataSize),
            case FieldType.PTDT: z = self.PTDT = self.PTDTField(r, dataSize),
            case FieldType.CTDA | FieldType.CTDT: z = self.CTDAs.addX(SCPTRecord.CTDAField(r, dataSize)),
            case _: z = Record._empty
        return z
# end::PACK[]

# PGRD.Path grid - 3400 - tag::PGRD[]
# dep: REFRRecord
class PGRDRecord(Record):
    class DATAField:
        x: int
        y: int
        granularity: int
        pointCount: int

        def __init__(self, r: Header, dataSize: int):
            if r.format != FormType.TES3:
                self.x = self.y = self.granularity = 0
                self.pointCount = r.readInt16()
                return
            self.x = r.readInt32()
            self.y = r.readInt32()
            self.granularity = r.readInt16()
            self.pointCount = r.readInt16()

    class PGRPField:
        point: Vector3
        connections: int

        def __init__(self, r: Header, dataSize: int):
            self.point = r.readVector3()
            self.Connections = r.readByte()
            r.skip(3) # Unused

    class PGRRField:
        def __init__(self, r: Header, dataSize: int):
            self.startPointId: int = r.readInt16()
            self.endPointId: int = r.readInt16()

    class PGRIField:
        def __init__(self, r: Header, dataSize: int):
            self.pointId: int = r.readInt16()
            self.foreignNode: Vector3 = r.skip(2).readVector3() # 2:Unused (can merge back)

    class PGRLField:
        def __init__(self, r: Header, dataSize: int):
            self.reference: Ref[REFRRecord] = Ref[REFRRecord](REFRRecord, r.readUInt32())
            self.pointIds: list[int] = r.readFArray(lambda z: (r.readInt16(), r.skip(2))[0], (dataSize - 4) >> 2) # 2:Unused (can merge back)

    DATA: DATAField # Number of nodes
    PGRPs: list[PGRPField]
    PGRC: UNKNField
    PGAG: UNKNField
    PGRRs: list[PGRRField] # Point-to-Point Connections
    PGRLs: list[PGRLField] # Point-to-Reference Mappings
    PGRIs: list[PGRIField] # Inter-Cell Connections

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.DATA: z = self.DATA = self.DATAField(r, dataSize)
            case FieldType.PGRP: z = self.PGRPs = r.readFArray(lambda z: self.PGRPField(r, 16), dataSize >> 4)
            case FieldType.PGRC: z = self.PGRC = r.readUNKN(dataSize)
            case FieldType.PGAG: z = self.PGAG = r.readUNKN(dataSize)
            case FieldType.PGRR: z = self.PGRRs = r.readFArray(lambda z: self.PGRRField(r, 4), dataSize >> 2); r.skip(dataSize % 4)
            case FieldType.PGRL: z = _nca(self, 'PGRLs', listx()).addX(self.PGRLField(r, dataSize))
            case FieldType.PGRI: z = self.PGRIs = r.readFArray(lambda z: self.PGRIField(r, 16), dataSize >> 4)
            case _: z = Record._empty
        return z
# end::PGRD[]

# PROB.Probe - 3000 - tag::PROB[]
# dep: SCPTRecord
class PROBRecord(Record, IHaveMODL):
    class PBDTField:
        def __init__(self, r: Header, dataSize: int):
            self.weight: float = r.readSingle()
            self.value: int = r.readInt32()
            self.quality: float = r.readSingle()
            self.uses: int = r.readInt32()

    MODL: MODLGroup # Model Name
    FNAM: STRVField # Item Name
    PBDT: PBDTField # Probe Data
    ICON: FILEField # Inventory Icon
    SCRI: RefField['SCPTRecord'] # Script Name

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        if r.format == FormType.TES3:
            match type:
                case FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
                case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
                case FieldType.FNAM: z = self.FNAM = r.readSTRV(dataSize)
                case FieldType.PBDT: z = self.PBDT = self.PBDTField(r, dataSize)
                case FieldType.ITEX: z = self.ICON = r.readFILE(dataSize)
                case FieldType.SCRI: z = self.SCRI = RefField[SCPTRecord](SCPTRecord, r, dataSize)
                case _: z = Record._empty
            return z
        return None
# end::PROB[]

# QUST.Quest - 0450 - tag::QUST[]
# dep: SCPTRecord
class QUSTRecord(Record):
    class DATAField:
        def __init__(self, r: Header, dataSize: int):
            self.flags: int = r.readByte()
            self.priority: int = r.readByte()

    FULL: STRVField # Item Name
    ICON: FILEField # Icon
    DATA: DATAField # Icon
    SCRI: RefField['SCPTRecord'] # Script Name
    SCHR: 'SCPTRecord.SCHRField' # Script Data
    SCDA: BYTVField # Compiled Script
    SCTX: STRVField # Script Source
    SCROs: list[RefField[Record]] = listx() # Global variable reference

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.FULL: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.ICON: z = self.ICON = r.readFILE(dataSize)
            case FieldType.DATA: z = self.DATA = self.DATAField(r, dataSize)
            case FieldType.SCRI: z = self.SCRI = RefField[SCPTRecord](SCPTRecord, r, dataSize)
            case FieldType.CTDA: z = r.skip(dataSize)
            case FieldType.INDX: z = r.skip(dataSize)
            case FieldType.QSDT: z = r.skip(dataSize)
            case FieldType.CNAM: z = r.skip(dataSize)
            case FieldType.QSTA: z = r.skip(dataSize)
            case FieldType.SCHR: z = self.SCHR = SCPTRecord.SCHRField(r, dataSize)
            case FieldType.SCDA: z = self.SCDA = r.readBYTV(dataSize)
            case FieldType.SCTX: z = self.SCTX = r.readSTRV(dataSize)
            case FieldType.SCRO: z = self.SCROs.addX(RefField[Record](Record, r, dataSize))
            case _: z = Record._empty
        return z
# end::QUST[]

# RACE.Race_Creature type - 3450 - tag::RACE[]
# dep: EYESRecord, HAIRRecord
class RACERecord(Record):
    # TESX
    class DATAField:
        class RaceFlag(Flag):
            Playable = 0x00000001
            FaceGenHead = 0x00000002
            Child = 0x00000004
            TiltFrontBack = 0x00000008
            TiltLeftRight = 0x00000010
            NoShadow = 0x00000020
            Swims = 0x00000040
            Flies = 0x00000080
            Walks = 0x00000100
            Immobile = 0x00000200
            NotPushable = 0x00000400
            NoCombatInWater = 0x00000800
            NoRotatingToHeadTrack = 0x00001000
            DontShowBloodSpray = 0x00002000
            DontShowBloodDecal = 0x00004000
            UsesHeadTrackAnims = 0x00008000
            SpellsAlignWMagicNode = 0x00010000
            UseWorldRaycastsForFootIK = 0x00020000
            AllowRagdollCollision = 0x00040000
            RegenHPInCombat = 0x00080000
            CantOpenDoors = 0x00100000
            AllowPCDialogue = 0x00200000
            NoKnockdowns = 0x00400000
            AllowPickpocket = 0x00800000
            AlwaysUseProxyController = 0x01000000
            DontShowWeaponBlood = 0x02000000
            OverlayHeadPartList = 0x04000000 #{> Only one can be active <}
            OverrideHeadPartList = 0x08000000 #{> Only one can be active <}
            CanPickupItems = 0x10000000
            AllowMultipleMembraneShaders = 0x20000000
            CanDualWield = 0x40000000
            AvoidsRoads = 0x80000000

        class SkillBoost:
            skillId: int
            bonus: int

            def __init__(self, r: Header, dataSize: int):
                if r.format == FormType.TES3:
                    self.skillId = r.readInt32() & 0xFF
                    self.bonus = r.readInt32() & 0xFF
                    return
                self.skillId = r.readByte()
                self.bonus = r.readSByte()

        class RaceStats:
            height: float
            weight: float
            # Attributes
            strength: int
            intelligence: int
            willpower: int
            agility: int
            speed: int
            endurance: int
            personality: int
            luck: int

        skillBoosts: list[SkillBoost] # Skill Boosts
        male: RaceStats = RaceStats()
        female: RaceStats = RaceStats()
        flags: int # 1 = Playable 2 = Beast Race

        def __init__(self, r: Header, dataSize: int):
            if r.format == FormType.TES3:
                self.skillBoosts = r.readFArray(lambda z: self.SkillBoost(r, 8), 7)
                self.male.strength = r.readInt32() & 0xFF; self.female.strength = r.readInt32() & 0xFF
                self.male.intelligence = r.readInt32() & 0xFF; self.female.intelligence = r.readInt32() & 0xFF
                self.male.willpower = r.readInt32() & 0xFF; self.female.willpower = r.readInt32() & 0xFF
                self.male.agility = r.readInt32() & 0xFF; self.female.agility = r.readInt32() & 0xFF
                self.male.speed = r.readInt32() & 0xFF; self.female.speed = r.readInt32() & 0xFF
                self.male.endurance = r.readInt32() & 0xFF; self.female.endurance = r.readInt32() & 0xFF
                self.male.personality = r.readInt32() & 0xFF; self.female.personality = r.readInt32() & 0xFF
                self.male.luck = r.readInt32(); self.female.luck = r.readInt32() & 0xFF
                self.male.height = r.readSingle(); self.female.height = r.readSingle()
                self.male.weight = r.readSingle(); self.female.weight = r.readSingle()
                self.flags = r.readUInt32()
                return
            self.skillBoosts = r.readFArray(lambda z: self.SkillBoost(r, 2), 7)
            r.readInt16() # padding
            self.male.height = r.readSingle(); self.female.height = r.readSingle()
            self.male.weight = r.readSingle(); self.female.weight = r.readSingle()
            self.flags = r.readUInt32()

        def ATTRField(self, r: Header, dataSize: int):
            self.male.Strength = r.readByte()
            self.male.Intelligence = r.readByte()
            self.male.Willpower = r.readByte()
            self.male.Agility = r.readByte()
            self.male.Speed = r.readByte()
            self.male.Endurance = r.readByte()
            self.male.Personality = r.readByte()
            self.male.Luck = r.readByte()
            self.female.Strength = r.readByte()
            self.female.Intelligence = r.readByte()
            self.female.Willpower = r.readByte()
            self.female.Agility = r.readByte()
            self.female.Speed = r.readByte()
            self.female.Endurance = r.readByte()
            self.female.Personality = r.readByte()
            self.female.Luck = r.readByte()
            return self

    # TES4
    class FacePartGroup:
        class Indx(Enum): Head = 0; Ear_Male = 1; Ear_Female = 2; Mouth = 3; Teeth_Lower = 4; Teeth_Upper = 5; Tongue = 6; Eye_Left = 7; Eye_Right = 8
        def __init__(self, INDX: UI32Field = None):
            self.INDX: UI32Field = INDX
            self.MODL: MODLGroup = None 
            self.ICON: FILEField = None

    class BodyPartGroup:
        class Indx(Enum): UpperBody = 0; LowerBody = 1; Hand = 2; Foot = 3; Tail = 4
        def __init__(self, INDX: UI32Field = None):
            self.INDX: UI32Field = INDX
            self.ICON: FILEField = None

    class BodyGroup:
        MODL: FILEField
        MODB: FLTVField
        BodyParts: list['BodyPartGroup'] = listx()

    FULL: STRVField # Race name
    DESC: STRVField # Race description
    SPLOs: list[STRVField] = listx() # NPCs: Special power/ability name
    # TESX
    DATA: DATAField # RADT:DATA/ATTR: Race data/Base Attributes
    # TES4
    VNAM: Ref2Field['RACERecord'] # Voice
    DNAM: Ref2Field[HAIRRecord] # Default Hair
    CNAM: BYTEField # Default Hair Color
    PNAM: FLTVField # FaceGen - Main clamp
    UNAM: FLTVField # FaceGen - Face clamp
    XNAM: UNKNField # Unknown
    #
    HNAMs: list[RefField[HAIRRecord]] = listx()
    ENAMs: list[RefField[EYESRecord]] = listx()
    FGGS: BYTVField # FaceGen Geometry-Symmetric
    FGGA: BYTVField # FaceGen Geometry-Asymmetric
    FGTS: BYTVField # FaceGen Texture-Symmetric
    SNAM: UNKNField # Unknown

    # Parts
    FaceParts: list[FacePartGroup] = listx()
    Bodys: list[BodyGroup] = [BodyGroup(), BodyGroup()]
    _nameState: int
    _genderState: int

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        if r.format == FormType.TES3:
            match type:
                case FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
                case FieldType.FNAM: z = self.FULL = r.readSTRV(dataSize)
                case FieldType.RADT: z = self.DATA = self.DATAField(r, dataSize)
                case FieldType.NPCS: z = self.SPLOs.addX(r.readSTRV(dataSize))
                case FieldType.DESC: z = self.DESC = r.readSTRV(dataSize)
                case _: z = Record._empty
            return z
        if r.format == FormType.TES4:
            match self._nameState:
                # preamble
                case 0:
                    match type:
                        case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
                        case FieldType.FULL: z = self.FULL = r.readSTRV(dataSize)
                        case FieldType.DESC: z = self.DESC = r.readSTRV(dataSize)
                        case FieldType.DATA: z = self.DATA = self.DATAField(r, dataSize)
                        case FieldType.SPLO: z = self.SPLOs.addX(r.readSTRV(dataSize))
                        case FieldType.VNAM: z = self.VNAM = Ref2Field[RACERecord](RACERecord, r, dataSize)
                        case FieldType.DNAM: z = self.DNAM = Ref2Field[HAIRRecord](HAIRRecord, r, dataSize)
                        case FieldType.CNAM: z = self.CNAM = r.readS(BYTEField, dataSize)
                        case FieldType.PNAM: z = self.PNAM = r.readS(FLTVField, dataSize)
                        case FieldType.UNAM: z = self.UNAM = r.readS(FLTVField, dataSize)
                        case FieldType.XNAM: z = self.XNAM = r.readUNKN(dataSize)
                        case FieldType.ATTR: z = self.DATA.ATTRField(r, dataSize)
                        case FieldType.NAM0: self._nameState += 1
                        case _: z = Record._empty
                # face data
                case 1:
                    match type:
                        case FieldType.INDX: z = self.FaceParts.addX(self.FacePartGroup(INDX = r.readS(UI32Field, dataSize)))
                        case FieldType.MODL: z = self.FaceParts.last().MODL = MODLGroup(r, dataSize)
                        case FieldType.ICON: z = self.FaceParts.last().ICON = r.readFILE(dataSize)
                        case FieldType.MODB: z = self.FaceParts.last().MODL.MODBField(r, dataSize)
                        case FieldType.NAM1: self._nameState += 1
                        case _: z = Record._empty,
                # body data
                case 2:
                    match type:
                        case FieldType.MNAM: z = self._genderState = 0
                        case FieldType.FNAM: z = self._genderState = 1
                        case FieldType.MODL: z = self.Bodys[_genderState].MODL = r.readFILE(dataSize)
                        case FieldType.MODB: z = self.Bodys[_genderState].MODB = r.readS(FLTVField, dataSize)
                        case FieldType.INDX: z = self.Bodys[_genderState].BodyParts.addX(self.BodyPartGroup(INDX = r.readS(UI32Field, dataSize)))
                        case FieldType.ICON: z = self.Bodys[_genderState].BodyParts.last().ICON = r.readFILE(dataSize)
                        case FieldType.HNAM: z = self.HNAMs.addRangeX(r.readFArray(lambda z: RefField[HAIRRecord](HAIRRecord, r, 4), dataSize >> 2)); self._nameState += 1
                        case _: z = Record._empty,
                # postamble
                case 3:
                    match type:
                        case FieldType.HNAM: z = self.HNAMs.addRangeX(r.readFArray(lambda z: RefField[HAIRRecord](HAIRRecord, r, 4), dataSize >> 2))
                        case FieldType.ENAM: z = self.ENAMs.addRangeX(r.readFArray(lambda z: RefField[EYESRecord](EYESRecord, r, 4), dataSize >> 2))
                        case FieldType.FGGS: z = self.FGGS = r.readBYTV(dataSize)
                        case FieldType.FGGA: z = self.FGGA = r.readBYTV(dataSize)
                        case FieldType.FGTS: z = self.FGTS = r.readBYTV(dataSize)
                        case FieldType.SNAM: z = self.SNAM = r.readUNKN(dataSize)
                        case _: z = Record._empty,
                case _: z = Record._empty
            return z
        return None
# end::RACE[]

# REPA.Repair Item - 3000 - tag::REPA[]
# dep: SCPTRecord
class REPARecord(Record, IHaveMODL):
    class RIDTField:
        def __init__(self, r: Header, dataSize: int):
            self.weight: float = r.readSingle()
            self.value: int = r.readInt32()
            self.uses: int = r.readInt32()
            self.quality: float = r.readSingle()

    MODL: MODLGroup # Model Name
    FNAM: STRVField # Item Name
    RIDT: RIDTField # Repair Data
    ICON: FILEField # Inventory Icon
    SCRI: RefField['SCPTRecord'] # Script Name

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        if r.format == FormType.TES3:
            match type:
                case FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
                case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
                case FieldType.FNAM: z = self.FNAM = r.readSTRV(dataSize)
                case FieldType.RIDT: z = self.RIDT = self.RIDTField(r, dataSize)
                case FieldType.ITEX: z = self.ICON = r.readFILE(dataSize)
                case FieldType.SCRI: z = self.SCRI = RefField[SCPTRecord](SCPTRecord, r, dataSize)
                case _: z = Record._empty
            return z
        return None
# end::REPA[]

# REFR.Placed Object - 0450 - tag::REFR[]
# dep: CELLRecord, ^KEYMRecord
class REFRRecord(Record):
    class XTELField:
        def __init__(self, r: Header, dataSize: int):
            self.door: Ref[REFRRecord] = Ref[REFRRecord](REFRRecord, r.readUInt32())
            self.position: Vector3 = r.readVector3()
            self.rotation: Vector3 = r.readVector3()

    class DATAField:
        def __init__(self, r: Header, dataSize: int):
            self.position: Vector3 = r.readVector3()
            self.rotation: Vector3 = r.readVector3()

    class XLOCField:
        def __repr__(self): return f'{self.key}'
        lockLevel: int
        key: Ref['KEYMRecord'] 
        flags: int
        def __init__(self, r: Header, dataSize: int):
            self.lockLevel = r.readByte()
            r.skip(3); # Unused
            self.key = Ref[KEYMRecord](KEYMRecord, r.readUInt32())
            if dataSize == 16: r.skip(4) # Unused
            self.flags = r.readByte()
            r.skip(3) # Unused

    class XESPField:
        def __repr__(self): return f'{self.reference}'
        reference: Ref[Record]
        flags: int

        def __init__(self, r: Header, dataSize: int):
            self.reference = Ref[Record](Record, r.readUInt32())
            self.flags = r.readByte()
            r.skip(3) # Unused

    class XSEDField:
        def __repr__(self): return f'{self.seed}'
        Seed: int

        def __init__(self, r: Header, dataSize: int):
            self.seed = r.readByte()
            if dataSize == 4: r.skip(3) # Unused

    class XMRKGroup:
        def __repr__(self): return f'{self.FULL.value}'
        FNAM: BYTEField # Map Flags
        FULL: STRVField # Name
        TNAM: BYTEField # Type

    NAME: RefField[Record] # Base
    XTEL: XTELField # Teleport Destination (optional)
    DATA: DATAField # Position/Rotation
    XLOC: XLOCField # Lock information (optional)
    XOWNs: list['CELLRecord.XOWNGroup'] # Ownership (optional)
    XESP: XESPField # Enable Parent (optional)
    XTRG: RefField[Record] # Target (optional)
    XSED: XSEDField # SpeedTree (optional)
    XLOD: BYTVField # Distant LOD Data (optional)
    XCHG: FLTVField # Charge (optional)
    XHLT: FLTVField # Health (optional)
    XPCI: RefField['CELLRecord'] # Unused (optional)
    XLCM: IN32Field # Level Modifier (optional)
    XRTM: RefField['REFRRecord'] # Unknown (optional)
    XACT: UI32Field # Action Flag (optional)
    XCNT: IN32Field # Count (optional)
    XMRKs: list[XMRKGroup] # Ownership (optional)
    #ONAM: bool # Open by Default
    XRGD: BYTVField # Ragdoll Data (optional)
    XSCL: FLTVField # Scale (optional)
    XSOL: BYTEField # Contained Soul (optional)
    _nextFull: int

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.NAME: z = self.NAME = RefField[Record](Record, r, dataSize)
            case FieldType.XTEL: z = self.XTEL = self.XTELField(r, dataSize)
            case FieldType.DATA: z = self.DATA = self.DATAField(r, dataSize)
            case FieldType.XLOC: z = self.XLOC = self.XLOCField(r, dataSize)
            case FieldType.XOWN: z = _nca(self, 'XOWNs', listx()).addX(CELLRecord.XOWNGroup(XOWN = RefField[Record](Record, r, dataSize)))
            case FieldType.XRNK: z = self.XOWNs.last().XRNK = r.readS(IN32Field, dataSize)
            case FieldType.XGLB: z = self.XOWNs.last().XGLB = RefField[Record](Record, r, dataSize)
            case FieldType.XESP: z = self.XESP = self.XESPField(r, dataSize)
            case FieldType.XTRG: z = self.XTRG = RefField[Record](Record, r, dataSize)
            case FieldType.XSED: z = self.XSED = self.XSEDField(r, dataSize)
            case FieldType.XLOD: z = self.XLOD = r.readBYTV(dataSize)
            case FieldType.XCHG: z = self.XCHG = r.readS(FLTVField, dataSize)
            case FieldType.XHLT: z = self.XCHG = r.readS(FLTVField, dataSize)
            case FieldType.XPCI: z = self.XPCI = RefField[CELLRecord](CELLRecord, r, dataSize); self._nextFull = 1
            case FieldType.FULL if self._nextFull == 1: z = self.XPCI.value.setName(r.readFAString(dataSize)) #:matchif
            case FieldType.FULL if self._nextFull == 2: z = self.XMRKs.last().FULL = r.readSTRV(dataSize) #:matchif
            case FieldType.FULL if self._nextFull != 1 and self._nextFull != 2: self._nextFull = 0 #:matchif
            case FieldType.XLCM: z = self.XLCM = r.readS(IN32Field, dataSize)
            case FieldType.XRTM: z = self.XRTM = RefField[REFRRecord](REFRRecord, r, dataSize)
            case FieldType.XACT: z = self.XACT = r.readS(UI32Field, dataSize)
            case FieldType.XCNT: z = self.XCNT = r.readS(IN32Field, dataSize)
            case FieldType.XMRK: z = _nca(self, 'XMRKs', listx()).addX(self.XMRKGroup()); self._nextFull = 2
            case FieldType.FNAM: z = self.XMRKs.last().FNAM = r.readS(BYTEField, dataSize)
            case FieldType.TNAM: z = self.XMRKs.last().TNAM = r.readS(BYTEField, dataSize)
            case FieldType.ONAM: z = True
            case FieldType.XRGD: z = self.XRGD = r.readBYTV(dataSize)
            case FieldType.XSCL: z = self.XSCL = r.readS(FLTVField, dataSize)
            case FieldType.XSOL: z = self.XSOL = r.readS(BYTEField, dataSize)
            case _: z = Record._empty
        return z
# end::REFR[]

# REGN.Region - 3450 - tag::REGN[]
# dep: WRLDRecord, ^GRASRecord, GLOBRecord, SOUNRecord, WTHRRecord
class REGNRecord(Record):
    # TESX
    class RDATField:
        class REGNType(Enum): Objects = 2; Weather = 3; Map = 4; Landscape = 5; Grass = 6; Sound = 7
        def __init__(self, r: Header = None, dataSize: int = 0, RDSDs: list['RDGSField'] = None):
            self.type: int = None
            self.flags: REGNType = None
            self.priority: int = None
            # groups
            self.RDOTs: list['RDOTField'] = None # Objects
            self.RDMP: STRVField = None # MapName
            self.RDGSs: list['RDGSField'] = None # Grasses
            self.RDMD: UI32Field = None # Music Type
            self.RDSDs: list['RDSDField'] = RDSDs # Sounds
            self.RDWTs: list['RDWTField'] = None # Weather Types
            if not r: return
            self.type = r.readUInt32()
            self.flags = REGNType(r.readByte())
            self.priority = r.readByte()
            r.skip(2) # Unused

    class RDOTField:
        def __repr__(self): return f'{self.object}'
        object: Ref[Record]
        parentIdx: int
        density: float
        clustering: int
        minSlope: int # (degrees)
        maxSlope: int # (degrees)
        flags: int
        radiusWrtParent: int
        radius: int
        minHeight: float
        maxHeight: float
        sink: float
        sinkVariance: float
        sizeVariance: float
        angleVariance: Int3
        vertexShading: ByteColor4 # RGB + Shading radius (0 - 200) %

        def __init__(self, r: Header, dataSize: int):
            self.object = Ref[Record](Record, r.readUInt32())
            self.parentIdx = r.ReadUInt16()
            r.skip(2) # Unused
            self.density = r.readSingle()
            self.clustering = r.readByte()
            self.minSlope = r.readByte()
            self.maxSlope = r.readByte()
            self.flags = r.readByte()
            self.radiusWrtParent = r.ReadUInt16()
            self.radius = r.ReadUInt16()
            self.minHeight = r.readSingle()
            self.maxHeight = r.readSingle()
            self.sink = r.readSingle()
            self.sinkVariance = r.readSingle()
            self.sizeVariance = r.readSingle()
            self.angleVariance = Int3(r.ReadUInt16(), r.ReadUInt16(), r.ReadUInt16())
            r.skip(2) # Unused
            self.vertexShading = r.readS(ByteColor4, dataSize)

    class RDGSField:
        def __repr__(self): return f'{self.grass}'
        grass: Ref[GRASRecord] 

        def __init__(self, r: Header, dataSize: int):
            self.grass = Ref[GRASRecord](GRASRecord, r.readUInt32())
            r.skip(4) # Unused

    class RDSDField:
        def __repr__(self): return f'{self.sound}'
        sound: Ref['SOUNRecord']
        flags: int
        chance: int

        def __init__(self, r: Header, dataSize: int):
            if r.format == FormType.TES3:
                self.sound = Ref[SOUNRecord](SOUNRecord, r.readFAString(32))
                self.flags = 0
                self.chance = r.readByte()
                return
            self.sound = Ref[SOUNRecord](SOUNRecord, r.readUInt32())
            self.flags = r.readUInt32()
            self.chance = r.readUInt32() # float with TES5

    class RDWTField:
        def __repr__(self): return f'{self.weather}'
        @staticmethod
        def sizeOf(format: FormType) -> int: 8 if format == FormType.TES4 else 12
        def __init__(self, r: Header, dataSize: int):
            self.weather: Ref[WTHRRecord] = Ref[WTHRRecord](WTHRRecord, r.readUInt32())
            self.chance: int = r.readUInt32()
            self.global_: Ref[GLOBRecord] = Ref[GLOBRecord](GLOBRecord, r.readUInt32()) if r.format == FormType.TES5 else Ref[GLOBRecord](GLOBRecord)

    # TES3
    class WEATField:
        clear: int
        cloudy: int
        foggy: int
        overcast: int
        rain: int
        thunder: int
        ash: int
        blight: int

        def __init__(self, r: Header, dataSize: int):
            self.clear = r.readByte()
            self.cloudy = r.readByte()
            self.foggy = r.readByte()
            self.overcast = r.readByte()
            self.rain = r.readByte()
            self.thunder = r.readByte()
            self.ash = r.readByte()
            self.blight = r.readByte()
            # v1.3 ESM files add 2 bytes to WEAT subrecords.
            if dataSize == 10: r.skip(2)

    # TES4
    class RPLIField:
        def __init__(self, r: Header, dataSize: int):
            self.edgeFalloff: int = r.readUInt32() # (World Units)
            self.points: list[Vector2] # Region Point List Data
        def RPLDField(self, r: Header, dataSize: int) -> object: z = self.points = r.readFArray(lambda z: r.readVector2(), dataSize >> 3); return z

    ICON: STRVField # Icon / Sleep creature
    WNAM: RefField['WRLDRecord'] # Worldspace - Region name
    RCLR: CREFField # Map Color (COLORREF)
    RDATs: list[RDATField] = listx() # Region Data Entries / TES3: Sound Record (order determines the sound priority)
    # TES3
    WEAT: WEATField # Weather Data
    # TES4
    RPLIs: list[RPLIField] = listx() # Region Areas

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.WNAM | FieldType.FNAM: z = self.WNAM = RefField[WRLDRecord](WRLDRecord, r, dataSize)
            case FieldType.WEAT: z = self.WEAT = self.WEATField(r, dataSize) # TES3
            case FieldType.ICON | FieldType.BNAM: z = self.ICON = r.readSTRV(dataSize)
            case FieldType.RCLR | FieldType.CNAM: z = self.RCLR = r.readS(CREFField, dataSize)
            case FieldType.SNAM: z = self.RDATs.addX(self.RDATField(RDSDs = [self.RDSDField(r, dataSize)]))
            case FieldType.RPLI: z = self.RPLIs.addX(RPLIField(r, dataSize))
            case FieldType.RPLD: z = self.RPLIs.last().RPLDField(r, dataSize)
            case FieldType.RDAT: z = self.RDATs.addX(self.RDATField(r, dataSize))
            case FieldType.RDOT: z = self.RDATs.last().RDOTs = r.readFArray(lambda z: self.RDOTField(r, dataSize), dataSize // 52)
            case FieldType.RDMP: z = self.RDATs.last().RDMP = r.readSTRV(dataSize)
            case FieldType.RDGS: z = self.RDATs.last().RDGSs = r.readFArray(lambda z: self.RDGSField(r, dataSize), dataSize // 8)
            case FieldType.RDMD: z = self.RDATs.last().RDMD = r.readS(UI32Field, dataSize)
            case FieldType.RDSD: z = self.RDATs.last().RDSDs = r.readFArray(lambda z: self.RDSDField(r, dataSize), dataSize // 12)
            case FieldType.RDWT: z = self.RDATs.last().RDWTs = r.readFArray(lambda z: self.RDWTField(r, dataSize), dataSize // self.RDWTField.sizeOf(r.format))
            case _: z = Record._empty
        return z
# end::REGN[]

# ROAD.Road - 0400 - tag::ROAD[]
# dep: PGRDRecord
class ROADRecord(Record):
    PGRPs: list[PGRDRecord.PGRPField]
    PGRR: UNKNField

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.PGRP: z = self.PGRPs = r.readFArray(lambda z: PGRDRecord.PGRPField(r, dataSize), dataSize >> 4)
            case FieldType.PGRR: z = self.PGRR = r.readUNKN(dataSize)
            case _: z = Record._empty
        return z
# end::ROAD[]

# SBSP.Subspace - 0400 - tag::SBSP[]
# dep: None
class SBSPRecord(Record):
    class DNAMField:
        def __init__(self, r: Header, dataSize: int):
            self.x: float = r.readSingle() # X dimension
            self.y: float = r.readSingle() # Y dimension
            self.z: float = r.readSingle() # Z dimension

    DNAM: DNAMField

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize),
            case FieldType.DNAM: z = self.DNAM = self.DNAMField(r, dataSize),
            case _: z = Record._empty
        return z
# end::SBSP[]

# SCPT.Script - 3400 - tag::SCPT[]
# dep: None
class SCPTRecord(Record):
    # TESX
    class CTDAField:
        class INFOType(Enum): Nothing = 0; Function = 1; Global = 2; Local = 3; Journal = 4; Item = 5; Dead = 6; NotId = 7; NotFaction = 8; NotClass = 9; NotRace = 10; NotCell = 11; NotLocal = 12
        # TES3: 0 = [=], 1 = [!=], 2 = [>], 3 = [>=], 4 = [<], 5 = [<=]
        # TES4: 0 = [=], 2 = [!=], 4 = [>], 6 = [>=], 8 = [<], 10 = [<=]
        compareOp: int
        # (00-71) - sX = Global/Local/Not Local types, JX = Journal type, IX = Item Type, DX = Dead Type, XX = Not ID Type, FX = Not Faction, CX = Not Class, RX = Not Race, LX = Not Cell
        functionId: str
        # TES3
        index: int # (0-5)
        type: int
        # Except for the function type, this is the ID for the global/local/etc. Is not nessecarily NULL terminated.The function type SCVR sub-record has
        name: str
        # TES4
        comparisonValue: float
        parameter1: int # Parameter #1
        parameter2: int # Parameter #2

        def __init__(self, r: Header, dataSize: int):
            if r.format == FormType.TES3:
                self.index = r.readByte()
                self.type = r.readByte()
                self.functionId = r.readFAString(2)
                self.compareOp = (r.readByte() << 1) & 0xFF
                self.name = r.readFAString(dataSize - 5)
                self.comparisonValue = self.parameter1 = self.parameter2 = 0
                return
            self.compareOp = r.readByte()
            r.skip(3); # Unused
            self.comparisonValue = r.readSingle()
            self.functionId = r.readFAString(4)
            self.parameter1 = r.readInt32()
            self.parameter2 = r.readInt32()
            if dataSize != 24: r.skip(4) # Unused
            self.index = self.type = 0
            self.name = None

    # TES3
    class SCHDField:
        def __repr__(self): return f'{self.name}'
        def __init__(self, r: Header, dataSize: int):
            self.name: str = r.readFAString(32)
            self.numShorts: int = r.readInt32()
            self.numLongs: int = r.readInt32()
            self.numFloats: int = r.readInt32()
            self.scriptDataSize: int = r.readInt32()
            self.localVarSize: int = r.readInt32()
            self.variables: list[str] = None
        def SCVRField(self, r: Header, dataSize: int) -> object: z = self.variables = r.readVAStringList(dataSize); return z

    # TES4
    class SCHRField:
        def __repr__(self): return f'{self.refCount}'
        refCount: int
        compiledSize: int
        variableCount: int
        type: int # 0x000 = Object, 0x001 = Quest, 0x100 = Magic Effect

        def __init__(self, r: Header, dataSize: int):
            r.skip(4) # Unused
            self.refCount = r.readUInt32()
            self.compiledSize = r.readUInt32()
            self.variableCount = r.readUInt32()
            self.type = r.readUInt32()
            if dataSize == 20: return
            r.skip(dataSize - 20)

    class SLSDField:
        def __repr__(self): return f'{self.idx}:{self.variableName}'
        idx: int
        type: int
        variableName: str

        def __init__(self, r: Header, dataSize: int):
            self.idx = r.readUInt32()
            r.readUInt32() # Unknown
            r.readUInt32() # Unknown
            r.readUInt32() # Unknown
            self.type = r.readUInt32()
            r.readUInt32() # Unknown
            # SCVRField
            self.variableName = None
        def SCVRField(self, r: Header, dataSize: int) -> object: z = self.variableName = r.readFUString(dataSize); return z

    def __repr__(self): return f'SCPT: {self.EDID.value or self.SCHD.name}'
    SCDA: BYTVField # Compiled Script
    SCTX: STRVField # Script Source
    # TES3
    SCHD: SCHDField # Script Data
    # TES4
    SCHR: SCHRField # Script Data
    SLSDs: list[SLSDField] = listx() # Variable data
    SCRVs: list[SLSDField] = listx() # Ref variable data (one for each ref declared)
    SCROs: list[RefField[Record]] = listx() # Global variable reference

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.SCHD: z = self.SCHD = self.SCHDField(r, dataSize)
            case FieldType.SCVR: z = self.SLSDs.last().SCVRField(r, dataSize) if r.format != FormType.TES3 else self.SCHD.SCVRField(r, dataSize)
            case FieldType.SCDA | FieldType.SCDT: z = self.SCDA = r.readBYTV(dataSize)
            case FieldType.SCTX: z = self.SCTX = r.readSTRV(dataSize)
            # TES4
            case FieldType.SCHR: z = self.SCHR = self.SCHRField(r, dataSize)
            case FieldType.SLSD: z = self.SLSDs.addX(self.SLSDField(r, dataSize))
            case FieldType.SCRO: z = self.SCROs.addX(RefField[Record](Record, r, dataSize))
            case FieldType.SCRV: z = self.SCRVs.addX(self.then(r.readUInt32(), lambda v: self.SLSDs.single(lambda s: s.idx == idx)))
            case _: z = Record._empty
        return z
# end::SCPT[]

# SGST.Sigil Stone - 0400 - tag::SGST[]
# dep: ENCHRecord, SCPTRecord
class SGSTRecord(Record, IHaveMODL):
    class DATAField:
        def __init__(self, r: Header, dataSize: int):
            self.uses: int = r.readByte()
            self.value: int = r.readInt32()
            self.weight: float = r.readSingle()

    MODL: MODLGroup # Model
    FULL: STRVField # Item Name
    DATA: DATAField # Sigil Stone Data
    ICON: FILEField # Icon
    SCRI: RefField[SCPTRecord] # Script (optional)
    EFITs: list[ENCHRecord.EFITField] = listx() # Effect Data
    SCITs: list[ENCHRecord.SCITField] = listx() # Script Effect Data

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize)
            case FieldType.FULL: z = self.FULL = r.readSTRV(dataSize) if len(self.SCITs) == 0 else self.SCITs.last().FULLField(r, dataSize)
            case FieldType.DATA: z = self.DATA = self.DATAField(r, dataSize)
            case FieldType.ICON: z = self.ICON = r.readFILE(dataSize)
            case FieldType.SCRI: z = self.SCRI = RefField[SCPTRecord](SCPTRecord, r, dataSize)
            case FieldType.EFID: z = r.skip(dataSize)
            case FieldType.EFIT: z = self.EFITs.addX(ENCHRecord.EFITField(r, dataSize))
            case FieldType.SCIT: z = self.SCITs.addX(ENCHRecord.SCITField(r, dataSize))
            case _: z = Record._empty
        return z
# end::SGST[]

# SKIL.Skill - 3450 - tag::SKIL[]
# dep: None
class SKILRecord(Record):
    # TESX
    class DATAField:
        action: int
        attribute: int
        specialization: int # 0 = Combat, 1 = Magic, 2 = Stealth
        useValue: list[float] # The use types for each skill are hard-coded.

        def __init__(self, r: Header, dataSize: int):
            self.action = 0 if r.format == FormType.TES3 else r.readInt32()
            self.attribute = r.readInt32()
            self.specialization = r.readUInt32()
            self.useValue = r.readPArray(None, 'f', 4 if r.format == FormType.TES3 else 2)

    def __repr__(self): return f'SKIL: {self.INDX.value}:{self.EDID.value}'
    INDX: IN32Field # Skill ID
    DATA: DATAField # Skill Data
    DESC: STRVField # Skill description
    # TES4
    ICON: FILEField # Icon
    ANAM: STRVField # Apprentice Text
    JNAM: STRVField # Journeyman Text
    ENAM: STRVField # Expert Text
    MNAM: STRVField # Master Text

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.INDX: z = self.INDX = r.readS(IN32Field, dataSize)
            case FieldType.DATA | FieldType.SKDT: z = self.DATA = self.DATAField(r, dataSize)
            case FieldType.DESC: z = self.DESC = r.readSTRV(dataSize)
            case FieldType.ICON: z = self.ICON = r.readFILE(dataSize)
            case FieldType.ANAM: z = self.ANAM = r.readSTRV(dataSize)
            case FieldType.JNAM: z = self.JNAM = r.readSTRV(dataSize)
            case FieldType.ENAM: z = self.ENAM = r.readSTRV(dataSize)
            case FieldType.MNAM: z = self.MNAM = r.readSTRV(dataSize)
            case _: z = Record._empty
        return z
# end::SKIL[]

# SLGM.Soul Gem - 0450 - tag::SLGM[]
# dep: SCPTRecord
class SLGMRecord(Record, IHaveMODL):
    class DATAField:
        def __init__(self, r: Header, dataSize: int):
            self.value: int = r.readInt32()
            self.weight: float = r.readSingle()

    MODL: MODLGroup # Model
    FULL: STRVField # Item Name
    SCRI: RefField[SCPTRecord] # Script (optional)
    DATA: DATAField # Type of soul contained in the gem
    ICON: FILEField # Icon (optional)
    SOUL: BYTEField # Type of soul contained in the gem
    SLCP: BYTEField # Soul gem maximum capacity

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize)
            case FieldType.FULL: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.SCRI: z = self.SCRI = RefField[SCPTRecord](SCPTRecord, r, dataSize)
            case FieldType.DATA: z = self.DATA = self.DATAField(r, dataSize)
            case FieldType.ICON: z = self.ICON = r.readFILE(dataSize)
            case FieldType.SOUL: z = self.SOUL = r.readS(BYTEField, dataSize)
            case FieldType.SLCP: z = self.SLCP = r.readS(BYTEField, dataSize)
            case _: z = Record._empty
        return z
# end::SLGM[]

# SNDG.Sound Generator - 3000 - tag::SNDG[]
# dep: None
class SNDGRecord(Record):
    class SNDGType(Enum): LeftFoot = 0; RightFoot = 1; SwimLeft = 2; SwimRight = 3; Moan = 4; Roar = 5; Scream = 6; Land = 7
    DATA: IN32Field # Sound Type Data
    SNAM: STRVField # Sound ID
    CNAM: STRVField # Creature name (optional)

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        if r.format == FormType.TES3:
            match type:
                case FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
                case FieldType.DATA: z = self.DATA = r.readS(IN32Field, dataSize)
                case FieldType.SNAM: z = self.SNAM = r.readSTRV(dataSize)
                case FieldType.CNAM: z = self.CNAM = r.readSTRV(dataSize)
                case _: z = Record._empty
            return z
        return None
# end::SNDG[]

# SNDR.Sound Reference - 0050 - tag::SNDR[]
# dep: None
class SNDRRecord(Record):
    CNAM: CREFField # RGB color

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.CNAM: z = self.CNAM = r.readS(CREFField, dataSize)
            case _: z = Record._empty
        return z
# end::SNDR[]

# SOUN.Sound - 3450 - tag::SOUN[]
# dep: None
class SOUNRecord(Record):
    class SOUNFlags(Flag):
        RandomFrequencyShift = 0x0001
        PlayAtRandom = 0x0002
        EnvironmentIgnored = 0x0004
        RandomLocation = 0x0008
        Loop = 0x0010
        MenuSound = 0x0020
        _2D = 0x0040
        _360LFE = 0x0080

    # TESX
    class DATAField:
        volume: int # (0=0.00, 255=1.00)
        minRange: int # Minimum attenuation distance
        maxRange: int # Maximum attenuation distance
        # Bethesda4
        frequencyAdjustment: int # Frequency adjustment %
        flags: int # Flags
        staticAttenuation: int # Static Attenuation (db)
        stopTime: int # Stop time
        startTime: int # Start time

        def __init__(self, r: Header, dataSize: int):
            self.volume = r.readByte() if r.format == FormType.TES3 else 0
            self.minRange = r.readByte()
            self.maxRange = r.readByte()
            if r.format == FormType.TES3: return
            self.frequencyAdjustment = r.readSByte()
            r.readByte() # Unused
            self.flags = r.readUInt16()
            r.readUInt16() # Unused
            if dataSize == 8: return
            self.staticAttenuation = r.readUInt16()
            self.stopTime = r.readByte()
            self.startTime = r.readByte()

    FNAM: FILEField # Sound Filename (relative to Sounds\)
    DATA: DATAField # Sound Data

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.FNAM: z = self.FNAM = r.readFILE(dataSize)
            case FieldType.SNDX: z = self.DATA = self.DATAField(r, dataSize)
            case FieldType.SNDD: z = self.DATA = self.DATAField(r, dataSize)
            case FieldType.DATA: z = self.DATA = self.DATAField(r, dataSize)
            case _: z = Record._empty
        return z
# end::SOUN[]

# SPEL.Spell - 3450 - tag::SPEL[]
# dep: ENCHRecord
class SPELRecord(Record):
    # TESX
    class SPITField:
        def __repr__(self): return f'{self.type}'
        def __init__(self, r: Header, dataSize: int):
            # TES3: 0 = Spell, 1 = Ability, 2 = Blight, 3 = Disease, 4 = Curse, 5 = Power
            # TES4: 0 = Spell, 1 = Disease, 2 = Power, 3 = Lesser Power, 4 = Ability, 5 = Poison
            self.type: int = r.readUInt32()
            self.spellCost: int = r.readInt32()
            self.flags: int = r.readUInt32() # 0x0001 = AutoCalc, 0x0002 = PC Start, 0x0004 = Always Succeeds
            # TES4
            SpellLevel: int = r.readInt32() if r.format != FormType.TES3 else 0

    FULL: STRVField # Spell name
    SPIT: SPITField # Spell data
    EFITs: list[ENCHRecord.EFITField] = listx() # Effect Data
    # TES4
    SCITs: list[ENCHRecord.SCITField] = listx() # Script effect data

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.FULL if len(self.SCITs) == 0: z = self.FULL = r.readSTRV(dataSize) #:matchif
            case FieldType.FULL if len(self.SCITs) != 0: z = self.SCITs.last().FULLField(r, dataSize) #:matchif
            case FieldType.FNAM: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.SPIT | FieldType.SPDT: z = self.SPIT = self.SPITField(r, dataSize)
            case FieldType.EFID: z = r.skip(dataSize)
            case FieldType.EFIT | FieldType.ENAM: z = self.EFITs.addX(ENCHRecord.EFITField(r, dataSize))
            case FieldType.SCIT: z = self.SCITs.addX(ENCHRecord.SCITField(r, dataSize))
            case _: z = Record._empty
        return z
# end::SPEL[]

# SSCR.Start Script - 3000 - tag::XXXX[]
# dep: None
class SSCRRecord(Record):
    DATA: STRVField # Digits

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        if r.format == FormType.TES3:
            match type:
                case FieldType.NAME: z = self.EDID = r.readSTRV(dataSize),
                case FieldType.DATA: z = self.DATA = r.readSTRV(dataSize),
                case _: z = Record._empty
            return z
        return None
# end::SSCR[]

# STAT.Static - 3450 - tag::STAT[]
# dep: None
class STATRecord(Record, IHaveMODL):
    MODL: MODLGroup # Model

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readSTRV(dataSize),
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize),
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize),
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize),
            case _: z = Record._empty
        return z
# end::STAT[]

# TES3.Plugin Info - 3000 - tag::TES3[]
# dep: None
class TES3Record(Record):
    class HEDRField:
        def __init__(self, r: Header, dataSize: int):
            self.version: float = r.readSingle()
            self.fileType: int = r.readUInt32()
            self.companyName: str = r.readFAString(32)
            self.fileDescription: str = r.readFAString(256)
            self.numRecords: int = r.readUInt32()

    HEDR: HEDRField
    MASTs: list[STRVField]
    DATAs: list[INTVField]

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.HEDR: z = self.HEDR = self.HEDRField(r, dataSize)
            case FieldType.MAST: z = _nca(self, 'MASTs', listx()).addX(r.readSTRV(dataSize))
            case FieldType.DATA: z = _nca(self, 'DATAs', listx()).addX(r.readINTV(dataSize))
            case _: z = Record._empty
        return z
# end::TES3[]

# TES4.Plugin Info - 0450 - tag::TES4[]
# dep: None
class TES4Record(Record):
    class HEDRField:
        _struct = ('<fiI', 12)
        def __init__(self, tuple):
            (self.version,
            self.numRecords, # Number of records and groups (not including TES4 record itself).
            self.nextObjectId) = tuple #Next available object ID.

    HEDR: HEDRField
    CNAM: STRVField # author (Optional)
    SNAM: STRVField # description (Optional)
    MASTs: list[STRVField] # master
    DATAs: list[INTVField] # fileSize
    ONAM: UNKNField # overrides (Optional)
    INTV: IN32Field # unknown
    INCC: IN32Field # unknown (Optional)
    # TES5
    TNAM: UNKNField # overrides (Optional)

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.HEDR: z = self.HEDR = r.readS(self.HEDRField, dataSize)
            case FieldType.OFST: z = r.skip(dataSize)
            case FieldType.DELE: z = r.skip(dataSize)
            case FieldType.CNAM: z = self.CNAM = r.readSTRV(dataSize)
            case FieldType.SNAM: z = self.SNAM = r.readSTRV(dataSize)
            case FieldType.MAST: z = _nca(self, 'MASTs', listx()).addX(r.readSTRV(dataSize))
            case FieldType.DATA: z = _nca(self, 'DATAs', listx()).addX(r.readINTV(dataSize))
            case FieldType.ONAM: z = self.ONAM = r.readUNKN(dataSize)
            case FieldType.INTV: z = self.INTV = r.readS(IN32Field, dataSize)
            case FieldType.INCC: z = self.INCC = r.readS(IN32Field, dataSize)
            # TES5
            case FieldType.TNAM: z = self.TNAM = r.readUNKN(dataSize)
            case _: z = Record._empty
        return z
# end::TES4[]

# TREE.Tree - 0450 - tag::TREE[]
# dep: None
class TREERecord(Record, IHaveMODL):
    class SNAMField:
        def __init__(self, r: Header, dataSize: int):
            self.values: list[int] = r.readPArray(None, 'i', dataSize >> 2)

    class CNAMField:
        def __init__(self, r: Header, dataSize: int):
            self.leafCurvature: float = r.readSingle()
            self.minimumLeafAngle: float = r.readSingle()
            self.maximumLeafAngle: float = r.readSingle()
            self.branchDimmingValue: float = r.readSingle()
            self.leafDimmingValue: float = r.readSingle()
            self.shadowRadius: int = r.readInt32()
            self.rockSpeed: float = r.readSingle()
            self.rustleSpeed: float = r.readSingle()

    class BNAMField:
        def __init__(self, r: Header, dataSize: int):
            self.width: float = r.readSingle()
            self.height: float = r.readSingle()

    MODL: MODLGroup # Model
    ICON: FILEField # Leaf Texture
    SNAM: SNAMField # SpeedTree Seeds, array of ints
    CNAM: CNAMField # Tree Parameters
    BNAM: BNAMField # Billboard Dimensions

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize)
            case FieldType.ICON: z = self.ICON = r.readFILE(dataSize)
            case FieldType.SNAM: z = self.SNAM = self.SNAMField(r, dataSize)
            case FieldType.CNAM: z = self.CNAM = self.CNAMField(r, dataSize)
            case FieldType.BNAM: z = self.BNAM = self.BNAMField(r, dataSize)
            case _: z = Record._empty
        return z
# end::TREE[]

# WATR.Water Type - 0450 - tag::WATR[]
# dep: SOUNRecord
class WATRRecord(Record):
    class DATAField:
        windVelocity: float
        windDirection: float
        waveAmplitude: float
        waveFrequency: float
        sunPower: float
        reflectivityAmount: float
        fresnelAmount: float
        scrollXSpeed: float
        scrollYSpeed: float
        fogDistance_NearPlane: float
        fogDistance_FarPlane: float
        shallowColor: ByteColor4
        deepColor: ByteColor4
        reflectionColor: ByteColor4
        textureBlend: int
        rainSimulator_Force: float
        rainSimulator_Velocity: float
        rainSimulator_Falloff: float
        rainSimulator_Dampner: float
        rainSimulator_StartingSize: float
        displacementSimulator_Force: float
        displacementSimulator_Velocity: float
        displacementSimulator_Falloff: float
        displacementSimulator_Dampner: float
        displacementSimulator_StartingSize: float
        damage: int

        def __init__(self, r: Header, dataSize: int):
            if dataSize != 102 and dataSize != 86 and dataSize != 62 and dataSize != 42 and dataSize != 2: self.windVelocity = 1
            if dataSize == 2: self.damage = r.readUInt16(); return
            self.windVelocity = r.readSingle()
            self.windDirection = r.readSingle()
            self.waveAmplitude = r.readSingle()
            self.waveFrequency = r.readSingle()
            self.sunPower = r.readSingle()
            self.reflectivityAmount = r.readSingle()
            self.fresnelAmount = r.readSingle()
            self.scrollXSpeed = r.readSingle()
            self.scrollYSpeed = r.readSingle()
            self.fogDistance_NearPlane = r.readSingle()
            if dataSize == 42: self.damage = r.readUInt16(); return
            self.fogDistance_FarPlane = r.readSingle()
            self.shallowColor = r.readS(ByteColor4, dataSize)
            self.deepColor = r.readS(ByteColor4, dataSize)
            self.reflectionColor = r.readS(ByteColor4, dataSize)
            self.textureBlend = r.readByte()
            r.skip(3) # Unused
            if dataSize == 62: self.damage = r.readUInt16(); return
            self.rainSimulator_Force = r.readSingle()
            self.rainSimulator_Velocity = r.readSingle()
            self.rainSimulator_Falloff = r.readSingle()
            self.rainSimulator_Dampner = r.readSingle()
            self.rainSimulator_StartingSize = r.readSingle()
            self.displacementSimulator_Force = r.readSingle()
            if dataSize == 86:
                # DisplacementSimulator_Velocity = DisplacementSimulator_Falloff = DisplacementSimulator_Dampner = DisplacementSimulator_StartingSize = 0F
                self.damage = r.readUInt16()
                return
            self.displacementSimulator_Velocity = r.readSingle()
            self.displacementSimulator_Falloff = r.readSingle()
            self.displacementSimulator_Dampner = r.readSingle()
            self.displacementSimulator_StartingSize = r.readSingle()
            self.damage = r.readUInt16()

    class GNAMField:
        def __init__(self, r: Header, dataSize: int):
            self.daytime: Ref[WATRRecord] = Ref[WATRRecord](WATRRecord, r.readUInt32())
            self.nighttime: Ref[WATRRecord] = Ref[WATRRecord](WATRRecord, r.readUInt32())
            self.underwater: Ref[WATRRecord] = Ref[WATRRecord](WATRRecord, r.readUInt32())

    TNAM: STRVField # Texture
    ANAM: BYTEField # Opacity
    FNAM: BYTEField # Flags
    MNAM: STRVField # Material ID
    SNAM: RefField[SOUNRecord] # Sound
    DATA: DATAField # DATA
    GNAM: GNAMField # GNAM

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.TNAM: z = self.TNAM = r.readSTRV(dataSize)
            case FieldType.ANAM: z = self.ANAM = r.readS(BYTEField, dataSize)
            case FieldType.FNAM: z = self.FNAM = r.readS(BYTEField, dataSize)
            case FieldType.MNAM: z = self.MNAM = r.readSTRV(dataSize)
            case FieldType.SNAM: z = self.SNAM = RefField[SOUNRecord](SOUNRecord, r, dataSize)
            case FieldType.DATA: z = self.DATA = self.DATAField(r, dataSize)
            case FieldType.GNAM: z = self.GNAM = self.GNAMField(r, dataSize)
            case _: z = Record._empty
        return z
# end::WATR[]

# WEAP.Weapon - 3450 - tag::WEAP[]
# dep: ENCHRecord, SCPTRecord
class WEAPRecord(Record, IHaveMODL):
    class DATAField:
        class WEAPType(Enum): ShortBladeOneHand = 0; LongBladeOneHand = 1; LongBladeTwoClose = 2; BluntOneHand = 3; BluntTwoClose = 4; BluntTwoWide = 5; SpearTwoWide = 6; AxeOneHand = 7; AxeTwoHand = 8; MarksmanBow = 9; MarksmanCrossbow = 10; MarksmanThrown = 11; Arrow = 12; Bolt = 13
        weight: float
        value: int
        type: int
        health: int
        speed: float
        reach: float
        damage: int # EnchantPts
        chopMin: int
        chopMax: int
        slashMin: int
        slashMax: int
        thrustMin: int
        thrustMax: int
        flags: int # 0 = ?, 1 = Ignore Normal Weapon Resistance?

        def __init__(self, r: Header, dataSize: int):
            if r.format == FormType.TES3: 
                self.weight = r.readSingle()
                self.value = r.readInt32()
                self.type = r.readUInt16()
                self.health = r.readInt16()
                self.speed = r.readSingle()
                self.reach = r.readSingle()
                self.damage = r.readInt16()
                self.chopMin = r.readByte()
                self.chopMax = r.readByte()
                self.slashMin = r.readByte()
                self.slashMax = r.readByte()
                self.thrustMin = r.readByte()
                self.thrustMax = r.readByte()
                self.flags = r.readInt32()
                return
            self.type = r.readUInt32()
            self.speed = r.readSingle()
            self.reach = r.readSingle()
            self.flags = r.readInt32()
            self.value = r.readInt32()
            self.health = r.readInt32()
            self.weight = r.readSingle()
            self.damage = r.readInt16()
            self.chopMin = self.chopMax = self.slashMin = self.slashMax = self.thrustMin = self.thrustMax = 0

    MODL: MODLGroup # Model
    FULL: STRVField # Item Name
    DATA: DATAField # Weapon Data
    ICON: FILEField # Male Icon (optional)
    ENAM: RefField[ENCHRecord] # Enchantment ID
    SCRI: RefField[SCPTRecord] # Script (optional)
    # TES4
    ANAM: IN16Field # Enchantment points (optional)

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODTField(r, dataSize)
            case FieldType.FULL | FieldType.FNAM: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.DATA | FieldType.WPDT: z = self.DATA = self.DATAField(r, dataSize)
            case FieldType.ICON | FieldType.ITEX: z = self.ICON = r.readFILE(dataSize)
            case FieldType.ENAM: z = self.ENAM = RefField[ENCHRecord](ENCHRecord, r, dataSize)
            case FieldType.SCRI: z = self.SCRI = RefField[SCPTRecord](SCPTRecord, r, dataSize)
            case FieldType.ANAM: z = self.ANAM = r.readS(IN16Field, dataSize)
            case _: z = Record._empty
        return z
# end::WEAP[]

# WRLD.Worldspace - 0450 - tag::WRLD[]
# dep: CLMTRecord, WATRRecord, WRLDRecord, ^REFRRecord
class WRLDRecord(Record):
    class MNAMField:
        _struct = ('<2i4h', 16)
        def __init__(self, tuple):
            (self.usableDimensions,
            # Cell Coordinates
            self.nwCell_X,
            self.nwCell_Y,
            self.seCell_X,
            self.seCell_Y) = tuple

    class NAM0Field:
        # _struct = ('<4f', 16)
        def __init__(self, r: Header, dataSize: int):
            self.min: Vector2 = r.readVector2()
            self.max: Vector2 = Vector2.Zero
        def NAM9Field(self, r: Header, dataSize: int) -> object: z = self.max = r.readVector2(); return z

    # TES5
    class RNAMField:
        class Reference:
            _struct = ('<I2h', 16)
            def __init__(self, tuple):
                (self.ref,
                self.x,
                self.y) = tuple
                self.ref: RefId[REFRRecord] = RefId[REFRRecord](REFRRecord, self.ref)
        def __init__(self, r: Header, dataSize: int):
            self.gridX: int = r.readInt16()
            self.gridY: int = r.readInt16()
            self.gridReferences: list[Reference] = r.readL32SArray(Reference, referenceSize >> 3)
            assert((dataSize - 8) >> 3 == len(self.gridReferences))

    FULL: STRVField
    WNAM: RefField['WRLDRecord'] # Parent Worldspace
    CNAM: RefField[CLMTRecord] # Climate
    NAM2: RefField[WATRRecord] # Water
    ICON: FILEField # Icon
    MNAM: MNAMField # Map Data
    DATA: BYTEField # Flags
    NAM0: NAM0Field # Object Bounds
    SNAM: UI32Field # Music
    # TES5
    RNAMs: list[RNAMField] = listx() # Large References

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.FULL: z = self.FULL = r.readSTRV(dataSize)
            case FieldType.WNAM: z = self.WNAM = RefField[WRLDRecord](WRLDRecord, r, dataSize)
            case FieldType.CNAM: z = self.CNAM = RefField[CLMTRecord](CLMTRecord, r, dataSize)
            case FieldType.NAM2: z = self.NAM2 = RefField[WATRRecord](WATRRecord, r, dataSize)
            case FieldType.ICON: z = self.ICON = r.readFILE(dataSize)
            case FieldType.MNAM: z = self.MNAM = r.readS(self.MNAMField, dataSize)
            case FieldType.DATA: z = self.DATA = r.readS(BYTEField, dataSize)
            case FieldType.NAM0: z = self.NAM0 = self.NAM0Field(r, dataSize)
            case FieldType.NAM9: z = self.NAM0.NAM9Field(r, dataSize)
            case FieldType.SNAM: z = self.SNAM = r.readS(UI32Field, dataSize)
            case FieldType.OFST: z = r.skip(dataSize)
            # TES5
            case FieldType.RNAM: z = self.RNAMs.addX(self.RNAMField(r, dataSize))
            case _: z = Record._empty
        return z
# end::WRLD[]

# WTHR.Weather - 0450 - tag::WTHR[]
# dep: ^SOUNRecord
class WTHRRecord(Record, IHaveMODL):
    class FNAMField:
        def __init__(self, r: Header, dataSize: int):
            self.dayNear: float = r.readSingle()
            self.dayFar: float = r.readSingle()
            self.nightNear: float = r.readSingle()
            self.nightFar: float = r.readSingle()

    class HNAMField:
        def __init__(self, r: Header, dataSize: int):
            self.eyeAdaptSpeed: float = r.readSingle()
            self.blurRadius: float = r.readSingle()
            self.blurPasses: float = r.readSingle()
            self.emissiveMult: float = r.readSingle()
            self.targetLUM: float = r.readSingle()
            self.upperLUMClamp: float = r.readSingle()
            self.brightScale: float = r.readSingle()
            self.brightClamp: float = r.readSingle()
            self.lumRampNoTex: float = r.readSingle()
            self.lumRampMin: float = r.readSingle()
            self.lumRampMax: float = r.readSingle()
            self.sunlightDimmer: float = r.readSingle()
            self.grassDimmer: float = r.readSingle()
            self.treeDimmer: float = r.readSingle()

    class DATAField:
        def __init__(self, r: Header, dataSize: int):
            self.windSpeed: int = r.readByte()
            self.cloudSpeed_Lower: int = r.readByte()
            self.cloudSpeed_Upper: int = r.readByte()
            self.transDelta: int = r.readByte()
            self.sunGlare: int = r.readByte()
            self.sunDamage: int = r.readByte()
            self.precipitation_BeginFadeIn: int = r.readByte()
            self.precipitation_EndFadeOut: int = r.readByte()
            self.thunderLightning_BeginFadeIn: int = r.readByte()
            self.thunderLightning_EndFadeOut: int = r.readByte()
            self.thunderLightning_Frequency: int = r.readByte()
            self.weatherClassification: int = r.readByte()
            self.lightningColor: ByteColor4 = ByteColor4(r.readByte(), r.readByte(), r.readByte(), 255)

    class SNAMField:
        def __init__(self, r: Header, dataSize: int):
            self.sound: Ref[SOUNRecord] = Ref[SOUNRecord](SOUNRecord, r.readUInt32()) # Sound FormId
            self.type: int  = r.readUInt32() # Sound Type - 0=Default, 1=Precipitation, 2=Wind, 3=Thunder

    MODL: MODLGroup # Model
    CNAM: FILEField # Lower Cloud Layer
    DNAM: FILEField # Upper Cloud Layer
    NAM0: BYTVField # Colors by Types/Times
    FNAM: FNAMField # Fog Distance
    HNAM: HNAMField # HDR Data
    DATA: DATAField # Weather Data
    SNAMs: list[SNAMField] = listx() # Sounds

    def createField(self, r: Header, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readSTRV(dataSize)
            case FieldType.MODL: z = self.MODL = MODLGroup(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODBField(r, dataSize)
            case FieldType.CNAM: z = self.CNAM = r.readFILE(dataSize)
            case FieldType.DNAM: z = self.DNAM = r.readFILE(dataSize)
            case FieldType.NAM0: z = self.NAM0 = r.readBYTV(dataSize)
            case FieldType.FNAM: z = self.FNAM = self.FNAMField(r, dataSize)
            case FieldType.HNAM: z = self.HNAM = self.HNAMField(r, dataSize)
            case FieldType.DATA: z = self.DATA = self.DATAField(r, dataSize)
            case FieldType.SNAM: z = self.SNAMs.addX(SNAMField(r, dataSize))
            case _: z = Record._empty
        return z
# end::WTHR[]

#endregion
