using GameX.Gamebryo.Formats;
using GameX.Gamebryo.Formats.Nif;
using System;
using System.Collections.Generic;
using System.Numerics;
using static GameX.Gamebryo.Engines.NifObjectBuilder;
using Stride.Engine;

namespace GameX.Engines.Stride;

/// <summary>
/// Copies NiTriShape positions and indices. No colliders, no skinning, no NIF reparse.
/// </summary>
public sealed class StrideNifShape {
    public string Name;
    public Vector3[] Positions;
    public int[] Indices;
}

public static class StrideNifObjectBuilder {
    public static Entity BuildObject(object path, bool isStatic, object materialManager) {
        _ = materialManager;
        var nif = (Binary_Nif)path;
        var name = string.IsNullOrEmpty(nif.Name) ? "nif" : nif.Name;
        var shapes = new List<StrideNifShape>();
        var materials = new List<object>();
        if (nif.Roots != null)
            foreach (var root in nif.Roots)
                if (root != null) Walk(root.Value, shapes, materials);
        var holder = new StrideObjectHolder {
            Name = name,
            IsStatic = isStatic,
            Mesh = shapes.ToArray(),
            Materials = materials.ToArray(),
            Groups = Array.Empty<(string Name, Vector3 Position)>(),
        };
        var entity = new Entity(holder.Name);
        entity.Add(holder);
        return entity;
    }

    static void Walk(NiObject node, List<StrideNifShape> shapes, List<object> materials) {
        if (node is NiNode n) {
            if (n.Children == null) return;
            foreach (var child in n.Children)
                if (child != null) Walk(child.Value, shapes, materials);
            return;
        }
        if (node is NiTriShape shape) Copy(shape, shapes, materials);
    }

    static void Copy(NiTriShape shape, List<StrideNifShape> shapes, List<object> materials) {
        if (shape.Data?.Value is not NiTriShapeData data) return;
        var positions = data.Vertices != null ? (Vector3[])data.Vertices.Clone() : [];
        int[] indices;
        if (data.Triangles == null || data.Triangles.Length == 0) indices = [];
        else {
            indices = new int[data.Triangles.Length * 3];
            for (var i = 0; i < data.Triangles.Length; i++) {
                var tri = data.Triangles[i];
                var b = i * 3;
                indices[b] = tri.v1;
                indices[b + 1] = tri.v2;
                indices[b + 2] = tri.v3;
            }
        }
        shapes.Add(new StrideNifShape { Name = shape.Name, Positions = positions, Indices = indices });
        materials.Add(shape.Properties != null ? ToMaterialProp(shape) : null);
    }
}
