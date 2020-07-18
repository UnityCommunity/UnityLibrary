using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// usage: attach this script to any gameobject in which you want to render the webcam display
// create a material, use Unlit/Texture as a shader and place it to the gameobject's material placeholder in the mesh renderer component

namespace UnityLibrary
{
    public class WebCamDisplay : MonoBehaviour
    {
        void Start()
        {
            WebCamTexture webCam = new WebCamTexture();
            webCam.deviceName = WebCamTexture.devices[0].name;
            this.GetComponent<MeshRenderer>().material.mainTexture = webCam;
            webCam.Play();
        }
    }
}

