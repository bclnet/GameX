using OpenStack;
using OpenStack.Gfx;
using OpenStack.Gfx.Unity;
using System;
using System.Collections.Generic;
using static OpenStack.Gfx.GfX;

namespace GameX.Platforms.Unity;

public static class UnityRenderer
{
    public static Renderer CreateRenderer(object parent, IList<IOpenGfx> gfx, object obj, string type)
        => type switch
        {
            "TestTri" => new UnityTestTriRenderer(gfx[XModel] as UnityGfxModel, obj),
            "Material" => new UnityMaterialRenderer(gfx[XModel] as UnityGfxModel, obj),
            "Particle" => new UnityParticleRenderer(gfx[XModel] as UnityGfxModel, obj),
            "Texture" or "VideoTexture" => new UnityTextureRenderer(gfx[XModel] as UnityGfxModel, obj),
            "Object" => new UnityObjectRenderer(gfx[XModel] as UnityGfxModel, obj),
            "Cell" => new UnityCellRenderer(gfx[XModel] as UnityGfxModel, obj),
            "Engine" => new UnityEngineRenderer(gfx[XModel] as UnityGfxModel, obj),
            _ => new UnityObjectRenderer(gfx[XModel] as UnityGfxModel, obj),
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

    [UnityEngine.Header("View")]
    public string FamilyId = "Bethesda";
    public string PakUri = "game:/Morrowind.bsa#Morrowind";
    public string Type = "Texture";
    public string Path = "bookart/boethiah_256.dds";
    //public string Path = "meshes/x/ex_common_balcony_01.nif";

    protected Family Family;
    protected PakFile Source;
    Renderer Renderer;

    public void Awake()
    {
        if (string.IsNullOrEmpty(FamilyId)) return;
        Family = FamilyManager.GetFamily(FamilyId);
        if (!string.IsNullOrEmpty(PakUri)) Source = Family.OpenPakFile(new Uri(PakUri));
        Renderer = UnityRenderer.CreateRenderer(this, Source?.Gfx, Path, Type);
    }

    public void OnDestroy()
    {
        Renderer?.Dispose();
        Source?.Dispose();
    }

    public void Start() => Renderer?.Start();
    public void Update() => Renderer?.Update(0);
}
