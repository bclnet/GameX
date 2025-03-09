using GameX;
using GameX.Platforms;
using OpenStack.Gfx;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SimpleEngine = System.Object;

namespace Views;

#region ViewBase 

public abstract class ViewBase(IUnityGfx gfx, object obj) : IDisposable
{
    protected readonly IUnityGfx Gfx = gfx;
    protected readonly object Obj = obj;
    
    public virtual void Dispose() { }
    public abstract void Start();
    public virtual void Update() { }

    public static ViewBase Create(object parent, IUnityGfx gfx, object obj)
    {
        //ViewKind switch
        //{
        //    Kind.Texture => new ViewTexture(this),
        //    Kind.Object => new ViewObject(this),
        //    Kind.Cell => new ViewCell(this),
        //    Kind.Engine => new ViewEngine(this),
        //    _ => new ViewObject(this),
        //};
        return default;
    }
}

#endregion

#region ViewCell

//static Estate Estate = EstateManager.GetEstate("Tes");
//static TesUnityPakFile PakFile = new TesUnityPakFile(Estate.OpenPakFile(new Uri("game:/Morrowind.bsa#Morrowind")));
////static TesUnityPakFile PakFile = new TesUnityPakFile(Estate.OpenPakFile(new Uri("game:/Bloodmoon.bsa#Morrowind")));
////static TesUnityPakFile PakFile = new TesUnityPakFile(Estate.OpenPakFile(new Uri("game:/Tribunal.bsa#Morrowind")));
////static TesUnityPakFile PakFile = new TesUnityPakFile(Estate.OpenPakFile(new Uri("game:/Oblivion.bsa#Oblivion")));
////static TesUnityPakFile PakFile = new TesUnityPakFile(Estate.OpenPakFile(new Uri("game:/Skyrim.esm#SkyrimVR")));
////static TesUnityPakFile PakFile = new TesUnityPakFile(Estate.OpenPakFile(new Uri("game:/Fallout4.esm#Fallout4")));
////static TesUnityPakFile PakFile = new TesUnityPakFile(Estate.OpenPakFile(new Uri("Fallout4.esm#Fallout4VR")));

public class ViewCell(IUnityGfx gfx, object obj) : ViewBase(gfx, obj)
{
    public override void Start()
    {
        //TestLoadCell(new Vector3(((-2 << 5) + 1) * ConvertUtils.ExteriorCellSideLengthInMeters, 0, ((-1 << 5) + 1) * ConvertUtils.ExteriorCellSideLengthInMeters));
        //TestLoadCell(new Vector3((-1 << 3) * ConvertUtils.ExteriorCellSideLengthInMeters, 0, (-1 << 3) * ConvertUtils.ExteriorCellSideLengthInMeters));
        //TestLoadCell(new Vector3(0 * ConvertUtils.ExteriorCellSideLengthInMeters, 0, 0 * ConvertUtils.ExteriorCellSideLengthInMeters));
        //TestLoadCell(new Vector3((1 << 3) * ConvertUtils.ExteriorCellSideLengthInMeters, 0, (1 << 3) * ConvertUtils.ExteriorCellSideLengthInMeters));
        //TestLoadCell(new Vector3((1 << 5) * ConvertUtils.ExteriorCellSideLengthInMeters, 0, (1 << 5) * ConvertUtils.ExteriorCellSideLengthInMeters));
        //TestAllCells();
    }

    //public static Int3 GetCellId(Vector3 point, int world) => new Int3(Mathf.FloorToInt(point.x / ConvertUtils.ExteriorCellSideLengthInMeters), Mathf.FloorToInt(point.z / ConvertUtils.ExteriorCellSideLengthInMeters), world);

    //static void TestLoadCell(Vector3 position)
    //{
    //    var cellId = GetCellId(position, 60);
    //    var cell = DatFile.FindCellRecord(cellId);
    //    var land = ((TesDataPack)DatFile).FindLANDRecord(cellId);
    //    Log($"LAND #{land?.Id}");
    //}

    //static void TestAllCells()
    //{
    //    var cells = ((TesDataPack)DatFile).GroupByLabel["CELL"].Records;
    //    Log($"CELLS: {cells.Count}");
    //    foreach (var record in cells.Cast<CELLRecord>())
    //        Log(record.EDID.Value);
    //}
}

#endregion

#region ViewEngine

public class ViewEngine(IUnityGfx gfx, object obj) : ViewBase(gfx, obj)
{
    SimpleEngine Engine;
    GameObject PlayerPrefab = GameObject.Find("Player00");

    public override void Dispose()
    {
        base.Dispose();
        //Engine?.Dispose();
    }

