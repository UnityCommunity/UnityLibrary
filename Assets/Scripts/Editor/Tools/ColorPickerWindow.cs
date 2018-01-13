using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ColorPickerWindow : EditorWindow
{

	protected Color color = Color.white;
	protected Color32 color32 = new Color32 ( 255, 255, 255, 255 );
	protected string hexCode = "FFFFFF";

	[MenuItem ( "Tools/Color Picker" )]
	public static void Init ()
	{
		var window = EditorWindow.GetWindow<ColorPickerWindow> ( "Color Picker" );
		window.Show ();
	}

	protected virtual void OnGUI ()
	{
		this.color = EditorGUILayout.ColorField ( "Color", this.color );
		if ( GUI.changed )
		{
			this.color32 = this.color;
			this.hexCode = ColorUtility.ToHtmlStringRGB ( this.color );
		}
		this.hexCode = EditorGUILayout.TextField ( "Hex Code", this.hexCode );
		if ( GUI.changed )
		{
			ColorUtility.TryParseHtmlString ( this.hexCode, out this.color );
		}
		this.color32.r = ( byte )EditorGUILayout.IntSlider ( "Red", this.color32.r, 0, 255 );
		this.color32.g = ( byte )EditorGUILayout.IntSlider ( "Green", this.color32.g, 0, 255 );
		this.color32.b = ( byte )EditorGUILayout.IntSlider ( "Blue", this.color32.b, 0, 255 );
		this.color32.a = ( byte )EditorGUILayout.IntSlider ( "Alpha", this.color32.a, 0, 255 );
		if ( GUI.changed )
		{
			this.color = this.color32;
			this.hexCode = ColorUtility.ToHtmlStringRGB ( this.color );
		}
		EditorGUILayout.TextField (
			"Color Code",
			string.Format (
				"new Color ( {0}f, {1}f, {2}f, {3}f )",
				this.color.r,
				this.color.g,
				this.color.b,
				this.color.a ) );
		EditorGUILayout.TextField (
			"Color32 Code",
			string.Format (
				"new Color32 ( {0}, {1}, {2}, {3} )",
				this.color32.r,
				this.color32.g,
				this.color32.b,
				this.color32.a ) );
	}
	
}
