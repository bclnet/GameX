using GameX.ID.Engines;
using GameX.ID.Formats;
using Mirage;
using OpenX;
using OpenX.Gfx;
using OpenX.Gfx.Mirage;
using System.Threading.Tasks;
using static GameX.ID.Engines.BspObjectBuilder;

namespace GameX.Engines.Mirage;

/// <summary>
/// A BSP becomes a Node. One child mesh per texture. Quake is Z-up; the builder swaps Y and Z.
/// </summary>
public static class MirageBspObjectBuilder {
    public static async Task<Node> BuildObject(ISource source, object path, bool isStatic, MaterialManager<Material, Texture> materialManager) {
        var bsp = (Binary_BspX)path;
        var level = bsp.Level;
        var name = "bsp";
        var textureManager = materialManager.TextureManager;
        foreach (var texturePath in GetTexturePaths(bsp)) textureManager.Preload(source, texturePath);
        var data = ToGeometry(bsp);
        var gobj = new Node(name) { IsStatic = isStatic };
        for (var i = 0; i < data.Submeshes.Length; i++) {
            var sub = data.Submeshes[i];
            var mesh = MirageEngine.Tree.Resources.Add(new Mesh {
                Name = $"{name}#{sub.TexId}",
                Vertices = data.Vertices,
                Normals = data.Normals,
                UVs = data.UVs,
                Triangles = sub.Triangles,
            });
            mesh.RecalculateBounds();
            BspTexture tex = null;
            if (level != null && level.Textures != null && sub.TexId >= 0 && sub.TexId < level.Textures.Length) tex = level.Textures[sub.TexId];
            var (material, _) = await materialManager.Create(source, ToMaterialProp(tex));
            var child = gobj.AddChild($"tex{sub.TexId}");
            child.AddComponent(new MeshRenderer { Mesh = mesh, Material = material });
        }
        return gobj;
    }
}
