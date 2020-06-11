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

            // record undo
            Undo.RecordObject(col.transform, "Fit Box Collider To Children");

            // get child mesh bounds
            var b = GetRecursiveMeshBounds(col.gameObject);

            // set collider local center and size
            col.center = col.transform.root.InverseTransformVector(b.center) - col.transform.position;
            col.size = b.size;
        }

        public static Bounds GetRecursiveMeshBounds(GameObject go)
        {
            var r = go.GetComponentsInChildren<Renderer>();
            if (r.Length > 0)
            {
                var b = r[0].bounds;
                for (int i = 1; i < r.Length; i++)
                {
                    b.Encapsulate(r[i].bounds);
                }
                return b;
            }
            else // TODO no renderers
            {
                return new Bounds(Vector3.one, Vector3.one);
            }
        }
    }
}
