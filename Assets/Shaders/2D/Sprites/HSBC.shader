// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/HSBC Effect"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Hue ("Hue", Range(0, 1.0)) = 0
		_Saturation ("Saturation", Range(0, 1.0)) = 0.5
		_Brightness ("Brightness", Range(0, 1.0)) = 0.5
		_Contrast ("Contrast", Range(0, 1.0)) = 0.5
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
			
			#include "UnityCG.cginc"
			

			inline float3 applyHue(float3 aColor, float aHue)
            {
                float angle = radians(aHue);
                float3 k = float3(0.57735, 0.57735, 0.57735);
                float cosAngle = cos(angle);
                
                return aColor * cosAngle + cross(k, aColor) * sin(angle) + k * dot(k, aColor) * (1 - cosAngle);
            }
			
			inline float4 applyHSBCEffect(float4 startColor, fixed4 hsbc)
            {
                float hue = 360 * hsbc.r;
                float saturation = hsbc.g * 2;
                float brightness = hsbc.b * 2 - 1;
                float contrast = hsbc.a * 2;
 
                float4 outputColor = startColor;
                outputColor.rgb = applyHue(outputColor.rgb, hue);
                outputColor.rgb = (outputColor.rgb - 0.5f) * contrast + 0.5f;
                outputColor.rgb = outputColor.rgb + brightness;
                float3 intensity = dot(outputColor.rgb, float3(0.39, 0.59, 0.11));
    			outputColor.rgb = lerp(intensity, outputColor.rgb, saturation);
                 
                return outputColor;
            }

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				fixed4 hsbc : COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed _Hue, _Saturation, _Brightness, _Contrast;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.hsbc = fixed4(_Hue, _Saturation, _Brightness, _Contrast);
				
				return o;
			}
			
			fixed4 frag (v2f i) : COLOR
			{
				fixed4 startColor = tex2D(_MainTex, i.uv);				
				float4 hsbcColor = applyHSBCEffect(startColor, i.hsbc);
				
				return hsbcColor*startColor.a;
			}
			ENDCG
		}
	}
}