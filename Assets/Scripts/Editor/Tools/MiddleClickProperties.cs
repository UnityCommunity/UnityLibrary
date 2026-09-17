// Press Middle Mouse Button on a GameObject in the Hierarchy to open its properties in the Inspector.

using Unity.Hierarchy;
using Unity.Hierarchy.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityLibrary.EditorTools
{
    static class MiddleClickProperties
    {
        const string k_Hooked = "middleclick-properties--hooked";

        [InitializeOnLoadMethod]
        static void Init()
        {
            HierarchyWindow.BindViewItem += OnBindViewItem;
        }

        static void OnBindViewItem(HierarchyWindow window, HierarchyView view, HierarchyViewItem item)
        {
            // View items are recycled, so register the callback only once per element.
            if (item.ClassListContains(k_Hooked))
                return;

            item.AddToClassList(k_Hooked);
            item.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
        }

        static void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != (int)MouseButton.MiddleMouse)
                return;

            if (evt.currentTarget is not HierarchyViewItem item)
                return;

            if (item.Handler is not HierarchyGameObjectHandler handler)
                return;

            GameObject go = handler.GetGameObject(item.Node);
            if (go == null)
                return;

            EditorUtility.OpenPropertyEditor(go);
            evt.StopPropagation();
        }
    }
}