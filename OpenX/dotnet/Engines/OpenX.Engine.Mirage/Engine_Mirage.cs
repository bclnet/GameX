using Mirage;
using OpenX.Client;
using OpenX.Gfx;
using OpenX.Gfx.Mirage;
using OpenX.Sfx;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
#pragma warning disable CS1998

[assembly: InternalsVisibleTo("OpenX.EngineTests")]

namespace OpenX;

#region Client

/// <summary>
/// MirageClientHost - runs the scene tree for a fixed number of frames.
/// </summary>
public class MirageClientHost(int frames = 1, float delta = SceneTree.DefaultDelta) : IClientHost {
    public void Run() => MirageEngine.Tree.Run(frames, delta);
    public void Dispose() { }
}

#endregion

#region Builders

/// <summary>
/// MirageObjectModelBuilder - asks the registered family builder for a Node tree and parks it under _Prefabs; Instance() clones it into the scene.
/// </summary>
class MirageObjectModelBuilder : ObjectModelBuilder<Node, Material, Texture> {
    Node _prefabObj;

    public override Node Instance(Node src, Node parent) {
        var s = src.Clone();
        (parent ?? MirageEngine.Tree.Root).AddChild(s);
        return s;
    }

    public override async Task<Node> Create(ISource source, object path, bool isStatic, MaterialManager<Material, Texture> materialManager) {
        if (!MirageX.BuildersByType.TryGetValue(path.GetType(), out var builder)) throw new ArgumentOutOfRangeException(nameof(path), $"No Mirage builder registered for {path.GetType().Name}");
        var s = await builder(source, path, isStatic, materialManager);
        _prefabObj.AddChild(s);
        return s;
    }

    public override void Ensure() {
        if (_prefabObj != null && _prefabObj.Tree == MirageEngine.Tree) return;
        _prefabObj = MirageEngine.Tree.Root.Find("_Prefabs") ?? MirageEngine.Tree.Root.AddChild(new Node("_Prefabs") { Visible = false });
    }
}

/// <summary>
/// MirageShaderBuilder
/// </summary>
class MirageShaderBuilder : ShaderBuilder<Shader> {
    public override Shader Create(object path, Dictionary<string, bool> args = null) {
        var s = MirageEngine.Tree.Resources.Get<Shader>(path?.ToString() ?? "default");
        s.Defines = [.. (args ?? []).Where(x => x.Value).Select(x => x.Key).OrderBy(x => x, StringComparer.Ordinal)];
        return s;
    }
}

/// <summary>
/// MirageSpriteBuilder
/// </summary>
class MirageSpriteBuilder : SpriteBuilder<Sprite> {
    public override Sprite Default => MirageEngine.Tree.Resources.Get<Sprite>("default");
    public override Sprite Create(ISprite spr) => spr.Create("MR", x => {
        var s = MirageEngine.Tree.Resources.Add(new Sprite { Name = MirageEngine.NameOf(spr, "sprite"), Width = spr.Width, Height = spr.Height });
        if (x is ITexture tex) s.Texture = MirageEngine.Tree.Resources.Add(MirageTextureBuilder.FromTexture(tex, s.Name));
        return s;
    });
    public override void Delete(Sprite spr) => spr.Deleted = true;
}

/// <summary>
/// MirageTextureBuilder - records the shape of every texture the engine is asked to upload.
/// </summary>
class MirageTextureBuilder : TextureBuilder<Texture> {
    Texture _defaultTexture;
    public override Texture Default => _defaultTexture ??= CreateSolid(4, 4, [0.9f, 0.2f, 0.8f, 1f]);

    public override Texture CreateNormalMap(Texture src, float strength) {
        var t = MirageEngine.Tree.Resources.Add(new Texture { Name = $"{src.Name}_normal", Width = src.Width, Height = src.Height, Format = "NormalMap" });
        t.Bytes = src.Bytes;
        return t;
    }

