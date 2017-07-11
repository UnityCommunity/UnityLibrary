using UnityEditor;
using UnityEngine;
using System.IO;
// Editor tool to easily paste scripts from web (automagically creates new file)
// original source: https://unitycoder.com/blog/2017/07/12/editor-plugin-paste-script-to-file/

namespace UnityLibrary
{
    public class CopyPasteHelper : EditorWindow
    {
        // settings: output folder is set as Assets/Scripts/Paste/
        static string baseFolder = "Scripts";
        static string subFolder = "Paste";

        [MenuItem("Window/UnityLibrary/CopyPasteHelper")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            var window = EditorWindow.GetWindow(typeof(CopyPasteHelper));
            window.titleContent = new GUIContent("CopyPasteHelper");
            window.minSize = new Vector2(64, 24);
        }

        // mainloop
        void OnGUI()
        {
            if (GUILayout.Button("C#", GUILayout.Width(44)))
            {
                CreateScript();
            }
        }

        static void CreateScript()
        {
            // combine paths, TODO: cleanup
            var fileName = "NewScript.cs"; // default filename if cannot parse from text
            var mainFolder = Application.dataPath + Path.DirectorySeparatorChar + baseFolder + Path.DirectorySeparatorChar;
            var childFolder = subFolder + Path.DirectorySeparatorChar;
            var fullPath = mainFolder + childFolder + fileName;

            // check folders and create them if missing
            if (Directory.Exists(mainFolder) == false)
            {
                Debug.Log("Creating missing folder: " + mainFolder);
                AssetDatabase.CreateFolder("Assets", baseFolder);
            }
            if (Directory.Exists(mainFolder + childFolder) == false)
            {
                Debug.Log("Creating missing folder: " + mainFolder + childFolder);
                AssetDatabase.CreateFolder("Assets" + Path.DirectorySeparatorChar + baseFolder, subFolder);
            }

            // get clipboard text
            string clipBoardContents = ReadClipBoard();
            if (string.IsNullOrEmpty(clipBoardContents.Trim()))
            {
                Debug.LogError("Nothing to paste..");
                return;
            }

            // TODO: validate and detect string type (c# or shader)
            // TODO: get script class name or shader name from string

            // confirm overwrite dialog
            if (File.Exists(fullPath) == true)
            {
                // TODO: add option to autorename file (and class name)
                if (EditorUtility.DisplayDialog("Copy Paste Helper", "Replace existing file: " + fullPath, "Replace", "Cancel") == false)
                {
                    return;
                }
            }

            // save to file
            File.WriteAllText(fullPath, clipBoardContents, System.Text.Encoding.Default);
            Debug.Log("<color=green>CopyPasteHelper</color> Script saved at: " + fullPath);

            AssetDatabase.Refresh();

            // show-select created asset
            Object newScript = AssetDatabase.LoadAssetAtPath("Assets" + Path.DirectorySeparatorChar + baseFolder + Path.DirectorySeparatorChar + subFolder + Path.DirectorySeparatorChar + fileName, typeof(Object));
            EditorGUIUtility.PingObject(newScript);
        }


        // paste text to temporary texteditor and then get text
        public static string ReadClipBoard()
        {
            TextEditor textEditor = new TextEditor();
            textEditor.multiline = true;
            textEditor.Paste();
            return textEditor.text;
        }
    }
}
