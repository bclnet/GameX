using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using System.Numerics;

namespace Mirage;

[TestClass]
public class SceneTreeTests {
    [TestMethod]
    public void Step_ReadiesOnceThenProcessesEveryFrame() {
        var tree = new SceneTree();
        int readies = 0, processes = 0;
        tree.Root.AddChild("A").AddComponent(new Script { OnReady = () => readies++, OnProcess = _ => processes++ });
        tree.Run(3);
        Assert.AreEqual(1, readies);
        Assert.AreEqual(3, processes);
        Assert.AreEqual(3, tree.Frame);
        Assert.AreEqual(3 * SceneTree.DefaultDelta, tree.Time, 1e-6f);
    }

    [TestMethod]
    public void Step_SkipsDisabledComponents() {
        var tree = new SceneTree();
        var processes = 0;
        var s = tree.Root.AddChild("A").AddComponent(new Script { OnProcess = _ => processes++ });
        tree.Step();
        s.Enabled = false;
        tree.Step();
        Assert.AreEqual(1, processes);
    }

    [TestMethod]
    public void Step_ProcessesDepthFirstInChildOrder() {
        var tree = new SceneTree();
        var order = "";
        var a = tree.Root.AddChild("A"); a.AddComponent(new Script { OnProcess = _ => order += "A" });
        a.AddChild("A1").AddComponent(new Script { OnProcess = _ => order += "1" });
        tree.Root.AddChild("B").AddComponent(new Script { OnProcess = _ => order += "B" });
        tree.Step();
        Assert.AreEqual("A1B", order);
    }

    [TestMethod]
    public void Run_IsDeterministic() {
        var tree = new SceneTree();
        var mover = tree.Root.AddChild("Mover");
        mover.AddComponent(new Script { OnProcess = d => mover.Transform.Position += new Vector3(d, 0, 0) });
        tree.Run(60, 1f / 60f);
        Assert.AreEqual(1f, mover.Transform.Position.X, 1e-4f);
        Assert.IsTrue(SceneText.AreEqual("Root\n  Mover pos=(1,0,0)\n    @Script\n", tree.ToText()));
    }

    [TestMethod]
    public void RunUntil_StopsWhenThePredicateHolds() {
        var tree = new SceneTree();
        var n = 0;
        tree.Root.AddComponent(new Script { OnProcess = _ => n++ });
        Assert.IsTrue(tree.RunUntil(() => n >= 5, maxFrames: 100));
        Assert.AreEqual(5, tree.Frame);
        Assert.IsFalse(tree.RunUntil(() => n >= 1000, maxFrames: 10));
    }

    [TestMethod]
    public void Journal_RecordsWhatHappened() {
        var tree = new SceneTree();
        var a = tree.Root.AddChild("A");
        a.AddComponent<Camera>();
        tree.Step();
        tree.Log("hello");
        a.Destroy();
        CollectionAssert.AreEqual(new[] { "add Root/A", "component Root/A:Camera", "frame 1", "ready Root/A:Camera", "log hello", "destroy Root/A" }, tree.Journal);
        CollectionAssert.AreEqual(new[] { "ready Root/A:Camera" }, tree.JournalOf("ready ").ToArray());
    }

    [TestMethod]
    public void Trace_EchoesTheJournalToOut() {
        var tree = new SceneTree { Trace = true, Out = new StringWriter() };
        tree.Root.AddChild("A");
        tree.Step();
        Assert.AreEqual("add Root/A\nframe 1\n", tree.Out.ToString().Replace("\r\n", "\n"));
    }

    [TestMethod]
    public void Load_ReplacesTheSceneFromText() {
        var tree = new SceneTree();
        tree.Root.AddChild("Old");
        tree.Load("World pos=(0,1,0)\n  A\n    @MeshRenderer mesh=Cube\n");
        Assert.AreEqual("World", tree.Root.Name);
        Assert.AreEqual(new Vector3(0, 1, 0), tree.Root.Transform.Position);
        Assert.IsNull(tree.Find("Old"));
        Assert.IsTrue(tree.Resources.TryGet<Mesh>("Cube", out _));
        Assert.AreEqual("World pos=(0,1,0)\n  A\n    @MeshRenderer mesh=Cube\n", tree.ToText());
        Assert.AreSame(tree, tree.Find("A").Tree, "loaded nodes are in the tree");
    }

    [TestMethod]
    public void Clear_ResetsEverything() {
        var tree = new SceneTree();
        tree.Root.AddChild("A");
        tree.Resources.Get<Mesh>("Cube");
        tree.Run(2);
        tree.Clear();
        Assert.AreEqual(0, tree.Root.Children.Count);
        Assert.AreEqual(0, tree.Frame);
        Assert.IsFalse(tree.Resources.All.Any());
        Assert.AreEqual(0, tree.Journal.Count);
    }

    [TestMethod]
    public void Print_WritesSceneTextToOut() {
        var tree = new SceneTree { Out = new StringWriter() };
        tree.Root.AddChild("A");
        tree.Print();
        Assert.AreEqual("Root\n  A\n", tree.Out.ToString());
    }
}
