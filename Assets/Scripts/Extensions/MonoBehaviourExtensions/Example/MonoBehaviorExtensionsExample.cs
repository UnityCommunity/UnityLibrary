using UnityEngine;
using Sacristan.Utils.Extensions;

public class MonoBehaviorExtensionsTest : MonoBehaviour
{
    private void Start()
    {
        this.InvokeRepeatingSafe(Tick, 1f, 1f);
        this.InvokeSafe(Tock, 2f);
    }

    private void Tick()
    {
        Debug.Log("Tick");
    }

    private void Tock()
    {
        Debug.Log("Tock");
    }
}
