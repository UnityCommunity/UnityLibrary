// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// simplified skybox shader (unity 5.3.x)
// removed tint, rotation, exposure, HDR calculation

Shader "UnityLibrary/Skybox/Cubemap-Simple" 
{
	Properties {
		[NoScaleOffset] _Tex ("Cubemap   (HDR)", Cube) = "grey" {}
	}

	SubShader {
		Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
		Cull Off ZWrite Off

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			samplerCUBE _Tex;
		
			struct appdata_t {
				float4 vertex : POSITION;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float3 texcoord : TEXCOORD0;
			};

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.texcoord = v.vertex.xyz;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				return half4(texCUBE(_Tex, i.texcoord).rgb,1);
			}
			ENDCG 
		}
	} 	
	Fallback Off
}
