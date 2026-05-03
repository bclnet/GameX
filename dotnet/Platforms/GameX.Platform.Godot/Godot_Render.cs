using Godot;
using OpenStack;
using OpenStack.Gfx;
using OpenStack.Gfx.Godot;
using System;
#pragma warning disable CS9113

namespace GameX.Platforms.Godot;

public static class GodotRenderer {
    public static Renderer CreateRenderer(object parent, IOpenGfx[] gfx, object obj, string type) {
        if (obj is IHaveOpenGfx z) gfx = z.Gfx;
        return type switch {
            "TestTri" => new TestTriRenderer(parent as Node, gfx, obj),
            "Texture" => new TextureRenderer(parent as Node, gfx, obj, 0..),
            //"Object" => new ObjectRenderer(parent as Node, gfx, obj),
            //"Cell" => new CellRenderer(parent as Node, gfx, obj),
            //"Engine" => new EngineRenderer(parent as Node, gfx, obj),
            _ => default
        };
    }
}

public class ViewInfo : Node {
    static ViewInfo() => PlatformX.Activate(GodotPlatform.This);

    public string FamilyId = "Bethesda";
    public string ArcUri = "game:/#Morrowind";
    public string Type = "Texture";
    public string Path = "Morrowind.bsa:bookart/boethiah_256.dds";
    //public string Path = "meshes/x/ex_common_balcony_01.nif";

    protected Family Family;
    protected Archive Source;
    Renderer Renderer;

    public override void _Ready() {
        // parse args
        var args = System.Environment.GetCommandLineArgs();
        ShellState s;
        if (args.Length > 1 && (s = ShellState.Parse(args[1])) != null) { FamilyId = s.FamilyId; ArcUri = s.ArcUri; Type = s.Type; Path = s.Path; }
        //Log.Info($"FamilyId: '{FamilyId}'");
        //Log.Info($"ArcUri: '{ArcUri}'");
        //Log.Info($"Type: '{Type}'");
        //Log.Info($"Path: '{Path}'");

        if (string.IsNullOrEmpty(FamilyId)) return;
        Family = FamilyManager.GetFamily(FamilyId);
        if (!string.IsNullOrEmpty(ArcUri)) Source = Family.GetArchive(new Uri(ArcUri));
        var value = Source.GetAsset<object>(Path).Result;
        Renderer = GodotRenderer.CreateRenderer(this, Source?.Gfx, value, Type);
        Renderer?.Start();
    }

    public override void _Process(double delta) => Renderer?.Update((float)delta);

    public override void _Notification(int what) {
        base._Notification(what);
        switch ((long)what) {
            case NotificationPredelete:
                Renderer?.Dispose();
                Source?.Dispose();
                break;
        }
    }
}
