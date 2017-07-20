// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// source: http://forum.unity3d.com/threads/tangent-space-normal-map-seams-problem-fixed.384063/#post-2496937

Shader "Custom/DebugNormalsTangents" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		[NoScaleOffset] _BumpMap ("Normal Map", 2D) = "bump" {}
		[KeywordEnum(None, Mesh Normals, Mesh Tangents, Tangent Normals, World Normals)] _Display ("Debug Display", Float) = 0
	}
 
SubShader {
    Tags { "RenderType"="Opaque" }
    LOD 100
   
    Pass {  
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _DISPLAY_MESH_NORMALS _DISPLAY_MESH_TANGENTS _DISPLAY_TANGENT_NORMALS _DISPLAY_WORLD_NORMALS
           
            #include "UnityCG.cginc"
 
            struct v2f {
                float4 vertex : SV_POSITION;
                half2 texcoord : TEXCOORD0;
                half3 tspace0 : TEXCOORD1; // tangent.x, bitangent.x, normal.x
                half3 tspace1 : TEXCOORD2; // tangent.y, bitangent.y, normal.y
                half3 tspace2 : TEXCOORD3; // tangent.z, bitangent.z, normal.z
            };
 
            sampler2D _MainTex;
            sampler2D _BumpMap;
 
            fixed4 _LightColor0;
           
            v2f vert (appdata_tan v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
 
                half3 wNormal = UnityObjectToWorldNormal(v.normal);
                half3 wTangent = UnityObjectToWorldDir(v.tangent.xyz);
                // compute bitangent from cross product of normal and tangent
                half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                half3 wBitangent = cross(wNormal, wTangent) * tangentSign;
                // output the tangent space matrix
                o.tspace0 = half3(wTangent.x, wBitangent.x, wNormal.x);
                o.tspace1 = half3(wTangent.y, wBitangent.y, wNormal.y);
                o.tspace2 = half3(wTangent.z, wBitangent.z, wNormal.z);
                return o;
            }
           
            fixed4 frag (v2f i) : SV_Target
            {
                #ifdef _DISPLAY_MESH_NORMALS
                return fixed4(fixed3(i.tspace0.z, i.tspace1.z, i.tspace2.z) * 0.5 + 0.5, 1);
                #endif
 
                #ifdef _DISPLAY_MESH_TANGENTS
                return fixed4(fixed3(i.tspace0.x, i.tspace1.x, i.tspace2.x) * 0.5 + 0.5, 1);
                #endif
 
                half3 tnormal = UnpackNormal(tex2D(_BumpMap, i.texcoord));
 
                #ifdef _DISPLAY_TANGENT_NORMALS
                return fixed4(tnormal * 0.5 + 0.5, 1);
                #endif
 
                half3 worldNormal;
                worldNormal.x = dot(i.tspace0, tnormal);
                worldNormal.y = dot(i.tspace1, tnormal);
                worldNormal.z = dot(i.tspace2, tnormal);
 
                #ifdef _DISPLAY_WORLD_NORMALS
                return fixed4(worldNormal * 0.5 + 0.5, 1);
                #endif
 
                fixed4 col = tex2D(_MainTex, i.texcoord);
                half ndotl = dot(worldNormal, -_WorldSpaceLightPos0.xyz);
                col.rgb *= (ndotl * 0.5 + 0.5) * _LightColor0.rgb;
 
                return col;
            }
        ENDCG
    }
}
 
}
 
