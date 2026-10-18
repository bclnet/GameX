using System;

namespace OpenX.Gfx.Wgpu;

#region TestTriRenderer

/// <summary>
/// TestTriRenderer
/// </summary>
public class TestTriRenderer : Renderer {
    readonly WgpuGfxModel GfxModel;

    public TestTriRenderer(IOpenGfx[] gfx, ISource source, object obj) {
        GfxModel = (WgpuGfxModel)gfx[GfX.XModel];
    }
}

#endregion

#region TextureRenderer

/// <summary>
/// TextureRenderer
/// </summary>
public class TextureRenderer : Renderer {
    readonly WgpuGfxModel GfxModel;
    readonly ISource Source;
    readonly object Obj;
    readonly Range Level;

    public TextureRenderer(IOpenGfx[] gfx, ISource source, object obj, Range level) =>
        (GfxModel, Source, Obj, Level) = ((WgpuGfxModel)gfx[GfX.XModel], source, obj, level);
}

#endregion
