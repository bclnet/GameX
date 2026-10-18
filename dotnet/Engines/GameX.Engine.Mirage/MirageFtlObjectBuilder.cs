using GameX.Arkane.Engines;
using GameX.Arkane.Formats.Danae;
using Mirage;
using OpenX;
using OpenX.Gfx;
using OpenX.Gfx.Mirage;
using System.Numerics;
using System.Threading.Tasks;
using static GameX.Arkane.Engines.FtlObjectBuilder;

namespace GameX.Engines.Mirage;

/// <summary>
/// An FTL becomes a Node. One child mesh per texture, plus a child at each group origin.
/// Danae is Y-up, so positions stay as stored.
/// </summary>
public static class MirageFtlObjectBuilder {
    public static async Task<Node> BuildObject(ISource source, object path, bool isStatic, MaterialManager<Material, Texture> materialManager) {
        var obj = ((Binary_Ftl)path).Obj;
        var name = string.IsNullOrEmpty(obj.File) ? "ftl" : obj.File;
        var textureManager = materialManager.TextureManager;
        foreach (var texturePath in GetTexturePaths(obj)) textureManager.Preload(source, texturePath);
        var data = ToGeometry(obj);
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
            E_TEXTURE tex = null;
            if (obj.Textures != null && sub.TexId >= 0 && sub.TexId < obj.Textures.Length) tex = obj.Textures[sub.TexId];
            var (material, _) = await materialManager.Create(source, ToMaterialProp(tex));
            var child = gobj.AddChild($"tex{sub.TexId}");
            child.AddComponent(new MeshRenderer { Mesh = mesh, Material = material });
        }
        foreach (var (groupName, pos) in GroupPositions(obj)) {
            var child = gobj.AddChild(string.IsNullOrEmpty(groupName) ? "group" : groupName);
            child.Transform.Position = pos;
        }
        return gobj;
    }
}
