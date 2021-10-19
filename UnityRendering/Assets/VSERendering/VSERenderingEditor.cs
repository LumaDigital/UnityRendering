using System;
using System.Reflection;
using System.Linq;
using System.IO;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using UnityEngine.Timeline;


using HeadlessStudio.OfflineRenderHD.Runtime;
using Utils;

[CustomEditor(typeof(VSERendering))]
public class VSERenderingEditor : Editor
{
    private static List<string> ExpectedCommands =
        new List<string>()
            {
                        "-FRAMES",
                        "-CUTSCENE",
                        "-OUTPUT",
                        "-MAKEVVOUTPUT"
            };

    private const int EditorSpace = 10;
    private const int EditorLabelWidth = 175;

    private const string CutsceneAnimationsDirectory = "N:\\Cricket\\Cutscene Playables";

    private const string VisionVideoEncodeType = "VP8_UV20M4L8";

    private const string BackExrName = "tempBack";
    private const string ActorsExrName = "tempActors";
    private const string UVsExrName = "tempUVs";

    private const string MainPngName = "Main";
    private const string LightPngName = "Light";
    private const string StencilPngName = "Stencil";
    private const string MapPngName = "Map";
    private const string UVPngName = "UV";

    private const string ExrExtension = "exr";
    private const string PngExtension = "png";
    private const string VVExtension = "vv";

    private string showOrHideSettingsText;
    private int externalToolsDelayTicks;

    private Color RenderCancelButtonColour;
    private string RenderCancelText = "Busy";

    #region Properties
    private VSERendering VseRendering
    {
        get
        {
            if (vseRendering == null)
            {
                // Using FindObject because target is null during 
                // application.playing in CustomUpdating
                vseRendering = FindObjectOfType<VSERendering>();
            }
            return vseRendering;
        }
    }
    private VSERendering vseRendering;

    private string TempInputAnimationDirectory
    {
        get
        {
            if (tempInputAnimationDirectory == null ||
                tempInputAnimationDirectory == string.Empty)
            {
                tempInputAnimationDirectory = 
                    Path.Combine(
                        "Assets",
                        "TempAssets",
                        "Playables",
                        "Rendering");
            }

            if (!Directory.Exists(tempInputAnimationDirectory))
                Directory.CreateDirectory(tempInputAnimationDirectory);

            return tempInputAnimationDirectory;
        }
    }
    private string tempInputAnimationDirectory;

    private bool Rendering
    {
        get
        {
            return ExrInspectorProgressColour == Color.green || 
                CompInspectorProgressColour == Color.green || 
                VisionVideoInspectorProgressColour == Color.green;
        }
    }

    private string CutsceneAnimationsPath
    {
        get
        {
            return Path.Combine(CutsceneAnimationsDirectory, VseRendering.CutsceneName);
        }
    }

    private string VideoScalingToolPath
    {
        get
        {
            if (videoScalingToolPath == null || videoScalingToolPath == string.Empty)
            {
                videoScalingToolPath =
                    Application.dataPath + 
                    "//VSERendering//ExternalTools//VideoScaling//VideoScaling.py";
            }
            return videoScalingToolPath;
        }
    }
    private string videoScalingToolPath;

    private string CompVVPath
    {
        get
        {
            if (compVVPath == null || compVVPath == string.Empty)
            {
                compVVPath =
                    Application.dataPath + 
                    "//VSERendering//ExternalTools//compvv.exe";
            }
            return compVVPath;
        }
    }
    private string compVVPath;

    private string MakeVVPath
    {
        get
        {
            if (makeVVPath == null || makeVVPath == string.Empty)
            {
                makeVVPath =
                    Application.dataPath +
                    "//VSERendering//ExternalTools//makevv.exe";
            }
            return makeVVPath;
        }
    }
    private string makeVVPath;

    private string BackDummyExrPath
    {
        get
        {
            if (backDummyExrPath == null || backDummyExrPath == string.Empty)
            {
                backDummyExrPath =
                    Application.dataPath + 
                    "//VSERendering//DummyEXR//BackDummy.exr";
            }
            return backDummyExrPath;
        }
    }
    private string backDummyExrPath;

    private GUIStyle HoverButtonStyle
    {
        get
        {
            if (hoverButtonStyle == null)
            {
                // GUI.Skin.button style performs hover features faster than other skins
                hoverButtonStyle = new GUIStyle(GUI.skin.button);
                hoverButtonStyle.fontStyle = FontStyle.Bold;
                hoverButtonStyle.alignment = TextAnchor.MiddleLeft;
                hoverButtonStyle.hover.textColor = Color.blue;
            }
            return hoverButtonStyle;
        }
    }
    private GUIStyle hoverButtonStyle;

