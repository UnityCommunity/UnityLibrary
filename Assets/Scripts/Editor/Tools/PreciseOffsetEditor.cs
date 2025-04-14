// gameobject position fiddling tool (when dragging x,y,z axist in transform panel is too fast)

using UnityEngine;
using UnityEditor;

namespace UnityLibrary.SceneTools
{
    public class PreciseOffsetEditor : EditorWindow
    {
        private const string SpeedMultiplierKey = "UnityLibrary_PreciseOffset_SpeedMultiplier";

        private GameObject selectedObject;
        private Vector3 offsetSliderValues = Vector3.zero;
        private float speedMultiplier = 0.01f;

        private Vector3 originalPosition;
        private GameObject lastSelectedObject;

        [MenuItem("Tools/UnityLibrary/Precise Model Offset")]
        public static void ShowWindow()
        {
            var win = GetWindow<PreciseOffsetEditor>("Precise Model Offset");
            win.minSize = new Vector2(300, 220);
            win.maxSize = new Vector2(300, 220);
        }

        private void OnEnable()
        {
            speedMultiplier = EditorPrefs.GetFloat(SpeedMultiplierKey, 0.01f);
        }

        private void OnDisable()
        {
            EditorPrefs.SetFloat(SpeedMultiplierKey, speedMultiplier);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Precise Model Offset", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            int selectedCount = Selection.gameObjects.Length;
            selectedObject = Selection.activeGameObject;

            if (selectedCount == 0)
            {
                EditorGUILayout.HelpBox("No GameObject selected.", MessageType.Warning);
                return;
            }

            if (selectedCount == 1)
            {
                EditorGUILayout.LabelField("Selected: " + selectedObject.name);
            }
            else
            {
                EditorGUILayout.LabelField("Selected: (multiple)");
            }

            if (selectedObject != lastSelectedObject)
            {
                originalPosition = selectedObject.transform.position;
                offsetSliderValues = Vector3.zero;
                lastSelectedObject = selectedObject;
            }

            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();
            speedMultiplier = EditorGUILayout.Slider("Speed Multiplier", speedMultiplier, 0.001f, 1f);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetFloat(SpeedMultiplierKey, speedMultiplier);
            }

            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();

            // Store old values to detect changes
            Vector3 oldOffset = offsetSliderValues;

            offsetSliderValues.x = EditorGUILayout.Slider("X Offset", offsetSliderValues.x, -100f, 100f);
            offsetSliderValues.y = EditorGUILayout.Slider("Y Offset", offsetSliderValues.y, -100f, 100f);
            offsetSliderValues.z = EditorGUILayout.Slider("Z Offset", offsetSliderValues.z, -100f, 100f);

            Vector3 delta = offsetSliderValues - oldOffset;

            if (delta != Vector3.zero)
            {
                Undo.RecordObject(selectedObject.transform, "Precise Offset");

                Vector3 newPosition = selectedObject.transform.position;

                if (!Mathf.Approximately(delta.x, 0f))
                    newPosition.x = originalPosition.x + offsetSliderValues.x * speedMultiplier;

                if (!Mathf.Approximately(delta.y, 0f))
                    newPosition.y = originalPosition.y + offsetSliderValues.y * speedMultiplier;

                if (!Mathf.Approximately(delta.z, 0f))
                    newPosition.z = originalPosition.z + offsetSliderValues.z * speedMultiplier;

                selectedObject.transform.position = newPosition;

                EditorUtility.SetDirty(selectedObject);
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Reset Offset"))
            {
                offsetSliderValues = Vector3.zero;
                if (selectedObject != null)
                {
                    selectedObject.transform.position = originalPosition;
                }
            }
        }
    }
}
