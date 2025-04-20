using OpenStack;
using OpenStack.Gfx;
using OpenStack.Gfx.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using static OpenStack.Gfx.GfxX;

namespace GameX.Platforms.Unity;

public static class UnityRenderer
{
    public static Renderer CreateRenderer(object parent, UnityGfxModel gfx, object obj, string type)
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

public class UnityTestTriRenderer(UnityGfxModel gfx, object obj) : TestTriRenderer(gfx, obj) { }
public class UnityCellRenderer(UnityGfxModel gfx, object obj) : CellRenderer(gfx, obj) { }
public class UnityMaterialRenderer(UnityGfxModel gfx, object obj) : Renderer { }
public class UnityParticleRenderer(UnityGfxModel gfx, object obj) : Renderer { }
public class UnityEngineRenderer(UnityGfxModel gfx, object obj) : EngineRenderer(gfx, obj) { }
public class UnityObjectRenderer(UnityGfxModel gfx, object obj) : ObjectRenderer(gfx, obj) { }
public class UnityTextureRenderer(UnityGfxModel gfx, object obj) : TextureRenderer(gfx, obj) { }

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
    protected UnityGfxModel Gfx;

    Renderer Renderer;

    public void Awake()
    {
        if (string.IsNullOrEmpty(FamilyId)) return;
        Family = FamilyManager.GetFamily(FamilyId);
        if (!string.IsNullOrEmpty(PakUri)) PakFiles.Add(Family.OpenPakFile(new Uri(PakUri)));
        var first = PakFiles.FirstOrDefault();
        Gfx = (UnityGfxModel)first?.Gfx[XModel];
        Renderer = UnityRenderer.CreateRenderer(this, Gfx, Param1, ViewKind.ToString());
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
