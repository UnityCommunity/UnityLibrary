using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoResolution : MonoBehaviour
{
        public int setWidth = 1440;
        public int setHeight = 2560;
        
        private void Start()
        {
            // Get the main camera and its current dimensions
            Camera camera = Camera.main;
            Rect rect = camera.rect;

            // Calculate the scale height and width of the screen
            float scaleHeight = ((float)Screen.width / Screen.height) / ((float)9 / 16);
            float scaleWidth = 1f / scaleHeight;

            // Adjust the camera's dimensions based on the scale height and width
            if (scaleHeight < 1)
            {
                rect.height = scaleHeight;
                rect.y = (1f - scaleHeight) / 2f;
            }
            else
            {
                rect.width = scaleWidth;
                rect.x = (1f - scaleWidth) / 2f;
            }
            
            camera.rect = rect;

            SetResolution();
        }

        public void SetResolution()
        {
            // Get the current device's screen dimensions
            int deviceWidth = Screen.width;
            int deviceHeight = Screen.height;

            // Set the screen resolution to the desired dimensions, while maintaining aspect ratio
            Screen.SetResolution(setWidth, (int)(((float)deviceHeight / deviceWidth) * setWidth), true);

            // Adjust the camera's dimensions based on the new resolution
            if ((float)setWidth / setHeight < (float)deviceWidth / deviceHeight)
            {
                float newWidth = ((float)setWidth / setHeight) / ((float)deviceWidth / deviceHeight);
                Camera.main.rect = new Rect((1f - newWidth) / 2f, 0f, newWidth, 1f);
            }
            else
            {
                float newHeight = ((float)deviceWidth / deviceHeight) / ((float)setWidth / setHeight);
                Camera.main.rect = new Rect(0f, (1f - newHeight) / 2f, 1f, newHeight); // 새로운 Rect 적용
            }
        }
}
