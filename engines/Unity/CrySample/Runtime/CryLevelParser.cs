using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEngine;

namespace CryLevelImporter
{
    /// <summary>
    /// Parses an *extracted* CryEngine 3 level folder at runtime.
    /// Expected inputs (unzip level.pak into the level folder first):
    ///   LevelData.xml            - terrain dimensions, layer list
    ///   Mission_*.xml            - entity + brush placements per mission
    ///   TimeOfDay.xml            - lighting curves (sampled, not fully imported)
    ///   terrain/heightmap.raw    - optional 16-bit raw heightmap export
    /// Formats vary slightly across CryEngine builds; every read below is
    /// defensive and falls back to defaults instead of throwing.
    /// </summary>
    public static class CryLevelParser
    {
        public static CryLevelData Parse(string levelFolder)
        {
            var data = new CryLevelData
            {
                RootPath = levelFolder,
                LevelName = new DirectoryInfo(levelFolder).Name
            };

            ParseLevelData(data);
            ParseMissions(data);
            ParseTimeOfDay(data);
            LoadHeightmap(data);
            return data;
        }

        // ---------------------------------------------------------- LevelData

        static void ParseLevelData(CryLevelData data)
        {
            var path = FindFile(data.RootPath, "LevelData.xml") ?? FindFile(data.RootPath, "leveldata.xml");
            if (path == null) { Debug.LogWarning("[CryImporter] LevelData.xml not found; using terrain defaults."); return; }

            var doc = LoadXml(path);
            if (doc == null) return;

            // <LevelInfo HeightmapSize="1024" HeightmapUnitSize="2" HeightmapMaxHeight="1024" ... />
            var info = doc.SelectSingleNode("//LevelInfo");
            if (info?.Attributes != null)
            {
                data.HeightmapResolution = GetIntAttr(info, "HeightmapSize", data.HeightmapResolution);
                data.UnitSize            = GetFloatAttr(info, "HeightmapUnitSize", data.UnitSize);
                data.TerrainMaxHeight    = GetFloatAttr(info, "HeightmapMaxHeight", data.TerrainMaxHeight);
            }

            // <Layers><Layer Name="..." Parent="..." Visible="1" External="0"/></Layers>
            foreach (XmlNode node in doc.SelectNodes("//Layers/Layer"))
            {
                data.Layers.Add(new CryLayer
                {
                    Name = GetAttr(node, "Name", "Unnamed"),
                    Parent = GetAttr(node, "Parent", null),
                    VisibleByDefault = GetIntAttr(node, "Visible", 1) != 0,
                    ExternalStreamed = GetIntAttr(node, "External", 0) != 0,
                });
            }
        }

        // ----------------------------------------------------------- Missions

        static void ParseMissions(CryLevelData data)
        {
            foreach (var missionFile in Directory.GetFiles(data.RootPath, "Mission_*.xml", SearchOption.TopDirectoryOnly))
            {
                var doc = LoadXml(missionFile);
                if (doc == null) continue;

                foreach (XmlNode node in doc.SelectNodes("//Objects/Object"))
                {
                    var type = GetAttr(node, "Type", "");
                    if (string.Equals(type, "Entity", StringComparison.OrdinalIgnoreCase))
                        data.Entities.Add(ParseEntity(node));
                    else if (string.Equals(type, "Brush", StringComparison.OrdinalIgnoreCase))
                        data.Brushes.Add(ParseBrush(node));
                }

                // Some builds nest entities under <Mission><Objects> with <Entity> nodes directly
                foreach (XmlNode node in doc.SelectNodes("//Objects/Entity"))
                    data.Entities.Add(ParseEntity(node));
            }

            Debug.Log($"[CryImporter] Parsed {data.Entities.Count} entities, {data.Brushes.Count} brushes, {data.Layers.Count} layers.");
        }

        static CryEntity ParseEntity(XmlNode node)
        {
            var e = new CryEntity
            {
                Name = GetAttr(node, "Name", "Entity"),
                EntityClass = GetAttr(node, "EntityClass", GetAttr(node, "Class", "Unknown")),
                Layer = GetAttr(node, "Layer", null),
            };
            ReadTransform(node, out e.Position, out e.Rotation, out var scale);
            e.Scale = scale;

            // <Properties a="1" b="2"> and nested <Properties2> tables
            foreach (var propsName in new[] { "Properties", "Properties2" })
            {
                var props = node.SelectSingleNode(propsName);
                if (props?.Attributes == null) continue;
                foreach (XmlAttribute attr in props.Attributes)
                    e.Properties[attr.Name] = attr.Value;
            }
            return e;
        }

        static CryBrush ParseBrush(XmlNode node)
        {
            var b = new CryBrush
            {
                Name = GetAttr(node, "Name", "Brush"),
                PrefabPath = GetAttr(node, "Prefab", GetAttr(node, "Geometry", "")),
                Layer = GetAttr(node, "Layer", null),
            };
            ReadTransform(node, out b.Position, out b.Rotation, out var scale);
            b.Scale = scale;
            return b;
        }

