using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace CryLevelImporter
{
    /// <summary>
    /// Drop this on an empty GameObject, point LevelFolder at an extracted
    /// Crysis 2 / CryEngine 3 level directory, and press Play.
    ///
    /// Load order:
    ///   1. Parse XML + heightmap off the main thread
    ///   2. Build terrain
    ///   3. Create layer containers (Cry object layers -> child GameObjects)
    ///   4. Spawn brushes and entities, time-sliced across frames
    ///   5. Apply time-of-day lighting
    /// </summary>
    public class CryLevelLoader : MonoBehaviour
    {
        [Header("Source")]
        [Tooltip("Path to an extracted level folder (unzip level.pak into it first)")]
        public string LevelFolder = "";

        [Header("Options")]
        public bool BuildTerrain = true;
        public bool SpawnEntities = true;
        public bool SpawnBrushes = true;
        public bool ApplyTimeOfDay = true;
        [Tooltip("Objects instantiated per frame while building")]
        public int SpawnBudgetPerFrame = 64;

        [Header("Runtime (read-only)")]
        public string Status = "Idle";

        public CryLevelData Level { get; private set; }
        public CryEntitySpawner EntitySpawner { get; } = new CryEntitySpawner();
        public CryBrushResolver BrushResolver { get; } = new CryBrushResolver();

        readonly Dictionary<string, Transform> _layerContainers = new Dictionary<string, Transform>();

        void Start()
        {
            if (!string.IsNullOrEmpty(LevelFolder))
                StartCoroutine(LoadRoutine(LevelFolder));
        }

        public IEnumerator LoadRoutine(string folder)
        {
            Status = "Parsing…";

            // 1. Parse on a worker thread — XML + heightmap I/O never touches
            //    the Unity API, so it's safe off the main thread.
            var parseTask = Task.Run(() => CryLevelParser.Parse(folder));
            while (!parseTask.IsCompleted) yield return null;

            if (parseTask.IsFaulted)
            {
                Status = "Parse failed (see console)";
                Debug.LogException(parseTask.Exception);
                yield break;
            }

            Level = parseTask.Result;
            var root = new GameObject($"CryLevel:{Level.LevelName}").transform;

            // 2. Terrain
            if (BuildTerrain)
            {
                Status = "Building terrain…";
                yield return null;
                CryTerrainBuilder.Build(Level, root);
            }

            // 3. Layers -> container GameObjects (the Scene-per-layer version
            //    would call SceneManager.LoadSceneAsync(additive) here instead)
            var layersRoot = new GameObject("Layers").transform;
            layersRoot.SetParent(root, false);
            foreach (var layer in Level.Layers)
            {
                var container = new GameObject(layer.Name);
                container.transform.SetParent(ResolveLayerParent(layer, layersRoot), false);
                container.SetActive(layer.VisibleByDefault);
                layer.Container = container;
                _layerContainers[layer.Name] = container.transform;
            }
            var defaultLayer = new GameObject("(no layer)").transform;
            defaultLayer.SetParent(layersRoot, false);

            // 4. Brushes + entities, time-sliced
            int budget = SpawnBudgetPerFrame;

            if (SpawnBrushes)
            {
                Status = $"Spawning {Level.Brushes.Count} brushes…";
                foreach (var brush in Level.Brushes)
                {
                    BrushResolver.Resolve(brush, ContainerFor(brush.Layer, defaultLayer));
                    if (--budget <= 0) { budget = SpawnBudgetPerFrame; yield return null; }
                }
            }

            if (SpawnEntities)
            {
                Status = $"Spawning {Level.Entities.Count} entities…";
                foreach (var entity in Level.Entities)
                {
                    EntitySpawner.Spawn(entity, ContainerFor(entity.Layer, defaultLayer));
                    if (--budget <= 0) { budget = SpawnBudgetPerFrame; yield return null; }
                }
            }

            // 5. Lighting
            if (ApplyTimeOfDay)
            {
                Status = "Applying time of day…";
                ApplyTod(Level.TimeOfDay, root);
            }

            Status = $"Loaded: {Level.Entities.Count} entities, {Level.Brushes.Count} brushes";
            Debug.Log($"[CryImporter] {Status}");
        }

        /// <summary>Toggle a CryEngine object layer on/off at runtime (streaming stand-in).</summary>
        public void SetLayerActive(string layerName, bool active)
        {
            if (_layerContainers.TryGetValue(layerName, out var t))
                t.gameObject.SetActive(active);
        }

        Transform ContainerFor(string layerName, Transform fallback)
            => layerName != null && _layerContainers.TryGetValue(layerName, out var t) ? t : fallback;

        Transform ResolveLayerParent(CryLayer layer, Transform layersRoot)
            => layer.Parent != null && _layerContainers.TryGetValue(layer.Parent, out var p) ? p : layersRoot;

        static void ApplyTod(CryTimeOfDay tod, Transform root)
        {
            var sunGo = new GameObject("Sun (TimeOfDay)");
            sunGo.transform.SetParent(root, false);
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.color = tod.SunColor;
            sun.intensity = tod.SunIntensity;
            sun.shadows = LightShadows.Soft;

            // Convert ToD hour + latitude/longitude into a sun direction.
            float hourAngle = (tod.Time / 24f) * 360f - 90f;
            sunGo.transform.rotation =
                Quaternion.Euler(0f, tod.SunLongitude, 0f) *
                Quaternion.Euler(hourAngle, 0f, 0f) *
                Quaternion.Euler(tod.SunLatitude, 0f, 0f);

            RenderSettings.sun = sun;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogColor = tod.FogColor;
            RenderSettings.fogDensity = Mathf.Max(tod.FogDensity, 0.0005f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = tod.FogColor * 1.1f;
            RenderSettings.ambientEquatorColor = tod.FogColor * 0.7f;
            RenderSettings.ambientGroundColor = tod.FogColor * 0.35f;
        }
    }
}
