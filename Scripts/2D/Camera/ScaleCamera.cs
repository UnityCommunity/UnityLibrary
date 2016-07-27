using UnityEngine;

// pixel perfect camera helpers, from old unity 2D tutorial videos

[ExecuteInEditMode]
public class ScaleCamera : MonoBehaviour 
{
	public int targetWidth = 640;
	public float pixelsToUnits = 100;

	void Start()
    {
		    int height = Mathf.RoundToInt(targetWidth / (float)Screen.width * Screen.height);
		    GetComponent<Camera>().orthographicSize = height / pixelsToUnits / 2;
	  }
}
