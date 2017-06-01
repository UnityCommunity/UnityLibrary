// Unity Tree Billboard (not speedtree) replacement shader that works with Legacy Global Fog
// topic: https://forum.unity3d.com/threads/global-fog-doesnt-affect-billboard-trees.473300/

Shader "Hidden/TerrainEngine/BillboardTree" {
	Properties {
		_MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
		_Cutoff("Cutoff", float) = 0.33 // adjust this value here in shader code
	}
	
	SubShader {
		Tags { "Queue" = "AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout" }
		
		Pass {
			ColorMask rgb
			ZWrite Off Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#include "UnityCG.cginc"
			#include "TerrainEngine.cginc"

			struct v2f {
				float4 pos : SV_POSITION;
				fixed4 color : COLOR0;
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert (appdata_tree_billboard v) {
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				TerrainBillboardTree(v.vertex, v.texcoord1.xy, v.texcoord.y);
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv.x = v.texcoord.x;
				o.uv.y = v.texcoord.y > 0;
				o.color = v.color;
				UNITY_TRANSFER_FOG(o,o.pos);
				return o;
			}

			fixed _Cutoff;
			sampler2D _MainTex;
			fixed4 frag(v2f input) : SV_Target
			{
				fixed4 col = tex2D( _MainTex, input.uv);
				col.rgb *= input.color.rgb;
				clip(col.a-_Cutoff);
				UNITY_APPLY_FOG(input.fogCoord, col);
				return col;
			}
			ENDCG			
		}
	}
	Fallback Off
}
