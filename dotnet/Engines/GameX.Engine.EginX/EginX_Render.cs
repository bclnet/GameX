using OpenX;
using OpenX.Gfx;
using OpenX.Gfx.EginX;
using GameX.Arkane.Formats.Danae;
using GameX.Gamebryo.Formats;
#pragma warning disable CS9113

namespace GameX.Engines.EginX;

public static class EginXRenderer {
    static EginXRenderer() {
        OpenX.Gfx.EginX.EginX.BuildersByType[typeof(Binary_Nif)] = EginXNifObjectBuilder.BuildObject;
        OpenX.Gfx.EginX.EginX.BuildersByType[typeof(Binary_Ftl)] = EginXFtlObjectBuilder.BuildObject;
        OpenX.Gfx.EginX.EginX.BuildersByType[typeof(Binary_Fts)] = EginXFtsObjectBuilder.BuildObject;
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
