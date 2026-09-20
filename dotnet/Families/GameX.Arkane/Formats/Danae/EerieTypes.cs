using OpenX;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
#pragma warning disable CS8500

namespace GameX.Arkane.Formats.Danae;

//struct
//{
//    Vector3 v[3];
//} EERIE_TRI; // Aligned 1 2 4

//struct
//{
//    EERIE_2D min;
//EERIE_2D max;
//} EERIE_2D_BBOX; // Aligned 1 2 4 8

//struct
//{
//    Vector3 min;
//Vector3 max;
//} EERIE_3D_BBOX; // Aligned 1 2 4

//struct
//{
//    char exist;
//char type;
//char treat;
//char selected;
//short extras;
//short status; // on/off 1/0
//Vector3 pos;
//float fallstart;
//float fallend;
//float falldiff;
//float falldiffmul;
//float precalc;
//EERIE_RGB rgb255;
//float intensity;
//EERIE_RGB rgb;
//float i;
//Vector3 mins;
//Vector3 maxs;
//float temp;
//long ltemp;
//EERIE_RGB ex_flicker;
//float ex_radius;
//float ex_frequency;
//float ex_size;
//float ex_speed;
//float ex_flaresize;
//long tl;
//unsigned long time_creation;
//long duration; // will start to fade before the end of duration...
//long sample;
//} E_LIGHT; // Aligned 1 2 4

//enum EERIE_TYPES_EXTRAS_MODE
//{
//    EXTRAS_SEMIDYNAMIC = 0x00000001,
//    EXTRAS_EXTINGUISHABLE = 0x00000002,
//    EXTRAS_STARTEXTINGUISHED = 0x00000004,
//    EXTRAS_SPAWNFIRE = 0x00000008,
//    EXTRAS_SPAWNSMOKE = 0x00000010,
//    EXTRAS_OFF = 0x00000020,
//    EXTRAS_COLORLEGACY = 0x00000040,
//    EXTRAS_NOCASTED = 0x00000080,
//    EXTRAS_FIXFLARESIZE = 0x00000100,
//    EXTRAS_FIREPLACE = 0x00000200,
//    EXTRAS_NO_IGNIT = 0x00000400,
//    EXTRAS_FLARE = 0x00000800
//};

//#define TYP_SPECIAL1 1


////*************************************************************************************
//// EERIE Types
////*************************************************************************************

//struct E_MATRIX

public enum MATERIAL {
    NONE_ = 0,
    WEAPON = 1,
    FLESH = 2,
    METAL = 3,
    GLASS = 4,
    CLOTH = 5,
    WOOD = 6,
    EARTH = 7,
    WATER = 8,
    ICE = 9,
    GRAVEL = 10,
    STONE = 11,
    FOOT_LARGE = 12,
    FOOT_BARE = 13,
    FOOT_SHOE = 14,
    FOOT_METAL = 15,
    FOOT_STEALTH = 16,
}

[Flags]
public enum POLY : int {
    NONE_ = 0,
    NO_SHADOW = 1,
    DOUBLESIDED = 1 << 1,
    TRANS = 1 << 2,
    WATER = 1 << 3,
    GLOW = 1 << 4,
    //
    IGNORE = 1 << 5,
    QUAD = 1 << 6,
    TILED = 1 << 7,
    METAL = 1 << 8,
    HIDE = 1 << 9,
    //
    STONE = 1 << 10,
    WOOD = 1 << 11,
    GRAVEL = 1 << 12,
    EARTH = 1 << 13,
    NOCOL = 1 << 14,
    LAVA = 1 << 15,
    CLIMB = 1 << 16,
    FALL = 1 << 17,
    NOPATH = 1 << 18,
    NODRAW = 1 << 19,
    PRECISE_PATH = 1 << 20,
    NO_CLIMB = 1 << 21,
    ANGULAR = 1 << 22,
    ANGULAR_IDX0 = 1 << 23,
    ANGULAR_IDX1 = 1 << 24,
    ANGULAR_IDX2 = 1 << 25,
    ANGULAR_IDX3 = 1 << 26,
    LATE_MIP = 1 << 27,
}

