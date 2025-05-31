using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace GameX.Formats.Unknown;

#region IUnknownBone

public interface IUnknownBone {
    string Name { get; }
    Matrix4x4 WorldToBone { get; } // 4x3 matrix
    Matrix4x4 BoneToWorld { get; } // 4x3 matrix of world translations/rotations of the bones.
}

#endregion

#region IUnknownFileModel

public interface IUnknownFileModel : IUnknownFileObject {
    IEnumerable<IUnknownModel> Models { get; }
    IEnumerable<UnknownMesh> Meshes { get; }
    IEnumerable<IUnknownMaterial> Materials { get; }
    IEnumerable<IUnknownProxy> Proxies { get; }
    IUnknownSkin SkinningInfo { get; }
    IEnumerable<string> RootNodes { get; }
}

#endregion

#region IUnknownFileObject

public interface IUnknownFileObject {
    public struct Source {
        public string Author;
        public string SourceFile;
    }

    string Name { get; }
    string Path { get; }
    IEnumerable<Source> Sources { get; }
}

#endregion

#region IUnknownMaterial

public interface IUnknownMaterial {
    string Name { get; }
    Vector3? Diffuse { get; } // Color:RGB
    Vector3? Specular { get; } // Color:RGB
    Vector3? Emissive { get; } // Color:RGB
    float Shininess { get; }
    float Opacity { get; }
    IEnumerable<IUnknownTexture> Textures { get; }
}

#endregion

#region IUnknownModel

public interface IUnknownModel {
    string Path { get; }
}

#endregion

#region IUnknownProxy

public interface IUnknownProxy {
    public struct Proxy {
        public Vector3[] Vertexs;
        public int[] Indexs;
    }

    Proxy[] PhysicalProxys { get; }
}

#endregion

#region IUnknownSkin

public interface IUnknownSkin {
    public struct BoneMap {
        public int[] BoneIndex;
        public int[] Weight; // Byte / 256?
    }

    public struct IntVertex {
        public Vector3 Obsolete0;
        public Vector3 Position;
        public Vector3 Obsolete2;
        public ushort[] BoneIDs; // 4 bone IDs
        public float[] Weights; // Should be 4 of these
        public object Color;
    }

    bool HasSkinningInfo { get; }
    ICollection<IUnknownBone> CompiledBones { get; }
    IntVertex[] IntVertexs { get; }
    BoneMap[] BoneMaps { get; }
    ushort[] Ext2IntMaps { get; }
}

#endregion

#region IUnknownTexture

public interface IUnknownTexture {
    [Flags]
    public enum Map {
        Diffuse = 1 << 0,
        Bumpmap = 1 << 1,
        Specular = 1 << 2,
        Environment = 1 << 3,
        Decal = 1 << 4,
        SubSurface = 1 << 5,
        Opacity = 1 << 6,
        Detail = 1 << 7,
        Heightmap = 1 << 8,
        BlendDetail = 1 << 9,
        Custom = 1 << 10,
    }

    string Path { get; }
    Map Maps { get; }
}

#endregion

#region IUnknownUnknown

//public enum ChunkType
//{
//    Node = 1,
//    Mesh,
//    Helper,
//}

//IGenericNode Root { get; }
//IEnumerable<IGenericNode> Nodes { get; }

//public interface IGenericNode
//{
//    string Name { get; }
//    ChunkType Type { get; }
//    IGenericNode Parent { get; }
//    IChunk Object { get; }
//    public IEnumerable<IGenericNode> Children { get; set; }
//}

//public interface IChunk
//{
//    ChunkType Type { get; }
//}

//public interface IChunkMesh : IChunk
//{
//    int Id { get; set; }
//    int MeshSubsets { get; set; }
//    int VerticesData { get; set; }
//    int VertsUVsData { get; set; }
//}

#endregion

#region UnknownFileWriter

public abstract class UnknownFileWriter {
    public static readonly Dictionary<string, Func<IUnknownFileModel, UnknownFileWriter>> Factories = new Dictionary<string, Func<IUnknownFileModel, UnknownFileWriter>>(StringComparer.OrdinalIgnoreCase);

    // ARGS
    public DirectoryInfo DataDir = null;
    public const bool NoConflicts = false;
    public const bool TiffTextures = false;
    public const bool SkipShieldNodes = false;
    public const bool SkipStreamNodes = false;
    public const bool GroupMeshes = true;
    public const bool Smooth = true;

    public IUnknownFileModel File { get; internal set; }

    public UnknownFileWriter(IUnknownFileModel file) => File = file;

    public abstract void Write(string outputDir = null, bool preservePath = true);

    protected FileInfo GetFileInfo(string extension, string outputDir = null, bool preservePath = true) {
        var fileName = $"temp.{extension}";
        // Empty output directory means place alongside original models If you want relative path, use "."
        if (string.IsNullOrWhiteSpace(outputDir)) fileName = Path.Combine(new FileInfo(File.Path).DirectoryName, $"{Path.GetFileNameWithoutExtension(File.Path)}{(NoConflicts ? "_out" : string.Empty)}.{extension}");
        else {
            // If we have an output directory
            var preserveDir = preservePath ? Path.GetDirectoryName(File.Path) : string.Empty;
            // Remove drive letter if necessary
            if (!string.IsNullOrWhiteSpace(preserveDir) && !string.IsNullOrWhiteSpace(Path.GetPathRoot(preserveDir))) preserveDir = preserveDir.Replace(Path.GetPathRoot(preserveDir), string.Empty);
            fileName = Path.Combine(outputDir, preserveDir, Path.ChangeExtension(Path.GetFileNameWithoutExtension(File.Path), extension));
        }
        return new FileInfo(fileName);
    }

    public static UnknownFileWriter Factory(string name, IUnknownFileModel model) => Factories.TryGetValue(name, out var factory)
        ? factory(model)
        : throw new ArgumentOutOfRangeException(nameof(name), name);
}

#endregion

#region UnknownMesh

public class UnknownMesh {
    [Flags]
    public enum Effect {
        ScaleOffset = 0x1
    }

    public struct Subset {
        public Range Vertexs;
        public Range Indexs;
        public int MatId;
        //public float Radius;
        //public Vector3 Center;
    }

    public ref struct SubsetMesh {
        public Span<Vector3> Vertexs;
        public Span<int> Indexs;
        public Span<Vector3> Normals;
        public Span<Vector2> UVs;
    }

    public string Name;
    public Vector3 MinBound;
    public Vector3 MaxBound;
    public Subset[] Subsets;
    public Vector3[] Vertexs;
    public Vector2[] UVs;
    public Vector3[] Normals;
    public int[] Indexs;
    public Effect Effects;
    public (Vector3 scale, Vector3 offset) ScaleOffset3;
    public (Vector4 scale, Vector4 offset) ScaleOffset4;

    public SubsetMesh this[Subset i] => new() {
        Vertexs = Vertexs.AsSpan(i.Vertexs),
        UVs = UVs.AsSpan(i.Vertexs),
        Normals = Normals != null ? Normals.AsSpan(i.Vertexs) : null,
        Indexs = Indexs.AsSpan(i.Indexs),
    };

    public Vector3 GetTransform(Vector3 vertex) => vertex;
}

#endregion