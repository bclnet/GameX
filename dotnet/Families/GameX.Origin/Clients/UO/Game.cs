using GameX.Eng;
using GameX.Xbox;
using System;
using System.Threading.Tasks;

namespace GameX.Origin.Clients.UO;

public class UOGameController<Texture2D> : GameController {
    //AudioManager Audio;
    Main Main;
    public UOGameController(Archive game, IPluginHost pluginHost) : base(game, pluginHost) {
        DeviceManager = new GraphicsDeviceManager(this);
        TypeX.ScanTypes([typeof(XboxArchive)]);
        Main = new(((UOGame)game.Game).Uop);
    }

    protected override async Task LoadContent() {
        await base.LoadContent();
        //await Fonts<Texture2D>.Load(Game, Device);
        //SolidColorTextureCache.Load(Device);

        //Audio = new AudioManager();
        // using var ms = new MemoryStream(Loader.GetBackgroundImage());
        // _background = Texture2D.FromStream(GraphicsDevice, ms);

#if false
        //SetScene(new MainScene(this));
#else
        await Main.Load(Game, this);
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