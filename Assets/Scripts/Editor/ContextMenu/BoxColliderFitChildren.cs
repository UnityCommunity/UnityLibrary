using UnityEngine;
using UnityEditor;

namespace UnityLibrary
{
    public class BoxColliderFitChildren : MonoBehaviour
    {
        [MenuItem("CONTEXT/BoxCollider/Fit to Children")]
        static void FitColliderToChildren(MenuCommand command)
        {
            BoxCollider col = (BoxCollider)command.context;

            // Record undo
            Undo.RecordObject(col.transform, "Fit Box Collider To Children");

            // Get world-space bounds of all child meshes
            var worldBounds = GetRecursiveMeshBounds(col.gameObject);

            if (worldBounds.size == Vector3.zero)
            {
                Debug.LogWarning("No valid meshes found to fit the BoxCollider.");
                return;
            }

            // Convert world-space center to local space
            Vector3 localCenter = col.transform.InverseTransformPoint(worldBounds.center);

            // Convert world-space size to local space
            Vector3 localSize = col.transform.InverseTransformVector(worldBounds.size);

            // Ensure size is positive
            localSize = new Vector3(Mathf.Abs(localSize.x), Mathf.Abs(localSize.y), Mathf.Abs(localSize.z));

            // Fix potential center flipping
            if (Vector3.Dot(col.transform.right, Vector3.right) < 0)
            {
                localCenter.x = -localCenter.x;
            }
            if (Vector3.Dot(col.transform.up, Vector3.up) < 0)
            {
                localCenter.y = -localCenter.y;
            }
            if (Vector3.Dot(col.transform.forward, Vector3.forward) < 0)
            {
                localCenter.z = -localCenter.z;
            }

            // Apply to collider
            col.center = localCenter;
            col.size = localSize;
        }

        public static Bounds GetRecursiveMeshBounds(GameObject go)
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();

            if (renderers.Length == 0)
                return new Bounds();

            // Start with the first renderer’s bounds in world space
            Bounds worldBounds = renderers[0].bounds;

            for (int i = 1; i < renderers.Length; i++)
            {
                worldBounds.Encapsulate(renderers[i].bounds);
            }

            return worldBounds;
        }
    }
}
