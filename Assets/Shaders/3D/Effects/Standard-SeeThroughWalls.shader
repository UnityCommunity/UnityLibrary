// 2 pass shader to draw object with blurry texture and slightly transparent when behind walls

Shader "UnityLibrary/Standard/Effects/SeeThroughWalls"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_BlurSize("BlurSize", float) = 10
		_SeeThroughOpacity("SeeThroughOpacityAdjust", Range(0,1)) = 0.5
	}

		SubShader
		{
			// Regular Pass
			Tags { "RenderType" = "Opaque" }
			LOD 200

			CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma surface surf Standard fullforwardshadows

			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.0

			sampler2D _MainTex;

			struct Input
			{
				float2 uv_MainTex;
			};

			half _Glossiness;
			half _Metallic;
			fixed4 _Color;

			// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
			// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
			// #pragma instancing_options assumeuniformscaling
			UNITY_INSTANCING_BUFFER_START(Props)
				// put more per-instance properties here
			UNITY_INSTANCING_BUFFER_END(Props)

			void surf(Input IN, inout SurfaceOutputStandard o)
			{
				// Albedo comes from a texture tinted by color
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
				o.Albedo = c.rgb;
				// Metallic and smoothness come from slider variables
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				o.Alpha = c.a;
			}
			ENDCG

			// see through pass

			Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
			LOD 200
//			Blend OneMinusDstColor One // Soft Additive
			Blend SrcAlpha OneMinusSrcAlpha // Traditional transparency
//			Blend One OneMinusSrcAlpha // Premultiplied transparency
//			Blend One One // Additive
//			Blend OneMinusDstColor One // Soft Additive
//			Blend DstColor Zero // Multiplicative
//			Blend DstColor SrcColor // 2x Multiplicative

			ZWrite Off
			ZTest Greater

			CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
			#pragma surface surf Standard alpha:fade

			// Use shader model 3.0 target, to get nicer looking lighting
			#pragma target 3.0

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			float _BlurSize;
			float _SeeThroughOpacity;

			struct Input
			{
				float2 uv_MainTex;
			};

			half _Glossiness;
			half _Metallic;
			fixed4 _Color;

			UNITY_INSTANCING_BUFFER_START(Props)
				// put more per-instance properties here
				UNITY_INSTANCING_BUFFER_END(Props)

				void surf(Input IN, inout SurfaceOutputStandard o)
				{
				// small blur effect on the texture
				fixed4 cR = tex2D(_MainTex, float2(IN.uv_MainTex.x + _MainTex_TexelSize.x * _BlurSize,IN.uv_MainTex.y));
				fixed4 cL = tex2D(_MainTex, float2(IN.uv_MainTex.x - _MainTex_TexelSize.x * _BlurSize,IN.uv_MainTex.y));
				fixed4 cT = tex2D(_MainTex, float2(IN.uv_MainTex.x,IN.uv_MainTex.y + _MainTex_TexelSize.y * _BlurSize));
				fixed4 cB = tex2D(_MainTex, float2(IN.uv_MainTex.x,IN.uv_MainTex.y - _MainTex_TexelSize.y * _BlurSize));
				fixed4 c = (cR + cL + cT + cB) * 0.25;
				o.Albedo = c;
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
				// make object also bit transparent
				o.Alpha = c.a* _SeeThroughOpacity;
			}
			ENDCG

		} // subshader
			FallBack "Diffuse"
} // shader