    public override Texture CreateSolid(int width, int height, float[] rgbas) {
        var name = $"solid_{width}x{height}_{string.Join("_", rgbas.Take(4).Select(x => SceneText.Format(x)))}";
        return MirageEngine.Tree.Resources.Add(new Texture { Name = name, Width = width, Height = height, Format = "RGBA32", Bytes = width * height * 4 });
    }

    public override Texture Create(Texture reuse, ITexture src, Range? level = null) => src.Create("MR", x => {
        var t = reuse ?? MirageEngine.Tree.Resources.Add(new Texture { Name = MirageEngine.NameOf(src, "texture") });
        Fill(t, src);
        switch (x) {
            case TextureAsDds z:
                t.Format = "DDS";
                t.Bytes = z.Bytes?.Length ?? 0;
                if (MirageEngine.KeepPixels) t.Pixels = z.Bytes;
                break;
            case TextureAsBytes z:
                t.Format = MirageX.FormatName(z.Format);
                t.Bytes = z.Bytes?.Length ?? 0;
                t.Spans = z.Spans?.Length ?? 0;
                if (MirageEngine.KeepPixels) t.Pixels = z.Bytes;
                break;
            case null: t.Format = "None"; break;
            default: throw new ArgumentOutOfRangeException(nameof(x), $"{x}");
        }
        if (level != null) t.MipMaps = Math.Max(1, Math.Min(src.MipMaps, level.Value.End.GetOffset(src.MipMaps)) - level.Value.Start.GetOffset(src.MipMaps));
        return t;
    });

    public override void Delete(Texture src) => src.Deleted = true;

    internal static Texture FromTexture(ITexture src, string name) { var t = new Texture { Name = name }; Fill(t, src); return t; }
    static void Fill(Texture t, ITexture src) {
        t.Width = src.Width; t.Height = src.Height; t.Depth = Math.Max(1, src.Depth); t.MipMaps = Math.Max(1, src.MipMaps); t.Flags = (int)src.TexFlags;
        t.Deleted = false;
    }
}

/// <summary>
/// MirageMaterialBuilder - a Material records the props it was built from and the textures it pulled through the TextureManager.
/// </summary>
class MirageMaterialBuilder(TextureManager<Texture> textureManager) : MaterialBuilder<Material, Texture>(textureManager) {
    Material _defaultMaterial;
    public override Material Default => _defaultMaterial ??= MirageEngine.Tree.Resources.Get<Material>("default");

    Material _terrainMaterial;
    public override Material Terrain => _terrainMaterial ??= MirageEngine.Tree.Resources.Get<Material>("terrain");

    public override async Task<Material> Create(ISource source, object path) {
        switch (path) {
            case MaterialStdProp p: {
                    var m = MirageEngine.Tree.Resources.Add(new Material { Name = MirageEngine.NameOf(p, p is MaterialStd2Prop ? "std2" : "std") });
                    foreach (var kv in p.Textures.OrderBy(x => x.Key, StringComparer.Ordinal)) {
                        if (kv.Value == null) continue;
                        try { m.Textures[kv.Key] = (await TextureManager.Create(source, kv.Value)).tex; }
                        catch (Exception e) { m.Props[$"tex:{kv.Key}:error"] = e.Message; Log.Warn($"{kv.Key}: {kv.Value}: {e.Message}"); }
                    }
                    if (p.AlphaBlended) { m.Props["alphaBlended"] = true; m.Props["srcBlend"] = p.SrcBlendMode; m.Props["dstBlend"] = p.DstBlendMode; }
                    if (p.AlphaTest) { m.Props["alphaTest"] = true; m.Props["alphaCutoff"] = p.AlphaCutoff; }
                    if (p is MaterialStd2Prop p2) {
                        m.Props["zWrite"] = p2.ZWrite;
                        m.Props["diffuse"] = p2.DiffuseColor; m.Props["specular"] = p2.SpecularColor; m.Props["emissive"] = p2.EmissiveColor;
                        m.Props["glossiness"] = p2.Glossiness; m.Props["alpha"] = p2.Alpha;
                    }
                    return m;
                }
            case MaterialShaderProp p: {
                    var m = MirageEngine.Tree.Resources.Add(new Material { Name = MirageEngine.NameOf(p, p.ShaderName ?? "shader") });
                    m.Shader = new MirageShaderBuilder().Create(p.ShaderName, p.ShaderArgs);
                    if (p is MaterialShaderVProp v) {
                        foreach (var kv in v.IntParams ?? []) m.Props[$"int:{kv.Key}"] = kv.Value;
                        foreach (var kv in v.FloatParams ?? []) m.Props[$"float:{kv.Key}"] = kv.Value;
                        foreach (var kv in v.VectorParams ?? []) m.Props[$"vec:{kv.Key}"] = kv.Value;
                        foreach (var kv in v.TextureParams ?? []) m.Props[$"tex:{kv.Key}"] = kv.Value;
                        foreach (var kv in v.IntAttributes ?? []) m.Props[$"attr:{kv.Key}"] = kv.Value;
                    }
                    return m;
                }
            default: throw new ArgumentOutOfRangeException(nameof(path), $"{path}");
        }
    }
}

