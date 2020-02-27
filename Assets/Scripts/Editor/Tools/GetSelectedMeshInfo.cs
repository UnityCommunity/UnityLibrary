// display selected gameobject mesh stats (should work on prefabs,models in project window also)

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityLibrary
{
    public class GetSelectedMeshInfo : EditorWindow
    {
        bool selectionChanged = false;

        int totalMeshes = 0;
        int totalVertices = 0;
        int totalTris = 0;

        List<int> top10 = new List<int>();
        MeshFilter[] meshes;

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

                // update mesh info only if selection changed
                if (selectionChanged == true)
                {
                    top10.Clear();

                    totalMeshes = 0;
                    totalVertices = 0;
                    totalTris = 0;

                    // get all meshes
                    meshes = selection.GetComponentsInChildren<MeshFilter>();
                    for (int i = 0, length = meshes.Length; i < length; i++)
                    {
                        int verts = meshes[i].sharedMesh.vertexCount;
                        totalVertices += verts;
                        totalTris += meshes[i].sharedMesh.triangles.Length / 3;
                        totalMeshes++;
                        top10.Add(verts);
                    }
                    selectionChanged = false;

                    // sort top10
                    top10.Sort();
                }

                // display stats
                EditorGUILayout.LabelField("Meshes: " + totalMeshes);
                EditorGUILayout.LabelField("Vertices: " + totalVertices);
                EditorGUILayout.LabelField("Triangles: " + totalTris);
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("TOP 10", EditorStyles.boldLabel);

                // top 10
                if (meshes != null)
                {
                    // start from last index
                    int from = meshes.Length;
                    // until 10 meshes or if less than 10
                    int to = meshes.Length - Mathf.Min(10, from);
                    for (int i = from-1; i >= to; i--)
                    {
                        int percent = (int)(top10[i] / (float)totalVertices * 100f);
                        EditorGUILayout.LabelField(meshes[i].name + " = " + top10[i] + " (" + percent + "%)");
                    }
                }
            }

        }

        void OnSelectionChange()
        {
            selectionChanged = true;
            // force redraw window
            Repaint();
        }
    }
}
