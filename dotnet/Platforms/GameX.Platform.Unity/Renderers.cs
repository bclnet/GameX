using OpenStack;
using OpenStack.Gfx;
using OpenStack.Unity;
using OpenStack.Unity.Renderers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameX.Platforms.Unity;

public static class UnityRenderer
{
    public static Renderer CreateRenderer(object parent, UnityGfx3dModel gfx, object obj, string type)
        => type switch
        {
            "TestTri" => new UnityTestTriRenderer(gfx, obj),
            "Material" => new UnityMaterialRenderer(gfx, obj),
            "Particle" => new UnityParticleRenderer(gfx, obj),
            "Texture" or "VideoTexture" => new UnityTextureRenderer(gfx, obj),
            "Object" => new UnityObjectRenderer(gfx, obj),
            "Cell" => new UnityCellRenderer(gfx, obj),
            "Engine" => new UnityEngineRenderer(gfx, obj),
            _ => new UnityObjectRenderer(gfx, obj),
        };
}

public class UnityTestTriRenderer(UnityGfx3dModel gfx, object obj) : TestTriRenderer(gfx, obj) { }
public class UnityCellRenderer(UnityGfx3dModel gfx, object obj) : CellRenderer(gfx, obj) { }
public class UnityMaterialRenderer(UnityGfx3dModel gfx, object obj) : Renderer { }
public class UnityParticleRenderer(UnityGfx3dModel gfx, object obj) : Renderer { }
public class UnityEngineRenderer(UnityGfx3dModel gfx, object obj) : EngineRenderer(gfx, obj) { }
public class UnityObjectRenderer(UnityGfx3dModel gfx, object obj) : ObjectRenderer(gfx, obj) { }
public class UnityTextureRenderer(UnityGfx3dModel gfx, object obj) : TextureRenderer(gfx, obj) { }

public class ViewInfo : UnityEngine.MonoBehaviour
{
    static ViewInfo() => PlatformX.Activate(UnityPlatform.This);

    public enum Kind { Texture, TextureCursor, Object, Cell, Engine }

    [UnityEngine.Header("Pak Settings")]
    public string FamilyId = "Bethesda";
    public string PakUri = "game:/Morrowind.bsa#Morrowind";

    [UnityEngine.Header("View Params")]
    public Kind ViewKind = Kind.Texture;
    public string Param1 = "bookart/boethiah_256.dds";
    //public string Param1 = "meshes/x/ex_common_balcony_01.nif";

    protected Family Family;
    protected List<PakFile> PakFiles = [];
    protected UnityGfx3dModel Gfx;

    Renderer Renderer;

    public void Awake()
    {
        if (string.IsNullOrEmpty(FamilyId)) return;
        Family = FamilyManager.GetFamily(FamilyId);
        if (!string.IsNullOrEmpty(PakUri)) PakFiles.Add(Family.OpenPakFile(new Uri(PakUri)));
        var first = PakFiles.FirstOrDefault();
        Gfx = (UnityGfx3dModel)first?.Gfx[GFX.X3dModel];
        Renderer = UnityRenderer.CreateRenderer(this, Gfx, Param1, ViewKind.ToString());
        Renderer?.Start();
    }

    public void OnDestroy()
    {
        Renderer?.Dispose();
        foreach (var pakFile in PakFiles) pakFile.Dispose();
        PakFiles.Clear();
    }

    public void Start() => Renderer?.Start();
    public void Update() => Renderer?.Update(0);
}
