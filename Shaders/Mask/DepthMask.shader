Shader "Custom/Mask/Depth" {

	SubShader{
		Tags{ "Queue" = "Geometry+10" }

		ColorMask 0
		ZWrite On

		Pass{}
	}
}