using GameX.Arkane.Formats.Danae;
using OpenX;
using OpenX.Gfx;
using System.Threading.Tasks;
using UnityEngine;
using static GameX.Arkane.Engines.FtlObjectBuilder;
using static GameX.Arkane.Engines.FtsObjectBuilder;
using Object = UnityEngine.GameObject;

namespace GameX.Engines.Unity;

/// <summary>
/// An FTL becomes one mesh. Faces are split by texture. Danae is Y-up, so axes are not swapped.
/// </summary>
public static class UnityFtlObjectBuilder {
    public static async Task<Object> BuildObject(ISource source, object path, bool isStatic, MaterialManager<Material, Texture2D> materialManager) {
        var obj = ((Binary_Ftl)path).Obj;
        var name = string.IsNullOrEmpty(obj.File) ? "ftl" : obj.File;
        var textureManager = materialManager.TextureManager;
        foreach (var texturePath in GetTexturePaths(obj)) textureManager.Preload(source, texturePath);
        var data = ToGeometry(obj);
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
            E_TEXTURE tex = null;
            if (obj.Textures != null && sub.TexId >= 0 && sub.TexId < obj.Textures.Length) tex = obj.Textures[sub.TexId];
            (materials[i], _) = await materialManager.Create(source, ToMaterialProp(tex));
        }
        if (data.Vertices.Length > 0) mesh.RecalculateBounds();
        gobj.AddComponent<MeshFilter>().mesh = mesh;
        gobj.AddComponent<MeshRenderer>().materials = materials;
        foreach (var (groupName, pos) in GroupPositions(obj)) {
            var child = new Object(string.IsNullOrEmpty(groupName) ? "group" : groupName);
            child.transform.SetParent(gobj.transform, false);
            child.transform.localPosition = new Vector3(pos.X, pos.Y, pos.Z);
        }
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

/// <summary>
/// An FTS map becomes one mesh. Polys are split by texture. Danae is Y-up, so axes are not swapped.
/// </summary>
public static class UnityFtsObjectBuilder {
    public static async Task<Object> BuildObject(ISource source, object path, bool isStatic, MaterialManager<Material, Texture2D> materialManager) {
        var level = ((Binary_Fts)path).Level;
        var name = "fts";
        var textureManager = materialManager.TextureManager;
        foreach (var texturePath in GetTexturePaths(level)) textureManager.Preload(source, texturePath);
        var data = ToGeometry(level);
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
            E_TEXTURE tex = null;
            if (level.Textures != null && sub.TexId >= 0 && sub.TexId < level.Textures.Length) tex = level.Textures[sub.TexId];
            (materials[i], _) = await materialManager.Create(source, ToMaterialProp(tex, sub.Type));
        }
        if (data.Vertices.Length > 0) mesh.RecalculateBounds();
        gobj.AddComponent<MeshFilter>().mesh = mesh;
        gobj.AddComponent<MeshRenderer>().materials = materials;
        foreach (var (groupName, pos) in ScenePositions(level)) {
            var child = new Object(groupName);
            child.transform.SetParent(gobj.transform, false);
            child.transform.localPosition = new Vector3(pos.X, pos.Y, pos.Z);
        }
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
