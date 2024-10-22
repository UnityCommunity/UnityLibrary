// prints out which buttons in current scene are referencing your given method

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.Events;
using System.Reflection;

namespace UnityLibrary
{
    public class FindWhatButtonCallsMyMethod : EditorWindow
    {
        private string methodName = "MyMethodHere";

        [MenuItem("Tools/Find Buttons with Method")]
        public static void ShowWindow()
        {
            GetWindow<FindWhatButtonCallsMyMethod>("FindWhatButtonCallsMyMethod");
        }

        private void OnGUI()
        {
            GUILayout.Label("Find Buttons that call Method", EditorStyles.boldLabel);
            methodName = EditorGUILayout.TextField("Method Name:", methodName);

            if (GUILayout.Button("Find Buttons"))
            {
                FindButtonsWithMethod();
            }
        }

        private void FindButtonsWithMethod()
        {
            Button[] allButtons = FindObjectsOfType<Button>();

            foreach (var button in allButtons)
            {
                UnityEvent clickEvent = button.onClick;

                for (int i = 0; i < clickEvent.GetPersistentEventCount(); i++)
                {
                    Object target = clickEvent.GetPersistentTarget(i);
                    string method = clickEvent.GetPersistentMethodName(i);

                    if (method == methodName)
                    {
                        MethodInfo methodInfo = target.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (methodInfo != null)
                        {
                            ParameterInfo[] parameters = methodInfo.GetParameters();

                            // Check if the method has arguments
                            if (parameters.Length > 0)
                            {
                                Debug.Log($"Button '{button.gameObject.name}' calls method '{methodName}' with parameters on object '{target.name}'", button.gameObject);
                            }
                            else
                            {
                                Debug.Log($"Button '{button.gameObject.name}' calls method '{methodName}' with no parameters on object '{target.name}'", button.gameObject);
                            }
                        }
                    }
                }
            }

            Debug.Log("Search Complete");
        }
    } // class
} // namespace
