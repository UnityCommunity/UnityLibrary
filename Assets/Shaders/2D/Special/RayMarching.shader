// "ShaderToy Tutorial - Ray Marching for Dummies!"  https://www.shadertoy.com/view/XlGBW3
// by Martijn Steinrucken aka BigWings/CountFrolic - 2018
// License Creative Commons Attribution-NonCommercial-ShareAlike 3.0 Unported License.
// This shader is part of a tutorial on YouTube https://youtu.be/PGtv-dBi2wE
// 01-10-2019 converted to unity shader (with some adjusted variable names) : unitycoder.com

Shader "UnityLibrary/2D/Special/RayMarching"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

			      #define MAX_STEPS 100 // maximum raycast loops
			      #define MAX_DIST 100. // ray cannot go further than this distance
			      #define SURF_DIST .01 // how near to surface we should raycast to

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
				        float2 uv : TEXCOORD0;
			      };

            // returns distance to the scene objects (we have 2 objects here, sphere and ground plane)
            float GetDist(float3 p) 
            {
              float4 sphere = float4(0, 1, 6, 1); // x,y,z,radius
              float sphereDist = length(p - sphere.xyz) - sphere.w;
              float planeDist = p.y;
              float distance = min(sphereDist, planeDist); // return closest distance
              return distance;
            }


            // jump along the ray until we get close enough to some of our objects
            float RayMarch(float3 rayOrigin, float3 rayDirection)
            {
              float distanceOrigin = 0.;
              for (int i = 0; i < MAX_STEPS; i++) 
              {
                float3 p = rayOrigin + rayDirection * distanceOrigin;
                float dS = GetDist(p);
                distanceOrigin += dS;
                if (distanceOrigin > MAX_DIST || dS < SURF_DIST) break;
              }
              return distanceOrigin;
            }

            float3 GetNormal(float3 p) 
            {
              float d = GetDist(p);
              float2 e = float2(.01, 0);
              float3 n = d - float3(GetDist(p - e.xyy),GetDist(p - e.yxy),GetDist(p - e.yyx));
              return normalize(n);
            }

            float GetLight(float3 p) 
            {
              float3 lightPos = float3(0, 5, 6);
              lightPos.xz += float2(sin(_Time.y), cos(_Time.y)) * 2.0;
              float3 l = normalize(lightPos - p);
              float3 n = GetNormal(p);
              float dif = clamp(dot(n, l), 0.0, 1.0);
              float d = RayMarch(p + n * SURF_DIST * 2.0, l);
              if (d < length(lightPos - p)) dif *= 0.1;
              return dif;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
			        	o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
              float2 uv = (i.uv - 0.5);
              float3 col = float3(0,0,0);

              float3 rayOrigin = float3(0,1,0);
              float3 rayDirection = normalize(float3(uv.x, uv.y, 1));

              float distance = RayMarch(rayOrigin, rayDirection);
              float3 p = rayOrigin + rayDirection * distance;

              float dif = GetLight(p);
              col = float3(dif,dif,dif);

              return float4(col,1);
            }
            ENDCG
        } // pass
    } // subshader
} // shader