/// <summary>
/// MirageAudioBuilder
/// </summary>
class MirageAudioBuilder : AudioBuilder<Audio> {
    public override Audio Create(ISource source, object path) => MirageEngine.Tree.Resources.Add(new Audio { Name = MirageEngine.NameOf(path, "audio") });
    public override void Delete(ISource source, Audio audio) => audio.Deleted = true;
}

#endregion

#region Gfx

/// <summary>
/// MirageGfxApi - the scene-graph half of IOpenGfx, on Mirage nodes.
/// </summary>
public class MirageGfxApi : IOpenGfxApi<Node, Material> {
    static Node RootOr(Node parent) => parent ?? MirageEngine.Tree.Root;

    public Node CreateObject(string name, string tag = null, Node parent = null) => RootOr(parent).AddChild(new Node(name) { Tag = tag });
    public object CreateMesh(object mesh) => mesh as Mesh ?? MirageEngine.Tree.Resources.Get<Mesh>(MirageEngine.NameOf(mesh, "mesh"));
    public void AddMeshRenderer(Node source, object mesh, Material material, bool enabled, bool isStatic) {
        source.AddComponent(new MeshRenderer { Mesh = (Mesh)CreateMesh(mesh), Material = material, Enabled = enabled });
        source.IsStatic = isStatic;
    }
    public void AddSkinnedMeshRenderer(Node source, object mesh, Material material, bool enabled, bool isStatic) {
        source.AddComponent(new SkinnedMeshRenderer { Mesh = (Mesh)CreateMesh(mesh), Material = material, Enabled = enabled });
        source.IsStatic = isStatic;
    }
    public void AddMeshCollider(Node source, object mesh, bool isKinematic, bool isStatic) {
        if (!isStatic) {
            source.AddComponent<BoxCollider>();
            source.AddComponent(new Rigidbody { IsKinematic = isKinematic });
        }
        else source.AddComponent(new MeshCollider { Mesh = (Mesh)CreateMesh(mesh) });
    }
    public void Attach(GfxAttach method, Node src, params object[] args) => MirageEngine.Tree.Log($"attach {method} {src.Path} {string.Join(" ", args ?? [])}");
    public void Parent(Node source, Node parent) => parent.AddChild(source);
    public void Transform(Node source, Vector3 position, Quaternion rotation, Vector3 localScale) {
        source.Transform.Position = position;
        source.Transform.Rotation = rotation;
        source.Transform.Scale = localScale;
    }
    public void Transform(Node source, Vector3 position, Matrix4x4 rotation, Vector3 localScale) => Transform(source, position, rotation.ToMirageRotation(), localScale);
    public void AddMissingMeshCollidersRecursively(Node source, bool isStatic) { if (isStatic && source.GetComponentInChildren<Collider>() == null) source.AddMissingMeshCollidersRecursively(); }
    public void SetLayerRecursively(Node source, int layer) => source.SetLayerRecursively(layer);
    public void SetVisible(Node src, bool visible) => src.Visible = visible;
    public void Destroy(Node src) => src.Destroy();
}

