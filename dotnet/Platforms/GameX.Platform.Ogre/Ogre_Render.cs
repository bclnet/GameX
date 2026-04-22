using OpenStack;
using OpenStack.Gfx;
using OpenStack.Gfx.Ogre;
#pragma warning disable CS9113, CS0169

namespace GameX.Platforms.Ogre;

public static class OgreRenderer {
    public static Renderer CreateRenderer(object parent, IOpenGfx[] gfx, object obj, string type)
        => type switch {
            "TestTri" => new TestTriRenderer(gfx, obj),
            "Texture" => new TextureRenderer(gfx, obj, 0..),
            //"Object" => new ObjectRenderer(gfx, obj),
            //"Cell" => new CellRenderer(gfx, obj),
            //"Engine" => new EngineRenderer(gfx, obj),
            _ => default,
        };
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