[StructLayout(LayoutKind.Sequential)]
public struct TLVERTEX {
    public static (string, int) Struct = ("<4f2I2f", 32);
    public Vector3 S;           // Screen coordinates
    public float Rhw;           // Reciprocal of homogeneous w
    public uint Color;          // Vertex color
    public uint Specular;       // Specular component of vertex
    public Vector2 T;           // Texture coordinates
}

public struct E_CYLINDER {
    public static (string, int) Struct = ("<5f", 20);
    public Vector3 origin;
    public float radius;
    public float height;
}

public struct E_SPHERE {
    public static (string, int) Struct = ("<4f", 16);
    public Vector3 Origin;
    public float Radius;
}

[DebuggerDisplay("Texture: {Path}")]
public class E_TEXTURE {
    public int Id;
    public string Path;
    public POLY Poly;
}

public struct E_POLY {
    public POLY Type;  // at least 16 bits
    public Vector3 Min;
    public Vector3 Max;
    public Vector3 Norm;
    public Vector3 Norm2;
    public TLVERTEX[] V; // new TLVERTEX[4];
    public TLVERTEX[] Tv; // new TLVERTEX[4];
    public Vector3[] Nrml; // new Vector3[4];
    public E_TEXTURE Tex;
    public Vector3 Center;
    public float TransVal;
    public float Area;
    public short Room;
    public short Misc;
    //public float DistBump;
    //public ushort[] UslInd;// new ushort[4];
    internal void memset() {
        Misc = 0;
    }
}

public struct E_VERTEX {
    public TLVERTEX Vert;
    public Vector3 V;
    public Vector3 Norm;
    public Vector3 VWorld;
}

public struct E_FACE {
    public int FaceType;  // 0 = flat, 1 = text, 2 = Double-Side
    public short TexId;
    public Vector3<ushort> Vid;
    public Vector3 U;
    public Vector3 V;

    public float TransVal;
    public Vector3 Norm;
    public Vector3[] Nrmls;
    public float Temp;

    public Vector3<short> Ou;
    public Vector3<short> Ov;
    public Vector2[] Color;
}

//#define MAX_PFACE 16
//struct E_PFACE
//{
//    //short faceidx[MAX_PFACE];
//    //int facetype;
//    //short texid;  //long
//    //short nbvert;
//    //float transval;
//    //ushort vid[MAX_PFACE];
//    //float u[MAX_PFACE];
//    //float v[MAX_PFACE];
//    //D3DCOLOR color[MAX_PFACE];
//}


////***********************************************************************
////*		BEGIN EERIE OBJECT STRUCTURES									*
////***********************************************************************
//struct
//{

//    short nb_Nvertex;
//short nb_Nfaces;
//short* Nvertex;
//short* Nfaces;
//} NEIGHBOURS_DATA; // Aligned 1 2 4

public struct PROGRESSIVE_DATA { // Aligned 1 2 4
    public const int SIZEOF = 16;
    // ingame data
    public short ActualCollapse; // -1 = no collapse
    public short NeedComputing;
    public float CollapseRatio;
    // static data
    public float CollapseCost;
    public short CollapseCandidate;
    public short Padd;
}

[StructLayout(LayoutKind.Sequential)]
public struct E_SPRINGS {
    public static (string, int) Struct = ("<2h3fi", 20);
    public short Startidx;
    public short Endidx;
    public float Restlength;
    public float Constant; // spring constant
    public float Damping;  // spring damping
    public int Type;
}

//#define CLOTHES_FLAG_NORMAL	0
//#define CLOTHES_FLAG_FIX	1
//#define CLOTHES_FLAG_NOCOL	2

[StructLayout(LayoutKind.Sequential)]
public struct CLOTHESVERTEX {
    public static (string, int) Struct = ("<h2b22f", 92);
    public short Idx;
    public byte Flags;
    public byte Coll;
    public Vector3 Pos;
    public Vector3 Velocity;
    public Vector3 Force;
    public float Mass; // 1.f/mass
    //
    public Vector3 T_Pos;
    public Vector3 T_Velocity;
    public Vector3 T_Force;
    //
    public Vector3 Lastpos;
}

public struct CLOTHES_DATA {
    public CLOTHESVERTEX[] Cvert; //public CLOTHESVERTEX[] backup;
    public short NumCvert;
    public short NumSprings;
    public E_SPRINGS[] Springs;
}

