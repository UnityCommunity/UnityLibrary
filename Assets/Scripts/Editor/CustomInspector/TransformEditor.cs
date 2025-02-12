// source https://gist.github.com/unitycoder/e5e6384f087639c0d9edc93aa3820468

using UnityEngine;
using UnityEditor;

namespace OddTales.Framework.Core.EditorExtension
{
    /// <summary>
    /// Custom inspector for Transform component. Using only DrawDefaultInspector would give different display.
    /// Script based on Unity wiki implementation : https://wiki.unity3d.com/index.php/TransformInspector
    /// Buttons to reset, copy, paste Transform values. 
    /// Context menu to round/truncate values, hide/show tools.
    /// </summary>
    [CanEditMultipleObjects, CustomEditor(typeof(Transform))]
    public class TransformEditor : Editor
    {
        private const float FIELD_WIDTH = 212.0f;
        private const bool WIDE_MODE = true;

        private const float POSITION_MAX = 100000.0f;

        private static GUIContent positionGUIContent = new GUIContent(LocalString("Position"));
        private static GUIContent rotationGUIContent = new GUIContent(LocalString("Rotation"));
        private static GUIContent scaleGUIContent = new GUIContent(LocalString("Scale"));

        private static string positionWarningText = LocalString("Due to floating-point precision limitations, it is recommended to bring the world coordinates of the GameObject within a smaller range.");

        private SerializedProperty positionProperty, rotationProperty, scaleProperty;

        private static Vector3? positionClipboard = null;
        private static Quaternion? rotationClipboard = null;
        private static Vector3? scaleClipboard = null;

        private const string SHOW_TOOLS_KEY = "TransformEditor_ShowTools";
        private const string SHOW_RESET_TOOLS_KEY = "TransformEditor_ShowResetTools";
        private const string SHOW_PASTE_TOOLS_KEY = "TransformEditor_ShowPasteTools";
        private const string SHOW_ADVANCED_PASTE_TOOLS_KEY = "TransformEditor_ShowAdvancedPasteTools";
        private const string SHOW_CLIPBOARD_INFORMATIONS_KEY = "TransformEditor_ShowClipboardInformations";
        private const string SHOW_SHORTCUTS_KEY = "TransformEditor_ShowHelpbox";


#if UNITY_2017_3_OR_NEWER
        private static System.Reflection.MethodInfo getLocalizedStringMethod;
#endif


        /// <summary> Get translated Transform label </summary>
        private static string LocalString(string text)
        {
#if UNITY_2017_3_OR_NEWER
            // Since Unity 2017.3, static class LocalizationDatabase is no longer public. Need to use reflection to access it.
            if (getLocalizedStringMethod == null)
            {
                System.Reflection.Assembly assembly = typeof(UnityEditor.EditorWindow).Assembly;
                System.Type localizationDatabaseType = assembly.GetType("UnityEditor.LocalizationDatabase");

                getLocalizedStringMethod = localizationDatabaseType.GetMethod("GetLocalizedString");
            }

            return (string)getLocalizedStringMethod.Invoke(null, new object[] { text });
#else
			return LocalizationDatabase.GetLocalizedString(text);
#endif
        }

        public void OnEnable()
        {
            positionProperty = serializedObject.FindProperty("m_LocalPosition");
            rotationProperty = serializedObject.FindProperty("m_LocalRotation");
            scaleProperty = serializedObject.FindProperty("m_LocalScale");

            // Init options
            if (!EditorPrefs.HasKey(SHOW_TOOLS_KEY)) EditorPrefs.SetBool(SHOW_TOOLS_KEY, true);
            if (!EditorPrefs.HasKey(SHOW_RESET_TOOLS_KEY)) EditorPrefs.SetBool(SHOW_RESET_TOOLS_KEY, true);
            if (!EditorPrefs.HasKey(SHOW_PASTE_TOOLS_KEY)) EditorPrefs.SetBool(SHOW_PASTE_TOOLS_KEY, true);
            if (!EditorPrefs.HasKey(SHOW_ADVANCED_PASTE_TOOLS_KEY)) EditorPrefs.SetBool(SHOW_ADVANCED_PASTE_TOOLS_KEY, true);
            if (!EditorPrefs.HasKey(SHOW_CLIPBOARD_INFORMATIONS_KEY)) EditorPrefs.SetBool(SHOW_CLIPBOARD_INFORMATIONS_KEY, true);
            if (!EditorPrefs.HasKey(SHOW_SHORTCUTS_KEY)) EditorPrefs.SetBool(SHOW_SHORTCUTS_KEY, true);
        }


