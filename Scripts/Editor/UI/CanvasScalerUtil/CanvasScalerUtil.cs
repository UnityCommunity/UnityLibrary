using UnityEngine;

/// <summary>
/// Author: https://github.com/Sacristan
/// Script to hastily create correct canvas scales. Logic handled at editor's side
/// </summary>
[RequireComponent(typeof(Canvas))]
public class CanvasScalerUtil : MonoBehaviour
{
    [SerializeField]
    private uint canvasWidth = 800;

    [SerializeField]
    private uint canvasHeight = 600;

    [SerializeField]
    private float canvasWorldSizeInMeters = 1;

    public uint CanvasWidth { get { return canvasWidth; } }
    public uint CanvasHeight { get { return canvasHeight; } }
    public float CanvasWorldSizeInMeters { get { return canvasWorldSizeInMeters; } }

    #region MonoBehaviour
    private void Awake()
    {
        Debug.Log("CanvasScalerUtil has nothing to do with realtime logic");
        Destroy(this);
    }
    #endregion
}
