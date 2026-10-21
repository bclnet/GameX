using GameX.Arkane.Engines;
using GameX.Arkane.Formats.Danae;
using OpenX;
using OpenX.Gfx;
using OpenX.Gfx.OpenGL;
using System;
using System.Numerics;
using System.Threading.Tasks;
using static GameX.Arkane.Engines.FtlObjectBuilder;

namespace GameX.Engines.OpenGL;

/// <summary>
/// An FTL kept as the shared mesh. Danae is Y-up, so positions stay as stored.
/// OpenGL's material builder only accepts MaterialShaderVProp, so the props stay MaterialStd2Prop.
/// </summary>
public class OpenGLFtlObject : IGlDrawable {
    public string Name;
    public bool IsStatic;
    public FtlMesh Data;
    public MaterialStd2Prop[] Materials = [];
    public (string Name, Vector3 Position)[] Groups = [];
    GlMeshDraw _draw;

    public void DrawGl() {
        if (_draw == null) _draw = new GlMeshDraw(Data?.Vertices, Cat(Data?.Submeshes));
        _draw.DrawGl();
    }

    static int[] Cat(FtlSubmesh[] subs) {
        if (subs == null || subs.Length == 0) return [];
        var n = 0;
        foreach (var s in subs) n += s?.Triangles?.Length ?? 0;
        var all = new int[n];
        var o = 0;
        foreach (var s in subs) {
            if (s?.Triangles == null) continue;
            Array.Copy(s.Triangles, 0, all, o, s.Triangles.Length);
            o += s.Triangles.Length;
        }
        return all;
    }
}

public static class OpenGLFtlObjectBuilder {
    public static Task<object> BuildObject(ISource source, object path, bool isStatic, MaterialManager<GLRenderMaterial, int> materialManager) {
        var obj = ((Binary_Ftl)path).Obj;
        var name = string.IsNullOrEmpty(obj.File) ? "ftl" : obj.File;
        var textureManager = materialManager.TextureManager;
        foreach (var texturePath in GetTexturePaths(obj)) textureManager.Preload(source, texturePath);
        var data = ToGeometry(obj);
        var materials = new MaterialStd2Prop[data.Submeshes.Length];
        for (var i = 0; i < data.Submeshes.Length; i++) {
            var sub = data.Submeshes[i];
            E_TEXTURE tex = null;
            if (obj.Textures != null && sub.TexId >= 0 && sub.TexId < obj.Textures.Length) tex = obj.Textures[sub.TexId];
            materials[i] = ToMaterialProp(tex);
        }
        return Task.FromResult<object>(new OpenGLFtlObject { Name = name, IsStatic = isStatic, Data = data, Materials = materials, Groups = GroupPositions(obj) });
    }
}
