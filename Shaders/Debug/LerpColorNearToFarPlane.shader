// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// source: https://forum.unity3d.com/threads/depth-shader-invert-it.12692/#post-89430
// lerp 2 colors between camera NearClipPlane to FarClipPlane

Shader "UnityCommunity/Debug/LerpColorNearToFarPlane" 
{
	Properties {
		_ColorNear ("Color Near", Color) = (1,0,0,1)
		_ColorFar ("Color Far", Color) = (0,1,0,1)
	}
	SubShader 
	{
		Pass 
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
 
			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float4 color : COLOR;
			};
 
			uniform float4 _ColorNear;
			uniform float4 _ColorFar;
 
			v2f vert(appdata v) 
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				float depth;
				COMPUTE_EYEDEPTH(depth);
				float factor = depth * _ProjectionParams.w;
				o.color = lerp(_ColorNear, _ColorFar, factor);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				return i.color;
			}

			ENDCG
		}
	}
}
