// PORT-SOURCE: Families/GameX.Bethesda/Formats/Records.cs
// PORT-SHA: bdd7cd1643b0ef97
// PORT-STATUS: done
// PORT-GENERATED: gen_enums.py — do not hand-edit; regenerate instead.
//
// 66 enum(s), 1300 members, generated from the C#
// rather than transcribed. At this size a one-digit typo would mis-identify a
// game asset in a way no test would obviously catch.
//
// C# allows duplicate discriminants within an enum; Rust does not. Where that
// happens the first member becomes the variant and the rest become associated
// consts pointing at it, so every C# name still resolves.
//
// Per enum:
//   FormType                           u32   enum        295 members
//   FieldType                          u32   enum        317 members
//   ActorValue                         i32   enum         73 members
//   KeywordType                        i32   enum         85 members
//   RecordEsmFlags                     u32   bitflags     23 members
//   RecordGroupGroupType               u32   enum         11 members
//   BmdtBipedFlag                      u32   bitflags     20 members
//   BmdtGeneralFlag                    u8    bitflags      8 members
//   ModlModdFlag                       i32   bitflags      4 members
//   DestDestFlag                       u8    bitflags      1 members
//   DestDstdFlag                       u8    bitflags      3 members
//   EfidType_                          i32   enum          3 members
//   CtdaINFOType                       u8    enum         13 members
//   VmadScriptStatus                   u8    bitflags      3 members
//   VmadPropertyStatus                 u8    enum          2 members
//   VmadPropertyType                   u8    enum         15 members
//   AACTRecordFlag                     i32   enum         11 members
//   AAPDRecordFlag                     i32   enum         11 members
//   ACHRRecordFlag                     i32   enum          7 members
//   DataFlag                           u8    bitflags      2 members
//   EnamRange_                         u32   enum          3 members
//   DataFlag2                          u32   bitflags      1 members
//   DataType_                          u8    enum          4 members
//   DataARMOType                       i32   enum         11 members
//   ARMORecordNodeFlag                 u32   bitflags     32 members
//   ARMORecordFlag                     u8    bitflags      2 members
//   ARMORecordSkillType                u32   enum          3 members
//   ARTORecordDnamFlag                 u32   bitflags      3 members
//   ASPCRecordAnam                     u32   enum         31 members
//   ASTPRecordDataFlag                 u32   bitflags      1 members
//   BODYRecordPart                     u8    enum         15 members
//   BODYRecordFlag                     u8    bitflags      2 members
//   BODYRecordPartType                 u8    enum          3 members
//   BOOKRecordFlag                     u8    bitflags      2 members
//   BpndFlag                           u8    bitflags      7 members
//   BpndPartType_                      u8    enum          6 members
//   CELLRecordFlag                     u16   bitflags      9 members
//   DataSpecialization_                u32   enum          3 members
//   DataFlag3                          u32   bitflags      2 members
//   DataService                        u32   bitflags     18 members
//   DataType_2                         u32   enum         10 members
//   CREARecordFlag                     u32   bitflags     11 members
//   CREA3RecordAIFlags                 u32   bitflags     18 members
//   DIALRecordType3                    u8    enum          5 members
//   DIALRecordType4                    u8    enum          8 members
//   EnitType3                          i32   enum          4 members
//   EnitType4                          i32   enum          4 members
//   EnitFlag                           i32   bitflags      1 members
//   FadtFlag                           u32   bitflags      1 members
//   PlvdSpecType                       u32   enum          6 members
//   DataColorFlags                     i32   bitflags      9 members
//   MGEFRecordMFEGFlag                 u32   bitflags     32 members
//   NPC_3RecordNPC_3Flags              u32   bitflags      7 members
//   NPC_4RecordNPC_4Flags              u32   bitflags     10 members
//   RACERecordDataFlag                 u32   enum         32 members
//   RACE4RecordFaceIndx                u32   enum          9 members
//   RACE4RecordBodyIndx                u32   enum          5 members
//   XtelFlag                           u32   bitflags      3 members
//   REGNRecordREGNType                 u8    enum          8 members
//   SNDGRecordSNDGType                 u32   enum          8 members
//   SOUNRecordFlag                     u16   bitflags     13 members
//   TERMRecordDnamDifficulty           u8    enum          6 members
//   TERMRecordDnamFlag                 u8    bitflags      4 members
//   TXSTRecordDnamFlag                 u16   bitflags      3 members
//   TXSTRecordFlag                     u8    bitflags      4 members
//   DataWEAPType                       i32   enum         14 members

#![allow(non_camel_case_types, non_upper_case_globals)]

/// C# `enum FormType : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum FormType {
    AACT = 0x54434141,
    AAPD = 0x44504141,
    AAMD = 0x444d4141,
    ACHR = 0x52484341,
    ACRE = 0x45524341,
    ACTI = 0x49544341,
    ADDN = 0x4e444441,
    AECH = 0x48434541,
    AFFE = 0x45464641,
    ALCH = 0x48434c41,
    ALOC = 0x434f4c41,
    AMBS = 0x53424d41,
    AMDL = 0x4c444d41,
    AMEF = 0x46454d41,
    AMMO = 0x4f4d4d41,
    ANIO = 0x4f494e41,
    AOPF = 0x46504f41,
    AOPS = 0x53504f41,
    AORU = 0x55524f41,
    APPA = 0x41505041,
    ARMA = 0x414d5241,
    ARMO = 0x4f4d5241,
    ARTO = 0x4f545241,
    ASPC = 0x43505341,
    ASTM = 0x4d545341,
    ASTP = 0x50545341,
    ATMO = 0x4f4d5441,
    ATXO = 0x4f585441,
    AUVF = 0x46565541,
    AVIF = 0x46495641,
    AVMD = 0x444d5641,
    AVTR = 0x52545641,
    BIOM = 0x4d4f4942,
    BMOD = 0x444f4d42,
    BNDS = 0x53444e42,
    BODY = 0x59444f42,
    BOOK = 0x4b4f4f42,
    BPTD = 0x44545042,
    BSGN = 0x4e475342,
    CAMS = 0x534d4143,
    CCRD = 0x44524343,
    CDCK = 0x4b434443,
    CELL = 0x4c4c4543,
    CHAL = 0x4c414843,
    CHIP = 0x50494843,
    CLAS = 0x53414c43,
    CLDC = 0x43444c43,
    CLDF = 0x46444c43,
    CLFM = 0x4d464c43,
    CLMT = 0x544d4c43,
    CLOT = 0x544f4c43,
    CMNY = 0x594e4d43,
    CMPO = 0x4f504d43,
    CNCY = 0x59434e43,
    CNDF = 0x46444e43,
    COBJ = 0x4a424f43,
    COEN = 0x4e454f43,
    COLL = 0x4c4c4f43,
    CONT = 0x544e4f43,
    CPRD = 0x44525043,
    CPTH = 0x48545043,
    CREA = 0x41455243,
    CSEN = 0x4e455343,
    CSNO = 0x4f4e5343,
    CSTY = 0x59545343,
    CUR3 = 0x33525543,
    CURV = 0x56525543,
    DCGF = 0x46474344,
    DEBR = 0x52424544,
    DEHY = 0x59484544,
    DFOB = 0x424f4644,
    DIAL = 0x4c414944,
    DIST = 0x54534944,
    DLBR = 0x52424c44,
    DLVW = 0x57564c44,
    DOBJ = 0x4a424f44,
    DMGT = 0x54474d44,
    DOOR = 0x524f4f44,
    DUAL = 0x4c415544,
    ECAT = 0x54414345,
    ECZN = 0x4e5a4345,
    EFSH = 0x48534645,
    EFSQ = 0x51534645,
    EMOT = 0x544f4d45,
    ENCH = 0x48434e45,
    ENTM = 0x4d544e45,
    EQUP = 0x50555145,
    EQWG = 0x47575145,
    EXPL = 0x4c505845,
    EYES = 0x53455945,
    FACT = 0x54434146,
    FFKW = 0x574b4646,
    FISH = 0x48534946,
    FLOR = 0x524f4c46,
    FLST = 0x54534c46,
    FOGV = 0x56474f46,
    FORC = 0x43524f46,
    FSTP = 0x50545346,
    FSTS = 0x53545346,
    FURN = 0x4e525546,
    FXPD = 0x44505846,
    GBFT = 0x54464247,
    GBFM = 0x4d464247,
    GCVR = 0x52564347,
    GDRY = 0x59524447,
    GLOB = 0x424f4c47,
    GMRW = 0x57524d47,
    GMST = 0x54534d47,
    GPOF = 0x464f5047,
    GPOG = 0x474f5047,
    GRAS = 0x53415247,
    GRUP = 0x50555247,
    HAIR = 0x52494148,
    HAZD = 0x445a4148,
    HDPT = 0x54504448,
    HUNG = 0x474e5548,
    IDLE = 0x454c4449,
    IDLM = 0x4d4c4449,
    IMAD = 0x44414d49,
    IMGS = 0x53474d49,
    IMOD = 0x444f4d49,
    INFO = 0x4f464e49,
    INGR = 0x52474e49,
    IPCT = 0x54435049,
    IPDS = 0x53445049,
    IRES = 0x53455249,
    KEYM = 0x4d59454b,
    KSSM = 0x4d53534b,
    KYWD = 0x4457594b,
    LAND = 0x444e414c,
    LAYR = 0x5259414c,
    LCRT = 0x5452434c,
    LCTN = 0x4e54434c,
    LEVC = 0x4356454c,
    LEVI = 0x4956454c,
    LGDI = 0x4944474c,
    LGTM = 0x4d54474c,
    LIGH = 0x4847494c,
    LMSW = 0x57534d4c,
    LOCK = 0x4b434f4c,
    LOUT = 0x54554f4c,
    LSCR = 0x5243534c,
    LSCT = 0x5443534c,
    LSPR = 0x5250534c,
    LTEX = 0x5845544c,
    LVLB = 0x424c564c,
    LVLC = 0x434c564c,
    LVLI = 0x494c564c,
    LVLN = 0x4e4c564c,
    LVLP = 0x504c564c,
    LVPC = 0x4350564c,
    LVSC = 0x4353564c,
    LVSP = 0x5053564c,
    MAAM = 0x4d41414d,
    MATO = 0x4f54414d,
    MATT = 0x5454414d,
    MDSP = 0x5053444d,
    MESG = 0x4753454d,
    MGEF = 0x4645474d,
    MICN = 0x4e43494d,
    MISC = 0x4353494d,
    MOVT = 0x54564f4d,
    MRPH = 0x4850524d,
    MSCS = 0x5343534d,
    MSET = 0x5445534d,
    MSTT = 0x5454534d,
    MSWP = 0x5057534d,
    MTPT = 0x5450544d,
    MUSC = 0x4353554d,
    MUST = 0x5453554d,
    NAVI = 0x4956414e,
    NAVM = 0x4d56414e,
    NOCM = 0x4d434f4e,
    NOTE = 0x45544f4e,
    NPC_ = 0x5f43504e,
    OMOD = 0x444f4d4f,
    OSWP = 0x5057534f,
    OTFT = 0x5446544f,
    OVIS = 0x5349564f,
    PACH = 0x48434150,
    PACK = 0x4b434150,
    PARW = 0x57524150,
    PBAR = 0x52414250,
    PBEA = 0x41454250,
    PCBN = 0x4e424350,
    PCCN = 0x4e434350,
    PCMT = 0x544d4350,
    PCON = 0x4e4f4350,
    PCRD = 0x44524350,
    PDCL = 0x4c434450,
    PEPF = 0x46504550,
    PERK = 0x4b524550,
    PERS = 0x53524550,
    PFLA = 0x414c4650,
    PGRD = 0x44524750,
    PGRE = 0x45524750,
    PHZD = 0x445a4850,
    PKIN = 0x4e494b50,
    PLYR = 0x52594c50,
    PLYT = 0x54594c50,
    PMFT = 0x54464d50,
    PMIS = 0x53494d50,
    PNDT = 0x54444e50,
    PPAK = 0x4b415050,
    PROB = 0x424f5250,
    PROJ = 0x4a4f5250,
    PSDC = 0x43445350,
    PTST = 0x54535450,
    PWAT = 0x54415750,
    QMDL = 0x4c444d51,
    QUST = 0x54535551,
    RACE = 0x45434152,
    RADS = 0x53444152,
    RCCT = 0x54434352,
    RCPE = 0x45504352,
    REFR = 0x52464552,
    REGN = 0x4e474552,
    RELA = 0x414c4552,
    REPA = 0x41504552,
    REPU = 0x55504552,
    RESO = 0x4f534552,
    REVB = 0x42564552,
    RFCT = 0x54434652,
    RFGP = 0x50474652,
    RGDL = 0x4c444752,
    ROAD = 0x44414f52,
    RSGD = 0x44475352,
    RSPJ = 0x4a505352,
    SBSP = 0x50534253,
    SCCO = 0x4f434353,
    SCEN = 0x4e454353,
    SCOL = 0x4c4f4353,
    SCPT = 0x54504353,
    SCRL = 0x4c524353,
    SCSN = 0x4e534353,
    SDLT = 0x544c4453,
    SECH = 0x48434553,
    SFBK = 0x4b424653,
    SFPC = 0x43504653,
    SFPT = 0x54504653,
    SFTR = 0x52544653,
    SGST = 0x54534753,
    SHOU = 0x554f4853,
    SKIL = 0x4c494b53,
    SLGM = 0x4d474c53,
    SLPD = 0x44504c53,
    SMBN = 0x4e424d53,
    SMEN = 0x4e454d53,
    SMQN = 0x4e514d53,
    SNCT = 0x54434e53,
    SNDG = 0x47444e53,
    SNDR = 0x52444e53,
    SOPM = 0x4d504f53,
    SOUN = 0x4e554f53,
    SPCH = 0x48435053,
    SPEL = 0x4c455053,
    SPGD = 0x44475053,
    SSCR = 0x52435353,
    STAG = 0x47415453,
    STAT = 0x54415453,
    STBH = 0x48425453,
    STDT = 0x54445453,
    STHD = 0x44485453,
    STMP = 0x504d5453,
    STND = 0x444e5453,
    SUNP = 0x504e5553,
    TACT = 0x54434154,
    TES3 = 0x33534554,
    TES4 = 0x34534554,
    TES5 = 0x35534554,
    TES6 = 0x36534554,
    TERM = 0x4d524554,
    TLOD = 0x444f4c54,
    TMLM = 0x4d4c4d54,
    TODD = 0x44444f54,
    TOFT = 0x54464f54,
    TRAV = 0x56415254,
    TREE = 0x45455254,
    TRNS = 0x534e5254,
    TXST = 0x54535854,
    UTIL = 0x4c495455,
    VTYP = 0x50595456,
    VOLI = 0x494c4f56,
    WATR = 0x52544157,
    WAVE = 0x45564157,
    WBAR = 0x52414257,
    WEAP = 0x50414557,
    WKMF = 0x464d4b57,
    WOOP = 0x504f4f57,
    WRLD = 0x444c5257,
    WSPR = 0x52505357,
    WTHR = 0x52485457,
    WTHS = 0x53485457,
    WWED = 0x44455757,
    ZOOM = 0x4d4f4f5a,
}