/// <summary>
/// MirageGfxSprite2D
/// </summary>
public class MirageGfxSprite2D : IOpenGfxSprite<Node, Sprite> {
    readonly SpriteManager<Sprite> _spriteManager = new(new MirageSpriteBuilder());
    public SpriteManager<Sprite> SpriteManager => _spriteManager;
    public void Preload(ISource source, object path) => _spriteManager.Preload(source, path);
    public Task<(Sprite spr, object tag)> Create(ISource source, object path, Node parent = default) => _spriteManager.Create(source, path);
}

/// <summary>
/// MirageGfxSprite3D
/// </summary>
public class MirageGfxSprite3D : IOpenGfxSprite<Node, Sprite> {
    readonly SpriteManager<Sprite> _spriteManager = new(new MirageSpriteBuilder());
    public SpriteManager<Sprite> SpriteManager => _spriteManager;
    public void Preload(ISource source, object path) => _spriteManager.Preload(source, path);
    public Task<(Sprite spr, object tag)> Create(ISource source, object path, Node parent = default) => _spriteManager.Create(source, path);
}

/// <summary>
/// MirageGfxModel
/// </summary>
public class MirageGfxModel : IOpenGfxModel<Node, Material, Texture, Shader> {
    readonly MaterialManager<Material, Texture> _materialManager;
    readonly ObjectModelManager<Node, Material, Texture> _objectManager;
    readonly ShaderManager<Shader> _shaderManager;
    readonly TextureManager<Texture> _textureManager;
    public MirageGfxModel() {
        _textureManager = new TextureManager<Texture>(new MirageTextureBuilder());
        _materialManager = new MaterialManager<Material, Texture>(_textureManager, new MirageMaterialBuilder(_textureManager));
        _objectManager = new ObjectModelManager<Node, Material, Texture>(_materialManager, new MirageObjectModelBuilder());
        _shaderManager = new ShaderManager<Shader>(new MirageShaderBuilder());
    }

    public MaterialManager<Material, Texture> MaterialManager => _materialManager;
    public ObjectModelManager<Node, Material, Texture> ObjectManager => _objectManager;
    public ShaderManager<Shader> ShaderManager => _shaderManager;
    public TextureManager<Texture> TextureManager => _textureManager;
    public void Preload(ISource source, object path) => _objectManager.Preload(source, path);
    public void PreloadTexture(ISource source, object path) => _textureManager.Preload(source, path);
    public Task<(Node obj, object tag)> Create(ISource source, object path, bool isStatic, Node parent = default) => _objectManager.Create(source, path, isStatic, parent);
    public Task<(Shader sha, object tag)> CreateShader(ISource source, object path, Dictionary<string, bool> args = null) => _shaderManager.Create(source, path, args);
    public Task<(Texture tex, object tag)> CreateTexture(ISource source, object path, Range? level = null) => _textureManager.Create(source, path, level);
    public void Post(Node src, Vector3 position, Vector3 eulerAngles, float? scale, Node parent = default) {
        if (parent != null) parent.AddChild(src);
        src.Transform.Position = position;
        src.Transform.EulerAngles = eulerAngles;
        if (scale != null) src.Transform.Scale = new Vector3(scale.Value);
    }
}

/// <summary>
/// MirageGfxLight
/// </summary>
public class MirageGfxLight : IOpenGfxLight<Node> {
    public Node Create(string name, Vector3? position, float radius, Color color, bool indoors, Node parent = default) {
        var n = (parent ?? MirageEngine.Tree.Root).AddChild(new Node(name));
        if (position != null) n.Transform.Position = position.Value;
        n.AddComponent(new Light { Radius = radius, Color = color, Indoors = indoors });
        return n;
    }
    public Node CreateProbe(string name, Vector3? position, Node parent = null) {
        var n = (parent ?? MirageEngine.Tree.Root).AddChild(new Node(name));
        if (position != null) n.Transform.Position = position.Value;
        n.AddComponent<LightProbe>();
        return n;
    }
}

