using UnityEngine;
using HeadlessStudio.OfflineRenderHD.Runtime;

/// <summary>
/// Simple controller script for offline render when running in the UnityEditor
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(OfflineRenderHD))]
public class OfflineRenderHDController : MonoBehaviour
{
    private OfflineRenderHD _offlineRenderHD;

    private void LateUpdate()
    {
        if (!_offlineRenderHD)
            _offlineRenderHD = GetComponent<OfflineRenderHD>();
#if UNITY_EDITOR
        if (_offlineRenderHD.realtimeCapture) return;

        if (_offlineRenderHD && _offlineRenderHD.enabled && _offlineRenderHD.Save)
        {
            var start = _offlineRenderHD.startFrame < 0 ? 0 : _offlineRenderHD.startFrame;
            var end = _offlineRenderHD.endFrame < 0 ? int.MaxValue : _offlineRenderHD.endFrame;
            var l = (_offlineRenderHD.globalFrameNumber - start);
            if (l < 0) l = 0;
            else if (start > 0) l++;
            var progress = 1f * l / (end - start + 1);
            var abort = UnityEditor.EditorUtility.DisplayCancelableProgressBar("Rendering",
                                                                               string.Format(
                                                                                   "Render frame {0} of {1} ({2:###.00}%)",
                                                                                   l,
                                                                                   (end - start + 1), progress * 100f), progress);
            if (_offlineRenderHD.globalFrameNumber > _offlineRenderHD.endFrame || abort)
            {
                UnityEditor.EditorApplication.isPlaying = false;
                UnityEditor.EditorUtility.ClearProgressBar();
            }
        }
#endif
    }
}
