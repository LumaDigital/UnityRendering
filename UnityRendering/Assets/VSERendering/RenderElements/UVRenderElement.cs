using System;

using UnityEngine.Rendering.HighDefinition;


namespace HeadlessStudio.OfflineRenderHD.Runtime.Elements
{
    [Serializable]
    [ElementOptions("Vision UV Texture Test")]
    public class UVRenderElement : BaseRenderElement
    {
        private void OnEnable()
        {
            // Layer name
            Name = "UV";
        }

        protected override void InternalRender(HDRenderPipeline pipeline)
        {
            FindObjectOfType<VSERendering>().SetupUVTextures();

            pipeline.debugDisplaySettings.SetDebugViewCommonMaterialProperty(
                UnityEngine.Rendering.HighDefinition.Attributes.MaterialSharedProperty.Albedo);
            RenderHelper();
            pipeline.debugDisplaySettings.SetDebugLightingMode(DebugLightingMode.None);
        }

        public override void SaveOpenEXR()
        {
            if (ElementFrameBuffer)
            {
                SaveSimple(FlipResult(), "", "RGBA");
            }
        }
    }
}