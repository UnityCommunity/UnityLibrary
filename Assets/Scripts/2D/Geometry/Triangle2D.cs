using UnityEngine;

namespace UnityLibrary
{
    // if you need to run collision checks within a triangular area, the easiest
    // way to do that in Unity is to use a polygon collider.  this is not very
    // performant.  this class allows you to run those collision checks without
    // the performance overhead.

    public class Triangle2D
    {
        Vector2[] vertices = new Vector2[3];

        public Triangle2D(Vector2 v1, Vector2 v2, Vector2 v3) 
        {
            Update(v1, v2, v3);
        }

        // update triangle by defining all its vertices
        public void Update(Vector2 v1, Vector2 v2, Vector2 v3) 
        {
            vertices[0] = v1;
            vertices[1] = v2;
            vertices[2] = v3;
        }

        // update triangle by redefining its origin (remaining points update relative to that)
        public void Update(Vector2 v1) 
        {
            Vector2 delta = v1 - vertices[0];
            vertices[0] = v1;
            vertices[1] += delta;
            vertices[2] += delta;
        }

        // update triangle with rotation and pivot point
        public void Update(Vector2 v1, Vector2 v2, Vector2 v3, float rotation, Vector2 pivot) 
        {
            vertices[0] = v1.Rotate(rotation, pivot);
            vertices[1] = v2.Rotate(rotation, pivot);
            vertices[2] = v3.Rotate(rotation, pivot);
        }

        float Sign(Vector2 p1, Vector2 p2, Vector2 p3) 
        {
            return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
        }

        public bool Contains(Vector2 pt, bool debug = false) 
        {
            float d1, d2, d3;
            bool has_neg, has_pos;

            d1 = Sign(pt, vertices[0], vertices[1]);
            d2 = Sign(pt, vertices[1], vertices[2]);
            d3 = Sign(pt, vertices[2], vertices[0]);

            has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

            bool contains = ! (has_neg && has_pos);

            return contains;
        }
    }
}

