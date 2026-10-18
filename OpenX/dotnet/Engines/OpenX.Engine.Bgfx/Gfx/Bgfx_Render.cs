using System;

namespace OpenX.Gfx.Bgfx;

#region TestTriRenderer

/// <summary>
/// TestTriRenderer
/// </summary>
public class TestTriRenderer : Renderer {
    readonly BgfxGfxModel GfxModel;

    public TestTriRenderer(IOpenGfx[] gfx, ISource source, object obj) {
        GfxModel = (BgfxGfxModel)gfx[GfX.XModel];
    }
}

#endregion

#region TextureRenderer

/// <summary>
/// TextureRenderer
/// </summary>
public class TextureRenderer : Renderer {
    readonly BgfxGfxModel GfxModel;
    readonly ISource Source;
    readonly object Obj;
    readonly Range Level;

    public TextureRenderer(IOpenGfx[] gfx, ISource source, object obj, Range level) =>
        (GfxModel, Source, Obj, Level) = ((BgfxGfxModel)gfx[GfX.XModel], source, obj, level);
}

#endregion
