using HeadlessStudio.OfflineRenderHD.Runtime.Elements;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

/// <summary>
/// Motion vectors needs to be the first render element to be calculate, else we just get black.
/// </summary>
[Serializable]
[ElementOptions("MotionVector Pass", 101)]
public class MotionVectorRenderPass : BaseRenderElement
{
    public void OnEnable()
    {
        Name = "MotionVector Pass";
    }

    [NonSerialized]
    private CustomPassVolume _customPassVolume;

    public override void InitializeElement()
    {
        base.InitializeElement();
        var customPassVolume = Resources.Load<CustomPassVolume>("MotionVectorsElementPass");
        if (customPassVolume == null)
        {
            Debug.LogError("Missing MotionVectorsElementPass Prefab");
            Enabled = false;
            return;
        }
        _customPassVolume = Instantiate(customPassVolume.gameObject).GetComponent<CustomPassVolume>();
        _customPassVolume.hideFlags = HideFlags.DontSave;
        //var segmentation = _customPassVolume.customPasses.Find(_ => _.name == "Motion Pass");
        //segmentation.targetColorBuffer = CustomPass.TargetBuffer.None;
        //segmentation.targetDepthBuffer = CustomPass.TargetBuffer.None;
        _customPassVolume.gameObject.SetActive(false);
    }

    protected override void PreRender()
    {
        _customPassVolume.gameObject.SetActive(true);
        //var segmentation = _customPassVolume.customPasses.Find(_ => _.name == "Motion Pass");
        //segmentation.targetColorBuffer = CustomPass.TargetBuffer.Camera;
        //segmentation.targetDepthBuffer = CustomPass.TargetBuffer.Camera;
    }

    protected override void PostRender()
    {
        //var segmentation = _customPassVolume.customPasses.Find(_ => _.name == "Motion Pass");
        //segmentation.targetColorBuffer = CustomPass.TargetBuffer.None;
        //segmentation.targetDepthBuffer = CustomPass.TargetBuffer.None;
        _customPassVolume.gameObject.SetActive(false);
    }
}