    #region EditorPrefs
    // OfflineRender will still show a progress bar after cancelling
    // Thereofore a delay is used to wait for the application to stop
    // before clearing
    private bool DelayProgressBarClearing
    {
        get
        {
            return EditorPrefs.GetBool("VSERendering_DelayProgressBarClearing");
        }
        set
        {
            EditorPrefs.SetBool("VSERendering_DelayProgressBarClearing", value);
        }
    }
    private bool RenderAll
    {
        get
        {
            return EditorPrefs.GetBool("VSERendering_RenderAll");
        }
        set
        {
            EditorPrefs.SetBool("VSERendering_RenderAll", value);
        }
    }

    private bool CompWorkaroundRender
    {
        get
        {
            return EditorPrefs.GetBool(
                "VSERendering_CompWorkaroundRender");
        }
        set
        {
            EditorPrefs.SetBool(
                "VSERendering_CompWorkaroundRender",
                value);
        }
    }

    private bool RenderBack
    {
        get
        {
            return EditorPrefs.GetBool(
                "VSERendering_RenderBack");
        }
        set
        {
            EditorPrefs.SetBool(
                "VSERendering_RenderBack",
                value);
        }
    }

    private bool RenderActors
    {
        get
        {
            return EditorPrefs.GetBool(
                "VSERendering_RenderActors");
        }
        set
        {
            EditorPrefs.SetBool(
                "VSERendering_RenderActors",
                value);
        }
    }

    private bool RenderUVs
    {
        get
        {
            return EditorPrefs.GetBool(
                "VSERendering_RenderUVs");
        }
        set
        {
            EditorPrefs.SetBool(
                "VSERendering_RenderUVs",
                value);
        }
    }

    private bool ExrRendering
    {
        get
        {
            return EditorPrefs.GetBool(
                "VSERendering_ExrRendering");
        }
        set
        {
            EditorPrefs.SetBool(
                "VSERendering_ExrRendering",
                value);
        }
    }

    private bool RunCompositingTool
    {
        get
        {
            return EditorPrefs.GetBool(
                "VSERendering_CompositeRenders");
        }
        set
        {
            EditorPrefs.SetBool(
                "VSERendering_CompositeRenders",
                value);
        }
    }

    private bool RunMakeVisionVideoTool
    {
        get
        {
            return EditorPrefs.GetBool(
                "VSERendering_MakeVisionVideoFile");
        }
        set
        {
            EditorPrefs.SetBool(
                "VSERendering_MakeVisionVideoFile",
                value);
        }
    }

    private string RenderLog
    {
        get
        {
            return EditorPrefs.GetString(
                "VSERendering_RenderLog");
        }
        set
        {
            EditorPrefs.SetString(
                "VSERendering_RenderLog",
                value);
        }
    }

    private Color ExrInspectorProgressColour
    {
        get
        {
            return EditorPrefsUtils.GetColour(
                "VSERendering_ExrProgressColour");
        }
        set
        {
            EditorPrefsUtils.SetColour(
                "VSERendering_ExrProgressColour",
                ref value);
        }
    }

    private Color CompInspectorProgressColour
    {
        get
        {
            return EditorPrefsUtils.GetColour(
                "VSERendering_CompInspectorProgressColour");
        }
        set
        {
            EditorPrefsUtils.SetColour(
                "VSERendering_CompInspectorProgressColour",
                ref value);
        }
    }

    private Color VisionVideoInspectorProgressColour
    {
        get
        {
            return EditorPrefsUtils.GetColour(
                "VSERendering_VisionVideoInspectorProgressColour");
        }
        set
        {
            EditorPrefsUtils.SetColour(
                "VSERendering_VisionVideoInspectorProgressColour",
                ref value);
        }
    }
    #endregion
    #endregion

