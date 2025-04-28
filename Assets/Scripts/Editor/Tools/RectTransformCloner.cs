// clones UI RectTransform values from one GameObject to another in Unity Editor (only for identical hierarchy)

using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

namespace UnityLibrary.Tools
{
    public class RectTransformCloner : EditorWindow
    {
        private GameObject source;
        private GameObject target;
        private bool requireIdenticalNames = true;
        private bool cloneTMPAlignment = false;

        [MenuItem("Tools/RectTransform Cloner")]
        private static void ShowWindow()
        {
            var window = GetWindow<RectTransformCloner>();
            window.titleContent = new GUIContent("RectTransform Cloner");
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Clone RectTransform", EditorStyles.boldLabel);

            source = (GameObject)EditorGUILayout.ObjectField("Source", source, typeof(GameObject), true);
            target = (GameObject)EditorGUILayout.ObjectField("Target", target, typeof(GameObject), true);
            requireIdenticalNames = EditorGUILayout.Toggle("Require Identical Names", requireIdenticalNames);
            cloneTMPAlignment = EditorGUILayout.Toggle("Clone TMP Alignment", cloneTMPAlignment);

            if (GUILayout.Button("Clone RectTransforms"))
            {
                if (source == null || target == null)
                {
                    Debug.LogError("Source and Target must be assigned.");
                    return;
                }

                string errorMessage;
                if (!CompareHierarchies(source.transform, target.transform, out errorMessage))
                {
                    Debug.LogError("Source and Target hierarchies do not match!\n" + errorMessage, target);
                    return;
                }

                Undo.RegisterFullObjectHierarchyUndo(target, "Clone RectTransform Values");
                CopyRectTransforms(source.transform, target.transform);

                Debug.Log("RectTransform values cloned successfully.", target);

                if (target.transform.parent != null)
                {
                    RectTransform parentRect = target.transform.parent as RectTransform;
                    if (parentRect != null)
                    {
                        LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
                    }
                    else
                    {
                        Debug.LogWarning("Target's parent is not a RectTransform, cannot force layout rebuild.", target);
                    }
                }
                else
                {
                    Debug.LogWarning("Target has no parent, cannot force layout rebuild.", target);
                }
                EditorUtility.SetDirty(target);
                SceneView.RepaintAll();
            }
        }

        private bool CompareHierarchies(Transform source, Transform target, out string errorMessage)
        {
            errorMessage = "";

            if (source.childCount != target.childCount)
            {
                errorMessage = $"Child count mismatch at {GetTransformPath(source)}: Source has {source.childCount}, Target has {target.childCount}";
                return false;
            }

            for (int i = 0; i < source.childCount; i++)
            {
                var sourceChild = source.GetChild(i);
                var targetChild = target.GetChild(i);

                if (requireIdenticalNames && sourceChild.name != targetChild.name)
                {
                    errorMessage = $"Child name mismatch at {GetTransformPath(sourceChild)}: Source has '{sourceChild.name}', Target has '{targetChild.name}'";
                    return false;
                }

                if (!CompareHierarchies(sourceChild, targetChild, out errorMessage))
                {
                    return false;
                }
            }

            return true;
        }

        private void CopyRectTransforms(Transform source, Transform target)
        {
            var sourceRect = source as RectTransform;
            var targetRect = target as RectTransform;

            if (sourceRect != null && targetRect != null)
            {
                CopyRectTransformValues(sourceRect, targetRect);

                if (cloneTMPAlignment)
                {
                    var sourceTMP = source.GetComponent<TextMeshProUGUI>();
                    var targetTMP = target.GetComponent<TextMeshProUGUI>();
                    if (sourceTMP != null && targetTMP != null)
                    {
                        Undo.RecordObject(targetTMP, "Clone TMP Alignment");
                        targetTMP.alignment = sourceTMP.alignment;
                    }
                }
            }

            for (int i = 0; i < source.childCount; i++)
            {
                CopyRectTransforms(source.GetChild(i), target.GetChild(i));
            }
        }

        private void CopyRectTransformValues(RectTransform source, RectTransform target)
        {
            target.anchoredPosition = source.anchoredPosition;
            target.sizeDelta = source.sizeDelta;
            target.anchorMin = source.anchorMin;
            target.anchorMax = source.anchorMax;
            target.pivot = source.pivot;
            target.localRotation = source.localRotation;
            target.localScale = source.localScale;
            target.localPosition = source.localPosition;
        }

        private string GetTransformPath(Transform t)
        {
            if (t.parent == null)
                return t.name;
            return GetTransformPath(t.parent) + "/" + t.name;
        }
    }
}
