from enum import IntFlag
from gamex import PakFile
from gamex.core.client import IPluginHost, GameController
from gamex.core.eng.eng import GraphicsDeviceManager
from gamex.families.Origin.renderers.UO.sprites import Fonts
# from gamex.core.formats.network import PacketLogger

# UOGameController
class UOGameController(GameController):
    def __init__(self, game: PakFile, pluginHost: IPluginHost):
        super().__init__(game, pluginHost)
        self.deviceManager = GraphicsDeviceManager(self)
    def loadContent(self) -> None:
        super().loadContent()
        Fonts.load(self.game, self.device)
        # SolidColorTextureCache.Initialize(GraphicsDevice);
        # Audio = new AudioManager();

        # var bytes = Loader.GetBackgroundImage().ToArray();
        # using var ms = new MemoryStream(bytes);
        # _background = Texture2D.FromStream(GraphicsDevice, ms);
        
#if false
        #SetScene(new MainScene(this));
#else
        #UO.Load(this);
        #Audio.Initialize();

        # TODO: temporary fix to avoid crash when laoding plugins
        # Settings.GlobalSettings.Encryption = (byte)NetClient.Socket.Load(UO.FileManager.Version, (EncryptionType) Settings.GlobalSettings.Encryption);
        # Debug.Trace("Loading plugins...");
        # PluginHost?.Initialize();
        # foreach (string p in Settings.GlobalSettings.Plugins) Plugin.Create(p);
        # _pluginsInitialized = true;
        # Debug.Trace("Done!");
        
        # SetScene(new LoginScene(UO.World));
#endif
        # SetWindowPositionBySettings();

    def unloadContent(self) -> None: pass
