// converts LineRenderer points from local space to world space via context menu in Unity Editor

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace UnityLibrary.ContextMenu
{
    public static class LineRendererToWorldSpace
    {
        private const string MenuPath = "CONTEXT/LineRenderer/Convert Points To World Space";

        [MenuItem(MenuPath, true)]
        private static bool Validate(MenuCommand command)
        {
            return command != null && command.context is LineRenderer;
        }

        [MenuItem(MenuPath)]
        private static void Convert(MenuCommand command)
        {
            var lr = (LineRenderer)command.context;
            if (lr == null) return;

            if (lr.useWorldSpace)
            {
                Debug.Log("LineRenderer is already using World Space.");
                return;
            }

            int count = lr.positionCount;
            if (count == 0) return;

            Transform t = lr.transform;

            Undo.RecordObject(lr, "Convert LineRenderer To World Space");

            Vector3[] local = new Vector3[count];
            lr.GetPositions(local);

            Vector3[] world = new Vector3[count];
            for (int i = 0; i < count; i++)
                world[i] = t.TransformPoint(local[i]);

            lr.useWorldSpace = true;
            lr.SetPositions(world);

            EditorUtility.SetDirty(lr);

            if (!Application.isPlaying)
                EditorSceneManager.MarkSceneDirty(lr.gameObject.scene);
        }
    }
}
