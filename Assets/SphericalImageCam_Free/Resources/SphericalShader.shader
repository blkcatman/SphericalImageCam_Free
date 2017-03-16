//SphericalShader.shader
//
//Copyright (c) 2015 Tatsuro Matsubara
//SphericalImageCam_Free is licensed under a Creative Commons Attribution-ShareAlike 4.0 International License.
//See also http://creativecommons.org/licenses/by-sa/4.0/
//
Shader "Hidden/SphericalShader" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "" {}
		_rt("rt", Vector) = (0,0,0,0)
		_fl("fl", Vector) = (0,0,0,0)
		_ps("ps", Vector) = (0,0,1,1)
	}
	
	CGINCLUDE

	#pragma shader_feature _VS _VC
    #pragma target 3.0
	#include "UnityCG.cginc"
	
	struct v2f {
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;
	};
	
	sampler2D _MainTex;
	float4 _rt;
	float4 _fl;
	float4 _ps;
	
	v2f vert( appdata_img v ) 
	{
		v2f o;
		o.pos = float4(
			  v.vertex.x*2.0*_rt.z + _rt.x - _rt.z,
			  v.vertex.y*2.0*_rt.w + _rt.y - _rt.w,
			  v.vertex.z,
			  v.vertex.w);
		o.uv = float2(
			_fl.x + _fl.z*v.texcoord.x,
			_fl.y + _fl.w*v.texcoord.y);


	
	#if UNITY_UV_STARTS_AT_TOP
		o.pos.y = -o.pos.y;
	#else
		o.pos.y = o.pos.y;
	#endif

	o.pos.x = o.pos.x * _ps.z + _ps.x;
	o.pos.y = o.pos.y * _ps.w + _ps.y;
		
		return o;
	}
	
	half4 frag(v2f i) : COLOR 
	{
		float2 samp;
	#ifdef _VS
		float PPI = 0.3333333*3.14159265358979323846264;
		float2 coords = (i.uv - 0.5) * 2.0;
		float2 pos = tan(float2(coords.x*PPI, coords.y*PPI)) * 0.57735026919;
		samp = float2(pos.x, (1.0 / cos(coords.x*PPI)) * pos.y) / 2.0 + 0.5;
	#endif
	#ifdef _VC
		float theta = i.uv.x * 6.28318530717958647692529;
		float r = (1.0 / cos(i.uv.y*1.5707963267949)) * i.uv.y;
		samp = float2(-sin(theta)*r, cos(theta)*r) / 2.0 + 0.5;
	#endif
		if (samp.x < -0.0001 || samp.x > 1.0001 || samp.y < -0.0001 || samp.y > 1.0001) {
			discard;
		}
		
		half4 color = tex2D (_MainTex, samp);
		color.a = 1.0;
		
		return color;
	}

	ENDCG 
	
Subshader {
  Pass {
 	  Tags {"LightMode"="Always" "Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
	  ZTest Always Cull Off ZWrite Off
	  Fog { Mode off }
	  
      CGPROGRAM
      #pragma fragmentoption ARB_precision_hint_fastest 
      #pragma vertex vert
      #pragma fragment frag
      ENDCG
  }
  
}

Fallback off
	
}