impl FormType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x54434141 => Self::AACT,
            0x44504141 => Self::AAPD,
            0x444d4141 => Self::AAMD,
            0x52484341 => Self::ACHR,
            0x45524341 => Self::ACRE,
            0x49544341 => Self::ACTI,
            0x4e444441 => Self::ADDN,
            0x48434541 => Self::AECH,
            0x45464641 => Self::AFFE,
            0x48434c41 => Self::ALCH,
            0x434f4c41 => Self::ALOC,
            0x53424d41 => Self::AMBS,
            0x4c444d41 => Self::AMDL,
            0x46454d41 => Self::AMEF,
            0x4f4d4d41 => Self::AMMO,
            0x4f494e41 => Self::ANIO,
            0x46504f41 => Self::AOPF,
            0x53504f41 => Self::AOPS,
            0x55524f41 => Self::AORU,
            0x41505041 => Self::APPA,
            0x414d5241 => Self::ARMA,
            0x4f4d5241 => Self::ARMO,
            0x4f545241 => Self::ARTO,
            0x43505341 => Self::ASPC,
            0x4d545341 => Self::ASTM,
            0x50545341 => Self::ASTP,
            0x4f4d5441 => Self::ATMO,
            0x4f585441 => Self::ATXO,
            0x46565541 => Self::AUVF,
            0x46495641 => Self::AVIF,
            0x444d5641 => Self::AVMD,
            0x52545641 => Self::AVTR,
            0x4d4f4942 => Self::BIOM,
            0x444f4d42 => Self::BMOD,
            0x53444e42 => Self::BNDS,
            0x59444f42 => Self::BODY,
            0x4b4f4f42 => Self::BOOK,
            0x44545042 => Self::BPTD,
            0x4e475342 => Self::BSGN,
            0x534d4143 => Self::CAMS,
            0x44524343 => Self::CCRD,
            0x4b434443 => Self::CDCK,
            0x4c4c4543 => Self::CELL,
            0x4c414843 => Self::CHAL,
            0x50494843 => Self::CHIP,
            0x53414c43 => Self::CLAS,
            0x43444c43 => Self::CLDC,
            0x46444c43 => Self::CLDF,
            0x4d464c43 => Self::CLFM,
            0x544d4c43 => Self::CLMT,
            0x544f4c43 => Self::CLOT,
            0x594e4d43 => Self::CMNY,
            0x4f504d43 => Self::CMPO,
            0x59434e43 => Self::CNCY,
            0x46444e43 => Self::CNDF,
            0x4a424f43 => Self::COBJ,
            0x4e454f43 => Self::COEN,
            0x4c4c4f43 => Self::COLL,
            0x544e4f43 => Self::CONT,
            0x44525043 => Self::CPRD,
            0x48545043 => Self::CPTH,
            0x41455243 => Self::CREA,
            0x4e455343 => Self::CSEN,
            0x4f4e5343 => Self::CSNO,
            0x59545343 => Self::CSTY,
            0x33525543 => Self::CUR3,
            0x56525543 => Self::CURV,
            0x46474344 => Self::DCGF,
            0x52424544 => Self::DEBR,
            0x59484544 => Self::DEHY,
            0x424f4644 => Self::DFOB,
            0x4c414944 => Self::DIAL,
            0x54534944 => Self::DIST,
            0x52424c44 => Self::DLBR,
            0x57564c44 => Self::DLVW,
            0x4a424f44 => Self::DOBJ,
            0x54474d44 => Self::DMGT,
            0x524f4f44 => Self::DOOR,
            0x4c415544 => Self::DUAL,
            0x54414345 => Self::ECAT,
            0x4e5a4345 => Self::ECZN,
            0x48534645 => Self::EFSH,
            0x51534645 => Self::EFSQ,
            0x544f4d45 => Self::EMOT,
            0x48434e45 => Self::ENCH,
            0x4d544e45 => Self::ENTM,
            0x50555145 => Self::EQUP,
            0x47575145 => Self::EQWG,
            0x4c505845 => Self::EXPL,
            0x53455945 => Self::EYES,
            0x54434146 => Self::FACT,
            0x574b4646 => Self::FFKW,
            0x48534946 => Self::FISH,
            0x524f4c46 => Self::FLOR,
            0x54534c46 => Self::FLST,
            0x56474f46 => Self::FOGV,
            0x43524f46 => Self::FORC,
            0x50545346 => Self::FSTP,
            0x53545346 => Self::FSTS,
            0x4e525546 => Self::FURN,
            0x44505846 => Self::FXPD,
            0x54464247 => Self::GBFT,
            0x4d464247 => Self::GBFM,
            0x52564347 => Self::GCVR,
            0x59524447 => Self::GDRY,
            0x424f4c47 => Self::GLOB,
            0x57524d47 => Self::GMRW,
            0x54534d47 => Self::GMST,
            0x464f5047 => Self::GPOF,
            0x474f5047 => Self::GPOG,
            0x53415247 => Self::GRAS,
            0x50555247 => Self::GRUP,
            0x52494148 => Self::HAIR,
            0x445a4148 => Self::HAZD,
            0x54504448 => Self::HDPT,
            0x474e5548 => Self::HUNG,
            0x454c4449 => Self::IDLE,
            0x4d4c4449 => Self::IDLM,
            0x44414d49 => Self::IMAD,
            0x53474d49 => Self::IMGS,
            0x444f4d49 => Self::IMOD,
            0x4f464e49 => Self::INFO,
            0x52474e49 => Self::INGR,
            0x54435049 => Self::IPCT,
            0x53445049 => Self::IPDS,
            0x53455249 => Self::IRES,
            0x4d59454b => Self::KEYM,
            0x4d53534b => Self::KSSM,
            0x4457594b => Self::KYWD,
            0x444e414c => Self::LAND,
            0x5259414c => Self::LAYR,
            0x5452434c => Self::LCRT,
            0x4e54434c => Self::LCTN,
            0x4356454c => Self::LEVC,
            0x4956454c => Self::LEVI,
            0x4944474c => Self::LGDI,
            0x4d54474c => Self::LGTM,
            0x4847494c => Self::LIGH,
            0x57534d4c => Self::LMSW,
            0x4b434f4c => Self::LOCK,
            0x54554f4c => Self::LOUT,
            0x5243534c => Self::LSCR,
            0x5443534c => Self::LSCT,
            0x5250534c => Self::LSPR,
            0x5845544c => Self::LTEX,
            0x424c564c => Self::LVLB,
            0x434c564c => Self::LVLC,
            0x494c564c => Self::LVLI,
            0x4e4c564c => Self::LVLN,
            0x504c564c => Self::LVLP,
            0x4350564c => Self::LVPC,
            0x4353564c => Self::LVSC,
            0x5053564c => Self::LVSP,
            0x4d41414d => Self::MAAM,
            0x4f54414d => Self::MATO,
            0x5454414d => Self::MATT,
            0x5053444d => Self::MDSP,
            0x4753454d => Self::MESG,
            0x4645474d => Self::MGEF,
            0x4e43494d => Self::MICN,
            0x4353494d => Self::MISC,
            0x54564f4d => Self::MOVT,
            0x4850524d => Self::MRPH,
            0x5343534d => Self::MSCS,
            0x5445534d => Self::MSET,
            0x5454534d => Self::MSTT,
            0x5057534d => Self::MSWP,
            0x5450544d => Self::MTPT,
            0x4353554d => Self::MUSC,
            0x5453554d => Self::MUST,
            0x4956414e => Self::NAVI,
            0x4d56414e => Self::NAVM,
            0x4d434f4e => Self::NOCM,
            0x45544f4e => Self::NOTE,
            0x5f43504e => Self::NPC_,
            0x444f4d4f => Self::OMOD,
            0x5057534f => Self::OSWP,
            0x5446544f => Self::OTFT,
            0x5349564f => Self::OVIS,
            0x48434150 => Self::PACH,
            0x4b434150 => Self::PACK,
            0x57524150 => Self::PARW,
            0x52414250 => Self::PBAR,
            0x41454250 => Self::PBEA,
            0x4e424350 => Self::PCBN,
            0x4e434350 => Self::PCCN,
            0x544d4350 => Self::PCMT,
            0x4e4f4350 => Self::PCON,
            0x44524350 => Self::PCRD,
            0x4c434450 => Self::PDCL,
            0x46504550 => Self::PEPF,
            0x4b524550 => Self::PERK,
            0x53524550 => Self::PERS,
            0x414c4650 => Self::PFLA,
            0x44524750 => Self::PGRD,
            0x45524750 => Self::PGRE,
            0x445a4850 => Self::PHZD,
            0x4e494b50 => Self::PKIN,
            0x52594c50 => Self::PLYR,
            0x54594c50 => Self::PLYT,
            0x54464d50 => Self::PMFT,
            0x53494d50 => Self::PMIS,
            0x54444e50 => Self::PNDT,
            0x4b415050 => Self::PPAK,
            0x424f5250 => Self::PROB,
            0x4a4f5250 => Self::PROJ,
            0x43445350 => Self::PSDC,
            0x54535450 => Self::PTST,
            0x54415750 => Self::PWAT,
            0x4c444d51 => Self::QMDL,
            0x54535551 => Self::QUST,
            0x45434152 => Self::RACE,
            0x53444152 => Self::RADS,
            0x54434352 => Self::RCCT,
            0x45504352 => Self::RCPE,
            0x52464552 => Self::REFR,
            0x4e474552 => Self::REGN,
            0x414c4552 => Self::RELA,
            0x41504552 => Self::REPA,
            0x55504552 => Self::REPU,
            0x4f534552 => Self::RESO,
            0x42564552 => Self::REVB,
            0x54434652 => Self::RFCT,
            0x50474652 => Self::RFGP,
            0x4c444752 => Self::RGDL,
            0x44414f52 => Self::ROAD,
            0x44475352 => Self::RSGD,
            0x4a505352 => Self::RSPJ,
            0x50534253 => Self::SBSP,
            0x4f434353 => Self::SCCO,
            0x4e454353 => Self::SCEN,
            0x4c4f4353 => Self::SCOL,
            0x54504353 => Self::SCPT,
            0x4c524353 => Self::SCRL,
            0x4e534353 => Self::SCSN,
            0x544c4453 => Self::SDLT,
            0x48434553 => Self::SECH,
            0x4b424653 => Self::SFBK,
            0x43504653 => Self::SFPC,
            0x54504653 => Self::SFPT,
            0x52544653 => Self::SFTR,
            0x54534753 => Self::SGST,
            0x554f4853 => Self::SHOU,
            0x4c494b53 => Self::SKIL,
            0x4d474c53 => Self::SLGM,
            0x44504c53 => Self::SLPD,
            0x4e424d53 => Self::SMBN,
            0x4e454d53 => Self::SMEN,
            0x4e514d53 => Self::SMQN,
            0x54434e53 => Self::SNCT,
            0x47444e53 => Self::SNDG,
            0x52444e53 => Self::SNDR,
            0x4d504f53 => Self::SOPM,
            0x4e554f53 => Self::SOUN,
            0x48435053 => Self::SPCH,
            0x4c455053 => Self::SPEL,
            0x44475053 => Self::SPGD,
            0x52435353 => Self::SSCR,
            0x47415453 => Self::STAG,
            0x54415453 => Self::STAT,
            0x48425453 => Self::STBH,
            0x54445453 => Self::STDT,
            0x44485453 => Self::STHD,
            0x504d5453 => Self::STMP,
            0x444e5453 => Self::STND,
            0x504e5553 => Self::SUNP,
            0x54434154 => Self::TACT,
            0x33534554 => Self::TES3,
            0x34534554 => Self::TES4,
            0x35534554 => Self::TES5,
            0x36534554 => Self::TES6,
            0x4d524554 => Self::TERM,
            0x444f4c54 => Self::TLOD,
            0x4d4c4d54 => Self::TMLM,
            0x44444f54 => Self::TODD,
            0x54464f54 => Self::TOFT,
            0x56415254 => Self::TRAV,
            0x45455254 => Self::TREE,
            0x534e5254 => Self::TRNS,
            0x54535854 => Self::TXST,
            0x4c495455 => Self::UTIL,
            0x50595456 => Self::VTYP,
            0x494c4f56 => Self::VOLI,
            0x52544157 => Self::WATR,
            0x45564157 => Self::WAVE,
            0x52414257 => Self::WBAR,
            0x50414557 => Self::WEAP,
            0x464d4b57 => Self::WKMF,
            0x504f4f57 => Self::WOOP,
            0x444c5257 => Self::WRLD,
            0x52505357 => Self::WSPR,
            0x52485457 => Self::WTHR,
            0x53485457 => Self::WTHS,
            0x44455757 => Self::WWED,
            0x4d4f4f5a => Self::ZOOM,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

