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
                    // Add children if it's a GameObject and find sprites in it
                    if (obj is GameObject gameObject)
                    {
                        AddChildrenAndSprites(gameObject);
                    }
                    // If it's a Texture2D, load its Sprites (if any)
                    else if (obj is Texture2D texture)
                    {
                        AddTextureSprites(texture);
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

        private void AddChildrenAndSprites(GameObject obj)
        {
            // Add the GameObject itself
            allObjects.Add(obj);

            // Check if the GameObject has a SpriteRenderer and add the Sprite
            var spriteRenderer = obj.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                allObjects.Add(spriteRenderer.sprite); // Add the sprite separately
            }

            // Recursively add children and their sprites
            foreach (Transform child in obj.transform)
            {
                AddChildrenAndSprites(child.gameObject);
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

                // Add the asset itself
                if (asset != null)
                {
                    allObjects.Add(asset);

                    // If it's a Texture2D, also add any Sprites inside it
                    if (asset is Texture2D texture)
                    {
                        AddTextureSprites(texture);
                    }
                }
            }
        }

        private void AddTextureSprites(Texture2D texture)
        {
            // Check if the texture contains any sprites (e.g., from a sprite sheet)
            string path = AssetDatabase.GetAssetPath(texture);
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);

            foreach (Object asset in assets)
            {
                if (asset is Sprite sprite)
                {
                    allObjects.Add(sprite); // Add each sprite separately
                }
            }
        }

        private void SelectObjects(List<Object> objects)
        {
            Selection.objects = objects.ToArray();
        }
    }
}
