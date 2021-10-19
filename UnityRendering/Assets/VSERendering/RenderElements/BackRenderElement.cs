using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;


namespace HeadlessStudio.OfflineRenderHD.Runtime.Elements
{
    [Serializable]
    [ElementOptions("Vision Back")]
    public class BackRenderElement : BaseRenderElement
    {
        private void OnEnable()
        {
            // Layer name
            Name = "Back";
        }

        protected override void InternalRender(HDRenderPipeline pipeline)
        {
            RenderHelper();
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