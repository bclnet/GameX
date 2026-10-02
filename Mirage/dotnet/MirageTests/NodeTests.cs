using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Numerics;

namespace Mirage;

[TestClass]
public class NodeTests {
    [TestMethod]
    public void AddChild_SetsParentAndPath() {
        var root = new Node("Root");
        var a = root.AddChild("A");
        var b = a.AddChild("B");
        Assert.AreSame(a, b.Parent);
        Assert.AreEqual("Root/A/B", b.Path);
        CollectionAssert.AreEqual(new[] { root, a, b }, root.DescendantsAndSelf().ToArray());
    }

    [TestMethod]
    public void AddChild_MovesANodeThatAlreadyHasAParent() {
        var root = new Node("Root");
        var a = root.AddChild("A");
        var b = root.AddChild("B");
        var c = a.AddChild("C");
        b.AddChild(c);
        Assert.AreEqual(0, a.Children.Count);
        Assert.AreSame(b, c.Parent);
    }

    [TestMethod]
    public void AddChild_RefusesACycle() {
        var root = new Node("Root");
        var a = root.AddChild("A");
        Assert.ThrowsExactly<InvalidOperationException>(() => a.AddChild(root));
        Assert.ThrowsExactly<InvalidOperationException>(() => a.AddChild(a));
    }

    [TestMethod]
    public void Find_ByPathAndByName() {
        var root = new Node("Root");
        var weapon = root.AddChild("Player").AddChild("Hand").AddChild("Weapon");
        Assert.AreSame(weapon, root.Find("Player/Hand/Weapon"));
        Assert.AreSame(weapon, root.Find("Weapon"), "a bare name searches the subtree");
        Assert.IsNull(root.Find("Player/Weapon"));
    }

    [TestMethod]
    public void Components_AddGetAndChildrenSearch() {
        var root = new Node("Root");
        var child = root.AddChild("Child");
        var mr = child.AddComponent<MeshRenderer>();
        Assert.AreSame(child, mr.Node);
        Assert.IsNull(root.GetComponent<MeshRenderer>());
        Assert.AreSame(mr, root.GetComponentInChildren<MeshRenderer>());
        Assert.ThrowsExactly<InvalidOperationException>(() => root.AddComponent(mr), "a component belongs to one node");
    }

    [TestMethod]
    public void Reparent_KeepWorld_PreservesWorldPosition() {
        var root = new Node("Root");
        var a = root.AddChild("A"); a.Transform.Position = new Vector3(10, 0, 0);
        var b = root.AddChild("B"); b.Transform.Position = new Vector3(0, 5, 0);
        var c = a.AddChild("C"); c.Transform.Position = new Vector3(1, 1, 1);
        var before = c.WorldPosition;
        c.Reparent(b, keepWorld: true);
        Assert.AreSame(b, c.Parent);
        Assert.IsTrue(Vector3.Distance(before, c.WorldPosition) < 1e-4f);
        Assert.IsTrue(Vector3.Distance(new Vector3(11, -4, 1), c.Transform.Position) < 1e-4f);
    }

    [TestMethod]
    public void Clone_IsDeepAndDetached() {
        var root = new Node("Root");
        var a = root.AddChild("A"); a.Transform.Position = new Vector3(1, 2, 3);
        a.AddComponent(new Light { Radius = 3 });
        a.AddChild("B").AddComponent<MeshRenderer>();
        var clone = a.Clone();
        Assert.IsNull(clone.Parent);
        Assert.AreEqual(SceneText.Write(a), SceneText.Write(clone));
        clone.Find("B").Name = "Changed";
        Assert.AreEqual("B", a.Children[0].Name, "the original is untouched");
        Assert.AreNotSame(a.GetComponent<Light>(), clone.GetComponent<Light>());
    }

    [TestMethod]
    public void Destroy_DetachesAndMarks() {
        var root = new Node("Root");
        var a = root.AddChild("A");
        a.Destroy();
        Assert.IsTrue(a.Destroyed);
        Assert.AreEqual(0, root.Children.Count);
        Assert.IsNull(a.Parent);
    }

    [TestMethod]
    public void Transform_EulerAnglesRoundTrip() {
        var t = new Transform { EulerAngles = new Vector3(0, 90, 0) };
        var p = Vector3.Transform(Vector3.UnitX, t.Rotation);
        Assert.IsTrue(Vector3.Distance(new Vector3(0, 0, -1), p) < 1e-4f, $"got {p}");
    }
}