/// C# `enum FieldType : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum FieldType {
    AADT = 0x54444141,
    ACBS = 0x53424341,
    AHCF = 0x46434841,
    AHCM = 0x4d434841,
    AIDT = 0x54444941,
    AI_A = 0x415f4941,
    AI_E = 0x455f4941,
    AI_F = 0x465f4941,
    AI_T = 0x545f4941,
    AI_W = 0x575f4941,
    ALDT = 0x54444c41,
    AMBI = 0x49424d41,
    ANAM = 0x4d414e41,
    AODT = 0x54444f41,
    ASND = 0x444e5341,
    ATKD = 0x444b5441,
    ATKE = 0x454b5441,
    ATTR = 0x52545441,
    ATXT = 0x54585441,
    AVFX = 0x58465641,
    BIPL = 0x4c504942,
    BKDT = 0x54444b42,
    BMCT = 0x54434d42,
    BMDT = 0x54444d42,
    BNAM = 0x4d414e42,
    BODT = 0x54444f42,
    BPND = 0x444e5042,
    BPNI = 0x494e5042,
    BPNN = 0x4e4e5042,
    BPNT = 0x544e5042,
    BPTN = 0x4e545042,
    BSND = 0x444e5342,
    BTXT = 0x54585442,
    BVFX = 0x58465642,
    BYDT = 0x54445942,
    CIS2 = 0x32534943,
    CITC = 0x43544943,
    CLDT = 0x54444c43,
    CNAM = 0x4d414e43,
    CNDT = 0x54444e43,
    CNTO = 0x4f544e43,
    COED = 0x44454f43,
    CRGR = 0x52475243,
    CRVA = 0x41565243,
    CSAD = 0x44415343,
    CSCR = 0x52435343,
    CSDI = 0x49445343,
    CSDC = 0x43445343,
    CSDT = 0x54445343,
    CSND = 0x444e5343,
    CSTD = 0x44545343,
    CTDA = 0x41445443,
    CTDT = 0x54445443,
    CVFX = 0x58465643,
    DATA = 0x41544144,
    DELE = 0x454c4544,
    DESC = 0x43534544,
    DEST = 0x54534544,
    DFTF = 0x46544644,
    DFTM = 0x4d544644,
    DMDL = 0x4c444d44,
    DMDT = 0x54444d44,
    DNAM = 0x4d414e44,
    DODT = 0x54444f44,
    DSTD = 0x44545344,
    DSTF = 0x46545344,
    EDID = 0x44494445,
    EFID = 0x44494645,
    EFIT = 0x54494645,
    EITM = 0x4d544945,
    ENAM = 0x4d414e45,
    ENDT = 0x54444e45,
    ENIT = 0x54494e45,
    ESCE = 0x45435345,
    ETYP = 0x50595445,
    FADT = 0x54444146,
    FCHT = 0x54484346,
    FGGA = 0x41474746,
    FGGS = 0x53474746,
    FGTS = 0x53544746,
    FLAG = 0x47414c46,
    FLMV = 0x564d4c46,
    FLTV = 0x56544c46,
    FNAM = 0x4d414e46,
    FPRT = 0x54525046,
    FRMR = 0x524d5246,
    FTSF = 0x46535446,
    FTSM = 0x4d535446,
    FULL = 0x4c4c5546,
    GNAM = 0x4d414e47,
    HCLF = 0x464c4348,
    HCLR = 0x524c4348,
    HEAD = 0x44414548,
    HEDR = 0x52444548,
    HNAM = 0x4d414e48,
    HSND = 0x444e5348,
    HVFX = 0x58465648,
    ICON = 0x4e4f4349,
    ICO2 = 0x324f4349,
    INAM = 0x4d414e49,
    INCC = 0x43434e49,
    INDX = 0x58444e49,
    INTV = 0x56544e49,
    IRDT = 0x54445249,
    ITEX = 0x58455449,
    ITXT = 0x54585449,
    JAIL = 0x4c49414a,
    JNAM = 0x4d414e4a,
    JOUT = 0x54554f4a,
    KFFZ = 0x5a46464b,
    KNAM = 0x4d414e4b,
    KSIZ = 0x5a49534b,
    KWDA = 0x4144574b,
    LHDT = 0x5444484c,
    LKDT = 0x54444b4c,
    LNAM = 0x4d414e4c,
    LVLD = 0x444c564c,
    LVLF = 0x464c564c,
    LVLO = 0x4f4c564c,
    MAST = 0x5453414d,
    MCDT = 0x5444434d,
    MCHT = 0x5448434d,
    MEDT = 0x5444454d,
    MIC2 = 0x3243494d,
    MICO = 0x4f43494d,
    MNAM = 0x4d414e4d,
    MO2B = 0x42324f4d,
    MO2S = 0x53324f4d,
    MO2T = 0x54324f4d,
    MO3B = 0x42334f4d,
    MO3S = 0x53334f4d,
    MO3T = 0x54334f4d,
    MO4B = 0x42344f4d,
    MO4S = 0x53344f4d,
    MO4T = 0x54344f4d,
    MOD2 = 0x32444f4d,
    MOD3 = 0x33444f4d,
    MOD4 = 0x34444f4d,
    MODB = 0x42444f4d,
    MODD = 0x44444f4d,
    MODL = 0x4c444f4d,
    MODS = 0x53444f4d,
    MODT = 0x54444f4d,
    MOSD = 0x44534f4d,
    MPAI = 0x4941504d,
    MPAV = 0x5641504d,
    MPRT = 0x5452504d,
    MTNM = 0x4d4e544d,
    MTYP = 0x5059544d,
    NAM0 = 0x304d414e,
    NAM1 = 0x314d414e,
    NAM2 = 0x324d414e,
    NAM3 = 0x334d414e,
    NAM4 = 0x344d414e,
    NAM5 = 0x354d414e,
    NAM7 = 0x374d414e,
    NAM8 = 0x384d414e,
    NAM9 = 0x394d414e,
    NAME = 0x454d414e,
    NIFT = 0x5446494e,
    NIFZ = 0x5a46494e,
    NNAM = 0x4d414e4e,
    NPCO = 0x4f43504e,
    NPCS = 0x5343504e,
    NPDT = 0x5444504e,
    OBND = 0x444e424f,
    OFST = 0x5453464f,
    ONAM = 0x4d414e4f,
    PBDT = 0x54444250,
    PFIG = 0x47494650,
    PFPC = 0x43504650,
    PGAG = 0x47414750,
    PGRC = 0x43524750,
    PGRI = 0x49524750,
    PGRL = 0x4c524750,
    PGRP = 0x50524750,
    PGRR = 0x52524750,
    PHTN = 0x4e544850,
    PHWT = 0x54574850,
    PKDT = 0x54444b50,
    PKID = 0x44494b50,
    PLCN = 0x4e434c50,
    PLDT = 0x54444c50,
    PLVD = 0x44564c50,
    PNAM = 0x4d414e50,
    PSDT = 0x54445350,
    PTDT = 0x54445450,
    PTEX = 0x58455450,
    QNAM = 0x4d414e51,
    QSDT = 0x54445351,
    QSTA = 0x41545351,
    QSTF = 0x46545351,
    QSTI = 0x49545351,
    QSTN = 0x4e545351,
    QSTR = 0x52545351,
    RADT = 0x54444152,
    RAGA = 0x41474152,
    RCLR = 0x524c4352,
    RDAT = 0x54414452,
    RDGS = 0x53474452,
    RDMD = 0x444d4452,
    RDMP = 0x504d4452,
    RDOT = 0x544f4452,
    RDSD = 0x44534452,
    RDWT = 0x54574452,
    REPL = 0x4c504552,
    RGNN = 0x4e4e4752,
    RIDT = 0x54444952,
    RNAM = 0x4d414e52,
    RNMV = 0x564d4e52,
    RPLD = 0x444c5052,
    RPLI = 0x494c5052,
    RPRF = 0x46525052,
    RPRM = 0x4d525052,
    SCDA = 0x41444353,
    SCDT = 0x54444353,
    SCHD = 0x44484353,
    SCHR = 0x52484353,
    SCIT = 0x54494353,
    SCPT = 0x54504353,
    SCRI = 0x49524353,
    SCRO = 0x4f524353,
    SCRV = 0x56524353,
    SCTX = 0x58544353,
    SCVR = 0x52564353,
    SDSC = 0x43534453,
    SKDT = 0x54444b53,
    SLCP = 0x50434c53,
    SLSD = 0x44534c53,
    SNAM = 0x4d414e53,
    SNDD = 0x44444e53,
    SNDX = 0x58444e53,
    SNMV = 0x564d4e53,
    SOUL = 0x4c554f53,
    SPCT = 0x54435053,
    SPDT = 0x54445053,
    SPED = 0x44455053,
    SPIT = 0x54495053,
    SPLO = 0x4f4c5053,
    STOL = 0x4c4f5453,
    STRV = 0x56525453,
    SWMV = 0x564d5753,
    TCLF = 0x464c4354,
    TCLT = 0x544c4354,
    TEXT = 0x54584554,
    TNAM = 0x4d414e54,
    TINC = 0x434e4954,
    TIND = 0x444e4954,
    TINI = 0x494e4954,
    TINL = 0x4c4e4954,
    TINP = 0x504e4954,
    TINT = 0x544e4954,
    TINV = 0x564e4954,
    TIRS = 0x53524954,
    TPIC = 0x43495054,
    TRDT = 0x54445254,
    TX00 = 0x30305854,
    TX01 = 0x31305854,
    TX02 = 0x32305854,
    TX03 = 0x33305854,
    TX04 = 0x34305854,
    TX05 = 0x35305854,
    TX06 = 0x36305854,
    TX07 = 0x37305854,
    UNAM = 0x4d414e55,
    UNES = 0x53454e55,
    VCLR = 0x524c4356,
    VENC = 0x434e4556,
    VEND = 0x444e4556,
    VENV = 0x564e4556,
    VHGT = 0x54474856,
    VMAD = 0x44414d56,
    VNAM = 0x4d414e56,
    VNML = 0x4c4d4e56,
    VTCK = 0x4b435456,
    VTEX = 0x58455456,
    VTXT = 0x54585456,
    WAIT = 0x54494157,
    WEAT = 0x54414557,
    WHGT = 0x54474857,
    WKMV = 0x564d4b57,
    WLST = 0x54534c57,
    WNAM = 0x4d414e57,
    WPDT = 0x54445057,
    XACT = 0x54434158,
    XCCM = 0x4d434358,
    XCHG = 0x47484358,
    XCNT = 0x544e4358,
    XCLC = 0x434c4358,
    XCLL = 0x4c4c4358,
    XCLR = 0x524c4358,
    XCLW = 0x574c4358,
    XCMT = 0x544d4358,
    XCWT = 0x54574358,
    XESP = 0x50534558,
    XGLB = 0x424c4758,
    XHLT = 0x544c4858,
    XHRS = 0x53524858,
    XLCM = 0x4d434c58,
    XLOC = 0x434f4c58,
    XLOD = 0x444f4c58,
    XMRC = 0x43524d58,
    XMRK = 0x4b524d58,
    XNAM = 0x4d414e58,
    XOWN = 0x4e574f58,
    XPCI = 0x49435058,
    XRGD = 0x44475258,
    XRNK = 0x4b4e5258,
    XRTM = 0x4d545258,
    XSCL = 0x4c435358,
    XSED = 0x44455358,
    XSOL = 0x4c4f5358,
    XTEL = 0x4c455458,
    XTRG = 0x47525458,
    XXXX = 0x58585858,
    YNAM = 0x4d414e59,
    ZNAM = 0x4d414e5a,
}

