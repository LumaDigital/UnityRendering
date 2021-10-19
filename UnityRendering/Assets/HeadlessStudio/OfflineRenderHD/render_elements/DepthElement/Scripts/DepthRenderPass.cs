using HeadlessStudio.OfflineRenderHD.Runtime;
using HeadlessStudio.OfflineRenderHD.Runtime.Elements;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[Serializable]
[ElementOptions("Depth Pass", Order = 2)]
public class DepthRenderPass : BaseRenderElement
{
    public void OnEnable()
    {
        Name = "Depth Pass";
    }

    [NonSerialized]
    private CustomPassVolume _customPassVolume;

    public override void InitializeElement()
    {
        base.InitializeElement();
        var customPassVolume = Resources.Load<CustomPassVolume>("DepthElementPass");
        if(customPassVolume == null)
        {
            Debug.LogError("Missing Segmentation Prefab");
            Enabled = false;
            return;
        }
        _customPassVolume = Instantiate(customPassVolume.gameObject).GetComponent<CustomPassVolume>();
        _customPassVolume.hideFlags = HideFlags.DontSave;
        //var segmentation = _customPassVolume.customPasses.Find(_ => _.name == "Depth Pass");
        //segmentation.targetColorBuffer = CustomPass.TargetBuffer.None;
        //segmentation.targetDepthBuffer = CustomPass.TargetBuffer.None;
        _customPassVolume.gameObject.SetActive(false);
    }

    public override void SaveOpenEXR()
    {
        if (ElementFrameBuffer)
        {
            SaveSimple(FlipResult(),"Depth", "Z___");
        }
    }

    protected override void PreRender()
    {
        _customPassVolume.gameObject.SetActive(true);
        //var segmentation = _customPassVolume.customPasses.Find(_ => _.name == "Depth Pass");
        //segmentation.targetColorBuffer = CustomPass.TargetBuffer.Camera;
        //segmentation.targetDepthBuffer = CustomPass.TargetBuffer.Camera;
    }

    protected override void PostRender()
    {
        //var segmentation = _customPassVolume.customPasses.Find(_ => _.name == "Depth Pass");
        //segmentation.targetColorBuffer = CustomPass.TargetBuffer.None;
        //segmentation.targetDepthBuffer = CustomPass.TargetBuffer.None;
        _customPassVolume.gameObject.SetActive(false);
    }
}
