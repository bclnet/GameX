using OpenStack.Gfx;
using OpenStack.Gfx.Textures;
using Stride.Core.Diagnostics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Rendering;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Debug = OpenStack.Debug;

namespace GameX.Platforms;

// StrideExtensions
public static class StrideExtensions { }

// StrideObjectBuilder : MISSING

// StrideShaderBuilder : MISSING

// StrideTextureBuilder
public class StrideTextureBuilder : TextureBuilderBase<Texture>
{
    Texture _defaultTexture;
    public override Texture DefaultTexture => _defaultTexture ??= CreateDefaultTexture();

    public void Release()
    {
        if (_defaultTexture != null) { /*DeleteTexture(_defaultTexture);*/ _defaultTexture = null; }
    }

    Texture CreateDefaultTexture() => CreateSolidTexture(4, 4, new[]
    {
        0.9f, 0.2f, 0.8f, 1f,
        0f, 0.9f, 0f, 1f,
        0.9f, 0.2f, 0.8f, 1f,
        0f, 0.9f, 0f, 1f,

        0f, 0.9f, 0f, 1f,
        0.9f, 0.2f, 0.8f, 1f,
        0f, 0.9f, 0f, 1f,
        0.9f, 0.2f, 0.8f, 1f,

        0.9f, 0.2f, 0.8f, 1f,
        0f, 0.9f, 0f, 1f,
        0.9f, 0.2f, 0.8f, 1f,
        0f, 0.9f, 0f, 1f,

        0f, 0.9f, 0f, 1f,
        0.9f, 0.2f, 0.8f, 1f,
        0f, 0.9f, 0f, 1f,
        0.9f, 0.2f, 0.8f, 1f,
    });

    public override Texture CreateTexture(Texture reuse, ITexture source, Range? level = null)
    {
        var (bytes, format, _) = source.Begin("ST");
        try
        {
            return null;
        }
        finally { source.End(); }
    }

    public override Texture CreateSolidTexture(int width, int height, float[] pixels)
    {
        return null;
    }

    public override Texture CreateNormalMap(Texture texture, float strength)
    {
        throw new NotImplementedException();
    }

    public override void DeleteTexture(Texture texture) { }
}

// StrideMaterialBuilder : MISSING

// IStridefx
public interface IStrideGfx : IOpenGfxAny<Entity, Material, Texture, int> { }

// StrideGfx
public class StrideGfx : IStrideGfx
{
    readonly PakFile _source;
    readonly ITextureManager<Texture> _textureManager;
    readonly MaterialManager<Material, Texture> _materialManager;
    readonly ObjectManager<Entity, Material, Texture> _objectManager;
    readonly ShaderManager<int> _shaderManager;

    public StrideGfx(PakFile source)
    {
        _source = source;
        _textureManager = new TextureManager<Texture>(source, new StrideTextureBuilder());
        //_materialManager = new MaterialManager<Material, int>(source, _textureManager, new StrideMaterialBuilder(_textureManager));
        //_objectManager = new ObjectManager<Model, Material, int>(source, _materialManager, new StrideObjectBuilder());
        //_shaderManager = new ShaderManager<int>(source, new StrideShaderBuilder());
    }

    public PakFile Source => _source;
    public ITextureManager<Texture> TextureManager => _textureManager;
    public IMaterialManager<Material, Texture> MaterialManager => _materialManager;
    public IObjectManager<Entity, Material, Texture> ObjectManager => _objectManager;
    public IShaderManager<int> ShaderManager => _shaderManager;
    public Texture CreateTexture(object path, Range? level = null) => _textureManager.CreateTexture(path, level).tex;
    public void PreloadTexture(object path) => throw new NotImplementedException();
    public Entity CreateObject(object path) => throw new NotImplementedException();
    public void PreloadObject(object path) => throw new NotImplementedException();
    public int CreateShader(object path, IDictionary<string, bool> args = null) => throw new NotImplementedException();

    public Task<T> LoadFileObject<T>(object path) => _source.LoadFileObject<T>(path);
}

// StrideSfx
public class StrideSfx(PakFile source) : SystemSfx(source)
{
}

// StridePlatform
public static class StridePlatform
{
    static Logger Log;

    public static unsafe bool Startup()
    {
        Log = GlobalLogger.GetLogger(typeof(StridePlatform).FullName);
        Log.Debug("Start loading MyTexture");
        try
        {
            Platform.PlatformType = "ST";
            Platform.GfxFactory = source => new StrideGfx(source);
            Platform.SfxFactory = source => new StrideSfx(source);
            Debug.AssertFunc = x => System.Diagnostics.Debug.Assert(x);
            Debug.LogFunc = a => Log.Info(a);
            Debug.LogFormatFunc = (a, b) => Log.Info(string.Format(a, b));
            return true;
        }
        catch { return false; }
    }
}