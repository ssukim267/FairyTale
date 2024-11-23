namespace NOT_Lonely.Weatherade
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Rendering;

#if UNITY_EDITOR
    using UnityEditor;
    using UnityEngine.UI;

    [ExecuteInEditMode]
#endif

    public class RainCoverage : CoverageBase
    {
        public enum DebugOption
        {
            AreaMask,
            Puddles,
            DripsAndSpots
        }

#pragma warning disable 0414
        [SerializeField] private bool textureMasksFoldout = true;
        [SerializeField] private bool wetnessFoldout = true;
        [SerializeField] private bool puddlesFoldout = true;
        [SerializeField] private bool ripplesAndSpotsFoldout = true;
        [SerializeField] private bool dripsFoldout = true;
        [SerializeField] private DebugOption debugOption = DebugOption.AreaMask;
#pragma warning restore 0414

        [SerializeField] private Texture2D primaryMasks;
        [SerializeField] private Texture2DArray ripplesTex;
        [SerializeField] private Color wetColor = new Color(0.25f, 0.3f, 0.45f);
        [Range(0, 1)] public float wetnessAmount = 0.3f;
        [Range(0, 1)] public float puddlesAmount = 1;
        [SerializeField, Range(0, 1)] private float puddlesMult = 0.8f;
        [SerializeField] private Vector2 puddlesRange = new Vector2(0.5f, 1);
        [SerializeField] private float puddlesTiling = 0.5f;
        [SerializeField, Range(0, 1)] private float puddlesSlope = 0.005f;
        [SerializeField] private bool ripples = true;
        [SerializeField, Range(0, 15)] public int ripplesAmount = 4;
        [SerializeField] private float ripplesIntensity = 0.5f;
        [SerializeField] private float ripplesTiling = 0.5f;
        [SerializeField, Range(0, 120)] private float ripplesFPS = 30;
        [SerializeField, Range(0, 5)] private float spotsIntensity = 5;
        [SerializeField, Range(0, 1)] private float spotsAmount = 0.99f;
        [SerializeField] private bool drips = true;
        [SerializeField, Range(0, 5)] private float dripsIntensity = 1;
        [SerializeField] private float dripsSpeed = 0.2f;
        [SerializeField] private Vector2 dripsTiling = new Vector2(2, 1);
        [SerializeField] private float distortionTiling = 2;
        [SerializeField] private float distortionAmount = 0.002f;

#if UNITY_EDITOR
        [MenuItem("GameObject/NOT_Lonely/Weatherade/Rain Coverage Instance", false, 10)]
        public static void CreateNewSnowCoverageInstance()
        {
            if (NL_Utilities.FindObjectOfType<CoverageBase>(true))
            {
                Debug.LogWarning("Only one instance of 'Weatherade Coverage' is allowed.");
                return;
            }
            RainCoverage rainCoverageInstance = new GameObject("SRS_RainCoverageInstance", typeof(RainCoverage)).GetComponent<RainCoverage>();
            Selection.activeObject = rainCoverageInstance;

            Texture2D defaultPrimaryMasks = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/NOT_Lonely/Weatherade SRS/Textures/RainMasks.tif");
            Texture2DArray defaultRipplesTex = AssetDatabase.LoadAssetAtPath<Texture2DArray>("Assets/NOT_Lonely/Weatherade SRS/Textures/RainRipplesArray.asset");
            rainCoverageInstance.primaryMasks = defaultPrimaryMasks;
            rainCoverageInstance.ripplesTex = defaultRipplesTex;

            rainCoverageInstance.UpdateCoverageMaterials();
        }
#endif

        public override void OnEnable()
        {
            base.OnEnable();
            Shader.DisableKeyword("SRS_SNOW_ON");

#if UNITY_EDITOR
            //SwitchCustomDeferredShading(false);
#endif
        }

#if UNITY_EDITOR
        public override void ValidateValues()
        {
            puddlesTiling = Mathf.Max(0, puddlesTiling);
            ripplesFPS = Mathf.Max(0, Mathf.CeilToInt(ripplesFPS));
            ripplesTiling = Mathf.Max(0, ripplesTiling);
            ripplesIntensity = Mathf.Max(0, ripplesIntensity);
            distortionAmount = Mathf.Max(0, distortionAmount);
            distortionTiling = Mathf.Max(0, distortionTiling);
            blendByNormalsPower = Mathf.Clamp01(blendByNormalsPower);
            base.ValidateValues();
        }
#endif

        public override void GetCoverageShaders()
        {
            meshCoverageShaders = new Shader[1];
            meshCoverageShaders[0] = Shader.Find("NOT_Lonely/Weatherade/Rain Coverage");

            terrainCoverageShaders = new Shader[1];
            terrainCoverageShaders[0] = Shader.Find("NOT_Lonely/Weatherade/Rain Coverage (Terrain)");
        }

        public static void UpdateMtl(Material material)
        {
            if (instance != null) instance.UpdateCoverageMaterial(material);
        }

        public override void UpdateCoverageMaterial(Material material)
        {
            base.UpdateCoverageMaterial(material);

            if (material.HasFloat("_RipplesOverride") && material.GetFloat("_RipplesOverride") == 0)
            {
                material.SetFloat("_Ripples", ripples ? 1 : 0);
                material.SetKeyword(new LocalKeyword(material.shader, "_RIPPLES_ON"), ripples);
            }

            if (material.HasFloat("_DripsOverride") && material.GetFloat("_DripsOverride") == 0)
            {
                material.SetFloat("_Drips", drips ? 1 : 0);
                material.SetKeyword(new LocalKeyword(material.shader, "_DRIPS_ON"), drips);
            }

            Shader.SetGlobalTexture("_PrimaryMasks", primaryMasks);
            Shader.SetGlobalTexture("_RipplesTex", ripplesTex);
            if (ripplesTex != null) Shader.SetGlobalFloat("_RipplesFramesCount", ripplesTex.depth);
            //SetTexture(material, "_PrimaryMasks", primaryMasks);
            //SetTextureArray(material, "_RipplesTex", ripplesTex);
            //if (ripplesTex != null) SetFloat(material, "_RipplesFramesCount", ripplesTex.depth);
            SetColor(material, "_WetColor", wetColor);
            SetFloat(material, "_WetnessAmount", wetnessAmount);
            SetFloat(material, "_PuddlesAmount", puddlesAmount);
            SetFloat(material, "_PuddlesMult", puddlesMult);
            SetVector(material, "_PuddlesRange", puddlesRange);
            SetFloat(material, "_PuddlesTiling", puddlesTiling);
            SetFloat(material, "_PuddlesSlope", puddlesSlope);
            
            SetFloat(material, "_RipplesFPS", ripplesFPS);
            SetFloat(material, "_RipplesTiling", ripplesTiling);
            SetFloat(material, "_RipplesIntensity", ripplesIntensity);
            SetFloat(material, "_RipplesAmount", ripplesAmount);
            SetFloat(material, "_SpotsIntensity", spotsIntensity);
            SetFloat(material, "_SpotsAmount", spotsAmount);
            SetFloat(material, "_DripsIntensity", dripsIntensity);
            SetFloat(material, "_DripsSpeed", dripsSpeed);
            SetVector(material, "_DripsTiling", dripsTiling);
            SetFloat(material, "_DistortionAmount", distortionAmount);
            SetFloat(material, "_DistortionTiling", distortionTiling);
        }
    }
}
