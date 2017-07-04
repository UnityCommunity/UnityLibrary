using UnityEngine;
using UnityEditor;

/// <summary>
/// Author: Girts Kesteris 2017
/// Script to hastily create correct canvas scales. Logic handled at editor's side
/// </summary>
[CustomEditor(typeof(CanvasScalerUtil))]
public class CanvasScalerUtilEditor : Editor
{
    CanvasScalerUtil t;

    private void OnEnable()
    {
        t = (CanvasScalerUtil) target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Scale Canvas")) ScaleCanvas();

    }

    private void ScaleCanvas()
    {
        Canvas canvas = t.GetComponent<Canvas>();
        RectTransform rectTransform = t.GetComponent<RectTransform>();

        Vector2 sizeDelta = new Vector2(t.CanvasWidth, t.CanvasHeight);
        Vector3 scale = t.CanvasWorldSizeInMeters / (float) t.CanvasWidth * Vector3.one;

        if (canvas.renderMode != RenderMode.WorldSpace)
        {
            Debug.Log("CanvasScalerUtil: Swiched to WorldSpace Render Mode from " + canvas.renderMode);
            canvas.renderMode = RenderMode.WorldSpace;
        }

        Debug.LogFormat("CanvasScalerUtil: calculated sizeDelta: {0} scale: {1}", sizeDelta, scale);

        rectTransform.sizeDelta = sizeDelta;
        rectTransform.localScale = scale;
    }
}
