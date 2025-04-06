using Godot;
using OpenStack;
using OpenStack.Gfx;
using OpenStack.Godot;
using OpenStack.Godot.Renderers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameX.Platforms.Godot;

public static class GodotRenderer
{
    public static Renderer CreateRenderer(object parent, GodotGfx3dModel gfx, object obj, string type)
        => type switch
        {
            "TestTri" => new GodotTestTriRenderer(parent as Node, gfx, obj),
            "Texture" => new GodotTextureRenderer(parent as Node, gfx, obj),
            "Object" => new GodotObjectRenderer(parent as Node, gfx, obj),
            "Cell" => new GodotCellRenderer(parent as Node, gfx, obj),
            "Engine" => new GodotEngineRenderer(parent as Node, gfx, obj),
            _ => new GodotObjectRenderer(parent as Node, gfx, obj),
        };
}

public class GodotTestTriRenderer(Node parent, GodotGfx3dModel gfx, object obj) : TestTriRenderer(parent, gfx, obj) { }
public class GodotCellRenderer(Node parent, GodotGfx3dModel gfx, object obj) : Renderer { }
public class GodotEngineRenderer(Node parent, GodotGfx3dModel gfx, object obj) : Renderer { }
public class GodotObjectRenderer(Node parent, GodotGfx3dModel gfx, object obj) : Renderer { }
public class GodotTextureRenderer(Node parent, GodotGfx3dModel gfx, object obj) : TextureRenderer(parent, gfx, obj, Level)
{
    static System.Range Level = 0..;
}

public class ViewInfo : Node
{
    static ViewInfo() => PlatformX.Activate(GodotPlatform.This);

    public enum Kind { Texture, TextureCursor, Object, Cell, Engine }

    public string FamilyId = "Bethesda";
    public string PakUri = "game:/Morrowind.bsa#Morrowind";

    public Kind ViewKind = Kind.Texture;
    public string Param1 = "bookart/boethiah_256.dds";
    //public string Param1 = "meshes/x/ex_common_balcony_01.nif";

    protected Family Family;
    protected List<PakFile> PakFiles = [];
    protected GodotGfx3dModel Gfx;

    Renderer Renderer;

    public override void _Ready()
    {
        if (string.IsNullOrEmpty(FamilyId)) return;
        Family = FamilyManager.GetFamily(FamilyId);
        if (!string.IsNullOrEmpty(PakUri)) PakFiles.Add(Family.OpenPakFile(new Uri(PakUri)));
        var first = PakFiles.FirstOrDefault();
        Gfx = (GodotGfx3dModel)first?.Gfx[GFX.X3dModel];
        Renderer = GodotRenderer.CreateRenderer(this, Gfx, Param1, ViewKind.ToString());
        Renderer?.Start();
    }

    public override void _Process(double delta) => Renderer?.Update((float)delta);

    public override void _Notification(int what)
    {
        base._Notification(what);
        switch ((long)what)
        {
            case NotificationPredelete:
                Renderer?.Dispose();
                foreach (var pakFile in PakFiles) pakFile.Dispose();
                PakFiles.Clear();
                break;
        }
    }
}
