// source: http://forum.unity3d.com/threads/surface-shader-fresnel-reflective-bumped-specular.63250/

Shader "Reflective/Bumped Specular Fresnel" 
{
	Properties 
	{
        _Color ("Main Color", Color) = (1,1,1,1)
        _SpecColor ("Specular Color", Color) = (0.5,0.5,0.5,1)
        _Shininess ("Shininess", Range (0.01, 1)) = 0.078125
        _ReflectColor ("Reflection Color", Color) = (1,1,1,0.5)
        _MainTex ("Base (RGB) RefStrGloss (A)", 2D) = "white" {}
        _Cube ("Reflection Cubemap", Cube) = "" {}
        _BumpMap ("Normalmap", 2D) = "bump" {}
        _Fresnel("Fresnel Coef.", Range (0.01, 0.7)) = 0.35
	}
     
	SubShader 
	{
	Tags { "RenderType"="Opaque" }
	LOD 400
	CGPROGRAM
	#pragma surface surf BlinnPhong
	#pragma target 3.0
     
	sampler2D _MainTex;
	sampler2D _BumpMap;
	samplerCUBE _Cube;
     
	float4 _Color;
	float4 _ReflectColor;
	float _Shininess;
	float _Fresnel;
     
	struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
			//float3 normal;
			float3 viewDir;
			float3 worldRefl;
			INTERNAL_DATA
	};
     
	//taken from somewhere in internet
	float fresnel(float VdotN, float eta)
	{
		float sqr_eta = eta * eta; // square of IOR
		float etaCos = eta * VdotN; // η·cos(Θ)
		float sqr_etaCos = etaCos*etaCos; // squared
		float one_minSqrEta = 1.0 - sqr_eta; // 1 – η2
		float value = etaCos - sqrt(one_minSqrEta + sqr_etaCos);
		value *= value / one_minSqrEta; // square and divide by 1 – η2
		return min(1.0, value * value); // square again
	}
     
	//float fFresnel = fresnel(dot(vViewNormal, vNormalWS), indexOfRefraction);
     
	void surf (Input IN, inout SurfaceOutput o) 
	{
		half4 tex = tex2D(_MainTex, IN.uv_MainTex);
		half4 c = tex * _Color;
		o.Albedo = c.rgb;
           
		o.Gloss = tex.a;
		o.Specular = _Shininess;
           
		o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
           
		float3 worldRefl = WorldReflectionVector (IN, o.Normal);
     
		half4 reflcol = texCUBE (_Cube, worldRefl);
		reflcol *= tex.a;
           
		//reflcol = lerp(c, reflcol, fresnel(worldRefl, _Fresnel));
		reflcol = lerp(c, reflcol, fresnel(dot(normalize(IN.viewDir),o.Normal), _Fresnel));
     
		o.Emission = reflcol.rgb * _ReflectColor.rgb;
		o.Alpha = reflcol.a * _ReflectColor.a;
	}
	ENDCG
	}
     
	FallBack "Reflective/Bumped Diffuse"
}