    private static void CommandlineInitialization()
    {
        string[] commands = Environment.GetCommandLineArgs();
        List<bool> availableCommands =
            Enumerable.Repeat(false, ExpectedCommands.Count).ToList();
        availableCommands[availableCommands.Count - 1] = true; // -MAKEVVOUTPUT can be left empty

        string errorMessage = string.Empty;
        int frameCount = 0;
        string cutsceneName = string.Empty;
        string outputPath = string.Empty;
        string makeVVOutputPath = string.Empty;

        // Command detection
        for (int i = 0; i < commands.Length; i++)
        {
            if (i == commands.Length - 1)
            {
                break;
            }

            int nextArgumentIndex = i + 1;
            string argumentValue = commands[nextArgumentIndex];
            switch (commands[i].ToUpper())
            {
                case "-FRAMES":
                    {
                        int frames;
                        if (argumentValue != string.Empty && int.TryParse(argumentValue, out frames))
                        {
                            availableCommands[0] = true;
                            frameCount = frames;
                        }
                        else
                            errorMessage += "-frames needs to be an integer\n";

                        break;
                    }

                case "-CUTSCENE":
                    {
                        if (Directory.Exists(Path.Combine(CutsceneAnimationsDirectory, argumentValue)))
                        {
                            availableCommands[1] = true;
                            cutsceneName = argumentValue;
                        }
                        else
                            errorMessage += "-Missing argumentValue playables in " + CutsceneAnimationsDirectory + "\n";

                        break;
                    }

                case "-OUTPUT":
                    {
                        if (Directory.Exists(argumentValue))
                        {
                            availableCommands[2] = true;
                            outputPath = argumentValue;
                        }
                        else
                            errorMessage += "-Output needs to be an existing directory path\n";

                        break;
                    }

                case "-MAKEVVOUTPUT":
                    {
                        if (argumentValue == string.Empty || Directory.Exists(argumentValue))
                        {
                            makeVVOutputPath = argumentValue;
                        }
                        else
                        {
                            errorMessage += "-MakeVVOutput needs to be an existing directory path or left empty\n";
                            availableCommands[3] = false;
                        }

                        break;
                    }
            }
        }

        // Error Handling
        if (availableCommands.Any(element => element == false))
        {
            errorMessage += "\n\nMissing/Error with the following commands: ";

            for (int i = 0; i < availableCommands.Count; i++)
            {
                if (availableCommands[i] == false)
                    errorMessage += "\n" + ExpectedCommands[i];
            }
        }

        if (errorMessage != string.Empty)
        {
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string path = Path.Combine(desktopPath, "UnityRenderingLog.txt");
            if (File.Exists(path))
                File.Delete(path);

            File.WriteAllText(
                path,
                errorMessage);

            EditorApplication.Exit(1);
            return;
        }

        VSERendering vseRenderingComponent = FindObjectOfType<VSERendering>();
        vseRenderingComponent.RenderInitializedFromCutscene = true;
        vseRenderingComponent.EndFrame = (uint)frameCount;
        vseRenderingComponent.CutsceneName = cutsceneName;
        vseRenderingComponent.OutputPath = outputPath;
        vseRenderingComponent.MakeVVOutputPath = makeVVOutputPath;
        vseRenderingComponent.CloseApplicationAfterMakeVV = true;
        vseRenderingComponent.UpdateAllRenderPaths();

        Selection.activeGameObject = vseRenderingComponent.gameObject;
        vseRenderingComponent.FocusEditorForRendering();
    }
    private void OnEnable()
    {
        if (VseRendering.TargetCamera.GetComponent<OfflineRenderHD>() == null)
        {
            VseRendering.TargetCamera.gameObject.AddComponent<OfflineRenderHD>();
            VseRendering.OfflineRenderHD.ClearRenderElements();
        }
        VseRendering.UpdateOfflineRenderComponent();

        EditorApplication.update += CustomUpdate;

        if (vseRendering.RenderInitializedFromCutscene)
        {
            CompGarbageCleanup();

            RenderLog = string.Empty;
            CompWorkaroundRender = true;

            HandleDummyExrRenders();
            ExrInspectorProgressColour = Color.green;
            VseRendering.UpdateOriginalMaterials();
            HandleExrRenders();

            vseRendering.RenderInitializedFromCutscene = false;

            GameObject timelineObject = GameObject.Find("Timeline");
            Selection.activeGameObject = timelineObject;

            // make sure the window is open
            EditorApplication.ExecuteMenuItem("Window/Sequencing/Timeline");

            Type timelineWindow = 
                Type.GetType("UnityEditor.Timeline.TimelineWindow, Unity.Timeline.Editor");

            // Assuming there always only one timeline window
            var timeline_window = Resources.FindObjectsOfTypeAll(timelineWindow)[0] as EditorWindow;

            PropertyInfo propertyInfo = timeline_window.GetType().GetProperty("locked");
            propertyInfo.SetValue(timeline_window, true, null);
            timeline_window.Repaint();

            Selection.activeGameObject = vseRendering.gameObject;
        }
    }

