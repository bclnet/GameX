using OpenX.Client;
using OpenX.Gfx;
using OpenX.Gfx.Raylib;
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
/// RaylibClientHost
/// </summary>
public class RaylibClientHost : IClientHost {
    public void Dispose() => throw new NotImplementedException();
    public void Run() => throw new NotImplementedException();
}

#endregion

#region Engine

/// <summary>
/// RaylibGfxSprite3D
/// </summary>
public class RaylibGfxSprite3D : IOpenGfxSprite<object, object> {
    readonly SpriteManager<object> _spriteManager;
    public RaylibGfxSprite3D() { }

    public SpriteManager<object> SpriteManager => _spriteManager;
    public void Preload(ISource source, object path) => _spriteManager.Preload(source, path);
    public Task<(object spr, object tag)> Create(ISource source, object path, object parent = default) => _spriteManager.Create(source, path);
}

/// <summary>
/// RaylibGfxModel
/// </summary>
public class RaylibGfxModel : IOpenGfxModel<object, object, object, object> {
    readonly MaterialManager<object, object> _materialManager;
    readonly ObjectModelManager<object, object, object> _objectManager;
    readonly ShaderManager<object> _shaderManager;
    readonly TextureManager<object> _textureManager;
    public RaylibGfxModel() { }

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
/// RaylibSfx
/// </summary>
public class RaylibSfx : SystemSfx { }

/// <summary>
/// RaylibEngine
/// </summary>
public class RaylibEngine : Engine {
    public static readonly Engine This = new RaylibEngine();
    RaylibEngine() : base("RL", "Raylib") {
        Caps = EngineX.Caps.Drawing;
        GfxFactory = () => [null, null, new RaylibGfxSprite3D(), new RaylibGfxModel(), null, null];
        SfxFactory = () => [new RaylibSfx()];
    }

    /// <summary>
    /// Calls IsWindowReady. Does not open a window. Returns the binding version string.
    /// </summary>
    public static string Version() => RaylibX.Version();
}

/// <summary>
/// RaylibShellEngine
/// </summary>
public class RaylibShellEngine : Engine {
    public static readonly Engine This = new RaylibShellEngine();
    RaylibShellEngine() : base("RL", "Raylib") { }
}

#endregion
