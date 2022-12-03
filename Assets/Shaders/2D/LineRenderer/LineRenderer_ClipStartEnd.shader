Shader "UnityLibrary/LineRenderer/ClipStartEnd"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Start ("Start", Range (0.0,1.0)) = 0.25
		_End ("End", Range (0.0,1.0)) = 0.75
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag

		#include "UnityCG.cginc"

		struct appdata
		{
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
			float4 color : COLOR;
		};

		struct v2f
		{
			float2 uv : TEXCOORD0;
			float4 vertex : SV_POSITION;
			float4 color : COLOR;
		};

		sampler2D _MainTex;
		float4 _MainTex_ST;
		float _Start;
		float _End;


		v2f vert (appdata v)
		{
			v2f o;
			o.vertex = UnityObjectToClipPos(v.vertex);
			o.uv = TRANSFORM_TEX(v.uv, _MainTex);
			o.color = v.color;
			return o;
		}

		fixed4 frag(v2f i) : SV_Target
		{
			fixed4 col = tex2D(_MainTex, i.uv)*i.color;
			clip(-(i.uv.x <_Start || i.uv.x > _End));
			// return fixed4(i.uv.x,0,0,0); // view UV x
			return col;
		}
		ENDCG
		}
	}
} 