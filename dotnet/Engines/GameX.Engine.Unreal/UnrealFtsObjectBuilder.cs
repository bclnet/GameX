using GameX.Arkane.Formats.Danae;
using static GameX.Arkane.Engines.FtsObjectBuilder;

namespace GameX.Engines.Unreal;

/// <summary>
/// An FTS map kept as one mesh. Danae is Y-up, so positions stay as stored.
/// </summary>
public static class UnrealFtsObjectBuilder {
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
        var holder = new UnrealObjectHolder {
            Name = name,
            IsStatic = isStatic,
            Mesh = mesh,
            Materials = materials,
            Groups = ScenePositions(level),
        };
        return holder;
    }
}
