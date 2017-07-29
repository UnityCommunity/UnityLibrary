// pixel perfect camera helpers, from old unity 2D tutorial videos
// source: https://www.youtube.com/watch?v=rMCLWt1DuqI

using UnityEngine;

namespace UnityLibrary
{
    [ExecuteInEditMode]
    public class PixelPerfectCamera : MonoBehaviour
    {
        public float pixelsToUnits = 100;
        Camera cam;

        void Start()
        {
            cam = GetComponent<Camera>();
            if (cam == null)
            {
                Debug.LogError("Camera not found..", gameObject);
                this.enabled = false;
                return;
            }
            SetScale();
        }

        // in editor need to update in a loop, in case of game window resizes
#if UNITY_EDITOR
        void Update()
        {
            SetScale();
        }
#endif

        void SetScale()
        {
            cam.orthographicSize = Screen.height / pixelsToUnits / 2;
        }
    }
}
