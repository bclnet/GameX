using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Mirage;

/// <summary>
/// Resource - a named asset shared between nodes (a mesh, a texture, ...). Scene text refers to resources by name.
/// </summary>
public abstract class Resource {
    public string Name;
    public bool Deleted;

    /// <summary>
    /// The name used in text - the type name.
    /// </summary>
    public string Kind => GetType().Name;

    /// <summary>
    /// Writes the resource's own properties (not its name) for text output.
    /// </summary>
    public virtual void WriteProps(SceneText.Props w) { }
    public override string ToString() => SceneText.Write(this);
}

/// <summary>
/// Mesh
/// </summary>
public class Mesh : Resource {
    public Vector3[] Vertices;
    public Vector3[] Normals;
    public Vector2[] UVs;
    public int[] Triangles;
    public Vector3 BoundsMin, BoundsMax;

    public int VertexCount => Vertices?.Length ?? 0;
    public int TriangleCount => (Triangles?.Length ?? 0) / 3;

    public void RecalculateBounds() {
        if (Vertices == null || Vertices.Length == 0) { BoundsMin = BoundsMax = Vector3.Zero; return; }
        BoundsMin = BoundsMax = Vertices[0];
        foreach (var v in Vertices) { BoundsMin = Vector3.Min(BoundsMin, v); BoundsMax = Vector3.Max(BoundsMax, v); }
    }

    public void RecalculateNormals() {
        if (Vertices == null || Triangles == null) return;
        var normals = new Vector3[Vertices.Length];
        for (var i = 0; i + 2 < Triangles.Length; i += 3) {
            int a = Triangles[i], b = Triangles[i + 1], c = Triangles[i + 2];
            var n = Vector3.Cross(Vertices[b] - Vertices[a], Vertices[c] - Vertices[a]);
            normals[a] += n; normals[b] += n; normals[c] += n;
        }
        for (var i = 0; i < normals.Length; i++) normals[i] = normals[i] == Vector3.Zero ? Vector3.UnitY : Vector3.Normalize(normals[i]);
        Normals = normals;
    }

    public override void WriteProps(SceneText.Props w) {
        w.Prop("vertices", VertexCount);
        w.Prop("triangles", TriangleCount);
        w.Flag("normals", Normals != null);
        w.Flag("uvs", UVs != null);
        if (VertexCount > 0) { w.Prop("min", BoundsMin); w.Prop("max", BoundsMax); }
    }

    /// <summary>
    /// A unit triangle in the XY plane.
    /// </summary>
    public static Mesh Triangle(string name = "Triangle") {
        var m = new Mesh { Name = name, Vertices = [new(-1, -1, 0), new(1, -1, 0), new(0, 1, 0)], UVs = [new(0, 0), new(1, 0), new(0.5f, 1)], Triangles = [0, 1, 2] };
        m.RecalculateNormals(); m.RecalculateBounds();
        return m;
    }

    /// <summary>
    /// A 2x2 quad in the XY plane.
    /// </summary>
    public static Mesh Quad(string name = "Quad") {
        var m = new Mesh { Name = name, Vertices = [new(-1, -1, 0), new(-1, 1, 0), new(1, 1, 0), new(1, -1, 0)], UVs = [new(0, 1), new(0, 0), new(1, 0), new(1, 1)], Triangles = [0, 1, 2, 0, 2, 3] };
        m.RecalculateNormals(); m.RecalculateBounds();
        return m;
    }
}

/// <summary>
/// Texture - the shape of an uploaded texture; the pixels themselves are only counted.
/// </summary>
public class Texture : Resource {
    public int Width, Height, Depth = 1, MipMaps = 1;
    public string Format;
    public int Flags;
    public int Bytes;
    public int Spans;
    /// <summary>
    /// The decoded pixel data, kept only when the engine is asked to (see MirageEngine.KeepPixels).
    /// </summary>
    public byte[] Pixels;

    public override void WriteProps(SceneText.Props w) {
        w.Prop("size", $"{Width}x{Height}" + (Depth > 1 ? $"x{Depth}" : ""));
        w.Prop("format", Format);
        w.Prop("mips", MipMaps, 1);
        w.Prop("bytes", Bytes, 0);
        w.Prop("spans", Spans, 0);
        w.Prop("flags", Flags, 0);
    }
}

