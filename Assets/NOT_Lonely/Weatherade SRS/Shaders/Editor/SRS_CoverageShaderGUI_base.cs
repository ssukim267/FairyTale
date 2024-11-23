using NOT_Lonely.Weatherade;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using static SRS_CoverageShaderGUI_base;

public class SRS_CoverageShaderGUI_base : ShaderGUI
{
    public MaterialEditor m_MaterialEditor;
    public Material material;
    public MaterialProperty[] props;
    public bool m_inspectorInitiated = false;
    public float currentInspectorWidth;

    public class OverridableBase<LocalVal>
    {
        ///<summary> Material property which is used in the shader </summary>
        public MaterialProperty prop;
        ///<summary> Name of the property used in the shader </summary>
        public string propName;
        ///<summary> Name of the override value used in the shader </summary>
        public string ovrdName;
        ///<summary> Override state of the property </summary>
        public bool ovrd;
        ///<summary> Flag value that indicates that current material has an override for the value </summary>
        public bool hasLocalVal;
        ///<summary> Local value </summary>
        public LocalVal localVal;
        ///<summary> GUI label for the property displayed in the inspector. </summary>
        public GUIContent label;
    }

    public class ToggleOverridable : OverridableBase<bool>
    {
        public string keywordName;
    }
    public class TextureOverridable : OverridableBase<Texture>{ }
    public class Texture2DArrayOverridable : OverridableBase<Texture2DArray> { }
    public class ColorOverridable : OverridableBase<Color>{ }
    public class FloatOverridable : OverridableBase<float> { }
    public class Vector2DOverridable : OverridableBase<Vector2>
    {
        ///<summary> Temp Vector2D value. Used to make it save the local value properly </summary>
        public Vector2 tempVal;
    }

    public enum BlendMode
    {
        Opaque,
        Cutout,
        Fade,
        Transparent
    }

    public enum CullMode
    {
        Off,
        Front,
        Back
    }

    public enum OcclusionChannel
    {
        OcclusionMap,
        MetallicGreen
    }

    public enum SmoothnessMapChannel
    {
        MetallicAlpha,
        AlbedoAlpha
    }

    public CullMode cullingMode = CullMode.Back;

    public MaterialProperty blendMode = null;
    public MaterialProperty terrainHolesTexture = null;
    public MaterialProperty albedoMap = null;
    public MaterialProperty albedoColor = null;
    public MaterialProperty cutoff = null;
    public MaterialProperty metallicMap = null;
    public MaterialProperty metallic = null;
    public MaterialProperty smoothness = null;
    public MaterialProperty smoothnessScale = null;
    public MaterialProperty smoothnessMapChannel = null;
    public MaterialProperty occlusionMapChannel = null;
    public MaterialProperty bumpScale = null;
    public MaterialProperty bumpMap = null;
    public MaterialProperty occlusionStrength = null;
    public MaterialProperty occlusionMap = null;
    public MaterialProperty emissionColorForRendering = null;
    public MaterialProperty emissionMap = null;
    public MaterialProperty alphaToCoverage = null;
    public MaterialProperty useBlueNoiseDither = null;

    public ToggleOverridable coverage = new ToggleOverridable() { propName = "_Coverage", keywordName = "_COVERAGE_ON" };
    public ToggleOverridable stochastic = new ToggleOverridable(){ propName = "_Stochastic", keywordName = "_STOCHASTIC_ON" };
    public ToggleOverridable useAveragedNormals = new ToggleOverridable() { propName = "_UseAveragedNormals", keywordName = "_USE_AVERAGED_NORMALS" };
    public ToggleOverridable paintableCoverage = new ToggleOverridable() { propName = "_PaintableCoverage", keywordName = "_PAINTABLE_COVERAGE_ON" };
    public FloatOverridable distanceFadeStart = new FloatOverridable() { propName = "_DistanceFadeStart" };
    public FloatOverridable distanceFadeFalloff = new FloatOverridable() { propName = "_DistanceFadeFalloff" };
    public FloatOverridable coverageAreaBias = new FloatOverridable() { propName = "_CoverageAreaBias" };
    public FloatOverridable coverageLeakReduction = new FloatOverridable() { propName = "_CoverageLeakReduction" };
    public FloatOverridable coverageAreaMaskRange = new FloatOverridable() { propName = "_CoverageAreaMaskRange" };
    public FloatOverridable precipitationDirOffset = new FloatOverridable() { propName = "_PrecipitationDirOffset" };
    public Vector2DOverridable precipitationDirRange = new Vector2DOverridable() { propName = "_PrecipitationDirRange" };
    public FloatOverridable blendByNormalsStrength = new FloatOverridable() { propName = "_BlendByNormalsStrength" };
    public FloatOverridable blendByNormalsPower = new FloatOverridable() { propName = "_BlendByNormalsPower" };

