using System;

namespace OpenX.Gfx.Stride;

#region TestTriRenderer

/// <summary>
/// TestTriRenderer
/// </summary>
public class TestTriRenderer : Renderer {
    readonly StrideGfxModel GfxModel;

    public TestTriRenderer(IOpenGfx[] gfx, ISource source, object obj) {
        GfxModel = (StrideGfxModel)gfx[GfX.XModel];
    }
}

#endregion

#region TextureRenderer

/// <summary>
/// TextureRenderer
/// </summary>
public class TextureRenderer : Renderer {
    readonly StrideGfxModel GfxModel;
    readonly ISource Source;
    readonly object Obj;
    readonly Range Level;
    readonly object Texture;
    float FrameDelay;

    public TextureRenderer(IOpenGfx[] gfx, ISource source, object obj, Range level) {
        GfxModel = (StrideGfxModel)gfx[GfX.XModel];
        Source = source;
        Obj = obj;
        Level = level;
        GfxModel.TextureManager.Delete(source, obj);
        Texture = GfxModel.TextureManager.Create(source, obj, level).Result.tex;
    }

    public override void Start() {
        Log.Info($"MakeTexture");
        Log.Info($"Done");
    }

    public override void Update(float deltaTime) {
        if (Obj is not ITextureFrames obj || GfxModel == null || !obj.HasFrames) return;
        FrameDelay += deltaTime;
        if (FrameDelay <= obj.Fps || !obj.NextFrame()) return;
        FrameDelay = 0; // reset delay between frames
        GfxModel.TextureManager.Reload(Source, obj, Level);
    }
}

#endregion

//if (!string.IsNullOrEmpty(View.Param1)) MakeTexture(View.Param1);
//Entity MakeTexture(string path)
//{
//    //var obj = GeometricPrimitive.Plane.New(Game.GraphicsDevice).ToMeshDraw();
//    var obj = new Entity("Name", rotation: Quaternion.CreateFromYawPitchRoll(-90f, 180f, -180f))
//    {
//        new ModelComponent(new PlaneProceduralModel().Generate(Game.Services))
//    };
//    //var obj = Content.CreatePrimitive(PrimitiveType.Plane);
//    //obj.transform.rotation = ;
//    //var meshRenderer = obj.GetComponent<MeshRenderer>();
//    //(meshRenderer.material, _) = Gfx.MaterialManager.CreateMaterial(new FixedMaterialInfo { MainFilePath = path });
//    return obj;
//}

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
        if (Obj == null || !StrideX.BuildersByType.TryGetValue(Obj.GetType(), out var build)) return;
        Holder = build(Obj, true, null);
    }

    public override void Update(float deltaTime) { }
}

#endregion
