Shader "OfflineRender/Motion Vectors Full"
{
    HLSLINCLUDE

    #pragma vertex Vert

    #pragma target 4.5
    #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Builtin/BuiltinData.hlsl"

    // The PositionInputs struct allow you to retrieve a lot of useful information for your fullScreenShader:
    // struct PositionInputs
    // {
    //     float3 positionWS;  // World space position (could be camera-relative)
    //     float2 positionNDC; // Normalized screen coordinates within the viewport    : [0, 1) (with the half-pixel offset)
    //     uint2  positionSS;  // Screen space pixel coordinates                       : [0, NumPixels)
    //     uint2  tileCoord;   // Screen tile coordinates                              : [0, NumTiles)
    //     float  deviceDepth; // Depth from the depth buffer                          : [0, 1] (typically reversed)
    //     float  linearDepth; // View space Z coordinate                              : [Near, Far]
    // };

    // To sample custom buffers, you have access to these functions:
    // But be careful, on most platforms you can't sample to the bound color buffer. It means that you
    // can't use the SampleCustomColor when the pass color buffer is set to custom (and same for camera the buffer).
    // float4 SampleCustomColor(float2 uv);
    // float4 LoadCustomColor(uint2 pixelCoords);
    // float LoadCustomDepth(uint2 pixelCoords);
    // float SampleCustomDepth(float2 uv);

    // There are also a lot of utility function you can use inside Common.hlsl and Color.hlsl,
    // you can check them out in the source code of the core SRP package.

    float2 SampleMotionVectors(float2 coords)
    {
        float2 motionVectorNDC;
        DecodeMotionVector(SAMPLE_TEXTURE2D_X(_CameraMotionVectorsTexture, s_point_clamp_sampler, coords), motionVectorNDC);

        return motionVectorNDC;
    }

    float4 FullScreenPass(Varyings varyings) : SV_Target
    {
        UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(varyings);
        float depth = LoadCameraDepth(varyings.positionCS.xy);
        PositionInputs posInput = GetPositionInput(varyings.positionCS.xy, _ScreenSize.zw, depth, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);
        float3 viewDirection = GetWorldSpaceNormalizeViewDir(posInput.positionWS);

        float2 mv = SampleMotionVectors(posInput.positionNDC.xy);

        // Background color intensity - keep this low unless you want to make your eyes bleed
        const float kMinIntensity = 0.03f;
        const float kMaxIntensity = 0.50f;

        // Map motion vector direction to color wheel (hue between 0 and 360deg)
        float phi = atan2(mv.x, mv.y);
        float hue = (phi / PI + 1.0) * 0.5;
        float r = abs(hue * 6.0 - 3.0) - 1.0;
        float g = 2.0 - abs(hue * 6.0 - 2.0);
        float b = 2.0 - abs(hue * 6.0 - 4.0);

        float maxSpeed = 60.0f / 0.15f; // Admit that 15% of a move the viewport by second at 60 fps is really fast
        float absoluteLength = saturate(length(mv.xy) * maxSpeed);
        float3 color = float3(r, g, b) * lerp(kMinIntensity, kMaxIntensity, absoluteLength);
        color = saturate(color);

        return float4(color.rgb, 1.0);
    }

    ENDHLSL

    SubShader
    {
        Pass
        {
            Name "Motion Vectors Full"

            ZWrite Off
            ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            HLSLPROGRAM
                #pragma fragment FullScreenPass
            ENDHLSL
        }
    }
    Fallback Off
}
