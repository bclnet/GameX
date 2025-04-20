using OpenStack;
using OpenStack.Gfx;
using OpenStack.Gfx.Ogre;
using System;
using System.Collections.Generic;

namespace GameX.Platforms.Ogre;

public static class OgreRenderer
{
    public static Renderer CreateRenderer(object parent, OgreGfxModel gfx, object obj, string type)
        => type switch
        {
            "TestTri" => new OgreTestTriRenderer(gfx, obj),
            "Texture" => new OgreTextureRenderer(gfx, obj),
            "Object" => new OgreObjectRenderer(gfx, obj),
            "Cell" => new OgreCellRenderer(gfx, obj),
            "Engine" => new OgreEngineRenderer(gfx, obj),
            _ => new OgreObjectRenderer(gfx, obj),
        };
}

public class OgreTestTriRenderer(OgreGfxModel gfx, object obj) : TestTriRenderer(gfx, obj) { }
public class OgreCellRenderer(OgreGfxModel gfx, object obj) : Renderer { }
public class OgreEngineRenderer(OgreGfxModel gfx, object obj) : Renderer { }
public class OgreObjectRenderer(OgreGfxModel gfx, object obj) : Renderer { }
public class OgreTextureRenderer(OgreGfxModel gfx, object obj) : TextureRenderer(gfx, obj, Level)
{
    static Range Level = 0..;
}

public class ViewInfo
{
    static ViewInfo() => PlatformX.Activate(OgrePlatform.This);

    public enum Kind { Texture, TextureCursor, Object, Cell, Engine }

    public string FamilyId = "Bethesda";
    public string PakUri = "game:/Morrowind.bsa#Morrowind";

    public Kind ViewKind = Kind.Texture;
    public string Param1 = "bookart/boethiah_256.dds";
    //public string Param1 = "meshes/x/ex_common_balcony_01.nif";

    protected Family Family;
    protected List<PakFile> PakFiles = [];
    protected OgreGfxModel Gfx;

    Renderer Renderer;

    //public override void _Ready()
    //{
    //    if (string.IsNullOrEmpty(FamilyId)) return;
    //    Family = FamilyManager.GetFamily(FamilyId);
    //    if (!string.IsNullOrEmpty(PakUri)) PakFiles.Add(Family.OpenPakFile(new Uri(PakUri)));
    //    var first = PakFiles.FirstOrDefault();
    //    Gfx = (IOgreGfx3d)first?.Gfx;
    //    Renderer = OgreRenderer.CreateRenderer(this, Gfx, Param1, ViewKind.ToString());
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
