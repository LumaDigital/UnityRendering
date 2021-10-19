using System.Collections.Generic;
using System.Linq;
using HeadlessStudio.OfflineRenderHD.Runtime;
using HeadlessStudio.OfflineRenderHD.Runtime.Elements;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[ElementOptions("Beauty With Alpha Pass", Order = 1)]
public class BeautyWithAlphaElement : BaseRenderElement
{
    private RenderTexture _alphaTexture;
    private Material _material;
    [ElementUserControlable]
    public bool emptyExrLayerName = true;

    public void OnEnable()
    {
        Name = "Beauty With Alpha";
    }

    /// <summary>
    /// Grab the opacity debug view here, before calling the normal render.
    /// </summary>
    protected override void PreRender()
    {
        var old = ElementFrameBuffer;
        ElementFrameBuffer = _alphaTexture;
        var current = RenderPipelineManager.currentPipeline as HDRenderPipeline;
        current.debugDisplaySettings.data.materialDebugSettings.SetDebugViewMaterial(100);
        RenderHelper();
        current.debugDisplaySettings.data.materialDebugSettings.SetDebugViewMaterial(0);
        _alphaTexture = ElementFrameBuffer;
        ElementFrameBuffer = old;
    }

    public override void InitializeElement()
    {
        _enabled = true;
        ElementFrameBuffer = CreateFrameBuffer();
        _alphaTexture = CreateFrameBuffer();
    }

    public override void CheckForErrors(OfflineRenderHD parent)
    {
        if (parent != null && parent.renderElements.Any(x => x is BeautyRenderElement))
        {
            Message = "ERROR: Please remove the normal beauty render element.";
        }
        else
        {
            Message = string.Empty;
        }
    }

    public override void SaveOpenEXR()
    {
        if (ElementFrameBuffer)
        {
            if (_material == null) _material = new Material(Shader.Find("Hidden/beauty_combine"));

            var tmp = RenderTexture.GetTemporary(ElementFrameBuffer.descriptor);
            _material.SetTexture("_MainTex", ElementFrameBuffer);
            _material.SetTexture("_AlphaTex", _alphaTexture);
            Graphics.Blit(null, tmp, _material);
            Graphics.Blit(tmp, ElementFrameBuffer);
            tmp.Release();

            SaveSimple(FlipResult(), emptyExrLayerName ? "":"Beauty", "?");
        }
    }

    public override bool ValidateField(string fieldName, OfflineRenderHD offline)
    {
        if (fieldName != "_enabled") return false;
        if (offline.movieExport)
            return true;
        return false;
    }
}