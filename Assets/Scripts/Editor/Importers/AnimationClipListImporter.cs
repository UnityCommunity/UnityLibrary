// Checks for a .txt file with the same name as an imported .fbx file (in Assets/Models/ folder), containing a list of animation clips to add to the ModelImporter.
// .txt file should be tab-delimited with the following columns: "title", "start frame", "end frame" (and optional description, not used).
// example: 
// Take0	10	40	asdf
// Take1	50	80	wasdf..

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityLibrary.Importers
{
    public class AnimationClipListImporter : AssetPostprocessor
    {
        void OnPreprocessModel()
        {
            ModelImporter modelImporter = assetImporter as ModelImporter;
            if (modelImporter == null) return;

            string assetPath = assetImporter.assetPath;
            if (!assetPath.StartsWith("Assets/Models") || !assetPath.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase)) return;

            string txtPath = Path.ChangeExtension(assetPath, ".txt");
            if (!File.Exists(txtPath)) return;

            try
            {
                List<ModelImporterClipAnimation> clips = new List<ModelImporterClipAnimation>();
                string[] lines = File.ReadAllLines(txtPath);

                foreach (string line in lines)
                {
                    string[] parts = line.Split('\t');
                    if (parts.Length < 3) continue; // Ensure we have at least "title, start, end"

                    string title = parts[0].Trim();
                    if (!int.TryParse(parts[1], out int startFrame) || !int.TryParse(parts[2], out int endFrame))
                        continue;

                    ModelImporterClipAnimation clip = new ModelImporterClipAnimation
                    {
                        name = title,
                        firstFrame = startFrame,
                        lastFrame = endFrame,
                        loopTime = false
                    };

                    clips.Add(clip);
                }

                if (clips.Count > 0)
                {
                    modelImporter.clipAnimations = clips.ToArray();
                    Debug.Log($"Added {clips.Count} animation clips to {assetPath}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to process animation data for {assetPath}: {ex.Message}");
            }
        }
    }
}