public struct COLLISION_SPHERE {
    public static (string, int) Struct = ("<2hf", 8);
    public short Idx;
    public short Flags;
    public float Radius;
}

public struct COLLISION_SPHERES_DATA {
    public int NumSpheres;
    public COLLISION_SPHERE[] Spheres;
}

//struct
//{

//    Vector3 initpos;
//Vector3 temp;
//Vector3 pos;
//Vector3 velocity;
//Vector3 force;
//Vector3 inertia;
//float mass;
//} PHYSVERT; // Aligned 1 2 4

//struct
//{

//    PHYSVERT* vert;
//long nb_physvert;
//short active;
//short stopcount;
//float radius; //radius around vert[0].pos for spherical collision
//float storedtiming;
//} PHYSICS_BOX_DATA; // Aligned 1 2 4


//struct
//{

//    long sx;
//long sy;
//unsigned long bpp;
//unsigned char* bmpdata;
//} EERIE_MAP; // Aligned 1 2 4


[DebuggerDisplay("Group: {Name}")]
public class E_GROUPLIST {
    public string Name;
    public int Origin;
    public int NumIndex;
    public int[] Indexes;
    public float Size;
}

[DebuggerDisplay("Task: {Name}")]
public struct E_ACTIONLIST {
    public string Name;
    public int Idx; //index vertex;
    public int Act; //action
    public int Sfx; //sfx
}

//struct
//{

//    float xmin;
//float xmax;
//float ymin;
//float ymax;
//float zmin;
//float zmax;
//} CUB3D; // Aligned 1 2 4

//struct
//{

//    long link_origin;
//Vector3 link_position;
//Vector3 scale;
//Vector3 rot;
//unsigned long flags;
//} E_MOD_INFO; // Aligned 1 2 4

//struct
//{

//    long lgroup; //linked to group n� if lgroup=-1 NOLINK
//long lidx;
//long lidx2;
//void* obj;
//E_MOD_INFO modinfo;
//void* io;
//} E_LINKED; // Aligned 1 2 4


[DebuggerDisplay("Selection: {Name}")]
public struct E_SELECTIONS {
    public string Name;
    public int NumSelected; public int[] Selected;
}

//#define DRAWFLAG_HIGHLIGHT	1

public struct E_FASTACCESS {
    public short ViewAttach;
    public short PrimaryAttach;
    public short LeftAttach;
    public short WeaponAttach;
    public short SecondaryAttach;
    public short MouthGroup;
    public short JawGroup;
    public short HeadGroupOrigin;
    public short HeadGroup;
    public short MouthGroupOrigin;
    public short VRight;
    public short URight;
    public short Fire;
    public short SelHead;
    public short SelChest;
    public short SelLeggings;
    public short CarryAttach;
    public short _Padd;
}

public struct E_BONE {
    public int NumIdxVertices; public int[] IdxVertices;
    public E_GROUPLIST OriginalGroup;
    public int Father;
    public Quaternion QuatAnim; public Vector3 TransAnim; public Vector3 ScaleAnim;
    public Quaternion QuatLast; public Vector3 TransLast; public Vector3 ScaleLast;
    public Quaternion QuatInit; public Vector3 TransInit; public Vector3 ScaleInit;
    public Vector3 TransInitGlobal;
    public void AddIdxToBone(int idx) { Array.Resize(ref IdxVertices, NumIdxVertices + 1); IdxVertices[NumIdxVertices++] = idx; }
}

public class E_CDATA {
    public E_BONE[] Bones; public int NumBones;
}

////////////////////////////////////////////////////////////////////////////////////
//struct
//{

//    float x;
//float y;
//float z;
//float w;
//} EERIE_3DPAD;

[DebuggerDisplay("Obj: {File}")]
public class E_3DOBJ {
    //public string Name;
    public string File;
    //public Vector3 Pos;
    public Vector3 Point0;
    //public Vector3 Angle;
    public int Origin;
    //public int Ident;
    public int NumVertex;
    //public int TrueNumVertex;
    public int NumFaces;
    public int NumPfaces;
    public int NumMaps;
    public int NumGroups;
    public int NumAction;
    public int NumSelections;
    //public uint DrawFlags;
    public Vector4[] VertexLocal;
    public E_VERTEX[] VertexList; //public E_VERTEX[] VertexList3;

