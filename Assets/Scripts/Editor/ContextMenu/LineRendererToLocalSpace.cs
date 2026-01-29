// converts line renderer points from world space to local space

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace UnityLibrary.ContextMenu
{
    public static class LineRendererToLocalSpace
    {
        private const string MenuPath = "CONTEXT/LineRenderer/Convert Points To Local Space";

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

            int count = lr.positionCount;
            if (count == 0) return;

            Transform t = lr.transform;

            Undo.RecordObject(lr, "Convert LineRenderer To Local Space");

            // Get current positions in world space no matter what mode it's in.
            Vector3[] world = new Vector3[count];
            if (lr.useWorldSpace)
            {
                lr.GetPositions(world);
            }
            else
            {
                Vector3[] local = new Vector3[count];
                lr.GetPositions(local);
                for (int i = 0; i < count; i++)
                    world[i] = t.TransformPoint(local[i]);
            }

            // Convert world -> local, switch mode, write back.
            Vector3[] newLocal = new Vector3[count];
            for (int i = 0; i < count; i++)
                newLocal[i] = t.InverseTransformPoint(world[i]);

            lr.useWorldSpace = false;
            lr.SetPositions(newLocal);

            EditorUtility.SetDirty(lr);

            if (!Application.isPlaying)
                EditorSceneManager.MarkSceneDirty(lr.gameObject.scene);
        }
    }
}
