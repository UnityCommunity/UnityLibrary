Shader "UnityLibrary/2D/Effects/ColorCycle"
{
	Properties
	{
		_MainTex ("Grayscale", 2D) = "white" {}
		_GradientTex ("Gradient", 2D) = "white" {}
		_Speed("Speed",float) = 1
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
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			sampler2D _GradientTex;
			float4 _MainTex_ST;
			float _Speed;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// get (grayscale texture) pixel color
				float gray = tex2D(_MainTex, i.uv).r;

				// get scrolling
				float scroll = frac(_Time.x*_Speed);

				// get gradient color from texture
				fixed4 col = tex2D(_GradientTex,float2(gray+scroll,0.5));

				return col;
			}
			ENDCG
		}
	}
}
