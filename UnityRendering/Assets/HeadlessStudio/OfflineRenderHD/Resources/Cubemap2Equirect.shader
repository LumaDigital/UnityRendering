Shader "Unlit/Equirect"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" "PreviewType" = "Plane"}
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			#define PI 3.141592654

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float3 _SphereRotate;

			float map(float v, float low1, float high1, float low2, float high2) {
				return (v - low1) / (high1 - low1)*(high2 - low2);
			}

			float2 xyzToLonLat(float3 v) {
				float3 p = normalize(v);
				float lat = map(asin(p.y), PI*0.5, -PI * 0.5, 0.0, 1.0);
				float lon = map(atan2(p.x, -p.z), PI, -PI, 0.0, 1.0);
				return float2(lon, lat);
			}

			float3 lonLatToXYZ(float2 lonLat) {
				float lon = map(lonLat.x, 0.0, 1.0, -PI, PI);
				float lat = map(lonLat.y, 0.0, 1.0, -PI * 0.5, PI*0.5);
				float x = sin(lat)*sin(lon);
				float y = cos(lat);
				float z = sin(lat)*cos(lon);
				return float3(x, y, z);
			}

			float3 xRot(float3 v, float theta) {
				float x = v.x;
				float y = v.y*cos(theta) - v.z*sin(theta);
				float z = v.y*sin(theta) + v.z*cos(theta);
				return float3(x, y, z);
			}

			float3 yRot(float3 v, float theta) {
				float x = v.z*sin(theta) + v.x*cos(theta);
				float y = v.y;
				float z = v.z*cos(theta) - v.x*sin(theta);
				return float3(x, y, z);
			}

			float3 zRot(float3 v, float theta) {
				float x = v.x*cos(theta) - v.y*sin(theta);
				float y = v.x*sin(theta) + v.y*cos(theta);
				float z = v.z;
				return float3(x, y, z);
			}

			float2 equiRemap(float2 lonLat, float2 delta) {
				float3 v = lonLatToXYZ(lonLat);
				v = yRot(v, _SphereRotate.x);
				v = xRot(v, _SphereRotate.y);
				v = zRot(v, _SphereRotate.z);
				return xyzToLonLat(v);
			}

			//v2f vert(appdata v)
			//{
			//	v2f o;
			//	o.vertex = UnityObjectToClipPos(v.vertex);
			//	o.uv = v.uv;

			//	return o;
			//}

			float M_PI = 3.141592654f;

			float4 frag(v2f_img i) : SV_Target
			{
				float2 lonlat = i.uv.xy;
				float3 v = lonLatToXYZ(lonlat);
				v = yRot(v, _SphereRotate.y);
				v = xRot(v, _SphereRotate.x);
				v = zRot(v, _SphereRotate.z);
				lonlat = xyzToLonLat(v);
				float4 o = tex2D(_MainTex, lonlat);
				return o;
			}
			ENDCG
		}
	}
}
