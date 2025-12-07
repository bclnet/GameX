using OpenStack.Client;

namespace GameX;

#region GameClient

public class GameClient(ClientState state) : ClientBase() {
    public Archive Archive = state.Archive;
    public object Tag = state.Tag;
}

#endregion