using UnityEngine;

namespace QuakeImporter
{
    /// <summary>
    /// Maps Quake entity classnames to spawn functions. Unknown classnames that still carry
    /// brush geometry get their geometry built anyway (so the level isn't missing chunks),
    /// just without any special behavior attached.
    /// </summary>
    public static class QuakeEntitySpawner
    {
        public delegate GameObject SpawnFn(QEntity entity, Transform parent, bool flipWinding, float worldScale);

        private static readonly System.Collections.Generic.Dictionary<string, SpawnFn> Spawners =
            new System.Collections.Generic.Dictionary<string, SpawnFn>
            {
                ["worldspawn"] = SpawnBrushGeometry,
                ["func_wall"] = SpawnBrushGeometry,
                ["func_door"] = SpawnFuncDoor,
                ["trigger_multiple"] = SpawnTriggerMultiple,
                ["light"] = SpawnLight,
                ["info_player_start"] = SpawnPlayerStart,
            };

        public static GameObject Spawn(QEntity entity, Transform parent, bool flipWinding, float worldScale)
        {
            if (Spawners.TryGetValue(entity.Classname, out var fn))
                return fn(entity, parent, flipWinding, worldScale);

            if (entity.Brushes.Count > 0)
                return SpawnBrushGeometry(entity, parent, flipWinding, worldScale);

            Debug.LogWarning($"[QuakeImporter] No spawner for classname '{entity.Classname}', skipping.");
            return null;
        }

        private static GameObject SpawnBrushGeometry(QEntity e, Transform parent, bool flipWinding, float worldScale)
        {
            var go = new GameObject(string.IsNullOrEmpty(e.Classname) ? "brush_entity" : e.Classname);
            go.transform.SetParent(parent, false);
            QuakeGeometryBuilder.BuildEntityBrushes(e, go.transform, flipWinding, worldScale);
            return go;
        }

        private static GameObject SpawnPlayerStart(QEntity e, Transform parent, bool flipWinding, float worldScale)
        {
            var go = new GameObject("info_player_start");
            go.transform.SetParent(parent, false);
            go.transform.position = e.Origin;
            go.transform.rotation = Quaternion.Euler(0f, -e.GetFloat("angle", 0f), 0f);
            go.AddComponent<QuakePlayerStart>();
            return go;
        }

        private static GameObject SpawnLight(QEntity e, Transform parent, bool flipWinding, float worldScale)
        {
            var go = new GameObject("light");
            go.transform.SetParent(parent, false);
            go.transform.position = e.Origin;

            var light = go.AddComponent<Light>();
            light.type = LightType.Point;

            float quakeLight = e.GetFloat("light", 200f);
            light.range = quakeLight * 0.05f;
            light.intensity = quakeLight / 150f;
            light.color = ParseLightColor(e);
            return go;
        }

        private static Color ParseLightColor(QEntity e)
        {
            if (e.Properties.TryGetValue("_color", out var raw))
            {
                var parts = raw.Split(' ');
                if (parts.Length == 3 &&
                    float.TryParse(parts[0], out var r) &&
                    float.TryParse(parts[1], out var g) &&
                    float.TryParse(parts[2], out var b))
                {
                    return new Color(r, g, b);
                }
            }
            return Color.white;
        }

        private static GameObject SpawnFuncDoor(QEntity e, Transform parent, bool flipWinding, float worldScale)
        {
            var go = new GameObject("func_door");
            go.transform.SetParent(parent, false);
            QuakeGeometryBuilder.BuildEntityBrushes(e, go.transform, flipWinding, worldScale);

            var door = go.AddComponent<QuakeFuncDoor>();
            door.Speed = e.GetFloat("speed", 100f) * 0.04f; // rough Quake-units/sec -> Unity m/s
            door.WaitSeconds = e.GetFloat("wait", 3f);
            door.MoveAngleDegrees = e.GetFloat("angle", 0f);
            door.Initialize();
            return go;
        }

        private static GameObject SpawnTriggerMultiple(QEntity e, Transform parent, bool flipWinding, float worldScale)
        {
            var go = new GameObject("trigger_multiple");
            go.transform.SetParent(parent, false);
            QuakeGeometryBuilder.BuildEntityBrushes(e, go.transform, flipWinding, worldScale);

            go.AddComponent<QuakeTriggerRelay>();
            return go;
        }
    }
}
