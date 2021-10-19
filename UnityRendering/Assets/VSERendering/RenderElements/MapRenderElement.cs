using System;
using UnityEngine.Rendering.HighDefinition;


namespace HeadlessStudio.OfflineRenderHD.Runtime.Elements
{
    [Serializable]
    [ElementOptions("Vision Map")]
    public class MapRenderElement : BaseRenderElement
    {

        private void OnEnable()
        {
            // Layer name
            Name = "Map";
        }

        protected override void InternalRender(HDRenderPipeline pipeline)
        {
            FindObjectOfType<VSERendering>().SetupMapTextures();

            pipeline.debugDisplaySettings.SetDebugViewCommonMaterialProperty(
                UnityEngine.Rendering.HighDefinition.Attributes.MaterialSharedProperty.Albedo);
            RenderHelper();
            pipeline.debugDisplaySettings.SetDebugLightingMode(DebugLightingMode.None);
        }

        public override void SaveOpenEXR()
        {
            if (ElementFrameBuffer)
            {
                SaveSimple(FlipResult(), "Map", "_?");
            }
        }
    }
}