using System.Collections.Generic;
using UnityEngine;

namespace QuakeImporter
{
    /// <summary>
    /// Resolves a Quake texture name to a Unity Material. Looks for a hand-authored
    /// material at Resources/QuakeMaterials/&lt;textureName&gt; first (drop your own
    /// converted WAD textures there); otherwise generates a flat-colored placeholder
    /// so the level is at least visually distinguishable per-surface out of the box.
    /// </summary>
    public static class QuakeMaterialUtil
    {
        private static readonly Dictionary<string, Material> Cache = new Dictionary<string, Material>();

        public static Material GetMaterial(string textureName)
        {
            if (Cache.TryGetValue(textureName, out var cached)) return cached;

            var loaded = Resources.Load<Material>($"QuakeMaterials/{textureName}");
            if (loaded != null)
            {
                Cache[textureName] = loaded;
                return loaded;
            }

            Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                ?? Shader.Find("Standard")
                ?? Shader.Find("Diffuse");

            var generated = new Material(shader) { name = $"Quake_{textureName}" };
            if (generated.HasProperty("_BaseColor")) generated.SetColor("_BaseColor", ColorFromName(textureName));
            else if (generated.HasProperty("_Color")) generated.SetColor("_Color", ColorFromName(textureName));

            Cache[textureName] = generated;
            return generated;
        }

        private static Color ColorFromName(string name)
        {
            int hash = 17;
            foreach (char c in name) hash = hash * 31 + c;
            var rng = new System.Random(hash);
            return new Color(
                (float)rng.NextDouble() * 0.55f + 0.35f,
                (float)rng.NextDouble() * 0.55f + 0.35f,
                (float)rng.NextDouble() * 0.55f + 0.35f);
        }
    }
}
