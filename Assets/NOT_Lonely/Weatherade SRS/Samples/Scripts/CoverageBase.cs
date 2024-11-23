namespace NOT_Lonely.Weatherade
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Rendering;
#if USING_URP
    using UnityEngine.Rendering.Universal;
#endif

#if UNITY_EDITOR
    using UnityEditor;
    [ExecuteInEditMode]
#endif

    public class CoverageBase : MonoBehaviour
    {
#pragma warning disable 0414
        [SerializeField] private bool depthTexProps = true;
        [SerializeField] private bool surfaceProps = true;
        [SerializeField] private bool areaMaskFoldout = false;
        [SerializeField] private bool distanceFadeFoldout = false;
        [SerializeField] private bool blendByNormalFoldout = false;
#pragma warning restore 0414

        public enum DepthTextureFormat
        {
            RGFloat,
            RGHalf,
            ARGBFloat,
            ARGBHalf
        }

        public float areaSize = 100;
        public float areaDepth = 50;
        public LayerMask depthLayerMask = -1; 
        public DepthTextureFormat depthTextureFormat = DepthTextureFormat.RGFloat;
        [SerializeField, Range(16, 4096)] private int depthTextureResolution = 1024;
        [SerializeField, Range(0, 64)] private int blurKernelSize = 20;
        [Range(0, 1)][SerializeField] private float coverageAreaFalloffHardness = 0.75f;
        public bool useFollowTarget = true;
        public Transform followTarget;
        [SerializeField] private float targetPositionOffsetY = 25;
        [SerializeField, Range(0, 1)] private float updateRate = 1;
        [SerializeField, Range(0, 1)] private float updateDistanceThreshold = 0.3f;

        [SerializeField] private float distanceFadeStart = 1000;
        [SerializeField] private float distanceFadeFalloff = 50;
        [SerializeField] private bool coverage = true;
        [SerializeField] private bool paintableCoverage;
        [SerializeField] private bool stochastic;

        //[SerializeField] private Vector2 coverageAreaMaskRange = new Vector2(0.75f, 1);
        [SerializeField, Range(0, 1)] private float coverageAreaMaskRange = 1;
        [Range(0.001f, 0.3f)][SerializeField] private float coverageAreaBias = 0.025f;
        [Range(0, 0.99f), SerializeField] private float coverageLeakReduction = 0;
        [SerializeField, Range(-1, 2)] private float precipitationDirOffset = 0;
        [SerializeField] private Vector2 precipitationDirRange = new Vector2(0, 1);
        [SerializeField] private float blendByNormalsStrength = 5;
        public float blendByNormalsPower = 1;

        public Vector3 targetPosOffset;
        public Camera sceneDepthCam { get; private set; }
#if USING_URP
        [SerializeField] private UniversalAdditionalCameraData urpAdditionalCamData;
#endif
        [SerializeField] private Material texPackMtl;
        [SerializeField] private Material blurMtl;
        private float vsmExp = 5;

        [SerializeField] public RenderTexture sceneDepthTex { get; private set; }
        [SerializeField] private RenderTexture texBlured;
        [SerializeField] private RenderTexture texRGBA;

        [SerializeField] private List<Material> materials;
        private bool pendingUpdate = false;
        private float tickCounter;
        public bool hasTracesComponent = false;
        
#if !USING_URP && !USING_HDRP
    #if UNITY_EDITOR
            public static GlobalKeyword sparkleKeyword;
            public static GlobalKeyword sssKeyword;
            public static GlobalKeyword sparkleTexLSKeyword;
            public static GlobalKeyword sparkleTexSSKeyword;
            public static GlobalKeyword srsSnowKeyword;
    #endif
#else
        public int srsRendererId;
        public Dictionary<Camera, RTHandle> rtHandlesDictionary;
#endif
        private WaitForSeconds posCheckRateWait = new WaitForSeconds(0.5f);

        public Shader[] meshCoverageShaders;
        public Shader[] terrainCoverageShaders;

        public static CoverageBase instance;

        public delegate void OnSRSReady();
        public OnSRSReady onSRSReady;

        public delegate void OnVolumeTeleported(Vector2 posDelta);
        public OnVolumeTeleported onVolumeTeleported;

        private string consoleMsgAccentColor;
        private string consoleMsgRegularColor;
        private Texture2DArray blueNoiseTex;
        private Camera sceneViewCam;
        
        public virtual void OnEnable()
        {
            DefineConsoleMsgColors();

            if (instance == null) instance = this;
            else if (instance != this)
            {
                DestroyImmediate(gameObject);
            }

            blueNoiseTex = (Texture2DArray)Resources.Load("BlueNoise16x16x32");

#if UNITY_EDITOR
#if USING_URP
            if(!Application.isPlaying) 
                srsRendererId = URP_RendererSetter.SetWeatheradeRenderer(AssetDatabase.LoadAssetAtPath<ScriptableRendererData>("Assets/NOT_Lonely/Weatherade SRS/Resources/SRS_DepthRenderer.asset"));
#endif
            EditorApplication.update += EditorUpdate;
            EditorApplication.playModeStateChanged += OnPlaymodeChanged;
            Selection.selectionChanged += OnSelectionChanged;
#if !USING_URP && !USING_HDRP
            sparkleKeyword = GlobalKeyword.Create("SPARKLE_ON");
            sssKeyword = GlobalKeyword.Create("SSS_ON");
            sparkleTexLSKeyword = GlobalKeyword.Create("SPARKLE_TEX_LS");
            sparkleTexSSKeyword = GlobalKeyword.Create("SPARKLE_TEX_SS");
            srsSnowKeyword = GlobalKeyword.Create("SRS_SNOW_ON");
#endif
#endif
            GetCoverageShaders();
            GetCoverageMaterials();
            Init();
            UpdateCoverageMaterials();
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.update -= EditorUpdate;
            EditorApplication.playModeStateChanged -= OnPlaymodeChanged;
            Selection.selectionChanged -= OnSelectionChanged;
#endif
        }

        private void DefineConsoleMsgColors()
        {
#if UNITY_EDITOR
            if (EditorGUIUtility.isProSkin)
            {
                consoleMsgAccentColor = "<color=#40b8ff>";
                consoleMsgRegularColor = "<color=white>";
            }
            else
            {
                consoleMsgAccentColor = "<color=#006381>";
                consoleMsgRegularColor = "<color=black>";
            }
#else
            consoleMsgAccentColor = "<color=white>";
            consoleMsgRegularColor = "<color=white>";
#endif
        }


#if UNITY_EDITOR
#if !USING_URP && !USING_HDRP
        public void SwitchCustomDeferredShading(bool state)
        {
            SerializedObject graphicsManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);
            SerializedProperty m_Mode = graphicsManager.FindProperty("m_Deferred" + ".m_Mode");
            SerializedProperty m_Shader = graphicsManager.FindProperty("m_Deferred" + ".m_Shader");

            if (state)
            {
                m_Mode.intValue = 2;
                m_Shader.objectReferenceValue = Shader.Find("Hidden/NOT_Lonely/Weatherade/DeferredShading");
            }
            else
            {
                m_Mode.intValue = 1;
            }
            graphicsManager.ApplyModifiedProperties();

        }