    public E_FACE[] FaceList;
    //public E_PFACE* PfaceList;
    //public E_MAP* MapList;
    public E_GROUPLIST[] GroupList;
    public E_ACTIONLIST[] ActionList;
    public E_SELECTIONS[] Selections;
    public E_TEXTURE[] Textures;

    //public char* OriginalTextures;
    //public CUB3D Cub;
    //public E_QUAT Quat;
    //public E_LINKED* Linked;
    //public int NumLinked;

    //public PHYSICS_BOX_DATA Pbox;
    //public PROGRESSIVE_DATA Pdata;
    //public NEIGHBOURS_DATA Ndata;
    public CLOTHES_DATA Cdata;
    public COLLISION_SPHERES_DATA Sdata;
    public E_FASTACCESS FastAccess;
    public E_CDATA CData;

    internal void CenterObjectCoordinates() {
        var offset = VertexList[Origin].V;
        if (offset.X == 0 && offset.Y == 0 && offset.Z == 0) return;
        Log.Info($"NOT CENTERED {File}\n");
        for (var i = 0; i < NumVertex; i++) { VertexList[i].V -= offset; VertexList[i].Vert.S -= offset; }
        Point0 -= offset;
    }

    internal void CreateCedricData() {
        int GetFather(int origin, int startGroup) {
            for (var i = startGroup; i >= 0; i--)
                for (var j = 0; j < GroupList[i].NumIndex; j++)
                    if (GroupList[i].Indexes[j] == origin) return i;
            return -1;
        }

        bool[] temp; int i;
        CData = new E_CDATA();
        if (NumGroups <= 0) {
            CData.NumBones = 1; CData.Bones = new E_BONE[1];
            ref E_BONE s = ref CData.Bones[0];
            for (i = 0; i < NumVertex; i++) s.AddIdxToBone(i);
            s.TransInitGlobal = s.TransInit;
            s.OriginalGroup = null;
            s.Father = -1;
        }
        else {
            CData.NumBones = NumGroups; CData.Bones = new E_BONE[CData.NumBones];
            temp = new bool[NumVertex];
            for (i = NumGroups - 1; i >= 0; i--) {
                ref E_BONE s = ref CData.Bones[i];
                var vorigin = VertexList[GroupList[i].Origin];
                for (var j = 0; j < GroupList[i].NumIndex; j++)
                    if (!temp[GroupList[i].Indexes[j]]) { temp[GroupList[i].Indexes[j]] = true; s.AddIdxToBone(GroupList[i].Indexes[j]); }
                s.TransInit = vorigin.V;
                s.TransInitGlobal = s.TransInit;
                s.OriginalGroup = GroupList[i];
                s.Father = GetFather(GroupList[i].Origin, i - 1);
            }

            // Try to correct lonely vertex
            for (i = 0; i < NumVertex; i++) {
                var ok = false;
                for (var j = 0; j < NumGroups; j++) {
                    for (var k = 0; k < GroupList[j].NumIndex; k++)
                        if (GroupList[j].Indexes[k] == i) { ok = true; break; }
                    if (ok) break;
                }
                if (!ok) CData.Bones[0].AddIdxToBone(i);
            }

            for (i = NumGroups - 1; i >= 0; i--) {
                ref E_BONE s = ref CData.Bones[i];
                if (s.Father >= 0) s.TransInit -= CData.Bones[s.Father].TransInit;
                s.TransInitGlobal = s.TransInit;
            }
        }

        // Build proper mesh
        E_CDATA obj = CData;
        for (i = 0; i != obj.NumBones; i++) {
            ref E_BONE s = ref obj.Bones[i];
            if (s.Father >= 0) {
                ref E_BONE f = ref obj.Bones[s.Father];
                s.QuatAnim = f.QuatAnim * s.QuatInit; // Rotation
                TransformVertexQuat(ref f.QuatAnim, ref s.TransInit, ref s.TransAnim); // Translation
                s.TransAnim = f.TransAnim + s.TransAnim;
                s.ScaleAnim = new Vector3(1f, 1f, 1f); // Scale
            }
            else {
                s.QuatAnim = s.QuatInit; // Rotation
                s.TransAnim = s.TransInit; // Translation
                s.ScaleAnim = new Vector3(1f, 1f, 1f); // Scale
            }
        }
        VertexLocal = new Vector4[NumVertex];
        for (i = 0; i != obj.NumBones; i++) {
            ref E_BONE s = ref obj.Bones[i];
            var vec = s.TransAnim;
            for (var v = 0; v != s.NumIdxVertices; v++) {
                var t = VertexList[s.IdxVertices[v]].V - vec;
                TransformInverseVertexQuat(ref s.QuatAnim, ref t, ref t);
                VertexLocal[s.IdxVertices[v]] = new Vector4(t.X, t.Y, t.Z, 0f);
            }
        }
    }

