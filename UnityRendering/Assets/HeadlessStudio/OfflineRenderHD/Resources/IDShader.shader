Shader "ycdivfx/IDShader"
{
	Properties
	{
		_ID_Color("ID Color", COLOR) = (1,0,0,1)
	}
	SubShader
	{
		Tags { "RenderType"="ID" }
		Pass
		{
			Blend Zero One
			Color [_ID_Color]
		}
	}
	Fallback "Transparent/VertexLit"
}
