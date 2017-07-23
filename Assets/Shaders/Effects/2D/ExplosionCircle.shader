// source: http://unitycoder.com/blog/2013/08/11/scorched-earth-terrain-wip/
// reference: http://johnwhigham.blogspot.sg/2011/11/planetary-rings.html

Shader "UnityLibrary/ExplosionCircle" {
  Properties {
  _MainTex ("Base (RGB)", 2D) = "white" {}
  _Radius ("Radius", float) = 0.5
  }
 
SubShader {
  Tags
  {
  "Queue"="Transparent"
  "IgnoreProjector"="True"
  "RenderType"="Transparent"
  }
  LOD 200
  Lighting Off
   
  CGPROGRAM
  #pragma surface surf Lambert
   
  sampler2D _MainTex;
  float _Radius;
   
  struct Input {
  float2 uv_MainTex;
  };
   
  void surf (Input IN, inout SurfaceOutput o)
  {
  float dist = distance(IN.uv_MainTex, float2(0.5,0.5));
  if ( abs(dist) > _Radius)     clip (-1.0);
  float val = dist*128;
  float c1 = 1-sin(val+_Time.x)+0.5;
  float c2 = cos(val+_Time.y)+0.5;
  float c3 = 0;
  o.Albedo = float3(c1,c2,c3)*3;
  o.Alpha = 1;
  }
  ENDCG
  }
FallBack "Diffuse"
}