    internal void PrecomputeFastAccess() {
        short GetSelection(string selName) {
            for (var i = 0; i < NumSelections; i++)
                if (Selections[i].Name.Equals(selName, StringComparison.OrdinalIgnoreCase)) return (short)i;
            return -1;
        }
        short GetGroup(string groupName) {
            for (var i = 0; i < NumGroups; i++)
                if (GroupList[i].Name.Equals(groupName, StringComparison.OrdinalIgnoreCase)) return (short)i;
            return -1;
        }
        short GetActionPointIdx(string text) {
            for (var i = 0; i < NumAction; i++)
                if (ActionList[i].Name.Equals(text, StringComparison.OrdinalIgnoreCase)) return (short)ActionList[i].Idx;
            return -1;
        }
        short mouthGroup, headGroup;
        FastAccess = new E_FASTACCESS {
            VRight = GetActionPointIdx("V_RIGHT"),
            URight = GetActionPointIdx("U_RIGHT"),
            ViewAttach = GetActionPointIdx("View_attach"),
            PrimaryAttach = GetActionPointIdx("PRIMARY_ATTACH"),
            LeftAttach = GetActionPointIdx("LEFT_ATTACH"),
            WeaponAttach = GetActionPointIdx("WEAPON_ATTACH"),
            SecondaryAttach = GetActionPointIdx("SECONDARY_ATTACH"),
            JawGroup = GetGroup("jaw"),
            MouthGroup = mouthGroup = GetGroup("mouth all"),
            MouthGroupOrigin = (short)(mouthGroup == -1 ? -1 : GroupList[mouthGroup].Origin),
            HeadGroup = headGroup = GetGroup("head"),
            HeadGroupOrigin = (short)(headGroup == -1 ? -1 : GroupList[headGroup].Origin),
            Fire = GetActionPointIdx("FIRE"),
            CarryAttach = GetActionPointIdx("CARRY_ATTACH"),
            SelHead = GetSelection("head"),
            SelChest = GetSelection("chest"),
            SelLeggings = GetSelection("leggings"),
        };
    }

    void TransformVertexQuat(ref Quaternion q, ref Vector3 s, ref Vector3 t) {
        float rx = s.X * q.W - s.Y * q.Z + s.Z * q.Y, ry = s.Y * q.W - s.Z * q.X + s.X * q.Z, rz = s.Z * q.W - s.X * q.Y + s.Y * q.X, rw = s.X * q.X + s.Y * q.Y + s.Z * q.Z;
        t.X = q.W * rx + q.X * rw + q.Y * rz - q.Z * ry; t.Y = q.W * ry + q.Y * rw + q.Z * rx - q.X * rz; t.Z = q.W * rz + q.Z * rw + q.X * ry - q.Y * rx;
    }

    void TransformInverseVertexQuat(ref Quaternion q, ref Vector3 s, ref Vector3 t) {
        var p = Quaternion.Inverse(q);
        float x = s.X, y = s.Y, z = s.Z;
        float qx = p.X, qy = p.Y, qz = p.Z, qw = p.W;
        float rx = x * qw - y * qz + z * qy, ry = y * qw - z * qx + x * qz, rz = z * qw - x * qy + y * qx, rw = x * qx + y * qy + z * qz;
        t.X = qw * rx + qx * rw + qy * rz - qz * ry; t.Y = qw * ry + qy * rw + qz * rx - qx * rz; t.Z = qw * rz + qz * rw + qx * ry - qy * rx;
    }
}

