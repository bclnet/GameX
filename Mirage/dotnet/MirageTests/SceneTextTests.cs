using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Drawing;
using System.Numerics;

namespace Mirage;

[TestClass]
public class SceneTextTests {
    const string Sample = @"
Root
  Player layer=1 pos=(1,0,0)
    @MeshRenderer mesh=Cube material=Std
    @MeshCollider mesh=Cube
    Weapon rot=(0,0.7071,0,0.7071) scale=(2,2,2) hidden
      @MeshRenderer mesh=Sword
  ""Sky Light"" tag=light static
    @Light radius=5 color=#FF8000 indoors
";

    [TestMethod]
    public void Write_OmitsIdentityAndDefaults() {
        var root = new Node("Root");
        root.AddChild("Plain");
        Assert.AreEqual("Root\n  Plain\n", SceneText.Write(root));
    }

    [TestMethod]
    public void Write_EmitsTransformTagsAndComponents() {
        var tree = new SceneTree();
        var p = tree.Root.AddChild("Player");
        p.Layer = 1; p.Transform.Position = new Vector3(1, 0, 0);
        p.AddComponent(new MeshRenderer { Mesh = tree.Resources.Get<Mesh>("Cube"), Material = tree.Resources.Get<Material>("Std") });
        p.AddComponent(new MeshCollider { Mesh = tree.Resources.Get<Mesh>("Cube") });
        var w = p.AddChild("Weapon");
        w.Transform.Rotation = new Quaternion(0, 0.7071f, 0, 0.7071f); w.Transform.Scale = new Vector3(2, 2, 2); w.Visible = false;
        w.AddComponent(new MeshRenderer { Mesh = tree.Resources.Get<Mesh>("Sword") });
        var l = tree.Root.AddChild("Sky Light"); l.Tag = "light"; l.IsStatic = true;
        l.AddComponent(new Light { Radius = 5, Color = Color.FromArgb(255, 128, 0), Indoors = true });
        Assert.AreEqual(Sample.TrimStart('\r', '\n'), tree.ToText());
    }

    [TestMethod]
    public void Parse_RoundTripsThroughWrite() {
        var root = SceneText.Parse(Sample);
        Assert.AreEqual(Sample.TrimStart('\r', '\n'), SceneText.Write(root));
        var weapon = root.Find("Player/Weapon");
        Assert.IsFalse(weapon.Visible);
        Assert.AreEqual(new Vector3(2, 2, 2), weapon.Transform.Scale);
        var light = root.Find("Sky Light").GetComponent<Light>();
        Assert.AreEqual(5f, light.Radius);
        Assert.IsTrue(light.Indoors);
        Assert.AreEqual(Color.FromArgb(255, 128, 0).ToArgb(), light.Color.ToArgb());
        Assert.AreSame(root.Find("Player").GetComponent<MeshRenderer>().Mesh, root.Find("Player").GetComponent<MeshCollider>().Mesh, "one resource per name");
    }

    [TestMethod]
    public void Parse_ToleratesIndentStyleBlankLinesAndComments() {
        var text = "\n# a comment\nRoot\n\n\tA\n\t\t@Camera\n\t\tB\n";
        Assert.AreEqual("Root\n  A\n    @Camera\n    B\n", SceneText.Normalize(text));
        Assert.IsTrue(SceneText.AreEqual(text, "Root\n  A\n    @Camera fov=60\n    B\n"), "defaults compare equal");
    }

    [TestMethod]
    public void Parse_RejectsBadInput() {
        Assert.ThrowsExactly<FormatException>(() => SceneText.Parse("Root\n    TooDeep\n"));
        Assert.ThrowsExactly<FormatException>(() => SceneText.Parse("Root\n  @NoSuchComponent\n"));
        Assert.ThrowsExactly<FormatException>(() => SceneText.Parse("Root\n  A nosuch=1\n"));
        Assert.ThrowsExactly<FormatException>(() => SceneText.Parse("Root\nSecond\n"));
        Assert.ThrowsExactly<FormatException>(() => SceneText.Parse("@Camera\n"));
        Assert.ThrowsExactly<FormatException>(() => SceneText.Parse(""));
    }

    [TestMethod]
    public void Quote_EscapesAndRoundTrips() {
        var root = new Node("Root");
        root.AddChild("has \"quotes\" and spaces");
        var text = SceneText.Write(root);
        Assert.AreEqual("has \"quotes\" and spaces", SceneText.Parse(text).Children[0].Name);
    }

    [TestMethod]
    public void Register_LetsTheParserCreateCustomComponents() {
        SceneText.Register<Spin>();
        var root = SceneText.Parse("Root\n  A\n    @Spin speed=2.5\n");
        Assert.AreEqual(2.5f, root.Find("A").GetComponent<Spin>().Speed);
        Assert.AreEqual("Root\n  A\n    @Spin speed=2.5\n", SceneText.Write(root));
    }

    class Spin : Component {
        public float Speed;
        public override void WriteProps(SceneText.Props w) => w.Prop("speed", Speed);
        public override bool ReadProp(string key, string value, ResourceSet r) { if (key != "speed") return false; Speed = SceneText.ParseFloat(value); return true; }
    }

    [TestMethod]
    public void Resources_WriteAsOneLineEach() {
        var set = new ResourceSet();
        set.Add(Mesh.Quad());
        set.Add(new Texture { Name = "tex", Width = 4, Height = 2, Format = "RGBA32", Bytes = 32 });
        Assert.AreEqual("Mesh Quad vertices=4 triangles=2 normals uvs min=(-1,-1,0) max=(1,1,0)\nTexture tex size=4x2 format=RGBA32 bytes=32\n", set.ToString());
    }
}
