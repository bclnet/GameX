using OpenStack.Gfx;
using OpenStack.Gfx.Textures;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameX.Platforms;

// SdlExtensions
public static class SdlExtensions
{
}

// SdlObjectBuilder
// MISSING


// SdlTextureBuilder
public class SdlTextureBuilder : TextureBuilderBase<object>
{
    public override object DefaultTexture => throw new NotImplementedException();
    public override object CreateNormalMap(object texture, float strength) => throw new NotImplementedException();
    public override object CreateSolidTexture(int width, int height, float[] rgba) => throw new NotImplementedException();
    public override object CreateTexture(object reuse, ITexture source, Range? level = null) => throw new NotImplementedException();
    public override void DeleteTexture(object texture) => throw new NotImplementedException();
}

// ISdlGfx
public interface ISdlGfx : IOpenGfxAny<object, object, object, object> { }

// SdlGfx
public class SdlGfx(PakFile source) : ISdlGfx
{
    readonly PakFile _source = source;
    readonly TextureManager<object> _textureManager = new(source, new SdlTextureBuilder());

    public PakFile Source => _source;
    public ITextureManager<object> TextureManager => _textureManager;
    public IMaterialManager<object, object> MaterialManager => default;
    public IObjectManager<object, object, object> ObjectManager => default;
    public IShaderManager<object> ShaderManager => default;
    public object CreateTexture(object path, Range? level = null) => _textureManager.CreateTexture(path, level).tex;
    public void PreloadTexture(object path) => _textureManager.PreloadTexture(path);
    public object CreateObject(object path) => default;
    public void PreloadObject(object path) { }
    public object CreateShader(object path, IDictionary<string, bool> args = null) => default;

    public Task<T> LoadFileObject<T>(object path) => _source.LoadFileObject<T>(path);
}

// SdlSfx
public class SdlSfx(PakFile source) : SystemSfx(source)
{
}

// SdlPlatform
public class SdlPlatform : Platform
{
    public static readonly Platform This = new SdlPlatform();
    SdlPlatform() : base("SD", "SDL 3")
    {
        GfxFactory = source => new SdlGfx(source);
        SfxFactory = source => new SdlSfx(source);
    }
}
