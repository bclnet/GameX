using GameX.Uncore.Formats;
using OpenStack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using static GameX.Bethesda.Formats.Records.FormType;
using static System.IO.Polyfill;
#pragma warning disable CS9113

namespace GameX.Bethesda.Formats.Records;

#region Links

// TES3
//https://github.com/TES5Edit/TES5Edit/blob/dev/wbDefinitionsTES3.pas
//https://en.uesp.net/morrow/tech/mw_esm.txt
//https://github.com/mlox/mlox/blob/master/util/tes3cmd/tes3cmd
// TES4
//https://github.com/WrinklyNinja/esplugin/tree/master/src
//https://github.com/TES5Edit/TES5Edit/blob/dev/wbDefinitionsTES4.pas 
// TES5
//https://github.com/TES5Edit/TES5Edit/blob/dev/wbDefinitionsTES5.pas 
//https://en.uesp.net/wiki/TES3Mod:Mod_File_Format
//https://en.uesp.net/wiki/TES4Mod:Mod_File_Format
//https://en.uesp.net/wiki/TES5Mod:Mod_File_Format

//https://tes5edit.github.io/fopdoc/Fallout3/Records.html
//https://tes5edit.github.io/fopdoc/FalloutNV/Records.html
//https://tes5edit.github.io/fopdoc/Fallout4/Records.html

//https://github.com/TES5Edit/TES5Edit/blob/dev-4.1.6/Core/wbDefinitionsFNV.pas
//https://github.com/TES5Edit/TES5Edit/blob/dev-4.1.6/Core/wbDefinitionsFO3.pas
//https://github.com/TES5Edit/TES5Edit/blob/dev-4.1.6/Core/wbDefinitionsFO4.pas
//https://github.com/TES5Edit/TES5Edit/blob/dev-4.1.6/Core/wbDefinitionsFO76.pas
//https://github.com/TES5Edit/TES5Edit/blob/dev-4.1.6/Core/wbDefinitionsSF1.pas
//https://github.com/TES5Edit/TES5Edit/blob/dev-4.1.6/Core/wbDefinitionsTES3.pas
//https://github.com/TES5Edit/TES5Edit/blob/dev-4.1.6/Core/wbDefinitionsTES4.pas
//https://github.com/TES5Edit/TES5Edit/blob/dev-4.1.6/Core/wbDefinitionsTES5.pas

#endregion

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
    BOIM = 0x4D494F42,
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
    PWAT = 0x54415750,
    QUST = 0x54535551,
    RFCT = 0x54434652,
    REGN = 0x4E474552,
    RACE = 0x45434152,
    RADS = 0x53444152,
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
    STDT = 0x54445453,
    SUNP = 0x504E5553,
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
    TMLM = 0x4D4C4D54,
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
// Encoding.ASCII.GetString(BitConverter.GetBytes(0x53424341))
public enum FieldType : uint {
    AADT = 0x54444141,
    ACBS = 0x53424341,
    AHCF = 0x46434841,
    AHCM = 0x4D434841,
    AIDT = 0x54444941,
    AI_A = 0x415F4941,
    AI_E = 0x455F4941,
    AI_F = 0x465F4941,
    AI_T = 0x545F4941,
    AI_W = 0x575F4941,
    ALDT = 0x54444C41,
    AMBI = 0x49424D41,
    ANAM = 0x4D414E41,
    AODT = 0x54444F41,
    ASND = 0x444E5341,
    ATKD = 0x444B5441,
    ATKE = 0x454B5441,
    ATTR = 0x52545441,
    ATXT = 0x54585441,
    AVFX = 0x58465641,

    BKDT = 0x54444B42,
    BMDT = 0x54444D42,
    BNAM = 0x4D414E42,
    BODT = 0x54444F42,
    BPND = 0x444E5042,
    BPNI = 0x494E5042,
    BPNN = 0x4E4E5042,
    BPNT = 0x544E5042,
    BPTN = 0x4E545042,
    BSND = 0x444E5342,
    BTXT = 0x54585442,
    BVFX = 0x58465642,
    BYDT = 0x54445942,

    CIS2 = 0x32534943,
    CITC = 0x43544943,
    CLDT = 0x54444C43,
    CNAM = 0x4D414E43,
    CNDT = 0x54444E43,
    CNTO = 0x4F544E43,
    CRGR = 0x52475243,
    CRVA = 0x41565243,
    CSAD = 0x44415343,
    CSCR = 0x52435343,
    CSDI = 0x49445343,
    CSDC = 0x43445343,
    CSDT = 0x54445343,
    CSND = 0x444E5343,
    CSTD = 0x44545343,
    CTDA = 0x41445443,
    CTDT = 0x54445443,
    CVFX = 0x58465643,

    DATA = 0x41544144,
    DELE = 0x454C4544,
    DESC = 0x43534544,
    DFTF = 0x46544644,
    DFTM = 0x4D544644,
    DNAM = 0x4D414E44,
    DODT = 0x54444F44,

    EDID = 0x44494445,
    EFID = 0x44494645,
    EFIT = 0x54494645,
    ENAM = 0x4D414E45,
    ENDT = 0x54444E45,
    ENIT = 0x54494E45,
    ESCE = 0x45435345,

    FADT = 0x54444146,
    FGGA = 0x41474746,
    FGGS = 0x53474746,
    FGTS = 0x53544746,
    FLAG = 0x47414C46,
    FLMV = 0x564D4C46,
    FLTV = 0x56544C46,
    FNAM = 0x4D414E46,
    FRMR = 0x524D5246,
    FTSF = 0x46535446,
    FTSM = 0x4D535446,
    FULL = 0x4C4C5546,

    GNAM = 0x4D414E47,

    HCLF = 0x464C4348,
    HCLR = 0x524C4348,
    HEAD = 0x44414548,
    HEDR = 0x52444548,
    HNAM = 0x4D414E48,
    HSND = 0x444E5348,
    HVFX = 0x58465648,

    ICON = 0x4E4F4349,
    ICO2 = 0x324F4349,
    INAM = 0x4D414E49,
    INCC = 0x43434E49,
    INDX = 0x58444E49,
    INTV = 0x56544E49,
    IRDT = 0x54445249,
    ITEX = 0x58455449,

    JAIL = 0x4C49414A,
    JNAM = 0x4D414E4A,
    JOUT = 0x54554F4A,

    KFFZ = 0x5A46464B,
    KNAM = 0x4D414E4B,
    KSIZ = 0x5A49534B,
    KWDA = 0x4144574B,

    LHDT = 0x5444484C,
    LKDT = 0x54444B4C,
    LNAM = 0x4D414E4C,
    LVLD = 0x444C564C,
    LVLF = 0x464C564C,
    LVLO = 0x4F4C564C,

    MAST = 0x5453414D,
    MCDT = 0x5444434D,
    MEDT = 0x5444454D,
    MICO = 0x4F43494D,
    MNAM = 0x4D414E4D,
    MO2B = 0x42324F4D,
    MO2T = 0x54324F4D,
    MO3B = 0x42334F4D,
    MO3T = 0x54334F4D,
    MO4B = 0x42344F4D,
    MO4T = 0x54344F4D,
    MOD2 = 0x32444F4D,
    MOD3 = 0x33444F4D,
    MOD4 = 0x34444F4D,
    MODB = 0x42444F4D,
    MODD = 0x44444F4D,
    MODL = 0x4C444F4D,
    MODS = 0x53444F4D,
    MODT = 0x54444F4D,
    MPAI = 0x4941504D,
    MPAV = 0x5641504D,
    MTNM = 0x4D4E544D,
    MTYP = 0x5059544D,

    NAM0 = 0x304D414E,
    NAM1 = 0x314D414E,
    NAM2 = 0x324D414E,
    NAM3 = 0x334D414E,
    NAM4 = 0x344D414E,
    NAM5 = 0x354D414E,
    NAM7 = 0x374D414E,
    NAM8 = 0x384D414E,
    NAM9 = 0x394D414E,
    NAME = 0x454D414E,
    NIFT = 0x5446494E,
    NIFZ = 0x5A46494E,
    NNAM = 0x4D414E4E,
    NPCO = 0x4F43504E,
    NPCS = 0x5343504E,
    NPDT = 0x5444504E,

    OBND = 0x444E424F,
    OFST = 0x5453464F,
    ONAM = 0x4D414E4F,

    PBDT = 0x54444250,
    PFIG = 0x47494650,
    PFPC = 0x43504650,
    PGAG = 0x47414750,
    PGRC = 0x43524750,
    PGRI = 0x49524750,
    PGRL = 0x4C524750,
    PGRP = 0x50524750,
    PGRR = 0x52524750,
    PHTN = 0x4E544850,
    PHWT = 0x54574850,
    PKDT = 0x54444B50,
    PKID = 0x44494B50,
    PLCN = 0x4E434C50,
    PLDT = 0x54444C50,
    PLVD = 0x44564C50,
    PNAM = 0x4D414E50,
    PSDT = 0x54445350,
    PTDT = 0x54445450,
    PTEX = 0x58455450,

    QNAM = 0x4D414E51,
    QSDT = 0x54445351,
    QSTA = 0x41545351,
    QSTF = 0x46545351,
    QSTI = 0x49545351,
    QSTN = 0x4E545351,
    QSTR = 0x52545351,

    RADT = 0x54444152,
    RAGA = 0x41474152,
    RCLR = 0x524C4352,
    RDAT = 0x54414452,
    RDGS = 0x53474452,
    RDMD = 0x444D4452,
    RDMP = 0x504D4452,
    RDOT = 0x544F4452,
    RDSD = 0x44534452,
    RDWT = 0x54574452,
    RGNN = 0x4E4E4752,
    RIDT = 0x54444952,
    RNAM = 0x4D414E52,
    RNMV = 0x564D4E52,
    RPLD = 0x444C5052,
    RPLI = 0x494C5052,
    RPRF = 0x46525052,
    RPRM = 0x4D525052,

    SCDA = 0x41444353,
    SCDT = 0x54444353,
    SCHD = 0x44484353,
    SCHR = 0x52484353,
    SCIT = 0x54494353,
    SCPT = 0x54504353,
    SCRI = 0x49524353,
    SCRO = 0x4F524353,
    SCRV = 0x56524353,
    SCTX = 0x58544353,
    SCVR = 0x52564353,
    SDSC = 0x43534453,
    SKDT = 0x54444B53,
    SLCP = 0x50434C53,
    SLSD = 0x44534C53,
    SNAM = 0x4D414E53,
    SNDD = 0x44444E53,
    SNDX = 0x58444E53,
    SNMV = 0x564D4E53,
    SOUL = 0x4C554F53,
    SPCT = 0x54435053,
    SPDT = 0x54445053,
    SPED = 0x44455053,
    SPIT = 0x54495053,
    SPLO = 0x4F4C5053,
    STOL = 0x4C4F5453,
    STRV = 0x56525453,
    SWMV = 0x564D5753,

    TCLF = 0x464C4354,
    TCLT = 0x544C4354,
    TEXT = 0x54584554,
    TNAM = 0x4D414E54,
    TINC = 0x434E4954,
    TIND = 0x444E4954,
    TINI = 0x494E4954,
    TINL = 0x4C4E4954,
    TINP = 0x504E4954,
    TINT = 0x544E4954,
    TINV = 0x564E4954,
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

    UNAM = 0x4D414E55,
    UNES = 0x53454E55,

    VCLR = 0x524C4356,
    VENC = 0x434E4556,
    VEND = 0x444E4556,
    VENV = 0x564E4556,
    VHGT = 0x54474856,
    VNAM = 0x4D414E56,
    VNML = 0x4C4D4E56,
    VTCK = 0x4B435456,
    VTEX = 0x58455456,
    VTXT = 0x54585456,

    WAIT = 0x54494157,
    WEAT = 0x54414557,
    WHGT = 0x54474857,
    WKMV = 0x564D4B57,
    WLST = 0x54534C57,
    WNAM = 0x4D414E57,
    WPDT = 0x54445057,

    XACT = 0x54434158,
    XCCM = 0x4D434358,
    XCHG = 0x47484358,
    XCNT = 0x544E4358,
    XCLC = 0x434C4358,
    XCLL = 0x4C4C4358,
    XCLR = 0x524C4358,
    XCLW = 0x574C4358,
    XCMT = 0x544D4358,
    XCWT = 0x54574358,
    XESP = 0x50534558,
    XGLB = 0x424C4758,
    XHLT = 0x544C4858,
    XHRS = 0x53524858,
    XLCM = 0x4D434C58,
    XLOC = 0x434F4C58,
    XLOD = 0x444F4C58,
    XMRC = 0x43524D58,
    XMRK = 0x4B524D58,
    XNAM = 0x4D414E58,
    XOWN = 0x4E574F58,
    XPCI = 0x49435058,
    XRGD = 0x44475258,
    XRNK = 0x4B4E5258,
    XRTM = 0x4D545258,
    XSCL = 0x4C435358,
    XSED = 0x44455358,
    XSOL = 0x4C4F5358,
    XTEL = 0x4C455458,
    XTRG = 0x47525458,
    XXXX = 0x58585858,

    YNAM = 0x4D414E59,

    ZNAM = 0x4D414E5A,
}

public enum ActorValue : int {
    None_ = -1,
    Strength = 0, Intelligence, Willpower, Agility, Speed, Endurance, Personality, Luck, Health, Magicka, Fatigue, Encumbrance,
    Armorer, Athletics, Blade, Block, Blunt, HandToHand, HeavyArmor, Alchemy, Alteration, Conjuration, Destruction, Illusion,
    Mysticism, Restoration, Acrobatics, LightArmor, Marksman, Mercantile, Security, Sneak, Speechcraft,
    // Extra Actor Values
    Aggression, Confidence, Energy, Responsibility, Bounty, Fame, Infamy, MagickaMultiplier, NightEyeBonus, AttackBonus, DefendBonus, CastingPenalty, Blindness,
    Chameleon, Invisibility, Paralysis, Silence, Confusion, DetectItemRange, SpellAbsorbChance, SpellReflectChance, SwimSpeedMultiplier, WaterBreathing, WaterWalking, StuntedMagicka, DetectLifeRange,
    ReflectDamage, Telekinesis, ResistFire, ResistFrost, ResistDisease, ResistMagic, ResistNormalWeapons, ResistParalysis, ResistPoison, ResistShock, Vampirism, Darkness, ResistWaterDamage,
}

#endregion

#region Record

/// <summary>
/// Reader
/// </summary>
public class Reader(BinaryReader r, string binPath, FormType format, bool tes4a) : BinaryReader(r.BaseStream) {
    public string BinPath = binPath;
    public FormType Format = format;
    public bool Tes4a = tes4a;
    public int Version = 0;
}

/// <see cref="https://en.uesp.net/wiki/Tes3Mod:Mod_File_Format#Records"/>
/// <see cref="https://en.uesp.net/wiki/Tes4Mod:Mod_File_Format#Records"/>
/// <see cref="https://en.uesp.net/wiki/Tes5Mod:Mod_File_Format#Records"/>
public partial class Record {
    static readonly Dictionary<FormType, Func<FormType, Record>> Map = new() {
        { TES3, f => new TES3Record() },
        { TES4, f => new TES4Record() },
        // 0
        { LTEX, f => new LTEXRecord() },
        { STAT, f => new STATRecord() },
        { CELL, f => new CELLRecord() },
        { LAND, f => new LANDRecord() },
        // 1
        { DOOR, f => new DOORRecord() },
        { MISC, f => new MISCRecord() },
        { WEAP, f => new WEAPRecord() },
        { CONT, f => new CONTRecord() },
        { LIGH, f => new LIGHRecord() },
        { ARMO, f => new ARMORecord() },
        { CLOT, f => new CLOTRecord() },
        { REPA, f => new REPARecord() },
        { ACTI, f => new ACTIRecord() },
        { APPA, f => new APPARecord() },
        { LOCK, f => new LOCKRecord() },
        { PROB, f => new PROBRecord() },
        { INGR, f => new INGRRecord() },
        { BOOK, f => new BOOKRecord() },
        { ALCH, f => new ALCHRecord() },
        { CREA, f => f == TES3 ? new CREA3Record() : new CREA4Record() },
        { NPC_, f => f == TES3 ? new NPC_3Record() : new NPC_4Record() },
        // 2
        { GMST, f => new GMSTRecord() },
        { GLOB, f => new GLOBRecord() },
        { SOUN, f => new SOUNRecord() },
        { REGN, f => new REGNRecord() },
        // 3
        { CLAS, f => new CLASRecord() },
        { SPEL, f => new SPELRecord() },
        { BODY, f => new BODYRecord() },
        { PGRD, f => new PGRDRecord() },
        { INFO, f => f == TES3 ? new INFO3Record() : new INFO4Record() },
        { DIAL, f => new DIALRecord() },
        { SNDG, f => new SNDGRecord() },
        { ENCH, f => new ENCHRecord() },
        { SCPT, f => new SCPTRecord() },
        { SKIL, f => new SKILRecord() },
        { RACE, f => f == TES3 ? new RACE3Record() : f == TES4 ? new RACE4Record() : new RACE5Record() },
        { MGEF, f => new MGEFRecord() },
        { LEVI, f => new LEVIRecord() },
        { LEVC, f => new LEVCRecord() },
        { BSGN, f => new BSGNRecord() },
        { FACT, f => new FACTRecord() },
        { SSCR, f => new SSCRRecord() },
        // 4 - Oblivion
        { WRLD, f => new WRLDRecord() },
        { ACRE, f => new ACRERecord() },
        { ACHR, f => new ACHRRecord() },
        { REFR, f => new REFRRecord() },
        //
        { AMMO, f => new AMMORecord() },
        { ANIO, f => new ANIORecord() },
        { CLMT, f => new CLMTRecord() },
        { CSTY, f => new CSTYRecord() },
        { EFSH, f => new EFSHRecord() },
        { EYES, f => new EYESRecord() },
        { FLOR, f => new FLORRecord() },
        { FURN, f => new FURNRecord() },
        { GRAS, f => new GRASRecord() },
        { HAIR, f => new HAIRRecord() },
        { IDLE, f => new IDLERecord() },
        { KEYM, f => new KEYMRecord() },
        { LSCR, f => new LSCRRecord() },
        { LVLC, f => new LVLCRecord() },
        { LVLI, f => new LVLIRecord() },
        { LVSP, f => new LVSPRecord() },
        { PACK, f => new PACKRecord() },
        { QUST, f => new QUSTRecord() },
        { ROAD, f => new ROADRecord() },
        { SBSP, f => new SBSPRecord() },
        { SGST, f => new SGSTRecord() },
        { SLGM, f => new SLGMRecord() },
        { TREE, f => new TREERecord() },
        { WATR, f => new WATRRecord() },
        { WTHR, f => new WTHRRecord() },
        // 5 - Skyrim
        { AACT, f => new AACTRecord() },
        { ADDN, f => new ADDNRecord() },
        { ARMA, f => new ARMARecord() },
        { ARTO, f => new ARTORecord() },
        { ASPC, f => new ASPCRecord() },
        { ASTP, f => new ASTPRecord() },
        { AVIF, f => new AVIFRecord() },
        { DLBR, f => new DLBRRecord() },
        { DLVW, f => new DLVWRecord() },
        { SNDR, f => new SNDRRecord() },
        // Unknown
        { BPTD, f => new BPTDRecord() },
        { CAMS, f => new CAMSRecord() },
        { CLFM, f => new CLFMRecord() },
        { STDT, f => new STDTRecord() },
        { SUNP, f => new SUNPRecord() },
        { BOIM, f => new BOIMRecord() },
        { TERM, f => new TERMRecord() },
        { TMLM, f => new TMLMRecord() },
        { TRNS, f => new TRNSRecord() },
        { TXST, f => new TXSTRecord() },
        { BNDS, f => new BNDSRecord() },
        { DMGT, f => new DMGTRecord() },
        //
        { KYWD, f => new KYWDRecord() },
        { LCRT, f => new LCRTRecord() },
        { FLST, f => new FLSTRecord() },
        { OTFT, f => new OTFTRecord() },
        { HDPT, f => new HDPTRecord() },
        { MICN, f => new MICNRecord() },
    };

    static int CellsLoaded = 0;
    public static Record Factory(Reader r, FormType type) {
        Record record;
        if (type == CELL && CellsLoaded++ > 100) record = new Record(); // hack to limit cells loading
        else if (!Map.TryGetValue(type, out var z)) { Log.Info($"Unsupported record type: {type}"); record = new Record(); }
        else if (_factorySet != null && type != TES3 && type != TES4 && !_factorySet.Contains(type)) record = new Record();
        else { record = z(r.Format); record.Type = type; }
        record.Read(r);
        return record;
    }

    [Flags]
    public enum EsmFlags : uint {
        None_ = 0x00000000,                 // None
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
    public static readonly Record Empty = new();
    public override string ToString() => $"{Type}: {EDID}";
    public FormType Type;
    public uint DataSize;
    public EsmFlags Flags;
    public bool Compressed => (Flags & EsmFlags.Compressed) != 0;
    public uint Id;

    HashSet<FieldType> Ds = [];
    protected virtual HashSet<FieldType> DF3 => [];
    protected virtual HashSet<FieldType> DF4 => [];
    protected virtual HashSet<FieldType> DF5 => [];

    /// <summary>
    /// Reads an uninitialized subrecord to deserialize, or null to skip.
    /// </summary>
    /// <returns>Return an uninitialized subrecord to deserialize, or null to skip.</returns>
    public virtual object ReadField(Reader r, FieldType type, int dataSize) => Empty;

    /// <summary>
    /// Reads a record
    /// </summary>
    /// <param name="r"></param>
    public void Read(Reader r) {
        DataSize = r.ReadUInt32();
        if (r.Format == TES3) r.Skip(4); // unknown
        while (true) {
            Flags = (EsmFlags)r.ReadUInt32();
            if (r.Format == TES3) break;
            // tes4
            Id = r.ReadUInt32();
            r.Skip(4);
            if (r.Format == TES4) break;
            // tes5
            r.Skip(4);
            if (r.Format == TES5) break;
            break;
        }
        if (r.Tes4a) r.Skip(4);
    }

    /// <summary>
    /// Reads a records fields
    /// </summary>
    /// <param name="r"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void ReadFields(Reader r) {
        if (Compressed) {
            var lastSize = DataSize;
            DataSize = r.ReadUInt32();
            var data = r.DecompressZlib2((int)lastSize - 4, (int)DataSize);
            r = new Reader(new BinaryReader(new MemoryStream(data)), r.BinPath, r.Format, r.Tes4a);
        }
        long start = r.Tell(), end = start + DataSize;
        //Log.Info($"{Type}");
        var dfields = r.Format == TES3 ? DF3 : r.Format == TES4 ? DF4 : r.Format == TES5 ? DF5 : throw new Exception();
        while (!r.AtEnd(end)) {
            var fieldType = (FieldType)r.ReadUInt32();
            if (dfields != null && !dfields.Contains(fieldType) && Ds.Contains(fieldType)) throw new Exception($"d: {Type}Record.{fieldType}");
            Ds.Add(fieldType);
            //Log.Info($" - {fieldType}");
            var fieldDataSize = (int)(r.Format == TES3 ? r.ReadUInt32() : r.ReadUInt16());
            if (fieldType == FieldType.XXXX) {
                if (fieldDataSize != 4) throw new InvalidOperationException();
                fieldDataSize = (int)r.ReadUInt32();
                continue;
            }
            else if (fieldType == FieldType.OFST && Type == WRLD) { r.Seek(end); continue; }
            var tell = r.Tell();
            if (ReadField(r, fieldType, fieldDataSize) == Empty) { Log.Info($"Unsupported field type: {Type}Record:{fieldType}"); r.Skip(fieldDataSize); continue; }
            r.EnsureAtEnd(tell + fieldDataSize, $"Failed reading {Type}Record:{fieldType} field data at offset {tell} in {r.BinPath} of {r.Tell() - tell - fieldDataSize}");
        }
        //Log.Info($"END");
        r.EnsureAtEnd(end, $"Failed reading {Type} record data at offset {start} in {r.BinPath}");
        if (Compressed) r.Dispose();
    }
}

//[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct Ref<TRecord> where TRecord : Record {
    public override readonly string ToString() => $"{Type}:{Id}";
    public static (string, int) Struct = ("<I", 4);
    public readonly uint Id;
    public readonly string Type => typeof(TRecord).Name[..4];
    public Ref(Reader r, int dataSize) => Id = r.ReadUInt32();
    public Ref(uint id) => Id = id;
}
public readonly struct Ref2<TRecord> where TRecord : Record {
    public override readonly string ToString() => $"{Type}:{Id}x{Id2}";
    public static (string, int) Struct = ("<2I", 8);
    public readonly uint Id;
    public readonly uint Id2;
    public readonly string Type => typeof(TRecord).Name[..4];
    public Ref2(Reader r, int dataSize) { Id = r.ReadUInt32(); Id2 = r.ReadUInt32(); }
    public Ref2(uint id, uint id2) { Id = id; Id2 = id2; }
}
public readonly struct RefB<TRecord> where TRecord : Record {
    public override readonly string ToString() => $"{Type}:{Id}x{Value}";
    public static (string, int) Struct = ("<IB3x", 8);
    public readonly uint Id;
    public readonly byte Value;
    public readonly string Type => typeof(TRecord).Name[..4];
    public RefB(Reader r, int dataSize) { Id = r.ReadUInt32(); Value = r.ReadByte(); r.Skip(3); }
    public RefB(uint id, byte value) { Id = id; Value = value; }
}
public readonly struct RefS<TRecord> where TRecord : Record {
    public override readonly string ToString() => $"{Type}:{Name}";
    public readonly string Name;
    public readonly string Type => typeof(TRecord).Name[..4];
    public RefS(Reader r, int dataSize) => Name = r.ReadFUString(dataSize);
    public RefS(string name) => Name = name;
}
public readonly struct RefX<TRecord> where TRecord : Record {
    public override string ToString() => $"{Type}:{Name}{Id}";
    public readonly uint Id;
    public readonly string Name;
    public readonly string Type => typeof(TRecord).Name[..4];
    public RefX(Reader r, int dataSize) {
        if (dataSize == 4) { Id = r.ReadUInt32(); Name = null; }
        else { Id = 0; Name = r.ReadFUString(dataSize); }
    }
    public RefX(uint id, string name) { Id = id; Name = name; }
    public RefX(uint id) { Id = id; Name = null; }
    public RefX(string name) { Id = 0; Name = name; }
    public RefX<TRecord> SetName(string name) => new(Id, name);
}

#endregion

#region Record Group

public partial class RecordGroup {
    public enum GroupType : uint {
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
    public uint DataSize;
    public FormType Label;
    public GroupType Type;
    public long Position;
    public string Path;
    public List<Record> Records = [];
    public List<RecordGroup> Groups;
    public Dictionary<FormType, Record[]> RecordsByType;
    public Dictionary<FormType, RecordGroup[]> GroupsByLabel;
    public bool Preload => Label == 0 || Type == GroupType.Top || _factorySet.Contains(Label);

    public RecordGroup(Reader r, string path) {
        if (r == null) return;
        else if (r.Format == TES3) { DataSize = (uint)(r.BaseStream.Length - r.Tell()); Label = 0; Type = GroupType.Top; Position = r.Tell(); Path = path; return; }
        DataSize = (uint)(r.ReadUInt32() - (r.Format != TES4 ? 16 : 20));
        Label = (FormType)r.ReadUInt32();
        Type = (GroupType)r.ReadUInt32();
        r.Skip(4); // stamp + version
        if (r.Tes4a || r.Format != TES4) r.Skip(4); // unknown
        Position = r.Tell();
        Path = $"{path}{Label}/";
    }

