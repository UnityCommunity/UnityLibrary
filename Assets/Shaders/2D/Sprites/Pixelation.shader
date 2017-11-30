Shader "UnityCommunity/Sprites/Pixelation"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		size ("Pixel size",Range(1.0,1000.0)) = 100.0
	}
	Subshader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vertex_shader
			#pragma fragment pixel_shader
			#pragma target 2.0

			sampler2D _MainTex;
			float size;
			
			struct custom_type
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};
			
			custom_type vertex_shader (float4 vertex:POSITION, float2 uv:TEXCOORD0)
			{
				custom_type vs;
				vs.vertex = UnityObjectToClipPos (vertex);
				vs.uv = uv;
				return vs;
			}

			float4 pixel_shader (custom_type ps) : COLOR
			{
				return tex2D(_MainTex,ceil(ps.uv.xy*size)/size);	
			}			
			ENDCG
		}
	}
}