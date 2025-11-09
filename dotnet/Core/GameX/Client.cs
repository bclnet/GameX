using GameX.Eng;

namespace GameX;

#region IPluginHost

public interface IPluginHost { }

#endregion

#region GameController

public class GameController : Game {
    public PakFile Game;
    public IPluginHost PluginHost;

    public GameController(PakFile game, IPluginHost pluginHost) {
        Game = game;
        PluginHost = pluginHost;
    }
}

#endregion