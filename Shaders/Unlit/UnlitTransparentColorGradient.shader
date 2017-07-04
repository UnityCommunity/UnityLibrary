Shader "Custom/Unlit/Transparent Color Gradient" {
	Properties{
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" { }
	_Color("Color1", Color) = (1.000000,1.000000,1.000000,1.000000)
		_Color2("Color2", Color) = (1.000000,1.000000,1.000000,1.000000)
	}

		SubShader{
		LOD 100
		Tags{ "QUEUE" = "Transparent" "IGNOREPROJECTOR" = "true" "RenderType" = "Transparent" }
		Pass{
		Tags{ "QUEUE" = "Transparent" "IGNOREPROJECTOR" = "true" "RenderType" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha

		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma target 2.0
#include "UnityCG.cginc"
#pragma multi_compile_fog
#define USING_FOG (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2))

		// uniforms
		float4 _MainTex_ST;

	// vertex shader input data
	struct appdata {
		float3 pos : POSITION;
		float3 uv0 : TEXCOORD0;
	};

	// vertex-to-fragment interpolators
	struct v2f {
		fixed4 color : COLOR0;
		float2 uv0 : TEXCOORD0;
#if USING_FOG
		fixed fog : TEXCOORD1;
#endif
		float4 pos : SV_POSITION;
		float4 screenPos: TEXCOORD2;
	};

	// vertex shader
	v2f vert(appdata IN) {
		v2f o;
		half4 color = half4(0,0,0,1.1);
		float3 eyePos = mul(UNITY_MATRIX_MV, float4(IN.pos,1)).xyz;
		half3 viewDir = 0.0;
		o.color = saturate(color);
		// compute texture coordinates
		o.uv0 = IN.uv0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
		// fog
#if USING_FOG
		float fogCoord = length(eyePos.xyz); // radial fog distance
		UNITY_CALC_FOG_FACTOR(fogCoord);
		o.fog = saturate(unityFogFactor);
#endif
		// transform position
		o.pos = UnityObjectToClipPos(IN.pos);
		o.screenPos = ComputeScreenPos(o.pos);
		return o;
	}

	// textures
	sampler2D _MainTex;
	fixed4 _Color;
	fixed4 _Color2;

	// fragment shader
	fixed4 frag(v2f IN) : SV_Target{
		fixed4 col;
	fixed4 tex, tmp0, tmp1, tmp2;
	// SetTexture #0
	tex = tex2D(_MainTex, IN.uv0.xy);

	float2 screenPosition = (IN.screenPos.xy / IN.screenPos.w);

	//float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
	fixed4 color = lerp(_Color, _Color2, screenPosition.x);

	col.rgb = tex * color;
	col.a = tex.a * color.a;
	// fog
#if USING_FOG
	col.rgb = lerp(unity_FogColor.rgb, col.rgb, IN.fog);
#endif
	return col;
	}

		// texenvs
		//! TexEnv0: 01010102 01050106 [_MainTex] [_Color]
		ENDCG
	}
	}
}