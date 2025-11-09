using GameX.Eng;
using GameX.Origin.Renderers.UO;
using System.Threading.Tasks;

namespace GameX.Origin.Clients.UO;

public class UOGameController<Texture2D> : GameController {
    public UOGameController(PakFile game, IPluginHost pluginHost) : base(game, pluginHost) {
        DeviceManager = new GraphicsDeviceManager(this);
    }

    protected override async Task LoadContent() {
        await base.LoadContent();
        await Fonts<Texture2D>.Load(Game, Device);
        //SolidColorTextureCache.Load(Game, Device);
        //Audio = new AudioManager();
        // var bytes = Loader.GetBackgroundImage().ToArray();
        // using var ms = new MemoryStream(bytes);
        // _background = Texture2D.FromStream(GraphicsDevice, ms);

#if false
        //SetScene(new MainScene(this));
#else

        //UO.Load(this);
        //Audio.Initialize();

        // TODO: temporary fix to avoid crash when laoding plugins
        // Settings.GlobalSettings.Encryption = (byte)NetClient.Socket.Load(UO.FileManager.Version, (EncryptionType) Settings.GlobalSettings.Encryption);
        // Debug.Trace("Loading plugins...");
        // PluginHost?.Initialize();
        // foreach (string p in Settings.GlobalSettings.Plugins) Plugin.Create(p);
        // _pluginsInitialized = true;
        // Debug.Trace("Done!");

        // SetScene(new LoginScene(UO.World));
#endif
        // SetWindowPositionBySettings();
    }

    protected override Task UnloadContent() => Task.CompletedTask;
}