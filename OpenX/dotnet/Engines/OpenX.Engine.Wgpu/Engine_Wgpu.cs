using OpenX.Client;
using OpenX.Gfx;
using OpenX.Gfx.Wgpu;
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
/// WgpuClientHost
/// </summary>
public class WgpuClientHost : IClientHost {
    public void Dispose() => throw new NotImplementedException();
    public void Run() => throw new NotImplementedException();
}

#endregion

#region Engine

/// <summary>
/// WgpuGfxSprite3D
/// </summary>
public class WgpuGfxSprite3D : IOpenGfxSprite<object, object> {
    readonly SpriteManager<object> _spriteManager;
    public WgpuGfxSprite3D() { }

    public SpriteManager<object> SpriteManager => _spriteManager;
    public void Preload(ISource source, object path) => _spriteManager.Preload(source, path);
    public Task<(object spr, object tag)> Create(ISource source, object path, object parent = default) => _spriteManager.Create(source, path);
}

/// <summary>
/// WgpuGfxModel
/// </summary>
public class WgpuGfxModel : IOpenGfxModel<object, object, object, object> {
    readonly MaterialManager<object, object> _materialManager;
    readonly ObjectModelManager<object, object, object> _objectManager;
    readonly ShaderManager<object> _shaderManager;
    readonly TextureManager<object> _textureManager;
    public WgpuGfxModel() { }

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
/// WgpuSfx
/// </summary>
public class WgpuSfx : SystemSfx { }

/// <summary>
/// WgpuEngine
/// </summary>
public class WgpuEngine : Engine {
    public static readonly Engine This = new WgpuEngine();
    WgpuEngine() : base("WG", "Wgpu") {
        Caps = EngineX.Caps.Drawing;
        GfxFactory = () => [null, null, new WgpuGfxSprite3D(), new WgpuGfxModel(), null, null];
        SfxFactory = () => [new WgpuSfx()];
    }

    /// <summary>
    /// Creates a wgpu instance and requests an adapter. No window and no surface.
    /// </summary>
    public static WgpuInstance CreateInstance() => WgpuInstance.CreateInstance();
}

/// <summary>
/// WgpuShellEngine
/// </summary>
public class WgpuShellEngine : Engine {
    public static readonly Engine This = new WgpuShellEngine();
    WgpuShellEngine() : base("WG", "Wgpu") { }
}

#endregion
