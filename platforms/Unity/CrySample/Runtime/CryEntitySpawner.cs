using System;
using System.Collections.Generic;
using UnityEngine;

namespace CryLevelImporter
{
    /// <summary>
    /// Spawns Unity GameObjects from parsed CryEntity records.
    /// Uses a registry keyed by EntityClass so games can override how
    /// specific CryEngine classes materialize (same pattern as mapping
    /// Quake classnames to spawn functions).
    /// </summary>
    public class CryEntitySpawner
    {
        public delegate GameObject SpawnHandler(CryEntity entity, Transform parent);

        readonly Dictionary<string, SpawnHandler> _handlers =
            new Dictionary<string, SpawnHandler>(StringComparer.OrdinalIgnoreCase);

        public SpawnHandler FallbackHandler;

        public CryEntitySpawner()
        {
            // Built-in handlers for common CryEngine classes.
            Register("Light", SpawnLight);
            Register("DestroyableLight", SpawnLight);
            Register("SpawnPoint", SpawnMarker(Color.green));
            Register("TagPoint", SpawnMarker(Color.yellow));
            Register("ProximityTrigger", SpawnTrigger);
            Register("AreaTrigger", SpawnTrigger);
            Register("ParticleEffect", SpawnMarker(new Color(1f, 0.5f, 0f)));
            Register("EnvironmentProbe", SpawnProbe);
            FallbackHandler = SpawnMarker(Color.gray);
        }

        public void Register(string entityClass, SpawnHandler handler)
            => _handlers[entityClass] = handler;

        public GameObject Spawn(CryEntity entity, Transform parent)
        {
            var handler = _handlers.TryGetValue(entity.EntityClass, out var h) ? h : FallbackHandler;
            var go = handler?.Invoke(entity, parent);
            if (go == null) return null;

            go.name = $"{entity.EntityClass}:{entity.Name}";
            go.transform.SetPositionAndRotation(entity.Position, entity.Rotation);
            go.transform.localScale = entity.Scale;

            var meta = go.AddComponent<CryEntityInfo>();
            meta.EntityClass = entity.EntityClass;
            meta.CopyProperties(entity.Properties);
            return go;
        }

        // ------------------------------------------------------- Handlers

        static GameObject SpawnLight(CryEntity e, Transform parent)
        {
            var go = NewChild(parent);
            var light = go.AddComponent<Light>();
            light.type = LightType.Point;

            if (e.TryGetFloat("Radius", out var radius)) light.range = radius;
            if (e.TryGetFloat("fRadius", out radius)) light.range = radius;
            if (e.TryGetColor("clrDiffuse", out var color)) light.color = color;
            if (e.TryGetFloat("fDiffuseMultiplier", out var mult))
                light.intensity = Mathf.Clamp(mult, 0.1f, 8f);
            return go;
        }

        static GameObject SpawnTrigger(CryEntity e, Transform parent)
        {
            var go = NewChild(parent);
            var box = go.AddComponent<BoxCollider>();
            box.isTrigger = true;

            var size = Vector3.one * 2f;
            if (e.TryGetFloat("DimX", out var dx)) size.x = dx;
            if (e.TryGetFloat("DimZ", out var dz)) size.y = dz; // Cry Z-up -> Unity Y
            if (e.TryGetFloat("DimY", out var dy)) size.z = dy;
            box.size = size;
            return go;
        }

        static GameObject SpawnProbe(CryEntity e, Transform parent)
        {
            var go = NewChild(parent);
            var probe = go.AddComponent<ReflectionProbe>();
            probe.mode = UnityEngine.Rendering.ReflectionProbeMode.Realtime;
            probe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.OnAwake;
            if (e.TryGetFloat("BoxSizeX", out var bx) &&
                e.TryGetFloat("BoxSizeY", out var by) &&
                e.TryGetFloat("BoxSizeZ", out var bz))
                probe.size = new Vector3(bx, bz, by);
            return go;
        }

        static SpawnHandler SpawnMarker(Color color) => (e, parent) =>
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.SetParent(parent, false);
            go.transform.localScale = Vector3.one * 0.5f;
            var renderer = go.GetComponent<Renderer>();
            renderer.material.color = color;
            UnityEngine.Object.Destroy(go.GetComponent<Collider>());
            return go;
        };

        static GameObject NewChild(Transform parent)
        {
            var go = new GameObject();
            go.transform.SetParent(parent, false);
            return go;
        }
    }

    /// <summary>
    /// Carries the original CryEngine property table on spawned objects
    /// so gameplay code (or an inspector) can read the raw values.
    /// </summary>
    public class CryEntityInfo : MonoBehaviour
    {
        public string EntityClass;
        [SerializeField] List<string> _keys = new List<string>();
        [SerializeField] List<string> _values = new List<string>();

        public void CopyProperties(Dictionary<string, string> props)
        {
            foreach (var kv in props) { _keys.Add(kv.Key); _values.Add(kv.Value); }
        }

        public string Get(string key)
        {
            int i = _keys.FindIndex(k => string.Equals(k, key, StringComparison.OrdinalIgnoreCase));
            return i >= 0 ? _values[i] : null;
        }
    }
}
