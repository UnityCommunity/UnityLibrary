// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)
// lights up sprites from both sides

Shader "Sprites/Diffuse (2 Sided Light)"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		[HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
		[HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
		[PerRendererData] _AlphaTex("External Alpha", 2D) = "white" {}
		[PerRendererData] _EnableExternalAlpha("Enable External Alpha", Float) = 0
	}

		SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
				"PreviewType" = "Plane"
				"CanUseSpriteAtlas" = "True"
			}

			Cull Off
			Lighting Off
			ZWrite Off
			Blend One OneMinusSrcAlpha

			CGPROGRAM
			//#pragma surface surf Lambert vertex:vert nofog nolightmap nodynlightmap keepalpha noinstancing
			#pragma surface surf TwoSidedLambert vertex:vert nofog nolightmap nodynlightmap keepalpha noinstancing
			#pragma multi_compile_local _ PIXELSNAP_ON
			#pragma multi_compile _ ETC1_EXTERNAL_ALPHA
			#include "UnitySprites.cginc"

			struct Input
			{
				float2 uv_MainTex;
				fixed4 color;
			};

			// added "abs" 2 sided lighting
			half4 LightingTwoSidedLambert(SurfaceOutput s, half3 lightDir, half atten) {
				half NdotL = abs(dot(s.Normal, lightDir));
				half4 c;
				c.rgb = s.Albedo * _LightColor0.rgb * (NdotL * atten);
				c.a = s.Alpha;
				return c;
			}

			void vert(inout appdata_full v, out Input o)
			{
				v.vertex = UnityFlipSprite(v.vertex, _Flip);

				#if defined(PIXELSNAP_ON)
				v.vertex = UnityPixelSnap(v.vertex);
				#endif

				//v.normal.z *= -1;

				UNITY_INITIALIZE_OUTPUT(Input, o);
				o.color = v.color * _Color * _RendererColor;
			}

			void surf(Input IN, inout SurfaceOutput o)
			{
				fixed4 c = SampleSpriteTexture(IN.uv_MainTex) * IN.color;
				o.Albedo = c.rgb * c.a;
				o.Alpha = c.a;
			}
			ENDCG
		}

			Fallback "Transparent/VertexLit"
}
