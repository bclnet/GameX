using GameX.Arkane.Formats.Danae;
using static GameX.Arkane.Engines.FtlObjectBuilder;
using static GameX.Arkane.Engines.FtsObjectBuilder;

namespace GameX.Engines.Ogre;

/// <summary>
/// An FTL kept as the shared mesh. Danae is Y-up, so positions stay as stored.
/// </summary>
public static class OgreFtlObjectBuilder {
    public static object BuildObject(object path, bool isStatic, object materialManager) {
        var obj = ((Binary_Ftl)path).Obj;
        var name = string.IsNullOrEmpty(obj.File) ? "ftl" : obj.File;
        foreach (var texturePath in GetTexturePaths(obj))
            if (materialManager != null) _ = texturePath;
        var mesh = ToGeometry(obj);
        var materials = new object[mesh.Submeshes.Length];
        for (var i = 0; i < mesh.Submeshes.Length; i++) {
            var sub = mesh.Submeshes[i];
            E_TEXTURE tex = null;
            if (obj.Textures != null && sub.TexId >= 0 && sub.TexId < obj.Textures.Length) tex = obj.Textures[sub.TexId];
            materials[i] = ToMaterialProp(tex);
        }
        var holder = new OgreObjectHolder {
            Name = name,
            IsStatic = isStatic,
            Mesh = mesh,
            Materials = materials,
            Groups = GroupPositions(obj),
        };
        return holder;
    }
}

/// <summary>
/// An FTS map kept as one mesh. Danae is Y-up, so positions stay as stored.
/// </summary>
public static class OgreFtsObjectBuilder {
    public static object BuildObject(object path, bool isStatic, object materialManager) {
        var level = ((Binary_Fts)path).Level;
        var name = "fts";
        foreach (var texturePath in GetTexturePaths(level))
            if (materialManager != null) _ = texturePath;
        var mesh = ToGeometry(level);
        var materials = new object[mesh.Submeshes.Length];
        for (var i = 0; i < mesh.Submeshes.Length; i++) {
            var sub = mesh.Submeshes[i];
            E_TEXTURE tex = null;
            if (level.Textures != null && sub.TexId >= 0 && sub.TexId < level.Textures.Length) tex = level.Textures[sub.TexId];
            materials[i] = ToMaterialProp(tex, sub.Type);
        }
        var holder = new OgreObjectHolder {
            Name = name,
            IsStatic = isStatic,
            Mesh = mesh,
            Materials = materials,
            Groups = ScenePositions(level),
        };
        return holder;
    }
}
