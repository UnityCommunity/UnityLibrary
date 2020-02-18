// display selected gameobject mesh stats (should work on prefabs,models in project window also)

using UnityEditor;
using UnityEngine;

namespace UnityLibrary
{
    public class GetSelectedMeshInfo : EditorWindow
    {
        [MenuItem("Tools/UnityLibrary/GetMeshInfo")]
        public static void ShowWindow()
        {
            var window = GetWindow(typeof(GetSelectedMeshInfo));
            window.titleContent = new GUIContent("MeshInfo");
        }

        void OnGUI()
        {
            var selection = Selection.activeGameObject;

            if (selection != null)
            {
                EditorGUILayout.LabelField("Selected: " + selection.name);

                int totalMeshes = 0;
                int totalVertices = 0;
                int totalTris = 0;

                // get all meshes
                var meshes = selection.GetComponentsInChildren<MeshFilter>();
                for (int i = 0, length = meshes.Length; i < length; i++)
                {
                    totalVertices += meshes[i].sharedMesh.vertexCount;
                    totalTris += meshes[i].sharedMesh.triangles.Length / 3;
                    totalMeshes++;
                }

                // display stats
                EditorGUILayout.LabelField("Meshes: " + totalMeshes);
                EditorGUILayout.LabelField("Vertices: " + totalVertices);
                EditorGUILayout.LabelField("Triangles: " + totalTris);
            }

        }

        void OnSelectionChange()
        {
            // force redraw window
            Repaint();
        }
    }
}
