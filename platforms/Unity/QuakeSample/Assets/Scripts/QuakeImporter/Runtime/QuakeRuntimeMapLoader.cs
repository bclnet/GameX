using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace QuakeImporter
{
    /// <summary>
    /// Drop this on an empty GameObject in an empty scene and press Play.
    /// Reads a Quake .map (text format) at runtime - either from an assigned TextAsset,
    /// or from a path under StreamingAssets - and builds the full level into the scene:
    /// brush geometry as meshes, entities as GameObjects with behavior components.
    /// </summary>
    public class QuakeRuntimeMapLoader : MonoBehaviour
    {
        [Header("Map Source (TextAsset wins if both are set)")]
        public TextAsset mapTextAsset;
        [Tooltip("Path relative to Application.streamingAssetsPath.")]
        public string streamingAssetsMapPath = "Maps/sample_room.map";

        [Header("Conversion Settings")]
        [Tooltip("Quake units -> Unity meters. Must match the scale the .map's coordinates assume.")]
        public float worldScale = 1f / 32f;
        [Tooltip("Flip if geometry renders inside-out / backface-culled from inside rooms.")]
        public bool flipWinding = false;

        [Header("Quick Test Player")]
        [Tooltip("Spawns a basic WASD+mouselook capsule at the first info_player_start so you can walk the level immediately.")]
        public bool spawnTestPlayer = true;

        public Transform LevelRoot { get; private set; }

        private void Start()
        {
            string text = LoadMapText();
            if (!string.IsNullOrEmpty(text))
                BuildMap(text);
        }

        private string LoadMapText()
        {
            if (mapTextAsset != null) return mapTextAsset.text;

            string fullPath = Path.Combine(Application.streamingAssetsPath, streamingAssetsMapPath);

#if UNITY_ANDROID && !UNITY_EDITOR
            Debug.LogWarning("[QuakeImporter] On Android, StreamingAssets paths are inside a compressed " +
                              "archive and File.ReadAllText won't work - use UnityWebRequest instead, or " +
                              "assign mapTextAsset.");
#endif
            if (!File.Exists(fullPath))
            {
                Debug.LogError($"[QuakeImporter] Map not found at '{fullPath}'. " +
                                "Assign a TextAsset or check streamingAssetsMapPath.");
                return null;
            }

            return File.ReadAllText(fullPath);
        }

        /// <summary>Parses and spawns the given map text into the active scene. Returns the level root.</summary>
        public GameObject BuildMap(string mapText)
        {
            var options = new QuakeParseOptions { WorldScale = worldScale };
            QMap map;

            try
            {
                map = QuakeMapParser.Parse(mapText, options);
            }
            catch (Exception e)
            {
                Debug.LogError($"[QuakeImporter] Failed to parse map: {e.Message}");
                return null;
            }

            var root = new GameObject("QuakeLevel");
            LevelRoot = root.transform;

            var targetnameLookup = new Dictionary<string, List<GameObject>>();
            var spawned = new List<(QEntity entity, GameObject go)>();
            GameObject firstPlayerStart = null;

            foreach (var entity in map.Entities)
            {
                GameObject go = QuakeEntitySpawner.Spawn(entity, root.transform, flipWinding, worldScale);
                if (go == null) continue;

                spawned.Add((entity, go));

                if (firstPlayerStart == null && go.GetComponent<QuakePlayerStart>() != null)
                    firstPlayerStart = go;

                if (entity.Properties.TryGetValue("targetname", out var tn) && !string.IsNullOrEmpty(tn))
                {
                    if (!targetnameLookup.TryGetValue(tn, out var list))
                        targetnameLookup[tn] = list = new List<GameObject>();
                    list.Add(go);
                }
            }

            // Second pass: resolve "target" -> "targetname" references (e.g. trigger_multiple -> func_door).
            foreach (var (entity, go) in spawned)
            {
                if (!entity.Properties.TryGetValue("target", out var targetName) || string.IsNullOrEmpty(targetName))
                    continue;

                if (!targetnameLookup.TryGetValue(targetName, out var targets))
                {
                    Debug.LogWarning($"[QuakeImporter] '{entity.Classname}' targets unknown targetname '{targetName}'.");
                    continue;
                }

                var relay = go.GetComponent<QuakeTriggerRelay>();
                if (relay != null)
                    relay.Targets.AddRange(targets);
            }

            if (spawnTestPlayer && firstPlayerStart != null)
                QuakeQuickTestPlayer.Spawn(firstPlayerStart.transform.position, firstPlayerStart.transform.rotation);

            return root;
        }
    }
}
