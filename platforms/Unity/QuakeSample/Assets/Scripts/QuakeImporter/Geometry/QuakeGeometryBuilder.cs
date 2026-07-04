using UnityEngine;

namespace QuakeImporter
{
    /// <summary>Spawns one child GameObject per brush under a given entity's transform.</summary>
    public static class QuakeGeometryBuilder
    {
        public static void BuildEntityBrushes(QEntity entity, Transform parent, bool flipWinding, float worldScale)
        {
            for (int i = 0; i < entity.Brushes.Count; i++)
            {
                var built = QuakeBrushMeshBuilder.Build(entity.Brushes[i], flipWinding, worldScale);
                if (built == null) continue;

                var brushGo = new GameObject($"brush_{i}");
                brushGo.transform.SetParent(parent, false);

                if (built.RenderMesh != null)
                {
                    var mf = brushGo.AddComponent<MeshFilter>();
                    mf.sharedMesh = built.RenderMesh;
                    var mr = brushGo.AddComponent<MeshRenderer>();
                    mr.sharedMaterials = built.RenderMaterials;
                }

                if (built.CollisionMesh != null)
                {
                    var mc = brushGo.AddComponent<MeshCollider>();
                    mc.sharedMesh = built.CollisionMesh;
                    mc.convex = true; // brushes are convex by definition, so this is always valid
                    mc.isTrigger = built.IsTriggerOnly;
                }
            }
        }
    }
}
