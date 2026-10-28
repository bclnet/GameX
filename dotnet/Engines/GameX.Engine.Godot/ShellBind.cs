using OpenX;

namespace GameX.Engines.Godot;

public static class ShellBind {
    public static void Init() => ViewInfoX.Attach = view => {
        var info = new ViewInfo();
        view.AddChild(info);
        info._Ready();
        view.View = info;
        ShellPipe.OnItem = line => info.Show(ShellState.FromArgument(line));
    };
}
