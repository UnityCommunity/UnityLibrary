// NOTE updates whole selected folder (not just the ones that print warnings)

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityLibrary.Editor.Tools
{
    public class LegacyAssetUpgrader : EditorWindow
    {
        private DefaultAsset rootFolder;
        private bool includeSubfolders = true;
        private Vector2 scroll;
        private readonly List<string> log = new();

        [MenuItem("Tools/UnityLibrary/Batch/Upgrade Legacy Serialized Assets")]
        public static void ShowWindow()
        {
            GetWindow<LegacyAssetUpgrader>("Asset Upgrader");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Legacy Serialized Asset Upgrader", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "Re-saves assets and their .meta files using the current Unity serialization version.\n\n" +
                "This fixes warnings such as:\n" +
                "\"TextureImporter object at version 7, below the supported minimum (10)\"\n" +
                "\"GameObject object at version 5, below the supported minimum (6)\"",
                MessageType.Info);

            EditorGUILayout.Space();

            rootFolder = (DefaultAsset)EditorGUILayout.ObjectField("Root Folder", rootFolder, typeof(DefaultAsset), false);
            includeSubfolders = EditorGUILayout.Toggle("Include Subfolders", includeSubfolders);

            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(rootFolder == null))
            {
                if (GUILayout.Button("Upgrade All Assets In Folder", GUILayout.Height(35)))
                    UpgradeFolder();
            }

            if (GUILayout.Button("Upgrade Selected Assets", GUILayout.Height(28)))
                UpgradeSelected();

            EditorGUILayout.Space();

            if (log.Count > 0)
            {
                EditorGUILayout.LabelField("Log", EditorStyles.boldLabel);

                scroll = EditorGUILayout.BeginScrollView(scroll);

                foreach (string line in log)
                    EditorGUILayout.LabelField(line, EditorStyles.wordWrappedLabel);

                EditorGUILayout.EndScrollView();

                if (GUILayout.Button("Clear Log"))
                    log.Clear();
            }
        }

        private void UpgradeFolder()
        {
            string rootPath = AssetDatabase.GetAssetPath(rootFolder);

            if (!AssetDatabase.IsValidFolder(rootPath))
            {
                Debug.LogError("Selected object is not a folder.");
                return;
            }

            string[] guids = AssetDatabase.FindAssets("", new[] { rootPath });
            List<string> paths = new();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (AssetDatabase.IsValidFolder(path))
                    continue;

                if (!includeSubfolders)
                {
                    string directory = Path.GetDirectoryName(path)?.Replace("\\", "/");

                    if (!string.Equals(directory, rootPath, StringComparison.OrdinalIgnoreCase))
                        continue;
                }

                paths.Add(path);
            }

            UpgradeAssets(paths);
        }

        private void UpgradeSelected()
        {
            List<string> paths = new();

            foreach (UnityEngine.Object obj in Selection.objects)
            {
                string path = AssetDatabase.GetAssetPath(obj);

                if (string.IsNullOrEmpty(path))
                    continue;

                if (AssetDatabase.IsValidFolder(path))
                {
                    string[] guids = AssetDatabase.FindAssets("", new[] { path });

                    foreach (string guid in guids)
                    {
                        string assetPath = AssetDatabase.GUIDToAssetPath(guid);

                        if (!AssetDatabase.IsValidFolder(assetPath))
                            paths.Add(assetPath);
                    }
                }
                else
                {
                    paths.Add(path);
                }
            }

            UpgradeAssets(paths);
        }

        private void UpgradeAssets(List<string> paths)
        {
            if (paths.Count == 0)
            {
                Debug.LogWarning("No assets found.");
                return;
            }

            log.Clear();

            try
            {
                for (int i = 0; i < paths.Count; i++)
                {
                    string path = paths[i];

                    if (EditorUtility.DisplayCancelableProgressBar(
                        "Upgrading Legacy Assets",
                        $"{i + 1}/{paths.Count}\n{path}",
                        (float)i / paths.Count))
                    {
                        log.Add("Cancelled.");
                        break;
                    }

                    try
                    {
                        UpgradeAsset(path);
                        log.Add($"OK: {path}");
                    }
                    catch (Exception e)
                    {
                        log.Add($"FAILED: {path} - {e.Message}");
                        Debug.LogException(e);
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Legacy asset upgrade finished. Processed {paths.Count} assets.");
        }

        private void UpgradeAsset(string path)
        {
            string extension = Path.GetExtension(path).ToLowerInvariant();

            if (extension == ".prefab")
            {
                ResavePrefab(path);
                Reserialize(path);
                return;
            }

            if (extension == ".unity")
            {
                Reserialize(path);
                return;
            }

            AssetImporter importer = AssetImporter.GetAtPath(path);

            if (importer != null)
            {
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
            }

            Reserialize(path);
        }

        private static void ResavePrefab(string path)
        {
            GameObject root = null;

            try
            {
                root = PrefabUtility.LoadPrefabContents(path);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                if (root != null)
                    PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static void Reserialize(string path)
        {
            AssetDatabase.ForceReserializeAssets(
                new[] { path },
                ForceReserializeAssetsOptions.ReserializeAssetsAndMetadata);
        }
    }
}