        public override void OnInspectorGUI()
        {
            Rect beginRect = GUILayoutUtility.GetRect(0, 0);

            EditorGUIUtility.wideMode = TransformEditor.WIDE_MODE;
            EditorGUIUtility.labelWidth = EditorGUIUtility.currentViewWidth - TransformEditor.FIELD_WIDTH; // align field to right of inspector

            serializedObject.Update();

            EditorGUIUtility.labelWidth = 60; // To allow float fields to expand when inspector width is increased

            // Position GUI
            EditorGUILayout.BeginHorizontal();
            PositionPropertyField(positionProperty, positionGUIContent); // Note : Can't add generic menu if we use EditorGUILayout.PropertyField instead
            if (EditorPrefs.GetBool(SHOW_TOOLS_KEY) && EditorPrefs.GetBool(SHOW_RESET_TOOLS_KEY))
            {
                if (GUILayout.Button("Reset", GUILayout.Width(50)))
                {
                    Undo.RecordObjects(targets, "Reset Positions");
                    for (int i = 0; i < targets.Length; i++)
                    {
                        ((Transform)targets[i]).localPosition = Vector3.zero;
                    }
                    GUI.FocusControl(null);
                }
            }
            EditorGUILayout.EndHorizontal();

            // Rotation GUI
            EditorGUILayout.BeginHorizontal();
            RotationPropertyField(rotationProperty, rotationGUIContent); // Note : Can't add generic menu if we use EditorGUILayout.PropertyField instead
            if (EditorPrefs.GetBool(SHOW_TOOLS_KEY) && EditorPrefs.GetBool(SHOW_RESET_TOOLS_KEY))
            {
                if (GUILayout.Button("Reset", GUILayout.Width(50)))
                {
                    Undo.RecordObjects(targets, "Reset Rotations");
                    for (int i = 0; i < targets.Length; i++)
                    {
                        TransformUtils.SetInspectorRotation(((Transform)targets[i]), Vector3.zero);
                    }
                    GUI.FocusControl(null);
                }

            }
            EditorGUILayout.EndHorizontal();

            // Scale GUI
            EditorGUILayout.BeginHorizontal();
            ScalePropertyField(scaleProperty, scaleGUIContent); // Note : Can't add generic menu if we use EditorGUILayout.PropertyField instead
            if (EditorPrefs.GetBool(SHOW_TOOLS_KEY) && EditorPrefs.GetBool(SHOW_RESET_TOOLS_KEY))
            {
                if (GUILayout.Button("Reset", GUILayout.Width(50)))
                {
                    Undo.RecordObjects(targets, "Reset Scales");
                    for (int i = 0; i < targets.Length; i++)
                    {
                        ((Transform)targets[i]).localScale = Vector3.one;
                    }
                    GUI.FocusControl(null);
                }
            }
            EditorGUILayout.EndHorizontal();


            if (!ValidatePosition(((Transform)target).position)) EditorGUILayout.HelpBox(positionWarningText, MessageType.Warning); // Display floating-point warning message if values are too high

            if (EditorPrefs.GetBool(SHOW_TOOLS_KEY))
            {
                // Paste Tools GUI
                if (EditorPrefs.GetBool(SHOW_PASTE_TOOLS_KEY))
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Copy"))
                    {
                        positionClipboard = ((Transform)target).localPosition;
                        rotationClipboard = ((Transform)target).localRotation;
                        scaleClipboard = ((Transform)target).localScale;
                    }

                    if (!positionClipboard.HasValue) EditorGUI.BeginDisabledGroup(true);
                    if (GUILayout.Button("Paste"))
                    {
                        Undo.RecordObjects(targets, "Paste Clipboard Values");
                        for (int i = 0; i < targets.Length; i++)
                        {
                            ((Transform)targets[i]).localPosition = positionClipboard.Value;
                            ((Transform)targets[i]).localRotation = rotationClipboard.Value;
                            ((Transform)targets[i]).localScale = scaleClipboard.Value;
                        }
                        GUI.FocusControl(null);
                    }
                    if (!positionClipboard.HasValue) EditorGUI.EndDisabledGroup();
                    GUILayout.EndHorizontal();
                }

                // Advanced Paste Tools GUI
                if (EditorPrefs.GetBool(SHOW_ADVANCED_PASTE_TOOLS_KEY))
                {
                    GUILayout.BeginHorizontal();

                    if (!positionClipboard.HasValue) EditorGUI.BeginDisabledGroup(true);
                    if (GUILayout.Button("Paste position"))
                    {
                        Undo.RecordObjects(targets, "Paste Position Clipboard Value");
                        for (int i = 0; i < targets.Length; i++)
                        {
                            ((Transform)targets[i]).localPosition = positionClipboard.Value;
                        }
                        GUI.FocusControl(null);
                    }

                    if (GUILayout.Button("Paste rotation"))
                    {
                        Undo.RecordObjects(targets, "Paste Rotation Clipboard Value");
                        for (int i = 0; i < targets.Length; i++)
                        {
                            ((Transform)targets[i]).rotation = rotationClipboard.Value;
                        }
                        GUI.FocusControl(null);
                    }

                    if (GUILayout.Button("Paste scale"))
                    {
                        Undo.RecordObjects(targets, "Paste Scale Clipboard Value");
                        for (int i = 0; i < targets.Length; i++)
                        {
                            ((Transform)targets[i]).localScale = scaleClipboard.Value;
                        }
                        GUI.FocusControl(null);
                    }
                    if (!positionClipboard.HasValue) EditorGUI.EndDisabledGroup();

                    GUILayout.EndHorizontal();
                }

                // Clipboard GUI
                if (EditorPrefs.GetBool(SHOW_CLIPBOARD_INFORMATIONS_KEY))
                {
                    if (positionClipboard.HasValue && rotationClipboard.HasValue && scaleClipboard.HasValue)
                    {

                        GUIStyle helpboxStyle = new GUIStyle(EditorStyles.helpBox);
                        helpboxStyle.richText = true;

                        EditorGUILayout.TextArea("Clipboard values :\n" +
                        "Position : " + positionClipboard.Value.ToString("f2") + "\n" +
                        "Rotation : " + rotationClipboard.Value.ToString("f2") + "\n" +
                        "Scale : " + scaleClipboard.Value.ToString("f2"), helpboxStyle);
                    }
                }


                // Shortcuts GUI - Related to InspectorShortcuts.cs https://github.com/VoxelBoy/Useful-Unity-Scripts/blob/master/InspectorShortcuts.cs
                if (EditorPrefs.GetBool(SHOW_SHORTCUTS_KEY))
                {
                    EditorGUILayout.HelpBox("Inspector shortcuts :\n" +
                    "Toggle inspector lock : Ctrl + Shift + L\n" +
                    "Toggle inspector mode : Ctrl + Shift + D", MessageType.None);
                }
            }
            Rect endRect = GUILayoutUtility.GetLastRect();
            endRect.y += endRect.height;


