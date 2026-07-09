using UnityEngine;

namespace CryLevelImporter
{
    /// <summary>
    /// Builds a Unity Terrain at runtime from the parsed CryEngine heightmap.
    /// CryEngine heightmaps are row-major, north-up; the sample loop flips
    /// axes so the terrain lines up with the (x,z,y) entity conversion.
    /// </summary>
    public static class CryTerrainBuilder
    {
        public static Terrain Build(CryLevelData data, Transform parent)
        {
            var terrainData = new TerrainData();

            // Unity wants heightmap resolution as 2^n + 1; resample if needed.
            int srcRes = data.HeightmapResolution;
            int dstRes = Mathf.ClosestPowerOfTwo(srcRes) + 1;

            terrainData.heightmapResolution = dstRes;
            terrainData.size = new Vector3(
                data.TerrainSizeInMeters,
                data.TerrainMaxHeight,
                data.TerrainSizeInMeters);

            if (data.Heightmap != null)
            {
                var heights = new float[dstRes, dstRes];
                float scale = (srcRes - 1) / (float)(dstRes - 1);

                for (int y = 0; y < dstRes; y++)
                {
                    for (int x = 0; x < dstRes; x++)
                    {
                        // Bilinear sample of the source heightmap
                        float sx = x * scale;
                        float sy = y * scale;
                        int x0 = Mathf.Min((int)sx, srcRes - 2);
                        int y0 = Mathf.Min((int)sy, srcRes - 2);
                        float fx = sx - x0;
                        float fy = sy - y0;

                        float h00 = data.Heightmap[y0 * srcRes + x0]       / 65535f;
                        float h10 = data.Heightmap[y0 * srcRes + x0 + 1]   / 65535f;
                        float h01 = data.Heightmap[(y0 + 1) * srcRes + x0] / 65535f;
                        float h11 = data.Heightmap[(y0 + 1) * srcRes + x0 + 1] / 65535f;

                        // Unity indexes heights[z, x]; Cry rows run along its Y axis,
                        // which we've mapped to Unity Z, so rows map straight to z.
                        heights[y, x] = Mathf.Lerp(
                            Mathf.Lerp(h00, h10, fx),
                            Mathf.Lerp(h01, h11, fx), fy);
                    }
                }
                terrainData.SetHeights(0, 0, heights);
            }

            var go = Terrain.CreateTerrainGameObject(terrainData);
            go.name = "Terrain (CryEngine)";
            go.transform.SetParent(parent, false);
            go.transform.position = Vector3.zero;

            var terrain = go.GetComponent<Terrain>();
            terrain.allowAutoConnect = true;
            terrain.drawInstanced = true;
            return terrain;
        }
    }
}
