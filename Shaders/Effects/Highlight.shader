Shader "Custom/Highlight" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
}
SubShader {
	Pass {
		ZWrite Off
		Offset -1, -1
		Blend SrcAlpha OneMinusSrcAlpha
		Color [_Color]
	}
}
}
