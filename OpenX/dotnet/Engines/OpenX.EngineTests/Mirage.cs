using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mirage;
using OpenX.Gfx;
using OpenX.Gfx.Mirage;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace OpenX.Engines;

/// <summary>
/// A source that serves a handful of in-memory assets by path, the way an archive would.
/// </summary>
class FakeSource : ISource {
    public readonly Dictionary<string, object> Assets = [];
    public int Loads;
    public Task<T> GetAsset<T>(object path, object option = default, bool throwOnError = true) {
        Loads++;
        return Assets.TryGetValue(path.ToString(), out var z) ? Task.FromResult((T)z) : throw new FileNotFound(path.ToString());
    }
    public class FileNotFound(string path) : Exception($"not found: {path}") { }
}

class FakeTexture(int width, int height) : ITexture {
    public int Width => width;
    public int Height => height;
    public int Depth => 1;
    public int MipMaps => 3;
    public TextureFlags TexFlags => 0;
    public T Create<T>(string platform, Func<object, T> func) => func(new TextureAsBytes(new byte[width * height * 4], (TextureFormat.RGBA32, TexturePixel.Byte), [0..(width * height * 4)]));
}

class FakeFrames(int width, int height, int frames) : FakeTexture(width, height), ITextureFrames {
    int _frame;
    public int Fps => 10;
    public bool HasFrames => frames > 1;
    public bool NextFrame() => ++_frame < frames;
}

[TestClass]
public class MirageEngineTests {
    static readonly FakeSource Source = new() {
        Assets = {
            ["tex.rgba"] = new FakeTexture(4, 2),
            ["anim.rgba"] = new FakeFrames(2, 2, 3),
            ["mat.std"] = new MaterialStd2Prop { Textures = { ["Main"] = "tex.rgba" }, AlphaTest = true, AlphaCutoff = 0.5f, DiffuseColor = Color.Red, Alpha = 1f, Glossiness = 0.25f },
        }
    };

    [TestInitialize]
    public void Init() => MirageEngine.Reset();

    [TestMethod]
    public void Activate_ProvidesEveryGfxSlot() {
        Assert.AreSame(MirageEngine.This, EngineX.Current);
        Assert.AreEqual("MR", EngineX.Current.Id);
        Assert.IsInstanceOfType<MirageGfxApi>(EngineX.Gfx[GfX.XApi]);
        Assert.IsInstanceOfType<MirageGfxSprite2D>(EngineX.Gfx[GfX.XSprite2D]);
        Assert.IsInstanceOfType<MirageGfxSprite3D>(EngineX.Gfx[GfX.XSprite3D]);
        Assert.IsInstanceOfType<MirageGfxModel>(EngineX.Gfx[GfX.XModel]);
        Assert.IsInstanceOfType<MirageGfxLight>(EngineX.Gfx[GfX.XLight]);
        Assert.IsInstanceOfType<MirageGfxTerrain>(EngineX.Gfx[GfX.XTerrain]);
        Assert.IsInstanceOfType<MirageSfx>(EngineX.Sfx[0]);
    }

