using GameX.ID.Engines;
using GameX.ID.Formats;
using OpenX.Gfx;
using System.Collections.Generic;

namespace GameX.Engines.Wgpu;

/// <summary>
/// A BSP kept as the shared mesh. Quake is Z-up; the builder swaps Y and Z.
/// </summary>
public static class WgpuBspObjectBuilder {
    public static object BuildObject(object path, bool isStatic, object materialManager) {
        _ = materialManager;
        var bsp = (Binary_BspX)path;
        var level = bsp.Level;
        var mesh = BspObjectBuilder.ToGeometry(bsp);
        var paths = new List<string>();
        foreach (var texturePath in BspObjectBuilder.GetTexturePaths(bsp)) paths.Add(texturePath);
        var props = new MaterialStd2Prop[mesh.Submeshes.Length];
        for (var i = 0; i < mesh.Submeshes.Length; i++) {
            var sub = mesh.Submeshes[i];
            BspTexture tex = null;
            if (level != null && level.Textures != null && sub.TexId >= 0 && sub.TexId < level.Textures.Length) tex = level.Textures[sub.TexId];
            props[i] = BspObjectBuilder.ToMaterialProp(tex);
        }
        return new WgpuObject {
            Name = "bsp",
            IsStatic = isStatic,
            Mesh = mesh,
            Materials = new WgpuObjectMaterials { Paths = paths.ToArray(), Props = props },
        };
    }
}
