// editor tool to replace string from selected GameObject names

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace UnityLibrary.Tools
{
    public class ReplaceCharacterInGameObjectNames : EditorWindow
    {
        private string searchString = "|";
        private string replaceString = "@";

        [MenuItem("Tools/Replace Characters in GameObject Names")]
        public static void ShowWindow()
        {
            GetWindow<ReplaceCharacterInGameObjectNames>("Replace Characters");
        }

        private void OnGUI()
        {
            GUILayout.Label("Replace Characters in Selected GameObject Names", EditorStyles.boldLabel);

            searchString = EditorGUILayout.TextField("Search String", searchString);
            replaceString = EditorGUILayout.TextField("Replace String", replaceString);

            int selectedObjectCount = Selection.gameObjects.Length;
            GUILayout.Label($"Selected GameObjects: {selectedObjectCount}", EditorStyles.label);

            if (GUILayout.Button("Replace"))
            {
                ReplaceCharacters();
            }
        }

        private void ReplaceCharacters()
        {
            GameObject[] selectedObjects = Selection.gameObjects;

            if (selectedObjects.Length == 0)
            {
                Debug.LogWarning("No GameObjects selected.");
                return;
            }

            // Start a new undo group
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Replace Character in GameObject Names");
            int undoGroup = Undo.GetCurrentGroup();

            foreach (GameObject obj in selectedObjects)
            {
                if (obj.name.Contains(searchString))
                {
                    Undo.RecordObject(obj, "Replace Character in GameObject Name");
                    obj.name = obj.name.Replace(searchString, replaceString);
                    EditorUtility.SetDirty(obj);
                }
            }

            // End the undo group
            Undo.CollapseUndoOperations(undoGroup);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            Debug.Log($"Replaced '{searchString}' with '{replaceString}' in the names of selected GameObjects.");
        }
    }
}
