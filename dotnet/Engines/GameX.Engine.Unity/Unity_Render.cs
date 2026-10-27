#if ARKANE
using GameX.Arkane.Formats.Danae;
#endif
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
#if ARKANE
        UnityX.BuildersByType[typeof(Binary_Ftl)] = UnityFtlObjectBuilder.BuildObject;
        UnityX.BuildersByType[typeof(Binary_Fts)] = UnityFtsObjectBuilder.BuildObject;
#endif
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
    bool _opened;

    public void Awake() {
        var s = ShellState.FromArgs(Environment.GetCommandLineArgs());
        if (s != null) { FamilyId = s.FamilyId; ArcUri = s.ArcUri; Type = s.Type; Path = s.Path; }
        UnityEngine.Object.DontDestroyOnLoad(gameObject);
    }

    public void Start() {
        var scene = ShellState.SceneFor(Type);
        if (!SceneInBuild(scene) || UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == scene) { Open(); return; }
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
    }

    static bool SceneInBuild(string scene) {
        for (var i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings; i++) {
            var path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
            if (path.EndsWith("/" + scene + ".unity", StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode) {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        Open();
    }

    void Open() {
        if (_opened || string.IsNullOrEmpty(FamilyId) || string.IsNullOrEmpty(Path)) return;
        _opened = true;
        Family = FamilyManager.GetFamily(FamilyId);
        if (!string.IsNullOrEmpty(ArcUri)) Source = Family.GetArchive(new Uri(ArcUri));
        if (Source == null) return;
        var value = Source.GetAsset<object>(Path).Result;
        Renderer = UnityRenderer.CreateRenderer(this, EngineX.Gfx, Source, value, Type);
        Renderer?.Start();
    }

    public void OnDestroy() {
        Renderer?.Dispose();
        Source?.Dispose();
    }

    public void Update() => Renderer?.Update(0);
}
