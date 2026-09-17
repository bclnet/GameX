using GameX.Gamebryo.Formats;
using OpenX;
using OpenX.Gfx;
using OpenX.Gfx.Unity;
using System;
#pragma warning disable CS9113

namespace GameX.Engines.Unity;

public static class UnityRenderer {
    static UnityRenderer() {
        UnityX.BuildersByType[typeof(Binary_Nif)] = UnityNifObjectBuilder.BuildObject;
    }
    public static Renderer CreateRenderer(object parent, IOpenGfx[] gfx, ISource source, object obj, string type) {
        if (gfx == null) return null;
        if (obj is IHaveSource z) source = z.Source;
        return type switch {
            "TestTri" => new TestTriRenderer(gfx, source, obj),
            //"Material" => new MaterialRenderer(gfx, source, obj),
            //"Particle" => new ParticleRenderer(gfx, source, obj),
            "Texture" or "VideoTexture" => new TextureRenderer(gfx, source, obj),
            "Object" => new ObjectRenderer(gfx, source, obj),
            "Engine" => new EngineRenderer(gfx, source, obj),
            _ => new ObjectRenderer(gfx, source, obj),
        };
    }
}

public class ViewInfo : UnityEngine.MonoBehaviour {
    static ViewInfo() => EngineX.Activate(UnityXEngine.This);

    [UnityEngine.Header("View")]
    public string FamilyId = "Bethesda";
    public string ArcUri = "game:/#Morrowind";
    public string Type = "Engine";
    public string Path = "Morrowind.start";
    //public string Type = "Texture"; public string Path = "Morrowind.bsa:bookart/boethiah_256.dds";
    //public string Type = "Object"; public string Path = "Morrowind.bsa:meshes/x/ex_common_balcony_01.nif";
    //public string Type = "Engine"; public string Path = "Morrowind.start";

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
        Renderer = UnityRenderer.CreateRenderer(this, EngineX.Gfx, Source, value, Type);
    }

    public void OnDestroy() {
        Renderer?.Dispose();
        Source?.Dispose();
    }

    public void Start() => Renderer?.Start();
    public void Update() => Renderer?.Update(0);
}