        static void ReadTransform(XmlNode node, out Vector3 pos, out Quaternion rot, out Vector3 scale)
        {
            pos = Vector3.zero; rot = Quaternion.identity; scale = Vector3.one;

            if (CrySpace.TryParseVec3(GetAttr(node, "Pos", null), out var cryPos))
                pos = CrySpace.Position(cryPos);

            if (!CrySpace.TryParseQuat(GetAttr(node, "Rotate", null), out rot))
            {
                // Older format: Euler angles in "Angles"
                if (CrySpace.TryParseVec3(GetAttr(node, "Angles", null), out var euler))
                    rot = Quaternion.Euler(-euler.x, -euler.z, -euler.y);
            }

            if (CrySpace.TryParseVec3(GetAttr(node, "Scale", null), out var cryScale))
                scale = new Vector3(cryScale.x, cryScale.z, cryScale.y);
        }

        // -------------------------------------------------------- TimeOfDay

        static void ParseTimeOfDay(CryLevelData data)
        {
            var path = FindFile(data.RootPath, "TimeOfDay.xml");
            if (path == null) return;
            var doc = LoadXml(path);
            if (doc == null) return;

            var tod = data.TimeOfDay;
            var root = doc.SelectSingleNode("//TimeOfDay");
            if (root?.Attributes != null)
            {
                tod.Time = GetFloatAttr(root, "Time", tod.Time);
                tod.SunLatitude = GetFloatAttr(root, "SunLatitude", tod.SunLatitude);
                tod.SunLongitude = GetFloatAttr(root, "SunLongitude", tod.SunLongitude);
            }

            // Sample a couple of well-known variables at their first keyframe.
            tod.SunColor = SampleColorVar(doc, "Sun color", tod.SunColor);
            tod.FogColor = SampleColorVar(doc, "Fog color", tod.FogColor);
            var intensity = SampleFloatVar(doc, "Sun intensity", tod.SunIntensity * 10000f);
            // CryEngine sun intensity is in lux-ish units; normalize into a sane Unity range.
            tod.SunIntensity = Mathf.Clamp(intensity / 10000f, 0.2f, 3f);
            tod.FogDensity = SampleFloatVar(doc, "Volumetric fog: Global density", tod.FogDensity);
        }

        static Color SampleColorVar(XmlDocument doc, string varName, Color fallback)
        {
            var node = doc.SelectSingleNode($"//Variable[@Name='{varName}']//Key");
            var value = node != null ? GetAttr(node, "Value", null) : null;
            if (value != null && CrySpace.TryParseVec3(value, out var v))
                return new Color(v.x, v.y, v.z);
            return fallback;
        }

        static float SampleFloatVar(XmlDocument doc, string varName, float fallback)
        {
            var node = doc.SelectSingleNode($"//Variable[@Name='{varName}']//Key");
            var value = node != null ? GetAttr(node, "Value", null) : null;
            float f;
            if (value != null && float.TryParse(value, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out f))
                return f;
            return fallback;
        }

        // -------------------------------------------------------- Heightmap

        static void LoadHeightmap(CryLevelData data)
        {
            // Editor exports a 16-bit raw; also check a couple of common names.
            var candidates = new[]
            {
                Path.Combine(data.RootPath, "terrain", "heightmap.raw"),
                Path.Combine(data.RootPath, "heightmap.raw"),
                Path.Combine(data.RootPath, "terrain", "land_map.raw"),
            };

            foreach (var path in candidates)
            {
                if (!File.Exists(path)) continue;
                var bytes = File.ReadAllBytes(path);
                int samples = bytes.Length / 2;
                int res = (int)Mathf.Sqrt(samples);
                if (res * res != samples) continue; // not a square 16-bit map

                data.HeightmapResolution = res;
                data.Heightmap = new ushort[samples];
                Buffer.BlockCopy(bytes, 0, data.Heightmap, 0, bytes.Length);
                Debug.Log($"[CryImporter] Loaded heightmap {res}x{res} from {path}");
                return;
            }
            Debug.LogWarning("[CryImporter] No heightmap.raw found; terrain will be flat.");
        }

        // ---------------------------------------------------------- Helpers

        static XmlDocument LoadXml(string path)
        {
            try
            {
                var doc = new XmlDocument();
                doc.Load(path);
                return doc;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CryImporter] Failed to parse {path}: {ex.Message}");
                return null;
            }
        }

        static string FindFile(string root, string name)
        {
            var direct = Path.Combine(root, name);
            if (File.Exists(direct)) return direct;
            var hits = Directory.GetFiles(root, name, SearchOption.AllDirectories);
            return hits.Length > 0 ? hits[0] : null;
        }

        static string GetAttr(XmlNode node, string name, string fallback)
            => node?.Attributes?[name]?.Value ?? fallback;

        static int GetIntAttr(XmlNode node, string name, int fallback)
            => int.TryParse(GetAttr(node, name, null), out var v) ? v : fallback;

        static float GetFloatAttr(XmlNode node, string name, float fallback)
            => float.TryParse(GetAttr(node, name, null), System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : fallback;
    }
}
