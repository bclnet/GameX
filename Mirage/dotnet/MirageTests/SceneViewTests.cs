using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mirage;

[TestClass]
public class SceneViewTests {
    static Bitmap Open(byte[] png) {
        var bmp = new Bitmap(new System.IO.MemoryStream(png));
        Assert.IsTrue(png.Length > 8 && png[0] == 0x89 && png[1] == (byte)'P');
        return bmp;
    }

    static (int red, int ink) Scan(Bitmap bmp) {
        var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        try {
            var red = 0; var ink = 0;
            var row = new byte[data.Stride];
            for (var y = 0; y < bmp.Height; y++) {
                System.Runtime.InteropServices.Marshal.Copy(System.IntPtr.Add(data.Scan0, y * data.Stride), row, 0, row.Length);
                for (var x = 0; x < bmp.Width; x++) {
                    var i = x * 4;
                    var b = row[i]; var g = row[i + 1]; var r = row[i + 2];
                    if (r > 200 && g < 40 && b < 40) red++;
                    if (r != 28 || g != 30 || b != 36) ink++;
                }
            }
            return (red, ink);
        }
        finally { bmp.UnlockBits(data); }
    }

    [TestMethod]
    public void Png_DrawsKeptTexturePixels() {
        var tree = new SceneTree();
        var px = new byte[4 * 4 * 4];
        for (var i = 0; i < px.Length; i += 4) { px[i] = 255; px[i + 3] = 255; }
        tree.Resources.Add(new Texture { Name = "red", Width = 4, Height = 4, Format = "RGBA32", Bytes = px.Length, Pixels = px });
        using var bmp = Open(SceneView.Png(tree));
        Assert.IsTrue(Scan(bmp).red > 10);
    }

    [TestMethod]
    public void Png_DrawsMeshBounds() {
        var tree = new SceneTree();
        var mesh = Mesh.Triangle();
        tree.Resources.Add(mesh);
        tree.Root.AddChild("Obj").AddComponent(new MeshRenderer { Mesh = mesh });
        using var empty = Open(SceneView.Png(new SceneTree()));
        using var drawn = Open(SceneView.Png(tree));
        Assert.IsTrue(Scan(drawn).ink > Scan(empty).ink);
    }

    [TestMethod]
    public void Png_SkipsHiddenPrefab() {
        var tree = new SceneTree();
        var mesh = Mesh.Triangle();
        var hidden = tree.Root.AddChild("_Prefabs");
        hidden.Visible = false;
        hidden.AddChild("prefab").AddComponent(new MeshRenderer { Mesh = mesh });
        using var empty = Open(SceneView.Png(new SceneTree()));
        using var drawn = Open(SceneView.Png(tree));
        Assert.AreEqual(Scan(empty).ink, Scan(drawn).ink);
    }
}