            #region Context Menu
            Rect componentRect = new Rect(beginRect.x, beginRect.y, beginRect.width, endRect.y - beginRect.y);
            //EditorGUI.DrawRect(componentRect, Color.green); // Debug : display GenericMenu zone

            Event currentEvent = Event.current;

            if (currentEvent.type == EventType.ContextClick)
            {
                if (componentRect.Contains(currentEvent.mousePosition))
                {
                    GUI.FocusControl(null);

                    GenericMenu menu = new GenericMenu();

                    menu.AddItem(new GUIContent("Display/Tools"), EditorPrefs.GetBool(SHOW_TOOLS_KEY), ToggleOption, SHOW_TOOLS_KEY);
                    menu.AddSeparator("Display/");
                    menu.AddItem(new GUIContent("Display/Reset Tools"), EditorPrefs.GetBool(SHOW_RESET_TOOLS_KEY), ToggleOption, SHOW_RESET_TOOLS_KEY);
                    menu.AddItem(new GUIContent("Display/Paste Tools"), EditorPrefs.GetBool(SHOW_PASTE_TOOLS_KEY), ToggleOption, SHOW_PASTE_TOOLS_KEY);
                    menu.AddItem(new GUIContent("Display/Advanced Paste Tools"), EditorPrefs.GetBool(SHOW_ADVANCED_PASTE_TOOLS_KEY), ToggleOption, SHOW_ADVANCED_PASTE_TOOLS_KEY);
                    menu.AddItem(new GUIContent("Display/Clipboard informations"), EditorPrefs.GetBool(SHOW_CLIPBOARD_INFORMATIONS_KEY), ToggleOption, SHOW_CLIPBOARD_INFORMATIONS_KEY);
                    menu.AddItem(new GUIContent("Display/Shortcuts informations"), EditorPrefs.GetBool(SHOW_SHORTCUTS_KEY), ToggleOption, SHOW_SHORTCUTS_KEY);

                    // Round menu
                    menu.AddItem(new GUIContent("Round/Three Decimals"), false, Round, 3);
                    menu.AddItem(new GUIContent("Round/Two Decimals"), false, Round, 2);
                    menu.AddItem(new GUIContent("Round/One Decimal"), false, Round, 1);
                    menu.AddItem(new GUIContent("Round/Integer"), false, Round, 0);

                    // Truncate menu
                    menu.AddItem(new GUIContent("Truncate/Three Decimals"), false, Truncate, 3);
                    menu.AddItem(new GUIContent("Truncate/Two Decimals"), false, Truncate, 2);
                    menu.AddItem(new GUIContent("Truncate/One Decimal"), false, Truncate, 1);
                    menu.AddItem(new GUIContent("Truncate/Integer"), false, Truncate, 0);

                    menu.ShowAsContext();
                    currentEvent.Use();
                }
            }
            #endregion

