using GameX.Crytek.Formats.Models;
using GameX.Uncore.Formats;
using MathNet.Numerics;
using MathNet.Numerics.Financial;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
#pragma warning disable CS0649, CS8500

namespace GameX.Arkane.Formats.Danae;

#region Binary_Ftl
// https://github.com/OpenSourcedGames/Arx-Fatalis/blob/master/Sources/DANAE/ARX_FTL.cpp#L575

public class Binary_Ftl : IHaveMetaInfo, IWriteToStream {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Ftl(r));

    #region Headers

    const int FTL_MAGIC = 0x004c5446;
    const float FTL_VERSION = 0.83257f;

    [StructLayout(LayoutKind.Sequential)]
    struct FTL_HEADER {
        public static (string, int) Struct = ("<6i", 24);
        public int Offset3Ddata;                // -1 = no
        public int OffsetCylinder;              // -1 = no
        public int OffsetProgressiveData;       // -1 = no
        public int OffsetClothesData;           // -1 = no
        public int OffsetCollisionSpheres;      // -1 = no
        public int OffsetPhysicsBox;            // -1 = no
    }

    [StructLayout(LayoutKind.Sequential)]
    struct FTL_3DHEADER {
        public static (string, int) Struct = ("<7i256s", 28 + 256);
        public int NumVertex;
        public int NumFaces;
        public int NumMaps;
        public int NumGroups;
        public int NumAction;
        public int NumSelections;
        public int Origin;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string Name;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct FTL_VERTEX {
        public static (string, int) Struct = ($"{TLVERTEX.Struct.Item1}6f", 32 + 24);
        public TLVERTEX Vert;
        public Vector3 V;
        public Vector3 Norm;
        public static implicit operator E_VERTEX(FTL_VERTEX s) {
            s.Vert.Color = 0xFF000000;
            return new E_VERTEX {
                Vert = s.Vert,
                V = s.V,
                Norm = s.Norm,
                VWorld = default,
            };
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct FTL_TEXTURE {
        public static (string, int) Struct = ("<256s", 256);
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string Name;
        public static implicit operator E_TEXTURE(FTL_TEXTURE s) {
            var name = s.Name;
            POLY poly = 0;
            if (name.Contains("NPC_")) poly |= POLY.LATE_MIP;
            if (name.Contains("nocol")) poly |= POLY.NOCOL;
            if (name.Contains("climb")) poly |= POLY.CLIMB; // change string depending on GFX guys
            if (name.Contains("fall")) poly |= POLY.FALL;
            if (name.Contains("lava")) poly |= POLY.LAVA;
            if (name.Contains("water")) poly |= POLY.WATER | POLY.TRANS;
            else if (name.Contains("spider_web")) poly |= POLY.WATER | POLY.TRANS;
            else if (name.Contains("[metal]")) poly |= POLY.METAL;
            return new E_TEXTURE {
                Path = s.Name,
                Poly = poly,
            };
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct FTL_FACE {
        public static (string, int) Struct = ("<4i3Hh6f6h14f", 116);
        public int FaceType;  // 0 = flat, 1 = text, 2 = Double-Side
        public Vector3<int> Rgb;
        public Vector3<ushort> Vid;
        public short TexId;
        public Vector3 U;
        public Vector3 V;
        public Vector3<short> Ou;
        public Vector3<short> Ov;
        public float TransVal;
        public Vector3 Norm;
        public Vector3 Nrmls0; public Vector3 Nrmls1; public Vector3 Nrmls2;
        public float Temp;
        public static implicit operator E_FACE(FTL_FACE s) => new() {
            FaceType = s.FaceType,
            TexId = s.TexId,
            U = s.U,
            V = s.V,
            Ou = s.Ou,
            Ov = s.Ov,
            TransVal = s.TransVal,
            Norm = s.Norm,
            Nrmls = [s.Nrmls0, s.Nrmls1, s.Nrmls2],
            Temp = s.Temp,
        };
    }

    [StructLayout(LayoutKind.Sequential)]
    struct FTL_GROUPLIST {
        public static (string, int) Struct = ("<256s3if", 256 + 16);
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string Name;
        public int Origin;
        public int NumIndex;
        public int Trash; // Indexes;
        public float Size;
        public static implicit operator E_GROUPLIST(FTL_GROUPLIST s) => new() {
            Name = s.Name,
            Origin = s.Origin,
            NumIndex = s.NumIndex,
            Size = s.Size,
        };
    }

    [StructLayout(LayoutKind.Sequential)]
    struct FTL_ACTIONLIST {
        public static (string, int) Struct = ("<256s3i", 256 + 12);
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string Name;
        public int Idx; //index vertex;
        public int Act; //action
        public int Sfx; //sfx
        public static implicit operator E_ACTIONLIST(FTL_ACTIONLIST s) => new() {
            Name = s.Name,
            Idx = s.Idx,
            Act = s.Act,
            Sfx = s.Sfx,
        };
    }

    [StructLayout(LayoutKind.Sequential)]
    struct FTL_SELECTIONS {
        public static (string, int) Struct = ("<64s2i", 64 + 8);
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)] string Name;
        public int NumSelected;
        public int Trash; //Selected;
        public static implicit operator E_SELECTIONS(FTL_SELECTIONS s) => new() {
            Name = s.Name,
            NumSelected = s.NumSelected,
        };
    }

    #endregion

    public readonly E_3DOBJ Obj;

    public Binary_Ftl(BinaryReader r) {
        var obj = Obj = new E_3DOBJ();
        var magic = r.ReadUInt32();
        if (magic != FTL_MAGIC) throw new FormatException($"Invalid FTL magic: \"{magic}\".");
        var version = r.ReadSingle();
        if (version != FTL_VERSION) throw new FormatException($"Invalid FLT version: \"{version}\".");
        r.Skip(512); // skip checksum
        var header = r.ReadS<FTL_HEADER>();

        // Check For & Load 3D Data
        if (header.Offset3Ddata != -1) {
            E_GROUPLIST _groupZ(FTL_GROUPLIST s) { var z = (E_GROUPLIST)s; z.Indexes = z.NumIndex > 0 ? r.ReadPArray<int>("i", z.NumIndex) : null; return z; }
            E_SELECTIONS _selectionsZ(FTL_SELECTIONS s) { var z = (E_SELECTIONS)s; z.Selected = r.ReadPArray<int>("i", z.NumSelected); return z; }
            r.Seek(header.Offset3Ddata);
            var s = r.ReadS<FTL_3DHEADER>();
            obj.NumVertex = s.NumVertex;
            obj.NumFaces = s.NumFaces;
            obj.NumMaps = s.NumMaps;
            obj.NumGroups = s.NumGroups;
            obj.NumAction = s.NumAction;
            obj.NumSelections = s.NumSelections;
            obj.Origin = s.Origin;
            obj.File = s.Name;
            obj.Vertexs = s.NumVertex > 0 ? [.. r.ReadSArray<FTL_VERTEX>(s.NumVertex).Cast<E_VERTEX>()] : null; obj.Point0 = s.NumVertex > 0 ? obj.Vertexs[obj.Origin].V : default;
            obj.Faces = s.NumFaces > 0 ? [.. r.ReadSArray<FTL_FACE>(s.NumFaces).Cast<E_FACE>()] : null;
            obj.Textures = s.NumMaps > 0 ? [.. r.ReadSEach<FTL_TEXTURE>(s.NumMaps).Cast<E_TEXTURE>()] : null;
            obj.Groups = s.NumGroups > 0 ? [.. r.ReadSEach<FTL_GROUPLIST>(s.NumGroups).Select(_groupZ)] : null;
            obj.Actions = s.NumAction > 0 ? [.. r.ReadSEach<FTL_ACTIONLIST>(s.NumAction).Cast<E_ACTIONLIST>()] : null;
            obj.Selections = s.NumSelections > 0 ? [.. r.ReadSEach<FTL_SELECTIONS>(s.NumSelections).Select(_selectionsZ)] : null;
        }

        // collision spheres
        if (header.OffsetCollisionSpheres != -1) {
            r.Seek(header.OffsetCollisionSpheres);
            obj.Spheres = r.ReadL32SArray<COLLISION_SPHERE>();
        }

        // progressive data
        if (header.OffsetProgressiveData != -1) {
            r.Seek(header.OffsetProgressiveData);
            var numVertex = r.ReadInt32();
            r.Skip(PROGRESSIVE_DATA.SIZEOF * numVertex);
        }

        // clothes data
        if (header.OffsetClothesData != -1) {
            r.Seek(header.OffsetClothesData);
            int numCvert = r.ReadInt32(), numSprings = r.ReadInt32();
            obj.Cdata = new CLOTHES_DATA {
                Cvert = r.ReadSArray<CLOTHESVERTEX>(numCvert),
                Springs = r.ReadSArray<E_SPRINGS>(numSprings),
            };
        }

        // process
        obj.CenterObjectCoordinates();
        obj.CreateCedricData();
        obj.PrecomputeFastAccess();
    }

    public void WriteToStream(Stream stream) => this.Serialize(stream);
    public override string ToString() => this.Serialize();

    // IHaveMetaInfo
    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => [
        new(null, new MetaContent { Type = "Text", Name = "Name", Value = this }),
        new("FTL", items: [
            new($"Obj: {Obj}"),
        ])
    ];
}

#endregion

#region Binary_Fts
// https://github.com/OpenSourcedGames/Arx-Fatalis/blob/master/Sources/EERIE/EERIEPoly.cpp#L3755

public unsafe class Binary_Fts : IHaveMetaInfo, IWriteToStream {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Fts(r));

    #region Headers

    struct ANCHOR_DATA {
        public Vector3 Pos;
        public short NumLinked;
        public short Flags;
        public int[] Linked;
        public float Radius;
        public float Height;
    }

    public struct E_BKG_INFO {
        public byte Treat;
        public bool Nothing;
        public short NumPoly;
        public short NumIAnchors;
        public short NumPolyin;
        public float FrustrumMinY;
        public float FrustrumMaxY;
        public E_POLY[] Polydata;
        //public E_POLY[][] Polyin;
        public int[] IAnchors; // index on anchors list
        public int Flags;
        public float TileMinY;
        public float TileMaxY;
        //public static implicit operator E_BKG_INFO(F_SCENE_INFO s) => new() {
        //    FaceType = s.FaceType,
        //    TexId = s.TexId,
        //    U = s.U,
        //    V = s.V,
        //    Ou = s.Ou,
        //    Ov = s.Ov,
        //    TransVal = s.TransVal,
        //    Norm = s.Norm,
        //    Nrmls = [s.Nrmls0, s.Nrmls1, s.Nrmls2],
        //    Temp = s.Temp,
        //};
    }

    struct E_SMINMAX {
        public short Min;
        public short Max;
    }

    //const int MAX_GOSUB = 10;
    //const int MAX_SHORTCUT = 80;
    //const int MAX_SCRIPTTIMERS = 5;
    //const int FBD_TREAT = 1;
    //const int FBD_NOTHING = 2;

    struct F_BKG_DATA {
        public byte Treat;
        public byte Nothing;
        public short NumPoly;
        public short NumIAnchors;
        public short NumPolyin;
        public int Flags;
        public float FrustrumMinY;
        public float FrustrumMaxY;
        public E_POLY[] Polydata;
        public E_POLY[][] Polyin;
        public int[] IAnchors; // index on anchors list
    }

    const int MAX_BKGX = 160, MAX_BKGZ = 160, BKG_SIZX = 100, BKG_SIZZ = 100;

    class E_BACKGROUND {
        public F_BKG_DATA[,] fastdata = new F_BKG_DATA[MAX_BKGX, MAX_BKGZ];
        public int exist = 1;
        public short XSize;
        public short ZSize;
        public short Xdiv;
        public short Zdiv;
        public float Xmul;
        public float Zmul;
        public E_BKG_INFO[] Backg;
        public Vector3 Ambient;
        public Vector3 Ambient255;
        public E_SMINMAX[] MinMax;
        public int NumAnchors;
        public ANCHOR_DATA[] Anchors;
        public string Name;
        public E_BACKGROUND(short sx = MAX_BKGX, short sz = MAX_BKGZ, short xdiv = BKG_SIZX, short zdiv = BKG_SIZZ) {
            XSize = sx;
            ZSize = sz;
            if (xdiv < 0) xdiv = 1;
            if (zdiv < 0) zdiv = 1;
            Xdiv = xdiv;
            Zdiv = zdiv;
            Xmul = 1f / Xdiv;
            Zmul = 1f / Zdiv;
            Backg = new E_BKG_INFO[sx * sz];
            for (var i = 0; i < Backg.Length; i++) Backg[i].Nothing = true;
            MinMax = new E_SMINMAX[sz];
            for (var i = 0; i < MinMax.Length; i++) { MinMax[i].Min = 9999; MinMax[i].Max = -1; }
        }
    }

    //const float NON_PORTAL_VERSION = 0.136f;
    const float FTS_VERSION = 0.141f;
    //const int SIZ_WRK = 10;

    [StructLayout(LayoutKind.Sequential)]
    struct FTS_HEADER {
        public static (string, int) Struct = ("<256sifi3i", 256 + 24);
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string Path;
        public int Count;
        public float Version;
        public int Compressedsize;
        public fixed int Pad[3];
    }

    [StructLayout(LayoutKind.Sequential)]
    struct FTS_HEADER2 {
        public static (string, int) Struct = ("<256s", 256);
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string Path;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct F_VERTEX {
        //public static (string, int) Struct = ("<5f", 20);
        public float sy;
        public float ssx;
        public float ssz;
        public float stu;
        public float stv;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct F_POLY {
        public static (string, int) Struct = ("<20fi20fi2h", 172);
        public F_VERTEX V0; public F_VERTEX V1; public F_VERTEX V2; public F_VERTEX V3;
        public int TexPtr;
        public Vector3 Norm; public Vector3 Norm2;
        public Vector3 Nrml0; public Vector3 Nrml1; public Vector3 Nrml2; public Vector3 Nrml3;
        public float TransVal;
        public float Area;
        public POLY Type;
        public short Room;
        public short Paddy;
        public E_POLY To(E_TEXTURE[] textures, E_BACKGROUND bkg) {
            static void DeclareEGInfo(E_BACKGROUND bkg, float x, float y, float z) {
                var posx = (int)(float)(x * bkg.Xmul);
                if (posx < 0) return;
                else if (posx >= bkg.XSize) return;
                var posz = (int)(float)(z * bkg.Zmul);
                if (posz < 0) return;
                else if (posz >= bkg.ZSize) return;
                ref E_BKG_INFO eg = ref bkg.Backg[posx + posz * bkg.XSize];
                eg.Nothing = false;
            }

            var texPtr = TexPtr;
            var t = new E_POLY {
                Room = Room,
                Area = Area,
                Norm = Norm,
                Norm2 = Norm2,
                Nrml = [Nrml0, Nrml1, Nrml2, Nrml3],
                Tex = texPtr != 0 ? textures.FirstOrDefault(x => x.Id == texPtr) : null,
                TransVal = TransVal,
                Type = Type,
                V = [
                    new() { Color = 0xFFFFFFFF, Rhw = 1f, Specular = 1, S = new Vector3(V0.ssx, V0.sy, V0.ssz), T = new Vector2(V0.stu, V0.stv) },
                    new() { Color = 0xFFFFFFFF, Rhw = 1f, Specular = 1, S = new Vector3(V1.ssx, V1.sy, V1.ssz), T = new Vector2(V1.stu, V1.stv) },
                    new() { Color = 0xFFFFFFFF, Rhw = 1f, Specular = 1, S = new Vector3(V2.ssx, V2.sy, V2.ssz), T = new Vector2(V2.stu, V2.stv) },
                    new() { Color = 0xFFFFFFFF, Rhw = 1f, Specular = 1, S = new Vector3(V3.ssx, V3.sy, V3.ssz), T = new Vector2(V3.stu, V3.stv) },
                ]
            };

            // clone v
            t.Tv = (TLVERTEX[])t.V.Clone();
            for (var kk = 0; kk < 4; kk++) t.Tv[kk].Color = 0xFF000000;

            // re-center
            int to; float div;
            if ((Type & POLY.QUAD) != 0) { to = 4; div = 0.25f; }
            else { to = 3; div = 0.333333333333f; }
            t.Center = Vector3.Zero;
            for (var h = 0; h < to; h++) {
                t.Center += t.V[h].S;
                if (h != 0) {
                    t.Max.X = Math.Max(t.Max.X, t.V[h].S.X); t.Min.X = Math.Min(t.Min.X, t.V[h].S.X);
                    t.Max.Y = Math.Max(t.Max.Y, t.V[h].S.Y); t.Min.Y = Math.Min(t.Min.Y, t.V[h].S.Y);
                    t.Max.Z = Math.Max(t.Max.Z, t.V[h].S.Z); t.Min.Z = Math.Min(t.Min.Z, t.V[h].S.Z);
                }
                else t.Min = t.Max = t.V[0].S;
            }
            t.Center *= div;

            // distance
            var dist = 0f;
            for (var h = 0; h < to; h++) dist = Math.Max(dist, Vector3.Distance(t.V[h].S, t.Center));
            t.V[0].Rhw = dist;

            // declare
            DeclareEGInfo(bkg, t.Center.X, t.Center.Y, t.Center.Z);
            DeclareEGInfo(bkg, t.V[0].S.X, t.V[0].S.Y, t.V[0].S.Z);
            DeclareEGInfo(bkg, t.V[1].S.X, t.V[1].S.Y, t.V[1].S.Z);
            DeclareEGInfo(bkg, t.V[2].S.X, t.V[2].S.Y, t.V[2].S.Z);
            if ((Type & POLY.QUAD) != 0) DeclareEGInfo(bkg, t.V[3].S.X, t.V[3].S.Y, t.V[3].S.Z);
            return t;
        }
    }

    struct F_SCENE_HEADER {
        public static (string, int) Struct = ("<f5i6f2i", 56);
        public float Version;
        public int SizeX;
        public int SizeZ;
        public int NumTextures;
        public int NumPolys;
        public int NumAnchors;
        public Vector3 PlayerPos;
        public Vector3 MscenePos;
        public int NumPortals;
        public int NumRooms;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct F_TEXTURE_CONTAINER {
        public static (string, int) Struct = ("<2i256s", 8 + 256);
        public int TcPtr;
        public int TempPtr;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string Fic;
        public static implicit operator E_TEXTURE(F_TEXTURE_CONTAINER s) => new() {
            Id = s.TcPtr,
            Path = s.Fic
        };
    }

    [StructLayout(LayoutKind.Sequential)]
    struct F_ANCHOR_DATA {
        public static (string, int) Struct = ("<5f2h", 24);
        public Vector3 Pos;
        public float Radius;
        public float Height;
        public short NumLinked;
        public short Flags;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct F_SCENE_INFO {
        public static (string, int) Struct = ("<2I", 8);
        public int NumPoly;
        public int NumIAnchors;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ROOM_DIST_DATA { //:includes ROOM_DIST_DATA_SAVE
        public static (string, int) Struct = ("<7f", 28);
        public float Distance; // -1 means use truedist
        public Vector3 StartPos;
        public Vector3 EndPos;
    }

    public class F_LEVEL {
        public Vector3 PlayerPos;
        public Vector3 MscenePos;
        public E_TEXTURE[] Textures;
        public E_BKG_INFO[] Backg;
        public E_PORTAL_DATA Portals;
        public int NumRoomDistance;
        public ROOM_DIST_DATA[] RoomDistance;
    }

    #endregion

    public readonly F_LEVEL Level;
    readonly E_BACKGROUND Bkg;

    public Binary_Fts(BinaryReader r) {
        static void SetRoomDistance(F_LEVEL level, long i, long j, ROOM_DIST_DATA rdd) {
            if (i < 0 || j < 0 || i >= level.NumRoomDistance || j >= level.NumRoomDistance || level.RoomDistance == null) return;
            level.RoomDistance[i + j * level.NumRoomDistance] = rdd;
        }

        var header = r.ReadS<FTS_HEADER>();
        if (header.Version != FTS_VERSION) throw new FormatException("BAD MAGIC");
        if (header.Count > 0) {
            var count = 0;
            while (count < header.Count) {
                r.ReadS<FTS_HEADER2>();
                r.Skip(512); // skip check
                count++;
                if (count > 60) throw new FormatException("BAD HEADER");
            }
        }

        int i, j;
        Level = new F_LEVEL();
        Bkg = new E_BACKGROUND();
        var s = new MemoryStream(r.DecompressBlast((int)(r.BaseStream.Length - r.BaseStream.Position), header.Compressedsize));
        using var r2 = new BinaryReader(s);
        {
            // read
            var fsh = r2.ReadS<F_SCENE_HEADER>();
            if (fsh.Version != FTS_VERSION) throw new FormatException("BAD MAGIC");
            if (fsh.SizeX != Bkg.XSize) throw new FormatException("BAD HEADER");
            if (fsh.SizeZ != Bkg.ZSize) throw new FormatException("BAD HEADER");
            Level.PlayerPos = fsh.PlayerPos;
            Level.MscenePos = fsh.MscenePos;
            //Log.Info($"Header2: {r2.Tell():x}, 56");

            // textures
            var textures = Level.Textures = [.. r2.ReadSArray<F_TEXTURE_CONTAINER>(fsh.NumTextures).Cast<E_TEXTURE>()];
            //Log.Info($"Texture: {r2.Tell():x}");

            // backg
            var backg = Bkg.Backg;
            for (j = 0; j < fsh.SizeZ; j++)
                for (i = 0; i < fsh.SizeX; i++) {
                    ref E_BKG_INFO bi = ref backg[i + j * fsh.SizeX];
                    var fsi = r2.ReadS<F_SCENE_INFO>();
                    //if (fsi.NumPoly > 0) Log.Info($"F[{j},{i}]: {r2.Tell():x}, {fsi.NumPoly}, {fsi.NumIAnchors}");
                    bi.NumIAnchors = (short)fsi.NumIAnchors;
                    bi.NumPoly = (short)fsi.NumPoly;
                    bi.Polydata = fsi.NumPoly > 0 ? [.. r2.ReadSArray<F_POLY>(fsi.NumPoly).Select(z => z.To(textures, Bkg))] : null;
                    bi.Treat = 0;
                    bi.Nothing = fsi.NumPoly == 0;
                    bi.FrustrumMaxY = -99999999f;
                    bi.FrustrumMinY = 99999999f;
                    bi.IAnchors = fsi.NumIAnchors <= 0 ? null : r2.ReadPArray<int>("i", fsi.NumIAnchors);
                }
            //Log.Info($"Background: {r2.Tell():x}");

            // anchors
            Bkg.NumAnchors = fsh.NumAnchors;
            var anchors = Bkg.Anchors = fsh.NumAnchors > 0 ? new ANCHOR_DATA[fsh.NumAnchors] : null;
            for (i = 0; i < fsh.NumAnchors; i++) {
                ref ANCHOR_DATA a = ref anchors[i];
                var fad = r2.ReadS<F_ANCHOR_DATA>();
                a.Flags = fad.Flags;
                a.Pos = fad.Pos;
                a.NumLinked = fad.NumLinked;
                a.Height = fad.Height;
                a.Radius = fad.Radius;
                a.Linked = fad.NumLinked > 0 ? r2.ReadPArray<int>("i", fad.NumLinked) : null;
            }
            //Log.Info($"Anchors: {r2.Tell():x}");

            // rooms
            E_PORTAL_DATA portals = null;
            if (fsh.NumRooms > 0) {
                portals = Level.Portals = new E_PORTAL_DATA {
                    NumRooms = fsh.NumRooms,
                    Room = new E_ROOM_DATA[portals.NumRooms + 1],
                    NumTotal = fsh.NumPortals,
                    Portals = [.. r2.ReadSArray<E_SAVE_PORTALS>(fsh.NumPortals).Cast<E_PORTALS>()],
                };
                for (i = 0; i < portals.NumRooms + 1; i++) {
                    var x = r2.ReadS<E_SAVE_ROOM_DATA>();
                    portals.Room[i] = new E_ROOM_DATA {
                        NumPortals = x.NumPortals,
                        NumPolys = x.NumPolys,
                        Portals = x.NumPortals > 0 ? r2.ReadPArray<int>("i", x.NumPortals) : null,
                        EpData = x.NumPolys > 0 ? r2.ReadSArray<EP_DATA>(x.NumPolys) : null,
                    };
                }
            }
            //Log.Info($"Portals: {r2.Tell():x}");
            if (portals != null) {
                var numRoomDistance = Level.NumRoomDistance = portals.NumRooms + 1;
                Level.RoomDistance = new ROOM_DIST_DATA[numRoomDistance * numRoomDistance];
                for (var n = 0; n < numRoomDistance; n++)
                    for (var m = 0; m < numRoomDistance; m++)
                        SetRoomDistance(Level, m, n, r2.ReadS<ROOM_DIST_DATA>());
            }
            else { Level.NumRoomDistance = 0; Level.RoomDistance = null; }
            //Log.Info($"RoomDistance: {r2.Tell():x}");
        }
        ComputePolyIn();
        //PATHFINDER_Create();
        //PORTAL_Blend_Portals_And_Rooms();
        //ComputePortalVertexBuffer();
    }

    static void ComputePolyIn() {
    }

    public void WriteToStream(Stream stream) => this.Serialize(stream);
    public override string ToString() => this.Serialize();

    // IHaveMetaInfo
    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        => [
            new(null, new MetaContent { Type = "Text", Name = "Name", Value = this }),
            new("FTS", items: [
                new($"Level: {Level}"),
                new($"Bkg: {Bkg}"),
            ])
        ];
}

#endregion

#region Binary_Tea

public class Binary_Tea : IHaveMetaInfo {
    public static Task<object> Factory(BinaryReader r, FileSource f, Archive s) => Task.FromResult((object)new Binary_Tea(r));

    // https://github.com/OpenSourcedGames/Arx-Fatalis/blob/master/Sources/EERIE/EERIEAnim.cpp#L355
    public Binary_Tea(BinaryReader r) {
    }

    // IHaveMetaInfo
    List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        => [
            new("BinaryTEA", items: [
                new($"Type: Center"),
            ])
        ];
}

#endregion
