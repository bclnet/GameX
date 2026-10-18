using GameX.Arkane.Formats.Danae;
using OpenX.Gfx;
using System.Collections.Generic;
using System.Numerics;

namespace GameX.Arkane.Engines;

/// <summary>
/// Shared mesh for an FTL. Danae is Y-up, so positions stay as stored.
/// </summary>
public static class FtlObjectBuilder {
    public class FtlSubmesh {
        public int TexId;
        public int[] Triangles;
    }

    public class FtlMesh {
        public Vector3[] Vertices = [];
        public Vector3[] Normals = [];
        public Vector2[] UVs = [];
        public FtlSubmesh[] Submeshes = [];
    }

    public static IEnumerable<string> GetTexturePaths(E_3DOBJ obj) {
        if (obj.Textures == null) yield break;
        foreach (var t in obj.Textures)
            if (t != null && !string.IsNullOrEmpty(t.Path)) yield return t.Path;
    }

    public static MaterialStd2Prop ToMaterialProp(E_TEXTURE tex) {
        var mp = new MaterialStd2Prop();
        if (tex == null) return mp;
        if (!string.IsNullOrEmpty(tex.Path)) mp.Textures["Main"] = tex.Path;
        if (tex.Poly.HasFlag(POLY.TRANS) || tex.Poly.HasFlag(POLY.WATER)) { mp.AlphaBlended = true; mp.Alpha = 1; }
        return mp;
    }

    public static (string Name, Vector3 Position)[] GroupPositions(E_3DOBJ obj) {
        if (obj.Groups == null || obj.Vertexs == null) return [];
        var r = new (string Name, Vector3 Position)[obj.Groups.Length];
        for (var i = 0; i < obj.Groups.Length; i++) {
            var g = obj.Groups[i];
            r[i] = (g.Name, obj.Vertexs[g.Origin].V);
        }
        return r;
    }

    public static FtlMesh ToGeometry(E_3DOBJ obj) {
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();
        var order = new List<int>();
        var tris = new Dictionary<int, List<int>>();
        if (obj.Faces != null && obj.Vertexs != null)
            foreach (var face in obj.Faces) {
                if (face == null) continue;
                E_TEXTURE tex = null;
                if (obj.Textures != null && face.TexId >= 0 && face.TexId < obj.Textures.Length) tex = obj.Textures[face.TexId];
                if (tex != null && (tex.Poly.HasFlag(POLY.HIDE) || tex.Poly.HasFlag(POLY.NODRAW))) continue;
                if (!tris.TryGetValue(face.TexId, out var list)) { list = []; tris[face.TexId] = list; order.Add(face.TexId); }
                int[] ids = [face.Vid.X, face.Vid.Y, face.Vid.Z];
                float[] us = [face.U.X, face.U.Y, face.U.Z], vs = [face.V.X, face.V.Y, face.V.Z];
                var baseI = vertices.Count;
                for (var c = 0; c < 3; c++) {
                    var src = obj.Vertexs[ids[c]];
                    vertices.Add(src.V);
                    Vector3 n;
                    if (face.Nrmls != null && c < face.Nrmls.Length) n = face.Nrmls[c];
                    else if (face.Norm != Vector3.Zero) n = face.Norm;
                    else n = src.Norm;
                    normals.Add(n);
                    uvs.Add(new Vector2(us[c], vs[c]));
                }
                list.Add(baseI); list.Add(baseI + 1); list.Add(baseI + 2);
                if (face.FaceType == 2 || (tex != null && tex.Poly.HasFlag(POLY.DOUBLESIDED))) { list.Add(baseI); list.Add(baseI + 2); list.Add(baseI + 1); }
            }
        var sub = new FtlSubmesh[order.Count];
        for (var i = 0; i < order.Count; i++) sub[i] = new FtlSubmesh { TexId = order[i], Triangles = [.. tris[order[i]]] };
        return new FtlMesh { Vertices = [.. vertices], Normals = [.. normals], UVs = [.. uvs], Submeshes = sub };
    }
}
