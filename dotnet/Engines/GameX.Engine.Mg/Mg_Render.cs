using OpenX;
using OpenX.Gfx;
using OpenX.Gfx.Mg;
using GameX.Arkane.Formats.Danae;
using GameX.Gamebryo.Formats;
#pragma warning disable CS9113

namespace GameX.Engines.Mg;

public static class MgRenderer {
    static MgRenderer() {
        MgX.BuildersByType[typeof(Binary_Nif)] = MgNifObjectBuilder.BuildObject;
        MgX.BuildersByType[typeof(Binary_Ftl)] = MgFtlObjectBuilder.BuildObject;
        MgX.BuildersByType[typeof(Binary_Fts)] = MgFtsObjectBuilder.BuildObject;
    }
    public static Renderer CreateRenderer(object parent, IOpenGfx[] gfx, ISource source, object obj, string type) {
        if (obj is IHaveSource z) source = z.Source;
        return type switch {
            "TestTri" => new TestTriRenderer(gfx, source, obj),
            "Texture" => new SpriteRenderer(gfx, source, obj),
            "Object" => new ObjectRenderer(gfx, source, obj),
            _ => default
        };
    }
}
