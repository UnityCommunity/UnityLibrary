// shows renderer stats from selected objects in the editor

using UnityEngine;
using UnityEditor;

namespace UnityLibrary
{
    public class RendererTypeCounter : EditorWindow
    {
        private int activeMeshRendererCount;
        private int inactiveMeshRendererCount;
        private int activeSkinnedMeshRendererCount;
        private int inactiveSkinnedMeshRendererCount;
        private int activeSpriteRendererCount;
        private int inactiveSpriteRendererCount;
        private int totalGameObjectCount;

        [MenuItem("Tools/Renderer Type Counter")]
        public static void ShowWindow()
        {
            GetWindow<RendererTypeCounter>("Renderer Type Counter");
        }

        private void OnGUI()
        {
            if (GUILayout.Button("Count Renderers"))
            {
                CountRenderersInSelection();
            }

            GUILayout.Space(10);

            GUILayout.Label("Active Mesh Renderers: " + activeMeshRendererCount);
            GUILayout.Label("Inactive Mesh Renderers: " + inactiveMeshRendererCount);
            GUILayout.Label("Active Skinned Mesh Renderers: " + activeSkinnedMeshRendererCount);
            GUILayout.Label("Inactive Skinned Mesh Renderers: " + inactiveSkinnedMeshRendererCount);
            GUILayout.Label("Active Sprite Renderers: " + activeSpriteRendererCount);
            GUILayout.Label("Inactive Sprite Renderers: " + inactiveSpriteRendererCount);
            GUILayout.Label("Total GameObjects: " + totalGameObjectCount);
        }

        private void CountRenderersInSelection()
        {
            activeMeshRendererCount = 0;
            inactiveMeshRendererCount = 0;
            activeSkinnedMeshRendererCount = 0;
            inactiveSkinnedMeshRendererCount = 0;
            activeSpriteRendererCount = 0;
            inactiveSpriteRendererCount = 0;
            totalGameObjectCount = 0;

            foreach (GameObject obj in Selection.gameObjects)
            {
                CountRenderersRecursively(obj);
            }

            Repaint();
        }

        private void CountRenderersRecursively(GameObject obj)
        {
            totalGameObjectCount++;

            bool isActive = obj.activeInHierarchy;

            if (obj.GetComponent<MeshRenderer>())
            {
                if (isActive)
                {
                    activeMeshRendererCount++;
                }
                else
                {
                    inactiveMeshRendererCount++;
                }
            }

            if (obj.GetComponent<SkinnedMeshRenderer>())
            {
                if (isActive)
                {
                    activeSkinnedMeshRendererCount++;
                }
                else
                {
                    inactiveSkinnedMeshRendererCount++;
                }
            }

            if (obj.GetComponent<SpriteRenderer>())
            {
                if (isActive)
                {
                    activeSpriteRendererCount++;
                }
                else
                {
                    inactiveSpriteRendererCount++;
                }
            }

            foreach (Transform child in obj.transform)
            {
                CountRenderersRecursively(child.gameObject);
            }
        }
    }
}