/// <summary>
/// Shader
/// </summary>
public class Shader : Resource {
    public List<string> Defines = [];
    public override void WriteProps(SceneText.Props w) { if (Defines.Count > 0) w.Prop("defines", string.Join("|", Defines)); }
}

/// <summary>
/// Material
/// </summary>
public class Material : Resource {
    public Shader Shader;
    public readonly Dictionary<string, Texture> Textures = [];
    /// <summary>
    /// Every other material property (blend modes, colours, shader params), by name.
    /// </summary>
    public readonly Dictionary<string, object> Props = [];

    public override void WriteProps(SceneText.Props w) {
        w.Prop("shader", Shader?.Name);
        foreach (var kv in Textures.OrderBy(x => x.Key, StringComparer.Ordinal)) w.Prop($"tex:{kv.Key}", kv.Value?.Name);
        foreach (var kv in Props.OrderBy(x => x.Key, StringComparer.Ordinal)) w.Prop(kv.Key, kv.Value);
    }
}

/// <summary>
/// Sprite
/// </summary>
public class Sprite : Resource {
    public int Width, Height;
    public Texture Texture;
    public override void WriteProps(SceneText.Props w) { w.Prop("size", $"{Width}x{Height}"); w.Prop("texture", Texture?.Name); }
}

/// <summary>
/// Audio
/// </summary>
public class Audio : Resource {
    public int Channels, SampleRate, Bits, Samples;
    public override void WriteProps(SceneText.Props w) { w.Prop("channels", Channels, 0); w.Prop("rate", SampleRate, 0); w.Prop("bits", Bits, 0); w.Prop("samples", Samples, 0); }
}

/// <summary>
/// TerrainData
/// </summary>
public class TerrainData : Resource {
    public int Offset;
    public int Rows, Cols;
    public float HeightRange, SampleDistance;
    public int Layers;
    public int AlphaLayers;
    public override void WriteProps(SceneText.Props w) {
        w.Prop("offset", Offset, 0);
        w.Prop("size", $"{Rows}x{Cols}");
        w.Prop("range", HeightRange);
        w.Prop("sample", SampleDistance);
        w.Prop("layers", Layers, 0);
        w.Prop("alpha", AlphaLayers, 0);
    }
}

/// <summary>
/// ResourceSet - resources by kind and name. Scene text resolves "mesh=Cube" here, creating an empty placeholder when nothing is registered.
/// </summary>
public class ResourceSet {
    readonly Dictionary<(Type, string), Resource> _items = [];

    public IEnumerable<Resource> All => _items.Values;

    public T Add<T>(T resource) where T : Resource {
        if (resource == null) throw new ArgumentNullException(nameof(resource));
        if (string.IsNullOrEmpty(resource.Name)) throw new ArgumentException("resource needs a name", nameof(resource));
        _items[(typeof(T), resource.Name)] = resource;
        return resource;
    }

    /// <summary>
    /// Gets a resource by name, creating an empty one when it does not exist yet. Null or empty names give null.
    /// </summary>
    public T Get<T>(string name) where T : Resource, new() {
        if (string.IsNullOrEmpty(name)) return null;
        if (_items.TryGetValue((typeof(T), name), out var r)) return (T)r;
        var n = new T { Name = name };
        _items[(typeof(T), name)] = n;
        return n;
    }

    public bool TryGet<T>(string name, out T resource) where T : Resource {
        var ok = _items.TryGetValue((typeof(T), name ?? ""), out var r);
        resource = ok ? (T)r : null;
        return ok;
    }

    public void Clear() => _items.Clear();

    /// <summary>
    /// One line per resource, sorted, e.g. "Mesh Cube vertices=8 triangles=12".
    /// </summary>
    public override string ToString() {
        var b = new StringBuilder();
        foreach (var r in All.OrderBy(x => x.Kind, StringComparer.Ordinal).ThenBy(x => x.Name, StringComparer.Ordinal)) b.AppendLine(SceneText.Write(r));
        return b.ToString();
    }
}
