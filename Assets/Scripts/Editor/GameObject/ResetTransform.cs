using UnityEngine;
using UnityEditor;

// reset transform position, rotation and scale

namespace UnityLibrary
{
    public class ResetTransform : ScriptableObject
    {
        [MenuItem("GameObject/Reset Transform #r")]
        static public void MoveSceneViewCamera()
        {
            // TODO add multiple object support
            var go = Selection.activeGameObject;
            if (go != null)
            {
                // TODO: add undo
                go.transform.position = Vector3.zero;
                go.transform.rotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;
            }
        }

    } // class 
} // namespace
