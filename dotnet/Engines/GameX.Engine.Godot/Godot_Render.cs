using GameX.Arkane.Formats.Danae;
using GameX.Gamebryo.Formats;
using Godot;
using OpenX;
using OpenX.Gfx;
using OpenX.Gfx.Godot;
using System;
#pragma warning disable CS9113

namespace GameX.Engines.Godot;

public static class GodotRenderer {
    static GodotRenderer() {
        GodotX.BuildersByType[typeof(Binary_Nif)] = GodotNifObjectBuilder.BuildObject;
        GodotX.BuildersByType[typeof(Binary_Ftl)] = GodotFtlObjectBuilder.BuildObject;
        GodotX.BuildersByType[typeof(Binary_Fts)] = GodotFtsObjectBuilder.BuildObject;
    }

    public static Renderer CreateRenderer(object parent, IOpenGfx[] gfx, ISource source, object obj, string type) {
        if (obj is IHaveSource z) source = z.Source;
        return type switch {
            "TestTri" => new TestTriRenderer(parent as Node, gfx, source, obj),
            "Texture" => new TextureRenderer(parent as Node, gfx, source, obj, 0..),
            "Object" => new ObjectRenderer(parent as Node, gfx, source, obj),
            //"Engine" => new EngineRenderer(parent as Node, gfx, source, obj),
            _ => default
        };
    }
}

public class ViewInfo : Node {
    static ViewInfo() => EngineX.Activate(GodotEngine.This);

    public string FamilyId = "Arkane";
    public string ArcUri = "game:/#AF";
    public string Type = "Texture";
    public string Path = "Graph/interface/misc/quit1.bmp";

    //public string FamilyId = "Bethesda";
    //public string ArcUri = "game:/#Morrowind";
    //public string Type = "Texture";
    //public string Path = "Morrowind.bsa:bookart/boethiah_256.dds";
    ////public string Path = "meshes/x/ex_common_balcony_01.nif";

    protected Family Family;
    protected Archive Source;
    Renderer Renderer;
    Node _sceneNode;
    string _scene;

    public override void _Ready() {
        try {
            var s = ShellState.FromArgs(System.Environment.GetCommandLineArgs()) ?? ShellState.FromArgs(OS.GetCmdlineUserArgs());
            if (s != null) { FamilyId = s.FamilyId; ArcUri = s.ArcUri; Type = s.Type; Path = s.Path; }
            Open();
        }
        catch (Exception e) { Log.Exception(e); }
    }

    public override void _Process(double delta) => Renderer?.Update((float)delta);

    public void Show(ShellState next) {
        if (next == null) return;
        if (next.FamilyId == FamilyId && next.ArcUri == ArcUri && next.Type == Type && next.Path == Path) return;
        Renderer?.Dispose();
        Renderer = null;
        Source?.Dispose();
        Source = null;
        FamilyId = next.FamilyId;
        ArcUri = next.ArcUri;
        Type = next.Type;
        Path = next.Path;
        Open();
    }

    void Open() {
        try { OpenCore(); }
        catch (Exception e) { Log.Exception(e); }
    }

    void OpenCore() {
        var scene = ShellState.SceneFor(Type) == "Scene2D" ? "res://scenes/scene_2d.tscn" : "res://scenes/scene_3d.tscn";
        if (_scene != scene) {
            if (_sceneNode != null) { _sceneNode.QueueFree(); _sceneNode = null; }
            _scene = scene;
            var packed = GD.Load<PackedScene>(scene);
            if (packed != null) { _sceneNode = packed.Instantiate(); AddChild(_sceneNode); }
        }
        if (string.IsNullOrEmpty(FamilyId) || string.IsNullOrEmpty(Path)) return;
        Family = FamilyManager.GetFamily(FamilyId);
        if (!string.IsNullOrEmpty(ArcUri)) Source = Family.GetArchive(new Uri(ArcUri));
        if (Source == null) return;
        var value = Source.GetAsset<object>(Path).Result;
        Renderer = GodotRenderer.CreateRenderer(this, EngineX.Gfx, Source, value, Type);
        Renderer?.Start();
    }

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
