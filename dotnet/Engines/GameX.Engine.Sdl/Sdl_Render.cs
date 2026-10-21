using OpenX;
using OpenX.Gfx;
using OpenX.Gfx.Sdl;
using GameX.Arkane.Formats.Danae;
using GameX.Gamebryo.Formats;
#pragma warning disable CS9113

namespace GameX.Engines.Sdl;

public static class SdlRenderer {
    static SdlRenderer() {
        SdlX.BuildersByType[typeof(Binary_Nif)] = SdlNifObjectBuilder.BuildObject;
        SdlX.BuildersByType[typeof(Binary_Ftl)] = SdlFtlObjectBuilder.BuildObject;
        SdlX.BuildersByType[typeof(Binary_Fts)] = SdlFtsObjectBuilder.BuildObject;
    }
    public static Renderer CreateRenderer(object parent, IOpenGfx[] gfx, ISource source, object obj, string type) {
        if (obj is IHaveSource z) source = z.Source;
        return type switch {
            "TestTri" => new TestTriRenderer(gfx, source, obj),
            "Texture" => new SpriteRenderer(gfx, source, obj),
            "Object" => new ObjectRenderer(gfx, source, obj),
            _ => default,
        };
    }
}
