using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Mirage;

/// <summary>
/// Transform - position, rotation and scale of a node, relative to its parent.
/// </summary>
public sealed class Transform {
    public Vector3 Position = Vector3.Zero;
    public Quaternion Rotation = Quaternion.Identity;
    public Vector3 Scale = Vector3.One;

    /// <summary>
    /// True when nothing has been set - the text form omits identity transforms.
    /// </summary>
    public bool IsIdentity => Position == Vector3.Zero && Rotation == Quaternion.Identity && Scale == Vector3.One;

    /// <summary>
    /// Sets the rotation from euler angles in degrees (pitch, yaw, roll).
    /// </summary>
    public Vector3 EulerAngles {
        set => Rotation = Quaternion.CreateFromYawPitchRoll(MathX.ToRadians(value.Y), MathX.ToRadians(value.X), MathX.ToRadians(value.Z));
    }

    /// <summary>
    /// The local matrix (scale, then rotation, then translation).
    /// </summary>
    public Matrix4x4 LocalMatrix => Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateTranslation(Position);

    public Transform Clone() => new() { Position = Position, Rotation = Rotation, Scale = Scale };
    public void Reset() { Position = Vector3.Zero; Rotation = Quaternion.Identity; Scale = Vector3.One; }
}

/// <summary>
/// Node - a scene object. Unity's GameObject, Godot's Node: a name, a transform,
/// a list of components, and children.
/// </summary>
public class Node {
    static int _nextId;

    /// <summary>
    /// A per-process serial, only for telling two unnamed nodes apart in logs.
    /// </summary>
    public readonly int Id = ++_nextId;
    public string Name;
    public string Tag;
    public int Layer;
    public bool Visible = true;
    public bool IsStatic;
    public readonly Transform Transform = new();
    readonly List<Node> _children = [];
    readonly List<Component> _components = [];
    SceneTree _tree;

    public Node(string name = null) => Name = name ?? "Node";

    public Node Parent { get; private set; }
    public IReadOnlyList<Node> Children => _children;
    public IReadOnlyList<Component> Components => _components;

    /// <summary>
    /// The tree this node is in, or null when detached. Set on the root by the tree; inherited by descendants.
    /// </summary>
    public SceneTree Tree {
        get => Parent != null ? Parent.Tree : _tree;
        internal set => _tree = value;
    }
    public bool InTree => Tree != null;
    public bool IsRoot => Parent == null && _tree != null;

    /// <summary>
    /// "Root/Parent/Name" - the path Find() understands.
    /// </summary>
    public string Path => Parent != null ? $"{Parent.Path}/{Name}" : Name;

    /// <summary>
    /// World matrix - this node's local matrix composed with its ancestors'.
    /// </summary>
    public Matrix4x4 WorldMatrix => Parent != null ? Transform.LocalMatrix * Parent.WorldMatrix : Transform.LocalMatrix;
    public Vector3 WorldPosition => Vector3.Transform(Vector3.Zero, WorldMatrix);

    #region Hierarchy

    /// <summary>
    /// Adds a child. A child that already has a parent is moved.
    /// </summary>
    public Node AddChild(Node child) {
        if (child == null) throw new ArgumentNullException(nameof(child));
        if (child == this || child.IsAncestorOf(this)) throw new InvalidOperationException($"{child.Name} cannot be its own descendant");
        child.Parent?._children.Remove(child);
        child.Parent = this;
        child._tree = null;
        _children.Add(child);
        Tree?.Record($"add {child.Path}");
        return child;
    }

    /// <summary>
    /// Creates and adds a named child.
    /// </summary>
    public Node AddChild(string name) => AddChild(new Node(name));

    /// <summary>
    /// Removes a child without destroying it; it can be added elsewhere.
    /// </summary>
    public bool RemoveChild(Node child) {
        if (child == null || child.Parent != this) return false;
        Tree?.Record($"remove {child.Path}");
        _children.Remove(child);
        child.Parent = null;
        return true;
    }