#endif

        private void OnPlaymodeChanged(PlayModeStateChange state)
        {
            //if (state == PlayModeStateChange.EnteredEditMode) Init();
        }

        private void EditorUpdate()
        {
            if (!Application.isPlaying && useFollowTarget && SceneView.lastActiveSceneView != null)
            {
                sceneViewCam = SceneView.lastActiveSceneView.camera;
                if (sceneViewCam != null)
                {
                    if (Vector3.Distance(sceneViewCam.transform.position + targetPosOffset, transform.position) > Mathf.Lerp(0, areaSize / 2, updateDistanceThreshold))
                    {
                        UpdateSRSPosition();
                    }
                }
            }

            if (pendingUpdate)
            {
                tickCounter++;

                if (tickCounter > 0)
                {
                    pendingUpdate = false;
                    tickCounter = 0;

                    GetDepth();
                    onSRSReady?.Invoke();
                }
            }
        }

        private void OnSelectionChanged()
        {
            if (Selection.activeGameObject == gameObject) GetCoverageMaterials();
        }

        public virtual void ValidateValues()
        {
            areaSize = Mathf.Max(0, areaSize);
            areaDepth = Mathf.Max(0, areaDepth);
            blendByNormalsStrength = Mathf.Max(0, blendByNormalsStrength);
            blendByNormalsPower = Mathf.Max(0, blendByNormalsPower);
            depthTextureResolution = Mathf.ClosestPowerOfTwo(Mathf.Clamp(depthTextureResolution, 16, 4096));
            blurKernelSize = Mathf.Clamp(blurKernelSize % 2 == 1 ? blurKernelSize + 1 : blurKernelSize, 0, 128);
            updateRate = (float)Math.Round(updateRate, 2);
            coverageAreaMaskRange = Mathf.Clamp((float)Math.Round(coverageAreaMaskRange, 3), 0, 1);
            precipitationDirRange.x = Mathf.Clamp((float)Math.Round(precipitationDirRange.x, 3), 0, 1);
            precipitationDirRange.y = Mathf.Clamp((float)Math.Round(precipitationDirRange.y, 3), 0, 1);
            distanceFadeStart = Mathf.Max(0, distanceFadeStart);
            distanceFadeFalloff = Mathf.Max(0, distanceFadeFalloff);
            UpdateCoverageMaterials();
        }