    public static IEnumerable<RecordGroup> ReadAll(Reader r) {
        if (r.Format == TES3) { yield return new RecordGroup(r, ""); yield break; }
        while (!r.AtEnd()) {
            var type = (FormType)r.ReadUInt32();
            if (type != GRUP) throw new InvalidOperationException($"{type} not GRUP");
            yield return new RecordGroup(r, "");
        }
    }

    public void Read(Reader r, List<FileSource> files) {
        r.Seek(Position);
        var end = Position + DataSize;
        while (!r.AtEnd(end)) {
            var type = (FormType)r.ReadUInt32();
            if (type == GRUP) {
                Groups ??= [];
                var s = new RecordGroup(r, Path);
                if (s.Preload || true) s.Read(r, files);
                else r.Seek(r.Tell() + s.DataSize);
                Groups.Add(s);
                continue;
            }
            var record = Record.Factory(r, type);
            if (record.Type == 0) { r.Skip(record.DataSize); continue; }
            record.ReadFields(r);
            Records.Add(record);
        }
        RecordsByType = Records.GroupBy(s => s.Type).ToDictionary(s => s.Key, s => s.ToArray());
        GroupsByLabel = Groups?.GroupBy(s => s.Label).ToDictionary(s => s.Key, s => s.ToArray());
        // add items
        //$"{group.Label}"
        files.AddRange(RecordsByType.Select(s => new FileSource {
            Path = Path + Encoding.ASCII.GetString(BitConverter.GetBytes((uint)s.Key)),
            //Arc = new SubEsm(source, this, null, null),
            FileSize = 1,
            Flags = (int)s.Key,
            Tag = this
        }));
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
        //Load();
        var cellBlockId = (FormType)BitConverter.ToUInt32(cellBlockIdx);
        return GroupsByLabel.TryGetValue(cellBlockId, out var z) ? [.. z.Select(x => x.EnsureCell(cellId))] : null;
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
        //Load();
        CELLsById ??= [];
        LANDsById ??= cellId.Z >= 0 ? [] : null;
        //if (GroupsByLabel.TryGetValue(cellSubBlockId, out var cellSubBlocks)) {
        //    // find cell
        //    var cellSubBlock = cellSubBlocks.Single();
        //    cellSubBlock.Load(true);
        //    foreach (var cell in cellSubBlock.Records.Cast<CELLRecord>()) {
        //        cell.GridId = new Int3(cell.XCLC.Value.GridX, cell.XCLC.Value.GridY, !cell.IsInterior ? cellId.Z : -1);
        //        CELLsById.Add(cell.GridId, cell);
        //        // find children
        //        if (cellSubBlock.GroupsByLabel.TryGetValue(cell.Header.Id, out var cellChildren)) {
        //            var cellChild = cellChildren.Single();
        //            var cellTemporaryChildren = cellChild.Groups.Single(s => s.Headers.First().Type == GroupHeader.GroupType.CellTemporaryChildren);
        //            foreach (var land in cellTemporaryChildren.Records.Cast<LANDRecord>()) {
        //                land.GridId = new Int3(cell.XCLC.Value.GridX, cell.XCLC.Value.GridY, !cell.IsInterior ? cellId.Z : -1);
        //                LANDsById.Add(land.GridId, land);
        //            }
        //        }
        //    }
        //    EnsureCELLsByLabel.Add(cellSubBlockId);
        //    return this;
        //}
        return null;
    }
}

#endregion

#region Fields

public class Modl(Reader r, int dataSize) {
    [Flags] public enum ModdFlag { Head = 0x01, Torso = 0x02, RightHand = 0x04, LeftHand = 0x08 } // #Fallout
    public class Mods(Reader r, int dataSize) {
        public string X3dName = r.ReadL32UString();
        public Ref<TXSTRecord> NewTexture = new(r.ReadUInt32());
        public uint X3dIndex = r.ReadUInt32();
    }
    public override string ToString() => $"{Value}";
    public string Value = r.ReadFUString(dataSize);
    public float Bound;
    public byte[] Textures; // Texture Files Hashes
    public Mods[] AltTextures; // Alternate Textures
    public ModdFlag FaceGenModelFlags; // FaceGen Model Flags
    public string Icon; // Icon
    public object MODB(Reader r, int dataSize) => Bound = r.ReadSingle();
    public object MODT(Reader r, int dataSize) => Textures = r.ReadBytes(dataSize); // Texture File Hashes
    public object MODS(Reader r, int dataSize) => AltTextures = r.ReadL32FArray(z => new Mods(r, dataSize)); // Alternate Textures
    public object MODD(Reader r, int dataSize) => FaceGenModelFlags = (ModdFlag)(r.ReadByte()); // FaceGen Model Flags
    public object ICON(Reader r, int dataSize) => Icon = r.ReadFUString(dataSize); // Icon
}

// https://en.uesp.net/wiki/Skyrim_Mod:Mod_File_Format/Model_Textures_Field
/// Same as Modl.MODT?
//public class Modt {
//    public uint Count;
//    public Modt(Reader r, int dataSize) {
//        Count = r.ReadUInt32();
//        var Unknown4Count = Count >= 1 ? r.ReadUInt32() : 0;
//        var Unknown5Count = Count >= 2 ? r.ReadUInt32() : 0;
//        var Unknown3 = Unknown4Count > 0 ? r.ReadPArray<uint>("I", Count - 2) : null;
//        var Unknown4 = Unknown5Count > 0 ? r.ReadPArray<uint>("I", Unknown5Count) : null;
//    }
//}

public interface IHaveMODL {
    Modl MODL { get; }
}

public struct Datv { public override readonly string ToString() => "Datv"; public bool B; public int I; public float F; public string S; }
public struct Cnto<TRecord>(Reader r, int dataSize) where TRecord : Record { public override readonly string ToString() => $"{Item}:{Count}"; public Ref<TRecord> Item = new(r, dataSize); public uint Count = r.ReadUInt32(); }
public struct CntoX<TRecord> {
    public override readonly string ToString() => $"{Item}";
    public RefX<Record> Item; // The ID of the item
    public uint Count; // Number of the item
    public CntoX(Reader r, int dataSize) {
        if (r.Format == TES3) { Count = r.ReadUInt32(); Item = new RefX<Record>(0, r.ReadFAString(32)); }
        else { Item = new RefX<Record>(r.ReadUInt32(), null); Count = r.ReadUInt32(); }
    }
}
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Position {
    public Vector3 Translate;
    public Vector3 Rotation;
}

partial class Record {
    public string EDID; // Editor ID
}

#endregion

#region Extensions

public static class Extensions {
    public static TResult Then<T, TResult>(this Record s, T value, Func<T, TResult> func) => func(value);
    public static T AddX<T>(this IList<T> s, T value) { s.Add(value); return value; }
    public static IEnumerable<T> AddRangeX<T>(this List<T> s, IEnumerable<T> value) { s.AddRange(value); return value; }
    public static long ReadINTV(this Reader r, int length)
        => length switch {
            1 => r.ReadByte(),
            2 => r.ReadInt16(),
            4 => r.ReadInt32(),
            8 => r.ReadInt64(),
            _ => throw new NotImplementedException($"{length})"),
        };
    public static Datv ReadDATV(this Reader r, int length, char type)
        => type switch {
            'b' => new Datv { B = r.ReadInt32() != 0 },
            'i' => new Datv { I = r.ReadInt32() },
            'f' => new Datv { F = r.ReadSingle() },
            's' => new Datv { S = r.ReadFUString(length) },
            _ => throw new InvalidOperationException($"{type}"),
        };

}

#endregion

#region Records

//partial class Record { static readonly HashSet<FormType> _factorySet = [FormType.ACTI, FormType.ALCH, FormType.APPA, FormType.ARMO, FormType.BODY, FormType.BSGN, FormType.CELL, FormType.CLAS, FormType.CLOT, FormType.CONT, FormType.CREA]; }
//partial class Record { static readonly HashSet<FormType> _factorySet = [FormType.DIAL, FormType.INFO, FormType.DOOR, FormType.ENCH, FormType.FACT, FormType.GLOB]; }
partial class Record { static readonly HashSet<FormType> _factorySet = null; }
partial class RecordGroup { static readonly HashSet<FormType> _factorySet = [NPC_]; }

/// <summary>
/// AACT.Action - 00500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/AACT"/>
/// <see cref="https://tes5edit.github.io/fopdoc/Fallout4/Records/AACT.html"/>
public class AACTRecord : Record {
    public ByteColor4 CNAM; // RGB Color

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.CNAM => CNAM = r.ReadS<ByteColor4>(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// ACRE.Placed creature - 04000
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/ACRE"/>
/// <see cref="https://tes5edit.github.io/fopdoc/Fallout3/Records/ACHR.html"/>
public class ACRERecord : Record {
    public Ref<Record> NAME; // Base
    public REFRRecord.Data DATA; // Position/Rotation
    public REFRRecord.Xrgd[] XRGDs; // Ragdoll Data (optional)
    public REFRRecord.Xesp? XESP; // Enable Parent (optional)
    public List<CELLRecord.Xown> XOWNs; // Ownership (optional)
    public float XSCL; // Scale (optional)

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.NAME => NAME = new Ref<Record>(r, dataSize),
        FieldType.DATA => DATA = r.ReadS<REFRRecord.Data>(dataSize),
        FieldType.XRGD => XRGDs = r.ReadSArray<REFRRecord.Xrgd>(dataSize / 28),
        FieldType.XESP => XESP = new REFRRecord.Xesp(r, dataSize),
        FieldType.XOWN => (XOWNs ??= []).AddX(new CELLRecord.Xown { XOWN = new RefX<Record>(r, dataSize) }),
        FieldType.XGLB => XOWNs.Last().XGLB = new RefX<Record>(r, dataSize),
        FieldType.XRNK => XOWNs.Last().XRNK = r.ReadInt32(),
        FieldType.XSCL => XSCL = r.ReadSingle(),
        _ => Empty,
    };
}

/// <summary>
/// ACHR.Actor Reference - 04050
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/ACHR"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/ACHR"/>
/// <see cref="https://tes5edit.github.io/fopdoc/Fallout3/Records/ACHR.html"/>
public class ACHRRecord : Record {
    public Ref<Record> NAME; // Base
    public REFRRecord.Data DATA; // Position/Rotation
    public REFRRecord.Xrgd[] XRGDs; // Ragdoll Data (optional)
    public REFRRecord.Xesp? XESP; // Enable Parent (optional)
    public RefX<CELLRecord>? XPCI; // Unused (optional)
    public byte[] XLOD; // Distant LOD Data (optional)
    public Ref<REFRRecord>? XMRC; // Merchant Container (optional)
    public Ref<ACRERecord>? XHRS; // Horse (optional)
    public float? XSCL; // Scale (optional)

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.NAME => NAME = new Ref<Record>(r, dataSize),
        FieldType.DATA => DATA = r.ReadS<REFRRecord.Data>(dataSize),
        FieldType.XRGD => XRGDs = r.ReadSArray<REFRRecord.Xrgd>(dataSize / 28),
        FieldType.XESP => XESP = new REFRRecord.Xesp(r, dataSize),
        FieldType.XPCI => XPCI = new RefX<CELLRecord>(r, dataSize),
        FieldType.FULL => XPCI.Value.SetName(r.ReadFAString(dataSize)),
        FieldType.XLOD => XLOD = r.ReadBytes(dataSize),
        FieldType.XMRC => XMRC = new Ref<REFRRecord>(r, dataSize),
        FieldType.XHRS => XHRS = new Ref<ACRERecord>(r, dataSize),
        FieldType.XSCL => XSCL = r.ReadSingle(),
        _ => Empty,
    };
}

/// <summary>
/// ACTI.Activator - 34500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/ACTI">
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/ACTI"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/ACTI"/>
/// <see cref="https://tes5edit.github.io/fopdoc/Fallout3/Records/ACTI.html"/>
public class ACTIRecord : Record, IHaveMODL {
    public string FULL; // Item Name
    public Modl MODL { get; set; } // Model Name
    public RefX<SCPTRecord>? SCRI; // Script (Optional)
    // TES4
    public RefX<SOUNRecord>? SNAM; // Sound (Optional)

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadFUString(dataSize),
        FieldType.FULL or FieldType.FNAM => FULL = r.ReadFUString(dataSize),
        FieldType.MODL => MODL = new Modl(r, dataSize),
        FieldType.MODB => MODL.MODB(r, dataSize),
        FieldType.MODT => MODL.MODT(r, dataSize),
        FieldType.SCRI => SCRI = new RefX<SCPTRecord>(r, dataSize),
        // TES4
        FieldType.SNAM => SNAM = new RefX<SOUNRecord>(r, dataSize),
        _ => Empty,
    };
}

/// <summary>
/// ADDN-Addon Node - 00500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/ADDN"/>
/// <see cref="https://tes5edit.github.io/fopdoc/Fallout3/Records/ADDN.html"/>
public class ADDNRecord : Record {
    public ByteColor4 CNAM; // RGB Color

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.CNAM => CNAM = r.ReadS<ByteColor4>(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// ALCH.Potion - 345S0
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/ALCH">
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/ALCH"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/ALCH"/>
/// <see cref="https://starfieldwiki.net/wiki/Starfield_Mod:Mod_File_Format/ALCH">
/// <see cref="https://tes5edit.github.io/fopdoc/Fallout3/Records/ALCH.html"/>
public class ALCHRecord : Record, IHaveMODL {
    public class Data {
        [Flags] public enum Flag : byte { NoAutoCalculate = 0x01, FoodItem = 0x02 }
        public float Weight;
        public int Value;
        public Flag Flags;
        public Data(Reader r, int dataSize) {
            Weight = r.ReadSingle();
            if (r.Format == TES3) {
                Value = r.ReadInt32();
                Flags = (Flag)r.ReadInt32();
            }
        }
        public object ENIT(Reader r, int dataSize) {
            Value = r.ReadInt32();
            Flags = (Flag)r.ReadByte();
            r.Skip(3); // Unknown
            return true;
        }
    }
    // TES3
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Enam {
        public enum Range_ : uint { Self = 0, Touch, Target }
        public static (string, int) Struct = ("<h2B5I", 24);
        public short EffectId;
        public byte SkillId; // for skill related effects, -1/0 otherwise
        public byte AttributeId; // for attribute related effects, -1/0 otherwise
        public Range_ Range;
        public uint Area;
        public uint Duration;
        public uint MagnitudeMin;
        public uint MagnitudeMax;
    }

    public Modl MODL { get; set; } // Model
    public string FULL; // Item Name
    public Data DATA; // Alchemy Data
    public List<Enam> ENAMs = []; // Enchantment
    public RefX<SCPTRecord>? SCRI; // Script (optional)
    // TES4
    public List<ENCHRecord.Efit> EFITs = []; // Effect Data
    public List<ENCHRecord.Scit> SCITs = []; // Script Effect Data

    protected override HashSet<FieldType> DF3 => [FieldType.ENAM];
    protected override HashSet<FieldType> DF4 => [FieldType.FULL, FieldType.EFID, FieldType.EFIT];
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadFUString(dataSize),
        FieldType.MODL => MODL = new Modl(r, dataSize),
        FieldType.MODB => MODL.MODB(r, dataSize),
        FieldType.MODT => MODL.MODT(r, dataSize),
        FieldType.ICON or FieldType.TEXT => MODL.ICON(r, dataSize),
        FieldType.FULL => SCITs.Count == 0 ? FULL = r.ReadFUString(dataSize) : SCITs.Last().FULL(r, dataSize),
        FieldType.FNAM => FULL = r.ReadFUString(dataSize),
        FieldType.DATA or FieldType.ALDT => DATA = new Data(r, dataSize),
        FieldType.ENAM => ENAMs.AddX(r.ReadS<Enam>(dataSize)),
        FieldType.SCRI => SCRI = new RefX<SCPTRecord>(r, dataSize),
        // TES4
        FieldType.ENIT => DATA.ENIT(r, dataSize),
        FieldType.EFID => r.Skip(dataSize), // ignored
        FieldType.EFIT => EFITs.AddX(new ENCHRecord.Efit(r, dataSize)),
        FieldType.SCIT => SCITs.AddX(new ENCHRecord.Scit(r, dataSize)),
        _ => Empty,
    };
}

/// <see cref="https://tes5edit.github.io/fopdoc/Fallout3/Records/ADDN.html"/>

/// <summary>
/// AMMO.Ammo - 045S0
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/AMMO"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/AMMO"/>
/// <see cref="https://starfieldwiki.net/wiki/Starfield_Mod:Mod_File_Format/AMMO">
/// <see cref="https://tes5edit.github.io/fopdoc/Fallout3/Records/AMMO.html"/>
public class AMMORecord : Record, IHaveMODL {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Data {
        [Flags] public enum Flag : uint { IgnoresNormalWeaponResistance = 0x1 }
        public static (string, int) Struct = ("<f2IfH", 18);
        public float Speed;
        public Flag Flags;
        public uint Value;
        public float Weight;
        public ushort Damage;
    }

    public Modl MODL { get; set; } // Model
    public string FULL; // Item Name
    public Ref<ENCHRecord>? ENAM; // Enchantment ID (optional)
    public short? ANAM; // Enchantment Points (optional)
    public Data DATA; // Ammo Data

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.MODL => MODL = new Modl(r, dataSize),
        FieldType.MODB => MODL.MODB(r, dataSize),
        FieldType.MODT => MODL.MODT(r, dataSize),
        FieldType.ICON => MODL.ICON(r, dataSize),
        FieldType.FULL => FULL = r.ReadFUString(dataSize),
        FieldType.ENAM => ENAM = new Ref<ENCHRecord>(r, dataSize),
        FieldType.ANAM => ANAM = r.ReadInt16(),
        FieldType.DATA => DATA = r.ReadS<Data>(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// ANIO.Animated Object - 04500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/ANIO"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/ANIO"/>
/// <see cref="https://tes5edit.github.io/fopdoc/Fallout3/Records/ANIO.html"/>
public class ANIORecord : Record, IHaveMODL {
    public Modl MODL { get; set; } // Model
    public Ref<IDLERecord>? DATA; // IDLE Animation

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.MODL => MODL = new Modl(r, dataSize),
        FieldType.MODB => MODL.MODB(r, dataSize),
        FieldType.DATA => DATA = new Ref<IDLERecord>(r, dataSize),
        _ => Empty,
    };
}

/// <summary>
/// APPA.Alchem Apparatus - 34500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/APPA">
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/APPA"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/APPA"/>
public class APPARecord : Record, IHaveMODL {
    public struct Data {
        public enum Type_ : byte { MortarAndPestle = 0, Albemic, Calcinator, Retort }
        public Type_ Type;
        public int Value;
        public float Weight;
        public float Quality;
        public Data(Reader r, int dataSize) {
            if (r.Format == TES3) {
                Type = (Type_)r.ReadInt32();
                Quality = r.ReadSingle();
                Weight = r.ReadSingle();
                Value = r.ReadInt32();
                return;
            }
            Type = (Type_)r.ReadByte();
            Value = r.ReadInt32();
            Weight = r.ReadSingle();
            Quality = r.ReadSingle();
        }
    }