impl FieldType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x54444141 => Self::AADT,
            0x53424341 => Self::ACBS,
            0x46434841 => Self::AHCF,
            0x4d434841 => Self::AHCM,
            0x54444941 => Self::AIDT,
            0x415f4941 => Self::AI_A,
            0x455f4941 => Self::AI_E,
            0x465f4941 => Self::AI_F,
            0x545f4941 => Self::AI_T,
            0x575f4941 => Self::AI_W,
            0x54444c41 => Self::ALDT,
            0x49424d41 => Self::AMBI,
            0x4d414e41 => Self::ANAM,
            0x54444f41 => Self::AODT,
            0x444e5341 => Self::ASND,
            0x444b5441 => Self::ATKD,
            0x454b5441 => Self::ATKE,
            0x52545441 => Self::ATTR,
            0x54585441 => Self::ATXT,
            0x58465641 => Self::AVFX,
            0x4c504942 => Self::BIPL,
            0x54444b42 => Self::BKDT,
            0x54434d42 => Self::BMCT,
            0x54444d42 => Self::BMDT,
            0x4d414e42 => Self::BNAM,
            0x54444f42 => Self::BODT,
            0x444e5042 => Self::BPND,
            0x494e5042 => Self::BPNI,
            0x4e4e5042 => Self::BPNN,
            0x544e5042 => Self::BPNT,
            0x4e545042 => Self::BPTN,
            0x444e5342 => Self::BSND,
            0x54585442 => Self::BTXT,
            0x58465642 => Self::BVFX,
            0x54445942 => Self::BYDT,
            0x32534943 => Self::CIS2,
            0x43544943 => Self::CITC,
            0x54444c43 => Self::CLDT,
            0x4d414e43 => Self::CNAM,
            0x54444e43 => Self::CNDT,
            0x4f544e43 => Self::CNTO,
            0x44454f43 => Self::COED,
            0x52475243 => Self::CRGR,
            0x41565243 => Self::CRVA,
            0x44415343 => Self::CSAD,
            0x52435343 => Self::CSCR,
            0x49445343 => Self::CSDI,
            0x43445343 => Self::CSDC,
            0x54445343 => Self::CSDT,
            0x444e5343 => Self::CSND,
            0x44545343 => Self::CSTD,
            0x41445443 => Self::CTDA,
            0x54445443 => Self::CTDT,
            0x58465643 => Self::CVFX,
            0x41544144 => Self::DATA,
            0x454c4544 => Self::DELE,
            0x43534544 => Self::DESC,
            0x54534544 => Self::DEST,
            0x46544644 => Self::DFTF,
            0x4d544644 => Self::DFTM,
            0x4c444d44 => Self::DMDL,
            0x54444d44 => Self::DMDT,
            0x4d414e44 => Self::DNAM,
            0x54444f44 => Self::DODT,
            0x44545344 => Self::DSTD,
            0x46545344 => Self::DSTF,
            0x44494445 => Self::EDID,
            0x44494645 => Self::EFID,
            0x54494645 => Self::EFIT,
            0x4d544945 => Self::EITM,
            0x4d414e45 => Self::ENAM,
            0x54444e45 => Self::ENDT,
            0x54494e45 => Self::ENIT,
            0x45435345 => Self::ESCE,
            0x50595445 => Self::ETYP,
            0x54444146 => Self::FADT,
            0x54484346 => Self::FCHT,
            0x41474746 => Self::FGGA,
            0x53474746 => Self::FGGS,
            0x53544746 => Self::FGTS,
            0x47414c46 => Self::FLAG,
            0x564d4c46 => Self::FLMV,
            0x56544c46 => Self::FLTV,
            0x4d414e46 => Self::FNAM,
            0x54525046 => Self::FPRT,
            0x524d5246 => Self::FRMR,
            0x46535446 => Self::FTSF,
            0x4d535446 => Self::FTSM,
            0x4c4c5546 => Self::FULL,
            0x4d414e47 => Self::GNAM,
            0x464c4348 => Self::HCLF,
            0x524c4348 => Self::HCLR,
            0x44414548 => Self::HEAD,
            0x52444548 => Self::HEDR,
            0x4d414e48 => Self::HNAM,
            0x444e5348 => Self::HSND,
            0x58465648 => Self::HVFX,
            0x4e4f4349 => Self::ICON,
            0x324f4349 => Self::ICO2,
            0x4d414e49 => Self::INAM,
            0x43434e49 => Self::INCC,
            0x58444e49 => Self::INDX,
            0x56544e49 => Self::INTV,
            0x54445249 => Self::IRDT,
            0x58455449 => Self::ITEX,
            0x54585449 => Self::ITXT,
            0x4c49414a => Self::JAIL,
            0x4d414e4a => Self::JNAM,
            0x54554f4a => Self::JOUT,
            0x5a46464b => Self::KFFZ,
            0x4d414e4b => Self::KNAM,
            0x5a49534b => Self::KSIZ,
            0x4144574b => Self::KWDA,
            0x5444484c => Self::LHDT,
            0x54444b4c => Self::LKDT,
            0x4d414e4c => Self::LNAM,
            0x444c564c => Self::LVLD,
            0x464c564c => Self::LVLF,
            0x4f4c564c => Self::LVLO,
            0x5453414d => Self::MAST,
            0x5444434d => Self::MCDT,
            0x5448434d => Self::MCHT,
            0x5444454d => Self::MEDT,
            0x3243494d => Self::MIC2,
            0x4f43494d => Self::MICO,
            0x4d414e4d => Self::MNAM,
            0x42324f4d => Self::MO2B,
            0x53324f4d => Self::MO2S,
            0x54324f4d => Self::MO2T,
            0x42334f4d => Self::MO3B,
            0x53334f4d => Self::MO3S,
            0x54334f4d => Self::MO3T,
            0x42344f4d => Self::MO4B,
            0x53344f4d => Self::MO4S,
            0x54344f4d => Self::MO4T,
            0x32444f4d => Self::MOD2,
            0x33444f4d => Self::MOD3,
            0x34444f4d => Self::MOD4,
            0x42444f4d => Self::MODB,
            0x44444f4d => Self::MODD,
            0x4c444f4d => Self::MODL,
            0x53444f4d => Self::MODS,
            0x54444f4d => Self::MODT,
            0x44534f4d => Self::MOSD,
            0x4941504d => Self::MPAI,
            0x5641504d => Self::MPAV,
            0x5452504d => Self::MPRT,
            0x4d4e544d => Self::MTNM,
            0x5059544d => Self::MTYP,
            0x304d414e => Self::NAM0,
            0x314d414e => Self::NAM1,
            0x324d414e => Self::NAM2,
            0x334d414e => Self::NAM3,
            0x344d414e => Self::NAM4,
            0x354d414e => Self::NAM5,
            0x374d414e => Self::NAM7,
            0x384d414e => Self::NAM8,
            0x394d414e => Self::NAM9,
            0x454d414e => Self::NAME,
            0x5446494e => Self::NIFT,
            0x5a46494e => Self::NIFZ,
            0x4d414e4e => Self::NNAM,
            0x4f43504e => Self::NPCO,
            0x5343504e => Self::NPCS,
            0x5444504e => Self::NPDT,
            0x444e424f => Self::OBND,
            0x5453464f => Self::OFST,
            0x4d414e4f => Self::ONAM,
            0x54444250 => Self::PBDT,
            0x47494650 => Self::PFIG,
            0x43504650 => Self::PFPC,
            0x47414750 => Self::PGAG,
            0x43524750 => Self::PGRC,
            0x49524750 => Self::PGRI,
            0x4c524750 => Self::PGRL,
            0x50524750 => Self::PGRP,
            0x52524750 => Self::PGRR,
            0x4e544850 => Self::PHTN,
            0x54574850 => Self::PHWT,
            0x54444b50 => Self::PKDT,
            0x44494b50 => Self::PKID,
            0x4e434c50 => Self::PLCN,
            0x54444c50 => Self::PLDT,
            0x44564c50 => Self::PLVD,
            0x4d414e50 => Self::PNAM,
            0x54445350 => Self::PSDT,
            0x54445450 => Self::PTDT,
            0x58455450 => Self::PTEX,
            0x4d414e51 => Self::QNAM,
            0x54445351 => Self::QSDT,
            0x41545351 => Self::QSTA,
            0x46545351 => Self::QSTF,
            0x49545351 => Self::QSTI,
            0x4e545351 => Self::QSTN,
            0x52545351 => Self::QSTR,
            0x54444152 => Self::RADT,
            0x41474152 => Self::RAGA,
            0x524c4352 => Self::RCLR,
            0x54414452 => Self::RDAT,
            0x53474452 => Self::RDGS,
            0x444d4452 => Self::RDMD,
            0x504d4452 => Self::RDMP,
            0x544f4452 => Self::RDOT,
            0x44534452 => Self::RDSD,
            0x54574452 => Self::RDWT,
            0x4c504552 => Self::REPL,
            0x4e4e4752 => Self::RGNN,
            0x54444952 => Self::RIDT,
            0x4d414e52 => Self::RNAM,
            0x564d4e52 => Self::RNMV,
            0x444c5052 => Self::RPLD,
            0x494c5052 => Self::RPLI,
            0x46525052 => Self::RPRF,
            0x4d525052 => Self::RPRM,
            0x41444353 => Self::SCDA,
            0x54444353 => Self::SCDT,
            0x44484353 => Self::SCHD,
            0x52484353 => Self::SCHR,
            0x54494353 => Self::SCIT,
            0x54504353 => Self::SCPT,
            0x49524353 => Self::SCRI,
            0x4f524353 => Self::SCRO,
            0x56524353 => Self::SCRV,
            0x58544353 => Self::SCTX,
            0x52564353 => Self::SCVR,
            0x43534453 => Self::SDSC,
            0x54444b53 => Self::SKDT,
            0x50434c53 => Self::SLCP,
            0x44534c53 => Self::SLSD,
            0x4d414e53 => Self::SNAM,
            0x44444e53 => Self::SNDD,
            0x58444e53 => Self::SNDX,
            0x564d4e53 => Self::SNMV,
            0x4c554f53 => Self::SOUL,
            0x54435053 => Self::SPCT,
            0x54445053 => Self::SPDT,
            0x44455053 => Self::SPED,
            0x54495053 => Self::SPIT,
            0x4f4c5053 => Self::SPLO,
            0x4c4f5453 => Self::STOL,
            0x56525453 => Self::STRV,
            0x564d5753 => Self::SWMV,
            0x464c4354 => Self::TCLF,
            0x544c4354 => Self::TCLT,
            0x54584554 => Self::TEXT,
            0x4d414e54 => Self::TNAM,
            0x434e4954 => Self::TINC,
            0x444e4954 => Self::TIND,
            0x494e4954 => Self::TINI,
            0x4c4e4954 => Self::TINL,
            0x504e4954 => Self::TINP,
            0x544e4954 => Self::TINT,
            0x564e4954 => Self::TINV,
            0x53524954 => Self::TIRS,
            0x43495054 => Self::TPIC,
            0x54445254 => Self::TRDT,
            0x30305854 => Self::TX00,
            0x31305854 => Self::TX01,
            0x32305854 => Self::TX02,
            0x33305854 => Self::TX03,
            0x34305854 => Self::TX04,
            0x35305854 => Self::TX05,
            0x36305854 => Self::TX06,
            0x37305854 => Self::TX07,
            0x4d414e55 => Self::UNAM,
            0x53454e55 => Self::UNES,
            0x524c4356 => Self::VCLR,
            0x434e4556 => Self::VENC,
            0x444e4556 => Self::VEND,
            0x564e4556 => Self::VENV,
            0x54474856 => Self::VHGT,
            0x44414d56 => Self::VMAD,
            0x4d414e56 => Self::VNAM,
            0x4c4d4e56 => Self::VNML,
            0x4b435456 => Self::VTCK,
            0x58455456 => Self::VTEX,
            0x54585456 => Self::VTXT,
            0x54494157 => Self::WAIT,
            0x54414557 => Self::WEAT,
            0x54474857 => Self::WHGT,
            0x564d4b57 => Self::WKMV,
            0x54534c57 => Self::WLST,
            0x4d414e57 => Self::WNAM,
            0x54445057 => Self::WPDT,
            0x54434158 => Self::XACT,
            0x4d434358 => Self::XCCM,
            0x47484358 => Self::XCHG,
            0x544e4358 => Self::XCNT,
            0x434c4358 => Self::XCLC,
            0x4c4c4358 => Self::XCLL,
            0x524c4358 => Self::XCLR,
            0x574c4358 => Self::XCLW,
            0x544d4358 => Self::XCMT,
            0x54574358 => Self::XCWT,
            0x50534558 => Self::XESP,
            0x424c4758 => Self::XGLB,
            0x544c4858 => Self::XHLT,
            0x53524858 => Self::XHRS,
            0x4d434c58 => Self::XLCM,
            0x434f4c58 => Self::XLOC,
            0x444f4c58 => Self::XLOD,
            0x43524d58 => Self::XMRC,
            0x4b524d58 => Self::XMRK,
            0x4d414e58 => Self::XNAM,
            0x4e574f58 => Self::XOWN,
            0x49435058 => Self::XPCI,
            0x44475258 => Self::XRGD,
            0x4b4e5258 => Self::XRNK,
            0x4d545258 => Self::XRTM,
            0x4c435358 => Self::XSCL,
            0x44455358 => Self::XSED,
            0x4c4f5358 => Self::XSOL,
            0x4c455458 => Self::XTEL,
            0x47525458 => Self::XTRG,
            0x58585858 => Self::XXXX,
            0x4d414e59 => Self::YNAM,
            0x4d414e5a => Self::ZNAM,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

