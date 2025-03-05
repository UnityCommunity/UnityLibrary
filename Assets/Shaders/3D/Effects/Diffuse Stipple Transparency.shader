// Diffuse shader with stipple transparency
// by Alex Ocias - https://ocias.com
// source: https://ocias.com/blog/unity-stipple-transparency-shader/
// based on an article by Digital Rune: https://www.digitalrune.com/Blog/Post/1743/Screen-Door-Transparency

// Simplified Diffuse shader. Differences from regular Diffuse one:
// - no Main Color
// - fully supports only 1 directional light. Other lights can affect it, but it will be per-vertex/SH.

Shader "Ocias/Diffuse (Stipple Transparency)" {
Properties {
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_Transparency ("Transparency", Range(0,1)) = 1.0
}
SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 150

CGPROGRAM
#pragma surface surf Lambert noforwardadd

sampler2D _MainTex;

struct Input {
	float2 uv_MainTex;
	float4 screenPos;
};

half _Transparency;

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
	o.Albedo = c.rgb;
	o.Alpha = c.a;

	// Screen-door transparency: Discard pixel if below threshold.
	float4x4 thresholdMatrix =
	{  1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
	  13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
	   4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
	  16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
	};
	float4x4 _RowAccess = { 1,0,0,0, 0,1,0,0, 0,0,1,0, 0,0,0,1 };
	float2 pos = IN.screenPos.xy / IN.screenPos.w;
	pos *= _ScreenParams.xy; // pixel position
	clip(_Transparency - thresholdMatrix[fmod(pos.x, 4)] * _RowAccess[fmod(pos.y, 4)]);
}
ENDCG
}

Fallback "Mobile/VertexLit"
}
