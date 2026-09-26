// displays the currently held keys in a small window, useful for recording tutorials or presentations.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityLibrary.EditorTools
{
    public class KeystrokeVisualizer : EditorWindow
    {
        private readonly List<KeyCode> heldKeys = new List<KeyCode>();

        private bool ctrlHeld;
        private bool shiftHeld;
        private bool altHeld;
        private bool cmdHeld;

        private string displayedKeys = "";
        private double lastReleaseTime;

        private float displayDuration = 2.0f;
        private int fontSize = 30;
        private bool showMouseClicks = true;
        private bool showSettings;

        private GUIStyle keyStyle;

        private bool HasHeldKeys => ctrlHeld || shiftHeld || altHeld || cmdHeld || heldKeys.Count > 0;

        [MenuItem("Tools/UnityLibrary/Keystroke Visualizer")]
        public static void OpenWindow()
        {
            KeystrokeVisualizer window = GetWindow<KeystrokeVisualizer>(true, "Keystroke Visualizer", true);
            window.minSize = new Vector2(360, 120);
            window.ShowUtility();
        }

        private void OnEnable()
        {
            SceneView.beforeSceneGui += OnSceneGUI;
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            SceneView.beforeSceneGui -= OnSceneGUI;
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            if (HasHeldKeys && !(EditorWindow.focusedWindow is SceneView))
            {
                ClearHeldKeys();
                lastReleaseTime = EditorApplication.timeSinceStartup;
            }

            if (HasHeldKeys || EditorApplication.timeSinceStartup - lastReleaseTime < displayDuration)
                Repaint();
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            Event e = Event.current;

            if (e == null)
                return;

            if (e.type == EventType.KeyDown)
            {
                HandleKeyDown(e);
            }
            else if (e.type == EventType.KeyUp)
            {
                HandleKeyUp(e);
            }
            else if (showMouseClicks && e.type == EventType.MouseDown)
            {
                HandleMouseDown(e);
            }
        }

        private void HandleKeyDown(Event e)
        {
            KeyCode key = e.keyCode;

            if (key == KeyCode.None)
                return;

            UpdateModifiers(e);

            if (!IsModifier(key) && !heldKeys.Contains(key))
                heldKeys.Add(key);

            UpdateHeldDisplay();
        }

        private void HandleKeyUp(Event e)
        {
            KeyCode key = e.keyCode;

            if (key == KeyCode.None)
                return;

            UpdateModifiers(e);

            if (IsModifier(key))
            {
                ReleaseModifier(key);
            }
            else
            {
                heldKeys.Remove(key);
            }

            if (HasHeldKeys)
            {
                UpdateHeldDisplay();
            }
            else
            {
                lastReleaseTime = EditorApplication.timeSinceStartup;
            }

            Repaint();
        }

        private void UpdateModifiers(Event e)
        {
            ctrlHeld = e.control;
            shiftHeld = e.shift;
            altHeld = e.alt;
            cmdHeld = e.command;
        }

        private void ReleaseModifier(KeyCode key)
        {
            switch (key)
            {
                case KeyCode.LeftControl:
                case KeyCode.RightControl:
                    ctrlHeld = false;
                    break;

                case KeyCode.LeftShift:
                case KeyCode.RightShift:
                    shiftHeld = false;
                    break;

                case KeyCode.LeftAlt:
                case KeyCode.RightAlt:
                    altHeld = false;
                    break;

                case KeyCode.LeftCommand:
                case KeyCode.RightCommand:
                    cmdHeld = false;
                    break;
            }
        }

        private void UpdateHeldDisplay()
        {
            List<string> parts = new List<string>();

            if (ctrlHeld)
                parts.Add("Ctrl");

            if (cmdHeld)
                parts.Add("Cmd");

            if (altHeld)
                parts.Add("Alt");

            if (shiftHeld)
                parts.Add("Shift");

            foreach (KeyCode key in heldKeys)
                parts.Add(FormatKey(key));

            if (parts.Count > 0)
                displayedKeys = string.Join(" + ", parts);

            Repaint();
        }

        private void HandleMouseDown(Event e)
        {
            string mouseButton = "";

            switch (e.button)
            {
                case 0: mouseButton = "Left Click"; break;
                case 1: mouseButton = "Right Click"; break;
                case 2: mouseButton = "Middle Click"; break;
                default: return;
            }

            UpdateModifiers(e);

            List<string> parts = new List<string>();

            if (ctrlHeld)
                parts.Add("Ctrl");

            if (cmdHeld)
                parts.Add("Cmd");

            if (altHeld)
                parts.Add("Alt");

            if (shiftHeld)
                parts.Add("Shift");

            parts.Add(mouseButton);

            displayedKeys = string.Join(" + ", parts);
            lastReleaseTime = EditorApplication.timeSinceStartup;

            Repaint();
        }

        private void ClearHeldKeys()
        {
            heldKeys.Clear();
            ctrlHeld = false;
            shiftHeld = false;
            altHeld = false;
            cmdHeld = false;
        }

        private static bool IsModifier(KeyCode key)
        {
            return key == KeyCode.LeftShift || key == KeyCode.RightShift ||
                   key == KeyCode.LeftControl || key == KeyCode.RightControl ||
                   key == KeyCode.LeftAlt || key == KeyCode.RightAlt ||
                   key == KeyCode.LeftCommand || key == KeyCode.RightCommand;
        }

        private static string FormatKey(KeyCode key)
        {
            string name = key.ToString();

            if (name.StartsWith("Alpha"))
                return name.Substring(5);

            switch (key)
            {
                case KeyCode.Return: return "Enter";
                case KeyCode.Escape: return "Esc";
                case KeyCode.Space: return "Space";
                case KeyCode.Backspace: return "Backspace";
                case KeyCode.Delete: return "Delete";
                case KeyCode.LeftArrow: return "Left";
                case KeyCode.RightArrow: return "Right";
                case KeyCode.UpArrow: return "Up";
                case KeyCode.DownArrow: return "Down";
                case KeyCode.KeypadEnter: return "Numpad Enter";
                default: return name;
            }
        }

        private void OnGUI()
        {
            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), new Color(0.07f, 0.07f, 0.09f, 1f));

            if (keyStyle == null)
            {
                keyStyle = new GUIStyle(EditorStyles.boldLabel);
                keyStyle.alignment = TextAnchor.MiddleCenter;
                keyStyle.wordWrap = true;
                keyStyle.normal.textColor = Color.white;
            }

            keyStyle.fontSize = fontSize;

            double elapsed = EditorApplication.timeSinceStartup - lastReleaseTime;

            float alpha = HasHeldKeys ? 1f : Mathf.Clamp01((float)(displayDuration - elapsed) / 0.35f);

            float contentHeight = showSettings ? position.height - 115f : position.height - 30f;
            contentHeight = Mathf.Max(30f, contentHeight);

            if (!string.IsNullOrEmpty(displayedKeys) && alpha > 0f)
            {
                Color oldColor = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, alpha);
                GUI.Label(new Rect(12, 8, position.width - 24, contentHeight), displayedKeys, keyStyle);
                GUI.color = oldColor;
            }

            GUILayout.FlexibleSpace();

            showSettings = EditorGUILayout.Foldout(showSettings, "Settings", true);

            if (showSettings)
            {
                fontSize = EditorGUILayout.IntSlider("Font Size", fontSize, 16, 60);
                displayDuration = EditorGUILayout.Slider("Duration", displayDuration, 0.5f, 5f);
                showMouseClicks = EditorGUILayout.Toggle("Mouse Clicks", showMouseClicks);
            }

            GUILayout.Space(8);
        }
    }
}
