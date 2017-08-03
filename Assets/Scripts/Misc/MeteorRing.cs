using UnityEngine;
using System.Collections;

// Usage: Attach to gameobject (enable gizmos to see Debug.DrawRay())
// reference: http://forum.unity3d.com/threads/procedural-generation-in-a-specific-shape-question.421659/
namespace UnityLibrary
{
    public class MeteorRing : MonoBehaviour
    {
        public int totalCount = 5000;
        public float ringRadius = 10;
        public float ringHeight = 1;

        void Start()
        {
            for (int i = 0; i < totalCount; i++)
            {
                // outer ring
                float angle = i * (Mathf.PI * 2) / totalCount;
                var x = Mathf.Sin(angle) * ringRadius;
                var y = Mathf.Cos(angle) * ringRadius;
                var pos = new Vector3(x, 0, y);

                // spread within outer ring
                pos += Random.insideUnitSphere * ringHeight;

                // draw
                Debug.DrawRay(pos, Vector3.up * 0.05f, Color.yellow, 100);
            }
        }
    }
}
