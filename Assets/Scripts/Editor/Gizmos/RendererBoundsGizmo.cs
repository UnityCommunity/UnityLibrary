// attach this to mesh, draws renderer bounds when gameobject is selected
// original source: http://answers.unity.com/answers/137475/view.html

using UnityEngine;

namespace UnityLibrary
{
    public class RendererBoundsGizmo : MonoBehaviour
    {
        void OnDrawGizmos()
        {
            OnDrawGizmosSelected();
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            //center sphere
            //Gizmos.DrawSphere(transform.position, 0.1f);
            var r = transform.GetComponent<Renderer>();
            if (r != null) Gizmos.DrawWireCube(transform.position, r.bounds.size);
        }
    }
}
