// https://unitycoder.com/blog/2018/11/30/sprite-sheet-flip-book-shader/

Shader "UnityLibrary/Sprites/FlipBook (Cutout)"
{
	Properties
	{
		[Header(Texture Sheet)]
		_MainTex("Texture", 2D) = "white" {}
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
			uint _ColumnsX;
			uint _RowsY;
			float _AnimationSpeed;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);

				// get single sprite size
				float2 size = float2(1.0f / _ColumnsX, 1.0f / _RowsY);
				uint totalFrames = _ColumnsX * _RowsY;

				// use timer to increment index
				uint index = _Time.y*_AnimationSpeed;

				// wrap x and y indexes
				uint indexX = index % _ColumnsX;
				uint indexY = floor((index % totalFrames) / _ColumnsX);

				// get offsets to our sprite index
				float2 offset = float2(size.x*indexX,-size.y*indexY);

				// get single sprite UV
				float2 newUV = v.uv*size;

				// flip Y (to start 0 from top)
				newUV.y = newUV.y + size.y*(_RowsY - 1);

				o.uv = newUV + offset;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
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
