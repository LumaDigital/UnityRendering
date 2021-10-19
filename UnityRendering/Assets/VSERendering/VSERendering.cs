using System.Linq;
using System.IO;
using System.Collections.Generic;

using UnityEngine;

using HeadlessStudio.OfflineRenderHD.Runtime;
using HeadlessStudio.OfflineRenderHD.Runtime.Elements;

public class VSERendering : MonoBehaviour
{
    public enum QualitySetting
    {
        HighQuality = 6,
        LowQuality = 7
    }

    private const string BackOutputName = "tempBack.${frame}";
    private const string ActorsOutputName = "tempActors.${frame}";
    private const string UVsOutputName = "tempUVs.${frame}";

    [Header("Render Settings")]
    public Camera TargetCamera;
    public string CutsceneName;
    [Tooltip("Hover over the EXR label to see how resolutions are applied to each EXR")]
    public string OutputPath;
    [Tooltip("If empty, makeVV will output to the the above path")]
    public string MakeVVOutputPath = string.Empty;
    public uint StartFrame = 0;
    public uint EndFrame = 300;
    public bool CloseApplicationAfterMakeVV;
    [Space()]
    [Tooltip("Resolution for Back and Actors EXR")]
    public uint RenderResolutionWidth = 1920;
    public uint RenderResolutionHeight = 1080;
    [Space()]
    [Tooltip("Resolution for the final file, UVs EXR uses this resolution multipled by 5")]
    public uint OutputResolutionWidth = 1280;
    public uint OutputResolutionHeight = 720;

    [Header("Materials")]
    public Material UVPassMaterial;
    public Material HairUVMaterial;
    public Material BackPassMaterial;
    public Material ActorPassMaterial;
    public Material FullMatteBlackMaterial;
    public Material MapOneMaterial;
    public Material MapTwoMaterial;
    public Material MapThreeMaterial;

    [Header("Mesh Renderers")]
    public List<SkinnedMeshRenderer> ATeamKitRenderers;
    public List<SkinnedMeshRenderer> BTeamKitRenderers;

    public List<MeshRenderer> BannerMeshRenderers;
    public List<MeshRenderer> RemainingMeshRenderers;
    public List<SkinnedMeshRenderer> RemainingSkinMeshRenderers;
    public List<SkinnedMeshRenderer> HairAndBodySkinRenderers;

    [HideInInspector]
    public List<Material> OriginalRemainingMeshMaterials;
    [HideInInspector]
    public List<Material> OriginalRemainingSkinnedMeshMaterials;
    [HideInInspector]
    public List<Material> OriginalHairAndBodySkinnedMeshMaterials;
    [HideInInspector]
    public string ExrPath;
    [HideInInspector]
    public string PngPath;
    [HideInInspector]
    public string RenderResolutionPngPath;
    [HideInInspector]
    public string VvPath;
    [HideInInspector]
    public bool ShowSettings;
    [HideInInspector]
    public bool RenderInitializedFromCutscene;

    public OfflineRenderHD OfflineRenderHD
    {
        get
        {
            if (offlineRenderHD == null)
            {
                offlineRenderHD = FindObjectOfType<OfflineRenderHD>();
            }
            return offlineRenderHD;
        }
    }
    private OfflineRenderHD offlineRenderHD;

