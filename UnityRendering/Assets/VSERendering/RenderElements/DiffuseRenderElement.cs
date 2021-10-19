using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;


namespace HeadlessStudio.OfflineRenderHD.Runtime.Elements
{
    [Serializable]
    [ElementOptions("Vision Diffuse")]
    public class DiffuseRenderElement : BaseRenderElement
    {
        // Glow materials to be used for actors such as an LED display
        private List<MeshRenderer> GlowMaterialObjects
        {
            get
            {
                if (meshRenderers == null)
                {
                    meshRenderers = 
                        FindObjectsOfType<MeshRenderer>().Where(
                            element => element.material.name.Contains("Glow")).ToList();
                }
                return meshRenderers;
            }
        }
        private List<MeshRenderer> meshRenderers;

        private void OnEnable()
        {
            // Layer name
            Name = "diffuse";
        }

        protected override void InternalRender(HDRenderPipeline pipeline)
        {
            GlowMaterialObjects.Select(element => element.enabled = false).ToList();

            pipeline.debugDisplaySettings.SetDebugLightingMode(DebugLightingMode.DiffuseLighting);
            RenderHelper();
            pipeline.debugDisplaySettings.SetDebugLightingMode(DebugLightingMode.None);

            GlowMaterialObjects.Select(element => element.enabled = true).ToList();
        }
    }
}