using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using Object = UnityEngine.Object;

// Editor tool to easily paste scripts from web (automagically creates new file)
// original source: https://unitycoder.com/blog/2017/07/12/editor-plugin-paste-script-to-file/
// additional browser helper (add copy button to unity docs) https://unitycoder.com/blog/2017/07/13/browser-plugin-add-copy-button-to-unity-scripting-docs/

namespace UnityLibrary
{
    public class CopyPasteHelper : EditorWindow
    {
        // settings: output folder is set as Assets/Scripts/Paste/
        // TODO: need to support shaders folder too
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
            if (GUILayout.Button("Paste", GUILayout.Width(44)))
            {
                PasteToFile();
            }
        }

        static void PasteToFile()
        {
            // combine paths, TODO: cleanup
            var mainFolder = Application.dataPath + Path.DirectorySeparatorChar + baseFolder + Path.DirectorySeparatorChar;
            var childFolder = subFolder + Path.DirectorySeparatorChar;

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
            var clipboardString = ReadClipboard();
            if (string.IsNullOrEmpty(clipboardString.Trim()))
            {
                Debug.LogError("Nothing to paste..");
                return;
            }

            // TODO: check if its editor script, then place to editor/ folder

            var fileName = GetFileName(clipboardString);
            var fullPath = mainFolder + childFolder + fileName;

            // TODO: fix line endings (so that there wont be visual studio popup)

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
            File.WriteAllText(fullPath, clipboardString, System.Text.Encoding.Default);
            Debug.Log("<color=green>CopyPasteHelper</color> Script saved at: " + fullPath);

            AssetDatabase.Refresh();

            // FIXME: script compilation can fail here if it was bad script, then the lines below wont run?

            // show-select created asset
            Object newScript = AssetDatabase.LoadAssetAtPath("Assets" + Path.DirectorySeparatorChar + baseFolder + Path.DirectorySeparatorChar + subFolder + Path.DirectorySeparatorChar + fileName, typeof(Object));
            EditorGUIUtility.PingObject(newScript);

            // FIXME: this doesnt work, file is still there but becomes zombie (have to delete through explorer..)
            // Undo.RegisterCreatedObjectUndo(newScript, "CopyPaste Helper object creation");
        }


        // paste text to temporary texteditor and then get text
        static string ReadClipboard()
        {
            TextEditor textEditor = new TextEditor();
            textEditor.multiline = true;
            textEditor.Paste();
            return textEditor.text;
        }


        // returns class name (or shader name) and file extension .cs or .shader
        static string GetFileName(string str)
        {
            var fileName = "NewScript";
            var fileExtension = ".cs";

            // check if this looks like c# or shader
            if (str.IndexOf("using ") > -1 || str.IndexOf("class ") > -1 || str.IndexOf("namespace ") > -1)
            {
                // should be c#, try to get class name
                // NOTE: this would fail if comment lines have same "class " string
                var classSplit = str.Split(new string[] { "class " }, StringSplitOptions.None);

                var index = 0;
                var looping = true;
                while (looping == true)
                {
                    var c = classSplit[1].Substring(index++, 1);
                    // check characters until hit end of class name
                    switch (c)
                    {
                        case " ":
                        case ":":
                        case "{":
                        case "\t":
                        case ",":
                        case "\r":
                        case "\n":
                            looping = false;
                            break;
                        default:
                            break;
                    }

                    if (index >= classSplit[1].Length)
                    {
                        fileName = "NewScript"; // default..
                        Debug.LogError("Failed parsing class name..");
                        looping = false;
                        index = -1;
                    }
                }

                // we founded end of class name
                if (index > 0)
                {
                    fileName = classSplit[1].Substring(0, --index);
                } else
                {
                    Debug.LogError("Failed parsing class name..");
                }

            } else if (str.IndexOf("Shader ") > -1 || str.IndexOf("SubShader") > -1 || str.IndexOf("CGPROGRAM") > -1)
            {
                fileName = "NewShader";

                // probably its shader then, get name
                // TODO: this would fail if name starts without space: 'Shader"myshader..'
                var classSplit = str.Split(new string[] { "Shader " }, StringSplitOptions.None);

                if (classSplit.Length > 0)
                {
                    var shaderNameSplit = classSplit[1].Split('"');

                    if (shaderNameSplit.Length > 1)
                    {
                        // TODO: this takes the whole shader name string, custom/some/more/here.shader, could take name without path
                        fileName = shaderNameSplit[1];
                    } else
                    {
                        Debug.LogError("Failed parsing shader name..");
                    }
                } else
                {
                    Debug.LogError("Failed parsing shader name..");
                }

                fileExtension = ".shader";
            } else
            {
                // unknown format
                Debug.LogError("Unknown format..saving as default c#");
            }

            // just to be sure, cleanup illegal characters from filename, https://stackoverflow.com/a/13617375/5452781
            var invalidChars = Path.GetInvalidFileNameChars();
            fileName = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');

            return fileName + fileExtension;
        }
    }
}
