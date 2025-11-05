using GameX.Eng;
using System;
using OpenStack;

namespace GameX.Origin.Games.UO;

public class UOGameController : GameController {
    public UOGameController(IPluginHost pluginHost) : base(pluginHost) {

    }

    protected override void LoadContent() {
        base.LoadContent();
        // Fonts.initialize(self.graphicsDevice);
        //SolidColorTextureCache.Initialize(GraphicsDevice);
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

    protected override void UnloadContent() { }
}