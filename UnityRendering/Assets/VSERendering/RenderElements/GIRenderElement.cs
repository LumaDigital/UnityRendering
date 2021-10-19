using System;
using UnityEngine.Rendering.HighDefinition;


namespace HeadlessStudio.OfflineRenderHD.Runtime.Elements
{
    [Serializable]
    [ElementOptions("Vision GI")]
    public class GIRenderElement : BaseRenderElement
    {

        private void OnEnable()
        {
            // Layer name
            Name = "gi";
        }

        protected override void InternalRender(HDRenderPipeline pipeline)
        {
            // According to Daniel Santana, Tech director at headless studio:
            // "Reflections and GI aren't fully exposed on the Pipeline Debug"
            // Vision's actors pass still requires these layers even if they're 
            // blank therefore we render a dummy layer until we figure out a way
            // to create a reflection and GI pass.
            pipeline.debugDisplaySettings.SetDebugViewProperties(
                UnityEngine.Rendering.HighDefinition.Attributes.DebugViewProperties.Instancing);
            RenderHelper();
            pipeline.debugDisplaySettings.SetDebugLightingMode(DebugLightingMode.None);
        }
    }
}