using System;

namespace OpenX.Gfx.Vk;

#region TestTriRenderer

/// <summary>
/// TestTriRenderer
/// </summary>
public class TestTriRenderer : Renderer {
    readonly VkGfxModel GfxModel;

    /// <summary>
    /// TestTriRenderer
    /// </summary>
    public TestTriRenderer(IOpenGfx[] gfx, ISource source, object obj) {
        GfxModel = (VkGfxModel)gfx[GfX.XModel];
    }
}

#endregion

#region TextureRenderer

/// <summary>
/// TextureRenderer
/// </summary>
public class TextureRenderer : Renderer {
    readonly VkGfxModel GfxModel;
    readonly ISource Source;
    readonly object Obj;
    readonly Range Level;
    readonly object Texture;
    float FrameDelay;

    /// <summary>
    /// TextureRenderer
    /// </summary>
    public TextureRenderer(IOpenGfx[] gfx, ISource source, object obj, Range level) {
        GfxModel = (VkGfxModel)gfx[GfX.XModel];
        Source = source;
        Obj = obj;
        Level = level;
        GfxModel.TextureManager.Delete(source, obj);
        (Texture, _) = GfxModel.TextureManager.Create(source, obj, level).Result;
    }

    /// <summary>
    /// Start
    /// </summary>
    public override void Start() {
        Log.Info($"MakeTexture");
        Log.Info($"Done");
    }

    /// <summary>
    /// Update
    /// </summary>
    public override void Update(float deltaTime) {
        if (Obj is not ITextureFrames obj || GfxModel == null || !obj.HasFrames) return;
        FrameDelay += deltaTime;
        if (FrameDelay <= obj.Fps || !obj.NextFrame()) return;
        FrameDelay = 0; // reset delay between frames
        GfxModel.TextureManager.Reload(Source, obj, Level);
    }
}

#endregion
