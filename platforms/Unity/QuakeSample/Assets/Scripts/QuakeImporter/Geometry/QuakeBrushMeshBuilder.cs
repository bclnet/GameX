using System.Collections.Generic;
using UnityEngine;

namespace QuakeImporter
{
    /// <summary>Result of converting one QBrush into Unity-usable mesh data.</summary>
    public class BuiltBrush
    {
        public Mesh RenderMesh;
        public Material[] RenderMaterials;
        public Mesh CollisionMesh;
        public bool IsTriggerOnly;
    }

    /// <summary>
    /// Turns a brush (a convex solid defined only by bounding planes - no vertex data) into
    /// actual mesh geometry. Quake brushes have no stored vertices, so we derive them by
    /// intersecting every triple of planes and keeping points that lie inside all the others.
    /// </summary>
    public static class QuakeBrushMeshBuilder
    {
        private const float DistEpsilon = 0.01f;
        private const float WeldEpsilonSq = 0.02f * 0.02f;

        public static BuiltBrush Build(QBrush brush, bool flipWinding, float worldScale)
        {
            int n = brush.Faces.Count;
            if (n < 4) return null; // fewer than 4 planes can't bound a finite solid

            var faceVerts = new List<Vector3>[n];
            for (int i = 0; i < n; i++) faceVerts[i] = new List<Vector3>();

            // Brute-force plane-triple intersection: O(n^3), fine for typical brush face counts (4-20).
            for (int i = 0; i < n; i++)
            for (int j = i + 1; j < n; j++)
            for (int k = j + 1; k < n; k++)
            {
                if (!TryIntersect(brush.Faces[i].Plane, brush.Faces[j].Plane, brush.Faces[k].Plane, out var p))
                    continue;

                bool inside = true;
                for (int m = 0; m < n; m++)
                {
                    if (brush.Faces[m].Plane.DistanceToPoint(p) > DistEpsilon) { inside = false; break; }
                }
                if (!inside) continue;

                AddUnique(faceVerts[i], p);
                AddUnique(faceVerts[j], p);
                AddUnique(faceVerts[k], p);
            }

            var renderVerts = new List<Vector3>();
            var renderUVs = new List<Vector2>();
            var renderTrisByTexture = new Dictionary<string, List<int>>();

            var collisionVerts = new List<Vector3>();
            var collisionTris = new List<int>();

            bool anySolidNonTrigger = false;
            bool anyTrigger = false;

            for (int f = 0; f < n; f++)
            {
                var face = brush.Faces[f];
                var verts = faceVerts[f];
                if (verts.Count < 3) continue; // degenerate face (e.g. redundant plane), skip

                verts = SortFaceVertices(verts, face.Plane.Normal);
                if (flipWinding) verts.Reverse();

                string texLower = (face.Texture ?? "wall").ToLowerInvariant();
                bool visible = QuakeSurfaceTypes.IsVisible(texLower);
                bool solid = QuakeSurfaceTypes.IsSolid(texLower);
                bool triggerOnly = QuakeSurfaceTypes.IsTriggerOnly(texLower);

                if (visible)
                {
                    if (!renderTrisByTexture.TryGetValue(face.Texture, out var triList))
                        renderTrisByTexture[face.Texture] = triList = new List<int>();

                    int baseIndex = renderVerts.Count;
                    foreach (var v in verts)
                    {
                        renderVerts.Add(v);
                        renderUVs.Add(ComputeUV(v, face, worldScale));
                    }
                    FanTriangulate(triList, baseIndex, verts.Count);
                }

                if (solid || triggerOnly)
                {
                    anySolidNonTrigger |= solid && !triggerOnly;
                    anyTrigger |= triggerOnly;

                    int baseIndex = collisionVerts.Count;
                    collisionVerts.AddRange(verts);
                    FanTriangulate(collisionTris, baseIndex, verts.Count);
                }
            }

            var result = new BuiltBrush();

            if (renderVerts.Count > 0)
            {
                var mesh = new Mesh { name = "QuakeBrushRender" };
                if (renderVerts.Count > 65000) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                mesh.SetVertices(renderVerts);
                mesh.SetUVs(0, renderUVs);
                mesh.subMeshCount = renderTrisByTexture.Count;

                var materials = new Material[renderTrisByTexture.Count];
                int subIndex = 0;
                foreach (var kvp in renderTrisByTexture)
                {
                    mesh.SetTriangles(kvp.Value, subIndex);
                    materials[subIndex] = QuakeMaterialUtil.GetMaterial(kvp.Key);
                    subIndex++;
                }

                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                mesh.RecalculateTangents();

                result.RenderMesh = mesh;
                result.RenderMaterials = materials;
            }

            if (collisionVerts.Count > 0)
            {
                var colMesh = new Mesh { name = "QuakeBrushCollision" };
                if (collisionVerts.Count > 65000) colMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                colMesh.SetVertices(collisionVerts);
                colMesh.SetTriangles(collisionTris, 0);
                colMesh.RecalculateNormals();
                colMesh.RecalculateBounds();

                result.CollisionMesh = colMesh;
                result.IsTriggerOnly = anyTrigger && !anySolidNonTrigger;
            }

            return result;
        }

        private static void FanTriangulate(List<int> tris, int baseIndex, int vertCount)
        {
            for (int t = 1; t < vertCount - 1; t++)
            {
                tris.Add(baseIndex);
                tris.Add(baseIndex + t);
                tris.Add(baseIndex + t + 1);
            }
        }

