// editor tool to copy names of selected GameObjects to clipboard as a list (so you can paste them in Excel or others..)

using UnityEngine;
using UnityEditor;
using System.Text;
namespace UnityLibrary.Tools
{
    public class CopyGameObjectNames : EditorWindow
    {
        private string gameObjectNames = string.Empty;

        [MenuItem("Tools/Copy GameObject Names")]
        public static void ShowWindow()
        {
            GetWindow<CopyGameObjectNames>("Copy GameObject Names");
        }

        private void OnGUI()
        {
            GUILayout.Label("Copy Names of Selected GameObjects", EditorStyles.boldLabel);

            if (GUILayout.Button("Fetch Names"))
            {
                FetchNames();
            }

            GUILayout.Label("GameObject Names:", EditorStyles.label);
            gameObjectNames = EditorGUILayout.TextArea(gameObjectNames, GUILayout.Height(200));

            if (GUILayout.Button("Copy to Clipboard"))
            {
                CopyToClipboard();
            }
        }

        private void FetchNames()
        {
            StringBuilder sb = new StringBuilder();
            GameObject[] selectedObjects = Selection.gameObjects;

            foreach (GameObject obj in selectedObjects)
            {
                sb.AppendLine(obj.name);
            }

            gameObjectNames = sb.ToString();
        }

        private void CopyToClipboard()
        {
            EditorGUIUtility.systemCopyBuffer = gameObjectNames;
            Debug.Log("GameObject names copied to clipboard.");
        }
    }
}