    public static bool unityStandardShaderProps = true;
    public static bool coverageProperties = true;
    public static readonly string[] blendNames = Enum.GetNames(typeof(BlendMode));

    public virtual void FindProperties()
    {
        blendMode = FindProperty("_Mode", props, false);
        terrainHolesTexture = FindProperty("_TerrainHolesTexture", props, false);
        albedoMap = FindProperty("_MainTex", props, false);
        albedoColor = FindProperty("_Color", props, false);
        cutoff = FindProperty("_Cutoff", props, false);
        metallicMap = FindProperty("_MetallicGlossMap", props, false);
        metallic = FindProperty("_Metallic", props, false);
        smoothnessScale = FindProperty("_GlossMapScale", props, false);
        smoothness = FindProperty("_Glossiness", props, false);
        alphaToCoverage = FindProperty("_AlphaToCoverage", props, false);
        useBlueNoiseDither = FindProperty("_UseBlueNoiseDither", props, false);
        smoothnessMapChannel = FindProperty("_SmoothnessTextureChannel", props, false);
        occlusionMapChannel = FindProperty("_OcclusionTextureChannel", props, false);
        bumpScale = FindProperty("_BumpScale", props, false);
        bumpMap = FindProperty("_BumpMap", props, false);
        occlusionStrength = FindProperty("_OcclusionStrength", props, false);
        occlusionMap = FindProperty("_OcclusionMap", props, false);
        emissionColorForRendering = FindProperty("_EmissionColor", props, false);
        emissionMap = FindProperty("_EmissionMap", props, false);

        #region CoveragePropsFind

        InitOverridable(coverage);
        InitOverridable(stochastic);
        InitOverridable(useAveragedNormals);
        InitOverridable(paintableCoverage);
        InitOverridable(distanceFadeStart);
        InitOverridable(distanceFadeFalloff);
        InitOverridable(coverageAreaBias);
        InitOverridable(coverageLeakReduction);
        InitOverridable(coverageAreaMaskRange);
        InitOverridable(precipitationDirOffset);
        InitOverridable(precipitationDirRange);
        InitOverridable(blendByNormalsStrength);
        InitOverridable(blendByNormalsPower);

        #endregion
    }

    public void InitOverridable<LocalVal>(OverridableBase<LocalVal> overridable)
    {
        overridable.prop = FindProperty(overridable.propName, props, false);
        overridable.ovrdName = $"_{overridable.propName.Substring(1)}Override";

        string labelFieldName = $"{char.ToLower(overridable.propName[1])}{overridable.propName.Substring(2)}Text";

        Type type = typeof(NL_Styles);
        FieldInfo field = type.GetField(labelFieldName, BindingFlags.Public | BindingFlags.Static);

        if (field != null) overridable.label = (GUIContent)field.GetValue(null);
        else overridable.label = new GUIContent(overridable.propName);
    }

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        props = properties;
        FindProperties();
        m_MaterialEditor = materialEditor;

        material = materialEditor.target as Material;

        ShaderPropertiesGUI(material);

