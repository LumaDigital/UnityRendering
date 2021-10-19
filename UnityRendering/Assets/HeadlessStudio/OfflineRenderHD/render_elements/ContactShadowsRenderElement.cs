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
    [ElementOptions("Contact Shadows", 5)]
    public class ContactShadowsRenderElement : BaseRenderElement
    {
        private void OnEnable()
        {
            Name = "DeferredShadows";
        }

        protected override void InternalRender(HDRenderPipeline pipeline)
        {
            pipeline.debugDisplaySettings.data.fullScreenDebugMode = FullScreenDebugMode.ContactShadows;
            RenderHelper();
            pipeline.debugDisplaySettings.data.fullScreenDebugMode = FullScreenDebugMode.None;
        }

    }
}