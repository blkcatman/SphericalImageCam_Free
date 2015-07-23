//SphericalShader.shader
//
//Copyright (c) 2015 Tatsuro Matsubara
//SphericalImageCam_Free is licensed under a Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.
//See also http://creativecommons.org/licenses/by-nc-sa/4.0/
//
Shader "Hidden/SphericalShader" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "" {}
	}
	
	CGINCLUDE
	
	#pragma shader_feature _VF _VR _VL _VB _VT
	#pragma shader_feature _VS _VC

    #pragma target 3.0
	#include "UnityCG.cginc"
	
	struct v2f {
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;
	};
	
	sampler2D _MainTex;
	
	v2f vert( appdata_img v ) 
	{
		v2f o;
		float4 ps;
	#ifdef _VF
		ps = float4(0, 0, 0.33356, 0.6666667);
	#endif
	#ifdef _VR
		ps = float4(0.6666667, 0, 0.33356, 0.6666667);
	#endif
	#ifdef _VL
		ps = float4(-0.6666667, 0, 0.33356, 0.6666667);
	#endif
	#ifdef _VB
		ps = float4(0, -0.4355, 1, 0.568);
	#endif
	#ifdef _VT
		ps = float4(0,  0.4355, 1, 0.568);
	#endif
		o.pos = float4(
			  v.vertex.x*2.0*ps.z + ps.x - ps.z,
			  v.vertex.y*2.0*ps.w + ps.y - ps.w,
			  v.vertex.z,
			  v.vertex.w);
			
		o.uv = v.texcoord.xy;
	
	#ifdef _VB
		o.uv.x = 1.0 - o.uv.x;
	#endif
	#ifdef _VT
		o.uv.y = 1.0 - o.uv.y;
	#endif
	
	#if UNITY_UV_STARTS_AT_TOP
		o.pos.y = -o.pos.y;
	#else
		o.pos.y = o.pos.y;
	#endif
		
		return o;
	}
	
	half4 frag(v2f i) : COLOR 
	{
		float2 samp;
	#ifdef _VS
		float PI = 3.14159265358979323846264;
		float2 coords = (i.uv - 0.5) * 2.0;
		float fLength = tan(0.16666666666667*PI);
		float2 pos = float2(tan(coords.x*0.3333333*PI), tan(coords.y*0.3333333*PI)) * fLength;
		samp = float2(pos.x, (1.0 / cos(coords.x*0.3333333*PI)) * pos.y) / 2.0 + 0.5;
	#endif
	#ifdef _VC
		float theta = i.uv.x * 6.28318530717958647692529;
		float r = (1.0 / cos(i.uv.y*1.5707963267949)) * i.uv.y;
		samp = float2(-sin(theta)*r, cos(theta)*r) / 2.0 + 0.5;
	#endif
		
		half4 color = tex2D (_MainTex, samp);
		color.a = 1.0;
		//
		if(samp.x < -0.0001 || samp.x > 1.0001 || samp.y < -0.0001 || samp.y > 1.0001) {
			clip(-1.0);
		}
		
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