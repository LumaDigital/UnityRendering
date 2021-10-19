using HeadlessStudio.OfflineRenderHD.Runtime;
using HeadlessStudio.OfflineRenderHD.Runtime.Elements;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[Serializable]
[ElementOptions("Segmentation Pass")]
public class SegmentationRenderPass : BaseRenderElement
{
    public void OnEnable()
    {
        Name = "Segmentation Pass";
    }

    [NonSerialized]
    private CustomPassVolume _customPassVolume;

    public override void InitializeElement()
    {
        base.InitializeElement();
        var customPassVolume = Resources.Load<CustomPassVolume>("Segmentation Pass");
        if (customPassVolume == null)
        {
            Debug.LogError("Missing Segmentation Prefab");
            Enabled = false;
            return;
        }
        _customPassVolume = Instantiate(customPassVolume.gameObject).GetComponent<CustomPassVolume>();
        _customPassVolume.hideFlags = HideFlags.DontSave;
        //var segmentation = _customPassVolume.customPasses.Find(_ => _.name == "Segmentation");
        //segmentation.targetColorBuffer = CustomPass.TargetBuffer.None;
        //segmentation.targetDepthBuffer = CustomPass.TargetBuffer.None;
        //_customPassVolume.enabled = false;
        _customPassVolume.gameObject.SetActive(false);
    }

    protected override void PreRender()
    {
        //_customPassVolume.enabled = true;
        _customPassVolume.gameObject.SetActive(true);
        //var segmentation = _customPassVolume.customPasses.Find(_ => _.name == "Segmentation");
        //segmentation.targetColorBuffer = CustomPass.TargetBuffer.Camera;
        //segmentation.targetDepthBuffer = CustomPass.TargetBuffer.Camera;
    }

    protected override void PostRender()
    {
        //_customPassVolume.enabled = false;
        //var segmentation = _customPassVolume.customPasses.Find(_ => _.name == "Segmentation");
        //segmentation.targetColorBuffer = CustomPass.TargetBuffer.None;
        //segmentation.targetDepthBuffer = CustomPass.TargetBuffer.None;
        _customPassVolume.gameObject.SetActive(false);
    }
}
