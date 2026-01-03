using GameX.Uncore.Formats;
using OpenStack;
using OpenStack.Gfx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using static GameX.Bethesda.Formats.Records.Header;
using static GameX.Bethesda.Formats.Records.FormType;
using static System.IO.Polyfill;
#pragma warning disable CS9113

namespace GameX.Bethesda.Formats.Records;

#region Enums

public enum FormType : uint {
    APPA = 0x41505041,
    ARMA = 0x414D5241,
    AACT = 0x54434141,
    ASPC = 0x43505341,
    ACTI = 0x49544341,
    ARMO = 0x4F4D5241,
    AMMO = 0x4F4D4D41,
    ASTP = 0x50545341,
    ARTO = 0x4F545241,
    AMDL = 0x4C444D41,
    AECH = 0x48434541,
    ACRE = 0x45524341,
    AORU = 0x55524F41,
    ALCH = 0x48434C41,
    ACHR = 0x52484341,
    ANIO = 0x4F494E41,
    AVIF = 0x46495641,
    ADDN = 0x4E444441,
    BOOK = 0x4B4F4F42,
    BSGN = 0x4E475342,
    BODY = 0x59444F42,
    BNDS = 0x53444E42,
    BPTD = 0x44545042,
    CMPO = 0x4F504D43,
    CLAS = 0x53414C43,
    CSTY = 0x59545343,
    CONT = 0x544E4F43,
    CLMT = 0x544D4C43,
    CELL = 0x4C4C4543,
    CAMS = 0x534D4143,
    CPTH = 0x48545043,
    CREA = 0x41455243,
    CLFM = 0x4D464C43,
    COLL = 0x4C4C4F43,
    CLOT = 0x544F4C43,
    COBJ = 0x4A424F43,
    DMGT = 0x54474D44,
    DOOR = 0x524F4F44,
    DOBJ = 0x4A424F44,
    DFOB = 0x424F4644,
    DUAL = 0x4C415544,
    DLBR = 0x52424C44,
    DEBR = 0x52424544,
    DLVW = 0x57564C44,
    DIAL = 0x4C414944,
    EQUP = 0x50555145,
    EYES = 0x53455945,
    EFSH = 0x48534645,
    ECZN = 0x4E5A4345,
    EXPL = 0x4C505845,
    ENCH = 0x48434E45,
    FACT = 0x54434146,
    FURN = 0x4E525546,
    FLOR = 0x524F4C46,
    FSTP = 0x50545346,
    FSTS = 0x53545346,
    FLST = 0x54534C46,
    GRUP = 0x50555247,
    GMST = 0x54534D47,
    GLOB = 0x424F4C47,
    GRAS = 0x53415247,
    GDRY = 0x59524447,
    HAZD = 0x445A4148,
    HDPT = 0x54504448,
    HAIR = 0x52494148,
    INGR = 0x52474E49,
    IDLM = 0x4D4C4449,
    INFO = 0x4F464E49,
    IDLE = 0x454C4449,
    IPCT = 0x54435049,
    IPDS = 0x53445049,
    IMGS = 0x53474D49,
    IMAD = 0x44414D49,
    INNR = 0x524E4E49,
    KYWD = 0x4457594B,
    KEYM = 0x4D59454B,
    KSSM = 0x4D53534B,
    LIGH = 0x4847494C,
    LCTN = 0x4E54434C,
    LCRT = 0x5452434C,
    LTEX = 0x5845544C,
    LVLN = 0x4E4C564C,
    LVLI = 0x494C564C,
    LAND = 0x444E414C,
    LSCR = 0x5243534C,
    LVSP = 0x5053564C,
    LGTM = 0x4D54474C,
    LVLC = 0x434C564C,
    LEVC = 0x4356454C,
    LOCK = 0x4B434F4C,
    LENS = 0x534E454C,
    LSPR = 0x5250534C,
    LEVI = 0x4956454C,
    LAYR = 0x5259414C,
    MATT = 0x5454414D,
    MSTT = 0x5454534D,
    MGEF = 0x4645474D,
    MICN = 0x4E43494D,
    MISC = 0x4353494D,
    MESG = 0x4753454D,
    MUSC = 0x4353554D,
    MUST = 0x5453554D,
    MOVT = 0x54564F4D,
    MATO = 0x4F54414D,
    MSWP = 0x5057534D,
    NONE = 0x454E4F4E,
    NPC_ = 0x5F43504E,
    NOTE = 0x45544F4E,
    NAVI = 0x4956414E,
    NAVM = 0x4D56414E,
    NOCM = 0x4D434F4E,
    OTFT = 0x5446544F,
    OMOD = 0x444F4D4F,
    OVIS = 0x5349564F,
    PROJ = 0x4A4F5250,
    PMIS = 0x53494D50,
    PARW = 0x57524150,
    PGRE = 0x45524750,
    PBEA = 0x41454250,
    PFLA = 0x414C4650,
    PCON = 0x4E4F4350,
    PBAR = 0x52414250,
    PHZD = 0x445A4850,
    PACK = 0x4B434150,
    PERK = 0x4B524550,
    PKIN = 0x4E494B50,
    PROB = 0x424F5250,
    PGRD = 0x44524750,
    QUST = 0x54535551,
    RFCT = 0x54434652,
    REGN = 0x4E474552,
    RACE = 0x45434152,
    REFR = 0x52464552,
    RGDL = 0x4C444752,
    REVB = 0x42564552,
    ROAD = 0x44414F52,
    REPA = 0x41504552,
    RFGP = 0x50474652,
    RELA = 0x414C4552,
    SPGD = 0x44475053,
    SLGM = 0x4D474C53,
    STAT = 0x54415453,
    SCOL = 0x4C4F4353,
    SOUN = 0x4E554F53,
    SKIL = 0x4C494B53,
    SCPT = 0x54504353,
    SPEL = 0x4C455053,
    SCRL = 0x4C524353,
    SMBN = 0x4E424D53,
    SMQN = 0x4E514D53,
    SMEN = 0x4E454D53,
    SHOU = 0x554F4853,
    SBSP = 0x50534253,
    SNDR = 0x52444E53,
    SNCT = 0x54434E53,
    SOPM = 0x4D504F53,
    SCCO = 0x4F434353,
    SCSN = 0x4E534353,
    STAG = 0x47415453,
    SGST = 0x54534753,
    SSCR = 0x52435353,
    SCEN = 0x4E454353,
    SNDG = 0x47444E53,
    TOFT = 0x54464F54,
    TERM = 0x4D524554,
    TREE = 0x45455254,
    TLOD = 0x444F4C54,
    TES3 = 0x33534554,
    TES4 = 0x34534554,
    TES5 = 0x35534554,
    TES6 = 0x36534554,
    TRNS = 0x534E5254,
    TXST = 0x54535854,
    TACT = 0x54434154,
    VTYP = 0x50595456,
    WRLD = 0x444C5257,
    WEAP = 0x50414557,
    WTHR = 0x52485457,
    WATR = 0x52544157,
    WOOP = 0x504F4F57,
    ZOOM = 0x4D4F4F5A,
}

// $"0x{BitConverter.ToUInt32(Encoding.ASCII.GetBytes("FULL")):X}"
public enum FieldType : uint {
    ANAM = 0x4D414E41,
    AVFX = 0x58465641,
    ASND = 0x444E5341,
    AADT = 0x54444141,
    AODT = 0x54444F41,
    ALDT = 0x54444C41,
    AMBI = 0x49424D41,
    ATXT = 0x54585441,
    ATTR = 0x52545441,
    AIDT = 0x54444941,
    AI_W = 0x575F4941,
    AI_T = 0x545F4941,
    AI_F = 0x465F4941,
    AI_E = 0x455F4941,
    AI_A = 0x415F4941,
    BTXT = 0x54585442,
    BVFX = 0x58465642,
    BSND = 0x444E5342,
    BMDT = 0x54444D42,
    BKDT = 0x54444B42,
    BNAM = 0x4D414E42,
    BYDT = 0x54445942,
    CSTD = 0x44545343,
    CSAD = 0x44415343,
    CNAM = 0x4D414E43,
    CTDA = 0x41445443,
    CTDT = 0x54445443,
    CVFX = 0x58465643,
    CSND = 0x444E5343,
    CNDT = 0x54444E43,
    CLDT = 0x54444C43,
    CNTO = 0x4F544E43,
    DATA = 0x41544144,
    DNAM = 0x4D414E44,
    DESC = 0x43534544,
    DELE = 0x454C4544,
    DODT = 0x54444F44,
    ENDT = 0x54444E45,
    ESCE = 0x45435345,
    ENIT = 0x54494E45,
    EFID = 0x44494645,
    EFIT = 0x54494645,
    EDID = 0x44494445,
    ENAM = 0x4D414E45,
    FADT = 0x54444146,
    FGGS = 0x53474746,
    FGGA = 0x41474746,
    FGTS = 0x53544746,
    FRMR = 0x524D5246,
    FLAG = 0x47414C46,
    FLTV = 0x56544C46,
    FULL = 0x4C4C5546,
    FNAM = 0x4D414E46,
    GNAM = 0x4D414E47,
    HEDR = 0x52444548,
    HSND = 0x444E5348,
    HVFX = 0x58465648,
    HNAM = 0x4D414E48,
    ICON = 0x4E4F4349,
    ICO2 = 0x324F4349,
    INDX = 0x58444E49,
    INTV = 0x56544E49,
    INCC = 0x43434E49,
    INAM = 0x4D414E49,
    ITEX = 0x58455449,
    IRDT = 0x54445249,
    JNAM = 0x4D414E4A,
    KNAM = 0x4D414E4B,
    LVLD = 0x444C564C,
    LVLF = 0x464C564C,
    LVLO = 0x4F4C564C,
    LNAM = 0x4D414E4C,
    LKDT = 0x54444B4C,
    LHDT = 0x5444484C,
    MODL = 0x4C444F4D,
    MODB = 0x42444F4D,
    MODT = 0x54444F4D,
    MNAM = 0x4D414E4D,
    MAST = 0x5453414D,
    MEDT = 0x5444454D,
    MOD2 = 0x32444F4D,
    MO2B = 0x42324F4D,
    MO2T = 0x54324F4D,
    MOD3 = 0x33444F4D,
    MO3B = 0x42334F4D,
    MO3T = 0x54334F4D,
    MOD4 = 0x34444F4D,
    MO4B = 0x42344F4D,
    MO4T = 0x54344F4D,
    MCDT = 0x5444434D,
    NPCS = 0x5343504E,
    NAM1 = 0x314D414E,
    NAME = 0x454D414E,
    NAM2 = 0x324D414E,
    NAM0 = 0x304D414E,
    NAM9 = 0x394D414E,
    NNAM = 0x4D414E4E,
    NAM5 = 0x354D414E,
    NPCO = 0x4F43504E,
    NPDT = 0x5444504E,
    OFST = 0x5453464F,
    ONAM = 0x4D414E4F,
    PGRP = 0x50524750,
    PGRR = 0x52524750,
    PFIG = 0x47494650,
    PFPC = 0x43504650,
    PKDT = 0x54444B50,
    PLDT = 0x54444C50,
    PSDT = 0x54445350,
    PTDT = 0x54445450,
    PBDT = 0x54444250,
    PTEX = 0x58455450,
    PGRC = 0x43524750,
    PGAG = 0x47414750,
    PGRL = 0x4C524750,
    PGRI = 0x49524750,
    PNAM = 0x4D414E50,
    QSDT = 0x54445351,
    QSTA = 0x41545351,
    QSTI = 0x49545351,
    QSTR = 0x52545351,
    QSTN = 0x4E545351,
    QSTF = 0x46545351,
    QNAM = 0x4D414E51,
    RIDT = 0x54444952,
    RNAM = 0x4D414E52,
    RCLR = 0x524C4352,
    RPLI = 0x494C5052,
    RPLD = 0x444C5052,
    RDAT = 0x54414452,
    RDOT = 0x544F4452,
    RDMP = 0x504D4452,
    RDGS = 0x53474452,
    RDMD = 0x444D4452,
    RDSD = 0x44534452,
    RDWT = 0x54574452,
    RADT = 0x54444152,
    RGNN = 0x4E4E4752,
    SCIT = 0x54494353,
    SCRI = 0x49524353,
    SCHR = 0x52484353,
    SCDA = 0x41444353,
    SCTX = 0x58544353,
    SCRO = 0x4F524353,
    SOUL = 0x4C554F53,
    SLCP = 0x50434C53,
    SNAM = 0x4D414E53,
    SPLO = 0x4F4C5053,
    SCHD = 0x44484353,
    SCVR = 0x52564353,
    SCDT = 0x54444353,
    SLSD = 0x44534C53,
    SCRV = 0x56524353,
    SCPT = 0x54504353,
    STRV = 0x56525453,
    SKDT = 0x54444B53,
    SNDX = 0x58444E53,
    SNDD = 0x44444E53,
    SPIT = 0x54495053,
    SPDT = 0x54445053,
    TNAM = 0x4D414E54,
    TPIC = 0x43495054,
    TRDT = 0x54445254,
    TCLT = 0x544C4354,
    TCLF = 0x464C4354,
    TEXT = 0x54584554,
    UNAM = 0x4D414E55,
    VNAM = 0x4D414E56,
    VTXT = 0x54585456,
    VNML = 0x4C4D4E56,
    VHGT = 0x54474856,
    VCLR = 0x524C4356,
    VTEX = 0x58455456,
    WLST = 0x54534C57,
    WNAM = 0x4D414E57,
    WHGT = 0x54474857,
    WPDT = 0x54445057,
    WEAT = 0x54414557,
    XTEL = 0x4C455458,
    XLOC = 0x434F4C58,
    XTRG = 0x47525458,
    XSED = 0x44455358,
    XCHG = 0x47484358,
    XHLT = 0x544C4858,
    XLCM = 0x4D434C58,
    XRTM = 0x4D545258,
    XACT = 0x54434158,
    XCNT = 0x544E4358,
    XMRK = 0x4B524D58,
    XXXX = 0x58585858,
    XOWN = 0x4E574F58,
    XRNK = 0x4B4E5258,
    XGLB = 0x424C4758,
    XESP = 0x50534558,
    XSCL = 0x4C435358,
    XRGD = 0x44475258,
    XPCI = 0x49435058,
    XLOD = 0x444F4C58,
    XMRC = 0x43524D58,
    XHRS = 0x53524858,
    XSOL = 0x4C4F5358,
    XCLC = 0x434C4358,
    XCLL = 0x4C4C4358,
    XCLW = 0x574C4358,
    XCLR = 0x524C4358,
    XCMT = 0x544D4358,
    XCCM = 0x4D434358,
    XCWT = 0x54574358,
    XNAM = 0x4D414E58,
}

#endregion

#region Header

public interface IHaveMODL {
    MODLGroup MODL { get; }
}

public class Header : BinaryReader {
    [Flags]
    public enum EsmFlags : uint {
        EsmFile = 0x00000001,               // ESM file. (TES4.HEDR record only.)
        Deleted = 0x00000020,               // Deleted
        R00 = 0x00000040,                   // Constant / (REFR) Hidden From Local Map (Needs Confirmation: Related to shields)
        R01 = 0x00000100,                   // Must Update Anims / (REFR) Inaccessible
        R02 = 0x00000200,                   // (REFR) Hidden from local map / (ACHR) Starts dead / (REFR) MotionBlurCastsShadows
        R03 = 0x00000400,                   // Quest item / Persistent reference / (LSCR) Displays in Main Menu
        InitiallyDisabled = 0x00000800,     // Initially disabled
        Ignored = 0x00001000,               // Ignored
        VisibleWhenDistant = 0x00008000,    // Visible when distant
        R04 = 0x00010000,                   // (ACTI) Random Animation Start
        R05 = 0x00020000,                   // (ACTI) Dangerous / Off limits (Interior cell) Dangerous Can't be set withough Ignore Object Interaction
        Compressed = 0x00040000,            // Data is compressed
        CantWait = 0x00080000,              // Can't wait
                                            // tes5
        R06 = 0x00100000,                   // (ACTI) Ignore Object Interaction Ignore Object Interaction Sets Dangerous Automatically
        IsMarker = 0x00800000,              // Is Marker
        R07 = 0x02000000,                   // (ACTI) Obstacle / (REFR) No AI Acquire
        NavMesh01 = 0x04000000,             // NavMesh Gen - Filter
        NavMesh02 = 0x08000000,             // NavMesh Gen - Bounding Box
        R08 = 0x10000000,                   // (FURN) Must Exit to Talk / (REFR) Reflected By Auto Water
        R09 = 0x20000000,                   // (FURN/IDLM) Child Can Use / (REFR) Don't Havok Settle
        R10 = 0x40000000,                   // NavMesh Gen - Ground / (REFR) NoRespawn
        R11 = 0x80000000,                   // (REFR) MultiBound
    }

    public override string ToString() => $"{Type}:{Group?.Type}";
    public string BinPath;
    public FormType Format;
    public Header Parent;
    public FormType Type;
    public uint DataSize;
    public EsmFlags Flags;
    public bool Compressed => (Flags & EsmFlags.Compressed) != 0;
    public uint Id;
    public GroupHeader Group;
    public long Position;

    public Header(BinaryReader b, string binPath, FormType format, Header parent = default) : base(b.BaseStream) {
        var r = this;
        BinPath = binPath;
        Format = format;
        Parent = parent;
        Type = (FormType)r.ReadUInt32();
        if (Type == GRUP) {
            DataSize = (uint)(r.ReadUInt32() - (Format == TES4 ? 20 : 24));
            Group = new GroupHeader {
                Header = this,
                Label = (FormType)r.ReadUInt32(),
                Type = (GroupHeader.GroupType)r.ReadInt32(),
                DataSize = DataSize
            };
            r.ReadUInt32(); // stamp | stamp + unknown
            if (Format != TES4) r.ReadUInt32(); // version + unknown
            Position = Group.Position = r.Tell();
            return;
        }
        DataSize = r.ReadUInt32();
        if (Format == TES3) r.ReadUInt32(); // unknown
        while (true) {
            Flags = (EsmFlags)r.ReadUInt32();
            if (Format == TES3) break;
            Id = r.ReadUInt32();
            r.ReadUInt32();
            if (Format == TES4) break;
            r.ReadUInt32();
            if (Format == TES5) break;
        }
        Position = r.Tell();
    }
}

public class GroupHeader {
    public enum GroupType : int {
        Top = 0,                    // Label: Record type
        WorldChildren,              // Label: Parent (WRLD)
        InteriorCellBlock,          // Label: Block number
        InteriorCellSubBlock,       // Label: Sub-block number
        ExteriorCellBlock,          // Label: Grid Y, X (Note the reverse order)
        ExteriorCellSubBlock,       // Label: Grid Y, X (Note the reverse order)
        CellChildren,               // Label: Parent (CELL)
        TopicChildren,              // Label: Parent (DIAL)
        CellPersistentChilden,      // Label: Parent (CELL)
        CellTemporaryChildren,      // Label: Parent (CELL)
        CellVisibleDistantChildren, // Label: Parent (CELL)
    }

    public override string ToString() => $"{Label}";
    public Header Header;
    public FormType Label;
    public GroupType Type;
    public uint DataSize;
    public long Position;
}

public class FieldHeader(Header r) {
    public override string ToString() => $"{Type}";
    public FieldType Type = (FieldType)r.ReadUInt32();
    public int DataSize = (int)(r.Format == TES3 ? r.ReadUInt32() : r.ReadUInt16());
}

#endregion

#region Fields

public struct STRVField { public override readonly string ToString() => Value; public string Value; }
public struct FILEField { public override readonly string ToString() => Value; public string Value; }
public struct DATVField { public override readonly string ToString() => "DATV"; public bool B; public int I; public float F; public string S; }
public struct FLTVField { public override readonly string ToString() => $"{Value}"; public static (string, int) Struct = ("<f", 4); public float Value; }
public struct BYTEField { public override readonly string ToString() => $"{Value}"; public static (string, int) Struct = ("<C", 1); public byte Value; }
public struct IN16Field { public override readonly string ToString() => $"{Value}"; public static (string, int) Struct = ("<h", 2); public short Value; }
public struct UI16Field { public override readonly string ToString() => $"{Value}"; public static (string, int) Struct = ("<H", 2); public ushort Value; }
public struct IN32Field { public override readonly string ToString() => $"{Value}"; public static (string, int) Struct = ("<i", 4); public int Value; }
public struct UI32Field { public override readonly string ToString() => $"{Value}"; public static (string, int) Struct = ("<I", 4); public uint Value; }
public struct INTVField { public override readonly string ToString() => $"{Value}"; public static (string, int) Struct = ("<q", 8); public long Value; public UI16Field AsUI16Field => new() { Value = (ushort)Value }; }
public struct CREFField { public override readonly string ToString() => $"{Color}"; public static (string, int) Struct = ("<4c", 4); public ByteColor4 Color; }
public struct BYTVField { public override readonly string ToString() => $"BYTS"; public byte[] Value; }
public struct UNKNField { public override readonly string ToString() => $"UNKN"; public byte[] Value; }
public class MODLGroup(Header r, int dataSize) {
    public override string ToString() => $"{Value}";
    public string Value = r.ReadFUString(dataSize);
    public float Bound;
    public byte[] Textures; // Texture Files Hashes
    public object MODBField(Header r, int dataSize) => Bound = r.ReadSingle();
    public object MODTField(Header r, int dataSize) => Textures = r.ReadBytes(dataSize);
}
public struct CNTOField {
    public override readonly string ToString() => $"{Item}";
    public uint ItemCount; // Number of the item
    public Ref<Record> Item; // The ID of the item
    public CNTOField(Header r, int dataSize) {
        if (r.Format == TES3) { ItemCount = r.ReadUInt32(); Item = new Ref<Record>(r.ReadFAString(32)); return; }
        Item = new Ref<Record>(r.ReadUInt32()); ItemCount = r.ReadUInt32();
    }
}

#endregion

#region Record

public class Record : IRecord {
    public static readonly Record Empty = new();
    static readonly Dictionary<FormType, (Func<Record> f, Func<int, bool> l)> Map = new() {
        { TES3, (() => new TES3Record(), x => true) },
        { TES4, (() => new TES4Record(), x => true) },
        // 0                                 
        { LTEX, (() => new LTEXRecord(), x => x > 0) },
        { STAT, (() => new STATRecord(), x => x > 0) },
        { CELL, (() => new CELLRecord(), x => x > 0) },
        { LAND, (() => new LANDRecord(), x => x > 0) },
        // 1                                 
        { DOOR, (() => new DOORRecord(), x => x > 1) },
        { MISC, (() => new MISCRecord(), x => x > 1) },
        { WEAP, (() => new WEAPRecord(), x => x > 1) },
        { CONT, (() => new CONTRecord(), x => x > 1) },
        { LIGH, (() => new LIGHRecord(), x => x > 1) },
        { ARMO, (() => new ARMORecord(), x => x > 1) },
        { CLOT, (() => new CLOTRecord(), x => x > 1) },
        { REPA, (() => new REPARecord(), x => x > 1) },
        { ACTI, (() => new ACTIRecord(), x => x > 1) },
        { APPA, (() => new APPARecord(), x => x > 1) },
        { LOCK, (() => new LOCKRecord(), x => x > 1) },
        { PROB, (() => new PROBRecord(), x => x > 1) },
        { INGR, (() => new INGRRecord(), x => x > 1) },
        { BOOK, (() => new BOOKRecord(), x => x > 1) },
        { ALCH, (() => new ALCHRecord(), x => x > 1) },
        { CREA, (() => new CREARecord(), x => x > 1 && true) },
        { NPC_, (() => new NPC_Record(), x => x > 1 && true) },
        // 2                                 
        { GMST, (() => new GMSTRecord(), x => x > 2) },
        { GLOB, (() => new GLOBRecord(), x => x > 2) },
        { SOUN, (() => new SOUNRecord(), x => x > 2) },
        { REGN, (() => new REGNRecord(), x => x > 2) },
        // 3                                 
        { CLAS, (() => new CLASRecord(), x => x > 3) },
        { SPEL, (() => new SPELRecord(), x => x > 3) },
        { BODY, (() => new BODYRecord(), x => x > 3) },
        { PGRD, (() => new PGRDRecord(), x => x > 3) },
        { INFO, (() => new INFORecord(), x => x > 3) },
        { DIAL, (() => new DIALRecord(), x => x > 3) },
        { SNDG, (() => new SNDGRecord(), x => x > 3) },
        { ENCH, (() => new ENCHRecord(), x => x > 3) },
        { SCPT, (() => new SCPTRecord(), x => x > 3) },
        { SKIL, (() => new SKILRecord(), x => x > 3) },
        { RACE, (() => new RACERecord(), x => x > 3) },
        { MGEF, (() => new MGEFRecord(), x => x > 3) },
        { LEVI, (() => new LEVIRecord(), x => x > 3) },
        { LEVC, (() => new LEVCRecord(), x => x > 3) },
        { BSGN, (() => new BSGNRecord(), x => x > 3) },
        { FACT, (() => new FACTRecord(), x => x > 3) },
        { SSCR, (() => new SSCRRecord(), x => x > 3) },
        // 4 - Oblivion                      
        { WRLD, (() => new WRLDRecord(), x => x > 0) },
        { ACRE, (() => new ACRERecord(), x => x > 1) },
        { ACHR, (() => new ACHRRecord(), x => x > 1) },
        { REFR, (() => new REFRRecord(), x => x > 1) },
        //                                   
        { AMMO, (() => new AMMORecord(), x => x > 4) },
        { ANIO, (() => new ANIORecord(), x => x > 4) },
        { CLMT, (() => new CLMTRecord(), x => x > 4) },
        { CSTY, (() => new CSTYRecord(), x => x > 4) },
        { EFSH, (() => new EFSHRecord(), x => x > 4) },
        { EYES, (() => new EYESRecord(), x => x > 4) },
        { FLOR, (() => new FLORRecord(), x => x > 4) },
        { FURN, (() => new FURNRecord(), x => x > 4) },
        { GRAS, (() => new GRASRecord(), x => x > 4) },
        { HAIR, (() => new HAIRRecord(), x => x > 4) },
        { IDLE, (() => new IDLERecord(), x => x > 4) },
        { KEYM, (() => new KEYMRecord(), x => x > 4) },
        { LSCR, (() => new LSCRRecord(), x => x > 4) },
        { LVLC, (() => new LVLCRecord(), x => x > 4) },
        { LVLI, (() => new LVLIRecord(), x => x > 4) },
        { LVSP, (() => new LVSPRecord(), x => x > 4) },
        { PACK, (() => new PACKRecord(), x => x > 4) },
        { QUST, (() => new QUSTRecord(), x => x > 4) },
        { ROAD, (() => new ROADRecord(), x => x > 4) },
        { SBSP, (() => new SBSPRecord(), x => x > 4) },
        { SGST, (() => new SGSTRecord(), x => x > 4) },
        { SLGM, (() => new SLGMRecord(), x => x > 4) },
        { TREE, (() => new TREERecord(), x => x > 4) },
        { WATR, (() => new WATRRecord(), x => x > 4) },
        { WTHR, (() => new WTHRRecord(), x => x > 4) },
        // 5 - Skyrim                        
        { AACT, (() => new AACTRecord(), x => x > 5) },
        { ADDN, (() => new ADDNRecord(), x => x > 5) },
        { ARMA, (() => new ARMARecord(), x => x > 5) },
        { ARTO, (() => new ARTORecord(), x => x > 5) },
        { ASPC, (() => new ASPCRecord(), x => x > 5) },
        { ASTP, (() => new ASTPRecord(), x => x > 5) },
        { AVIF, (() => new AVIFRecord(), x => x > 5) },
        { DLBR, (() => new DLBRRecord(), x => x > 5) },
        { DLVW, (() => new DLVWRecord(), x => x > 5) },
        { SNDR, (() => new SNDRRecord(), x => x > 5) },
    };
    public override string ToString() => $"{GetType().Name[..4]}: {EDID.Value}";
    internal Header Header;
    public uint Id => Header.Id;
    public STRVField EDID; // Editor ID

