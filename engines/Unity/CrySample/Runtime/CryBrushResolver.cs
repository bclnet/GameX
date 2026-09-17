using System;
using System.Collections.Generic;
using UnityEngine;

namespace CryLevelImporter
{
    /// <summary>
    /// Resolves brush (.cgf) references to Unity objects.
    /// The binary CGF format isn't parsed here — this resolver spawns
    /// labeled placeholder boxes by default, and exposes a hook so a real
    /// mesh pipeline (pre-converted prefabs, an Addressables lookup, or a
    /// future CGF parser) can take over per-path.
    /// </summary>
    public class CryBrushResolver
    {
        /// <summary>Optional: map "objects/foo/bar.cgf" -> prefab. Checked first.</summary>
        public Func<string, GameObject> PrefabLookup;

        readonly Dictionary<string, Material> _materialCache = new Dictionary<string, Material>();

        public GameObject Resolve(CryBrush brush, Transform parent)
        {
            GameObject go = null;

            var prefab = PrefabLookup?.Invoke(brush.PrefabPath);
            if (prefab != null)
            {
                go = UnityEngine.Object.Instantiate(prefab, parent);
            }
            else
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.transform.SetParent(parent, false);
                go.GetComponent<Renderer>().sharedMaterial = PlaceholderMaterial(brush.PrefabPath);
            }

            go.name = $"Brush:{brush.Name}";
            go.transform.SetPositionAndRotation(brush.Position, brush.Rotation);
            go.transform.localScale = brush.Scale;
            go.isStatic = true;
            return go;
        }

        Material PlaceholderMaterial(string path)
        {
            // Stable color per source path so identical assets read as identical.
            var key = path ?? "";
            if (_materialCache.TryGetValue(key, out var mat)) return mat;

            var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            mat = new Material(shader);
            UnityEngine.Random.InitState(key.GetHashCode());
            mat.color = Color.HSVToRGB(UnityEngine.Random.value, 0.35f, 0.8f);
            _materialCache[key] = mat;
            return mat;
        }
    }
}
