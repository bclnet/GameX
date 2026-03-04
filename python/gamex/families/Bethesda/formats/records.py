import os, sys
from io import BytesIO
from itertools import groupby
from typing import TypeVar, get_args
from enum import Enum, Flag, IntEnum, IntFlag
from struct import unpack
from numpy import ndarray, array
from collections.abc import Iterator
from openstk import log, Int2, Int3, Byte3, Float3
from openstk.core.drawing import Color
from gamex import FileSource, BinaryReader, ArcBinaryT
from gamex.core.globalx import ByteColor4
from gamex.families.Uncore.formats.compression import decompressZlib2

sys.setrecursionlimit(1500)

# types
type Vector3 = ndarray

#region Enums

class FormType(IntEnum):
    def __str__(self): return self.to_bytes(4, byteorder='little').decode('ascii')
    ZERO = 0x00000000
    ONE_ = 0x00000001
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
    BOIM = 0x4D494F42
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
    PWAT = 0x54415750
    QUST = 0x54535551
    RFCT = 0x54434652
    REGN = 0x4E474552
    RACE = 0x45434152
    RADS = 0x53444152
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
    STDT = 0x54445453
    SUNP = 0x504E5553
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
    TMLM = 0x4D4C4D54
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

    @classmethod
    def _missing_(cls, value):
        s = int.__new__(cls); s._value_ = value; s._name_ = f'_{hex(value)}'
        # print(f'_missing_: {s}')
        return s

class FieldType(Enum):
    AADT = 0x54444141
    ACBS = 0x53424341
    AHCF = 0x46434841
    AHCM = 0x4D434841
    AIDT = 0x54444941
    AI_A = 0x415F4941
    AI_E = 0x455F4941
    AI_F = 0x465F4941
    AI_T = 0x545F4941
    AI_W = 0x575F4941
    ALDT = 0x54444C41
    AMBI = 0x49424D41
    ANAM = 0x4D414E41
    AODT = 0x54444F41
    ASND = 0x444E5341
    ATKD = 0x444B5441
    ATKE = 0x454B5441
    ATTR = 0x52545441
    ATXT = 0x54585441
    AVFX = 0x58465641

    BKDT = 0x54444B42
    BMDT = 0x54444D42
    BNAM = 0x4D414E42
    BODT = 0x54444F42
    BPND = 0x444E5042
    BPNI = 0x494E5042
    BPNN = 0x4E4E5042
    BPNT = 0x544E5042
    BPTN = 0x4E545042
    BSND = 0x444E5342
    BTXT = 0x54585442
    BVFX = 0x58465642
    BYDT = 0x54445942

    CIS2 = 0x32534943
    CITC = 0x43544943
    CLDT = 0x54444C43
    CNAM = 0x4D414E43
    CNDT = 0x54444E43
    CNTO = 0x4F544E43
    CRGR = 0x52475243
    CRVA = 0x41565243
    CSAD = 0x44415343
    CSCR = 0x52435343
    CSDI = 0x49445343
    CSDC = 0x43445343
    CSDT = 0x54445343
    CSTD = 0x44545343
    CSND = 0x444E5343
    CTDA = 0x41445443
    CTDT = 0x54445443
    CVFX = 0x58465643

    DATA = 0x41544144
    DELE = 0x454C4544
    DESC = 0x43534544
    DFTF = 0x46544644
    DFTM = 0x4D544644
    DNAM = 0x4D414E44
    DODT = 0x54444F44

    EDID = 0x44494445
    EFID = 0x44494645
    EFIT = 0x54494645
    ENAM = 0x4D414E45
    ENDT = 0x54444E45
    ENIT = 0x54494E45
    ESCE = 0x45435345

    FADT = 0x54444146
    FGGA = 0x41474746
    FGGS = 0x53474746
    FGTS = 0x53544746
    FLAG = 0x47414C46
    FLMV = 0x564D4C46
    FLTV = 0x56544C46
    FNAM = 0x4D414E46
    FRMR = 0x524D5246
    FTSF = 0x46535446
    FTSM = 0x4D535446
    FULL = 0x4C4C5546

    GNAM = 0x4D414E47

    HCLF = 0x464C4348
    HCLR = 0x524C4348
    HEAD = 0x44414548
    HEDR = 0x52444548
    HNAM = 0x4D414E48
    HSND = 0x444E5348
    HVFX = 0x58465648
    
    ICON = 0x4E4F4349
    ICO2 = 0x324F4349
    INAM = 0x4D414E49
    INCC = 0x43434E49
    INDX = 0x58444E49
    INTV = 0x56544E49
    IRDT = 0x54445249
    ITEX = 0x58455449
    
    JAIL = 0x4C49414A
    JNAM = 0x4D414E4A
    JOUT = 0x54554F4A

    KFFZ = 0x5A46464B
    KNAM = 0x4D414E4B
    KSIZ = 0x5A49534B
    KWDA = 0x4144574B

    LHDT = 0x5444484C
    LKDT = 0x54444B4C
    LNAM = 0x4D414E4C
    LVLD = 0x444C564C
    LVLF = 0x464C564C
    LVLO = 0x4F4C564C

    MAST = 0x5453414D
    MCDT = 0x5444434D
    MEDT = 0x5444454D
    MICO = 0x4F43494D
    MNAM = 0x4D414E4D
    MO2B = 0x42324F4D
    MO2T = 0x54324F4D
    MO3B = 0x42334F4D
    MO3T = 0x54334F4D
    MO4B = 0x42344F4D
    MO4T = 0x54344F4D
    MOD2 = 0x32444F4D
    MOD3 = 0x33444F4D
    MOD4 = 0x34444F4D
    MODB = 0x42444F4D
    MODD = 0x44444F4D
    MODL = 0x4C444F4D
    MODS = 0x53444F4D
    MODT = 0x54444F4D
    MPAI = 0x4941504D
    MPAV = 0x5641504D
    MTNM = 0x4D4E544D
    MTYP = 0x5059544D

    NAM0 = 0x304D414E
    NAM1 = 0x314D414E
    NAM2 = 0x324D414E
    NAM3 = 0x334D414E
    NAM4 = 0x344D414E
    NAM5 = 0x354D414E
    NAM7 = 0x374D414E
    NAM8 = 0x384D414E
    NAM9 = 0x394D414E
    NAME = 0x454D414E
    NIFT = 0x5446494E
    NIFZ = 0x5A46494E
    NNAM = 0x4D414E4E
    NPCO = 0x4F43504E
    NPCS = 0x5343504E
    NPDT = 0x5444504E

    OBND = 0x444E424F
    OFST = 0x5453464F
    ONAM = 0x4D414E4F

    PBDT = 0x54444250
    PFIG = 0x47494650
    PFPC = 0x43504650
    PGAG = 0x47414750
    PGRC = 0x43524750
    PGRI = 0x49524750
    PGRL = 0x4C524750
    PGRP = 0x50524750
    PGRR = 0x52524750
    PHTN = 0x4E544850
    PHWT = 0x54574850
    PKDT = 0x54444B50
    PKID = 0x44494B50
    PLCN = 0x4E434C50
    PLDT = 0x54444C50
    PLVD = 0x44564C50
    PNAM = 0x4D414E50
    PSDT = 0x54445350
    PTDT = 0x54445450
    PTEX = 0x58455450

    QNAM = 0x4D414E51
    QSDT = 0x54445351
    QSTA = 0x41545351
    QSTF = 0x46545351
    QSTI = 0x49545351
    QSTN = 0x4E545351
    QSTR = 0x52545351
    
    RADT = 0x54444152
    RAGA = 0x41474152
    RCLR = 0x524C4352
    RDAT = 0x54414452
    RDGS = 0x53474452
    RDMD = 0x444D4452
    RDMP = 0x504D4452
    RDOT = 0x544F4452
    RDSD = 0x44534452
    RDWT = 0x54574452
    RGNN = 0x4E4E4752
    RIDT = 0x54444952
    RNAM = 0x4D414E52
    RNMV = 0x564D4E52
    RPLD = 0x444C5052
    RPLI = 0x494C5052
    RPRF = 0x46525052
    RPRM = 0x4D525052

    SCDA = 0x41444353
    SCDT = 0x54444353
    SCHD = 0x44484353
    SCHR = 0x52484353
    SCIT = 0x54494353
    SCPT = 0x54504353
    SCRI = 0x49524353
    SCRO = 0x4F524353
    SCRV = 0x56524353
    SCTX = 0x58544353
    SCVR = 0x52564353
    SDSC = 0x43534453
    SKDT = 0x54444B53
    SLCP = 0x50434C53
    SLSD = 0x44534C53
    SNAM = 0x4D414E53
    SNDD = 0x44444E53
    SNDX = 0x58444E53
    SNMV = 0x564D4E53
    SOUL = 0x4C554F53
    SPCT = 0x54435053
    SPDT = 0x54445053
    SPED = 0x44455053
    SPIT = 0x54495053
    SPLO = 0x4F4C5053
    STOL = 0x4C4F5453
    STRV = 0x56525453
    SWMV = 0x564D5753

    TCLF = 0x464C4354
    TCLT = 0x544C4354
    TEXT = 0x54584554
    TNAM = 0x4D414E54
    TINC = 0x434E4954
    TIND = 0x444E4954
    TINI = 0x494E4954
    TINL = 0x4C4E4954
    TINP = 0x504E4954
    TINT = 0x544E4954
    TINV = 0x564E4954
    TIRS = 0x53524954
    TPIC = 0x43495054
    TRDT = 0x54445254
    TX00 = 0x30305854
    TX01 = 0x31305854
    TX02 = 0x32305854
    TX03 = 0x33305854
    TX04 = 0x34305854
    TX05 = 0x35305854
    TX06 = 0x36305854
    TX07 = 0x37305854

    UNAM = 0x4D414E55
    UNES = 0x53454E55

    VCLR = 0x524C4356
    VENC = 0x434E4556
    VEND = 0x444E4556
    VENV = 0x564E4556
    VHGT = 0x54474856
    VNAM = 0x4D414E56
    VNML = 0x4C4D4E56
    VTCK = 0x4B435456
    VTEX = 0x58455456
    VTXT = 0x54585456
    
    WAIT = 0x54494157
    WEAT = 0x54414557
    WHGT = 0x54474857
    WKMV = 0x564D4B57
    WLST = 0x54534C57
    WNAM = 0x4D414E57
    WPDT = 0x54445057

    XACT = 0x54434158
    XCCM = 0x4D434358
    XCHG = 0x47484358
    XCNT = 0x544E4358
    XCLC = 0x434C4358
    XCLL = 0x4C4C4358
    XCLR = 0x524C4358
    XCLW = 0x574C4358
    XCMT = 0x544D4358
    XCWT = 0x54574358
    XESP = 0x50534558
    XGLB = 0x424C4758
    XHLT = 0x544C4858
    XHRS = 0x53524858
    XLCM = 0x4D434C58
    XLOC = 0x434F4C58
    XLOD = 0x444F4C58
    XMRC = 0x43524D58
    XMRK = 0x4B524D58
    XNAM = 0x4D414E58
    XOWN = 0x4E574F58
    XPCI = 0x49435058
    XRGD = 0x44475258
    XRNK = 0x4B4E5258
    XRTM = 0x4D545258
    XSCL = 0x4C435358
    XSED = 0x44455358
    XSOL = 0x4C4F5358
    XTEL = 0x4C455458
    XTRG = 0x47525458
    XXXX = 0x58585858
    
    YNAM = 0x4D414E59

    ZNAM = 0x4D414E5A

class ActorValue(IntEnum):
    None_ = -1
    Strength = 0; Intelligence = 1; Willpower = 2; Agility = 3; Speed = 4; Endurance = 5; Personality = 6; Luck = 7; Health = 8; Magicka = 9; Fatigue = 10; Encumbrance = 11
    Armorer = 12; Athletics = 13; Blade = 14; Block = 15; Blunt = 16; HandToHand = 17; HeavyArmor = 18; Alchemy = 19; Alteration = 20; Conjuration = 21; Destruction = 22; Illusion = 23
    Mysticism = 24; Restoration = 25; Acrobatics = 26; LightArmor = 27; Marksman = 28; Mercantile = 29; Security = 30; Sneak = 31; Speechcraft = 32
    # Extra Actor Values
    Aggression = 33; Confidence = 34; Energy = 35; Responsibility = 36; Bounty = 37; Fame = 38; Infamy = 39; MagickaMultiplier = 40; NightEyeBonus = 41; AttackBonus = 42; DefendBonus = 43; CastingPenalty = 44; Blindness = 45
    Chameleon = 46; Invisibility = 47; Paralysis = 48; Silence = 49; Confusion = 50; DetectItemRange = 51; SpellAbsorbChance = 52; SpellReflectChance = 53; SwimSpeedMultiplier = 54; WaterBreathing = 55; WaterWalking = 56; StuntedMagicka = 57; DetectLifeRange = 58
    ReflectDamage = 59; Telekinesis = 60; ResistFire = 61; ResistFrost = 62; ResistDisease = 63; ResistMagic = 64; ResistNormalWeapons = 65; ResistParalysis = 66; ResistPoison = 67; ResistShock = 68; Vampirism = 69; Darkness = 70; ResistWaterDamage = 71

#endregion

#region Record

# Reader
class Reader(BinaryReader):
    def __init__(self, r: BinaryReader, binPath: str, format: FormType, tes4a: bool):
        super().__init__(r.f, r.length)
        self.binPath = binPath
        self.format = format
        self.tes4a = tes4a
        self.version = 0

class Record:
    _mapx: dict[FormType, callable] = {
        FormType.TES3: lambda f: TES3Record(),
        FormType.TES4: lambda f: TES4Record(),
        # 0
        FormType.LTEX: lambda f: LTEXRecord(),
        FormType.STAT: lambda f: STATRecord(),
        FormType.CELL: lambda f: CELLRecord(),
        FormType.LAND: lambda f: LANDRecord(),
        # 1
        FormType.DOOR: lambda f: DOORRecord(),
        FormType.MISC: lambda f: MISCRecord(),
        FormType.WEAP: lambda f: WEAPRecord(),
        FormType.CONT: lambda f: CONTRecord(),
        FormType.LIGH: lambda f: LIGHRecord(),
        FormType.ARMO: lambda f: ARMORecord(),
        FormType.CLOT: lambda f: CLOTRecord(),
        FormType.REPA: lambda f: REPARecord(),
        FormType.ACTI: lambda f: ACTIRecord(),
        FormType.APPA: lambda f: APPARecord(),
        FormType.LOCK: lambda f: LOCKRecord(),
        FormType.PROB: lambda f: PROBRecord(),
        FormType.INGR: lambda f: INGRRecord(),
        FormType.BOOK: lambda f: BOOKRecord(),
        FormType.ALCH: lambda f: ALCHRecord(),
        FormType.CREA: lambda f: CREA3Record() if f == FormType.TES3 else CREA4Record(),
        FormType.NPC_: lambda f: NPC_3Record() if f == FormType.TES3 else NPC_4Record(),
        # 2
        FormType.GMST: lambda f: GMSTRecord(),
        FormType.GLOB: lambda f: GLOBRecord(),
        FormType.SOUN: lambda f: SOUNRecord(),
        FormType.REGN: lambda f: REGNRecord(),
        # 3
        FormType.CLAS: lambda f: CLASRecord(),
        FormType.SPEL: lambda f: SPELRecord(),
        FormType.BODY: lambda f: BODYRecord(),
        FormType.PGRD: lambda f: PGRDRecord(),
        FormType.INFO: lambda f: INFO3Record() if f == FormType.TES3 else INFO4Record(),
        FormType.DIAL: lambda f: DIALRecord(),
        FormType.SNDG: lambda f: SNDGRecord(),
        FormType.ENCH: lambda f: ENCHRecord(),
        FormType.SCPT: lambda f: SCPTRecord(),
        FormType.SKIL: lambda f: SKILRecord(),
        FormType.RACE: lambda f: RACE3Record() if f == FormType.TES3 else RACE4Record() if f == FormType.TES4 else RACE5Record(),
        FormType.MGEF: lambda f: MGEFRecord(),
        FormType.LEVI: lambda f: LEVIRecord(),
        FormType.LEVC: lambda f: LEVCRecord(),
        FormType.BSGN: lambda f: BSGNRecord(),
        FormType.FACT: lambda f: FACTRecord(),
        FormType.SSCR: lambda f: SSCRRecord(),
        # 4 - Oblivion                      ,
        FormType.WRLD: lambda f: WRLDRecord(),
        FormType.ACRE: lambda f: ACRERecord(),
        FormType.ACHR: lambda f: ACHRRecord(),
        FormType.REFR: lambda f: REFRRecord(),
        #                                   ,
        FormType.AMMO: lambda f: AMMORecord(),
        FormType.ANIO: lambda f: ANIORecord(),
        FormType.CLMT: lambda f: CLMTRecord(),
        FormType.CSTY: lambda f: CSTYRecord(),
        FormType.EFSH: lambda f: EFSHRecord(),
        FormType.EYES: lambda f: EYESRecord(),
        FormType.FLOR: lambda f: FLORRecord(),
        FormType.FURN: lambda f: FURNRecord(),
        FormType.GRAS: lambda f: GRASRecord(),
        FormType.HAIR: lambda f: HAIRRecord(),
        FormType.IDLE: lambda f: IDLERecord(),
        FormType.KEYM: lambda f: KEYMRecord(),
        FormType.LSCR: lambda f: LSCRRecord(),
        FormType.LVLC: lambda f: LVLCRecord(),
        FormType.LVLI: lambda f: LVLIRecord(),
        FormType.LVSP: lambda f: LVSPRecord(),
        FormType.PACK: lambda f: PACKRecord(),
        FormType.QUST: lambda f: QUSTRecord(),
        FormType.ROAD: lambda f: ROADRecord(),
        FormType.SBSP: lambda f: SBSPRecord(),
        FormType.SGST: lambda f: SGSTRecord(),
        FormType.SLGM: lambda f: SLGMRecord(),
        FormType.TREE: lambda f: TREERecord(),
        FormType.WATR: lambda f: WATRRecord(),
        FormType.WTHR: lambda f: WTHRRecord(),
        # 5 - Skyrim
        FormType.AACT: lambda f: AACTRecord(),
        FormType.ADDN: lambda f: ADDNRecord(),
        FormType.ARMA: lambda f: ARMARecord(),
        FormType.ARTO: lambda f: ARTORecord(),
        FormType.ASPC: lambda f: ASPCRecord(),
        FormType.ASTP: lambda f: ASTPRecord(),
        FormType.AVIF: lambda f: AVIFRecord(),
        FormType.DLBR: lambda f: DLBRRecord(),
        FormType.DLVW: lambda f: DLVWRecord(),
        FormType.SNDR: lambda f: SNDRRecord(),
        # Unknown
        FormType.BOIM: lambda f: BOIMRecord(),
        FormType.BNDS: lambda f: BNDSRecord(),
        FormType.DMGT: lambda f: DMGTRecord(),
        FormType.TRNS: lambda f: TRNSRecord(),
        FormType.TXST: lambda f: TXSTRecord(),
        #
        FormType.KYWD: lambda f: KYWDRecord(),
        FormType.LCRT: lambda f: LCRTRecord(),
        FormType.FLST: lambda f: FLSTRecord(),
        FormType.OTFT: lambda f: OTFTRecord(),
        FormType.HDPT: lambda f: HDPTRecord(),
        FormType.MICN: lambda f: MICNRecord(),
    }
    cellsLoaded: int = 0
    @staticmethod
    def factory(r: Reader, type: FieldType) -> 'Record':
        record = None
        if type == FormType.CELL and Record.cellsLoaded > 100: Record.cellsLoaded += 1; record = Record() # hack to limit cells loading
        if not (z := Record._mapx.get(type)): print(f'Unsupported record type: {type}'); record = Record()
        # if type != FormType.TES3 and type != FormType.TES4 and type not in Record._factorySet: record = Record()
        else: record = z(r.format); record.type = type
        record.read(r)
        return record
    class EsmFlags(IntFlag):
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

        # @classmethod
        # def _missing_(cls, value):
        #     s = int.__new__(cls); s._value_ = value; s._name_ = f'_{hex(value)}'
        #     print(f'_missing_: {hex(value)}')
        #     return s
    # tag::Record[]
    _empty: 'Record'
    def __repr__(self) -> str: return f'{self.type}: {self.EDID.value if self.EDID else None}'
    # def __repr__(self) -> str: return f'{self.type}: {self.groupType}'
    type: FormType = 0
    dataSize: int 
    flags: EsmFlags
    @property
    def compressed(self) -> bool: return Record.EsmFlags.Compressed in self.flags
    id: int = 0

    # Reads an uninitialized subrecord to deserialize, or null to skip.
    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object: return Record._empty

    # Reads a record - tag::Record.read[]
    def read(self, r: Reader) -> None:
        self.dataSize = r.readUInt32()
        if r.format == FormType.TES3: r.skip(4) # unknown
        while True:
            self.flags = Record.EsmFlags(r.readUInt32())
            if r.format == FormType.TES3: break
            self.id = r.readUInt32()
            r.skip(4)
            if r.format == FormType.TES4: break
            r.skip(4)
            if r.format == FormType.TES5: break
            break
        if r.tes4a: r.skip(4)
    # end::Record.read[]

    # Reads a records fields - tag::Record.readFields[]
    def readFields(self, r: Reader) -> None:
        if self.compressed:
            lastSize = self.dataSize
            self.dataSize = r.readUInt32()
            data = decompressZlib2(r, lastSize - 4, self.dataSize)
            r = Reader(BinaryReader(BytesIO(data)), r.binPath, r.format, r.tes4a)
        start = r.tell(); end = start + self.dataSize
        # log.info(f'{self.type}')
        while not r.atEnd(end):
            fieldType = FieldType(r.readUInt32())
            fieldDataSize = r.readUInt32() if r.format == FormType.TES3 else r.readUInt16()
            if fieldType == FieldType.XXXX:
                if fieldDataSize != 4: raise Exception()
                fieldDataSize = r.readUInt32()
                continue
            elif fieldType == FieldType.OFST and self.type == FormType.WRLD: r.seek(end); continue
            tell = r.tell()
            if self.readField(r, fieldType, fieldDataSize) == Record._empty: print(f'Unsupported ESM record type: {self.type}Record:{fieldType}'); r.skip(fieldDataSize); continue
            r.ensureAtEnd(tell + fieldDataSize, f'Failed reading {self.type}Record:{fieldType} field data at offset {tell} in {r.binPath} of {r.tell() - tell - fieldDataSize}')
        r.ensureAtEnd(end, f'Failed reading {self.type}Record record data at offset {start} in {r.binPath}')
        if self.compressed: r.dispose()
    # end::Record.readFields[]
Record._empty = Record()

class Ref[T: Record]:
    _struct = ('<I', 4)
    def __repr__(self) -> str: return f'{self.type}:{self.id}'
    def __init__(self, t: type, *args):
        self.type = (t if isinstance(t, str) else t.__name__)[:4]
        if isinstance(args[0], Reader): r = args[0]; self.id = r.readUInt32()
        else: self.id: int = args[0]
class Ref2[T: Record]:
    _struct = ('<2I', 8)
    def __repr__(self) -> str: return f'{self.type}:{self.id}x{self.id2}'
    def __init__(self, t: type, *args):
        self.type = (t if isinstance(t, str) else t.__name__)[:4]
        if isinstance(args[0], Reader): r = args[0]; self.id = r.readUInt32(); self.id2 = r.readUInt32()
        else: self.id: int = args[0]; self.id2: int = args[1]
class RefB[T: Record]:
    _struct = ('<IB3x', 8)
    def __repr__(self) -> str: return f'{self.type}:{self.id}x{self.value}'
    def __init__(self, t: type, *args):
        self.type = (t if isinstance(t, str) else t.__name__)[:4]
        if isinstance(args[0], Reader): r = args[0]; self.id = r.readUInt32(); self.value = r.readByte(); r.skip(3)
        else: self.id: int = args[0]; self.value: int = args[1]
class RefS[T: Record]:
    def __repr__(self) -> str: return f'{self.type}:{self.id}'
    def __init__(self, t: type, *args):
        self.type = (t if isinstance(t, str) else t.__name__)[:4]
        if isinstance(args[0], Reader): r = args[0]; dataSize = args[1]; self.name = r.readFUString(dataSize)
        else: self.name: str = args[0]
class RefX[T: Record]:
    def __repr__(self) -> str: return f'{self.type}:{self.name}{self.id}'
    def __init__(self, t: type, *args): 
        self.type = (t if isinstance(t, str) else t.__name__)[:4]
        match len(args):
            case 0: self.id, self.name = (0, None)
            case 1 if isinstance(args[0], int): self.id, self.name = (args[0], None)
            case 1 if isinstance(args[0], str): self.id, self.name = (0, args[0])
            case 2:
                if isinstance(args[0], Reader):
                    r = args[0]; dataSize = args[1]
                    if dataSize == 4: self.id = r.readUInt32(); self.name = None
                    else: self.id = 0; self.name = r.readFUString(dataSize)
                else: self.id, self.name = args
            case _: raise NotImplementedError('RefX')
    def setName(self, name: str) -> 'RefX': z = self.name = name; return z

#endregion

#region Record Group

class RecordGroup:
    class GroupType(Enum):
        Top = 0                         # Label: Record type
        WorldChildren = 1               # Label: Parent (WRLD)
        InteriorCellBlock = 2           # Label: Block number
        InteriorCellSubBlock = 3        # Label: Sub-block number
        ExteriorCellBlock = 4           # Label: Grid Y, X (Note the reverse order)
        ExteriorCellSubBlock = 5        # Label: Grid Y, X (Note the reverse order)
        CellChildren = 6                # Label: Parent (CELL)
        TopicChildren = 7               # Label: Parent (DIAL)
        CellPersistentChilden = 8       # Label: Parent (CELL)
        CellTemporaryChildren = 9       # Label: Parent (CELL)
        CellVisibleDistantChildren = 10 # Label: Parent (CELL)
    dataSize: int
    label: FormType
    type: GroupType
    position: int
    path: str
    records: list[Record]
    groups: list['RecordGroup'] = None
    groupsByLabel: dict[int, 'RecordGroup'] = None
    def preload(self) -> bool: return (self.label == 0 or self.type == RecordGroup.GroupType.Top) and self.label in RecordGroup._factorySet
    # h.label in [FormType.CELL | FormType.WRLD]: self.load() # or FormType.DIAL

    def __repr__(self) -> str: return f'{self.label}'
    def __init__(self, r: Reader, path: str):
        self.records = []
        if not r: return
        if r.format == FormType.TES3: self.dataSize = r.length - r.tell(); self.label = 0; self.type = RecordGroup.GroupType.Top; self.position = r.tell(); self.path = path; return
        self.dataSize: int = r.readUInt32() - (16 if r.format == FormType.TES4 else 20)
        self.label: FormType = FormType(r.readUInt32())
        self.type: GroupType = RecordGroup.GroupType(r.readInt32())
        r.skip(4) # stamp + version
        if r.tes4a or r.format != FormType.TES4: r.skip(4) # unknown
        self.position = r.tell()
        self.path = f'{path}{str(self.label)}/'

    @staticmethod
    def readAll(r: Reader) -> Iterator['RecordGroup']:
         if r.format == FormType.TES3: yield RecordGroup(r, ''); return
         while not r.atEnd():
            type = FormType(r.readUInt32())
            if type != FormType.GRUP: raise Exception(f'{type} not GRUP')
            yield RecordGroup(r, '')

    def read(self, r: Reader, files: list[FileSource]) -> None:
        r.seek(self.position)
        end = self.position + self.dataSize
        while not r.atEnd(end):
            type = FormType(r.readUInt32())
            # print(f'{type}')
            if type == FormType.GRUP:
                _nca(self, 'groups', [])
                s = RecordGroup(r, self.path)
                if s.preload or True: s.read(r, files)
                else: r.Seek(r.tell() + s.dataSize)
                self.groups.append(s)
                continue
            record = Record.factory(r, type)
            if record.type == 0: r.skip(record.dataSize); continue
            record.readFields(r)
            self.records.append(record)
        self.recordsByType = { s:list(g) for s, g in groupby(sorted(self.records, key=lambda s: s.type), lambda s: s.type) }
        self.groupsByLabel = { s:list(g) for s, g in groupby(sorted(self.groups, key=lambda s: s.label), lambda s: s.label) } if self.groups else None
        # add items
        files.extend([FileSource(
            path = self.path + str(k),
            flags = k,
            tag = v) for k, v in self.recordsByType.items()])

