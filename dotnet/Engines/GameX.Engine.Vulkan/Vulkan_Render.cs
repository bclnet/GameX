using OpenX;
using OpenX.Gfx;
using OpenX.Gfx.Vulkan;
#pragma warning disable CS9113, CS0169

namespace GameX.Engines.Vulkan;

/// <summary>
/// VulkanRenderer
/// </summary>
public static class VulkanRenderer {
    /// <summary>
    /// CreateRenderer
    /// </summary>
    public static Renderer CreateRenderer(object parent, IOpenGfx[] gfx, ISource source, object obj, string type) {
        if (obj is IHaveSource z) source = z.Source;
        return type switch {
            "TestTri" => new TestTriRenderer(gfx, source, obj),
            "Texture" => new TextureRenderer(gfx, source, obj, 0..),
            //"Object" => new ObjectRenderer(gfx, source, obj),
            //"Cell" => new CellRenderer(gfx, source, obj),
            //"Engine" => new EngineRenderer(gfx, source, obj),
            _ => default,
        };
    }
}

/// <summary>
/// ViewInfo
/// </summary>
public class ViewInfo {
    static ViewInfo() => EngineX.Activate(VulkanEngine.This);

    /// <summary>FamilyId</summary>
    public string FamilyId = "Bethesda";
    /// <summary>ArcUri</summary>
    public string ArcUri = "game:/Morrowind.bsa#Morrowind";
    /// <summary>Type</summary>
    public string Type = "Texture";
    /// <summary>Path</summary>
    public string Path = "bookart/boethiah_256.dds";
    //public string Path = "meshes/x/ex_common_balcony_01.nif";

    /// <summary>Family</summary>
    protected Family Family;
    /// <summary>Source</summary>
    protected Archive Source;
    /// <summary>Gfx</summary>
    protected VulkanGfxModel Gfx;
    Renderer Renderer;

    //public override void _Ready()
    //{
    //    if (string.IsNullOrEmpty(FamilyId)) return;
    //    Family = FamilyManager.GetFamily(FamilyId);
    //    if (!string.IsNullOrEmpty(ArcUri)) Archives.Add(Family.OpenArchive(new Uri(ArcUri)));
    //    var first = Archives.FirstOrDefault();
    //    Gfx = (IOgreGfx3d)first?.Gfx;
    //    Renderer = VulkanRenderer.CreateRenderer(this, Gfx, Param1, ViewKind.ToString());
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