/// C# `enum ActorValue : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum ActorValue {
    None_ = -0x1,
    Strength = 0x0,
    Intelligence = 0x1,
    Willpower = 0x2,
    Agility = 0x3,
    Speed = 0x4,
    Endurance = 0x5,
    Personality = 0x6,
    Luck = 0x7,
    Health = 0x8,
    Magicka = 0x9,
    Fatigue = 0xa,
    Encumbrance = 0xb,
    Armorer = 0xc,
    Athletics = 0xd,
    Blade = 0xe,
    Block = 0xf,
    Blunt = 0x10,
    HandToHand = 0x11,
    HeavyArmor = 0x12,
    Alchemy = 0x13,
    Alteration = 0x14,
    Conjuration = 0x15,
    Destruction = 0x16,
    Illusion = 0x17,
    Mysticism = 0x18,
    Restoration = 0x19,
    Acrobatics = 0x1a,
    LightArmor = 0x1b,
    Marksman = 0x1c,
    Mercantile = 0x1d,
    Security = 0x1e,
    Sneak = 0x1f,
    Speechcraft = 0x20,
    Aggression = 0x21,
    Confidence = 0x22,
    Energy = 0x23,
    Responsibility = 0x24,
    Bounty = 0x25,
    Fame = 0x26,
    Infamy = 0x27,
    MagickaMultiplier = 0x28,
    NightEyeBonus = 0x29,
    AttackBonus = 0x2a,
    DefendBonus = 0x2b,
    CastingPenalty = 0x2c,
    Blindness = 0x2d,
    Chameleon = 0x2e,
    Invisibility = 0x2f,
    Paralysis = 0x30,
    Silence = 0x31,
    Confusion = 0x32,
    DetectItemRange = 0x33,
    SpellAbsorbChance = 0x34,
    SpellReflectChance = 0x35,
    SwimSpeedMultiplier = 0x36,
    WaterBreathing = 0x37,
    WaterWalking = 0x38,
    StuntedMagicka = 0x39,
    DetectLifeRange = 0x3a,
    ReflectDamage = 0x3b,
    Telekinesis = 0x3c,
    ResistFire = 0x3d,
    ResistFrost = 0x3e,
    ResistDisease = 0x3f,
    ResistMagic = 0x40,
    ResistNormalWeapons = 0x41,
    ResistParalysis = 0x42,
    ResistPoison = 0x43,
    ResistShock = 0x44,
    Vampirism = 0x45,
    Darkness = 0x46,
    ResistWaterDamage = 0x47,
}

impl ActorValue {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            -0x1 => Self::None_,
            0x0 => Self::Strength,
            0x1 => Self::Intelligence,
            0x2 => Self::Willpower,
            0x3 => Self::Agility,
            0x4 => Self::Speed,
            0x5 => Self::Endurance,
            0x6 => Self::Personality,
            0x7 => Self::Luck,
            0x8 => Self::Health,
            0x9 => Self::Magicka,
            0xa => Self::Fatigue,
            0xb => Self::Encumbrance,
            0xc => Self::Armorer,
            0xd => Self::Athletics,
            0xe => Self::Blade,
            0xf => Self::Block,
            0x10 => Self::Blunt,
            0x11 => Self::HandToHand,
            0x12 => Self::HeavyArmor,
            0x13 => Self::Alchemy,
            0x14 => Self::Alteration,
            0x15 => Self::Conjuration,
            0x16 => Self::Destruction,
            0x17 => Self::Illusion,
            0x18 => Self::Mysticism,
            0x19 => Self::Restoration,
            0x1a => Self::Acrobatics,
            0x1b => Self::LightArmor,
            0x1c => Self::Marksman,
            0x1d => Self::Mercantile,
            0x1e => Self::Security,
            0x1f => Self::Sneak,
            0x20 => Self::Speechcraft,
            0x21 => Self::Aggression,
            0x22 => Self::Confidence,
            0x23 => Self::Energy,
            0x24 => Self::Responsibility,
            0x25 => Self::Bounty,
            0x26 => Self::Fame,
            0x27 => Self::Infamy,
            0x28 => Self::MagickaMultiplier,
            0x29 => Self::NightEyeBonus,
            0x2a => Self::AttackBonus,
            0x2b => Self::DefendBonus,
            0x2c => Self::CastingPenalty,
            0x2d => Self::Blindness,
            0x2e => Self::Chameleon,
            0x2f => Self::Invisibility,
            0x30 => Self::Paralysis,
            0x31 => Self::Silence,
            0x32 => Self::Confusion,
            0x33 => Self::DetectItemRange,
            0x34 => Self::SpellAbsorbChance,
            0x35 => Self::SpellReflectChance,
            0x36 => Self::SwimSpeedMultiplier,
            0x37 => Self::WaterBreathing,
            0x38 => Self::WaterWalking,
            0x39 => Self::StuntedMagicka,
            0x3a => Self::DetectLifeRange,
            0x3b => Self::ReflectDamage,
            0x3c => Self::Telekinesis,
            0x3d => Self::ResistFire,
            0x3e => Self::ResistFrost,
            0x3f => Self::ResistDisease,
            0x40 => Self::ResistMagic,
            0x41 => Self::ResistNormalWeapons,
            0x42 => Self::ResistParalysis,
            0x43 => Self::ResistPoison,
            0x44 => Self::ResistShock,
            0x45 => Self::Vampirism,
            0x46 => Self::Darkness,
            0x47 => Self::ResistWaterDamage,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

/// C# `enum KeywordType : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum KeywordType {
    None_ = 0x0,
    ComponentTechLevel = 0x1,
    AttachPoint = 0x2,
    ComponentProperty = 0x3,
    InstantiationFilter = 0x4,
    ModAssociation = 0x5,
    Sound = 0x6,
    AnimArchetype = 0x7,
    FunctionCall = 0x8,
    RecipeFilter = 0x9,
    AttractionType = 0xa,
    DialogueSubtype = 0xb,
    QuestTarget = 0xc,
    AnimFlavor = 0xd,
    AnimGender = 0xe,
    AnimFace = 0xf,
    QuestGroup = 0x10,
    AnimInjured = 0x11,
    DispelEffect = 0x12,
    CrowdTarget = 0x13,
    ExclusiveLocationEncounterType = 0x14,
    WeaponHolster = 0x15,
    HUDMarkerOverride = 0x16,
    InteractionRootOffset = 0x17,
    MiscItemQuality = 0x18,
    ComponentQuantity = 0x19,
    QuestType = 0x1a,
    FactionType = 0x1b,
    Traversal = 0x1c,
    InventoryCategory = 0x1d,
    FormLink = 0x1e,
    Manufacturer = 0x1f,
    UIIconPersonalEffect = 0x20,
    UIIconEnvironmentEffect = 0x21,
    PrimitiveType = 0x22,
    PlanetType = 0x23,
    PlanetAtmosphereType = 0x24,
    PlanetAtmosphereToxicity = 0x25,
    PlanetGravityType = 0x26,
    PlanetWaterAbundance = 0x27,
    PlanetWaterQuality = 0x28,
    PlanetMagnetosphere = 0x29,
    PlanetFloraProbability = 0x2a,
    PlanetFaunaProbability = 0x2b,
    PlanetTraits = 0x2c,
    PlanetTemperatureType = 0x2d,
    PlanetPressureType = 0x2e,
    PlanetFloraAbundance = 0x2f,
    PlanetFaunaAbundance = 0x30,
    BiomeMarkerType = 0x31,
    HandScannerInfoType = 0x32,
    ShipModuleClass = 0x33,
    LayeredMaterialSwapKey = 0x34,
    UIIconLinkageName = 0x35,
    MissionType = 0x36,
    SoundEngine = 0x37,
    SoundEngineMod = 0x38,
    SoundCockpit = 0x39,
    SoundGravDrive = 0x3a,
    PerkTraitRestriction = 0x3b,
    SoundCCTSkin = 0x3c,
    SoundCCTSize = 0x3d,
    SoundCCTSpeed = 0x3e,
    PhotoModeCategory = 0x3f,
    ExcludeFromGI_Raytracing = 0x40,
    IncludeInGI_Raytracing = 0x41,
    HairColor = 0x42,
    FacialHairColor = 0x43,
    EyeColor = 0x44,
    BiomeHoudiniStyle = 0x45,
    AnimFlavor_AnimObject = 0x46,
    BrowColor = 0x47,
    HairSubtype = 0x48,
    FacialHairSubtype = 0x49,
    BrowSubtype = 0x4a,
    AVMSConditionSequence = 0x4b,
    BiomeCreatureType = 0x4c,
    ShipModuleUpgrade = 0x4d,
    DisplayName = 0x4e,
    AVMSAppearanceVariationMod = 0x4f,
    UIIconTreatment = 0x50,
    FormPair = 0x51,
    ItemDescription = 0x52,
    WeaponTypeDisplay = 0x53,
    AVMSConditionKeyword = 0x54,
}

