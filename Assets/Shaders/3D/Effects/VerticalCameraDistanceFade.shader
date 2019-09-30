// similar to height fog effect, but starts fading from camera.Y distance (with some offset)
// modified standard shader, with custom lighting pass to make faded part completely black

Shader "UnityLibrary/Standard/Effects/VerticalCameraDistance"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		//_BumpMap("Normalmap", 2D) = "bump" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_FadeToColor("FadeToColor", Color) = (0,0,0,1)
		_FadeStartY("FadeStartFromCameraY", Float) = -2
		_FadeEndY("FadeEndFromCameraY", Float) = -4
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM

//			#pragma surface surf Standard fullforwardshadows vertex:vert
		#pragma surface surf Custom fullforwardshadows // vertex:vert
		#include "UnityPBSLighting.cginc"

		struct appdata {
			float4 vertex : POSITION;
			float4 tangent : TANGENT;
			float3 normal : NORMAL;
			float2 texcoord : TEXCOORD0;
			float2 texcoord1 : TEXCOORD1;
			float2 texcoord2 : TEXCOORD2;
		};

		struct Input {
			float2 uv_MainTex;
			//float2 uv_BumpMap;
			float3 worldPos;
		};

			// Metallic workflow
			// modified from UnityPBSLighting.cginc
			struct SurfaceOutputStandardCustom
			{
				fixed3 Albedo;      // base (diffuse or specular) color
				float3 Normal;      // tangent space normal, if written
				half3 Emission;
				half Metallic;      // 0=non-metal, 1=metal
				half Smoothness;    // 0=rough, 1=smooth
				half Occlusion;     // occlusion (default 1)
				fixed Alpha;        // alpha for transparencies
				float Fader; // test
			};

			sampler2D _MainTex;
			//sampler2D _BumpMap;

			half _Glossiness;
			half _Metallic;
			fixed4 _Color;
			fixed4 _FadeToColor;
			float _FadeStartY;
			float _FadeEndY;

			float remap(float source, float sourceFrom, float sourceTo, float targetFrom, float targetTo)
			{
				return targetFrom + (source - sourceFrom)*(targetTo - targetFrom) / (sourceTo - sourceFrom);
			}

			// from UnityPBSLighting.cginc
			inline half4 LightingCustom(SurfaceOutputStandardCustom s, float3 viewDir, UnityGI gi)
			{
				s.Normal = normalize(s.Normal);

				half oneMinusReflectivity;
				half3 specColor;
				s.Albedo = DiffuseAndSpecularFromMetallic(s.Albedo, s.Metallic, /*out*/ specColor, /*out*/ oneMinusReflectivity);

				// shader relies on pre-multiply alpha-blend (_SrcBlend = One, _DstBlend = OneMinusSrcAlpha)
				// this is necessary to handle transparency in physically correct way - only diffuse component gets affected by alpha
				half outputAlpha;
				s.Albedo = PreMultiplyAlpha(s.Albedo, s.Alpha, oneMinusReflectivity, /*out*/ outputAlpha);

				half4 c = UNITY_BRDF_PBS(s.Albedo, specColor, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, gi.light, gi.indirect);
				c.a = outputAlpha;

				// make it black if outside range
				return c*(1 - s.Fader);
			}

			inline void LightingCustom_GI(SurfaceOutputStandardCustom s,UnityGIInput data,inout UnityGI gi)
			{
				#if defined(UNITY_PASS_DEFERRED) && UNITY_ENABLE_REFLECTION_BUFFERS
				gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal);
				#else
				Unity_GlossyEnvironmentData g = UnityGlossyEnvironmentSetup(s.Smoothness, data.worldViewDir, s.Normal, lerp(unity_ColorSpaceDielectricSpec.rgb, s.Albedo, s.Metallic));
				gi = UnityGlobalIllumination(data, s.Occlusion, s.Normal, g);
				#endif
			}

			void surf(Input IN, inout SurfaceOutputStandardCustom o)
			{
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;

				// fade
				float fadeStartY = _WorldSpaceCameraPos.y + _FadeStartY;
				float fadeEndY = _WorldSpaceCameraPos.y + _FadeEndY;
				float pixelY = clamp(IN.worldPos.y, fadeEndY, fadeStartY);
				float distanceY = remap(pixelY, fadeStartY, fadeEndY, 0, 1);
				c.rgb = lerp(c.rgb, _FadeToColor, distanceY);

				o.Fader = distanceY;
				o.Albedo = c.rgb;
				// Metallic and smoothness come from slider variables
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				//o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
				o.Alpha = c.a;

			}
			ENDCG
	}
	FallBack "Diffuse"
}
