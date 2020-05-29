// Tries to move BoxCollider(3D)-component to match UI panel/image position, by adjusting collider pivot value

using UnityEngine;
using UnityEditor;

namespace UnityLibrary
{
    public class SetBoxColliderToUI : MonoBehaviour
    {
        [MenuItem("CONTEXT/BoxCollider/Match Position to UI")]
        static void FixPosition(MenuCommand command)
        {
            BoxCollider b = (BoxCollider)command.context;

            // record undo
            Undo.RecordObject(b.transform, "Set Box Collider To UI");

            // fix pos from Pivot
            var r = b.gameObject.GetComponent<RectTransform>();
            if (r == null) return;

            //Debug.Log("pivot "+r.pivot);

            var center = b.center;

            center.x = 0.5f - r.pivot.x;
            center.y = 0.5f - r.pivot.y;

            b.center = center;
        }
    }
}