impl KeywordType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::None_,
            0x1 => Self::ComponentTechLevel,
            0x2 => Self::AttachPoint,
            0x3 => Self::ComponentProperty,
            0x4 => Self::InstantiationFilter,
            0x5 => Self::ModAssociation,
            0x6 => Self::Sound,
            0x7 => Self::AnimArchetype,
            0x8 => Self::FunctionCall,
            0x9 => Self::RecipeFilter,
            0xa => Self::AttractionType,
            0xb => Self::DialogueSubtype,
            0xc => Self::QuestTarget,
            0xd => Self::AnimFlavor,
            0xe => Self::AnimGender,
            0xf => Self::AnimFace,
            0x10 => Self::QuestGroup,
            0x11 => Self::AnimInjured,
            0x12 => Self::DispelEffect,
            0x13 => Self::CrowdTarget,
            0x14 => Self::ExclusiveLocationEncounterType,
            0x15 => Self::WeaponHolster,
            0x16 => Self::HUDMarkerOverride,
            0x17 => Self::InteractionRootOffset,
            0x18 => Self::MiscItemQuality,
            0x19 => Self::ComponentQuantity,
            0x1a => Self::QuestType,
            0x1b => Self::FactionType,
            0x1c => Self::Traversal,
            0x1d => Self::InventoryCategory,
            0x1e => Self::FormLink,
            0x1f => Self::Manufacturer,
            0x20 => Self::UIIconPersonalEffect,
            0x21 => Self::UIIconEnvironmentEffect,
            0x22 => Self::PrimitiveType,
            0x23 => Self::PlanetType,
            0x24 => Self::PlanetAtmosphereType,
            0x25 => Self::PlanetAtmosphereToxicity,
            0x26 => Self::PlanetGravityType,
            0x27 => Self::PlanetWaterAbundance,
            0x28 => Self::PlanetWaterQuality,
            0x29 => Self::PlanetMagnetosphere,
            0x2a => Self::PlanetFloraProbability,
            0x2b => Self::PlanetFaunaProbability,
            0x2c => Self::PlanetTraits,
            0x2d => Self::PlanetTemperatureType,
            0x2e => Self::PlanetPressureType,
            0x2f => Self::PlanetFloraAbundance,
            0x30 => Self::PlanetFaunaAbundance,
            0x31 => Self::BiomeMarkerType,
            0x32 => Self::HandScannerInfoType,
            0x33 => Self::ShipModuleClass,
            0x34 => Self::LayeredMaterialSwapKey,
            0x35 => Self::UIIconLinkageName,
            0x36 => Self::MissionType,
            0x37 => Self::SoundEngine,
            0x38 => Self::SoundEngineMod,
            0x39 => Self::SoundCockpit,
            0x3a => Self::SoundGravDrive,
            0x3b => Self::PerkTraitRestriction,
            0x3c => Self::SoundCCTSkin,
            0x3d => Self::SoundCCTSize,
            0x3e => Self::SoundCCTSpeed,
            0x3f => Self::PhotoModeCategory,
            0x40 => Self::ExcludeFromGI_Raytracing,
            0x41 => Self::IncludeInGI_Raytracing,
            0x42 => Self::HairColor,
            0x43 => Self::FacialHairColor,
            0x44 => Self::EyeColor,
            0x45 => Self::BiomeHoudiniStyle,
            0x46 => Self::AnimFlavor_AnimObject,
            0x47 => Self::BrowColor,
            0x48 => Self::HairSubtype,
            0x49 => Self::FacialHairSubtype,
            0x4a => Self::BrowSubtype,
            0x4b => Self::AVMSConditionSequence,
            0x4c => Self::BiomeCreatureType,
            0x4d => Self::ShipModuleUpgrade,
            0x4e => Self::DisplayName,
            0x4f => Self::AVMSAppearanceVariationMod,
            0x50 => Self::UIIconTreatment,
            0x51 => Self::FormPair,
            0x52 => Self::ItemDescription,
            0x53 => Self::WeaponTypeDisplay,
            0x54 => Self::AVMSConditionKeyword,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum RecordEsmFlags : u32`.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct RecordEsmFlags: u32 {
        const None_ = 0x0;
        const EsmFile = 0x1;
        const Deleted = 0x20;
        const R00 = 0x40;
        const R01 = 0x100;
        const R02 = 0x200;
        const R03 = 0x400;
        const InitiallyDisabled = 0x800;
        const Ignored = 0x1000;
        const VisibleWhenDistant = 0x8000;
        const R04 = 0x10000;
        const R05 = 0x20000;
        const Compressed = 0x40000;
        const CantWait = 0x80000;
        const R06 = 0x100000;
        const IsMarker = 0x800000;
        const R07 = 0x2000000;
        const NavMesh01 = 0x4000000;
        const NavMesh02 = 0x8000000;
        const R08 = 0x10000000;
        const R09 = 0x20000000;
        const R10 = 0x40000000;
        const R11 = 0x80000000;
    }
}

/// C# `enum RecordGroupGroupType : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum RecordGroupGroupType {
    Top = 0x0,
    WorldChildren = 0x1,
    InteriorCellBlock = 0x2,
    InteriorCellSubBlock = 0x3,
    ExteriorCellBlock = 0x4,
    ExteriorCellSubBlock = 0x5,
    CellChildren = 0x6,
    TopicChildren = 0x7,
    CellPersistentChilden = 0x8,
    CellTemporaryChildren = 0x9,
    CellVisibleDistantChildren = 0xa,
}