        private static void AddUnique(List<Vector3> list, Vector3 p)
        {
            foreach (var existing in list)
                if ((existing - p).sqrMagnitude < WeldEpsilonSq) return;
            list.Add(p);
        }

        private static bool TryIntersect(QPlane a, QPlane b, QPlane c, out Vector3 point)
        {
            Vector3 n1 = a.Normal, n2 = b.Normal, n3 = c.Normal;
            Vector3 n2xn3 = Vector3.Cross(n2, n3);
            float denom = Vector3.Dot(n1, n2xn3);

            if (Mathf.Abs(denom) < 1e-6f)
            {
                point = Vector3.zero;
                return false;
            }

            Vector3 n3xn1 = Vector3.Cross(n3, n1);
            Vector3 n1xn2 = Vector3.Cross(n1, n2);

            point = (a.Distance * n2xn3 + b.Distance * n3xn1 + c.Distance * n1xn2) / denom;
            return true;
        }

        /// <summary>
        /// Plane-triple intersection gives an unordered point cloud per face; this sorts
        /// them into a proper polygon winding around the face's centroid, then self-corrects
        /// the winding direction to match the face's outward normal (rather than relying on
        /// a hardcoded assumption about Unity's front-face convention).
        /// </summary>
        private static List<Vector3> SortFaceVertices(List<Vector3> verts, Vector3 normal)
        {
            Vector3 centroid = Vector3.zero;
            foreach (var v in verts) centroid += v;
            centroid /= verts.Count;

            Vector3 tangent = Vector3.Cross(normal, Vector3.up);
            if (tangent.sqrMagnitude < 0.001f) tangent = Vector3.Cross(normal, Vector3.right);
            tangent.Normalize();
            Vector3 bitangent = Vector3.Cross(normal, tangent);

            verts.Sort((p, q) =>
            {
                float angP = Mathf.Atan2(Vector3.Dot(p - centroid, bitangent), Vector3.Dot(p - centroid, tangent));
                float angQ = Mathf.Atan2(Vector3.Dot(q - centroid, bitangent), Vector3.Dot(q - centroid, tangent));
                return angP.CompareTo(angQ);
            });

            if (verts.Count >= 3)
            {
                Vector3 geomNormal = Vector3.Cross(verts[1] - verts[0], verts[2] - verts[0]);
                if (Vector3.Dot(geomNormal, normal) < 0f)
                    verts.Reverse();
            }

            return verts;
        }

        // --- Texture axis projection (id software's classic "TextureAxisFromPlane" table) ---

        private static readonly Vector3[] BaseAxisQuakeSpace =
        {
            new Vector3(0,0,1),  new Vector3(1,0,0), new Vector3(0,-1,0),  // floor
            new Vector3(0,0,-1), new Vector3(1,0,0), new Vector3(0,-1,0),  // ceiling
            new Vector3(1,0,0),  new Vector3(0,1,0), new Vector3(0,0,-1), // west wall
            new Vector3(-1,0,0), new Vector3(0,1,0), new Vector3(0,0,-1), // east wall
            new Vector3(0,1,0),  new Vector3(1,0,0), new Vector3(0,0,-1), // south wall
            new Vector3(0,-1,0), new Vector3(1,0,0), new Vector3(0,0,-1), // north wall
        };

        private static readonly Vector3[] BaseAxisUnitySpace = BuildUnitySpaceAxis();

        private static Vector3[] BuildUnitySpaceAxis()
        {
            var result = new Vector3[BaseAxisQuakeSpace.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = QuakeCoordSpace.ConvertDirection(BaseAxisQuakeSpace[i]);
            return result;
        }

        private static void TextureAxisFromNormal(Vector3 normal, out Vector3 uAxis, out Vector3 vAxis)
        {
            int best = 0;
            float bestDot = -1f;
            for (int i = 0; i < 6; i++)
            {
                float d = Vector3.Dot(normal, BaseAxisUnitySpace[i * 3]);
                if (d > bestDot) { bestDot = d; best = i; }
            }
            uAxis = BaseAxisUnitySpace[best * 3 + 1];
            vAxis = BaseAxisUnitySpace[best * 3 + 2];
        }

        /// <summary>
        /// Approximates id's per-face texture rotation: projects onto the dominant-axis
        /// UV basis, rotates that 2D basis by the face's stored angle, then applies scale/offset.
        /// Note: UV math is done in original Quake-unit magnitudes (vertex / worldScale),
        /// since texel density is defined in Quake units, not in whatever Unity meters we scaled to.
        /// </summary>
        private static Vector2 ComputeUV(Vector3 vertexUnitySpace, QFace face, float worldScale)
        {
            TextureAxisFromNormal(face.Plane.Normal, out var uAxis, out var vAxis);

            float ang = face.Rotation * Mathf.Deg2Rad;
            float c = Mathf.Cos(ang), s = Mathf.Sin(ang);
            Vector3 ru = uAxis * c - vAxis * s;
            Vector3 rv = uAxis * s + vAxis * c;

            Vector3 quakeSpaceVertex = vertexUnitySpace / (worldScale == 0f ? 1f : worldScale);

            const float defaultTextureSize = 256f; // unknown without the actual texture asset; tweak per-material if needed
            float u = Vector3.Dot(quakeSpaceVertex, ru) / face.Scale.x + face.Offset.x;
            float v = Vector3.Dot(quakeSpaceVertex, rv) / face.Scale.y + face.Offset.y;

            return new Vector2(u / defaultTextureSize, -v / defaultTextureSize);
        }
    }
}
