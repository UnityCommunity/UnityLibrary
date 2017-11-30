//useful links:
//https://www.shadertoy.com/view/XdS3RW
//see also: https://docs.gimp.org/en/gimp-concepts-layer-modes.html

Shader "UnityCommunity/Sprites/PhotoshopBlends"
{
	Properties
	{
		_source ("Upper Layer", 2D) = "white" {}
		_destination ("Lower Layer", 2D) = "white" {}
		number("Mode 0-24", Int) = 0	
	}

	Subshader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vertex_shader
			#pragma fragment pixel_shader
			#pragma target 3.0

			struct custom_type
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0; 
			};
			
			sampler2D _destination,_source;
			int number;
						
			float3 darken( float3 s, float3 d )
			{
				return min(s,d);
			}

			float3 multiply( float3 s, float3 d )
			{
				return s*d;
			}

			float3 colorBurn( float3 s, float3 d )
			{
				return 1.0 - (1.0 - d) / s;
			}

			float3 linearBurn( float3 s, float3 d )
			{
				return s + d - 1.0;
			}

			float3 darkerColor( float3 s, float3 d )
			{
				return (s.x + s.y + s.z < d.x + d.y + d.z) ? s : d;
			}

			float3 lighten( float3 s, float3 d )
			{
				return max(s,d);
			}

			float3 screen( float3 s, float3 d )
			{
				return s + d - s * d;
			}

			float3 colorDodge( float3 s, float3 d )
			{
				return d / (1.0 - s);
			}

			float3 linearDodge( float3 s, float3 d )
			{
				return s + d;
			}

			float3 lighterColor( float3 s, float3 d )
			{
				return (s.x + s.y + s.z > d.x + d.y + d.z) ? s : d;
			}

			float overlay( float s, float d )
			{
				return (d < 0.5) ? 2.0 * s * d : 1.0 - 2.0 * (1.0 - s) * (1.0 - d);
			}

			float3 overlay( float3 s, float3 d )
			{
				float3 c;
				c.x = overlay(s.x,d.x);
				c.y = overlay(s.y,d.y);
				c.z = overlay(s.z,d.z);
				return c;
			}

			float softLight( float s, float d )
			{
				return (s < 0.5) ? d - (1.0 - 2.0 * s) * d * (1.0 - d) 
					: (d < 0.25) ? d + (2.0 * s - 1.0) * d * ((16.0 * d - 12.0) * d + 3.0) 
								 : d + (2.0 * s - 1.0) * (sqrt(d) - d);
			}

			float3 softLight( float3 s, float3 d )
			{
				float3 c;
				c.x = softLight(s.x,d.x);
				c.y = softLight(s.y,d.y);
				c.z = softLight(s.z,d.z);
				return c;
			}

			float hardLight( float s, float d )
			{
				return (s < 0.5) ? 2.0 * s * d : 1.0 - 2.0 * (1.0 - s) * (1.0 - d);
			}

			float3 hardLight( float3 s, float3 d )
			{
				float3 c;
				c.x = hardLight(s.x,d.x);
				c.y = hardLight(s.y,d.y);
				c.z = hardLight(s.z,d.z);
				return c;
			}

			float vividLight( float s, float d )
			{
				return (s < 0.5) ? 1.0 - (1.0 - d) / (2.0 * s) : d / (2.0 * (1.0 - s));
			}

			float3 vividLight( float3 s, float3 d )
			{
				float3 c;
				c.x = vividLight(s.x,d.x);
				c.y = vividLight(s.y,d.y);
				c.z = vividLight(s.z,d.z);
				return c;
			}

			float3 linearLight( float3 s, float3 d )
			{
				return 2.0 * s + d - 1.0;
			}

			float pinLight( float s, float d )
			{
				return (2.0 * s - 1.0 > d) ? 2.0 * s - 1.0 : (s < 0.5 * d) ? 2.0 * s : d;
			}

			float3 pinLight( float3 s, float3 d )
			{
				float3 c;
				c.x = pinLight(s.x,d.x);
				c.y = pinLight(s.y,d.y);
				c.z = pinLight(s.z,d.z);
				return c;
			}

			float3 hardlerp( float3 s, float3 d )
			{
				return floor(s + d);
			}

			float3 difference( float3 s, float3 d )
			{
				return abs(d - s);
			}

			float3 exclusion( float3 s, float3 d )
			{
				return s + d - 2.0 * s * d;
			}

			float3 subtract( float3 s, float3 d )
			{
				return s - d;
			}

			float3 divide( float3 s, float3 d )
			{
				return s / d;
			}

			float3 rgb2hsv(float3 c)
			{
				float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
				float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
				float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));				
				float d = q.x - min(q.w, q.y);
				float e = 1.0e-10;
				return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
			}

			float3 hsv2rgb(float3 c)
			{
				float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
				float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
				return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
			}

			float3 hue( float3 s, float3 d )
			{
				d = rgb2hsv(d);
				d.x = rgb2hsv(s).x;
				return hsv2rgb(d);
			}

			float3 color( float3 s, float3 d )
			{
				s = rgb2hsv(s);
				s.z = rgb2hsv(d).z;
				return hsv2rgb(s);
			}

			float3 saturation( float3 s, float3 d )
			{
				d = rgb2hsv(d);
				d.y = rgb2hsv(s).y;
				return hsv2rgb(d);
			}

			float3 luminosity( float3 s, float3 d )
			{
				float dLum = dot(d, float3(0.3, 0.59, 0.11));
				float sLum = dot(s, float3(0.3, 0.59, 0.11));
				float lum = sLum - dLum;
				float3 c = d + lum;
				float minC = min(min(c.x, c.y), c.z);
				float maxC = max(max(c.x, c.y), c.z);
				if(minC < 0.0) return sLum + ((c - sLum) * sLum) / (sLum - minC);
				else if(maxC > 1.0) return sLum + ((c - sLum) * (1.0 - sLum)) / (maxC - sLum);
				else return c;
			}

			float3 blend( float3 s, float3 d, int id )
			{
				if(id==0)	return darken(s,d);
				if(id==1)	return multiply(s,d);
				if(id==2)	return colorBurn(s,d);
				if(id==3)	return linearBurn(s,d);
				if(id==4)	return darkerColor(s,d);				
				if(id==5)	return lighten(s,d);
				if(id==6)	return screen(s,d);
				if(id==7)	return colorDodge(s,d);
				if(id==8)	return linearDodge(s,d);
				if(id==9)	return lighterColor(s,d);				
				if(id==10)	return overlay(s,d);
				if(id==11)	return softLight(s,d);
				if(id==12)	return hardLight(s,d);
				if(id==13)	return vividLight(s,d);
				if(id==14)	return linearLight(s,d);
				if(id==15)	return pinLight(s,d);
				if(id==16)	return hardlerp(s,d);				
				if(id==17)	return difference(s,d);
				if(id==18)	return exclusion(s,d);
				if(id==19)	return subtract(s,d);
				if(id==20)	return divide(s,d);				
				if(id==21)	return hue(s,d);
				if(id==22)	return color(s,d);
				if(id==23)	return saturation(s,d);
				if(id==24)	return luminosity(s,d);				
				return float3(0,0,0);
			}			
						
			custom_type vertex_shader (float4 vertex:POSITION, float2 uv:TEXCOORD0)
			{
				custom_type vs;
				vs.vertex = UnityObjectToClipPos(vertex);
				vs.uv = uv;
				return vs;
			}

			float4 pixel_shader (custom_type ps) : SV_TARGET
			{
				float2 uv = ps.uv.xy;
				int id =number;
				float3 s = tex2D(_source, uv).xyz;
				float3 d = tex2D(_destination, uv).xyz;
				float3 c = blend(s,d,id);
				return float4(c,1.0);			
			}
			ENDCG
		}
	}
}