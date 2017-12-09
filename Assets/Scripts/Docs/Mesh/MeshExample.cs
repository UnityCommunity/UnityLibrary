using UnityEngine;

/// <summary>
/// Build single triangle mesh from script, with vertex colors, normals, uvs.
/// Usage: Assign this script into empty gameobject in the scene, press play. Optional: Add point light to scene for testing lights
/// </summary>

namespace UnityLibrary
{
    public class MeshExample : MonoBehaviour
    {

        void Start()
        {
            // create empty gameobject with meshrenderer and meshfilter
            var mr = gameObject.AddComponent<MeshRenderer>();
            var mf = gameObject.AddComponent<MeshFilter>();

            // build new mesh
            Mesh mesh = new Mesh();
            // assign to meshfilter
            mf.mesh = mesh;

            // create one triangle face
            // 3 vertices
            var vertices = new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(1, 0, 1) };
            // connect vertices to build triangle face
            var triangles = new int[] { 0, 1, 2 };
            // assign UV per vertex
            var uvs = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1) };
            // assign normal direction per vertex
            var normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up };
            // assign color per vertex
            var colors = new Color[] { Color.red, Color.green, Color.blue };

            // assign values to mesh
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.normals = normals;
            mesh.colors = colors;

            // if have issues of disappearing mesh, uncomment next line
            //mesh.RecalculateBounds();

            // assign sprite diffuce shader material to see vertex colors and lights
            var shader = Shader.Find("Sprites/Diffuse");
            var material = new Material(shader);
            mr.material = material;
        }

    }
}