#endregion

#region Fields

class Obnd:
    _struct = ('<6h', 12)
    def __init__(self, t):
        (self.x1,
        self.y1,
        self.z1,
        self.x2,
        self.y2,
        self.z2) = t

class Modl:
    class ModdFlag(Flag): Head = 0x01; Torso = 0x02; RightHand = 0x04; LeftHand = 0x08 #Fallout
    class Mods:
        def __init__(self, r: Reader, dataSize: int):
            self.x3dName: str = r.readL32UString()
            self.newTexture: Ref[TXSTRecord] = Ref[TXSTRecord](TXSTRecord, r.readUInt32())
            self.x3dIndex: int = r.readUInt32()
    def __repr__(self) -> str: return f'{self.value}'
    def __init__(self, r: Reader, dataSize: int): self.value: str = r.readFUString(dataSize)
    bound: float = 0
    textures: bytes = None # Texture Files Hashes
    altTextures: list[Mods] = None # Alternate Textures
    faceGenModelFlags: ModdFlag = None # FaceGen Model Flags
    icon: str = None # Icon
    def MODB(self, r: Reader, dataSize: int) -> object: z = self.bound = r.readSingle(); return z
    def MODT(self, r: Reader, dataSize: int) -> object: z = self.textures = r.readBytes(dataSize); return z # Texture File Hashes
    def MODS(self, r: Reader, dataSize: int) -> object: z = self.altTextures = r.readL32FArray(lambda z: Modl.Mods(r, dataSize)); return z # Alternate Textures
    def MODD(self, r: Reader, dataSize: int) -> object: z = self.faceGenModelFlags = Modl.ModdFlag(r.readByte()); return z # FaceGen Model Flags
    def ICON(self, r: Reader, dataSize: int) -> object: z = self.icon = r.readFUString(dataSize); return z # Icon

# class Modt:
#     def __init__(self, r: Reader, dataSize: int):
#         self.count = r.readUInt32()
#         self.unknown4Count = r.readUInt32() if self.count >= 1 else 0
#         self.unknown5Count = r.readUInt32() if self.count >= 2 else 0
#         self.unknown3 = r.readPArray<uint>('I', self.count - 2) if self.unknown4Count > 0 else None
#         self.unknown4 = r.readPArray<uint>('I', self.unknown5Count) if self.unknown5Count > 0 else None

# IHaveMODL
class IHaveMODL:
    # MODL: Modl
    pass

class Datv:
    def __repr__(self) -> str: return f'DATV'
    def __init__(self, b: bool = None, i: int = None, f: float = None, s: str = None): self.b: bool = b; self.i: int = i; self.f: float = f; self.s: str = s
class Cnto[T: Record]:
    def __repr__(self) -> str: return f'{self.item}:{self.count}'
    def __init__(self, t: type, r: Reader, dataSize: int): self.item = RefX(t, r.readUInt32()); self.count = r.readUInt32()
class CntoX[T: Record]:
    def __repr__(self) -> str: return f'{self.item}'
    item: 'RefX[Record]' # The ID of the item
    count: int # Number of the item
    def __init__(self, t: type, r: Reader = None, dataSize: int = 0):
        if not r: self.count = 0; self.item = RefX(Record); return
        if r.format == FormType.TES3: self.count = r.readUInt32(); self.item = RefX(Record, r.readFAString(32)); return
        self.item = RefX(Record, r.readUInt32()); self.count = r.readUInt32()

Record.EDID: str = str() # Editor ID

#endregion

#region Extensions

def _nca(self, name, value): return getattr(self, name, None) or (setattr(self, name, value), getattr(self, name))[1]
def then[T, TResult](s: Record, value: T, func: callable) -> TResult: return func(value)
class EList[T](list[T]):
    def last[T](s: list[T]) -> T: return s[-1]
    def single[T](s: list[T], func: callable) -> T: return next(iter([x for x in s if func(x)]), None)
    def addX[T](s: list[T], value: T) -> T: s.append(value); return value
    def addRangeX[T](s: list[T], value: iter) -> iter: s.extend(value); return value
def listx(s: list = []): return EList(s)
def readINTV(r: Reader, length: int) -> int:
    match length:
        case 1: return r.readByte()
        case 2: return r.readInt16()
        case 4: return r.readInt32()
        case 8: return r.readInt64()
        case _: raise Exception(f'{length}')
def readDATV(r: Reader, length: int, type: chr) -> Datv:
    match type:
        case 'b': return Datv(b=r.readInt32() != 0)
        case 'i': return Datv(i=r.readInt32())
        case 'f': return Datv(f=r.readSingle())
        case 's': return Datv(s=r.readFUString(length))
        case _: raise Exception(f'{type}')

# monkey patch
Record.then = then
Reader.readINTV = readINTV
Reader.readDATV = readDATV

#endregion

#region Records

# Record._factorySet = { FormType.ACTI, FormType.ALCH, FormType.APPA, FormType.ARMO, FormType.BODY, FormType.BSGN, FormType.CELL, FormType.CLAS, FormType.CLOT, FormType.CONT, FormType.CREA }
# Record._factorySet = { FormType.DIAL, FormType.INFO, FormType.DOOR, FormType.ENCH, FormType.FACT, FormType.GLOB }
# Record._factorySet = { FormType.MGEF, FormType.REGN, FormType.LIGH, FormType.DIAL }
Record._factorySet = None #{ FormType.GLOB }
RecordGroup._factorySet = { FormType.NPC_ }

# AACT.Action - 0050 - tag::AACT[]
class AACTRecord(Record):
    CNAM: ByteColor4 # RGB Color
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.CNAM: z = self.CNAM = r.readS(ByteColor4, dataSize)
            case _: z = Record._empty
        return z
# end::AACT[]