#endif

        private void GetCoverageMaterials()
        {
            int mtlCount = 0;
            materials = new List<Material>();
            Renderer[] allRenderers = NL_Utilities.FindObjectsOfType<MeshRenderer>(true);

            for (int i = 0; i < allRenderers.Length; i++)
            {
                for (int m = 0; m < allRenderers[i].sharedMaterials.Length; m++)
                {
                    Material curMtl = allRenderers[i].sharedMaterials[m];
                    if (curMtl == null)
                    {
                        Debug.LogWarning($"<b>{consoleMsgAccentColor}{instance}:</color></b> {consoleMsgRegularColor}GameObject '{allRenderers[i].name}' has missing materials and will be ignored.</color>");
                        continue;
                    }

                    if (curMtl.shader == Shader.Find("Hidden/InternalErrorShader"))
                    {
                        Debug.LogWarning($"<b>{consoleMsgAccentColor}{instance}:</color></b> {consoleMsgRegularColor}GameObject '{allRenderers[i].name}' has a material ({curMtl.name}) with a missing shader. This material will be ignored.</color>");
                        continue;
                    }
                    mtlCount++;

                    for (int s = 0; s < meshCoverageShaders.Length; s++)
                    {
                        if (curMtl.shader == meshCoverageShaders[s] && !materials.Contains(curMtl)) materials.Add(curMtl);
                    }
                }
            }

            Terrain[] allTerrains = NL_Utilities.FindObjectsOfType<Terrain>(true);

            for (int i = 0; i < allTerrains.Length; i++)
            {
                Material curMtl = allTerrains[i].materialTemplate;
                if (curMtl == null)
                {
                    Debug.LogWarning($"<b>{consoleMsgAccentColor}{instance}:</color></b> {consoleMsgRegularColor}Terrain '{allTerrains[i].name}' has missing materials and will be ignored.</color>");
                    continue;
                }

                if (curMtl.shader == Shader.Find("Hidden/InternalErrorShader"))
                {
                    Debug.LogWarning($"<b>{consoleMsgAccentColor}{instance}:</color></b> {consoleMsgRegularColor}Terrain '{allTerrains[i].name}' has a material ({curMtl.name}) with a missing shader. This material will be ignored.</color>");
                    continue;
                }

                mtlCount++;

                for (int s = 0; s < terrainCoverageShaders.Length; s++)
                {
                    if (curMtl.shader == terrainCoverageShaders[s] && !materials.Contains(curMtl))
                    {
                        materials.Add(curMtl);
                    }
                }
            }
        }

        public virtual void GetCoverageShaders()
        {
            throw new NotImplementedException();
        }

        private IEnumerator PositionUpdate()
        {
            posCheckRateWait = new WaitForSeconds(updateRate);
            while (true)
            {
                if (Application.isPlaying)
                {
                    if (followTarget != null)
                    {
                        if (Vector3.Distance(followTarget.position + targetPosOffset, transform.position) > Mathf.Lerp(0, areaSize / 2, updateDistanceThreshold))
                        {
                            UpdateSRSPosition();
                        }
                    }
                }

                if (updateRate > 0.02f) yield return posCheckRateWait;
                else yield return null;
            }
        }

        public void UpdateSRSPosition()
        {
            Vector3 followPos;
#if UNITY_EDITOR

            if (Application.isPlaying)
            {
                followPos = followTarget.position;
            }
            else if (sceneViewCam != null)
            {
                followPos = sceneViewCam.transform.position;
            }
            else
            {
                followPos = followTarget.position;
            }

            if (!Application.isPlaying) CalculateTargetPosOffsetsXZ();
#else
            followPos = followTarget.position;
#endif
            //Align cam pos to texture texels grid. TODO: add support for tilt camera
            float x = followPos.x + targetPosOffset.x;
            float z = followPos.z + targetPosOffset.z;

            float texelSize = ((float)1 / (float)depthTextureResolution) * areaSize;

            x /= texelSize;
            z /= texelSize;

            int xSnap = (int)x;
            int zSnap = (int)z;

            float posX = xSnap * texelSize;
            float posZ = zSnap * texelSize;

            Vector3 snappedPos = new Vector3(posX, followPos.y + targetPosOffset.y, posZ);
            //

            Vector2 pos2D = new Vector2(transform.position.x, transform.position.z);
            transform.position = snappedPos;
            Vector2 newPos2D = new Vector2(transform.position.x, transform.position.z);


            if (Application.isPlaying)
            {
                UpdateDepthCam();
                GetDepth();
            }
            else
            {
                UpdateDepthCam();
                pendingUpdate = true;
            }

            onVolumeTeleported?.Invoke(newPos2D - pos2D);
        }

        public void Init()
        {
            if (instance != this) return;

            if (Application.isPlaying)
            {
                if (useFollowTarget)
                {
                    if (followTarget == null && Camera.main != null)
                        followTarget = Camera.main.transform; // try use the main camera as a follow target if it's not set manually

                    if (followTarget != null)
                    {
                        CalculateTargetPosOffsetsXZ();
                        transform.position = followTarget.position + targetPosOffset;
                        //StartCoroutine(PositionUpdate());
                    }
                    else Debug.LogWarning("Can't find a camera with a 'MainCamera' tag. The Follow Target feature will be disabled.");
                }
            }

            InitTexturesAndCam();
            UpdateDepth();

            if (this != null && useFollowTarget && followTarget != null) StartCoroutine(PositionUpdate());
        }

        public void CalculateTargetPosOffsetsXZ()
        {
            float xOffset = targetPositionOffsetY * (Mathf.Tan((transform.localEulerAngles.z) * Mathf.Deg2Rad));
            float zOffset = targetPositionOffsetY * (Mathf.Tan((transform.localEulerAngles.x) * Mathf.Deg2Rad));
            targetPosOffset = new Vector3(-xOffset, targetPositionOffsetY, zOffset);
        }

        public void UpdateDepth()
        {
            if (Application.isPlaying)
            {
                StopAllCoroutines();
                StartCoroutine(UpdateDepthRoutine());
            }
            else
            {
                UpdateDepthCam();
                pendingUpdate = true;
            }
        }

        IEnumerator UpdateDepthRoutine()
        {
            UpdateDepthCam();

            yield return new WaitForEndOfFrame();

            GetDepth();
            onSRSReady?.Invoke();
        }

        public void InitTexturesAndCam()
        {
            texPackMtl = new Material(Shader.Find("Hidden/NOT_Lonely/Weatherade/NL_TexturePacking"));
            blurMtl = new Material(Shader.Find("Hidden/NOT_Lonely/NL_GaussianBlur"));

            int depthPrecission = (depthTextureFormat == DepthTextureFormat.RGFloat || depthTextureFormat == DepthTextureFormat.ARGBFloat) ? 32 : 16;
            sceneDepthTex = NL_Utilities.UpdateOrCreateRT(sceneDepthTex, depthTextureResolution, RenderTextureFormat.Depth, nameof(sceneDepthTex), depthPrecission);

            if (depthTextureFormat == DepthTextureFormat.RGFloat || depthTextureFormat == DepthTextureFormat.ARGBFloat)
            {
                texBlured = new RenderTexture(sceneDepthTex.width, sceneDepthTex.height, 0, RenderTextureFormat.RGFloat, RenderTextureReadWrite.Linear);
                texBlured.wrapMode = TextureWrapMode.Repeat;
            }

            if (depthTextureFormat == DepthTextureFormat.RGHalf || depthTextureFormat == DepthTextureFormat.ARGBHalf)
            {
                texBlured = new RenderTexture(sceneDepthTex.width, sceneDepthTex.height, 0, RenderTextureFormat.RGHalf, RenderTextureReadWrite.Linear);
                texBlured.wrapMode = TextureWrapMode.Repeat;
            }

            if (depthTextureFormat == DepthTextureFormat.ARGBFloat)
            {
                texRGBA = new RenderTexture(sceneDepthTex.width, sceneDepthTex.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
                texRGBA.wrapMode = TextureWrapMode.Repeat;
            }
            if (depthTextureFormat == DepthTextureFormat.ARGBHalf)
            {
                texRGBA = new RenderTexture(sceneDepthTex.width, sceneDepthTex.height, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
                texRGBA.wrapMode = TextureWrapMode.Repeat;
            }

            if (sceneDepthCam == null)
            {
                GameObject camGO = GameObject.Find("SRS_InstanceDepthCam");

                if (camGO != null) sceneDepthCam = camGO.GetComponent<Camera>();
                else
                {
                    camGO = new GameObject("SRS_InstanceDepthCam");
                    sceneDepthCam = camGO.AddComponent<Camera>();
                }

                camGO.hideFlags = HideFlags.HideInHierarchy; 
            }

            //sceneDepthCam.hideFlags = HideFlags.None;

#if USING_URP
    #if UNITY_6000_0_OR_NEWER //In Unity 6+ URP use a dictionary to store the Camera/RTHandle pair and pass it to the SRS_RenderDepthWithReplacement render feature
            SRS_Manager.InitDataTransfer();
            SRS_Manager.srs_dataTransfer.AddCameraRTHandlePair(sceneDepthCam, RTHandles.Alloc(sceneDepthTex));
            SRS_Manager.srs_dataTransfer.UpdateCameraRTHandlePairs();
    #endif
            if (urpAdditionalCamData == null)
                urpAdditionalCamData = sceneDepthCam.GetComponent<UniversalAdditionalCameraData>();
            if (urpAdditionalCamData == null)
                urpAdditionalCamData = sceneDepthCam.gameObject.AddComponent<UniversalAdditionalCameraData>();

            urpAdditionalCamData.SetRenderer(srsRendererId);

            urpAdditionalCamData.renderShadows = false;
            urpAdditionalCamData.antialiasing = AntialiasingMode.None;
            urpAdditionalCamData.dithering = false;
#endif
            sceneDepthCam.enabled = false;

            sceneDepthCam.transform.parent = transform;
            sceneDepthCam.transform.localPosition = Vector3.zero;
            sceneDepthCam.transform.localEulerAngles = Vector3.right * 90;

            sceneDepthCam.renderingPath = RenderingPath.Forward;
            sceneDepthCam.cullingMask = depthLayerMask;
            sceneDepthCam.useOcclusionCulling = false;
            sceneDepthCam.allowHDR = false;
            sceneDepthCam.allowMSAA = false;
            sceneDepthCam.clearFlags = CameraClearFlags.Color;
            sceneDepthCam.backgroundColor = Color.black;
            sceneDepthCam.depth = -10000;
            sceneDepthCam.orthographic = true;
            sceneDepthCam.aspect = 1;
            sceneDepthCam.depthTextureMode = DepthTextureMode.Depth;
            sceneDepthCam.forceIntoRenderTexture = true;
            sceneDepthCam.targetTexture = sceneDepthTex;
            
#if !USING_URP
                sceneDepthCam.stereoTargetEye = StereoTargetEyeMask.None;
#endif
            /*
            Shader.SetGlobalFloat("SRS_coverageAmountGlobal", coverageAmount);
            if (coverageTex0 != null) Shader.SetGlobalTexture("_CoverageTex0", coverageTex0);
            */
#if !USING_URP && !USING_HDRP
            sceneDepthCam.SetReplacementShader(Shader.Find("Hidden/NOT_Lonely/Weatherade/DepthRenderer"), "SRSGroupName");
#endif
        }

        private void UpdateDepthCam()
        {
            if (sceneDepthCam == null) return;

            sceneDepthCam.orthographicSize = areaSize / 2;
            sceneDepthCam.farClipPlane = areaDepth;
            Shader.SetGlobalVector("_depthCamDir", sceneDepthCam.transform.forward);
            Shader.SetGlobalMatrix("_depthCamMatrix", GL.GetGPUProjectionMatrix(sceneDepthCam.projectionMatrix, true) * sceneDepthCam.worldToCameraMatrix);
        }

        private void GetDepth()
        {
            sceneDepthCam.Render();  
            ConvertDepthToVSM();

            if (depthTextureFormat == DepthTextureFormat.RGFloat || depthTextureFormat == DepthTextureFormat.RGHalf)
            {
                Shader.SetGlobalTexture("_SRS_depth", texBlured);
            }
            else
            {
                ConvertToRGBA();
                Shader.SetGlobalTexture("_SRS_depth", texRGBA);

                if ((Application.isPlaying && followTarget == null) || (!Application.isPlaying && sceneViewCam == null))
                {
                    if(texBlured != null) texBlured.Release();
                }
            }

            if ((Application.isPlaying && followTarget == null) || (!Application.isPlaying && sceneViewCam == null))
            {
                sceneDepthTex.Release();
            }
        }

        private void ConvertDepthToVSM()
        {
            RenderTexture tempRT01 = RenderTexture.GetTemporary(texBlured.width, texBlured.height, 0, texBlured.format, RenderTextureReadWrite.Linear);
            texPackMtl.SetTexture("_DepthTex", sceneDepthTex);

#if USING_URP
            vsmExp = depthTextureFormat == DepthTextureFormat.ARGBFloat || depthTextureFormat == DepthTextureFormat.RGFloat ? 12 : 2;
#else
            vsmExp = depthTextureFormat == DepthTextureFormat.ARGBFloat || depthTextureFormat == DepthTextureFormat.RGFloat ? 15 : 5;
#endif
            Shader.SetGlobalFloat("_VsmExp", vsmExp);

            Graphics.Blit(sceneDepthTex, tempRT01, texPackMtl, 0);

            RenderTexture tempRT02 = RenderTexture.GetTemporary(texBlured.width, texBlured.height, 0, texBlured.format, RenderTextureReadWrite.Linear);
            blurMtl.SetInt("_kernelSize", blurKernelSize);
            blurMtl.SetTexture("_tex", tempRT01);
            Graphics.Blit(tempRT01, tempRT02, blurMtl, 2);
            blurMtl.SetTexture("_tex", tempRT02);
            Graphics.Blit(tempRT02, texBlured, blurMtl, 3);

            RenderTexture.ReleaseTemporary(tempRT01);
            RenderTexture.ReleaseTemporary(tempRT02);
        }

        private void ConvertToRGBA()
        {
            texPackMtl.SetTexture("_rg", texBlured);
            texPackMtl.SetTexture("_b", sceneDepthTex);
            Graphics.Blit(sceneDepthTex, texRGBA, texPackMtl, 1);
        }

        public void UpdateCoverageMaterials()
        {
            for (int i = 0; i < materials.Count; i++)
            {
                if (materials[i] != null) UpdateCoverageMaterial(materials[i]);
            }
        }

        public virtual void SetFloat(Material mtl, string shaderPropName, float val)
        {
            if (mtl.HasFloat($"{shaderPropName}Override") && mtl.GetFloat($"{shaderPropName}Override") == 0) mtl.SetFloat(shaderPropName, val);
        }

        public virtual void SetVector(Material mtl, string shaderPropName, Vector2 val)
        {
            if (mtl.HasFloat($"{shaderPropName}Override") && mtl.GetFloat($"{shaderPropName}Override") == 0) mtl.SetVector(shaderPropName, val);
        }

        public virtual void SetTexture(Material mtl, string shaderPropName, Texture2D val)
        {
            if (mtl.HasFloat($"{shaderPropName}Override") && mtl.GetFloat($"{shaderPropName}Override") == 0) mtl.SetTexture(shaderPropName, val);
        }

        public virtual void SetTextureArray(Material mtl, string shaderPropName, Texture2DArray val)
        {
            if (mtl.HasFloat($"{shaderPropName}Override") && mtl.GetFloat($"{shaderPropName}Override") == 0) mtl.SetTexture(shaderPropName, val);
        }

        public virtual void SetColor(Material mtl, string shaderPropName, Color val)
        {
            if (mtl.HasFloat($"{shaderPropName}Override") && mtl.GetFloat($"{shaderPropName}Override") == 0) mtl.SetColor(shaderPropName, val);
        }

#if UNITY_EDITOR
        public virtual void SetKeyword(Material mtl, string keywordName, string shaderPropName, bool val)
        {
            if (mtl.HasFloat($"{shaderPropName}Override") && mtl.GetFloat($"{shaderPropName}Override") == 0)
            {
                mtl.SetFloat(shaderPropName, val ? 1 : 0);
                mtl.SetKeyword(new LocalKeyword(mtl.shader, keywordName), val);
            }
        }
#endif

        public virtual void UpdateCoverageMaterial(Material material)
        {
            if (instance == null) return;
            if (material.shader == Shader.Find("Hidden/InternalErrorShader"))
            {
                Debug.LogWarning($"<b>{consoleMsgAccentColor}{instance}:</color></b> {consoleMsgRegularColor}Missing shader for the '{material.name}' material. This material will be ignored.</color>");
                return;
            }

            material.SetFloat("_CoverageAreaFalloffHardness", instance.coverageAreaFalloffHardness);

#if UNITY_EDITOR
            SetKeyword(material, "_COVERAGE_ON", "_Coverage", instance.coverage);
            SetKeyword(material, "_PAINTABLE_COVERAGE_ON", "_PaintableCoverage", instance.paintableCoverage);

            if (material.HasFloat("_StochasticOverride") && material.GetFloat("_StochasticOverride") == 0)
            {
                material.SetFloat("_Stochastic", stochastic ? 1 : 0);
                material.SetKeyword(new LocalKeyword(material.shader, "_STOCHASTIC_ON"), stochastic);
            }
#endif

            Shader.SetGlobalTexture("_BlueNoise", blueNoiseTex);

            SetFloat(material, "_DistanceFadeStart", instance.distanceFadeStart);
            SetFloat(material, "_DistanceFadeFalloff", instance.distanceFadeFalloff);
            SetFloat(material, "_CoverageAreaMaskRange", instance.coverageAreaMaskRange);
            SetFloat(material, "_CoverageAreaBias", instance.coverageAreaBias);
            SetFloat(material, "_CoverageLeakReduction", instance.coverageLeakReduction);
            SetFloat(material, "_PrecipitationDirOffset", instance.precipitationDirOffset);
            SetVector(material, "_PrecipitationDirRange", instance.precipitationDirRange);
            SetFloat(material, "_BlendByNormalsStrength", instance.blendByNormalsStrength);
            SetFloat(material, "_BlendByNormalsPower", instance.blendByNormalsPower);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!enabled || !gameObject.activeInHierarchy) return;

            Vector3 areaGizmoSize = new Vector3(areaSize, areaSize, areaDepth);
            Vector3 areaGizmoCenter = new Vector3(0, 0, areaDepth * 0.5f);
            Gizmos.matrix = sceneDepthCam.transform.localToWorldMatrix;
            Gizmos.color = new Color(0, 0.656f, 0.9f, 1);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(areaSize, areaSize, 0));

            NL_Utilities.DrawArrowGizmo(transform.position, -transform.up * areaDepth, new Color(0, 0.656f, 0.9f, 1), 0, Color.black, areaSize / 10);

            Gizmos.DrawIcon(transform.position, "NOT_Lonely/Weatherade/SRS_CoverageIcon.png", true);
        }

        private void OnDrawGizmosSelected()
        {
            if (!enabled || !gameObject.activeInHierarchy) return;
            if (sceneDepthCam == null) return;

            Vector3 areaGizmoSize = new Vector3(areaSize, areaSize, areaDepth);
            Vector3 areaGizmoCenter = new Vector3(0, 0, areaDepth * 0.5f);

            Gizmos.matrix = sceneDepthCam.transform.localToWorldMatrix;

            Gizmos.color = new Color(0, 0.656f, 0.9f, 1);
            Gizmos.DrawWireCube(areaGizmoCenter, areaGizmoSize);

            Gizmos.color = new Color(0, 0.656f, 0.9f, 0.15f);
            Gizmos.DrawCube(areaGizmoCenter, areaGizmoSize);
        }
#endif
    }
}
