using GameX.Arkane.Engines;
using GameX.Arkane.Formats.Danae;
using Godot;
using OpenX;
using OpenX.Gfx;
using System.Threading.Tasks;
using static GameX.Arkane.Engines.FtsObjectBuilder;

namespace GameX.Engines.Godot;

/// <summary>
/// An FTS map becomes a Node3D. One mesh child per texture, plus a child at the player and the scene origin.
/// Danae is Y-up, so axes are not swapped.
/// </summary>
public static class GodotFtsObjectBuilder {
    public static async Task<Node> BuildObject(ISource source, object path, bool isStatic, MaterialManager<Material, Texture2D> materialManager) {
        var level = ((Binary_Fts)path).Level;
        var name = "fts";
        var textureManager = materialManager.TextureManager;
        foreach (var texturePath in GetTexturePaths(level)) textureManager.Preload(source, texturePath);
        var data = ToGeometry(level);
        var gobj = new Node3D { Name = name };
        for (var i = 0; i < data.Submeshes.Length; i++) {
            var sub = data.Submeshes[i];
            var mesh = MakeMesh(data.Vertices, data.Normals, data.UVs, sub.Triangles);
            E_TEXTURE tex = null;
            if (level.Textures != null && sub.TexId >= 0 && sub.TexId < level.Textures.Length) tex = level.Textures[sub.TexId];
            var (material, _) = await materialManager.Create(source, ToMaterialProp(tex, sub.Type));
            var child = new MeshInstance3D { Name = $"tex{sub.TexId}", Mesh = mesh };
            if (mesh.GetSurfaceCount() > 0) child.SetSurfaceOverrideMaterial(0, material);
            gobj.AddChild(child);
        }
        foreach (var (groupName, pos) in ScenePositions(level)) {
            var child = new Node3D { Name = groupName };
            child.Position = new Vector3(pos.X, pos.Y, pos.Z);
            gobj.AddChild(child);
        }
        return gobj;
    }

    static ArrayMesh MakeMesh(System.Numerics.Vector3[] vertices, System.Numerics.Vector3[] normals, System.Numerics.Vector2[] uvs, int[] triangles) {
        var mesh = new ArrayMesh();
        if (vertices == null || vertices.Length == 0 || triangles == null || triangles.Length == 0) return mesh;
        var gVertices = new Vector3[vertices.Length];
        for (var i = 0; i < vertices.Length; i++) gVertices[i] = new Vector3(vertices[i].X, vertices[i].Y, vertices[i].Z);
        var arrays = new global::Godot.Collections.Array();
        arrays.Resize((int)Mesh.ArrayType.Max);
        arrays[(int)Mesh.ArrayType.Vertex] = gVertices;
        if (normals != null && normals.Length == vertices.Length) {
            var gNormals = new Vector3[normals.Length];
            for (var i = 0; i < normals.Length; i++) gNormals[i] = new Vector3(normals[i].X, normals[i].Y, normals[i].Z);
            arrays[(int)Mesh.ArrayType.Normal] = gNormals;
        }
        if (uvs != null && uvs.Length == vertices.Length) {
            var gUvs = new Vector2[uvs.Length];
            for (var i = 0; i < uvs.Length; i++) gUvs[i] = new Vector2(uvs[i].X, uvs[i].Y);
            arrays[(int)Mesh.ArrayType.TexUV] = gUvs;
        }
        arrays[(int)Mesh.ArrayType.Index] = triangles;
        mesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
        return mesh;
    }
}
