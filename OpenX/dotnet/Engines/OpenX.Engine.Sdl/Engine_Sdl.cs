using OpenX.Client;
using OpenX.Gfx;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
#pragma warning disable CS0649, CS0169

[assembly: InternalsVisibleTo("OpenX.GfxTests")]

namespace OpenX;

#region Client

public class SdlClientHost : IClientHost {
    public void Dispose() => throw new NotImplementedException();
    public void Run() => throw new NotImplementedException();
}

#endregion

#region Engine

// SdlObjectBuilder
// MISSING

// SdlGfxSprite2D
public class SdlGfxSprite2D : IOpenGfxSprite<object, object> {
    readonly SpriteManager<object> _spriteManager;
    public SdlGfxSprite2D() {
        //_spriteManager = new SpriteManager<Sprite2D>(source, new GodotSpriteBuilder());
    }

    public SpriteManager<object> SpriteManager => _spriteManager;
    public void Preload(ISource source, object path) => _spriteManager.Preload(source, path);
    public Task<(object spr, object tag)> Create(ISource source, object path, object parent = default) => _spriteManager.Create(source, path);
}

// SdlSfx
public class SdlSfx : SystemSfx { }

// SdlEngine
public class SdlEngine : Engine {
    public static readonly Engine This = new SdlEngine();
    SdlEngine() : base("SD", "SDL 3") {
        Caps = EngineX.Caps.Drawing;
        GfxFactory = () => [new SdlGfxSprite2D(), null, null, null, null, null];
        SfxFactory = () => [new SdlSfx()];
    }
}

#endregion