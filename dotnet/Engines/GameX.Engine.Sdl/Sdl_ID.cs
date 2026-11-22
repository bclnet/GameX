using GameX.ID.Formats;
using static GameX.ID.Engines.BspObjectBuilder;

namespace GameX.Engines.Sdl;

/// <summary>
/// A BSP kept as the shared mesh. Quake is Z-up; the builder swaps Y and Z.
/// </summary>
public static class SdlBspObjectBuilder {
    public static object BuildObject(object path, bool isStatic, object materialManager) {
        var bsp = (Binary_BspX)path;
        var level = bsp.Level;
        foreach (var texturePath in GetTexturePaths(bsp))
            if (materialManager != null) _ = texturePath;
        var mesh = ToGeometry(bsp);
        var materials = new object[mesh.Submeshes.Length];
        for (var i = 0; i < mesh.Submeshes.Length; i++) {
            var sub = mesh.Submeshes[i];
            BspTexture tex = null;
            if (level != null && level.Textures != null && sub.TexId >= 0 && sub.TexId < level.Textures.Length) tex = level.Textures[sub.TexId];
            materials[i] = ToMaterialProp(tex);
        }
        return new SdlObjectHolder {
            Name = "bsp",
            IsStatic = isStatic,
            Mesh = mesh,
            Materials = materials,
        };
    }
}