//struct
//{
//    long nbobj;
//E_3DOBJ** objs;
//Vector3 pos;
//Vector3 point0;
//long nbtex;
//TextureContainer** texturecontainer;
//long nblight;
//E_LIGHT** light;
//float ambient_r;
//float ambient_g;
//float ambient_b;
//CUB3D cub;
//} E_3DSCENE; // Aligned 1 2 4


//#define MAX_SCENES 64
//struct
//{
//    long nb_scenes;
//E_3DSCENE* scenes[MAX_SCENES];
//CUB3D cub;
//Vector3 pos;
//Vector3 point0;
//} E_MULTI3DSCENE; // Aligned 1 2 4

//struct
//{
//    long num_frame;
//long flag;
//int master_key_frame;
//short f_translate; //int
//short f_rotate; //int
//float time;
//Vector3 translate;
//EERIE_QUAT quat;
//long sample;
//} E_FRAME; // Aligned 1 2 4

//struct
//{
//    int key;
//Vector3 translate;
//EERIE_QUAT quat;
//Vector3 zoom;
//} E_GROUP; // Aligned 1 2 4

//// Animation playing flags
//#define EA_LOOP			1	// Must be looped at end (indefinitely...)
//#define EA_REVERSE		2	// Is played reversed (from end to start)
//#define EA_PAUSED		4	// Is paused
//#define EA_ANIMEND		8	// Has just finished
//#define	EA_STATICANIM	16	// Is a static Anim (no movement offset returned).
//#define	EA_STOPEND		32	// Must Be Stopped at end.
//#define EA_FORCEPLAY	64	// User controlled... MUST be played...
//#define EA_EXCONTROL	128	// ctime externally set, no update.
//struct
//{

//    float anim_time;
//unsigned long flag;
//long nb_groups;
//long nb_key_frames;
//E_FRAME* frames;
//E_GROUP* groups;
//unsigned char* voidgroups;
//} E_ANIM; // Aligned 1 2 4

////-------------------------------------------------------------------------
//Portal Data;

[StructLayout(LayoutKind.Sequential)]
public struct SAVE_EPOLY {
    public POLY Type;  // at least 16 bits
    public Vector3 Min; public Vector3 Max;
    public Vector3 Norm; public Vector3 Norm2;
    public TLVERTEX V0; public TLVERTEX V1; public TLVERTEX V2; public TLVERTEX V3;
    public TLVERTEX Tv0; public TLVERTEX Tv1; public TLVERTEX Tv2; public TLVERTEX Tv3;
    public Vector3 Nrml0; public Vector3 Nrml1; public Vector3 Nrml2; public Vector3 Nrml3;
    public int TexPtr;
    public Vector3 Center;
    public float TransVal;
    public float Area;
    public short Room;
    public short Misc;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct E_SAVE_PORTALS {
    public static (string, int) Struct = ("<?", sizeof(E_SAVE_PORTALS));
    public SAVE_EPOLY Poly;
    public int Room1; // facing normal
    public int Room2;
    public short UsePortal;
    public short Paddy;
}

public unsafe struct E_PORTALS {
    public static (string, int) Struct = ("<?", sizeof(E_PORTALS));
    public E_POLY Poly;
    public int Room1; // facing normal
    public int Room2;
    public short UsePortal;
    public short Paddy;

