namespace NOT_Lonely.Weatherade
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Rendering;
#if USING_URP
    using UnityEngine.Rendering.Universal;
#endif

#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
    public class SRS_TraceMaskGenerator : MonoBehaviour
    {
        [SerializeField] private CoverageBase srs_coverageBase;
        [SerializeField] private bool allowEditInPlaymode = false;
        [SerializeField, Range(16, 4096)] private int maskRes = 1024;
        [SerializeField, Range(-1, 1)] private float depthBias = 0;
        [SerializeField, Range(0, 12)] private int blurKernelSize = 4;
        [SerializeField, Range(0.01f, 5f)] private float normalSpread = 1f;
        [SerializeField] private float sourceMaskBrightness = 3;
        [SerializeField, Range(0, 1)] private float borderIntensity = 0.35f;
        [SerializeField, Range(0, 1)] private float indent = 1;
        [SerializeField] private float decaySpeed = 0.002f;
        [Range(0, 1)][SerializeField] private float areaFalloffHardness = 0.75f;
        [Range(0, 1)]public float noiseIntensity = 0.35f;
        [SerializeField] private float noiseTiling = 1;
        [SerializeField] private Camera traceObjsCam;
        [SerializeField] private Camera traceSurfCam;
        [SerializeField] private LayerMask traceSurfLayermask = -1;
        [SerializeField] private LayerMask traceObjsLayermask = -1;
        [SerializeField] private RenderTexture traceObjsRT;
        [SerializeField] private RenderTexture surfaceDepthRT;
        [SerializeField] private RenderTexture traceMaskRT;
        [SerializeField] private RenderTexture traceTex;
        [SerializeField] private RenderTexture finalTraceMask;
        [SerializeField] private RenderTexture bluredRT;
        [SerializeField] private Material traceMaskCalcMtl;
        [SerializeField] private Material blurMtl;

        private static readonly int uvOffset_id = Shader.PropertyToID("_UVOffset");
        private static readonly int srs_traceSurfCamMatrix_id = Shader.PropertyToID("_SRS_TraceSurfCamMatrix");
        private static readonly int kernelSize_id = Shader.PropertyToID("_kernelSize");
        private static readonly int tex_id = Shader.PropertyToID("_tex");
        private static readonly int srs_traceMask_id = Shader.PropertyToID("_SRS_TraceMask");
        private static readonly int srs_traceMaskBasic_id = Shader.PropertyToID("_SRS_TraceMaskBasic");

        private Vector2 delta;

        [SerializeField] private int lastRes;
        
        public static SRS_TraceMaskGenerator instance;

        private void Awake()
        {
            srs_coverageBase = GetComponent<SnowCoverage>();
            if(srs_coverageBase == null)
            {
                Debug.LogWarning("WEATHERADE TRACES RENDERER must be added on the same object as Snow Coverage Instance is.");
                DestroyImmediate(this);
                return;
            }

            if (instance == null) instance = this;
            else if (instance != this)
            {
                Debug.LogWarning("Only one WEATHERADE TRACES RENDERER is alowed.");
                DestroyImmediate(this);
                return;
            }

            srs_coverageBase.hasTracesComponent = true;
        }

        private void OnEnable()
        {
            srs_coverageBase.onSRSReady += Init;
            srs_coverageBase.onVolumeTeleported += UpdateUVOffset;

#if UNITY_EDITOR
            Undo.undoRedoPerformed += OnUndoRedo;
#endif
        }

#if UNITY_EDITOR
        void OnUndoRedo()
        {
            if(Selection.activeGameObject == gameObject) CreateStuff();
        }
#endif

        private void OnDisable()
        {
            if (srs_coverageBase == null) return;

            srs_coverageBase.onSRSReady -= Init;
            srs_coverageBase.onVolumeTeleported -= UpdateUVOffset;

#if UNITY_EDITOR
            Undo.undoRedoPerformed -= OnUndoRedo;
#endif
        }

        private void OnDestroy()
        {
            if(srs_coverageBase != null) srs_coverageBase.hasTracesComponent = false;
        }

        public void ValidateValues()
        {
            decaySpeed = Mathf.Max(0, decaySpeed);
            maskRes = Mathf.ClosestPowerOfTwo(Mathf.Clamp(maskRes, 16, 4096));
            blurKernelSize = Mathf.Clamp(blurKernelSize % 2 == 1 ? blurKernelSize + 1 : blurKernelSize, 0, 128);

            if (lastRes != maskRes) CreateStuff();

            SetVariables();

            lastRes = maskRes;
        }

        private void SetVariables()
        {
            if (traceSurfCam == null) return;

            float volumeDepth = traceSurfCam.farClipPlane - traceSurfCam.nearClipPlane;
            //Primary Mask
            traceMaskCalcMtl.SetFloat("_AreaFalloffHardness", areaFalloffHardness);
            traceMaskCalcMtl.SetTexture("_SRS_SurfaceDepth", surfaceDepthRT);
            traceMaskCalcMtl.SetTexture("_SRS_TraceObjsDepth", traceObjsRT);
            traceMaskCalcMtl.SetTexture("_SRS_LastTraceMask", traceMaskRT);
            traceMaskCalcMtl.SetFloat("_VolumeDepth", volumeDepth);
            traceMaskCalcMtl.SetFloat("_DepthBias", depthBias / volumeDepth);

            //Detail Mask
            traceMaskCalcMtl.SetFloat("_NoiseIntensity", noiseIntensity);
            traceMaskCalcMtl.SetFloat("_NoiseTiling", srs_coverageBase.areaSize * noiseTiling * 2);
            traceMaskCalcMtl.SetFloat("_SourceMaskBrightness", sourceMaskBrightness);
            traceMaskCalcMtl.SetFloat("_BorderIntensity", borderIntensity);
            traceMaskCalcMtl.SetFloat("_Indent", indent);

            //Final composite
            traceMaskCalcMtl.SetFloat("_NormalSpread", (1 / traceSurfCam.orthographicSize) * 0.05f * normalSpread);

            Shader.SetGlobalTexture("_SRS_TraceTex", traceTex);
            traceMaskCalcMtl.SetFloat("_DecaySpeed", decaySpeed);
        }

        private void CreateStuff()
        {
            traceObjsRT = NL_Utilities.UpdateOrCreateRT(traceObjsRT, maskRes, RenderTextureFormat.Depth, nameof(traceObjsRT));
            surfaceDepthRT = NL_Utilities.UpdateOrCreateRT(surfaceDepthRT, maskRes / 2, RenderTextureFormat.Depth, nameof(surfaceDepthRT));
            traceMaskRT = NL_Utilities.UpdateOrCreateRT(traceMaskRT, maskRes, RenderTextureFormat.R16, nameof(traceMaskRT));
            traceTex = NL_Utilities.UpdateOrCreateRT(traceTex, maskRes, RenderTextureFormat.ARGB32, nameof(traceTex));
            finalTraceMask = NL_Utilities.UpdateOrCreateRT(finalTraceMask, maskRes, RenderTextureFormat.R16, nameof(finalTraceMask));
            bluredRT = NL_Utilities.UpdateOrCreateRT(bluredRT, maskRes, RenderTextureFormat.R16, nameof(bluredRT));

            traceMaskCalcMtl = new Material(Shader.Find("Hidden/NOT_Lonely/Weatherade/NL_TraceMaskGen"));
            blurMtl = new Material(Shader.Find("Hidden/NOT_Lonely/NL_GaussianBlur"));

            traceSurfCam = SetupCam(traceSurfCam, "SRS_TraceSurfCam", surfaceDepthRT, traceSurfLayermask);
            traceObjsCam = SetupCam(traceObjsCam, "SRS_TraceObjsCam", traceObjsRT, traceObjsLayermask, traceSurfCam.transform, new Vector3(0, 0, srs_coverageBase.sceneDepthCam.farClipPlane), new Vector3(180, 0, 0));
            traceSurfCam.transform.eulerAngles = Vector3.right * 90;

#if UNITY_6000_0_OR_NEWER && USING_URP //In Unity 6+ URP use a dictionary to store the Camera/RTHandle pair which then passed into the SRS_RenderDepthWithReplacement render feature
            SRS_Manager.InitDataTransfer();
            SRS_Manager.srs_dataTransfer.AddCameraRTHandlePair(traceSurfCam, RTHandles.Alloc(surfaceDepthRT));
            SRS_Manager.srs_dataTransfer.AddCameraRTHandlePair(traceObjsCam, RTHandles.Alloc(traceObjsRT));
            SRS_Manager.srs_dataTransfer.UpdateCameraRTHandlePairs();
#endif
            SetVariables();
            SimOneStep();
        }

        public void Init()
        {
            if (!enabled) return;

            CreateStuff();
            SetVariables();

            if (Application.isPlaying)
            {
                StopAllCoroutines();
                StartCoroutine(UpdateMaskRoutine());
            }
            else
            {
                SimOneStep();
            }
        }

        private Camera SetupCam(Camera cam, string camName, RenderTexture rt, LayerMask layermask, Transform parent = null, Vector3 localPos = new Vector3(), Vector3 localRot = new Vector3())
        {
            //find or create a cam
            if (cam == null)
            {
                GameObject camGO = GameObject.Find(camName);

                if (camGO != null)
                {
                    cam = camGO.GetComponent<Camera>();
                }
                else
                {
                    camGO = new GameObject(camName);
                    cam = camGO.AddComponent<Camera>();
                }
            }

            cam.gameObject.hideFlags = HideFlags.HideInHierarchy;

            //set cam settings
#if USING_URP
            UniversalAdditionalCameraData urpAdditionalCamData;
            urpAdditionalCamData = cam.GetComponent<UniversalAdditionalCameraData>();
            if (urpAdditionalCamData == null)
                urpAdditionalCamData = cam.gameObject.AddComponent<UniversalAdditionalCameraData>();

            urpAdditionalCamData.SetRenderer(srs_coverageBase.srsRendererId);

            urpAdditionalCamData.renderShadows = false;
            urpAdditionalCamData.antialiasing = AntialiasingMode.None;
            urpAdditionalCamData.dithering = false;
#endif

            cam.enabled = false;
            cam.transform.parent = parent;
            cam.transform.localPosition = localPos;
            cam.transform.localEulerAngles = localRot;

            cam.renderingPath = RenderingPath.VertexLit;
            cam.cullingMask = layermask;
            cam.useOcclusionCulling = false;
            cam.allowHDR = false;
            cam.allowMSAA = false;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            cam.depth = -9999;
            cam.orthographic = true;
            cam.aspect = 1;
            cam.farClipPlane = srs_coverageBase.sceneDepthCam.farClipPlane;
            cam.orthographicSize = srs_coverageBase.sceneDepthCam.orthographicSize;
            cam.depthTextureMode = DepthTextureMode.Depth;
            
#if !USING_URP
            cam.stereoTargetEye = StereoTargetEyeMask.None;
            cam.SetReplacementShader(Shader.Find("Hidden/NOT_Lonely/Weatherade/DepthRenderer"), "SRSGroupName");
#endif 
            cam.forceIntoRenderTexture = true;
            cam.targetTexture = rt;

            return cam;
        }

        private void UpdateUVOffset(Vector2 d)
        {
            delta = d;
        }

        private void SetUvs()
        {
            float f = 1 / srs_coverageBase.areaSize;
            float u = delta.x * f;
            float v = delta.y * f;

            Vector2 uv = new Vector2(u, v);
            traceMaskCalcMtl.SetVector(uvOffset_id, uv);

            delta = Vector2.zero;
        }

        private IEnumerator UpdateMaskRoutine()
        {
            while (true)
            {
                if(enabled) SimOneStep();
                yield return null;
            }
        }

        public void SimOneStep()
        {
            if (allowEditInPlaymode && Application.isEditor) SetVariables();

            traceSurfCam.transform.position = transform.position - new Vector3(srs_coverageBase.targetPosOffset.x, 0, srs_coverageBase.targetPosOffset.z);
            SetUvs();
            Shader.SetGlobalMatrix(srs_traceSurfCamMatrix_id, GL.GetGPUProjectionMatrix(traceSurfCam.projectionMatrix, true) * traceSurfCam.worldToCameraMatrix);

            traceObjsCam.Render();
            traceSurfCam.Render();

            //Calc basic mask
            RenderTexture tempRT = RenderTexture.GetTemporary(maskRes, maskRes, 0, RenderTextureFormat.R16, RenderTextureReadWrite.Linear);
            Graphics.Blit(surfaceDepthRT, tempRT, traceMaskCalcMtl, 0);
            Graphics.Blit(tempRT, traceMaskRT);
            RenderTexture.ReleaseTemporary(tempRT);
            
            //Blur mask
            tempRT = RenderTexture.GetTemporary(maskRes, maskRes, 0, RenderTextureFormat.R16, RenderTextureReadWrite.Linear);
            blurMtl.SetInt(kernelSize_id, blurKernelSize);
            blurMtl.SetTexture(tex_id, traceMaskRT);
            Graphics.Blit(traceMaskRT, tempRT, blurMtl, 0);
            blurMtl.SetTexture(tex_id, tempRT);
            Graphics.Blit(tempRT, bluredRT, blurMtl, 1);
            RenderTexture.ReleaseTemporary(tempRT);

            //Detial Mask
            traceMaskCalcMtl.SetTexture(srs_traceMask_id, bluredRT);
            Graphics.Blit(bluredRT, finalTraceMask, traceMaskCalcMtl, 1);

            //Calc normals
            tempRT = RenderTexture.GetTemporary(maskRes, maskRes, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            traceMaskCalcMtl.SetTexture(srs_traceMask_id, finalTraceMask);
            traceMaskCalcMtl.SetTexture(srs_traceMaskBasic_id, bluredRT);
            Graphics.Blit(finalTraceMask, tempRT, traceMaskCalcMtl, 2);
            Graphics.Blit(tempRT, traceTex);
            RenderTexture.ReleaseTemporary(tempRT);
        }
    }
}
