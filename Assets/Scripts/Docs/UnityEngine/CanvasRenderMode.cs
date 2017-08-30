using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// usage: Attach this script to gameobject with Canvas component
// click mouse button to switch modes *note: worldspace will not be visible without scaling it
// https://docs.unity3d.com/ScriptReference/Canvas-renderMode.html

namespace UnityLibrary
{
    public class CanvasRenderMode : MonoBehaviour
    {
        Canvas canvas;

        void Start()
        {
            canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Canvas not found..", gameObject);
                this.enabled = false;
            }
        }

        void Update()
        {
            // switch modes on left mouse click
            if (Input.GetMouseButtonDown(0))
            {
                if (canvas.renderMode != RenderMode.WorldSpace)
                {
                    canvas.renderMode = RenderMode.WorldSpace;
                } else
                {
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                }
            }
        }
    }
}
