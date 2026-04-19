using OpenStack;
using OpenStack.Gfx;
using OpenStack.Gfx.Stride;
using System;
using System.Collections.Generic;
using static OpenStack.Gfx.GfX;
#pragma warning disable CS9113

namespace GameX.Platforms.Stride;

public static class StrideRenderer {
    public static Renderer CreateRenderer(object parent, IList<IOpenGfx> gfx, object obj, string type)
        => type switch {
            "TestTri" => new TestTriRenderer(gfx[XModel] as StrideGfxModel, obj),
            //"Material" => new MaterialRenderer(gfx[XModel] as StrideGfxModel, obj),
            //"Particle" => new ParticleRenderer(gfx[XModel] as StrideGfxModel, obj),
            "Texture" or "VideoTexture" => new TextureRenderer(gfx[XModel] as StrideGfxModel, obj, 0..),
            //"Object" => new ObjectRenderer(gfx[XModel] as StrideGfxModel, obj),
            //"Cell" => new CellRenderer(gfx[XModel] as StrideGfxModel, obj),
            "World" => throw new NotImplementedException(),
            //"Engine" => new EngineRenderer(gfx[XModel] as StrideGfxModel, obj),
            _ => default,
        };
}

//public class StrideTestTriRenderer(StrideGfxModel gfx, object obj) : TestTriRenderer(gfx, obj) { }
//public class StrideCellRenderer(StrideGfxModel gfx, object obj) : Renderer { }
//public class StrideMaterialRenderer(StrideGfxModel gfx, object obj) : Renderer { }
//public class StrideParticleRenderer(StrideGfxModel gfx, object obj) : Renderer { }
//public class StrideEngineRenderer(StrideGfxModel gfx, object obj) : Renderer { }
//public class StrideObjectRenderer(StrideGfxModel gfx, object obj) : Renderer { }
//public class StrideTextureRenderer(StrideGfxModel gfx, object obj) : TextureRenderer(gfx, obj, Level) {
//    static Range Level = 0..;
//}