# ACRE.Placed creature - 0400 - tag::ACRE[]
class ACRERecord(Record):
    NAME: Ref[Record] # Base
    DATA: 'REFRRecord.Data' # Position/Rotation
    XRGDs: list['REFRRecord.Xrgd'] = None # Ragdoll Data (optional)
    XESP: 'REFRRecord.Xesp' = None # Enable Parent (optional)
    XOWNs: list['CELLRecord.Xown'] # Ownership (optional)
    XSCL: float # Scale (optional)
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.NAME: z = self.NAME = Ref[Record](Record, r, dataSize)
            case FieldType.DATA: z = self.DATA = r.readS(REFRRecord.Data, dataSize)
            case FieldType.XRGD: z = self.XRGDs = r.readSArray(REFRRecord.Xrgd, dataSize // 28)
            case FieldType.XESP: z = self.XESP = REFRRecord.Xesp(r, dataSize)
            case FieldType.XOWN: z = _nca(self, 'XOWNs', listx()).addX(CELLRecord.Xown(XOWN = RefX[Record](Record, r, dataSize)))
            case FieldType.XRNK: z = self.XOWNs.last().XRNK = r.readInt32()
            case FieldType.XGLB: z = self.XOWNs.last().XGLB = RefX[Record](Record, r, dataSize)
            case FieldType.XSCL: z = self.XSCL = r.readSingle()
            case _: z = Record._empty
        return z
# end::ACRE[]

# ACHR.Actor Reference - 0450 - tag::ACHR[]
class ACHRRecord(Record):
    NAME: RefX[Record] # Base
    DATA: 'REFRRecord.Data' # Position/Rotation
    XRGD: list['REFRRecord.Xrgd'] = None # Ragdoll Data (optional)
    XESP: 'REFRRecord.Xesp' = None # Enable Parent (optional)
    XPCI: RefX['CELLRecord'] = None # Unused (optional)
    XLOD: bytes = None # Distant LOD Data (optional)
    XMRC: RefX['REFRRecord'] = None # Merchant Container (optional)
    XHRS: RefX[ACRERecord] = None # Horse (optional)
    XSCL: float = None # Scale (optional)
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.NAME: z = self.NAME = RefX[Record](Record, r, dataSize)
            case FieldType.DATA: z = self.DATA = r.readS(REFRRecord.Data, dataSize)
            case FieldType.XRGD: z = self.XRGDs = r.readSArray(REFRRecord.Xrgd, dataSize // 28)
            case FieldType.XESP: z = self.XESP = REFRRecord.Xesp(r, dataSize)
            case FieldType.XPCI: z = self.XPCI = RefX[CELLRecord](CELLRecord, r, dataSize)
            case FieldType.FULL: z = self.XPCI.setName(r.readFAString(dataSize))
            case FieldType.XLOD: z = self.XLOD = r.readBytes(dataSize)
            case FieldType.XMRC: z = self.XMRC = RefX[REFRRecord](REFRRecord, r, dataSize)
            case FieldType.XHRS: z = self.XHRS = RefX[ACRERecord](ACRERecord, r, dataSize)
            case FieldType.XSCL: z = self.XSCL = r.readSingle()
            case _: z = Record._empty
        return z
# end::ACHR[]

# ACTI.Activator - 3450 - tag::ACTI[]
class ACTIRecord(Record, IHaveMODL):
    FULL: str # Item Name
    OBND: Obnd # Object Boundary
    MODL: Modl # Model Name
    SCRI: RefX['SCPTRecord'] = None # Script (Optional)
    # TES4
    SNAM: RefX['SOUNRecord'] = None # Sound (Optional)
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readFUString(dataSize)
            case FieldType.FULL | FieldType.FNAM: z = self.FULL = r.readFUString(dataSize)
            case FieldType.OBND: z = self.OBND = r.readS(Obnd, dataSize)
            case FieldType.MODL: z = self.MODL = Modl(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODB(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODT(r, dataSize)
            case FieldType.SCRI: z = self.SCRI = RefX[SCPTRecord](SCPTRecord, r, dataSize)
            # TES4
            case FieldType.SNAM: z = self.SNAM = RefX[SOUNRecord](SOUNRecord, r, dataSize)
            case _: z = Record._empty
        return z
# end::ACTI[]

# ADDN-Addon Node - 0050 - tag::ADDN[]
class ADDNRecord(Record):
    OBND: Obnd # Object Boundary
    CNAM: ByteColor4 # RGB Color
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.OBND: z = self.OBND = r.readS(Obnd, dataSize)
            case FieldType.CNAM: z = self.CNAM = r.readS(ByteColor4, dataSize)
            case _: z = Record._empty
        return z
# end::ADDN[]

# ALCH.Potion - 3450 - tag::ALCH[]
class ALCHRecord(Record, IHaveMODL):
    class Data:
        class Flag(Flag): NoAutoCalculate = 0x01; FoodItem = 0x02 
        Weight: float
        Value: int
        Flags: Flag
        def __init__(self, r: Reader, dataSize: int):
            self.weight = r.readSingle()
            if r.format == FormType.TES3:
                self.value = r.readInt32()
                self.flags = ALCHRecord.Data.Flag(r.readInt32())
        def ENIT(self, r: Reader, dataSize: int) -> object:
            self.value = r.readInt32()
            self.flags = ALCHRecord.Data.Flag(r.readByte())
            r.skip(3) # Unknown
            return True
    # TES3
    class Enam:
        class Range(Enum): Self = 0; Touch = 1; Target = 2
        _struct = ('<h2B5I', 24)
        def __init__(self, t):
            (self.effectId,
            self.skillId, # for skill related effects, -1/0 otherwise
            self.attributeId, # for attribute related effects, -1/0 otherwise
            self.range,
            self.area,
            self.duration,
            self.magnitudeMin,
            self.magnitudeMax) = t
            self.range = ALCHRecord.Enam.Range(self.range)

    MODL: Modl # Model
    FULL: str # Item Name
    DATA: Data # Alchemy Data
    ENAMs: list[Enam] = [] # Enchantment
    SCRI: RefX['SCPTRecord'] = None # Script (optional)
    # TES4
    EFITs: list['ENCHRecord.Efit'] = [] # Effect Data
    SCITs: list['ENCHRecord.Scit'] = [] # Script Effect Data
    def __init__(self): super().__init__(); self.ENAMs = listx(); self.EFITs = listx(); self.SCITs = listx()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readFUString(dataSize)
            case FieldType.MODL: z = self.MODL = Modl(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODB(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODT(r, dataSize)
            case FieldType.ICON | FieldType.TEXT: z = self.MODL.ICON(r, dataSize)
            case FieldType.FULL: z = FULL = r.readFUString(dataSize) if len(self.SCITs) == 0 else self.SCITs.last().FULL(r, dataSize)
            case FieldType.FNAM: z = self.FULL = r.readFUString(dataSize)
            case FieldType.DATA | FieldType.ALDT: z = self.DATA = ALCHRecord.Data(r, dataSize)
            case FieldType.ENAM: z = self.ENAMs.addX(r.readS(ALCHRecord.Enam, dataSize))
            case FieldType.SCRI: z = self.SCRI = RefX[SCPTRecord](SCPTRecord, r, dataSize)
            # TES4
            case FieldType.ENIT: z = self.DATA.ENIT(r, dataSize)
            case FieldType.EFID: z = r.skip(dataSize) #TODO
            case FieldType.EFIT: z = self.EFITs.addX(ENCHRecord.Efit(r, dataSize))
            case FieldType.SCIT: z = self.SCITs.addX(ENCHRecord.Scit(r, dataSize))
            case _: z = Record._empty
        return z
# end::ALCH[]

# AMMO.Ammo - 0450 - tag::AMMO[]
class AMMORecord(Record, IHaveMODL):
    class Data:
        class Flag(Flag): IgnoresNormalWeaponResistance = 0x1
        _struct = ('<f2IfH', 18)
        def __init__(self, t):
            (self.speed,
            self.flags,
            self.value,
            self.weight,
            self.damage) = t
            self.flags = AMMORecord.Data.Flag(self.flags)

    MODL: Modl # Model
    FULL: str # Item Name
    ENAM: RefX['ENCHRecord'] = None # Enchantment ID (optional)
    ANAM: int = None # Enchantment Points (optional)
    DATA: Data # Ammo Data
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.MODL: z = self.MODL = Modl(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODB(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODT(r, dataSize)
            case FieldType.ICON: z = self.MODL.ICON(r, dataSize)
            case FieldType.FULL: z = self.FULL = r.readFUString(dataSize)
            case FieldType.ENAM: z = self.ENAM = RefX[ENCHRecord](ENCHRecord, r, dataSize)
            case FieldType.ANAM: z = self.ANAM = r.readInt16()
            case FieldType.DATA: z = self.DATA = r.readS(AMMORecord.Data, dataSize)
            case _: z = Record._empty
        return z
# end::AMMO[]

# ANIO.Animated Object - 0450 - tag::ANIO[]
class ANIORecord(Record, IHaveMODL):
    MODL: Modl # Model
    DATA: RefX['IDLERecord'] = None # IDLE Animation
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.MODL: z = self.MODL = Modl(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODB(r, dataSize)
            case FieldType.DATA: z = self.DATA = RefX[IDLERecord](IDLERecord, r, dataSize)
            case _: z = Record._empty
        return z
# end::ANIO[]

# APPA.Alchem Apparatus - 3450 - tag::APPA[]
class APPARecord(Record, IHaveMODL):
    class Data:
        class Type_(Enum): MortarAndPestle = 0; Albemic = 1; Calcinator = 2; Retort = 3
        type: Type_
        value: int
        weight: float
        quality: float
        def __init__(self, r: Reader, dataSize: int):
            if r.format == FormType.TES3:
                self.type = APPARecord.Data.Type_(r.readInt32() & 0xFF)
                self.quality = r.readSingle()
                self.weight = r.readSingle()
                self.value = r.readInt32()
                return
            self.type = APPARecord.Data.Type_(r.readByte())
            self.value = r.readInt32()
            self.weight = r.readSingle()
            self.quality = r.readSingle()

    MODL: Modl # Model
    FULL: str # Item Name
    DATA: Data # Alchemy Data
    SCRI: RefX['SCPTRecord'] = None # Script Name
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readFUString(dataSize)
            case FieldType.MODL: z = self.MODL = Modl(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODB(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODT(r, dataSize)
            case FieldType.ICON | FieldType.ITEX: z = self.MODL.ICON(r, dataSize)
            case FieldType.FULL | FieldType.FNAM: z = self.FULL = r.readFUString(dataSize)
            case FieldType.DATA | FieldType.AADT: z = self.DATA = APPARecord.Data(r, dataSize)
            case FieldType.SCRI: z = self.SCRI = RefX[SCPTRecord](SCPTRecord, r, dataSize)
            case _: z = Record._empty
        return z
# end::APPA[]

# ARMA.Armature (Model) - 0050 - tag::ARMA[]
class ARMARecord(Record):
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case _: z = Record._empty
        return z
# end::ARMA[]

# ARMO.Armor - 3450 - tag::ARMA[]
class ARMORecord(Record, IHaveMODL):
    class Data:
        class ARMOType(Enum): Helmet = 0; Cuirass = 2; L_Pauldron = 3; R_Pauldron = 4; Greaves = 5; Boots = 6; L_Gauntlet = 7; R_Gauntlet = 8; Shield = 9; L_Bracer = 10; R_Bracer = 11
        armour: int
        value: int
        health: int
        weight: float
        # TES3
        type: int
        enchantPts: int
        def __init__(self, r: Reader, dataSize: int):
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

    class Bodt:
        class NodeFlag(Flag):
            Head = 0x00000001
            Hair = 0x00000002
            Body = 0x00000004
            Hands = 0x00000008
            Forearms = 0x00000010
            Amulet = 0x00000020
            Ring = 0x00000040
            Feet = 0x00000080
            Calves = 0x00000100
            Shield = 0x00000200
            Tail = 0x00000400
            LongHair = 0x00000800
            Circlet = 0x00001000
            Ears = 0x00002000
            BodyAddOn3 = 0x00004000
            BodyAddOn4 = 0x00008000
            BodyAddOn5 = 0x00010000
            BodyAddOn6 = 0x00020000
            BodyAddOn7 = 0x00040000
            BodyAddOn8 = 0x00080000
            DecapitateHead = 0x00100000
            Decapitate = 0x00200000
            BodyAddOn9 = 0x00400000
            BodyAddOn10 = 0x00800000
            BodyAddOn11 = 0x01000000
            BodyAddOn12 = 0x02000000
            BodyAddOn13 = 0x04000000
            BodyAddOn14 = 0x08000000
            BodyAddOn15 = 0x10000000
            BodyAddOn16 = 0x20000000
            BodyAddOn17 = 0x40000000
            FX01 = 0x80000000
        class Flag(Flag): ModulatesVoice = 0x00000001; NonPlayable = 0x00000010
        class SkillType(Enum): LightArmor = 0; HeavyArmor = 1; None_ = 2
        _struct = { 8: '<IB3s', 12: '<IB3sI' }
        def __init__(self, t):
            match len(t):
                case 3:
                    (self.nodeFlags, # Body part node flags
                    self.flags,
                    self.junkData) = t
                    self.nodeFlags = AMMORecord.Data.NodeFlag(self.nodeFlags)
                    self.flags = AMMORecord.Data.Flag(self.flags)
                    self.skill = AMMORecord.Data.SkillType(0)
                case 4:
                    (self.nodeFlags, # Body part node flags
                    self.flags,
                    self.junkData,
                    self.skill) = t
                    self.nodeFlags = AMMORecord.Data.NodeFlag(self.nodeFlags)
                    self.flags = AMMORecord.Data.Flag(self.flags)
                    self.skill = AMMORecord.Data.SkillType(self.skill)

    MODL: Modl # Male Biped Model
    FULL: str # Item Name
    DATA: Data # Armour Data
    SCRI: RefX['SCPTRecord'] = None # Script Name (optional)
    ENAM: RefX['ENCHRecord'] = None # Enchantment FormId (optional)
    # TES3
    INDXs: list['CLOTRecord.Indx'] = [] # Body Part Index
    # TES4
    BMDT: int = 0 #! Flags
    MOD2: Modl = None #! Male World Model (optional)
    MOD3: Modl = None #! Female Biped Model (optional)
    MOD4: Modl = None #! Female World Model (optional)
    ANAM: int = None # Enchantment Points (optional)
    def __init__(self): super().__init__(); self.INDXs = listx()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readFUString(dataSize)
            case FieldType.MODL: z = self.MODL = Modl(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODB(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODT(r, dataSize)
            case FieldType.ICON | FieldType.ITEX: z = self.MODL.ICON(r, dataSize)
            case FieldType.FULL | FieldType.FNAM: z = self.FULL = r.readFUString(dataSize)
            case FieldType.DATA | FieldType.AODT: z = self.DATA = ARMORecord.Data(r, dataSize)
            case FieldType.SCRI: z = self.SCRI = RefX[SCPTRecord](SCPTRecord, r, dataSize)
            case FieldType.ENAM: z = self.ENAM = RefX[ENCHRecord](ENCHRecord, r, dataSize)
            # TES3
            case FieldType.INDX: z = self.INDXs.addX(CLOTRecord.Indx(INDX = r.readINTV(dataSize)))
            case FieldType.BNAM: z = self.INDXs.last().BNAM = r.readFUString(dataSize)
            case FieldType.CNAM: z = self.INDXs.last().CNAM = r.readFUString(dataSize)
            # TES4
            case FieldType.BMDT: z = self.BMDT = r.readUInt32()
            case FieldType.MOD2: z = self.MOD2 = Modl(r, dataSize)
            case FieldType.MO2B: z = self.MOD2.MODB(r, dataSize)
            case FieldType.MO2T: z = self.MOD2.MODT(r, dataSize)
            case FieldType.ICO2: z = self.MOD2.ICON(r, dataSize)
            case FieldType.MOD3: z = self.MOD3 = Modl(r, dataSize)
            case FieldType.MO3B: z = self.MOD3.MODB(r, dataSize)
            case FieldType.MO3T: z = self.MOD3.MODT(r, dataSize)
            case FieldType.MOD4: z = self.MOD4 = Modl(r, dataSize)
            case FieldType.MO4B: z = self.MOD4.MODB(r, dataSize)
            case FieldType.MO4T: z = self.MOD4.MODT(r, dataSize)
            case FieldType.ANAM: z = self.ANAM = r.readInt16()
            case _: z = Record._empty
        return z
# end::ARMO[]

# ARTO.Art Object - 0050 - tag::ARTO[]
class ARTORecord(Record):
    CNAM: ByteColor4 # RGB Color
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.CNAM: z = self.CNAM = r.readS(ByteColor4, dataSize)
            case _: z = Record._empty
        return z
# end::ARTO[]

# ASPC.Acoustic Space - 0050 - tag::ASPC[]
class ASPCRecord(Record):
    CNAM: ByteColor4 # RGB Color
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.CNAM: z = self.CNAM = r.readS(ByteColor4, dataSize)
            case _: z = Record._empty
        return z
# end::ASPC[]

# ASTP.Association Type - 0050 - tag::ASTP[]
class ASTPRecord(Record):
    CNAM: ByteColor4 # RGB Color
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize),
            case FieldType.CNAM: z = self.CNAM = r.readS(ByteColor4, dataSize),
            case _: z = Record._empty
        return z
# end::ASTP[]

# AVIF.Actor Values_Perk Tree Graphics - 0050 - tag::ASTP[]
class AVIFRecord(Record):
    CNAM: ByteColor4 # RGB Color
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.DID = r.readFUString(dataSize)
            case FieldType.CNAM: z = self.CNAM = r.readS(ByteColor4, dataSize)
            case _: z = Record._empty
        return z
# end::AVIF[]

# BNDS.Bendable Spline - 0400 #F4 - tag::BNDS[]
class BNDSRecord(Record):
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.DID = r.readFUString(dataSize)
            case _: z = Record._empty
        return z
# end::BNDS[]

# BODY.Body - 3000 - tag::ASTP[]
class BODYRecord(Record, IHaveMODL):
    class Part(Enum): Head = 0; Hair = 1; Neck = 2; Chest = 3; Groin = 4; Hand = 5; Wrist = 6; Forearm = 7; Upperarm = 8; Foot = 9; Ankle = 10; Knee = 11; Upperleg = 12; Clavicle = 13; Tail = 14
    class Flag(Flag): Female = 1; Playable = 2
    class PartType(Enum): Skin = 0; Clothing = 1; Armor = 2

    class Bydt:
        _struct = ('<4B', 4)
        def __init__(self, t):
            (self.part,
            self.vampire,
            self.flags,
            self.partType) = t
            self.part = BODYRecord.Part(self.part)
            self.flags = BODYRecord.Flag(self.flags)
            self.partType = BODYRecord.PartType(self.partType)

    MODL: Modl # NIF Model
    FNAM: str # Body Name
    BYDT: Bydt
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        if r.format == FormType.TES3:
            match type:
                case FieldType.NAME: z = self.EDID = r.readFUString(dataSize)
                case FieldType.MODL: z = self.MODL = Modl(r, dataSize)
                case FieldType.FNAM: z = self.FNAM = r.readFUString(dataSize)
                case FieldType.BYDT: z = self.BYDT = r.readS(BODYRecord.Bydt, dataSize)
                case _: z = Record._empty
            return z
        return None
# end::BODY[]

# BOIM.Biome - 0050 - tag::BOIM[]
class BOIMRecord(Record):
    FULL: str # Item Name
    SNAM: str # Sub Name
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.SNAM: z = self.SNAM = r.readFUString(dataSize)
            case _: z = Record._empty
        return z
# end::BODY[]

# BOOK.Book - 3450 - tag::BOOK[]
class BOOKRecord(Record, IHaveMODL):
    class Flag(Flag): Scroll = 0x01; CantBeTaken = 0x02
    class Data:
        flags: Flag
        teaches: int # SkillId - (-1 is no skill)
        value: int
        weight: float
        # TES3
        enchantPts: int = 0
        def __init__(self, r: Reader, dataSize: int):
            if r.format == FormType.TES3:
                self.weight = r.readSingle()
                self.value = r.readInt32()
                self.flags = BOOKRecord.Flag(r.readInt32())
                self.teaches = ActorValue(r.readInt32())
                self.enchantPts = r.readInt32()
                return
            self.flags = BOOKRecord.Flag(r.readByte())
            self.teaches = ActorValue(r.readSByte())
            self.value = r.readInt32()
            self.weight = r.readSingle()

    MODL: Modl # Model (optional)
    FULL: str # Item Name
    DATA: Data # Book Data
    DESC: str = str() #! Book Text
    SCRI: RefX['SCPTRecord'] = None # Script Name (optional)
    ENAM: RefX['ENCHRecord'] = None # Enchantment FormId (optional)
    # TES4
    ANAM: int = None # Enchantment points (optional)
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readFUString(dataSize)
            case FieldType.MODL: z = self.MODL = Modl(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODB(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODT(r, dataSize)
            case FieldType.ICON | FieldType.ITEX: z = self.MODL.ICON(r, dataSize)
            case FieldType.FULL | FieldType.FNAM: z = self.FULL = r.readFUString(dataSize)
            case FieldType.DATA | FieldType.BKDT: z = self.DATA = BOOKRecord.Data(r, dataSize)
            case FieldType.SCRI: z = self.SCRI = RefX[SCPTRecord](SCPTRecord, r, dataSize)
            case FieldType.DESC | FieldType.TEXT: z = self.DESC = str(r.readFUString(dataSize).replace('\ufffd', '\1'))
            case FieldType.ENAM: z = self.ENAM = RefX[ENCHRecord](ENCHRecord, r, dataSize)
            # TES4
            case FieldType.ANAM: z = self.ANAM = r.readInt16()
            case _: z = Record._empty
        return z
# end::BODY[]

# BPTD.Body Part Data - 00500 - tag::BPTD[]
class BPTDRecord(Record):
    class Bpnd:
        class Flag(Flag):
            Severable = 1 << 0
            IKData = 1 << 1
            IKData_BipedData = 1 << 2
            Explodable = 1 << 3
            IKData_IsHead = 1 << 4
            IKData_Headtracking = 1 << 5
            ToHitChance_Absolute = 1 << 6
        class PartType_(Enum): Torso = 0; Head = 1; Eye = 2; LookAt = 3; FlyGrab = 4; Saddle = 5
        _struct = ('<f3Bb2BH2I2fi2If6f2I2BHf', 84)
        def __init__(self, t):
            goreEffectsPositioning = self.goreEffectsPositioning = Position()
            (self.damageMult,
            self.flags,
            self.partType,
            self.healthPercent,
            self.actorValue,
            self.toHitChance,
            self.explodable_ExplosionChance,
            self.explodable_DebrisCount,
            self.explodable_Debris,
            self.explodable_Explosion,
            self.trackingMaxAngle,
            self.explodable_DebrisScale,
            self.severable_DebrisCount,
            self.severable_Debris,
            self.severable_Explosion,
            self.severable_DebrisScale,
            goreEffectsPositioning.translate.x, goreEffectsPositioning.translate.y, goreEffectsPositioning.translate.z, goreEffectsPositioning.rotation.x, goreEffectsPositioning.rotation.y, goreEffectsPositioning.rotation.z,
            self.severable_ImpactDataSet,
            self.explodable_ImpactDataSet,
            self.severable_DecalCount,
            self.explodable_DecalCount,
            self.unknown,
            self.limbReplacementScale) = t
            self.flags = BPTDRecord.Bpnd.Flag(self.flags)
            self.partType = BPTDRecord.Bpnd.PartType_(self.partType)
            self.actorValue = ActorValue(self.actorValue)
            self.explodable_Debris = Ref[Record](self.explodable_Debris)
            self.explodable_Explosion = Ref[Record](self.explodable_Explosion)
            self.severable_Debris = Ref[Record](self.severable_Debris)
            self.severable_Explosion = Ref[Record](self.severable_Explosion)
            self.severable_ImpactDataSet = Ref[Record](self.severable_ImpactDataSet)
            self.explodable_ImpactDataSet = Ref[Record](self.explodable_ImpactDataSet)

    MODL: Modl # Model
    BPTN: str # Body part name
    BPNN: str # Body part node name
    BPNT: str # Body part node title
    BPNI: str # Body part node info
    BPND: Bpnd # Body part node data
    NAM1: str # Limb Replacement Model
    NAM4: str # Gore Effects
    NAM5: str # Hashes
    RAGA: Ref[Record] # Hashes //TODO RGDLRecord

    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.MODL: z = self.MODL = Modl(r, dataSize)
            case FieldType.BPTN: z = self.BPTN = r.readFUString(dataSize)
            case FieldType.BPNN: z = self.BPNN = r.readFUString(dataSize)
            case FieldType.BPNT: z = self.BPNT = r.readFUString(dataSize)
            case FieldType.BPNI: z = self.BPNI = r.readFUString(dataSize)
            case FieldType.BPND: z = self.BPND = r.readS(BPTDRecord,Bpnd, dataSize)
            case FieldType.NAM1: z = self.NAM1 = r.readFUString(dataSize)
            case FieldType.NAM4: z = self.NAM4 = r.readFUString(dataSize)
            case FieldType.NAM5: z = self.NAM5 = r.readFUString(dataSize)
            case FieldType.RAGA: z = self.RAGA = Ref[Record](Record, r, dataSize)
            case _: z = Record._empty
        return z
# end::BPTD[]

# BSGN.Birthsign - 3400 - tag::BSGN[]
class BSGNRecord(Record):
    FULL: str # Sign Name
    ICON: str # Texture
    DESC: str # Description
    NPCSs: list[str] # TES3: Spell/ability
    SPLOs: list[RefX[Record]] = None #! TES4: (points to a SPEL or LVSP record)
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readFUString(dataSize)
            case FieldType.FULL | FieldType.FNAM: z = self.FULL = r.readFUString(dataSize)
            case FieldType.ICON | FieldType.TNAM: z = self.ICON = r.readFUString(dataSize)
            case FieldType.DESC: z = self.DESC = r.readFUString(dataSize)
            case FieldType.SPLO | FieldType.NPCS: z = _nca(self, 'SPLOs', listx()).addX(RefX[Record](Record, r, dataSize))
            # case FieldType.NPCS: z = _nca(self, 'NPCSs', listx()).addX(r.readFUString(dataSize))
            case _: z = Record._empty
        return z
# end::BSGN[]

# CAMS.Camera Shot - 00500 - tag::CAMS[]
class CAMSRecord(Record):
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case _: z = Record._empty
        return z
# end::CAMS[]

# CELL.Cell - 3450 - tag::CELL[]
class CELLRecord(Record): #ICellRecord
    class Flag(Flag):
        Interior = 0x0001
        HasWater = 0x0002
        InvertFastTravel = 0x0004 # IllegalToSleepHere
        BehaveLikeExterior = 0x0008 # BehaveLikeExterior (Tribunal), Force hide land (exterior cell) / Oblivion interior (interior cell)
        Unknown1 = 0x0010
        PublicArea = 0x0020 # Public place
        HandChanged = 0x0040
        ShowSky = 0x0080 # Behave like exterior
        UseSkyLighting = 0x0100

    class Xclc:
        def __repr__(self): return f'{self.gridX}x{self.gridY}'
        _struct = { 8: '<2i', 12: '<2iI' }
        def __init__(self, t):
            match len(t):
                case 2:
                    (self.gridX,
                    self.gridY) = t
                    self.flags = 0
                case 3:
                    (self.gridX,
                    self.gridY,
                    self.flags) = t

    class Xcll:
        _struct = { 16: '<12cf', 36: '<12c2f2i2f', 40: '<12c2f2i3f' }
        def __init__(self, t):
            ambientColor = self.ambientColor = ByteColor4()
            directionalColor = self.directionalColor = ByteColor4()
            fogColor = self.fogColor = ByteColor4()
            match len(t):
                case 13:
                    (ambientColor.r, ambientColor.g, ambientColor.b, ambientColor.a,
                    directionalColor.r, directionalColor.g, directionalColor.b, directionalColor.a, # SunlightColor
                    fogColor.r, fogColor.g, fogColor.b, fogColor.a,
                    self.fogNear) = t # FogDensity
                case 18:
                    (ambientColor.r, ambientColor.g, ambientColor.b, ambientColor.a,
                    directionalColor.r, directionalColor.g, directionalColor.b, directionalColor.a, # SunlightColor
                    fogColor.r, fogColor.g, fogColor.b, fogColor.a,
                    self.fogNear, # FogDensity
                    # TES4
                    self.fogFar,
                    self.directionalRotationXY,
                    self.directionalRotationZ,
                    self.directionalFade,
                    self.fogClipDist) = t
                case 19:
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
                    self.fogPow) = t

    class Xown:
        def __init__(self, XOWN: RefX[Record]):
            self.XOWN: RefX[Record] = XOWN
            self.XRNK: int = None # Faction rank
            self.XGLB: RefX[Record] = None

    class Xyza:
        _struct = ('<3f3f', 24)
        def __init__(self, t):
            position = self.position = Float3()
            eulerAngles = self.eulerAngles = Float3()
            (position.x, position.y, position.z,
            eulerAngles.x, eulerAngles.y, eulerAngles.z) = t
            
    class Ref_:
        def __repr__(self): return f'CREF: {self.EDID}'
        FRMR: int = None # Object Index (starts at 1)
        # This is used to uniquely identify objects in the cell. For files the index starts at 1 and is incremented for each object added. For modified objects the index is kept the same.
        EDID: str # Object ID
        XSCL: float = None # Scale (Static)
        DELE: int = None # Indicates that the reference is deleted.
        DODT: 'Xyza' = None # XYZ Pos, XYZ Rotation of exit
        DNAM: str = str() #! Door exit name (Door objects)
        FLTV: float = None # Follows the DNAM optionally, lock level
        KNAM: str = str() #! Door key
        TNAM: str = str() #! Trap name
        UNAM: int = None # Reference Blocked (only occurs once in MORROWIND.ESM)
        ANAM: str = str() #! Owner ID string
        BNAM: str = str() #! Global variable/rank ID
        INTV: int = None # Number of uses, occurs even for objects that don't use it
        NAM9: int = None # Unknown
        XSOL: str = str() #! Soul Extra Data (ID string of creature)
        DATA: 'Xyza' # RefX Position Data
        # TES?
        CNAM: str = str() #! Unknown
        NAM0: int = None # Unknown
        XCHG: int = None # Unknown
        INDX: int = None # Unknown

    FULL: str # Full Name / TES3:RGNN - Region name
    DATA: int # Flags
    XCLC: Xclc = None # Cell Data (only used for exterior cells)
    XCLL: Xcll = None # Lighting (only used for interior cells)
    XCLW: float = None # Water Height
    # TES3
    NAM0: int = None # Number of objects in cell in current file (Optional)
    INTV: int = 0 #! Unknown
    NAM5: ByteColor4 = None # Map Color (COLORREF)
    # TES4
    XCLRs: list[Ref['REGNRecord']] = None #! Regions
    XCMT: int = None # Music (optional)
    XCCM: RefX['CLMTRecord'] = None # Climate
    XCWT: RefX['WATRRecord'] = None # Water
    XOWNs: list[Xown] = [] # Ownership
    # Referenced Object Data Grouping
    refObjs: list[Ref_] = []
    _inFRMR: bool = False
    _lastRef: Ref_
    # Grid
    isInterior: bool
    gridId: Int3
    ambientLight: Color
    def __init__(self): super().__init__(); self.XOWNs = listx(); self.refObjs = listx()

    def complete(self, r: Reader) -> None:
        self.isInterior = (self.DATA & 0x01) == 0x01
        self.gridId = None #Int3(self.XCLC.gridX, self.XCLC.gridY, -1 if self.isInterior else 0)
        self.ambientLight = self.XCLL.ambientColor.asColor if self.XCLL else None

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        # print(f'   {type}')
        if not self._inFRMR and type == FieldType.FRMR: self._inFRMR = True
        if not self._inFRMR:
            match type:
                case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readFUString(dataSize)
                case FieldType.FULL | FieldType.RGNN: z = self.FULL = r.readFUString(dataSize)
                case FieldType.DATA:
                    z = self.DATA = r.readINTV(4 if r.format == FormType.TES3 else dataSize)
                    if r.format == FormType.TES3: self.XCLC = r.readS(CELLRecord.Xclc, 8)
                case FieldType.XCLC: z = self.XCLC = r.readS(CELLRecord.Xclc, dataSize)
                case FieldType.XCLL | FieldType.AMBI: z = self.XCLL = r.readS(CELLRecord.Xcll, dataSize)
                case FieldType.XCLW | FieldType.WHGT: z = self.XCLW = r.readSingle()
                # TES3
                case FieldType.NAM0: z = self.NAM0 = r.readUInt32()
                case FieldType.INTV: z = self.INTV = r.readINTV(dataSize)
                case FieldType.NAM5: z = self.NAM5 = r.readS(ByteColor4, dataSize)
                # TES4
                case FieldType.XCLR: z = self.XCLRs = r.readFArray(lambda z: Ref(REGNRecord, r, 4), dataSize >> 2)
                case FieldType.XCMT: z = self.XCMT = r.readByte()
                case FieldType.XCCM: z = self.XCCM = RefX[CLMTRecord](CLMTRecord, r, dataSize)
                case FieldType.XCWT: z = self.XCWT = RefX[WATRRecord](WATRRecord, r, dataSize)
                case FieldType.XOWN: z = self.XOWNs.addX(CELLRecord.Xown(XOWN = RefX(Record, r, dataSize)))
                case FieldType.XRNK: z = self.XOWNs.last().XRNK = r.readInt32()
                case FieldType.XGLB: z = self.XOWNs.last().XGLB = RefX(Record, r, dataSize)
                case _: z = Record._empty
            return z
        # Referenced Object Data Grouping
        match type:
            # RefObjDataGroup sub-records
            case FieldType.FRMR: self._lastRef = self.refObjs.addX(CELLRecord.Ref_()); z = self._lastRef.FRMR = r.readUInt32()
            case FieldType.NAME: z = self._lastRef.EDID = r.readFUString(dataSize)
            case FieldType.XSCL: z = self._lastRef.XSCL = r.readSingle()
            case FieldType.DODT: z = self._lastRef.DODT = r.readS(CELLRecord.Xyza, dataSize)
            case FieldType.DNAM: z = self._lastRef.DNAM = r.readFUString(dataSize)
            case FieldType.FLTV: z = self._lastRef.FLTV = r.readSingle()
            case FieldType.KNAM: z = self._lastRef.KNAM = r.readFUString(dataSize)
            case FieldType.TNAM: z = self._lastRef.TNAM = r.readFUString(dataSize)
            case FieldType.UNAM: z = self._lastRef.UNAM = r.readByte()
            case FieldType.ANAM: z = self._lastRef.ANAM = r.readFUString(dataSize)
            case FieldType.BNAM: z = self._lastRef.BNAM = r.readFUString(dataSize)
            case FieldType.INTV: z = self._lastRef.INTV = r.readInt32()
            case FieldType.NAM9: z = self._lastRef.NAM9 = r.readUInt32()
            case FieldType.XSOL: z = self._lastRef.XSOL = r.readFUString(dataSize)
            case FieldType.DATA: z = self._lastRef.DATA = r.readS(CELLRecord.Xyza, dataSize)
            # TES
            case FieldType.CNAM: z = self._lastRef.CNAM = r.readFUString(dataSize)
            case FieldType.NAM0: z = self._lastRef.NAM0 = r.readUInt32()
            case FieldType.XCHG: z = self._lastRef.XCHG = r.readInt32()
            case FieldType.INDX: z = self._lastRef.INDX = r.readInt32()
            case _: z = Record._empty
        return z
# end::CELL[]

# CLAS.Class - 3450 - tag::CLAS[]
class CLASRecord(Record):
    class Data:
        class Specialization_(Enum): Combat = 0; Magic = 1; Stealth = 2
        class Flag(IntFlag): Playable = 0x00000001; Guard = 0x00000002
        class Service(IntFlag):
            Weapon = 0x00001
            Armor = 0x00002
            Clothing = 0x00004
            Books = 0x00008
            Ingredients = 0x00010
            Picks = 0x00020
            Probes = 0x00040
            Lights = 0x00080
            Apparatus = 0x00100
            RepairItems = 0x00200
            Misc = 0x00400
            Spells = 0x00800
            MagicItems = 0x01000
            Potions = 0x02000
            Training = 0x04000
            Spellmaking = 0x08000
            Enchanting = 0x10000
            Repair = 0x20000
        skillTrained: ActorValue = ActorValue.None_
        maximumTrainingLevel: int = 0
        unused: int = 0
        def __init__(self, r: Reader, dataSize: int):
            if r.format == FormType.TES3:
                self.primaryAttributes = [ActorValue(r.readUInt32()), ActorValue(r.readUInt32())]
                self.specialization = CLASRecord.Data.Specialization_(r.readUInt32())
                self.majorSkills = r.readPArray(ActorValue, 'I', 10)
                self.flags = CLASRecord.Data.Flag(r.readUInt32())
                self.services = CLASRecord.Data.Service(r.readUInt32()) # Buys/Sells and Services
            elif r.format == FormType.TES4:
                if not r.tes4a:
                    self.primaryAttributes = [ActorValue(r.readUInt32()), ActorValue(r.readUInt32())]
                    self.specialization = CLASRecord.Data.Specialization_(r.readUInt32())
                    self.majorSkills = r.readPArray(ActorValue, 'i', 7)
                else:
                    self.majorSkills = r.readPArray(ActorValue, 'i', 4)
                self.flags = CLASRecord.Data.Flag(r.readUInt32())
                self.services = CLASRecord.Data.Service(r.readUInt32()) # Buys/Sells and Services
                if not r.tes4a and dataSize == 48: return
                self.skillTrained = ActorValue(r.readSByte())
                self.maximumTrainingLevel = r.readByte() # (0-100)
                self.unused = r.readUInt16()
                if self.skillTrained != ActorValue.None_: self.skillTrained += 12
            elif r.format == FormType.TES5:
                r.skip(dataSize) # TODO
            else: raise NotImplementedError('CLASRecord')

    class Attr:
        _struct = ('<7B', 7)
        def __repr__(self): return f'SPECIAL'
        def __init__(self, t):
            (self.strength,
            self.perception,
            self.endurance,
            self.charisma,
            self.intelligence,
            self.agility,
            self.luck) = t

    FULL: str # Name
    DESC: str = str() #! Description
    DATA: Data # Data
    # TES4
    ICON: str = None # Large icon filename (Optional)
    MICO: str = None # Small icon filename (Optional)
    ATTR: Attr = None # SPECIAL (Fallout)
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readFUString(dataSize)
            case FieldType.FULL | FieldType.FNAM: z = self.FULL = r.readFUString(dataSize)
            case FieldType.CLDT | FieldType.DATA: z = self.DATA = CLASRecord.Data(r, dataSize)
            case FieldType.DESC: z = self.DESC = r.readFUString(dataSize)
            # TES4
            case FieldType.ICON: z = self.ICON = r.readFUString(dataSize)
            case FieldType.MICO: z = self.MICO = r.readFUString(dataSize)
            case FieldType.ATTR: z = self.ATTR = r.readS(CLASRecord.Attr, dataSize)
            case _: z = Record._empty
        return z
# end::CLAS[]

# CLFM.Color - 00500 - tag::CLFM[]
class CLFMRecord(Record):
    def __init__(self): super().__init__()
    
    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case _: z = Record._empty
        return z
# end::CLFM[]

# CLMT.Climate - 0450 - tag::CLMT[]
class CLMTRecord(Record, IHaveMODL):
    class Wlst:
        def __init__(self, r: Reader, dataSize: int):
            self.weather: RefX[WTHRRecord] = RefX[WTHRRecord](WTHRRecord, r.readUInt32())
            self.chance: int = r.readInt32()

    class Tnam:
        _struct = ('<6B', 6)
        def __init__(self, t):
            (self.sunriseBegin,
            self.sunriseEnd,
            self.sunsetBegin,
            self.sunsetEnd,
            self.volatility,
            self.moonsPhaseLength) = t

    MODL: Modl # Model
    FNAM: str # Sun Texture
    GNAM: str # Sun Glare Texture
    WLSTs: list[Wlst] = [] # Climate
    TNAM: Tnam # Timing
    def __init__(self): super().__init__(); self.WLSTs = listx() 
    
    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.MODL: z = self.MODL = Modl(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODB(r, dataSize)
            case FieldType.FNAM: z = self.FNAM = r.readFUString(dataSize)
            case FieldType.GNAM: z = self.GNAM = r.readFUString(dataSize)
            case FieldType.WLST: z = self.WLSTs.addRangeX(r.readFArray(lambda z: CLMTRecord.Wlst(r, dataSize), dataSize >> 3))
            case FieldType.TNAM: z = self.TNAM = r.readS(CLMTRecord.Tnam, dataSize)
            case _: z = Record._empty
        return z
# end::CLMT[]


# CLOT.Clothing - 3450 - tag::CLOT[]
class CLOTRecord(Record, IHaveMODL):
    class Data:
        class Type(Enum): Pants = 0; Shoes = 1; Shirt = 2; Belt = 3; Robe = 4; R_Glove = 5; L_Glove = 6; Skirt = 7; Ring = 8; Amulet = 9
        value: int
        weight: float
        # TES3
        type: Type = Type(0) #!
        enchantPts: int = 0 #!
        def __init__(self, r: Reader, dataSize: int):
            if r.format == FormType.TES3:
                self.type = CLOTRecord.Data.Type(r.readInt32())
                self.weight = r.readSingle()
                self.value = r.readInt16()
                self.enchantPts = r.readInt16()
                return
            self.value = r.readInt32()
            self.weight = r.readSingle()

    class Indx:
        def __repr__(self): return f'{self.INDX}: {self.BNAM}'
        def __init__(self, INDX: int):
            self.INDX: int = INDX
            self.BNAM: str = str()
            self.CNAM: str = str()

    MODL: Modl # Model Name
    FULL: str # Item Name
    DATA: Data # Clothing Data
    ENAM: str = None # Enchantment Name
    SCRI: RefX['SCPTRecord'] = None # Script Name
    # TES3
    INDXs: list[Indx] = [] # Body Part Index (Moved to Race)
    # TES4
    BMDT: int = 0 #! Clothing Flags
    MOD2: Modl = None #! Male world model (optional)
    MOD3: Modl = None #! Female biped (optional)
    MOD4: Modl = None #! Female world model (optional)
    ANAM: int = None # Enchantment points (optional)
    def __init__(self): super().__init__(); self.INDXs = listx()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readFUString(dataSize)
            case FieldType.MODL: z = self.MODL = Modl(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODB(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODT(r, dataSize)
            case FieldType.ICON | FieldType.ITEX: z = self.MODL.ICON(r, dataSize)
            case FieldType.FULL | FieldType.FNAM: z = self.FULL = r.readFUString(dataSize)
            case FieldType.DATA | FieldType.CTDT: z = self.DATA = CLOTRecord.Data(r, dataSize)
            case FieldType.INDX: z = self.INDXs.addX(CLOTRecord.Indx(INDX = r.readINTV(dataSize)))
            case FieldType.BNAM: z = self.INDXs.last().BNAM = r.readFUString(dataSize)
            case FieldType.CNAM: z = self.INDXs.last().CNAM = r.readFUString(dataSize)
            case FieldType.ENAM: z = self.ENAM = r.readFUString(dataSize)
            case FieldType.SCRI: z = self.SCRI = RefX[SCPTRecord](SCPTRecord, r, dataSize)
            case FieldType.BMDT: z = self.BMDT = r.readUInt32()
            case FieldType.MOD2: z = self.MOD2 = Modl(r, dataSize)
            case FieldType.MO2B: z = self.MOD2.MODB(r, dataSize)
            case FieldType.MO2T: z = self.MOD2.MODT(r, dataSize)
            case FieldType.ICO2: z = self.MOD2.ICON(r, dataSize)
            case FieldType.MOD3: z = self.MOD3 = Modl(r, dataSize)
            case FieldType.MO3B: z = self.MOD3.MODB(r, dataSize)
            case FieldType.MO3T: z = self.MOD3.MODT(r, dataSize)
            case FieldType.MOD4: z = self.MOD4 = Modl(r, dataSize)
            case FieldType.MO4B: z = self.MOD4.MODB(r, dataSize)
            case FieldType.MO4T: z = self.MOD4.MODT(r, dataSize)
            case FieldType.ANAM: z = self.ANAM = r.readInt16()
            case _: z = Record._empty
        return z
# end::CLOT[]

# CONT.Container - 3450 - tag::CONT[]
class CONTRecord(Record, IHaveMODL):
    class Data:
        flags: int # flags 0x0001 = Organic, 0x0002 = Respawns, organic only, 0x0008 = Default, unknown
        weight: float
        def __init__(self, r: Reader, dataSize: int):
            if r.format == FormType.TES3:
                self.weight = r.readSingle()
                return
            self.flags = r.readByte()
            self.weight = r.readSingle()
        def FLAG(self, r: Reader, dataSize: int) -> object: z = self.flags = r.readUInt32() & 0xFF; return z

    MODL: Modl # Model
    FULL: str # Container Name
    DATA: Data # Container Data
    SCRI: RefX['SCPTRecord'] = None
    CNTOs: list[CntoX[Record]] = []
    # TES4
    SNAM: RefX['SOUNRecord'] # Open sound
    QNAM: RefX['SOUNRecord'] # Close sound
    def __init__(self): super().__init__(); self.CNTOs = listx()
    
    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readFUString(dataSize)
            case FieldType.MODL: z = self.MODL = Modl(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODB(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODT(r, dataSize)
            case FieldType.FULL | FieldType.FNAM: z = self.FULL = r.readFUString(dataSize)
            case FieldType.DATA | FieldType.CNDT: z = self.DATA = CONTRecord.Data(r, dataSize)
            case FieldType.FLAG: z = self.DATA.FLAG(r, dataSize)
            case FieldType.CNTO | FieldType.NPCO: z = self.CNTOs.addX(CntoX(Record, r, dataSize))
            case FieldType.SCRI: z = self.SCRI = RefX[SCPTRecord](SCPTRecord, r, dataSize)
            case FieldType.SNAM: z = self.SNAM = RefX[SOUNRecord](SOUNRecord, r, dataSize)
            case FieldType.QNAM: z = self.QNAM = RefX[SOUNRecord](SOUNRecord, r, dataSize)
            case _: z = Record._empty
        return z
# end::CONT[]

# CREA.Creature - 3450 - tag::CREA[]
class CREARecord(Record, IHaveMODL):
    class Flag(Flag):
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

    MODL: Modl # NIF Model
    FULL: str # Full name
    def __init__(self): super().__init__()
# end::CREA[]

# tag::CREA3[]
class CREA3Record(CREARecord):
    class Npdt96:
        _struct = ('<24i', 96)
        def __init__(self, t):
            (self.type, # 0 = Creature, 1 = Daedra, 2 = Undead, 3 = Humanoid
            self.level,
            self.strength,
            self.intelligence,
            self.willpower,
            self.agility,
            self.speed,
            self.endurance,
            self.personality,
            self.luck,
            self.health,
            self.spellPts,
            self.fatigue,
            self.soul,
            self.combat,
            self.magic,
            self.stealth,
            self.attackMin1,
            self.attackMax1,
            self.attackMin2,
            self.attackMax2,
            self.attackMin3,
            self.attackMax3,
            self.gold) = t

    class Npdt52:
        _struct = ('<h8B27sB3h4Bi', 52)
        def __init__(self, t):
            (self.level,
            self.strength,
            self.intelligence,
            self.willpower,
            self.agility,
            self.speed,
            self.endurance,
            self.personality,
            self.luck,
            self.skills,
            self.reputation,
            self.health,
            self.spellPts,
            self.fatigue,
            self.disposition,
            self.factionId,
            self.rank,
            self.unknown1,
            self.gold) = t

    class Npdt12:
        _struct = ('<h6Bi', 12)
        def __init__(self, t):
            (self.level,
            self.disposition,
            self.factionId,
            self.rank,
            self.unknown1,
            self.unknown2,
            self.unknown3,
            self.gold) = t

    class AIFlags(IntFlag):
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
        Misc = 0x00400 # Miscellaneous
        Spells = 0x00800
        MagicItems = 0x01000
        Potions = 0x02000
        Training = 0x04000
        Spellmaking = 0x08000
        Recharge = 0x10000 # Enchanting
        RepairItem = 0x20000

    class Aidt:
        _struct = ('<8BI', 12)
        def __init__(self, t):
            (self.hello,
            self.unknown1,
            self.fight,
            self.flee,
            self.alarm,
            self.unknown2,
            self.unknown3,
            self.unknown4,
            self.flags) = t
            self.flags = CREA3Record.AIFlags(self.flags)

    # Activate package
    class Ai_a:
        _struct = ('<32sB', 33)
        def __init__(self, t):
            (self.name,
            self.unknown) = t

    # Escort package
    class Ai_e:
        _struct = ('<3fh32sh', 48)
        def __init__(self, t):
            (self.x,
            self.y,
            self.z,
            self.duration,
            self.id,
            self.unknown) = t

    # Follow package
    class Ai_f:
        _struct = ('<3fh32sh', 48)
        def __init__(self, t):
            (self.x,
            self.y,
            self.z,
            self.duration,
            self.id,
            self.unknown) = t

    # Travel package
    class Ai_t:
        _struct = ('<4f', 16)
        def __init__(self, t):
            (self.x,
            self.y,
            self.z,
            self.unknown) = t

    # Wander package
    class Ai_w:
        _struct = ('<2hB8sB', 14)
        def __init__(self, t):
            (self.distance,
            self.duration,
            self.timeOfDay,
            self.idles,
            self.unknown) = t

    class Ai:
        def __repr__(self) -> str: return f'{self.ai}'
        def __init__(self, r: Reader, dataSize: int, type: FieldType):
            match type:
                case FieldType.AI_A: self.AI = r.readS(CREA3Record.Ai_a, dataSize) # AI Activate
                case FieldType.AI_E: self.AI = r.readS(CREA3Record.Ai_e, dataSize) # AI Escort
                case FieldType.AI_F: self.AI = r.readS(CREA3Record.Ai_f, dataSize) # AI Follow
                case FieldType.AI_T: self.AI = r.readS(CREA3Record.Ai_t, dataSize) # AI Travel
                case FieldType.AI_W: self.AI = r.readS(CREA3Record.Ai_w, dataSize) # AI Wander
            self.AI: object = None # AI
            self.CNAM: str = None # Cell escort/follow to string (optional)

    CNAM: str # Sound Gen Creature
    NPDT: object # Creature data
    FLAG: int # Creature Flags
    SCRI: RefX['SCPTRecord'] = None # Script
    NPCOs: list[CntoX[Record]] = [] # Item record
    AIDT: Aidt # AI data
    AIs: list[Ai] = [] # AI packages
    XSCL: float = None # Scale (optional), Only present if the scale is not 1.0
    NPCSs: list[str] = []
    def __init__(self): super().__init__(); self.NPCOs = listx(); self.AIs = listx(); self.NPCSs = listx()
    
    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.NAME: z = self.EDID = r.readFUString(dataSize)
            case FieldType.MODL: z = self.MODL = Modl(r, dataSize)
            case FieldType.CNAM: z = self.CNAM = r.readFUString(dataSize)
            case FieldType.FNAM: z = self.FULL = r.readFUString(dataSize)
            case FieldType.NPDT: z = self.NPDT = r.readS(CREA3Record.Npdt96, dataSize)
            case FieldType.FLAG: z = self.FLAG = r.readInt32()
            case FieldType.SCRI: z = self.SCRI = RefX[SCPTRecord](SCPTRecord, r, dataSize)
            case FieldType.NPCO: z = self.NPCOs.addX(CntoX(Record, r, dataSize))
            case FieldType.AIDT: z = self.AIDT = r.readS(CREA3Record.Aidt, dataSize)
            case FieldType.AI_A | FieldType.AI_E | FieldType.AI_F | FieldType.AI_T | FieldType.AI_W: z = self.AIs.addX(CREA3Record.Ai(r, dataSize, type))
            case FieldType.CNDT: z = self.AIs.last().CNDT = r.readFUString(dataSize)
            case FieldType.XSCL: z = self.XSCL = r.readSingle()
            case FieldType.NPCS: z = self.NPCSs.addX(r.readFAString(dataSize))
            case _: z = Record._empty
        return z
# end::CREA3[]

# tag::CREA4[]
class CREA4Record(Record, IHaveMODL):
    class Acbs:
        _struct = ('<I3Hh2H', 16)
        def __init__(self, t):
            (self.flags,     # Flags
            self.baseSpell,  # Base spell points
            self.fatigue,    # Fatigue
            self.barterGold, # Barter gold
            self.level,      # Level/Offset level
            self.calcMin,    # Calc Min
            self.calcMax) = t # Calc Max

    class Aidt:
        _struct = ('<4BI2BH', 12)
        def __init__(self, t):
            (self.aggression,# Aggression
            self.confidence, # Confidence
            self.energyLevel,# Energy Level
            self.barterGold, # Barter gold
            self.aiFlags,    # Flags
            self.trainSkill, # Training skill
            self.trainLevel, # Training level - Value is same as index into skills array below.
            self.aiUnknown) = t # Unused?
            self.aiFlags = CREA3Record.AIFlags(self.aiFlags)

    class Csdt:
        def __init__(self, CSDT: int):
            self.CSDT: int = CSDT # Soundtype
            self.CSDI: Ref[SOUNRecord] = None # TESSound
            self.CSDC: int = None # Chance

    ACBS: str # Configuration
    NIFZ: str # NIF-files used by the creature
    ACBS: Acbs # Configuration
    SNAMs: list[RefB['FACTRecord']] = [] # Factions
    INAM: Ref['LVLIRecord'] # Death Item
    RNAM: int # Attack reach
    SPLOs: list[str] = [] # Spells
    SCRI: Ref['SCPTRecord'] # Script
    CNTOs: list[Cnto[Record]] = [] # Items
    PKIDs: list[RefS['PACKRecord']] = [] # AI Packages
    CSCR: Ref['CSTYRecord'] # Combat Style
    CSCR: Ref['CREA4Record'] # Inherits Sounds from
    CSDTs: list[Csdt] = [] # Soundtypes
    BNAM: float # Base Scale
    TNAM: float # Turning Speed
    WNAM: float # Foot Weight
    NAM0: str # Blood Spray
    NAM1: str # Blood Decal
    KFFZ: str # Optional Animation List
    def __init__(self): super().__init__(); self.SNAMs = listx(); self.SPLOs = listx(); self.CNTOs = listx(); self.PKIDs = listx(); self.CSDTs = listx()
    
    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.FULL: z = self.FULL = r.readFUString(dataSize)
            case FieldType.MODL: z = self.MODL = Modl(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODB(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODT(r, dataSize)
            case FieldType.NIFZ: z = self.NIFZ = r.readFUString(dataSize)
            case FieldType.ACBS: z = self.ACBS = r.readS(CREA4Record.Acbs, dataSize)
            case FieldType.SNAM: z = self.SNAMs.addX(RefB[FACTRecord](FACTRecord, r, dataSize))
            case FieldType.INAM: z = self.INAM = Ref[LVLIRecord](LVLIRecord, r, dataSize)
            case FieldType.RNAM: z = self.RNAM = r.readByte()
            case FieldType.SPLO: z = self.SPLOs.addX(r.readFUString(dataSize))
            case FieldType.SCRI: z = self.SCRI = Ref[SCPTRecord](SCPTRecord, r, dataSize)
            case FieldType.CNTO: z = self.CNTOs.addX(Cnto[Record](Record, r, dataSize))
            case FieldType.PKID: z = self.PKIDs.addX(RefS[PACKRecord](PACKRecord, r, dataSize))
            case FieldType.ZNAM: z = self.ZNAM = Ref[CSTYRecord](CSTYRecord, r, dataSize)
            case FieldType.CSCR: z = self.CSCR = Ref[CSTYRecord](CSTYRecord, r, dataSize)
            case FieldType.CSDT: z = self.CSDTs.addX(CREA4Record.Csdt(CSDT = r.readInt32()))
            case FieldType.CSDI: z = self.CSDTs.last().CSDI = Ref[SOUNRecord](SOUNRecord, r, dataSize)
            case FieldType.CSDC: z = self.CSDTs.last().CSDC = r.readByte()
            case FieldType.BNAM: z = self.BNAM = r.readSingle()
            case FieldType.TNAM: z = self.TNAM = r.readSingle()
            case FieldType.WNAM: z = self.WNAM = r.readSingle()
            case FieldType.NAM0: z = self.NAM0 = r.readFUString(dataSize)
            case FieldType.NAM1: z = self.NAM1 = r.readFUString(dataSize)
            case FieldType.KFFZ: z = self.KFFZ = r.readFUString(dataSize)
            case FieldType.NIFT: z = r.skip(dataSize) #TODO
            case FieldType.AIDT: z = r.skip(dataSize) #TODO
            case FieldType.DATA: z = r.skip(dataSize) #TODO
            case _: z = Record._empty
        return z
# end::CREA4[]

# CSTY.Combat Style - 0450 - tag::CSTY[]
class CSTYRecord(Record):
    class Cstd:
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
        def __init__(self, r: Reader, dataSize: int):
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
            if dataSize == 84: return
            self.rangeMult_Optimal = r.readSingle()
            self.RangeMult_Max = r.readSingle()
            if dataSize == 92: return
            self.switchDistance_Melee = r.readSingle()
            self.switchDistance_Ranged = r.readSingle()
            self.buffStandoffDistance = r.readSingle()
            if dataSize == 104: return
            self.rangedStandoffDistance = r.readSingle()
            self.GroupStandoffDistance = r.readSingle()
            if dataSize == 112: return
            self.rushingAttackPercentChance = r.readByte()
            r.skip(3) # Unused
            self.rushingAttackDistanceMult = r.readSingle()
            if dataSize == 120: return
            self.flags2 = r.readUInt32()

    class Csad:
        _struct = ('<21f', 84)
        def __init__(self, t):
            (self.dodgeFatigueModMult,
            self.dodgeFatigueModBase,
            self.encumbSpeedModBase,
            self.encumbSpeedModMult,
            self.dodgeWhileUnderAttackMult,
            self.dodgeNotUnderAttackMult,
            self.dodgeBackWhileUnderAttackMult,
            self.dodgeBackNotUnderAttackMult,
            self.dodgeForwardWhileAttackingMult,
            self.dodgeForwardNotAttackingMult,
            self.blockSkillModifierMult,
            self.blockSkillModifierBase,
            self.blockWhileUnderAttackMult,
            self.blockNotUnderAttackMult,
            self.attackSkillModifierMult,
            self.attackSkillModifierBase,
            self.attackWhileUnderAttackMult,
            self.attackNotUnderAttackMult,
            self.attackDuringBlockMult,
            self.powerAttFatigueModBase,
            self.powerAttFatigueModMult) = t

    CSTD: Cstd # Standard
    CSAD: Csad # Advanced
    def __init__(self): super().__init__()
    
    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.CSTD: z = self.CSTD = CSTYRecord.Cstd(r, dataSize)
            case FieldType.CSAD: z = self.CSAD = r.readS(CSTYRecord.Csad, dataSize)
            case _: z = Record._empty
        return z
# end::CSTY[]

# DIAL.Dialog Topic - 3450 - tag::DIAL[]
class DIALRecord(Record):
    _lastRecord: 'DIALRecord'
    class DIALType(Enum): RegularTopic = 0; Voice = 1; Greeting = 2; Persuasion = 3; Journal = 4
    FULL: str # Dialogue Name
    DATA: int # Dialogue Type
    QSTIs: list[RefX['QUSTRecord']] = None #! Quests (optional)
    INFOs: list['INFO3Record'] = [] # Info Records
    def __init__(self): super().__init__(); self.INFOs = listx()
    
    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = self.FULL = r.readFUString(dataSize); DIALRecord._lastRecord = self
            case FieldType.FULL: z = self.FULL = r.readFUString(dataSize)
            case FieldType.DATA: z = self.DATA = r.readByte()
            case FieldType.QSTI | FieldType.QSTR: z = _nca(self, 'QSTIs', listx()).addX(RefX[QUSTRecord](QUSTRecord, r, dataSize))
            case _: z = Record._empty
        return z
# end::DIAL[]

# DLBR.Dialog Branch - 0050 - tag::DIAL[]
class DLBRRecord(Record):
    CNAM: ByteColor4 # RGB color
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.CNAM: z = self.CNAM = r.readS(ByteColor4, dataSize)
            case _: z = Record._empty
        return z
# end::DLBR[]

# DLVW.Dialog View - 0050 - tag::DLVW[]
class DLVWRecord(Record):
    CNAM: ByteColor4 # RGB color
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.CNAM: z = self.CNAM = r.readS(ByteColor4, dataSize)
            case _: z = Record._empty
        return z
# end::DLVW[]

# DMGT.Damage Type - 0400 #F4 - tag::DMGT[]
class DMGTRecord(Record):
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case _: z = Record._empty
        return z
# end::DMGT[]

# DOOR.Door - 3450 - tag::DOOR[]
class DOORRecord(Record, IHaveMODL):
    FULL: str # Door name
    MODL: Modl # NIF model filename
    SCRI: RefX['SCPTRecord'] = None # Script (optional)
    SNAM: RefX['SOUNRecord'] = None # Open Sound
    ANAM: RefX['SOUNRecord'] = None # Close Sound
    # TES4
    BNAM: RefX['SOUNRecord'] = None # Loop Sound
    FNAM: int = 0 #! Flags
    TNAMs: list[RefX[Record]] = [] # Random teleport destination
    def __init__(self): super().__init__(); self.TNAMs = listx()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = self.FULL = r.readFUString(dataSize)
            case FieldType.FULL: z = self.FULL = r.readFUString(dataSize)
            case FieldType.FNAM if r.format != FormType.TES3: z = self.FNAM = r.readByte() #:matchif
            case FieldType.FNAM if r.format == FormType.TES3: z = self.FULL = r.readFUString(dataSize) #:matchif
            case FieldType.MODL: z = self.MODL = Modl(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODB(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODT(r, dataSize)
            case FieldType.SCRI: z = self.SCRI = RefX[SCPTRecord](SCPTRecord, r, dataSize)
            case FieldType.SNAM: z = self.SNAM = RefX[SOUNRecord](SOUNRecord, r, dataSize)
            case FieldType.ANAM: z = self.ANAM = RefX[SOUNRecord](SOUNRecord, r, dataSize)
            case FieldType.BNAM: z = self.ANAM = RefX[SOUNRecord](SOUNRecord, r, dataSize)
            case FieldType.TNAM: z = self.TNAMs.addX(RefX[Record](Record, r, dataSize))
            case _: z = Record._empty
        return z
# end::DOOR[]

# EFSH.Effect Shader - 0450 - tag::EFSH[]
class EFSHRecord(Record):
    class Data:
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
        def __init__(self, r: Reader, dataSize: int):
            if dataSize != 224 and dataSize != 96: self.flags = 0
            self.flags = r.readByte()
            r.skip(3) # Unused
            self.membraneShader_SourceBlendMode = r.readUInt32()
            self.membraneShader_BlendOperation = r.readUInt32()
            self.membraneShader_ZTestFunction = r.readUInt32()
            self.fillTextureEffect_Color = r.readS(ByteColor4, 4)
            self.fillTextureEffect_AlphaFadeInTime = r.readSingle()
            self.fillTextureEffect_FullAlphaTime = r.readSingle()
            self.fillTextureEffect_AlphaFadeOutTime = r.readSingle()
            self.fillTextureEffect_PresistentAlphaRatio = r.readSingle()
            self.fillTextureEffect_AlphaPulseAmplitude = r.readSingle()
            self.fillTextureEffect_AlphaPulseFrequency = r.readSingle()
            self.fillTextureEffect_TextureAnimationSpeed_U = r.readSingle()
            self.fillTextureEffect_TextureAnimationSpeed_V = r.readSingle()
            self.edgeEffect_FallOff = r.readSingle()
            self.edgeEffect_Color = r.readS(ByteColor4, 4)
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
            self.colorKey1_Color = r.readS(ByteColor4, 4)
            self.colorKey2_Color = r.readS(ByteColor4, 4)
            self.colorKey3_Color = r.readS(ByteColor4, 4)
            self.colorKey1_ColorAlpha = r.readSingle()
            self.colorKey2_ColorAlpha = r.readSingle()
            self.colorKey3_ColorAlpha = r.readSingle()
            self.colorKey1_ColorKeyTime = r.readSingle()
            self.colorKey2_ColorKeyTime = r.readSingle()
            self.colorKey3_ColorKeyTime = r.readSingle()

    ICON: str # Fill Texture
    ICO2: str # Particle Shader Texture
    DATA: Data # Data
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.ICON: z = self.ICON = r.readFUString(dataSize)
            case FieldType.ICO2: z = self.ICO2 = r.readFUString(dataSize)
            case FieldType.DATA: z = self.DATA = EFSHRecord.Data(r, dataSize)
            case _: z = Record._empty
        return z
# end::EFSH[]

# ENCH.Enchantment - 3450 - tag::ENCH[]
class ENCHRecord(Record):
    class Enit:
        class Type3(Enum): CastOnce = 0; CastStrikes = 1; CastWhenUsed = 2; ConstantEffect = 3
        class Type4(Enum): Scroll = 0; Staff = 1; Weapon = 2; Apparel = 3
        class Flag(Flag): AutoCalc = 0x01
        type: int
        enchantCost: int
        chargeAmount: int # Charge
        flags: int
        def __init__(self, r: Reader, dataSize: int):
            self.type = r.readInt32()
            if r.format == FormType.TES3:
                self.enchantCost = r.readInt32()
                self.chargeAmount = r.readInt32()
            else:
                self.chargeAmount = r.readInt32()
                self.enchantCost = r.readInt32()
            self.flags = r.readInt32()

    class Efit:
        class Type_(Enum): Self = 0; Touch = 1; Target = 2
        effectId: str
        type: Type_
        area: int
        duration: int
        magnitudeMin: int
        # TES3
        skillId: int # (-1 if NA)
        attributeId: int # (-1 if NA)
        magnitudeMax: int
        # TES4
        actorValue: ActorValue = ActorValue.None_
        def __init__(self, r: Reader, dataSize: int):
            if r.format == FormType.TES3:
                self.effectId = str(r.readUInt16())
                self.skillId = r.readSByte()
                self.attributeId = r.readSByte()
                self.type = self.Type_(r.readInt32())
                self.area = r.readInt32()
                self.duration = r.readInt32()
                self.magnitudeMin = r.readInt32()
                self.magnitudeMax = r.readInt32()
                return
            self.effectId = r.readFAString(4)
            self.magnitudeMin = r.readInt32()
            self.area = r.readInt32()
            self.duration = r.readInt32()
            self.type = self.Type_(r.readInt32())
            self.actorValue = ActorValue(r.readInt32())

    # TES4
    class Scit:
        name: str
        scriptFormId: int
        school: int # 0 = Alteration, 1 = Conjuration, 2 = Destruction, 3 = Illusion, 4 = Mysticism, 5 = Restoration
        visualEffect: str
        flags: int
        def __init__(self, r: Reader, dataSize: int):
            self.name = 'Script Effect'
            self.scriptFormId = r.readInt32()
            if dataSize == 4: return
            self.school = r.readInt32()
            self.visualEffect = r.readFAString(4)
            self.flags = r.readUInt32() if dataSize > 12 else 0
        def FULL(self, r: Reader, dataSize: int) -> object: z = self.name = r.readFUString(dataSize); return z

    FULL: str # Enchant name
    ENIT: Enit # Enchant Data
    EFITs: list[Efit] = [] # Effect Data
    # TES4
    SCITs: list[Scit] = [] # Script effect data
    def __init__(self): super().__init__(); self.EFITs = listx(); self.SCITs = listx()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = self.FULL = r.readFUString(dataSize)
            case FieldType.FULL if len(self.SCITs) == 0: z = self.FULL = r.readFUString(dataSize) #:matchif
            case FieldType.FULL if len(self.SCITs) != 0: z = self.SCITs.last().FULL(r, dataSize) #:matchif
            case FieldType.ENIT | FieldType.ENDT: z = self.ENIT = ENCHRecord.Enit(r, dataSize)
            case FieldType.EFID: z = r.skip(dataSize)
            case FieldType.EFIT | FieldType.ENAM: z = self.EFITs.addX(ENCHRecord.Efit(r, dataSize))
            case FieldType.SCIT: z = self.SCITs.addX(ENCHRecord.Scit(r, dataSize))
            case _: z = Record._empty
        return z
# end::ENCH[]

# EQUP.Equip Slots - 005S0 - tag::EQUP[]
class EQUPRecord(Record):
    FULL: str
    DATA: int
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.DATA: z = self.DATA = r.readByte()
            case _: z = Record._empty
        return z
# end::EQUP[]

# EYES.Eyes - 0450 - tag::EYES[]
class EYESRecord(Record):
    FULL: str
    ICON: str
    DATA: int # Playable
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.FULL: z = self.FULL = r.readFUString(dataSize)
            case FieldType.ICON: z = self.ICON = r.readFUString(dataSize)
            case FieldType.DATA: z = self.DATA = r.readByte()
            case _: z = Record._empty
        return z
# end::EYES[]

# FACT.Faction - 3450 - tag::FACT[]
class FACTRecord(Record):
    class Rnam:
        def __repr__(self): return f'{self.RNAM}:{self.MNAM}'
        def __init__(self, RNAM: int = None, MNAM: str = None):
            self.RNAM: int = RNAM # rank
            self.MNAM: str = MNAM # male
            self.FNAM: str = None # female
            self.INAM: str = None # insignia

    # TES3
    class RankModifier:
        _struct = ('5I', 20)
        def __init__(self, t):
            attributes = self.attributes = [None]*2
            (attributes[0], attributes[1],
            self.primarySkill,
            self.favoredSkill,
            self.factionReaction) = t

    class Fadt:
        class Flag(Flag): HiddenFromPlayer = 0x1
        _struct = ('2I20s20s20s20s20s20s20s20s20s20s7iI', 240)
        def __init__(self, t):
            attributes = self.attributes = [None]*2
            rankModifiers = self.rankModifiers = [None]*10
            skills = self.skills = [None]*7
            (attributes[0], attributes[1],
            rankModifiers[0], rankModifiers[1], rankModifiers[2], rankModifiers[3], rankModifiers[4], rankModifiers[5], rankModifiers[6], rankModifiers[7], rankModifiers[8], rankModifiers[9],
            skills[0], skills[1], skills[2], skills[3], skills[4], skills[5], skills[6],
            self.flags) = t
            self.rankModifiers = [FACTRecord.RankModifier(unpack(FACTRecord.RankModifier._struct[0], s)) for s in self.rankModifiers]

    class Anam:
        def __repr__(self): return f'{self.ANAM}:{self.INTV}'
        def __init__(self, ANAM: str = None, INTV: int = None):
            self.ANAM: str = ANAM # rank
            self.INTV: int = INTV # male

    # TES4
    class Xnam:
        def __repr__(self): return f'{self.formId}'
        def __init__(self, r: Reader = None, dataSize: int = None):
            if not r: self.formId = self.mod = self.combat = 0; return
            self.formId: int = r.readInt32()
            self.mod: int = r.readInt32()
            self.combat: int = r.readInt32() if r.tes4a or r.format > FormType.TES4 else 0

    # TES5
    class Crva:
        _struct = { 12: '<2B5H', 16: '<2B5Hf', 20: '<2B5Hf2H' }
        def __init__(self, t):
            match len(t):
                case 7:
                    (self.arrest,
                    self.attackOnSight,
                    self.murder,
                    self.assault,
                    self.trespass,
                    self.pickpocket,
                    self.unused) = t # usually 0, but not always, changing data has no effect in CK
                case 8:
                    (self.arrest,
                    self.attackOnSight,
                    self.murder,
                    self.assault,
                    self.trespass,
                    self.pickpocket,
                    self.unused, # usually 0, but not always, changing data has no effect in CK
                    self.stealMult) = t
                case 10:
                    (self.arrest,
                    self.attackOnSight,
                    self.murder,
                    self.assault,
                    self.trespass,
                    self.pickpocket,
                    self.unused, # usually 0, but not always, changing data has no effect in CK
                    self.stealMult,
                    self.escape,
                    self.werewolf) = t

    class Venv:
        _struct = ('2HI2BH', 12)
        def __init__(self, t):
            (self.startHour,
            self.endHour,
            self.radius,
            self.buysStolenItems, # Wording in CK is misleading
            self.notSellBuy, # Causes vendor to buy/sell everything except what's in the Vendor List
            self.unused) = t

    class Plvd:
        class SpecType(Enum):
            NearReference = 0 # REFR formID follows
            InCell = 1 # CELL formID follows
            NearPackageStartLocation = 2 # Not used in original files
            NearEditorLocation = 3
            LinkedReference = 6 # KWYD formID follows not used in original files
            NearSelf = 12
        _struct = ('3I', 12)
        def __init__(self, t):
            (self.type,
            self.id,
            self.unused) = t
            self.type = FACTRecord.Plvd.SpecType(self.type)
            self.id = Ref[Record](self.id)

    FULL: str # Faction name
    RNAMs: list[Rnam] = [] # Ranks
    FADT: Fadt = None # Faction data
    ANAMs: list[str] = [] # Factions
    # TES4
    XNAMs: list[Xnam] = [] # Interfaction Relations
    DATA: int = 0 #! Flags (byte, uint32)
    CNAM: int = 0 #!
    # TES5
    JAIL: Ref['REFRRecord'] # Prison Marker
    WAIT: Ref['REFRRecord'] # Follower Wait Marker
    STOL: Ref['REFRRecord'] # Evidence Chest
    CRGR: Ref['FLSTRecord'] # Crime Group
    JOUT: Ref['OTFTRecord'] # Jail outfit the player is given.
    CRVA: Crva # Crime Gold
    VEND: Ref['FLSTRecord'] # Vendor List
    VENC: Ref['REFRRecord'] # Vendor Chest
    VENV: Venv # Vendor
    PLVD: Plvd # Where to sell goods
    def __init__(self): super().__init__(); self.RNAMs = listx(); self.ANAMs = listx(); self.XNAMs = listx(); self.RNAMs = listx()
    
    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        if r.format == FormType.TES3:
            match type:
                case FieldType.NAME: z = self.EDID = r.readFUString(dataSize)
                case FieldType.FNAM: z = self.FULL = r.readFUString(dataSize)
                case FieldType.RNAM: z = self.RNAM = r.readFUString(dataSize)
                case FieldType.FADT: z = self.FADT = r.readS(FACTRecord.Fadt, dataSize)
                case FieldType.ANAM: z = self.ANAMs.addX(FACTRecord.Anam(ANAM = r.readFUString(dataSize)))
                case FieldType.INTV: z = self.ANAMs.last().INTV = r.readINTV(dataSize)
                case _: z = Record._empty
            return z
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.FULL: z = self.FULL = r.readFUString(dataSize)
            case FieldType.XNAM: z = self.XNAMs.addX(FACTRecord.Xnam(r, dataSize))
            case FieldType.DATA: z = self.DATA = r.readINTV(dataSize)
            case FieldType.CNAM: z = self.CNAM = r.readUInt32() #TES4
            case FieldType.JAIL: z = self.JAIL = Ref[REFRRecord](REFRRecord, r, dataSize) #TES5
            case FieldType.WAIT: z = self.WAIT = Ref[REFRRecord](REFRRecord, r, dataSize) #TES5
            case FieldType.STOL: z = self.STOL = Ref[REFRRecord](REFRRecord, r, dataSize) #TES5
            case FieldType.PLCN: z = self.PLCN = Ref[REFRRecord](REFRRecord, r, dataSize) #TES5
            case FieldType.CRGR: z = self.CRGR = Ref[FLSTRecord](FLSTRecord, r, dataSize) #TES5
            case FieldType.JOUT: z = self.JOUT = Ref[OTFTRecord](OTFTRecord, r, dataSize) #TES5
            case FieldType.CRVA: z = self.CRVA = r.readS(FACTRecord.Crva, dataSize) #TES5
            # ??
            case FieldType.RNAM: z = self.RNAMs.addX(FACTRecord.Rnam(RNAM = r.readInt32()))
            case FieldType.MNAM: z = self.RNAMs.last().MNAM = r.readFUString(dataSize)
            case FieldType.FNAM: z = self.RNAMs.last().FNAM = r.readFUString(dataSize)
            case FieldType.INAM: z = self.RNAMs.last().INAM = r.readFUString(dataSize) #TES4
            case FieldType.VEND: z = self.VEND = Ref[FLSTRecord](FLSTRecord, r, dataSize) #TES5
            case FieldType.VENC: z = self.VENC = Ref[REFRRecord](REFRRecord, r, dataSize) #TES5
            case FieldType.VENV: z = self.VENV = r.readS(FACTRecord.Venv, dataSize) #TES5
            case FieldType.PLVD: z = self.PLVD = r.readS(FACTRecord.Plvd, dataSize) #TES5
            case FieldType.CITC: z = r.skip(dataSize) #TES5 TODO
            case FieldType.CTDA: z = r.skip(dataSize) #TES5 TODO
            case FieldType.CIS2: z = r.skip(dataSize) #TES5 TODO
            case _: z = Record._empty
        return z
# end::FACT[]

# FLOR.Flora - 0450 - tag::FLOR[]
class FLORRecord(Record, IHaveMODL):
    MODL: Modl # Model
    FULL: str # Plant Name
    SCRI: RefX['SCPTRecord'] # Script (optional)
    PFIG: RefX['INGRRecord'] # The ingredient the plant produces (optional)
    PFPC: bytes # Spring, Summer, Fall, Winter Ingredient Production (byte)
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.MODL: z = self.MODL = Modl(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODB(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODT(r, dataSize)
            case FieldType.FULL: z = self.FULL = r.readFUString(dataSize)
            case FieldType.SCRI: z = self.SCRI = RefX[SCPTRecord](SCPTRecord, r, dataSize)
            case FieldType.PFIG: z = self.PFIG = RefX[INGRRecord](INGRRecord, r, dataSize)
            case FieldType.PFPC: z = self.PFPC = r.readBytes(dataSize)
            case _: z = Record._empty
        return z
# end::FLOR[]

# FLST.Form List (non-leveled list) - 00500 - tag::FLST[]
class FLSTRecord(Record):
    LNAM: Ref[Record] # Object
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.LNAM: z = self.LNAM = Ref[Record](Record, r, dataSize)
            case _: z = Record._empty
        return z
# end::FLST[]

# FURN.Furniture - 0450 - tag::FURN[]
class FURNRecord(Record, IHaveMODL):
    MODL: Modl # Model
    FULL: str # Furniture Name
    SCRI: RefX['SCPTRecord'] # Script (optional)
    MNAM: int # Active marker flags, required. A bit field with a bit value of 1 indicating that the matching marker position in the NIF file is active.
    def __init__(self): super().__init__()
    
    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.MODL: z = self.MODL = Modl(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODB(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODT(r, dataSize)
            case FieldType.FULL: z = self.FULL = r.readFUString(dataSize)
            case FieldType.SCRI: z = self.SCRI = RefX[SCPTRecord](SCPTRecord, r, dataSize)
            case FieldType.MNAM: z = self.MNAM = r.readInt32()
            case _: z = Record._empty
        return z
# end::FURN[]

# GLOB.Global - 3450 - tag::GLOB[]
class GLOBRecord(Record):
    FNAM: str = None # Type of global (s, l, f)
    FLTV: float = None # Float data
    def __init__(self): super().__init__()
    
    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readFUString(dataSize)
            case FieldType.FNAM: z = self.FNAM = chr(r.readByte())
            case FieldType.FLTV: z = self.FLTV = r.readSingle()
            case _: z = Record._empty
        return z
# end::GLOB[]

# GMST.Game Setting - 3450 - tag::GMST[]
class GMSTRecord(Record):
    DATA: Datv # Data
    def __init__(self): super().__init__()
    
    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        if r.format == FormType.TES3:
            match type:
                case FieldType.NAME: z = self.EDID = r.readFUString(dataSize)
                case FieldType.STRV: z = self.DATA = r.readDATV(dataSize, 's')
                case FieldType.INTV: z = self.DATA = r.readDATV(dataSize, 'i')
                case FieldType.FLTV: z = self.DATA = r.readDATV(dataSize, 'f')
                case _: z = Record._empty
            return z
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.DATA: z = self.DATA = r.readDATV(dataSize, self.EDID[0])
            case _: z = Record._empty
        return z
# end::GMST[]

# GRAS.Grass - 0450 - tag::GRAS[]
class GRASRecord(Record):
    class Data:
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
        def __init__(self, r: Reader, dataSize: int):
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

    MODL: Modl 
    DATA: Data 
    def __init__(self): super().__init__()
    
    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.MODL: z = self.MODL = Modl(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODB(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODT(r, dataSize)
            case FieldType.DATA: z = self.DATA = GRASRecord.Data(r, dataSize)
            case _: z = Record._empty
        return z
# end::GRAS[]

# HAIR.Hair - 0400 - tag::HAIR[]
class HAIRRecord(Record, IHaveMODL):
    FULL: str
    MODL: Modl
    DATA: int # Playable, Not Male, Not Female, Fixed
    def __init__(self): super().__init__()
    
    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.FULL: z = self.FULL = r.readFUString(dataSize)
            case FieldType.MODL: z = self.MODL = Modl(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODB(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODT(r, dataSize)
            case FieldType.ICON: z = self.MODL.ICON(r, dataSize)
            case FieldType.DATA: z = self.DATA = r.readByte()
            case _: z = Record._empty
        return z
# end::HAIR[]

# HDPT.Head Part - 00500 - tag::HDPT[]
class HDPTRecord(Record):
    class Nam0:
        def __init__(self, NAM0: int):
            self.NAM0: int = NAM0 # Option type
            self.NAM1: str = None # .tri file    

    FULL: str # Name
    MODL: Modl # Model
    DATA: int # Flags
    PNAM: int # Type
    HNAMs: list[Ref['HDPTRecord']] = [] # Additional part
    NAM0s: list[int] = [] # Option type
    NAM1: str # .tri file
    TNAM: Ref['TXSTRecord'] = None #! Base texture
    RNAM: Ref[FLSTRecord] = None #! Resource list
    CNAM: Ref[Record] = None #! Color (seen in Dawnguard.esm)
    def __init__(self): super().__init__(); self.HNAMs = listx(); self.NAM0s = listx()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.FULL: z = self.FULL = r.readFUString(dataSize)
            case FieldType.MODL: z = self.MODL = Modl(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODT(r, dataSize)
            case FieldType.MODS: z = self.MODL.MODS(r, dataSize)
            case FieldType.DATA: z = self.DATA = r.readByte()
            case FieldType.PNAM: z = self.PNAM = r.readUInt32()
            case FieldType.HNAM: z = self.HNAMs.addX(Ref[HDPTRecord](HDPTRecord, r, dataSize))
            case FieldType.NAM0: z = self.NAM0s.addX(Nam0(NAM0 = r.readUInt32()))
            case FieldType.NAM1: z = self.NAM0s.last().NAM1 = r.readFUString(dataSize)
            case FieldType.TNAM: z = self.TNAM = Ref[TXSTRecord](TXSTRecord, r, dataSize)
            case FieldType.RNAM: z = self.RNAM = Ref[FLSTRecord](FLSTRecord, r, dataSize)
            case FieldType.CNAM: z = self.CNAM = Ref[Record](TXSTRecord, r, dataSize)
            case _: z = Record._empty
        return z
# end::HDPT[]

# IDLE.Idle Animations - 0450 - tag::IDLE[]
class IDLERecord(Record, IHaveMODL):
    MODL: Modl
    CTDAs: list['SCPTRecord.CTDAField'] = [] # Conditions
    ANAM: int
    DATAs: list[RefX['IDLERecord']]
    def __init__(self): super().__init__(); self.CTDAs = listx()
    
    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.MODL: z = self.MODL = Modl(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODB(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODT(r, dataSize)
            case FieldType.CTDA | FieldType.CTDT: z = self.CTDAs.addX(SCPTRecord.Ctda(r, dataSize))
            case FieldType.ANAM: z = self.ANAM = r.readByte()
            case FieldType.DATA: z = self.DATAs = r.readFArray(lambda z: RefX[IDLERecord](IDLERecord, r, 4), dataSize >> 2)
            case _: z = Record._empty
        return z
# end::IDLE[]

# INFO.Dialog Topic Info - 3450 - tag::INFO[]
class INFORecord(Record):
    def __init__(self): super().__init__()
# end::INFO[]

# tag::INFO3[]
class INFO3Record(INFORecord):
    class Data:
        _struct = ('<2i4B', 12)
        def __init__(self, t):
            (self.unknown1,
            self.disposition,
            self.rank, # (0-10)
            self.gender, # 0xFF = None, 0x00 = Male, 0x01 = Female
            self.pcRank, # (0-10)
            self.unknown2) = t

    PNAM: RefX['INFO3Record'] # Previous info ID
    NNAM: str = None # Next info ID (form a linked list of INFOs for the DIAL). First INFO has an empty PNAM, last has an empty NNAM.
    DATA: 'DATA3Field' # Info data
    ONAM: str = None # Actor
    RNAM: str = None # Race
    CNAM: str = None # Class
    FNAM: str = None # Faction 
    ANAM: str = None # Cell
    DNAM: str = None # PC Faction
    NAME: str = str() #! The info response string (512 max)
    SNAM: str = None # Sound
    QSTN: int = None # Journal Name
    QSTF: int = None # Journal Finished
    QSTR: int = None # Journal Restart
    SCVRs: list['SCPTRecord.SCVRGroup'] = [] # String for the function/variable choice
    BNAM: str = None # Result text (not compiled)
    def __init__(self): super().__init__(); self.SCVRs = listx()
    
    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.INAM: z = DIALRecord._lastRecord.INFOs.addX(self) if DIALRecord._lastRecord else None; self.EDID = r.readFUString(dataSize)
            case FieldType.PNAM: z = self.PNAM = RefX[INFO3Record](INFO3Record, r, dataSize)
            case FieldType.NNAM: z = self.NNAM = r.readFUString(dataSize)
            case FieldType.DATA: z = self.DATA = r.readS(INFO3Record.Data, dataSize)
            case FieldType.ONAM: z = self.ONAM = r.readFUString(dataSize)
            case FieldType.RNAM: z = self.RNAM = r.readFUString(dataSize)
            case FieldType.CNAM: z = self.CNAM = r.readFUString(dataSize)
            case FieldType.FNAM: z = self.FNAM = r.readFUString(dataSize)
            case FieldType.ANAM: z = self.ANAM = r.readFUString(dataSize)
            case FieldType.DNAM: z = self.DNAM = r.readFUString(dataSize)
            case FieldType.NAME: z = self.NAME = r.readFUString(dataSize) #; self.TES3.NAME.value = self.TES3.NAME.value.replace('\ufffd', '\1')
            case FieldType.SNAM: z = self.SNAM = r.readFUString(dataSize)
            case FieldType.QSTN: z = self.QSTN = r.readByte()
            case FieldType.QSTF: z = self.QSTF = r.readByte()
            case FieldType.QSTR: z = self.QSTR = r.readByte()
            case FieldType.SCVR: z = self.SCVRs.addX(SCPTRecord.Scvr(SCVR = SCPTRecord.Ctda(r, dataSize)))
            case FieldType.INTV: z = self.SCVRs.last().INTV = r.readINTV(dataSize)
            case FieldType.FLTV: z = self.SCVRs.last().FLTV = r.readSingle()
            case FieldType.BNAM: z = self.BNAM = r.readFUString(dataSize)
            case _: z = Record._empty
        return z
# end::INFO3[]

# tag::INFO4[]
class INFO4Record(INFORecord):
    class Data:
        def __init__(self, r: Reader, dataSize: int):
            self.type: int = r.readByte()
            self.nextSpeaker: int = r.readByte()
            self.flags: int = r.readByte() if dataSize == 3 else 0

    class Trdt:
        emotionType: int
        emotionValue: int
        responseNumber: int
        responseText: str
        actorNotes: str
        def __init__(self, r: Reader, dataSize: int):
            self.emotionType = r.readUInt32()
            self.emotionValue = r.readInt32()
            r.skip(4) # Unused
            self.responseNumber = r.readByte()
            r.skip(3) # Unused
        def NAM1(self, r: Reader, dataSize: int) -> object: z = self.responseText = r.readFUString(dataSize); return z
        def NAM2(self, r: Reader, dataSize: int) -> object: z = self.actorNotes = r.readFUString(dataSize); return z

    DATA: Data # Info data
    QSTI: RefX['QUSTRecord'] # Quest
    TPIC: RefX[DIALRecord] # Topic
    NAMEs: list[RefX[DIALRecord]] = [] # Topics
    TRDTs: list[Trdt] = [] # Responses
    CTDAs: list['SCPTRecord.Ctda'] = [] # Conditions
    TCLTs: list[RefX[DIALRecord]] = [] # Choices
    TCLFs: list[RefX[DIALRecord]] = [] # Link From Topics
    SCHR: 'SCPTRecord.Schr' # Script Data
    SCDA: bytes # Compiled Script
    SCTX: str # Script Source
    SCROs: list[RefX[Record]] = [] # Global variable reference
    PNAM: Ref['INFO4Record'] = None #! Previous INFO ID
    def __init__(self): super().__init__(); self.NAMEs = listx(); self.TRDTs = listx(); self.CTDAs = listx(); self.TCLTs = listx(); self.TCLFs = listx(); self.SCROs = listx()
    
    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.DATA: z = self.DATA = INFO4Record.Data(r, dataSize)
            case FieldType.QSTI: z = self.QSTI = RefX[QUSTRecord](QUSTRecord, r, dataSize)
            case FieldType.TPIC: z = self.TPIC = RefX[DIALRecord](DIALRecord, r, dataSize)
            case FieldType.NAME: z = self.NAMEs.addX(RefX[DIALRecord](DIALRecord, r, dataSize))
            case FieldType.TRDT: z = self.TRDTs.addX(INFO4Record.Trdt(r, dataSize))
            case FieldType.NAM1: z = self.TRDTs.last().NAM1(r, dataSize)
            case FieldType.NAM2: z = self.TRDTs.last().NAM2(r, dataSize)
            case FieldType.CTDA | FieldType.CTDT: z = self.CTDAs.addX(SCPTRecord.Ctda(r, dataSize))
            case FieldType.TCLT: z = self.TCLTs.addX(RefX[DIALRecord](DIALRecord, r, dataSize))
            case FieldType.TCLF: z = self.TCLFs.addX(RefX[DIALRecord](DIALRecord, r, dataSize))
            case FieldType.SCHR | FieldType.SCHD: z = self.SCHR = SCPTRecord.Schr(r, dataSize)
            case FieldType.SCDA: z = self.SCDA = r.readBytes(dataSize)
            case FieldType.SCTX: z = self.SCTX = r.readFUString(dataSize)
            case FieldType.SCRO: z = self.SCROs.addX(RefX[Record](Record, r, dataSize))
            case FieldType.PNAM: z = self.PNAM = Ref[INFO4Record](INFO4Record, r, dataSize)
            case _: z = Record._empty
        return z
# end::INFO4[]

# INGR.Ingredient - 3450 - tag::INGR[]
class INGRRecord(Record, IHaveMODL):
    # TES3
    class Irdt:
        def __init__(self, r: Reader, dataSize: int):
            self.weight: float = r.readSingle()
            self.value: int = r.readInt32()
            self.effectId: list[int] = [r.readInt32(), r.readInt32(), r.readInt32(), r.readInt32()] # 0 or -1 means no effect
            self.skillId: list[int] = [r.readInt32(), r.readInt32(), r.readInt32(), r.readInt32()] # only for Skill related effects, 0 or -1 otherwise
            self.attributeId: list[int] = [r.readInt32(), r.readInt32(), r.readInt32(), r.readInt32()] # only for Attribute related effects, 0 or -1 otherwise

    # TES4
    class Data:
        def __init__(self, r: Reader, dataSize: int):
            self.weight: float = r.readSingle()
            self.value: int = 0
            self.flags: int = 0
        def ENIT(self, r: Reader, dataSize: int) -> object: z = self.value = r.readInt32(); self.flags = r.readUInt32(); return z

    MODL: Modl # Model Name
    FULL: str # Item Name
    IRDT: Irdt # Ingrediant Data # TES3
    DATA: Data # Ingrediant Data # TES4
    SCRI: RefX['SCPTRecord'] # Script Name
    # TES4
    EFITs: list[ENCHRecord.Efit] = [] # Effect Data
    SCITs: list[ENCHRecord.Scit] = [] # Script effect data
    def __init__(self): super().__init__(); self.EFITs = listx(); self.SCITs = listx()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readFUString(dataSize)
            case FieldType.MODL: z = self.MODL = Modl(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODB(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODT(r, dataSize)
            case FieldType.ICON | FieldType.ITEX: z = self.MODL.ICON(r, dataSize)
            case FieldType.FULL if len(self.SCITs) == 0: z = self.FULL = r.readFUString(dataSize) #:matchif
            case FieldType.FULL if len(self.SCITs) != 0: z = self.SCITs.last().FULL(r, dataSize) #:matchif
            case FieldType.FNAM: z = self.FULL = r.readFUString(dataSize)
            case FieldType.DATA: z = self.DATA = INGRRecord.Data(r, dataSize)
            case FieldType.IRDT: z = self.IRDT = INGRRecord.Irdt(r, dataSize)
            case FieldType.SCRI: z = self.SCRI = RefX[SCPTRecord](SCPTRecord, r, dataSize)
            #
            case FieldType.ENIT: z = self.DATA.ENIT(r, dataSize)
            case FieldType.EFID: z = r.skip(dataSize)
            case FieldType.EFIT: z = self.EFITs.addX(ENCHRecord.Efit(r, dataSize))
            case FieldType.SCIT: z = self.SCITs.addX(ENCHRecord.Scit(r, dataSize))
            case _: z = Record._empty
        return z
# end::INGR[]

# KEYM.Key - 0450 - tag::KEYM[]
class KEYMRecord(Record, IHaveMODL):
    class Data:
        _struct = ('<if', 8)
        def __init__(self, t):
            (self.value,
            self.weight) = t

    MODL: Modl # Model
    FULL: str # Item Name
    SCRI: RefX['SCPTRecord'] # Script (optional)
    DATA: Data # Type of soul contained in the gem
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.MODL: z = self.MODL = Modl(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODB(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODT(r, dataSize)
            case FieldType.ICON: z = self.MODL.ICON(r, dataSize)
            case FieldType.FULL: z = self.FULL = r.readFUString(dataSize)
            case FieldType.SCRI: z = self.SCRI = RefX[SCPTRecord](SCPTRecord, r, dataSize)
            case FieldType.DATA: z = self.DATA = r.readS(KEYMRecord.Data, dataSize)
            case _: z = Record._empty
        return z
# end::KEYM[]

# KYWD.Keyword - 00500 - tag::KYWD[]
class KYWDRecord(Record):
    CNAM: ByteColor4 # Used to identify keywords in the editor.
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.CNAM: z = self.CNAM = r.readS(ByteColor4, dataSize)
            case _: z = Record._empty
        return z
# end::KYWD[]

# LAND.Land - 3450 - tag::LAND[]
class LANDRecord(Record):
    class Vnml:
        def __init__(self, r: Reader, dataSize: int):
            self.vertexs: list[Byte3] = r.readPArray(Byte3, '3B', dataSize // 3) # XYZ 8 bit floats

    class Vhgt:
        referenceHeight: float # A height offset for the entire cell. Decreasing this value will shift the entire cell land down.
        heightData: list[int] # HeightData
        def __init__(self, r: Reader, dataSize: int):
            self.referenceHeight = r.readSingle()
            count = dataSize - 4 - 3
            self.heightData = r.readPArray(None, 'b', count)
            r.skip(3) # Unused

    class Vclr:
        def __init__(self, r: Reader, dataSize: int):
            self.colors: list[ByteColor3] = r.readSArray(ByteColor3, dataSize // 24) # 24-bit RGB

    class Vtex:
        textureIndicesT3: list[int]
        textureIndicesT4: list[int]
        def __init__(self, r: Reader, dataSize: int):
            if r.format == FormType.TES3:
                self.textureIndicesT3 = r.readPArray(None, 'H', dataSize >> 1)
                self.textureIndicesT4 = None
                return
            self.textureIndicesT3 = None
            self.textureIndicesT4 = r.readPArray(None, 'I', dataSize >> 2)

    # TES3
    class Cord:
        def __repr__(self): return f'{self.cellX},{self.cellY}'
        _struct = ('<2i', 8)
        def __init__(self, t):
            (self.cellX,
            self.cellY) = t

    class Wnam:
        # Low-LOD heightmap (signed chars)
        def __init__(self, r: Reader, dataSize: int):
            r.skip(dataSize)
            #var heightCount = dataSize;
            #for (var i = 0; i < heightCount; i++) { var height = r.readByte(); }

    # TES4
    class Btxt:
        _struct = ('<I2Bh', 8)
        def __init__(self, t):
            (self.texture,
            self.quadrant,
            self.pad01,
            self.layer) = t

    class Vtxt:
        _struct = ('<2Hf', 8)
        def __init__(self, t):
            (self.position,
            self.pad01,
            self.opacity) = t

    class Atxt:
        def __init__(self, ATXT: 'Btxt'):
            self.ATXT: 'Btxt' = ATXT
            self.VTXTs: list['Vtxt'] = None

    def __repr__(self): return f'LAND: {self.INTV}'
    DATA: int # Unknown (default of 0x09) Changing this value makes the land 'disappear' in the editor.
    # A RGB color map 65x65 pixels in size representing the land normal vectors.
    # The signed value of the 'color' represents the vector's component. Blue
    # is vertical(Z), Red the X direction and Green the Y direction.Note that
    # the y-direction of the data is from the bottom up.
    VNML: Vnml
    VHGT: Vhgt # Height data
    VCLR: Vnml = None # Vertex color array, looks like another RBG image 65x65 pixels in size. (Optional)
    VTEX: Vtex = None # A 16x16 array of short texture indices. (Optional)
    # TES3
    INTV: Cord # The cell coordinates of the cell
    WNAM: Wnam # Unknown byte data.
    # TES4
    BTXTs: list[Btxt] = [None]*4 # Base Layer
    ATXTs: list[Atxt] # Alpha Layer
    _lastATXT: Atxt
    # Grid
    gridId: Int3 # => Int3(INTV.CellX, INTV.CellY, 0);
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.DATA: z = self.DATA = r.readInt32()
            case FieldType.VNML: z = self.VNML = LANDRecord.Vnml(r, dataSize)
            case FieldType.VHGT: z = self.VHGT = LANDRecord.Vhgt(r, dataSize)
            case FieldType.VCLR: z = self.VCLR = LANDRecord.Vnml(r, dataSize)
            case FieldType.VTEX: z = self.VTEX = LANDRecord.Vtex(r, dataSize)
            # TES3
            case FieldType.INTV: z = self.INTV = r.readS(LANDRecord.Cord, dataSize)
            case FieldType.WNAM: z = self.WNAM = LANDRecord.Wnam(r, dataSize)
            # TES4
            case FieldType.BTXT: z = self.then(r.readS(LANDRecord.Btxt, dataSize), lambda v: self.BTXTs.__setitem__(v.quadrant, v))
            case FieldType.ATXT: z = _nca(self, 'ATXTs', listx([None]*4)); self.then(r.readS(LANDRecord.Btxt, dataSize), lambda v: ((z := LANDRecord.Atxt(ATXT = v), setattr(self, '_lastATXT', z))))
            case FieldType.VTXT: z = self._lastATXT.VTXTs = r.readSArray(LANDRecord.Vtxt, dataSize >> 3)
            case _: z = Record._empty
        return z
# end::LAND[]

# LCRT.Location Reference Type - 00500 - tag::LCRT[]
class LCRTRecord(Record):
    CNAM: ByteColor4 # RGB Hex color code, last byte always 0x00
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.CNAM: z = self.CNAM = r.readS(ByteColor4, dataSize)
            case _: z = Record._empty
        return z
# end::LCRT[]

# LEVC.Leveled Creature - 3000 - tag::LEVC[]
class LEVCRecord(Record):
    DATA: int # List data - 1 = Calc from all levels <= PC level
    NNAM: int # Chance None?
    INDX: int # Number of items in list
    CNAMs: list[str] = [] # ID string of list item
    INTVs: list[int] = [] # PC level for previous CNAM
    # The CNAM/INTV can occur many times in pairs
    def __init__(self): super().__init__(); self.CNAMs = listx(); self.INTVs = listx()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        if r.format == FormType.TES3:
            match type:
                case FieldType.NAME: z = self.EDID = r.readFUString(dataSize)
                case FieldType.DATA: z = self.DATA = r.readInt32()
                case FieldType.NNAM: z = self.NNAM = r.readByte()
                case FieldType.INDX: z = self.INDX = r.readInt32()
                case FieldType.CNAM: z = self.CNAMs.addX(r.readFUString(dataSize))
                case FieldType.INTV: z = self.INTVs.addX(r.readInt16())
                case _: z = Record._empty
            return z
        return None
# end::LEVC[]

# LEVI.Leveled item - 3000 - tag::LEVI[]
class LEVIRecord(Record):
    DATA: int # List data - 1 = Calc from all levels <= PC level, 2 = Calc for each item
    NNAM: int # Chance None?
    INDX: int # Number of items in list
    INAMs: list[str] = [] # ID string of list item
    INTVs: list[int] = [] # PC level for previous INAM
    # The CNAM/INTV can occur many times in pairs
    def __init__(self): super().__init__(); self.INAMs = listx(); self.INTVs = listx()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        if r.format == FormType.TES3:
            match type:
                case FieldType.NAME: z = self.EDID = r.readFUString(dataSize)
                case FieldType.DATA: z = self.DATA = r.readInt32()
                case FieldType.NNAM: z = self.NNAM = r.readByte()
                case FieldType.INDX: z = self.INDX = r.readInt32()
                case FieldType.INAM: z = self.INAMs.addX(r.readFUString(dataSize))
                case FieldType.INTV: z = self.INTVs.addX(r.readInt16())
                case _: z = Record._empty
            return z
        return None
# end::LEVI[]

# LIGH.Light - 3450 - tag::LIGH[]
class LIGHRecord(Record, IHaveMODL):
    class Data:
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
        def __init__(self, r: Reader, dataSize: int):
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

    MODL: Modl # Model
    FULL: str = None # Item Name (optional)
    DATA: Data # Light Data
    SCPT: str = None # Script Name (optional)??
    SCRI: RefX['SCPTRecord'] = None # Script FormId (optional)
    FNAM: float # Fade Value
    SNAM: RefX['SOUNRecord'] # Sound FormId (optional)
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readFUString(dataSize)
            case FieldType.FULL: z = self.FULL = r.readFUString(dataSize)
            case FieldType.FNAM if r.format != FormType.TES3: z = self.FNAM = r.readSingle() #:matchif
            case FieldType.FNAM if r.format == FormType.TES3: z = self.FULL = r.readFUString(dataSize) #:matchif
            case FieldType.DATA | FieldType.LHDT: z = self.DATA = LIGHRecord.Data(r, dataSize)
            case FieldType.SCPT: z = self.SCPT = r.readFUString(dataSize)
            case FieldType.SCRI: z = self.SCRI = RefX[SCPTRecord](SCPTRecord, r, dataSize)
            case FieldType.MODL: z = self.MODL = Modl(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODB(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODT(r, dataSize)
            case FieldType.ICON | FieldType.ITEX: z = self.MODL.ICON(r, dataSize)
            case FieldType.SNAM: z = self.SNAM = RefX[SOUNRecord](SOUNRecord, r, dataSize)
            case _: z = Record._empty
        return z
# end::LIGH[]

# LOCK.Lock - 3450 - tag::LOCK[]
class LOCKRecord(Record, IHaveMODL):
    class Lkdt:
        _struct = ('<fifi', 16)
        def __init__(self, t):
            (self.weight,
            self.value,
            self.quality,
            self.uses) = t

    MODL: Modl # Model Name
    FNAM: str # Item Name
    LKDT: Lkdt # Lock Data
    SCRI: RefX['SCPTRecord'] # Script Name
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        if r.format == FormType.TES3:
            match type:
                case FieldType.NAME: z = self.EDID = r.readFUString(dataSize)
                case FieldType.MODL: z = self.MODL = Modl(r, dataSize)
                case FieldType.ITEX: z = self.MODL.ICON(r, dataSize)
                case FieldType.FNAM: z = self.FNAM = r.readFUString(dataSize)
                case FieldType.LKDT: z = self.LKDT = r.readS(LOCKRecord.Lkdt, dataSize)
                case FieldType.SCRI: z = self.SCRI = RefX[SCPTRecord](SCPTRecord, r, dataSize)
                case _: z = Record._empty
            return z
        return None
# end::LOCK[]

# LSCR.Load Screen - 0450 - tag::LSCR[]
class LSCRRecord(Record):
    class Lnam:
        def __init__(self, r: Reader, dataSize: int):
            self.direct: Ref[Record] = Ref[Record](Record, r.readUInt32())
            self.indirectWorld: Ref[WRLDRecord] = Ref[WRLDRecord](WRLDRecord, r.readUInt32())
            self.indirectGridX: int = r.readInt16()
            self.indirectGridY: int = r.readInt16()

    ICON: str # Icon
    DESC: str # Description
    LNAMs: list[Lnam] # LoadForm
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.ICON: z = self.ICON = r.readFUString(dataSize)
            case FieldType.DESC: z = self.DESC = r.readFUString(dataSize)
            case FieldType.LNAM: z = _nca(self, 'LNAMs', listx()).addX(LSCRRecord.Lnam(r, dataSize))
            case _: z = Record._empty
        return z
# end::LSCR[]

# LTEX.Land Texture - 3450 - tag::LTEX[]
class LTEXRecord(Record):
    class Hnam:
        _struct = ('<3B', 3)
        def __init__(self, t):
            (self.materialType,
            self.friction,
            self.restitution) = t

    ICON: str # Texture
    # TES3
    INTV: int
    # TES4
    HNAM: Hnam # Havok data
    SNAM: int # Texture specular exponent
    GNAMs: list[RefX[GRASRecord]] = [] # Potential grass
    def __init__(self): super().__init__(); self.GNAMs = listx()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readFUString(dataSize),
            case FieldType.INTV: z = self.INTV = r.readINTV(dataSize),
            case FieldType.ICON | FieldType.DATA: z = self.ICON = r.readFUString(dataSize),
            # TES4
            case FieldType.HNAM: z = self.HNAM = r.readS(LTEXRecord.Hnam, dataSize),
            case FieldType.SNAM: z = self.SNAM = r.readByte(),
            case FieldType.GNAM: z = self.GNAMs.addX(RefX[GRASRecord](GRASRecord, r, dataSize)),
            case _: z = Record._empty
        return z
# end::LTEX[]

# LVLC.Leveled Creature - 0400 - tag::LVLC[]
class LVLCRecord(Record):
    LVLD: int # Chance
    LVLF: int # Flags - 0x01 = Calculate from all levels <= player's level, 0x02 = Calculate for each item in count
    SCRI: RefX['SCPTRecord'] # Script (optional)
    TNAM: RefX[CREA4Record] # Creature Template (optional)
    LVLOs: list['LVLIRecord.LVLOField'] = []
    def __init__(self): super().__init__(); self.LVLOs = listx()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.LVLD: z = self.LVLD = r.readByte()
            case FieldType.LVLF: z = self.LVLF = r.readByte()
            case FieldType.SCRI: z = self.SCRI = RefX[SCPTRecord](SCPTRecord, r, dataSize)
            case FieldType.TNAM: z = self.TNAM = RefX[CREA4Record](CREA4Record, r, dataSize)
            case FieldType.LVLO: z = self.LVLOs.addX(LVLIRecord.Lvlo(r, dataSize))
            case _: z = Record._empty
        return z
# end::LVLC[]

# LVLI.Leveled Item - 0400 - tag::LVLI[]
class LVLIRecord(Record):
    class Lvlo:
        level: int
        itemFormId: RefX[Record]
        count: int
        def __init__(self, r: Reader, dataSize: int):
            self.level = r.readInt16()
            r.skip(2) # Unused
            self.itemFormId = RefX[Record](Record, r.readUInt32())
            if dataSize == 12:
                self.count = r.readInt16()
                r.skip(2) # Unused
            else: self.count = 0

    LVLD: int # Chance
    LVLF: int # Flags - 0x01 = Calculate from all levels <= player's level, 0x02 = Calculate for each item in count
    DATA: int = None # Data (optional)
    LVLOs: list[Lvlo] = []
    def __init__(self): super().__init__(); self.LVLOs = listx()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize),
            case FieldType.LVLD: z = self.LVLD = r.readByte(),
            case FieldType.LVLF: z = self.LVLF = r.readByte(),
            case FieldType.DATA: z = self.DATA = r.readByte(),
            case FieldType.LVLO: z = self.LVLOs.addX(self.Lvlo(r, dataSize)),
            case _: z = Record._empty
        return z
# end::LVLI[]

# LVSP.Leveled Spell - 0400 - tag::LVSP[]
class LVSPRecord(Record):
    LVLD: int # Chance
    LVLF: int # Flags
    LVLOs: list[LVLIRecord.Lvlo] = [] # Number of items in list
    def __init__(self): super().__init__(); self.LVLOs = listx()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize),
            case FieldType.LVLD: z = self.LVLD = r.readByte(),
            case FieldType.LVLF: z = self.LVLF = r.readByte(),
            case FieldType.LVLO: z = self.LVLOs.addX(LVLIRecord.Lvlo(r, dataSize)),
            case _: z = Record._empty
        return z
# end::LVSP[]

# MGEF.Magic Effect - 3400 - tag::MGEF[]
class MGEFRecord(Record):
    # TES3
    class Medt:
        def __init__(self, r: Reader, dataSize: int):
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

    class Data:
        flags: int
        baseCost: float
        assocItem: int
        magicSchool: int
        resistValue: int
        counterEffectCount: int # Must be updated automatically when ESCE length changes!
        light: RefX[LIGHRecord]
        projectileSpeed: float
        effectShader: RefX[EFSHRecord]
        enchantEffect: RefX[EFSHRecord]
        castingSound: RefX['SOUNRecord']
        boltSound: RefX['SOUNRecord']
        hitSound: RefX['SOUNRecord']
        areaSound: RefX['SOUNRecord']
        constantEffectEnchantmentFactor: float
        constantEffectBarterFactor: float
        def __init__(self, r: Reader, dataSize: int):
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
            self.counterEffectCount = r.readUInt16()
            r.skip(2) # Unused
            self.light = RefX[LIGHRecord](LIGHRecord, r.readUInt32())
            self.projectileSpeed = r.readSingle()
            self.effectShader = RefX[EFSHRecord](EFSHRecord, r.readUInt32())
            if dataSize == 36: return
            self.enchantEffect = RefX[EFSHRecord](EFSHRecord, r.readUInt32())
            self.castingSound = RefX[SOUNRecord](SOUNRecord, r.readUInt32())
            self.boltSound = RefX[SOUNRecord](SOUNRecord, r.readUInt32())
            self.hitSound = RefX[SOUNRecord](SOUNRecord, r.readUInt32())
            self.areaSound = RefX[SOUNRecord](SOUNRecord, r.readUInt32())
            self.constantEffectEnchantmentFactor = r.readSingle()
            self.constantEffectBarterFactor = r.readSingle()

    def __repl__(self): return f'MGEF: {self.INDX.value}:{self.EDID.value}'
    DESC: str # Description
    # TES3
    INDX: int # The Effect ID (0 to 137)
    MEDT: Medt # Effect Data
    ICON: str # Effect Icon
    PTEX: str # Particle texture
    CVFX: str # Casting visual
    BVFX: str # Bolt visual
    HVFX: str # Hit visual
    AVFX: str # Area visual
    CSND: str = None # Cast sound (optional)
    BSND: str = None # Bolt sound (optional)
    HSND: str = None # Hit sound (optional)
    ASND: str = None # Area sound (optional)
    # TES4
    FULL: str
    MODL: Modl
    DATA: Data
    ESCEs: list[str]
    def __init__(self):
        super().__init__()
        self.DATA = self.ESCEs = self.MODL = None; self.FULL = self.AVFX = self.BVFX = self.HVFX = self.CVFX = self.DESC = str()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        if r.format == FormType.TES3:
            match type:
                case FieldType.INDX: z = self.INDX = r.readINTV(dataSize)
                case FieldType.MEDT: z = self.MEDT = MGEFRecord.Medt(r, dataSize)
                case FieldType.ITEX: z = self.ICON = r.readFUString(dataSize)
                case FieldType.PTEX: z = self.PTEX = r.readFUString(dataSize)
                case FieldType.CVFX: z = self.CVFX = r.readFUString(dataSize)
                case FieldType.BVFX: z = self.BVFX = r.readFUString(dataSize)
                case FieldType.HVFX: z = self.HVFX = r.readFUString(dataSize)
                case FieldType.AVFX: z = self.AVFX = r.readFUString(dataSize)
                case FieldType.DESC: z = self.DESC = r.readFUString(dataSize)
                case FieldType.CSND: z = self.CSND = r.readFUString(dataSize)
                case FieldType.BSND: z = self.BSND = r.readFUString(dataSize)
                case FieldType.HSND: z = self.HSND = r.readFUString(dataSize)
                case FieldType.ASND: z = self.ASND = r.readFUString(dataSize)
                case _: z = Record._empty
            return z
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.FULL: z = self.FULL = r.readFUString(dataSize)
            case FieldType.DESC: z = self.DESC = r.readFUString(dataSize)
            case FieldType.MODL: z = self.MODL = Modl(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODB(r, dataSize)
            case FieldType.ICON: z = self.MODL.ICON(r, dataSize)
            case FieldType.DATA: z = self.DATA = MGEFRecord.Data(r, dataSize)
            case FieldType.ESCE: z = self.ESCEs = r.readFArray(lambda z: r.readFUString(4), dataSize >> 2)
            case _: z = Record._empty
        return z
# end::MGEF[]

# MICN.Menu Icon - 0050 - tag::MICN[]
class MICNRecord(Record):
    ICON: str # Large icon filename
    MICO: str # Small icon filename
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.ICON: z = self.ICON = r.readFUString(dataSize)
            case FieldType.MICO: z = self.MICO = r.readFUString(dataSize)
            case _: z = Record._empty
        return z
# end::MICN[]

# MISC.Misc Item - 3450 - tag::MISC[]
class MISCRecord(Record, IHaveMODL):
    class Data:
        weight: float
        value: int
        unknown: int
        def __init__(self, r: Reader, dataSize: int):
            if r.format == FormType.TES3:
                self.weight = r.readSingle()
                self.value = r.readUInt32()
                self.unknown = r.readUInt32()
                return
            self.value = r.readUInt32()
            self.weight = r.readSingle()
            self.unknown = 0

    MODL: Modl # Model
    FULL: str # Item Name
    DATA: Data # Misc Item Data
    SCRI: RefX['SCPTRecord'] # Script FormID (optional)
    # TES3
    ENAM: RefX[ENCHRecord] # enchantment ID
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readFUString(dataSize)
            case FieldType.MODL: z = self.MODL = Modl(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODB(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODT(r, dataSize)
            case FieldType.ICON | FieldType.ITEX: z = self.MODL.ICON(r, dataSize)
            case FieldType.FULL | FieldType.FNAM: z = self.FULL = r.readFUString(dataSize)
            case FieldType.DATA | FieldType.MCDT: z = self.DATA = MISCRecord.Data(r, dataSize)
            case FieldType.ENAM: z = self.ENAM = RefX[ENCHRecord](ENCHRecord, r, dataSize)
            case FieldType.SCRI: z = self.SCRI = RefX[SCPTRecord](SCPTRecord, r, dataSize)
            case _: z = Record._empty
        return z
# end::MISC[]

# NPC_.Non-Player Character - 3450 - tag::NPC_3[]
class NPC_3Record(CREA3Record):
    class NPC_3Flags(Flag):
        Female = 0x0001
        Essential = 0x0002
        Respawn = 0x0004
        None_ = 0x0008
        Autocalc = 0x0010
        BloodSkel = 0x0400
        BloodMetal = 0x0800

    class Dodt:
        _struct = ('<6f', 24)
        def __init__(self, t):
            (self.xPos,
            self.yPos,
            self.zPos,
            self.xRot,
            self.yRot,
            self.zRot) = t

    class DodtX:
        def __repr__(self): return f'{self.DODT}'
        def __init__(self, DODT: 'NPC_3Record.Dodt'):
            self.DODT: c = DODT # Cell Travel Destination
            self.DNAM: str = None # Cell name for previous DODT, if interior

    RNAM: str # Race Name
    ANAM: str # Faction name
    BNAM: str # Head model
    KNAM: str # Hair model
    DODTs: list[DodtX] = [] # Cell Travel Destination
    def __init__(self): super().__init__(); self.DODTs = listx()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.NAME: z = self.EDID = r.readFUString(dataSize)
            case FieldType.FNAM: z = self.FULL = r.readFUString(dataSize)
            case FieldType.MODL: z = self.MODL = Modl(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODB(r, dataSize)
            case FieldType.RNAM: z = self.RNAM = r.readFUString(dataSize)
            case FieldType.ANAM: z = self.ANAM = r.readFUString(dataSize)
            case FieldType.BNAM: z = self.BNAM = r.readFUString(dataSize)
            case FieldType.CNAM: z = self.CNAM = r.readFUString(dataSize)
            case FieldType.KNAM: z = self.KNAM = r.readFUString(dataSize)
            case FieldType.NPDT: z = self.NPDT = r.readS(NPC_3Record.Npdt52, dataSize) if dataSize == 52 else (r.readS(NPC_3Record.Npdt12, dataSize) if dataSize == 12 else None)
            case FieldType.FLAG: z = self.FLAG = r.readInt32()
            case FieldType.NPCO: z = self.NPCOs.addX(CntoX(Record, r, dataSize))
            case FieldType.NPCS: z = self.NPCSs.addX(r.readFAString(dataSize))
            case FieldType.AIDT: z = self.AIDT = r.readS(CREA3Record.Aidt, dataSize)
            case FieldType.AI_A | FieldType.AI_E | FieldType.AI_F | FieldType.AI_T | FieldType.AI_W: z = self.AIs.addX(CREA3Record.Ai(r, dataSize, type))
            case FieldType.CNDT: z = self.AIs.last().CNDT = r.readFUString(dataSize)
            case FieldType.DODT: z = self.DODTs.addX(NPC_3Record.DodtX(DODT = r.readS(NPC_3Record.Dodt, dataSize)))
            case FieldType.DNAM: z = self.DODTs.last().DNAM = r.readFUString(dataSize)
            case FieldType.XSCL: z = self.XSCL = r.readSingle()
            case FieldType.SCRI: z = self.SCRI = RefX[SCPTRecord](SCPTRecord, r, dataSize)
            case _: z = Record._empty
        return z
# end::NPC_3[]

# NPC_.Non-Player Character - 3450 - tag::NPC_4[]
class NPC_4Record(CREA4Record):
    class NPC_4Flags(Flag):
        Female = 0x0001
        Essential = 0x0002
        Respawn = 0x0008
        Autocalc = 0x0010
        PCLevelOffset = 0x000080
        NoLowLevelProcessing = 0x000200
        NoRumors = 0x002000
        Summonable = 0x004000
        NoPersuasion = 0x008000
        CanCorpseCheck = 0x100000

    class Data:
        _struct = ('<21si8s', 33)
        def __init__(self, t):
            (self.skills,
            self.health,
            self.attributes) = t

    class Hclr:
        _struct = ('<4B', 4)
        def __init__(self, t):
            (self.red,
            self.green,
            self.blue,
            self.custom) = t

    RNAM: Ref['RACERecord'] # Race
    AIDT: CREA4Record.Aidt # AI Data
    CNAM: RefX[CLASRecord] # Class
    DATA: Data # Stats
    HNAM: RefX[HAIRRecord] # Hair
    LNAM: float # Hair length
    ENAM: RefX[EYESRecord] # Eyes
    HCLR: Hclr # Hair color
    FGGS: bytes # FaceGen Geometry-Symmetric
    FGGA: bytes # FaceGen Geometry-Asymmetric
    FGTS: bytes # FaceGen Texture-Symmetic
    FNAM: float # FaceGen Texture-Symmetic
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.FULL: z = self.FULL = r.readFUString(dataSize)
            case FieldType.MODL: z = self.MODL = Modl(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODB(r, dataSize)
            case FieldType.ACBS: z = self.ACBS = r.readS(CREA4Record.Acbs, dataSize)
            case FieldType.SNAM: z = self.SNAMs.addX(RefB(FACTRecord, r, dataSize))
            case FieldType.INAM: z = self.INAM = Ref(LVLIRecord, r, dataSize)
            case FieldType.RNAM: z = self.RNAM = Ref(RACERecord, r, dataSize)
            case FieldType.SPLO: z = self.SPLOs.addX(r.readFUString(dataSize))
            case FieldType.SCRI: z = self.SCRI = Ref[SCPTRecord](SCPTRecord, r, dataSize)
            case FieldType.CNTO: z = self.CNTOs.addX(Cnto(Record, r, dataSize))
            case FieldType.AIDT: z = self.AIDT = r.readS(CREA4Record.Aidt, dataSize)
            case FieldType.PKID: z = self.PKIDs.addX(RefS[PACKRecord](PACKRecord, r, dataSize))
            case FieldType.CNAM: z = self.CNAM = RefX[CLASRecord](CLASRecord, r, dataSize)
            case FieldType.AIDT: z = self.DATA = r.readS(NPC_4Record.Data, dataSize)
            case FieldType.HNAM: z = self.HNAM = RefX[HAIRRecord](HAIRRecord, r, dataSize)
            case FieldType.LNAM: z = self.LNAM = r.readSingle()
            case FieldType.ENAM: z = self.ENAM = RefX[EYESRecord](EYESRecord, r, dataSize)
            case FieldType.HCLR: z = self.HCLR = r.readS(NPC_4Record.Hclr, dataSize)
            case FieldType.ZNAM: z = self.ZNAM = Ref[CSTYRecord](CSTYRecord, r, dataSize)
            case FieldType.FGGS: z = self.FGGS = r.readBytes(dataSize)
            case FieldType.FGGA: z = self.FGGA = r.readBytes(dataSize)
            case FieldType.FGTS: z = self.FGTS = r.readBytes(dataSize)
            case FieldType.FNAM: z = self.FNAM = r.readUInt16()
            case FieldType.KFFZ: z = self.KFFZ = r.readFUString(dataSize)
            case FieldType.NIFT: z = r.skip(dataSize) #TODO
            case FieldType.AIDT: z = r.skip(dataSize) #TODO
            case FieldType.DATA: z = r.skip(dataSize) #TODO
            case _: z = Record._empty
        return z
# end::NPC_4[]

# OTFT.Outfit - 00500 - tag::OTFT[]
class OTFTRecord(Record):
    INAM: list[Ref[Record]] # Inventory list - Array of ARMO or LVLI
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.INAM: z = self.INAM = r.readFArray(lambda z: Ref[Record](Record, r, 4), dataSize >> 2)
            case _: z = Record._empty
        return z
# end::OTFT[]

# PACK.AI Package - 0450 - tag::PACK[]
class PACKRecord(Record):
    class Pkdt:
        _struct = { 4: '<I', 8: '<I4B', 16: '<I4BI' }
        def __init__(self, t):
            if not isinstance(t, tuple): self.flags = t; return
            match len(t):
                case 5:
                    (self.flags,
                    self.packageType,
                    self.interruptOverride,
                    self.preferredSpeed,
                    self.unknown) = t
                case 6:
                    (self.flags,
                    self.packageType,
                    self.interruptOverride,
                    self.preferredSpeed,
                    self.unknown,
                    self.interruptFlags) = t
                case _: raise NotImplementedError('PKDTField')

    class Psdt:
        _struct = ('<3Bbi', 8)
        def __init__(self, t):
            (self.month,
            self.dayOfWeek,
            self.date,
            self.time,
            self.duration) = t

    class Pldt:
        _struct = ('<iIi', 12)
        def __init__(self, t):
            (self.type,
            self.target,
            self.radius) = t

    class Ptdt:
        _struct = ('<iIi', 12)
        def __init__(self, t):
            (self.type,
            self.target,
            self.count) = t

    PKDT: Pkdt # General
    PLDT: Pldt # Location
    PSDT: Psdt # Schedule
    PTDT: Ptdt # Target
    CTDAs: list['SCPTRecord.Ctda'] = [] # Conditions
    def __init__(self): super().__init__(); self.CTDAs = listx()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.PKDT: z = self.PKDT = r.readS(PACKRecord.Pkdt, dataSize)
            case FieldType.PLDT: z = self.PLDT = r.readS(PACKRecord.Pldt, dataSize)
            case FieldType.PSDT: z = self.PSDT = r.readS(PACKRecord.Psdt, dataSize)
            case FieldType.PTDT: z = self.PTDT = r.readS(PACKRecord.Ptdt, dataSize)
            case FieldType.CTDA | FieldType.CTDT: z = self.CTDAs.addX(SCPTRecord.Ctda(r, dataSize))
            case _: z = Record._empty
        return z
# end::PACK[]

# PGRD.Path grid - 3400 - tag::PGRD[]
class PGRDRecord(Record):
    class Data:
        x: int
        y: int
        granularity: int
        pointCount: int
        def __init__(self, r: Reader, dataSize: int):
            if r.format != FormType.TES3:
                self.x = self.y = self.granularity = 0
                self.pointCount = r.readInt16()
                return
            self.x = r.readInt32()
            self.y = r.readInt32()
            self.granularity = r.readInt16()
            self.pointCount = r.readInt16()

    class Pgrp:
        _struct = ('<3fB3s', 16)
        def __init__(self, t):
            point = self.point = array([None]*3)
            (point[0], point[1], point[2],
            self.connections,
            self.unused) = t

    class Pgrr:
        _struct = ('<2h', 4)
        def __init__(self, t):
            (self.startPointId,
            self.endPointId) = t

    class Pgri:
        _struct = ('<hH3f', 16)
        def __init__(self, t):
            foreignNode = self.foreignNode = array([None]*3)
            (self.pointId,
            self.unused,
            foreignNode[0], foreignNode[1], foreignNode[2]) = t

    class Pgrl:
        def __init__(self, r: Reader, dataSize: int):
            self.reference: RefX[REFRRecord] = RefX[REFRRecord](REFRRecord, r.readUInt32())
            self.pointIds: list[int] = r.readFArray(lambda z: (r.readInt16(), r.skip(2))[0], (dataSize - 4) >> 2) # 2:Unused (can merge back)

    DATA: Data # Number of nodes
    PGRPs: list[Pgrp]
    PGRC: bytes
    PGAG: bytes
    PGRRs: list[Pgrr] # Point-to-Point Connections
    PGRLs: list[Pgrl] # Point-to-Reference Mappings
    PGRIs: list[Pgri] # Inter-Cell Connections
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readFUString(dataSize)
            case FieldType.DATA: z = self.DATA = PGRDRecord.Data(r, dataSize)
            case FieldType.PGRP: z = self.PGRPs = r.readSArray(PGRDRecord.Pgrp, dataSize >> 4)
            case FieldType.PGRC: z = self.PGRC = r.readBytes(dataSize)
            case FieldType.PGAG: z = self.PGAG = r.readBytes(dataSize)
            case FieldType.PGRR: z = self.PGRRs = r.readSArray(PGRDRecord.Pgrr, dataSize >> 2); r.skip(dataSize % 4)
            case FieldType.PGRL: z = _nca(self, 'PGRLs', listx()).addX(PGRDRecord.Pgrl(r, dataSize))
            case FieldType.PGRI: z = self.PGRIs = r.readSArray(PGRDRecord.Pgri, dataSize >> 4)
            case _: z = Record._empty
        return z
# end::PGRD[]

# PROB.Probe - 3000 - tag::PROB[]
class PROBRecord(Record, IHaveMODL):
    class Pbdt:
        _struct = ('<fifi', 16)
        def __init__(self, t):
            (self.weight,
            self.value,
            self.quality,
            self.uses) = t

    MODL: Modl # Model Name
    FNAM: str # Item Name
    PBDT: Pbdt # Probe Data
    SCRI: RefX['SCPTRecord'] # Script Name
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        if r.format == FormType.TES3:
            match type:
                case FieldType.NAME: z = self.EDID = r.readFUString(dataSize)
                case FieldType.MODL: z = self.MODL = Modl(r, dataSize)
                case FieldType.ITEX: z = self.MODL.ICON(r, dataSize)
                case FieldType.FNAM: z = self.FNAM = r.readFUString(dataSize)
                case FieldType.PBDT: z = self.PBDT = r.readS(PROBRecord.Pbdt, dataSize)
                case FieldType.SCRI: z = self.SCRI = RefX[SCPTRecord](SCPTRecord, r, dataSize)
                case _: z = Record._empty
            return z
        return None
# end::PROB[]

# QUST.Quest - 0450 - tag::QUST[]
class QUSTRecord(Record):
    class Data:
        _struct = ('<2B', 2)
        def __init__(self, t):
            (self.flags,
            self.priority) = t

    FULL: str # Item Name
    ICON: str # Icon
    DATA: Data # Icon
    SCRI: RefX['SCPTRecord'] # Script Name
    SCHRs: list['SCPTRecord.SCHRField'] = [] # Script Data
    SCDAs: list['SCPTRecord.SCDAField'] = [] # Compiled Script
    SCTXs: list[str] = [] # Script Source
    SCROs: list[RefX[Record]] = [] # Global variable reference
    def __init__(self): super().__init__(); self.SCHRs = listx(); self.SCDAs = listx(); self.SCTXs = listx(); self.SCROs = listx()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.FULL: z = self.FULL = r.readFUString(dataSize)
            case FieldType.ICON: z = self.ICON = r.readFUString(dataSize)
            case FieldType.DATA: z = self.DATA = r.readS(QUSTRecord.Data, dataSize)
            case FieldType.SCRI: z = self.SCRI = RefX[SCPTRecord](SCPTRecord, r, dataSize)
            case FieldType.CTDA: z = r.skip(dataSize)
            case FieldType.INDX: z = r.skip(dataSize)
            case FieldType.QSDT: z = r.skip(dataSize)
            case FieldType.CNAM: z = r.skip(dataSize)
            case FieldType.QSTA: z = r.skip(dataSize)
            case FieldType.SCHR: z = self.SCHRs.addX(SCPTRecord.Schr(r, dataSize))
            case FieldType.SCDA: z = self.SCDAs.addX(SCPTRecord.Scda(r, dataSize))
            case FieldType.SCTX: z = self.SCTXs.addX(r.readFUString(dataSize))
            case FieldType.SCRO: z = self.SCROs.addX(RefX[Record](Record, r, dataSize))
            case _: z = Record._empty
        return z
# end::QUST[]

# RACE.Race_Creature type - 3450 - tag::RACE[]
class RACERecord(Record):
    class DataFlag(Flag):
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

    class Data:
        class SkillBoost:
            _struct = ('<Bb', 2)
            skillId: int
            bonus: int
            def __init__(self, *args):
                match len(args):
                    case 1: (self.skillId, self.bonus) = args[0]
                    case 2: self.skillId = args[0]; self.skillId = args[1]

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

        class Attrib_:
            _struct = { 128 - 36: '<7f3IfI5fIfI2f10f', 164 - 36: '<7f3IfI5fIfI2f10f36x' }
            def __init__(self, t):
                unknowns = self.unknowns = [None]*7
                (self.startingHealth,
                self.startingMagicka,
                self.startingStamina,
                self.baseCarryWeight,
                self.baseMass,
                self.accelerationRate,
                self.decelerationRate,
                self.size, # lookup: 0=Small, 1=Medium, 2=Large, 3=Extra Large
                self.headBipedObject,
                self.hairBipedObject,
                self.injuredHealthPct,
                self.shieldBipedObject,
                self.healthRegen,
                self.magickaRegen,
                self.staminaRegen,
                self.unarmedDamage,
                self.unarmedReach,
                self.bodyBipedObject,
                self.aimAngleTolerance,
                self.unknown,
                self.angularAccelerationRate,
                self.angularTolerance,
                self.flags,
                self.mountDataOffsetX,
                self.mountDataOffsetY,
                unknowns[0], unknowns[1], unknowns[2], unknowns[3], unknowns[4], unknowns[5], unknowns[6]) = t
                self.headBipedObject = Ref[Record](self.headBipedObject)
                self.hairBipedObject = Ref[Record](self.hairBipedObject)
                self.shieldBipedObject = Ref[Record](self.shieldBipedObject)

        skillBoosts: list[SkillBoost] # Skill Boosts
        male: RaceStats = RaceStats()
        female: RaceStats = RaceStats()
        flags: 'DataFlag'
        attrib: Attrib_
        def __init__(self, r: Reader, dataSize: int):
            if r.format == FormType.TES3:
                self.skillBoosts = r.readFArray(lambda z: RACERecord.Data.SkillBoost(r.readInt32() & 0xFF, r.readInt32() & 0xFF), 7)
                self.male.strength = r.readInt32() & 0xFF; self.female.strength = r.readInt32() & 0xFF
                self.male.intelligence = r.readInt32() & 0xFF; self.female.intelligence = r.readInt32() & 0xFF
                self.male.willpower = r.readInt32() & 0xFF; self.female.willpower = r.readInt32() & 0xFF
                self.male.agility = r.readInt32() & 0xFF; self.female.agility = r.readInt32() & 0xFF
                self.male.speed = r.readInt32() & 0xFF; self.female.speed = r.readInt32() & 0xFF
                self.male.endurance = r.readInt32() & 0xFF; self.female.endurance = r.readInt32() & 0xFF
                self.male.personality = r.readInt32() & 0xFF; self.female.personality = r.readInt32() & 0xFF
                self.male.luck = r.readInt32(); self.female.luck = r.readInt32() & 0xFF
            else:
                self.skillBoosts = r.readSArray(RACERecord.Data.SkillBoost, 7)
                r.readInt16() # padding
            self.male.height = r.readSingle(); self.female.height = r.readSingle()
            self.male.weight = r.readSingle(); self.female.weight = r.readSingle()
            self.flags = RACERecord.DataFlag(r.readUInt32())
        def ATTR(self, r: Reader, dataSize: int):
            if dataSize == 2:
                self.male.strength = r.readByte()
                self.female.strength = r.readByte()
                return self
            self.male.strength = r.readByte()
            self.male.intelligence = r.readByte()
            self.male.willpower = r.readByte()
            self.male.agility = r.readByte()
            self.male.speed = r.readByte()
            self.male.endurance = r.readByte()
            self.male.personality = r.readByte()
            self.male.luck = r.readByte()
            self.female.strength = r.readByte()
            self.female.intelligence = r.readByte()
            self.female.willpower = r.readByte()
            self.female.agility = r.readByte()
            self.female.speed = r.readByte()
            self.female.endurance = r.readByte()
            self.female.personality = r.readByte()
            self.female.luck = r.readByte()
            return self

    FULL: str # Race name
    DESC: str # Race description
    SPLOs: list[str] = [] # NPCs: Special power/ability name
    DATA: Data # RADT:DATA/ATTR: Race data/Base Attributes
    def __init__(self): super().__init__(); self.SPLOs = listx()
# end::RACE[]

# tag::RACE3[]
class RACE3Record(RACERecord):
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.NAME: z = self.EDID = r.readFUString(dataSize)
            case FieldType.FNAM: z = self.FULL = r.readFUString(dataSize)
            case FieldType.RADT: z = self.DATA = RACERecord.Data(r, dataSize)
            case FieldType.NPCS: z = self.SPLOs.addX(r.readFUString(dataSize))
            case FieldType.DESC: z = self.DESC = r.readFUString(dataSize)
            case _: z = Record._empty
        return z
# end::RACE3[]

# tag::RACE4[]
class RACE4Record(RACERecord):
    class FaceIndx(Enum): Head = 0; Ear_Male = 1; Ear_Female = 2; Mouth = 3; Teeth_Lower = 4; Teeth_Upper = 5; Tongue = 6; Eye_Left = 7; Eye_Right = 8
    class BodyIndx(Enum): UpperBody = 0; LowerBody = 1; Hand = 2; Foot = 3; Tail = 4

    VNAM: Ref2['RACERecord'] # Voice
    DNAM: Ref2[HAIRRecord] # Default Hair
    CNAM: int # Default Hair Color
    PNAM: float # FaceGen - Main clamp
    UNAM: float # FaceGen - Face clamp
    XNAM: bytes # Unknown
    FACEs: list[Modl] = [None]*9
    BODYs: list[list[Modl]] = [[None]*5, [None]*5]
    HNAMs: list[RefX[HAIRRecord]] = []
    ENAMs: list[RefX[EYESRecord]] = []
    FGGS: bytes # FaceGen Geometry-Symmetric
    FGGA: bytes # FaceGen Geometry-Asymmetric
    FGTS: bytes # FaceGen Texture-Symmetric
    SNAM: bytes # Unknown
    # fallout
    ONAM: Ref[RACERecord] # Older
    YNAM: Ref[RACERecord] # Younger
    VTCK: Ref2[Record] # Voices #TODO VTYPRecord
    _index: int
    _last: Modl
    _nam: int
    _nam2: int
    def __init__(self): super().__init__(); self._nam = 0; self._nam2 = 0; self.SPLOs = listx(); self.HNAMs = listx(); self.ENAMs = listx(); self.FaceParts = listx()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.FULL: z = self.FULL = r.readFUString(dataSize)
            case FieldType.DESC: z = self.DESC = r.readFUString(dataSize)
            case FieldType.DATA: z = self.DATA = RACERecord.Data(r, dataSize)
            case FieldType.SPLO: z = self.SPLOs.addX(r.readFUString(dataSize))
            case FieldType.VNAM: z = self.VNAM = Ref2[RACERecord](RACERecord, r, dataSize)
            case FieldType.DNAM: z = self.DNAM = Ref2[HAIRRecord](HAIRRecord, r, dataSize)
            case FieldType.CNAM: z = self.CNAM = r.readINTV(dataSize)
            case FieldType.PNAM: z = self.PNAM = r.readSingle()
            case FieldType.UNAM: z = self.UNAM = r.readSingle()
            case FieldType.XNAM: z = self.XNAM = r.readBytes(dataSize)
            case FieldType.ATTR: z = self.DATA.ATTR(r, dataSize)
            # section: face/body data
            case FieldType.NAM0: z = self._nam = 0
            case FieldType.NAM1: z = self._nam = 1
            case FieldType.NAM2: z = self._nam = 2
            case FieldType.MNAM: z = self._nam2 = 0
            case FieldType.FNAM: z = self._nam2 = 1
            case FieldType.INDX: z = self._index = r.readUInt32()
            case FieldType.MODL: z = self._last = self.FACEs[self._index] = Modl(r, dataSize) if self._nam == 0 else self.BODYs[self._nam2][self._index]
            case FieldType.MODB: z = self._last.MODB(r, dataSize)
            case FieldType.MODT: z = self._last.MODT(r, dataSize)
            case FieldType.ICON: z = self._last.ICON(r, dataSize)
            # section: end
            case FieldType.HNAM: z = self.HNAMs.addRangeX(r.readFArray(lambda z: RefX[HAIRRecord](HAIRRecord, r, 4), dataSize >> 2))
            case FieldType.ENAM: z = self.ENAMs.addRangeX(r.readFArray(lambda z: RefX[EYESRecord](EYESRecord, r, 4), dataSize >> 2))
            case FieldType.FGGS: z = self.FGGS = r.readBytes(dataSize)
            case FieldType.FGGA: z = self.FGGA = r.readBytes(dataSize)
            case FieldType.FGTS: z = self.FGTS = r.readBytes(dataSize)
            case FieldType.SNAM: z = self.SNAM = r.readBytes(dataSize)
            # fallout
            case FieldType.ONAM: z = self.ONAM = Ref[RACERecord](RACERecord, r, dataSize)
            case FieldType.YNAM: z = self.YNAM = Ref[RACERecord](RACERecord, r, dataSize)
            case FieldType.VTCK: z = self.VTCK = Ref2[Record](Record, r, dataSize) #TODO VTYPRecord
            case _: z = Record._empty
        return z
# end::RACE4[]

# tag::RACE5[]
class RACE5Record(RACERecord):
    class Body:
        ANAM: str
        MODT: object
    
    _state: int
    _state2: int
    SPCT: int # Spell count
    WNAM: Ref[ARMORecord] # Skin
    BODT: ARMORecord.Bodt # Body template
    KSIZ: int # Keyword count
    KWDA: list[Ref[KYWDRecord]] # Keywords
    Bodys: list[Body] = [Body(), Body()]
    def __init__(self): super().__init__(); self._state = 0; self._state2 = 0

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.FULL: z = self.FULL = r.readFUString(dataSize)
            case FieldType.DESC: z = self.DESC = r.readFUString(dataSize)
            case FieldType.DATA: z = self.DATA = RACERecord.Data(r, dataSize)
            case FieldType.SPCT: z = self.SPCT = r.readUInt32()
            case FieldType.SPLO: z = self.SPLOs.addX(r.readFUString(dataSize))
            case FieldType.WNAM: z = self.WNAM = Ref[ARMORecord](ARMORecord, r, dataSize)
            case FieldType.BODT: z = self.BODT = r.readS(ARMORecord.Bodt, dataSize)
            case FieldType.KSIZ: z = self.KSIZ = r.readUInt32()
            case FieldType.KWDA: z = self.KWDA = r.readFArray(lambda z: Ref[KYWDRecord](KYWDRecord, r, 4), self.KSIZ)
            # body
            case FieldType.MNAM: z = self._state2 = 0
            case FieldType.FNAM: z = self._state2 = 1
            case FieldType.MODL: z = self.Bodys[self._state2].ANAM = r.readFUString(dataSize)
            case FieldType.MODB: z = self.Bodys[self._state2].MODT = Modt(r, dataSize)
            case _: z = Record._empty
        return z
# end::RACE5[]

# REPA.Repair Item - 3000 - tag::REPA[]
class REPARecord(Record, IHaveMODL):
    class Ridt:
        _struct = ('<f2if', 16)
        def __init__(self, t):
            (self.weight,
            self.value,
            self.uses,
            self.quality) = t

    MODL: Modl # Model Name
    FNAM: str # Item Name
    RIDT: Ridt # Repair Data
    SCRI: RefX['SCPTRecord'] # Script Name
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        if r.format == FormType.TES3:
            match type:
                case FieldType.NAME: z = self.EDID = r.readFUString(dataSize)
                case FieldType.MODL: z = self.MODL = Modl(r, dataSize)
                case FieldType.ITEX: z = self.MODL.ICON(r, dataSize)
                case FieldType.FNAM: z = self.FNAM = r.readFUString(dataSize)
                case FieldType.RIDT: z = self.RIDT = r.readS(REPARecord.Ridt, dataSize)
                case FieldType.SCRI: z = self.SCRI = RefX[SCPTRecord](SCPTRecord, r, dataSize)
                case _: z = Record._empty
            return z
        return None
# end::REPA[]

# REFR.Placed Object - 0450 - tag::REFR[]
class REFRRecord(Record):
    # Teleport Destination
    class Xtel:
        class Flag(Flag): NoAlarm = 0x00; NoLoadScreen = 0x01; RelativePosition = 0x02
        def __init__(self, r: Reader, dataSize: int):
            self.door: Ref[REFRRecord] = Ref[REFRRecord](REFRRecord, r.readUInt32())
            self.position: Vector3 = r.readVector3()
            self.rotation: Vector3 = r.readVector3()
            self.flags: Flag = Flag(r.readUInt32()) if r.format > FormType.TES4 else 0
            self.transitionInterior: Ref[CELLRecord] = Ref[CELLRecord](CELLRecord, r.readUInt32()) if r.format >= FormType.TES5 else 0

    # Coords
    class Data:
        _struct = ('<6f', 24)
        def __init__(self, t):
            position = self.position = array([None]*3)
            rotation = self.rotation = array([None]*3)
            (position[0], position[1], position[2],
            rotation[0], rotation[1], rotation[2]) = t

    # Ragdoll Data
    class Xrgd:
        _struct = ('<B3c6f', 28)
        def __init__(self, t):
            unused = self.unused = [None]*3
            position = self.position = array([None]*3)
            rotation = self.rotation = array([None]*3)
            (self.boneId,
            unused[0], unused[1], unused[2],
            position[0], position[1], position[2],
            rotation[0], rotation[1], rotation[2]) = t

    class Xloc:
        def __repr__(self): return f'{self.key}'
        lockLevel: int
        key: RefX['KEYMRecord'] 
        flags: int
        def __init__(self, r: Reader, dataSize: int):
            self.lockLevel = r.readByte()
            r.skip(3); # Unused
            self.key = RefX[KEYMRecord](KEYMRecord, r.readUInt32())
            if dataSize == 16: r.skip(4) # Unused
            self.flags = r.readByte()
            r.skip(3) # Unused

    class Xesp:
        def __repr__(self): return f'{self.reference}'
        reference: RefX[Record]
        flags: int
        def __init__(self, r: Reader, dataSize: int):
            self.reference = RefX[Record](Record, r.readUInt32())
            self.flags = r.readByte()
            r.skip(3) # Unused

    class Xsed:
        def __repr__(self): return f'{self.seed}'
        _struct = { 1: '<B', 4: '<B3x' }
        def __init__(self, t): self.seed = tuple

    class Xmrk:
        def __repr__(self): return f'{self.FULL.value}'
        FNAM: int # Map Flags
        FULL: str # Name
        TNAM: int # Type

    NAME: RefX[Record] # Base
    XTEL: Xtel = None # Teleport Destination (optional)
    DATA: Data # Position/Rotation
    XLOC: Xloc = None # Lock information (optional)
    XOWNs: list['CELLRecord.XOWNGroup'] # Ownership (optional)
    XESP: Xesp = None # Enable Parent (optional)
    XTRG: RefX[Record] = None # Target (optional)
    XSED: Xsed = None # SpeedTree (optional)
    XLOD: bytes = None # Distant LOD Data (optional)
    XCHG: float = None # Charge (optional)
    XHLT: float = None # Health (optional)
    XPCI: RefX['CELLRecord'] = None # Unused (optional)
    XLCM: int = None # Level Modifier (optional)
    XRTM: RefX['REFRRecord'] = None # Unknown (optional)
    XACT: int = None # Action Flag (optional)
    XCNT: int = None # Count (optional)
    XMRKs: list[Xmrk] # Ownership (optional)
    #ONAM: bool = None # Open by Default
    XRGD: bytes = None # Ragdoll Data (optional)
    XSCL: float = None # Scale (optional)
    XSOL: int = None # Contained Soul (optional)
    _nextFull: int
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.NAME: z = self.NAME = RefX[Record](Record, r, dataSize)
            case FieldType.XTEL: z = self.XTEL = REFRRecord.Xtel(r, dataSize)
            case FieldType.DATA: z = self.DATA = r.readS(REFRRecord.Data, dataSize)
            case FieldType.XLOC: z = self.XLOC = REFRRecord.Xloc(r, dataSize)
            case FieldType.XOWN: z = _nca(self, 'XOWNs', listx()).addX(CELLRecord.Xown(XOWN = RefX[Record](Record, r, dataSize)))
            case FieldType.XRNK: z = self.XOWNs.last().XRNK = r.readInt32()
            case FieldType.XGLB: z = self.XOWNs.last().XGLB = RefX[Record](Record, r, dataSize)
            case FieldType.XESP: z = self.XESP = REFRRecord.Xesp(r, dataSize)
            case FieldType.XTRG: z = self.XTRG = RefX[Record](Record, r, dataSize)
            case FieldType.XSED: z = self.XSED = r.readS(REFRRecord.Xsed, dataSize)
            case FieldType.XLOD: z = self.XLOD = r.readBytes(dataSize)
            case FieldType.XCHG: z = self.XCHG = r.readSingle()
            case FieldType.XHLT: z = self.XCHG = r.readSingle()
            case FieldType.XPCI: z = self.XPCI = RefX[CELLRecord](CELLRecord, r, dataSize); self._nextFull = 1
            case FieldType.FULL if self._nextFull == 1: z = self.XPCI.setName(r.readFAString(dataSize)) #:matchif
            case FieldType.FULL if self._nextFull == 2: z = self.XMRKs.last().FULL = r.readFUString(dataSize) #:matchif
            case FieldType.FULL if self._nextFull != 1 and self._nextFull != 2: self._nextFull = 0 #:matchif
            case FieldType.XLCM: z = self.XLCM = r.readInt32()
            case FieldType.XRTM: z = self.XRTM = RefX[REFRRecord](REFRRecord, r, dataSize)
            case FieldType.XACT: z = self.XACT = r.readUInt32()
            case FieldType.XCNT: z = self.XCNT = r.readInt32()
            case FieldType.XMRK: z = _nca(self, 'XMRKs', listx()).addX(REFRRecord.Xmrk()); self._nextFull = 2
            case FieldType.FNAM: z = self.XMRKs.last().FNAM = r.readByte()
            case FieldType.TNAM: z = self.XMRKs.last().TNAM = r.readUInt16()
            case FieldType.ONAM: z = True
            case FieldType.XRGD: z = self.XRGD = r.readBytes(dataSize)
            case FieldType.XSCL: z = self.XSCL = r.readSingle()
            case FieldType.XSOL: z = self.XSOL = r.readByte()
            case _: z = Record._empty
        return z
# end::REFR[]

# REGN.Region - 3450 - tag::REGN[]
class REGNRecord(Record):
    class REGNType(Enum): None_ = 0; One = 1; Objects = 2; Weather = 3; Map = 4; Landscape = 5; Grass = 6; Sound = 7

    class Rdat:
        def __init__(self, r: Reader = None, dataSize: int = 0, RDSDs: list['RDGSField'] = None):
            self.type: int = None
            self.flags: REGNType = None
            self.priority: int = None
            # groups
            self.RDOTs: list['Rdot'] = None # Objects
            self.RDMP: str = None # MapName
            self.RDGSs: list['Rdgs'] = None # Grasses
            self.RDMD: int = None # Music Type
            self.RDSDs: list['Rdsd'] = RDSDs # Sounds
            self.RDWTs: list['Rdwt'] = None # Weather Types
            if not r: return
            self.type = r.readUInt32()
            self.flags = REGNRecord.REGNType(r.readByte())
            self.priority = r.readByte()
            r.skip(2) # Unused

    class Rdot:
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
        def __init__(self, r: Reader, dataSize: int):
            self.object = Ref[Record](Record, r.readUInt32())
            self.parentIdx = r.readUInt16()
            r.skip(2) # Unused
            self.density = r.readSingle()
            self.clustering = r.readByte()
            self.minSlope = r.readByte()
            self.maxSlope = r.readByte()
            self.flags = r.readByte()
            self.radiusWrtParent = r.readUInt16()
            self.radius = r.readUInt16()
            self.minHeight = r.readSingle()
            self.maxHeight = r.readSingle()
            self.sink = r.readSingle()
            self.sinkVariance = r.readSingle()
            self.sizeVariance = r.readSingle()
            self.angleVariance = Int3(r.readUInt16(), r.readUInt16(), r.readUInt16())
            r.skip(2) # Unused
            self.vertexShading = r.readS(ByteColor4, 4)

    class Rdgs:
        def __repr__(self): return f'{self.grass}'
        grass: Ref[GRASRecord] 
        def __init__(self, r: Reader, dataSize: int):
            self.grass = Ref[GRASRecord](GRASRecord, r.readUInt32())
            r.skip(4) # Unused

    class Rdsd:
        def __repr__(self): return f'{self.sound}'
        sound: RefX['SOUNRecord']
        flags: int
        chance: int
        def __init__(self, r: Reader, dataSize: int):
            if r.format == FormType.TES3:
                self.sound = RefX[SOUNRecord](SOUNRecord, r.readFAString(32))
                self.flags = 0
                self.chance = r.readByte()
                return
            self.sound = RefX[SOUNRecord](SOUNRecord, r.readUInt32())
            self.flags = r.readUInt32()
            self.chance = r.readUInt32() # float with TES5

    class Rdwt:
        def __repr__(self): return f'{self.weather}'
        def __init__(self, r: Reader, dataSize: int):
            self.weather: Ref[WTHRRecord] = Ref[WTHRRecord](WTHRRecord, r.readUInt32())
            self.chance: int = r.readUInt32()
            self.global_: Ref[GLOBRecord] = Ref[GLOBRecord](GLOBRecord, r.readUInt32()) if r.format == FormType.TES5 else Ref[GLOBRecord](GLOBRecord, 0)

    # TES3
    class Weat:
        # v1.3 ESM files add 2 bytes to WEAT subrecords.
        _struct = { 8: '<8B', 10: '<8B2x' }
        def __init__(self, t):
            (self.clear,
            self.cloudy,
            self.foggy,
            self.overcast,
            self.rain,
            self.thunder,
            self.ash,
            self.blight) = t

    # TES4
    class Rpli:
        def __init__(self, r: Reader, dataSize: int):
            self.edgeFalloff: int = r.readUInt32() # (World Units)
            self.points: list[Vector2] # Region Point List Data
        def RPLD(self, r: Reader, dataSize: int) -> object: z = self.points = r.readFArray(lambda z: r.readVector2(), dataSize >> 3); return z

    ICON: str # Icon / Sleep creature
    WNAM: RefX['WRLDRecord'] # Worldspace - Region name
    RCLR: ByteColor4 # Map Color (COLORREF)
    RDATs: list[Rdat] = [] # Region Data Entries / TES3: Sound Record (order determines the sound priority)
    # TES3
    WEAT: Weat = None # Weather Data
    # TES4
    RPLIs: list[Rpli] = [] # Region Areas
    _last: Rdat
    def __init__(self): super().__init__(); self.RDATs = listx(); self.RPLIs = listx()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readFUString(dataSize)
            case FieldType.WNAM | FieldType.FNAM: z = self.WNAM = RefX[WRLDRecord](WRLDRecord, r, dataSize)
            case FieldType.WEAT: z = self.WEAT = r.readS(REGNRecord.Weat, dataSize)
            case FieldType.ICON | FieldType.BNAM: z = self.ICON = r.readFUString(dataSize)
            case FieldType.RCLR | FieldType.CNAM: z = self.RCLR = r.readS(ByteColor4, dataSize)
            case FieldType.SNAM: z = self._last = self.RDATs.addX(REGNRecord.Rdat(RDSDs = [REGNRecord.Rdsd(r, dataSize)]))
            case FieldType.RPLI: z = self.RPLIs.addX(REGNRecord.Rpli(r, dataSize))
            case FieldType.RPLD: z = self.RPLIs.last().RPLD(r, dataSize)
            case FieldType.RDAT: z = self._last = self.RDATs.addX(REGNRecord.Rdat(r, dataSize))
            case FieldType.RDOT: z = self._last.RDOTs = r.readFArray(lambda z: REGNRecord.Rdot(r, dataSize), dataSize // 52)
            case FieldType.RDMP: z = self._last.RDMP = r.readFUString(dataSize)
            case FieldType.RDGS: z = self._last.RDGSs = r.readFArray(lambda z: REGNRecord.Rdgs(r, dataSize), dataSize // 8)
            case FieldType.RDMD: z = self._last.RDMD = r.readUInt32()
            case FieldType.RDSD: z = self._last.RDSDs = r.readFArray(lambda z: REGNRecord.Rdsd(r, dataSize), dataSize // 12)
            case FieldType.RDWT: z = self._last.RDWTs = r.readFArray(lambda z: REGNRecord.Rdwt(r, dataSize), dataSize // (8 if r.format == FormType.TES4 else 12))
            case _: z = Record._empty
        return z
# end::REGN[]

# ROAD.Road - 0400 - tag::ROAD[]
class ROADRecord(Record):
    PGRPs: list[PGRDRecord.Pgrp]
    PGRR: bytes
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.PGRP: z = self.PGRPs = r.readSArray(PGRDRecord.Pgrp, dataSize >> 4)
            case FieldType.PGRR: z = self.PGRR = r.readBytes(dataSize)
            case _: z = Record._empty
        return z
# end::ROAD[]

# SBSP.Subspace - 0400 - tag::SBSP[]
class SBSPRecord(Record):
    class Dnam:
        _struct = ('<3f', 12)
        def __init__(self, t):
            (self.x, # X dimension
            self.y, # Y dimension
            self.z) = t # Z dimension

    DNAM: Dnam
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.DNAM: z = self.DNAM = r.readS(SBSPRecord.Dnam, dataSize)
            case _: z = Record._empty
        return z
# end::SBSP[]

# SCPT.Script - 3400 - tag::SCPT[]
class SCPTRecord(Record):
    class Ctda:
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
        def __init__(self, r: Reader, dataSize: int):
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
            if dataSize == 24: r.skip(4) # Unused
            self.index = self.type = 0; self.name = None

    class Scvr:
        def __init__(self, SCVR: 'SCPTRecord.Ctda'):
            self.SCVR = SCVR
            self.INTV: int = None #
            self.FLTV: float = None # The function/variable result for the previous SCVR

    # TES3
    class Schd:
        def __repr__(self): return f'{self.name}'
        def __init__(self, r: Reader, dataSize: int):
            self.name: str = r.readFAString(32)
            self.numShorts: int = r.readInt32()
            self.numLongs: int = r.readInt32()
            self.numFloats: int = r.readInt32()
            self.scriptDataSize: int = r.readInt32()
            self.localVarSize: int = r.readInt32()
            self.variables: list[str] = None
        def SCVR(self, r: Reader, dataSize: int) -> object: z = self.variables = r.readVAStringList(dataSize); return z

    # TES4
    class Schr:
        def __repr__(self): return f'{self.refCount}'
        def __init__(self, r: Reader, dataSize: int):
            self.refCount: int = r.skip(4).readUInt32() # 4:Unused
            self.compiledSize: int = r.readUInt32()
            self.variableCount: int = r.readUInt32()
            self.type: int = r.readUInt32() # 0x000 = Object, 0x001 = Quest, 0x100 = Magic Effect
            r.skip(dataSize - 20 if dataSize > 20 else 0)
            
    class Scda:
        def __repr__(self): return f'{self.data}'
        def __init__(self, r: Reader, dataSize: int):
            self.data = r.readBytes(dataSize)

    class Slsd:
        def __repr__(self): return f'{self.idx}:{self.variableName}'
        idx: int
        type: int
        variableName: str
        def __init__(self, r: Reader, dataSize: int):
            self.idx = r.readUInt32()
            r.readUInt32() # Unknown
            r.readUInt32() # Unknown
            r.readUInt32() # Unknown
            self.type = r.readUInt32()
            r.readUInt32() # Unknown
            # SCVRField
            self.variableName = None
        def SCVR(self, r: Reader, dataSize: int) -> object: z = self.variableName = r.readFUString(dataSize); return z

    def __repr__(self): return f'SCPT: {self.EDID.value or self.SCHD.name}'
    SCDA: bytes # Compiled Script
    SCTX: str # Script Source
    # TES3
    SCHD: Schd # Script Data
    # TES4
    SCHR: Schr # Script Data
    SLSDs: list[Slsd] = [] # Variable data
    SCRVs: list[Slsd] = [] # RefX variable data (one for each ref declared)
    SCROs: list[RefX[Record]] = [] # Global variable reference
    def __init__(self): super().__init__(); self.SLSDs = listx(); self.SCRVs = listx(); self.SCROs = listx()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.SCHD: z = self.SCHD = SCPTRecord.Schd(r, dataSize)
            case FieldType.SCVR: z = self.SLSDs.last().SCVR(r, dataSize) if r.format != FormType.TES3 else self.SCHD.SCVR(r, dataSize)
            case FieldType.SCDA | FieldType.SCDT: z = self.SCDA = r.readBytes(dataSize)
            case FieldType.SCTX: z = self.SCTX = r.readFUString(dataSize)
            # TES4
            case FieldType.SCHR: z = self.SCHR = SCPTRecord.Schr(r, dataSize)
            case FieldType.SLSD: z = self.SLSDs.addX(SCPTRecord.Slsd(r, dataSize))
            case FieldType.SCRO: z = self.SCROs.addX(RefX[Record](Record, r, dataSize))
            case FieldType.SCRV: z = self.SCRVs.addX(self.then(r.readUInt32(), lambda v: self.SLSDs.single(lambda s: s.idx == v)))
            case _: z = Record._empty
        return z
# end::SCPT[]

# SGST.Sigil Stone - 0400 - tag::SGST[]
class SGSTRecord(Record, IHaveMODL):
    class Data:
        _struct = ('<Bif', 9)
        def __init__(self, t):
            (self.uses,
            self.value,
            self.weight) = t

    MODL: Modl # Model
    FULL: str # Item Name
    DATA: Data # Sigil Stone Data
    SCRI: RefX[SCPTRecord] = None # Script (optional)
    EFITs: list[ENCHRecord.Efit] = [] # Effect Data
    SCITs: list[ENCHRecord.Scit] = [] # Script Effect Data
    def __init__(self): super().__init__(); self.EFITs = listx(); self.SCITs = listx()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.MODL: z = self.MODL = Modl(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODB(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODT(r, dataSize)
            case FieldType.ICON: z = self.MODL.ICON(r, dataSize)
            case FieldType.FULL: z = self.FULL = r.readFUString(dataSize) if len(self.SCITs) == 0 else self.SCITs.last().FULL(r, dataSize)
            case FieldType.DATA: z = self.DATA = r.readS(SGSTRecord.Data, dataSize)
            case FieldType.SCRI: z = self.SCRI = RefX[SCPTRecord](SCPTRecord, r, dataSize)
            case FieldType.EFID: z = r.skip(dataSize)
            case FieldType.EFIT: z = self.EFITs.addX(ENCHRecord.Efit(r, dataSize))
            case FieldType.SCIT: z = self.SCITs.addX(ENCHRecord.Scit(r, dataSize))
            case _: z = Record._empty
        return z
# end::SGST[]

# SKIL.Skill - 3450 - tag::SKIL[]
class SKILRecord(Record):
    class Data:
        def __init__(self, r: Reader, dataSize: int):
            self.action: int = 0 if r.format == FormType.TES3 else r.readInt32()
            self.attribute: int = r.readInt32()
            self.specialization: int = r.readUInt32() # 0 = Combat, 1 = Magic, 2 = Stealth
            self.useValue: list[float] = r.readPArray(None, 'f', 4 if r.format == FormType.TES3 else 2) # The use types for each skill are hard-coded

    def __repr__(self): return f'SKIL: {self.INDX.value}:{self.EDID.value}'
    INDX: int # Skill ID
    DATA: Data # Skill Data
    DESC: str # Skill description
    # TES4
    ICON: str # Icon
    ANAM: str # Apprentice Text
    JNAM: str # Journeyman Text
    ENAM: str # Expert Text
    MNAM: str # Master Text
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.INDX: z = self.INDX = r.readInt32()
            case FieldType.DATA | FieldType.SKDT: z = self.DATA = SKILRecord.Data(r, dataSize)
            case FieldType.DESC: z = self.DESC = r.readFUString(dataSize)
            case FieldType.ICON: z = self.ICON = r.readFUString(dataSize)
            case FieldType.ANAM: z = self.ANAM = r.readFUString(dataSize)
            case FieldType.JNAM: z = self.JNAM = r.readFUString(dataSize)
            case FieldType.ENAM: z = self.ENAM = r.readFUString(dataSize)
            case FieldType.MNAM: z = self.MNAM = r.readFUString(dataSize)
            case _: z = Record._empty
        return z
# end::SKIL[]

# SLGM.Soul Gem - 0450 - tag::SLGM[]
class SLGMRecord(Record, IHaveMODL):
    class Data:
        _struct = ('<if', 8)
        def __init__(self, t):
            (self.value,
            self.weight) = t

    MODL: Modl # Model
    FULL: str # Item Name
    SCRI: RefX[SCPTRecord] # Script (optional)
    DATA: Data # Type of soul contained in the gem
    SOUL: int # Type of soul contained in the gem
    SLCP: int # Soul gem maximum capacity
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.MODL: z = self.MODL = Modl(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODB(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODT(r, dataSize)
            case FieldType.ICON: z = self.MODL.ICON(r, dataSize)
            case FieldType.FULL: z = self.FULL = r.readFUString(dataSize)
            case FieldType.SCRI: z = self.SCRI = RefX[SCPTRecord](SCPTRecord, r, dataSize)
            case FieldType.DATA: z = self.DATA = r.readS(SLGMRecord.Data, dataSize)
            case FieldType.SOUL: z = self.SOUL = r.readByte()
            case FieldType.SLCP: z = self.SLCP = r.readByte()
            case _: z = Record._empty
        return z
# end::SLGM[]

# SNDG.Sound Generator - 3000 - tag::SNDG[]
class SNDGRecord(Record):
    class SNDGType(Enum): LeftFoot = 0; RightFoot = 1; SwimLeft = 2; SwimRight = 3; Moan = 4; Roar = 5; Scream = 6; Land = 7
    DATA: int # Sound Type Data
    SNAM: str # Sound ID
    CNAM: str = None # Creature name (optional)
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        if r.format == FormType.TES3:
            match type:
                case FieldType.NAME: z = self.EDID = r.readFUString(dataSize)
                case FieldType.DATA: z = self.DATA = r.readInt32()
                case FieldType.SNAM: z = self.SNAM = r.readFUString(dataSize)
                case FieldType.CNAM: z = self.CNAM = r.readFUString(dataSize)
                case _: z = Record._empty
            return z
        return None
# end::SNDG[]

# SNDR.Sound Reference - 0050 - tag::SNDR[]
class SNDRRecord(Record):
    CNAM: ByteColor4 # RGB color
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.CNAM: z = self.CNAM = r.readS(ByteColor4, dataSize)
            case _: z = Record._empty
        return z
# end::SNDR[]

# SOUN.Sound - 3450 - tag::SOUN[]
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
    
    class Data:
        _struct = { 2: '<2B', 8: '<2BbBI', 12: '<2BbBIH2B', 36: '<2BbBIH2B6hiQ' }
        minRange: int # Minimum attenuation distance
        maxRange: int # Maximum attenuation distance
        # TES4
        frequencyAdjustment: int # Frequency adjustment %
        unused: int # Unused
        flags: int # Flags #TODO might need to clip was ushort/ushort
        # 12
        staticAttenuation: int # Static Attenuation (db)
        stopTime: int # Stop time
        startTime: int # Start time
        # 36
        attenuationPoint1: int # The first point on the attenuation curve.
        attenuationPoint2: int # The second point on the attenuation curve.
        attenuationPoint3: int # The third point on the attenuation curve.
        attenuationPoint4: int # The fourth point on the attenuation curve.
        attenuationPoint5: int # The fifth point on the attenuation curve.
        reverbAttenuationControl: int
        priority: int
        unknown: int
        def __init__(self, t):
            match len(t):
                case 2:
                    (self.minRange,
                    self.maxRange) = t
                case 5:
                    (self.minRange,
                    self.maxRange,
                    self.frequencyAdjustment,
                    self.unused,
                    self.flags) = t
                case 8:
                    (self.minRange,
                    self.maxRange,
                    self.frequencyAdjustment,
                    self.unused,
                    self.flags,
                    self.staticAttenuation,
                    self.stopTime,
                    self.startTime) = t
                case 16:
                    (self.minRange,
                    self.maxRange,
                    self.frequencyAdjustment,
                    self.unused,
                    self.flags,
                    self.staticAttenuation,
                    self.stopTime,
                    self.startTime,
                    self.attenuationPoint1,
                    self.attenuationPoint2,
                    self.attenuationPoint3,
                    self.attenuationPoint4,
                    self.attenuationPoint5,
                    self.reverbAttenuationControl,
                    self.priority,
                    self.unknown) = t

    FULL: str # Sound Filename (relative to Sounds\)
    OBND: Obnd # Object Boundary
    DATA: Data # Sound Data
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readFUString(dataSize)
            case FieldType.FNAM: z = self.FULL = r.readFUString(dataSize)
            case FieldType.OBND: z = self.OBND = r.readS(Obnd, dataSize)
            case FieldType.DATA | FieldType.SNDX | FieldType.SNDD: self.DATA_Volume = r.readByte() if r.format == FormType.TES3 else 0; z = self.DATA = r.readS(SOUNRecord.Data, dataSize - 1)
            case _: z = Record._empty
        return z
# end::SOUN[]

# SPEL.Spell - 3450 - tag::SPEL[]
class SPELRecord(Record):
    class Spit:
        def __repr__(self): return f'{self.type}'
        def __init__(self, r: Reader, dataSize: int):
            # TES3: 0 = Spell, 1 = Ability, 2 = Blight, 3 = Disease, 4 = Curse, 5 = Power
            # TES4: 0 = Spell, 1 = Disease, 2 = Power, 3 = Lesser Power, 4 = Ability, 5 = Poison
            self.type: int = r.readUInt32()
            self.spellCost: int = r.readInt32()
            self.flags: int = r.readUInt32() # 0x0001 = AutoCalc, 0x0002 = PC Start, 0x0004 = Always Succeeds
            # TES4
            SpellLevel: int = r.readInt32() if r.format != FormType.TES3 else 0

    FULL: str # Spell name
    SPIT: Spit # Spell data
    EFITs: list[ENCHRecord.Efit] = [] # Effect Data
    # TES4
    SCITs: list[ENCHRecord.Scit] = [] # Script effect data
    def __init__(self): super().__init__(); self.EFITs = listx(); self.SCITs = listx()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readFUString(dataSize)
            case FieldType.FULL if len(self.SCITs) == 0: z = self.FULL = r.readFUString(dataSize) #:matchif
            case FieldType.FULL if len(self.SCITs) != 0: z = self.SCITs.last().FULL(r, dataSize) #:matchif
            case FieldType.FNAM: z = self.FULL = r.readFUString(dataSize)
            case FieldType.SPIT | FieldType.SPDT: z = self.SPIT = SPELRecord.Spit(r, dataSize)
            case FieldType.EFID: z = r.skip(dataSize)
            case FieldType.EFIT | FieldType.ENAM: z = self.EFITs.addX(ENCHRecord.Efit(r, dataSize))
            case FieldType.SCIT: z = self.SCITs.addX(ENCHRecord.Scit(r, dataSize))
            case _: z = Record._empty
        return z
# end::SPEL[]

# SSCR.Start Script - 3000 - tag::XXXX[]
class SSCRRecord(Record):
    DATA: str # Digits
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        if r.format == FormType.TES3:
            match type:
                case FieldType.NAME: z = self.EDID = r.readFUString(dataSize),
                case FieldType.DATA: z = self.DATA = r.readFUString(dataSize),
                case _: z = Record._empty
            return z
        return None
# end::SSCR[]

# STAT.Static - 3450 - tag::STAT[]
class STATRecord(Record, IHaveMODL):
    MODL: Modl # Model
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readFUString(dataSize)
            case FieldType.MODL: z = self.MODL = Modl(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODB(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODT(r, dataSize)
            case _: z = Record._empty
        return z
# end::STAT[]

# STDT.xx - 000S0 - tag::STDT[]
class STDTRecord(Record):
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case _: z = Record._empty
        return z
# end::STDT[]

# SUNP.xx - 000S0 - tag::SUNP[]
class SUNPRecord(Record):
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case _: z = Record._empty
        return z
# end::SUNP[]

# TES3.Plugin Info - 3000 - tag::TES3[]
class TES3Record(Record):
    class Hedr:
        _struct = ('<fI32s256sI', 300)
        def __init__(self, t):
            (self.version,
            self.fileType,
            self.companyName,
            self.fileDescription,
            self.numRecords) = t
            self.companyName = self.companyName.decode('ascii')
            self.fileDescription = self.fileDescription.decode('ascii')

    HEDR: Hedr
    MASTs: list[str]
    DATAs: list[int]
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.HEDR: z = self.HEDR = r.readS(TES3Record.Hedr, dataSize)
            case FieldType.MAST: z = _nca(self, 'MASTs', listx()).addX(r.readFUString(dataSize))
            case FieldType.DATA: z = _nca(self, 'DATAs', listx()).addX(r.readINTV(dataSize))
            case _: z = Record._empty
        return z
# end::TES3[]

# TES4.Plugin Info - 0450 - tag::TES4[]
class TES4Record(Record):
    class Hedr:
        _struct = ('<fiI', 12)
        def __init__(self, t):
            (self.version,
            self.numRecords, # Number of records and groups (not including TES4 record itself).
            self.nextObjectId) = t # Next available object ID.

    HEDR: Hedr
    CNAM: str = None # author (Optional)
    SNAM: str = None # description (Optional)
    MASTs: list[str] # master
    DATAs: list[int] # fileSize
    ONAM: bytes = None # overrides (Optional)
    INTV: int # unknown
    INCC: int = None # unknown (Optional)
    # TES5
    TNAM: bytes = None # overrides (Optional)
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.HEDR: z = self.HEDR = r.readS(TES4Record.Hedr, dataSize)
            case FieldType.OFST: z = r.skip(dataSize)
            case FieldType.DELE: z = r.skip(dataSize)
            case FieldType.CNAM: z = self.CNAM = r.readFUString(dataSize)
            case FieldType.SNAM: z = self.SNAM = r.readFUString(dataSize)
            case FieldType.MAST: z = _nca(self, 'MASTs', listx()).addX(r.readFUString(dataSize))
            case FieldType.DATA: z = _nca(self, 'DATAs', listx()).addX(r.readINTV(dataSize))
            case FieldType.ONAM: z = self.ONAM = r.readBytes(dataSize)
            case FieldType.INTV: z = self.INTV = r.readInt32()
            case FieldType.INCC: z = self.INCC = r.readInt32()
            # TES5
            case FieldType.TNAM: z = self.TNAM = r.readBytes(dataSize)
            case _: z = Record._empty
        return z
# end::TES4[]

# TERM.Computer Terminals - 000S0 - tag::TERM[]
class TERMRecord(Record):
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case _: z = Record._empty
        return z
# end::TERM[]

# TMLM.Terminal Menus - 000S0 - tag::TMLM[]
class TMLMRecord(Record):
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case _: z = Record._empty
        return z
# end::TMLM[]

# TREE.Tree - 0450 - tag::TREE[]
class TREERecord(Record, IHaveMODL):
    class Snam:
        def __init__(self, r: Reader, dataSize: int):
            self.values: list[int] = r.readPArray(None, 'i', dataSize >> 2)

    class Cnam:
        _struct = ('<5fi2f', 32)
        def __init__(self, t):
            (self.leafCurvature,
            self.minimumLeafAngle,
            self.maximumLeafAngle,
            self.branchDimmingValue,
            self.leafDimmingValue,
            self.shadowRadius,
            self.rockSpeed,
            self.rustleSpeed) = t

    class Bnam:
        _struct = ('<2f', 8)
        def __init__(self, t):
            (self.width,
            self.height) = t

    MODL: Modl # Model
    SNAM: Snam # SpeedTree Seeds, array of ints
    CNAM: Cnam # Tree Parameters
    BNAM: Bnam # Billboard Dimensions
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.MODL: z = self.MODL = Modl(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODB(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODT(r, dataSize)
            case FieldType.ICON: z = self.MODL.ICON(r, dataSize)
            case FieldType.SNAM: z = self.SNAM = TREERecord.Snam(r, dataSize)
            case FieldType.CNAM: z = self.CNAM = r.readS(TREERecord.Cnam, dataSize)
            case FieldType.BNAM: z = self.BNAM = r.readS(TREERecord.Bnam, dataSize)
            case _: z = Record._empty
        return z
# end::TREE[]

# dep: None
# TRNS.TRNS Record - 0400 #F4 - tag::TRNS[]
class TRNSRecord(Record):
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case _: z = Record._empty
        return z
# end::TRNS[]

# TXST.Texture Set - 0450 #F4 - tag::TXST[]
class TXSTRecord(Record):
    class DnamFlag(Flag):
        NotHasSpecularMap = 0x01 # not Has specular map
        FacegenTextures = 0x02 # Facegen Textures
        HasModelSpaceNormalMap = 0x04 # Has model space normal map

    class Dodt:
        class Flag(Flag):
            Parallax = 0x01 # Parallax (enables the Scale and Passes values in the CK)
            AlphaBlending = 0x02 # Alpha Blending
            AlphaTesting = 0x04 # Alpha Testing
            Not4Subtextures = 0x08 # not 4 Subtextures
        _struct = ('<7f8B', 36)
        def __init__(self, t):
            unknown = self.unknown = [None]*2
            color = self.color = ByteColor4()
            (self.minWidth,         # Min Width
            self.maxWidth,          # Max Width
            self.minHeight,         # Min Height
            self.maxHeight,         # Max Height
            self.depth,             # Depth
            self.shininess,         # Shininess
            self.parallaxScale,     # Parallax Scale
            self.parallaxPasses,    # Parallax Passes
            self.flags,             # Flags
            unknown[0], unknown[1], # Unknown but not neverused
            color.r, color.g, color.b, color.a) = t # Color

    OBND: Obnd # Object Boundary
    TX00: str # Texture path, color map
    TX01: str # Texture path, normal map (tangent- or model-space)
    TX02: str # Texture path, mask (environment or light)
    TX03: str # Texture path, tone map (for skins) or glow map (for things)
    TX04: str # Texture path, detail map (roughness, complexion, age)
    TX05: str # Texture path, environment map (cubemaps mostly)
    TX06: str # Texture path Multilayer (does not occur in Skyrim.esm)
    TX07: str # Texture path, specularity map (for skinny bodies, and for furry bodies)
    DODT: Dodt # Decal Data
    DNAM: int # Flags
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.OBND: z = self.OBND = r.readS(Obnd, dataSize)
            case FieldType.TX00: z = self.TX00 = r.readFUString(dataSize)
            case FieldType.TX01: z = self.TX01 = r.readFUString(dataSize)
            case FieldType.TX02: z = self.TX02 = r.readFUString(dataSize)
            case FieldType.TX03: z = self.TX03 = r.readFUString(dataSize)
            case FieldType.TX04: z = self.TX04 = r.readFUString(dataSize)
            case FieldType.TX05: z = self.TX05 = r.readFUString(dataSize)
            case FieldType.TX06: z = self.TX06 = r.readFUString(dataSize)
            case FieldType.TX07: z = self.TX07 = r.readFUString(dataSize)
            case FieldType.DODT: z = self.DODT = r.readS(TXSTRecord.Dodt, dataSize)
            case FieldType.DNAM: z = self.DNAM = r.readUInt16()
            case _: z = Record._empty
        return z
# end::TXST[]

# WATR.Water Type - 0450 - tag::WATR[]
class WATRRecord(Record):
    class Data:
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
        def __init__(self, r: Reader, dataSize: int):
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
            self.shallowColor = r.readS(ByteColor4)
            self.deepColor = r.readS(ByteColor4)
            self.reflectionColor = r.readS(ByteColor4)
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

    class Gnam:
        def __init__(self, r: Reader, dataSize: int):
            self.daytime: RefX[WATRRecord] = RefX[WATRRecord](WATRRecord, r.readUInt32())
            self.nighttime: RefX[WATRRecord] = RefX[WATRRecord](WATRRecord, r.readUInt32())
            self.underwater: RefX[WATRRecord] = RefX[WATRRecord](WATRRecord, r.readUInt32())

    TNAM: str # Texture
    ANAM: int # Opacity
    FNAM: int # Flags
    MNAM: str # Material ID
    SNAM: RefX[SOUNRecord] # Sound
    DATA: Data # DATA
    GNAM: Gnam # GNAM
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.TNAM: z = self.TNAM = r.readFUString(dataSize)
            case FieldType.ANAM: z = self.ANAM = r.readByte()
            case FieldType.FNAM: z = self.FNAM = r.readByte()
            case FieldType.MNAM: z = self.MNAM = r.readFUString(dataSize)
            case FieldType.SNAM: z = self.SNAM = RefX[SOUNRecord](SOUNRecord, r, dataSize)
            case FieldType.DATA: z = self.DATA = WATRRecord.Data(r, dataSize)
            case FieldType.GNAM: z = self.GNAM = WATRRecord.Gnam(r, dataSize)
            case _: z = Record._empty
        return z
# end::WATR[]

# WEAP.Weapon - 3450 - tag::WEAP[]
class WEAPRecord(Record, IHaveMODL):
    class Data:
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
        def __init__(self, r: Reader, dataSize: int):
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

    MODL: Modl # Model
    FULL: str # Item Name
    DATA: Data # Weapon Data
    ENAM: RefX[ENCHRecord] # Enchantment ID
    SCRI: RefX[SCPTRecord] # Script (optional)
    # TES4
    ANAM: int = None # Enchantment points (optional)
    def __init__(self): super().__init__()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID | FieldType.NAME: z = self.EDID = r.readFUString(dataSize)
            case FieldType.MODL: z = self.MODL = Modl(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODB(r, dataSize)
            case FieldType.MODT: z = self.MODL.MODT(r, dataSize)
            case FieldType.ICON | FieldType.ITEX: z = self.MODL.ICON(r, dataSize)
            case FieldType.FULL | FieldType.FNAM: z = self.FULL = r.readFUString(dataSize)
            case FieldType.DATA | FieldType.WPDT: z = self.DATA = WEAPRecord.Data(r, dataSize)
            case FieldType.ENAM: z = self.ENAM = RefX[ENCHRecord](ENCHRecord, r, dataSize)
            case FieldType.SCRI: z = self.SCRI = RefX[SCPTRecord](SCPTRecord, r, dataSize)
            case FieldType.ANAM: z = self.ANAM = r.readInt16()
            case _: z = Record._empty
        return z
# end::WEAP[]

# WRLD.Worldspace - 0450 - tag::WRLD[]
class WRLDRecord(Record):
    class Mnam:
        _struct = ('<2i4h', 16)
        def __init__(self, t):
            usableDimensions = self.usableDimensions = Int2()
            (usableDimensions.x, usableDimensions.y,
            # Cell Coordinates
            self.nwCell_X,
            self.nwCell_Y,
            self.seCell_X,
            self.seCell_Y) = t

    class Nam0:
        def __init__(self, r: Reader, dataSize: int):
            self.min: Vector2 = r.readVector2()
            self.max: Vector2 = array([0]*2)
        def NAM9(self, r: Reader, dataSize: int) -> object: self.max = r.readVector2(); return 0

    # TES5
    class Rnam:
        class Reference:
            _struct = ('<I2h', 16)
            def __init__(self, t):
                (self.ref,
                self.x,
                self.y) = t
                self.ref: Ref[REFRRecord] = Ref[REFRRecord](REFRRecord, self.ref)
        def __init__(self, r: Reader, dataSize: int):
            self.gridX: int = r.readInt16()
            self.gridY: int = r.readInt16()
            self.gridReferences: list[Reference] = r.readL32SArray(Reference, referenceSize >> 3)
            assert((dataSize - 8) >> 3 == len(self.gridReferences))

    FULL: str
    WNAM: RefX['WRLDRecord'] = None # Parent Worldspace
    CNAM: RefX[CLMTRecord] = None # Climate
    NAM2: RefX[WATRRecord] = None # Water
    ICON: str = None # Icon
    MNAM: Mnam = None # Map Data
    DATA: int = None # Flags
    NAM0: Nam0 # Object Bounds
    SNAM: int = None # Music
    # TES5
    RNAMs: list[Rnam] = [] # Large References
    def __init__(self): super().__init__(); self.RNAMs = listx()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.FULL: z = self.FULL = r.readFUString(dataSize)
            case FieldType.WNAM: z = self.WNAM = RefX[WRLDRecord](WRLDRecord, r, dataSize)
            case FieldType.CNAM: z = self.CNAM = RefX[CLMTRecord](CLMTRecord, r, dataSize)
            case FieldType.NAM2: z = self.NAM2 = RefX[WATRRecord](WATRRecord, r, dataSize)
            case FieldType.ICON: z = self.ICON = r.readFUString(dataSize)
            case FieldType.MNAM: z = self.MNAM = r.readS(WRLDRecord.Mnam, dataSize)
            case FieldType.DATA: z = self.DATA = r.readByte()
            case FieldType.NAM0: z = self.NAM0 = WRLDRecord.Nam0(r, dataSize)
            case FieldType.NAM9: z = self.NAM0.NAM9(r, dataSize)
            case FieldType.SNAM: z = self.SNAM = r.readUInt32()
            case FieldType.OFST: z = r.skip(dataSize)
            # TES5
            case FieldType.RNAM: z = self.RNAMs.addX(WRLDRecord.Rnam(r, dataSize))
            case _: z = Record._empty
        return z
# end::WRLD[]

# WTHR.Weather - 0450 - tag::WTHR[]
class WTHRRecord(Record, IHaveMODL):
    class Fnam:
        _struct = ('<4f', 16)
        def __init__(self, t):
            (self.dayNear,
            self.dayFar,
            self.nightNear,
            self.nightFar) = t

    class Hnam:
        _struct = ('<14f', 56)
        def __init__(self, t):
            (self.eyeAdaptSpeed,
            self.blurRadius,
            self.blurPasses,
            self.emissiveMult,
            self.targetLUM,
            self.upperLUMClamp,
            self.brightScale,
            self.brightClamp,
            self.lumRampNoTex,
            self.lumRampMin,
            self.lumRampMax,
            self.sunlightDimmer,
            self.grassDimmer,
            self.treeDimmer) = t

    class Data:
        _struct = ('<15B', 15)
        def __init__(self, t):
            lightningColor = self.lightningColor = ByteColor4()
            (self.windSpeed,
            self.cloudSpeed_Lower,
            self.cloudSpeed_Upper,
            self.transDelta,
            self.sunGlare,
            self.sunDamage,
            self.precipitation_BeginFadeIn,
            self.precipitation_EndFadeOut,
            self.thunderLightning_BeginFadeIn,
            self.thunderLightning_EndFadeOut,
            self.thunderLightning_Frequency,
            self.weatherClassification,
            lightningColor.x, lightningColor.y, lightningColor.z) = t
        def fill(self) -> None: self.lightningColor.a = 255

    class Snam:
        def __init__(self, r: Reader, dataSize: int):
            self.sound: RefX[SOUNRecord] = RefX[SOUNRecord](SOUNRecord, r.readUInt32()) # Sound FormId
            self.type: int = r.readUInt32() # Sound Type - 0=Default, 1=Precipitation, 2=Wind, 3=Thunder

    MODL: Modl # Model
    CNAM: str # Lower Cloud Layer
    DNAM: str # Upper Cloud Layer
    NAM0: bytes # Colors by Types/Times
    FNAM: Fnam # Fog Distance
    HNAM: Hnam # HDR Data
    DATA: Data # Weather Data
    SNAMs: list[Snam] = [] # Sounds
    def __init__(self): super().__init__(); self.SNAMs = listx()

    def readField(self, r: Reader, type: FieldType, dataSize: int) -> object:
        match type:
            case FieldType.EDID: z = self.EDID = r.readFUString(dataSize)
            case FieldType.MODL: z = self.MODL = Modl(r, dataSize)
            case FieldType.MODB: z = self.MODL.MODB(r, dataSize)
            case FieldType.CNAM: z = self.CNAM = r.readFUString(dataSize)
            case FieldType.DNAM: z = self.DNAM = r.readFUString(dataSize)
            case FieldType.NAM0: z = self.NAM0 = r.readBytes(dataSize)
            case FieldType.FNAM: z = self.FNAM = r.readS(WTHRRecord.Fnam, dataSize)
            case FieldType.HNAM: z = self.HNAM = r.readS(WTHRRecord.Hnam, dataSize)
            case FieldType.DATA: z = self.DATA = r.readS(WTHRRecord.Data, dataSize); self.DATA.fill()
            case FieldType.SNAM: z = self.SNAMs.addX(WTHRRecord.Snam(r, dataSize))
            case _: z = Record._empty
        return z
# end::WTHR[]

#endregion

#region Backfill

CONTRecord.SNAM = RefX[SOUNRecord](SOUNRecord)
CONTRecord.QNAM = RefX[SOUNRecord](SOUNRecord)

#endregion