    /// <summary>
    /// Return an uninitialized subrecord to deserialize, or null to skip.
    /// </summary>
    /// <returns>Return an uninitialized subrecord to deserialize, or null to skip.</returns>
    public virtual object CreateField(Header r, FieldType type, int dataSize) => Empty;

    public void Read(Header r) {
        long start = r.Tell(), end = start + Header.DataSize;
        while (!r.AtEnd(end)) {
            var field = new FieldHeader(r);
            if (field.Type == FieldType.XXXX) {
                if (field.DataSize != 4) throw new InvalidOperationException();
                field.DataSize = (int)r.ReadUInt32();
                continue;
            }
            else if (field.Type == FieldType.OFST && Header.Type == WRLD) { r.Seek(end); continue; }
            var tell = r.Tell();
            if (CreateField(r, field.Type, field.DataSize) == Empty) { Log.Info($"Unsupported ESM record type: {Header.Type}:{field.Type}"); r.Skip(field.DataSize); continue; }
            r.EnsureAtEnd(tell + field.DataSize, $"Failed reading {Header.Type}:{field.Type} field data at offset {tell} in {r.BinPath} of {r.Tell() - tell - field.DataSize}");
        }
        r.EnsureAtEnd(end, $"Failed reading {Header.Type} record data at offset {start} in {r.BinPath}");
    }

    public static Record Factory(Header r, int level) {
        if (!Map.TryGetValue(r.Type, out var z)) { Log.Info($"Unsupported ESM record type: {r.Type}"); return null; }
        if (!z.l(level)) return null;
        var record = z.f();
        record.Header = r;
        return record;
    }
}

public readonly struct RefId<TRecord> where TRecord : Record {
    public override readonly string ToString() => $"{Type}:{Id}";
    public readonly uint Id;
    public readonly string Type => typeof(TRecord).Name[..4];
}

public readonly struct Ref<TRecord> where TRecord : Record {
    public override string ToString() => $"{Type}:{Name}{Id}";
    public readonly uint Id;
    public readonly string Name;
    public string Type => typeof(TRecord).Name[..4];
    public Ref(uint id) { Id = id; Name = null; }
    public Ref(string name) { Id = 0; Name = name; }
    Ref(uint id, string name) { Id = id; Name = name; }
    public Ref<TRecord> SetName(string name) => new(Id, name);
}

public struct RefField<TRecord>(Header r, int dataSize) where TRecord : Record {
    public override readonly string ToString() => $"{Value}";
    public Ref<TRecord> Value = dataSize == 4 ? new Ref<TRecord>(r.ReadUInt32()) : new Ref<TRecord>(r.ReadFAString(dataSize));
    public Ref<TRecord> SetName(string name) => Value = Value.SetName(name);
}

public struct Ref2Field<TRecord>(Header r, int dataSize) where TRecord : Record {
    public override readonly string ToString() => $"{Value1}z{Value2}";
    public Ref<TRecord> Value1 = new(r.ReadUInt32());
    public Ref<TRecord> Value2 = new(r.ReadUInt32());
}

#endregion

#region Record Group

public partial class RecordGroup(int level) {
    static int cellsLoaded = 0;
    public override string ToString() => Headers.First.Value.ToString();
    public FormType Label => Headers.First.Value.Label;
    public LinkedList<GroupHeader> Headers = [];
    public List<Record> Records = [];
    public List<RecordGroup> Groups;
    public Dictionary<uint, RecordGroup[]> GroupsByLabel;
    readonly int Level = level;
    int skip;

    public void AddHeader(GroupHeader h, bool load = true) {
        //Log.Info($"Read: {r.Label}");
        Headers.AddLast(h);
        if (load && h.Label != 0 && h.Type == GroupHeader.GroupType.Top)
            switch (h.Label) {
                case CELL or WRLD: Load(); break; // or DIAL
            }
    }

    public List<Record> Load(bool loadAll = false) {
        if (skip == Headers.Count) return Records;
        lock (Records) {
            if (skip == Headers.Count) return Records;
            foreach (var h in Headers.Skip(skip)) ReadGroup(h, loadAll);
            skip = Headers.Count;
            return Records;
        }
    }

    void ReadGroup(GroupHeader h, bool loadAll) {
        var r = h.Header;
        r.Seek(h.Position);
        var end = h.Position + h.DataSize;
        while (!r.AtEnd(end)) {
            var r2 = new Header(r, r.BinPath, r.Format);
            if (r2.Type == GRUP) {
                var group = ReadGRUP(r, r2.Group);
                if (loadAll) group.Load(loadAll);
                continue;
            }
            // HACK to limit cells loading
            if (r2.Type == CELL && cellsLoaded > int.MaxValue) { r.Skip(r2.DataSize); continue; }
            var record = Record.Factory(r2, Level);
            if (record == null) { r.Skip(r2.DataSize); continue; }
            ReadRecord(r, record, r2.Compressed);
            Records.Add(record);
            if (r2.Type == CELL) cellsLoaded++;
        }
        GroupsByLabel = Groups?.GroupBy(s => (uint)s.Label).ToDictionary(s => s.Key, s => s.ToArray());
    }

    RecordGroup ReadGRUP(Header r, GroupHeader h) {
        var nextPosition = r.Tell() + h.DataSize;
        Groups ??= [];
        var group = new RecordGroup(Level);
        group.AddHeader(h);
        Groups.Add(group);
        r.Seek(nextPosition);
        // print header path
        //var headerPath = string.Join("/", [.. GetHeaderPath([], r)]);
        //Log.Info($"Grup: {headerPath} {h.GroupType}");
        return group;
    }

    //static List<string> GetHeaderPath(List<string> b, Header header) {
    //    if (header.Parent != null) GetHeaderPath(b, header.Parent);
    //    //b.Add(header.GroupType != Header.HeaderGroupType.Top ? BitConverter.ToString(header.Label).Replace("-", string.Empty) : Encoding.ASCII.GetString(header.Label));
    //    return b;
    //}

    void ReadRecord(Header r, Record record, bool compressed) {
        //Log.Info($"Recd: {record.Header.Type}");
        if (!compressed) { record.Read(r); return; }
        var newDataSize = r.ReadUInt32();
        var newData = r.DecompressZlib2((int)record.Header.DataSize - 4, (int)newDataSize);
        // read record
        record.Header.Position = 0;
        record.Header.DataSize = newDataSize;
        using var r2 = new Header(new BinaryReader(new MemoryStream(newData)), r.BinPath, r.Format); record.Read(r2);
    }
}

// RecordGroup+WrldAndCell
partial class RecordGroup {
    internal HashSet<uint> EnsureCELLsByLabel;
    internal Dictionary<Int3, CELLRecord> CELLsById;
    internal Dictionary<Int3, LANDRecord> LANDsById;

    public RecordGroup[] EnsureWrldAndCell(Int3 cellId) {
        var cellBlockX = (short)(cellId.X >> 5);
        var cellBlockY = (short)(cellId.Y >> 5);
        var cellBlockIdx = new byte[4];
        Buffer.BlockCopy(BitConverter.GetBytes(cellBlockY), 0, cellBlockIdx, 0, 2);
        Buffer.BlockCopy(BitConverter.GetBytes(cellBlockX), 0, cellBlockIdx, 2, 2);
        Load();
        var cellBlockId = BitConverter.ToUInt32(cellBlockIdx);
        if (GroupsByLabel.TryGetValue(cellBlockId, out var cellBlocks))
            return cellBlocks.Select(x => x.EnsureCell(cellId)).ToArray();
        return null;
    }

    //= nxn[nbits] + 4x4[2bits] + 8x8[3bit]
    public RecordGroup EnsureCell(Int3 cellId) {
        EnsureCELLsByLabel ??= [];
        var cellBlockX = (short)(cellId.X >> 5);
        var cellBlockY = (short)(cellId.Y >> 5);
        var cellSubBlockX = (short)(cellId.X >> 3);
        var cellSubBlockY = (short)(cellId.Y >> 3);
        var cellSubBlockIdx = new byte[4];
        Buffer.BlockCopy(BitConverter.GetBytes(cellSubBlockY), 0, cellSubBlockIdx, 0, 2);
        Buffer.BlockCopy(BitConverter.GetBytes(cellSubBlockX), 0, cellSubBlockIdx, 2, 2);
        var cellSubBlockId = BitConverter.ToUInt32(cellSubBlockIdx);
        if (EnsureCELLsByLabel.Contains(cellSubBlockId)) return this;
        Load();
        CELLsById ??= [];
        LANDsById ??= cellId.Z >= 0 ? [] : null;
        if (GroupsByLabel.TryGetValue(cellSubBlockId, out var cellSubBlocks)) {
            // find cell
            var cellSubBlock = cellSubBlocks.Single();
            cellSubBlock.Load(true);
            foreach (var cell in cellSubBlock.Records.Cast<CELLRecord>()) {
                cell.GridId = new Int3(cell.XCLC.Value.GridX, cell.XCLC.Value.GridY, !cell.IsInterior ? cellId.Z : -1);
                CELLsById.Add(cell.GridId, cell);
                // find children
                if (cellSubBlock.GroupsByLabel.TryGetValue(cell.Id, out var cellChildren)) {
                    var cellChild = cellChildren.Single();
                    var cellTemporaryChildren = cellChild.Groups.Single(s => s.Headers.First().Type == GroupHeader.GroupType.CellTemporaryChildren);
                    foreach (var land in cellTemporaryChildren.Records.Cast<LANDRecord>()) {
                        land.GridId = new Int3(cell.XCLC.Value.GridX, cell.XCLC.Value.GridY, !cell.IsInterior ? cellId.Z : -1);
                        LANDsById.Add(land.GridId, land);
                    }
                }
            }
            EnsureCELLsByLabel.Add(cellSubBlockId);
            return this;
        }
        return null;
    }
}

#endregion

#region Extensions

public static class Extensions {
    public static TResult Then<T, TResult>(this Record s, T value, Func<T, TResult> then) => then(value);
    public static T AddX<T>(this IList<T> s, T value) { s.Add(value); return value; }
    public static IEnumerable<T> AddRangeX<T>(this List<T> s, IEnumerable<T> value) { s.AddRange(value); return value; }
    public static INTVField ReadINTV(this Header r, int length)
        => length switch {
            1 => new INTVField { Value = r.ReadByte() },
            2 => new INTVField { Value = r.ReadInt16() },
            4 => new INTVField { Value = r.ReadInt32() },
            8 => new INTVField { Value = r.ReadInt64() },
            _ => throw new NotImplementedException($"Tried to read an INTV subrecord with an unsupported size ({length})"),
        };
    public static DATVField ReadDATV(this Header r, int length, char type)
        => type switch {
            'b' => new DATVField { B = r.ReadInt32() != 0 },
            'i' => new DATVField { I = r.ReadInt32() },
            'f' => new DATVField { F = r.ReadSingle() },
            's' => new DATVField { S = r.ReadFUString(length) },
            _ => throw new InvalidOperationException($"{type}"),
        };
    public static STRVField ReadSTRV(this Header r, int length) => new() { Value = r.ReadFUString(length) };
    public static STRVField ReadSTRV_ZPad(this Header r, int length) => new() { Value = r.ReadFAString(length) };
    public static FILEField ReadFILE(this Header r, int length) => new() { Value = r.ReadFUString(length) };
    public static BYTVField ReadBYTV(this Header r, int length) => new() { Value = r.ReadBytes(length) };
    public static UNKNField ReadUNKN(this Header r, int length) => new() { Value = r.ReadBytes(length) };
}

#endregion

#region Records

