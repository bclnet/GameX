using System;

namespace OpenX.Gfx.Raylib;

#region TestTriRenderer

/// <summary>
/// TestTriRenderer
/// </summary>
public class TestTriRenderer : Renderer {
    readonly RaylibGfxModel GfxModel;

    public TestTriRenderer(IOpenGfx[] gfx, ISource source, object obj) {
        GfxModel = (RaylibGfxModel)gfx[GfX.XModel];
    }
}

#endregion

#region TextureRenderer

/// <summary>
/// TextureRenderer
/// </summary>
public class TextureRenderer : Renderer {
    readonly RaylibGfxModel GfxModel;
    readonly ISource Source;
    readonly object Obj;
    readonly Range Level;

    public TextureRenderer(IOpenGfx[] gfx, ISource source, object obj, Range level) =>
        (GfxModel, Source, Obj, Level) = ((RaylibGfxModel)gfx[GfX.XModel], source, obj, level);
}

#endregion
