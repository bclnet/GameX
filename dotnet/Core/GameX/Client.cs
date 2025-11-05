using GameX.Eng;
using System;

namespace GameX;

#region IPluginHost

public interface IPluginHost { }

#endregion

#region GameController

public class GameController : Game {
    public IPluginHost PluginHost;

    public GameController(IPluginHost pluginHost) {
        PluginHost = pluginHost;
    }
}

#endregion