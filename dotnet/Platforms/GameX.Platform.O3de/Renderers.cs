using OpenStack;
using OpenStack.Gfx;
using OpenStack.O3de;
using OpenStack.O3de.Renderers;
using System;
using System.Collections.Generic;

namespace GameX.Platforms.O3de;

public static class O3deRenderer
{
    public static Renderer CreateRenderer(object parent, O3deGfxModel gfx, object obj, string type)
        => type switch
        {
            "TestTri" => new O3deTestTriRenderer(gfx, obj),
            "Texture" => new O3deTextureRenderer(gfx, obj),
            "Object" => new O3deObjectRenderer(gfx, obj),
            "Cell" => new O3deCellRenderer(gfx, obj),
            "Engine" => new O3deEngineRenderer(gfx, obj),
            _ => new O3deObjectRenderer(gfx, obj),
        };
}

public class O3deTestTriRenderer(O3deGfxModel gfx, object obj) : TestTriRenderer(gfx, obj) { }
public class O3deCellRenderer(O3deGfxModel gfx, object obj) : Renderer { }
public class O3deEngineRenderer(O3deGfxModel gfx, object obj) : Renderer { }
public class O3deObjectRenderer(O3deGfxModel gfx, object obj) : Renderer { }
public class O3deTextureRenderer(O3deGfxModel gfx, object obj) : TextureRenderer(gfx, obj, Level)
{
    static Range Level = 0..;
}

public class ViewInfo
{
    static ViewInfo() => PlatformX.Activate(O3dePlatform.This);

    public enum Kind { Texture, TextureCursor, Object, Cell, Engine }

    public string FamilyId = "Bethesda";
    public string PakUri = "game:/Morrowind.bsa#Morrowind";

    public Kind ViewKind = Kind.Texture;
    public string Param1 = "bookart/boethiah_256.dds";
    //public string Param1 = "meshes/x/ex_common_balcony_01.nif";

    protected Family Family;
    protected List<PakFile> PakFiles = [];
    protected O3deGfxModel Gfx;

    Renderer Renderer;

    //public override void _Ready()
    //{
    //    if (string.IsNullOrEmpty(FamilyId)) return;
    //    Family = FamilyManager.GetFamily(FamilyId);
    //    if (!string.IsNullOrEmpty(PakUri)) PakFiles.Add(Family.OpenPakFile(new Uri(PakUri)));
    //    var first = PakFiles.FirstOrDefault();
    //    Gfx = (IO3deGfx3d)first?.Gfx;
    //    Renderer = O3deRenderer.CreateRenderer(this, Gfx, Param1, ViewKind.ToString());
    //    Renderer?.Start();
    //}

    //public override void _Process(double delta) => Renderer?.Update((float)delta);

    //public override void _Notification(int what)
    //{
    //    base._Notification(what);
    //    switch ((long)what)
    //    {
    //        case NotificationPredelete:
    //            //Renderer?.Dispose();
    //            foreach (var pakFile in PakFiles) pakFile.Dispose();
    //            PakFiles.Clear();
    //            break;
    //    }
    //}
}
