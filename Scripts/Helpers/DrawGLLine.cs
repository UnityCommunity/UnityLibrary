using UnityEngine;

// Draws lines with GL : https://docs.unity3d.com/ScriptReference/GL.html
// Usage: Attach this script to gameobject in scene

public class DrawGLLine : MonoBehaviour
{
    public Color lineColor = Color.red;

    Material lineMaterial;

    void CreateLineMaterial()
    {
        if (!lineMaterial)
        {
            // Unity has a built-in shader that is useful for drawing simple colored things
            var shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }


    // cannot call this on update, line wont be visible then.. and if used OnPostRender() thats works when attached to camera only
    void OnRenderObject()
    {
        CreateLineMaterial();
        lineMaterial.SetPass(0);

        GL.PushMatrix();
        GL.MultMatrix(transform.localToWorldMatrix);

        GL.Begin(GL.LINES);
        GL.Color(lineColor);
        // start line from transform position
        GL.Vertex(transform.position);
        // end line 100 units forward from transform position
        GL.Vertex(transform.position + transform.forward * 100);

        GL.End();
        GL.PopMatrix();
    }
}
