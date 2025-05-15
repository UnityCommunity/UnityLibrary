// finds what scripts reference a given GameObject in the scene (in events, public fields..)

using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace UnityLibrary.Editor
{
    public class FindWhoReferencesThisGameObject : EditorWindow
    {
        private GameObject target;
        private Vector2 scroll;

        private class ReferenceResult
        {
            public string message;
            public GameObject owner;
        }

        private List<ReferenceResult> results = new List<ReferenceResult>();

        [MenuItem("Tools/UnityLibrary/Find References To GameObject")]
        public static void ShowWindow()
        {
            var win = GetWindow<FindWhoReferencesThisGameObject>("Find References");
            win.minSize = new Vector2(500, 300);
        }

        private void OnGUI()
        {
            GUILayout.Label("Find scripts that reference this GameObject", EditorStyles.boldLabel);
            target = EditorGUILayout.ObjectField("Target GameObject", target, typeof(GameObject), true) as GameObject;

            if (GUILayout.Button("Find References"))
            {
                results.Clear();
                if (target != null)
                {
                    FindReferences(target);
                }
                else
                {
                    Debug.LogWarning("Please assign a GameObject.");
                }
            }

            if (results.Count > 0)
            {
                GUILayout.Label("Results:", EditorStyles.boldLabel);
                scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(400));
                foreach (var res in results)
                {
                    if (GUILayout.Button(res.message, GUILayout.ExpandWidth(true)))
                    {
                        EditorGUIUtility.PingObject(res.owner);
                        Selection.activeGameObject = res.owner;
                    }
                }
                GUILayout.EndScrollView();
            }
        }

        private void FindReferences(GameObject target)
        {
            var allObjects = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>(true);

            foreach (var mono in allObjects)
            {
                if (mono == null || mono.gameObject == target) continue;

                var type = mono.GetType();
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                foreach (var field in fields)
                {
                    if (typeof(UnityEventBase).IsAssignableFrom(field.FieldType))
                    {
                        var unityEvent = field.GetValue(mono) as UnityEventBase;
                        if (unityEvent != null)
                        {
                            int count = unityEvent.GetPersistentEventCount();
                            for (int i = 0; i < count; i++)
                            {
                                var listener = unityEvent.GetPersistentTarget(i);
                                if (listener == target)
                                {
                                    results.Add(new ReferenceResult
                                    {
                                        message = $"{mono.name} ({type.Name}) -> UnityEvent '{field.Name}'",
                                        owner = mono.gameObject
                                    });
                                }
                            }
                        }
                    }
                    else if (typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType))
                    {
                        var value = field.GetValue(mono) as UnityEngine.Object;
                        if (value == target)
                        {
                            results.Add(new ReferenceResult
                            {
                                message = $"{mono.name} ({type.Name}) -> Field '{field.Name}'",
                                owner = mono.gameObject
                            });
                        }
                    }
                }
            }

            if (results.Count == 0)
            {
                results.Add(new ReferenceResult
                {
                    message = "No references found.",
                    owner = null
                });
            }
        }
    }
}
