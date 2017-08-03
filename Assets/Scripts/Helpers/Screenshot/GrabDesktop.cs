using UnityEngine;

using Application = UnityEngine.Application;
using System;
using System.Runtime.InteropServices;

// Drag windows desktop image using System.Drawing.dll
// guide on using System.Drawing.dll in unity : http://answers.unity3d.com/answers/253571/view.html

/// <summary>
/// Instructions: 
/// 1.- Uncomment the code below
/// 2.- Create a "Plugins" folder in your project
/// 3.- Import the System.Drawing.dll and System.Windows.Forms.dll from Unity_Location\Editor\Data\Mono\lib\mono\2.0
/// </summary>

//Uncomment this:

/*
//using System.Windows.Forms.Screen;
//using System.Drawing;
//using System.Drawing.Imaging;
namespace UnityLibrary{
public class GrabDesktop : MonoBehaviour
{
    [DllImport("kernel32", SetLastError = true)]
    static extern IntPtr LoadLibrary(string lpFileName);

    static bool CheckLibrary(string fileName)
    {
        return LoadLibrary(fileName) == IntPtr.Zero;
    }
    private void Start()
    {
        try
        {
            // screenshot source: http://stackoverflow.com/a/363008/5452781

            //Create a new bitmap.
            var bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format32bppArgb);

            // Create a graphics object from the bitmap.
            var gfxScreenshot = System.Drawing.Graphics.FromImage(bmpScreenshot);

            // Take the screenshot from the upper left corner to the right bottom corner.
            gfxScreenshot.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);

            // Save the screenshot to the specified path that the user has chosen.
            bmpScreenshot.Save(Application.dataPath + "/Screenshot.png", ImageFormat.Png);
        }catch(Exception e) { 
            Debug.LogError("You must import the dll to the project, refer to the instructions in the Script for more details");
        }
        
    }
}
}
*/
