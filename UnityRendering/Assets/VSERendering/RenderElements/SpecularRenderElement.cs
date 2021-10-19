using System;
using UnityEngine.Rendering.HighDefinition;


namespace HeadlessStudio.OfflineRenderHD.Runtime.Elements
{
    [Serializable]
    [ElementOptions("Vision Specular")]
    public class SpecularRenderElement : BaseRenderElement
    {

        private void OnEnable()
        {
            // Layer name
            Name = "specular";
        }

        protected override void InternalRender(HDRenderPipeline pipeline)
        {
            pipeline.debugDisplaySettings.SetDebugLightingMode(DebugLightingMode.SpecularLighting);
            RenderHelper();
            pipeline.debugDisplaySettings.SetDebugLightingMode(DebugLightingMode.None);
        }
    }
}