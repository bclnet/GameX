using System;

namespace OpenX.Gfx.Diligent;

#region TestTriRenderer

/// <summary>
/// TestTriRenderer
/// </summary>
public class TestTriRenderer : Renderer {
    readonly DiligentGfxModel GfxModel;

    public TestTriRenderer(IOpenGfx[] gfx, ISource source, object obj) {
        GfxModel = (DiligentGfxModel)gfx[GfX.XModel];
    }
}

#endregion

#region TextureRenderer

/// <summary>
/// TextureRenderer
/// </summary>
public class TextureRenderer : Renderer {
    readonly DiligentGfxModel GfxModel;
    readonly ISource Source;
    readonly object Obj;
    readonly Range Level;

    public TextureRenderer(IOpenGfx[] gfx, ISource source, object obj, Range level) =>
        (GfxModel, Source, Obj, Level) = ((DiligentGfxModel)gfx[GfX.XModel], source, obj, level);
}

#endregion
