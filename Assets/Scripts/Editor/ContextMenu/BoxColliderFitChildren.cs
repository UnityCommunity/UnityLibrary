// Adjust Box Collider to fit child meshes inside
// Usage: You have empty parent transform, with child meshes inside, add box collider to parent then use this

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

            // record undo
            Undo.RecordObject(col.transform, "Fit Box Collider To Children");

            // first reset transform rotation
            var origRot = col.transform.rotation;
            col.transform.rotation = Quaternion.identity;

            // get child mesh bounds
            var b = GetRecursiveMeshBounds(col.gameObject);

            // set collider local center and size
            col.center = col.transform.root.InverseTransformVector(b.center) - col.transform.position;

            // keep size positive
            var size = b.size;
            size.x = Mathf.Abs(size.x);
            size.y = Mathf.Abs(size.y);
            size.z = Mathf.Abs(size.z);

            col.size = b.size;

            // restore rotation
            col.transform.rotation = origRot;
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
            else // TODO no renderers?
            {
                //return new Bounds(Vector3.one, Vector3.one);
                return new Bounds();
            }
        }
    }
}
