// select multiple files or folders from Project window, then can filter by type and select all objects of that type

using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace UnityLibrary
{
    public class SelectAssetsByType : EditorWindow
    {
        private Dictionary<System.Type, List<Object>> groupedObjects;
        private List<Object> allObjects;

        [MenuItem("Tools/UnityLibrary/Select Assets By Type")]
        public static void ShowWindow()
        {
            GetWindow<SelectAssetsByType>("Select Assets By Type");
        }

        private void OnEnable()
        {
            // Automatically refresh whenever selection changes
            Selection.selectionChanged += RefreshSelection;
            RefreshSelection();
        }

        private void OnDisable()
        {
            // Unsubscribe from selection change event
            Selection.selectionChanged -= RefreshSelection;
        }

        private void OnGUI()
        {
            if (groupedObjects != null)
            {
                foreach (var group in groupedObjects)
                {
                    if (group.Value.Count > 0)
                    {
                        // Show type and number of objects
                        if (GUILayout.Button($"{group.Value.Count} {group.Key.Name}(s)"))
                        {
                            // Select objects of this type in the editor
                            SelectObjects(group.Value);
                        }
                    }
                }
            }
        }

        private void RefreshSelection()
        {
            groupedObjects = new Dictionary<System.Type, List<Object>>();
            allObjects = new List<Object>();

            // Get selected objects and their children
            var selectedObjects = Selection.objects;
            foreach (var obj in selectedObjects)
            {
                allObjects.Add(obj);

                // If the object is a folder, include assets inside the folder recursively
                if (AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(obj)))
                {
                    AddFolderContents(AssetDatabase.GetAssetPath(obj));
                }
                else
                {
                    // Add children if it's a GameObject
                    if (obj is GameObject gameObject)
                    {
                        AddChildren(gameObject);
                    }
                }
            }

            // Group by type
            foreach (var obj in allObjects)
            {
                var type = obj.GetType();
                if (!groupedObjects.ContainsKey(type))
                {
                    groupedObjects[type] = new List<Object>();
                }
                groupedObjects[type].Add(obj);
            }

            // Refresh the window UI
            Repaint();
        }

        private void AddChildren(GameObject obj)
        {
            foreach (Transform child in obj.transform)
            {
                allObjects.Add(child.gameObject);
                AddChildren(child.gameObject);
            }
        }

        private void AddFolderContents(string folderPath)
        {
            // Get all assets in the folder and subfolders
            string[] guids = AssetDatabase.FindAssets("", new[] { folderPath });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (asset != null)
                {
                    allObjects.Add(asset);
                }
            }
        }

        private void SelectObjects(List<Object> objects)
        {
            Selection.objects = objects.ToArray();
        }
    }
}