    [TestMethod]
    public void GfxApi_BuildsTheSceneGraph() {
        var api = (MirageGfxApi)EngineX.Gfx[GfX.XApi];
        var world = api.CreateObject("World");
        var player = api.CreateObject("Player", "player", world);
        api.Transform(player, new Vector3(1, 2, 3), Quaternion.Identity, new Vector3(2, 2, 2));
        api.AddMeshRenderer(player, "Cube", MirageEngine.Tree.Resources.Get<Material>("Std"), true, true);
        api.AddMeshCollider(player, "Cube", false, true);
        var prop = api.CreateObject("Prop", null, world);
        api.AddMeshCollider(prop, "Cube", true, false);
        api.SetVisible(prop, false);
        api.SetLayerRecursively(world, 4);
        Assert.IsTrue(SceneText.AreEqual(@"
Root
  World layer=4
    Player tag=player layer=4 pos=(1,2,3) scale=(2,2,2) static
      @MeshRenderer mesh=Cube material=Std
      @MeshCollider mesh=Cube
    Prop layer=4 hidden
      @BoxCollider
      @Rigidbody kinematic
", MirageEngine.Tree.ToText()), MirageEngine.Tree.ToText());
        api.Destroy(prop);
        Assert.IsNull(MirageEngine.Tree.Find("Prop"));
    }

    [TestMethod]
    public void GfxApi_MatrixTransformBecomesAQuaternion() {
        var api = (MirageGfxApi)EngineX.Gfx[GfX.XApi];
        var n = api.CreateObject("N");
        api.Transform(n, Vector3.Zero, Matrix4x4.CreateRotationY(MathF.PI / 2), Vector3.One);
        var p = Vector3.Transform(Vector3.UnitX, n.Transform.Rotation);
        Assert.IsTrue(Vector3.Distance(new Vector3(0, 0, -1), p) < 1e-4f, $"{p}");
    }

    [TestMethod]
    public void TextureRenderer_UploadsAndDescribesTheTexture() {
        var r = new TextureRenderer(EngineX.Gfx, Source, Source.Assets["tex.rgba"], 0..);
        r.Start();
        Assert.AreEqual("Root\n  Texture\n    @MeshRenderer mesh=Quad material=plane:texture#" + r.Tex.Name.Split('#')[1] + "\n", MirageEngine.Tree.ToText());
        Assert.AreEqual(4, r.Tex.Width);
        Assert.AreEqual(2, r.Tex.Height);
        Assert.AreEqual("RGBA32:Byte", r.Tex.Format);
        Assert.AreEqual(32, r.Tex.Bytes);
        Assert.AreEqual(1, r.Tex.Spans);
        Assert.AreEqual("plane", r.Shader.Name);
        StringAssert.Contains(MirageEngine.Tree.Resources.ToString(), "size=4x2 format=RGBA32:Byte mips=3 bytes=32 spans=1");
        r.Dispose();
        Assert.AreEqual("Root\n", MirageEngine.Tree.ToText());
    }

    [TestMethod]
    public void TextureRenderer_AdvancesAnimatedTextures() {
        var r = new TextureRenderer(EngineX.Gfx, Source, Source.Assets["anim.rgba"], 0..);
        r.Start();
        r.Update(0.1f); r.Update(0.1f); r.Update(0.1f);
        Assert.AreEqual(2, r.Frames, "three frames: two advances, then the end");
        CollectionAssert.AreEqual(new[] { "log texture frame 1", "log texture frame 2" }, MirageEngine.Tree.JournalOf("log texture").ToArray());
    }

    [TestMethod]
    public void TextureManager_CachesBySourceAndPath() {
        var model = (MirageGfxModel)EngineX.Gfx[GfX.XModel];
        var before = Source.Loads;
        var (a, _) = model.CreateTexture(Source, "tex.rgba").Result;
        var (b, _) = model.CreateTexture(Source, "tex.rgba").Result;
        Assert.AreSame(a, b);
        Assert.AreEqual(before + 1, Source.Loads);
    }

    [TestMethod]
    public void MaterialRenderer_PullsTexturesThroughTheTextureManager() {
        var r = new MaterialRenderer(EngineX.Gfx, Source, Source.Assets["mat.std"]);
        r.Start();
        var m = r.Material;
        Assert.AreEqual(4, m.Textures["Main"].Width);
        Assert.AreEqual(true, m.Props["alphaTest"]);
        Assert.AreEqual(0.5f, m.Props["alphaCutoff"]);
        Assert.AreEqual(Color.Red, m.Props["diffuse"]);
        var line = SceneText.Write(m);
        StringAssert.StartsWith(line, "Material std2#");
        StringAssert.Contains(line, " tex:Main=texture#");
        StringAssert.Contains(line, " alphaCutoff=0.5 alphaTest diffuse=#FF0000");
    }

    [TestMethod]
    public void Light_And_Terrain_BecomeNodes() {
        var light = (MirageGfxLight)EngineX.Gfx[GfX.XLight];
        light.Create("Sun", new Vector3(0, 10, 0), 5f, Color.FromArgb(255, 200, 100), false);
        light.CreateProbe("Probe", null);
        var terrain = (MirageGfxTerrain)EngineX.Gfx[GfX.XTerrain];
        var data = terrain.CreateData(7, new float[3, 4], 100f, 2f, [new GfxTerrainLayer<Texture>(), new GfxTerrainLayer<Texture>()], new float[3, 4, 2]);
        terrain.Create("Ground", Vector3.Zero, data);
        Assert.IsTrue(SceneText.AreEqual(@"
Root
  Sun pos=(0,10,0)
    @Light radius=5 color=#FFC864
  Probe
    @LightProbe
  Ground
    @Terrain data=terrain_7
", MirageEngine.Tree.ToText()), MirageEngine.Tree.ToText());
        StringAssert.Contains(MirageEngine.Tree.Resources.ToString(), "TerrainData terrain_7 offset=7 size=3x4 range=100 sample=2 layers=2 alpha=2");
    }

    [TestMethod]
    public void ObjectRenderer_UsesTheRegisteredBuilder() {
        MirageX.BuildersByType[typeof(string)] = (source, path, isStatic, mm) => {
            var n = new Node($"model:{path}");
            n.AddChild("Part").AddComponent(new MeshRenderer { Mesh = MirageEngine.Tree.Resources.Get<Mesh>("part") });
            return Task.FromResult(n);
        };
        try {
            Source.Assets["m.model"] = "m.model";
            var r = new ObjectRenderer(EngineX.Gfx, Source, "m.model");
            r.Start();
            Assert.IsTrue(SceneText.AreEqual(@"
Root
  Object
    model:m.model
      Part
        @MeshRenderer mesh=part
  _Prefabs hidden
    model:m.model
      Part
        @MeshRenderer mesh=part
", MirageEngine.Tree.ToText()), MirageEngine.Tree.ToText());
            Assert.AreNotSame(MirageEngine.Tree.Find("_Prefabs/model:m.model"), r.Obj, "the scene gets an instance, the prefab stays");
        }
        finally { MirageX.BuildersByType.Remove(typeof(string)); Source.Assets.Remove("m.model"); }
    }

    [TestMethod]
    public void Reset_GivesAFreshTreeAndFreshManagers() {
        var model = (MirageGfxModel)EngineX.Gfx[GfX.XModel];
        model.CreateTexture(Source, "tex.rgba").Wait();
        MirageEngine.Tree.Root.AddChild("Junk");
        MirageEngine.Reset();
        Assert.AreEqual("Root\n", MirageEngine.Tree.ToText());
        Assert.AreNotSame(model, EngineX.Gfx[GfX.XModel]);
        var before = Source.Loads;
        ((MirageGfxModel)EngineX.Gfx[GfX.XModel]).CreateTexture(Source, "tex.rgba").Wait();
        Assert.AreEqual(before + 1, Source.Loads, "nothing cached across a reset");
    }

    [TestMethod]
    public void Log_GoesToTheJournal() {
        Log.Info("hello from the engine");
        CollectionAssert.Contains(MirageEngine.Tree.Journal, "log hello from the engine");
    }

    [TestMethod]
    public void ClientHost_RunsTheLoop() {
        var n = 0;
        MirageEngine.Tree.Root.AddComponent(new Script { OnProcess = _ => n++ });
        using var host = new MirageClientHost(frames: 5);
        host.Run();
        Assert.AreEqual(5, n);
        Assert.AreEqual(5, MirageEngine.Tree.Frame);
    }
}
