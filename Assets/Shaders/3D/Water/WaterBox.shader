// water waves on a single mesh box
// single mesh approach. It's a single 64x64x1 unit box with 128x128 divisions.
// The shader is checking if the starting y for the vertex is >0.5 before it modifies the position,
// and checking the normal is facing up before modifying that
// source: @bgolus https://forum.unity.com/threads/intersect-box-with-wave-plane.826761/#post-5475174

Shader "Custom/WaterBox"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Color2 ("Color 2", Color) = (1,1,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200

        CGINCLUDE
        struct Input
        {
            float h;
        };

        half _Glossiness;
        fixed4 _Color, _Color2;

        void waves(inout appdata_full v, inout Input IN)
        {
            IN.h = 0.0;

            float s0x, c0x, s1x, c1x;
            float s0y, c0y, s1y, c1y;
            sincos(v.vertex.x * 0.6 + _Time.y + v.vertex.z * 0.1, s0x, c0x);
            sincos(v.vertex.z * 0.5 + _Time.y * 2.0 + v.vertex.x * 0.1, s0y, c0y);
            sincos(v.vertex.x * 0.3 + _Time.y - v.vertex.z * 0.1, s1x, c1x);
            sincos(v.vertex.z * 0.21 + _Time.y * 2.0 - v.vertex.x * 0.1, s1y, c1y);

            float offset = (s0x + s0y) * 0.15 + (s1x + s1y) * 1.0 + 1.5;
            if (v.vertex.y > 0.5)
            {
                v.vertex.y += offset;
                IN.h = 0.0;
                if (v.normal.y > 0.7)
                {
                    v.normal = normalize(v.normal + float3(c0y * 0.15 + c1y * 0.3, 0.0, c0x * 0.15 + c1x * 0.3));
                }
            }
            else
            {
                IN.h = offset + 1.0;
            }
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 color = lerp(_Color2, _Color, smoothstep(1.5, 0.0, pow(IN.h, 0.5)));
            o.Albedo = color.rgb;
            o.Metallic = 0;
            o.Smoothness = _Glossiness;
            o.Alpha = color.a;
        }
        ENDCG

        Cull Front
        CGPROGRAM
        #pragma surface surf Standard alpha:premul vertex:vert
        #pragma target 3.0

        void vert(inout appdata_full v, out Input IN)
        {
           waves(v, IN);
           v.normal = -v.normal;
        }
        ENDCG

        Cull Back
        CGPROGRAM
        #pragma surface surf Standard alpha:premul vertex:vert
        #pragma target 3.0

        void vert(inout appdata_full v, out Input IN)
        {
           waves(v, IN);
        }
        ENDCG
    }
}
