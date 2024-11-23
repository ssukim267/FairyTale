using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
[ExecuteInEditMode]
#endif
[DefaultExecutionOrder(-99)]
public class SRS_DataTransfer : MonoBehaviour
{
#if USING_URP
    private Dictionary<Camera, RTHandle> rtHandles;
    private List<Camera> tempCams = new List<Camera>();
    private List<RTHandle> tempHandles = new List<RTHandle>();

    private void Awake()
    {
        ResetTempLists();
    }
#endif
    private void OnEnable()
    {
#if USING_URP
        RenderPipelineManager.beginCameraRendering += OnCamPreRender;
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += OnPlaymodeStateChanged;
#endif
#else
        Camera.onPreRender += OnCamPreRender;
#endif
    }

    private void OnDisable()
    {
#if USING_URP
        RenderPipelineManager.beginCameraRendering -= OnCamPreRender;
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged -= OnPlaymodeStateChanged;
#endif
#else
        Camera.onPreRender -= OnCamPreRender;
#endif
    }

#if USING_URP
    private void OnPlaymodeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
            UpdateCameraRTHandlePairs();
        else
            ResetTempLists();
    }

    private void ResetTempLists()
    {
        tempCams = new List<Camera>();
        tempHandles = new List<RTHandle>();
    }

    public void UpdateCameraRTHandlePairs()
    {
        SRS_RenderDepthWithReplacement.rtHandles = new Dictionary<Camera, RTHandle>();
        rtHandles = new Dictionary<Camera, RTHandle>();

        for (int i = 0; i < tempCams.Count; i++)
        {
            rtHandles.Add(tempCams[i], tempHandles[i]);
        }

        SRS_RenderDepthWithReplacement.rtHandles = rtHandles;
    }

    public void AddCameraRTHandlePair(Camera cam, RTHandle rtHandle)
    {
        if (tempCams.Count != tempHandles.Count) ResetTempLists(); //reset lists if their counts are different

        if (!tempCams.Contains(cam))
        {
            tempCams.Add(cam);
            tempHandles.Add(rtHandle);
        }
    }

    private void OnCamPreRender(ScriptableRenderContext context, Camera cam)
#else
    private void OnCamPreRender(Camera cam)
#endif
    {
        if ((Application.isPlaying && cam.tag == "MainCamera") || !Application.isPlaying)
        {
            Shader.SetGlobalVector("_viewCamUp", cam.transform.up);
            Shader.SetGlobalVector("_viewCamRight", cam.transform.right);
        }
    }
}