/// <summary>
/// MirageGfxTerrain
/// </summary>
public class MirageGfxTerrain : IOpenGfxTerrain<Node, Material, Texture> {
    public object CreateData(int offset, float[,] heights, float heightRange, float sampleDistance, GfxTerrainLayer<Texture>[] layers, float[,,] alphaMap)
        => MirageEngine.Tree.Resources.Add(new TerrainData {
            Name = $"terrain_{offset}",
            Offset = offset,
            Rows = heights?.GetLength(0) ?? 0, Cols = heights?.GetLength(1) ?? 0,
            HeightRange = heightRange, SampleDistance = sampleDistance,
            Layers = layers?.Length ?? 0,
            AlphaLayers = alphaMap?.GetLength(2) ?? 0,
        });
    public Node Create(string name, Vector3? position, object data, Node parent = null) {
        var n = (parent ?? MirageEngine.Tree.Root).AddChild(new Node(name));
        if (position != null) n.Transform.Position = position.Value;
        n.AddComponent(new Terrain { Data = data as TerrainData });
        return n;
    }
}

/// <summary>
/// MirageSfx
/// </summary>
public class MirageSfx : IOpenSfx<Audio> {
    readonly AudioManager<Audio> _audioManager = new(new MirageAudioBuilder());
    public AudioManager<Audio> AudioManager => _audioManager;
    public async Task<(Audio aud, object tag)> CreateAudio(ISource source, object path) => _audioManager.Create(source, path);
}

#endregion

#region Engine

/// <summary>
/// MirageEngine - engine code "MR". Everything it is asked to draw lands in Tree, a headless scene graph with a text form.
/// </summary>
public class MirageEngine : Engine {
    public static readonly Engine This = new MirageEngine();

    /// <summary>
    /// The scene everything renders into. Reset() replaces it.
    /// </summary>
    public static SceneTree Tree { get; private set; } = new();

    /// <summary>
    /// Keep decoded pixel data on Texture resources (off by default - tests usually only need the shape).
    /// </summary>
    public static bool KeepPixels;

    MirageEngine() : base("MR", "Mirage") {
        Caps = EngineX.Caps.None_;
        GfxFactory = () => [new MirageGfxApi(), new MirageGfxSprite2D(), new MirageGfxSprite3D(), new MirageGfxModel(), new MirageGfxLight(), new MirageGfxTerrain()];
        SfxFactory = () => [new MirageSfx()];
        LogFunc = a => Tree.Log(a);
    }

    /// <summary>
    /// A fresh tree, and fresh Gfx/Sfx (so no manager cache survives). Activates the engine if it is not current.
    /// </summary>
    public static SceneTree Reset() {
        Tree = new SceneTree();
        if (EngineX.Current == This) EngineX.Activate(UnknownEngine.This);
        EngineX.Activate(This);
        return Tree;
    }

    /// <summary>
    /// A resource name for an asset: its Name property if it has one, else its ToString, else a fallback.
    /// </summary>
    public static string NameOf(object obj, string fallback) {
        if (obj == null) return fallback;
        if (obj is string s) return s;
        var prop = obj.GetType().GetProperty("Name") ?? (System.Reflection.MemberInfo)obj.GetType().GetField("Name");
        var name = prop switch {
            System.Reflection.PropertyInfo p when p.PropertyType == typeof(string) => p.GetValue(obj) as string,
            System.Reflection.FieldInfo f when f.FieldType == typeof(string) => f.GetValue(obj) as string,
            _ => null,
        };
        if (!string.IsNullOrEmpty(name)) return name;
        var str = obj.ToString();
        return string.IsNullOrEmpty(str) || str == obj.GetType().FullName ? $"{fallback}#{obj.GetHashCode():x}" : str;
    }
}

#endregion
