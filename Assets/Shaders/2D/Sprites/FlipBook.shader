// https://unitycoder.com/blog/2018/11/30/sprite-sheet-flip-book-shader/

Shader "UnityLibrary/Sprites/FlipBook (AlphaTest)"
{
	Properties
	{
		[Header(Texture Sheet)]
		_MainTex ("Texture", 2D) = "white" {}
		_Cutoff("Alpha Cutoff", Range(0,1)) = 0.15
		[Header(Settings)]
		_ColumnsX("Columns (X)", int) = 1
		_RowsY("Rows (Y)", int) = 1
		_AnimationSpeed("Frames Per Seconds", float) = 10
	}
	SubShader
	{
		Tags {
			"Queue" = "AlphaTest"
			"IgnoreProjector" = "True"
			"PreviewType" = "Plane"
			"RenderType" = "TransparentCutout"
			"DisableBatching" = "True"
		}

		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert 
			#pragma fragment frag
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

			float _Cutoff;
			sampler2D _MainTex;
			float4 _MainTex_ST;
			int _ColumnsX;
			int _RowsY;
			float _AnimationSpeed;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);

				// based on http://wiki.unity3d.com/index.php?title=Animating_Tiled_texture

				// Calculate index
				float index = floor(_Time.y*_AnimationSpeed);

				// repeat when exhausting all frames
				index = index % (_ColumnsX * _RowsY);

				// Size of tile
				float2 size = float2(1.0f / _ColumnsX, 1.0f / _RowsY);

				// split into horizontal and vertical index
				float uIndex = floor(index % _ColumnsX);
				float vIndex = floor(index / _RowsY);

				// build offset
				// v coordinate is the bottom of the image in opengl so we need to invert.
				float2 offset = float2(uIndex * size.x, 1.0f - size.y - vIndex * size.y);

				o.uv = v.uv*size + offset;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);

				// cutout
				clip(col.a - _Cutoff);

				return col;
			}
			ENDCG
		}
	}
}
