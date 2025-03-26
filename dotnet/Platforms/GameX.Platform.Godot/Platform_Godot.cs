using Godot;
using OpenStack.Gfx;
using OpenStack.Gfx.Textures;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GShader = Godot.Shader;

namespace GameX.Platforms;

// GodotExtensions
public static class GodotExtensions { }

// GodotObjectBuilder : MISSING

// GodotShaderBuilder : MISSING

// GodotTextureBuilder
public class GodotTextureBuilder : TextureBuilderBase<Texture>
{
    Texture _defaultTexture;
    public override Texture DefaultTexture => _defaultTexture ??= CreateDefaultTexture();

    public void Release()
    {
        if (_defaultTexture != null) { /*DeleteTexture(_defaultTexture);*/ _defaultTexture = null; }
    }

    Texture CreateDefaultTexture() => CreateSolidTexture(4, 4, [
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
    ]);

    public override Texture CreateTexture(Texture reuse, ITexture source, System.Range? level = null)
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

// GodotMaterialBuilder : MISSING

// IGodotfx
public interface IGodotGfx : IOpenGfxAny<Node, Material, Texture, GShader> { }

// GodotGfx
public class GodotGfx : IGodotGfx
{
    readonly PakFile _source;
    readonly ITextureManager<Texture> _textureManager;
    readonly MaterialManager<Material, Texture> _materialManager;
    readonly ObjectManager<Node, Material, Texture> _objectManager;
    readonly ShaderManager<GShader> _shaderManager;

    public GodotGfx(PakFile source)
    {
        _source = source;
        _textureManager = new TextureManager<Texture>(source, new GodotTextureBuilder());
        //_materialManager = new MaterialManager<Material, int>(source, _textureManager, new GodotMaterialBuilder(_textureManager));
        //_objectManager = new ObjectManager<Model, Material, int>(source, _materialManager, new GodotObjectBuilder());
        //_shaderManager = new ShaderManager<int>(source, new GodotShaderBuilder());
    }

    public PakFile Source => _source;
    public ITextureManager<Texture> TextureManager => _textureManager;
    public IMaterialManager<Material, Texture> MaterialManager => _materialManager;
    public IObjectManager<Node, Material, Texture> ObjectManager => _objectManager;
    public IShaderManager<GShader> ShaderManager => _shaderManager;
    public Texture CreateTexture(object path, System.Range? level = null) => _textureManager.CreateTexture(path, level).tex;
    public void PreloadTexture(object path) => throw new NotImplementedException();
    public Node CreateObject(object path) => throw new NotImplementedException();
    public void PreloadObject(object path) => throw new NotImplementedException();
    public GShader CreateShader(object path, IDictionary<string, bool> args = null) => throw new NotImplementedException();

    public Task<T> LoadFileObject<T>(object path) => _source.LoadFileObject<T>(path);
}

// GodotSfx
public class GodotSfx(PakFile source) : SystemSfx(source)
{
}

// GodotPlatform
public class GodotPlatform : Platform
{
    public static readonly Platform This = new GodotPlatform();
    GodotPlatform() : base("GD", "Godot")
    {
        GfxFactory = source => new GodotGfx(source);
        SfxFactory = source => new GodotSfx(source);
        LogFunc = a => GD.Print(a?.Replace("\r", ""));
        LogFormatFunc = (a, b) => GD.Print(string.Format(a, b)?.Replace("\r", ""));
    }
}

// GodotShellPlatform
public class GodotShellPlatform : Platform
{
    public static readonly Platform This = new GodotShellPlatform();
    GodotShellPlatform() : base("GD", "Godot") { }
}