impl RecordGroupGroupType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x0 => Self::Top,
            0x1 => Self::WorldChildren,
            0x2 => Self::InteriorCellBlock,
            0x3 => Self::InteriorCellSubBlock,
            0x4 => Self::ExteriorCellBlock,
            0x5 => Self::ExteriorCellSubBlock,
            0x6 => Self::CellChildren,
            0x7 => Self::TopicChildren,
            0x8 => Self::CellPersistentChilden,
            0x9 => Self::CellTemporaryChildren,
            0xa => Self::CellVisibleDistantChildren,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum BmdtBipedFlag : u32`.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct BmdtBipedFlag: u32 {
        const Head = 0x1;
        const Hair = 0x2;
        const UpperBody = 0x4;
        const LeftHand = 0x8;
        const RightHand = 0x10;
        const Weapon = 0x20;
        const PipBoy = 0x40;
        const Backpack = 0x80;
        const Necklace = 0x100;
        const Headband = 0x200;
        const Hat = 0x400;
        const EyeGlasses = 0x800;
        const NoseRing = 0x1000;
        const Earrings = 0x2000;
        const Mask = 0x4000;
        const Choker = 0x8000;
        const MouthObject = 0x10000;
        const BodyAddOn1 = 0x20000;
        const BodyAddOn2 = 0x40000;
        const BodyAddOn3 = 0x80000;
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum BmdtGeneralFlag : u8`.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct BmdtGeneralFlag: u8 {
        const X1 = 0x1;
        const Z2 = 0x2;
        const X3 = 0x4;
        const X4 = 0x8;
        const X5 = 0x10;
        const PowerArmor = 0x20;
        const NonPlayable = 0x40;
        const Heavy = 0x80;
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum ModlModdFlag : i32`.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct ModlModdFlag: i32 {
        const Head = 0x1;
        const Torso = 0x2;
        const RightHand = 0x4;
        const LeftHand = 0x8;
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum DestDestFlag : u8`.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct DestDestFlag: u8 {
        const VATSTargetable = 0x1;
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum DestDstdFlag : u8`.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct DestDstdFlag: u8 {
        const CapDamage = 0x1;
        const Disable = 0x2;
        const Destroy = 0x4;
    }
}

/// C# `enum EfidType_ : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum EfidType_ {
    Self = 0x0,
    Touch = 0x1,
    Target = 0x2,
}

impl EfidType_ {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::Self,
            0x1 => Self::Touch,
            0x2 => Self::Target,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

/// C# `enum CtdaINFOType : u8`.
#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum CtdaINFOType {
    Nothing = 0x0,
    Function = 0x1,
    Global = 0x2,
    Local = 0x3,
    Journal = 0x4,
    Item = 0x5,
    Dead = 0x6,
    NotId = 0x7,
    NotFaction = 0x8,
    NotClass = 0x9,
    NotRace = 0xa,
    NotCell = 0xb,
    NotLocal = 0xc,
}

impl CtdaINFOType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u8) -> Option<Self> {
        Some(match v {
            0x0 => Self::Nothing,
            0x1 => Self::Function,
            0x2 => Self::Global,
            0x3 => Self::Local,
            0x4 => Self::Journal,
            0x5 => Self::Item,
            0x6 => Self::Dead,
            0x7 => Self::NotId,
            0x8 => Self::NotFaction,
            0x9 => Self::NotClass,
            0xa => Self::NotRace,
            0xb => Self::NotCell,
            0xc => Self::NotLocal,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u8 {
        self as u8
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum VmadScriptStatus : u8`.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct VmadScriptStatus: u8 {
        const Local = 0x0;
        const Inherited = 0x2;
        const Removed = 0x4;
    }
}

/// C# `enum VmadPropertyStatus : u8`.
#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum VmadPropertyStatus {
    Edited = 0x1,
    Removed = 0x3,
}

impl VmadPropertyStatus {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u8) -> Option<Self> {
        Some(match v {
            0x1 => Self::Edited,
            0x3 => Self::Removed,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u8 {
        self as u8
    }
}

/// C# `enum VmadPropertyType : u8`.
#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum VmadPropertyType {
    None_ = 0x0,
    Object = 0x1,
    String = 0x2,
    Int32 = 0x3,
    Float = 0x4,
    Bool = 0x5,
    Variable = 0x6,
    Struct = 0x7,
    Objects = 0xb,
    Strings = 0xc,
    Int32s = 0xd,
    Floats = 0xe,
    Bools = 0xf,
    Variables = 0x10,
    Structs = 0x11,
}

impl VmadPropertyType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u8) -> Option<Self> {
        Some(match v {
            0x0 => Self::None_,
            0x1 => Self::Object,
            0x2 => Self::String,
            0x3 => Self::Int32,
            0x4 => Self::Float,
            0x5 => Self::Bool,
            0x6 => Self::Variable,
            0x7 => Self::Struct,
            0xb => Self::Objects,
            0xc => Self::Strings,
            0xd => Self::Int32s,
            0xe => Self::Floats,
            0xf => Self::Bools,
            0x10 => Self::Variables,
            0x11 => Self::Structs,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u8 {
        self as u8
    }
}

/// C# `enum AACTRecordFlag : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum AACTRecordFlag {
    NonPlayable = 0x4,
    GroundPiece = 0x10,
    HiddenFromLocalMap = 0x200,
    UsedAsPlatform = 0x800,
    Restricted = 0x8000,
    HasCurrents = 0x80000,
    _Navmesh_Filter = 0x4000000,
    _Navmesh_BoundingBox = 0x8000000,
    _Navmesh_OnlyCut = 0x10000000,
    _Navmesh_IgnoreErosion_ChildCanUse = 0x20000000,
    _Navmesh_Ground = 0x40000000,
}

impl AACTRecordFlag {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x4 => Self::NonPlayable,
            0x10 => Self::GroundPiece,
            0x200 => Self::HiddenFromLocalMap,
            0x800 => Self::UsedAsPlatform,
            0x8000 => Self::Restricted,
            0x80000 => Self::HasCurrents,
            0x4000000 => Self::_Navmesh_Filter,
            0x8000000 => Self::_Navmesh_BoundingBox,
            0x10000000 => Self::_Navmesh_OnlyCut,
            0x20000000 => Self::_Navmesh_IgnoreErosion_ChildCanUse,
            0x40000000 => Self::_Navmesh_Ground,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

/// C# `enum AAPDRecordFlag : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum AAPDRecordFlag {
    NonPlayable = 0x4,
    GroundPiece = 0x10,
    HiddenFromLocalMap = 0x200,
    UsedAsPlatform = 0x800,
    Restricted = 0x8000,
    HasCurrents = 0x80000,
    _Navmesh_Filter = 0x4000000,
    _Navmesh_BoundingBox = 0x8000000,
    _Navmesh_OnlyCut = 0x10000000,
    _Navmesh_IgnoreErosion_ChildCanUse = 0x20000000,
    _Navmesh_Ground = 0x40000000,
}

impl AAPDRecordFlag {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x4 => Self::NonPlayable,
            0x10 => Self::GroundPiece,
            0x200 => Self::HiddenFromLocalMap,
            0x800 => Self::UsedAsPlatform,
            0x8000 => Self::Restricted,
            0x80000 => Self::HasCurrents,
            0x4000000 => Self::_Navmesh_Filter,
            0x8000000 => Self::_Navmesh_BoundingBox,
            0x10000000 => Self::_Navmesh_OnlyCut,
            0x20000000 => Self::_Navmesh_IgnoreErosion_ChildCanUse,
            0x40000000 => Self::_Navmesh_Ground,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

/// C# `enum ACHRRecordFlag : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum ACHRRecordFlag {
    StartsDead = 0x200,
    Persistent = 0x400,
    InitiallyDisabled = 0x800,
    StartsUnconscious = 0x2000,
    VisibleWhenDistant = 0x8000,
    NoAIAcquire = 0x2000000,
    DontHavokSettle = 0x20000000,
}

impl ACHRRecordFlag {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x200 => Self::StartsDead,
            0x400 => Self::Persistent,
            0x800 => Self::InitiallyDisabled,
            0x2000 => Self::StartsUnconscious,
            0x8000 => Self::VisibleWhenDistant,
            0x2000000 => Self::NoAIAcquire,
            0x20000000 => Self::DontHavokSettle,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum DataFlag : u8`.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct DataFlag: u8 {
        const NoAutoCalculate = 0x1;
        const FoodItem = 0x2;
    }
}

/// C# `enum EnamRange_ : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum EnamRange_ {
    Self = 0x0,
    Touch = 0x1,
    Target = 0x2,
}

impl EnamRange_ {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x0 => Self::Self,
            0x1 => Self::Touch,
            0x2 => Self::Target,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum DataFlag2 : u32`.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct DataFlag2: u32 {
        const IgnoresNormalWeaponResistance = 0x1;
    }
}

/// C# `enum DataType_ : u8`.
#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum DataType_ {
    MortarAndPestle = 0x0,
    Albemic = 0x1,
    Calcinator = 0x2,
    Retort = 0x3,
}

impl DataType_ {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u8) -> Option<Self> {
        Some(match v {
            0x0 => Self::MortarAndPestle,
            0x1 => Self::Albemic,
            0x2 => Self::Calcinator,
            0x3 => Self::Retort,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u8 {
        self as u8
    }
}

/// C# `enum DataARMOType : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum DataARMOType {
    Helmet = 0x0,
    Cuirass = 0x1,
    L_Pauldron = 0x2,
    R_Pauldron = 0x3,
    Greaves = 0x4,
    Boots = 0x5,
    L_Gauntlet = 0x6,
    R_Gauntlet = 0x7,
    Shield = 0x8,
    L_Bracer = 0x9,
    R_Bracer = 0xa,
}

impl DataARMOType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::Helmet,
            0x1 => Self::Cuirass,
            0x2 => Self::L_Pauldron,
            0x3 => Self::R_Pauldron,
            0x4 => Self::Greaves,
            0x5 => Self::Boots,
            0x6 => Self::L_Gauntlet,
            0x7 => Self::R_Gauntlet,
            0x8 => Self::Shield,
            0x9 => Self::L_Bracer,
            0xa => Self::R_Bracer,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum ARMORecordNodeFlag : u32`.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct ARMORecordNodeFlag: u32 {
        const Head = 0x1;
        const Hair = 0x2;
        const Body = 0x4;
        const Hands = 0x8;
        const Forearms = 0x10;
        const Amulet = 0x20;
        const Ring = 0x40;
        const Feet = 0x80;
        const Calves = 0x100;
        const Shield = 0x200;
        const Tail = 0x400;
        const LongHair = 0x800;
        const Circlet = 0x1000;
        const Ears = 0x2000;
        const BodyAddOn3 = 0x4000;
        const BodyAddOn4 = 0x8000;
        const BodyAddOn5 = 0x10000;
        const BodyAddOn6 = 0x20000;
        const BodyAddOn7 = 0x40000;
        const BodyAddOn8 = 0x80000;
        const DecapitateHead = 0x100000;
        const Decapitate = 0x200000;
        const BodyAddOn9 = 0x400000;
        const BodyAddOn10 = 0x800000;
        const BodyAddOn11 = 0x1000000;
        const BodyAddOn12 = 0x2000000;
        const BodyAddOn13 = 0x4000000;
        const BodyAddOn14 = 0x8000000;
        const BodyAddOn15 = 0x10000000;
        const BodyAddOn16 = 0x20000000;
        const BodyAddOn17 = 0x40000000;
        const FX01 = 0x80000000;
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum ARMORecordFlag : u8`.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct ARMORecordFlag: u8 {
        const ModulatesVoice = 0x1;
        const NonPlayable = 0x10;
    }
}

/// C# `enum ARMORecordSkillType : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum ARMORecordSkillType {
    LightArmor = 0x0,
    HeavyArmor = 0x1,
    None_ = 0x2,
}

impl ARMORecordSkillType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x0 => Self::LightArmor,
            0x1 => Self::HeavyArmor,
            0x2 => Self::None_,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum ARTORecordDnamFlag : u32`.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct ARTORecordDnamFlag: u32 {
        const MagicCasting = 0x0;
        const MagicHitEffect = 0x1;
        const EnchantmentEffect = 0x2;
    }
}

/// C# `enum ASPCRecordAnam : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum ASPCRecordAnam {
    None_ = 0x0,
    Default = 0x1,
    Generic = 0x2,
    PaddedCell = 0x3,
    Room = 0x4,
    Bathroom = 0x5,
    Livingroom = 0x6,
    StoneRoom = 0x7,
    Auditorium = 0x8,
    Concerthall = 0x9,
    Cave = 0xa,
    Arena = 0xb,
    Hangar = 0xc,
    CarpetedHallway = 0xd,
    Hallway = 0xe,
    StoneCorridor = 0xf,
    Alley = 0x10,
    Forest = 0x11,
    City = 0x12,
    Mountains = 0x13,
    Quarry = 0x14,
    Plain = 0x15,
    Parkinglot = 0x16,
    Sewerpipe = 0x17,
    Underwater = 0x18,
    SmallRoom = 0x19,
    MediumRoom = 0x1a,
    LargeRoom = 0x1b,
    MediumHall = 0x1c,
    LargeHall = 0x1d,
    Plate = 0x1e,
}

impl ASPCRecordAnam {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x0 => Self::None_,
            0x1 => Self::Default,
            0x2 => Self::Generic,
            0x3 => Self::PaddedCell,
            0x4 => Self::Room,
            0x5 => Self::Bathroom,
            0x6 => Self::Livingroom,
            0x7 => Self::StoneRoom,
            0x8 => Self::Auditorium,
            0x9 => Self::Concerthall,
            0xa => Self::Cave,
            0xb => Self::Arena,
            0xc => Self::Hangar,
            0xd => Self::CarpetedHallway,
            0xe => Self::Hallway,
            0xf => Self::StoneCorridor,
            0x10 => Self::Alley,
            0x11 => Self::Forest,
            0x12 => Self::City,
            0x13 => Self::Mountains,
            0x14 => Self::Quarry,
            0x15 => Self::Plain,
            0x16 => Self::Parkinglot,
            0x17 => Self::Sewerpipe,
            0x18 => Self::Underwater,
            0x19 => Self::SmallRoom,
            0x1a => Self::MediumRoom,
            0x1b => Self::LargeRoom,
            0x1c => Self::MediumHall,
            0x1d => Self::LargeHall,
            0x1e => Self::Plate,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum ASTPRecordDataFlag : u32`.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct ASTPRecordDataFlag: u32 {
        const FamilyAssociation = 0x1;
    }
}

/// C# `enum BODYRecordPart : u8`.
#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum BODYRecordPart {
    Head = 0x0,
    Hair = 0x1,
    Neck = 0x2,
    Chest = 0x3,
    Groin = 0x4,
    Hand = 0x5,
    Wrist = 0x6,
    Forearm = 0x7,
    Upperarm = 0x8,
    Foot = 0x9,
    Ankle = 0xa,
    Knee = 0xb,
    Upperleg = 0xc,
    Clavicle = 0xd,
    Tail = 0xe,
}

impl BODYRecordPart {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u8) -> Option<Self> {
        Some(match v {
            0x0 => Self::Head,
            0x1 => Self::Hair,
            0x2 => Self::Neck,
            0x3 => Self::Chest,
            0x4 => Self::Groin,
            0x5 => Self::Hand,
            0x6 => Self::Wrist,
            0x7 => Self::Forearm,
            0x8 => Self::Upperarm,
            0x9 => Self::Foot,
            0xa => Self::Ankle,
            0xb => Self::Knee,
            0xc => Self::Upperleg,
            0xd => Self::Clavicle,
            0xe => Self::Tail,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u8 {
        self as u8
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum BODYRecordFlag : u8`.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct BODYRecordFlag: u8 {
        const Female = 0x1;
        const Playable = 0x2;
    }
}

/// C# `enum BODYRecordPartType : u8`.
#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum BODYRecordPartType {
    Skin = 0x0,
    Clothing = 0x1,
    Armor = 0x2,
}

impl BODYRecordPartType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u8) -> Option<Self> {
        Some(match v {
            0x0 => Self::Skin,
            0x1 => Self::Clothing,
            0x2 => Self::Armor,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u8 {
        self as u8
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum BOOKRecordFlag : u8`.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct BOOKRecordFlag: u8 {
        const Scroll = 0x1;
        const CantBeTaken = 0x2;
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum BpndFlag : u8`.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct BpndFlag: u8 {
        const Severable = 0x1;
        const IKData = 0x2;
        const IKData_BipedData = 0x4;
        const Explodable = 0x8;
        const IKData_IsHead = 0x10;
        const IKData_Headtracking = 0x20;
        const ToHitChance_Absolute = 0x40;
    }
}

/// C# `enum BpndPartType_ : u8`.
#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum BpndPartType_ {
    Torso = 0x0,
    Head = 0x1,
    Eye = 0x2,
    LookAt = 0x3,
    FlyGrab = 0x4,
    Saddle = 0x5,
}

impl BpndPartType_ {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u8) -> Option<Self> {
        Some(match v {
            0x0 => Self::Torso,
            0x1 => Self::Head,
            0x2 => Self::Eye,
            0x3 => Self::LookAt,
            0x4 => Self::FlyGrab,
            0x5 => Self::Saddle,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u8 {
        self as u8
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum CELLRecordFlag : u16`.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct CELLRecordFlag: u16 {
        const Interior = 0x1;
        const HasWater = 0x2;
        const InvertFastTravel = 0x4;
        const BehaveLikeExterior = 0x8;
        const Unknown1 = 0x10;
        const PublicArea = 0x20;
        const HandChanged = 0x40;
        const ShowSky = 0x80;
        const UseSkyLighting = 0x100;
    }
}

/// C# `enum DataSpecialization_ : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum DataSpecialization_ {
    Combat = 0x0,
    Magic = 0x1,
    Stealth = 0x2,
}

impl DataSpecialization_ {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x0 => Self::Combat,
            0x1 => Self::Magic,
            0x2 => Self::Stealth,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum DataFlag3 : u32`.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct DataFlag3: u32 {
        const Playable = 0x1;
        const Guard = 0x2;
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum DataService : u32`.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct DataService: u32 {
        const Weapon = 0x1;
        const Armor = 0x2;
        const Clothing = 0x4;
        const Books = 0x8;
        const Ingredients = 0x10;
        const Picks = 0x20;
        const Probes = 0x40;
        const Lights = 0x80;
        const Apparatus = 0x100;
        const RepairItems = 0x200;
        const Misc = 0x400;
        const Spells = 0x800;
        const MagicItems = 0x1000;
        const Potions = 0x2000;
        const Training = 0x4000;
        const Spellmaking = 0x8000;
        const Enchanting = 0x10000;
        const Repair = 0x20000;
    }
}

/// C# `enum DataType_2 : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum DataType_2 {
    Pants = 0x0,
    Shoes = 0x1,
    Shirt = 0x2,
    Belt = 0x3,
    Robe = 0x4,
    R_Glove = 0x5,
    L_Glove = 0x6,
    Skirt = 0x7,
    Ring = 0x8,
    Amulet = 0x9,
}

impl DataType_2 {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x0 => Self::Pants,
            0x1 => Self::Shoes,
            0x2 => Self::Shirt,
            0x3 => Self::Belt,
            0x4 => Self::Robe,
            0x5 => Self::R_Glove,
            0x6 => Self::L_Glove,
            0x7 => Self::Skirt,
            0x8 => Self::Ring,
            0x9 => Self::Amulet,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum CREARecordFlag : u32`.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct CREARecordFlag: u32 {
        const Biped = 0x1;
        const Respawn = 0x2;
        const WeaponAndShield = 0x4;
        const None_ = 0x8;
        const Swims = 0x10;
        const Flies = 0x20;
        const Walks = 0x40;
        const DefaultFlags = 0x48;
        const Essential = 0x80;
        const SkeletonBlood = 0x400;
        const MetalBlood = 0x800;
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum CREA3RecordAIFlags : u32`.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct CREA3RecordAIFlags: u32 {
        const Weapons = 0x1;
        const Armor = 0x2;
        const Clothing = 0x4;
        const Books = 0x8;
        const Ingrediant = 0x10;
        const Picks = 0x20;
        const Probes = 0x40;
        const Lights = 0x80;
        const Apparatus = 0x100;
        const Repair = 0x200;
        const Misc = 0x400;
        const Spells = 0x800;
        const MagicItems = 0x1000;
        const Potions = 0x2000;
        const Training = 0x4000;
        const Spellmaking = 0x8000;
        const Recharge = 0x10000;
        const RepairItem = 0x20000;
    }
}

/// C# `enum DIALRecordType3 : u8`.
#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum DIALRecordType3 {
    Topic = 0x0,
    Voice = 0x1,
    Greeting = 0x2,
    Persuasion = 0x3,
    Journal = 0x4,
}

impl DIALRecordType3 {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u8) -> Option<Self> {
        Some(match v {
            0x0 => Self::Topic,
            0x1 => Self::Voice,
            0x2 => Self::Greeting,
            0x3 => Self::Persuasion,
            0x4 => Self::Journal,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u8 {
        self as u8
    }
}

/// C# `enum DIALRecordType4 : u8`.
#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum DIALRecordType4 {
    Topic = 0x0,
    Conversation = 0x1,
    Combat = 0x2,
    Persuasion = 0x3,
    Detection = 0x4,
    Service = 0x5,
    Miscellaneous = 0x6,
    Radio = 0x7,
}

impl DIALRecordType4 {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u8) -> Option<Self> {
        Some(match v {
            0x0 => Self::Topic,
            0x1 => Self::Conversation,
            0x2 => Self::Combat,
            0x3 => Self::Persuasion,
            0x4 => Self::Detection,
            0x5 => Self::Service,
            0x6 => Self::Miscellaneous,
            0x7 => Self::Radio,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u8 {
        self as u8
    }
}

/// C# `enum EnitType3 : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum EnitType3 {
    CastOnce = 0x0,
    CastStrikes = 0x1,
    CastWhenUsed = 0x2,
    ConstantEffect = 0x3,
}

impl EnitType3 {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::CastOnce,
            0x1 => Self::CastStrikes,
            0x2 => Self::CastWhenUsed,
            0x3 => Self::ConstantEffect,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

/// C# `enum EnitType4 : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum EnitType4 {
    Scroll = 0x0,
    Staff = 0x1,
    Weapon = 0x2,
    Apparel = 0x3,
}

impl EnitType4 {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::Scroll,
            0x1 => Self::Staff,
            0x2 => Self::Weapon,
            0x3 => Self::Apparel,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum EnitFlag : i32`.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct EnitFlag: i32 {
        const AutoCalc = 0x1;
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum FadtFlag : u32`.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct FadtFlag: u32 {
        const HiddenFromPlayer = 0x1;
    }
}

/// C# `enum PlvdSpecType : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum PlvdSpecType {
    NearReference = 0x0,
    InCell = 0x1,
    NearPackageStartLocation = 0x2,
    NearEditorLocation = 0x3,
    LinkedReference = 0x6,
    NearSelf = 0xc,
}

impl PlvdSpecType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x0 => Self::NearReference,
            0x1 => Self::InCell,
            0x2 => Self::NearPackageStartLocation,
            0x3 => Self::NearEditorLocation,
            0x6 => Self::LinkedReference,
            0xc => Self::NearSelf,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum DataColorFlags : i32`.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct DataColorFlags: i32 {
        const Dynamic = 0x1;
        const CanCarry = 0x2;
        const Negative = 0x4;
        const Flicker = 0x8;
        const Fire = 0x10;
        const OffDefault = 0x20;
        const FlickerSlow = 0x40;
        const Pulse = 0x80;
        const PulseSlow = 0x100;
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum MGEFRecordMFEGFlag : u32`.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct MGEFRecordMFEGFlag: u32 {
        const Hostile = 0x1;
        const Recover = 0x2;
        const Detrimental = 0x4;
        const MagnitudePercent = 0x8;
        const Self = 0x10;
        const Touch = 0x20;
        const Target = 0x40;
        const NoDuration = 0x80;
        const NoMagnitude = 0x100;
        const NoArea = 0x200;
        const FXPersist = 0x400;
        const Spellmaking = 0x800;
        const Enchanting = 0x1000;
        const NoIngredient = 0x2000;
        const Unknown14 = 0x4000;
        const Unknown15 = 0x8000;
        const UseWeapon = 0x10000;
        const UseArmor = 0x20000;
        const UseCreature = 0x40000;
        const UseSkill = 0x80000;
        const UseAttribute = 0x100000;
        const Unknown21 = 0x200000;
        const Unknown22 = 0x400000;
        const Unknown23 = 0x800000;
        const UseActorValue = 0x1000000;
        const SprayProjectileType = 0x2000000;
        const BoltProjectileType = 0x4000000;
        const NoHitEffect = 0x8000000;
        const Unknown28 = 0x10000000;
        const Unknown29 = 0x20000000;
        const Unknown30 = 0x40000000;
        const Unknown31 = 0x80000000;
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum NPC_3RecordNPC_3Flags : u32`.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct NPC_3RecordNPC_3Flags: u32 {
        const Female = 0x1;
        const Essential = 0x2;
        const Respawn = 0x4;
        const None_ = 0x8;
        const Autocalc = 0x10;
        const BloodSkel = 0x400;
        const BloodMetal = 0x800;
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum NPC_4RecordNPC_4Flags : u32`.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct NPC_4RecordNPC_4Flags: u32 {
        const Female = 0x1;
        const Essential = 0x2;
        const Respawn = 0x8;
        const Autocalc = 0x10;
        const PCLevelOffset = 0x80;
        const NoLowLevelProcessing = 0x200;
        const NoRumors = 0x2000;
        const Summonable = 0x4000;
        const NoPersuasion = 0x8000;
        const CanCorpseCheck = 0x100000;
    }
}

/// C# `enum RACERecordDataFlag : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum RACERecordDataFlag {
    Playable = 0x1,
    FaceGenHead = 0x2,
    Child = 0x4,
    TiltFrontBack = 0x8,
    TiltLeftRight = 0x10,
    NoShadow = 0x20,
    Swims = 0x40,
    Flies = 0x80,
    Walks = 0x100,
    Immobile = 0x200,
    NotPushable = 0x400,
    NoCombatInWater = 0x800,
    NoRotatingToHeadTrack = 0x1000,
    DontShowBloodSpray = 0x2000,
    DontShowBloodDecal = 0x4000,
    UsesHeadTrackAnims = 0x8000,
    SpellsAlignWMagicNode = 0x10000,
    UseWorldRaycastsForFootIK = 0x20000,
    AllowRagdollCollision = 0x40000,
    RegenHPInCombat = 0x80000,
    CantOpenDoors = 0x100000,
    AllowPCDialogue = 0x200000,
    NoKnockdowns = 0x400000,
    AllowPickpocket = 0x800000,
    AlwaysUseProxyController = 0x1000000,
    DontShowWeaponBlood = 0x2000000,
    OverlayHeadPartList = 0x4000000,
    OverrideHeadPartList = 0x8000000,
    CanPickupItems = 0x10000000,
    AllowMultipleMembraneShaders = 0x20000000,
    CanDualWield = 0x40000000,
    AvoidsRoads = 0x80000000,
}

impl RACERecordDataFlag {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x1 => Self::Playable,
            0x2 => Self::FaceGenHead,
            0x4 => Self::Child,
            0x8 => Self::TiltFrontBack,
            0x10 => Self::TiltLeftRight,
            0x20 => Self::NoShadow,
            0x40 => Self::Swims,
            0x80 => Self::Flies,
            0x100 => Self::Walks,
            0x200 => Self::Immobile,
            0x400 => Self::NotPushable,
            0x800 => Self::NoCombatInWater,
            0x1000 => Self::NoRotatingToHeadTrack,
            0x2000 => Self::DontShowBloodSpray,
            0x4000 => Self::DontShowBloodDecal,
            0x8000 => Self::UsesHeadTrackAnims,
            0x10000 => Self::SpellsAlignWMagicNode,
            0x20000 => Self::UseWorldRaycastsForFootIK,
            0x40000 => Self::AllowRagdollCollision,
            0x80000 => Self::RegenHPInCombat,
            0x100000 => Self::CantOpenDoors,
            0x200000 => Self::AllowPCDialogue,
            0x400000 => Self::NoKnockdowns,
            0x800000 => Self::AllowPickpocket,
            0x1000000 => Self::AlwaysUseProxyController,
            0x2000000 => Self::DontShowWeaponBlood,
            0x4000000 => Self::OverlayHeadPartList,
            0x8000000 => Self::OverrideHeadPartList,
            0x10000000 => Self::CanPickupItems,
            0x20000000 => Self::AllowMultipleMembraneShaders,
            0x40000000 => Self::CanDualWield,
            0x80000000 => Self::AvoidsRoads,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

/// C# `enum RACE4RecordFaceIndx : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum RACE4RecordFaceIndx {
    Head = 0x0,
    Ear_Male = 0x1,
    Ear_Female = 0x2,
    Mouth = 0x3,
    Teeth_Lower = 0x4,
    Teeth_Upper = 0x5,
    Tongue = 0x6,
    Eye_Left = 0x7,
    Eye_Right = 0x8,
}

impl RACE4RecordFaceIndx {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x0 => Self::Head,
            0x1 => Self::Ear_Male,
            0x2 => Self::Ear_Female,
            0x3 => Self::Mouth,
            0x4 => Self::Teeth_Lower,
            0x5 => Self::Teeth_Upper,
            0x6 => Self::Tongue,
            0x7 => Self::Eye_Left,
            0x8 => Self::Eye_Right,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

/// C# `enum RACE4RecordBodyIndx : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum RACE4RecordBodyIndx {
    UpperBody = 0x0,
    LowerBody = 0x1,
    Hand = 0x2,
    Foot = 0x3,
    Tail = 0x4,
}

impl RACE4RecordBodyIndx {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x0 => Self::UpperBody,
            0x1 => Self::LowerBody,
            0x2 => Self::Hand,
            0x3 => Self::Foot,
            0x4 => Self::Tail,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum XtelFlag : u32`.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct XtelFlag: u32 {
        const NoAlarm = 0x0;
        const NoLoadScreen = 0x1;
        const RelativePosition = 0x2;
    }
}

/// C# `enum REGNRecordREGNType : u8`.
#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum REGNRecordREGNType {
    None_ = 0x0,
    One = 0x1,
    Objects = 0x2,
    Weather = 0x3,
    Map = 0x4,
    Landscape = 0x5,
    Grass = 0x6,
    Sound = 0x7,
}

impl REGNRecordREGNType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u8) -> Option<Self> {
        Some(match v {
            0x0 => Self::None_,
            0x1 => Self::One,
            0x2 => Self::Objects,
            0x3 => Self::Weather,
            0x4 => Self::Map,
            0x5 => Self::Landscape,
            0x6 => Self::Grass,
            0x7 => Self::Sound,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u8 {
        self as u8
    }
}

/// C# `enum SNDGRecordSNDGType : u32`.
#[repr(u32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum SNDGRecordSNDGType {
    LeftFoot = 0x0,
    RightFoot = 0x1,
    SwimLeft = 0x2,
    SwimRight = 0x3,
    Moan = 0x4,
    Roar = 0x5,
    Scream = 0x6,
    Land = 0x7,
}

impl SNDGRecordSNDGType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u32) -> Option<Self> {
        Some(match v {
            0x0 => Self::LeftFoot,
            0x1 => Self::RightFoot,
            0x2 => Self::SwimLeft,
            0x3 => Self::SwimRight,
            0x4 => Self::Moan,
            0x5 => Self::Roar,
            0x6 => Self::Scream,
            0x7 => Self::Land,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u32 {
        self as u32
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum SOUNRecordFlag : u16`.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct SOUNRecordFlag: u16 {
        const RandomFrequencyShift = 0x2;
        const PlayAtRandom = 0x4;
        const EnvironmentIgnored = 0x8;
        const RandomLocation = 0x10;
        const Loop = 0x20;
        const MenuSound = 0x40;
        const _2D = 0x80;
        const _360LFE = 0x100;
        const DialogueSound = 0x200;
        const EnvelopeFast = 0x400;
        const EnvelopeSlow = 0x800;
        const _2DRadius = 0x1000;
        const MuteWhenSubmerged = 0x2000;
    }
}

/// C# `enum TERMRecordDnamDifficulty : u8`.
#[repr(u8)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum TERMRecordDnamDifficulty {
    VeryEasy = 0x0,
    Easy = 0x1,
    Average = 0x2,
    Hard = 0x3,
    VeryHard = 0x4,
    RequiresKey = 0x5,
}

impl TERMRecordDnamDifficulty {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: u8) -> Option<Self> {
        Some(match v {
            0x0 => Self::VeryEasy,
            0x1 => Self::Easy,
            0x2 => Self::Average,
            0x3 => Self::Hard,
            0x4 => Self::VeryHard,
            0x5 => Self::RequiresKey,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> u8 {
        self as u8
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum TERMRecordDnamFlag : u8`.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct TERMRecordDnamFlag: u8 {
        const Leveled = 0x1;
        const Unlocked = 0x2;
        const AlternateColors = 0x4;
        const HideWelcomeTextWhenDisplayingText = 0x8;
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum TXSTRecordDnamFlag : u16`.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct TXSTRecordDnamFlag: u16 {
        const NotHasSpecularMap = 0x1;
        const FacegenTextures = 0x2;
        const HasModelSpaceNormalMap = 0x4;
    }
}

bitflags::bitflags! {
    /// C# `[Flags] enum TXSTRecordFlag : u8`.
    #[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Default)]
    pub struct TXSTRecordFlag: u8 {
        const Parallax = 0x1;
        const AlphaBlending = 0x2;
        const AlphaTesting = 0x4;
        const Not4Subtextures = 0x8;
    }
}

/// C# `enum DataWEAPType : i32`.
#[repr(i32)]
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash)]
pub enum DataWEAPType {
    ShortBladeOneHand = 0x0,
    LongBladeOneHand = 0x1,
    LongBladeTwoClose = 0x2,
    BluntOneHand = 0x3,
    BluntTwoClose = 0x4,
    BluntTwoWide = 0x5,
    SpearTwoWide = 0x6,
    AxeOneHand = 0x7,
    AxeTwoHand = 0x8,
    MarksmanBow = 0x9,
    MarksmanCrossbow = 0xa,
    MarksmanThrown = 0xb,
    Arrow = 0xc,
    Bolt = 0xd,
}

impl DataWEAPType {
    /// Decode from the on-disk value. `None` for anything undefined —
    /// a C-style cast would produce a value outside the enum, which is UB.
    pub fn from_raw(v: i32) -> Option<Self> {
        Some(match v {
            0x0 => Self::ShortBladeOneHand,
            0x1 => Self::LongBladeOneHand,
            0x2 => Self::LongBladeTwoClose,
            0x3 => Self::BluntOneHand,
            0x4 => Self::BluntTwoClose,
            0x5 => Self::BluntTwoWide,
            0x6 => Self::SpearTwoWide,
            0x7 => Self::AxeOneHand,
            0x8 => Self::AxeTwoHand,
            0x9 => Self::MarksmanBow,
            0xa => Self::MarksmanCrossbow,
            0xb => Self::MarksmanThrown,
            0xc => Self::Arrow,
            0xd => Self::Bolt,
            _ => return None,
        })
    }
    /// The on-disk value.
    pub fn to_raw(self) -> i32 {
        self as i32
    }
}
