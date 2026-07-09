using System;
using System.Collections.Generic;
using UnityEngine;

namespace CryLevelImporter
{
    /// <summary>
    /// In-memory representation of a parsed CryEngine 3 level
    /// (the extracted contents of a level folder / level.pak).
    /// </summary>
    public class CryLevelData
    {
        public string LevelName;
        public string RootPath;

        // From LevelData.xml / LevelInfo.xml
        public int HeightmapResolution = 1024;   // samples per side
        public float UnitSize = 2f;              // meters per heightmap sample
        public float TerrainMaxHeight = 1024f;   // meters at full heightmap value
        public int TerrainSizeInMeters => (int)(HeightmapResolution * UnitSize);

        public List<CryLayer> Layers = new List<CryLayer>();
        public List<CryEntity> Entities = new List<CryEntity>();
        public List<CryBrush> Brushes = new List<CryBrush>();
        public CryTimeOfDay TimeOfDay = new CryTimeOfDay();

        // Raw 16-bit heightmap, row-major, may be null if not found on disk
        public ushort[] Heightmap;
    }

    /// <summary>
    /// CryEngine object layer. Maps naturally to a container GameObject
    /// (or, in a bigger pipeline, an additively-loaded sub-scene).
    /// </summary>
    public class CryLayer
    {
        public string Name;
        public string Parent;          // layer hierarchy
        public bool VisibleByDefault = true;
        public bool ExternalStreamed;  // CryEngine "external" layers stream at runtime
        public GameObject Container;   // filled in at build time
    }

    /// <summary>
    /// A CryEngine entity: class name + transform + property table.
    /// Mirrors the &lt;Entity&gt; nodes inside Mission_*.xml.
    /// </summary>
    public class CryEntity
    {
        public string Name;
        public string EntityClass;     // e.g. "SpawnPoint", "TagPoint", "Light", "ProximityTrigger"
        public string Layer;
        public Vector3 Position;       // already converted to Unity space
        public Quaternion Rotation;
        public Vector3 Scale = Vector3.one;
        public Dictionary<string, string> Properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public bool TryGetFloat(string key, out float value)
        {
            value = 0f;
            return Properties.TryGetValue(key, out var s) &&
                   float.TryParse(s, System.Globalization.NumberStyles.Float,
                                  System.Globalization.CultureInfo.InvariantCulture, out value);
        }

        public bool TryGetColor(string key, out Color color)
        {
            color = Color.white;
            if (!Properties.TryGetValue(key, out var s)) return false;
            var parts = s.Split(',');
            if (parts.Length < 3) return false;
            float r, g, b;
            if (float.TryParse(parts[0], out r) && float.TryParse(parts[1], out g) && float.TryParse(parts[2], out b))
            {
                color = new Color(r, g, b);
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Static brush geometry reference (a .cgf placement).
    /// The mesh itself is not parsed here; resolvers can map the
    /// prefab path to real geometry or a placeholder.
    /// </summary>
    public class CryBrush
    {
        public string Name;
        public string PrefabPath;      // e.g. "objects/buildings/wall_a.cgf"
        public string Layer;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale = Vector3.one;
    }

    /// <summary>
    /// Minimal time-of-day snapshot pulled from TimeOfDay.xml curves,
    /// sampled at a single time (full ToD would keep the curves).
    /// </summary>
    public class CryTimeOfDay
    {
        public float Time = 12f;                       // hours
        public Color SunColor = new Color(1f, 0.96f, 0.88f);
        public float SunIntensity = 1.1f;
        public Color FogColor = new Color(0.6f, 0.7f, 0.85f);
        public float FogDensity = 0.002f;
        public float SunLatitude = 35f;                // degrees
        public float SunLongitude = 240f;              // degrees
    }
}
