using System;
using UnityEngine.Rendering.HighDefinition;


namespace HeadlessStudio.OfflineRenderHD.Runtime.Elements
{
    [Serializable]
    [ElementOptions("Vision Matte")]
    public class MatteRenderElement : BaseRenderElement
    {

        private void OnEnable()
        {
            // Layer name
            Name = "matte";
        }

        protected override void InternalRender(HDRenderPipeline pipeline)
        {
            // Need to figure out a way to render emission only shaders for this pass
            // Example: the Score in ice hockey. In the mean time, the pass will be blank.
            pipeline.debugDisplaySettings.SetDebugViewProperties(
                UnityEngine.Rendering.HighDefinition.Attributes.DebugViewProperties.Instancing);
            RenderHelper();
            pipeline.debugDisplaySettings.SetDebugLightingMode(DebugLightingMode.None);
        }
    }
}