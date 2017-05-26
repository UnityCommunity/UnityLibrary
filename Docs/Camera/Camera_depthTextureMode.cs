// page https://docs.unity3d.com/ScriptReference/Camera-depthTextureMode.html
// usage: attach this script to Camera

using UnityEngine;

namespace UnityLibrary
{
    public class Camera_depthTextureMode : MonoBehaviour
    {
        void OnEnable()
        {
            var cam = Getcomponent<Camera>();
            if (cam!=null)
            {
              // enable camera depth texture
              cam.depthTextureMode = DepthTextureMode.Depth;
            }
//          Debug.Log(Camera.main.depthTextureMode);
        }
    }
}
