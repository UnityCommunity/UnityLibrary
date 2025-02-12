#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEngine;

namespace UnityEditor
{
    [CustomEditor(typeof(RectTransform), true)]
    [CanEditMultipleObjects]
    public class CustomRectTransformCopyInspector : Editor
    {
        // Unity's built-in editor
        Editor defaultEditor = null;
        RectTransform rectTransform;

        private static RectTransformData copiedData;

        void OnEnable()
        {
            // Use reflection to get the default Unity RectTransform editor
            defaultEditor = Editor.CreateEditor(targets, Type.GetType("UnityEditor.RectTransformEditor, UnityEditor"));
            rectTransform = target as RectTransform;
        }

        void OnDisable()
        {
            // Destroy the default editor to avoid memory leaks
            if (defaultEditor != null)
            {
                MethodInfo disableMethod = defaultEditor.GetType().GetMethod("OnDisable",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (disableMethod != null)
                    disableMethod.Invoke(defaultEditor, null);

                DestroyImmediate(defaultEditor);
            }
        }

        public override void OnInspectorGUI()
        {
            // Draw Unity's default RectTransform Inspector
            defaultEditor.OnInspectorGUI();

            // Add Copy and Paste buttons
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("C", GUILayout.Width(30))) // Copy
            {
                CopyRectTransform(rectTransform);
            }

            if (GUILayout.Button("P", GUILayout.Width(30))) // Paste
            {
                PasteRectTransform(rectTransform);
            }

            GUILayout.EndHorizontal();
        }

        private void CopyRectTransform(RectTransform rectTransform)
        {
            copiedData = new RectTransformData(rectTransform);
            Debug.Log("RectTransform copied!");
        }

        private void PasteRectTransform(RectTransform rectTransform)
        {
            if (copiedData == null)
            {
                Debug.LogWarning("No RectTransform data to paste!");
                return;
            }

            Undo.RecordObject(rectTransform, "Paste RectTransform");

            copiedData.ApplyTo(rectTransform);
            Debug.Log("RectTransform pasted!");

            EditorUtility.SetDirty(rectTransform);
        }

        private class RectTransformData
        {
            public Vector2 anchorMin;
            public Vector2 anchorMax;
            public Vector2 anchoredPosition;
            public Vector2 sizeDelta;
            public Vector2 pivot;
            public Quaternion rotation;

            public RectTransformData(RectTransform rectTransform)
            {
                anchorMin = rectTransform.anchorMin;
                anchorMax = rectTransform.anchorMax;
                anchoredPosition = rectTransform.anchoredPosition;
                sizeDelta = rectTransform.sizeDelta;
                pivot = rectTransform.pivot;
                rotation = rectTransform.rotation;
            }

            public void ApplyTo(RectTransform rectTransform)
            {
                rectTransform.anchorMin = anchorMin;
                rectTransform.anchorMax = anchorMax;
                rectTransform.anchoredPosition = anchoredPosition;
                rectTransform.sizeDelta = sizeDelta;
                rectTransform.pivot = pivot;
                rectTransform.rotation = rotation;
            }
        }
    }
}
#endif
