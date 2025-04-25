// draws grid lines in the game view (useful for seeing the resolution of ui elements in the game view)
// usage: attach to a game object in the scene, set gameobject tag to "EditorOnly" to remove from builds

using UnityEngine;

namespace UnityLibrary.EditorTools
{
    [ExecuteAlways]
    public class GameViewGridOverlay : MonoBehaviour
    {
#if UNITY_EDITOR
        public bool drawGrid = true;

        public int gridSpacingX = 64;
        public int gridSpacingY = 64;

        public int startOffsetX = 0;
        public int startOffsetY = 0;

        public Color gridColor = new Color(1f, 1f, 1f, 0.5f);

        private void OnGUI()
        {
            if (!drawGrid || Application.isPlaying) return;

            Color oldColor = GUI.color;
            GUI.color = gridColor;

            // Horizontal lines
            for (int y = startOffsetX; y < Screen.height; y += gridSpacingY)
            {
                GUI.DrawTexture(new Rect(0, y, Screen.width, 1), Texture2D.whiteTexture);
            }

            // Vertical lines
            for (int x = startOffsetY; x < Screen.width; x += gridSpacingX)
            {
                GUI.DrawTexture(new Rect(x, 0, 1, Screen.height), Texture2D.whiteTexture);
            }

            GUI.color = oldColor;
        }
#endif
    }
}
