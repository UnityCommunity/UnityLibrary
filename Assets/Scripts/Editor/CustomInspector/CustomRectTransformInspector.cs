// source https://gist.github.com/GieziJo/f80bcb24c4caa68ebfb204148ccd4b18
// ===============================
// AUTHOR          : J. Giezendanner
// CREATE DATE     : 12.03.2020
// MODIFIED DATE   : 
// PURPOSE         : Adds helper functions to the RectTransform to align the rect to the anchors and vise-versa
// SPECIAL NOTES   : Sources for certain informations:
//                   Display anchors gizmos:
//                   https://forum.unity.com/threads/recttransform-custom-editor-ontop-of-unity-recttransform-custom-editor.455925/
//                   Draw default inspector:
//                   https://forum.unity.com/threads/extending-instead-of-replacing-built-in-inspectors.407612/
// ===============================
// Change History:
//==================================

#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor.SceneManagement;
using UnityEngine;


namespace UnityEditor
{
    [CustomEditor(typeof(RectTransform), true)]
    [CanEditMultipleObjects]
    public class CustomRectTransformInspector : Editor
    {
        //Unity's built-in editor
        Editor defaultEditor = null;
        RectTransform rectTransform;

        bool rect2Anchors_foldout = false;
        bool anchors2Rect_foldout = false;
        bool rect2Anchors__previousState = false;
        bool anchors2Rect_previousState = false;

        private bool playerPrefsChecked = false;

        void OnEnable()
        {
            //When this inspector is created, also create the built-in inspector
            defaultEditor = Editor.CreateEditor(targets, Type.GetType("UnityEditor.RectTransformEditor, UnityEditor"));
            rectTransform = target as RectTransform;
        }

        void OnDisable()
        {
            //When OnDisable is called, the default editor we created should be destroyed to avoid memory leakage.
            //Also, make sure to call any required methods like OnDisable

            if (defaultEditor != null)
            {
                MethodInfo disableMethod = defaultEditor.GetType().GetMethod("OnDisable",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (disableMethod != null)
                    disableMethod.Invoke(defaultEditor, null);
                DestroyImmediate(defaultEditor);
            }
        }

        void checkPlayerPrefs()
        {
            rect2Anchors_foldout = PlayerPrefs.GetInt("giezi_tools_rect2Anchors_foldout_bool", 0) != 0;
            anchors2Rect_foldout = PlayerPrefs.GetInt("giezi_tools_anchors2Rect_foldout_bool", 0) != 0;

            rect2Anchors__previousState = rect2Anchors_foldout;
            anchors2Rect_previousState = anchors2Rect_foldout;
        }


        public override void OnInspectorGUI()
        {
            if (!playerPrefsChecked)
            {
                checkPlayerPrefs();
                playerPrefsChecked = true;
            }

            defaultEditor.OnInspectorGUI();


            if (rectTransform.parent != null)
            {
                var centerButtonStyle = new GUIStyle(GUI.skin.button);
                centerButtonStyle.fontStyle = FontStyle.Bold;

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Helper Functions", EditorStyles.boldLabel);

                rect2Anchors_foldout = EditorGUILayout.Foldout(rect2Anchors_foldout, "Set Rect to Anchors");

                if (rect2Anchors_foldout)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.BeginVertical();
                    if (GUILayout.Button("Top Left"))
                        setRectValue("topLeft");
                    if (GUILayout.Button("Left"))
                        setRectValue("left");
                    if (GUILayout.Button("Bottom Left"))
                        setRectValue("bottomLeft");
                    GUILayout.EndVertical();
                    GUILayout.BeginVertical();
                    if (GUILayout.Button("Top"))
                        setRectValue("top");
                    if (GUILayout.Button("All", centerButtonStyle))
                        setRectValue("all");
                    if (GUILayout.Button("Bottom"))
                        setRectValue("bottom");
                    GUILayout.EndVertical();
                    GUILayout.BeginVertical();
                    if (GUILayout.Button("Top Right"))
                        setRectValue("topRight");
                    if (GUILayout.Button("Right"))
                        setRectValue("right");
                    if (GUILayout.Button("Bottom Right"))
                        setRectValue("bottomRight");
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                }

                anchors2Rect_foldout = EditorGUILayout.Foldout(anchors2Rect_foldout, "Set Anchors to Rect");

                if (anchors2Rect_foldout)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.BeginVertical();
                    if (GUILayout.Button("Top Left"))
                        setAnchorsToRect("topLeft");
                    if (GUILayout.Button("Left"))
                        setAnchorsToRect("left");
                    if (GUILayout.Button("Bottom Left"))
                        setAnchorsToRect("bottomLeft");
                    GUILayout.EndVertical();
                    GUILayout.BeginVertical();
                    if (GUILayout.Button("Top"))
                        setAnchorsToRect("top");
                    if (GUILayout.Button("All", centerButtonStyle))
                        setAnchorsToRect("all");
                    if (GUILayout.Button("Bottom"))
                        setAnchorsToRect("bottom");
                    GUILayout.EndVertical();
                    GUILayout.BeginVertical();
                    if (GUILayout.Button("Top Right"))
                        setAnchorsToRect("topRight");
                    if (GUILayout.Button("Right"))
                        setAnchorsToRect("right");
                    if (GUILayout.Button("Bottom Right"))
                        setAnchorsToRect("bottomRight");
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                }


                if (rect2Anchors_foldout != rect2Anchors__previousState)
                {
                    rect2Anchors__previousState = rect2Anchors_foldout;
                    PlayerPrefs.SetInt("giezi_tools_rect2Anchors_foldout_bool", rect2Anchors_foldout ? 1 : 0);
                }

                if (anchors2Rect_foldout != anchors2Rect_previousState)
                {
                    anchors2Rect_previousState = anchors2Rect_foldout;
                    PlayerPrefs.SetInt("giezi_tools_anchors2Rect_foldout_bool", anchors2Rect_foldout ? 1 : 0);
                }
            }
        }


