namespace NOT_Lonely.Weatherade
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Rendering;

#if UNITY_EDITOR
    using UnityEditor;
    [ExecuteInEditMode]
#endif

    public class SnowCoverage : CoverageBase
    {
#pragma warning disable 0414
        [SerializeField] private bool basicSettingsFoldout = false;
        [SerializeField] private bool tessellationFoldout = false;
        [SerializeField] private bool displacementFoldout = false;
        [SerializeField] private bool tracesFoldout = false;
        [SerializeField] private bool sparklesFoldout = false;
#pragma warning restore 0414

        public enum SparkleMaskSurce
        {
            MainCoverageTexAlpha,
            SparkleMask
        }

        [SerializeField] private bool traces;
        [SerializeField] private float tracesNormalScale = 0.8f;
        [Range(0, 1), SerializeField] private float tracesBlendFactor = 0.5f;
        [SerializeField, ColorUsage(false)] private Color tracesColor = new Color(0.1f, 0.25f, 0.4f);
        [SerializeField] private Vector2 tracesColorBlendRange = new Vector2(0, 1);
        [SerializeField] private bool traceDetail;
        [SerializeField] private Texture2D traceDetailTex;
        [SerializeField] private float traceDetailTiling = 1;
        [SerializeField] private float traceDetailNormalScale = 1;
        [SerializeField] private float traceDetailIntensity = 0.5f;
        
        //Tessellation
        [SerializeField] private bool tessellation;
        [SerializeField, Range(5, 100)] private float tessEdgeL = 10;
        [SerializeField, Range(0, 1)] private float tessFactorSnow = 0;
        [SerializeField] private float tessMaxDisp = 0.35f;
        [SerializeField] private Vector2 tessSnowdriftRange = new Vector2(0.63f, 0.775f);

        [SerializeField] private Texture2D coverageTex0;
        [SerializeField] private bool useAveragedNormals;
        [Range(0, 1)] public float coverageAmount = 0.75f;
        [SerializeField] private Color coverageColor = Color.white;
        [Range(0, 1)][SerializeField] private float emissionMasking = 0.98f;
        [Range(0, 1)][SerializeField] private float maskByAlpha = 1;
        [SerializeField] private float coverageNormalScale0 = 1;
        [Range(0, 1), SerializeField] private float heightMap0Contrast = 0.2f;
        [Range(0, 1)][SerializeField] private float coverageNormalsOverlay = 0.75f;
        //[Range(0, 1)][SerializeField] private float baseCoverageNormalsBlend = 1;
        [SerializeField] private float coverageTiling = 0.15f;
        
        //Displacement
        [SerializeField] private bool displacement;
        [SerializeField] private float coverageDisplacement = 1f;
        [SerializeField][Range(0, 1)] private float coverageDisplacementOffset = 0.3f;
        
        //Sparkle
        [SerializeField] private bool sparkle = true;
        [SerializeField] private bool sss = true;
        [SerializeField] private float sss_intensity = 1;
        [SerializeField] private Texture2D sparkleTex;
        [Range(0, 1)][SerializeField] private float sparklesAmount = 0.75f;
        [SerializeField] private float sparkleDistFalloff = 50;
        [SerializeField] private float localSparkleTiling = 1;
        [SerializeField] private float screenSpaceSparklesTiling = 2;
        [SerializeField] private SparkleMaskSurce sparkleTexSS = SparkleMaskSurce.SparkleMask;
        [SerializeField] private SparkleMaskSurce sparkleTexLS = SparkleMaskSurce.SparkleMask;
        [SerializeField] private float sparklesBrightness = 20;
        [SerializeField] private float sparklesLightmapMaskPower = 4.5f;
        [Range(0, 1)][SerializeField] private float sparklesHighlightMaskExpansion = 0.95f;

        //[SerializeField, Range(0, 1)] private float snowAOIntensity = 1;

#if UNITY_EDITOR
        [MenuItem("GameObject/NOT_Lonely/Weatherade/Snow Coverage Instance", false, 10)]
        public static void CreateNewSnowCoverageInstance()
        {
            if (NL_Utilities.FindObjectOfType<CoverageBase>(true))
            {
                Debug.LogWarning("Only one instance of 'Weatherade Coverage' is allowed.");
                return;
            }
            SnowCoverage snowCoverageInstance = new GameObject("SRS_SnowCoverageInstance", typeof(SnowCoverage)).GetComponent<SnowCoverage>();
            Selection.activeObject = snowCoverageInstance;

            Texture2D defaultTex0 = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/NOT_Lonely/Weatherade SRS/Textures/Snow_01_n_h_sm.tif");
            Texture2D defaultSparkleTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/NOT_Lonely/Weatherade SRS/Textures/SparkleMask.tif");
            snowCoverageInstance.coverageTex0 = defaultTex0;
            snowCoverageInstance.sparkleTex = defaultSparkleTex;

            snowCoverageInstance.UpdateCoverageMaterials();

        }
#endif

#if UNITY_EDITOR
        public override void ValidateValues()
        {
            sss_intensity = Mathf.Max(0, sss_intensity);
            coverageDisplacement = Mathf.Max(0, coverageDisplacement);
            sparklesBrightness = Mathf.Max(0, sparklesBrightness);
            sparkleDistFalloff = Mathf.Max(0, sparkleDistFalloff);
            screenSpaceSparklesTiling = Mathf.Max(0, screenSpaceSparklesTiling);
            sparklesLightmapMaskPower = Mathf.Max(1, sparklesLightmapMaskPower);
            coverageTiling = Mathf.Max(0.0001f, coverageTiling);
            tessMaxDisp = Mathf.Max(0, tessMaxDisp);

            base.ValidateValues();
        }
#endif
        public override void OnEnable()
        {
            base.OnEnable();
            Shader.EnableKeyword("SRS_SNOW_ON");
        }

        public override void GetCoverageShaders()
        {
            meshCoverageShaders = new Shader[2];
            terrainCoverageShaders = new Shader[2];
            meshCoverageShaders[0] = Shader.Find("NOT_Lonely/Weatherade/Snow Coverage");
            meshCoverageShaders[1] = Shader.Find("Hidden/NOT_Lonely/Weatherade/Snow Coverage (Tessellation)");
            terrainCoverageShaders[0] = Shader.Find("NOT_Lonely/Weatherade/Snow Coverage (Terrain)");
            terrainCoverageShaders[1] = Shader.Find("Hidden/NOT_Lonely/Weatherade/Snow Coverage (Terrain-Tessellation)");
        }

        public static void UpdateMtl(Material material)
        {
            if(instance != null) instance.UpdateCoverageMaterial(material);
        }

        public override void UpdateCoverageMaterial(Material material)
        {
            base.UpdateCoverageMaterial(material);

            //Global variables for the Deferred rendering path
#if UNITY_EDITOR
            Shader.SetKeyword(sparkleKeyword, sparkle);
            Shader.SetKeyword(sssKeyword, sss);
            SwitchCustomDeferredShading(sparkle || sss);
#endif
            if (sparkle)
            {
#if UNITY_EDITOR
                Shader.SetKeyword(sparkleTexLSKeyword, (float)sparkleTexLS > 0);
                Shader.SetKeyword(sparkleTexSSKeyword, (float)sparkleTexSS > 0);
                if (!Shader.IsKeywordEnabled(sparkleTexLSKeyword) || !Shader.IsKeywordEnabled(sparkleTexSSKeyword)) Shader.SetGlobalTexture("_CoverageTex0", coverageTex0);
#endif
                Shader.SetGlobalTexture("_SparkleTex", sparkleTex);
                Shader.SetGlobalFloat("_SparklesAmount", sparklesAmount);
                Shader.SetGlobalFloat("_SparkleDistFalloff", sparkleDistFalloff);
                Shader.SetGlobalFloat("_LocalSparkleTiling", localSparkleTiling);
                Shader.SetGlobalFloat("_ScreenSpaceSparklesTiling", screenSpaceSparklesTiling);
                Shader.SetGlobalFloat("_SparklesBrightness", sparklesBrightness);
                Shader.SetGlobalFloat("_SparklesHighlightMaskExpansion", sparklesHighlightMaskExpansion);
                
            }
            Shader.SetGlobalFloat("_SSS_intensity", sss_intensity);
            //
            Shader.SetGlobalTexture("_TraceDetailTex", traceDetailTex);
#if UNITY_EDITOR
            if (material.HasFloat("_TracesOverride") && material.GetFloat("_TracesOverride") == 0)
            {
                material.SetFloat("_Traces", traces ? 1 : 0);
                material.SetKeyword(new LocalKeyword(material.shader, "_TRACES_ON"), traces);
            }

            if (material.HasFloat("_DisplacementOverride") && material.GetFloat("_DisplacementOverride") == 0)
            {
                material.SetFloat("_Displacement", displacement ? 1 : 0);
                material.SetKeyword(new LocalKeyword(material.shader, "_DISPLACEMENT_ON"), displacement);
            }

            if (material.HasFloat("_UseAveragedNormalsOverride") && material.GetFloat("_UseAveragedNormalsOverride") == 0)
            {
                material.SetFloat("_UseAveragedNormals", useAveragedNormals ? 1 : 0);
                material.SetKeyword(new LocalKeyword(material.shader, "_USE_AVERAGED_NORMALS"), useAveragedNormals);
            }

            if (material.HasFloat("_SparkleOverride") && material.GetFloat("_SparkleOverride") == 0)
            {
                material.SetFloat("_Sparkle", sparkle ? 1 : 0);
                material.SetKeyword(new LocalKeyword(material.shader, "_SPARKLE_ON"), sparkle);
            }
            if (material.HasFloat("_SssOverride") && material.GetFloat("_SssOverride") == 0)
            {
                material.SetFloat("_Sss", sss ? 1 : 0);
                material.SetKeyword(new LocalKeyword(material.shader, "_SSS_ON"), sss);
            }

            if (material.HasFloat("_SparkleTexSSOverride") && material.GetFloat("_SparkleTexSSOverride") == 0)
            {
                material.SetFloat("_SparkleTexSS", (float)sparkleTexSS);
                material.SetKeyword(new LocalKeyword(material.shader, "_SPARKLE_TEX_SS"), (float)sparkleTexSS > 0);
            }
            if (material.HasFloat("_SparkleTexLSOverride") && material.GetFloat("_SparkleTexLSOverride") == 0)
            {
                material.SetFloat("_SparkleTexLS", (float)sparkleTexLS);
                material.SetKeyword(new LocalKeyword(material.shader, "_SPARKLE_TEX_LS"), (float)sparkleTexLS > 0);
            }
#endif

            if (material.HasFloat("_TessellationOverride") && material.GetFloat("_TessellationOverride") == 0)
            {
#if UNITY_EDITOR
                material.SetFloat("_Tessellation", tessellation ? 1 : 0);

                Shader simpleShader = Shader.Find("NOT_Lonely/Weatherade/Snow Coverage");
                Shader tessShader = Shader.Find("Hidden/NOT_Lonely/Weatherade/Snow Coverage (Tessellation)");
                Shader terrainSimpleShader = Shader.Find("NOT_Lonely/Weatherade/Snow Coverage (Terrain)");
                Shader terrainTessShader = Shader.Find("Hidden/NOT_Lonely/Weatherade/Snow Coverage (Terrain-Tessellation)");

                if (material.GetFloat("_Tessellation") == 1)
                {
                    if (material.shader == simpleShader) material.shader = tessShader;
                    if (material.shader == terrainSimpleShader) material.shader = terrainTessShader;
                }
                else
                {
                    if (material.shader == tessShader) material.shader = simpleShader;
                    if (material.shader == terrainTessShader) material.shader = terrainSimpleShader;
                }
#endif
            }

            SetFloat(material, "_TessEdgeL", tessEdgeL);
            SetFloat(material, "_TessFactorSnow", tessFactorSnow);
            SetFloat(material, "_TessMaxDisp", tessMaxDisp);
            SetVector(material, "_TessSnowdriftRange", tessSnowdriftRange);

            if (material.HasFloat("_TraceDetailOverride") && material.GetFloat("_TraceDetailOverride") == 0)
            {
                material.SetFloat("_TraceDetail", traceDetail ? 1 : 0);
                material.SetKeyword(new LocalKeyword(material.shader, "_TRACE_DETAIL"), traceDetail);
            }

            //Traces
            SetFloat(material, "_TracesNormalScale", tracesNormalScale);
            SetFloat(material, "_TracesBlendFactor", tracesBlendFactor);
            SetVector(material, "_TracesColorBlendRange", tracesColorBlendRange);
            SetColor(material, "_TracesColor", tracesColor);
            //SetTexture(material, "_TraceDetailTex", traceDetailTex);
            SetFloat(material, "_TraceDetailTiling", traceDetailTiling);
            SetFloat(material, "_TraceDetailNormalScale", traceDetailNormalScale);
            SetFloat(material, "_TraceDetailIntensity", traceDetailIntensity);

            SetTexture(material, "_CoverageTex0", coverageTex0);
            SetColor(material, "_CoverageColor", coverageColor);
            SetFloat(material, "_CoverageAmount", coverageAmount);
            SetFloat(material, "_EmissionMasking", emissionMasking);
            SetFloat(material, "_MaskCoverageByAlpha", maskByAlpha);
            SetFloat(material, "_CoverageNormalScale0", coverageNormalScale0);
            SetFloat(material, "_HeightMap0Contrast", heightMap0Contrast);
            SetFloat(material, "_CoverageNormalsOverlay", coverageNormalsOverlay);
            //SetFloat(material, "_BaseCoverageNormalsBlend", baseCoverageNormalsBlend);
            SetFloat(material, "_CoverageTiling", coverageTiling);
            SetFloat(material, "_CoverageDisplacement", coverageDisplacement);
            SetFloat(material, "_CoverageDisplacementOffset", coverageDisplacementOffset);
            
            //Sparkle and SSS
            SetFloat(material, "_SSS_intensity", sss_intensity);
            SetFloat(material, "_SparklesAmount", sparklesAmount);
            SetFloat(material, "_SparkleDistFalloff", sparkleDistFalloff);
            SetTexture(material, "_SparkleTex", sparkleTex);
            SetFloat(material, "_LocalSparkleTiling", localSparkleTiling);
            SetFloat(material, "_SparklesBrightness", sparklesBrightness);
            SetFloat(material, "_ScreenSpaceSparklesTiling", screenSpaceSparklesTiling);
            SetFloat(material, "_SparklesHighlightMaskExpansion", sparklesHighlightMaskExpansion);
            SetFloat(material, "_SparklesLightmapMaskPower", sparklesLightmapMaskPower);

            //SetFloat(material, "_SnowAOIntensity", snowAOIntensity);
        }
    }
}
