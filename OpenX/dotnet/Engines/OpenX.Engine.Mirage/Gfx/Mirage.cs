using Mirage;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace OpenX.Gfx.Mirage;

#region Extensions

/// <summary>
/// MirageX
/// </summary>
public static class MirageX {
    /// <summary>
    /// Object builders by asset type - a family engine registers how its model format becomes a Node tree.
    /// </summary>
    public static Dictionary<Type, Func<ISource, object, bool, MaterialManager<Material, Texture>, Task<Node>>> BuildersByType = [];

    /// <summary>
    /// Mirage uses System.Numerics directly, so these only exist to mirror the other engines' ToUnity/ToGodot.
    /// </summary>
    public static Vector3 ToMirage(this Vector3 source) => source;
    public static Quaternion ToMirage(this Quaternion source) => source;
    public static Quaternion ToMirageRotation(this Matrix4x4 source) => Quaternion.CreateFromRotationMatrix(source);

    /// <summary>
    /// Adds mesh colliders to every descendant with a mesh renderer but no collider, including the node itself.
    /// </summary>
    public static void AddMissingMeshCollidersRecursively(this Node source, bool isStatic = true) {
        foreach (var n in source.DescendantsAndSelf()) {
            var mr = n.GetComponent<MeshRenderer>();
            if (mr == null || n.HasComponent<Collider>()) continue;
            n.AddComponent(new MeshCollider { Mesh = mr.Mesh });
        }
    }

    /// <summary>
    /// The texture-format tag an ITexture hands the engine, as text. Formats arrive as (TextureFormat, TexturePixel) tuples or a raw value.
    /// </summary>
    public static string FormatName(object format) => format switch {
        null => null,
        ValueTuple<TextureFormat, TexturePixel> z => z.Item2 == TexturePixel.Unknown ? $"{z.Item1}" : $"{z.Item1}:{z.Item2}",
        _ => format.ToString(),
    };
}

#endregion
