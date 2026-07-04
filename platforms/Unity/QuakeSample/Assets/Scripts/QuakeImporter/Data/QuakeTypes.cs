using System.Collections.Generic;
using UnityEngine;

namespace QuakeImporter
{
    /// <summary>
    /// Options that control how raw .map coordinates are converted into Unity space.
    /// </summary>
    public class QuakeParseOptions
    {
        /// <summary>
        /// Quake units -> Unity meters. 1/32 is a common default (id software's levels
        /// were authored at roughly 32 units per meter-ish human scale).
        /// </summary>
        public float WorldScale = 1f / 32f;
    }

    /// <summary>
    /// A plane in "Dot(Normal, X) = Distance" form. A brush's solid volume is the
    /// intersection of the back half-spaces (Dot(Normal,X) &lt;= Distance) of all its faces.
    /// </summary>
    public struct QPlane
    {
        public Vector3 Normal;
        public float Distance;

        /// <summary>
        /// Builds a plane from three points using the same winding convention as id
        /// software's original map compiler: Normal = Cross(p0-p1, p2-p1), anchored at p1.
        /// Points must already be in the target coordinate space (i.e. call this AFTER
        /// any Quake-to-Unity axis conversion, not before).
        /// </summary>
        public static QPlane FromPoints(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            Vector3 normal = Vector3.Cross(p0 - p1, p2 - p1).normalized;
            float distance = Vector3.Dot(p1, normal);
            return new QPlane { Normal = normal, Distance = distance };
        }

        public float DistanceToPoint(Vector3 point) => Vector3.Dot(Normal, point) - Distance;
    }

    /// <summary>One textured face of a brush, as written in the .map file.</summary>
    public class QFace
    {
        public QPlane Plane;
        public string Texture = "wall";
        public Vector2 Offset;
        public float Rotation;
        public Vector2 Scale = Vector2.one;
    }

    /// <summary>A convex solid: the intersection of its faces' back half-spaces.</summary>
    public class QBrush
    {
        public List<QFace> Faces = new List<QFace>();
    }

    /// <summary>
    /// A single { ... } entity block: a classname, arbitrary key/value properties,
    /// and (for brush entities like worldspawn or func_door) a list of brushes.
    /// </summary>
    public class QEntity
    {
        public string Classname = "unknown";
        public Dictionary<string, string> Properties = new Dictionary<string, string>();
        public List<QBrush> Brushes = new List<QBrush>();
        public Vector3 Origin;

        public float GetFloat(string key, float fallback)
        {
            if (Properties.TryGetValue(key, out var raw) &&
                float.TryParse(raw, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var v))
                return v;
            return fallback;
        }

        public string GetString(string key, string fallback)
        {
            return Properties.TryGetValue(key, out var raw) ? raw : fallback;
        }
    }

    /// <summary>The fully parsed map: just a flat list of entities (worldspawn included).</summary>
    public class QMap
    {
        public List<QEntity> Entities = new List<QEntity>();
    }

    /// <summary>
    /// Quake (Z-up, right-handed) -> Unity (Y-up) coordinate conversion.
    /// This is a Y/Z axis swap plus a uniform scale. If geometry looks mirrored or
    /// inside-out after import, see QuakeRuntimeMapLoader.flipWinding before touching this.
    /// </summary>
    public static class QuakeCoordSpace
    {
        public static Vector3 ConvertPoint(Vector3 quakeRaw, QuakeParseOptions options)
        {
            return new Vector3(quakeRaw.x, quakeRaw.z, quakeRaw.y) * options.WorldScale;
        }

        /// <summary>Same axis swap, but for directions (no scale applied).</summary>
        public static Vector3 ConvertDirection(Vector3 quakeRaw)
        {
            return new Vector3(quakeRaw.x, quakeRaw.z, quakeRaw.y);
        }
    }

    /// <summary>
    /// Classifies special Quake texture names that don't behave like ordinary wall textures.
    /// Matches vanilla Quake tooling semantics (clip/skip/hint/origin/trigger).
    /// </summary>
    public static class QuakeSurfaceTypes
    {
        public static bool IsVisible(string textureLower) =>
            textureLower != "skip" && textureLower != "hint" &&
            textureLower != "origin" && textureLower != "trigger" && textureLower != "clip";

        public static bool IsSolid(string textureLower) =>
            textureLower != "skip" && textureLower != "hint" && textureLower != "origin";

        public static bool IsTriggerOnly(string textureLower) => textureLower == "trigger";
    }
}
