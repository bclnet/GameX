using OpenX.Client;
using OpenX.Gfx;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
#pragma warning disable CS0649, CS0169

[assembly: InternalsVisibleTo("OpenX.GfxTests")]

namespace OpenX;

#region Client

/// <summary>
/// DiligentClientHost
/// </summary>
public class DiligentClientHost : IClientHost {
    public void Dispose() => throw new NotImplementedException();
    public void Run() => throw new NotImplementedException();
}

#endregion

#region Engine

/// <summary>
/// DiligentGfxSprite3D
/// </summary>
public class DiligentGfxSprite3D : IOpenGfxSprite<object, object> {
    readonly SpriteManager<object> _spriteManager;
    public DiligentGfxSprite3D() { }

    public SpriteManager<object> SpriteManager => _spriteManager;
    public void Preload(ISource source, object path) => _spriteManager.Preload(source, path);
    public Task<(object spr, object tag)> Create(ISource source, object path, object parent = default) => _spriteManager.Create(source, path);
}

/// <summary>
/// DiligentGfxModel
/// </summary>
public class DiligentGfxModel : IOpenGfxModel<object, object, object, object> {
    readonly MaterialManager<object, object> _materialManager;
    readonly ObjectModelManager<object, object, object> _objectManager;
    readonly ShaderManager<object> _shaderManager;
    readonly TextureManager<object> _textureManager;
    public DiligentGfxModel() { }

    public MaterialManager<object, object> MaterialManager => _materialManager;
    public ObjectModelManager<object, object, object> ObjectManager => _objectManager;
    public ShaderManager<object> ShaderManager => _shaderManager;
    public TextureManager<object> TextureManager => _textureManager;
    public void Preload(ISource source, object path) => throw new NotImplementedException();
    public void PreloadTexture(ISource source, object path) => _textureManager.Preload(source, path);
    public Task<(object obj, object tag)> Create(ISource source, object path, bool isStatic, object parent = default) => throw new NotImplementedException();
    public Task<(object sha, object tag)> CreateShader(ISource source, object path, Dictionary<string, bool> args = null) => throw new NotImplementedException();
    public Task<(object tex, object tag)> CreateTexture(ISource source, object path, System.Range? level = null) => _textureManager.Create(source, path, level);
    public void Post(object src, Vector3 position, Vector3 eulerAngles, float? scale, object parent) => throw new NotImplementedException();
}

/// <summary>
/// DiligentSfx
/// </summary>
public class DiligentSfx : SystemSfx { }

/// <summary>
/// DiligentEngine
/// </summary>
public class DiligentEngine : Engine {
    public static readonly Engine This = new DiligentEngine();
    DiligentEngine() : base("DN", "Diligent") {
        Caps = EngineX.Caps.Drawing;
        GfxFactory = () => [null, null, new DiligentGfxSprite3D(), new DiligentGfxModel(), null, null];
        SfxFactory = () => [new DiligentSfx()];
    }
}

/// <summary>
/// DiligentShellEngine
/// </summary>
public class DiligentShellEngine : Engine {
    public static readonly Engine This = new DiligentShellEngine();
    DiligentShellEngine() : base("DN", "Diligent") { }
}

#endregion
