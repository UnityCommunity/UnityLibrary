// Simple shortcuts for moving selected GameObject around in hierarchy by Factuall
// SHIFT + CTRL + ALT +
// W - Move it up in hierarchy
// S - Move it down in hierarchy
// A - Unparent it down
// D - Parent it to next object in hierarchy

using UnityEditor;

namespace UnityLibrary
{
    public class UnparentMe
    {
        [MenuItem("GameObject/Unparent %#&a")]
        static void Detach()
        {
            
            if (Selection.activeGameObject != null && Selection.activeGameObject.transform.parent != null)
            {
                int newIndex = Selection.activeGameObject.transform.parent.GetSiblingIndex();
                Undo.SetTransformParent(Selection.activeGameObject.transform, Selection.activeGameObject.transform.parent.parent, "unparent selection");
                Selection.activeGameObject.transform.SetSiblingIndex(newIndex + 1);
            }
        }
        [MenuItem("GameObject/ParentDown %#&d")]
        static void ParentDown()
        {
            if (Selection.activeGameObject != null)
            {
                int parentChildCount = Selection.activeGameObject.transform.parent == null ?
                    Selection.activeGameObject.scene.rootCount : Selection.activeGameObject.transform.parent.childCount;
                if (Selection.activeGameObject.transform.GetSiblingIndex() == parentChildCount - 1) return;
                if (Selection.activeGameObject.transform.parent != null)
                {
                    if (Selection.activeGameObject.transform.parent.childCount + 1 > Selection.activeGameObject.transform.GetSiblingIndex())
                        Undo.SetTransformParent(Selection.activeGameObject.transform, Selection.activeGameObject.transform.parent.GetChild(Selection.activeGameObject.transform.GetSiblingIndex() + 1), "parent selection down in hierarchy");
                    
                }
                else
                {
                    if (Selection.activeGameObject.scene.rootCount + 1 > Selection.activeGameObject.transform.GetSiblingIndex())
                        Undo.SetTransformParent(Selection.activeGameObject.transform, Selection.activeGameObject.scene.GetRootGameObjects()[Selection.activeGameObject.transform.GetSiblingIndex() + 1].transform, "parent selection down in hierarchy");
                }
            }

        }
        [MenuItem("GameObject/Moveup %#&w")]
        static void MoveUp()
        {
            if (Selection.activeGameObject != null)
            {

                if (Selection.activeGameObject.transform.GetSiblingIndex() != 0)
                {
                    Undo.RegisterCompleteObjectUndo(Selection.activeGameObject.transform, "move selction up in hierarchy");
                    Selection.activeGameObject.transform.SetSiblingIndex(Selection.activeGameObject.transform.GetSiblingIndex() - 1);
                }
                
            }
        }
        [MenuItem("GameObject/Movedown %#&s")]
        static void MoveDown()
        {
            if (Selection.activeGameObject != null)
            {
                int parentChildCount = Selection.activeGameObject.transform.parent == null ?
                    Selection.activeGameObject.scene.rootCount : Selection.activeGameObject.transform.parent.childCount;
                if (Selection.activeGameObject.transform.GetSiblingIndex() != parentChildCount - 1)
                {
                    Undo.RegisterCompleteObjectUndo(Selection.activeGameObject.transform, "move selction down in hierarchy");
                    Selection.activeGameObject.transform.SetSiblingIndex(Selection.activeGameObject.transform.GetSiblingIndex() + 1);
                }

            }
        }
    }
}