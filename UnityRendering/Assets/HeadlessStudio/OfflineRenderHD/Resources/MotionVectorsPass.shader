Shader "Hidden/ycdivfx/MotionVectorsPass"
{
	Properties
	{
		_MainTex("Base", 2D) = "white" {}
	}

	CGINCLUDE

#include "UnityCG.cginc"

		sampler2D _MainTex;
		float4 _MainTex_TexelSize;
		float4 _MainTex_ST;

		half _Amplitude;

		sampler2D_half _CameraMotionVectorsTexture;

		struct v2f_common
		{
			float4 pos : SV_POSITION;
			half2 uv : TEXCOORD0;    // Screen space UV (supports stereo rendering)
			half2 uvAlt : TEXCOORD2; // Alternative UV (supports v-flip case)
		};

		// Vertex shader
		v2f_common vert_common(appdata_img v)
		{
			half2 uvAlt = v.texcoord;
#if UNITY_UV_STARTS_AT_TOP
			if (_MainTex_TexelSize.y < 0) uvAlt.y = 1 - uvAlt.y;
#endif

			v2f_common o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv = UnityStereoScreenSpaceUVAdjust(v.texcoord, _MainTex_ST);
			o.uvAlt = UnityStereoScreenSpaceUVAdjust(uvAlt, _MainTex_ST);

			return o;
		}

		float4 VectorToColor(float2 mv)
		{
			float phi = atan2(mv.x, mv.y);
			float hue = (phi / UNITY_PI + 1.0) * 0.5;

			float r = abs(hue * 6.0 - 3.0) - 1.0;
			float g = 2.0 - abs(hue * 6.0 - 2.0);
			float b = 2.0 - abs(hue * 6.0 - 4.0);
			float a = length(mv);

			return saturate(float4(r, g, b, a));
		}

		// Motion vectors overlay shader (fragment only)
		float4 frag_overlay(v2f_common i) : SV_Target
		{
			float2 mv = tex2D(_CameraMotionVectorsTexture, i.uvAlt).rg * _Amplitude;

			//float4 mc = VectorToColor(mv);

			return half4(mv, 0, 1);
			//return float4(mc);
		}


	ENDCG

	SubShader
	{
		Pass
		{
			CGPROGRAM
				#pragma vertex vert_common
				#pragma fragment frag_overlay
			ENDCG
		}
	}

	FallBack Off
}
