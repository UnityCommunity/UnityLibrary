using System.IO;
using UnityEditor;
using UnityEngine;

public class LegacyAnimationCreator
{
    [MenuItem("Assets/Create/Legacy Animation", priority = 402)]
    public static void CompressSelectedAnimationClips()
    {
        var clip = new AnimationClip();
        clip.legacy = true;
        clip.name = "New Legacy Animation";

        string path;
        var selection = Selection.activeObject;
        if (selection == null)
            path = "Assets";
        else
            path = AssetDatabase.GetAssetPath(selection.GetInstanceID());

        path = Path.GetDirectoryName(path);
        path += $"/{clip.name}.anim";

        ProjectWindowUtil.CreateAsset(clip, path);
        Selection.activeObject = clip;
        EditorUtility.SetDirty(clip);
    }
}