    private string FocusWindowToolPath
    {
        get
        {
            if (focusWindowToolPath == null || focusWindowToolPath == string.Empty)
            {
                focusWindowToolPath =
                    Application.dataPath +
                    "//VSERendering//ExternalTools//FocusWindow//FocusWindow.exe";
            }
            return focusWindowToolPath;
        }
    }
    private string focusWindowToolPath;

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            UpdateAllRenderPaths();
        }
    }

    public void FocusEditorForRendering()
    {
        string command = FocusWindowToolPath + " Unity";
        UnityEngine.Debug.Log(command);
        CommandlineUtils.ExecuteCommandline(command, useCommandlineWindow: true);
    }

    public void UpdateAllRenderPaths()
    {
        ExrPath = Path.Combine(OutputPath, "EXR");
        PngPath = Path.Combine(OutputPath, "PNG");
        RenderResolutionPngPath = Path.Combine(OutputPath, "MainPNG");
        VvPath = 
            MakeVVOutputPath == string.Empty ? Path.Combine(OutputPath, "VV") : Path.Combine(MakeVVOutputPath, "VV");
        UpdateOfflineRenderComponent();
    }

    public void UpdateOfflineRenderComponent()
    {
        OfflineRenderHD.Save = true;
        OfflineRenderHD.offscreen = true;
        OfflineRenderHD.startFrame = (int)StartFrame;
        OfflineRenderHD.endFrame = (int)EndFrame;
        OfflineRenderHD.outputPath = ExrPath;
        OfflineRenderHD.outputFrameFormat = "0000";
        OfflineRenderHD.captureFramerate = 30;
    }

    public void SetupBackRender(bool compVVWorkaround)
    {
        QualitySettings.SetQualityLevel((int)QualitySetting.HighQuality);

        SetActorsMaterial(BackPassMaterial);
        SetNonActorsMaterial();

        OfflineRenderHD.ClearRenderElements();
        OfflineRenderHD.renderElements.Add(
            (BackRenderElement)ScriptableObject.CreateInstance("BackRenderElement"));

        OfflineRenderHD.width =
            compVVWorkaround ? (int)OutputResolutionWidth : (int)RenderResolutionWidth;
        OfflineRenderHD.height =
            compVVWorkaround ? (int)OutputResolutionHeight : (int)RenderResolutionHeight;
        OfflineRenderHD.outputFilename = BackOutputName;
    }

    public void SetupActorsRender(bool compVVWorkaround)
    {
        SetActorsMaterial(ActorPassMaterial, true);
        SetNonActorsMaterial(FullMatteBlackMaterial);

        OfflineRenderHD.ClearRenderElements();
        OfflineRenderHD.renderElements.Add(
            (DiffuseRenderElement)ScriptableObject.CreateInstance("DiffuseRenderElement"));
        OfflineRenderHD.renderElements.Add(
            (GIRenderElement)ScriptableObject.CreateInstance("GIRenderElement"));
        OfflineRenderHD.renderElements.Add(
            (ReflectionRenderElement)ScriptableObject.CreateInstance("ReflectionRenderElement"));
        OfflineRenderHD.renderElements.Add(
            (SpecularRenderElement)ScriptableObject.CreateInstance("SpecularRenderElement"));
        OfflineRenderHD.renderElements.Add(
            (MatteRenderElement)ScriptableObject.CreateInstance("MatteRenderElement"));
        OfflineRenderHD.renderElements.Add(
            (StencilRenderElement)ScriptableObject.CreateInstance("StencilRenderElement"));

        OfflineRenderHD.width = 
            compVVWorkaround ? (int)OutputResolutionWidth : (int)RenderResolutionWidth;
        OfflineRenderHD.height = 
            compVVWorkaround ? (int)OutputResolutionHeight : (int)RenderResolutionHeight;

        OfflineRenderHD.outputFilename = ActorsOutputName;
        offlineRenderHD.useRGBAAsDefault = true;
    }

    public void SetupUVTextures()
    {
        foreach (SkinnedMeshRenderer skinRenderer in ATeamKitRenderers)
        {
            skinRenderer.sharedMaterial = UVPassMaterial;
        }
        foreach (SkinnedMeshRenderer skinRenderer in BTeamKitRenderers)
        {
            skinRenderer.sharedMaterial = UVPassMaterial;
        }
        foreach (MeshRenderer renderer in BannerMeshRenderers)
        {
            renderer.sharedMaterial = UVPassMaterial;
        }
        foreach (SkinnedMeshRenderer hairAndBodyRenderer in HairAndBodySkinRenderers)
        {
            hairAndBodyRenderer.sharedMaterial = HairUVMaterial;
        }
        SetNonActorsMaterial(FullMatteBlackMaterial);
    }

    public void SetupMapTextures()
    {
        foreach (SkinnedMeshRenderer skinRenderer in ATeamKitRenderers)
        {
            skinRenderer.sharedMaterial = MapOneMaterial;
        }
        foreach (SkinnedMeshRenderer skinRenderer in BTeamKitRenderers)
        {
            skinRenderer.sharedMaterial = MapTwoMaterial;
        }
        foreach (MeshRenderer renderer in BannerMeshRenderers)
        {
            renderer.sharedMaterial = MapThreeMaterial;
        }
        foreach (SkinnedMeshRenderer skinRenderer in HairAndBodySkinRenderers)
        {
            skinRenderer.sharedMaterial = MapOneMaterial;
        }
        SetNonActorsMaterial(FullMatteBlackMaterial);
    }

    public void SetupUVsRender()
    {
        foreach (SkinnedMeshRenderer skinRenderer in ATeamKitRenderers)
        {
            skinRenderer.sharedMaterial = MapOneMaterial;
        }
        foreach (SkinnedMeshRenderer skinRenderer in BTeamKitRenderers)
        {
            skinRenderer.sharedMaterial = MapTwoMaterial;
        }
        foreach (MeshRenderer renderer in BannerMeshRenderers)
        {
            renderer.sharedMaterial = MapThreeMaterial;
        }
        foreach (SkinnedMeshRenderer skinRenderer in HairAndBodySkinRenderers)
        {
            skinRenderer.sharedMaterial = MapOneMaterial;
        }
        SetNonActorsMaterial(FullMatteBlackMaterial);

        OfflineRenderHD.ClearRenderElements();
        OfflineRenderHD.renderElements.Add(
            (UVRenderElement)ScriptableObject.CreateInstance("UVRenderElement"));
        OfflineRenderHD.renderElements.Add(
            (MapRenderElement)ScriptableObject.CreateInstance("MapRenderElement"));

        // CompVV expects this pass to be 5 times the size of other pass' resolutions
        OfflineRenderHD.width = (int)OutputResolutionWidth * 5;
        OfflineRenderHD.height = (int)OutputResolutionHeight * 5;
        OfflineRenderHD.outputFilename = UVsOutputName;
    }

    // Set original materials if paramater is empty
    public void SetNonActorsMaterial(Material material = null)
    {
        foreach (MeshRenderer renderer in RemainingMeshRenderers)
        {
            if (material != null)
                renderer.sharedMaterial = material;
            else
                renderer.sharedMaterial =
                    OriginalRemainingMeshMaterials[RemainingMeshRenderers.IndexOf(renderer)];
        }
        foreach (SkinnedMeshRenderer renderer in RemainingSkinMeshRenderers)
        {
            if (material != null)
                renderer.sharedMaterial = material;
            else
                renderer.sharedMaterial =
                    OriginalRemainingSkinnedMeshMaterials[RemainingSkinMeshRenderers.IndexOf(renderer)];
        }
    }

    public void UpdateOriginalMaterials()
    {
        OriginalRemainingMeshMaterials =
            RemainingMeshRenderers.Select(element => element.sharedMaterial).ToList();

        OriginalRemainingSkinnedMeshMaterials = 
            RemainingSkinMeshRenderers.Select(element => element.sharedMaterial).ToList();

        OriginalHairAndBodySkinnedMeshMaterials =
            HairAndBodySkinRenderers.Select(element => element.sharedMaterial).ToList();
    }

    private void SetActorsMaterial(Material material, bool isActorPass = false)
    {
        ActorPassEntity [] actors = FindObjectsOfType<ActorPassEntity>();
        foreach (ActorPassEntity actor in actors)
        {
            // Use the actor pass material specified on the actor pass component and not the general supplied one
            if (isActorPass)
                actor.ApplyActorMaterial(null);
            else
                actor.ApplyActorMaterial(material);
        }

        foreach (MeshRenderer bannerRenderer in BannerMeshRenderers)
        {
            bannerRenderer.sharedMaterial = material;
        }

    }
}