        private void OnSceneGUI()
        {
            MethodInfo onSceneGUI_Method = defaultEditor.GetType()
                .GetMethod("OnSceneGUI", BindingFlags.NonPublic | BindingFlags.Instance);
            onSceneGUI_Method.Invoke(defaultEditor, null);
        }


        private void setAnchorsToRect(string field)
        {
            Vector2 anchorMax = new Vector2();
            Vector2 anchorMin = new Vector2();
            var parent = rectTransform.parent;
            anchorMin.x = rectTransform.offsetMin.x / parent.GetComponent<RectTransform>().rect.size.x;
            anchorMin.y = rectTransform.offsetMin.y / parent.GetComponent<RectTransform>().rect.size.y;
            anchorMax.x = rectTransform.offsetMax.x / parent.GetComponent<RectTransform>().rect.size.x;
            anchorMax.y = rectTransform.offsetMax.y / parent.GetComponent<RectTransform>().rect.size.y;


            switch (field)
            {
                case "topLeft":
                    anchorMax.x = 0;
                    rectTransform.anchorMax += anchorMax;
                    rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, 0);

                    anchorMin.y = 0;
                    rectTransform.anchorMin += anchorMin;
                    rectTransform.offsetMin = new Vector2(0, rectTransform.offsetMin.y);
                    break;
                case "top":
                    anchorMax.x = 0;
                    rectTransform.anchorMax += anchorMax;
                    rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, 0);
                    break;
                case "topRight":
                    rectTransform.anchorMax += anchorMax;
                    rectTransform.offsetMax = Vector2.zero;
                    break;
                case "bottomLeft":
                    rectTransform.anchorMin += anchorMin;
                    rectTransform.offsetMin = Vector2.zero;
                    break;
                case "bottom":
                    anchorMin.x = 0;
                    rectTransform.anchorMin += anchorMin;
                    rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, 0);
                    break;
                case "bottomRight":
                    anchorMin.x = 0;
                    rectTransform.anchorMin += anchorMin;
                    rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, 0);
                    anchorMax.y = 0;
                    rectTransform.anchorMax += anchorMax;
                    rectTransform.offsetMax = new Vector2(0, rectTransform.offsetMax.y);
                    break;
                case "left":
                    anchorMin.y = 0;
                    rectTransform.anchorMin += anchorMin;
                    rectTransform.offsetMin = new Vector2(0, rectTransform.offsetMin.y);
                    break;
                case "right":
                    anchorMax.y = 0;
                    rectTransform.anchorMax += anchorMax;
                    rectTransform.offsetMax = new Vector2(0, rectTransform.offsetMax.y);
                    break;
                case "all":
                    rectTransform.anchorMax += anchorMax;
                    rectTransform.anchorMin += anchorMin;
                    rectTransform.offsetMin = Vector2.zero;
                    rectTransform.offsetMax = Vector2.zero;
                    break;
            }

            handleChange();
        }


        private void setRectValue(string field)
        {
            switch (field)
            {
                case "topLeft":
                    rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, 0);
                    rectTransform.offsetMin = new Vector2(0, rectTransform.offsetMin.y);
                    break;
                case "top":
                    rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, 0);
                    break;
                case "topRight":
                    rectTransform.offsetMax = Vector2.zero;
                    break;
                case "bottomLeft":
                    rectTransform.offsetMin = Vector2.zero;
                    break;
                case "bottom":
                    rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, 0);
                    break;
                case "bottomRight":
                    rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, 0);
                    rectTransform.offsetMax = new Vector2(0, rectTransform.offsetMax.y);
                    break;
                case "left":
                    rectTransform.offsetMin = new Vector2(0, rectTransform.offsetMin.y);
                    break;
                case "right":
                    rectTransform.offsetMax = new Vector2(0, rectTransform.offsetMax.y);
                    break;
                case "all":
                    rectTransform.offsetMin = new Vector2(0, 0);
                    rectTransform.offsetMax = new Vector2(0, 0);
                    break;
            }

            handleChange();
        }

        private void handleChange()
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }
    }
}
#endif
