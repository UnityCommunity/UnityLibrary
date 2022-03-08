using UnityEngine;

namespace UnityLibrary
{
    static class Vector2Extensions
    {
        public static Vector2 Round(this Vector2 vector, int to = 0) => new Vector2(vector.x.Round(to), vector.y.Round(to)); 

        public static Vector2 Rotate(this Vector2 vector, float angle, Vector2 pivot = default(Vector2)) 
        {
            Vector2 rotated = Quaternion.Euler(new Vector3(0f, 0f, angle)) * (vector - pivot);
            return rotated + pivot;
        }
    }
}
