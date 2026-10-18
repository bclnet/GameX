using GameX.Arkane.Formats.Danae;
using OpenX.Gfx;
using System.Collections.Generic;
using System.Numerics;

namespace GameX.Arkane.Engines;

/// <summary>
/// Shared mesh for an FTS map. Danae is Y-up, so positions stay as stored.
/// A quad is a triangle strip: (0, 1, 2) then (1, 3, 2).
/// </summary>
public static class FtsObjectBuilder {
    public class FtsSubmesh {
        public int TexId;
        public POLY Type;
        public int[] Triangles;
    }

    public class FtsMesh {
        public Vector3[] Vertices = [];
        public Vector3[] Normals = [];
        public Vector2[] UVs = [];
        public FtsSubmesh[] Submeshes = [];
    }

    public static IEnumerable<string> GetTexturePaths(Binary_Fts.F_LEVEL level) {
        if (level.Textures == null) yield break;
        foreach (var t in level.Textures)
            if (t != null && !string.IsNullOrEmpty(t.Path)) yield return t.Path;
    }

    public static MaterialStd2Prop ToMaterialProp(E_TEXTURE tex, POLY type = default) {
        var mp = new MaterialStd2Prop();
        if (tex != null && !string.IsNullOrEmpty(tex.Path)) mp.Textures["Main"] = tex.Path;
        var flags = type;
        if (tex != null) flags |= tex.Poly;
        if (flags.HasFlag(POLY.TRANS) || flags.HasFlag(POLY.WATER)) { mp.AlphaBlended = true; mp.Alpha = 1; }
        return mp;
    }

    public static (string Name, Vector3 Position)[] ScenePositions(Binary_Fts.F_LEVEL level)
        => [("player", level.PlayerPos), ("mscene", level.MscenePos)];

    public static FtsMesh ToGeometry(Binary_Fts.F_LEVEL level) {
        var vertices = new List<Vector3>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();
        var order = new List<int>();
        var tris = new Dictionary<int, List<int>>();
        var types = new Dictionary<int, POLY>();
        if (level.Backg != null)
            foreach (var cell in level.Backg) {
                if (cell.Polydata == null) continue;
                foreach (var poly in cell.Polydata) {
                    if (poly == null) continue;
                    var tex = poly.Tex;
                    if (poly.Type.HasFlag(POLY.HIDE) || poly.Type.HasFlag(POLY.NODRAW)) continue;
                    if (tex != null && (tex.Poly.HasFlag(POLY.HIDE) || tex.Poly.HasFlag(POLY.NODRAW))) continue;
                    var quad = poly.Type.HasFlag(POLY.QUAD);
                    var count = quad ? 4 : 3;
                    if (poly.V == null || poly.V.Length < count) continue;
                    var texId = TexIndex(tex, level.Textures);
                    if (!tris.TryGetValue(texId, out var list)) { list = []; tris[texId] = list; order.Add(texId); types[texId] = POLY.NONE_; }
                    types[texId] |= poly.Type & (POLY.TRANS | POLY.WATER);
                    var baseI = vertices.Count;
                    for (var c = 0; c < count; c++) {
                        var src = poly.V[c];
                        vertices.Add(src.S);
                        Vector3 n;
                        if (poly.Nrml != null && c < poly.Nrml.Length) n = poly.Nrml[c];
                        else if (poly.Norm != Vector3.Zero) n = poly.Norm;
                        else n = Vector3.Zero;
                        normals.Add(n);
                        uvs.Add(src.T);
                    }
                    list.Add(baseI); list.Add(baseI + 1); list.Add(baseI + 2);
                    if (quad) { list.Add(baseI + 1); list.Add(baseI + 3); list.Add(baseI + 2); }
                    if (poly.Type.HasFlag(POLY.DOUBLESIDED)) {
                        list.Add(baseI); list.Add(baseI + 2); list.Add(baseI + 1);
                        if (quad) { list.Add(baseI + 1); list.Add(baseI + 2); list.Add(baseI + 3); }
                    }
                }
            }
        var sub = new FtsSubmesh[order.Count];
        for (var i = 0; i < order.Count; i++) sub[i] = new FtsSubmesh { TexId = order[i], Type = types[order[i]], Triangles = [.. tris[order[i]]] };
        return new FtsMesh { Vertices = [.. vertices], Normals = [.. normals], UVs = [.. uvs], Submeshes = sub };
    }

    static int TexIndex(E_TEXTURE tex, E_TEXTURE[] textures) {
        if (tex == null || textures == null || tex.Id == 0) return -1;
        for (var i = 0; i < textures.Length; i++)
            if (textures[i] != null && textures[i].Id == tex.Id) return i;
        return -1;
    }
}
