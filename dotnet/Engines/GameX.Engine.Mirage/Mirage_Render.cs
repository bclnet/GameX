using GameX.Gamebryo.Formats;
using OpenX;
using OpenX.Gfx;
using OpenX.Gfx.Mirage;

namespace GameX.Engines.Mirage;

/// <summary>
/// MirageRenderer - the GameX entry point for the Mirage engine (code "MR"). Same shape as OpenGLRenderer, but the result is a scene tree you can read as text.
/// </summary>
public static class MirageRenderer {
    static MirageRenderer() {
        MirageX.BuildersByType[typeof(Binary_Nif)] = MirageNifObjectBuilder.BuildObject;
    }

    /// <summary>
    /// Forces the static constructor, so the builders are registered before anything asks for them.
    /// </summary>
    public static void Register() { }

    public static Renderer CreateRenderer(object parent, IOpenGfx[] gfx, ISource source, object obj, string type) {
        if (obj is IHaveSource z) source = z.Source;
        return type switch {
            "TestTri" => new TestTriRenderer(gfx, source, obj),
            "Texture" or "VideoTexture" => new TextureRenderer(gfx, source, obj, 0.., false),
            "Sprite" => new SpriteRenderer(gfx, source, obj),
            "Object" => new ObjectRenderer(gfx, source, obj),
            "Material" => new MaterialRenderer(gfx, source, obj),
            "Audio" => new AudioRenderer(EngineX.Sfx, source, obj),
            _ => default,
        };
    }
}