    public Modl MODL { get; set; } // Model
    public string FULL; // Item Name
    public Data DATA; // Alchemy Data
    public RefX<SCPTRecord>? SCRI; // Script Name

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadFUString(dataSize),
        FieldType.MODL => MODL = new Modl(r, dataSize),
        FieldType.MODB => MODL.MODB(r, dataSize),
        FieldType.MODT => MODL.MODT(r, dataSize),
        FieldType.ICON or FieldType.ITEX => MODL.ICON(r, dataSize),
        FieldType.FULL or FieldType.FNAM => FULL = r.ReadFUString(dataSize),
        FieldType.DATA or FieldType.AADT => DATA = new Data(r, dataSize),
        FieldType.SCRI => SCRI = new RefX<SCPTRecord>(r, dataSize),
        _ => Empty,
    };
}

/// <summary>
/// ARMA.Armature (Model) - 00500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/ARMA"/>
public class ARMARecord : Record {
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// ARMO.Armor - 345S0
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/ARMO">
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/ARMO"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/ARMO"/>
/// <see cref="https://starfieldwiki.net/wiki/Starfield_Mod:Mod_File_Format/ARMO">
public class ARMORecord : Record, IHaveMODL {
    public struct Data {
        public enum ARMOType { Helmet = 0, Cuirass, L_Pauldron, R_Pauldron, Greaves, Boots, L_Gauntlet, R_Gauntlet, Shield, L_Bracer, R_Bracer, }
        public short Armour;
        public int Value;
        public int Health;
        public float Weight;
        // TES3
        public int Type;
        public int EnchantPts;
        public Data(Reader r, int dataSize) {
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

    // TES5
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct Bodt {
        [Flags]
        public enum NodeFlag : uint {
            Head = 0x00000001,
            Hair = 0x00000002,
            Body = 0x00000004,
            Hands = 0x00000008,
            Forearms = 0x00000010,
            Amulet = 0x00000020,
            Ring = 0x00000040,
            Feet = 0x00000080,
            Calves = 0x00000100,
            Shield = 0x00000200,
            Tail = 0x00000400,
            LongHair = 0x00000800,
            Circlet = 0x00001000,
            Ears = 0x00002000,
            BodyAddOn3 = 0x00004000,
            BodyAddOn4 = 0x00008000,
            BodyAddOn5 = 0x00010000,
            BodyAddOn6 = 0x00020000,
            BodyAddOn7 = 0x00040000,
            BodyAddOn8 = 0x00080000,
            DecapitateHead = 0x00100000,
            Decapitate = 0x00200000,
            BodyAddOn9 = 0x00400000,
            BodyAddOn10 = 0x00800000,
            BodyAddOn11 = 0x01000000,
            BodyAddOn12 = 0x02000000,
            BodyAddOn13 = 0x04000000,
            BodyAddOn14 = 0x08000000,
            BodyAddOn15 = 0x10000000,
            BodyAddOn16 = 0x20000000,
            BodyAddOn17 = 0x40000000,
            FX01 = 0x80000000,
        }
        [Flags]
        public enum Flag : byte { ModulatesVoice = 0x00000001, NonPlayable = 0x00000010 } // ARMA Only
        public enum SkillType : uint { LightArmor = 0, HeavyArmor, None_ }
        public static Dictionary<int, string> Struct = new() { [8] = "<IB3s", [12] = "<IB3sI" };
        public NodeFlag NodeFlags; // Body part node flags
        public Flag Flags;
        public fixed byte JunkData[3];
        public SkillType Skill;
    }

    public Modl MODL { get; set; } // Male Biped Model
    public string FULL; // Item Name
    public Data DATA; // Armour Data
    public RefX<SCPTRecord>? SCRI; // Script Name (optional)
    public RefX<ENCHRecord>? ENAM; // Enchantment FormId (optional)
    // TES3
    public List<CLOTRecord.Indx> INDXs = []; // Body Part Index
    // TES4
    public uint BMDT; // Flags
    public Modl MOD2; // Male World Model (optional)
    public Modl MOD3; // Female Biped Model (optional)
    public Modl MOD4; // Female World Model (optional)
    public short? ANAM; // Enchantment Points (optional)

    protected override HashSet<FieldType> DF3 => [FieldType.INDX, FieldType.BNAM, FieldType.CNAM];
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadFUString(dataSize),
        FieldType.MODL => MODL = new Modl(r, dataSize),
        FieldType.MODB => MODL.MODB(r, dataSize),
        FieldType.MODT => MODL.MODT(r, dataSize),
        FieldType.ICON or FieldType.ITEX => MODL.ICON(r, dataSize),
        FieldType.FULL or FieldType.FNAM => FULL = r.ReadFUString(dataSize),
        FieldType.DATA or FieldType.AODT => DATA = new Data(r, dataSize),
        FieldType.SCRI => SCRI = new RefX<SCPTRecord>(r, dataSize),
        FieldType.ENAM => ENAM = new RefX<ENCHRecord>(r, dataSize),
        // TES3
        FieldType.INDX => INDXs.AddX(new CLOTRecord.Indx { INDX = r.ReadINTV(dataSize) }),
        FieldType.BNAM => INDXs.Last().BNAM = r.ReadFUString(dataSize),
        FieldType.CNAM => INDXs.Last().CNAM = r.ReadFUString(dataSize),
        // TES4
        FieldType.BMDT => BMDT = r.ReadUInt32(),
        FieldType.MOD2 => MOD2 = new Modl(r, dataSize),
        FieldType.MO2B => MOD2.MODB(r, dataSize),
        FieldType.MO2T => MOD2.MODT(r, dataSize),
        FieldType.ICO2 => MOD2.ICON(r, dataSize),
        FieldType.MOD3 => MOD3 = new Modl(r, dataSize),
        FieldType.MO3B => MOD3.MODB(r, dataSize),
        FieldType.MO3T => MOD3.MODT(r, dataSize),
        FieldType.MOD4 => MOD4 = new Modl(r, dataSize),
        FieldType.MO4B => MOD4.MODB(r, dataSize),
        FieldType.MO4T => MOD4.MODT(r, dataSize),
        FieldType.ANAM => ANAM = r.ReadInt16(),
        _ => Empty,
    };
}

/// <summary>
/// ARTO.Art Object - 00500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/ARTO"/>
/// <see cref="https://starfieldwiki.net/wiki/Starfield_Mod:Mod_File_Format/ARTO">
public class ARTORecord : Record {
    public ByteColor4 CNAM; // RGB Color

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.CNAM => CNAM = r.ReadS<ByteColor4>(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// ASPC.Acoustic Space - 00500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/ASPC"/>
public class ASPCRecord : Record {
    public ByteColor4 CNAM; // RGB Color

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.CNAM => CNAM = r.ReadS<ByteColor4>(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// ASTP.Association Type - 00500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/ASTP"/>
public class ASTPRecord : Record {
    public ByteColor4 CNAM; // RGB Color

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.CNAM => CNAM = r.ReadS<ByteColor4>(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// AVIF.Actor Values_Perk Tree Graphics - 005S0
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/AVIF"/>
/// <see cref="https://starfieldwiki.net/wiki/Starfield_Mod:Mod_File_Format/AVIF">
public class AVIFRecord : Record {
    public ByteColor4 CNAM; // RGB Color

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.CNAM => CNAM = r.ReadS<ByteColor4>(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// BNDS.Bendable Spline - 04000 #F4
/// </summary>
/// <see cref="https://tes5edit.github.io/fopdoc/Fallout4/Records/BNDS.html"/>
public class BNDSRecord : Record {
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// BODY.Body - 30000
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/BODY"/>
public class BODYRecord : Record, IHaveMODL {
    public enum Part : byte { Head, Hair, Neck, Chest, Groin, Hand, Wrist, Forearm, Upperarm, Foot, Ankle, Knee, Upperleg, Clavicle, Tail }
    [Flags] public enum Flag : byte { Female = 1, Playable = 2 }
    public enum PartType : byte { Skin, Clothing, Armor }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Bydt {
        public static (string, int) Struct = ("<4B", 4);
        public Part Part;
        public byte Vampire;
        public Flag Flags;
        public PartType PartType;
    }

    public Modl MODL { get; set; } // NIF Model
    public string FNAM; // Body Name
    public Bydt BYDT;

    public override object ReadField(Reader r, FieldType type, int dataSize) => r.Format == TES3
        ? type switch {
            FieldType.NAME => EDID = r.ReadFUString(dataSize),
            FieldType.MODL => MODL = new Modl(r, dataSize),
            FieldType.FNAM => FNAM = r.ReadFUString(dataSize),
            FieldType.BYDT => BYDT = r.ReadS<Bydt>(dataSize),
            _ => Empty,
        }
        : Empty;
}

/// <summary>
/// BOIM.Biome - 000S0
/// </summary>
/// <see cref="https://starfieldwiki.net/wiki/Starfield_Mod:Mod_File_Format/BOIM">
public class BOIMRecord : Record {
    public string FULL; // Item Name
    public string SNAM; // Sub Name 

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.SNAM => SNAM = r.ReadFUString(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// BOOK.Book - 345S0
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/BOOK">
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/BOOK"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/BOOK"/>
/// <see cref="https://starfieldwiki.net/wiki/Starfield_Mod:Mod_File_Format/BOOK">
public class BOOKRecord : Record, IHaveMODL {
    [Flags] public enum Flag : byte { Scroll = 0x01, CantBeTaken = 0x02 }
    public struct Data {
        public Flag Flags;
        public ActorValue Teaches; // SkillId - (-1 is no skill)
        public int Value;
        public float Weight;
        // TES3
        public int EnchantPts;
        public Data(Reader r, int dataSize) {
            if (r.Format == TES3) {
                Weight = r.ReadSingle();
                Value = r.ReadInt32();
                Flags = (Flag)r.ReadUInt32();
                Teaches = (ActorValue)r.ReadInt32();
                EnchantPts = r.ReadInt32();
                return;
            }
            Flags = (Flag)r.ReadByte();
            Teaches = (ActorValue)r.ReadSByte();
            Value = r.ReadInt32();
            Weight = r.ReadSingle();
        }
    }

    public Modl MODL { get; set; } // Model (optional)
    public string FULL; // Item Name
    public Data DATA; // Book Data
    public string DESC; // Book Text
    public RefX<SCPTRecord>? SCRI; // Script Name (optional)
    public RefX<ENCHRecord>? ENAM; // Enchantment FormId (optional)
    // TES4
    public short? ANAM; // Enchantment points (optional)

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadFUString(dataSize),
        FieldType.MODL => MODL = new Modl(r, dataSize),
        FieldType.MODB => MODL.MODB(r, dataSize),
        FieldType.MODT => MODL.MODT(r, dataSize),
        FieldType.ICON or FieldType.ITEX => MODL.ICON(r, dataSize),
        FieldType.FULL or FieldType.FNAM => FULL = r.ReadFUString(dataSize),
        FieldType.DATA or FieldType.BKDT => DATA = new Data(r, dataSize),
        FieldType.SCRI => SCRI = new RefX<SCPTRecord>(r, dataSize),
        FieldType.DESC or FieldType.TEXT => DESC = new string(r.ReadFUString(dataSize).Replace('\ufffd', '\x1')),
        FieldType.ENAM => ENAM = new RefX<ENCHRecord>(r, dataSize),
        // TES4
        FieldType.ANAM => ANAM = r.ReadInt16(),
        _ => Empty,
    };
}

/// <summary>
/// BPTD.Body Part Data - 00500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/BPTD"/>
public class BPTDRecord : Record {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Bpnd {
        [Flags]
        public enum Flag : byte {
            Severable = 1 << 0,
            IKData = 1 << 1,
            IKData_BipedData = 1 << 2,
            Explodable = 1 << 3,
            IKData_IsHead = 1 << 4,
            IKData_Headtracking = 1 << 5,
            ToHitChance_Absolute = 1 << 6,
        }
        public enum PartType_ : byte { Torso, Head, Eye, LookAt, FlyGrab, Saddle }
        public static (string, int) Struct = ("<f3Bb2BH2I2fi2If6f2I2BHf", 84);
        public float DamageMult;
        public Flag Flags;
        public PartType_ PartType;
        public byte HealthPercent;
        public ActorValue ActorValue;
        public byte ToHitChance;
        public byte Explodable_ExplosionChance;
        public ushort Explodable_DebrisCount;
        public Ref<Record> Explodable_Debris; //TODO DEBRRecord
        public Ref<Record> Explodable_Explosion; //TODO EXPLRecord
        public float TrackingMaxAngle;
        public float Explodable_DebrisScale;
        public int Severable_DebrisCount;
        public Ref<Record> Severable_Debris; //TODO DEBRRecord
        public Ref<Record> Severable_Explosion; //TODO EXPLRecord
        public float Severable_DebrisScale;
        public Position GoreEffectsPositioning;
        public Ref<Record> Severable_ImpactDataSet; //TODO IPDSRecord
        public Ref<Record> Explodable_ImpactDataSet; //TODO IPDSRecord
        public byte Severable_DecalCount;
        public byte Explodable_DecalCount;
        public ushort Unknown;
        public float LimbReplacementScale;
    }

    public Modl MODL { get; set; } // Model
    public string BPTN; // Body part name
    public string BPNN; // Body part node name
    public string BPNT; // Body part node title
    public string BPNI; // Body part node info
    public Bpnd BPND; // Body part node data
    public string NAM1; // Limb Replacement Model
    public string NAM4; // Gore Effects
    public string NAM5; // Hashes
    public Ref<Record> RAGA; // Hashes //TODO RGDLRecord

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.MODL => MODL = new Modl(r, dataSize),
        FieldType.BPTN => BPTN = r.ReadFUString(dataSize),
        FieldType.BPNN => BPNN = r.ReadFUString(dataSize),
        FieldType.BPNT => BPNT = r.ReadFUString(dataSize),
        FieldType.BPNI => BPNI = r.ReadFUString(dataSize),
        FieldType.BPND => BPND = r.ReadS<Bpnd>(dataSize),
        FieldType.NAM1 => NAM1 = r.ReadFUString(dataSize),
        FieldType.NAM4 => NAM4 = r.ReadFUString(dataSize),
        FieldType.NAM5 => NAM5 = r.ReadFUString(dataSize),
        FieldType.RAGA => RAGA = new Ref<Record>(r, dataSize),
        _ => Empty,
    };
}

/// <summary>
/// BSGN.Birthsign - 34000
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/BSGN">
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/BSGN"/>
public class BSGNRecord : Record {
    public string FULL; // Sign Name
    public string ICON; // Texture
    public string DESC; // Description
    //public List<string> NPCSs; // TES3: Spell/ability
    public List<RefX<Record>> SPLOs; // TES4: (points to a SPEL or LVSP record)

    protected override HashSet<FieldType> DF3 => [FieldType.NPCS];
    protected override HashSet<FieldType> DF4 => [FieldType.SPLO];
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadFUString(dataSize),
        FieldType.FULL or FieldType.FNAM => FULL = r.ReadFUString(dataSize),
        FieldType.ICON or FieldType.TNAM => ICON = r.ReadFUString(dataSize),
        FieldType.DESC => DESC = r.ReadFUString(dataSize),
        FieldType.SPLO or FieldType.NPCS => (SPLOs ??= []).AddX(new RefX<Record>(r, dataSize)),
        //FieldType.NPCS => (NPCSs ??= []).AddX(r.ReadFUString(dataSize)),
        _ => Empty,
    };
}

/// <summary>
/// CAMS.Camera Shot - 00500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/CAMS"/>
public class CAMSRecord : Record {
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// CELL.Cell - 3450
/// </summary>
/// <see>https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/CELL</see>
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/CELL"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/CELL"/>
/// <see cref="https://starfieldwiki.net/wiki/Starfield_Mod:Mod_File_Format/CELL">
public class CELLRecord : Record {
    [Flags]
    public enum Flag : ushort {
        Interior = 0x0001,
        HasWater = 0x0002,
        InvertFastTravel = 0x0004, // IllegalToSleepHere
        BehaveLikeExterior = 0x0008, // BehaveLikeExterior (Tribunal), Force hide land (exterior cell) / Oblivion interior (interior cell)
        Unknown1 = 0x0010,
        PublicArea = 0x0020, // Public place
        HandChanged = 0x0040,
        ShowSky = 0x0080, // Behave like exterior
        UseSkyLighting = 0x0100,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Xclc {
        public override readonly string ToString() => $"{GridX}z{GridY}";
        public static Dictionary<int, string> Struct = new() { [8] = "<2i", [12] = "<2iI" };
        public int GridX;
        public int GridY;
        public uint Flags;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Xcll {
        public static Dictionary<int, string> Struct = new() { [16] = "<12cf", [36] = "<12c2f2i2f", [40] = "<12c2f2i3f" };
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

    public class Xown {
        public RefX<Record> XOWN;
        public int XRNK; // Faction rank
        public RefX<Record> XGLB;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Xyza {
        public static (string, int) Struct = ("<3f3f", 24);
        public Float3 Position;
        public Float3 EulerAngles;
    }

    public class Ref_ {
        public override string ToString() => $"CREF: {EDID}";
        public uint? FRMR; // Object Index (starts at 1)
        // This is used to uniquely identify objects in the cell. For new files the index starts at 1 and is incremented for each new object added. For modified objects the index is kept the same.
        public string EDID; // Object ID
        public float? XSCL; // Scale (Static)
        public int? DELE; // Indicates that the reference is deleted.
        public Xyza? DODT; // XYZ Pos, XYZ Rotation of exit
        public string DNAM; // Door exit name (Door objects)
        public float? FLTV; // Follows the DNAM optionally, lock level
        public string KNAM; // Door key
        public string TNAM; // Trap name
        public byte? UNAM; // Reference Blocked (only occurs once in MORROWIND.ESM)
        public string ANAM; // Owner ID string
        public string BNAM; // Global variable/rank ID
        public int? INTV; // Number of uses, occurs even for objects that don't use it
        public uint? NAM9; // Unknown
        public string XSOL; // Soul Extra Data (ID string of creature)
        public Xyza DATA; // Ref Position Data
        // TES?
        public string CNAM; // Unknown
        public uint? NAM0; // Unknown
        public int? XCHG; // Unknown
        public int? INDX; // Unknown
    }

    public string FULL; // Full Name / TES3:RGNN - Region name
    public ushort DATA; // Flags
    public Xclc? XCLC; // Cell Data (only used for exterior cells)
    public Xcll? XCLL; // Lighting (only used for interior cells)
    public float? XCLW; // Water Height
    // TES3
    public uint? NAM0; // Number of objects in cell in current file (Optional)
    public long INTV; // Unknown
    public ByteColor4? NAM5; // Map Color (COLORREF)
    // TES4
    public Ref<REGNRecord>[] XCLRs; // Regions
    public byte? XCMT; // Music (optional)
    public RefX<CLMTRecord>? XCCM; // Climate
    public RefX<WATRRecord>? XCWT; // Water
    public List<Xown> XOWNs = []; // Ownership
    // Referenced Object Data Grouping
    public List<Ref_> RefObjs = [];
    bool _inFRMR = false;
    Ref_ _lastRef;
    // Grid
    public bool IsInterior; // => (DATA & 0x01) == 0x01;
    public Int3 GridId; // => new Int3(XCLC.Value.GridX, XCLC.Value.GridY, !IsInterior ? 0 : -1);
    public Color? AmbientLight; // => XCLL?.AmbientColor.AsColor;

    public void Complete(Reader r) {
        IsInterior = (DATA & 0x01) == 0x01;
        GridId = r.Format == TES3 ? new Int3(XCLC.Value.GridX, XCLC.Value.GridY, IsInterior ? -1 : 0) : default;
        AmbientLight = XCLL?.AmbientColor.AsColor;
    }

    protected override HashSet<FieldType> DF3 => null;
    protected override HashSet<FieldType> DF4 => null;
    public override object ReadField(Reader r, FieldType type, int dataSize) {
        //Console.WriteLine($"   {type}");
        if (!_inFRMR && type == FieldType.FRMR) _inFRMR = true;
        if (!_inFRMR)
            return type switch {
                FieldType.EDID or FieldType.NAME => EDID = r.ReadFUString(dataSize),
                FieldType.FULL or FieldType.RGNN => FULL = r.ReadFUString(dataSize),
                FieldType.DATA => (DATA = (ushort)r.ReadINTV(r.Format == TES3 ? 4 : dataSize), r.Format == TES3 ? XCLC = r.ReadS<Xclc>(8) : null),
                FieldType.XCLC => XCLC = r.ReadS<Xclc>(dataSize),
                FieldType.XCLL or FieldType.AMBI => XCLL = r.ReadS<Xcll>(dataSize),
                FieldType.XCLW or FieldType.WHGT => XCLW = r.ReadSingle(),
                // TES3
                FieldType.NAM0 => NAM0 = r.ReadUInt32(),
                FieldType.INTV => INTV = r.ReadINTV(dataSize),
                FieldType.NAM5 => NAM5 = r.ReadS<ByteColor4>(dataSize),
                // TES4
                FieldType.XCLR => XCLRs = r.ReadFArray(z => new Ref<REGNRecord>(r, 4), dataSize >> 2),
                FieldType.XCMT => XCMT = r.ReadByte(),
                FieldType.XCCM => XCCM = new RefX<CLMTRecord>(r, dataSize),
                FieldType.XCWT => XCWT = new RefX<WATRRecord>(r, dataSize),
                FieldType.XOWN => XOWNs.AddX(new Xown { XOWN = new RefX<Record>(r, dataSize) }),
                FieldType.XRNK => XOWNs.Last().XRNK = r.ReadInt32(),
                FieldType.XGLB => XOWNs.Last().XGLB = new RefX<Record>(r, dataSize),
                _ => Empty,
            };
        // Referenced Object Data Grouping
        return type switch {
            // RefObjDataGroup sub-records
            FieldType.FRMR => (_lastRef = RefObjs.AddX(new Ref_())).FRMR = r.ReadUInt32(),
            FieldType.NAME => _lastRef.EDID = r.ReadFUString(dataSize),
            FieldType.XSCL => _lastRef.XSCL = r.ReadSingle(),
            FieldType.DODT => _lastRef.DODT = r.ReadS<Xyza>(dataSize),
            FieldType.DNAM => _lastRef.DNAM = r.ReadFUString(dataSize),
            FieldType.FLTV => _lastRef.FLTV = r.ReadSingle(),
            FieldType.KNAM => _lastRef.KNAM = r.ReadFUString(dataSize),
            FieldType.TNAM => _lastRef.TNAM = r.ReadFUString(dataSize),
            FieldType.UNAM => _lastRef.UNAM = r.ReadByte(),
            FieldType.ANAM => _lastRef.ANAM = r.ReadFUString(dataSize),
            FieldType.BNAM => _lastRef.BNAM = r.ReadFUString(dataSize),
            FieldType.INTV => _lastRef.INTV = r.ReadInt32(),
            FieldType.NAM9 => _lastRef.NAM9 = r.ReadUInt32(),
            FieldType.XSOL => _lastRef.XSOL = r.ReadFUString(dataSize),
            FieldType.DATA => _lastRef.DATA = r.ReadS<Xyza>(dataSize),
            // TES?
            FieldType.CNAM => _lastRef.CNAM = r.ReadFUString(dataSize),
            FieldType.NAM0 => _lastRef.NAM0 = r.ReadUInt32(),
            FieldType.XCHG => _lastRef.XCHG = r.ReadInt32(),
            FieldType.INDX => _lastRef.INDX = r.ReadInt32(),
            _ => Empty,
        };
    }
}

/// <summary>
/// CLAS.Class - 345S0
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/CLAS">
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/CLAS"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/CLAS"/>
/// <see cref="https://starfieldwiki.net/wiki/Starfield_Mod:Mod_File_Format/CLAS">
/// <see cref="https://tes5edit.github.io/fopdoc/Fallout4/Records/CLAS.html"/>
public class CLASRecord : Record {
    public struct Data {
        public enum Specialization_ : uint { Combat = 0, Magic, Stealth }
        [Flags] public enum Flag : uint { Playable = 0x00000001, Guard = 0x00000002 }
        [Flags]
        public enum Service : uint {
            Weapon = 0x00001,
            Armor = 0x00002,
            Clothing = 0x00004,
            Books = 0x00008,
            Ingredients = 0x00010,
            Picks = 0x00020,
            Probes = 0x00040,
            Lights = 0x00080,
            Apparatus = 0x00100,
            RepairItems = 0x00200,
            Misc = 0x00400,
            Spells = 0x00800,
            MagicItems = 0x01000,
            Potions = 0x02000,
            Training = 0x04000,
            Spellmaking = 0x08000,
            Enchanting = 0x10000,
            Repair = 0x20000
        }
        public ActorValue[] PrimaryAttributes;
        public Specialization_ Specialization;
        public ActorValue[] MajorSkills;
        public Flag Flags;
        public Service Services;
        public ActorValue SkillTrained = ActorValue.None_;
        public byte MaximumTrainingLevel;
        public ushort Unused;
        public Data(Reader r, int dataSize) {
            if (r.Format == TES3) {
                PrimaryAttributes = [(ActorValue)r.ReadUInt32(), (ActorValue)r.ReadUInt32()];
                Specialization = (Specialization_)r.ReadUInt32();
                MajorSkills = r.ReadPArray<ActorValue>("I", 10);
                Flags = (Flag)r.ReadUInt32();
                Services = (Service)r.ReadUInt32(); // Buys/Sells and Services
            }
            else if (r.Format == TES4) {
                if (!r.Tes4a) {
                    PrimaryAttributes = [(ActorValue)r.ReadUInt32(), (ActorValue)r.ReadUInt32()];
                    Specialization = (Specialization_)r.ReadUInt32();
                    MajorSkills = r.ReadPArray<ActorValue>("i", 7);
                }
                else
                    MajorSkills = r.ReadPArray<ActorValue>("i", 4);
                Flags = (Flag)r.ReadUInt32();
                Services = (Service)r.ReadUInt32(); // Buys/Sells and Services
                if (!r.Tes4a && dataSize == 48) return;
                SkillTrained = (ActorValue)r.ReadSByte();
                MaximumTrainingLevel = r.ReadByte(); // (0-100)
                Unused = r.ReadUInt16();
                if (SkillTrained != ActorValue.None_) SkillTrained += 12;
            }
            else if (r.Format == TES5) {
                r.Skip(dataSize); // TODO
            }
            else throw new NotImplementedException("CLASRecord");
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Attr {
        public override readonly string ToString() => $"SPECIAL";
        public static (string, int) Struct = ("<7B", 7);
        public byte Strength;
        public byte Perception;
        public byte Endurance;
        public byte Charisma;
        public byte Intelligence;
        public byte Agility;
        public byte Luck;
    }

    public string FULL; // Name
    public string DESC; // Description
    public Data DATA; // Data
    // TES4
    public string ICON; // Large icon filename (Optional)
    public string MICO; // Small icon filename (Optional)
    public Attr? ATTR; // SPECIAL (Fallout)

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadFUString(dataSize),
        FieldType.FULL or FieldType.FNAM => FULL = r.ReadFUString(dataSize),
        FieldType.DATA or FieldType.CLDT => DATA = new Data(r, dataSize),
        FieldType.DESC => DESC = r.ReadFUString(dataSize),
        // TES4
        FieldType.ICON => ICON = r.ReadFUString(dataSize),
        FieldType.MICO => MICO = r.ReadFUString(dataSize),
        FieldType.ATTR => ATTR = r.ReadS<Attr>(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// CLFM.Color - 00500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/CLFM"/>
public class CLFMRecord : Record {
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// CLMT.Climate - 04500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/CLMT"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/CLMT"/>
public class CLMTRecord : Record, IHaveMODL {
    public struct Wlst(Reader r, int dataSize) {
        public RefX<WTHRRecord> Weather = new(r.ReadUInt32());
        public int Chance = r.ReadInt32();
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Tnam {
        public static (string, int) Struct = ("<6B", 6);
        public byte SunriseBegin;
        public byte SunriseEnd;
        public byte SunsetBegin;
        public byte SunsetEnd;
        public byte Volatility;
        public byte MoonsPhaseLength;
    }

    public Modl MODL { get; set; } // Model
    public string FNAM; // Sun Texture
    public string GNAM; // Sun Glare Texture
    public List<Wlst> WLSTs = []; // Climate
    public Tnam TNAM; // Timing

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.MODL => MODL = new Modl(r, dataSize),
        FieldType.MODB => MODL.MODB(r, dataSize),
        FieldType.FNAM => FNAM = r.ReadFUString(dataSize),
        FieldType.GNAM => GNAM = r.ReadFUString(dataSize),
        FieldType.WLST => WLSTs.AddRangeX(r.ReadFArray(z => new Wlst(r, dataSize), dataSize >> 3)),
        FieldType.TNAM => TNAM = r.ReadS<Tnam>(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// CLOT.Clothing - 34000
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/CLOT">
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/CLOT"/>
public class CLOTRecord : Record, IHaveMODL {
    public struct Data {
        public enum Type_ : uint { Pants = 0, Shoes, Shirt, Belt, Robe, R_Glove, L_Glove, Skirt, Ring, Amulet }
        public int Value;
        public float Weight;
        // TES3
        public Type_ Type;
        public short EnchantPts;
        public Data(Reader r, int dataSize) {
            if (r.Format == TES3) {
                Type = (Type_)r.ReadInt32();
                Weight = r.ReadSingle();
                Value = r.ReadInt16();
                EnchantPts = r.ReadInt16();
                return;
            }
            Value = r.ReadInt32();
            Weight = r.ReadSingle();
        }
    }

    public class Indx {
        public override string ToString() => $"{INDX}: {BNAM}";
        public long INDX;
        public string BNAM;
        public string CNAM;
    }

    public Modl MODL { get; set; } // Model Name
    public string FULL; // Item Name
    public Data DATA; // Clothing Data
    public string ENAM; // Enchantment Name
    public RefX<SCPTRecord>? SCRI; // Script Name
    // TES3
    public List<Indx> INDXs = []; // Body Part Index (Moved to Race)
    // TES4
    public uint BMDT; // Clothing Flags
    public Modl MOD2; // Male world model (optional)
    public Modl MOD3; // Female biped (optional)
    public Modl MOD4; // Female world model (optional)
    public short? ANAM; // Enchantment points (optional)

    protected override HashSet<FieldType> DF3 => [FieldType.INDX, FieldType.BNAM, FieldType.CNAM];
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadFUString(dataSize),
        FieldType.MODL => MODL = new Modl(r, dataSize),
        FieldType.MODB => MODL.MODB(r, dataSize),
        FieldType.MODT => MODL.MODT(r, dataSize),
        FieldType.ICON or FieldType.ITEX => MODL.ICON(r, dataSize),
        FieldType.FULL or FieldType.FNAM => FULL = r.ReadFUString(dataSize),
        FieldType.DATA or FieldType.CTDT => DATA = new Data(r, dataSize),
        FieldType.INDX => INDXs.AddX(new Indx { INDX = r.ReadINTV(dataSize) }),
        FieldType.BNAM => INDXs.Last().BNAM = r.ReadFUString(dataSize),
        FieldType.CNAM => INDXs.Last().CNAM = r.ReadFUString(dataSize),
        FieldType.ENAM => ENAM = r.ReadFUString(dataSize),
        FieldType.SCRI => SCRI = new RefX<SCPTRecord>(r, dataSize),
        FieldType.BMDT => BMDT = r.ReadUInt32(),
        FieldType.MOD2 => MOD2 = new Modl(r, dataSize),
        FieldType.MO2B => MOD2.MODB(r, dataSize),
        FieldType.MO2T => MOD2.MODT(r, dataSize),
        FieldType.ICO2 => MOD2.ICON(r, dataSize),
        FieldType.MOD3 => MOD3 = new Modl(r, dataSize),
        FieldType.MO3B => MOD3.MODB(r, dataSize),
        FieldType.MO3T => MOD3.MODT(r, dataSize),
        FieldType.MOD4 => MOD4 = new Modl(r, dataSize),
        FieldType.MO4B => MOD4.MODB(r, dataSize),
        FieldType.MO4T => MOD4.MODT(r, dataSize),
        FieldType.ANAM => ANAM = r.ReadInt16(),
        _ => Empty,
    };
}

/// <summary>
/// CONT.Container - 34500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/CONT">
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/CONT"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/CONT"/>
public class CONTRecord : Record, IHaveMODL {
    public class Data {
        public byte Flags; // flags 0x0001 = Organic, 0x0002 = Respawns, organic only, 0x0008 = Default, unknown
        public float Weight;
        public Data(Reader r, int dataSize) {
            if (r.Format == TES3) {
                Weight = r.ReadSingle();
                return;
            }
            Flags = r.ReadByte();
            Weight = r.ReadSingle();
        }
        public object FLAG(Reader r, int dataSize) => Flags = (byte)r.ReadUInt32();
    }

    public Modl MODL { get; set; } // Model
    public string FULL; // Container Name
    public Data DATA; // Container Data
    public RefX<SCPTRecord>? SCRI;
    public List<CntoX<Record>> CNTOs = [];
    // TES4
    public RefX<SOUNRecord> SNAM; // Open sound
    public RefX<SOUNRecord> QNAM; // Close sound

    protected override HashSet<FieldType> DF3 => [FieldType.NPCO];
    protected override HashSet<FieldType> DF4 => [FieldType.CNTO];
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadFUString(dataSize),
        FieldType.MODL => MODL = new Modl(r, dataSize),
        FieldType.MODB => MODL.MODB(r, dataSize),
        FieldType.MODT => MODL.MODT(r, dataSize),
        FieldType.FULL or FieldType.FNAM => FULL = r.ReadFUString(dataSize),
        FieldType.DATA or FieldType.CNDT => DATA = new Data(r, dataSize),
        FieldType.FLAG => DATA.FLAG(r, dataSize),
        FieldType.CNTO or FieldType.NPCO => CNTOs.AddX(new CntoX<Record>(r, dataSize)),
        FieldType.SCRI => SCRI = new RefX<SCPTRecord>(r, dataSize),
        FieldType.SNAM => SNAM = new RefX<SOUNRecord>(r, dataSize),
        FieldType.QNAM => QNAM = new RefX<SOUNRecord>(r, dataSize),
        _ => Empty,
    };
}

/// <summary>
/// CREA.Creature - 34000
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/CREA">
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/CREA"/>
public abstract class CREARecord : Record, IHaveMODL {
    [Flags]
    public enum Flag : uint {
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

    public Modl MODL { get; set; } // NIF Model
    public string FULL; // Full name
}

public class CREA3Record : CREARecord {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Npdt96 {
        public static (string, int) Struct = ("<24i", 96);
        public int Type; // 0 = Creature, 1 = Daedra, 2 = Undead, 3 = Humanoid
        public int Level;
        public int Strength;
        public int Intelligence;
        public int Willpower;
        public int Agility;
        public int Speed;
        public int Endurance;
        public int Personality;
        public int Luck;
        public int Health;
        public int SpellPts;
        public int Fatigue;
        public int Soul;
        public int Combat;
        public int Magic;
        public int Stealth;
        public int AttackMin1;
        public int AttackMax1;
        public int AttackMin2;
        public int AttackMax2;
        public int AttackMin3;
        public int AttackMax3;
        public int Gold;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Npdt52 {
        public static (string, int) Struct = ("<h8B27sB3h4Bi", 52);
        public short Level;
        public byte Strength;
        public byte Intelligence;
        public byte Willpower;
        public byte Agility;
        public byte Speed;
        public byte Endurance;
        public byte Personality;
        public byte Luck;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 27)] public byte[] Skills;
        public byte Reputation;
        public short Health;
        public short SpellPts;
        public short Fatigue;
        public byte Disposition;
        public byte FactionId;
        public byte Rank;
        public byte Unknown1;
        public int Gold;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Npdt12 {
        public static (string, int) Struct = ("<h6Bi", 12);
        public short Level;
        public byte Disposition;
        public byte FactionId;
        public byte Rank;
        public byte Unknown1;
        public byte Unknown2;
        public byte Unknown3;
        public int Gold;
    }

    [Flags]
    public enum AIFlags : uint {
        Weapons = 0x00001,
        Armor = 0x00002,
        Clothing = 0x00004,
        Books = 0x00008,
        Ingrediant = 0x00010,
        Picks = 0x00020,
        Probes = 0x00040,
        Lights = 0x00080,
        Apparatus = 0x00100,
        Repair = 0x00200,
        Misc = 0x00400, // Miscellaneous
        Spells = 0x00800,
        MagicItems = 0x01000,
        Potions = 0x02000,
        Training = 0x04000,
        Spellmaking = 0x08000,
        Recharge = 0x10000, // Enchanting
        RepairItem = 0x20000
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Aidt {
        public static (string, int) Struct = ("<8BI", 12);
        public byte Hello;
        public byte Unknown1;
        public byte Fight;
        public byte Flee;
        public byte Alarm;
        public byte Unknown2;
        public byte Unknown3;
        public byte Unknown4;
        public AIFlags Flags;
    }

    /// <summary>
    /// Activate package
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Ai_a {
        public static (string, int) Struct = ("<32sB", 33);
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] internal byte[] Name_; public readonly string Name => Encoding.ASCII.GetString(Name_).TrimEnd('\0');
        public byte Unknown;
    }

    /// <summary>
    /// Escort package
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Ai_e {
        public static (string, int) Struct = ("<3fh32sh", 48);
        public float X;
        public float Y;
        public float Z;
        public short Duration;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] Id;
        public short Unknown;
    }

    /// <summary>
    /// Follow package
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Ai_f {
        public static (string, int) Struct = ("<3fh32sh", 48);
        public float X;
        public float Y;
        public float Z;
        public short Duration;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] Id;
        public short Unknown;
    }

    /// <summary>
    /// Travel package
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Ai_t {
        public static (string, int) Struct = ("<4f", 16);
        public float X;
        public float Y;
        public float Z;
        public float Unknown;
    }

    /// <summary>
    /// Wander package
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Ai_w {
        public static (string, int) Struct = ("<2hB8sB", 14);
        public short Distance;
        public short Duration;
        public byte TimeOfDay;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] Idles;
        public byte Unknown;
    }

    public class Ai(Reader r, int dataSize, FieldType type) {
        public override string ToString() => $"{AI}";
        public object AI = type switch {
            FieldType.AI_A => r.ReadS<Ai_a>(dataSize), // AI Activate
            FieldType.AI_E => r.ReadS<Ai_e>(dataSize), // AI Escort
            FieldType.AI_F => r.ReadS<Ai_f>(dataSize), // AI Follow
            FieldType.AI_T => r.ReadS<Ai_t>(dataSize), // AI Travel
            FieldType.AI_W => r.ReadS<Ai_w>(dataSize), // AI Wander
            _ => throw new Exception()
        }; // AI
        public string CNDT; // Cell escort/follow to string (optional)
    }

    public string CNAM; // Sound Gen Creature
    public object NPDT; // Creature data
    public int FLAG; // Creature Flags
    public RefX<SCPTRecord>? SCRI; // Script
    public List<CntoX<Record>> NPCOs = []; // Item record
    public Aidt AIDT; // AI data
    public List<Ai> AIs = []; // AI packages
    public float? XSCL; // Scale (optional), Only present if the scale is not 1.0
    public List<string> NPCSs = [];

    protected override HashSet<FieldType> DF3 => [FieldType.NPCO, FieldType.NPCS, FieldType.AI_A, FieldType.AI_E, FieldType.AI_F, FieldType.AI_T, FieldType.AI_W, FieldType.CNDT];
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.NAME => EDID = r.ReadFUString(dataSize),
        FieldType.MODL => MODL = new Modl(r, dataSize),
        FieldType.CNAM => CNAM = r.ReadFUString(dataSize),
        FieldType.FNAM => FULL = r.ReadFUString(dataSize),
        FieldType.NPDT => NPDT = r.ReadS<Npdt96>(dataSize),
        FieldType.FLAG => FLAG = r.ReadInt32(),
        FieldType.SCRI => SCRI = new RefX<SCPTRecord>(r, dataSize),
        FieldType.NPCO => NPCOs.AddX(new CntoX<Record>(r, dataSize)),
        FieldType.AIDT => AIDT = r.ReadS<Aidt>(dataSize),
        FieldType.AI_A or FieldType.AI_E or FieldType.AI_F or FieldType.AI_T or FieldType.AI_W => AIs.AddX(new Ai(r, dataSize, type)),
        FieldType.CNDT => AIs.Last().CNDT = r.ReadFUString(dataSize),
        FieldType.XSCL => XSCL = r.ReadSingle(),
        FieldType.NPCS => NPCSs.AddX(r.ReadFAString(dataSize)),
        _ => Empty,
    };
}

public class CREA4Record : CREARecord {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Acbs {
        public static (string, int) Struct = ("<I3Hh2H", 16);
        public uint Flags;          // Flags
        public ushort BaseSpell;    // Base spell points
        public ushort Fatigue;      // Fatigue
        public ushort BarterGold;   // Barter gold
        public short Level;         // Level/Offset level
        public ushort CalcMin;      // Calc Min
        public ushort CalcMax;      // Calc Max
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Aidt {
        public static (string, int) Struct = ("<4BI2BH", 12);
        public byte Aggression;  // Aggression
        public byte Confidence;  // Confidence
        public byte EnergyLevel; // Energy Level
        public byte BarterGold;  // Barter gold
        public CREA3Record.AIFlags AiFlags; // Flags
        public byte TrainSkill;  // Training skill
        public byte TrainLevel;  // Training level - Value is same as index into skills array below.
        public ushort AiUnknown; // Unused?
    }

    public class Csdt {
        public int CSDT; // Soundtype
        public Ref<SOUNRecord> CSDI; // TESSound
        public byte CSDC; // Chance
    }

    public string NIFZ; // NIF-files used by the creature
    public Acbs ACBS; // Configuration
    public List<RefB<FACTRecord>> SNAMs = []; // Factions
    public Ref<LVLIRecord>? INAM; // Death Item
    public byte RNAM; // Attack reach
    public List<string> SPLOs = []; // Spells
    public Ref<SCPTRecord>? SCRI; // Script
    public List<Cnto<Record>> CNTOs = []; // Items
    public List<RefS<PACKRecord>> PKIDs = []; // AI Packages
    public Ref<CSTYRecord> ZNAM; // Combat Style
    public Ref<CREA4Record> CSCR; // Inherits Sounds from
    public List<Csdt> CSDTs = []; // Soundtypes
    public float BNAM; // Base Scale
    public float TNAM; // Turning Speed
    public float WNAM; // Foot Weight
    public string NAM0; // Blood Spray
    public string NAM1; // Blood Decal
    public string KFFZ; // Optional Animation List

    protected override HashSet<FieldType> DF4 => [FieldType.SNAM, FieldType.SPLO, FieldType.CNTO, FieldType.PKID, FieldType.CSDT, FieldType.CSDI, FieldType.CSDC];
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.FULL => FULL = r.ReadFUString(dataSize),
        FieldType.MODL => MODL = new Modl(r, dataSize),
        FieldType.MODB => MODL.MODB(r, dataSize),
        FieldType.MODT => MODL.MODT(r, dataSize),
        FieldType.NIFZ => NIFZ = r.ReadFUString(dataSize),
        FieldType.ACBS => ACBS = r.ReadS<Acbs>(dataSize),
        FieldType.SNAM => SNAMs.AddX(new RefB<FACTRecord>(r, dataSize)),
        FieldType.INAM => INAM = new Ref<LVLIRecord>(r, dataSize),
        FieldType.RNAM => RNAM = r.ReadByte(),
        FieldType.SPLO => SPLOs.AddX(r.ReadFUString(dataSize)),
        FieldType.SCRI => SCRI = new Ref<SCPTRecord>(r, dataSize),
        FieldType.CNTO => CNTOs.AddX(new Cnto<Record>(r, dataSize)),
        FieldType.PKID => PKIDs.AddX(new RefS<PACKRecord>(r, dataSize)),
        FieldType.ZNAM => ZNAM = new Ref<CSTYRecord>(r, dataSize),
        FieldType.CSCR => CSCR = new Ref<CREA4Record>(r, dataSize),
        FieldType.CSDT => CSDTs.AddX(new Csdt { CSDT = r.ReadInt32() }),
        FieldType.CSDI => CSDTs.Last().CSDI = new Ref<SOUNRecord>(r, dataSize),
        FieldType.CSDC => CSDTs.Last().CSDC = r.ReadByte(),
        FieldType.BNAM => BNAM = r.ReadSingle(),
        FieldType.TNAM => TNAM = r.ReadSingle(),
        FieldType.WNAM => WNAM = r.ReadSingle(),
        FieldType.NAM0 => NAM0 = r.ReadFUString(dataSize),
        FieldType.NAM1 => NAM1 = r.ReadFUString(dataSize),
        FieldType.KFFZ => KFFZ = r.ReadFUString(dataSize),
        FieldType.NIFT => r.Skip(dataSize), //TODO
        FieldType.AIDT => r.Skip(dataSize), //TODO
        FieldType.DATA => r.Skip(dataSize), //TODO
        _ => Empty
    };
}

/// <summary>
/// CSTY.Combat Style - 00500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/CSTY"/>
public class CSTYRecord : Record {
    public class Cstd {
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
        public Cstd(Reader r, int dataSize) {
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
            if (dataSize == 84) return;
            RangeMult_Optimal = r.ReadSingle();
            RangeMult_Max = r.ReadSingle();
            if (dataSize == 92) return;
            SwitchDistance_Melee = r.ReadSingle();
            SwitchDistance_Ranged = r.ReadSingle();
            BuffStandoffDistance = r.ReadSingle();
            if (dataSize == 104) return;
            RangedStandoffDistance = r.ReadSingle();
            GroupStandoffDistance = r.ReadSingle();
            if (dataSize == 112) return;
            RushingAttackPercentChance = r.ReadByte();
            r.Skip(3); // Unused
            RushingAttackDistanceMult = r.ReadSingle();
            if (dataSize == 120) return;
            Flags2 = r.ReadUInt32();
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Csad {
        public static (string, int) Struct = ("<21f", 84);
        public float DodgeFatigueModMult;
        public float DodgeFatigueModBase;
        public float EncumbSpeedModBase;
        public float EncumbSpeedModMult;
        public float DodgeWhileUnderAttackMult;
        public float DodgeNotUnderAttackMult;
        public float DodgeBackWhileUnderAttackMult;
        public float DodgeBackNotUnderAttackMult;
        public float DodgeForwardWhileAttackingMult;
        public float DodgeForwardNotAttackingMult;
        public float BlockSkillModifierMult;
        public float BlockSkillModifierBase;
        public float BlockWhileUnderAttackMult;
        public float BlockNotUnderAttackMult;
        public float AttackSkillModifierMult;
        public float AttackSkillModifierBase;
        public float AttackWhileUnderAttackMult;
        public float AttackNotUnderAttackMult;
        public float AttackDuringBlockMult;
        public float PowerAttFatigueModBase;
        public float PowerAttFatigueModMult;
    }

    public Cstd CSTD; // Standard
    public Csad CSAD; // Advanced

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.CSTD => CSTD = new Cstd(r, dataSize),
        FieldType.CSAD => CSAD = r.ReadS<Csad>(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// DIAL.Dialog Topic - 34500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/DIAL">
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/DIAL"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/DIAL"/>
public class DIALRecord : Record {
    internal static DIALRecord _lastRecord;
    public enum Type3 : byte { Topic = 0, Voice, Greeting, Persuasion, Journal }
    public enum Type4 : byte { Topic = 0, Conversation, Combat, Persuasion, Detection, Service, Miscellaneous, Radio }
    public string FULL; // Dialogue Name
    public byte DATA; // Dialogue Type
    public List<RefX<QUSTRecord>> QSTIs; // Quests (optional)
    public List<INFO3Record> INFOs = []; // Info Records

    protected override HashSet<FieldType> DF4 => [FieldType.QSTI];
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => (EDID = FULL = r.ReadFUString(dataSize), _lastRecord = this),
        FieldType.FULL => FULL = r.ReadFUString(dataSize),
        FieldType.DATA => DATA = r.ReadByte(),
        FieldType.QSTI or FieldType.QSTR => (QSTIs ??= []).AddX(new RefX<QUSTRecord>(r, dataSize)),
        _ => Empty,
    };
}

/// <summary>
/// DLBR.Dialog Branch - 00500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/DLBR"/>
public class DLBRRecord : Record {
    public ByteColor4 CNAM; // RGB color

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.CNAM => CNAM = r.ReadS<ByteColor4>(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// DLVW.Dialog View - 00500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/DLVW"/>
public class DLVWRecord : Record {
    public ByteColor4 CNAM; // RGB color

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.CNAM => CNAM = r.ReadS<ByteColor4>(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// DMGT.Damage Type - 04000 #F4
/// </summary>
/// <see cref="https://tes5edit.github.io/fopdoc/Fallout4/Records/DMGT.html"/>
public class DMGTRecord : Record {
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// DOOR.Door - 34500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/DOOR">
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/DOOR"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/DOOR"/>
public class DOORRecord : Record, IHaveMODL {
    public string FULL; // Door name
    public Modl MODL { get; set; } // NIF model filename
    public RefX<SCPTRecord>? SCRI; // Script (optional)
    public RefX<SOUNRecord>? SNAM; // Open Sound
    public RefX<SOUNRecord>? ANAM; // Close Sound
    // TES4
    public RefX<SOUNRecord>? BNAM; // Loop Sound
    public byte FNAM; // Flags
    public List<RefX<Record>> TNAMs = []; // Random teleport destination

    protected override HashSet<FieldType> DF4 => [FieldType.TNAM];
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = FULL = r.ReadFUString(dataSize),
        FieldType.FULL => FULL = r.ReadFUString(dataSize),
        FieldType.FNAM => r.Format != TES3 ? FNAM = r.ReadByte() : FULL = r.ReadFUString(dataSize),
        FieldType.MODL => MODL = new Modl(r, dataSize),
        FieldType.MODB => MODL.MODB(r, dataSize),
        FieldType.MODT => MODL.MODT(r, dataSize),
        FieldType.SCRI => SCRI = new RefX<SCPTRecord>(r, dataSize),
        FieldType.SNAM => SNAM = new RefX<SOUNRecord>(r, dataSize),
        FieldType.ANAM => ANAM = new RefX<SOUNRecord>(r, dataSize),
        FieldType.BNAM => ANAM = new RefX<SOUNRecord>(r, dataSize),
        FieldType.TNAM => TNAMs.AddX(new RefX<Record>(r, dataSize)),
        _ => Empty,
    };
}

/// <summary>
/// EFSH.Effect Shader - 04500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/EFSH"/>
public class EFSHRecord : Record {
    public class Data {
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
        public Data(Reader r, int dataSize) {
            if (dataSize != 224 && dataSize != 96) Flags = 0;
            Flags = r.ReadByte();
            r.Skip(3); // Unused
            MembraneShader_SourceBlendMode = r.ReadUInt32();
            MembraneShader_BlendOperation = r.ReadUInt32();
            MembraneShader_ZTestFunction = r.ReadUInt32();
            FillTextureEffect_Color = r.ReadS<ByteColor4>(4);
            FillTextureEffect_AlphaFadeInTime = r.ReadSingle();
            FillTextureEffect_FullAlphaTime = r.ReadSingle();
            FillTextureEffect_AlphaFadeOutTime = r.ReadSingle();
            FillTextureEffect_PresistentAlphaRatio = r.ReadSingle();
            FillTextureEffect_AlphaPulseAmplitude = r.ReadSingle();
            FillTextureEffect_AlphaPulseFrequency = r.ReadSingle();
            FillTextureEffect_TextureAnimationSpeed_U = r.ReadSingle();
            FillTextureEffect_TextureAnimationSpeed_V = r.ReadSingle();
            EdgeEffect_FallOff = r.ReadSingle();
            EdgeEffect_Color = r.ReadS<ByteColor4>(4);
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
            ColorKey1_Color = r.ReadS<ByteColor4>(4);
            ColorKey2_Color = r.ReadS<ByteColor4>(4);
            ColorKey3_Color = r.ReadS<ByteColor4>(4);
            ColorKey1_ColorAlpha = r.ReadSingle();
            ColorKey2_ColorAlpha = r.ReadSingle();
            ColorKey3_ColorAlpha = r.ReadSingle();
            ColorKey1_ColorKeyTime = r.ReadSingle();
            ColorKey2_ColorKeyTime = r.ReadSingle();
            ColorKey3_ColorKeyTime = r.ReadSingle();
        }
    }

    public string ICON; // Fill Texture
    public string ICO2; // Particle Shader Texture
    public Data DATA; // Data

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.ICON => ICON = r.ReadFUString(dataSize),
        FieldType.ICO2 => ICO2 = r.ReadFUString(dataSize),
        FieldType.DATA => DATA = new Data(r, dataSize),
        _ => Empty,
    };
}

/// <summary>
/// ENCH.Enchantment - 345S0
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/ENCH">
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/ENCH"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/ENCH"/>
/// <see cref="https://starfieldwiki.net/wiki/Starfield_Mod:Mod_File_Format/ENCH">
public class ENCHRecord : Record {
    public struct Enit {
        public enum Type3 : int { CastOnce = 0, CastStrikes, CastWhenUsed, ConstantEffect }
        public enum Type4 : int { Scroll = 0, Staff, Weapon, Apparel }
        [Flags] public enum Flag : int { AutoCalc = 0x01 }
        public int Type;
        public int EnchantCost;
        public int ChargeAmount; // Charge
        public Flag Flags;
        public Enit(Reader r, int dataSize) {
            Type = r.ReadInt32();
            if (r.Format == TES3) {
                EnchantCost = r.ReadInt32();
                ChargeAmount = r.ReadInt32();
            }
            else {
                ChargeAmount = r.ReadInt32();
                EnchantCost = r.ReadInt32();
            }
            Flags = (Flag)r.ReadInt32();
        }
    }

    public class Efit {
        public enum Type_ : int { Self = 0, Touch, Target }
        public string EffectId;
        public Type_ Type;
        public int Area;
        public int Duration;
        public int MagnitudeMin;
        // TES3
        public sbyte SkillId; // (-1 if NA)
        public sbyte AttributeId; // (-1 if NA)
        public int MagnitudeMax;
        // TES4
        public ActorValue ActorValue = ActorValue.None_;
        public Efit(Reader r, int dataSize) {
            if (r.Format == TES3) {
                EffectId = r.ReadUInt16().ToString();
                SkillId = r.ReadSByte();
                AttributeId = r.ReadSByte();
                Type = (Type_)r.ReadInt32();
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
            Type = (Type_)r.ReadInt32();
            ActorValue = (ActorValue)r.ReadInt32();
        }
    }

    // TES4
    public class Scit {
        public string Name;
        public int ScriptFormId;
        public int School; // 0 = Alteration, 1 = Conjuration, 2 = Destruction, 3 = Illusion, 4 = Mysticism, 5 = Restoration
        public string VisualEffect;
        public uint Flags;
        public Scit(Reader r, int dataSize) {
            Name = "Script Effect";
            ScriptFormId = r.ReadInt32();
            if (dataSize == 4) return;
            School = r.ReadInt32();
            VisualEffect = r.ReadFAString(4);
            Flags = dataSize > 12 ? r.ReadUInt32() : 0;
        }
        public object FULL(Reader r, int dataSize) => Name = r.ReadFUString(dataSize);
    }

    public string FULL; // Enchant name
    public Enit ENIT; // Enchant Data
    public List<Efit> EFITs = []; // Effect Data
                                  // TES4
    public List<Scit> SCITs = []; // Script effect data

    protected override HashSet<FieldType> DF3 => [FieldType.ENAM];
    protected override HashSet<FieldType> DF4 => [FieldType.EFID, FieldType.EFIT];
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = FULL = r.ReadFUString(dataSize),
        FieldType.FULL => SCITs.Count == 0 ? FULL = r.ReadFUString(dataSize) : SCITs.Last().FULL(r, dataSize),
        FieldType.ENIT or FieldType.ENDT => ENIT = new Enit(r, dataSize),
        FieldType.EFID => r.Skip(dataSize),
        FieldType.EFIT or FieldType.ENAM => EFITs.AddX(new Efit(r, dataSize)),
        FieldType.SCIT => SCITs.AddX(new Scit(r, dataSize)),
        _ => Empty,
    };
}

/// <summary>
/// EQUP.Equip Slots - 005S0
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/EQUP"/>
/// <see cref="https://starfieldwiki.net/wiki/Starfield_Mod:Mod_File_Format/EQUP">
public class EQUPRecord : Record {
    public string FULL;
    public byte DATA;

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.DATA => DATA = r.ReadByte(),
        _ => Empty,
    };
}

/// <summary>
/// EYES.Eyes - 04500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/EYES"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/EYES"/>
public class EYESRecord : Record {
    public string FULL;
    public string ICON;
    public byte DATA; // Playable

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.FULL => FULL = r.ReadFUString(dataSize),
        FieldType.ICON => ICON = r.ReadFUString(dataSize),
        FieldType.DATA => DATA = r.ReadByte(),
        _ => Empty,
    };
}

/// <summary>
/// FACT.Faction - 345S0
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/FACT">
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/FACT"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/FACT"/>
/// <see cref="https://starfieldwiki.net/wiki/Starfield_Mod:Mod_File_Format/FACT">
public class FACTRecord : Record {
    public class Rnam {
        public override string ToString() => $"{RNAM}:{MNAM}";
        public int RNAM; // rank
        public string MNAM; // male
        public string FNAM; // female
        public string INAM; // insignia
    }

    // TES3
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RankModifier {
        public static (string, int) Struct = ("<5I", 20);
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public uint[] Attributes; // Attribute modifiers
        public uint PrimarySkill; // Primary skill modifier
        public uint FavoredSkill; // Favored skill modifier
        public uint FactionReaction; // Faction reaction modifier
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Fadt {
        [Flags] public enum Flag : uint { HiddenFromPlayer = 0x1 }
        public static (string, int) Struct = ("<2I20I20I20I20I52I7iI", 240);
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public uint[] Attributes;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)] public RankModifier[] RankModifiers;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)] public ActorValue[] Skills;
        public Flag Flags;
    }

    public class Anam {
        public override string ToString() => $"{ANAM}:{INTV}";
        public string ANAM; // Faction name
        public long INTV; // Faction reaction
    }

    // TES4
    public struct Xnam(Reader r, int dataSize) {
        public override readonly string ToString() => $"{FormId}";
        public int FormId = r.ReadInt32();
        public int Mod = r.ReadInt32();
        public int Combat = r.Tes4a || r.Format > TES4 ? r.ReadInt32() : 0;
    }

    // TES5
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Crva {
        public static Dictionary<int, string> Struct = new() { [12] = "<2B5H", [16] = "<2B5Hf", [20] = "<2B5Hf2H" };
        public byte Arrest;
        public byte AttackOnSight;
        public ushort Murder;
        public ushort Assault;
        public ushort Trespass;
        public ushort Pickpocket;
        public ushort Unused; // usually 0, but not always, changing data has no effect in CK
        public float StealMult;
        public ushort Escape;
        public ushort Werewolf;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Venv {
        public static (string, int) Struct = ("<2HI2BH", 12);
        public ushort StartHour;
        public ushort EndHour;
        public uint Radius;
        public byte BuysStolenItems; // Wording in CK is misleading
        public byte NotSellBuy; // Causes vendor to buy/sell everything except what's in the Vendor List
        public ushort Unused;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Plvd {
        public enum SpecType : uint {
            NearReference = 0, // REFR formID follows
            InCell, // CELL formID follows
            NearPackageStartLocation, // Not used in original files
            NearEditorLocation,
            LinkedReference = 6, // KWYD formID follows not used in original files
            NearSelf = 12,
        }
        public static (string, int) Struct = ("<3I", 12);
        public SpecType Type; // SpecificationType
        public Ref<Record> Id; // Meaning depends on previous int
        public uint Unused;
    }

    public string FULL; // Faction name
    public List<Rnam> RNAMs = []; // Ranks
    public Fadt? FADT; // Faction data
    public List<Anam> ANAMs = []; // Factions
    // TES4
    public List<Xnam> XNAMs = []; // Interfaction Relations
    public long DATA; // Flags (byte, uint32)
    public uint CNAM;
    // TES5
    public Ref<REFRRecord> JAIL; // Prison Marker
    public Ref<REFRRecord> WAIT; // Follower Wait Marker
    public Ref<REFRRecord> STOL; // Evidence Chest
    public Ref<REFRRecord> PLCN; // Player Belongings Chest
    public Ref<FLSTRecord> CRGR; // Crime Group
    public Ref<OTFTRecord> JOUT; // Jail outfit the player is given.
    public Crva CRVA; // Crime Gold
    public Ref<FLSTRecord> VEND; // Vendor List
    public Ref<REFRRecord> VENC; // Vendor Chest
    public Venv VENV; // Vendor
    public Plvd PLVD; // Where to sell goods

    protected override HashSet<FieldType> DF3 => [FieldType.RNAM, FieldType.ANAM, FieldType.INTV];
    protected override HashSet<FieldType> DF4 => [FieldType.XNAM, FieldType.RNAM, FieldType.MNAM, FieldType.FNAM, FieldType.INAM];
    protected override HashSet<FieldType> DF5 => [FieldType.XNAM, FieldType.RNAM, FieldType.MNAM, FieldType.FNAM];
    public override object ReadField(Reader r, FieldType type, int dataSize) => r.Format == TES3
        ? type switch {
            FieldType.NAME => EDID = r.ReadFUString(dataSize),
            FieldType.FNAM => FULL = r.ReadFUString(dataSize),
            FieldType.RNAM => RNAMs.AddX(new Rnam { MNAM = r.ReadFUString(dataSize) }),
            FieldType.FADT => FADT = r.ReadS<Fadt>(dataSize),
            FieldType.ANAM => ANAMs.AddX(new Anam { ANAM = r.ReadFUString(dataSize) }),
            FieldType.INTV => ANAMs.Last().INTV = r.ReadINTV(dataSize),
            _ => Empty,
        }
        : type switch {
            FieldType.EDID => EDID = r.ReadFUString(dataSize),
            FieldType.FULL => FULL = r.ReadFUString(dataSize),
            FieldType.XNAM => XNAMs.AddX(new Xnam(r, dataSize)),
            FieldType.DATA => DATA = r.ReadINTV(dataSize),
            FieldType.CNAM => CNAM = r.ReadUInt32(), //TES4
            FieldType.JAIL => JAIL = new Ref<REFRRecord>(r, dataSize), //TES5
            FieldType.WAIT => WAIT = new Ref<REFRRecord>(r, dataSize), //TES5
            FieldType.STOL => STOL = new Ref<REFRRecord>(r, dataSize), //TES5
            FieldType.PLCN => PLCN = new Ref<REFRRecord>(r, dataSize), //TES5
            FieldType.CRGR => CRGR = new Ref<FLSTRecord>(r, dataSize), //TES5
            FieldType.JOUT => JOUT = new Ref<OTFTRecord>(r, dataSize), //TES5
            FieldType.CRVA => CRVA = r.ReadS<Crva>(dataSize), //TES5
            // ??
            FieldType.RNAM => RNAMs.AddX(new Rnam { RNAM = r.ReadInt32() }),
            FieldType.MNAM => RNAMs.Last().MNAM = r.ReadFUString(dataSize),
            FieldType.FNAM => RNAMs.Last().FNAM = r.ReadFUString(dataSize),
            FieldType.INAM => RNAMs.Last().INAM = r.ReadFUString(dataSize), //TES4
            FieldType.VEND => VEND = new Ref<FLSTRecord>(r, dataSize), //TES5
            FieldType.VENC => VENC = new Ref<REFRRecord>(r, dataSize), //TES5
            FieldType.VENV => VENV = r.ReadS<Venv>(dataSize), //TES5
            FieldType.PLVD => PLVD = r.ReadS<Plvd>(dataSize), //TES5
            FieldType.CITC => r.Skip(dataSize), //TES5 TODO
            FieldType.CTDA => r.Skip(dataSize), //TES5 TODO
            FieldType.CIS2 => r.Skip(dataSize), //TES5 TODO
            _ => Empty,
        };
}

/// <summary>
/// FLOR.Flora - 045S0
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/FLOR"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/FLOR"/>
/// <see cref="https://starfieldwiki.net/wiki/Starfield_Mod:Mod_File_Format/FLOR">
/// </summary>
public class FLORRecord : Record, IHaveMODL {
    public Modl MODL { get; set; } // Model
    public string FULL; // Plant Name
    public RefX<SCPTRecord> SCRI; // Script (optional)
    public RefX<INGRRecord> PFIG; // The ingredient the plant produces (optional)
    public byte[] PFPC; // Spring, Summer, Fall, Winter Ingredient Production (byte)

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.MODL => MODL = new Modl(r, dataSize),
        FieldType.MODB => MODL.MODB(r, dataSize),
        FieldType.MODT => MODL.MODT(r, dataSize),
        FieldType.FULL => FULL = r.ReadFUString(dataSize),
        FieldType.SCRI => SCRI = new RefX<SCPTRecord>(r, dataSize),
        FieldType.PFIG => PFIG = new RefX<INGRRecord>(r, dataSize),
        FieldType.PFPC => PFPC = r.ReadBytes(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// FLST.Form List (non-leveled list) - 00500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/FLST"/>
public class FLSTRecord : Record {
    public Ref<Record> LNAM; // Object

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.LNAM => LNAM = new Ref<Record>(r, dataSize),
        _ => false,
    };
}

/// <summary>
/// FURN.Furniture - 04500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/FURN"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/FURN"/>
public class FURNRecord : Record, IHaveMODL {
    public Modl MODL { get; set; } // Model
    public string FULL; // Furniture Name
    public RefX<SCPTRecord> SCRI; // Script (optional)
    public int MNAM; // Active marker flags, required. A bit field with a bit value of 1 indicating that the matching marker position in the NIF file is active.

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.MODL => MODL = new Modl(r, dataSize),
        FieldType.MODB => MODL.MODB(r, dataSize),
        FieldType.MODT => MODL.MODT(r, dataSize),
        FieldType.FULL => FULL = r.ReadFUString(dataSize),
        FieldType.SCRI => SCRI = new RefX<SCPTRecord>(r, dataSize),
        FieldType.MNAM => MNAM = r.ReadInt32(),
        _ => Empty,
    };
}

/// <summary>
/// GLOB.Global - 34500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/GLOB">
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/GLOB"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/GLOB"/>
public class GLOBRecord : Record {
    public char FNAM; // Type of global (s, l, f)
    public float FLTV; // Float data

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadFUString(dataSize),
        FieldType.FNAM => FNAM = (char)r.ReadByte(),
        FieldType.FLTV => FLTV = r.ReadSingle(),
        _ => Empty,
    };
}

/// <summary>
/// GMST.Game Setting - 345S0
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/GMST">
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/GMST"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/GMST"/>
/// <see cref="https://starfieldwiki.net/wiki/Starfield_Mod:Mod_File_Format/GMST">
public class GMSTRecord : Record {
    public Datv DATA; // Data

    public override object ReadField(Reader r, FieldType type, int dataSize) => r.Format == TES3
        ? type switch {
            FieldType.NAME => EDID = r.ReadFUString(dataSize),
            FieldType.STRV => DATA = r.ReadDATV(dataSize, 's'),
            FieldType.INTV => DATA = r.ReadDATV(dataSize, 'i'),
            FieldType.FLTV => DATA = r.ReadDATV(dataSize, 'f'),
            _ => Empty,
        }
        : type switch {
            FieldType.EDID => EDID = r.ReadFUString(dataSize),
            FieldType.DATA => DATA = r.ReadDATV(dataSize, EDID[0]),
            _ => Empty,
        };
}

/// <summary>
/// GRAS.Grass - 04500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/GRAS"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/GRAS"/>
public class GRASRecord : Record {
    public struct Data {
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
        public Data(Reader r, int dataSize) {
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

    public Modl MODL;
    public Data DATA;

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.MODL => MODL = new Modl(r, dataSize),
        FieldType.MODB => MODL.MODB(r, dataSize),
        FieldType.MODT => MODL.MODT(r, dataSize),
        FieldType.DATA => DATA = new Data(r, dataSize),
        _ => Empty,
    };
}

/// <summary>
/// HAIR.Hair - 04500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/HAIR"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/HAIR"/>
public class HAIRRecord : Record, IHaveMODL {
    public string FULL;
    public Modl MODL { get; set; }
    public byte DATA; // Playable, Not Male, Not Female, Fixed

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.FULL => FULL = r.ReadFUString(dataSize),
        FieldType.MODL => MODL = new Modl(r, dataSize),
        FieldType.MODB => MODL.MODB(r, dataSize),
        FieldType.MODT => MODL.MODT(r, dataSize),
        FieldType.ICON => MODL.ICON(r, dataSize),
        FieldType.DATA => DATA = r.ReadByte(),
        _ => Empty,
    };
}

/// <summary>
/// HDPT.Head Part - 00500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/HDPT"/>
/// <see cref="https://tes5edit.github.io/fopdoc/Fallout3/Records/HDPT.html"/>
public class HDPTRecord : Record, IHaveMODL {
    public class Nam0 {
        public uint NAM0; // Option type
        public string NAM1; // .tri file
    }

    public string FULL; // Name
    public Modl MODL { get; set; } // Model
    public byte DATA; // Flags
    public uint PNAM; // Type
    public List<Ref<HDPTRecord>> HNAMs = []; // Additional part
    public List<Nam0> NAM0s = []; // Option type
    public Ref<TXSTRecord>? TNAM; // Base texture
    public Ref<FLSTRecord>? RNAM; // Resource list
    public Ref<Record>? CNAM; // Color (seen in Dawnguard.esm)

    protected override HashSet<FieldType> DF4 => [FieldType.HNAM];
    protected override HashSet<FieldType> DF5 => [FieldType.HNAM, FieldType.NAM0, FieldType.NAM1];
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.FULL => FULL = r.ReadFUString(dataSize),
        FieldType.MODL => MODL = new Modl(r, dataSize),
        FieldType.MODT => MODL.MODT(r, dataSize),
        FieldType.MODS => MODL.MODS(r, dataSize),
        FieldType.DATA => DATA = r.ReadByte(),
        FieldType.PNAM => PNAM = r.ReadUInt32(),
        FieldType.HNAM => HNAMs.AddX(new Ref<HDPTRecord>(r, dataSize)),
        FieldType.NAM0 => NAM0s.AddX(new Nam0 { NAM0 = r.ReadUInt32() }),
        FieldType.NAM1 => NAM0s.Last().NAM1 = r.ReadFUString(dataSize),
        FieldType.TNAM => TNAM = new Ref<TXSTRecord>(r, dataSize),
        FieldType.RNAM => RNAM = new Ref<FLSTRecord>(r, dataSize),
        FieldType.CNAM => CNAM = new Ref<Record>(r, dataSize),
        _ => false,
    };
}

/// <summary>
/// IDLE.Idle Animations - 04500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/IDLE"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/IDLE"/>
public class IDLERecord : Record, IHaveMODL {
    public Modl MODL { get; set; }
    public List<SCPTRecord.Ctda> CTDAs = []; // Conditions
    public byte ANAM;
    public Ref<IDLERecord>[] DATAs;

    protected override HashSet<FieldType> DF4 => [FieldType.CTDA, FieldType.CTDT];
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.MODL => MODL = new Modl(r, dataSize),
        FieldType.MODB => MODL.MODB(r, dataSize),
        FieldType.MODT => MODL.MODT(r, dataSize),
        FieldType.CTDA or FieldType.CTDT => CTDAs.AddX(new SCPTRecord.Ctda(r, dataSize)),
        FieldType.ANAM => ANAM = r.ReadByte(),
        FieldType.DATA => DATAs = r.ReadFArray(z => new Ref<IDLERecord>(r, 4), dataSize >> 2),
        _ => Empty,
    };
}

/// <summary>
/// INFO.Dialog Topic Info - 34500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/INFO">
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/INFO"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/INFO"/>
public class INFORecord : Record {
}

public class INFO3Record : INFORecord {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Data {
        public static (string, int) Struct = ("<2i4B", 12);
        public int Unknown1;
        public int Disposition;
        public byte Rank; // (0-10)
        public byte Gender; // 0xFF = None, 0x00 = Male, 0x01 = Female
        public byte PCRank; // (0-10)
        public byte Unknown2;
    }

    public RefX<INFO3Record> PNAM; // Previous info ID
    public string NNAM; // Next info ID (form a linked list of INFOs for the DIAL). First INFO has an empty PNAM, last has an empty NNAM.
    public Data DATA; // Info data
    public string ONAM; // Actor
    public string RNAM; // Race
    public string CNAM; // Class
    public string FNAM; // Faction 
    public string ANAM; // Cell
    public string DNAM; // PC Faction
    public string NAME; // The info response string (512 max)
    public string SNAM; // Sound
    public byte? QSTN; // Journal Name
    public byte? QSTF; // Journal Finished
    public byte? QSTR; // Journal Restart
    public List<SCPTRecord.Scvr> SCVRs = []; // String for the function/variable choice
    public string BNAM; // Result text (not compiled)

    protected override HashSet<FieldType> DF3 => [FieldType.SCVR, FieldType.INTV, FieldType.FLTV];
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.INAM => (z: DIALRecord._lastRecord?.INFOs.AddX(this), EDID = r.ReadFUString(dataSize)).z,
        FieldType.PNAM => PNAM = new RefX<INFO3Record>(r, dataSize),
        FieldType.NNAM => NNAM = r.ReadFUString(dataSize),
        FieldType.DATA => DATA = r.ReadS<Data>(dataSize),
        FieldType.ONAM => ONAM = r.ReadFUString(dataSize),
        FieldType.RNAM => RNAM = r.ReadFUString(dataSize),
        FieldType.CNAM => CNAM = r.ReadFUString(dataSize),
        FieldType.FNAM => FNAM = r.ReadFUString(dataSize),
        FieldType.ANAM => ANAM = r.ReadFUString(dataSize),
        FieldType.DNAM => DNAM = r.ReadFUString(dataSize),
        FieldType.NAME => NAME = r.ReadFUString(dataSize), //TES3.NAME.Value = TES3.NAME.Value.Replace('\ufffd', '\x1')).z,
        FieldType.SNAM => SNAM = r.ReadFUString(dataSize),
        FieldType.QSTN => QSTN = r.ReadByte(),
        FieldType.QSTF => QSTF = r.ReadByte(),
        FieldType.QSTR => QSTR = r.ReadByte(),
        FieldType.SCVR => SCVRs.AddX(new SCPTRecord.Scvr { SCVR = new SCPTRecord.Ctda(r, dataSize) }),
        FieldType.INTV => SCVRs.Last().INTV = r.ReadINTV(dataSize),
        FieldType.FLTV => SCVRs.Last().FLTV = r.ReadSingle(),
        FieldType.BNAM => BNAM = r.ReadFUString(dataSize),
        _ => Empty,
    };
}

public class INFO4Record : INFORecord {
    public struct Data(Reader r, int dataSize) {
        public byte Type = r.ReadByte();
        public byte NextSpeaker = r.ReadByte();
        public byte Flags = dataSize == 3 ? r.ReadByte() : (byte)0;
    }

    public class Trdt {
        public uint EmotionType;
        public int EmotionValue;
        public byte ResponseNumber;
        public string ResponseText;
        public string ActorNotes;
        public Trdt(Reader r, int dataSize) {
            EmotionType = r.ReadUInt32();
            EmotionValue = r.ReadInt32();
            r.Skip(4); // Unused
            ResponseNumber = r.ReadByte();
            r.Skip(3); // Unused
        }
        public object NAM1(Reader r, int dataSize) => ResponseText = r.ReadFUString(dataSize);
        public object NAM2(Reader r, int dataSize) => ActorNotes = r.ReadFUString(dataSize);
    }

    public Data DATA; // Info data
    public RefX<QUSTRecord> QSTI; // Quest
    public RefX<DIALRecord> TPIC; // Topic
    public List<RefX<DIALRecord>> NAMEs = []; // Topics
    public List<Trdt> TRDTs = []; // Responses
    public List<SCPTRecord.Ctda> CTDAs = []; // Conditions
    public List<RefX<DIALRecord>> TCLTs = []; // Choices
    public List<RefX<DIALRecord>> TCLFs = []; // Link From Topics
    public SCPTRecord.Schr SCHR; // Script Data
    public byte[] SCDA; // Compiled Script
    public string SCTX; // Script Source
    public List<RefX<Record>> SCROs = []; // Global variable reference
    public Ref<INFO4Record>? PNAM; // Previous INFO ID

    protected override HashSet<FieldType> DF4 => [FieldType.NAME, FieldType.CTDA, FieldType.CTDT, FieldType.TRDT, FieldType.NAM1, FieldType.NAM2, FieldType.TCLT, FieldType.TCLF, FieldType.SCRO];
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.DATA => DATA = new Data(r, dataSize),
        FieldType.QSTI => QSTI = new RefX<QUSTRecord>(r, dataSize),
        FieldType.TPIC => TPIC = new RefX<DIALRecord>(r, dataSize),
        FieldType.NAME => NAMEs.AddX(new RefX<DIALRecord>(r, dataSize)),
        FieldType.TRDT => TRDTs.AddX(new Trdt(r, dataSize)),
        FieldType.NAM1 => TRDTs.Last().NAM1(r, dataSize),
        FieldType.NAM2 => TRDTs.Last().NAM2(r, dataSize),
        FieldType.CTDA or FieldType.CTDT => CTDAs.AddX(new SCPTRecord.Ctda(r, dataSize)),
        FieldType.TCLT => TCLTs.AddX(new RefX<DIALRecord>(r, dataSize)),
        FieldType.TCLF => TCLFs.AddX(new RefX<DIALRecord>(r, dataSize)),
        FieldType.SCHR or FieldType.SCHD => SCHR = new SCPTRecord.Schr(r, dataSize),
        FieldType.SCDA => SCDA = r.ReadBytes(dataSize),
        FieldType.SCTX => SCTX = r.ReadFUString(dataSize),
        FieldType.SCRO => SCROs.AddX(new RefX<Record>(r, dataSize)),
        FieldType.PNAM => PNAM = new Ref<INFO4Record>(r, dataSize),
        _ => Empty,
    };
}

/// <summary>
/// INGR.Ingredient - 34500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/INGR">
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/INGR"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/INGR"/>
public class INGRRecord : Record, IHaveMODL {
    // TES3
    public struct Irdt(Reader r, int dataSize) {
        public float Weight = r.ReadSingle();
        public int Value = r.ReadInt32();
        public int[] EffectId = [r.ReadInt32(), r.ReadInt32(), r.ReadInt32(), r.ReadInt32()]; // 0 or -1 means no effect
        public int[] SkillId = [r.ReadInt32(), r.ReadInt32(), r.ReadInt32(), r.ReadInt32()]; // only for Skill related effects, 0 or -1 otherwise
        public int[] AttributeId = [r.ReadInt32(), r.ReadInt32(), r.ReadInt32(), r.ReadInt32()]; // only for Attribute related effects, 0 or -1 otherwise
    }

    // TES4
    public class Data(Reader r, int dataSize) {
        public float Weight = r.ReadSingle();
        public int Value;
        public uint Flags;
        public object ENIT(Reader r, int dataSize) { var z = Value = r.ReadInt32(); Flags = r.ReadUInt32(); return z; }
    }

    public Modl MODL { get; set; } // Model Name
    public string FULL; // Item Name
    public Irdt IRDT; // Ingrediant Data // TES3
    public Data DATA; // Ingrediant Data // TES4
    public RefX<SCPTRecord> SCRI; // Script Name
    // TES4
    public List<ENCHRecord.Efit> EFITs = []; // Effect Data
    public List<ENCHRecord.Scit> SCITs = []; // Script effect data

    protected override HashSet<FieldType> DF4 => [FieldType.FULL, FieldType.EFID, FieldType.EFIT];
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadFUString(dataSize),
        FieldType.MODL => MODL = new Modl(r, dataSize),
        FieldType.MODB => MODL.MODB(r, dataSize),
        FieldType.MODT => MODL.MODT(r, dataSize),
        FieldType.ICON or FieldType.ITEX => MODL.ICON(r, dataSize),
        FieldType.FULL => SCITs.Count == 0 ? FULL = r.ReadFUString(dataSize) : SCITs.Last().FULL(r, dataSize),
        FieldType.FNAM => FULL = r.ReadFUString(dataSize),
        FieldType.DATA => DATA = new Data(r, dataSize),
        FieldType.IRDT => IRDT = new Irdt(r, dataSize),
        FieldType.SCRI => SCRI = new RefX<SCPTRecord>(r, dataSize),
        //
        FieldType.ENIT => DATA.ENIT(r, dataSize),
        FieldType.EFID => r.Skip(dataSize),
        FieldType.EFIT => EFITs.AddX(new ENCHRecord.Efit(r, dataSize)),
        FieldType.SCIT => SCITs.AddX(new ENCHRecord.Scit(r, dataSize)),
        _ => Empty,
    };
}

/// <summary>
/// KEYM.Key - 04500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/KEYM"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/KEYM"/>
public class KEYMRecord : Record, IHaveMODL {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Data {
        public static (string, int) Struct = ("<if", 8);
        public int Value;
        public float Weight;
    }

    public Modl MODL { get; set; } // Model
    public string FULL; // Item Name
    public RefX<SCPTRecord> SCRI; // Script (optional)
    public Data DATA; // Type of soul contained in the gem

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.MODL => MODL = new Modl(r, dataSize),
        FieldType.MODB => MODL.MODB(r, dataSize),
        FieldType.MODT => MODL.MODT(r, dataSize),
        FieldType.ICON => MODL.ICON(r, dataSize),
        FieldType.FULL => FULL = r.ReadFUString(dataSize),
        FieldType.SCRI => SCRI = new RefX<SCPTRecord>(r, dataSize),
        FieldType.DATA => DATA = r.ReadS<Data>(dataSize),
        _ => false,
    };
}

/// <summary>
/// KYWD.Keyword - 00500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/KYWD"/>
public class KYWDRecord : Record {
    public ByteColor4 CNAM; // Used to identify keywords in the editor.

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.CNAM => CNAM = r.ReadS<ByteColor4>(dataSize),
        _ => false,
    };
}

/// <summary>
/// LAND.Land - 34500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/LAND">
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/LAND"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/LAND"/>
public class LANDRecord : Record {
    public struct Vnml(Reader r, int dataSize) {
        public Byte3[] Vertexs = r.ReadPArray<Byte3>("3B", dataSize / 3); // XYZ 8 bit floats
    }

    public struct Vhgt {
        public float ReferenceHeight; // A height offset for the entire cell. Decreasing this value will shift the entire cell land down.
        public sbyte[] HeightData; // HeightData
        public Vhgt(Reader r, int dataSize) {
            ReferenceHeight = r.ReadSingle();
            var count = dataSize - 4 - 3;
            HeightData = r.ReadPArray<sbyte>("B", count);
            r.Skip(3); // Unused
        }
    }

    public struct Vclr(Reader r, int dataSize) {
        public ByteColor3[] Colors = r.ReadSArray<ByteColor3>(dataSize / 24); // 24-bit RGB
    }

    public struct Vtex {
        public ushort[] TextureIndicesT3;
        public uint[] TextureIndicesT4;
        public Vtex(Reader r, int dataSize) {
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
    public struct Cord {
        public override readonly string ToString() => $"{CellX},{CellY}";
        public static (string, int) Struct = ("<2i", 8);
        public int CellX;
        public int CellY;
    }

    public struct Wnam {
        // Low-LOD heightmap (signed chars)
        public Wnam(Reader r, int dataSize) {
            r.Skip(dataSize);
            //var heightCount = dataSize;
            //for (var i = 0; i < heightCount; i++) { var height = r.ReadByte(); }
        }
    }

    // TES4
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Btxt {
        public static (string, int) Struct = ("<I2Bh", 8);
        public uint Texture;
        public byte Quadrant;
        public byte Pad01;
        public short Layer;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Vtxt {
        public static (string, int) Struct = ("<2Hf", 8);
        public ushort Position;
        public ushort Pad01;
        public float Opacity;
    }

    public class Atxt {
        public Btxt ATXT;
        public Vtxt[] VTXTs;
    }

    public override string ToString() => $"LAND: {INTV}";
    public int DATA; // Unknown (default of 0x09) Changing this value makes the land 'disappear' in the editor.
                     // A RGB color map 65x65 pixels in size representing the land normal vectors.
                     // The signed value of the 'color' represents the vector's component. Blue
                     // is vertical(Z), Red the X direction and Green the Y direction.Note that
                     // the y-direction of the data is from the bottom up.
    public Vnml VNML;
    public Vhgt VHGT; // Height data
    public Vnml? VCLR; // Vertex color array, looks like another RBG image 65x65 pixels in size. (Optional)
    public Vtex? VTEX; // A 16x16 array of short texture indices. (Optional)
                       // TES3
    public Cord INTV; // The cell coordinates of the cell
    public Wnam WNAM; // Unknown byte data.
    // TES4
    public Btxt[] BTXTs = new Btxt[4]; // Base Layer
    public Atxt[] ATXTs; // Alpha Layer
    Atxt _lastATXT;
    // Grid
    public Int3 GridId; // => new Int3(INTV.CellX, INTV.CellY, 0);

    protected override HashSet<FieldType> DF4 => [FieldType.BTXT, FieldType.ATXT, FieldType.VTXT];
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.DATA => DATA = r.ReadInt32(),
        FieldType.VNML => VNML = new Vnml(r, dataSize),
        FieldType.VHGT => VHGT = new Vhgt(r, dataSize),
        FieldType.VCLR => VCLR = new Vnml(r, dataSize),
        FieldType.VTEX => VTEX = new Vtex(r, dataSize),
        // TES3
        FieldType.INTV => INTV = r.ReadS<Cord>(dataSize),
        FieldType.WNAM => WNAM = new Wnam(r, dataSize),
        // TES4
        FieldType.BTXT => this.Then(r.ReadS<Btxt>(dataSize), v => BTXTs[v.Quadrant] = v),
        FieldType.ATXT => (z: ATXTs ??= new Atxt[4], this.Then(r.ReadS<Btxt>(dataSize), v => ATXTs[v.Quadrant] = _lastATXT = new Atxt { ATXT = v })).z,
        FieldType.VTXT => _lastATXT.VTXTs = r.ReadSArray<Vtxt>(dataSize >> 3),
        _ => Empty,
    };
}

/// <summary>
/// LCRT.Location Reference Type - 00500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/LCRT"/>
public class LCRTRecord : Record {
    public ByteColor4 CNAM; // RGB Hex color code, last byte always 0x00

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.CNAM => CNAM = r.ReadS<ByteColor4>(dataSize),
        _ => false,
    };
}

/// <summary>
/// LEVC.Leveled Creature - 30000
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/LEVC">
public class LEVCRecord : Record {
    public int DATA; // List data - 1 = Calc from all levels <= PC level
    public byte NNAM; // Chance None?
    public int INDX; // Number of items in list
    public List<string> CNAMs = []; // ID string of list item
    public List<short> INTVs = []; // PC level for previous CNAM
                                   // The CNAM/INTV can occur many times in pairs

    protected override HashSet<FieldType> DF3 => [FieldType.CNAM, FieldType.INTV];
    public override object ReadField(Reader r, FieldType type, int dataSize) => r.Format == TES3
        ? type switch {
            FieldType.NAME => EDID = r.ReadFUString(dataSize),
            FieldType.DATA => DATA = r.ReadInt32(),
            FieldType.NNAM => NNAM = r.ReadByte(),
            FieldType.INDX => INDX = r.ReadInt32(),
            FieldType.CNAM => CNAMs.AddX(r.ReadFUString(dataSize)),
            FieldType.INTV => INTVs.AddX(r.ReadInt16()),
            _ => Empty,
        }
        : Empty;
}

/// <summary>
/// LEVI.Leveled item - 30000
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/LEVI">
public class LEVIRecord : Record {
    public int DATA; // List data - 1 = Calc from all levels <= PC level, 2 = Calc for each item
    public byte NNAM; // Chance None?
    public int INDX; // Number of items in list
    public List<string> INAMs = []; // ID string of list item
    public List<short> INTVs = []; // PC level for previous INAM
                                   // The CNAM/INTV can occur many times in pairs

    protected override HashSet<FieldType> DF3 => [FieldType.INAM, FieldType.INTV];
    public override object ReadField(Reader r, FieldType type, int dataSize) => r.Format == TES3
        ? type switch {
            FieldType.NAME => EDID = r.ReadFUString(dataSize),
            FieldType.DATA => DATA = r.ReadInt32(),
            FieldType.NNAM => NNAM = r.ReadByte(),
            FieldType.INDX => INDX = r.ReadInt32(),
            FieldType.INAM => INAMs.AddX(r.ReadFUString(dataSize)),
            FieldType.INTV => INTVs.AddX(r.ReadInt16()),
            _ => Empty,
        }
        : Empty;
}

/// <summary>
/// LIGH.Light - 34500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/GMST">
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/GMST"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/GMST"/>
/// <see cref="https://starfieldwiki.net/wiki/Starfield_Mod:Mod_File_Format/GMST">
public class LIGHRecord : Record, IHaveMODL {
    public struct Data {
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
        public Data(Reader r, int dataSize) {
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

    public Modl MODL { get; set; } // Model
    public string FULL; // Item Name (optional)
    public Data DATA; // Light Data
    public string SCPT; // Script Name (optional)??
    public RefX<SCPTRecord>? SCRI; // Script FormId (optional)
    public float FNAM; // Fade Value
    public RefX<SOUNRecord> SNAM; // Sound FormId (optional)

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadFUString(dataSize),
        FieldType.FULL => FULL = r.ReadFUString(dataSize),
        FieldType.FNAM => r.Format != TES3 ? FNAM = r.ReadSingle() : FULL = r.ReadFUString(dataSize),
        FieldType.DATA or FieldType.LHDT => DATA = new Data(r, dataSize),
        FieldType.SCPT => SCPT = r.ReadFUString(dataSize),
        FieldType.SCRI => SCRI = new RefX<SCPTRecord>(r, dataSize),
        FieldType.MODL => MODL = new Modl(r, dataSize),
        FieldType.MODB => MODL.MODB(r, dataSize),
        FieldType.MODT => MODL.MODT(r, dataSize),
        FieldType.ICON or FieldType.ITEX => MODL.ICON(r, dataSize),
        FieldType.SNAM => SNAM = new RefX<SOUNRecord>(r, dataSize),
        _ => Empty,
    };
}

/// <summary>
/// LOCK.Lock - 34500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/GMST">
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/GMST"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/GMST"/>
/// <see cref="https://starfieldwiki.net/wiki/Starfield_Mod:Mod_File_Format/GMST">
public class LOCKRecord : Record, IHaveMODL {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Lkdt {
        public static (string, int) Struct = ("<fifi", 16);
        public float Weight;
        public int Value;
        public float Quality;
        public int Uses;
    }

    public Modl MODL { get; set; } // Model Name
    public string FNAM; // Item Name
    public Lkdt LKDT; // Lock Data
    public RefX<SCPTRecord> SCRI; // Script Name

    public override object ReadField(Reader r, FieldType type, int dataSize) => r.Format == TES3
        ? type switch {
            FieldType.NAME => EDID = r.ReadFUString(dataSize),
            FieldType.MODL => MODL = new Modl(r, dataSize),
            FieldType.ITEX => MODL.ICON(r, dataSize),
            FieldType.FNAM => FNAM = r.ReadFUString(dataSize),
            FieldType.LKDT => LKDT = r.ReadS<Lkdt>(dataSize),
            FieldType.SCRI => SCRI = new RefX<SCPTRecord>(r, dataSize),
            _ => Empty,
        }
        : Empty;
}

/// <summary>
/// LSCR.Load Screen - 04500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/GMST">
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/GMST"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/GMST"/>
/// <see cref="https://starfieldwiki.net/wiki/Starfield_Mod:Mod_File_Format/GMST">
public class LSCRRecord : Record {
    public struct Lnam(Reader r, int dataSize) {
        public Ref<Record> Direct = new(r.ReadUInt32());
        public Ref<WRLDRecord> IndirectWorld = new(r.ReadUInt32());
        public short IndirectGridX = r.ReadInt16();
        public short IndirectGridY = r.ReadInt16();
    }

    public string ICON; // Icon
    public string DESC; // Description
    public List<Lnam> LNAMs; // LoadForm

    protected override HashSet<FieldType> DF4 => [FieldType.LNAM];
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.ICON => ICON = r.ReadFUString(dataSize),
        FieldType.DESC => DESC = r.ReadFUString(dataSize),
        FieldType.LNAM => (LNAMs ??= []).AddX(new Lnam(r, dataSize)),
        _ => Empty,
    };
}

/// <summary>
/// LTEX.Land Texture - 34500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/GMST">
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/GMST"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/GMST"/>
/// <see cref="https://starfieldwiki.net/wiki/Starfield_Mod:Mod_File_Format/GMST">
public class LTEXRecord : Record {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Hnam {
        public static (string, int) Struct = ("<3B", 3);
        public byte MaterialType;
        public byte Friction;
        public byte Restitution;
    }

    public string ICON; // Texture
    // TES3
    public long INTV;
    // TES4
    public Hnam HNAM; // Havok data
    public byte SNAM; // Texture specular exponent
    public List<RefX<GRASRecord>> GNAMs = []; // Potential grass

    protected override HashSet<FieldType> DF4 => [FieldType.GNAM];
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadFUString(dataSize),
        FieldType.INTV => INTV = r.ReadINTV(dataSize),
        FieldType.ICON or FieldType.DATA => ICON = r.ReadFUString(dataSize),
        // TES4
        FieldType.HNAM => HNAM = r.ReadS<Hnam>(dataSize),
        FieldType.SNAM => SNAM = r.ReadByte(),
        FieldType.GNAM => GNAMs.AddX(new RefX<GRASRecord>(r, dataSize)),
        _ => Empty,
    };
}

/// <summary>
/// LVLC.Leveled Creature - 04000
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/GMST">
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/GMST"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/GMST"/>
/// <see cref="https://starfieldwiki.net/wiki/Starfield_Mod:Mod_File_Format/GMST">
public class LVLCRecord : Record {
    public byte LVLD; // Chance
    public byte LVLF; // Flags - 0x01 = Calculate from all levels <= player's level, 0x02 = Calculate for each item in count
    public RefX<SCPTRecord> SCRI; // Script (optional)
    public RefX<CREA4Record> TNAM; // Creature Template (optional)
    public List<LVLIRecord.Lvlo> LVLOs = [];

    protected override HashSet<FieldType> DF4 => [FieldType.LVLO];
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.LVLD => LVLD = r.ReadByte(),
        FieldType.LVLF => LVLF = r.ReadByte(),
        FieldType.SCRI => SCRI = new RefX<SCPTRecord>(r, dataSize),
        FieldType.TNAM => TNAM = new RefX<CREA4Record>(r, dataSize),
        FieldType.LVLO => LVLOs.AddX(new LVLIRecord.Lvlo(r, dataSize)),
        _ => Empty,
    };
}

/// <summary>
/// LVLI.Leveled Item - 04000
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/GMST">
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/GMST"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/GMST"/>
/// <see cref="https://starfieldwiki.net/wiki/Starfield_Mod:Mod_File_Format/GMST">
public class LVLIRecord : Record {
    public struct Lvlo {
        public short Level;
        public RefX<Record> ItemFormId;
        public int Count;
        public Lvlo(Reader r, int dataSize) {
            Level = r.ReadInt16();
            r.Skip(2); // Unused
            ItemFormId = new RefX<Record>(r.ReadUInt32());
            if (dataSize == 12) {
                Count = r.ReadInt16();
                r.Skip(2); // Unused
            }
            else Count = 0;
        }
    }

    public byte LVLD; // Chance
    public byte LVLF; // Flags - 0x01 = Calculate from all levels <= player's level, 0x02 = Calculate for each item in count
    public byte? DATA; // Data (optional)
    public List<Lvlo> LVLOs = [];

    protected override HashSet<FieldType> DF4 => [FieldType.LVLO];
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.LVLD => LVLD = r.ReadByte(),
        FieldType.LVLF => LVLF = r.ReadByte(),
        FieldType.DATA => DATA = r.ReadByte(),
        FieldType.LVLO => LVLOs.AddX(new Lvlo(r, dataSize)),
        _ => Empty,
    };
}

/// <summary>
/// LVSP.Leveled Spell - 04500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/LVSP"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/LVSP"/>
public class LVSPRecord : Record {
    public byte LVLD; // Chance
    public byte LVLF; // Flags
    public List<LVLIRecord.Lvlo> LVLOs = []; // Number of items in list

    protected override HashSet<FieldType> DF4 => [FieldType.LVLO];
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.LVLD => LVLD = r.ReadByte(),
        FieldType.LVLF => LVLF = r.ReadByte(),
        FieldType.LVLO => LVLOs.AddX(new LVLIRecord.Lvlo(r, dataSize)),
        _ => Empty,
    };
}

/// <summary>
/// MGEF.Magic Effect - 34000
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/GMST">
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/GMST"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/GMST"/>
/// <see cref="https://starfieldwiki.net/wiki/Starfield_Mod:Mod_File_Format/GMST">
public class MGEFRecord : Record {
    // TES3
    public struct Medt(Reader r, int dataSize) {
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

    public class Data {
        public uint Flags;
        public float BaseCost;
        public int AssocItem;
        public int MagicSchool;
        public int ResistValue;
        public uint CounterEffectCount; // Must be updated automatically when ESCE length changes!
        public RefX<LIGHRecord> Light;
        public float ProjectileSpeed;
        public RefX<EFSHRecord> EffectShader;
        public RefX<EFSHRecord> EnchantEffect;
        public RefX<SOUNRecord> CastingSound;
        public RefX<SOUNRecord> BoltSound;
        public RefX<SOUNRecord> HitSound;
        public RefX<SOUNRecord> AreaSound;
        public float ConstantEffectEnchantmentFactor;
        public float ConstantEffectBarterFactor;
        public Data(Reader r, int dataSize) {
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
            Light = new RefX<LIGHRecord>(r.ReadUInt32());
            ProjectileSpeed = r.ReadSingle();
            EffectShader = new RefX<EFSHRecord>(r.ReadUInt32());
            if (dataSize == 36) return;
            EnchantEffect = new RefX<EFSHRecord>(r.ReadUInt32());
            CastingSound = new RefX<SOUNRecord>(r.ReadUInt32());
            BoltSound = new RefX<SOUNRecord>(r.ReadUInt32());
            HitSound = new RefX<SOUNRecord>(r.ReadUInt32());
            AreaSound = new RefX<SOUNRecord>(r.ReadUInt32());
            ConstantEffectEnchantmentFactor = r.ReadSingle();
            ConstantEffectBarterFactor = r.ReadSingle();
        }
    }

    public override string ToString() => $"MGEF: {INDX}:{EDID}";
    public string DESC; // Description
    // TES3
    public long INDX; // The Effect ID (0 to 137)
    public Medt MEDT; // Effect Data
    public string ICON; // Effect Icon
    public string PTEX; // Particle texture
    public string CVFX; // Casting visual
    public string BVFX; // Bolt visual
    public string HVFX; // Hit visual
    public string AVFX; // Area visual
    public string CSND; // Cast sound (optional)
    public string BSND; // Bolt sound (optional)
    public string HSND; // Hit sound (optional)
    public string ASND; // Area sound (optional)
    // TES4
    public string FULL;
    public Modl MODL;
    public Data DATA;
    public string[] ESCEs;

    public override object ReadField(Reader r, FieldType type, int dataSize) => r.Format == TES3
        ? type switch {
            FieldType.INDX => INDX = r.ReadINTV(dataSize),
            FieldType.MEDT => MEDT = new Medt(r, dataSize),
            FieldType.ITEX => ICON = r.ReadFUString(dataSize),
            FieldType.PTEX => PTEX = r.ReadFUString(dataSize),
            FieldType.CVFX => CVFX = r.ReadFUString(dataSize),
            FieldType.BVFX => BVFX = r.ReadFUString(dataSize),
            FieldType.HVFX => HVFX = r.ReadFUString(dataSize),
            FieldType.AVFX => AVFX = r.ReadFUString(dataSize),
            FieldType.DESC => DESC = r.ReadFUString(dataSize),
            FieldType.CSND => CSND = r.ReadFUString(dataSize),
            FieldType.BSND => BSND = r.ReadFUString(dataSize),
            FieldType.HSND => HSND = r.ReadFUString(dataSize),
            FieldType.ASND => ASND = r.ReadFUString(dataSize),
            _ => Empty,
        }
        : type switch {
            FieldType.EDID => EDID = r.ReadFUString(dataSize),
            FieldType.FULL => FULL = r.ReadFUString(dataSize),
            FieldType.DESC => DESC = r.ReadFUString(dataSize),
            FieldType.MODL => MODL = new Modl(r, dataSize),
            FieldType.MODB => MODL.MODB(r, dataSize),
            FieldType.ICON => MODL.ICON(r, dataSize),
            FieldType.DATA => DATA = new Data(r, dataSize),
            FieldType.ESCE => ESCEs = r.ReadFArray(z => r.ReadFUString(4), dataSize >> 2),
            _ => Empty,
        };
}

/// <summary>
/// MICN.Menu Icon - 00500
/// </summary>
/// <see cref="https://tes5edit.github.io/fopdoc/Fallout3/Records/MICN.html"/>
public class MICNRecord : Record {
    public string ICON; // Large icon filename
    public string MICO; // Small icon filename

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.ICON => ICON = r.ReadFUString(dataSize),
        FieldType.MICO => MICO = r.ReadFUString(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// MISC.Misc Item - 34500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/GMST">
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/GMST"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/GMST"/>
/// <see cref="https://starfieldwiki.net/wiki/Starfield_Mod:Mod_File_Format/GMST">
public class MISCRecord : Record, IHaveMODL {
    public struct Data {
        public float Weight;
        public uint Value;
        public uint Unknown;
        public Data(Reader r, int dataSize) {
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

    public Modl MODL { get; set; } // Model
    public string FULL; // Item Name
    public Data DATA; // Misc Item Data
    public RefX<SCPTRecord> SCRI; // Script FormID (optional)
    // TES3
    public RefX<ENCHRecord> ENAM; // enchantment ID

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadFUString(dataSize),
        FieldType.MODL => MODL = new Modl(r, dataSize),
        FieldType.MODB => MODL.MODB(r, dataSize),
        FieldType.MODT => MODL.MODT(r, dataSize),
        FieldType.ICON or FieldType.ITEX => MODL.ICON(r, dataSize),
        FieldType.FULL or FieldType.FNAM => FULL = r.ReadFUString(dataSize),
        FieldType.DATA or FieldType.MCDT => DATA = new Data(r, dataSize),
        FieldType.ENAM => ENAM = new RefX<ENCHRecord>(r, dataSize),
        FieldType.SCRI => SCRI = new RefX<SCPTRecord>(r, dataSize),
        _ => Empty,
    };
}

/// <summary>
/// NPC_.Non-Player Character - 34500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/NPC_">
public class NPC_3Record : CREA3Record {
    [Flags]
    public enum NPC_3Flags : uint {
        Female = 0x0001,
        Essential = 0x0002,
        Respawn = 0x0004,
        None_ = 0x0008,
        Autocalc = 0x0010,
        BloodSkel = 0x0400,
        BloodMetal = 0x0800,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Dodt {
        public static (string, int) Struct = ("<6f", 24);
        public float XPos;
        public float YPos;
        public float ZPos;
        public float XRot;
        public float YRot;
        public float ZRot;
    }

    public class DodtX {
        public Dodt DODT; // Cell Travel Destination
        public string DNAM; // Cell name for previous DODT, if interior
    }

    public string RNAM; // Race Name
    public string ANAM; // Faction name
    public string BNAM; // Head model
    public string KNAM; // Hair model
    public List<DodtX> DODTs = []; // Cell Travel Destination

    protected override HashSet<FieldType> DF3 => [FieldType.NPCO, FieldType.NPCS, FieldType.DODT, FieldType.DNAM, FieldType.AI_A, FieldType.AI_E, FieldType.AI_F, FieldType.AI_T, FieldType.AI_W, FieldType.CNDT];
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.NAME => EDID = r.ReadFUString(dataSize),
        FieldType.FNAM => FULL = r.ReadFUString(dataSize),
        FieldType.MODL => MODL = new Modl(r, dataSize),
        FieldType.MODB => MODL.MODB(r, dataSize),
        FieldType.RNAM => RNAM = r.ReadFUString(dataSize),
        FieldType.ANAM => ANAM = r.ReadFUString(dataSize),
        FieldType.BNAM => BNAM = r.ReadFUString(dataSize),
        FieldType.CNAM => CNAM = r.ReadFUString(dataSize),
        FieldType.KNAM => KNAM = r.ReadFUString(dataSize),
        FieldType.NPDT => NPDT = dataSize switch { 52 => r.ReadS<Npdt52>(dataSize), 12 => r.ReadS<Npdt12>(dataSize) },
        FieldType.FLAG => FLAG = r.ReadInt32(),
        FieldType.NPCO => NPCOs.AddX(new CntoX<Record>(r, dataSize)),
        FieldType.NPCS => NPCSs.AddX(r.ReadFAString(dataSize)),
        FieldType.AIDT => AIDT = r.ReadS<Aidt>(dataSize),
        FieldType.AI_A or FieldType.AI_E or FieldType.AI_F or FieldType.AI_T or FieldType.AI_W => AIs.AddX(new Ai(r, dataSize, type)),
        FieldType.CNDT => AIs.Last().CNDT = r.ReadFUString(dataSize),
        FieldType.DODT => DODTs.AddX(new DodtX { DODT = r.ReadS<Dodt>(dataSize) }),
        FieldType.DNAM => DODTs.Last().DNAM = r.ReadFUString(dataSize),
        FieldType.XSCL => XSCL = r.ReadSingle(),
        FieldType.SCRI => SCRI = new RefX<SCPTRecord>(r, dataSize),
        _ => Empty,
    };
}

/// <summary>
/// NPC_.Non-Player Character - 34500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/NPC_"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/NPC_"/>
/// <see cref="https://starfieldwiki.net/wiki/Starfield_Mod:Mod_File_Format/NPC_">
public class NPC_4Record : CREA4Record {
    [Flags]
    public enum NPC_4Flags : uint {
        Female = 0x0001,
        Essential = 0x0002,
        Respawn = 0x0008,
        Autocalc = 0x0010,
        PCLevelOffset = 0x000080,
        NoLowLevelProcessing = 0x000200,
        NoRumors = 0x002000,
        Summonable = 0x004000,
        NoPersuasion = 0x008000,
        CanCorpseCheck = 0x100000,
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Data {
        public static (string, int) Struct = ("<21si8s", 33);
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 21)] public byte[] Skills;  // Skills
        public int Health; // Health. (Fatigue and Base Spell Points are stored in ACBS.)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] Attributes;  // Skills
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Hclr {
        public static (string, int) Struct = ("<4B", 4);
        public byte Red;    // Red
        public byte Green;  // Green
        public byte Blue;   // Blue
        public byte Custom; // ?Custom color flag?
    }

    public new Ref<RACERecord> RNAM; // Race
    public Aidt AIDT; // AI Data
    public RefX<CLASRecord> CNAM; // Class
    public Data DATA; // Stats
    public RefX<HAIRRecord> HNAM; // Hair
    public float LNAM; // Hair length
    public RefX<EYESRecord> ENAM; // Eyes
    public Hclr HCLR; // Hair color
    public byte[] FGGS; // FaceGen Geometry-Symmetric
    public byte[] FGGA; // FaceGen Geometry-Asymmetric
    public byte[] FGTS; // FaceGen Texture-Symmetic
    public ushort FNAM; // FaceGen Texture-Symmetic

    protected override HashSet<FieldType> DF4 => [FieldType.SNAM, FieldType.SPLO, FieldType.CNTO, FieldType.PKID];
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.FULL => FULL = r.ReadFUString(dataSize),
        FieldType.MODL => MODL = new Modl(r, dataSize),
        FieldType.MODB => MODL.MODB(r, dataSize),
        FieldType.ACBS => ACBS = r.ReadS<Acbs>(dataSize),
        FieldType.SNAM => SNAMs.AddX(new RefB<FACTRecord>(r, dataSize)),
        FieldType.INAM => INAM = new Ref<LVLIRecord>(r, dataSize),
        FieldType.RNAM => RNAM = new Ref<RACERecord>(r, dataSize),
        FieldType.SPLO => SPLOs.AddX(r.ReadFUString(dataSize)),
        FieldType.SCRI => SCRI = new Ref<SCPTRecord>(r, dataSize),
        FieldType.CNTO => CNTOs.AddX(new Cnto<Record>(r, dataSize)),
        FieldType.AIDT => AIDT = r.ReadS<Aidt>(dataSize),
        FieldType.PKID => PKIDs.AddX(new RefS<PACKRecord>(r, dataSize)),
        FieldType.CNAM => CNAM = new RefX<CLASRecord>(r, dataSize),
        FieldType.DATA => DATA = r.ReadS<Data>(dataSize),
        FieldType.HNAM => HNAM = new RefX<HAIRRecord>(r, dataSize),
        FieldType.LNAM => LNAM = r.ReadSingle(),
        FieldType.ENAM => ENAM = new RefX<EYESRecord>(r, dataSize),
        FieldType.HCLR => HCLR = r.ReadS<Hclr>(dataSize),
        FieldType.ZNAM => ZNAM = new Ref<CSTYRecord>(r, dataSize),
        FieldType.FGGS => r.ReadBytes(dataSize),
        FieldType.FGGA => r.ReadBytes(dataSize),
        FieldType.FGTS => r.ReadBytes(dataSize),
        FieldType.FNAM => FNAM = r.ReadUInt16(),
        FieldType.KFFZ => r.Skip(dataSize), //TODO
        _ => Empty,
    };
}

/// <summary>
/// OTFT.Outfit - 00500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/OTFT"/>
public class OTFTRecord : Record {
    public Ref<Record>[] INAM; // Inventory list - Array of ARMO or LVLI

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.INAM => INAM = r.ReadFArray(z => new Ref<Record>(r, 4), dataSize >> 2),
        _ => false,
    };
}

/// <summary>
/// PACK.AI Package - 04500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/PACK"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/PACK"/>
public class PACKRecord : Record {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Pkdt {
        public static Dictionary<int, string> Struct = new() { [4] = "<I", [8] = "<I4B", [16] = "<I4BI" };
        public uint Flags;
        public byte PackageType;
        public byte InterruptOverride; // Only observed values are 0 (None) and 4 (Combat)
        public byte PreferredSpeed; // This value is only relevant if the Preferred Speed Misc Flag is set (0x2000 above)
        public byte Unknown; // Possibly padding
        public uint InterruptFlags;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Psdt {
        public static (string, int) Struct = ("<3Bbi", 8);
        public byte Month;
        public byte DayOfWeek;
        public byte Date;
        public sbyte Time;
        public int Duration;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Pldt {
        public static (string, int) Struct = ("<iIi", 12);
        public int Type;
        public uint Target;
        public int Radius;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Ptdt {
        public static (string, int) Struct = ("<iIi", 12);
        public int Type;
        public uint Target;
        public int Count;
    }

    public Pkdt PKDT; // General
    public Pldt PLDT; // Location
    public Psdt PSDT; // Schedule
    public Ptdt PTDT; // Target
    public List<SCPTRecord.Ctda> CTDAs = []; // Conditions

    protected override HashSet<FieldType> DF4 => [FieldType.CTDA, FieldType.CTDT];
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.PKDT => PKDT = r.ReadS<Pkdt>(dataSize),
        FieldType.PLDT => PLDT = r.ReadS<Pldt>(dataSize),
        FieldType.PSDT => PSDT = r.ReadS<Psdt>(dataSize),
        FieldType.PTDT => PTDT = r.ReadS<Ptdt>(dataSize),
        FieldType.CTDA or FieldType.CTDT => CTDAs.AddX(new SCPTRecord.Ctda(r, dataSize)),
        _ => Empty,
    };
}

/// <summary>
/// PGRD.Path grid - 34000
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/GMST">
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/GMST"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/GMST"/>
/// <see cref="https://starfieldwiki.net/wiki/Starfield_Mod:Mod_File_Format/GMST">
public unsafe class PGRDRecord : Record {
    public struct Data {
        public int X;
        public int Y;
        public short Granularity;
        public short PointCount;
        public Data(Reader r, int dataSize) {
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

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Pgrp {
        public static (string, int) Struct = ("<3fB3s", 16);
        public Vector3 Point;
        public byte Connections;
        public fixed byte Unused[3];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Pgrr {
        public static (string, int) Struct = ("<2h", 4);
        public short StartPointId;
        public short EndPointId;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Pgri {
        public static (string, int) Struct = ("<hH3f", 16);
        public short PointId;
        public ushort Unused;
        public Vector3 ForeignNode;
    }

    public struct Pgrl(Reader r, int dataSize) {
        public RefX<REFRRecord> Reference = new(r.ReadUInt32());
        public short[] PointIds = r.ReadFArray(z => (z: r.ReadInt16(), r.Skip(2)).z, (dataSize - 4) >> 2); // 2:Unused (can merge back)
    }

    public Data DATA; // Number of nodes
    public Pgrp[] PGRPs;
    public byte[] PGRC;
    public byte[] PGAG;
    public Pgrr[] PGRRs; // Point-to-Point Connections
    public List<Pgrl> PGRLs; // Point-to-Reference Mappings
    public Pgri[] PGRIs; // Inter-Cell Connections

    protected override HashSet<FieldType> DF4 => [FieldType.PGRL];
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadFUString(dataSize),
        FieldType.DATA => DATA = new Data(r, dataSize),
        FieldType.PGRP => PGRPs = r.ReadSArray<Pgrp>(dataSize >> 4),
        FieldType.PGRC => PGRC = r.ReadBytes(dataSize),
        FieldType.PGAG => PGAG = r.ReadBytes(dataSize),
        FieldType.PGRR => (z: PGRRs = r.ReadSArray<Pgrr>(dataSize >> 2), r.Skip(dataSize % 4)).z,
        FieldType.PGRL => (PGRLs ??= []).AddX(new Pgrl(r, dataSize)),
        FieldType.PGRI => PGRIs = r.ReadSArray<Pgri>(dataSize >> 4),
        _ => Empty,
    };
}

/// <summary>
/// PROB.Probe - 30000
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/GMST">
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/GMST"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/GMST"/>
/// <see cref="https://starfieldwiki.net/wiki/Starfield_Mod:Mod_File_Format/GMST">
public class PROBRecord : Record, IHaveMODL {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Pbdt {
        public static (string, int) Struct = ("<fifi", 16);
        public float Weight;
        public int Value;
        public float Quality;
        public int Uses;
    }

    public Modl MODL { get; set; } // Model Name
    public string FNAM; // Item Name
    public Pbdt PBDT; // Probe Data
    public RefX<SCPTRecord> SCRI; // Script Name

    public override object ReadField(Reader r, FieldType type, int dataSize) => r.Format == TES3
        ? type switch {
            FieldType.NAME => EDID = r.ReadFUString(dataSize),
            FieldType.MODL => MODL = new Modl(r, dataSize),
            FieldType.ITEX => MODL.ICON(r, dataSize),
            FieldType.FNAM => FNAM = r.ReadFUString(dataSize),
            FieldType.PBDT => PBDT = r.ReadS<Pbdt>(dataSize),
            FieldType.SCRI => SCRI = new RefX<SCPTRecord>(r, dataSize),
            _ => Empty,
        }
        : Empty;
}

/// <summary>
/// QUST.Quest - 04500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/QUST">
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/QUST"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/QUST"/>
/// <see cref="https://starfieldwiki.net/wiki/Starfield_Mod:Mod_File_Format/QUST">
public class QUSTRecord : Record {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Data {
        public static (string, int) Struct = ("<2B", 2);
        public byte Flags;
        public byte Priority;
    }

    public string FULL; // Item Name
    public string ICON; // Icon
    public Data DATA; // Icon
    public RefX<SCPTRecord> SCRI; // Script Name
    public List<SCPTRecord.Schr> SCHRs = []; // Script Data //TODO Group?
    public List<SCPTRecord.Scda> SCDAs = []; // Compiled Script //TODO Group?
    public List<string> SCTXs = []; // Script Source //TODO Group?
    public List<RefX<Record>> SCROs = []; // Global variable reference

    protected override HashSet<FieldType> DF4 => [FieldType.CTDA, FieldType.INDX, FieldType.QSDT, FieldType.CNAM, FieldType.QSTA, FieldType.SCHR, FieldType.SCDA, FieldType.SCTX, FieldType.SCRO];
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.FULL => FULL = r.ReadFUString(dataSize),
        FieldType.ICON => ICON = r.ReadFUString(dataSize),
        FieldType.DATA => DATA = r.ReadS<Data>(dataSize),
        FieldType.SCRI => SCRI = new RefX<SCPTRecord>(r, dataSize),
        FieldType.CTDA => r.Skip(dataSize), //TODO multi
        FieldType.INDX => r.Skip(dataSize), //TODO multi
        FieldType.QSDT => r.Skip(dataSize), //TODO multi
        FieldType.CNAM => r.Skip(dataSize), //TODO multi
        FieldType.QSTA => r.Skip(dataSize), //TODO
        FieldType.SCHR => SCHRs.AddX(new SCPTRecord.Schr(r, dataSize)),
        FieldType.SCDA => SCDAs.AddX(new SCPTRecord.Scda(r, dataSize)),
        FieldType.SCTX => SCTXs.AddX(r.ReadFUString(dataSize)),
        FieldType.SCRO => SCROs.AddX(new RefX<Record>(r, dataSize)),
        _ => Empty,
    };
}

/// <summary>
/// RACE.Race_Creature type - 345S0
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/RACE">
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/RACE"/>
/// <see cref="https://tes5edit.github.io/fopdoc/Fallout3/Records/RACE.html"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/RACE"/>
public abstract class RACERecord : Record {
    public enum DataFlag : uint {
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
        OverlayHeadPartList = 0x04000000, // Only one can be active
        OverrideHeadPartList = 0x08000000, // Only one can be active
        CanPickupItems = 0x10000000,
        AllowMultipleMembraneShaders = 0x20000000,
        CanDualWield = 0x40000000,
        AvoidsRoads = 0x80000000,
    }

    public class Data {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SkillBoost(byte skillId, sbyte bonus) {
            public static (string, int) Struct = ("<Bb", 2);
            public byte SkillId = skillId;
            public sbyte Bonus = bonus;
        }

        public struct RaceStats {
            public float Height;
            public float Weight;
            // Attributes
            public byte Strength;
            public byte Intelligence;
            public byte Willpower;
            public byte Agility;
            public byte Speed;
            public byte Endurance;
            public byte Personality;
            public byte Luck;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Attrib_ {
            public static Dictionary<int, string> Struct = new() { [128 - 36] = "<7f3IfI5fIfI2f10f", [164 - 36] = "<7f3IfI5fIfI2f10f36x" };
            public float StartingHealth;
            public float StartingMagicka;
            public float StartingStamina;
            public float BaseCarryWeight;
            public float BaseMass;
            public float AccelerationRate;
            public float DecelerationRate;
            public uint Size; // lookup: 0=Small, 1=Medium, 2=Large, 3=Extra Large
            public Ref<Record> HeadBipedObject;
            public Ref<Record> HairBipedObject;
            public float InjuredHealthPct; // value: From 0 to 1
            public Ref<Record> ShieldBipedObject;
            public float HealthRegen;
            public float MagickaRegen;
            public float StaminaRegen;
            public float UnarmedDamage;
            public float UnarmedReach;
            public Ref<Record> BodyBipedObject;
            public float AimAngleTolerance;
            public uint Unknown;
            public float AngularAccelerationRate;
            public float AngularTolerance;
            public uint Flags;
            public uint MountDataOffsetX;
            public uint MountDataOffsetY;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)] public uint[] Unknowns;
        }

        public SkillBoost[] SkillBoosts; // Skill Boosts
        public RaceStats Male = new();
        public RaceStats Female = new();
        public DataFlag Flags;
        public Attrib_ Attrib;
        public Data(Reader r, int dataSize) {
            if (r.Format == TES3) {
                SkillBoosts = r.ReadFArray(z => new SkillBoost((byte)r.ReadInt32(), (sbyte)r.ReadInt32()), 7);
                Male.Strength = (byte)r.ReadInt32(); Female.Strength = (byte)r.ReadInt32();
                Male.Intelligence = (byte)r.ReadInt32(); Female.Intelligence = (byte)r.ReadInt32();
                Male.Willpower = (byte)r.ReadInt32(); Female.Willpower = (byte)r.ReadInt32();
                Male.Agility = (byte)r.ReadInt32(); Female.Agility = (byte)r.ReadInt32();
                Male.Speed = (byte)r.ReadInt32(); Female.Speed = (byte)r.ReadInt32();
                Male.Endurance = (byte)r.ReadInt32(); Female.Endurance = (byte)r.ReadInt32();
                Male.Personality = (byte)r.ReadInt32(); Female.Personality = (byte)r.ReadInt32();
                Male.Luck = (byte)r.ReadInt32(); Female.Luck = (byte)r.ReadInt32();
            }
            else {
                SkillBoosts = r.ReadSArray<SkillBoost>(7);
                r.ReadInt16(); // padding
            }
            Male.Height = r.ReadSingle(); Female.Height = r.ReadSingle();
            Male.Weight = r.ReadSingle(); Female.Weight = r.ReadSingle();
            Flags = (DataFlag)r.ReadUInt32();
            if (r.Format == TES5) Attrib = r.ReadS<Attrib_>(dataSize - 36);
        }
        public object ATTR(Reader r, int dataSize) {
            if (dataSize == 2) {
                Male.Strength = r.ReadByte();
                Female.Strength = r.ReadByte();
                return this;
            }
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

    public string FULL; // Race name
    public string DESC; // Race description
    public List<string> SPLOs = []; // NPCs: Special power/ability name
    public Data DATA; // RADT:DATA/ATTR: Race data/Base Attributes
}

public class RACE3Record : RACERecord {
    protected override HashSet<FieldType> DF3 => [FieldType.NPCS];
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.NAME => EDID = r.ReadFUString(dataSize),
        FieldType.FNAM => FULL = r.ReadFUString(dataSize),
        FieldType.RADT => DATA = new Data(r, dataSize),
        FieldType.NPCS => SPLOs.AddX(r.ReadFUString(dataSize)),
        FieldType.DESC => DESC = r.ReadFUString(dataSize),
        _ => Empty,
    };
}

public class RACE4Record : RACERecord {
    public enum FaceIndx : uint { Head = 0, Ear_Male, Ear_Female, Mouth, Teeth_Lower, Teeth_Upper, Tongue, Eye_Left, Eye_Right }
    public enum BodyIndx : uint { UpperBody = 0, LowerBody, Hand, Foot, Tail }

    public Ref2<RACERecord> VNAM; // Voice
    public Ref2<HAIRRecord> DNAM; // Default Hair
    public long CNAM; // Default Hair Color
    public float PNAM; // FaceGen - Main clamp
    public float UNAM; // FaceGen - Face clamp
    public byte[] XNAM; // Unknown
    public Modl[] FACEs = new Modl[8];
    public Modl[][] BODYs = [new Modl[5], new Modl[5]];
    public List<RefX<HAIRRecord>> HNAMs = [];
    public List<RefX<EYESRecord>> ENAMs = [];
    public byte[] FGGS; // FaceGen Geometry-Symmetric
    public byte[] FGGA; // FaceGen Geometry-Asymmetric
    public byte[] FGTS; // FaceGen Texture-Symmetric
    public byte[] SNAM; // Unknown
    // fallout
    public Ref<RACERecord> ONAM; // Older
    public Ref<RACERecord> YNAM; // Younger
    public Ref2<Record> VTCK; // Voices //TODO VTYPRecord
    int _index;
    Modl _last;
    byte _nam;
    byte _nam2;

    protected override HashSet<FieldType> DF4 => null;
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.FULL => FULL = r.ReadFUString(dataSize),
        FieldType.DESC => DESC = r.ReadFUString(dataSize),
        FieldType.DATA => DATA = new Data(r, dataSize),
        FieldType.SPLO => SPLOs.AddX(r.ReadFUString(dataSize)),
        FieldType.VNAM => VNAM = new Ref2<RACERecord>(r, dataSize),
        FieldType.DNAM => DNAM = new Ref2<HAIRRecord>(r, dataSize),
        FieldType.CNAM => CNAM = r.ReadINTV(dataSize),
        FieldType.PNAM => PNAM = r.ReadSingle(),
        FieldType.UNAM => UNAM = r.ReadSingle(),
        FieldType.XNAM => XNAM = r.ReadBytes(dataSize),
        FieldType.ATTR => DATA.ATTR(r, dataSize),
        // section: Face/Body Data
        FieldType.NAM0 => _nam = 0,
        FieldType.NAM1 => _nam = 1,
        FieldType.NAM2 => _nam = 2,
        FieldType.MNAM => _nam2 = 0,
        FieldType.FNAM => _nam2 = 1,
        FieldType.INDX => _index = (int)r.ReadUInt32(),
        FieldType.MODL => _last = _nam == 0 ? FACEs[_index] = new Modl(r, dataSize) : BODYs[_nam2][_index] = new Modl(r, dataSize),
        FieldType.MODB => _last.MODB(r, dataSize),
        FieldType.MODT => _last.MODT(r, dataSize),
        FieldType.MODD => _last.MODD(r, dataSize),
        FieldType.ICON => _last.ICON(r, dataSize),
        // section: end
        FieldType.HNAM => HNAMs.AddRangeX(r.ReadFArray(z => new RefX<HAIRRecord>(r, 4), dataSize >> 2)),
        FieldType.ENAM => ENAMs.AddRangeX(r.ReadFArray(z => new RefX<EYESRecord>(r, 4), dataSize >> 2)),
        FieldType.FGGS => FGGS = r.ReadBytes(dataSize),
        FieldType.FGGA => FGGA = r.ReadBytes(dataSize),
        FieldType.FGTS => FGTS = r.ReadBytes(dataSize),
        FieldType.SNAM => SNAM = r.ReadBytes(dataSize),
        // fallout
        FieldType.ONAM => ONAM = new Ref<RACERecord>(r, dataSize),
        FieldType.YNAM => YNAM = new Ref<RACERecord>(r, dataSize),
        FieldType.VTCK => VTCK = new Ref2<Record>(r, dataSize), //TODO VTYPRecord
        _ => Empty,
    };
}

public class RACE5Record : RACERecord {
    public class Body {
        public string ANAM;
        public object MODT;
    }

    byte _state;
    byte _state2;
    public uint SPCT; // Spell count
    public Ref<ARMORecord> WNAM; // Skin
    public ARMORecord.Bodt BODT; // Body template
    public uint KSIZ; // Keyword count
    public Ref<KYWDRecord>[] KWDA; // Keywords
    public Body[] Bodys = [new Body(), new Body()];

    protected override HashSet<FieldType> DF5 => null;
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.FULL => FULL = r.ReadFUString(dataSize),
        FieldType.DESC => DESC = r.ReadFUString(dataSize),
        FieldType.DATA => DATA = new Data(r, dataSize),
        FieldType.SPCT => SPCT = r.ReadUInt32(),
        FieldType.SPLO => SPLOs.AddX(r.ReadFUString(dataSize)),
        FieldType.WNAM => WNAM = new Ref<ARMORecord>(r, dataSize),
        FieldType.BODT => BODT = r.ReadS<ARMORecord.Bodt>(dataSize),
        FieldType.KSIZ => KSIZ = r.ReadUInt32(),
        FieldType.KWDA => KWDA = r.ReadFArray(z => new Ref<KYWDRecord>(r, 4), KSIZ),
        // body
        FieldType.MNAM => _state2 = 0,
        FieldType.FNAM => _state2 = 1,
        FieldType.MODL => Bodys[_state2].ANAM = r.ReadFUString(dataSize),
        FieldType.MODT => Bodys[_state2].MODT = new Modt(r, dataSize),
        //FieldType.VNAM => VNAM = new REF2Field<RACERecord>(r, dataSize),
        //FieldType.DNAM => DNAM = new REF2Field<HAIRRecord>(r, dataSize),
        //FieldType.CNAM => CNAM = r.ReadByte(dataSize),
        //FieldType.PNAM => PNAM = r.ReadSingle(dataSize),
        //FieldType.UNAM => UNAM = r.ReadSingle(dataSize),
        //FieldType.XNAM => XNAM = r.ReadBytes(dataSize),
        //FieldType.ATTR => DATA.ATTRField(r, dataSize),
        FieldType.NAM0 => _state++,
        _ => Empty,
    };
}

/// <summary>
/// REPA.Repair Item - 30000
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/REPA">
public class REPARecord : Record, IHaveMODL {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Ridt {
        public static (string, int) Struct = ("<f2if", 16);
        public float Weight;
        public int Value;
        public int Uses;
        public float Quality;
    }

    public Modl MODL { get; set; } // Model Name
    public string FNAM; // Item Name
    public Ridt RIDT; // Repair Data
    public RefX<SCPTRecord> SCRI; // Script Name

    public override object ReadField(Reader r, FieldType type, int dataSize) => r.Format == TES3
        ? type switch {
            FieldType.NAME => EDID = r.ReadFUString(dataSize),
            FieldType.MODL => MODL = new Modl(r, dataSize),
            FieldType.ITEX => MODL.ICON(r, dataSize),
            FieldType.FNAM => FNAM = r.ReadFUString(dataSize),
            FieldType.RIDT => RIDT = r.ReadS<Ridt>(dataSize),
            FieldType.SCRI => SCRI = new RefX<SCPTRecord>(r, dataSize),
            _ => Empty,
        }
        : Empty;
}

/// <summary>
/// REFR.Placed Object - 04500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/REFR"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/REFR"/>
public unsafe class REFRRecord : Record {
    /// <summary>
    /// Teleport Destination
    /// </summary>
    /// <param name="r"></param>
    /// <param name="dataSize"></param>
    public struct Xtel(Reader r, int dataSize) {
        [Flags] public enum Flag : uint { NoAlarm = 0x00, NoLoadScreen = 0x01, RelativePosition = 0x02 }
        public Ref<REFRRecord> Door = new(r.ReadUInt32());
        public Vector3 Position = r.ReadVector3();
        public Vector3 Rotation = r.ReadVector3();
        public Flag Flags = r.Format > TES4 ? (Flag)r.ReadUInt32() : 0;
        public Ref<CELLRecord>? TransitionInterior = r.Format >= TES5 ? new Ref<CELLRecord>(r.ReadUInt32()) : null;
    }

    /// <summary>
    /// Coords
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Data {
        public static (string, int) Struct = ("<6f", 24);
        public Vector3 Position;
        public Vector3 Rotation;
    }

    /// <summary>
    /// Ragdoll Data
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Xrgd {
        public static (string, int) Struct = ("<B3c6f", 28);
        public byte BoneId;
        public fixed byte Unused[3];
        public Vector3 Position;
        public Vector3 Rotation;
    }

    public struct Xloc {
        public override readonly string ToString() => $"{Key}";
        public byte LockLevel;
        public RefX<KEYMRecord> Key;
        public byte Flags;
        public Xloc(Reader r, int dataSize) {
            LockLevel = r.ReadByte();
            r.Skip(3); // Unused
            Key = new RefX<KEYMRecord>(r.ReadUInt32());
            if (dataSize == 16) r.Skip(4); // Unused
            Flags = r.ReadByte();
            r.Skip(3); // Unused
        }
    }

    public struct Xesp(Reader r, int dataSize) {
        public override readonly string ToString() => $"{Reference}";
        public RefX<Record> Reference = new(r.ReadUInt32());
        public uint Flags = r.ReadUInt32();
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Xsed {
        public override readonly string ToString() => $"{Seed}";
        public static Dictionary<int, string> Struct = new() { [1] = "<B", [4] = "<B3x" };
        public byte Seed;
    }

    public class Xmrk {
        public override string ToString() => $"{FULL}";
        public byte FNAM; // Map Flags
        public string FULL; // Name
        public ushort TNAM; // Type
    }

    public RefX<Record> NAME; // Base
    public Xtel? XTEL; // Teleport Destination (optional)
    public Data DATA; // Position/Rotation
    public Xloc? XLOC; // Lock information (optional)
    public List<CELLRecord.Xown> XOWNs; // Ownership (optional)
    public Xesp? XESP; // Enable Parent (optional)
    public RefX<Record>? XTRG; // Target (optional)
    public Xsed? XSED; // SpeedTree (optional)
    public byte[]? XLOD; // Distant LOD Data (optional)
    public float? XCHG; // Charge (optional)
    public float? XHLT; // Health (optional)
    public RefX<CELLRecord>? XPCI; // Unused (optional)
    public int? XLCM; // Level Modifier (optional)
    public RefX<REFRRecord>? XRTM; // Unknown (optional)
    public uint? XACT; // Action Flag (optional)
    public int? XCNT; // Count (optional)
    public List<Xmrk> XMRKs; // Ownership (optional)
                             //public bool? ONAM; // Open by Default
    public Xrgd? XRGD; // Ragdoll Data (optional)
    public float? XSCL; // Scale (optional)
    public byte? XSOL; // Contained Soul (optional)
    int _nextFull;

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.NAME => NAME = new RefX<Record>(r, dataSize),
        FieldType.XTEL => XTEL = new Xtel(r, dataSize),
        FieldType.DATA => DATA = r.ReadS<Data>(dataSize),
        FieldType.XLOC => XLOC = new Xloc(r, dataSize),
        FieldType.XOWN => (XOWNs ??= []).AddX(new CELLRecord.Xown { XOWN = new RefX<Record>(r, dataSize) }),
        FieldType.XRNK => XOWNs.Last().XRNK = r.ReadInt32(),
        FieldType.XGLB => XOWNs.Last().XGLB = new RefX<Record>(r, dataSize),
        FieldType.XESP => XESP = new Xesp(r, dataSize),
        FieldType.XTRG => XTRG = new RefX<Record>(r, dataSize),
        FieldType.XSED => XSED = r.ReadS<Xsed>(dataSize),
        FieldType.XLOD => XLOD = r.ReadBytes(dataSize),
        FieldType.XCHG => XCHG = r.ReadSingle(),
        FieldType.XHLT => XCHG = r.ReadSingle(),
        FieldType.XPCI => (z: XPCI = new RefX<CELLRecord>(r, dataSize), _nextFull = 1).z,
        FieldType.FULL when _nextFull == 1 => XPCI.Value.SetName(r.ReadFAString(dataSize)), //:matchif
        FieldType.FULL when _nextFull == 2 => XMRKs.Last().FULL = r.ReadFUString(dataSize), //:matchif
        FieldType.FULL when _nextFull != 1 && _nextFull != 2 => _nextFull = 0, //:matchif
        FieldType.XLCM => XLCM = r.ReadInt32(),
        FieldType.XRTM => XRTM = new RefX<REFRRecord>(r, dataSize),
        FieldType.XACT => XACT = r.ReadUInt32(),
        FieldType.XCNT => XCNT = r.ReadInt32(),
        FieldType.XMRK => (z: (XMRKs ??= []).AddX(new Xmrk()), _nextFull = 2).z,
        FieldType.FNAM => XMRKs.Last().FNAM = r.ReadByte(),
        FieldType.TNAM => XMRKs.Last().TNAM = r.ReadUInt16(),
        FieldType.ONAM => true,
        FieldType.XRGD => XRGD = r.ReadS<Xrgd>(dataSize),
        FieldType.XSCL => XSCL = r.ReadSingle(),
        FieldType.XSOL => XSOL = r.ReadByte(),
        _ => Empty,
    };
}

/// <summary>
/// REGN.Region - 34500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/REGN">
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/REGN"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/REGN"/>
public class REGNRecord : Record {
    public enum REGNType : byte { None_ = 0, One, Objects, Weather, Map, Landscape, Grass, Sound }

    public class Rdat {
        public uint Type;
        public REGNType Flags;
        public byte Priority;
        // groups
        public Rdot[] RDOTs; // Objects
        public string RDMP; // MapName
        public Rdgs[] RDGSs; // Grasses
        public uint RDMD; // Music Type
        public Rdsd[] RDSDs; // Sounds
        public Rdwt[] RDWTs; // Weather Types
        public Rdat(Reader r = null, int dataSize = 0) {
            if (r == null) return;
            Type = r.ReadUInt32();
            Flags = (REGNType)r.ReadByte();
            Priority = r.ReadByte();
            r.Skip(2); // Unused
        }
    }

    // TODO: Make ReadS
    public struct Rdot {
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
        public Rdot(Reader r, int dataSize) {
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
            VertexShading = r.ReadS<ByteColor4>(4);
        }
    }

    public struct Rdgs {
        public override readonly string ToString() => $"{Grass}";
        public Ref<GRASRecord> Grass;
        public Rdgs(Reader r, int dataSize) {
            Grass = new Ref<GRASRecord>(r.ReadUInt32());
            r.Skip(4); // Unused
        }
    }

    public struct Rdsd {
        public override readonly string ToString() => $"{Sound}";
        public RefX<SOUNRecord> Sound;
        public uint Flags;
        public uint Chance;
        public Rdsd(Reader r, int dataSize) {
            if (r.Format == TES3) {
                Sound = new RefX<SOUNRecord>(r.ReadFAString(32));
                Flags = 0;
                Chance = r.ReadByte();
                return;
            }
            Sound = new RefX<SOUNRecord>(r.ReadUInt32());
            Flags = r.ReadUInt32();
            Chance = r.ReadUInt32(); // float with TES5
        }
    }

    public struct Rdwt(Reader r, int dataSize) {
        public override readonly string ToString() => $"{Weather}";
        public Ref<WTHRRecord> Weather = new(r.ReadUInt32());
        public uint Chance = r.ReadUInt32();
        public Ref<GLOBRecord> Global = r.Format == TES5 ? new Ref<GLOBRecord>(r.ReadUInt32()) : new Ref<GLOBRecord>(0);
    }

    // TES3
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Weat {
        // v1.3 ESM files add 2 bytes to WEAT subrecords.
        public static Dictionary<int, string> Struct = new() { [8] = "<8B", [10] = "<8B2x" };
        public byte Clear;
        public byte Cloudy;
        public byte Foggy;
        public byte Overcast;
        public byte Rain;
        public byte Thunder;
        public byte Ash;
        public byte Blight;
    }

    // TES4
    public class Rpli(Reader r, int dataSize) {
        public uint EdgeFalloff = r.ReadUInt32(); // (World Units)
        public Vector2[] Points; // Region Point List Data
        public object RPLD(Reader r, int dataSize) => Points = r.ReadFArray(z => r.ReadVector2(), dataSize >> 3);
    }

    public string ICON; // Icon / Sleep creature
    public RefX<WRLDRecord> WNAM; // Worldspace - Region name
    public ByteColor4 RCLR; // Map Color (COLORREF)
    public List<Rdat> RDATs = []; // Region Data Entries / TES3: Sound Record (order determines the sound priority)
    // TES3
    public Weat? WEAT; // Weather Data
    // TES4
    public List<Rpli> RPLIs = []; // Region Areas
    Rdat _last;

    protected override HashSet<FieldType> DF3 => [FieldType.SNAM, FieldType.RPLI, FieldType.RPLD, FieldType.RDAT, FieldType.RDOT, FieldType.RDMP, FieldType.RDGS, FieldType.RDGS, FieldType.RDMD, FieldType.RDSD, FieldType.RDWT];
    protected override HashSet<FieldType> DF4 => [FieldType.RDAT, FieldType.RPLI, FieldType.RPLD, FieldType.RDAT, FieldType.RDOT, FieldType.RDMP, FieldType.RDGS, FieldType.RDGS, FieldType.RDMD, FieldType.RDSD, FieldType.RDWT];
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadFUString(dataSize),
        FieldType.WNAM or FieldType.FNAM => WNAM = new RefX<WRLDRecord>(r, dataSize),
        FieldType.WEAT => WEAT = r.ReadS<Weat>(dataSize),
        FieldType.ICON or FieldType.BNAM => ICON = r.ReadFUString(dataSize),
        FieldType.RCLR or FieldType.CNAM => RCLR = r.ReadS<ByteColor4>(dataSize),
        FieldType.SNAM => _last = RDATs.AddX(new Rdat { RDSDs = [new Rdsd(r, dataSize)] }),
        FieldType.RPLI => RPLIs.AddX(new Rpli(r, dataSize)),
        FieldType.RPLD => RPLIs.Last().RPLD(r, dataSize),
        FieldType.RDAT => _last = RDATs.AddX(new Rdat(r, dataSize)),
        FieldType.RDOT => _last.RDOTs = r.ReadFArray(z => new Rdot(r, dataSize), dataSize / 52),
        FieldType.RDMP => _last.RDMP = r.ReadFUString(dataSize),
        FieldType.RDGS => _last.RDGSs = r.ReadFArray(z => new Rdgs(r, dataSize), dataSize / 8),
        FieldType.RDMD => _last.RDMD = r.ReadUInt32(),
        FieldType.RDSD => _last.RDSDs = r.ReadFArray(z => new Rdsd(r, dataSize), dataSize / 12),
        FieldType.RDWT => _last.RDWTs = r.ReadFArray(z => new Rdwt(r, dataSize), dataSize / (r.Format == TES4 ? 8 : 12)),
        _ => Empty,
    };
}

/// <summary>
/// ROAD.Road - 44000
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/ROAD">
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/ROAD"/>
public class ROADRecord : Record {
    public PGRDRecord.Pgrp[] PGRPs;
    public byte[] PGRR;

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.PGRP => PGRPs = r.ReadSArray<PGRDRecord.Pgrp>(dataSize >> 4),
        FieldType.PGRR => PGRR = r.ReadBytes(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// SBSP.Subspace - 0400
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/SBSP"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/SBSP"/>
public class SBSPRecord : Record {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Dnam {
        public static (string, int) Struct = ("<3f", 12);
        public float X; // X dimension
        public float Y; // Y dimension
        public float Z; // Z dimension
    }

    public Dnam DNAM;

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.DNAM => DNAM = r.ReadS<Dnam>(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// SCPT.Script - 34000
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/SCPT">
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/SCPT"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/SCPT"/>
public class SCPTRecord : Record {
    public struct Ctda {
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
        public Ctda(Reader r, int dataSize) {
            if (r.Format == TES3) {
                Index = r.ReadByte();
                Type = r.ReadByte();
                FunctionId = r.ReadFAString(2);
                CompareOp = (byte)(r.ReadByte() << 1);
                Name = r.ReadFAString(dataSize - 5);
                return;
            }
            CompareOp = r.ReadByte();
            r.Skip(3); // Unused
            ComparisonValue = r.ReadSingle();
            FunctionId = r.ReadFAString(4);
            Parameter1 = r.ReadInt32();
            Parameter2 = r.ReadInt32();
            if (dataSize == 24) r.Skip(4); // Unused
        }
    }

    public class Scvr {
        public Ctda SCVR;
        public long? INTV; //
        public float? FLTV; // The function/variable result for the previous SCVR
    }

    // TES3
    public class Schd(Reader r, int dataSize) {
        public override string ToString() => $"{Name}";
        public string Name = r.ReadFAString(32);
        public int NumShorts = r.ReadInt32();
        public int NumLongs = r.ReadInt32();
        public int NumFloats = r.ReadInt32();
        public int ScriptDataSize = r.ReadInt32();
        public int LocalVarSize = r.ReadInt32();
        public string[] Variables = null;
        public object SCVR(Reader r, int dataSize) => Variables = [.. r.ReadVAStringList(dataSize)];
    }

    // TES4
    public struct Schr(Reader r, int dataSize) {
        public override readonly string ToString() => $"{RefCount}";
        public uint RefCount = r.Skip(4).ReadUInt32(); // 4:Unused
        public uint CompiledSize = r.ReadUInt32();
        public uint VariableCount = r.ReadUInt32();
        public uint Type = (z: r.ReadUInt32(), r.Skip(dataSize > 20 ? dataSize - 20 : 0)).z; // 0x000 = Object, 0x001 = Quest, 0x100 = Magic Effect
    }

    public struct Scda(Reader r, int dataSize) {
        public override readonly string ToString() => $"{Data}";
        public byte[] Data = r.ReadBytes(dataSize);
    }

    public class Slsd {
        public override string ToString() => $"{Idx}:{VariableName}";
        public uint Idx;
        public uint Type;
        public string VariableName;
        public Slsd(Reader r, int dataSize) {
            Idx = r.ReadUInt32();
            r.ReadUInt32(); // Unknown
            r.ReadUInt32(); // Unknown
            r.ReadUInt32(); // Unknown
            Type = r.ReadUInt32();
            r.ReadUInt32(); // Unknown
            // SCVRField
            VariableName = null;
        }
        public object SCVR(Reader r, int dataSize) => VariableName = r.ReadFUString(dataSize);
    }

    public override string ToString() => $"SCPT: {EDID ?? SCHD.Name}";
    public byte[] SCDA; // Compiled Script
    public string SCTX; // Script Source
    // TES3
    public Schd SCHD; // Script Data
    // TES4
    public Schr SCHR; // Script Data
    public List<Slsd> SLSDs = []; // Variable data
    public List<Slsd> SCRVs = []; // Ref variable data (one for each ref declared)
    public List<RefX<Record>> SCROs = []; // Global variable reference

    protected override HashSet<FieldType> DF4 => [FieldType.SLSD, FieldType.SCVR, FieldType.SCRV, FieldType.SCRO];
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.SCHD => SCHD = new Schd(r, dataSize),
        FieldType.SCVR => r.Format != TES3 ? SLSDs.Last().SCVR(r, dataSize) : SCHD.SCVR(r, dataSize),
        FieldType.SCDA or FieldType.SCDT => SCDA = r.ReadBytes(dataSize),
        FieldType.SCTX => SCTX = r.ReadFUString(dataSize),
        // TES4
        FieldType.SCHR => SCHR = new Schr(r, dataSize),
        FieldType.SLSD => SLSDs.AddX(new Slsd(r, dataSize)),
        FieldType.SCRO => SCROs.AddX(new RefX<Record>(r, dataSize)),
        FieldType.SCRV => SCRVs.AddX(this.Then(r.ReadUInt32(), v => SLSDs.Single(x => x.Idx == v))),
        _ => Empty,
    };
}

/// <summary>
/// SGST.Sigil Stone - 04000
/// </summary>
public class SGSTRecord : Record, IHaveMODL {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Data {
        public static (string, int) Struct = ("<Bif", 9);
        public byte Uses;
        public int Value;
        public float Weight;
    }

    public Modl MODL { get; set; } // Model
    public string FULL; // Item Name
    public Data DATA; // Sigil Stone Data
    public RefX<SCPTRecord>? SCRI; // Script (optional)
    public List<ENCHRecord.Efit> EFITs = []; // Effect Data
    public List<ENCHRecord.Scit> SCITs = []; // Script Effect Data

    protected override HashSet<FieldType> DF4 => [FieldType.FULL, FieldType.EFID, FieldType.EFIT];
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.MODL => MODL = new Modl(r, dataSize),
        FieldType.MODB => MODL.MODB(r, dataSize),
        FieldType.MODT => MODL.MODT(r, dataSize),
        FieldType.ICON => MODL.ICON(r, dataSize),
        FieldType.FULL => SCITs.Count == 0 ? FULL = r.ReadFUString(dataSize) : SCITs.Last().FULL(r, dataSize),
        FieldType.DATA => DATA = r.ReadS<Data>(dataSize),
        FieldType.SCRI => SCRI = new RefX<SCPTRecord>(r, dataSize),
        FieldType.EFID => r.Skip(dataSize),
        FieldType.EFIT => EFITs.AddX(new ENCHRecord.Efit(r, dataSize)),
        FieldType.SCIT => SCITs.AddX(new ENCHRecord.Scit(r, dataSize)),
        _ => Empty,
    };
}

/// <summary>
/// SKIL.Skill - 34500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/SKIL">
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/SKIL"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/SKIL"/>
/// <see cref="https://starfieldwiki.net/wiki/Starfield_Mod:Mod_File_Format/SKIL">
public class SKILRecord : Record {
    public struct Data(Reader r, int dataSize) {
        public int Action = r.Format == TES3 ? 0 : r.ReadInt32();
        public int Attribute = r.ReadInt32();
        public uint Specialization = r.ReadUInt32(); // 0 = Combat, 1 = Magic, 2 = Stealth
        public float[] UseValue = r.ReadPArray<float>("f", r.Format == TES3 ? 4 : 2); // The use types for each skill are hard-coded.
    }

    public override string ToString() => $"SKIL: {INDX}:{EDID}";
    public int INDX; // Skill ID
    public Data DATA; // Skill Data
    public string DESC; // Skill description
    // TES4
    public string ICON; // Icon
    public string ANAM; // Apprentice Text
    public string JNAM; // Journeyman Text
    public string ENAM; // Expert Text
    public string MNAM; // Master Text

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.INDX => INDX = r.ReadInt32(),
        FieldType.DATA or FieldType.SKDT => DATA = new Data(r, dataSize),
        FieldType.DESC => DESC = r.ReadFUString(dataSize),
        FieldType.ICON => ICON = r.ReadFUString(dataSize),
        FieldType.ANAM => ANAM = r.ReadFUString(dataSize),
        FieldType.JNAM => JNAM = r.ReadFUString(dataSize),
        FieldType.ENAM => ENAM = r.ReadFUString(dataSize),
        FieldType.MNAM => MNAM = r.ReadFUString(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// SLGM.Soul Gem - 04500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/SLGM"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/SLGM"/>
public class SLGMRecord : Record, IHaveMODL {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Data {
        public static (string, int) Struct = ("<if", 8);
        public int Value;
        public float Weight;
    }

    public Modl MODL { get; set; } // Model
    public string FULL; // Item Name
    public RefX<SCPTRecord> SCRI; // Script (optional)
    public Data DATA; // Type of soul contained in the gem
    public byte SOUL; // Type of soul contained in the gem
    public byte SLCP; // Soul gem maximum capacity

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.MODL => MODL = new Modl(r, dataSize),
        FieldType.MODB => MODL.MODB(r, dataSize),
        FieldType.MODT => MODL.MODT(r, dataSize),
        FieldType.ICON => MODL.ICON(r, dataSize),
        FieldType.FULL => FULL = r.ReadFUString(dataSize),
        FieldType.SCRI => SCRI = new RefX<SCPTRecord>(r, dataSize),
        FieldType.DATA => DATA = r.ReadS<Data>(dataSize),
        FieldType.SOUL => SOUL = r.ReadByte(),
        FieldType.SLCP => SLCP = r.ReadByte(),
        _ => Empty,
    };
}

/// <summary>
/// SNDG.Sound Generator - 30000
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/SNDG">
public class SNDGRecord : Record {
    public enum SNDGType : uint { LeftFoot = 0, RightFoot, SwimLeft, SwimRight, Moan, Roar, Scream, Land }

    public int DATA; // Sound Type Data
    public string SNAM; // Sound ID
    public string CNAM; // Creature name (optional)

    public override object ReadField(Reader r, FieldType type, int dataSize) => r.Format == TES3
        ? type switch {
            FieldType.NAME => EDID = r.ReadFUString(dataSize),
            FieldType.DATA => DATA = r.ReadInt32(),
            FieldType.SNAM => SNAM = r.ReadFUString(dataSize),
            FieldType.CNAM => CNAM = r.ReadFUString(dataSize),
            _ => Empty,
        }
        : Empty;
}

/// <summary>
/// SNDR.Sound Reference - 00500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/SNDR"/>
public class SNDRRecord : Record {
    public ByteColor4 CNAM; // RGB color

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.CNAM => CNAM = r.ReadS<ByteColor4>(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// SOUN.Sound - 34500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/SOUN">
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/SOUN"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/SOUN"/>
public class SOUNRecord : Record {
    [Flags]
    public enum Flag : ushort {
        RandomFrequencyShift = 0x0001,
        PlayAtRandom = 0x0002,
        EnvironmentIgnored = 0x0004,
        RandomLocation = 0x0008,
        Loop = 0x0010,
        MenuSound = 0x0020,
        _2D = 0x0040,
        _360LFE = 0x0080,
    }

    public class Data {
        public byte Volume; // (0=0.00, 255=1.00)
        public byte MinRange; // Minimum attenuation distance
        public byte MaxRange; // Maximum attenuation distance
        // Bethesda4
        public sbyte FrequencyAdjustment; // Frequency adjustment %
        public ushort Flags; // Flags
        public ushort StaticAttenuation; // Static Attenuation (db)
        public byte StopTime; // Stop time
        public byte StartTime; // Start time
        public Data(Reader r, int dataSize) {
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

    public string FNAM; // Sound Filename (relative to Sounds\)
    public Data DATA; // Sound Data

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadFUString(dataSize),
        FieldType.FNAM => FNAM = r.ReadFUString(dataSize),
        FieldType.SNDX => DATA = new Data(r, dataSize),
        FieldType.SNDD => DATA = new Data(r, dataSize),
        FieldType.DATA => DATA = new Data(r, dataSize),
        _ => Empty,
    };
}

/// <summary>
/// SPEL.Spell - 345S0
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/SPEL">
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/SPEL"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/SPEL"/>
/// <see cref="https://starfieldwiki.net/wiki/Starfield_Mod:Mod_File_Format/SPEL">
public class SPELRecord : Record {
    public struct Spit(Reader r, int dataSize) {
        public override readonly string ToString() => $"{Type}";
        // TES3: 0 = Spell, 1 = Ability, 2 = Blight, 3 = Disease, 4 = Curse, 5 = Power
        // TES4: 0 = Spell, 1 = Disease, 2 = Power, 3 = Lesser Power, 4 = Ability, 5 = Poison
        public uint Type = r.ReadUInt32();
        public int SpellCost = r.ReadInt32();
        public uint Flags = r.ReadUInt32(); // 0x0001 = AutoCalc, 0x0002 = PC Start, 0x0004 = Always Succeeds
        // TES4
        public int SpellLevel = r.Format != TES3 ? r.ReadInt32() : 0;
    }

    public string FULL; // Spell name
    public Spit SPIT; // Spell data
    public List<ENCHRecord.Efit> EFITs = []; // Effect Data
    // TES4
    public List<ENCHRecord.Scit> SCITs = []; // Script effect data

    protected override HashSet<FieldType> DF3 => [FieldType.ENAM];
    protected override HashSet<FieldType> DF4 => [FieldType.FULL, FieldType.EFID, FieldType.EFIT];
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadFUString(dataSize),
        FieldType.FULL => SCITs.Count == 0 ? FULL = r.ReadFUString(dataSize) : SCITs.Last().FULL(r, dataSize),
        FieldType.FNAM => FULL = r.ReadFUString(dataSize),
        FieldType.SPIT or FieldType.SPDT => SPIT = new Spit(r, dataSize),
        FieldType.EFID => r.Skip(dataSize),
        FieldType.EFIT or FieldType.ENAM => EFITs.AddX(new ENCHRecord.Efit(r, dataSize)),
        FieldType.SCIT => SCITs.AddX(new ENCHRecord.Scit(r, dataSize)),
        _ => Empty,
    };
}

/// <summary>
/// SSCR.Start Script - 30000
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/SSCR">
public class SSCRRecord : Record {
    public string DATA; // Digits

    public override object ReadField(Reader r, FieldType type, int dataSize) => r.Format == TES3
        ? type switch {
            FieldType.NAME => EDID = r.ReadFUString(dataSize),
            FieldType.DATA => DATA = r.ReadFUString(dataSize),
            _ => Empty,
        }
        : Empty;
}

/// <summary>
/// STAT.Static - 3450
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/STAT">
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/STAT"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/STAT"/>
public class STATRecord : Record, IHaveMODL {
    public Modl MODL { get; set; } // Model

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadFUString(dataSize),
        FieldType.MODL => MODL = new Modl(r, dataSize),
        FieldType.MODB => MODL.MODB(r, dataSize),
        FieldType.MODT => MODL.MODT(r, dataSize),
        _ => Empty,
    };
}

/// <summary>
/// STDT.xx - 000S0
/// </summary>
/// <see cref="https://starfieldwiki.net/wiki/Starfield_Mod:Mod_File_Format/STDT">
public class STDTRecord : Record {
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// SUNP.xx - 000S0
/// </summary>
/// <see cref="https://starfieldwiki.net/wiki/Starfield_Mod:Mod_File_Format/SUNP">
public class SUNPRecord : Record {
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// TES3.Plugin Info - 30000
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/TES3"/>
public class TES3Record : Record {
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct Hedr {
        public static (string, int) Struct = ("<fI32s256sI", 300);
        public float Version;
        public uint FileType;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string CompanyName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string FileDescription;
        public uint NumRecords;
    }

    public Hedr HEDR;
    public List<string> MASTs;
    public List<long> DATAs;

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.HEDR => HEDR = r.ReadS<Hedr>(dataSize),
        FieldType.MAST => (MASTs ??= []).AddX(r.ReadFUString(dataSize)),
        FieldType.DATA => (DATAs ??= []).AddX(r.ReadINTV(dataSize)),
        _ => Empty,
    };
}

/// <summary>
/// TES4.Plugin Info - 04500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/TES4"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/TES4"/>
/// <see cref="https://starfieldwiki.net/wiki/Starfield_Mod:Mod_File_Format/TES4">
public class TES4Record : Record {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Hedr {
        public static (string, int) Struct = ("<fiI", 12);
        public float Version;
        public int NumRecords; // Number of records and groups (not including TES4 record itself).
        public uint NextObjectId; // Next available object ID.
    }

    public Hedr HEDR;
    public string CNAM; // author (Optional)
    public string SNAM; // description (Optional)
    public List<string> MASTs; // master
    public List<long> DATAs; // fileSize
    public byte[] ONAM; // overrides (Optional)
    public int INTV; // unknown
    public int? INCC; // unknown (Optional)
    // TES5
    public byte[] TNAM; // overrides (Optional)

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.HEDR => HEDR = r.ReadS<Hedr>(dataSize),
        FieldType.OFST => r.Skip(dataSize),
        FieldType.DELE => r.Skip(dataSize),
        FieldType.CNAM => CNAM = r.ReadFUString(dataSize),
        FieldType.SNAM => SNAM = r.ReadFUString(dataSize),
        FieldType.MAST => (MASTs ??= []).AddX(r.ReadFUString(dataSize)),
        FieldType.DATA => (DATAs ??= []).AddX(r.ReadINTV(dataSize)),
        FieldType.ONAM => ONAM = r.ReadBytes(dataSize),
        FieldType.INTV => INTV = r.ReadInt32(),
        FieldType.INCC => INCC = r.ReadInt32(),
        // TES5
        FieldType.TNAM => TNAM = r.ReadBytes(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// TERM.Computer Terminals - 000S0
/// </summary>
/// <see cref="https://starfieldwiki.net/wiki/Starfield_Mod:Mod_File_Format/TERM">
public class TERMRecord : Record {
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// TMLM.Terminal Menus - 000S0
/// </summary>
/// <see cref="https://starfieldwiki.net/wiki/Starfield_Mod:Mod_File_Format/TMLM">
public class TMLMRecord : Record {
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// TREE.Tree - 04500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/TREE"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/TREE"/>
public class TREERecord : Record, IHaveMODL {
    public struct Snam(Reader r, int dataSize) {
        public int[] Values = r.ReadPArray<int>("i", dataSize >> 2);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Cnam {
        public static (string, int) Struct = ("<5fi2f", 32);
        public float LeafCurvature;
        public float MinimumLeafAngle;
        public float MaximumLeafAngle;
        public float BranchDimmingValue;
        public float LeafDimmingValue;
        public int ShadowRadius;
        public float RockSpeed;
        public float RustleSpeed;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Bnam {
        public static (string, int) Struct = ("<2f", 8);
        public float Width;
        public float Height;
    }

    public Modl MODL { get; set; } // Model
    public Snam SNAM; // SpeedTree Seeds, array of ints
    public Cnam CNAM; // Tree Parameters
    public Bnam BNAM; // Billboard Dimensions

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.MODL => MODL = new Modl(r, dataSize),
        FieldType.MODB => MODL.MODB(r, dataSize),
        FieldType.MODT => MODL.MODT(r, dataSize),
        FieldType.ICON => MODL.ICON(r, dataSize),
        FieldType.SNAM => SNAM = new Snam(r, dataSize),
        FieldType.CNAM => CNAM = r.ReadS<Cnam>(dataSize),
        FieldType.BNAM => BNAM = r.ReadS<Bnam>(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// TRNS.TRNS Record - 04000 #F4
/// </summary>
/// <see cref="https://tes5edit.github.io/fopdoc/Fallout4/Records/TRNS.html"/>
public class TRNSRecord : Record {
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        _ => Empty,
    };
}

/// <summary>
/// TXST.Texture Set - 04500 #F4
/// </summary>
/// <see cref="https://tes5edit.github.io/fopdoc/Fallout4/Records/TXST.html"/>
/// <see cref="https://en.uesp.net/wiki/Skyrim_Mod:Mod_File_Format/TXST"/>
public class TXSTRecord : Record {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Obnd {
        public static (string, int) Struct = ("<6h", 12);
        public short X1;
        public short Y1;
        public short Z1;
        public short X2;
        public short Y2;
        public short Z2;
    }

    [Flags]
    public enum DnamFlag : ushort {
        NotHasSpecularMap = 0x01, // not Has specular map
        FacegenTextures = 0x02, // Facegen Textures
        HasModelSpaceNormalMap = 0x04, // Has model space normal map
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct Dodt {
        [Flags]
        public enum Flag : byte {
            Parallax = 0x01, // Parallax (enables the Scale and Passes values in the CK)
            AlphaBlending = 0x02, // Alpha Blending
            AlphaTesting = 0x04, // Alpha Testing
            Not4Subtextures = 0x08, // not 4 Subtextures
        }
        public static (string, int) Struct = ("<7f8B", 36);
        public float MinWidth;          // Min Width
        public float MaxWidth;          // Max Width
        public float MinHeight;         // Min Height
        public float MaxHeight;         // Max Height
        public float Depth;             // Depth
        public float Shininess;         // Shininess
        public float ParallaxScale;     // Parallax Scale
        public byte ParallaxPasses;     // Parallax Passes
        public Flag Flags;              // Flags
        public fixed byte Unknown[2];   // Unknown but not neverused
        public ByteColor4 Color;        // Color
    }

    public Obnd OBND; // Object Boundary
    public string TX00; // Texture path, color map
    public string TX01; // Texture path, normal map (tangent- or model-space)
    public string TX02; // Texture path, mask (environment or light)
    public string TX03; // Texture path, tone map (for skins) or glow map (for things)
    public string TX04; // Texture path, detail map (roughness, complexion, age)
    public string TX05; // Texture path, environment map (cubemaps mostly)
    public string TX06; // Texture path Multilayer (does not occur in Skyrim.esm)
    public string TX07; // Texture path, specularity map (for skinny bodies, and for furry bodies)
    public Dodt DODT; // Decal Data
    public ushort DNAM; // Flags

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.OBND => OBND = r.ReadS<Obnd>(dataSize),
        FieldType.TX00 => TX00 = r.ReadFUString(dataSize),
        FieldType.TX01 => TX01 = r.ReadFUString(dataSize),
        FieldType.TX02 => TX02 = r.ReadFUString(dataSize),
        FieldType.TX03 => TX03 = r.ReadFUString(dataSize),
        FieldType.TX04 => TX04 = r.ReadFUString(dataSize),
        FieldType.TX05 => TX05 = r.ReadFUString(dataSize),
        FieldType.TX06 => TX06 = r.ReadFUString(dataSize),
        FieldType.TX07 => TX07 = r.ReadFUString(dataSize),
        FieldType.DODT => DODT = r.ReadS<Dodt>(dataSize),
        FieldType.DNAM => r.ReadUInt16(),
        _ => Empty,
    };
}

/// <summary>
/// WATR.Water Type - 04500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/WATR"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/WATR"/>
public class WATRRecord : Record {
    public class Data {
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
        public Data(Reader r, int dataSize) {
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
            ShallowColor = r.ReadS<ByteColor4>(4);
            DeepColor = r.ReadS<ByteColor4>(4);
            ReflectionColor = r.ReadS<ByteColor4>(4);
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

    public struct Gnam(Reader r, int dataSize) {
        public RefX<WATRRecord> Daytime = new(r.ReadUInt32());
        public RefX<WATRRecord> Nighttime = new(r.ReadUInt32());
        public RefX<WATRRecord> Underwater = new(r.ReadUInt32());
    }

    public string TNAM; // Texture
    public byte ANAM; // Opacity
    public byte FNAM; // Flags
    public string MNAM; // Material ID
    public RefX<SOUNRecord> SNAM; // Sound
    public Data DATA; // DATA
    public Gnam GNAM; // GNAM

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.TNAM => TNAM = r.ReadFUString(dataSize),
        FieldType.ANAM => ANAM = r.ReadByte(),
        FieldType.FNAM => FNAM = r.ReadByte(),
        FieldType.MNAM => MNAM = r.ReadFUString(dataSize),
        FieldType.SNAM => SNAM = new RefX<SOUNRecord>(r, dataSize),
        FieldType.DATA => DATA = new Data(r, dataSize),
        FieldType.GNAM => GNAM = new Gnam(r, dataSize),
        _ => Empty,
    };
}

/// <summary>
/// WEAP.Weapon - 345S0
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES3Mod:Mod_File_Format/WEAP">
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/WEAP"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/WEAP"/>
/// <see cref="https://starfieldwiki.net/wiki/Starfield_Mod:Mod_File_Format/WEAP">
public class WEAPRecord : Record, IHaveMODL {
    public struct Data {
        public enum WEAPType { ShortBladeOneHand = 0, LongBladeOneHand, LongBladeTwoClose, BluntOneHand, BluntTwoClose, BluntTwoWide, SpearTwoWide, AxeOneHand, AxeTwoHand, MarksmanBow, MarksmanCrossbow, MarksmanThrown, Arrow, Bolt, }
        public float Weight;
        public int Value;
        public uint Type;
        public int Health;
        public float Speed;
        public float Reach;
        public short Damage; // EnchantPts
        public byte ChopMin;
        public byte ChopMax;
        public byte SlashMin;
        public byte SlashMax;
        public byte ThrustMin;
        public byte ThrustMax;
        public int Flags; // 0 = ?, 1 = Ignore Normal Weapon Resistance?
        public Data(Reader r, int dataSize) {
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
            Type = r.ReadUInt32();
            Speed = r.ReadSingle();
            Reach = r.ReadSingle();
            Flags = r.ReadInt32();
            Value = r.ReadInt32();
            Health = r.ReadInt32();
            Weight = r.ReadSingle();
            Damage = r.ReadInt16();
            ChopMin = ChopMax = SlashMin = SlashMax = ThrustMin = ThrustMax = 0;
        }
    }

    public Modl MODL { get; set; } // Model
    public string FULL; // Item Name
    public Data DATA; // Weapon Data
    public RefX<ENCHRecord> ENAM; // Enchantment ID
    public RefX<SCPTRecord> SCRI; // Script (optional)
    // TES4
    public short? ANAM; // Enchantment points (optional)

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID or FieldType.NAME => EDID = r.ReadFUString(dataSize),
        FieldType.MODL => MODL = new Modl(r, dataSize),
        FieldType.MODB => MODL.MODB(r, dataSize),
        FieldType.MODT => MODL.MODT(r, dataSize),
        FieldType.ICON or FieldType.ITEX => MODL.ICON(r, dataSize),
        FieldType.FULL or FieldType.FNAM => FULL = r.ReadFUString(dataSize),
        FieldType.DATA or FieldType.WPDT => DATA = new Data(r, dataSize),
        FieldType.ENAM => ENAM = new RefX<ENCHRecord>(r, dataSize),
        FieldType.SCRI => SCRI = new RefX<SCPTRecord>(r, dataSize),
        FieldType.ANAM => ANAM = r.ReadInt16(),
        _ => Empty,
    };
}

/// <summary>
/// WRLD.Worldspace - 045S0
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/WRLD"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/WRLD"/>
/// <see cref="https://starfieldwiki.net/wiki/Starfield_Mod:Mod_File_Format/WRLD">
public class WRLDRecord : Record {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Mnam {
        public static (string, int) Struct = ("<2i4h", 16);
        public Int2 UsableDimensions;
        // Cell Coordinates
        public short NWCell_X;
        public short NWCell_Y;
        public short SECell_X;
        public short SECell_Y;
    }

    public struct Nam0(Reader r, int dataSize) {
        public Vector2 Min = new(r.ReadSingle(), r.ReadSingle());
        public Vector2 Max = Vector2.Zero;
        public object NAM9(Reader r, int dataSize) => Max = new Vector2(r.ReadSingle(), r.ReadSingle());
    }

    // TES5
    public struct Rnam {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Reference {
            public static (string, int) Struct = ("<I2h", 8);
            public Ref<REFRRecord> Ref;
            public short X;
            public short Y;
        }
        public short GridX;
        public short GridY;
        public Reference[] GridReferences;
        public Rnam(Reader r, int dataSize) {
            GridX = r.ReadInt16();
            GridY = r.ReadInt16();
            GridReferences = r.ReadL32SArray<Reference>();
            Debug.Assert((dataSize - 8) >> 3 == GridReferences.Length);
        }
    }

    public string FULL;
    public RefX<WRLDRecord>? WNAM; // Parent Worldspace
    public RefX<CLMTRecord>? CNAM; // Climate
    public RefX<WATRRecord>? NAM2; // Water
    public string ICON; // Icon
    public Mnam? MNAM; // Map Data
    public byte? DATA; // Flags
    public Nam0 NAM0; // Object Bounds
    public uint? SNAM; // Music
    // TES5
    public List<Rnam> RNAMs = []; // Large References

    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.FULL => FULL = r.ReadFUString(dataSize),
        FieldType.WNAM => WNAM = new RefX<WRLDRecord>(r, dataSize),
        FieldType.CNAM => CNAM = new RefX<CLMTRecord>(r, dataSize),
        FieldType.NAM2 => NAM2 = new RefX<WATRRecord>(r, dataSize),
        FieldType.ICON => ICON = r.ReadFUString(dataSize),
        FieldType.MNAM => MNAM = r.ReadS<Mnam>(dataSize),
        FieldType.DATA => DATA = r.ReadByte(),
        FieldType.NAM0 => NAM0 = new Nam0(r, dataSize),
        FieldType.NAM9 => NAM0.NAM9(r, dataSize),
        FieldType.SNAM => SNAM = r.ReadUInt32(),
        FieldType.OFST => r.Skip(dataSize),
        // TES5
        FieldType.RNAM => RNAMs.AddX(new Rnam(r, dataSize)),
        _ => Empty,
    };
}

/// <summary>
/// WTHR.Weather - 04500
/// </summary>
/// <see cref="https://en.uesp.net/wiki/TES4Mod:Mod_File_Format/WTHR"/>
/// <see cref="https://en.uesp.net/wiki/TES5Mod:Mod_File_Format/WTHR"/>
public class WTHRRecord : Record, IHaveMODL {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Fnam {
        public static (string, int) Struct = ("<4f", 16);
        public float DayNear;
        public float DayFar;
        public float NightNear;
        public float NightFar;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Hnam {
        public static (string, int) Struct = ("<14f", 56);
        public float EyeAdaptSpeed;
        public float BlurRadius;
        public float BlurPasses;
        public float EmissiveMult;
        public float TargetLUM;
        public float UpperLUMClamp;
        public float BrightScale;
        public float BrightClamp;
        public float LUMRampNoTex;
        public float LUMRampMin;
        public float LUMRampMax;
        public float SunlightDimmer;
        public float GrassDimmer;
        public float TreeDimmer;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Data {
        public static (string, int) Struct = ("<15B", 15);
        public byte WindSpeed;
        public byte CloudSpeed_Lower;
        public byte CloudSpeed_Upper;
        public byte TransDelta;
        public byte SunGlare;
        public byte SunDamage;
        public byte Precipitation_BeginFadeIn;
        public byte Precipitation_EndFadeOut;
        public byte ThunderLightning_BeginFadeIn;
        public byte ThunderLightning_EndFadeOut;
        public byte ThunderLightning_Frequency;
        public byte WeatherClassification;
        public ByteColor4 LightningColor;
        public object Fill() => LightningColor.A = 255; // add 255
    }

    public struct Snam(Reader r, int dataSize) {
        public RefX<SOUNRecord> Sound = new(r.ReadUInt32()); // Sound FormId
        public uint Type = r.ReadUInt32(); // Sound Type - 0=Default, 1=Precipitation, 2=Wind, 3=Thunder
    }

    public Modl MODL { get; set; } // Model
    public string CNAM; // Lower Cloud Layer
    public string DNAM; // Upper Cloud Layer
    public byte[] NAM0; // Colors by Types/Times
    public Fnam FNAM; // Fog Distance
    public Hnam HNAM; // HDR Data
    public Data DATA; // Weather Data
    public List<Snam> SNAMs = []; // Sounds

    protected override HashSet<FieldType> DF4 => [FieldType.SNAM];
    public override object ReadField(Reader r, FieldType type, int dataSize) => type switch {
        FieldType.EDID => EDID = r.ReadFUString(dataSize),
        FieldType.MODL => MODL = new Modl(r, dataSize),
        FieldType.MODB => MODL.MODB(r, dataSize),
        FieldType.CNAM => CNAM = r.ReadFUString(dataSize),
        FieldType.DNAM => DNAM = r.ReadFUString(dataSize),
        FieldType.NAM0 => NAM0 = r.ReadBytes(dataSize),
        FieldType.FNAM => FNAM = r.ReadS<Fnam>(dataSize),
        FieldType.HNAM => HNAM = r.ReadS<Hnam>(dataSize),
        FieldType.DATA => (DATA = r.ReadS<Data>(dataSize), DATA.Fill()),
        FieldType.SNAM => SNAMs.AddX(new Snam(r, dataSize)),
        _ => Empty,
    };
}

#endregion
