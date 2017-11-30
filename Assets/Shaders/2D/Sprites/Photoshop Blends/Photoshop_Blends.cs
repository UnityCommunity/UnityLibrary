using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Photoshop_Blends : MonoBehaviour {
	public enum BlendModes{darken,multiply,colorBurn,linearBurn,darkerColor,lighten,screen,colorDodge,linearDodge,lighterColor,overlay,softLight,hardLight,vividLight,linearLight,pinLight,hardlerp,difference,exclusion,subtract,divide,hue,color,saturation,luminosity}
	public BlendModes blendmodes;
	private Material Mat;

	void Awake () {
		Material material = new Material (Shader.Find("UnityCommunity/Sprites/PhotoshopBlends"));
		GetComponent<MeshRenderer> ().sharedMaterial = material;
	}
	
	void Update () {
		
		GetComponent<MeshRenderer> ().sharedMaterial.SetInt ("number", blendmodes.GetHashCode ());
	}
}
