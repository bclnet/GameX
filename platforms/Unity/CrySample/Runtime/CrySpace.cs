using UnityEngine;

namespace CryLevelImporter
{
    /// <summary>
    /// CryEngine is Z-up, right-handed, meters. Unity is Y-up, left-handed, meters.
    /// Conversion used here: (x, y, z)cry -> (x, z, y)unity.
    /// This mirrors the axis swap most CryEngine->Unity pipelines use and keeps
    /// terrain/world coordinates intuitive (Cry Y = "north" becomes Unity Z).
    /// </summary>
    public static class CrySpace
    {
        public static Vector3 Position(float x, float y, float z)
        {
            return new Vector3(x, z, y);
        }

        public static Vector3 Position(Vector3 cry)
        {
            return new Vector3(cry.x, cry.z, cry.y);
        }

        /// <summary>
        /// Convert a CryEngine quaternion (Z-up RH) into Unity (Y-up LH).
        /// With the (x,z,y) axis swap, the handedness flip works out to
        /// negating the w component after swapping y/z.
        /// </summary>
        public static Quaternion Rotation(float x, float y, float z, float w)
        {
            return new Quaternion(-x, -z, -y, w);
        }

        /// <summary>
        /// Parse "x,y,z" CryEngine vector strings (comma separated, invariant culture).
        /// </summary>
        public static bool TryParseVec3(string s, out Vector3 v)
        {
            v = Vector3.zero;
            if (string.IsNullOrEmpty(s)) return false;
            var parts = s.Split(',');
            if (parts.Length < 3) return false;
            float x, y, z;
            var ci = System.Globalization.CultureInfo.InvariantCulture;
            var ns = System.Globalization.NumberStyles.Float;
            if (float.TryParse(parts[0], ns, ci, out x) &&
                float.TryParse(parts[1], ns, ci, out y) &&
                float.TryParse(parts[2], ns, ci, out z))
            {
                v = new Vector3(x, y, z);
                return true;
            }
            return false;
        }

        public static bool TryParseQuat(string s, out Quaternion q)
        {
            q = Quaternion.identity;
            if (string.IsNullOrEmpty(s)) return false;
            var parts = s.Split(',');
            if (parts.Length < 4) return false;
            float w, x, y, z;
            var ci = System.Globalization.CultureInfo.InvariantCulture;
            var ns = System.Globalization.NumberStyles.Float;
            // CryEngine serializes quaternions as "w,x,y,z"
            if (float.TryParse(parts[0], ns, ci, out w) &&
                float.TryParse(parts[1], ns, ci, out x) &&
                float.TryParse(parts[2], ns, ci, out y) &&
                float.TryParse(parts[3], ns, ci, out z))
            {
                q = Rotation(x, y, z, w);
                return true;
            }
            return false;
        }
    }
}