    /// <summary>
    /// Moves this node under a new parent. With keepWorld the world transform is preserved.
    /// </summary>
    public void Reparent(Node newParent, bool keepWorld = false) {
        if (newParent == null) throw new ArgumentNullException(nameof(newParent));
        var world = keepWorld ? WorldMatrix : default;
        newParent.AddChild(this);
        if (keepWorld && Matrix4x4.Invert(newParent.WorldMatrix, out var inv) && Matrix4x4.Decompose(world * inv, out var s, out var r, out var t)) {
            Transform.Scale = s; Transform.Rotation = r; Transform.Position = t;
        }
    }

    /// <summary>
    /// Detaches this node from its parent and marks it destroyed.
    /// </summary>
    public void Destroy() {
        Tree?.Record($"destroy {Path}");
        Parent?._children.Remove(this);
        Parent = null;
        _tree = null;
        Destroyed = true;
    }
    public bool Destroyed { get; private set; }

    public bool IsAncestorOf(Node node) {
        for (var p = node?.Parent; p != null; p = p.Parent) if (p == this) return true;
        return false;
    }

    /// <summary>
    /// This node and every descendant, depth first, in child order.
    /// </summary>
    public IEnumerable<Node> DescendantsAndSelf() {
        yield return this;
        foreach (var c in _children.ToArray())
            foreach (var d in c.DescendantsAndSelf()) yield return d;
    }
    public IEnumerable<Node> Descendants() => DescendantsAndSelf().Skip(1);

    /// <summary>
    /// Finds a node by relative path ("A/B/C"), or by name anywhere below when the path has no slash and no direct child matches.
    /// </summary>
    public Node Find(string path) {
        if (string.IsNullOrEmpty(path)) return this;
        var parts = path.Split('/');
        var node = this;
        foreach (var part in parts) {
            var next = node._children.FirstOrDefault(c => c.Name == part);
            if (next == null) return parts.Length == 1 ? Descendants().FirstOrDefault(d => d.Name == part) : null;
            node = next;
        }
        return node;
    }

    public void SetLayerRecursively(int layer) { foreach (var n in DescendantsAndSelf()) n.Layer = layer; }
    public void SetVisibleRecursively(bool visible) { foreach (var n in DescendantsAndSelf()) n.Visible = visible; }

    #endregion

    #region Components

    public T AddComponent<T>() where T : Component, new() => AddComponent(new T());
    public T AddComponent<T>(T component) where T : Component {
        if (component == null) throw new ArgumentNullException(nameof(component));
        if (component.Node != null) throw new InvalidOperationException($"{component.Kind} is already on {component.Node.Path}");
        component.Node = this;
        _components.Add(component);
        Tree?.Record($"component {Path}:{component.Kind}");
        return component;
    }
    public bool RemoveComponent(Component component) {
        if (component == null || component.Node != this) return false;
        _components.Remove(component);
        component.Node = null;
        return true;
    }
    public T GetComponent<T>() where T : Component => _components.OfType<T>().FirstOrDefault();
    public IEnumerable<T> GetComponents<T>() where T : Component => _components.OfType<T>();
    public T GetComponentInChildren<T>() where T : Component => DescendantsAndSelf().SelectMany(n => n._components.OfType<T>()).FirstOrDefault();
    public IEnumerable<T> GetComponentsInChildren<T>() where T : Component => DescendantsAndSelf().SelectMany(n => n._components.OfType<T>());
    public bool HasComponent<T>() where T : Component => _components.OfType<T>().Any();

    #endregion

    /// <summary>
    /// A deep copy: transform, components and children. Unity's Instantiate.
    /// </summary>
    public Node Clone() {
        var n = new Node(Name) { Tag = Tag, Layer = Layer, Visible = Visible, IsStatic = IsStatic };
        n.Transform.Position = Transform.Position; n.Transform.Rotation = Transform.Rotation; n.Transform.Scale = Transform.Scale;
        foreach (var c in _components) n.AddComponent(c.Clone());
        foreach (var c in _children) n.AddChild(c.Clone());
        return n;
    }

    /// <summary>
    /// The scene text for this node and its subtree.
    /// </summary>
    public override string ToString() => SceneText.Write(this);
}

/// <summary>
/// MathX
/// </summary>
public static class MathX {
    public static float ToRadians(float degrees) => degrees * (MathF.PI / 180f);
    public static float ToDegrees(float radians) => radians * (180f / MathF.PI);
}
