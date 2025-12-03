using OpenStack;
using OpenStack.Gfx;
using OpenStack.Gfx.O3de;
using System;
using System.Collections.Generic;
using static OpenStack.Gfx.GfX;

namespace GameX.Platforms.O3de;

public static class O3deRenderer {
    public static Renderer CreateRenderer(object parent, IList<IOpenGfx> gfx, object obj, string type)
        => type switch {
            "TestTri" => new O3deTestTriRenderer(gfx[XModel] as O3deGfxModel, obj),
            "Texture" => new O3deTextureRenderer(gfx[XModel] as O3deGfxModel, obj),
            "Object" => new O3deObjectRenderer(gfx[XModel] as O3deGfxModel, obj),
            "Cell" => new O3deCellRenderer(gfx[XModel] as O3deGfxModel, obj),
            "Engine" => new O3deEngineRenderer(gfx[XModel] as O3deGfxModel, obj),
            _ => new O3deObjectRenderer(gfx[XModel] as O3deGfxModel, obj),
        };
}

public class O3deTestTriRenderer(O3deGfxModel gfx, object obj) : TestTriRenderer(gfx, obj) { }
public class O3deCellRenderer(O3deGfxModel gfx, object obj) : Renderer { }
public class O3deEngineRenderer(O3deGfxModel gfx, object obj) : Renderer { }
public class O3deObjectRenderer(O3deGfxModel gfx, object obj) : Renderer { }
public class O3deTextureRenderer(O3deGfxModel gfx, object obj) : TextureRenderer(gfx, obj, Level) {
    static Range Level = 0..;
}

public class ViewInfo {
    static ViewInfo() => PlatformX.Activate(O3dePlatform.This);

    public string FamilyId = "Bethesda";
    public string PakUri = "game:/Morrowind.bsa#Morrowind";
    public string Type = "Texture";
    public string Path = "bookart/boethiah_256.dds";
    //public string Path = "meshes/x/ex_common_balcony_01.nif";

    protected Family Family;
    protected Archive Source;
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
