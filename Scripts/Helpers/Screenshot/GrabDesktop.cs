using UnityEngine;
using System.Collections;
using System.Drawing;
using Screen = System.Windows.Forms.Screen;
using Application = UnityEngine.Application;
using System.Drawing.Imaging;

// Drag windows desktop image using System.Drawing.dll
// guide on using System.Drawing.dll in unity : http://answers.unity3d.com/answers/253571/view.html *Note the .NET2.0 setting also


public class GrabDesktop : MonoBehaviour
{
    void Start()
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
    }
}