    private void CustomUpdate()
    {
        if (Rendering && RenderCancelButtonColour != Color.red)
        {
            RenderCancelButtonColour = Color.red;
            RenderCancelText = "Cancel";
        }

        if (!Rendering && RenderCancelButtonColour != Color.green)
        {
            RenderCancelButtonColour = Color.green;
            RenderCancelText = "Render";
        }

        if (externalToolsDelayTicks > 0)
        {
            Repaint();
            externalToolsDelayTicks--;
            if (externalToolsDelayTicks == 0)
            {
                StartExternalToolsRender();
            }
        }

        if (Application.isPlaying && !ExrRendering)
        {
            ExrRendering = true;
        }

        if (ExrRendering && !Application.isPlaying)
        {
            // EXR pass options are set to false after completion so that the
            // toggles serve as indicators to what pass is currently rendering
            if (RenderBack)
            {
                RenderBack = false;
                HandleExrRenders();
            }
            else if (RenderActors)
            {
                RenderActors = false;
                HandleExrRenders();
            }
            else
            {
                RenderUVs = false;
                HandleExrRenders();
            }
            ExrRendering = false;
        }
    }
    private void CompGarbageCleanup()
    {
        if (Directory.Exists(VseRendering.PngPath))
        {
            foreach (FileInfo file in new DirectoryInfo(VseRendering.PngPath).GetFiles())
            {
                file.Delete();
            }
        }
        if (Directory.Exists(VseRendering.RenderResolutionPngPath))
        {
            foreach (FileInfo file in new DirectoryInfo(VseRendering.RenderResolutionPngPath).GetFiles())
            {
                file.Delete();
            }
        }
    }

