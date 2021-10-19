using System;
using UnityEngine.Rendering.HighDefinition;


namespace HeadlessStudio.OfflineRenderHD.Runtime.Elements
{
    [Serializable]
    [ElementOptions("Vision Stencil")]
    public class StencilRenderElement : BaseRenderElement
    {

        private void OnEnable()
        {
            // Layer name
            Name = "stencil";
        }

        protected override void InternalRender(HDRenderPipeline pipeline)
        {
            pipeline.debugDisplaySettings.SetDebugViewCommonMaterialProperty(
                UnityEngine.Rendering.HighDefinition.Attributes.MaterialSharedProperty.Albedo);
            RenderHelper();
            pipeline.debugDisplaySettings.SetDebugLightingMode(DebugLightingMode.None);
        }
    }
}