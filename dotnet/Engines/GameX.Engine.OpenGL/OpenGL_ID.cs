using GameX.ID.Engines;
using GameX.ID.Formats;
using OpenX;
using OpenX.Gfx;
using OpenX.Gfx.OpenGL;
using System;
using System.Threading.Tasks;
using static GameX.ID.Engines.BspObjectBuilder;

namespace GameX.Engines.OpenGL;

/// <summary>
/// A BSP kept as the shared mesh. Quake is Z-up; the builder swaps Y and Z.
/// OpenGL's material builder only accepts MaterialShaderVProp, so the props stay MaterialStd2Prop.
/// </summary>
public class OpenGLBspObject : IGlDrawable {
    public string Name;
    public bool IsStatic;
    public BspMesh Data;
    public MaterialStd2Prop[] Materials = [];
    GlMeshDraw _draw;

    public void DrawGl() {
        if (_draw == null) _draw = new GlMeshDraw(Data?.Vertices, Cat(Data?.Submeshes));
        _draw.DrawGl();
    }

    static int[] Cat(BspSubmesh[] subs) {
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

public static class OpenGLBspObjectBuilder {
    public static Task<object> BuildObject(ISource source, object path, bool isStatic, MaterialManager<GLRenderMaterial, int> materialManager) {
        var bsp = (Binary_BspX)path;
        var level = bsp.Level;
        var textureManager = materialManager.TextureManager;
        foreach (var texturePath in GetTexturePaths(bsp)) textureManager.Preload(source, texturePath);
        var data = ToGeometry(bsp);
        var materials = new MaterialStd2Prop[data.Submeshes.Length];
        for (var i = 0; i < data.Submeshes.Length; i++) {
            var sub = data.Submeshes[i];
            BspTexture tex = null;
            if (level != null && level.Textures != null && sub.TexId >= 0 && sub.TexId < level.Textures.Length) tex = level.Textures[sub.TexId];
            materials[i] = ToMaterialProp(tex);
        }
        return Task.FromResult<object>(new OpenGLBspObject { Name = "bsp", IsStatic = isStatic, Data = data, Materials = materials });
    }
}
