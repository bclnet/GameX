using GameX.ID.Engines;
using GameX.ID.Formats;
using OpenX;
using OpenX.Gfx;
using System.Threading.Tasks;
using UnityEngine;
using static GameX.ID.Engines.BspObjectBuilder;
using Object = UnityEngine.GameObject;

namespace GameX.Engines.Unity;

/// <summary>
/// A BSP becomes one mesh. Faces are split by texture. Quake is Z-up; the builder swaps Y and Z.
/// </summary>
public static class UnityBspObjectBuilder {
    public static async Task<Object> BuildObject(ISource source, object path, bool isStatic, MaterialManager<Material, Texture2D> materialManager) {
        var bsp = (Binary_BspX)path;
        var level = bsp.Level;
        var name = "bsp";
        var textureManager = materialManager.TextureManager;
        foreach (var texturePath in GetTexturePaths(bsp)) textureManager.Preload(source, texturePath);
        var data = ToGeometry(bsp);
        var gobj = new Object(name) { isStatic = isStatic };
        var mesh = new Mesh { name = name };
        mesh.vertices = ToV3(data.Vertices);
        if (data.Normals.Length > 0) mesh.normals = ToV3(data.Normals);
        if (data.UVs.Length > 0) mesh.uv = ToV2(data.UVs);
        mesh.subMeshCount = data.Submeshes.Length;
        var materials = new Material[data.Submeshes.Length];
        for (var i = 0; i < data.Submeshes.Length; i++) {
            var sub = data.Submeshes[i];
            mesh.SetTriangles(sub.Triangles, i);
            BspTexture tex = null;
            if (level != null && level.Textures != null && sub.TexId >= 0 && sub.TexId < level.Textures.Length) tex = level.Textures[sub.TexId];
            (materials[i], _) = await materialManager.Create(source, ToMaterialProp(tex));
        }
        if (data.Vertices.Length > 0) mesh.RecalculateBounds();
        gobj.AddComponent<MeshFilter>().mesh = mesh;
        gobj.AddComponent<MeshRenderer>().materials = materials;
        return gobj;
    }

    static Vector3[] ToV3(System.Numerics.Vector3[] s) {
        var d = new Vector3[s.Length];
        for (var i = 0; i < s.Length; i++) d[i] = new Vector3(s[i].X, s[i].Y, s[i].Z);
        return d;
    }

    static Vector2[] ToV2(System.Numerics.Vector2[] s) {
        var d = new Vector2[s.Length];
        for (var i = 0; i < s.Length; i++) d[i] = new Vector2(s[i].X, s[i].Y);
        return d;
    }
}
