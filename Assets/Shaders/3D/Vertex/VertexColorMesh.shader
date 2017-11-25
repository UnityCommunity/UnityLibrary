// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// vertex colors shader for mesh

Shader "UnityCommunity/Vertex/VertexColorMesh"
{
	SubShader
	{
		Tags { "Queue"="Geometry"}
//		Lighting Off
		Fog { Mode Off }
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			
			struct appdata
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
			};
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				fixed4 color : COLOR;
			};
		
			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				return o;
			}
			
			half4 frag(v2f i) : COLOR
			{
				return i.color;
			}
			ENDCG
		}
	}
}
