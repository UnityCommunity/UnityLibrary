// display selected gameobject mesh stats (should work on prefabs,models in project window also)

using System.Collections.Generic;
using System.Linq;
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

        Dictionary<int, int> topList = new Dictionary<int, int>();
        IOrderedEnumerable<KeyValuePair<int, int>> sortedTopList;

        MeshFilter[] meshes;

        [MenuItem("Tools/UnityLibrary/GetMeshInfo")]
        public static void ShowWindow()
        {
            var window = GetWindow(typeof(GetSelectedMeshInfo));
            window.titleContent = new GUIContent("MeshInfo");
        }

        void OnGUI()
        {
            // TODO process all selected gameobjects
            var selection = Selection.activeGameObject;

            // if have selection
            if (selection == null)
            {
                EditorGUILayout.LabelField("Select gameobject from scene or hierarchy..");
            }
            else
            {
                EditorGUILayout.LabelField("Selected: " + selection.name);

                // update mesh info only if selection changed
                if (selectionChanged == true)
                {
                    selectionChanged = false;

                    // clear old top results
                    topList.Clear();

                    totalMeshes = 0;
                    totalVertices = 0;
                    totalTris = 0;

                    // check all meshes
                    meshes = selection.GetComponentsInChildren<MeshFilter>();
                    for (int i = 0, length = meshes.Length; i < length; i++)
                    {
                        int verts = meshes[i].sharedMesh.vertexCount;
                        totalVertices += verts;
                        totalTris += meshes[i].sharedMesh.triangles.Length / 3;
                        totalMeshes++;
                        topList.Add(i, verts);
                    }

                    // sort top list
                    sortedTopList = topList.OrderByDescending(x => x.Value);
                }

                // display stats
                // String.Format("{0:n0}", 9876); // No digits after the decimal point. Output: 9,876
                EditorGUILayout.LabelField("Meshes: " + $"{totalMeshes:n0}");
                EditorGUILayout.LabelField("Vertices: " + $"{totalVertices:n0}");
                EditorGUILayout.LabelField("Triangles: " + $"{totalTris:n0}");
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("TOP 20", EditorStyles.boldLabel);

                // top list
                if (meshes != null && sortedTopList != null)
                {
                    int i = 0;
                    foreach (var item in sortedTopList)
                    {
                        int percent = (int)(item.Value / (float)totalVertices * 100f);
                        EditorGUILayout.BeginHorizontal();
                        // ping button
                        if (GUILayout.Button(new GUIContent(" ", "Ping"), GUILayout.Width(16)))
                        {
                            EditorGUIUtility.PingObject(meshes[item.Key].transform);
                        }
                        EditorGUILayout.LabelField(meshes[item.Key].name + " = " + $"{item.Value:n0}" + " (" + percent + "%)");
                        GUILayout.ExpandWidth(true);
                        EditorGUILayout.EndHorizontal();

                        // show only first 20
                        if (++i > 20) break;
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
