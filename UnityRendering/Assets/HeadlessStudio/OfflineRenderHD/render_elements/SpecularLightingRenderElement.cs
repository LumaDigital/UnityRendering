// ycdivfx.OfflineRender.Runtime
// You can do it! VFX (c) 2016
// History:
// 2016/04/15 - Daniel Santana

#region

using System;
using UnityEngine.Rendering.HighDefinition;

#endregion

namespace HeadlessStudio.OfflineRenderHD.Runtime.Elements
{
    [Serializable]
    [ElementOptions("Specular Lighting")]
    public class SpecularLightingRenderElement : BaseRenderElement
    {

        private void OnEnable()
        {
            Name = "SpecularLighting";
        }

        protected override void InternalRender(HDRenderPipeline pipeline)
        {

            pipeline.debugDisplaySettings.SetDebugLightingMode(DebugLightingMode.SpecularLighting);
            RenderHelper();
            pipeline.debugDisplaySettings.SetDebugLightingMode(DebugLightingMode.None);
        }
    }
}