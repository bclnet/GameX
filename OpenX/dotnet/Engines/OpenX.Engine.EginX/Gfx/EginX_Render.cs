namespace OpenX.Gfx.EginX;

#region TestTriRenderer

/// <summary>
/// TestTriRenderer
/// </summary>
public class TestTriRenderer : Renderer {
    readonly EginXGfxSprite2D GfxSprite;

    public TestTriRenderer(IOpenGfx[] gfx, ISource source, object obj) {
        GfxSprite = (EginXGfxSprite2D)gfx[GfX.XSprite2D];
    }
}

#endregion

#region SpriteRenderer

/// <summary>
/// SpriteRenderer
/// </summary>
public class SpriteRenderer : Renderer {
    readonly EginXGfxSprite2D GfxSprite;
    readonly object Obj;
    readonly object Sprite;

    public SpriteRenderer(IOpenGfx[] gfx, ISource source, object obj) {
        GfxSprite = (EginXGfxSprite2D)gfx[GfX.XSprite2D];
        Obj = obj;
        GfxSprite.SpriteManager.Delete(source, obj);
        (Sprite, _) = GfxSprite.SpriteManager.Create(source, obj).Result;
    }

    public override void Start() {
        Log.Info($"MakeSprite");
        Log.Info($"Done");
    }
}

#endregion

#region ObjectRenderer

/// <summary>
/// ObjectRenderer. Calls BuildersByType when the object's type is registered. No GPU upload.
/// </summary>
public class ObjectRenderer : Renderer {
    readonly object Obj;
    public object Holder { get; private set; }

    public ObjectRenderer(IOpenGfx[] gfx, ISource source, object obj) {
        Obj = obj;
    }

    public override void Start() {
        if (Obj == null || !EginX.BuildersByType.TryGetValue(Obj.GetType(), out var build)) return;
        Holder = build(Obj, true, null);
    }

    public override void Update(float deltaTime) { }
}

#endregion