/// <summary>
/// AACT.Action - 0050
/// </summary>
public class AACTRecord : Record {
    public CREFField CNAM; // RGB Color

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        FieldType.CNAM => CNAM = r.ReadS<CREFField>(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// ACRE.Placed creature - 0400
/// </summary>
public class ACRERecord : Record {
    public RefField<Record> NAME; // Base
    public REFRRecord.DATAField DATA; // Position/Rotation
    public List<CELLRecord.XOWNGroup> XOWNs; // Ownership (optional)
    public REFRRecord.XESPField? XESP; // Enable Parent (optional)
    public FLTVField XSCL; // Scale (optional)
    public BYTVField? XRGD; // Ragdoll Data (optional)

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        FieldType.NAME => NAME = new RefField<Record>(r, dataSize),
        FieldType.DATA => DATA = new REFRRecord.DATAField(r, dataSize),
        FieldType.XOWN => (XOWNs ??= []).AddX(new CELLRecord.XOWNGroup { XOWN = new RefField<Record>(r, dataSize) }),
        FieldType.XRNK => XOWNs.Last().XRNK = r.ReadS<IN32Field>(dataSize),
        FieldType.XGLB => XOWNs.Last().XGLB = new RefField<Record>(r, dataSize),
        FieldType.XESP => XESP = new REFRRecord.XESPField(r, dataSize),
        FieldType.XSCL => XSCL = r.ReadS<FLTVField>(dataSize),
        FieldType.XRGD => XRGD = r.ReadBYTV(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// ACHR.Actor Reference - 0450
/// </summary>
public class ACHRRecord : Record {
    public RefField<Record> NAME; // Base
    public REFRRecord.DATAField DATA; // Position/Rotation
    public RefField<CELLRecord>? XPCI; // Unused (optional)
    public BYTVField? XLOD; // Distant LOD Data (optional)
    public REFRRecord.XESPField? XESP; // Enable Parent (optional)
    public RefField<REFRRecord>? XMRC; // Merchant Container (optional)
    public RefField<ACRERecord>? XHRS; // Horse (optional)
    public FLTVField? XSCL; // Scale (optional)
    public BYTVField? XRGD; // Ragdoll Data (optional)

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        FieldType.NAME => NAME = new RefField<Record>(r, dataSize),
        FieldType.DATA => DATA = new REFRRecord.DATAField(r, dataSize),
        FieldType.XPCI => XPCI = new RefField<CELLRecord>(r, dataSize),
        FieldType.FULL => XPCI.Value.SetName(r.ReadFAString(dataSize)),
        FieldType.XLOD => XLOD = r.ReadBYTV(dataSize),
        FieldType.XESP => XESP = new REFRRecord.XESPField(r, dataSize),
        FieldType.XMRC => XMRC = new RefField<REFRRecord>(r, dataSize),
        FieldType.XHRS => XHRS = new RefField<ACRERecord>(r, dataSize),
        FieldType.XSCL => XSCL = r.ReadS<FLTVField>(dataSize),
        FieldType.XRGD => XRGD = r.ReadBYTV(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// ACTI.Activator - 3450
/// </summary>
public class ACTIRecord : Record, IHaveMODL {
    public MODLGroup MODL { get; set; } // Model Name
    public STRVField FULL; // Item Name
    public RefField<SCPTRecord> SCRI; // Script (Optional)
    // TES4
    public RefField<SOUNRecord> SNAM; // Sound (Optional)

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadSTRV(dataSize),
        FieldType.MODL => MODL = new MODLGroup(r, dataSize),
        FieldType.MODB => MODL.MODBField(r, dataSize),
        FieldType.MODT => MODL.MODTField(r, dataSize),
        FieldType.FULL or FieldType.FNAM => FULL = r.ReadSTRV(dataSize),
        FieldType.SCRI => SCRI = new RefField<SCPTRecord>(r, dataSize),
        // TES4
        FieldType.SNAM => SNAM = new RefField<SOUNRecord>(r, dataSize),
        _ => Empty,
    };
}

/// <summary>
/// ADDN-Addon Node - 0050
/// </summary>
public class ADDNRecord : Record {
    public CREFField CNAM; // RGB Color

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        FieldType.CNAM => CNAM = r.ReadS<CREFField>(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// ALCH.Potion - 3450
/// </summary>
public class ALCHRecord : Record, IHaveMODL {
    // TESX
    public class DATAField {
        public float Weight;
        public int Value;
        public int Flags; //: AutoCalc

        public DATAField(Header r, int dataSize) {
            Weight = r.ReadSingle();
            if (r.Format == TES3) {
                Value = r.ReadInt32();
                Flags = r.ReadInt32();
            }
        }
        public object ENITField(Header r, int dataSize) {
            Value = r.ReadInt32();
            Flags = r.ReadByte();
            r.Skip(3); // Unknown
            return true;
        }
    }
    // TES3
    public struct ENAMField(Header r, int dataSize) {
        public short EffectId = r.ReadInt16();
        public byte SkillId = r.ReadByte(); // for skill related effects, -1/0 otherwise
        public byte AttributeId = r.ReadByte(); // for attribute related effects, -1/0 otherwise
        public int Unknown1 = r.ReadInt32();
        public int Unknown2 = r.ReadInt32();
        public int Duration = r.ReadInt32();
        public int Magnitude = r.ReadInt32();
        public int Unknown4 = r.ReadInt32();
    }

    public MODLGroup MODL { get; set; } // Model
    public STRVField FULL; // Item Name
    public DATAField DATA; // Alchemy Data
    public ENAMField? ENAM; // Enchantment
    public FILEField ICON; // Icon
    public RefField<SCPTRecord>? SCRI; // Script (optional)
    // TES4
    public List<ENCHRecord.EFITField> EFITs = []; // Effect Data
    public List<ENCHRecord.SCITField> SCITs = []; // Script Effect Data

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadSTRV(dataSize),
        FieldType.MODL => MODL = new MODLGroup(r, dataSize),
        FieldType.MODB => MODL.MODBField(r, dataSize),
        FieldType.MODT => MODL.MODTField(r, dataSize),
        FieldType.FULL => SCITs.Count == 0 ? FULL = r.ReadSTRV(dataSize) : SCITs.Last().FULLField(r, dataSize),
        FieldType.FNAM => FULL = r.ReadSTRV(dataSize),
        FieldType.DATA or FieldType.ALDT => DATA = new DATAField(r, dataSize),
        FieldType.ENAM => ENAM = new ENAMField(r, dataSize),
        FieldType.ICON or FieldType.TEXT => ICON = r.ReadFILE(dataSize),
        FieldType.SCRI => SCRI = new RefField<SCPTRecord>(r, dataSize),
        // TES4
        FieldType.ENIT => DATA.ENITField(r, dataSize),
        FieldType.EFID => r.Skip(dataSize),
        FieldType.EFIT => EFITs.AddX(new ENCHRecord.EFITField(r, dataSize)),
        FieldType.SCIT => SCITs.AddX(new ENCHRecord.SCITField(r, dataSize)),
        _ => Empty,
    };
}

/// <summary>
/// AMMO.Ammo - 0450
/// </summary>
public class AMMORecord : Record, IHaveMODL {
    public struct DATAField(Header r, int dataSize) {
        public float Speed = r.ReadSingle();
        public uint Flags = r.ReadUInt32();
        public uint Value = r.ReadUInt32();
        public float Weight = r.ReadSingle();
        public ushort Damage = r.ReadUInt16();
    }

    public MODLGroup MODL { get; set; } // Model
    public STRVField FULL; // Item Name
    public FILEField? ICON; // Male Icon (optional)
    public RefField<ENCHRecord>? ENAM; // Enchantment ID (optional)
    public IN16Field? ANAM; // Enchantment Points (optional)
    public DATAField DATA; // Ammo Data

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        FieldType.MODL => MODL = new MODLGroup(r, dataSize),
        FieldType.MODB => MODL.MODBField(r, dataSize),
        FieldType.MODT => MODL.MODTField(r, dataSize),
        FieldType.FULL => FULL = r.ReadSTRV(dataSize),
        FieldType.ICON => ICON = r.ReadFILE(dataSize),
        FieldType.ENAM => ENAM = new RefField<ENCHRecord>(r, dataSize),
        FieldType.ANAM => ANAM = r.ReadS<IN16Field>(dataSize),
        FieldType.DATA => DATA = new DATAField(r, dataSize),
        _ => Empty,
    };
}

/// <summary>
/// ANIO.Animated Object - 0450
/// </summary>
public class ANIORecord : Record, IHaveMODL {
    public MODLGroup MODL { get; set; } // Model
    public RefField<IDLERecord> DATA; // IDLE Animation

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        FieldType.MODL => MODL = new MODLGroup(r, dataSize),
        FieldType.MODB => MODL.MODBField(r, dataSize),
        FieldType.DATA => DATA = new RefField<IDLERecord>(r, dataSize),
        _ => Empty,
    };
}

/// <summary>
/// APPA.Alchem Apparatus - 3450
/// </summary>
public class APPARecord : Record, IHaveMODL {
    // TESX
    public struct DATAField {
        public byte Type; // 0 = Mortar and Pestle, 1 = Albemic, 2 = Calcinator, 3 = Retort
        public int Value;
        public float Weight;
        public float Quality;

        public DATAField(Header r, int dataSize) {
            if (r.Format == TES3) {
                Type = (byte)r.ReadInt32();
                Quality = r.ReadSingle();
                Weight = r.ReadSingle();
                Value = r.ReadInt32();
                return;
            }
            Type = r.ReadByte();
            Value = r.ReadInt32();
            Weight = r.ReadSingle();
            Quality = r.ReadSingle();
        }
    }

    public MODLGroup MODL { get; set; } // Model
    public STRVField FULL; // Item Name
    public DATAField DATA; // Alchemy Data
    public FILEField ICON; // Inventory Icon
    public RefField<SCPTRecord> SCRI; // Script Name

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadSTRV(dataSize),
        FieldType.MODL => MODL = new MODLGroup(r, dataSize),
        FieldType.MODB => MODL.MODBField(r, dataSize),
        FieldType.MODT => MODL.MODTField(r, dataSize),
        FieldType.FULL or FieldType.FNAM => FULL = r.ReadSTRV(dataSize),
        FieldType.DATA or FieldType.AADT => DATA = new DATAField(r, dataSize),
        FieldType.ICON or FieldType.ITEX => ICON = r.ReadFILE(dataSize),
        FieldType.SCRI => SCRI = new RefField<SCPTRecord>(r, dataSize),
        _ => Empty,
    };
}

/// <summary>
/// ARMA.Armature (Model) - 0050
/// </summary>
public class ARMARecord : Record {
    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// ARMO.Armor - 3450
/// </summary>
public class ARMORecord : Record, IHaveMODL {
    // TESX
    public struct DATAField {
        public enum ARMOType { Helmet = 0, Cuirass, L_Pauldron, R_Pauldron, Greaves, Boots, L_Gauntlet, R_Gauntlet, Shield, L_Bracer, R_Bracer, }
        public short Armour;
        public int Value;
        public int Health;
        public float Weight;
        // TES3
        public int Type;
        public int EnchantPts;

        public DATAField(Header r, int dataSize) {
            if (r.Format == TES3) {
                Type = r.ReadInt32();
                Weight = r.ReadSingle();
                Value = r.ReadInt32();
                Health = r.ReadInt32();
                // TES3
                EnchantPts = r.ReadInt32();
                Armour = (short)r.ReadInt32();
                return;
            }
            Armour = r.ReadInt16();
            Value = r.ReadInt32();
            Health = r.ReadInt32();
            Weight = r.ReadSingle();
            // TES3
            Type = default;
            EnchantPts = default;
        }
    }

    public MODLGroup MODL { get; set; } // Male Biped Model
    public STRVField FULL; // Item Name
    public FILEField ICON; // Male Icon
    public DATAField DATA; // Armour Data
    public RefField<SCPTRecord>? SCRI; // Script Name (optional)
    public RefField<ENCHRecord>? ENAM; // Enchantment FormId (optional)
    // TES3
    public List<CLOTRecord.INDXFieldGroup> INDXs = []; // Body Part Index
    // TES4
    public UI32Field BMDT; // Flags
    public MODLGroup MOD2; // Male World Model (optional)
    public MODLGroup MOD3; // Female Biped Model (optional)
    public MODLGroup MOD4; // Female World Model (optional)
    public FILEField? ICO2; // Female Icon (optional)
    public IN16Field? ANAM; // Enchantment Points (optional)

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadSTRV(dataSize),
        FieldType.MODL => MODL = new MODLGroup(r, dataSize),
        FieldType.MODB => MODL.MODBField(r, dataSize),
        FieldType.MODT => MODL.MODTField(r, dataSize),
        FieldType.FULL or FieldType.FNAM => FULL = r.ReadSTRV(dataSize),
        FieldType.DATA or FieldType.AODT => DATA = new DATAField(r, dataSize),
        FieldType.ICON or FieldType.ITEX => ICON = r.ReadFILE(dataSize),
        FieldType.SCRI => SCRI = new RefField<SCPTRecord>(r, dataSize),
        FieldType.ENAM => ENAM = new RefField<ENCHRecord>(r, dataSize),
        // TES3
        FieldType.INDX => INDXs.AddX(new CLOTRecord.INDXFieldGroup { INDX = r.ReadINTV(dataSize) }),
        FieldType.BNAM => INDXs.Last().BNAM = r.ReadSTRV(dataSize),
        FieldType.CNAM => INDXs.Last().CNAM = r.ReadSTRV(dataSize),
        // TES4
        FieldType.BMDT => BMDT = r.ReadS<UI32Field>(dataSize),
        FieldType.MOD2 => MOD2 = new MODLGroup(r, dataSize),
        FieldType.MO2B => MOD2.MODBField(r, dataSize),
        FieldType.MO2T => MOD2.MODTField(r, dataSize),
        FieldType.MOD3 => MOD3 = new MODLGroup(r, dataSize),
        FieldType.MO3B => MOD3.MODBField(r, dataSize),
        FieldType.MO3T => MOD3.MODTField(r, dataSize),
        FieldType.MOD4 => MOD4 = new MODLGroup(r, dataSize),
        FieldType.MO4B => MOD4.MODBField(r, dataSize),
        FieldType.MO4T => MOD4.MODTField(r, dataSize),
        FieldType.ICO2 => ICO2 = r.ReadFILE(dataSize),
        FieldType.ANAM => ANAM = r.ReadS<IN16Field>(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// ARTO.Art Object - 0050
/// </summary>
public class ARTORecord : Record {
    public CREFField CNAM; // RGB Color

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        FieldType.CNAM => CNAM = r.ReadS<CREFField>(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// ASPC.Acoustic Space - 0050
/// </summary>
public class ASPCRecord : Record {
    public CREFField CNAM; // RGB Color

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        FieldType.CNAM => CNAM = r.ReadS<CREFField>(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// ASTP.Association Type - 0050
/// </summary>
public class ASTPRecord : Record {
    public CREFField CNAM; // RGB Color

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        FieldType.CNAM => CNAM = r.ReadS<CREFField>(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// AVIF.Actor Values_Perk Tree Graphics - 0050
/// </summary>
public class AVIFRecord : Record {
    public CREFField CNAM; // RGB Color

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        FieldType.CNAM => CNAM = r.ReadS<CREFField>(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// BODY.Body - 3000
/// </summary>
public class BODYRecord : Record, IHaveMODL {
    public struct BYDTField(Header r, int dataSize) {
        public byte Part = r.ReadByte();
        public byte Vampire = r.ReadByte();
        public byte Flags = r.ReadByte();
        public byte PartType = r.ReadByte();
    }

    public MODLGroup MODL { get; set; } // NIF Model
    public STRVField FNAM; // Body Name
    public BYDTField BYDT;

    public override object CreateField(Header r, FieldType type, int dataSize) => r.Format == TES3
        ? type switch {
            FieldType.NAME => EDID = r.ReadSTRV(dataSize),
            FieldType.MODL => MODL = new MODLGroup(r, dataSize),
            FieldType.FNAM => FNAM = r.ReadSTRV(dataSize),
            FieldType.BYDT => BYDT = new BYDTField(r, dataSize),
            _ => Empty,
        }
        : null;
}

/// <summary>
/// BOOK.Book - 3450
/// </summary>
public class BOOKRecord : Record, IHaveMODL {
    public struct DATAField {
        public byte Flags; //: Scroll - (1 is scroll, 0 not)
        public byte Teaches; //: SkillId - (-1 is no skill)
        public int Value;
        public float Weight;
        // TES3
        public int EnchantPts;

        public DATAField(Header r, int dataSize) {
            if (r.Format == TES3) {
                Weight = r.ReadSingle();
                Value = r.ReadInt32();
                Flags = (byte)r.ReadInt32();
                Teaches = (byte)r.ReadInt32();
                EnchantPts = r.ReadInt32();
                return;
            }
            Flags = r.ReadByte();
            Teaches = r.ReadByte();
            Value = r.ReadInt32();
            Weight = r.ReadSingle();
            EnchantPts = default;
        }
    }

    public MODLGroup MODL { get; set; } // Model (optional)
    public STRVField FULL; // Item Name
    public DATAField DATA; // Book Data
    public STRVField DESC; // Book Text
    public FILEField ICON; // Inventory Icon (optional)
    public RefField<SCPTRecord> SCRI; // Script Name (optional)
    public RefField<ENCHRecord> ENAM; // Enchantment FormId (optional)
    // TES4
    public IN16Field? ANAM; // Enchantment points (optional)

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadSTRV(dataSize),
        FieldType.MODL => MODL = new MODLGroup(r, dataSize),
        FieldType.MODB => MODL.MODBField(r, dataSize),
        FieldType.MODT => MODL.MODTField(r, dataSize),
        FieldType.FULL or FieldType.FNAM => FULL = r.ReadSTRV(dataSize),
        FieldType.DATA or FieldType.BKDT => DATA = new DATAField(r, dataSize),
        FieldType.ICON or FieldType.ITEX => ICON = r.ReadFILE(dataSize),
        FieldType.SCRI => SCRI = new RefField<SCPTRecord>(r, dataSize),
        FieldType.DESC or FieldType.TEXT => DESC = r.ReadSTRV(dataSize),
        FieldType.ENAM => ENAM = new RefField<ENCHRecord>(r, dataSize),
        // TES4
        FieldType.ANAM => ANAM = r.ReadS<IN16Field>(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// BSGN.Birthsign - 3400
/// </summary>
public class BSGNRecord : Record {
    public STRVField FULL; // Sign Name
    public FILEField ICON; // Texture
    public STRVField DESC; // Description
    public List<STRVField> NPCSs = []; // TES3: Spell/ability
    public List<RefField<Record>> SPLOs = []; // TES4: (points to a SPEL or LVSP record)

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadSTRV(dataSize),
        FieldType.FULL or FieldType.FNAM => FULL = r.ReadSTRV(dataSize),
        FieldType.ICON or FieldType.TNAM => ICON = r.ReadFILE(dataSize),
        FieldType.DESC => DESC = r.ReadSTRV(dataSize),
        FieldType.SPLO => (SPLOs ??= []).AddX(new RefField<Record>(r, dataSize)),
        FieldType.NPCS => (NPCSs ??= []).AddX(r.ReadSTRV(dataSize)),
        _ => Empty,
    };
}

/// <summary>
/// CELL.Cell - 3450
/// </summary>
public unsafe class CELLRecord : Record, ICellRecord {
    [Flags]
    public enum CELLFlags : ushort {
        Interior = 0x0001,
        HasWater = 0x0002,
        InvertFastTravel = 0x0004, //: IllegalToSleepHere
        BehaveLikeExterior = 0x0008, //: BehaveLikeExterior (Tribunal), Force hide land (exterior cell) / Oblivion interior (interior cell)
        Unknown1 = 0x0010,
        PublicArea = 0x0020, // Public place
        HandChanged = 0x0040,
        ShowSky = 0x0080, // Behave like exterior
        UseSkyLighting = 0x0100,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct XCLCField {
        public override readonly string ToString() => $"{GridX}z{GridY}";
        public static Dictionary<int, string> Struct = new() { [8] = "<2i", [12] = "<2iI" };
        public int GridX;
        public int GridY;
        public uint Flags;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct XCLLField {
        public static Dictionary<int, string> Struct = new() { [16] = "<4c4c4cf", [36] = "<4c4c4cc2f2i2f", [40] = "<4c4c4c2f2i3f" };
        public ByteColor4 AmbientColor;
        public ByteColor4 DirectionalColor; // SunlightColor
        public ByteColor4 FogColor;
        public float FogNear; // FogDensity
        // TES4
        public float FogFar;
        public int DirectionalRotationXY;
        public int DirectionalRotationZ;
        public float DirectionalFade;
        public float FogClipDist;
        // TES5
        public float FogPow;
    }

    public class XOWNGroup {
        public RefField<Record> XOWN;
        public IN32Field XRNK; // Faction rank
        public RefField<Record> XGLB;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct XYZAField {
        public static (string, int) Struct = ("<3f3f", 24);
        public Float3 Position;
        public Float3 EulerAngles;
    }

    public class RefObj {
        public override string ToString() => $"CREF: {EDID.Value}";
        public UI32Field? FRMR; // Object Index (starts at 1)
        // This is used to uniquely identify objects in the cell. For new files the index starts at 1 and is incremented for each new object added. For modified objects the index is kept the same.
        public STRVField EDID; // Object ID
        public FLTVField? XSCL; // Scale (Static)
        public IN32Field? DELE; // Indicates that the reference is deleted.
        public XYZAField? DODT; // XYZ Pos, XYZ Rotation of exit
        public STRVField DNAM; // Door exit name (Door objects)
        public FLTVField? FLTV; // Follows the DNAM optionally, lock level
        public STRVField KNAM; // Door key
        public STRVField TNAM; // Trap name
        public BYTEField? UNAM; // Reference Blocked (only occurs once in MORROWIND.ESM)
        public STRVField ANAM; // Owner ID string
        public STRVField BNAM; // Global variable/rank ID
        public IN32Field? INTV; // Number of uses, occurs even for objects that don't use it
        public UI32Field? NAM9; // Unknown
        public STRVField XSOL; // Soul Extra Data (ID string of creature)
        public XYZAField DATA; // Ref Position Data
        // TES?
        public STRVField CNAM; // Unknown
        public UI32Field? NAM0; // Unknown
        public IN32Field? XCHG; // Unknown
        public IN32Field? INDX; // Unknown
    }

    public STRVField FULL; // Full Name / TES3:RGNN - Region name
    public UI16Field DATA; // Flags
    public XCLCField? XCLC; // Cell Data (only used for exterior cells)
    public XCLLField? XCLL; // Lighting (only used for interior cells)
    public FLTVField? XCLW; // Water Height
    // TES3
    public UI32Field? NAM0; // Number of objects in cell in current file (Optional)
    public INTVField INTV; // Unknown
    public CREFField? NAM5; // Map Color (COLORREF)
    // TES4
    public RefField<REGNRecord>[] XCLRs; // Regions
    public BYTEField? XCMT; // Music (optional)
    public RefField<CLMTRecord>? XCCM; // Climate
    public RefField<WATRRecord>? XCWT; // Water
    public List<XOWNGroup> XOWNs = []; // Ownership

    // Referenced Object Data Grouping
    public bool InFRMR = false;
    public List<RefObj> RefObjs = [];
    RefObj _lastRef;

    public bool IsInterior => (DATA.Value & 0x01) == 0x01;
    public Int3 GridId; // => new Int3(XCLC.Value.GridX, XCLC.Value.GridY, !IsInterior ? 0 : -1);
    public Colorf? AmbientLight => XCLL != null ? (Colorf?)XCLL.Value.AmbientColor.AsColor32 : null;

    public override object CreateField(Header r, FieldType type, int dataSize) {
        //Console.WriteLine($"   {type}");
        if (!InFRMR && type == FieldType.FRMR) InFRMR = true;
        if (!InFRMR)
            return type switch {
                FieldType.EDID or FieldType.NAME => EDID = r.ReadSTRV(dataSize),
                FieldType.FULL or FieldType.RGNN => FULL = r.ReadSTRV(dataSize),
                FieldType.DATA => (DATA = r.ReadINTV(r.Format == TES3 ? 4 : dataSize).AsUI16Field, r.Format == TES3 ? XCLC = r.ReadS<XCLCField>(r.Format == TES3 ? 8 : dataSize) : null),
                FieldType.XCLC => XCLC = r.ReadS<XCLCField>(r.Format == TES3 ? 8 : dataSize),
                FieldType.XCLL or FieldType.AMBI => XCLL = r.ReadS<XCLLField>(dataSize),
                FieldType.XCLW or FieldType.WHGT => XCLW = r.ReadS<FLTVField>(dataSize),
                // TES3
                FieldType.NAM0 => NAM0 = r.ReadS<UI32Field>(dataSize),
                FieldType.INTV => INTV = r.ReadINTV(dataSize),
                FieldType.NAM5 => NAM5 = r.ReadS<CREFField>(dataSize),
                // TES4
                FieldType.XCLR => XCLRs = r.ReadFArray(z => new RefField<REGNRecord>(r, 4), dataSize >> 2),
                FieldType.XCMT => XCMT = r.ReadS<BYTEField>(dataSize),
                FieldType.XCCM => XCCM = new RefField<CLMTRecord>(r, dataSize),
                FieldType.XCWT => XCWT = new RefField<WATRRecord>(r, dataSize),
                FieldType.XOWN => XOWNs.AddX(new XOWNGroup { XOWN = new RefField<Record>(r, dataSize) }),
                FieldType.XRNK => XOWNs.Last().XRNK = r.ReadS<IN32Field>(dataSize),
                FieldType.XGLB => XOWNs.Last().XGLB = new RefField<Record>(r, dataSize),
                _ => Empty,
            };
        // Referenced Object Data Grouping
        return type switch {
            // RefObjDataGroup sub-records
            FieldType.FRMR => RefObjs.AddX(_lastRef = new RefObj()).FRMR = r.ReadS<UI32Field>(dataSize),
            FieldType.NAME => _lastRef.EDID = r.ReadSTRV(dataSize),
            FieldType.XSCL => _lastRef.XSCL = r.ReadS<FLTVField>(dataSize),
            FieldType.DODT => _lastRef.DODT = r.ReadS<XYZAField>(dataSize),
            FieldType.DNAM => _lastRef.DNAM = r.ReadSTRV(dataSize),
            FieldType.FLTV => _lastRef.FLTV = r.ReadS<FLTVField>(dataSize),
            FieldType.KNAM => _lastRef.KNAM = r.ReadSTRV(dataSize),
            FieldType.TNAM => _lastRef.TNAM = r.ReadSTRV(dataSize),
            FieldType.UNAM => _lastRef.UNAM = r.ReadS<BYTEField>(dataSize),
            FieldType.ANAM => _lastRef.ANAM = r.ReadSTRV(dataSize),
            FieldType.BNAM => _lastRef.BNAM = r.ReadSTRV(dataSize),
            FieldType.INTV => _lastRef.INTV = r.ReadS<IN32Field>(dataSize),
            FieldType.NAM9 => _lastRef.NAM9 = r.ReadS<UI32Field>(dataSize),
            FieldType.XSOL => _lastRef.XSOL = r.ReadSTRV(dataSize),
            FieldType.DATA => _lastRef.DATA = r.ReadS<XYZAField>(dataSize),
            // TES?
            FieldType.CNAM => _lastRef.CNAM = r.ReadSTRV(dataSize),
            FieldType.NAM0 => _lastRef.NAM0 = r.ReadS<UI32Field>(dataSize),
            FieldType.XCHG => _lastRef.XCHG = r.ReadS<IN32Field>(dataSize),
            FieldType.INDX => _lastRef.INDX = r.ReadS<IN32Field>(dataSize),
            _ => Empty,
        };
    }
}

/// <summary>
/// CLAS.Class - 3450
/// </summary>
public class CLASRecord : Record {
    public struct DATAField {
        //wbArrayS('Primary Attributes', wbInteger('Primary Attribute', itS32, wbActorValueEnum), 2),
        //wbInteger('Specialization', itU32, wbSpecializationEnum),
        //wbArrayS('Major Skills', wbInteger('Major Skill', itS32, wbActorValueEnum), 7),
        //wbInteger('Flags', itU32, wbFlags(['Playable', 'Guard'])),
        //wbInteger('Buys/Sells and Services', itU32, wbServiceFlags),
        //wbInteger('Teaches', itS8, wbSkillEnum),
        //wbInteger('Maximum training level', itU8),
        //wbInteger('Unused', itU16)
        public DATAField(Header r, int dataSize) => r.Skip(dataSize);
    }

    public STRVField FULL; // Name
    public STRVField DESC; // Description
    // TES4
    public STRVField? ICON; // Icon (Optional)
    public DATAField DATA; // Data

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadSTRV(dataSize),
        FieldType.FULL or FieldType.FNAM => FULL = r.ReadSTRV(dataSize),
        FieldType.CLDT => r.Skip(dataSize), // TES3
        FieldType.DESC => DESC = r.ReadSTRV(dataSize),
        // TES4
        FieldType.ICON => ICON = r.ReadSTRV(dataSize),
        FieldType.DATA => DATA = new DATAField(r, dataSize),
        _ => Empty,
    };
}

/// <summary>
/// CLOT.Clothing - 3450
/// </summary>
public class CLOTRecord : Record, IHaveMODL {
    // TESX
    public struct DATAField {
        public enum CLOTType { Pants = 0, Shoes, Shirt, Belt, Robe, R_Glove, L_Glove, Skirt, Ring, Amulet }
        public int Value;
        public float Weight;
        // TES3
        public int Type;
        public short EnchantPts;

        public DATAField(Header r, int dataSize) {
            if (r.Format == TES3) {
                Type = r.ReadInt32();
                Weight = r.ReadSingle();
                Value = r.ReadInt16();
                EnchantPts = r.ReadInt16();
                return;
            }
            Value = r.ReadInt32();
            Weight = r.ReadSingle();
            Type = 0;
            EnchantPts = 0;
        }
    }

    public class INDXFieldGroup {
        public override string ToString() => $"{INDX.Value}: {BNAM.Value}";
        public INTVField INDX;
        public STRVField BNAM;
        public STRVField CNAM;
    }

    public MODLGroup MODL { get; set; } // Model Name
    public STRVField FULL; // Item Name
    public DATAField DATA; // Clothing Data
    public FILEField ICON; // Male Icon
    public STRVField ENAM; // Enchantment Name
    public RefField<SCPTRecord> SCRI; // Script Name
                                      // TES3
    public List<INDXFieldGroup> INDXs = []; // Body Part Index (Moved to Race)
                                            // TES4
    public UI32Field BMDT; // Clothing Flags
    public MODLGroup MOD2; // Male world model (optional)
    public MODLGroup MOD3; // Female biped (optional)
    public MODLGroup MOD4; // Female world model (optional)
    public FILEField? ICO2; // Female icon (optional)
    public IN16Field? ANAM; // Enchantment points (optional)

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadSTRV(dataSize),
        FieldType.MODL => MODL = new MODLGroup(r, dataSize),
        FieldType.MODB => MODL.MODBField(r, dataSize),
        FieldType.MODT => MODL.MODTField(r, dataSize),
        FieldType.FULL or FieldType.FNAM => FULL = r.ReadSTRV(dataSize),
        FieldType.DATA or FieldType.CTDT => DATA = new DATAField(r, dataSize),
        FieldType.ICON or FieldType.ITEX => ICON = r.ReadFILE(dataSize),
        FieldType.INDX => INDXs.AddX(new INDXFieldGroup { INDX = r.ReadINTV(dataSize) }),
        FieldType.BNAM => INDXs.Last().BNAM = r.ReadSTRV(dataSize),
        FieldType.CNAM => INDXs.Last().CNAM = r.ReadSTRV(dataSize),
        FieldType.ENAM => ENAM = r.ReadSTRV(dataSize),
        FieldType.SCRI => SCRI = new RefField<SCPTRecord>(r, dataSize),
        FieldType.BMDT => BMDT = r.ReadS<UI32Field>(dataSize),
        FieldType.MOD2 => MOD2 = new MODLGroup(r, dataSize),
        FieldType.MO2B => MOD2.MODBField(r, dataSize),
        FieldType.MO2T => MOD2.MODTField(r, dataSize),
        FieldType.MOD3 => MOD3 = new MODLGroup(r, dataSize),
        FieldType.MO3B => MOD3.MODBField(r, dataSize),
        FieldType.MO3T => MOD3.MODTField(r, dataSize),
        FieldType.MOD4 => MOD4 = new MODLGroup(r, dataSize),
        FieldType.MO4B => MOD4.MODBField(r, dataSize),
        FieldType.MO4T => MOD4.MODTField(r, dataSize),
        FieldType.ICO2 => ICO2 = r.ReadFILE(dataSize),
        FieldType.ANAM => ANAM = r.ReadS<IN16Field>(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// CLMT.Climate - 0450
/// </summary>
public class CLMTRecord : Record, IHaveMODL {
    public struct WLSTField(Header r, int dataSize) {
        public Ref<WTHRRecord> Weather = new(r.ReadUInt32());
        public int Chance = r.ReadInt32();
    }

    public struct TNAMField(Header r, int dataSize) {
        public byte SunriseBegin = r.ReadByte();
        public byte SunriseEnd = r.ReadByte();
        public byte SunsetBegin = r.ReadByte();
        public byte SunsetEnd = r.ReadByte();
        public byte Volatility = r.ReadByte();
        public byte MoonsPhaseLength = r.ReadByte();
    }

    public MODLGroup MODL { get; set; } // Model
    public FILEField FNAM; // Sun Texture
    public FILEField GNAM; // Sun Glare Texture
    public List<WLSTField> WLSTs = []; // Climate
    public TNAMField TNAM; // Timing

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        FieldType.MODL => MODL = new MODLGroup(r, dataSize),
        FieldType.MODB => MODL.MODBField(r, dataSize),
        FieldType.FNAM => FNAM = r.ReadFILE(dataSize),
        FieldType.GNAM => GNAM = r.ReadFILE(dataSize),
        FieldType.WLST => WLSTs.AddRangeX(r.ReadFArray(z => new WLSTField(r, dataSize), dataSize >> 3)),
        FieldType.TNAM => TNAM = new TNAMField(r, dataSize),
        _ => Empty,
    };
}

/// <summary>
/// CONT.Container - 3450
/// </summary>
public class CONTRecord : Record, IHaveMODL {
    // TESX
    public class DATAField {
        public byte Flags; // flags 0x0001 = Organic, 0x0002 = Respawns, organic only, 0x0008 = Default, unknown
        public float Weight;

        public DATAField(Header r, int dataSize) {
            if (r.Format == TES3) {
                Weight = r.ReadSingle();
                return;
            }
            Flags = r.ReadByte();
            Weight = r.ReadSingle();
        }
        public object FLAGField(Header r, int dataSize) => Flags = (byte)r.ReadUInt32();
    }

    public MODLGroup MODL { get; set; } // Model
    public STRVField FULL; // Container Name
    public DATAField DATA; // Container Data
    public RefField<SCPTRecord>? SCRI;
    public List<CNTOField> CNTOs = [];
    // TES4
    public RefField<SOUNRecord> SNAM; // Open sound
    public RefField<SOUNRecord> QNAM; // Close sound

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadSTRV(dataSize),
        FieldType.MODL => MODL = new MODLGroup(r, dataSize),
        FieldType.MODB => MODL.MODBField(r, dataSize),
        FieldType.MODT => MODL.MODTField(r, dataSize),
        FieldType.FULL or FieldType.FNAM => FULL = r.ReadSTRV(dataSize),
        FieldType.DATA or FieldType.CNDT => DATA = new DATAField(r, dataSize),
        FieldType.FLAG => DATA.FLAGField(r, dataSize),
        FieldType.CNTO or FieldType.NPCO => CNTOs.AddX(new CNTOField(r, dataSize)),
        FieldType.SCRI => SCRI = new RefField<SCPTRecord>(r, dataSize),
        FieldType.SNAM => SNAM = new RefField<SOUNRecord>(r, dataSize),
        FieldType.QNAM => QNAM = new RefField<SOUNRecord>(r, dataSize),
        _ => Empty,
    };
}

/// <summary>
/// CREA.Creature - 3450
/// </summary>
public class CREARecord : Record, IHaveMODL {
    [Flags]
    public enum CREAFlags : uint {
        Biped = 0x0001,
        Respawn = 0x0002,
        WeaponAndShield = 0x0004,
        None_ = 0x0008,
        Swims = 0x0010,
        Flies = 0x0020,
        Walks = 0x0040,
        DefaultFlags = 0x0048,
        Essential = 0x0080,
        SkeletonBlood = 0x0400,
        MetalBlood = 0x0800
    }

    public struct NPDTField(Header r, int dataSize) {
        public int Type = r.ReadInt32(); // 0 = Creature, 1 = Daedra, 2 = Undead, 3 = Humanoid
        public int Level = r.ReadInt32();
        public int Strength = r.ReadInt32();
        public int Intelligence = r.ReadInt32();
        public int Willpower = r.ReadInt32();
        public int Agility = r.ReadInt32();
        public int Speed = r.ReadInt32();
        public int Endurance = r.ReadInt32();
        public int Personality = r.ReadInt32();
        public int Luck = r.ReadInt32();
        public int Health = r.ReadInt32();
        public int SpellPts = r.ReadInt32();
        public int Fatigue = r.ReadInt32();
        public int Soul = r.ReadInt32();
        public int Combat = r.ReadInt32();
        public int Magic = r.ReadInt32();
        public int Stealth = r.ReadInt32();
        public int AttackMin1 = r.ReadInt32();
        public int AttackMax1 = r.ReadInt32();
        public int AttackMin2 = r.ReadInt32();
        public int AttackMax2 = r.ReadInt32();
        public int AttackMin3 = r.ReadInt32();
        public int AttackMax3 = r.ReadInt32();
        public int Gold = r.ReadInt32();
    }

    public struct AIDTField(Header r, int dataSize) {
        [Flags]
        public enum AIFlags : uint {
            Weapon = 0x00001,
            Armor = 0x00002,
            Clothing = 0x00004,
            Books = 0x00008,
            Ingrediant = 0x00010,
            Picks = 0x00020,
            Probes = 0x00040,
            Lights = 0x00080,
            Apparatus = 0x00100,
            Repair = 0x00200,
            Misc = 0x00400,
            Spells = 0x00800,
            MagicItems = 0x01000,
            Potions = 0x02000,
            Training = 0x04000,
            Spellmaking = 0x08000,
            Enchanting = 0x10000,
            RepairItem = 0x20000
        }

        public byte Hello = r.ReadByte();
        public byte Unknown1 = r.ReadByte();
        public byte Fight = r.ReadByte();
        public byte Flee = r.ReadByte();
        public byte Alarm = r.ReadByte();
        public byte Unknown2 = r.ReadByte();
        public byte Unknown3 = r.ReadByte();
        public byte Unknown4 = r.ReadByte();
        public uint Flags = r.ReadUInt32();
    }

    public struct AI_WField(Header r, int dataSize) {
        public short Distance = r.ReadInt16();
        public short Duration = r.ReadInt16();
        public byte TimeOfDay = r.ReadByte();
        public byte[] Idle = r.ReadBytes(8);
        public byte Unknown = r.ReadByte();
    }

    public struct AI_TField(Header r, int dataSize) {
        public float X = r.ReadSingle();
        public float Y = r.ReadSingle();
        public float Z = r.ReadSingle();
        public float Unknown = r.ReadSingle();
    }

    public struct AI_FField(Header r, int dataSize) {
        public float X = r.ReadSingle();
        public float Y = r.ReadSingle();
        public float Z = r.ReadSingle();
        public short Duration = r.ReadInt16();
        public string Id = r.ReadFAString(32);
        public short Unknown = r.ReadInt16();
    }

    public struct AI_AField(Header r, int dataSize) {
        public string Name = r.ReadFAString(32);
        public byte Unknown = r.ReadByte();
    }

    public MODLGroup MODL { get; set; } // NIF Model
    public STRVField FNAM; // Creature name
    public NPDTField NPDT; // Creature data
    public IN32Field FLAG; // Creature Flags
    public RefField<SCPTRecord> SCRI; // Script
    public CNTOField NPCO; // Item record
    public AIDTField AIDT; // AI data
    public AI_WField AI_W; // AI Wander
    public AI_TField? AI_T; // AI Travel
    public AI_FField? AI_F; // AI Follow
    public AI_FField? AI_E; // AI Escort
    public AI_AField? AI_A; // AI Activate
    public FLTVField? XSCL; // Scale (optional), Only present if the scale is not 1.0
    public STRVField? CNAM;
    public List<STRVField> NPCSs = [];

    public override object CreateField(Header r, FieldType type, int dataSize) => r.Format == TES3
        ? type switch {
            FieldType.NAME => EDID = r.ReadSTRV(dataSize),
            FieldType.MODL => MODL = new MODLGroup(r, dataSize),
            FieldType.FNAM => FNAM = r.ReadSTRV(dataSize),
            FieldType.NPDT => NPDT = new NPDTField(r, dataSize),
            FieldType.FLAG => FLAG = r.ReadS<IN32Field>(dataSize),
            FieldType.SCRI => SCRI = new RefField<SCPTRecord>(r, dataSize),
            FieldType.NPCO => NPCO = new CNTOField(r, dataSize),
            FieldType.AIDT => AIDT = new AIDTField(r, dataSize),
            FieldType.AI_W => AI_W = new AI_WField(r, dataSize),
            FieldType.AI_T => AI_T = new AI_TField(r, dataSize),
            FieldType.AI_F => AI_F = new AI_FField(r, dataSize),
            FieldType.AI_E => AI_E = new AI_FField(r, dataSize),
            FieldType.AI_A => AI_A = new AI_AField(r, dataSize),
            FieldType.XSCL => XSCL = r.ReadS<FLTVField>(dataSize),
            FieldType.CNAM => CNAM = r.ReadSTRV(dataSize),
            FieldType.NPCS => NPCSs.AddX(r.ReadSTRV_ZPad(dataSize)),
            _ => Empty,
        }
        : null;
}

/// <summary>
/// CSTY.Combat Style - 0450
/// </summary>
public class CSTYRecord : Record {
    public class CSTDField {
        public byte DodgePercentChance;
        public byte LeftRightPercentChance;
        public float DodgeLeftRightTimer_Min;
        public float DodgeLeftRightTimer_Max;
        public float DodgeForwardTimer_Min;
        public float DodgeForwardTimer_Max;
        public float DodgeBackTimer_Min;
        public float DodgeBackTimer_Max;
        public float IdleTimer_Min;
        public float IdleTimer_Max;
        public byte BlockPercentChance;
        public byte AttackPercentChance;
        public float RecoilStaggerBonusToAttack;
        public float UnconsciousBonusToAttack;
        public float HandToHandBonusToAttack;
        public byte PowerAttackPercentChance;
        public float RecoilStaggerBonusToPower;
        public float UnconsciousBonusToPowerAttack;
        public byte PowerAttack_Normal;
        public byte PowerAttack_Forward;
        public byte PowerAttack_Back;
        public byte PowerAttack_Left;
        public byte PowerAttack_Right;
        public float HoldTimer_Min;
        public float HoldTimer_Max;
        public byte Flags1;
        public byte AcrobaticDodgePercentChance;
        public float RangeMult_Optimal;
        public float RangeMult_Max;
        public float SwitchDistance_Melee;
        public float SwitchDistance_Ranged;
        public float BuffStandoffDistance;
        public float RangedStandoffDistance;
        public float GroupStandoffDistance;
        public byte RushingAttackPercentChance;
        public float RushingAttackDistanceMult;
        public uint Flags2;

        public CSTDField(Header r, int dataSize) {
            //if (dataSize != 124 && dataSize != 120 && dataSize != 112 && dataSize != 104 && dataSize != 92 && dataSize != 84) DodgePercentChance = 0;
            DodgePercentChance = r.ReadByte();
            LeftRightPercentChance = r.ReadByte();
            r.Skip(2); // Unused
            DodgeLeftRightTimer_Min = r.ReadSingle();
            DodgeLeftRightTimer_Max = r.ReadSingle();
            DodgeForwardTimer_Min = r.ReadSingle();
            DodgeForwardTimer_Max = r.ReadSingle();
            DodgeBackTimer_Min = r.ReadSingle();
            DodgeBackTimer_Max = r.ReadSingle();
            IdleTimer_Min = r.ReadSingle();
            IdleTimer_Max = r.ReadSingle();
            BlockPercentChance = r.ReadByte();
            AttackPercentChance = r.ReadByte();
            r.Skip(2); // Unused
            RecoilStaggerBonusToAttack = r.ReadSingle();
            UnconsciousBonusToAttack = r.ReadSingle();
            HandToHandBonusToAttack = r.ReadSingle();
            PowerAttackPercentChance = r.ReadByte();
            r.Skip(3); // Unused
            RecoilStaggerBonusToPower = r.ReadSingle();
            UnconsciousBonusToPowerAttack = r.ReadSingle();
            PowerAttack_Normal = r.ReadByte();
            PowerAttack_Forward = r.ReadByte();
            PowerAttack_Back = r.ReadByte();
            PowerAttack_Left = r.ReadByte();
            PowerAttack_Right = r.ReadByte();
            r.Skip(3); // Unused
            HoldTimer_Min = r.ReadSingle();
            HoldTimer_Max = r.ReadSingle();
            Flags1 = r.ReadByte();
            AcrobaticDodgePercentChance = r.ReadByte();
            r.Skip(2); // Unused
            if (dataSize == 84) return; RangeMult_Optimal = r.ReadSingle();
            RangeMult_Max = r.ReadSingle();
            if (dataSize == 92) return; SwitchDistance_Melee = r.ReadSingle();
            SwitchDistance_Ranged = r.ReadSingle();
            BuffStandoffDistance = r.ReadSingle();
            if (dataSize == 104) return; RangedStandoffDistance = r.ReadSingle();
            GroupStandoffDistance = r.ReadSingle();
            if (dataSize == 112) return; RushingAttackPercentChance = r.ReadByte();
            r.Skip(3); // Unused
            RushingAttackDistanceMult = r.ReadSingle();
            if (dataSize == 120) return; Flags2 = r.ReadUInt32();
        }
    }

    public struct CSADField(Header r, int dataSize) {
        public float DodgeFatigueModMult = r.ReadSingle();
        public float DodgeFatigueModBase = r.ReadSingle();
        public float EncumbSpeedModBase = r.ReadSingle();
        public float EncumbSpeedModMult = r.ReadSingle();
        public float DodgeWhileUnderAttackMult = r.ReadSingle();
        public float DodgeNotUnderAttackMult = r.ReadSingle();
        public float DodgeBackWhileUnderAttackMult = r.ReadSingle();
        public float DodgeBackNotUnderAttackMult = r.ReadSingle();
        public float DodgeForwardWhileAttackingMult = r.ReadSingle();
        public float DodgeForwardNotAttackingMult = r.ReadSingle();
        public float BlockSkillModifierMult = r.ReadSingle();
        public float BlockSkillModifierBase = r.ReadSingle();
        public float BlockWhileUnderAttackMult = r.ReadSingle();
        public float BlockNotUnderAttackMult = r.ReadSingle();
        public float AttackSkillModifierMult = r.ReadSingle();
        public float AttackSkillModifierBase = r.ReadSingle();
        public float AttackWhileUnderAttackMult = r.ReadSingle();
        public float AttackNotUnderAttackMult = r.ReadSingle();
        public float AttackDuringBlockMult = r.ReadSingle();
        public float PowerAttFatigueModBase = r.ReadSingle();
        public float PowerAttFatigueModMult = r.ReadSingle();
    }

    public CSTDField CSTD; // Standard
    public CSADField CSAD; // Advanced

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        FieldType.CSTD => CSTD = new CSTDField(r, dataSize),
        FieldType.CSAD => CSAD = new CSADField(r, dataSize),
        _ => Empty,
    };
}

/// <summary>
/// DIAL.Dialog Topic - 3450
/// </summary>
public class DIALRecord : Record {
    internal static DIALRecord LastRecord;
    public enum DIALType : byte { RegularTopic = 0, Voice, Greeting, Persuasion, Journal }
    public STRVField FULL; // Dialogue Name
    public BYTEField DATA; // Dialogue Type
    public List<RefField<QUSTRecord>> QSTIs; // Quests (optional)
    public List<INFORecord> INFOs = []; // Info Records

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => (EDID = r.ReadSTRV(dataSize), LastRecord = this),
        FieldType.FULL => FULL = r.ReadSTRV(dataSize),
        FieldType.DATA => DATA = r.ReadS<BYTEField>(dataSize),
        FieldType.QSTI or FieldType.QSTR => (QSTIs ??= []).AddX(new RefField<QUSTRecord>(r, dataSize)),
        _ => Empty,
    };
}

/// <summary>
/// DLBR.Dialog Branch - 0050
/// </summary>
public class DLBRRecord : Record {
    public CREFField CNAM; // RGB color

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        FieldType.CNAM => CNAM = r.ReadS<CREFField>(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// DLVW.Dialog View - 0050
/// </summary>
public class DLVWRecord : Record {
    public CREFField CNAM; // RGB color

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        FieldType.CNAM => CNAM = r.ReadS<CREFField>(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// DOOR.Door - 3450
/// </summary>
public class DOORRecord : Record, IHaveMODL {
    public STRVField FULL; // Door name
    public MODLGroup MODL { get; set; } // NIF model filename
    public RefField<SCPTRecord>? SCRI; // Script (optional)
    public RefField<SOUNRecord> SNAM; // Open Sound
    public RefField<SOUNRecord> ANAM; // Close Sound
                                      // TES4
    public RefField<SOUNRecord> BNAM; // Loop Sound
    public BYTEField FNAM; // Flags
    public RefField<Record> TNAM; // Random teleport destination

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadSTRV(dataSize),
        FieldType.FULL => FULL = r.ReadSTRV(dataSize),
        FieldType.FNAM => r.Format != TES3 ? FNAM = r.ReadS<BYTEField>(dataSize) : FULL = r.ReadSTRV(dataSize),
        FieldType.MODL => MODL = new MODLGroup(r, dataSize),
        FieldType.MODB => MODL.MODBField(r, dataSize),
        FieldType.MODT => MODL.MODTField(r, dataSize),
        FieldType.SCRI => SCRI = new RefField<SCPTRecord>(r, dataSize),
        FieldType.SNAM => SNAM = new RefField<SOUNRecord>(r, dataSize),
        FieldType.ANAM => ANAM = new RefField<SOUNRecord>(r, dataSize),
        FieldType.BNAM => ANAM = new RefField<SOUNRecord>(r, dataSize),
        FieldType.TNAM => TNAM = new RefField<Record>(r, dataSize),
        _ => Empty,
    };
}

/// <summary>
/// EFSH.Effect Shader - 0450
/// </summary>
public class EFSHRecord : Record {
    public class DATAField {
        public byte Flags;
        public uint MembraneShader_SourceBlendMode;
        public uint MembraneShader_BlendOperation;
        public uint MembraneShader_ZTestFunction;
        public ByteColor4 FillTextureEffect_Color;
        public float FillTextureEffect_AlphaFadeInTime;
        public float FillTextureEffect_FullAlphaTime;
        public float FillTextureEffect_AlphaFadeOutTime;
        public float FillTextureEffect_PresistentAlphaRatio;
        public float FillTextureEffect_AlphaPulseAmplitude;
        public float FillTextureEffect_AlphaPulseFrequency;
        public float FillTextureEffect_TextureAnimationSpeed_U;
        public float FillTextureEffect_TextureAnimationSpeed_V;
        public float EdgeEffect_FallOff;
        public ByteColor4 EdgeEffect_Color;
        public float EdgeEffect_AlphaFadeInTime;
        public float EdgeEffect_FullAlphaTime;
        public float EdgeEffect_AlphaFadeOutTime;
        public float EdgeEffect_PresistentAlphaRatio;
        public float EdgeEffect_AlphaPulseAmplitude;
        public float EdgeEffect_AlphaPulseFrequency;
        public float FillTextureEffect_FullAlphaRatio;
        public float EdgeEffect_FullAlphaRatio;
        public uint MembraneShader_DestBlendMode;
        public uint ParticleShader_SourceBlendMode;
        public uint ParticleShader_BlendOperation;
        public uint ParticleShader_ZTestFunction;
        public uint ParticleShader_DestBlendMode;
        public float ParticleShader_ParticleBirthRampUpTime;
        public float ParticleShader_FullParticleBirthTime;
        public float ParticleShader_ParticleBirthRampDownTime;
        public float ParticleShader_FullParticleBirthRatio;
        public float ParticleShader_PersistantParticleBirthRatio;
        public float ParticleShader_ParticleLifetime;
        public float ParticleShader_ParticleLifetime_Delta;
        public float ParticleShader_InitialSpeedAlongNormal;
        public float ParticleShader_AccelerationAlongNormal;
        public float ParticleShader_InitialVelocity1;
        public float ParticleShader_InitialVelocity2;
        public float ParticleShader_InitialVelocity3;
        public float ParticleShader_Acceleration1;
        public float ParticleShader_Acceleration2;
        public float ParticleShader_Acceleration3;
        public float ParticleShader_ScaleKey1;
        public float ParticleShader_ScaleKey2;
        public float ParticleShader_ScaleKey1Time;
        public float ParticleShader_ScaleKey2Time;
        public ByteColor4 ColorKey1_Color;
        public ByteColor4 ColorKey2_Color;
        public ByteColor4 ColorKey3_Color;
        public float ColorKey1_ColorAlpha;
        public float ColorKey2_ColorAlpha;
        public float ColorKey3_ColorAlpha;
        public float ColorKey1_ColorKeyTime;
        public float ColorKey2_ColorKeyTime;
        public float ColorKey3_ColorKeyTime;

        public DATAField(Header r, int dataSize) {
            if (dataSize != 224 && dataSize != 96) Flags = 0;
            Flags = r.ReadByte();
            r.Skip(3); // Unused
            MembraneShader_SourceBlendMode = r.ReadUInt32();
            MembraneShader_BlendOperation = r.ReadUInt32();
            MembraneShader_ZTestFunction = r.ReadUInt32();
            FillTextureEffect_Color = r.ReadS<ByteColor4>(dataSize);
            FillTextureEffect_AlphaFadeInTime = r.ReadSingle();
            FillTextureEffect_FullAlphaTime = r.ReadSingle();
            FillTextureEffect_AlphaFadeOutTime = r.ReadSingle();
            FillTextureEffect_PresistentAlphaRatio = r.ReadSingle();
            FillTextureEffect_AlphaPulseAmplitude = r.ReadSingle();
            FillTextureEffect_AlphaPulseFrequency = r.ReadSingle();
            FillTextureEffect_TextureAnimationSpeed_U = r.ReadSingle();
            FillTextureEffect_TextureAnimationSpeed_V = r.ReadSingle();
            EdgeEffect_FallOff = r.ReadSingle();
            EdgeEffect_Color = r.ReadS<ByteColor4>(dataSize);
            EdgeEffect_AlphaFadeInTime = r.ReadSingle();
            EdgeEffect_FullAlphaTime = r.ReadSingle();
            EdgeEffect_AlphaFadeOutTime = r.ReadSingle();
            EdgeEffect_PresistentAlphaRatio = r.ReadSingle();
            EdgeEffect_AlphaPulseAmplitude = r.ReadSingle();
            EdgeEffect_AlphaPulseFrequency = r.ReadSingle();
            FillTextureEffect_FullAlphaRatio = r.ReadSingle();
            EdgeEffect_FullAlphaRatio = r.ReadSingle();
            MembraneShader_DestBlendMode = r.ReadUInt32();
            if (dataSize == 96) return;
            ParticleShader_SourceBlendMode = r.ReadUInt32();
            ParticleShader_BlendOperation = r.ReadUInt32();
            ParticleShader_ZTestFunction = r.ReadUInt32();
            ParticleShader_DestBlendMode = r.ReadUInt32();
            ParticleShader_ParticleBirthRampUpTime = r.ReadSingle();
            ParticleShader_FullParticleBirthTime = r.ReadSingle();
            ParticleShader_ParticleBirthRampDownTime = r.ReadSingle();
            ParticleShader_FullParticleBirthRatio = r.ReadSingle();
            ParticleShader_PersistantParticleBirthRatio = r.ReadSingle();
            ParticleShader_ParticleLifetime = r.ReadSingle();
            ParticleShader_ParticleLifetime_Delta = r.ReadSingle();
            ParticleShader_InitialSpeedAlongNormal = r.ReadSingle();
            ParticleShader_AccelerationAlongNormal = r.ReadSingle();
            ParticleShader_InitialVelocity1 = r.ReadSingle();
            ParticleShader_InitialVelocity2 = r.ReadSingle();
            ParticleShader_InitialVelocity3 = r.ReadSingle();
            ParticleShader_Acceleration1 = r.ReadSingle();
            ParticleShader_Acceleration2 = r.ReadSingle();
            ParticleShader_Acceleration3 = r.ReadSingle();
            ParticleShader_ScaleKey1 = r.ReadSingle();
            ParticleShader_ScaleKey2 = r.ReadSingle();
            ParticleShader_ScaleKey1Time = r.ReadSingle();
            ParticleShader_ScaleKey2Time = r.ReadSingle();
            ColorKey1_Color = r.ReadS<ByteColor4>(dataSize);
            ColorKey2_Color = r.ReadS<ByteColor4>(dataSize);
            ColorKey3_Color = r.ReadS<ByteColor4>(dataSize);
            ColorKey1_ColorAlpha = r.ReadSingle();
            ColorKey2_ColorAlpha = r.ReadSingle();
            ColorKey3_ColorAlpha = r.ReadSingle();
            ColorKey1_ColorKeyTime = r.ReadSingle();
            ColorKey2_ColorKeyTime = r.ReadSingle();
            ColorKey3_ColorKeyTime = r.ReadSingle();
        }
    }

    public FILEField ICON; // Fill Texture
    public FILEField ICO2; // Particle Shader Texture
    public DATAField DATA; // Data

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        FieldType.ICON => ICON = r.ReadFILE(dataSize),
        FieldType.ICO2 => ICO2 = r.ReadFILE(dataSize),
        FieldType.DATA => DATA = new DATAField(r, dataSize),
        _ => Empty,
    };
}

/// <summary>
/// ENCH.Enchantment - 3450
/// </summary>
public class ENCHRecord : Record {
    // TESX
    public struct ENITField {
        // TES3: 0 = Cast Once, 1 = Cast Strikes, 2 = Cast when Used, 3 = Constant Effect
        // TES4: 0 = Scroll, 1 = Staff, 2 = Weapon, 3 = Apparel
        public int Type;
        public int EnchantCost;
        public int ChargeAmount; //: Charge
        public int Flags; //: AutoCalc

        public ENITField(Header r, int dataSize) {
            Type = r.ReadInt32();
            if (r.Format == TES3) {
                EnchantCost = r.ReadInt32();
                ChargeAmount = r.ReadInt32();
            }
            else {
                ChargeAmount = r.ReadInt32();
                EnchantCost = r.ReadInt32();
            }
            Flags = r.ReadInt32();
        }
    }

    public class EFITField {
        public string EffectId;
        public int Type; //:RangeType - 0 = Self, 1 = Touch, 2 = Target
        public int Area;
        public int Duration;
        public int MagnitudeMin;
        // TES3
        public byte SkillId; // (-1 if NA)
        public byte AttributeId; // (-1 if NA)
        public int MagnitudeMax;
        // TES4
        public int ActorValue;

        public EFITField(Header r, int dataSize) {
            if (r.Format == TES3) {
                EffectId = r.ReadFAString(2);
                SkillId = r.ReadByte();
                AttributeId = r.ReadByte();
                Type = r.ReadInt32();
                Area = r.ReadInt32();
                Duration = r.ReadInt32();
                MagnitudeMin = r.ReadInt32();
                MagnitudeMax = r.ReadInt32();
                return;
            }
            EffectId = r.ReadFAString(4);
            MagnitudeMin = r.ReadInt32();
            Area = r.ReadInt32();
            Duration = r.ReadInt32();
            Type = r.ReadInt32();
            ActorValue = r.ReadInt32();
        }
    }

    // TES4
    public class SCITField {
        public string Name;
        public int ScriptFormId;
        public int School; // 0 = Alteration, 1 = Conjuration, 2 = Destruction, 3 = Illusion, 4 = Mysticism, 5 = Restoration
        public string VisualEffect;
        public uint Flags;

        public SCITField(Header r, int dataSize) {
            Name = "Script Effect";
            ScriptFormId = r.ReadInt32();
            if (dataSize == 4) return;
            School = r.ReadInt32();
            VisualEffect = r.ReadFAString(4);
            Flags = dataSize > 12 ? r.ReadUInt32() : 0;
        }
        public object FULLField(Header r, int dataSize) => Name = r.ReadFUString(dataSize);
    }

    public STRVField FULL; // Enchant name
    public ENITField ENIT; // Enchant Data
    public List<EFITField> EFITs = []; // Effect Data
                                       // TES4
    public List<SCITField> SCITs = []; // Script effect data

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadSTRV(dataSize),
        FieldType.FULL => SCITs.Count == 0 ? FULL = r.ReadSTRV(dataSize) : SCITs.Last().FULLField(r, dataSize),
        FieldType.ENIT or FieldType.ENDT => ENIT = new ENITField(r, dataSize),
        FieldType.EFID => r.Skip(dataSize),
        FieldType.EFIT or FieldType.ENAM => EFITs.AddX(new EFITField(r, dataSize)),
        FieldType.SCIT => SCITs.AddX(new SCITField(r, dataSize)),
        _ => Empty,
    };
}

/// <summary>
/// EYES.Eyes - 0450
/// </summary>
public class EYESRecord : Record {
    public STRVField FULL;
    public FILEField ICON;
    public BYTEField DATA; // Playable

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        FieldType.FULL => FULL = r.ReadSTRV(dataSize),
        FieldType.ICON => ICON = r.ReadFILE(dataSize),
        FieldType.DATA => DATA = r.ReadS<BYTEField>(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// FACT.Faction - 3450
/// </summary>
public class FACTRecord : Record {
    // TESX
    public class RNAMGroup {
        public override string ToString() => $"{RNAM.Value}:{MNAM.Value}";
        public IN32Field RNAM; // rank
        public STRVField MNAM; // male
        public STRVField FNAM; // female
        public STRVField INAM; // insignia
    }

    // TES3
    public struct FADTField {
        public FADTField(Header r, int dataSize) => r.Skip(dataSize);
    }

    // TES4
    public struct XNAMField(Header r, int dataSize) {
        public override string ToString() => $"{FormId}";
        public int FormId = r.ReadInt32();
        public int Mod = r.ReadInt32();
        public int Combat = r.Format > TES4 ? r.ReadInt32() : 0;
    }

    public STRVField FNAM; // Faction name
    public List<RNAMGroup> RNAMs = []; // Rank Name
    public FADTField FADT; // Faction data
    public List<STRVField> ANAMs = []; // Faction name
    public List<INTVField> INTVs = []; // Faction reaction
                                       // TES4
    public XNAMField XNAM; // Interfaction Relations
    public INTVField DATA; // Flags (byte, uint32)
    public UI32Field CNAM;

    public override object CreateField(Header r, FieldType type, int dataSize) => r.Format == TES3
        ? type switch {
            FieldType.NAME => EDID = r.ReadSTRV(dataSize),
            FieldType.FNAM => FNAM = r.ReadSTRV(dataSize),
            FieldType.RNAM => RNAMs.AddX(new RNAMGroup { MNAM = r.ReadSTRV(dataSize) }),
            FieldType.FADT => FADT = new FADTField(r, dataSize),
            FieldType.ANAM => ANAMs.AddX(r.ReadSTRV(dataSize)),
            FieldType.INTV => INTVs.AddX(r.ReadINTV(dataSize)),
            _ => Empty,
        }
        : type switch {
            FieldType.EDID => EDID = r.ReadSTRV(dataSize),
            FieldType.FULL => FNAM = r.ReadSTRV(dataSize),
            FieldType.XNAM => XNAM = new XNAMField(r, dataSize),
            FieldType.DATA => DATA = r.ReadINTV(dataSize),
            FieldType.CNAM => CNAM = r.ReadS<UI32Field>(dataSize),
            FieldType.RNAM => RNAMs.AddX(new RNAMGroup { RNAM = r.ReadS<IN32Field>(dataSize) }),
            FieldType.MNAM => RNAMs.Last().MNAM = r.ReadSTRV(dataSize),
            FieldType.FNAM => RNAMs.Last().FNAM = r.ReadSTRV(dataSize),
            FieldType.INAM => RNAMs.Last().INAM = r.ReadSTRV(dataSize),
            _ => Empty,
        };
}

/// <summary>
/// FLOR.Flora - 0450
/// </summary>
public class FLORRecord : Record, IHaveMODL {
    public MODLGroup MODL { get; set; } // Model
    public STRVField FULL; // Plant Name
    public RefField<SCPTRecord> SCRI; // Script (optional)
    public RefField<INGRRecord> PFIG; // The ingredient the plant produces (optional)
    public BYTVField PFPC; // Spring, Summer, Fall, Winter Ingredient Production (byte)

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        FieldType.MODL => MODL = new MODLGroup(r, dataSize),
        FieldType.MODB => MODL.MODBField(r, dataSize),
        FieldType.MODT => MODL.MODTField(r, dataSize),
        FieldType.FULL => FULL = r.ReadSTRV(dataSize),
        FieldType.SCRI => SCRI = new RefField<SCPTRecord>(r, dataSize),
        FieldType.PFIG => PFIG = new RefField<INGRRecord>(r, dataSize),
        FieldType.PFPC => PFPC = r.ReadBYTV(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// FURN.Furniture - 0450
/// </summary>
public class FURNRecord : Record, IHaveMODL {
    public MODLGroup MODL { get; set; } // Model
    public STRVField FULL; // Furniture Name
    public RefField<SCPTRecord> SCRI; // Script (optional)
    public IN32Field MNAM; // Active marker flags, required. A bit field with a bit value of 1 indicating that the matching marker position in the NIF file is active.

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        FieldType.MODL => MODL = new MODLGroup(r, dataSize),
        FieldType.MODB => MODL.MODBField(r, dataSize),
        FieldType.MODT => MODL.MODTField(r, dataSize),
        FieldType.FULL => FULL = r.ReadSTRV(dataSize),
        FieldType.SCRI => SCRI = new RefField<SCPTRecord>(r, dataSize),
        FieldType.MNAM => MNAM = r.ReadS<IN32Field>(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// GLOB.Global - 3450
/// </summary>
public class GLOBRecord : Record {
    public BYTEField? FNAM; // Type of global (s, l, f)
    public FLTVField? FLTV; // Float data

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadSTRV(dataSize),
        FieldType.FNAM => FNAM = r.ReadS<BYTEField>(dataSize),
        FieldType.FLTV => FLTV = r.ReadS<FLTVField>(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// GMST.Game Setting - 3450
/// </summary>
public class GMSTRecord : Record {
    public DATVField DATA; // Data

    public override object CreateField(Header r, FieldType type, int dataSize) => r.Format == TES3
        ? type switch {
            FieldType.NAME => EDID = r.ReadSTRV(dataSize),
            FieldType.STRV => DATA = r.ReadDATV(dataSize, 's'),
            FieldType.INTV => DATA = r.ReadDATV(dataSize, 'i'),
            FieldType.FLTV => DATA = r.ReadDATV(dataSize, 'f'),
            _ => Empty,
        }
        : type switch {
            FieldType.EDID => EDID = r.ReadSTRV(dataSize),
            FieldType.DATA => DATA = r.ReadDATV(dataSize, EDID.Value[0]),
            _ => Empty,
        };
}

/// <summary>
/// GRAS.Grass - 0450
/// </summary>
public class GRASRecord : Record {
    public struct DATAField {
        public byte Density;
        public byte MinSlope;
        public byte MaxSlope;
        public ushort UnitFromWaterAmount;
        public uint UnitFromWaterType;
        //Above - At Least,
        //Above - At Most,
        //Below - At Least,
        //Below - At Most,
        //Either - At Least,
        //Either - At Most,
        //Either - At Most Above,
        //Either - At Most Below
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
            UnitFromWaterType = r.ReadUInt32();
            PositionRange = r.ReadSingle();
            HeightRange = r.ReadSingle();
            ColorRange = r.ReadSingle();
            WavePeriod = r.ReadSingle();
            Flags = r.ReadByte();
            r.Skip(3);
        }
    }

    public MODLGroup MODL;
    public DATAField DATA;

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        FieldType.MODL => MODL = new MODLGroup(r, dataSize),
        FieldType.MODB => MODL.MODBField(r, dataSize),
        FieldType.MODT => MODL.MODTField(r, dataSize),
        FieldType.DATA => DATA = new DATAField(r, dataSize),
        _ => Empty,
    };
}

/// <summary>
/// HAIR.Hair - 0400
/// </summary>
public class HAIRRecord : Record, IHaveMODL {
    public STRVField FULL;
    public MODLGroup MODL { get; set; }
    public FILEField ICON;
    public BYTEField DATA; // Playable, Not Male, Not Female, Fixed

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        FieldType.FULL => FULL = r.ReadSTRV(dataSize),
        FieldType.MODL => MODL = new MODLGroup(r, dataSize),
        FieldType.MODB => MODL.MODBField(r, dataSize),
        FieldType.ICON => ICON = r.ReadFILE(dataSize),
        FieldType.DATA => DATA = r.ReadS<BYTEField>(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// IDLE.Idle Animations - 0450
/// </summary>
public class IDLERecord : Record, IHaveMODL {
    public MODLGroup MODL { get; set; }
    public List<SCPTRecord.CTDAField> CTDAs = []; // Conditions
    public BYTEField ANAM;
    public RefField<IDLERecord>[] DATAs;

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        FieldType.MODL => MODL = new MODLGroup(r, dataSize),
        FieldType.MODB => MODL.MODBField(r, dataSize),
        FieldType.CTDA or FieldType.CTDT => CTDAs.AddX(new SCPTRecord.CTDAField(r, dataSize)),
        FieldType.ANAM => ANAM = r.ReadS<BYTEField>(dataSize),
        FieldType.DATA => DATAs = r.ReadFArray(z => new RefField<IDLERecord>(r, 4), dataSize >> 2),
        _ => Empty,
    };
}

/// <summary>
/// INFO.Dialog Topic Info - 3450
/// </summary>
public class INFORecord : Record {
    // TES3
    public struct DATA3Field(Header r, int dataSize) {
        public int Unknown1 = r.ReadInt32();
        public int Disposition = r.ReadInt32();
        public byte Rank = r.ReadByte(); // (0-10)
        public byte Gender = r.ReadByte(); // 0xFF = None, 0x00 = Male, 0x01 = Female
        public byte PCRank = r.ReadByte(); // (0-10)
        public byte Unknown2 = r.ReadByte();
    }

    public class TES3Group {
        public STRVField NNAM; // Next info ID (form a linked list of INFOs for the DIAL). First INFO has an empty PNAM, last has an empty NNAM.
        public DATA3Field DATA; // Info data
        public STRVField ONAM; // Actor
        public STRVField RNAM; // Race
        public STRVField CNAM; // Class
        public STRVField FNAM; // Faction 
        public STRVField ANAM; // Cell
        public STRVField DNAM; // PC Faction
        public STRVField NAME; // The info response string (512 max)
        public FILEField SNAM; // Sound
        public BYTEField QSTN; // Journal Name
        public BYTEField QSTF; // Journal Finished
        public BYTEField QSTR; // Journal Restart
        public SCPTRecord.CTDAField SCVR; // String for the function/variable choice
        public UNKNField INTV; //
        public UNKNField FLTV; // The function/variable result for the previous SCVR
        public STRVField BNAM; // Result text (not compiled)
    }

    // TES4
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
            EmotionType = r.ReadUInt32();
            EmotionValue = r.ReadInt32();
            r.Skip(4); // Unused
            ResponseNumber = r.ReadByte();
            r.Skip(3); // Unused
        }
        public object NAM1Field(Header r, int dataSize) => ResponseText = r.ReadFUString(dataSize);
        public object NAM2Field(Header r, int dataSize) => ActorNotes = r.ReadFUString(dataSize);
    }

    public class TES4Group {
        public DATA4Field DATA; // Info data
        public RefField<QUSTRecord> QSTI; // Quest
        public RefField<DIALRecord> TPIC; // Topic
        public List<RefField<DIALRecord>> NAMEs = []; // Topics
        public List<TRDTField> TRDTs = []; // Responses
        public List<SCPTRecord.CTDAField> CTDAs = []; // Conditions
        public List<RefField<DIALRecord>> TCLTs = []; // Choices
        public List<RefField<DIALRecord>> TCLFs = []; // Link From Topics
        public SCPTRecord.SCHRField SCHR; // Script Data
        public BYTVField SCDA; // Compiled Script
        public STRVField SCTX; // Script Source
        public List<RefField<Record>> SCROs = []; // Global variable reference
    }

    public RefField<INFORecord> PNAM; // Previous info ID
    public TES3Group TES3 = new();
    public TES4Group TES4 = new();

    public override object CreateField(Header r, FieldType type, int dataSize) => r.Format == FormType.TES3
        ? type switch {
            FieldType.INAM => (DIALRecord.LastRecord?.INFOs.AddX(this), EDID = r.ReadSTRV(dataSize)),
            FieldType.PNAM => PNAM = new RefField<INFORecord>(r, dataSize),
            FieldType.NNAM => TES3.NNAM = r.ReadSTRV(dataSize),
            FieldType.DATA => TES3.DATA = new DATA3Field(r, dataSize),
            FieldType.ONAM => TES3.ONAM = r.ReadSTRV(dataSize),
            FieldType.RNAM => TES3.RNAM = r.ReadSTRV(dataSize),
            FieldType.CNAM => TES3.CNAM = r.ReadSTRV(dataSize),
            FieldType.FNAM => TES3.FNAM = r.ReadSTRV(dataSize),
            FieldType.ANAM => TES3.ANAM = r.ReadSTRV(dataSize),
            FieldType.DNAM => TES3.DNAM = r.ReadSTRV(dataSize),
            FieldType.NAME => TES3.NAME = r.ReadSTRV(dataSize),
            FieldType.SNAM => TES3.SNAM = r.ReadFILE(dataSize),
            FieldType.QSTN => TES3.QSTN = r.ReadS<BYTEField>(dataSize),
            FieldType.QSTF => TES3.QSTF = r.ReadS<BYTEField>(dataSize),
            FieldType.QSTR => TES3.QSTR = r.ReadS<BYTEField>(dataSize),
            FieldType.SCVR => TES3.SCVR = new SCPTRecord.CTDAField(r, dataSize),
            FieldType.INTV => TES3.INTV = r.ReadUNKN(dataSize),
            FieldType.FLTV => TES3.FLTV = r.ReadUNKN(dataSize),
            FieldType.BNAM => TES3.BNAM = r.ReadSTRV(dataSize),
            _ => Empty,
        }
        : type switch {
            FieldType.DATA => TES4.DATA = new DATA4Field(r, dataSize),
            FieldType.QSTI => TES4.QSTI = new RefField<QUSTRecord>(r, dataSize),
            FieldType.TPIC => TES4.TPIC = new RefField<DIALRecord>(r, dataSize),
            FieldType.NAME => TES4.NAMEs.AddX(new RefField<DIALRecord>(r, dataSize)),
            FieldType.TRDT => TES4.TRDTs.AddX(new TRDTField(r, dataSize)),
            FieldType.NAM1 => TES4.TRDTs.Last().NAM1Field(r, dataSize),
            FieldType.NAM2 => TES4.TRDTs.Last().NAM2Field(r, dataSize),
            FieldType.CTDA or FieldType.CTDT => TES4.CTDAs.AddX(new SCPTRecord.CTDAField(r, dataSize)),
            FieldType.TCLT => TES4.TCLTs.AddX(new RefField<DIALRecord>(r, dataSize)),
            FieldType.TCLF => TES4.TCLFs.AddX(new RefField<DIALRecord>(r, dataSize)),
            FieldType.SCHR or FieldType.SCHD => TES4.SCHR = new SCPTRecord.SCHRField(r, dataSize),
            FieldType.SCDA => TES4.SCDA = r.ReadBYTV(dataSize),
            FieldType.SCTX => TES4.SCTX = r.ReadSTRV(dataSize),
            FieldType.SCRO => TES4.SCROs.AddX(new RefField<Record>(r, dataSize)),
            _ => Empty,
        };
}

/// <summary>
/// INGR.Ingredient - 3450
/// </summary>
public class INGRRecord : Record, IHaveMODL {
    // TES3
    public struct IRDTField(Header r, int dataSize) {
        public float Weight = r.ReadSingle();
        public int Value = r.ReadInt32();
        public int[] EffectId = [r.ReadInt32(), r.ReadInt32(), r.ReadInt32(), r.ReadInt32()]; // 0 or -1 means no effect
        public int[] SkillId = [r.ReadInt32(), r.ReadInt32(), r.ReadInt32(), r.ReadInt32()]; // only for Skill related effects, 0 or -1 otherwise
        public int[] AttributeId = [r.ReadInt32(), r.ReadInt32(), r.ReadInt32(), r.ReadInt32()]; // only for Attribute related effects, 0 or -1 otherwise
    }

    // TES4
    public class DATAField(Header r, int dataSize) {
        public float Weight = r.ReadSingle();
        public int Value;
        public uint Flags;

        public object ENITField(Header r, int dataSize) { var z = Value = r.ReadInt32(); Flags = r.ReadUInt32(); return z; }
    }

    public MODLGroup MODL { get; set; } // Model Name
    public STRVField FULL; // Item Name
    public IRDTField IRDT; // Ingrediant Data //: TES3
    public DATAField DATA; // Ingrediant Data //: TES4
    public FILEField ICON; // Inventory Icon
    public RefField<SCPTRecord> SCRI; // Script Name
    // TES4
    public List<ENCHRecord.EFITField> EFITs = []; // Effect Data
    public List<ENCHRecord.SCITField> SCITs = []; // Script effect data

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadSTRV(dataSize),
        FieldType.MODL => MODL = new MODLGroup(r, dataSize),
        FieldType.MODB => MODL.MODBField(r, dataSize),
        FieldType.MODT => MODL.MODTField(r, dataSize),
        FieldType.FULL => SCITs.Count == 0 ? FULL = r.ReadSTRV(dataSize) : SCITs.Last().FULLField(r, dataSize),
        FieldType.FNAM => FULL = r.ReadSTRV(dataSize),
        FieldType.DATA => DATA = new DATAField(r, dataSize),
        FieldType.IRDT => IRDT = new IRDTField(r, dataSize),
        FieldType.ICON or FieldType.ITEX => ICON = r.ReadFILE(dataSize),
        FieldType.SCRI => SCRI = new RefField<SCPTRecord>(r, dataSize),
        //
        FieldType.ENIT => DATA.ENITField(r, dataSize),
        FieldType.EFID => r.Skip(dataSize),
        FieldType.EFIT => EFITs.AddX(new ENCHRecord.EFITField(r, dataSize)),
        FieldType.SCIT => SCITs.AddX(new ENCHRecord.SCITField(r, dataSize)),
        _ => Empty,
    };
}

/// <summary>
/// KEYM.Key - 0400
/// </summary>
public class KEYMRecord : Record, IHaveMODL {
    public struct DATAField(Header r, int dataSize) {
        public int Value = r.ReadInt32();
        public float Weight = r.ReadSingle();
    }

    public MODLGroup MODL { get; set; } // Model
    public STRVField FULL; // Item Name
    public RefField<SCPTRecord> SCRI; // Script (optional)
    public DATAField DATA; // Type of soul contained in the gem
    public FILEField ICON; // Icon (optional)

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        FieldType.MODL => MODL = new MODLGroup(r, dataSize),
        FieldType.MODB => MODL.MODBField(r, dataSize),
        FieldType.MODT => MODL.MODTField(r, dataSize),
        FieldType.FULL => FULL = r.ReadSTRV(dataSize),
        FieldType.SCRI => SCRI = new RefField<SCPTRecord>(r, dataSize),
        FieldType.DATA => DATA = new DATAField(r, dataSize),
        FieldType.ICON => ICON = r.ReadFILE(dataSize),
        _ => false,
    };
}

/// <summary>
/// LAND.Land - 3450
/// </summary>
public unsafe class LANDRecord : Record {
    // TESX
    public struct VNMLField(Header r, int dataSize) {
        public Byte3[] Vertexs = r.ReadPArray<Byte3>("3B", dataSize / 3); // XYZ 8 bit floats
    }

    public struct VHGTField {
        public float ReferenceHeight; // A height offset for the entire cell. Decreasing this value will shift the entire cell land down.
        public sbyte[] HeightData; // HeightData

        public VHGTField(Header r, int dataSize) {
            ReferenceHeight = r.ReadSingle();
            var count = dataSize - 4 - 3;
            HeightData = r.ReadPArray<sbyte>("B", count);
            r.Skip(3); // Unused
        }
    }

    public struct VCLRField(Header r, int dataSize) {
        public ByteColor3[] Colors = r.ReadSArray<ByteColor3>(dataSize / 24); // 24-bit RGB
    }

    public struct VTEXField {
        public ushort[] TextureIndicesT3;
        public uint[] TextureIndicesT4;

        public VTEXField(Header r, int dataSize) {
            if (r.Format == TES3) {
                TextureIndicesT3 = r.ReadPArray<ushort>("H", dataSize >> 1);
                TextureIndicesT4 = null;
                return;
            }
            TextureIndicesT3 = null;
            TextureIndicesT4 = r.ReadPArray<uint>("I", dataSize >> 2);
        }
    }

    // TES3
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CORDField {
        public override readonly string ToString() => $"{CellX},{CellY}";
        public static (string, int) Struct = ("<2i", 8);
        public int CellX;
        public int CellY;
    }

    public struct WNAMField {
        // Low-LOD heightmap (signed chars)
        public WNAMField(Header r, int dataSize) {
            r.Skip(dataSize);
            //var heightCount = dataSize;
            //for (var i = 0; i < heightCount; i++) { var height = r.ReadByte(); }
        }
    }

    // TES4
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
        public static (string, int) Struct = ("<2Hf", 8);
        public ushort Position;
        public ushort Pad01;
        public float Opacity;
    }

    public class ATXTGroup {
        public BTXTField ATXT;
        public VTXTField[] VTXTs;
    }

    public override string ToString() => $"LAND: {INTV}";
    public IN32Field DATA; // Unknown (default of 0x09) Changing this value makes the land 'disappear' in the editor.
    // A RGB color map 65x65 pixels in size representing the land normal vectors.
    // The signed value of the 'color' represents the vector's component. Blue
    // is vertical(Z), Red the X direction and Green the Y direction.Note that
    // the y-direction of the data is from the bottom up.
    public VNMLField VNML;
    public VHGTField VHGT; // Height data
    public VNMLField? VCLR; // Vertex color array, looks like another RBG image 65x65 pixels in size. (Optional)
    public VTEXField? VTEX; // A 16x16 array of short texture indices. (Optional)
    // TES3
    public CORDField INTV; // The cell coordinates of the cell
    public WNAMField WNAM; // Unknown byte data.
    // TES4
    public BTXTField[] BTXTs = new BTXTField[4]; // Base Layer
    public ATXTGroup[] ATXTs; // Alpha Layer
    ATXTGroup _lastATXT;

    public Int3 GridId; // => new Int3(INTV.CellX, INTV.CellY, 0);

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.DATA => DATA = r.ReadS<IN32Field>(dataSize),
        FieldType.VNML => VNML = new VNMLField(r, dataSize),
        FieldType.VHGT => VHGT = new VHGTField(r, dataSize),
        FieldType.VCLR => VCLR = new VNMLField(r, dataSize),
        FieldType.VTEX => VTEX = new VTEXField(r, dataSize),
        // TES3
        FieldType.INTV => INTV = r.ReadS<CORDField>(dataSize),
        FieldType.WNAM => WNAM = new WNAMField(r, dataSize),
        // TES4
        FieldType.BTXT => this.Then(r.ReadS<BTXTField>(dataSize), btxt => BTXTs[btxt.Quadrant] = btxt),
        FieldType.ATXT => (ATXTs ??= new ATXTGroup[4], this.Then(r.ReadS<BTXTField>(dataSize), atxt => _lastATXT = ATXTs[atxt.Quadrant] = new ATXTGroup { ATXT = atxt })),
        FieldType.VTXT => _lastATXT.VTXTs = r.ReadSArray<VTXTField>(dataSize >> 3),
        _ => Empty,
    };
}

/// <summary>
/// LEVC.Leveled Creature - 3000
/// </summary>
public class LEVCRecord : Record {
    public IN32Field DATA; // List data - 1 = Calc from all levels <= PC level
    public BYTEField NNAM; // Chance None?
    public IN32Field INDX; // Number of items in list
    public List<STRVField> CNAMs = []; // ID string of list item
    public List<IN16Field> INTVs = []; // PC level for previous CNAM
    // The CNAM/INTV can occur many times in pairs

    public override object CreateField(Header r, FieldType type, int dataSize) => r.Format == TES3
        ? type switch {
            FieldType.NAME => EDID = r.ReadSTRV(dataSize),
            FieldType.DATA => DATA = r.ReadS<IN32Field>(dataSize),
            FieldType.NNAM => NNAM = r.ReadS<BYTEField>(dataSize),
            FieldType.INDX => INDX = r.ReadS<IN32Field>(dataSize),
            FieldType.CNAM => CNAMs.AddX(r.ReadSTRV(dataSize)),
            FieldType.INTV => INTVs.AddX(r.ReadS<IN16Field>(dataSize)),
            _ => Empty,
        }
        : null;
}

/// <summary>
/// LEVI.Leveled item - 3000
/// </summary>
public class LEVIRecord : Record {
    public IN32Field DATA; // List data - 1 = Calc from all levels <= PC level, 2 = Calc for each item
    public BYTEField NNAM; // Chance None?
    public IN32Field INDX; // Number of items in list
    public List<STRVField> INAMs = []; // ID string of list item
    public List<IN16Field> INTVs = []; // PC level for previous INAM
    // The CNAM/INTV can occur many times in pairs

    public override object CreateField(Header r, FieldType type, int dataSize) => r.Format == TES3
        ? type switch {
            FieldType.NAME => EDID = r.ReadSTRV(dataSize),
            FieldType.DATA => DATA = r.ReadS<IN32Field>(dataSize),
            FieldType.NNAM => NNAM = r.ReadS<BYTEField>(dataSize),
            FieldType.INDX => INDX = r.ReadS<IN32Field>(dataSize),
            FieldType.INAM => INAMs.AddX(r.ReadSTRV(dataSize)),
            FieldType.INTV => INTVs.AddX(r.ReadS<IN16Field>(dataSize)),
            _ => Empty,
        }
        : null;
}

/// <summary>
/// LIGH.Light - 3450
/// </summary>
public class LIGHRecord : Record, IHaveMODL {
    // TESX
    public struct DATAField {
        [Flags]
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
        // TES4
        public float FalloffExponent;
        public float FOV;

        public DATAField(Header r, int dataSize) {
            if (r.Format == TES3) {
                Weight = r.ReadSingle();
                Value = r.ReadInt32();
                Time = r.ReadInt32();
                Radius = r.ReadInt32();
                LightColor = r.ReadS<ByteColor4>(4);
                Flags = r.ReadInt32();
                FalloffExponent = 1;
                FOV = 90;
                return;
            }
            Time = r.ReadInt32();
            Radius = r.ReadInt32();
            LightColor = r.ReadS<ByteColor4>(4);
            Flags = r.ReadInt32();
            if (dataSize == 32) { FalloffExponent = r.ReadSingle(); FOV = r.ReadSingle(); }
            else { FalloffExponent = 1; FOV = 90; }
            Value = r.ReadInt32();
            Weight = r.ReadSingle();
        }
    }

    public MODLGroup MODL { get; set; } // Model
    public STRVField? FULL; // Item Name (optional)
    public DATAField DATA; // Light Data
    public STRVField? SCPT; // Script Name (optional)??
    public RefField<SCPTRecord>? SCRI; // Script FormId (optional)
    public FILEField? ICON; // Male Icon (optional)
    public FLTVField FNAM; // Fade Value
    public RefField<SOUNRecord> SNAM; // Sound FormId (optional)

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadSTRV(dataSize),
        FieldType.FULL => FULL = r.ReadSTRV(dataSize),
        FieldType.FNAM => r.Format != TES3 ? FNAM = r.ReadS<FLTVField>(dataSize) : FULL = r.ReadSTRV(dataSize),
        FieldType.DATA or FieldType.LHDT => DATA = new DATAField(r, dataSize),
        FieldType.SCPT => SCPT = r.ReadSTRV(dataSize),
        FieldType.SCRI => SCRI = new RefField<SCPTRecord>(r, dataSize),
        FieldType.ICON or FieldType.ITEX => ICON = r.ReadFILE(dataSize),
        FieldType.MODL => MODL = new MODLGroup(r, dataSize),
        FieldType.MODB => MODL.MODBField(r, dataSize),
        FieldType.MODT => MODL.MODTField(r, dataSize),
        FieldType.SNAM => SNAM = new RefField<SOUNRecord>(r, dataSize),
        _ => Empty,
    };
}

/// <summary>
/// LOCK.Lock - 3450
/// </summary>
public class LOCKRecord : Record, IHaveMODL {
    public struct LKDTField(Header r, int dataSize) {
        public float Weight = r.ReadSingle();
        public int Value = r.ReadInt32();
        public float Quality = r.ReadSingle();
        public int Uses = r.ReadInt32();
    }

    public MODLGroup MODL { get; set; } // Model Name
    public STRVField FNAM; // Item Name
    public LKDTField LKDT; // Lock Data
    public FILEField ICON; // Inventory Icon
    public RefField<SCPTRecord> SCRI; // Script Name

    public override object CreateField(Header r, FieldType type, int dataSize) => r.Format == TES3
        ? type switch {
            FieldType.NAME => EDID = r.ReadSTRV(dataSize),
            FieldType.MODL => MODL = new MODLGroup(r, dataSize),
            FieldType.FNAM => FNAM = r.ReadSTRV(dataSize),
            FieldType.LKDT => LKDT = new LKDTField(r, dataSize),
            FieldType.ITEX => ICON = r.ReadFILE(dataSize),
            FieldType.SCRI => SCRI = new RefField<SCPTRecord>(r, dataSize),
            _ => Empty,
        }
        : null;
}

/// <summary>
/// LSCR.Load Screen - 0450
/// </summary>
public class LSCRRecord : Record {
    public struct LNAMField(Header r, int dataSize) {
        public Ref<Record> Direct = new(r.ReadUInt32());
        public Ref<WRLDRecord> IndirectWorld = new(r.ReadUInt32());
        public short IndirectGridX = r.ReadInt16();
        public short IndirectGridY = r.ReadInt16();
    }

    public FILEField ICON; // Icon
    public STRVField DESC; // Description
    public List<LNAMField> LNAMs; // LoadForm

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        FieldType.ICON => ICON = r.ReadFILE(dataSize),
        FieldType.DESC => DESC = r.ReadSTRV(dataSize),
        FieldType.LNAM => (LNAMs ??= []).AddX(new LNAMField(r, dataSize)),
        _ => Empty,
    };
}

/// <summary>
/// LTEX.Land Texture - 3450
/// </summary>
public class LTEXRecord : Record {
    public struct HNAMField(Header r, int dataSize) {
        public byte MaterialType = r.ReadByte();
        public byte Friction = r.ReadByte();
        public byte Restitution = r.ReadByte();
    }

    public FILEField ICON; // Texture
    // TES3
    public INTVField INTV;
    // TES4
    public HNAMField HNAM; // Havok data
    public BYTEField SNAM; // Texture specular exponent
    public List<RefField<GRASRecord>> GNAMs = []; // Potential grass

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadSTRV(dataSize),
        FieldType.INTV => INTV = r.ReadINTV(dataSize),
        FieldType.ICON or FieldType.DATA => ICON = r.ReadFILE(dataSize),
        // TES4
        FieldType.HNAM => HNAM = new HNAMField(r, dataSize),
        FieldType.SNAM => SNAM = r.ReadS<BYTEField>(dataSize),
        FieldType.GNAM => GNAMs.AddX(new RefField<GRASRecord>(r, dataSize)),
        _ => Empty,
    };
}

/// <summary>
/// LVLC.Leveled Creature - 0400
/// </summary>
public class LVLCRecord : Record {
    public BYTEField LVLD; // Chance
    public BYTEField LVLF; // Flags - 0x01 = Calculate from all levels <= player's level, 0x02 = Calculate for each item in count
    public RefField<SCPTRecord> SCRI; // Script (optional)
    public RefField<CREARecord> TNAM; // Creature Template (optional)
    public List<LVLIRecord.LVLOField> LVLOs = [];

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        FieldType.LVLD => LVLD = r.ReadS<BYTEField>(dataSize),
        FieldType.LVLF => LVLF = r.ReadS<BYTEField>(dataSize),
        FieldType.SCRI => SCRI = new RefField<SCPTRecord>(r, dataSize),
        FieldType.TNAM => TNAM = new RefField<CREARecord>(r, dataSize),
        FieldType.LVLO => LVLOs.AddX(new LVLIRecord.LVLOField(r, dataSize)),
        _ => Empty,
    };
}

/// <summary>
/// LVLI.Leveled Item - 0400
/// </summary>
public class LVLIRecord : Record {
    public struct LVLOField {
        public short Level;
        public Ref<Record> ItemFormId;
        public int Count;

        public LVLOField(Header r, int dataSize) {
            Level = r.ReadInt16();
            r.Skip(2); // Unused
            ItemFormId = new Ref<Record>(r.ReadUInt32());
            if (dataSize == 12) {
                Count = r.ReadInt16();
                r.Skip(2); // Unused
            }
            else Count = 0;
        }
    }

    public BYTEField LVLD; // Chance
    public BYTEField LVLF; // Flags - 0x01 = Calculate from all levels <= player's level, 0x02 = Calculate for each item in count
    public BYTEField? DATA; // Data (optional)
    public List<LVLOField> LVLOs = [];

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        FieldType.LVLD => LVLD = r.ReadS<BYTEField>(dataSize),
        FieldType.LVLF => LVLF = r.ReadS<BYTEField>(dataSize),
        FieldType.DATA => DATA = r.ReadS<BYTEField>(dataSize),
        FieldType.LVLO => LVLOs.AddX(new LVLOField(r, dataSize)),
        _ => Empty,
    };
}

/// <summary>
/// LVSP.Leveled Spell - 0400
/// </summary>
public class LVSPRecord : Record {
    public BYTEField LVLD; // Chance
    public BYTEField LVLF; // Flags
    public List<LVLIRecord.LVLOField> LVLOs = []; // Number of items in list

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        FieldType.LVLD => LVLD = r.ReadS<BYTEField>(dataSize),
        FieldType.LVLF => LVLF = r.ReadS<BYTEField>(dataSize),
        FieldType.LVLO => LVLOs.AddX(new LVLIRecord.LVLOField(r, dataSize)),
        _ => Empty,
    };
}

/// <summary>
/// MGEF.Magic Effect - 3400
/// </summary>
public class MGEFRecord : Record {
    // TES3
    public struct MEDTField(Header r, int dataSize) {
        public int SpellSchool = r.ReadInt32(); // 0 = Alteration, 1 = Conjuration, 2 = Destruction, 3 = Illusion, 4 = Mysticism, 5 = Restoration
        public float BaseCost = r.ReadSingle();
        public int Flags = r.ReadInt32(); // 0x0200 = Spellmaking, 0x0400 = Enchanting, 0x0800 = Negative
        public ByteColor4 Color = new((byte)r.ReadInt32(), (byte)r.ReadInt32(), (byte)r.ReadInt32(), 255);
        public float SpeedX = r.ReadSingle();
        public float SizeX = r.ReadSingle();
        public float SizeCap = r.ReadSingle();
    }

    // TES4
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
        SprayProjectileType = 0x02000000, // (Ball if Spray, Bolt or Fog is not specified)
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
        public uint CounterEffectCount; // Must be updated automatically when ESCE length changes!
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
            Flags = r.ReadUInt32();
            BaseCost = r.ReadSingle();
            AssocItem = r.ReadInt32();
            //wbUnion('Assoc. Item', wbMGEFFAssocItemDecider, [
            //  wbFormIDCk('Unused', [NULL]),
            //  wbFormIDCk('Assoc. Weapon', [WEAP]),
            //  wbFormIDCk('Assoc. Armor', [ARMO, NULL{?}]),
            //  wbFormIDCk('Assoc. Creature', [CREA, LVLC, NPC_]),
            //  wbInteger('Assoc. Actor Value', itS32, wbActorValueEnum)
            MagicSchool = r.ReadInt32();
            ResistValue = r.ReadInt32();
            CounterEffectCount = r.ReadUInt16();
            r.Skip(2); // Unused
            Light = new Ref<LIGHRecord>(r.ReadUInt32());
            ProjectileSpeed = r.ReadSingle();
            EffectShader = new Ref<EFSHRecord>(r.ReadUInt32());
            if (dataSize == 36) return;
            EnchantEffect = new Ref<EFSHRecord>(r.ReadUInt32());
            CastingSound = new Ref<SOUNRecord>(r.ReadUInt32());
            BoltSound = new Ref<SOUNRecord>(r.ReadUInt32());
            HitSound = new Ref<SOUNRecord>(r.ReadUInt32());
            AreaSound = new Ref<SOUNRecord>(r.ReadUInt32());
            ConstantEffectEnchantmentFactor = r.ReadSingle();
            ConstantEffectBarterFactor = r.ReadSingle();
        }
    }

    public override string ToString() => $"MGEF: {INDX.Value}:{EDID.Value}";
    public STRVField DESC; // Description
                           // TES3
    public INTVField INDX; // The Effect ID (0 to 137)
    public MEDTField MEDT; // Effect Data
    public FILEField ICON; // Effect Icon
    public STRVField PTEX; // Particle texture
    public STRVField CVFX; // Casting visual
    public STRVField BVFX; // Bolt visual
    public STRVField HVFX; // Hit visual
    public STRVField AVFX; // Area visual
    public STRVField? CSND; // Cast sound (optional)
    public STRVField? BSND; // Bolt sound (optional)
    public STRVField? HSND; // Hit sound (optional)
    public STRVField? ASND; // Area sound (optional)
                            // TES4
    public STRVField FULL;
    public MODLGroup MODL;
    public DATAField DATA;
    public STRVField[] ESCEs;

    public override object CreateField(Header r, FieldType type, int dataSize) => r.Format == TES3
        ? type switch {
            FieldType.INDX => INDX = r.ReadINTV(dataSize),
            FieldType.MEDT => MEDT = new MEDTField(r, dataSize),
            FieldType.ITEX => ICON = r.ReadFILE(dataSize),
            FieldType.PTEX => PTEX = r.ReadSTRV(dataSize),
            FieldType.CVFX => CVFX = r.ReadSTRV(dataSize),
            FieldType.BVFX => BVFX = r.ReadSTRV(dataSize),
            FieldType.HVFX => HVFX = r.ReadSTRV(dataSize),
            FieldType.AVFX => AVFX = r.ReadSTRV(dataSize),
            FieldType.DESC => DESC = r.ReadSTRV(dataSize),
            FieldType.CSND => CSND = r.ReadSTRV(dataSize),
            FieldType.BSND => BSND = r.ReadSTRV(dataSize),
            FieldType.HSND => HSND = r.ReadSTRV(dataSize),
            FieldType.ASND => ASND = r.ReadSTRV(dataSize),
            _ => Empty,
        }
        : type switch {
            FieldType.EDID => EDID = r.ReadSTRV(dataSize),
            FieldType.FULL => FULL = r.ReadSTRV(dataSize),
            FieldType.DESC => DESC = r.ReadSTRV(dataSize),
            FieldType.ICON => ICON = r.ReadFILE(dataSize),
            FieldType.MODL => MODL = new MODLGroup(r, dataSize),
            FieldType.MODB => MODL.MODBField(r, dataSize),
            FieldType.DATA => DATA = new DATAField(r, dataSize),
            FieldType.ESCE => ESCEs = r.ReadFArray(z => r.ReadSTRV(4), dataSize >> 2),
            _ => Empty,
        };
}

/// <summary>
/// MISC.Misc Item - 3450
/// </summary>
public class MISCRecord : Record, IHaveMODL {
    // TESX
    public struct DATAField {
        public float Weight;
        public uint Value;
        public uint Unknown;

        public DATAField(Header r, int dataSize) {
            if (r.Format == TES3) {
                Weight = r.ReadSingle();
                Value = r.ReadUInt32();
                Unknown = r.ReadUInt32();
                return;
            }
            Value = r.ReadUInt32();
            Weight = r.ReadSingle();
            Unknown = 0;
        }
    }

    public MODLGroup MODL { get; set; } // Model
    public STRVField FULL; // Item Name
    public DATAField DATA; // Misc Item Data
    public FILEField ICON; // Icon (optional)
    public RefField<SCPTRecord> SCRI; // Script FormID (optional)
    // TES3
    public RefField<ENCHRecord> ENAM; // enchantment ID

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadSTRV(dataSize),
        FieldType.MODL => MODL = new MODLGroup(r, dataSize),
        FieldType.MODB => MODL.MODBField(r, dataSize),
        FieldType.MODT => MODL.MODTField(r, dataSize),
        FieldType.FULL or FieldType.FNAM => FULL = r.ReadSTRV(dataSize),
        FieldType.DATA or FieldType.MCDT => DATA = new DATAField(r, dataSize),
        FieldType.ICON or FieldType.ITEX => ICON = r.ReadFILE(dataSize),
        FieldType.ENAM => ENAM = new RefField<ENCHRecord>(r, dataSize),
        FieldType.SCRI => SCRI = new RefField<SCPTRecord>(r, dataSize),
        _ => Empty,
    };
}

/// <summary>
/// NPC_.Non-Player Character - 3450
/// </summary>
public class NPC_Record : Record, IHaveMODL {
    [Flags]
    public enum NPC_Flags : uint {
        Female = 0x0001,
        Essential = 0x0002,
        Respawn = 0x0004,
        None_ = 0x0008,
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
        // 12 byte version
        // public short Level;
        // public byte Disposition;
        // public byte FactionId;
        // public byte Rank;
        // public byte Unknown1;
        public byte Unknown2;
        public byte Unknown3;
        // public int Gold;

        public NPDTField(Header r, int dataSize) {
            if (dataSize == 52) {
                Level = r.ReadInt16();
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
                Health = r.ReadInt16();
                SpellPts = r.ReadInt16();
                Fatigue = r.ReadInt16();
                Disposition = r.ReadByte();
                FactionId = r.ReadByte();
                Rank = r.ReadByte();
                Unknown1 = r.ReadByte();
                Gold = r.ReadInt32();
            }
            else {
                Level = r.ReadInt16();
                Disposition = r.ReadByte();
                FactionId = r.ReadByte();
                Rank = r.ReadByte();
                Unknown1 = r.ReadByte();
                Unknown2 = r.ReadByte();
                Unknown3 = r.ReadByte();
                Gold = r.ReadInt32();
            }
        }
    }

    public struct DODTField(Header r, int dataSize) {
        public float XPos = r.ReadSingle();
        public float YPos = r.ReadSingle();
        public float ZPos = r.ReadSingle();
        public float XRot = r.ReadSingle();
        public float YRot = r.ReadSingle();
        public float ZRot = r.ReadSingle();
    }

    public STRVField FULL; // NPC name
    public MODLGroup MODL { get; set; } // Animation
    public STRVField RNAM; // Race Name
    public STRVField ANAM; // Faction name
    public STRVField BNAM; // Head model
    public STRVField CNAM; // Class name
    public STRVField KNAM; // Hair model
    public NPDTField NPDT; // NPC Data
    public INTVField FLAG; // NPC Flags
    public List<CNTOField> NPCOs = []; // NPC item
    public List<STRVField> NPCSs = []; // NPC spell
    public CREARecord.AIDTField AIDT; // AI data
    public CREARecord.AI_WField? AI_W; // AI
    public CREARecord.AI_TField? AI_T; // AI Travel
    public CREARecord.AI_FField? AI_F; // AI Follow
    public CREARecord.AI_FField? AI_E; // AI Escort
    public STRVField? CNDT; // Cell escort/follow to string (optional)
    public CREARecord.AI_AField? AI_A; // AI Activate
    public DODTField DODT; // Cell Travel Destination
    public STRVField DNAM; // Cell name for previous DODT, if interior
    public FLTVField? XSCL; // Scale (optional) Only present if the scale is not 1.0
    public RefField<SCPTRecord>? SCRI; // Unknown

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadSTRV(dataSize),
        FieldType.FULL or FieldType.FNAM => FULL = r.ReadSTRV(dataSize),
        FieldType.MODL => MODL = new MODLGroup(r, dataSize),
        FieldType.MODB => MODL.MODBField(r, dataSize),
        FieldType.RNAM => RNAM = r.ReadSTRV(dataSize),
        FieldType.ANAM => ANAM = r.ReadSTRV(dataSize),
        FieldType.BNAM => BNAM = r.ReadSTRV(dataSize),
        FieldType.CNAM => CNAM = r.ReadSTRV(dataSize),
        FieldType.KNAM => KNAM = r.ReadSTRV(dataSize),
        FieldType.NPDT => NPDT = new NPDTField(r, dataSize),
        FieldType.FLAG => FLAG = r.ReadINTV(dataSize),
        FieldType.NPCO => NPCOs.AddX(new CNTOField(r, dataSize)),
        FieldType.NPCS => NPCSs.AddX(r.ReadSTRV_ZPad(dataSize)),
        FieldType.AIDT => AIDT = new CREARecord.AIDTField(r, dataSize),
        FieldType.AI_W => AI_W = new CREARecord.AI_WField(r, dataSize),
        FieldType.AI_T => AI_T = new CREARecord.AI_TField(r, dataSize),
        FieldType.AI_F => AI_F = new CREARecord.AI_FField(r, dataSize),
        FieldType.AI_E => AI_E = new CREARecord.AI_FField(r, dataSize),
        FieldType.CNDT => CNDT = r.ReadSTRV(dataSize),
        FieldType.AI_A => AI_A = new CREARecord.AI_AField(r, dataSize),
        FieldType.DODT => DODT = new DODTField(r, dataSize),
        FieldType.DNAM => DNAM = r.ReadSTRV(dataSize),
        FieldType.XSCL => XSCL = r.ReadS<FLTVField>(dataSize),
        FieldType.SCRI => SCRI = new RefField<SCPTRecord>(r, dataSize),
        _ => Empty,
    };
}

/// <summary>
/// PACK.AI Package - 0450
/// </summary>
public class PACKRecord : Record {
    public struct PKDTField {
        public ushort Flags;
        public byte Type;

        public PKDTField(Header r, int dataSize) {
            Flags = r.ReadUInt16();
            Type = r.ReadByte();
            r.Skip(dataSize - 3); // Unused
        }
    }

    public struct PLDTField(Header r, int dataSize) {
        public int Type = r.ReadInt32();
        public uint Target = r.ReadUInt32();
        public int Radius = r.ReadInt32();
    }

    public struct PSDTField(Header r, int dataSize) {
        public byte Month = r.ReadByte();
        public byte DayOfWeek = r.ReadByte();
        public byte Date = r.ReadByte();
        public sbyte Time = r.ReadSByte();
        public int Duration = r.ReadInt32();
    }

    public struct PTDTField(Header r, int dataSize) {
        public int Type = r.ReadInt32();
        public uint Target = r.ReadUInt32();
        public int Count = r.ReadInt32();
    }

    public PKDTField PKDT; // General
    public PLDTField PLDT; // Location
    public PSDTField PSDT; // Schedule
    public PTDTField PTDT; // Target
    public List<SCPTRecord.CTDAField> CTDAs = []; // Conditions

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        FieldType.PKDT => PKDT = new PKDTField(r, dataSize),
        FieldType.PLDT => PLDT = new PLDTField(r, dataSize),
        FieldType.PSDT => PSDT = new PSDTField(r, dataSize),
        FieldType.PTDT => PTDT = new PTDTField(r, dataSize),
        FieldType.CTDA or FieldType.CTDT => CTDAs.AddX(new SCPTRecord.CTDAField(r, dataSize)),
        _ => Empty,
    };
}

/// <summary>
/// PGRD.Path grid - 3400
/// </summary>
public class PGRDRecord : Record {
    public struct DATAField {
        public int X;
        public int Y;
        public short Granularity;
        public short PointCount;

        public DATAField(Header r, int dataSize) {
            if (r.Format != TES3) {
                X = Y = Granularity = 0;
                PointCount = r.ReadInt16();
                return;
            }
            X = r.ReadInt32();
            Y = r.ReadInt32();
            Granularity = r.ReadInt16();
            PointCount = r.ReadInt16();
        }
    }

    public struct PGRPField {
        public Vector3 Point;
        public byte Connections;

        public PGRPField(Header r, int dataSize) {
            Point = r.ReadVector3();
            Connections = r.ReadByte();
            r.Skip(3); // Unused
        }
    }

    public struct PGRRField(Header r, int dataSize) {
        public short StartPointId = r.ReadInt16();
        public short EndPointId = r.ReadInt16();
    }

    public struct PGRIField(Header r, int dataSize) {
        public short PointId = r.ReadInt16();
        public Vector3 ForeignNode = r.Skip(2).ReadVector3(); // 2:Unused (can merge back)
    }

    public struct PGRLField(Header r, int dataSize) {
        public Ref<REFRRecord> Reference = new(r.ReadUInt32());
        public short[] PointIds = r.ReadFArray(z => (r.ReadInt16(), r.Skip(2)).Item1, (dataSize - 4) >> 2); // 2:Unused (can merge back)
    }

    public DATAField DATA; // Number of nodes
    public PGRPField[] PGRPs;
    public UNKNField PGRC;
    public UNKNField PGAG;
    public PGRRField[] PGRRs; // Point-to-Point Connections
    public List<PGRLField> PGRLs; // Point-to-Reference Mappings
    public PGRIField[] PGRIs; // Inter-Cell Connections

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadSTRV(dataSize),
        FieldType.DATA => DATA = new DATAField(r, dataSize),
        FieldType.PGRP => PGRPs = r.ReadFArray(z => new PGRPField(r, 16), dataSize >> 4),
        FieldType.PGRC => PGRC = r.ReadUNKN(dataSize),
        FieldType.PGAG => PGAG = r.ReadUNKN(dataSize),
        FieldType.PGRR => (PGRRs = r.ReadFArray(z => new PGRRField(r, 4), dataSize >> 2), r.Skip(dataSize % 4)).Item1,
        FieldType.PGRL => (PGRLs ??= []).AddX(new PGRLField(r, dataSize)),
        FieldType.PGRI => PGRIs = r.ReadFArray(z => new PGRIField(r, 16), dataSize >> 4),
        _ => Empty,
    };
}

/// <summary>
/// PROB.Probe - 3000
/// </summary>
public class PROBRecord : Record, IHaveMODL {
    public struct PBDTField(Header r, int dataSize) {
        public float Weight = r.ReadSingle();
        public int Value = r.ReadInt32();
        public float Quality = r.ReadSingle();
        public int Uses = r.ReadInt32();
    }

    public MODLGroup MODL { get; set; } // Model Name
    public STRVField FNAM; // Item Name
    public PBDTField PBDT; // Probe Data
    public FILEField ICON; // Inventory Icon
    public RefField<SCPTRecord> SCRI; // Script Name

    public override object CreateField(Header r, FieldType type, int dataSize) => r.Format == TES3
        ? type switch {
            FieldType.NAME => EDID = r.ReadSTRV(dataSize),
            FieldType.MODL => MODL = new MODLGroup(r, dataSize),
            FieldType.FNAM => FNAM = r.ReadSTRV(dataSize),
            FieldType.PBDT => PBDT = new PBDTField(r, dataSize),
            FieldType.ITEX => ICON = r.ReadFILE(dataSize),
            FieldType.SCRI => SCRI = new RefField<SCPTRecord>(r, dataSize),
            _ => Empty,
        }
        : null;
}

/// <summary>
/// QUST.Quest - 0450
/// </summary>
public class QUSTRecord : Record {
    public struct DATAField(Header r, int dataSize) {
        public byte Flags = r.ReadByte();
        public byte Priority = r.ReadByte();
    }

    public STRVField FULL; // Item Name
    public FILEField ICON; // Icon
    public DATAField DATA; // Icon
    public RefField<SCPTRecord> SCRI; // Script Name
    public SCPTRecord.SCHRField SCHR; // Script Data
    public BYTVField SCDA; // Compiled Script
    public STRVField SCTX; // Script Source
    public List<RefField<Record>> SCROs = []; // Global variable reference

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        FieldType.FULL => FULL = r.ReadSTRV(dataSize),
        FieldType.ICON => ICON = r.ReadFILE(dataSize),
        FieldType.DATA => DATA = new DATAField(r, dataSize),
        FieldType.SCRI => SCRI = new RefField<SCPTRecord>(r, dataSize),
        FieldType.CTDA => r.Skip(dataSize),
        FieldType.INDX => r.Skip(dataSize),
        FieldType.QSDT => r.Skip(dataSize),
        FieldType.CNAM => r.Skip(dataSize),
        FieldType.QSTA => r.Skip(dataSize),
        FieldType.SCHR => SCHR = new SCPTRecord.SCHRField(r, dataSize),
        FieldType.SCDA => SCDA = r.ReadBYTV(dataSize),
        FieldType.SCTX => SCTX = r.ReadSTRV(dataSize),
        FieldType.SCRO => SCROs.AddX(new RefField<Record>(r, dataSize)),
        _ => Empty,
    };
}
/// <summary>
/// RACE.Race_Creature type - 3450
/// </summary>
public class RACERecord : Record {
    // TESX
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
            OverlayHeadPartList = 0x04000000, //{> Only one can be active <}
            OverrideHeadPartList = 0x08000000, //{> Only one can be active <}
            CanPickupItems = 0x10000000,
            AllowMultipleMembraneShaders = 0x20000000,
            CanDualWield = 0x40000000,
            AvoidsRoads = 0x80000000,
        }

        public struct SkillBoost {
            public byte SkillId;
            public sbyte Bonus;

            public SkillBoost(Header r, int dataSize) {
                if (r.Format == TES3) {
                    SkillId = (byte)r.ReadInt32();
                    Bonus = (sbyte)r.ReadInt32();
                    return;
                }
                SkillId = r.ReadByte();
                Bonus = r.ReadSByte();
            }
        }

        public struct RaceStats {
            public float Height;
            public float Weight;
            // Attributes;
            public byte Strength;
            public byte Intelligence;
            public byte Willpower;
            public byte Agility;
            public byte Speed;
            public byte Endurance;
            public byte Personality;
            public byte Luck;
        }

        public SkillBoost[] SkillBoosts = new SkillBoost[7]; // Skill Boosts
        public RaceStats Male = new();
        public RaceStats Female = new();
        public uint Flags; // 1 = Playable 2 = Beast Race

        public DATAField(Header r, int dataSize) {
            if (r.Format == TES3) {
                for (var i = 0; i < SkillBoosts.Length; i++) SkillBoosts[i] = new SkillBoost(r, 8);
                Male.Strength = (byte)r.ReadInt32(); Female.Strength = (byte)r.ReadInt32();
                Male.Intelligence = (byte)r.ReadInt32(); Female.Intelligence = (byte)r.ReadInt32();
                Male.Willpower = (byte)r.ReadInt32(); Female.Willpower = (byte)r.ReadInt32();
                Male.Agility = (byte)r.ReadInt32(); Female.Agility = (byte)r.ReadInt32();
                Male.Speed = (byte)r.ReadInt32(); Female.Speed = (byte)r.ReadInt32();
                Male.Endurance = (byte)r.ReadInt32(); Female.Endurance = (byte)r.ReadInt32();
                Male.Personality = (byte)r.ReadInt32(); Female.Personality = (byte)r.ReadInt32();
                Male.Luck = (byte)r.ReadInt32(); Female.Luck = (byte)r.ReadInt32();
                Male.Height = r.ReadSingle(); Female.Height = r.ReadSingle();
                Male.Weight = r.ReadSingle(); Female.Weight = r.ReadSingle();
                Flags = r.ReadUInt32();
                return;
            }
            for (var i = 0; i < SkillBoosts.Length; i++) SkillBoosts[i] = new SkillBoost(r, 2);
            r.ReadInt16(); // padding
            Male.Height = r.ReadSingle(); Female.Height = r.ReadSingle();
            Male.Weight = r.ReadSingle(); Female.Weight = r.ReadSingle();
            Flags = r.ReadUInt32();
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

    // TES4
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

    public STRVField FULL; // Race name
    public STRVField DESC; // Race description
    public List<STRVField> SPLOs = []; // NPCs: Special power/ability name
    // TESX
    public DATAField DATA; // RADT:DATA/ATTR: Race data/Base Attributes
    // TES4
    public Ref2Field<RACERecord> VNAM; // Voice
    public Ref2Field<HAIRRecord> DNAM; // Default Hair
    public BYTEField CNAM; // Default Hair Color
    public FLTVField PNAM; // FaceGen - Main clamp
    public FLTVField UNAM; // FaceGen - Face clamp
    public UNKNField XNAM; // Unknown
    //
    public List<RefField<HAIRRecord>> HNAMs = [];
    public List<RefField<EYESRecord>> ENAMs = [];
    public BYTVField FGGS; // FaceGen Geometry-Symmetric
    public BYTVField FGGA; // FaceGen Geometry-Asymmetric
    public BYTVField FGTS; // FaceGen Texture-Symmetric
    public UNKNField SNAM; // Unknown

    // Parts
    public List<FacePartGroup> FaceParts = [];
    public BodyGroup[] Bodys = [new BodyGroup(), new BodyGroup()];
    sbyte _nameState;
    sbyte _genderState;

    public override object CreateField(Header r, FieldType type, int dataSize) =>
        r.Format == TES3 ? type switch {
            FieldType.NAME => EDID = r.ReadSTRV(dataSize),
            FieldType.FNAM => FULL = r.ReadSTRV(dataSize),
            FieldType.RADT => DATA = new DATAField(r, dataSize),
            FieldType.NPCS => SPLOs.AddX(r.ReadSTRV(dataSize)),
            FieldType.DESC => DESC = r.ReadSTRV(dataSize),
            _ => Empty,
        }
        : r.Format == TES4 ? _nameState switch {
            // preamble
            0 => type switch {
                FieldType.EDID => EDID = r.ReadSTRV(dataSize),
                FieldType.FULL => FULL = r.ReadSTRV(dataSize),
                FieldType.DESC => DESC = r.ReadSTRV(dataSize),
                FieldType.DATA => DATA = new DATAField(r, dataSize),
                FieldType.SPLO => SPLOs.AddX(r.ReadSTRV(dataSize)),
                FieldType.VNAM => VNAM = new Ref2Field<RACERecord>(r, dataSize),
                FieldType.DNAM => DNAM = new Ref2Field<HAIRRecord>(r, dataSize),
                FieldType.CNAM => CNAM = r.ReadS<BYTEField>(dataSize),
                FieldType.PNAM => PNAM = r.ReadS<FLTVField>(dataSize),
                FieldType.UNAM => UNAM = r.ReadS<FLTVField>(dataSize),
                FieldType.XNAM => XNAM = r.ReadUNKN(dataSize),
                FieldType.ATTR => DATA.ATTRField(r, dataSize),
                FieldType.NAM0 => _nameState++,
                _ => Empty,
            },
            // face data
            1 => type switch {
                FieldType.INDX => FaceParts.AddX(new FacePartGroup { INDX = r.ReadS<UI32Field>(dataSize) }),
                FieldType.MODL => FaceParts.Last().MODL = new MODLGroup(r, dataSize),
                FieldType.ICON => FaceParts.Last().ICON = r.ReadFILE(dataSize),
                FieldType.MODB => FaceParts.Last().MODL.MODBField(r, dataSize),
                FieldType.NAM1 => _nameState++,
                _ => Empty,
            },
            // body data
            2 => type switch {
                FieldType.MNAM => _genderState = 0,
                FieldType.FNAM => _genderState = 1,
                FieldType.MODL => Bodys[_genderState].MODL = r.ReadFILE(dataSize),
                FieldType.MODB => Bodys[_genderState].MODB = r.ReadS<FLTVField>(dataSize),
                FieldType.INDX => Bodys[_genderState].BodyParts.AddX(new BodyPartGroup { INDX = r.ReadS<UI32Field>(dataSize) }),
                FieldType.ICON => Bodys[_genderState].BodyParts.Last().ICON = r.ReadFILE(dataSize),
                FieldType.HNAM => (HNAMs.AddRangeX(r.ReadFArray(z => new RefField<HAIRRecord>(r, 4), dataSize >> 2)), _nameState++).Item1,
                _ => Empty,
            },
            // postamble
            3 => type switch {
                FieldType.HNAM => HNAMs.AddRangeX(r.ReadFArray(z => new RefField<HAIRRecord>(r, 4), dataSize >> 2)),
                FieldType.ENAM => ENAMs.AddRangeX(r.ReadFArray(z => new RefField<EYESRecord>(r, 4), dataSize >> 2)),
                FieldType.FGGS => FGGS = r.ReadBYTV(dataSize),
                FieldType.FGGA => FGGA = r.ReadBYTV(dataSize),
                FieldType.FGTS => FGTS = r.ReadBYTV(dataSize),
                FieldType.SNAM => SNAM = r.ReadUNKN(dataSize),
                _ => Empty,
            },
            _ => Empty,
        }
        : null;
}

/// <summary>
/// REPA.Repair Item - 3000
/// </summary>
public class REPARecord : Record, IHaveMODL {
    public struct RIDTField(Header r, int dataSize) {
        public float Weight = r.ReadSingle();
        public int Value = r.ReadInt32();
        public int Uses = r.ReadInt32();
        public float Quality = r.ReadSingle();
    }

    public MODLGroup MODL { get; set; } // Model Name
    public STRVField FNAM; // Item Name
    public RIDTField RIDT; // Repair Data
    public FILEField ICON; // Inventory Icon
    public RefField<SCPTRecord> SCRI; // Script Name

    public override object CreateField(Header r, FieldType type, int dataSize) => r.Format == TES3
        ? type switch {
            FieldType.NAME => EDID = r.ReadSTRV(dataSize),
            FieldType.MODL => MODL = new MODLGroup(r, dataSize),
            FieldType.FNAM => FNAM = r.ReadSTRV(dataSize),
            FieldType.RIDT => RIDT = new RIDTField(r, dataSize),
            FieldType.ITEX => ICON = r.ReadFILE(dataSize),
            FieldType.SCRI => SCRI = new RefField<SCPTRecord>(r, dataSize),
            _ => Empty,
        }
        : null;
}

/// <summary>
/// REFR.Placed Object - 0450
/// </summary>
public class REFRRecord : Record {
    public struct XTELField(Header r, int dataSize) {
        public Ref<REFRRecord> Door = new(r.ReadUInt32());
        public Vector3 Position = new(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
        public Vector3 Rotation = new(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
    }

    public struct DATAField(Header r, int dataSize) {
        public Vector3 Position = new(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
        public Vector3 Rotation = new(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
    }

    public struct XLOCField {
        public override readonly string ToString() => $"{Key}";
        public byte LockLevel;
        public Ref<KEYMRecord> Key;
        public byte Flags;

        public XLOCField(Header r, int dataSize) {
            LockLevel = r.ReadByte();
            r.Skip(3); // Unused
            Key = new Ref<KEYMRecord>(r.ReadUInt32());
            if (dataSize == 16) r.Skip(4); // Unused
            Flags = r.ReadByte();
            r.Skip(3); // Unused
        }
    }

    public struct XESPField {
        public override readonly string ToString() => $"{Reference}";
        public Ref<Record> Reference;
        public byte Flags;

        public XESPField(Header r, int dataSize) {
            Reference = new Ref<Record>(r.ReadUInt32());
            Flags = r.ReadByte();
            r.Skip(3); // Unused
        }
    }

    public struct XSEDField {
        public override readonly string ToString() => $"{Seed}";
        public byte Seed;

        public XSEDField(Header r, int dataSize) {
            Seed = r.ReadByte();
            if (dataSize == 4) r.Skip(3); // Unused
        }
    }

    public class XMRKGroup {
        public override string ToString() => $"{FULL.Value}";
        public BYTEField FNAM; // Map Flags
        public STRVField FULL; // Name
        public BYTEField TNAM; // Type
    }

    public RefField<Record> NAME; // Base
    public XTELField? XTEL; // Teleport Destination (optional)
    public DATAField DATA; // Position/Rotation
    public XLOCField? XLOC; // Lock information (optional)
    public List<CELLRecord.XOWNGroup> XOWNs; // Ownership (optional)
    public XESPField? XESP; // Enable Parent (optional)
    public RefField<Record>? XTRG; // Target (optional)
    public XSEDField? XSED; // SpeedTree (optional)
    public BYTVField? XLOD; // Distant LOD Data (optional)
    public FLTVField? XCHG; // Charge (optional)
    public FLTVField? XHLT; // Health (optional)
    public RefField<CELLRecord>? XPCI; // Unused (optional)
    public IN32Field? XLCM; // Level Modifier (optional)
    public RefField<REFRRecord>? XRTM; // Unknown (optional)
    public UI32Field? XACT; // Action Flag (optional)
    public IN32Field? XCNT; // Count (optional)
    public List<XMRKGroup> XMRKs; // Ownership (optional)
    //public bool? ONAM; // Open by Default
    public BYTVField? XRGD; // Ragdoll Data (optional)
    public FLTVField? XSCL; // Scale (optional)
    public BYTEField? XSOL; // Contained Soul (optional)
    int _nextFull;

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        FieldType.NAME => NAME = new RefField<Record>(r, dataSize),
        FieldType.XTEL => XTEL = new XTELField(r, dataSize),
        FieldType.DATA => DATA = new DATAField(r, dataSize),
        FieldType.XLOC => XLOC = new XLOCField(r, dataSize),
        FieldType.XOWN => (XOWNs ??= []).AddX(new CELLRecord.XOWNGroup { XOWN = new RefField<Record>(r, dataSize) }),
        FieldType.XRNK => XOWNs.Last().XRNK = r.ReadS<IN32Field>(dataSize),
        FieldType.XGLB => XOWNs.Last().XGLB = new RefField<Record>(r, dataSize),
        FieldType.XESP => XESP = new XESPField(r, dataSize),
        FieldType.XTRG => XTRG = new RefField<Record>(r, dataSize),
        FieldType.XSED => XSED = new XSEDField(r, dataSize),
        FieldType.XLOD => XLOD = r.ReadBYTV(dataSize),
        FieldType.XCHG => XCHG = r.ReadS<FLTVField>(dataSize),
        FieldType.XHLT => XCHG = r.ReadS<FLTVField>(dataSize),
        FieldType.XPCI => (_nextFull = 1, XPCI = new RefField<CELLRecord>(r, dataSize)),
        FieldType.FULL => _nextFull == 1 ? XPCI.Value.SetName(r.ReadFAString(dataSize)) : _nextFull == 2 ? XMRKs.Last().FULL = r.ReadSTRV(dataSize) : _nextFull = 0,
        FieldType.XLCM => XLCM = r.ReadS<IN32Field>(dataSize),
        FieldType.XRTM => XRTM = new RefField<REFRRecord>(r, dataSize),
        FieldType.XACT => XACT = r.ReadS<UI32Field>(dataSize),
        FieldType.XCNT => XCNT = r.ReadS<IN32Field>(dataSize),
        FieldType.XMRK => (_nextFull = 2, (XMRKs ??= []).AddX(new XMRKGroup())),
        FieldType.FNAM => XMRKs.Last().FNAM = r.ReadS<BYTEField>(dataSize),
        FieldType.TNAM => XMRKs.Last().TNAM = r.ReadS<BYTEField>(dataSize),
        FieldType.ONAM => true,
        FieldType.XRGD => XRGD = r.ReadBYTV(dataSize),
        FieldType.XSCL => XSCL = r.ReadS<FLTVField>(dataSize),
        FieldType.XSOL => XSOL = r.ReadS<BYTEField>(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// REGN.Region - 3450
/// </summary>
public class REGNRecord : Record {
    // TESX
    public class RDATField {
        public enum REGNType : byte { Objects = 2, Weather, Map, Landscape, Grass, Sound }

        public uint Type;
        public REGNType Flags;
        public byte Priority;
        // groups
        public RDOTField[] RDOTs; // Objects
        public STRVField RDMP; // MapName
        public RDGSField[] RDGSs; // Grasses
        public UI32Field RDMD; // Music Type
        public RDSDField[] RDSDs; // Sounds
        public RDWTField[] RDWTs; // Weather Types

        public RDATField() { }
        public RDATField(Header r, int dataSize) {
            Type = r.ReadUInt32();
            Flags = (REGNType)r.ReadByte();
            Priority = r.ReadByte();
            r.Skip(2); // Unused
        }
    }

    public struct RDOTField {
        public override readonly string ToString() => $"{Object}";
        public Ref<Record> Object;
        public ushort ParentIdx;
        public float Density;
        public byte Clustering;
        public byte MinSlope; // (degrees)
        public byte MaxSlope; // (degrees)
        public byte Flags;
        public ushort RadiusWrtParent;
        public ushort Radius;
        public float MinHeight;
        public float MaxHeight;
        public float Sink;
        public float SinkVariance;
        public float SizeVariance;
        public Int3 AngleVariance;
        public ByteColor4 VertexShading; // RGB + Shading radius (0 - 200) %

        public RDOTField(Header r, int dataSize) {
            Object = new Ref<Record>(r.ReadUInt32());
            ParentIdx = r.ReadUInt16();
            r.Skip(2); // Unused
            Density = r.ReadSingle();
            Clustering = r.ReadByte();
            MinSlope = r.ReadByte();
            MaxSlope = r.ReadByte();
            Flags = r.ReadByte();
            RadiusWrtParent = r.ReadUInt16();
            Radius = r.ReadUInt16();
            MinHeight = r.ReadSingle();
            MaxHeight = r.ReadSingle();
            Sink = r.ReadSingle();
            SinkVariance = r.ReadSingle();
            SizeVariance = r.ReadSingle();
            AngleVariance = new Int3(r.ReadUInt16(), r.ReadUInt16(), r.ReadUInt16());
            r.Skip(2); // Unused
            VertexShading = r.ReadS<ByteColor4>(dataSize);
        }
    }

    public struct RDGSField {
        public override readonly string ToString() => $"{Grass}";
        public Ref<GRASRecord> Grass;

        public RDGSField(Header r, int dataSize) {
            Grass = new Ref<GRASRecord>(r.ReadUInt32());
            r.Skip(4); // Unused
        }
    }

    public struct RDSDField {
        public override readonly string ToString() => $"{Sound}";
        public Ref<SOUNRecord> Sound;
        public uint Flags;
        public uint Chance;

        public RDSDField(Header r, int dataSize) {
            if (r.Format == TES3) {
                Sound = new Ref<SOUNRecord>(r.ReadFAString(32));
                Flags = 0;
                Chance = r.ReadByte();
                return;
            }
            Sound = new Ref<SOUNRecord>(r.ReadUInt32());
            Flags = r.ReadUInt32();
            Chance = r.ReadUInt32(); //: float with TES5
        }
    }

    public struct RDWTField(Header r, int dataSize) {
        public override readonly string ToString() => $"{Weather}";
        public static byte SizeOf(FormType format) => format == TES4 ? (byte)8 : (byte)12;
        public Ref<WTHRRecord> Weather = new(r.ReadUInt32());
        public uint Chance = r.ReadUInt32();
        public Ref<GLOBRecord> Global = r.Format == TES5 ? new Ref<GLOBRecord>(r.ReadUInt32()) : new Ref<GLOBRecord>();
    }

    // TES3
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
            // v1.3 ESM files add 2 bytes to WEAT subrecords.
            if (dataSize == 10)
                r.Skip(2);
        }
    }

    // TES4
    public class RPLIField(Header r, int dataSize) {
        public uint EdgeFalloff = r.ReadUInt32(); // (World Units)
        public Vector2[] Points; // Region Point List Data

        public object RPLDField(Header r, int dataSize) {
            Points = new Vector2[dataSize >> 3];
            for (var i = 0; i < Points.Length; i++) Points[i] = new Vector2(r.ReadSingle(), r.ReadSingle());
            return Points;
        }
    }

    public STRVField ICON; // Icon / Sleep creature
    public RefField<WRLDRecord> WNAM; // Worldspace - Region name
    public CREFField RCLR; // Map Color (COLORREF)
    public List<RDATField> RDATs = []; // Region Data Entries / TES3: Sound Record (order determines the sound priority)
    // TES3
    public WEATField? WEAT; // Weather Data
    // TES4
    public List<RPLIField> RPLIs = []; // Region Areas

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadSTRV(dataSize),
        FieldType.WNAM or FieldType.FNAM => WNAM = new RefField<WRLDRecord>(r, dataSize),
        FieldType.WEAT => WEAT = new WEATField(r, dataSize),//: TES3
        FieldType.ICON or FieldType.BNAM => ICON = r.ReadSTRV(dataSize),
        FieldType.RCLR or FieldType.CNAM => RCLR = r.ReadS<CREFField>(dataSize),
        FieldType.SNAM => RDATs.AddX(new RDATField { RDSDs = [new RDSDField(r, dataSize)] }),
        FieldType.RPLI => RPLIs.AddX(new RPLIField(r, dataSize)),
        FieldType.RPLD => RPLIs.Last().RPLDField(r, dataSize),
        FieldType.RDAT => RDATs.AddX(new RDATField(r, dataSize)),
        FieldType.RDOT => RDATs.Last().RDOTs = r.ReadFArray(z => new RDOTField(r, dataSize), dataSize / 52),
        FieldType.RDMP => RDATs.Last().RDMP = r.ReadSTRV(dataSize),
        FieldType.RDGS => RDATs.Last().RDGSs = r.ReadFArray(z => new RDGSField(r, dataSize), dataSize / 8),
        FieldType.RDMD => RDATs.Last().RDMD = r.ReadS<UI32Field>(dataSize),
        FieldType.RDSD => RDATs.Last().RDSDs = r.ReadFArray(z => new RDSDField(r, dataSize), dataSize / 12),
        FieldType.RDWT => RDATs.Last().RDWTs = r.ReadFArray(z => new RDWTField(r, dataSize), dataSize / RDWTField.SizeOf(r.Format)),
        _ => Empty,
    };
}

/// <summary>
/// ROAD.Road - 0400
/// </summary>
public class ROADRecord : Record {
    public PGRDRecord.PGRPField[] PGRPs;
    public UNKNField PGRR;

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.PGRP => PGRPs = r.ReadFArray(z => new PGRDRecord.PGRPField(r, dataSize), dataSize >> 4),
        FieldType.PGRR => PGRR = r.ReadUNKN(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// SBSP.Subspace - 0400
/// </summary>
public class SBSPRecord : Record {
    public struct DNAMField(Header r, int dataSize) {
        public float X = r.ReadSingle(); // X dimension
        public float Y = r.ReadSingle(); // Y dimension
        public float Z = r.ReadSingle(); // Z dimension
    }

    public DNAMField DNAM;

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        FieldType.DNAM => DNAM = new DNAMField(r, dataSize),
        _ => Empty,
    };
}

/// <summary>
/// SCPT.Script - 3400
/// </summary>
public class SCPTRecord : Record {
    // TESX
    public struct CTDAField {
        public enum INFOType : byte { Nothing = 0, Function, Global, Local, Journal, Item, Dead, NotId, NotFaction, NotClass, NotRace, NotCell, NotLocal }

        // TES3: 0 = [=], 1 = [!=], 2 = [>], 3 = [>=], 4 = [<], 5 = [<=]
        // TES4: 0 = [=], 2 = [!=], 4 = [>], 6 = [>=], 8 = [<], 10 = [<=]
        public byte CompareOp;
        // (00-71) - sX = Global/Local/Not Local types, JX = Journal type, IX = Item Type, DX = Dead Type, XX = Not ID Type, FX = Not Faction, CX = Not Class, RX = Not Race, LX = Not Cell
        public string FunctionId;
        // TES3
        public byte Index; // (0-5)
        public byte Type;
        // Except for the function type, this is the ID for the global/local/etc. Is not nessecarily NULL terminated.The function type SCVR sub-record has
        public string Name;
        // TES4
        public float ComparisonValue;
        public int Parameter1; // Parameter #1
        public int Parameter2; // Parameter #2

        public CTDAField(Header r, int dataSize) {
            if (r.Format == TES3) {
                Index = r.ReadByte();
                Type = r.ReadByte();
                FunctionId = r.ReadFAString(2);
                CompareOp = (byte)(r.ReadByte() << 1);
                Name = r.ReadFAString(dataSize - 5);
                ComparisonValue = Parameter1 = Parameter2 = 0;
                return;
            }
            CompareOp = r.ReadByte();
            r.Skip(3); // Unused
            ComparisonValue = r.ReadSingle();
            FunctionId = r.ReadFAString(4);
            Parameter1 = r.ReadInt32();
            Parameter2 = r.ReadInt32();
            if (dataSize != 24) r.Skip(4); // Unused
            Index = Type = 0;
            Name = null;
        }
    }

    // TES3
    public class SCHDField(Header r, int dataSize) {
        public override string ToString() => $"{Name}";
        public string Name = r.ReadFAString(32);
        public int NumShorts = r.ReadInt32();
        public int NumLongs = r.ReadInt32();
        public int NumFloats = r.ReadInt32();
        public int ScriptDataSize = r.ReadInt32();
        public int LocalVarSize = r.ReadInt32();
        public string[] Variables = null;
        public object SCVRField(Header r, int dataSize) => Variables = r.ReadZAStringList(dataSize).ToArray();
    }

    // TES4
    public struct SCHRField {
        public override readonly string ToString() => $"{RefCount}";
        public uint RefCount;
        public uint CompiledSize;
        public uint VariableCount;
        public uint Type; // 0x000 = Object, 0x001 = Quest, 0x100 = Magic Effect

        public SCHRField(Header r, int dataSize) {
            r.Skip(4); // Unused
            RefCount = r.ReadUInt32();
            CompiledSize = r.ReadUInt32();
            VariableCount = r.ReadUInt32();
            Type = r.ReadUInt32();
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
            Idx = r.ReadUInt32();
            r.ReadUInt32(); // Unknown
            r.ReadUInt32(); // Unknown
            r.ReadUInt32(); // Unknown
            Type = r.ReadUInt32();
            r.ReadUInt32(); // Unknown
            // SCVRField
            VariableName = null;
        }
        public object SCVRField(Header r, int dataSize) => VariableName = r.ReadFUString(dataSize);
    }

    public override string ToString() => $"SCPT: {EDID.Value ?? SCHD.Name}";
    public BYTVField SCDA; // Compiled Script
    public STRVField SCTX; // Script Source
    // TES3
    public SCHDField SCHD; // Script Data
    // TES4
    public SCHRField SCHR; // Script Data
    public List<SLSDField> SLSDs = []; // Variable data
    public List<SLSDField> SCRVs = []; // Ref variable data (one for each ref declared)
    public List<RefField<Record>> SCROs = []; // Global variable reference

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        FieldType.SCHD => SCHD = new SCHDField(r, dataSize),
        FieldType.SCVR => r.Format != TES3 ? SLSDs.Last().SCVRField(r, dataSize) : SCHD.SCVRField(r, dataSize),
        FieldType.SCDA or FieldType.SCDT => SCDA = r.ReadBYTV(dataSize),
        FieldType.SCTX => SCTX = r.ReadSTRV(dataSize),
        // TES4
        FieldType.SCHR => SCHR = new SCHRField(r, dataSize),
        FieldType.SLSD => SLSDs.AddX(new SLSDField(r, dataSize)),
        FieldType.SCRO => SCROs.AddX(new RefField<Record>(r, dataSize)),
        FieldType.SCRV => SCRVs.AddX(this.Then(r.ReadUInt32(), idx => SLSDs.Single(x => x.Idx == idx))),
        _ => Empty,
    };
}

/// <summary>
/// SGST.Sigil Stone - 0400
/// </summary>
public class SGSTRecord : Record, IHaveMODL {
    public struct DATAField(Header r, int dataSize) {
        public byte Uses = r.ReadByte();
        public int Value = r.ReadInt32();
        public float Weight = r.ReadSingle();
    }

    public MODLGroup MODL { get; set; } // Model
    public STRVField FULL; // Item Name
    public DATAField DATA; // Sigil Stone Data
    public FILEField ICON; // Icon
    public RefField<SCPTRecord>? SCRI; // Script (optional)
    public List<ENCHRecord.EFITField> EFITs = []; // Effect Data
    public List<ENCHRecord.SCITField> SCITs = []; // Script Effect Data

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        FieldType.MODL => MODL = new MODLGroup(r, dataSize),
        FieldType.MODB => MODL.MODBField(r, dataSize),
        FieldType.MODT => MODL.MODTField(r, dataSize),
        FieldType.FULL => SCITs.Count == 0 ? FULL = r.ReadSTRV(dataSize) : SCITs.Last().FULLField(r, dataSize),
        FieldType.DATA => DATA = new DATAField(r, dataSize),
        FieldType.ICON => ICON = r.ReadFILE(dataSize),
        FieldType.SCRI => SCRI = new RefField<SCPTRecord>(r, dataSize),
        FieldType.EFID => r.Skip(dataSize),
        FieldType.EFIT => EFITs.AddX(new ENCHRecord.EFITField(r, dataSize)),
        FieldType.SCIT => SCITs.AddX(new ENCHRecord.SCITField(r, dataSize)),
        _ => Empty,
    };
}

/// <summary>
/// SKIL.Skill - 3450
/// </summary>
public class SKILRecord : Record {
    // TESX
    public struct DATAField {
        public int Action;
        public int Attribute;
        public uint Specialization; // 0 = Combat, 1 = Magic, 2 = Stealth
        public float[] UseValue; // The use types for each skill are hard-coded.

        public DATAField(Header r, int dataSize) {
            Action = r.Format == TES3 ? 0 : r.ReadInt32();
            Attribute = r.ReadInt32();
            Specialization = r.ReadUInt32();
            UseValue = new float[r.Format == TES3 ? 4 : 2];
            for (var i = 0; i < UseValue.Length; i++) UseValue[i] = r.ReadSingle();
        }
    }

    public override string ToString() => $"SKIL: {INDX.Value}:{EDID.Value}";
    public IN32Field INDX; // Skill ID
    public DATAField DATA; // Skill Data
    public STRVField DESC; // Skill description
    // TES4
    public FILEField ICON; // Icon
    public STRVField ANAM; // Apprentice Text
    public STRVField JNAM; // Journeyman Text
    public STRVField ENAM; // Expert Text
    public STRVField MNAM; // Master Text

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        FieldType.INDX => INDX = r.ReadS<IN32Field>(dataSize),
        FieldType.DATA or FieldType.SKDT => DATA = new DATAField(r, dataSize),
        FieldType.DESC => DESC = r.ReadSTRV(dataSize),
        FieldType.ICON => ICON = r.ReadFILE(dataSize),
        FieldType.ANAM => ANAM = r.ReadSTRV(dataSize),
        FieldType.JNAM => JNAM = r.ReadSTRV(dataSize),
        FieldType.ENAM => ENAM = r.ReadSTRV(dataSize),
        FieldType.MNAM => MNAM = r.ReadSTRV(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// SLGM.Soul Gem - 0450
/// </summary>
public class SLGMRecord : Record, IHaveMODL {
    public struct DATAField(Header r, int dataSize) {
        public int Value = r.ReadInt32();
        public float Weight = r.ReadSingle();
    }

    public MODLGroup MODL { get; set; } // Model
    public STRVField FULL; // Item Name
    public RefField<SCPTRecord> SCRI; // Script (optional)
    public DATAField DATA; // Type of soul contained in the gem
    public FILEField ICON; // Icon (optional)
    public BYTEField SOUL; // Type of soul contained in the gem
    public BYTEField SLCP; // Soul gem maximum capacity

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        FieldType.MODL => MODL = new MODLGroup(r, dataSize),
        FieldType.MODB => MODL.MODBField(r, dataSize),
        FieldType.MODT => MODL.MODTField(r, dataSize),
        FieldType.FULL => FULL = r.ReadSTRV(dataSize),
        FieldType.SCRI => SCRI = new RefField<SCPTRecord>(r, dataSize),
        FieldType.DATA => DATA = new DATAField(r, dataSize),
        FieldType.ICON => ICON = r.ReadFILE(dataSize),
        FieldType.SOUL => SOUL = r.ReadS<BYTEField>(dataSize),
        FieldType.SLCP => SLCP = r.ReadS<BYTEField>(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// SNDG.Sound Generator - 3000
/// </summary>
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

    public IN32Field DATA; // Sound Type Data
    public STRVField SNAM; // Sound ID
    public STRVField? CNAM; // Creature name (optional)

    public override object CreateField(Header r, FieldType type, int dataSize) => r.Format == TES3
        ? type switch {
            FieldType.NAME => EDID = r.ReadSTRV(dataSize),
            FieldType.DATA => DATA = r.ReadS<IN32Field>(dataSize),
            FieldType.SNAM => SNAM = r.ReadSTRV(dataSize),
            FieldType.CNAM => CNAM = r.ReadSTRV(dataSize),
            _ => Empty,
        }
        : null;
}

/// <summary>
/// SNDR.Sound Reference - 0050
/// </summary>
public class SNDRRecord : Record {
    public CREFField CNAM; // RGB color

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        FieldType.CNAM => CNAM = r.ReadS<CREFField>(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// SOUN.Sound - 3450
/// </summary>
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

    // TESX
    public class DATAField {
        public byte Volume; // (0=0.00, 255=1.00)
        public byte MinRange; // Minimum attenuation distance
        public byte MaxRange; // Maximum attenuation distance
        // Bethesda4
        public sbyte FrequencyAdjustment; // Frequency adjustment %
        public ushort Flags; // Flags
        public ushort StaticAttenuation; // Static Attenuation (db)
        public byte StopTime; // Stop time
        public byte StartTime; // Start time

        public DATAField(Header r, int dataSize) {
            Volume = r.Format == TES3 ? r.ReadByte() : (byte)0;
            MinRange = r.ReadByte();
            MaxRange = r.ReadByte();
            if (r.Format == TES3) return;
            FrequencyAdjustment = r.ReadSByte();
            r.ReadByte(); // Unused
            Flags = r.ReadUInt16();
            r.ReadUInt16(); // Unused
            if (dataSize == 8) return;
            StaticAttenuation = r.ReadUInt16();
            StopTime = r.ReadByte();
            StartTime = r.ReadByte();
        }
    }

    public FILEField FNAM; // Sound Filename (relative to Sounds\)
    public DATAField DATA; // Sound Data

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadSTRV(dataSize),
        FieldType.FNAM => FNAM = r.ReadFILE(dataSize),
        FieldType.SNDX => DATA = new DATAField(r, dataSize),
        FieldType.SNDD => DATA = new DATAField(r, dataSize),
        FieldType.DATA => DATA = new DATAField(r, dataSize),
        _ => Empty,
    };
}

/// <summary>
/// SPEL.Spell - 3450
/// </summary>
public class SPELRecord : Record {
    // TESX
    public struct SPITField(Header r, int dataSize) {
        public override readonly string ToString() => $"{Type}";
        // TES3: 0 = Spell, 1 = Ability, 2 = Blight, 3 = Disease, 4 = Curse, 5 = Power
        // TES4: 0 = Spell, 1 = Disease, 2 = Power, 3 = Lesser Power, 4 = Ability, 5 = Poison
        public uint Type = r.ReadUInt32();
        public int SpellCost = r.ReadInt32();
        public uint Flags = r.ReadUInt32(); // 0x0001 = AutoCalc, 0x0002 = PC Start, 0x0004 = Always Succeeds
        // TES4
        public int SpellLevel = r.Format != TES3 ? r.ReadInt32() : 0;
    }

    public STRVField FULL; // Spell name
    public SPITField SPIT; // Spell data
    public List<ENCHRecord.EFITField> EFITs = []; // Effect Data
    // TES4
    public List<ENCHRecord.SCITField> SCITs = []; // Script effect data

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadSTRV(dataSize),
        FieldType.FULL => SCITs.Count == 0 ? FULL = r.ReadSTRV(dataSize) : SCITs.Last().FULLField(r, dataSize),
        FieldType.FNAM => FULL = r.ReadSTRV(dataSize),
        FieldType.SPIT or FieldType.SPDT => SPIT = new SPITField(r, dataSize),
        FieldType.EFID => r.Skip(dataSize),
        FieldType.EFIT or FieldType.ENAM => EFITs.AddX(new ENCHRecord.EFITField(r, dataSize)),
        FieldType.SCIT => SCITs.AddX(new ENCHRecord.SCITField(r, dataSize)),
        _ => Empty,
    };
}

/// <summary>
/// SSCR.Start Script - 3000
/// </summary>
public class SSCRRecord : Record {
    public STRVField DATA; // Digits

    public override object CreateField(Header r, FieldType type, int dataSize) => r.Format == TES3
        ? type switch {
            FieldType.NAME => EDID = r.ReadSTRV(dataSize),
            FieldType.DATA => DATA = r.ReadSTRV(dataSize),
            _ => Empty,
        }
        : null;
}

/// <summary>
/// STAT.Static - 3450
/// </summary>
public class STATRecord : Record, IHaveMODL {
    public MODLGroup MODL { get; set; } // Model

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadSTRV(dataSize),
        FieldType.MODL => MODL = new MODLGroup(r, dataSize),
        FieldType.MODB => MODL.MODBField(r, dataSize),
        FieldType.MODT => MODL.MODTField(r, dataSize),
        _ => Empty,
    };
}

/// <summary>
/// TES3.Plugin Info - 3000
/// </summary>
public class TES3Record : Record {
    public struct HEDRField(Header r, int dataSize) {
        public float Version = r.ReadSingle();
        public uint FileType = r.ReadUInt32();
        public string CompanyName = r.ReadFAString(32);
        public string FileDescription = r.ReadFAString(256);
        public uint NumRecords = r.ReadUInt32();
    }

    public HEDRField HEDR;
    public List<STRVField> MASTs;
    public List<INTVField> DATAs;

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.HEDR => HEDR = new HEDRField(r, dataSize),
        FieldType.MAST => (MASTs ??= []).AddX(r.ReadSTRV(dataSize)),
        FieldType.DATA => (DATAs ??= []).AddX(r.ReadINTV(dataSize)),
        _ => Empty,
    };
}

/// <summary>
/// TES4.Plugin Info - 0450
/// </summary>
public unsafe class TES4Record : Record {
    public struct HEDRField {
        public static (string, int) Struct = ("<fiI", 12);
        public float Version;
        public int NumRecords; // Number of records and groups (not including TES4 record itself).
        public uint NextObjectId; // Next available object ID.
    }

    public HEDRField HEDR;
    public STRVField? CNAM; // author (Optional)
    public STRVField? SNAM; // description (Optional)
    public List<STRVField> MASTs; // master
    public List<INTVField> DATAs; // fileSize
    public UNKNField? ONAM; // overrides (Optional)
    public IN32Field INTV; // unknown
    public IN32Field? INCC; // unknown (Optional)
    // TES5
    public UNKNField? TNAM; // overrides (Optional)

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.HEDR => HEDR = r.ReadS<HEDRField>(dataSize),
        FieldType.OFST => r.Skip(dataSize),
        FieldType.DELE => r.Skip(dataSize),
        FieldType.CNAM => CNAM = r.ReadSTRV(dataSize),
        FieldType.SNAM => SNAM = r.ReadSTRV(dataSize),
        FieldType.MAST => (MASTs ??= []).AddX(r.ReadSTRV(dataSize)),
        FieldType.DATA => (DATAs ??= []).AddX(r.ReadINTV(dataSize)),
        FieldType.ONAM => ONAM = r.ReadUNKN(dataSize),
        FieldType.INTV => INTV = r.ReadS<IN32Field>(dataSize),
        FieldType.INCC => INCC = r.ReadS<IN32Field>(dataSize),
        // TES5
        FieldType.TNAM => TNAM = r.ReadUNKN(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// TREE.Tree - 0450
/// </summary>
public class TREERecord : Record, IHaveMODL {
    public struct SNAMField {
        public int[] Values;

        public SNAMField(Header r, int dataSize) {
            Values = new int[dataSize >> 2];
            for (var i = 0; i < Values.Length; i++)
                Values[i] = r.ReadInt32();
        }
    }

    public struct CNAMField(Header r, int dataSize) {
        public float LeafCurvature = r.ReadSingle();
        public float MinimumLeafAngle = r.ReadSingle();
        public float MaximumLeafAngle = r.ReadSingle();
        public float BranchDimmingValue = r.ReadSingle();
        public float LeafDimmingValue = r.ReadSingle();
        public int ShadowRadius = r.ReadInt32();
        public float RockSpeed = r.ReadSingle();
        public float RustleSpeed = r.ReadSingle();
    }

    public struct BNAMField(Header r, int dataSize) {
        public float Width = r.ReadSingle();
        public float Height = r.ReadSingle();
    }

    public MODLGroup MODL { get; set; } // Model
    public FILEField ICON; // Leaf Texture
    public SNAMField SNAM; // SpeedTree Seeds, array of ints
    public CNAMField CNAM; // Tree Parameters
    public BNAMField BNAM; // Billboard Dimensions

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        FieldType.MODL => MODL = new MODLGroup(r, dataSize),
        FieldType.MODB => MODL.MODBField(r, dataSize),
        FieldType.MODT => MODL.MODTField(r, dataSize),
        FieldType.ICON => ICON = r.ReadFILE(dataSize),
        FieldType.SNAM => SNAM = new SNAMField(r, dataSize),
        FieldType.CNAM => CNAM = new CNAMField(r, dataSize),
        FieldType.BNAM => BNAM = new BNAMField(r, dataSize),
        _ => Empty,
    };
}

/// <summary>
/// WATR.Water Type - 0450
/// </summary>
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
            WindVelocity = r.ReadSingle();
            WindDirection = r.ReadSingle();
            WaveAmplitude = r.ReadSingle();
            WaveFrequency = r.ReadSingle();
            SunPower = r.ReadSingle();
            ReflectivityAmount = r.ReadSingle();
            FresnelAmount = r.ReadSingle();
            ScrollXSpeed = r.ReadSingle();
            ScrollYSpeed = r.ReadSingle();
            FogDistance_NearPlane = r.ReadSingle();
            if (dataSize == 42) { Damage = r.ReadUInt16(); return; }
            FogDistance_FarPlane = r.ReadSingle();
            ShallowColor = r.ReadS<ByteColor4>(dataSize);
            DeepColor = r.ReadS<ByteColor4>(dataSize);
            ReflectionColor = r.ReadS<ByteColor4>(dataSize);
            TextureBlend = r.ReadByte();
            r.Skip(3); // Unused
            if (dataSize == 62) { Damage = r.ReadUInt16(); return; }
            RainSimulator_Force = r.ReadSingle();
            RainSimulator_Velocity = r.ReadSingle();
            RainSimulator_Falloff = r.ReadSingle();
            RainSimulator_Dampner = r.ReadSingle();
            RainSimulator_StartingSize = r.ReadSingle();
            DisplacementSimulator_Force = r.ReadSingle();
            if (dataSize == 86) {
                //DisplacementSimulator_Velocity = DisplacementSimulator_Falloff = DisplacementSimulator_Dampner = DisplacementSimulator_StartingSize = 0F;
                Damage = r.ReadUInt16();
                return;
            }
            DisplacementSimulator_Velocity = r.ReadSingle();
            DisplacementSimulator_Falloff = r.ReadSingle();
            DisplacementSimulator_Dampner = r.ReadSingle();
            DisplacementSimulator_StartingSize = r.ReadSingle();
            Damage = r.ReadUInt16();
        }
    }

    public struct GNAMField(Header r, int dataSize) {
        public Ref<WATRRecord> Daytime = new(r.ReadUInt32());
        public Ref<WATRRecord> Nighttime = new(r.ReadUInt32());
        public Ref<WATRRecord> Underwater = new(r.ReadUInt32());
    }

    public STRVField TNAM; // Texture
    public BYTEField ANAM; // Opacity
    public BYTEField FNAM; // Flags
    public STRVField MNAM; // Material ID
    public RefField<SOUNRecord> SNAM; // Sound
    public DATAField DATA; // DATA
    public GNAMField GNAM; // GNAM

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        FieldType.TNAM => TNAM = r.ReadSTRV(dataSize),
        FieldType.ANAM => ANAM = r.ReadS<BYTEField>(dataSize),
        FieldType.FNAM => FNAM = r.ReadS<BYTEField>(dataSize),
        FieldType.MNAM => MNAM = r.ReadSTRV(dataSize),
        FieldType.SNAM => SNAM = new RefField<SOUNRecord>(r, dataSize),
        FieldType.DATA => DATA = new DATAField(r, dataSize),
        FieldType.GNAM => GNAM = new GNAMField(r, dataSize),
        _ => Empty,
    };
}

/// <summary>
/// WEAP.Weapon - 3450
/// </summary>
public class WEAPRecord : Record, IHaveMODL {
    public struct DATAField {
        public enum WEAPType { ShortBladeOneHand = 0, LongBladeOneHand, LongBladeTwoClose, BluntOneHand, BluntTwoClose, BluntTwoWide, SpearTwoWide, AxeOneHand, AxeTwoHand, MarksmanBow, MarksmanCrossbow, MarksmanThrown, Arrow, Bolt, }

        public float Weight;
        public int Value;
        public ushort Type;
        public short Health;
        public float Speed;
        public float Reach;
        public short Damage; //: EnchantPts;
        public byte ChopMin;
        public byte ChopMax;
        public byte SlashMin;
        public byte SlashMax;
        public byte ThrustMin;
        public byte ThrustMax;
        public int Flags; // 0 = ?, 1 = Ignore Normal Weapon Resistance?

        public DATAField(Header r, int dataSize) {
            if (r.Format == TES3) {
                Weight = r.ReadSingle();
                Value = r.ReadInt32();
                Type = r.ReadUInt16();
                Health = r.ReadInt16();
                Speed = r.ReadSingle();
                Reach = r.ReadSingle();
                Damage = r.ReadInt16();
                ChopMin = r.ReadByte();
                ChopMax = r.ReadByte();
                SlashMin = r.ReadByte();
                SlashMax = r.ReadByte();
                ThrustMin = r.ReadByte();
                ThrustMax = r.ReadByte();
                Flags = r.ReadInt32();
                return;
            }
            Type = (ushort)r.ReadUInt32();
            Speed = r.ReadSingle();
            Reach = r.ReadSingle();
            Flags = r.ReadInt32();
            Value = r.ReadInt32();
            Health = (short)r.ReadInt32();
            Weight = r.ReadSingle();
            Damage = r.ReadInt16();
            ChopMin = ChopMax = SlashMin = SlashMax = ThrustMin = ThrustMax = 0;
        }
    }

    public MODLGroup MODL { get; set; } // Model
    public STRVField FULL; // Item Name
    public DATAField DATA; // Weapon Data
    public FILEField ICON; // Male Icon (optional)
    public RefField<ENCHRecord> ENAM; // Enchantment ID
    public RefField<SCPTRecord> SCRI; // Script (optional)
                                      // TES4
    public IN16Field? ANAM; // Enchantment points (optional)

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadSTRV(dataSize),
        FieldType.MODL => MODL = new MODLGroup(r, dataSize),
        FieldType.MODB => MODL.MODBField(r, dataSize),
        FieldType.MODT => MODL.MODTField(r, dataSize),
        FieldType.FULL or FieldType.FNAM => FULL = r.ReadSTRV(dataSize),
        FieldType.DATA or FieldType.WPDT => DATA = new DATAField(r, dataSize),
        FieldType.ICON or FieldType.ITEX => ICON = r.ReadFILE(dataSize),
        FieldType.ENAM => ENAM = new RefField<ENCHRecord>(r, dataSize),
        FieldType.SCRI => SCRI = new RefField<SCPTRecord>(r, dataSize),
        FieldType.ANAM => ANAM = r.ReadS<IN16Field>(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// WRLD.Worldspace - 0450
/// </summary>
public unsafe class WRLDRecord : Record {
    public struct MNAMField {
        public static (string, int) Struct = ($"<2i4h", 16);
        public Int2 UsableDimensions;
        // Cell Coordinates
        public short NWCell_X;
        public short NWCell_Y;
        public short SECell_X;
        public short SECell_Y;
    }

    public struct NAM0Field(Header r, int dataSize) {
        //public static (string, int) Struct = ("<2f", 8);
        //public static (string, int) Struct = ("<4f", 16);
        public Vector2 Min = new(r.ReadSingle(), r.ReadSingle());
        public Vector2 Max = Vector2.Zero;
        public object NAM9Field(Header r, int dataSize) => Max = new Vector2(r.ReadSingle(), r.ReadSingle());
    }

    // TES5
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
            GridX = r.ReadInt16();
            GridY = r.ReadInt16();
            var referenceCount = r.ReadUInt32();
            var referenceSize = dataSize - 8;
            Log.Assert(referenceSize >> 3 == referenceCount);
            GridReferences = r.ReadSArray<Reference>(referenceSize >> 3);
        }
    }

    public STRVField FULL;
    public RefField<WRLDRecord>? WNAM; // Parent Worldspace
    public RefField<CLMTRecord>? CNAM; // Climate
    public RefField<WATRRecord>? NAM2; // Water
    public FILEField? ICON; // Icon
    public MNAMField? MNAM; // Map Data
    public BYTEField? DATA; // Flags
    public NAM0Field NAM0; // Object Bounds
    public UI32Field? SNAM; // Music
    // TES5
    public List<RNAMField> RNAMs = []; // Large References

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        FieldType.FULL => FULL = r.ReadSTRV(dataSize),
        FieldType.WNAM => WNAM = new RefField<WRLDRecord>(r, dataSize),
        FieldType.CNAM => CNAM = new RefField<CLMTRecord>(r, dataSize),
        FieldType.NAM2 => NAM2 = new RefField<WATRRecord>(r, dataSize),
        FieldType.ICON => ICON = r.ReadFILE(dataSize),
        FieldType.MNAM => MNAM = r.ReadS<MNAMField>(dataSize),
        FieldType.DATA => DATA = r.ReadS<BYTEField>(dataSize),
        FieldType.NAM0 => NAM0 = new NAM0Field(r, dataSize),
        FieldType.NAM9 => NAM0.NAM9Field(r, dataSize),
        FieldType.SNAM => SNAM = r.ReadS<UI32Field>(dataSize),
        FieldType.OFST => r.Skip(dataSize),
        // TES5
        FieldType.RNAM => RNAMs.AddX(new RNAMField(r, dataSize)),
        _ => Empty,
    };
}

/// <summary>
/// WTHR.Weather - 0450
/// </summary>
public class WTHRRecord : Record, IHaveMODL {
    public struct FNAMField(Header r, int dataSize) {
        public float DayNear = r.ReadSingle();
        public float DayFar = r.ReadSingle();
        public float NightNear = r.ReadSingle();
        public float NightFar = r.ReadSingle();
    }

    public struct HNAMField(Header r, int dataSize) {
        public float EyeAdaptSpeed = r.ReadSingle();
        public float BlurRadius = r.ReadSingle();
        public float BlurPasses = r.ReadSingle();
        public float EmissiveMult = r.ReadSingle();
        public float TargetLUM = r.ReadSingle();
        public float UpperLUMClamp = r.ReadSingle();
        public float BrightScale = r.ReadSingle();
        public float BrightClamp = r.ReadSingle();
        public float LUMRampNoTex = r.ReadSingle();
        public float LUMRampMin = r.ReadSingle();
        public float LUMRampMax = r.ReadSingle();
        public float SunlightDimmer = r.ReadSingle();
        public float GrassDimmer = r.ReadSingle();
        public float TreeDimmer = r.ReadSingle();
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
        public Ref<SOUNRecord> Sound = new(r.ReadUInt32()); // Sound FormId
        public uint Type = r.ReadUInt32(); // Sound Type - 0=Default, 1=Precipitation, 2=Wind, 3=Thunder
    }

    public MODLGroup MODL { get; set; } // Model
    public FILEField CNAM; // Lower Cloud Layer
    public FILEField DNAM; // Upper Cloud Layer
    public BYTVField NAM0; // Colors by Types/Times
    public FNAMField FNAM; // Fog Distance
    public HNAMField HNAM; // HDR Data
    public DATAField DATA; // Weather Data
    public List<SNAMField> SNAMs = []; // Sounds

    public override object CreateField(Header r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadSTRV(dataSize),
        FieldType.MODL => MODL = new MODLGroup(r, dataSize),
        FieldType.MODB => MODL.MODBField(r, dataSize),
        FieldType.CNAM => CNAM = r.ReadFILE(dataSize),
        FieldType.DNAM => DNAM = r.ReadFILE(dataSize),
        FieldType.NAM0 => NAM0 = r.ReadBYTV(dataSize),
        FieldType.FNAM => FNAM = new FNAMField(r, dataSize),
        FieldType.HNAM => HNAM = new HNAMField(r, dataSize),
        FieldType.DATA => DATA = new DATAField(r, dataSize),
        FieldType.SNAM => SNAMs.AddX(new SNAMField(r, dataSize)),
        _ => Empty,
    };
}

#endregion