            serializedObject.ApplyModifiedProperties();
        }


        private bool ValidatePosition(Vector3 position)
        {
            if (Mathf.Abs(position.x) > POSITION_MAX) return false;
            if (Mathf.Abs(position.y) > POSITION_MAX) return false;
            if (Mathf.Abs(position.z) > POSITION_MAX) return false;
            return true;
        }

        private void PositionPropertyField(SerializedProperty positionProperty, GUIContent content)
        {
            Transform transform = (Transform)targets[0];
            Vector3 localPosition = transform.localPosition;
            for (int i = 0; i < targets.Length; i++)
            {
                if (!localPosition.Equals(((Transform)targets[i]).localPosition))
                {
                    EditorGUI.showMixedValue = true;
                    break;
                }
            }

            EditorGUI.BeginChangeCheck();
            Vector3 newLocalPosition = EditorGUILayout.Vector3Field(content, localPosition);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObjects(targets, "Position Changed");
                for (int i = 0; i < targets.Length; i++)
                {
                    ((Transform)targets[i]).localPosition = newLocalPosition;
                }
                positionProperty.serializedObject.SetIsDifferentCacheDirty();
            }
            EditorGUI.showMixedValue = false;
        }

        private void RotationPropertyField(SerializedProperty rotationProperty, GUIContent content)
        {
            Transform transform = (Transform)targets[0];
            Vector3 localRotation = TransformUtils.GetInspectorRotation(transform);


            for (int i = 0; i < targets.Length; i++)
            {
                if (!localRotation.Equals(TransformUtils.GetInspectorRotation((Transform)targets[i])))
                {
                    EditorGUI.showMixedValue = true;
                    break;
                }
            }

            EditorGUI.BeginChangeCheck();
            Vector3 eulerAngles = EditorGUILayout.Vector3Field(content, localRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObjects(targets, "Rotation Changed");
                for (int i = 0; i < targets.Length; i++)
                {
                    //((Transform)targets[i]).localEulerAngles = eulerAngles;
                    TransformUtils.SetInspectorRotation(((Transform)targets[i]), eulerAngles);
                }
                rotationProperty.serializedObject.SetIsDifferentCacheDirty();
            }
            EditorGUI.showMixedValue = false;
        }

        private void ScalePropertyField(SerializedProperty scaleProperty, GUIContent content)
        {
            Transform transform = (Transform)targets[0];
            Vector3 localScale = transform.localScale;
            for (int i = 0; i < targets.Length; i++)
            {
                if (!localScale.Equals(((Transform)targets[i]).localScale))
                {
                    EditorGUI.showMixedValue = true;
                    break;
                }
            }

            EditorGUI.BeginChangeCheck();
            Vector3 newLocalScale = EditorGUILayout.Vector3Field(content, localScale);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObjects(targets, "Scale Changed");
                for (int i = 0; i < targets.Length; i++)
                {
                    ((Transform)targets[i]).localScale = newLocalScale;
                }
                scaleProperty.serializedObject.SetIsDifferentCacheDirty();
            }
            EditorGUI.showMixedValue = false;
        }


        #region Generic Menu Callbacks
        private void ToggleOption(object obj)
        {
            EditorPrefs.SetBool(obj.ToString(), !EditorPrefs.GetBool(obj.ToString()));
        }

        /// <summary> Round all values of the Transform to a given number of decimals </summary>
        private void Round(object objNumberOfDecimals)
        {
            int numberOfDecimals = (int)objNumberOfDecimals;

            Undo.RecordObjects(targets, "Round to " + numberOfDecimals + " decimals");
            for (int i = 0; i < targets.Length; i++)
            {
                ((Transform)targets[i]).localPosition = RoundVector(((Transform)targets[i]).localPosition, numberOfDecimals);
                ((Transform)targets[i]).localEulerAngles = RoundVector(((Transform)targets[i]).localEulerAngles, numberOfDecimals);
                ((Transform)targets[i]).localScale = RoundVector(((Transform)targets[i]).localScale, numberOfDecimals);
            }
        }

        /// <summary> Round all components of a Vector3 </summary>
        private Vector3 RoundVector(Vector3 vector, int numberOfDecimals)
        {
            vector.x = Mathf.Round(vector.x * Mathf.Pow(10.0f, (float)numberOfDecimals)) / Mathf.Pow(10.0f, (float)numberOfDecimals);
            vector.y = Mathf.Round(vector.y * Mathf.Pow(10.0f, (float)numberOfDecimals)) / Mathf.Pow(10.0f, (float)numberOfDecimals);
            vector.z = Mathf.Round(vector.z * Mathf.Pow(10.0f, (float)numberOfDecimals)) / Mathf.Pow(10.0f, (float)numberOfDecimals);
            return vector;
        }

        /// <summary> Truncate all values of the Transform to a given number of decimals </summary>
        private void Truncate(object objNumberOfDecimals)
        {
            int numberOfDecimals = (int)objNumberOfDecimals;

            Undo.RecordObjects(targets, "Truncate to " + numberOfDecimals + " decimals");
            for (int i = 0; i < targets.Length; i++)
            {
                ((Transform)targets[i]).localPosition = TruncateVector(((Transform)targets[i]).localPosition, numberOfDecimals);
                ((Transform)targets[i]).localEulerAngles = TruncateVector(((Transform)targets[i]).localEulerAngles, numberOfDecimals);
                ((Transform)targets[i]).localScale = TruncateVector(((Transform)targets[i]).localScale, numberOfDecimals);
            }
        }

        /// <summary> Truncate all components of a Vector3 </summary>
        private Vector3 TruncateVector(Vector3 vector, int numberOfDecimals)
        {
            vector.x = Mathf.Floor(vector.x * Mathf.Pow(10.0f, (float)numberOfDecimals)) / Mathf.Pow(10.0f, (float)numberOfDecimals);
            vector.y = Mathf.Floor(vector.y * Mathf.Pow(10.0f, (float)numberOfDecimals)) / Mathf.Pow(10.0f, (float)numberOfDecimals);
            vector.z = Mathf.Floor(vector.z * Mathf.Pow(10.0f, (float)numberOfDecimals)) / Mathf.Pow(10.0f, (float)numberOfDecimals);
            return vector;
        }
        #endregion
    }
}
