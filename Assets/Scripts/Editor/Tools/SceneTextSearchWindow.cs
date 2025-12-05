using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UnityLibrary.EditorTools
{
    public class SceneTextSearchWindow : EditorWindow
    {
        [Serializable]
        private class SearchResult
        {
            public Component component;
            public string text;
        }

        private string searchTerm = string.Empty;
        private string previousSearchTerm = string.Empty;
        private bool caseSensitive = false;
        private bool automaticSearch = true;

        private readonly List<SearchResult> results = new List<SearchResult>();
        private readonly HashSet<Component> seenComponents = new HashSet<Component>();
        private Vector2 scrollPos;

        [MenuItem("Tools/UnityLibrary/Scene Text Search")]
        public static void Open()
        {
            var window = GetWindow<SceneTextSearchWindow>("Scene Text Search");
            window.minSize = new Vector2(600, 300);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Search text in loaded scenes", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Search term", GUILayout.Width(80));

            string newSearchTerm = EditorGUILayout.TextField(searchTerm);

            if (GUILayout.Button("Search", GUILayout.Width(80)))
            {
                DoSearch();
            }

            EditorGUILayout.EndHorizontal();

            if (newSearchTerm != searchTerm)
            {
                searchTerm = newSearchTerm;

                if (automaticSearch)
                {
                    if (!string.IsNullOrEmpty(searchTerm) && searchTerm.Length > 1)
                    {
                        DoSearch();
                    }
                    else
                    {
                        results.Clear();
                        seenComponents.Clear();
                    }
                }

                previousSearchTerm = searchTerm;
            }

            EditorGUILayout.BeginHorizontal();
            caseSensitive = EditorGUILayout.Toggle("Case sensitive", caseSensitive);
            automaticSearch = EditorGUILayout.Toggle("Automatic search", automaticSearch);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Results: {results.Count}", EditorStyles.boldLabel);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            foreach (var r in results)
            {
                if (r.component == null)
                    continue;

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.ObjectField(r.component, typeof(Component), true, GUILayout.Width(220));

                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.TextField(Truncate(r.text, 200));
                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        private void DoSearch()
        {
            results.Clear();
            seenComponents.Clear();

            if (string.IsNullOrEmpty(searchTerm))
                return;

            string term = caseSensitive ? searchTerm : searchTerm.ToLowerInvariant();

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // UnityEngine.UI.Text
            SearchComponents<Text>(t => t.text, term);

            // TMP UGUI
            SearchComponents<TextMeshProUGUI>(t => t.text, term);

            // TMP 3D
            SearchComponents<TextMeshPro>(t => t.text, term);

            // Legacy TextMesh
            SearchComponents<TextMesh>(t => t.text, term);

            // Generic "other text components" with a string 'text' property/field
            SearchGenericTextComponents(term);

            stopwatch.Stop();
            Debug.Log($"SceneTextSearchWindow: Found {results.Count} results in {stopwatch.ElapsedMilliseconds} ms");
        }

        private void SearchComponents<T>(Func<T, string> getText, string term) where T : Component
        {
            var objects = Resources.FindObjectsOfTypeAll<T>();
            foreach (var comp in objects)
            {
                if (!IsSceneObject(comp))
                    continue;

                string value = getText(comp);
                if (StringMatches(value, term))
                {
                    AddResult(comp, value);
                }
            }
        }

        private void SearchGenericTextComponents(string term)
        {
            var monos = Resources.FindObjectsOfTypeAll<MonoBehaviour>();
            foreach (var mb in monos)
            {
                if (!IsSceneObject(mb))
                    continue;

                if (seenComponents.Contains(mb))
                    continue;

                Type type = mb.GetType();

                try
                {
                    var prop = type.GetProperty("text", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (prop != null && prop.PropertyType == typeof(string) && prop.CanRead)
                    {
                        string value = prop.GetValue(mb, null) as string;
                        if (StringMatches(value, term))
                        {
                            AddResult(mb, value);
                            continue;
                        }
                    }
                }
                catch
                {
                }

                try
                {
                    var field = type.GetField("text", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (field != null && field.FieldType == typeof(string))
                    {
                        string value = field.GetValue(mb) as string;
                        if (StringMatches(value, term))
                        {
                            AddResult(mb, value);
                        }
                    }
                }
                catch
                {
                }
            }
        }

        private bool StringMatches(string value, string term)
        {
            if (string.IsNullOrEmpty(value))
                return false;

            if (!caseSensitive)
                value = value.ToLowerInvariant();

            return value.Contains(term);
        }

        private void AddResult(Component component, string text)
        {
            if (component == null)
                return;

            if (seenComponents.Add(component))
            {
                results.Add(new SearchResult
                {
                    component = component,
                    text = text
                });
            }
        }

        private static bool IsSceneObject(Component comp)
        {
            if (comp == null)
                return false;

            var go = comp.gameObject;
            if (go == null)
                return false;

            if (EditorUtility.IsPersistent(go))
                return false;

            if (!go.scene.IsValid() || !go.scene.isLoaded)
                return false;

            return true;
        }

        private static string Truncate(string input, int maxLength)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            if (input.Length <= maxLength)
                return input;
            return input.Substring(0, maxLength) + "...";
        }
    }
}
