// Camera switcher, https://forum.unity3d.com/threads/how-can-i-switch-between-multiple-cameras-using-one-key-click.472009/
// usage: Assign cameras into the array, press C to switch into next camera

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityLibrary
{
    public class CameraSwitcher : MonoBehaviour
    {
        public Camera[] cameras;
        int currentCamera = 0;

        void Awake()
        {
            if (cameras == null || cameras.Length == 0)
            {
                Debug.LogError("No cameras assigned..", gameObject);
                this.enabled = false;
            }

            EnableOnlyFirstCamera();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                // disable current
                cameras[currentCamera].enabled = false;

                // increment index and wrap after finished array
                currentCamera = (currentCamera + 1) % cameras.Length;

                // enable next
                cameras[currentCamera].enabled = true;
            }
        }

        void EnableOnlyFirstCamera()
        {
            for (int i = 0; i < cameras.Length; i++)
            {
                // only 1==0 returns true
                cameras[i].enabled = (i == 0);
            }
        }
    }
}