        m_inspectorInitiated = true;// set 'init' flag right after the first GUI update to prevent calling things that need to be called only once
    }

    public virtual void ShaderPropertiesGUI(Material material)
    {
        if (NL_Styles.lineB == null || NL_Styles.lineB.normal.background == null) NL_Styles.GetStyles();

        // Use default labelWidth
        EditorGUIUtility.labelWidth = 0f;
        float currentInspectorWidth = EditorGUIUtility.currentViewWidth - 24 - 8;

        #region StandardShaderGUI
        // Primary properties

        if (terrainHolesTexture == null)
        {
            GUILayout.BeginHorizontal(NL_Styles.header);
            GUILayout.Space(currentInspectorWidth / 2 - 98f);
            unityStandardShaderProps = EditorGUILayout.Foldout(unityStandardShaderProps, "STANDARD SHADER PROPERTIES", true);
            GUILayout.EndHorizontal();

            if (unityStandardShaderProps)
            {
                GUILayout.Space(5);

                bool blendModeChanged = false;
                blendModeChanged = BlendModePopup();

                if (material.HasFloat("_CullMode"))
                {
                    cullingMode = (CullMode)material.GetFloat("_CullMode");
                    cullingMode = (CullMode)EditorGUILayout.EnumPopup(NL_Styles.cullModeText, cullingMode);
                    material.SetFloat("_CullMode", (float)cullingMode);
                }

                GUILayout.Label("Main Maps", EditorStyles.boldLabel);
                
                DoAlbedoArea();
                DoMetallicGlossArea();
                DoNormalArea();
                DoOcclusionArea();
                DoEmissionArea(material);

                //EditorGUI.BeginChangeCheck();
                m_MaterialEditor.TextureScaleOffsetProperty(albedoMap);

                //m_MaterialEditor.ShaderProperty(specularHighlights, NL_Styles.specHighlightsText);
                //m_MaterialEditor.ShaderProperty(glossyReflections, NL_Styles.glossyReflectionsText);

                EditorGUILayout.Space();
                GUILayout.Label("Advanced Options", EditorStyles.boldLabel);

                m_MaterialEditor.RenderQueueField();
                m_MaterialEditor.EnableInstancingField();
                m_MaterialEditor.DoubleSidedGIField();
                if(useBlueNoiseDither != null) m_MaterialEditor.ShaderProperty(useBlueNoiseDither, NL_Styles.useBlueNoiseDitherText);
                EditorGUILayout.Space();

                if (blendModeChanged)
                {
                    foreach (var obj in blendMode.targets)
                        SetupMaterialWithBlendMode((Material)obj, (BlendMode)((Material)obj).GetFloat("_Mode"), true);
                }
            }
        }
        #endregion

        EditorGUILayout.Space(1);

        GUILayout.BeginHorizontal(NL_Styles.header);
        GUILayout.Space(currentInspectorWidth / 2 - 56f);
        coverageProperties = EditorGUILayout.Foldout(coverageProperties, "COVERAGE PROPERTIES", true);
        GUILayout.EndHorizontal();
    }

    bool BlendModePopup()
    {
        EditorGUI.showMixedValue = blendMode.hasMixedValue;
        var mode = (BlendMode)blendMode.floatValue;

        EditorGUI.BeginChangeCheck();
        mode = (BlendMode)EditorGUILayout.Popup("Rendering Mode", (int)mode, blendNames);
        bool result = EditorGUI.EndChangeCheck();
        if (result)
        {
            m_MaterialEditor.RegisterPropertyChangeUndo("Rendering Mode");
            blendMode.floatValue = (float)mode;
        }

        EditorGUI.showMixedValue = false;

        return result;
    }

    void SetupMaterialWithBlendMode(Material material, BlendMode blendMode, bool overrideRenderQueue)
    {
        bool isTerrainShader = false;

        if (material.shader.name.Contains("Snow Coverage (Terrain)") 
            || material.shader.name.Contains("Snow Coverage (Terrain-Tessellation)")
            || material.shader.name.Contains("Rain Coverage (Terrain)"))
        {
            isTerrainShader = true;
            blendMode = BlendMode.Opaque;
        }

        int minRenderQueue = -1;
        int maxRenderQueue = 5000;
        int defaultRenderQueue = -1;
        switch (blendMode)
        {
            case BlendMode.Opaque:

                if (isTerrainShader)
                {
                    material.SetOverrideTag("RenderType", "Opaque");
                    material.SetOverrideTag("SRSGroupName", "Terrain");

                    material.DisableKeyword("_RENDERING_CUTOUT");
                    material.DisableKeyword("_RENDERING_FADE");
                    material.DisableKeyword("_RENDERING_TRANSPARENT");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry - 100;
                    
                }
                else
                {
                    material.SetOverrideTag("RenderType", "");
                    material.SetOverrideTag("SRSGroupName", "Opaque");
                    material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
                    material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetFloat("_ZWrite", 1.0f);
                    material.DisableKeyword("_RENDERING_CUTOUT");
                    material.DisableKeyword("_RENDERING_FADE");
                    material.DisableKeyword("_RENDERING_TRANSPARENT");
                    minRenderQueue = -1;
                    maxRenderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest - 1;
                    defaultRenderQueue = -1;
                }
                break;
            case BlendMode.Cutout:
                material.SetOverrideTag("RenderType", "TransparentCutout");
                material.SetOverrideTag("SRSGroupName", "Cutout");
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.Zero);
                material.SetFloat("_ZWrite", 1.0f);
                material.EnableKeyword("_RENDERING_CUTOUT");
                material.DisableKeyword("_RENDERING_FADE");
                material.DisableKeyword("_RENDERING_TRANSPARENT");
                minRenderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                maxRenderQueue = (int)UnityEngine.Rendering.RenderQueue.GeometryLast;
                defaultRenderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                break;
            case BlendMode.Fade:
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetOverrideTag("SRSGroupName", "Fade");
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetFloat("_ZWrite", 0.0f);
                material.DisableKeyword("_RENDERING_CUTOUT");
                material.EnableKeyword("_RENDERING_FADE");
                material.DisableKeyword("_RENDERING_TRANSPARENT");
                minRenderQueue = (int)UnityEngine.Rendering.RenderQueue.GeometryLast + 1;
                maxRenderQueue = (int)UnityEngine.Rendering.RenderQueue.Overlay - 1;
                defaultRenderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                break;
            case BlendMode.Transparent:
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetOverrideTag("SRSGroupName", "Transparent");
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetFloat("_ZWrite", 0.0f);
                material.DisableKeyword("_RENDERING_CUTOUT");
                material.DisableKeyword("_RENDERING_FADE");
                material.EnableKeyword("_RENDERING_TRANSPARENT");
                minRenderQueue = (int)UnityEngine.Rendering.RenderQueue.GeometryLast + 1;
                maxRenderQueue = (int)UnityEngine.Rendering.RenderQueue.Overlay - 1;
                defaultRenderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                break;
        }

        if (overrideRenderQueue || material.renderQueue < minRenderQueue || material.renderQueue > maxRenderQueue)
        {
            if (!overrideRenderQueue)
                Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null, "Render queue value outside of the allowed range ({0} - {1}) for selected Blend mode, resetting render queue to default", minRenderQueue, maxRenderQueue);
            material.renderQueue = defaultRenderQueue;
        }
    }
    void DoEmissionArea(Material material)
    {
        // Emission for GI?
        if (m_MaterialEditor.EmissionEnabledProperty())
        {
            bool hadEmissionTexture = emissionMap.textureValue != null;

            // Texture and HDR color controls
            m_MaterialEditor.TexturePropertyWithHDRColor(NL_Styles.emissionText, emissionMap, emissionColorForRendering, false);

            // If texture was assigned and color was black set color to white
            float brightness = emissionColorForRendering.colorValue.maxColorComponent;
            if (emissionMap.textureValue != null && !hadEmissionTexture && brightness <= 0f)
                emissionColorForRendering.colorValue = Color.white;

            // change the GI flag and fix it up with emissive as black if necessary
            m_MaterialEditor.LightmapEmissionFlagsProperty(MaterialEditor.kMiniTextureFieldLabelIndentLevel, true);
        }
    }

    void DoAlbedoArea()
    {
        m_MaterialEditor.TexturePropertySingleLine(NL_Styles.albedoText, albedoMap, albedoColor);
        BlendMode mode = (BlendMode)blendMode.floatValue;
        if (mode != BlendMode.Opaque)
        {
            m_MaterialEditor.ShaderProperty(cutoff, NL_Styles.alphaCutoffText, MaterialEditor.kMiniTextureFieldLabelIndentLevel);
        }

        if (alphaToCoverage != null && mode == BlendMode.Cutout) m_MaterialEditor.ShaderProperty(alphaToCoverage, NL_Styles.alphaToCoverageText, 3);
    }
    void DoNormalArea()
    {
        m_MaterialEditor.TexturePropertySingleLine(NL_Styles.normalMapText, bumpMap, bumpMap.textureValue != null ? bumpScale : null);
        if (bumpScale.floatValue != 1
            && UnityEditorInternal.InternalEditorUtility.IsMobilePlatform(EditorUserBuildSettings.activeBuildTarget))
            if (m_MaterialEditor.HelpBoxWithButton(
                EditorGUIUtility.TrTextContent("Bump scale is not supported on mobile platforms"),
                EditorGUIUtility.TrTextContent("Fix Now")))
            {
                bumpScale.floatValue = 1;
            }
    }

    void DoMetallicGlossArea()
    {
        bool hasGlossMap = false;
        bool hasMetallicMap = false;
        bool hasAlbedoMap = false;

        hasMetallicMap = metallicMap.textureValue != null;
        BlendMode mode = (BlendMode)blendMode.floatValue;

        if (smoothnessMapChannel != null && mode == BlendMode.Opaque)
        {
            if ((int)smoothnessMapChannel.floatValue == (int)SmoothnessMapChannel.AlbedoAlpha) hasAlbedoMap = albedoMap.textureValue != null;
            hasGlossMap = hasMetallicMap || hasAlbedoMap;
        }
        else
        {
            hasGlossMap = hasMetallicMap;
        }

        m_MaterialEditor.TexturePropertySingleLine(NL_Styles.metallicMapText, metallicMap, hasMetallicMap ? null : metallic);

        bool showSmoothnessScale = hasGlossMap;

        int indentation = 2; // align with labels of texture properties
        m_MaterialEditor.ShaderProperty(showSmoothnessScale ? smoothnessScale : smoothness, showSmoothnessScale ? NL_Styles.smoothnessScaleText : NL_Styles.smoothnessText, indentation);

        ++indentation;
        if (smoothnessMapChannel != null && mode == BlendMode.Opaque)
            m_MaterialEditor.ShaderProperty(smoothnessMapChannel, NL_Styles.smoothnessMapChannelText, indentation);
        else smoothnessMapChannel.floatValue = 0;

    }

    void DoOcclusionArea()
    {
        bool hasOcclusionMap = false;

        hasOcclusionMap = occlusionMap.textureValue != null && (int)occlusionMapChannel.floatValue == 0 || metallicMap.textureValue != null && (int)occlusionMapChannel.floatValue == 1;

        m_MaterialEditor.TexturePropertySingleLine(NL_Styles.occlusionText, occlusionMap, hasOcclusionMap ? occlusionStrength : null);

        if (occlusionMapChannel != null)
        {
            int indentation = 2;
            m_MaterialEditor.ShaderProperty(occlusionMapChannel, NL_Styles.occlusionMapChannelText, indentation);
        }
    }

    static OcclusionChannel GetOcclusionChannel(Material material)
    {
        int ch = (int)material.GetFloat("_OcclusionTextureChannel");
        if (ch == (int)OcclusionChannel.MetallicGreen)
            return OcclusionChannel.MetallicGreen;
        else
            return OcclusionChannel.OcclusionMap;
    }

    static SmoothnessMapChannel GetSmoothnessMapChannel(Material material)
    {
        int ch = (int)material.GetFloat("_SmoothnessTextureChannel");
        if (ch == (int)SmoothnessMapChannel.AlbedoAlpha)
            return SmoothnessMapChannel.AlbedoAlpha;
        else
            return SmoothnessMapChannel.MetallicAlpha;
    }

    public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
    {
        // _Emission property is lost after assigning Standard shader to the material
        // thus transfer it before assigning the new shader
        if (material.HasProperty("_Emission"))
        {
            material.SetColor("_EmissionColor", material.GetColor("_Emission"));
        }

        base.AssignNewShaderToMaterial(material, oldShader, newShader);

        if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/"))
        {
            SetupMaterialWithBlendMode(material, (BlendMode)material.GetFloat("_Mode"), true);
            return;
        }

        BlendMode blendMode = BlendMode.Opaque;
        if (oldShader.name.Contains("/Transparent/Cutout/"))
        {
            blendMode = BlendMode.Cutout;
        }
        else if (oldShader.name.Contains("/Transparent/"))
        {
            // NOTE: legacy shaders did not provide physically based transparency
            // therefore Fade mode
            blendMode = BlendMode.Fade;
        }
        material.SetFloat("_Mode", (float)blendMode);

        SetupMaterialWithBlendMode(material, blendMode, true);
    }

    public virtual void SetMaterialKeywords(Material material)
    {
        if (material.HasProperty("_MetallicGlossMap"))
            SetKeyword(material, "_METALLIC_MAP", material.GetTexture("_MetallicGlossMap"));
        if (material.HasProperty("_BumpMap"))
            SetKeyword(material, "_NORMALMAP", material.GetTexture("_BumpMap") || 
                (material.HasProperty("_CoverageTex0") && material.GetTexture("_CoverageTex0")) || 
                (material.HasProperty("_PrimaryMasks") && material.GetTexture("_PrimaryMasks")));
        if (material.HasProperty("_EmissionColor"))
        {
            MaterialEditor.FixupEmissiveFlag(material);
            bool shouldEmissionBeEnabled = (material.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack) == 0;
            SetKeyword(material, "_EMISSION", shouldEmissionBeEnabled);
            SetKeyword(material, "_EMISSION_MAP", material.GetTexture("_EmissionMap"));
        }
        if (material.HasProperty("_OcclusionTextureChannel"))
        {
            SetKeyword(material, "_OCCLUSION_MAP", material.GetTexture("_OcclusionMap"));
            SetKeyword(material, "_OCCLUSION_TEX_METALLIC_CHANNEL_G", GetOcclusionChannel(material) == OcclusionChannel.MetallicGreen);
        }
        if (material.HasProperty("_SmoothnessTextureChannel"))
            SetKeyword(material, "_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A", GetSmoothnessMapChannel(material) == SmoothnessMapChannel.AlbedoAlpha);
        if (material.HasProperty("_UseBlueNoiseDither"))
            SetKeyword(material, "_USE_BLUE_NOISE_DITHER", material.GetFloat("_UseBlueNoiseDither") == 1);

        if (material.HasProperty("_CoverageOverride") && material.GetFloat("_CoverageOverride") == 1)
            SetKeyword(material, "_COVERAGE_ON", material.GetFloat("_Coverage") == 1);
        if (material.HasProperty("_StochasticOverride") && material.GetFloat("_StochasticOverride") == 1)
            SetKeyword(material, "_STOCHASTIC_ON", material.GetFloat("_Stochastic") == 1);
        if (material.HasProperty("_PaintableCoverageOverride") && material.GetFloat("_PaintableCoverageOverride") == 1)
            SetKeyword(material, "_PAINTABLE_COVERAGE_ON", material.GetFloat("_PaintableCoverage") == 1);
        if (material.HasProperty("_UseAveragedNormalsOverride") && material.GetFloat("_UseAveragedNormalsOverride") == 1)
            SetKeyword(material, "_USE_AVERAGED_NORMALS", material.GetFloat("_UseAveragedNormals") == 1);
    }

    public static void SetKeyword(Material m, string keyword, bool state)
    {
        if (state)
            m.EnableKeyword(keyword);
        else
            m.DisableKeyword(keyword);
    }

    /// <summary>Overrides a global Texture property by the local one in the current material.</summary>
    /// <param name="overridable">The overridable class, that contains all the neccessary properties.</param>
    /// <param name="val">output local value.</param>
    /// <param name="indentation">an optional indent level of the property in the inspector GUI.</param>
    public void TextureValueOverride(TextureOverridable overridable, out Texture val, int indentation = 0)
    {
        GUILayout.BeginHorizontal();

        GUILayout.Space(4 + indentation);

        if (material.HasFloat(overridable.ovrdName)) overridable.ovrd = material.GetFloat(overridable.ovrdName) == 1;
        overridable.ovrd = EditorGUILayout.Toggle(new GUIContent(), overridable.ovrd, GUILayout.MaxWidth(16));

        if (overridable.ovrd)
        {
            if (overridable.hasLocalVal)
            {
                overridable.prop.textureValue = overridable.localVal;
                overridable.hasLocalVal = false;
            }
        }

        EditorGUI.BeginDisabledGroup(!overridable.ovrd);

        EditorGUILayout.PrefixLabel(overridable.label);
        EditorGUILayout.LabelField("", GUILayout.Width(0));
        m_MaterialEditor.TexturePropertySingleLine(new GUIContent(), overridable.prop);

        if (overridable.ovrd)
        {
            overridable.localVal = overridable.prop.textureValue;
            overridable.hasLocalVal = true;
        }

        material.SetFloat(overridable.ovrdName, overridable.ovrd ? 1 : 0);
        EditorGUI.EndDisabledGroup();
        GUILayout.EndHorizontal();

        //hasVal = overridable.hasLocalVal;
        val = overridable.localVal;
    }

    public void Texture2DArrayValueOverride(Texture2DArrayOverridable overridable, out Texture2DArray val, int indentation = 0)
    {
        GUILayout.BeginHorizontal();

        GUILayout.Space(4 + indentation);

        if (material.HasFloat(overridable.ovrdName)) overridable.ovrd = material.GetFloat(overridable.ovrdName) == 1;
        overridable.ovrd = EditorGUILayout.Toggle(new GUIContent(), overridable.ovrd, GUILayout.MaxWidth(16));

        if (overridable.ovrd)
        {
            if (overridable.hasLocalVal)
            {
                overridable.prop.textureValue = overridable.localVal;
                overridable.hasLocalVal = false;
            }
        }

        EditorGUI.BeginDisabledGroup(!overridable.ovrd);

        EditorGUILayout.PrefixLabel(overridable.label);
        EditorGUILayout.LabelField("", GUILayout.Width(0));
        m_MaterialEditor.TexturePropertySingleLine(new GUIContent(), overridable.prop);

        if (overridable.ovrd)
        {
            overridable.localVal = overridable.prop.textureValue as Texture2DArray;
            overridable.hasLocalVal = true;
        }

        material.SetFloat(overridable.ovrdName, overridable.ovrd ? 1 : 0);
        EditorGUI.EndDisabledGroup();
        GUILayout.EndHorizontal();

        //hasVal = overridable.hasLocalVal;
        val = overridable.localVal;
    }

    ///<summary> Overrides a global Color property by the local one in the current material.</summary>
    /// <param name="overridable">The overridable class, that contains all the neccessary properties.</param>
    /// <param name="val">output local value.</param>
    /// <param name="indentation">an optional indent level of the property in the inspector GUI.</param>
    public void ColorValueOverride(ColorOverridable overridable, out Color val, int indentation = 0)
    {
        GUILayout.BeginHorizontal();

        GUILayout.Space(4);

        if (material.HasFloat(overridable.ovrdName)) overridable.ovrd = material.GetFloat(overridable.ovrdName) == 1;
        overridable.ovrd = EditorGUILayout.Toggle(new GUIContent(), overridable.ovrd, GUILayout.MaxWidth(16));

        if (overridable.ovrd)
        {
            if (overridable.hasLocalVal)
            {
                overridable.prop.colorValue = overridable.localVal;
                overridable.hasLocalVal = false;
            }
        }

        EditorGUI.BeginDisabledGroup(!overridable.ovrd);
        m_MaterialEditor.ShaderProperty(overridable.prop, overridable.label, indentation);

        if (overridable.ovrd)
        {
            overridable.localVal = overridable.prop.colorValue;
            overridable.hasLocalVal = true;
        }

        material.SetFloat(overridable.ovrdName, overridable.ovrd ? 1 : 0);
        EditorGUI.EndDisabledGroup();
        GUILayout.EndHorizontal();

        //hasVal = overridable.hasLocalVal;
        val = overridable.localVal;
    }

    ///<summary> Overrides a global Toggle(float) property by the local one in the current material. 
    ///Ussually used for keyword switching. 
    ///A keyword state itself must be set in the ValidateMaterial method.</summary>
    /// <param name="overridable">The overridable class, that contains all the neccessary properties.</param>
    /// <param name="val">output local value.</param>
    /// <param name="indentation">an optional indent level of the property in the inspector GUI.</param>
    public void ToggleValueOverride(ToggleOverridable toggleProp, out bool val, int indentation = 0)
    {
        GUILayout.BeginHorizontal();

        GUILayout.Space(4);

        if (material.HasFloat(toggleProp.ovrdName)) toggleProp.ovrd = material.GetFloat(toggleProp.ovrdName) == 1;
        toggleProp.ovrd = EditorGUILayout.Toggle(new GUIContent(), toggleProp.ovrd, GUILayout.MaxWidth(16));

        if (toggleProp.ovrd)
        {
            if (toggleProp.hasLocalVal)
            {
                toggleProp.prop.floatValue = toggleProp.localVal ? 1 : 0;
                toggleProp.hasLocalVal = false;
            }
        }

        EditorGUI.BeginDisabledGroup(!toggleProp.ovrd);
        EditorGUILayout.PrefixLabel(toggleProp.label);
        GUILayout.Space(2);

        if (toggleProp.ovrd)
        {
            toggleProp.localVal = toggleProp.prop.floatValue == 1;
            toggleProp.localVal = EditorGUILayout.Toggle(toggleProp.localVal);
            toggleProp.hasLocalVal = true;
        }
        else
        {
            EditorGUILayout.Toggle(toggleProp.prop.floatValue == 1);
        }

        material.SetFloat(toggleProp.ovrdName, toggleProp.ovrd ? 1 : 0);
        EditorGUI.EndDisabledGroup();
        GUILayout.EndHorizontal();

        //hasVal = toggleProp.hasLocalVal;
        val = toggleProp.localVal;

        if (toggleProp.ovrd) toggleProp.prop.floatValue = val ? 1 : 0;
    }

    ///<summary> Overrides a global float property by the local one in the current material.</summary>
    /// <param name="overridable">The overridable class, that contains all the neccessary properties.</param>
    /// <param name="hasVal">output 'hasLocalValue' flag.</param>
    /// <param name="val">output local value.</param>
    /// <param name="limits">an optional min-max limits to clamp the float value.</param>
    /// <param name="indentation">an optional indent level of the property in the inspector GUI.</param>
    public void FloatValueOverride(FloatOverridable overridable, out float val, Vector2 limits = new Vector2(), bool roundToInt = false, int indentation = 0)
    {
        GUILayout.BeginHorizontal();

        GUILayout.Space(4);

        if (material.HasFloat(overridable.ovrdName)) overridable.ovrd = material.GetFloat(overridable.ovrdName) == 1;
        overridable.ovrd = EditorGUILayout.Toggle(new GUIContent(), overridable.ovrd, GUILayout.MaxWidth(16));

        if (roundToInt) overridable.localVal = Mathf.CeilToInt(overridable.localVal);

        if (limits != Vector2.zero) overridable.localVal = Mathf.Clamp(overridable.localVal, limits.x, limits.y);

        if (overridable.ovrd)
        {
            if (overridable.hasLocalVal)
            {
                overridable.prop.floatValue = overridable.localVal;
                overridable.hasLocalVal = false;
            }
        }

        EditorGUI.BeginDisabledGroup(!overridable.ovrd);

        if (overridable.ovrd) material.SetFloat(overridable.prop.name, overridable.prop.floatValue);
        m_MaterialEditor.ShaderProperty(overridable.prop, overridable.label, indentation);

        if (overridable.ovrd)
        {
            overridable.localVal = overridable.prop.floatValue;
            overridable.hasLocalVal = true;
        }

        material.SetFloat(overridable.ovrdName, overridable.ovrd ? 1 : 0);
        EditorGUI.EndDisabledGroup();
        GUILayout.EndHorizontal();

        //hasVal = overridable.hasLocalVal;
        val = overridable.localVal;
    }

    ///<summary> Overrides a range (Vector2D) property by the local one in the current material.</summary>
    /// <param name="overridable">The overridable class, that contains all the neccessary properties.</param>
    /// <param name="val">output local value.</param>
    /// <param name="indentation">an optional indent level of the property in the inspector GUI.</param>
    public void RangeValueOverride(Vector2DOverridable overridable, out Vector2 val, int indentation = 0)
    {
        GUILayout.BeginHorizontal();

        GUILayout.Space(4);

        if (material.HasFloat(overridable.ovrdName)) overridable.ovrd = material.GetFloat(overridable.ovrdName) == 1;
        overridable.ovrd = EditorGUILayout.Toggle(new GUIContent(), overridable.ovrd, GUILayout.MaxWidth(18));

        if (overridable.ovrd)
        {
            if (overridable.hasLocalVal)
            {
                overridable.prop.vectorValue = overridable.localVal;
                overridable.hasLocalVal = false;
            }
        }

        EditorGUI.BeginDisabledGroup(!overridable.ovrd);

        GUILayout.BeginHorizontal();
        GUILayout.Space(indentation - 2);
        EditorGUILayout.PrefixLabel(overridable.label);

        GUILayout.Space(2);

        if (!m_inspectorInitiated)
            overridable.tempVal = overridable.prop.vectorValue;

        if (overridable.ovrd)
        {
            overridable.tempVal.x = EditorGUILayout.FloatField(overridable.tempVal.x, GUILayout.MaxWidth(50));
            EditorGUILayout.MinMaxSlider(ref overridable.tempVal.x, ref overridable.tempVal.y, 0, 1);
            overridable.tempVal.y = EditorGUILayout.FloatField(overridable.tempVal.y, GUILayout.MaxWidth(50));
        }
        else
        {
            Vector2 refVal = overridable.prop.vectorValue;
            refVal.x = EditorGUILayout.FloatField(overridable.prop.vectorValue.x, GUILayout.MaxWidth(50));
            EditorGUILayout.MinMaxSlider(ref refVal.x, ref refVal.y, 0, 1);
            refVal.y = EditorGUILayout.FloatField(overridable.prop.vectorValue.y, GUILayout.MaxWidth(50));
        }

        overridable.tempVal.x = Mathf.Clamp((float)Math.Round(overridable.tempVal.x, 3), 0, 1);
        overridable.tempVal.y = Mathf.Clamp((float)Math.Round(overridable.tempVal.y, 3), 0, 1);

        GUILayout.EndHorizontal();

        if (overridable.ovrd)
        {
            material.SetVector(overridable.prop.name, overridable.tempVal);
            overridable.localVal = overridable.prop.vectorValue;
            overridable.hasLocalVal = true;
        }

        material.SetFloat(overridable.ovrdName, overridable.ovrd ? 1 : 0);
        EditorGUI.EndDisabledGroup();
        GUILayout.EndHorizontal();

        //hasVal = overridable.hasLocalVal;
        val = overridable.localVal;
    }

    ///<summary> Overrides a global Vector2D property by the local one in the current material.</summary>
    /// <param name="overridable">The overridable class, that contains all the neccessary properties.</param>
    /// <param name="val">output local value.</param>
    /// <param name="indentation">an optional indent level of the property in the inspector GUI.</param>
    public void Vector2DValueOverride(Vector2DOverridable overridable, out Vector2 val, int indentation = 0)
    {
        GUILayout.BeginHorizontal();

        GUILayout.Space(4);

        if (material.HasFloat(overridable.ovrdName)) overridable.ovrd = material.GetFloat(overridable.ovrdName) == 1;
        overridable.ovrd = EditorGUILayout.Toggle(new GUIContent(), overridable.ovrd, GUILayout.MaxWidth(18));

        if (overridable.ovrd)
        {
            if (overridable.hasLocalVal)
            {
                overridable.prop.vectorValue = overridable.localVal;
                overridable.hasLocalVal = false;
            }
        }

        EditorGUI.BeginDisabledGroup(!overridable.ovrd);

        GUILayout.BeginHorizontal();
        GUILayout.Space(indentation - 2);
        EditorGUILayout.PrefixLabel(overridable.label);

        GUILayout.Space(2);

        if (!m_inspectorInitiated)
            overridable.tempVal = overridable.prop.vectorValue;

        overridable.tempVal = EditorGUILayout.Vector2Field("", overridable.tempVal);
        overridable.tempVal.x = (float)Math.Round(overridable.tempVal.x, 3);
        overridable.tempVal.y = (float)Math.Round(overridable.tempVal.y, 3);

        material.SetVector(overridable.prop.name, overridable.tempVal);

        GUILayout.EndHorizontal();

        if (overridable.ovrd)
        {
            overridable.localVal = overridable.prop.vectorValue;
            overridable.hasLocalVal = true;
        }

        material.SetFloat(overridable.ovrdName, overridable.ovrd ? 1 : 0);
        EditorGUI.EndDisabledGroup();
        GUILayout.EndHorizontal();

        //hasVal = overridable.hasLocalVal;
        val = overridable.localVal;
        //modifiedTempVal = tempVal;
    }

    public string GetPropName(string ovrdName)
    {
        return ovrdName.Replace("Override", "");
    }
}