    internal void memset() {
    }
}

public struct EP_DATA {
    public static (string, int) Struct = ("<4h", 8);
    public short Px;
    public short Py;
    public short Idx;
    public short Padd;
}

public class E_ROOM_DATA {
    public int NumPortals;
    public int[] Portals;
    public int NumPolys;
    public EP_DATA[] EpData;
    public Vector3 Center;
    public float Radius;
    public ushort[] PussIndice;
    //public LPDIRECT3DVERTEXBUFFER7 VertexBuffer;
    public uint NumTextures;
    public E_TEXTURE TextureContainer;
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct E_SAVE_ROOM_DATA {
    public static (string, int) Struct = ("<2i6i", sizeof(E_SAVE_ROOM_DATA));
    public int NumPortals;
    public int NumPolys;
    public fixed int Padd[6];
}

public class E_PORTAL_DATA {
    public int NumRooms;
    public E_ROOM_DATA[] Room;
    public int NumTotal;  // of portals
    public E_PORTALS[] Portals;
}

//#define ARX_D3DVERTEX D3DTLVERTEX


//struct
//{
//    float x, y, z;
//int color;
//float tu, tv;
//} SMY_D3DVERTEX;

//struct
//{
//    float x, y, z;
//int color;
//float tu, tv;
//float tu2, tv2;
//float tu3, tv3;
//} SMY_D3DVERTEX3;

//struct
//{
//    float x, y, z;
//float rhw;
//int color;
//float tu, tv;
//float tu2, tv2;
//float tu3, tv3;
//} SMY_D3DVERTEX3_T;

//struct
//{
//    D3DTLVERTEX pD3DVertex[3];
//float uv[6];
//float color[3];
//} SMY_ZMAPPINFO;

//struct
//{
//    unsigned long uslStartVertex;
//unsigned long uslNbVertex;

//unsigned long uslStartCull;
//unsigned long uslNbIndiceCull;
//unsigned long uslStartNoCull;
//unsigned long uslNbIndiceNoCull;

//unsigned long uslStartCull_TNormalTrans;
//unsigned long uslNbIndiceCull_TNormalTrans;
//unsigned long uslStartNoCull_TNormalTrans;
//unsigned long uslNbIndiceNoCull_TNormalTrans;

//unsigned long uslStartCull_TMultiplicative;
//unsigned long uslNbIndiceCull_TMultiplicative;
//unsigned long uslStartNoCull_TMultiplicative;
//unsigned long uslNbIndiceNoCull_TMultiplicative;

//unsigned long uslStartCull_TAdditive;
//unsigned long uslNbIndiceCull_TAdditive;
//unsigned long uslStartNoCull_TAdditive;
//unsigned long uslNbIndiceNoCull_TAdditive;

//unsigned long uslStartCull_TSubstractive;
//unsigned long uslNbIndiceCull_TSubstractive;
//unsigned long uslStartNoCull_TSubstractive;
//unsigned long uslNbIndiceNoCull_TSubstractive;
//} SMY_ARXMAT;

//class CMY_DYNAMIC_VERTEXBUFFER
//{
//    public:
//		unsigned long uslFormat;
//    unsigned short ussMaxVertex;
//    unsigned short ussNbVertex;
//    unsigned short ussNbIndice;
//    LPDIRECT3DVERTEXBUFFER7 pVertexBuffer;
//    unsigned short* pussIndice;
//    public:
//		CMY_DYNAMIC_VERTEXBUFFER(unsigned short, unsigned long);
//    ~CMY_DYNAMIC_VERTEXBUFFER();

//    void* Lock(unsigned int);
//    bool UnLock();
//};

//#define FVF_D3DVERTEX	(D3DFVF_XYZ|D3DFVF_DIFFUSE|D3DFVF_TEX1|D3DFVF_TEXTUREFORMAT2)
//#define FVF_D3DVERTEX2	(D3DFVF_XYZ|D3DFVF_DIFFUSE|D3DFVF_TEX2|D3DFVF_TEXTUREFORMAT2)
//#define FVF_D3DVERTEX3	(D3DFVF_XYZ|D3DFVF_DIFFUSE|D3DFVF_TEX3|D3DFVF_TEXTUREFORMAT2)

//#define FVF_D3DVERTEX_T		(D3DFVF_XYZRHW|D3DFVF_DIFFUSE|D3DFVF_TEX1|D3DFVF_TEXTUREFORMAT2)
//#define FVF_D3DVERTEX2_T	(D3DFVF_XYZRHW|D3DFVF_DIFFUSE|D3DFVF_TEX2|D3DFVF_TEXTUREFORMAT2)
//#define FVF_D3DVERTEX3_T	(D3DFVF_XYZRHW|D3DFVF_DIFFUSE|D3DFVF_TEX3|D3DFVF_TEXTUREFORMAT2)

//extern long USE_PORTALS;
//extern EERIE_PORTAL_DATA* portals;