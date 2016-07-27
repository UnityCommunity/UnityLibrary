using UnityEngine;

// pixel perfect camera helpers, from old unity 2D tutorial videos

[ExecuteInEditMode]
public class PixelPerfectCamera : MonoBehaviour {

	public float pixelsToUnits = 100;

	void Start () 
	{
		GetComponent<Camera>().orthographicSize = Screen.height / pixelsToUnits / 2;
	}
}
