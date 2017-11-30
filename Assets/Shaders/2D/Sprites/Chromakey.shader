Shader "UnityCommunity/ChromakeyTransparent" {
Properties
{
_MainTex ("Base (RGB)", 2D) = "white" {}
_MaskCol ("Mask Color", Color)  = (1.0, 0.0, 0.0, 1.0)
_Sensitivity ("Threshold Sensitivity", Range(0,1)) = 0.5
_Smooth ("Smoothing", Range(0,1)) = 0.5
}

SubShader
{
Tags {"Queue"="Transparent" "IgnoreProjector"="true" "RenderType"="Transparent"}
 Blend SrcAlpha OneMinusSrcAlpha Cull Off

Pass
{

CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma fragmentoption ARB_precision_hint_fastest
#include "UnityCG.cginc"

struct appdata_t
{
float4 vertex   : POSITION;
float2 texcoord : TEXCOORD0;
};

struct v2f
{
half2 texcoord  : TEXCOORD0;
float4 vertex   : SV_POSITION;
};

sampler2D _MainTex;
fixed4 _Color;
float _Speed;

float4 _MaskCol;
float _Sensitivity;
float _Smooth;


v2f vert(appdata_t IN)
{
v2f OUT;
OUT.vertex = UnityObjectToClipPos(IN.vertex);
OUT.texcoord = IN.texcoord;
return OUT;
}

float4 frag (v2f i) : COLOR
{

float2 uv = i.texcoord.xy;
float4 c = tex2D(_MainTex, uv);

float maskY = 0.2989 * _MaskCol.r + 0.5866 * _MaskCol.g + 0.1145 * _MaskCol.b;
float maskCr = 0.7132 * (_MaskCol.r - maskY);
float maskCb = 0.5647 * (_MaskCol.b - maskY);

float Y = 0.2989 * c.r + 0.5866 * c.g + 0.1145 * c.b;
float Cr = 0.7132 * (c.r - Y);
float Cb = 0.5647 * (c.b - Y);

float blendValue = smoothstep(_Sensitivity, _Sensitivity + _Smooth, distance(float2(Cr, Cb), float2(maskCr, maskCb)));

return float4(c.rgb,c.a*blendValue);
}
ENDCG
}
}
Fallback "Sprites/Default"
}