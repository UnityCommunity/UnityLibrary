// simple image viewer inside unity editor
// use case: to keep reference image visible while working in single monitor

using UnityEngine;
using UnityEditor;

namespace UnityLibrary
{
    public class ReferenceImageViewer : EditorWindow
    {
        Texture2D tex;
        bool keepAspectRatio = false;

        [MenuItem("Window/Tools/Reference Image Viewer")]
        static void Init()
        {
            ReferenceImageViewer window = (ReferenceImageViewer)EditorWindow.GetWindow(typeof(ReferenceImageViewer));
            window.titleContent = new GUIContent("ReferenceImageViewer");
            window.Show();
        }

        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Image", GUILayout.Width(50));
            tex = (Texture2D)EditorGUILayout.ObjectField(tex, typeof(Texture2D), true, GUILayout.MinWidth(100));
            GUILayout.FlexibleSpace();
            keepAspectRatio = EditorGUILayout.ToggleLeft("KeepAspect", keepAspectRatio);
            EditorGUILayout.EndHorizontal();

            if (tex != null)
            {
                int topOffset = 20;

                // prep
                var maxWidth = position.width;
                var maxHeight = position.height - topOffset;
                var imgWidth = (float)tex.width;
                var imgHeight = (float)tex.height;

                // calc
                var widthRatio = maxWidth / imgWidth;
                var heightRatio = maxHeight / imgHeight;
                var bestRatio = Mathf.Min(widthRatio, heightRatio);

                // output
                var newWidth = imgWidth * bestRatio;
                var newHeight = imgHeight * bestRatio;

                if (keepAspectRatio == true)
                {
                    EditorGUI.DrawPreviewTexture(new Rect(0, topOffset, newWidth, newHeight), tex);
                }
                else
                {
                    EditorGUI.DrawPreviewTexture(new Rect(0, topOffset, maxWidth, maxHeight), tex);
                }
            }
        }
    }
}
