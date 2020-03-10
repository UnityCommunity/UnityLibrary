// source https://forum.unity.com/threads/adding-hue-saturation-and-contrast-to-standardshader.843697/#post-5572063

Shader "UnityLibrary/Standard/HueContrastSaturation"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo Map", 2D) = "black" {}
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _MetallicGlossMap ("Metallic (R) Smoothness (A) Map", 2D) = "black" {}
        _Hue ("Hue", Float) = 1.0
        _Contrast ("Contrast", Float) = 1.0
        _Saturation ("Saturation", Float) = 1.0      
    }

    Subshader
    {
        Tags { "RenderType" = "Opaque" }
        CGPROGRAM
        #pragma surface SurfaceShader Standard fullforwardshadows addshadow

        sampler2D _MainTex, _BumpMap, _MetallicGlossMap;
        float4 _Color;
        float _Hue, _Contrast, _Saturation;

        float4x4 contrastMatrix (float c)
        {
            float t = (1.0 - c) * 0.5;
            return float4x4 (c, 0, 0, 0, 0, c, 0, 0, 0, 0, c, 0, t, t, t, 1);
        }

        float3 RGBToHSV(float3 c)
        {
            float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
            float4 p = lerp( float4( c.bg, K.wz ), float4( c.gb, K.xy ), step( c.b, c.g ) );
            float4 q = lerp( float4( p.xyw, c.r ), float4( c.r, p.yzx ), step( p.x, c.r ) );
            float d = q.x - min( q.w, q.y );
            float e = 1.0e-10;
            return float3( abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
        }

        float3 HSVToRGB( float3 c )
        {
            float4 K = float4( 1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0 );
            float3 p = abs( frac( c.xxx + K.xyz ) * 6.0 - K.www );
            return c.z * lerp( K.xxx, saturate( p - K.xxx ), c.y );
        }

        float3 Hue( float3 p, float v )
        {
            p = RGBToHSV(p);
            p.x *= v;
            return HSVToRGB(p);
        }

        float3 Saturation( float3 p, float v )
        {
            p = RGBToHSV(p);
            p.y *= v;
            return HSVToRGB(p);
        }

        float3 Contrast( float3 p, float v )
        {
            return mul(float4(p,1.0), contrastMatrix(v)).rgb;
        }

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
            float2 uv_MetallicGlossMap;
        };

        void SurfaceShader (Input IN, inout SurfaceOutputStandard o)
        {
            float4 color = tex2D(_MainTex,IN.uv_MainTex) * _Color;
            color.rgb = Hue(color.rgb, _Hue);
            color.rgb = Saturation(color.rgb, _Saturation);
            color.rgb = Contrast(color.rgb, _Contrast);
            o.Albedo = color;
            o.Normal = UnpackNormal (tex2D(_BumpMap, IN.uv_BumpMap));
            o.Metallic = tex2D(_MetallicGlossMap, IN.uv_MetallicGlossMap).r;
            o.Smoothness = tex2D(_MetallicGlossMap, IN.uv_MetallicGlossMap).a;
        }

        ENDCG
    }
}
