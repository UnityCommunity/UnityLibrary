
Shader "UnityCommunity/Sprites/Outline"
{
Properties
{
_MainTex ("Base (RGB)", 2D) = "white" {}
_OutLineSpread ("Outline Spread", Range(0,0.01)) = 0.007
_Color ("Tint", Color) = (1,1,1,1)
_ColorX ("Tint", Color) = (1,1,1,1)
_Alpha ("Alpha", Range (0,1)) = 1.0
}

SubShader
{
Tags {"Queue"="Transparent" "IgnoreProjector"="true" "RenderType"="Transparent"}
ZWrite Off Blend SrcAlpha OneMinusSrcAlpha Cull Off

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
float4 color    : COLOR;
float2 texcoord : TEXCOORD0;
};

struct v2f
{
half2 texcoord  : TEXCOORD0;
float4 vertex   : SV_POSITION;
fixed4 color    : COLOR;
};

sampler2D _MainTex;
float _OutLineSpread;
fixed4 _Color;
fixed4 _ColorX;

v2f vert(appdata_t IN)
{
v2f OUT;
OUT.vertex = UnityObjectToClipPos(IN.vertex);
OUT.texcoord = IN.texcoord;
OUT.color = IN.color;
return OUT;
}

float4 frag (v2f i) : COLOR
{

fixed4 mainColor = (tex2D(_MainTex, i.texcoord+float2(-_OutLineSpread,_OutLineSpread))
+ tex2D(_MainTex, i.texcoord+float2(_OutLineSpread,-_OutLineSpread))
+ tex2D(_MainTex, i.texcoord+float2(_OutLineSpread,_OutLineSpread))
+ tex2D(_MainTex, i.texcoord-float2(_OutLineSpread,_OutLineSpread)));

mainColor.rgb = _ColorX.rgb;

fixed4 addcolor = tex2D(_MainTex, i.texcoord)*i.color;

if (mainColor.a > 0.40) { mainColor = _ColorX; }
if (addcolor.a > 0.40) { mainColor = addcolor; mainColor.a = addcolor.a; }

return mainColor*i.color.a;
}
ENDCG
}
}
Fallback "Sprites/Default"

}
