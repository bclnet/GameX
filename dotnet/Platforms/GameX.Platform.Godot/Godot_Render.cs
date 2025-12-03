using Godot;
using OpenStack;
using OpenStack.Gfx;
using OpenStack.Gfx.Godot;
using System;
using System.Collections.Generic;
using static OpenStack.Gfx.GfX;

namespace GameX.Platforms.Godot;

public static class GodotRenderer {
    public static Renderer CreateRenderer(object parent, IList<IOpenGfx> gfx, object obj, string type)
        => type switch {
            "TestTri" => new GodotTestTriRenderer(parent as Node, gfx[XModel] as GodotGfxModel, obj),
            "Texture" => new GodotTextureRenderer(parent as Node, gfx[XModel] as GodotGfxModel, obj),
            "Object" => new GodotObjectRenderer(parent as Node, gfx[XModel] as GodotGfxModel, obj),
            "Cell" => new GodotCellRenderer(parent as Node, gfx[XModel] as GodotGfxModel, obj),
            "Engine" => new GodotEngineRenderer(parent as Node, gfx[XModel] as GodotGfxModel, obj),
            _ => new GodotObjectRenderer(parent as Node, gfx[XModel] as GodotGfxModel, obj),
        };
}

public class GodotTestTriRenderer(Node parent, GodotGfxModel gfx, object obj) : TestTriRenderer(parent, gfx, obj) { }
public class GodotCellRenderer(Node parent, GodotGfxModel gfx, object obj) : Renderer { }
public class GodotEngineRenderer(Node parent, GodotGfxModel gfx, object obj) : Renderer { }
public class GodotObjectRenderer(Node parent, GodotGfxModel gfx, object obj) : Renderer { }
public class GodotTextureRenderer(Node parent, GodotGfxModel gfx, object obj) : TextureRenderer(parent, gfx, obj, Level) {
    static System.Range Level = 0..;
}

public class ViewInfo : Node {
    static ViewInfo() => PlatformX.Activate(GodotPlatform.This);

    public string FamilyId = "Bethesda";
    public string ArcUri = "game:/Morrowind.bsa#Morrowind";
    public string Type = "Texture";
    public string Path = "bookart/boethiah_256.dds";
    //public string Path = "meshes/x/ex_common_balcony_01.nif";

    protected Family Family;
    protected Archive Source;
    Renderer Renderer;

    public override void _Ready() {
        if (string.IsNullOrEmpty(FamilyId)) return;
        Family = FamilyManager.GetFamily(FamilyId);
        if (!string.IsNullOrEmpty(ArcUri)) Source = Family.OpenArchive(new Uri(ArcUri));
        Renderer = GodotRenderer.CreateRenderer(this, Source?.Gfx, Path, Type);
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
