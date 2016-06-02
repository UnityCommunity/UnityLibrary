// source: http://forum.unity3d.com/threads/fabio-policarpo-relief-mapping-with-correct-silhouettes.32451/page-2#post-518105

Shader "UnityLibrary/ReliefMapping" 
{
	Properties 
	{
		_Color 				("Main Color", Color) = (1,1,1,1)
		_SpecColor			("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
		_Height 				("Height", Float) = -0.05
		_Tile 					("Tile Factor", Float) = 1
		_Cutoff 				("Alpha cutoff", Range(0,1)) = 0.5
		_useAlpha			("Use Alpha", Range(0,1)) = 1
		_Shininess 			("Shininess", Range (0.01, 1)) = 0.078125
		_MainTex 			("Base (RGB), Spec (A)", 2D) = "white" {}
		_NormalMap 		("Normalmap", 2D) = "bump" {}
		_HeightMap 		("Height (A)", 2D) = "bump" {}
	}

	SubShader 
	{
		Tags {"IgnoreProjector"="True" "RenderType"="TransparentCutout"}
		LOD 300

		CGPROGRAM
		#pragma surface surf BlinnPhong alphatest:_Cutoff  
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _NormalMap;
		sampler2D _HeightMap;
		float4 _Color;
		float _Height;
		float _Tile;
		float _Shininess;
		float _useAlpha;
		
		struct Input { // vertex input
			
			float2 uv_MainTex;
			float2 uv_NormalMap;
			float2 uv_HeightMap;
			float3 viewDir;

		};
			
		void surf (Input IN, inout SurfaceOutput o) {
			
			IN.uv_MainTex *= _Tile;
			IN.viewDir 	= normalize(IN.viewDir); 
			
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			//setup the view ray
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			float3 p 					= float3(IN.uv_MainTex,0);
			float3 v 					= normalize(IN.viewDir*-1);
			v.z						= abs(v.z);
			
			//depth bias
			float depthBias		= 1.0 - v.z;
			depthBias				*= depthBias;
			depthBias 				*= depthBias;
			depthBias				= 1.0 - depthBias * depthBias; 
			
			v.xy						*= depthBias;
			v.xy						*= _Height;
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			//ray intersection
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			const int linearSearchSteps = 20;
			const int binarySearchSteps = 10;
			
			v /= v.z * linearSearchSteps;
			
			int i;
			for( i=0;i<linearSearchSteps;i++ )
			{
				//float tex = tex2D(_HeightMap, p.xy).a;
				float tex = tex2D(_HeightMap, p.xy).r; // using red channel instead of alpha
				if (p.z<tex)		p+=v;
			}
			
			for( i=0;i<binarySearchSteps;i++ )
			{
				v *= 0.5;

//				float tex = tex2D(_HeightMap, p.xy).a;
				float tex = tex2D(_HeightMap, p.xy).r;

				if (p.z < tex)		p += v;	else	p -= v;
			}
			
			
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
			//final output
			//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~			
			half4 tex 			= tex2D(_MainTex, p.xy);
			
			half3 normal 		= UnpackNormal(tex2D(_NormalMap,p.xy));	// normal map

			normal.z 			= sqrt(1.0 - dot(normal.xy,  normal.xy));
			o.Normal 			= normal; 
			
			if(_useAlpha){
				float alpha						=	1;		// border clamp
				if (p.x < 0) 	 	alpha		=	0;
				if (p.y < 0) 		alpha		=	0;
				if (p.x > _Tile)	alpha		=	0;
				if (p.y > _Tile)	alpha		=	0;
				
				o.Alpha = alpha;
			}
			
			o.Gloss = tex.a;
			o.Specular = _Shininess;
			o.Albedo = tex.rgb * _Color.rgb;

		} // surf

	ENDCG  
	}
	FallBack "Bumped Specular"
}