    public override void Start()
    {
        //var assetUri = new Uri("http://192.168.1.3/ASSETS/Morrowind/Morrowind.bsa#Morrowind");
        //var dataUri = new Uri("http://192.168.1.3/ASSETS/Morrowind/Morrowind.esm#Morrowind");

        //var assetUri = new Uri("game:/Morrowind.bsa#Morrowind");
        //var dataUri = new Uri("game:/Morrowind.esm#Morrowind");

        ////var assetUri = new Uri("game:/Oblivion*#Oblivion");
        ////var dataUri = new Uri("game:/Oblivion.esm#Oblivion");

        //Engine = new SimpleEngine(TesEstateHandler.Handler, assetUri, dataUri);

        //// engine
        //Engine.SpawnPlayer(PlayerPrefab, new Vector3(-137.94f, 2.30f, -1037.6f)); // new Int3(-2, -9)

        // engine - oblivion
        //Engine.SpawnPlayer(PlayerPrefab, new Int3(0, 0, 60), new Vector3(0, 0, 0));
    }

    //public override void Update() => Engine?.Update();
}

#endregion

#region ViewObject

public class ViewObject(IUnityGfx gfx, object obj) : ViewBase(gfx, obj)
{
    public override void Start()
    {
        //if (!string.IsNullOrEmpty(View.Param1)) MakeObject(View.Param1);
    }

    void MakeObject(object path) => Gfx.ObjectManager.CreateObject(path);
}

#endregion

#region ViewTexture

// game:/Morrowind.bsa#Morrowind
// http://192.168.1.3/ASSETS/Morrowind/Morrowind.bsa#Morrowind
// game:/Skyrim*#SkyrimVR
// game:/Fallout4*#Fallout4VR

public class ViewTexture(IUnityGfx gfx, object obj) : ViewBase(gfx, obj)
{
    class FixedMaterialInfo : IFixedMaterial
    {
        public string Name { get; set; }
        public string ShaderName { get; set; }
        public IDictionary<string, bool> GetShaderArgs() => null;
        public IDictionary<string, object> Data { get; set; }
        public string MainFilePath { get; set; }
        public string DarkFilePath { get; set; }
        public string DetailFilePath { get; set; }
        public string GlossFilePath { get; set; }
        public string GlowFilePath { get; set; }
        public string BumpFilePath { get; set; }
        public bool AlphaBlended { get; set; }
        public int SrcBlendMode { get; set; }
        public int DstBlendMode { get; set; }
        public bool AlphaTest { get; set; }
        public float AlphaCutoff { get; set; }
        public bool ZWrite { get; set; }
    }

    public override void Start()
    {
        //if (!string.IsNullOrEmpty(View.Param1)) MakeTexture(View.Param1);
        //if (!string.IsNullOrEmpty(View.Param2)) MakeCursor(View.Param2);
    }

    GameObject MakeTexture(string path)
    {
        var obj = GameObject.CreatePrimitive(PrimitiveType.Plane);
        obj.transform.rotation = Quaternion.Euler(-90f, 180f, -180f);
        var meshRenderer = obj.GetComponent<MeshRenderer>();
        (meshRenderer.material, _) = Gfx.MaterialManager.CreateMaterial(new FixedMaterialInfo { MainFilePath = path });
        return obj;
    }

    void MakeCursor(string path) => Cursor.SetCursor(Gfx.TextureManager.CreateTexture(path).tex, Vector2.zero, CursorMode.Auto);
}

#endregion

#region ViewInfo

public class ViewInfo : UnityEngine.MonoBehaviour
{
    public enum Kind
    {
        Texture,
        TextureCursor,
        Object,
        Cell,
        Engine,
    }

    ViewBase View;

    [Header("Pak Settings")]
    public string FamilyId = "Tes";
    public string PakUri = "game:/Morrowind.bsa#Morrowind";

    [Header("View Params")]
    public Kind ViewKind = Kind.Texture;
    public string Param1 = "bookart/boethiah_256.dds";
    //public string Param1 = "meshes/x/ex_common_balcony_01.nif";

    protected Family Family;
    protected List<PakFile> PakFiles = [];
    protected IUnityGfx Gfx;

    public void Awake()
    {
        if (string.IsNullOrEmpty(FamilyId)) return;
        Family = FamilyManager.GetFamily(FamilyId);
        if (!string.IsNullOrEmpty(PakUri)) PakFiles.Add(Family.OpenPakFile(new Uri(PakUri)));
        var first = PakFiles.FirstOrDefault();
        Gfx = (IUnityGfx)first?.Gfx;
        View = ViewBase.Create(this, Gfx, (ViewKind, Param1));
    }

    public void OnDestroy()
    {
        View?.Dispose();
        foreach (var pakFile in PakFiles) pakFile.Dispose();
        PakFiles.Clear();
    }
    public void Start() => View?.Start();
    public void Update() => View?.Update();
}

#endregion