    public override void OnInspectorGUI()
    {
        GUILayout.BeginVertical("box");

        if (DelayProgressBarClearing && !Application.isPlaying)
        {
            EditorUtility.ClearProgressBar();
            DelayProgressBarClearing = false;
            Debug.Log("Rendering Cancelled");
        }

        GUI.backgroundColor = RenderCancelButtonColour;
        if (Rendering && GUILayout.Button(RenderCancelText))
        {
            ExrInspectorProgressColour =
                CompInspectorProgressColour =
                VisionVideoInspectorProgressColour =
                Color.clear;

            RenderBack =
                RenderActors =
                RenderUVs =
                CompWorkaroundRender =
                RunCompositingTool =
                RunMakeVisionVideoTool =
                false;

            externalToolsDelayTicks = 0;
            EditorApplication.ExitPlaymode();
            DelayProgressBarClearing = true;
        }

        if (!Rendering && GUILayout.Button(RenderCancelText))
        {
            if (Directory.Exists(VseRendering.OutputPath))
            {
                RenderLog = string.Empty;
                if (CompWorkaroundRender)
                {
                    HandleDummyExrRenders();
                }

                if (RenderBack || RenderActors || RenderUVs)
                {
                    CompGarbageCleanup();
                    ExrInspectorProgressColour = Color.green;
                    VseRendering.UpdateOriginalMaterials();
                    HandleExrRenders();
                }
                else
                {
                    DelayExternalToolsRender(delayTicks: 2);
                }
            }
            else
            {
                EditorUtility.DisplayDialog(
                    title: "Error: Incorrect path",
                    message: "Path:\n" + vseRendering.OutputPath + "\nDoes not exist",
                    ok: "Okay");
            }
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(EditorSpace);
        EditorGUIUtility.labelWidth = EditorLabelWidth;
        CompWorkaroundRender =
            EditorGUILayout.Toggle("CompVV Workaround Render", CompWorkaroundRender);

        EditorGUI.BeginDisabledGroup(CompWorkaroundRender);
        DrawExrGui();
        DrawCompositeGui();
        DrawMakeVVGui();
        EditorGUI.EndDisabledGroup();

        GUILayout.EndVertical();

        GUILayout.BeginHorizontal();
        GUILayout.Space(EditorSpace);

        VseRendering.ShowSettings =
            EditorGUILayout.Foldout(
                VseRendering.ShowSettings,
                showOrHideSettingsText,
                toggleOnLabelClick: true);

        GUILayout.EndHorizontal();

        if (VseRendering.ShowSettings)
        {
            showOrHideSettingsText = "Hide Settings";
            base.OnInspectorGUI();
        }
        else
        {
            showOrHideSettingsText = "Show Settings";
        }

        if (GUILayout.Button("Populate Skinned Meshes"))
        {
            VseRendering.ATeamKitRenderers = new List<SkinnedMeshRenderer>();
            VseRendering.BTeamKitRenderers = new List<SkinnedMeshRenderer>();

            SkinnedMeshRenderer[] items = FindObjectsOfType<SkinnedMeshRenderer>(true);
            foreach (SkinnedMeshRenderer item in items)
            {
                switch (item.tag)
                {
                    case "TeamAKit":
                        VseRendering.ATeamKitRenderers.Add(item);
                        break;
                    case "TeamBKit":
                        VseRendering.BTeamKitRenderers.Add(item);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    private void DrawExrGui()
    {
        string RenderOptionName =
            ExrInspectorProgressColour == Color.green ? "EXR - Busy" : "EXR";

        GUILayout.Space(EditorSpace);
        GUI.backgroundColor = ExrInspectorProgressColour;
        EditorGUILayout.LabelField(string.Empty);
        GUI.Button(
            GUILayoutUtility.GetLastRect(),
            new GUIContent(RenderOptionName, VseRendering.ExrPath),
            HoverButtonStyle);

        string renderResolutionString = string.Empty;
        string outputResolutionString = string.Empty;
        if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
        {
            renderResolutionString =
                "\t: " + vseRendering.RenderResolutionWidth + "x" + vseRendering.RenderResolutionHeight;
            outputResolutionString =
                "\t: " + vseRendering.OutputResolutionWidth + "x" + vseRendering.OutputResolutionHeight;
        }

        GUI.backgroundColor = Color.white;

        GUILayout.BeginHorizontal();
        GUILayout.Space(EditorSpace);
        EditorGUI.BeginChangeCheck();
        RenderAll =
            EditorGUILayout.Toggle("Full Render", RenderBack && RenderActors && RenderUVs);

        if (EditorGUI.EndChangeCheck())
        {
            RenderBack = RenderActors = RenderUVs = RenderAll;
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(EditorSpace);
        RenderBack = 
            EditorGUILayout.Toggle("Back" + renderResolutionString, RenderBack);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(EditorSpace);
        RenderActors = 
            EditorGUILayout.Toggle("Actors" + renderResolutionString, RenderActors);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(EditorSpace);
        RenderUVs = 
            EditorGUILayout.Toggle("UVs" + outputResolutionString, RenderUVs);
        GUILayout.EndHorizontal();
    }

    private void DrawCompositeGui()
    {
        string RenderOptionName =
            CompInspectorProgressColour == Color.green ?
            "Compositing - Busy" :
            "Compositing";

        GUILayout.Space(EditorSpace);
        GUI.backgroundColor = CompInspectorProgressColour;
        EditorGUILayout.LabelField(string.Empty);
        GUI.Button(
            GUILayoutUtility.GetLastRect(),
            new GUIContent(RenderOptionName, VseRendering.PngPath),
            HoverButtonStyle);
        GUI.backgroundColor = Color.white;

        GUILayout.BeginHorizontal();
        GUILayout.Space(EditorSpace);
        RunCompositingTool = 
            EditorGUILayout.Toggle("Composite Renders", RunCompositingTool);
        GUILayout.EndHorizontal();
    }

    private void DrawMakeVVGui()
    {
        string RenderOptionName =
            VisionVideoInspectorProgressColour == Color.green ?
            "Vision Video - Busy" :
            "Vision Video";

        GUILayout.Space(EditorSpace);
        GUI.backgroundColor = VisionVideoInspectorProgressColour;
        EditorGUILayout.LabelField(string.Empty);
        GUI.Button(
            GUILayoutUtility.GetLastRect(),
            new GUIContent(RenderOptionName, VseRendering.VvPath),
            HoverButtonStyle);
        GUI.backgroundColor = Color.white;

        GUILayout.BeginHorizontal();
        GUILayout.Space(EditorSpace);
        RunMakeVisionVideoTool = EditorGUILayout.Toggle("Make VV file", RunMakeVisionVideoTool);
        GUILayout.EndHorizontal();
    }

    private void DelayExternalToolsRender(int delayTicks)
    {
        if (RunCompositingTool)
        {
            CompInspectorProgressColour = Color.green;
        }
        else if (RunMakeVisionVideoTool)
        {
            VisionVideoInspectorProgressColour = Color.green;
        }

        externalToolsDelayTicks = delayTicks;
    }

    private void StartExternalToolsRender()
    {
        if (RunCompositingTool)
        {
            RunCompositingTool = false;
            CompositeRenders(out bool cancelComposite);
            CompInspectorProgressColour = Color.clear;

            if (!cancelComposite)
            {
                if (CompWorkaroundRender)
                {
                    RenderBack = true;
                    RenderActors = true;
                    RunCompositingTool = true;
                    RunMakeVisionVideoTool = true;
                    CompWorkaroundRender = false;

                    ExrInspectorProgressColour = Color.green;
                    VseRendering.UpdateOriginalMaterials();
                    HandleExrRenders();
                }
                else
                {
                    ScaleImages();
                    DelayExternalToolsRender(delayTicks: 1);
                }
            }
        }
        else if (RunMakeVisionVideoTool)
        {
            RunMakeVisionVideoTool = false;
            MakeVisionVideoFile();
        }
    }

    private void HandleDummyExrRenders()
    {
        if (!Directory.Exists(VseRendering.ExrPath))
            Directory.CreateDirectory(VseRendering.ExrPath);

        RenderBack = false;
        RenderActors = true;
        RenderUVs = true;
        RunCompositingTool = true;
        RunMakeVisionVideoTool = false;

        for (int i = (int)VseRendering.StartFrame; i <= (int)VseRendering.EndFrame; i++)
        {
            float progressPercentage = ((float)i - 1) / VseRendering.EndFrame;
            EditorUtility.DisplayProgressBar(
                title: "Generating Dummy EXR files",
                info: "Progress: frame " +
                    (i - 1) +
                    " / " +
                    VseRendering.EndFrame +
                    " (" +
                    (progressPercentage * 100) +
                    "%)",
                progress: progressPercentage);

            string framePadding = "." + i.ToString().PadLeft(4, '0') + ".";

            string newBackDummyPath =
                Path.Combine(VseRendering.ExrPath, BackExrName + framePadding + ExrExtension);

            File.Copy(BackDummyExrPath, newBackDummyPath, overwrite: true);
        }
        EditorUtility.ClearProgressBar();

        RenderLog += "Back Exr dummy files generated\n";
    }

    private bool HandleInputAnimations()
    {
        if (VseRendering.CutsceneName == string.Empty)
        {
            Debug.Log("Cutscene name is empty, continuing render with no actors");
            return true;
        }
        else if (!Directory.Exists(CutsceneAnimationsPath))
        {
            Debug.Log(
                "Animations Path:\n\"" +
                CutsceneAnimationsPath + 
                "\" does not exist, ensure " + 
                CutsceneAnimationsDirectory +
                " has the animation playables generated from " +
                VseRendering.CutsceneName);

            ExrInspectorProgressColour =
                CompInspectorProgressColour =
                VisionVideoInspectorProgressColour =
                Color.clear;
            return false;
        }
        else
        {
            SetUpRenderTimeline();
            return true;
        }
    }

    private void SetUpRenderTimeline()
    {
        PlayableDirector director = FindObjectOfType<PlayableDirector>();
        TimelineAsset playableAsset = (TimelineAsset)director.playableAsset;

        // Deleting all tracks on the render timeline
        playableAsset.GetOutputTracks().Select(element => playableAsset.DeleteTrack(element)).ToList();

        Transform renderPreviewEntities = GameObject.Find("RenderPreviewEntities").transform;

        foreach (string animationPreviewFilePath in Directory.GetFiles(CutsceneAnimationsPath))
        {
            string fileName = Path.GetFileName(animationPreviewFilePath);
            string newFilePath = Path.Combine(TempInputAnimationDirectory, fileName);

            if (fileName.Contains("meta"))
                continue;

            File.Copy(animationPreviewFilePath, newFilePath, overwrite: true);
            Transform animationPreviewTransfrom = renderPreviewEntities.FindDeepChild(fileName.Split('.')[0]);

            //MG: Need to account for custom preview entities such as the linesman
            if (animationPreviewTransfrom == null)
                continue;

            animationPreviewTransfrom.parent.gameObject.SetActive(true);

            AssetDatabase.Refresh();
            AnimationClip animationClip =
                (AnimationClip)AssetDatabase.LoadAssetAtPath(newFilePath, typeof(AnimationClip));

            // Create new track
            AnimationTrack newTrack = playableAsset.CreateTrack<AnimationTrack>(null, "Animation Track");

            // Create the animation clip and initialize its properties
            TimelineClip timelinesClip = newTrack.CreateClip(animationClip);
            timelinesClip.duration = animationClip.averageDuration;
            timelinesClip.displayName = animationClip.name;

            director.SetGenericBinding(newTrack, animationPreviewTransfrom.gameObject);
            director.RebindPlayableGraphOutputs();
        }
    }

    private void HandleExrRenders()
    {
        if (!HandleInputAnimations())
            return;

        if (!Directory.Exists(VseRendering.ExrPath))
            Directory.CreateDirectory(VseRendering.ExrPath);

        if (RenderBack)
        {
            VseRendering.SetupBackRender(CompWorkaroundRender);
            RenderLog += "Offline Render: Back\n";
            EditorApplication.EnterPlaymode();
            FindObjectOfType<CrowdManager>(true).gameObject.SetActive(true);
        }
        else if (RenderActors)
        {
            VseRendering.SetupActorsRender(CompWorkaroundRender);
            RenderLog += "Offline Render: Actors\n";
            EditorApplication.EnterPlaymode();
            FindObjectOfType<CrowdManager>(true).gameObject.SetActive(false);
        }
        else if (RenderUVs)
        {
            VseRendering.SetupUVsRender();
            RenderLog += "Offline Render: UVs\n";
            EditorApplication.EnterPlaymode();
            FindObjectOfType<CrowdManager>(true).gameObject.SetActive(false);
        }
        else
        {
            QualitySettings.SetQualityLevel((int)VSERendering.QualitySetting.HighQuality);
            ExrInspectorProgressColour = Color.clear;
            VseRendering.SetNonActorsMaterial();
            DelayExternalToolsRender(delayTicks: 1);
        }
    }

    private void CompositeRenders(out bool cancelComposite)
    {
        if (!Directory.Exists(VseRendering.PngPath))
            Directory.CreateDirectory(VseRendering.PngPath);

        if (!Directory.Exists(VseRendering.RenderResolutionPngPath))
            Directory.CreateDirectory(VseRendering.RenderResolutionPngPath);

        cancelComposite = false;
        RenderLog += "\nCompVV Tool: ";
        for (int i = (int)VseRendering.StartFrame; i <= (int)VseRendering.EndFrame; i++)
        {
            float progressPercentage = ((float)i - 1) / VseRendering.EndFrame;

            if (EditorUtility.DisplayCancelableProgressBar(
                    title: "Compositing images (compvv.exe)",
                    info: "Progress: frame " +
                        (i - 1) +
                        " / " +
                        VseRendering.EndFrame +
                        " (" +
                        (progressPercentage * 100) +
                        "%)",
                    progress: progressPercentage))
            {
                RenderLog += "\n----Render cancelled by user----";
                RunMakeVisionVideoTool = false;
                cancelComposite = true;
                break;
            }

            string framePadding = "." + i.ToString().PadLeft(4, '0') + ".";

            // EXR input files
            string backExrFilePath =
                Path.Combine(VseRendering.ExrPath, BackExrName + framePadding + ExrExtension);
            string actorsExrFilePath =
                Path.Combine(VseRendering.ExrPath, ActorsExrName + framePadding + ExrExtension);
            string uvsExrFilePath =
                Path.Combine(VseRendering.ExrPath, UVsExrName + framePadding + ExrExtension);

            // PNG output files
            string mainPngFilePath =
                Path.Combine(VseRendering.RenderResolutionPngPath, MainPngName + framePadding + PngExtension);
            string lightPngFilePath =
                Path.Combine(VseRendering.RenderResolutionPngPath, LightPngName + framePadding + PngExtension);
            string stencilPngFilePath =
                Path.Combine(VseRendering.PngPath, StencilPngName + framePadding + PngExtension);
            string mapPngFilePath =
                Path.Combine(VseRendering.PngPath, MapPngName + framePadding + PngExtension);
            string uvPngFilePath =
                Path.Combine(VseRendering.PngPath, UVPngName + framePadding + PngExtension);

            string command =
                CompVVPath +
                " -back " +
                backExrFilePath +
                " -actors " +
                actorsExrFilePath +
                " -uv " +
                uvsExrFilePath +
                " -omain " +
                mainPngFilePath +
                " -olight " +
                lightPngFilePath +
                " -ostencil " +
                stencilPngFilePath +
                " -omap " +
                mapPngFilePath +
                " -ouv " +
                uvPngFilePath;

            RenderLog += CommandlineUtils.ExecuteCommandline(command);
        }
        EditorUtility.ClearProgressBar();

        WriteLogFile(RenderLog);
    }

    private void ScaleImages()
    {
        if (VseRendering.OutputResolutionHeight != VseRendering.RenderResolutionHeight ||
            VseRendering.OutputResolutionWidth != VseRendering.RenderResolutionWidth)
        {
            string resolutionString =
                VseRendering.OutputResolutionWidth +
                "x" +
                VseRendering.OutputResolutionHeight;

            // If the scaling tool is cancelled in progress, then an intermediary "CONVERTING" file
            // will be left undeleted in the png folder, causing issues with the tool.
            string convertingFilePath = VseRendering.RenderResolutionPngPath + "//CONVERTING.PNG";
            if (File.Exists(convertingFilePath))
            {
                File.Delete(convertingFilePath);
            }

            string command =
                VideoScalingToolPath +
                " " + VseRendering.RenderResolutionPngPath +
                " " + resolutionString;

            RenderLog += "\nVideoScaling Tool: " + CommandlineUtils.ExecuteCommandline(command, useCommandlineWindow: true);
            if (RenderLog.Contains("Fail"))
            {
                RenderLog += "\n----Render cancelled by user----";
                RunMakeVisionVideoTool = false;
            }

            EditorUtility.ClearProgressBar();
            WriteLogFile(RenderLog);
        }
    }

    private void MakeVisionVideoFile()
    {
        EditorUtility.DisplayProgressBar(
            "Generating Video (makevv.exe)",
            "Generating Vision Video file",
            0);

        if (!Directory.Exists(VseRendering.VvPath))
            Directory.CreateDirectory(VseRendering.VvPath);

        string fileName =
            VseRendering.CutsceneName == string.Empty ? SceneManager.GetActiveScene().name : VseRendering.CutsceneName;

        string vvFilePath = 
            Path.Combine(VseRendering.VvPath, fileName + "." + VVExtension);

        MoveRenderResolutionImages(toInputFolder: true);
        string command =
            MakeVVPath +
            " -o " +
            vvFilePath +
            " -dir " +
            VseRendering.PngPath +
            " -type " +
            VisionVideoEncodeType;

        RenderLog += "\nMakeVV Tool: " + CommandlineUtils.ExecuteCommandline(command);
        MoveRenderResolutionImages(toInputFolder: false);
        WriteLogFile(RenderLog);

        EditorUtility.ClearProgressBar();
        VisionVideoInspectorProgressColour = Color.clear;

        if (vseRendering.CloseApplicationAfterMakeVV)
            EditorApplication.Exit(0);
    }

    private void WriteLogFile(string contentsText)
    {
        string path = Path.Combine(vseRendering.OutputPath, "Log.txt");
        if (File.Exists(path))
            File.Delete(path);

        File.WriteAllText(
            path,
            RenderLog);
    }

    private void MoveRenderResolutionImages(bool toInputFolder)
    {
        for (int i = (int)VseRendering.StartFrame; i <= (int)VseRendering.EndFrame; i++)
        {
            string framePadding = "." + i.ToString().PadLeft(4, '0') + ".";

            string mainPngPath =
                Path.Combine(vseRendering.RenderResolutionPngPath, MainPngName + framePadding + PngExtension);
            string lightPngPath =
                Path.Combine(vseRendering.RenderResolutionPngPath, LightPngName + framePadding + PngExtension);

            string newMainPngPath =
                Path.Combine(vseRendering.PngPath, MainPngName + framePadding + PngExtension);
            string newlightPngPath =
                Path.Combine(vseRendering.PngPath, LightPngName + framePadding + PngExtension);

            if (toInputFolder)
            {
                if (File.Exists(newlightPngPath))
                {
                    File.Delete(newlightPngPath);
                }
                if (File.Exists(newMainPngPath))
                {
                    File.Delete(newMainPngPath);
                }

                File.Move(lightPngPath, newlightPngPath);
                File.Move(mainPngPath, newMainPngPath);
            }
            else
            {
                if (File.Exists(lightPngPath))
                {
                    File.Delete(lightPngPath);
                }
                if (File.Exists(mainPngPath))
                {
                    File.Delete(mainPngPath);
                }

                File.Move(newlightPngPath, lightPngPath);
                File.Move(newMainPngPath, mainPngPath);
            }
        }
    }
}