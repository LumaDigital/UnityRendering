Shader "Hidden/ycdivfx/ElementsShader"
{
	CGINCLUDE
#include "UnityCG.cginc"
#pragma multi_compile ___ UNITY_HDR_ON
#pragma multi_compile ___ OFFSCREEN

	//sampler2D _MainTex;

	sampler2D _TmpFrameBuffer;
	sampler2D _TmpFrameBuffer_TexelSize;
	
	sampler2D _CameraGBufferTexture0;
	sampler2D _CameraGBufferTexture1;
	sampler2D _CameraGBufferTexture2;
	sampler2D _CameraGBufferTexture3;
	uniform sampler2D _CameraReflectionsTexture;
	sampler2D_float _CameraDepthTexture;
	sampler2D_half _CameraMotionVectorsTexture;

	float4 _ClearColor;

	struct v2f {
		float4 pos : POSITION;
		float4 spos : TEXCOORD0;
	};

	v2f vert(appdata_img v)
	{
		v2f o;
		o.pos = o.spos = v.vertex;
		return o;
	}


	float2 get_texcoord(v2f i)
	{
		float2 t = i.spos.xy * 0.5 + 0.5;
		return t;
	}

	float2 get_texcoord_gb(v2f i)
	{
		float2 t = i.spos.xy * 0.5 + 0.5;
#if !UNITY_UV_STARTS_AT_TOP && OFFSCREEN
		t.y = 1.0 - t.y;
#endif
		return t;
	}

	struct render_element_out
	{
		half4 render_element : SV_Target0;
	};

	render_element_out get_albedo(v2f I)
	{
		float2 t = get_texcoord_gb(I);
		half4 ao = tex2D(_CameraGBufferTexture0, t);
		render_element_out O;
		O.render_element = half4(ao.rgb, 1.0);
		return O;
	}

	render_element_out get_emission(v2f I)
	{
		float2 t = get_texcoord_gb(I);
		half4 emission = tex2D(_CameraGBufferTexture3, t);
		render_element_out O;
		O.render_element = emission;
		return O;
	}

	render_element_out get_specular(v2f I)
	{
		float2 t = get_texcoord_gb(I);
		half4 ss = tex2D(_CameraGBufferTexture1, t);
		render_element_out O;
		O.render_element = half4(ss.rgb, 1.0);;
		return O;
	}

	render_element_out get_occlusion(v2f I)
	{
		float2 t = get_texcoord_gb(I);
		half4 ao = tex2D(_CameraGBufferTexture0, t);
		render_element_out O;
		O.render_element = half4(ao.aaa, 1.0);
		return O;
	}

	render_element_out get_depth(v2f I)
	{
		float2 t = get_texcoord_gb(I);
//		half depth = tex2D(_CameraDepthTexture, t);
//#if defined(UNITY_REVERSED_Z)
//		depth = 1.0 - depth;
//#endif
		float d = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, t);
		d = Linear01Depth(d);
#if UNITY_REVERSED_Z
		d = 1.0 - d;
#endif
		render_element_out O;
		O.render_element = half4(d,d,d, 1.0);
		return O;
	}

	render_element_out get_smoothness(v2f I)
	{
		float2 t = get_texcoord_gb(I);
		half4 ss = tex2D(_CameraGBufferTexture1, t);
		render_element_out O;
		O.render_element = half4(ss.aaa, 1.0);
		return O;
	}

	render_element_out get_normal(v2f I)
	{
		float2 t = get_texcoord_gb(I);
		half4 normal = tex2D(_CameraGBufferTexture2, t);
		render_element_out O;
		O.render_element = half4(normal.rgb, 1.0);
		return O;
	}

	render_element_out get_velocity(v2f I)
	{
		float2 t = get_texcoord_gb(I);
		half2 velocity = tex2D(_CameraMotionVectorsTexture, t).rg;
		render_element_out O;
		O.render_element = half4(velocity, 1.0, 1.0);
		return O;
	}

	render_element_out get_reflections(v2f I)
	{
		float2 t = get_texcoord_gb(I);
		half4 c = tex2D(_CameraReflectionsTexture, t);
		render_element_out o;
#ifdef UNITY_HDR_ON
		o.render_element = float4(c.rgb, 0.0f);
#else
		o.render_element = float4(exp2(-c.rgb), 0.0f);
#endif
		return o;
	}

	struct framebuffer_out
	{
		half4 color         : SV_Target0;
		half4 alpha         : SV_Target1;
	};

	framebuffer_out blit_buffer(v2f I)
	{
		float2 t = get_texcoord(I);
#if !UNITY_UV_STARTS_AT_TOP && OFFSCREEN
		t.y = 1.0 - t.y;
#endif
		half4 c = tex2D(_TmpFrameBuffer, t);
//#if !OFFSCREEN
//		c = tex2D(_MainTex, t);
//#endif
		framebuffer_out O;
		O.color = half4(c.rgb, 1.0);
		O.alpha = half4(c.aaa, 1.0);
		return O;
	}

	// clear gbuffer
	struct clear_out
	{
		half4 gb0   : SV_Target0;
		half4 gb1   : SV_Target1;
		half4 gb2   : SV_Target2;
		half4 gb3   : SV_Target3;
		float depth : SV_Depth;
	};

	clear_out clear_gbuffer(v2f I)
	{
		clear_out O;
		O.gb0.xyzw = 0.0;
		O.gb1.xyzw = 0.0;
		O.gb2.xyzw = 0.0;
		O.gb3.xyzw = 0.0;
		O.depth = 1.0;
		return O;
	}

	half4 clear(v2f I) : SV_Target
	{
		return _ClearColor;
	}

	ENDCG
		SubShader
	{
		Pass // Pass 0: Diffuse / Albedo
	{
		Blend Off
		Cull Off
		ZTest Off
		ZWrite Off
		CGPROGRAM
#pragma vertex vert
#pragma fragment get_albedo
		ENDCG
	}
		Pass // Pass 1: Emission
	{
		Blend Off
		Cull Off
		ZTest Off
		ZWrite Off
		CGPROGRAM
#pragma vertex vert
#pragma fragment get_emission
		ENDCG
	}
		Pass // Pass 2: Specular
	{
		Blend Off
		Cull Off
		ZTest Off
		ZWrite Off
		CGPROGRAM
#pragma vertex vert
#pragma fragment get_specular
		ENDCG
	}
		Pass // Pass 3: Occlusion
	{
		Blend Off
		Cull Off
		ZTest Off
		ZWrite Off
		CGPROGRAM
#pragma vertex vert
#pragma fragment get_occlusion
		ENDCG
	}
		Pass // Pass 4: Depth
	{
		Blend Off
		Cull Off
		ZTest Off
		ZWrite Off
		CGPROGRAM
#pragma vertex vert
#pragma fragment get_depth
		ENDCG
	}
		Pass // Pass 5: Smoothness
	{
		Blend Off
		Cull Off
		ZTest Off
		ZWrite Off
		CGPROGRAM
#pragma vertex vert
#pragma fragment get_smoothness
		ENDCG
	}
		Pass // Pass 6: Normal
	{
		Blend Off
		Cull Off
		ZTest Off
		ZWrite Off
		CGPROGRAM
#pragma vertex vert
#pragma fragment get_normal
		ENDCG
	}
		Pass // Pass 7: Velocity
	{
		Blend Off
		Cull Off
		ZTest Off
		ZWrite Off
		CGPROGRAM
#pragma vertex vert
#pragma fragment get_velocity
		ENDCG
	}
		Pass // Pass 8: Copy Buffer
	{
		Blend Off
		Cull Off
		ZTest Off
		ZWrite Off
		CGPROGRAM
#pragma vertex vert
#pragma fragment blit_buffer
		ENDCG
	}
		Pass // Pass 9: Clear Buffer
	{
		Blend Off
		Cull Off
		ZTest Off
		ZWrite Off
		CGPROGRAM
#pragma vertex vert
#pragma fragment clear
		ENDCG
	}
		Pass // Pass 10: Clear Buffer
	{
		Blend Off
		Cull Off
		ZTest Off
		ZWrite Off
		CGPROGRAM
#pragma vertex vert
#pragma fragment get_reflections
		ENDCG
	}
	}
}
