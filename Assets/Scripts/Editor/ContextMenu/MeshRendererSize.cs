
using UnityEditor;
using UnityEngine;

namespace UnityLibrary.EditorTools
{
    public static class MeshRendererSize
    {
        [MenuItem("CONTEXT/MeshRenderer/Get Size")]
        private static void GetSize(MenuCommand command)
        {
            MeshRenderer renderer = (MeshRenderer)command.context;

            Debug.Log(
                $"Renderer size (local): {FormatSize(renderer.localBounds.size)}\n" +
                $"Bounds size (world): {FormatSize(renderer.bounds.size)}",
                renderer
            );
        }

        private static string FormatSize(Vector3 size)
        {
            return $"<color=#FF5555>X: {size.x:F3}</color>, " +
                   $"<color=#55CC55>Y: {size.y:F3}</color>, " +
                   $"<color=#5599FF>Z: {size.z:F3}</color>";
        }
    }
}
