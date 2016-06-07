// original script : http://answers.unity3d.com/answers/252528/view.html

// Usage: Place this script in Editor/ folder
// Start the tool from menu, Window/Tools/Alpha-fy Images

using UnityEngine;
using UnityEditor;
using System.IO;

public class SpriteBackgroundRemover : EditorWindow
{
    Texture2D img;
    Texture2D newImg;
    Color colorToRemove = Color.magenta;
    public static SpriteBackgroundRemove win;

    [MenuItem("Window/Tools/Alpha-fy Images")]
    static void Init()
    {
        win = ScriptableObject.CreateInstance(typeof(SpriteBackgroundRemove)) as SpriteBackgroundRemove;
        win.minSize = new Vector2(300, 350);
        win.ShowUtility();
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal();

        /** Toolbar **/
        GUILayout.BeginVertical();
        img = (Texture2D)EditorGUILayout.ObjectField(img, typeof(Texture2D), false, GUILayout.MinWidth(128), GUILayout.MinHeight(128), GUILayout.MaxWidth(128), GUILayout.MaxHeight(128));

        colorToRemove = EditorGUILayout.ColorField(colorToRemove, GUILayout.MaxWidth(128));

        if (GUILayout.Button("Preview", GUILayout.MinWidth(128), GUILayout.MinHeight(32), GUILayout.MaxWidth(128), GUILayout.MaxHeight(128)))
            newImg = RemoveColor(colorToRemove, img);

        if (GUILayout.Button("Alpha-fy All", GUILayout.MinWidth(128), GUILayout.MinHeight(32), GUILayout.MaxWidth(128), GUILayout.MaxHeight(128)))
            RemoveColor(colorToRemove, (UnityEngine.Object[])Selection.GetFiltered(typeof(Texture2D), SelectionMode.Assets));

        GUILayout.EndVertical();

        GUILayout.BeginVertical();
        GUILayout.Label("Selected Files", EditorStyles.boldLabel);
        foreach (Texture2D selected in Selection.GetFiltered(typeof(Texture2D), SelectionMode.Assets))
        {
            GUILayout.Label(selected.name);
        }
        GUILayout.EndVertical();

        /** Image Display **/
        GUILayout.BeginVertical();
        GUILayout.Label("Preview", EditorStyles.boldLabel);
        if (newImg)
        {
            GUILayout.Label(newImg);
        }
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

    }

    // for multiple images
    void RemoveColor(Color c, UnityEngine.Object[] imgs)
    {
        if (!Directory.Exists("Assets/AlphaImages/"))
        {
            Directory.CreateDirectory("Assets/AlphaImages/");
        }

        float inc = 0f;
        foreach (Texture2D i in imgs)
        {
            inc++;
            if (inc % 512 == 0 && EditorUtility.DisplayCancelableProgressBar("Playin' With Pixels", "Seaching for Color Matches", ((float)inc / (float)imgs.Length)))
            {
                Debug.LogError("Cancelled..");
                break;
            }

            CheckTextureSettings(i);

            Color[] pixels = i.GetPixels(0, 0, i.width, i.height, 0);
            var clear = new Color(0, 0, 0, 0);

            for (int p = 0; p < pixels.Length; p++)
            {
                if (pixels[p] == c)
                {
                    pixels[p] = clear;
                }
            }

            Texture2D n = new Texture2D(i.width, i.height);
            n.SetPixels(0, 0, i.width, i.height, pixels, 0);
            n.Apply();

            byte[] bytes = n.EncodeToPNG();
            File.WriteAllBytes("Assets/AlphaImages/" + i.name + "_alpha.png", bytes);
        }

        EditorUtility.ClearProgressBar();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    // for single image
    Texture2D RemoveColor(Color c, Texture2D i)
    {
        CheckTextureSettings(i);

        Color[] pixels = i.GetPixels(0, 0, i.width, i.height, 0);

        var clear = new Color(0, 0, 0, 0);

        for (int p = 0; p < pixels.Length; p++)
        {
            if (p % 512 == 0 && EditorUtility.DisplayCancelableProgressBar("Playin' With Pixels", "Seaching for Color Matches", ((float)p / pixels.Length)))
            {
                Debug.LogError("Cancelled..");
                break;
            }

            if (pixels[p] == c)
            {
                pixels[p] = clear;
            }

        }

        Texture2D n = new Texture2D(i.width, i.height);
        n.SetPixels(0, 0, i.width, i.height, pixels, 0);
        n.Apply();
        EditorUtility.ClearProgressBar();
        return (n);
    }

    public void CheckTextureSettings(Texture2D texture)
    {
        if (texture == null) { Debug.LogError("CheckTextureSettings Failed - Texture is null"); return; }

        string path = AssetDatabase.GetAssetPath(texture);
        if (string.IsNullOrEmpty(path)) { Debug.LogError("CheckTextureSettings Failed - Texture path is null"); return; }

        TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;

        if (!textureImporter.isReadable)
        {
            Debug.Log("Enabling read/write for image " + path);
            //            textureImporter.mipmapEnabled = false;
            textureImporter.isReadable = true;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }

    }

}
