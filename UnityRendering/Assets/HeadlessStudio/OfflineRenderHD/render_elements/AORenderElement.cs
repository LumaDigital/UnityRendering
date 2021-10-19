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
    [ElementOptions("SSAO")]
    public class AORenderElement : BaseRenderElement
    {
        private void OnEnable()
        {
            Name = "SSAO";
        }

        protected override void InternalRender(HDRenderPipeline pipeline)
        {
            pipeline.debugDisplaySettings.data.fullScreenDebugMode = FullScreenDebugMode.ScreenSpaceAmbientOcclusion;
            RenderHelper();
            pipeline.debugDisplaySettings.data.fullScreenDebugMode = FullScreenDebugMode.None;
        }
    }
}