// Adjust Box Collider to fit child meshes inside
// Usage: You have empty parent transform, with child meshes inside, add box collider to parent then use this
// NOTE: Doesnt work if root transform is rotated

using UnityEngine;
using UnityEditor;

namespace UnityLibrary
{
    public class BoxColliderFitChildren : MonoBehaviour
    {
        [MenuItem("CONTEXT/BoxCollider/Fit to Children")]
        static void FixSize(MenuCommand command)
        {
            BoxCollider col = (BoxCollider)command.context;

            // Record undo for undo functionality
            Undo.RecordObject(col.transform, "Fit Box Collider To Children");

            // Get transformed bounds relative to the collider object
            Bounds localBounds = GetLocalBounds(col.transform);

            // Set collider local center and size
            col.center = localBounds.center;
            col.size = localBounds.size;
        }

        public static Bounds GetLocalBounds(Transform parent)
        {
            var renderers = parent.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return new Bounds(Vector3.zero, Vector3.zero); // No renderers

            // Initialize bounds in local space
            Bounds bounds = new Bounds(parent.InverseTransformPoint(renderers[0].bounds.center), 
                                       parent.InverseTransformVector(renderers[0].bounds.size));

            // Encapsulate all child renderers
            for (int i = 1; i < renderers.Length; i++)
            {
                var worldBounds = renderers[i].bounds;

                // Convert world bounds to local space
                Vector3 localCenter = parent.InverseTransformPoint(worldBounds.center);
                Vector3 localSize = parent.InverseTransformVector(worldBounds.size);

                bounds.Encapsulate(new Bounds(localCenter, localSize));
            }

            return bounds;
        }
    }
}
