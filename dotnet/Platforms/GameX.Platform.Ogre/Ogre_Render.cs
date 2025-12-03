using OpenStack;
using OpenStack.Gfx;
using OpenStack.Gfx.Ogre;
using System;
using System.Collections.Generic;
using static OpenStack.Gfx.GfX;

namespace GameX.Platforms.Ogre;

public static class OgreRenderer {
    public static Renderer CreateRenderer(object parent, IList<IOpenGfx> gfx, object obj, string type)
        => type switch {
            "TestTri" => new OgreTestTriRenderer(gfx[XModel] as OgreGfxModel, obj),
            "Texture" => new OgreTextureRenderer(gfx[XModel] as OgreGfxModel, obj),
            "Object" => new OgreObjectRenderer(gfx[XModel] as OgreGfxModel, obj),
            "Cell" => new OgreCellRenderer(gfx[XModel] as OgreGfxModel, obj),
            "Engine" => new OgreEngineRenderer(gfx[XModel] as OgreGfxModel, obj),
            _ => new OgreObjectRenderer(gfx[XModel] as OgreGfxModel, obj),
        };
}

public class OgreTestTriRenderer(OgreGfxModel gfx, object obj) : TestTriRenderer(gfx, obj) { }
public class OgreCellRenderer(OgreGfxModel gfx, object obj) : Renderer { }
public class OgreEngineRenderer(OgreGfxModel gfx, object obj) : Renderer { }
public class OgreObjectRenderer(OgreGfxModel gfx, object obj) : Renderer { }
public class OgreTextureRenderer(OgreGfxModel gfx, object obj) : TextureRenderer(gfx, obj, Level) {
    static Range Level = 0..;
}

public class ViewInfo {
    static ViewInfo() => PlatformX.Activate(OgrePlatform.This);

    public string FamilyId = "Bethesda";
    public string ArcUri = "game:/Morrowind.bsa#Morrowind";
    public string Type = "Texture";
    public string Path = "bookart/boethiah_256.dds";
    //public string Path = "meshes/x/ex_common_balcony_01.nif";

    protected Family Family;
    protected Archive Source;
    protected OgreGfxModel Gfx;
    Renderer Renderer;

    //public override void _Ready()
    //{
    //    if (string.IsNullOrEmpty(FamilyId)) return;
    //    Family = FamilyManager.GetFamily(FamilyId);
    //    if (!string.IsNullOrEmpty(ArcUri)) Archives.Add(Family.OpenArchive(new Uri(ArcUri)));
    //    var first = Archives.FirstOrDefault();
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
    //            foreach (var archive in Archives) archive.Dispose();
    //            Archives.Clear();
    //            break;
    //    }
    //}
}
