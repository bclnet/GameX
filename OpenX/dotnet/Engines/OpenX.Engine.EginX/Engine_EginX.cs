using EginX;
using OpenX.Client;
using OpenX.Gfx;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
#pragma warning disable CS0649, CS0169

[assembly: InternalsVisibleTo("OpenX.GfxTests")]

namespace OpenX;

#region Client

public class ExClientHost : Game, IClientHost {
    public ClientBase Client;
    public SceneBase Scene;
    public IPluginHost PluginHost;

    public ExClientHost(Func<ClientBase> client) {
        Client = client();
        DeviceManager = new GraphicsDeviceManager(this);
        //PluginHost = pluginHost; IPluginHost pluginHost, string title
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T GetScene<T>() where T : SceneBase => Scene as T;

    public void SetScene(SceneBase scene) { Scene?.Dispose(); Scene = scene; Scene?.Load(); }

    protected override async Task LoadContent() {
        await base.LoadContent();
        await Client.LoadContent();
    }

    protected override async Task UnloadContent() {
        await Client.UnloadContent();
        await base.UnloadContent();
    }
}


#endregion

#region Engine

// EginXObjectBuilder
// MISSING

// EginXGfxSprite2D
public class EginXGfxSprite2D : IOpenGfxSprite<object, object> {
    readonly SpriteManager<object> _spriteManager;
    public EginXGfxSprite2D() {
        //_spriteManager = new SpriteManager<Sprite2D>(new GodotSpriteBuilder());
    }

    public SpriteManager<object> SpriteManager => _spriteManager;
    public void Preload(ISource source, object path) => _spriteManager.Preload(source, path);
    public Task<(object spr, object tag)> Create(ISource source, object path, object parent = null) => _spriteManager.Create(source, path);
}

// EginXEngine
public class EginXEngine : Engine {
    public static readonly Engine This = new EginXEngine();
    EginXEngine() : base("EX", "EginX") {
        Caps = EngineX.Caps.Drawing;
        GfxFactory = () => [null, new EginXGfxSprite2D(), null, null, null, null];
        SfxFactory = () => [new SystemSfx()];
    }
}

#endregion