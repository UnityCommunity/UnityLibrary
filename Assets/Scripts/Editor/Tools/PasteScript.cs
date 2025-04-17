// new version of the https://github.com/UnityCommunity/UnityLibrary/blob/master/Assets/Scripts/Editor/Tools/CopyPasteHelper.cs
// creates c# script from clipboard when click button

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

namespace UnityLibrary.Tools
{
    public class PasteScript : EditorWindow
    {
        private string statusMessage = "";
        private string lastCreatedScriptPath = "";

        [MenuItem("Tools/UnityLibrary/Clipboard To C# Script")]
        public static void ShowWindow()
        {
            GetWindow<PasteScript>("Clipboard to Script");
        }

        void OnGUI()
        {
            if (GUILayout.Button("Create Script from Clipboard"))
            {
                TryCreateScriptFromClipboard();
            }

            GUILayout.Space(10);

            // Draw clickable HelpBox
            EditorGUILayout.HelpBox(statusMessage, MessageType.Info);

            Rect helpBoxRect = GUILayoutUtility.GetLastRect();
            if (!string.IsNullOrEmpty(lastCreatedScriptPath) && Event.current.type == EventType.MouseDown && helpBoxRect.Contains(Event.current.mousePosition))
            {
                var asset = AssetDatabase.LoadAssetAtPath<Object>(lastCreatedScriptPath);
                if (asset != null)
                {
                    EditorGUIUtility.PingObject(asset);
                }
                Event.current.Use(); // Consume the click
            }
        }

        void TryCreateScriptFromClipboard()
        {
            string clipboard = EditorGUIUtility.systemCopyBuffer;

            if (IsProbablyCSharp(clipboard))
            {
                string folderPath = "Assets/Scripts/Generated";
                Directory.CreateDirectory(folderPath);

                string className = GetClassName(clipboard) ?? "GeneratedScript";
                string path = AssetDatabase.GenerateUniqueAssetPath($"{folderPath}/{className}.cs");

                File.WriteAllText(path, clipboard);
                AssetDatabase.Refresh();

                statusMessage = $"Script created: {path}";
                lastCreatedScriptPath = path;
            }
            else
            {
                statusMessage = "Clipboard does not contain valid C# code.";
                lastCreatedScriptPath = "";
            }
        }

        bool IsProbablyCSharp(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;

            // Basic heuristic checks
            return Regex.IsMatch(text, @"\b(class|struct|interface|using|namespace)\b");
        }

        string GetClassName(string text)
        {
            Match match = Regex.Match(text, @"\bclass\s+(\w+)");
            return match.Success ? match.Groups[1].Value : null;
        }
    }
}
