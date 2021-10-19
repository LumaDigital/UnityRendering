// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "Hidden/ycdivfx/ProcessingShader" {
  Properties {
		_MainTex("Base", 2D) = "white" {}
		_Cube ("Cube", Cube) = "" {}
	}

	CGINCLUDE
#include "UnityCG.cginc"

	struct v2f {
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;
	};

	samplerCUBE _Cube;
	sampler2D _MainTex;

#define PI 3.141592653589793
#define HALFPI 1.57079632679

	v2f vert(appdata_img v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		float2 uv = v.texcoord.xy * 2 - 1;
		uv *= float2(PI, HALFPI);
		o.uv = uv;
		return o;
	}

	float2 get_texcoord_gb(v2f_img i)
	{
		float2 t = i.uv;
#if !UNITY_UV_STARTS_AT_TOP && OFFSCREEN
		t.y = 1.0 - t.y;
#endif
		return t;
	}

	half4 auto_flip(v2f_img i) : COLOR
	{
		float2 t = get_texcoord_gb(i);
		half4 o = tex2D(_MainTex, t);
		return o;
	}

	fixed4 frag(v2f i) : COLOR
	{
		float cosy = cos(i.uv.y);
		float3 normal = float3(0,0,0);
		normal.x = cos(i.uv.x) * cosy;
		normal.y = i.uv.y;
		normal.z = cos(i.uv.x - HALFPI) * cosy;
		return texCUBE(_Cube, normal);
	}

	half4 flip_buffer(v2f_img i) : COLOR
	{
		float2 t = i.uv;
#if !defined(OFFSCREEN) || !defined(UNITY_UV_STARTS_AT_TOP)
		t.y = 1.0 - t.y;
#endif
		half4 O = tex2D(_MainTex, t);
		O.a = 1.0;
		return O;
	}

	half4 flip_buffer_gamma(v2f_img i) : COLOR
	{
		float2 t = i.uv;
#if !defined(OFFSCREEN) || !defined(UNITY_UV_STARTS_AT_TOP)
		t.y = 1.0 - t.y;
#endif
		half4 O = tex2D(_MainTex, t);
		O = pow(O, 0.4545);
		O = clamp(O, 0.0, 1.0);
		O.a = 1.0;
		return O;
	}

	half4 correct_gamma(v2f_img i) : COLOR
	{
		half4 O = tex2D(_MainTex, i.uv);
		O = pow(O, 0.4545);
		O = clamp(O, 0.0, 1.0);
		O.a = 1.0;
		return O;
	}

	half4 correct_gamma2(v2f_img i) : COLOR
	{
		float2 t = i.uv;
		t.y = 1.0 - t.y;
		half4 O = tex2D(_MainTex, t);
		O.rgb = pow(O.rgb, 0.4545);
		return O;
	}

	half4 clamp_colors(v2f_img i) : COLOR
	{
		half4 O = tex2D(_MainTex, i.uv);
		O = clamp(O, 0.0, 1.0);
		O.a = 1.0;
		return O;
	}
	ENDCG
	Subshader {
		// 0 Calculate equi rectangular from cube map.
		Pass {
			Blend off
			ZTest Always Cull Off ZWrite Off
			Fog { Mode off }
			CGPROGRAM
#pragma fragmentoption ARB_precision_hint_fastest
#pragma vertex vert
#pragma fragment frag
			ENDCG
		}
		// 1 Flip
		Pass {
			Blend off
			ZTest Always Cull Off ZWrite Off
			Fog{ Mode off }
			CGPROGRAM
#pragma multi_compile ___ UNITY_HDR_ON
#pragma multi_compile ___ OFFSCREEN
#pragma vertex vert_img
#pragma fragment flip_buffer
			ENDCG
		}
		// 2 Flip, apply gamma and clamp.
		Pass{
			Blend off
			ZTest Always Cull Off ZWrite Off
			Fog{ Mode off }
			CGPROGRAM
#pragma multi_compile ___ UNITY_HDR_ON
#pragma multi_compile ___ OFFSCREEN
#pragma vertex vert_img
#pragma fragment flip_buffer_gamma
			ENDCG
		}
		// 3 Apply gamma and clamp.
		Pass{
			Blend off
			ZTest Always Cull Off ZWrite Off
			Fog{ Mode off }
			CGPROGRAM
#pragma multi_compile ___ UNITY_HDR_ON
#pragma multi_compile ___ OFFSCREEN
#pragma vertex vert_img
#pragma fragment correct_gamma
			ENDCG
		}
		// 4 Clamp colors.
		Pass{
			Blend off
			ZTest Always Cull Off ZWrite Off
			Fog{ Mode off }
			CGPROGRAM
#pragma multi_compile ___ UNITY_HDR_ON
#pragma multi_compile ___ OFFSCREEN
#pragma vertex vert_img
#pragma fragment clamp_colors
			ENDCG
		}
		// 5 Clamp colors.
		Pass{
			Blend off
			ZTest Always Cull Off ZWrite Off
			Fog{ Mode off }
			CGPROGRAM
#pragma multi_compile ___ UNITY_HDR_ON
#pragma multi_compile ___ OFFSCREEN
#pragma vertex vert_img
#pragma fragment auto_flip
			ENDCG
		}
		// 6 TosRGB.
		Pass{
			Blend off
			ZTest Always Cull Off ZWrite Off
			Fog{ Mode off }
			CGPROGRAM
#pragma multi_compile ___ UNITY_HDR_ON
#pragma multi_compile ___ OFFSCREEN
#pragma vertex vert_img
#pragma fragment correct_gamma2
			ENDCG
		}
	}
	Fallback Off
}