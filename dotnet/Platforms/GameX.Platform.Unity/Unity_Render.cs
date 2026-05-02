using OpenStack;
using OpenStack.Gfx;
using OpenStack.Gfx.Unity;
using System;
#pragma warning disable CS9113

namespace GameX.Platforms.Unity;

public static class UnityRenderer {
    public static Renderer CreateRenderer(object parent, IOpenGfx[] gfx, object obj, string type)
        => type switch {
            "TestTri" => new TestTriRenderer(gfx, obj),
            "Material" => new MaterialRenderer(gfx, obj),
            "Particle" => new ParticleRenderer(gfx, obj),
            "Texture" or "VideoTexture" => new TextureRenderer(gfx, obj),
            "Object" => new ObjectRenderer(gfx, obj),
            "Engine" => new EngineRenderer(gfx, obj),
            _ => new ObjectRenderer(gfx, obj),
        };
}

public class MaterialRenderer(IOpenGfx[] gfx, object obj) : Renderer { }
public class ParticleRenderer(IOpenGfx[] gfx, object obj) : Renderer { }

public class ViewInfo : UnityEngine.MonoBehaviour {
    static ViewInfo() => PlatformX.Activate(UnityPlatform.This);

    [UnityEngine.Header("View")]
    public string FamilyId = "Bethesda";
    public string ArcUri = "game:/#Morrowind";
    public string Type = "Engine";
    public string Path = "Morrowind.start";
    //public string Path = "Morrowind.bsa:bookart/boethiah_256.dds";
    //public string Path = "Morrowind.bsa:meshes/x/ex_common_balcony_01.nif";

    protected Family Family;
    protected Archive Source;
    Renderer Renderer;

    public void Awake() {
        // parse args
        var args = Environment.GetCommandLineArgs();
        ShellState s;
        if (args.Length > 1 && (s = ShellState.Parse(args[1])) != null) { FamilyId = s.FamilyId; ArcUri = s.ArcUri; Type = s.Type; Path = s.Path; }
        //Log.Info($"FamilyId: '{FamilyId}'");
        //Log.Info($"ArcUri: '{ArcUri}'");
        //Log.Info($"Type: '{Type}'");
        //Log.Info($"Path: '{Path}'");

        // awake
        if (string.IsNullOrEmpty(FamilyId)) return;
        Family = FamilyManager.GetFamily(FamilyId);
        if (!string.IsNullOrEmpty(ArcUri)) Source = Family.GetArchive(new Uri(ArcUri));
        var value = Source.GetAsset<object>(Path).Result;
        Renderer = UnityRenderer.CreateRenderer(this, Source?.Gfx, value, Type);
    }

    public void OnDestroy() {
        Renderer?.Dispose();
        Source?.Dispose();
    }

    public void Start() => Renderer?.Start();
    public void Update() => Renderer?.Update(0);
}
