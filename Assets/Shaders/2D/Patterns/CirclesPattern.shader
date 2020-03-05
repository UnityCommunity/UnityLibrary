// draws circle pattern
Shader "UnityLibrary/2D/Patterns/Circles"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _CircleSize("Size", Range(0,1)) = 0.5
        _Circles("Amount", Range(1,64)) = 8
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows vertex:vert
        #pragma target 3.0

        struct Input
        {
            float2 texcoord : TEXCOORD0;
        };

        sampler2D _MainTex;
        float _Circles;
        float _CircleSize;

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        // https://thebookofshaders.com/09/
        float2 tile(float2 _st, float _zoom)
        {
            _st *= _zoom;
            return frac(_st);
        }

        // https://thebookofshaders.com/07/
        float Circle(float2 _st, float _radius)
        {
            float2 dist = _st - float2(0.5, 0.5);
            return 1. - smoothstep(_radius - (_radius * 0.01), _radius + (_radius * 0.01), dot(dist, dist) * 4.0);
        }

        void vert(inout appdata_full v, out Input o) 
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.texcoord = v.texcoord;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float2 st = IN.texcoord.xy;
            float c = Circle(tile(st, round(_Circles)), _CircleSize);
            o.Albedo = c.rrr* _Color;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.r;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
