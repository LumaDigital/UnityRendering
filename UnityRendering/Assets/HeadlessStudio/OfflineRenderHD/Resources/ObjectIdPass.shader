Shader "ycdivfx/ObjectIdPass"
{
	SubShader
	{
		Tags { "Queue" = "Overlay" "RenderType"="ID" }
		Pass
		{
			Fog { Mode Off }
			Color [_ID_Color]
		}
	}
}
