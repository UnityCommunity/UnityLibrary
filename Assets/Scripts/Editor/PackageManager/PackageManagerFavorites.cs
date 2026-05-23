// adds custom buttons into PackageManager left panel (tested in 6.5)
// edit your own favorites in the code

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityLibrary.Editor.Tools
{

    [InitializeOnLoad]
    public static class PackageManagerFavorites
    {
        private const string InjectedElementName = "kelobyte-package-manager-dummy-panel";
        private static double nextScanTime;
        private static AddRequest addRequest;
        private static string pendingPackageName;

        static PackageManagerFavorites()
        {
            EditorApplication.update += OnEditorUpdate;
        }

        [MenuItem("Window/Package Management/Open Package Manager With Dummy Button")]
        private static void OpenPackageManagerAndInject()
        {
            Type windowType = GetPackageManagerWindowType();

            if (windowType == null)
            {
                Debug.LogWarning("Package Manager window type was not found.");
                return;
            }

            EditorWindow window = EditorWindow.GetWindow(windowType);
            window.Show();

            EditorApplication.delayCall += TryInject;
        }

        private static void OnEditorUpdate()
        {
            if (EditorApplication.timeSinceStartup < nextScanTime)
                return;

            nextScanTime = EditorApplication.timeSinceStartup + 0.5;
            TryInject();
        }

        private static void TryInject()
        {
            Type windowType = GetPackageManagerWindowType();

            if (windowType == null)
                return;

            UnityEngine.Object[] windows = Resources.FindObjectsOfTypeAll(windowType);

            foreach (UnityEngine.Object obj in windows)
            {
                EditorWindow window = obj as EditorWindow;

                if (window == null)
                    continue;

                InjectIntoWindow(window);
            }
        }

        private static Type GetPackageManagerWindowType()
        {
            Type type = Type.GetType("UnityEditor.PackageManager.UI.PackageManagerWindow, UnityEditor.PackageManagerUIModule");

            if (type != null)
                return type;

            foreach (System.Reflection.Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType("UnityEditor.PackageManager.UI.PackageManagerWindow");

                if (type != null)
                    return type;
            }

            return null;
        }

        private static void InjectIntoWindow(EditorWindow window)
        {
            VisualElement root = window.rootVisualElement;

            if (root == null)
                return;

            if (root.Q<VisualElement>(InjectedElementName) != null)
                return;

            VisualElement insertParent = FindInsertParent(root);
            int insertIndex = FindInsertIndex(insertParent);

            if (insertParent == null)
                return;

            VisualElement panel = CreateDummyPanel();

            if (insertIndex >= 0 && insertIndex <= insertParent.childCount)
                insertParent.Insert(insertIndex, panel);
            else
                insertParent.Add(panel);
        }

        private static VisualElement CreateDummyPanel()
        {
            VisualElement wrapper = new VisualElement();
            wrapper.name = InjectedElementName;

            wrapper.style.marginTop = 8;
            wrapper.style.marginBottom = 8;
            wrapper.style.paddingLeft = 6;
            wrapper.style.paddingRight = 6;

            Foldout foldout = new Foldout();
            foldout.text = "Favorites";
            foldout.value = true;

            Button newtonsoftButton = CreatePackageButton(
                "Newtonsoft Json",
                "com.unity.nuget.newtonsoft-json"
            );

            Button gltfastButton = CreatePackageButton(
                "Unity glTFast",
                "com.unity.cloud.gltfast"
            );

            Button unityGltfButton = CreatePackageButton(
                "Khronos UnityGLTF",
                "https://github.com/KhronosGroup/UnityGLTF.git"
            );

            Button myEssentials = CreatePackageButton(
                "Essentials",
                "  https://github.com/unitycoder/UnityEditorEssentials.git"
            );

            foldout.Add(newtonsoftButton);
            foldout.Add(gltfastButton);
            foldout.Add(unityGltfButton);
            foldout.Add(myEssentials);

            wrapper.Add(foldout);

            return wrapper;
        }

        private static Button CreatePackageButton(string label, string packageNameOrUrl)
        {
            Button button = new Button(() =>
            {
                AddPackage(packageNameOrUrl);
            });

            button.text = label;
            button.style.marginTop = 4;
            button.style.height = 24;

            return button;
        }

        private static void AddPackage(string packageNameOrUrl)
        {
            string packageToAdd = packageNameOrUrl.Trim();

            if (addRequest != null && !addRequest.IsCompleted)
            {
                Debug.LogWarning("A package add request is already in progress.");
                return;
            }

            Debug.Log($"Adding package: {packageToAdd}");

            pendingPackageName = packageToAdd;
            addRequest = Client.Add(packageToAdd);

            EditorApplication.update -= MonitorAddPackageRequest;
            EditorApplication.update += MonitorAddPackageRequest;
        }

        private static void MonitorAddPackageRequest()
        {
            if (addRequest == null || !addRequest.IsCompleted)
                return;

            EditorApplication.update -= MonitorAddPackageRequest;

            if (addRequest.Status == StatusCode.Success)
            {
                Debug.Log($"Package added: {pendingPackageName}");
                Client.Resolve();
                AssetDatabase.Refresh();
                CompilationPipeline.RequestScriptCompilation();
            }
            else
            {
                string errorMessage = addRequest.Error != null ? addRequest.Error.message : "Unknown error.";
                Debug.LogError($"Failed to add package '{pendingPackageName}': {errorMessage}");
            }

            addRequest = null;
            pendingPackageName = null;
        }

        private static VisualElement FindInsertParent(VisualElement root)
        {
            Label servicesLabel = FindLabel(root, "Services");

            if (servicesLabel != null && servicesLabel.parent != null && servicesLabel.parent.parent != null)
                return servicesLabel.parent.parent;

            Label cloudLabel = FindLabel(root, "Cloud");

            if (cloudLabel != null && cloudLabel.parent != null && cloudLabel.parent.parent != null)
                return cloudLabel.parent.parent;

            Label sourcesLabel = FindLabel(root, "Sources");

            if (sourcesLabel != null && sourcesLabel.parent != null && sourcesLabel.parent.parent != null)
                return sourcesLabel.parent.parent;

            return FindLikelyLeftPanel(root);
        }

        private static int FindInsertIndex(VisualElement parent)
        {
            if (parent == null)
                return -1;

            for (int i = 0; i < parent.childCount; i++)
            {
                VisualElement child = parent[i];

                if (ContainsLabel(child, "Services"))
                    return i + 1;
            }

            for (int i = 0; i < parent.childCount; i++)
            {
                VisualElement child = parent[i];

                if (ContainsLabel(child, "Cloud"))
                    return i + 1;
            }

            return -1;
        }

        private static VisualElement FindLikelyLeftPanel(VisualElement root)
        {
            List<VisualElement> all = new List<VisualElement>();
            Collect(root, all);

            foreach (VisualElement element in all)
            {
                Rect rect = element.worldBound;

                if (rect.width > 120 && rect.width < 260 && rect.height > 250 && rect.x < 250)
                    return element;
            }

            return null;
        }

        private static Label FindLabel(VisualElement root, string text)
        {
            List<VisualElement> all = new List<VisualElement>();
            Collect(root, all);

            foreach (VisualElement element in all)
            {
                Label label = element as Label;

                if (label != null && label.text == text)
                    return label;
            }

            return null;
        }

        private static bool ContainsLabel(VisualElement root, string text)
        {
            if (root == null)
                return false;

            Label label = root as Label;

            if (label != null && label.text == text)
                return true;

            for (int i = 0; i < root.childCount; i++)
            {
                if (ContainsLabel(root[i], text))
                    return true;
            }

            return false;
        }

        private static void Collect(VisualElement root, List<VisualElement> elements)
        {
            if (root == null)
                return;

            elements.Add(root);

            for (int i = 0; i < root.childCount; i++)
                Collect(root[i], elements);
        }
    }
}
#endif
