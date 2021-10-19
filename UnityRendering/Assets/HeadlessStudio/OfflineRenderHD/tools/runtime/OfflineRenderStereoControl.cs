using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

//[ExecuteInEditMode]
public class OfflineRenderStereoControl : MonoBehaviour
{
    public enum Mode
    {
        Mono = 0,
        Stereo = 1,
        StereoSBS = 2,
    }
    public Camera targetCamera;
    public GameObject leftEye;
    public GameObject rightEye;
    public float interpupillaryDistance = 0.064f;
    public float focalDistance = 10000;
    public int resolution = 2048;
    public int width = 2048;
    public Mode mode = Mode.Mono;
    public List<Camera> helperCameras = new List<Camera>();

    public RenderTexture Result => resultRT;

    public bool autoUpdate = true;
    private RTHandle resultRT;
    private ComputeBuffer _buffer;
    private ComputeShader _stereoCompute;
    private int _kernelIndex;
    private RenderTextureDescriptor _desc;
    private int _height;
    private readonly WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
    private static readonly string[] faceNames = new[]
    {
        "rightL",
        "leftL",
        "upL",
        "downL",
        "frontL",
        "backL",
        "rightR",
        "leftR",
        "upR",
        "downR",
        "frontR",
        "backR",
    };

    private ComputeBuffer BuildBuffer()
    {
        var long_min = Convert.ToDouble(Mathf.Deg2Rad * -90);
        var long_max = Convert.ToDouble(Mathf.Deg2Rad * 90);
        var lat_min = Convert.ToDouble(Mathf.Deg2Rad * -90);
        var lat_max = Convert.ToDouble(Mathf.Deg2Rad * 90);
        var ll = new[] { long_min - long_max, -long_min, lat_min - lat_max, -lat_min + Mathf.PI / 2 };
        var l = new NativeArray<double>(ll, Allocator.Temp);
        return NativeTools4VR.Build(l, width, _height, (uint)mode);
    }

    public void Execute()
    {
        if(resultRT == null)
        {
            return;
        }

        RenderTexture[] faces = new RenderTexture[12];
        for (int i = 0; i < faces.Length; i++)
        {
            Camera cam = helperCameras[i];
            // Use temporary render textures so we can use many cameras
            faces[i] = RenderTexture.GetTemporary(_desc);

            cam.targetTexture = faces[i];
            cam.Render();
            _stereoCompute.SetTexture(_kernelIndex, Shader.PropertyToID(faceNames[i]), faces[i]);
            if (mode == Mode.Mono && i == 5)
            {
                for (int j = 6; j < 12; j++)
                {
                    _stereoCompute.SetTexture(_kernelIndex, Shader.PropertyToID(faceNames[j]), Texture2D.blackTexture);
                }
                break;
            }
        }

        _stereoCompute.Dispatch(_kernelIndex, width / 8, _height / 8, 1);
        for (int i = 0; i < faces.Length; i++)
        {
            helperCameras[i].targetTexture = null;
            RenderTexture.ReleaseTemporary(faces[i]);
            faces[i] = null;
            if (mode == Mode.Mono && i == 5)
            {
                break;
            }
        }
    }

    private void View360Result(ScriptableRenderContext context, HDCamera cam)
    {
        var rt = cam.camera.targetTexture;
        var rtid = rt != null ?
            new RenderTargetIdentifier(rt) :
            new RenderTargetIdentifier(BuiltinRenderTextureType.CameraTarget);

        var cmd = CommandBufferPool.Get("360 Stereo Preview");
        cmd.Blit(resultRT, rtid);
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    private void OnEnable()
    {
        if (targetCamera == null)
        {
            return;
        }
        Startup();
    }

    public void Startup()
    {
        var data = targetCamera.GetComponent<HDAdditionalCameraData>();
        if (data != null)
        {
            data.customRender += View360Result;
        }

        foreach (var cam in helperCameras)
        {
            cam.TryGetComponent(out HDAdditionalCameraData hdData);
            data.CopyTo(hdData);
            if (hdData != null)
            {
                hdData.hasPersistentHistory = true;
            }

            var pos = cam.transform.localPosition;
            var rot = cam.transform.localRotation;
            var sca = cam.transform.localScale;
            
            cam.CopyFrom(targetCamera);
            cam.transform.localScale = sca;
            cam.transform.localPosition = pos;
            cam.transform.localRotation = rot;
            cam.usePhysicalProperties = false;
            cam.fieldOfView = 90;
            cam.targetDisplay = 4;
        }


        _stereoCompute = Instantiate(Resources.Load<ComputeShader>("StereoEquirectangular"));
        _kernelIndex = _stereoCompute.FindKernel("CSMain");
        _desc = new RenderTextureDescriptor(resolution, resolution, RenderTextureFormat.ARGBFloat, 0, 0)
        {
            useMipMap = false,
            autoGenerateMips = false,
        };
        _height = mode == Mode.Stereo ? width : width / 2;
        resultRT = RTHandles.Alloc(
            width,
            _height, 
            colorFormat: GraphicsFormat.R32G32B32A32_SFloat, 
            filterMode: FilterMode.Trilinear, 
            wrapMode: TextureWrapMode.Clamp, 
            autoGenerateMips: false, 
            enableRandomWrite: true,
            name: $"OfflineRender Equirectangular ({targetCamera.name})");

        _buffer = BuildBuffer();
        _stereoCompute.SetTexture(_kernelIndex, Shader.PropertyToID("result"), resultRT);
        _stereoCompute.SetInt(Shader.PropertyToID("width"), resultRT.rt.width);
        _stereoCompute.SetInt(Shader.PropertyToID("height"), resultRT.rt.height);
        _stereoCompute.SetBuffer(_kernelIndex, Shader.PropertyToID("lut"), _buffer);
    }

    private void OnDisable()
    {
        if (targetCamera != null)
        {
            var data = targetCamera.GetComponent<HDAdditionalCameraData>();
            if (data != null)
            {
                data.customRender -= View360Result;
            }
        }
        Cleanup();
    }

    private void OnDestroy()
    {
        Cleanup();
    }

    private void OnApplicationQuit()
    {
        Cleanup();
        GC.Collect();
    }

    private void Cleanup()
    {
        if(resultRT != null)
        {
            resultRT.Release();
            resultRT = null;
        }
        if (_buffer != null)
        {
            _buffer.Release();
            _buffer = null;
        }
        if (_stereoCompute != null)
        {
            Destroy(_stereoCompute);
            _stereoCompute = null;
        }
        for (int i = 0; i < helperCameras.Count; i++)
        {

        }
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        if (targetCamera)
        {
            transform.position = targetCamera.transform.position;
            transform.rotation = targetCamera.transform.rotation;
        }
        if (mode == Mode.Mono)
        {
            rightEye.transform.position = Vector3.zero;
            leftEye.SetActive(false);
        }
        else
        {
            leftEye.SetActive(true);
            var target = transform.position + transform.forward * focalDistance;
            leftEye.transform.position = transform.position + (-transform.right * (interpupillaryDistance / 2.0f));
            rightEye.transform.position = transform.position + (transform.right * (interpupillaryDistance / 2.0f));
            // leftEye.transform.LookAt(target, transform.up);
            // rightEye.transform.LookAt(target, transform.up);
        }
        if(autoUpdate)
            Execute();
    }
}
