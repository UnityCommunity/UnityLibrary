// HSV Debugger Shader by UnityCoder.com  - https://unitycoder.com/blog/2018/10/19/hsv-debugging-shader/

Shader "UnityLibrary/Debug/HSVDebugger"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	  _TargetColor ("Target Color", Color) = (1, 0, 0, 1)
		_HueThreshold ("Hue Threshold", Float) = 0.1
		_SatThreshold ("Saturation Threshold", Float) = 0.1
		_ValThreshold ("Value Threshold", Float) = 0.1
		[KeywordEnum(None, Hue, Saturation, Value, HueDistance, SaturationDistance, ValueDistance, ColorMatch, RemapHue)] _Mode ("Draw Mode",float) = 1
		_HueTex ("Hue Gradient Texture", 2D) = "white" {}
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _MODE_NONE _MODE_HUE _MODE_SATURATION _MODE_VALUE _MODE_HUEDISTANCE _MODE_SATURATIONDISTANCE _MODE_VALUEDISTANCE _MODE_COLORMATCH _MODE_REMAPHUE

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			sampler2D _HueTex;

			float4 _MainTex_ST;
			float _HueThreshold;
			float _SatThreshold;
			float _ValThreshold;
			float4 _TargetColor;
			
			// http://lolengine.net/blog/2013/07/27/rgb-to-hsv-in-glsl
			float3 rgb2hsv(float3 c)
			{
				float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
				float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
				float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
				float d = q.x - min(q.w, q.y);
				float e = 1.0e-10;
				return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// read main texture
				fixed4 tex = tex2D(_MainTex, i.uv);

				// convert colors
				float3 sourceHSV = rgb2hsv(tex.rgb);
				float3 targetHSV = rgb2hsv(_TargetColor.rbg);

				// get distances to our target color
				float hueDist = abs(sourceHSV.x - (1-targetHSV.x)); // why -1?
				float satDist = abs(sourceHSV.y - targetHSV.y);
				float valDist = abs(sourceHSV.z - targetHSV.z);

				float4 results = tex;

				// select results, based on enum dropdowm
				#ifdef _MODE_HUE
				results.rgb = sourceHSV.x;
				#endif

				#ifdef _MODE_SATURATION
				results.rgb = sourceHSV.y;
				#endif

				#ifdef _MODE_VALUE
				results.rgb = sourceHSV.z;
				#endif

				#ifdef _MODE_HUEDISTANCE
				results.rgb = hueDist;
				#endif

				#ifdef _MODE_SATURATIONDISTANCE
				results.rgb = satDist;
				#endif

				#ifdef _MODE_VALUEDISTANCE
				results.rgb = valDist;
				#endif

				#ifdef _MODE_REMAPHUE
				results.rgb = tex2D(_HueTex, float2(sourceHSV.x,0.5)).rgb;
				#endif

				#ifdef _MODE_COLORMATCH
				if (hueDist < _HueThreshold)
				{
					if (satDist < _SatThreshold)
					{
						if (valDist < _ValThreshold)
						{
							// display inverted color for matching area
							results.rgb = 1-results.rgb;
						}
					}
				}
				#endif

				// draw
				return results;
			}
			ENDCG
		} // pass
	} // subshader
} // shader
