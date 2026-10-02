using Mirage;
using OpenX.Sfx;
using System;
using System.Threading.Tasks;

namespace OpenX.Gfx.Mirage;

/// <summary>
/// MirageRenderer - base for renderers that build into MirageEngine.Tree. Each renderer owns one node under the root, named for its kind.
/// </summary>
public abstract class MirageRenderer(string name) : Renderer {
    public Node Node { get; private set; }
    protected SceneTree Tree => MirageEngine.Tree;

    public override void Start() {
        Node = Tree.Root.Find(name) ?? Tree.Root.AddChild(name);
    }

    public override void Dispose() => Node?.Destroy();
}

#region TestTriRenderer

/// <summary>
/// TestTriRenderer - a single triangle, the "hello world" every engine has.
/// </summary>
public class TestTriRenderer(IOpenGfx[] gfx, ISource source, object obj) : MirageRenderer("TestTri") {
    readonly MirageGfxModel GfxModel = (MirageGfxModel)gfx[GfX.XModel];

    public override void Start() {
        base.Start();
        var mesh = Tree.Resources.TryGet<Mesh>("Triangle", out var m) ? m : Tree.Resources.Add(Mesh.Triangle());
        var (shader, _) = GfxModel.ShaderManager.Create(source, "testtri").Result;
        var material = Tree.Resources.Get<Material>("testtri"); material.Shader = shader;
        Node.AddComponent(new MeshRenderer { Mesh = mesh, Material = material });
    }
}

#endregion

#region TextureRenderer

/// <summary>
/// TextureRenderer - uploads a texture through the TextureManager and shows it on a quad. Animated textures advance a frame per update.
/// </summary>
public class TextureRenderer : MirageRenderer {
    readonly MirageGfxModel GfxModel;
    readonly ISource Source;
    readonly object Obj;
    readonly Range Level;
    public readonly Texture Tex;
    public readonly Shader Shader;
    public bool Background;
    public int Frames { get; private set; }

    public TextureRenderer(IOpenGfx[] gfx, ISource source, object obj, Range level, bool background = false) : base("Texture") {
        GfxModel = (MirageGfxModel)gfx[GfX.XModel];
        Source = source;
        Obj = obj;
        Level = level;
        GfxModel.TextureManager.Delete(source, obj);
        (Tex, _) = GfxModel.TextureManager.Create(source, obj, level).Result;
        (Shader, _) = GfxModel.ShaderManager.Create(source, "plane").Result;
        Background = background;
    }

    public override void Start() {
        base.Start();
        var mesh = Tree.Resources.TryGet<Mesh>("Quad", out var m) ? m : Tree.Resources.Add(Mesh.Quad());
        var material = Tree.Resources.Get<Material>($"plane:{Tex.Name}");
        material.Shader = Shader;
        material.Textures["Main"] = Tex;
        if (Background) material.Props["background"] = true;
        Node.AddComponent(new MeshRenderer { Mesh = mesh, Material = material });
    }

    public override void Update(float deltaTime) {
        if (Obj is not ITextureFrames f || !f.HasFrames) return;
        if (!f.NextFrame()) return;
        Frames++;
        GfxModel.TextureManager.Reload(Source, Obj, Level);
        Tree.Log($"texture frame {Frames}");
    }
}

#endregion

#region ObjectRenderer

/// <summary>
/// ObjectRenderer - builds a model into the scene through the ObjectManager and the family's registered builder.
/// </summary>
public class ObjectRenderer(IOpenGfx[] gfx, ISource source, object obj) : MirageRenderer("Object") {
    readonly MirageGfxModel GfxModel = (MirageGfxModel)gfx[GfX.XModel];
    public Node Obj { get; private set; }

    public override void Start() {
        base.Start();
        (Obj, _) = GfxModel.ObjectManager.Create(source, obj, false, Node).Result;
    }
}

#endregion

#region MaterialRenderer

/// <summary>
/// MaterialRenderer - builds a material and shows it on a quad.
/// </summary>
public class MaterialRenderer(IOpenGfx[] gfx, ISource source, object obj) : MirageRenderer("Material") {
    readonly MirageGfxModel GfxModel = (MirageGfxModel)gfx[GfX.XModel];
    public Material Material { get; private set; }

    public override void Start() {
        base.Start();
        (Material, _) = GfxModel.MaterialManager.Create(source, obj).Result;
        var mesh = Tree.Resources.TryGet<Mesh>("Quad", out var m) ? m : Tree.Resources.Add(Mesh.Quad());
        Node.AddComponent(new MeshRenderer { Mesh = mesh, Material = Material });
    }
}

#endregion

#region SpriteRenderer

/// <summary>
/// SpriteRenderer
/// </summary>
public class SpriteRenderer(IOpenGfx[] gfx, ISource source, object obj) : MirageRenderer("Sprite") {
    readonly MirageGfxSprite2D GfxSprite = (MirageGfxSprite2D)gfx[GfX.XSprite2D];
    public Sprite Sprite { get; private set; }

    public override void Start() {
        base.Start();
        GfxSprite.SpriteManager.Delete(source, obj);
        (Sprite, _) = GfxSprite.SpriteManager.Create(source, obj).Result;
        Node.AddComponent(new Sprite2D { Sprite = Sprite });
    }
}

#endregion

#region AudioRenderer

/// <summary>
/// AudioRenderer - loads audio through the Sfx and parks it on an AudioSource.
/// </summary>
public class AudioRenderer(IOpenSfx[] sfx, ISource source, object obj) : MirageRenderer("Audio") {
    readonly MirageSfx Sfx = (MirageSfx)sfx[0];
    public Audio Audio { get; private set; }

    public override void Start() {
        base.Start();
        (Audio, _) = Sfx.CreateAudio(source, obj).Result;
        Node.AddComponent(new AudioSource { Audio = Audio, Playing = true });
    }
}

#endregion
