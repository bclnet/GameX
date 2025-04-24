using OpenStack;
using OpenStack.Gfx;
using OpenStack.Gfx.Unreal;
using System;
using System.Collections.Generic;
using static OpenStack.Gfx.GfX;

namespace GameX.Platforms.Unreal;

public static class UnrealRenderer {
    public static Renderer CreateRenderer(object parent, IList<IOpenGfx> gfx, object obj, string type)
        => type switch {
            "TestTri" => new OgreTestTriRenderer(gfx[XModel] as UnrealGfxModel, obj),
            "Texture" => new OgreTextureRenderer(gfx[XModel] as UnrealGfxModel, obj),
            "Object" => new OgreObjectRenderer(gfx[XModel] as UnrealGfxModel, obj),
            "Cell" => new OgreCellRenderer(gfx[XModel] as UnrealGfxModel, obj),
            "Engine" => new OgreEngineRenderer(gfx[XModel] as UnrealGfxModel, obj),
            _ => new OgreObjectRenderer(gfx[XModel] as UnrealGfxModel, obj),
        };
}

public class OgreTestTriRenderer(UnrealGfxModel gfx, object obj) : TestTriRenderer(gfx, obj) { }
public class OgreCellRenderer(UnrealGfxModel gfx, object obj) : Renderer { }
public class OgreEngineRenderer(UnrealGfxModel gfx, object obj) : Renderer { }
public class OgreObjectRenderer(UnrealGfxModel gfx, object obj) : Renderer { }
public class OgreTextureRenderer(UnrealGfxModel gfx, object obj) : TextureRenderer(gfx, obj, Level) {
    static Range Level = 0..;
}

public class ViewInfo {
    static ViewInfo() => PlatformX.Activate(UnrealPlatform.This);

    public enum Kind { Texture, TextureCursor, Object, Cell, Engine }

    public string FamilyId = "Bethesda";
    public string PakUri = "game:/Morrowind.bsa#Morrowind";

    public Kind ViewKind = Kind.Texture;
    public string Param1 = "bookart/boethiah_256.dds";
    //public string Param1 = "meshes/x/ex_common_balcony_01.nif";

    protected Family Family;
    protected List<PakFile> PakFiles = [];
    protected UnrealGfxModel Gfx;

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
