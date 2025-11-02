using System;

namespace GameX;

#region IPluginHost

public interface IPluginHost { }

#endregion

#region Game

public class Game : IDisposable {
    public void Dispose() { }
    public void Run() { }
}

#endregion

#region GameController

public class GameController(IPluginHost pluginHost) : Game {
    public IPluginHost PluginHost = pluginHost;
}

#endregion