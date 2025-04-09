// reference image viewer from external folder

using UnityEngine;
using UnityEditor;
using System.IO;

namespace UnityLibrary.Tools
{
    public class ReferenceImageViewer2 : EditorWindow
    {
        private const string EditorPrefsKey = "ReferenceImageViewer_ImagePath";

        [SerializeField]
        private string imagePath = "";
        [SerializeField]
        private Texture2D loadedImage;

        [MenuItem("Window/Reference Image Viewer")]
        public static void ShowWindow()
        {
            GetWindow<ReferenceImageViewer2>("Reference Image Viewer");
        }

        private void OnEnable()
        {
            // Delay the reload to ensure Unity's layout system is ready
            EditorApplication.delayCall += TryLoadImage;
        }

        private void OnDisable()
        {
            if (!string.IsNullOrEmpty(imagePath))
                EditorPrefs.SetString(EditorPrefsKey, imagePath);
            else
                EditorPrefs.DeleteKey(EditorPrefsKey);
        }

        private void TryLoadImage()
        {
            imagePath = EditorPrefs.GetString(EditorPrefsKey, "");
            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                LoadImageFromPath(imagePath);
                Repaint(); // Ensure it's drawn
            }
        }

        void OnGUI()
        {
            GUILayout.Label("Reference Image Viewer", EditorStyles.boldLabel);

            if (GUILayout.Button("Browse for Image"))
            {
                string path = EditorUtility.OpenFilePanel("Select Image", "", "png,jpg,jpeg");
                if (!string.IsNullOrEmpty(path))
                {
                    imagePath = path;
                    EditorPrefs.SetString(EditorPrefsKey, imagePath);
                    LoadImageFromPath(imagePath);
                }
            }

            // Fallback in case delayCall missed
            if (loadedImage == null && !string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                LoadImageFromPath(imagePath);
                Repaint();
            }

            if (loadedImage != null)
            {
                GUILayout.Space(10);

                float windowWidth = position.width - 20;
                float imageAspect = (float)loadedImage.width / loadedImage.height;

                float displayWidth = windowWidth;
                float displayHeight = windowWidth / imageAspect;

                if (displayHeight > position.height - 100)
                {
                    displayHeight = position.height - 100;
                    displayWidth = displayHeight * imageAspect;
                }

                Rect imageRect = GUILayoutUtility.GetRect(displayWidth, displayHeight, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
                EditorGUI.DrawPreviewTexture(imageRect, loadedImage, null, ScaleMode.ScaleToFit);
            }
        }

        private void LoadImageFromPath(string path)
        {
            try
            {
                byte[] fileData = File.ReadAllBytes(path);
                loadedImage = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (!loadedImage.LoadImage(fileData))
                {
                    Debug.LogError("Failed to load image.");
                    loadedImage = null;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Could not load image from path: {path}\n{e.Message}");
                loadedImage = null;
            }
        }
    }
}
