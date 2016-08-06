using UnityEngine;
using System.Collections;

// Example: Using Graphics.Blit to draw a full screen texture, with particle shader
// Usage: Attach to Main Camera
// Optional: Assign some texture into the displayTexture field in inspector

public class Graphics_Blit : MonoBehaviour
{
    public Texture displayTexture; // assign texture you want to blit fullscreen
    Material mat; // material(shader) to use for blitting

    void Awake()
    {
        if (displayTexture == null) displayTexture = Texture2D.whiteTexture; // use white texture, if nothing is set

        // use particle shader for the Blit material
        var shader = Shader.Find("Particles/Multiply (Double)");
        mat = new Material(shader);
    }

    // This function is called only if the script is attached to the camera and is enabled
    void OnPostRender()
    {
        // Copies source texture into destination render texture with a shader
        // Destination RenderTexture is null to blit directly to screen
        Graphics.Blit(displayTexture, null, mat);
    }
}
