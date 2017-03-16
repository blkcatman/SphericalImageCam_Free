//RectDraw.shader
//
//Copyright (c) 2015 Tatsuro Matsubara
//SphericalImageCam_Free is licensed under a Creative Commons Attribution-ShareAlike 4.0 International License.
//See also http://creativecommons.org/licenses/by-sa/4.0/
//
Shader "Hidden/RectDraw"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Position ("Position", Vector) = (0, 0, 0, 0)
		_Scale ("Scale", Vector) = (1, 1, 0, 0)
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

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

			float2 _Position;
			float2 _Scale;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				//o.vertex.x = o.vertex.x * _Scale.x + _Position.x;
				//o.vertex.y = o.vertex.y * _Scale.y + _Position.y;

				o.uv = v.uv;
				return o;
			}
			
			sampler2D _MainTex;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				return col;
			}
			ENDCG
		}
	}
}
