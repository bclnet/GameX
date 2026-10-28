using System;
using Godot;

// boot.tscn looks this class up by res://scripts/ViewInfoX.cs. GameX sets Attach.
[assembly: AssemblyHasScripts(new System.Type[] { typeof(OpenX.ViewInfoX) })]

namespace OpenX;

[ScriptPath("res://scripts/ViewInfoX.cs")]
public partial class ViewInfoX : Node {
    public static Action<ViewInfoX> Attach;
    public Node View;

    public override void _Ready() {
        Attach?.Invoke(this);
        ShellPipe.Start();
    }

    public override void _Process(double delta) {
        ShellPipe.Tick();
        View?._Process(delta);
    }

    public override void _Notification(int what) {
        if (what == NotificationPredelete) View?._Notification(what);
    }
}
