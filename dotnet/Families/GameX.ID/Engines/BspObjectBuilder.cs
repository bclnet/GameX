using GameX.ID.Formats;
using OpenX.Gfx;
using System.Collections.Generic;
using System.Numerics;

namespace GameX.ID.Engines;

/// <summary>
/// Shared mesh for a Quake BSP. Quake is Z-up: a point is (x, z, y) * WorldScale, a direction is (x, z, y).
/// </summary>
public static class BspObjectBuilder {
    public class BspSubmesh {
        public int TexId;
        public int[] Triangles;
    }

    public class BspMesh {
        public Vector3[] Vertices = [];
        public Vector3[] Normals = [];
        public Vector2[] UVs = [];
        public BspSubmesh[] Submeshes = [];
    }

    public const float WorldScale = 1f / 32f;

    public static IEnumerable<string> GetTexturePaths(Binary_BspX bsp) {
        var textures = bsp.Level?.Textures;
        if (textures == null) yield break;
        foreach (var t in textures)
            if (t != null && !string.IsNullOrEmpty(t.Name)) yield return t.Name;
    }

    public static MaterialStd2Prop ToMaterialProp(BspTexture tex) {
        var mp = new MaterialStd2Prop();
        if (tex == null) return mp;
        if (!string.IsNullOrEmpty(tex.Name)) mp.Textures["Main"] = tex.Name;
        return mp;
    }

    public static BspMesh ToGeometry(Binary_BspX bsp) {
        var level = bsp.Level;
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();
        var order = new List<int>();
        var tris = new Dictionary<int, List<int>>();
        if (level == null) return new BspMesh { Vertices = [], Normals = [], UVs = [], Submeshes = [] };
        var faces = level.Faces;
        var edges = level.Edges;
        var surf = level.SurfEdges;
        var verts = level.Vertices;
        var texinfo = level.Texinfo;
        var planes = level.Planes;
        var textures = level.Textures;
        int first, num;
        if (level.Models != null && level.Models.Length > 0) {
            first = level.Models[0].FirstFace;
            num = level.Models[0].NumFaces;
        } else {
            first = 0;
            num = faces.Length;
        }
        for (var f = 0; f < num; f++) {
            var face = faces[first + f];
            BspTexinfo ti = null;
            var texId = -1;
            if (texinfo != null && face.Texinfo >= 0 && face.Texinfo < texinfo.Length) {
                ti = texinfo[face.Texinfo];
                texId = ti.Miptex;
            }
            BspTexture tex = null;
            if (textures != null && texId >= 0 && texId < textures.Length) tex = textures[texId];
            if (tex != null && tex.Name != null) {
                var name = tex.Name.ToLowerInvariant();
                if (name == "skip" || name == "hint" || name == "origin" || name == "trigger" || name == "clip") continue;
            }
            var width = tex != null && tex.Width != 0 ? tex.Width : 256;
            var height = tex != null && tex.Height != 0 ? tex.Height : 256;
            var nrm = planes[face.PlaneNum].Normal;
            if (face.Side != 0) nrm = -nrm;
            var normal = new Vector3(nrm.X, nrm.Z, nrm.Y);
            var corners = new Vector3[face.NumEdges];
            for (var e = 0; e < face.NumEdges; e++) {
                var se = surf[face.FirstEdge + e];
                var vi = se >= 0 ? edges[se].A : edges[-se].B;
                corners[e] = verts[vi];
            }
            if (corners.Length < 3) continue;
            if (!tris.TryGetValue(texId, out var list)) { list = []; tris[texId] = list; order.Add(texId); }
            var baseI = vertices.Count;
            for (var c = 0; c < corners.Length; c++) {
                var q = corners[c];
                vertices.Add(new Vector3(q.X, q.Z, q.Y) * WorldScale);
                normals.Add(normal);
                float s = 0, t = 0;
                if (ti != null) {
                    s = Vector3.Dot(q, new Vector3(ti.Vecs0.X, ti.Vecs0.Y, ti.Vecs0.Z)) + ti.Vecs0.W;
                    t = Vector3.Dot(q, new Vector3(ti.Vecs1.X, ti.Vecs1.Y, ti.Vecs1.Z)) + ti.Vecs1.W;
                }
                uvs.Add(new Vector2(s / width, t / height));
            }
            for (var i = 1; i < corners.Length - 1; i++) {
                list.Add(baseI);
                list.Add(baseI + i);
                list.Add(baseI + i + 1);
            }
        }
        var sub = new BspSubmesh[order.Count];
        for (var i = 0; i < order.Count; i++) sub[i] = new BspSubmesh { TexId = order[i], Triangles = [.. tris[order[i]]] };
        return new BspMesh { Vertices = [.. vertices], Normals = [.. normals], UVs = [.. uvs], Submeshes = sub };
    }
}
