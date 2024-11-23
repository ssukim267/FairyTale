#if UNITY_EDITOR
using NOT_Lonely.TotalBrush;
using NOT_Lonely.Weatherade;
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class NL_SRS_RainCoverage_GUI : SRS_CoverageShaderGUI_base
{
    MaterialProperty specularHighlights = null;
    MaterialProperty glossyReflections = null;

    //TextureOverridable primaryMasks = new TextureOverridable() { propName = "_PrimaryMasks" };
    //Texture2DArrayOverridable ripplesTex = new Texture2DArrayOverridable() { propName = "_RipplesTex" };
    ColorOverridable wetColor = new ColorOverridable() { propName = "_WetColor" };
    FloatOverridable wetnessAmount = new FloatOverridable() { propName = "_WetnessAmount" };
    FloatOverridable puddlesAmount = new FloatOverridable() { propName = "_PuddlesAmount" };
    Vector2DOverridable puddlesRange = new Vector2DOverridable() { propName = "_PuddlesRange" };
    FloatOverridable puddlesMult = new FloatOverridable() { propName = "_PuddlesMult" };
    //FloatOverridable puddlesBlendContrast = new FloatOverridable() { propName = "_PuddlesBlendContrast" };
    //FloatOverridable puddlesBlendStrength = new FloatOverridable() { propName = "_PuddlesBlendStrength" };
    FloatOverridable puddlesTiling = new FloatOverridable() { propName = "_PuddlesTiling" };
    FloatOverridable puddlesSlope = new FloatOverridable() { propName = "_PuddlesSlope" };
    ToggleOverridable ripples = new ToggleOverridable() { propName = "_Ripples", keywordName = "_RIPPLES_ON" };
    FloatOverridable ripplesAmount = new FloatOverridable() { propName = "_RipplesAmount" };
    FloatOverridable ripplesIntensity = new FloatOverridable() { propName = "_RipplesIntensity" };
    FloatOverridable ripplesTiling = new FloatOverridable() { propName = "_RipplesTiling" };
    FloatOverridable ripplesFPS = new FloatOverridable() { propName = "_RipplesFPS" };
    FloatOverridable spotsIntensity = new FloatOverridable() { propName = "_SpotsIntensity" };
    FloatOverridable spotsAmount = new FloatOverridable() { propName = "_SpotsAmount" };
    ToggleOverridable drips = new ToggleOverridable() { propName = "_Drips", keywordName = "_DRIPS_ON" };
    FloatOverridable dripsIntensity = new FloatOverridable() { propName = "_DripsIntensity" };
    FloatOverridable dripsSpeed = new FloatOverridable() { propName = "_DripsSpeed" };
    Vector2DOverridable dripsTiling = new Vector2DOverridable() { propName = "_DripsTiling" };
    FloatOverridable distortionTiling = new FloatOverridable() { propName = "_DistortionTiling" };
    FloatOverridable distortionAmount = new FloatOverridable() { propName = "_DistortionAmount" };

    public override void FindProperties()
    {
        base.FindProperties();

        specularHighlights = FindProperty("_SpecularHighlights", props, false);
        glossyReflections = FindProperty("_GlossyReflections", props, false);

        //InitOverridable(primaryMasks);
        //InitOverridable(ripplesTex);
        InitOverridable(wetColor);
        InitOverridable(wetnessAmount);
        InitOverridable(puddlesAmount);
        InitOverridable(puddlesMult);
        InitOverridable(puddlesRange);
        //InitOverridable(puddlesBlendContrast);
        //InitOverridable(puddlesBlendStrength);
        InitOverridable(puddlesTiling);
        InitOverridable(puddlesSlope);
        InitOverridable(ripples);
        InitOverridable(ripplesAmount);
        InitOverridable(ripplesIntensity);
        InitOverridable(ripplesTiling);
        InitOverridable(ripplesFPS);
        InitOverridable(spotsIntensity);
        InitOverridable(spotsAmount);
        InitOverridable(drips);
        InitOverridable(dripsIntensity);
        InitOverridable(dripsSpeed);
        InitOverridable(dripsTiling);
        InitOverridable(distortionTiling);
        InitOverridable(distortionAmount);
    }

    public override void ShaderPropertiesGUI(Material material)
    {
        base.ShaderPropertiesGUI(material);

        if (coverageProperties)
        {
            if (RainCoverage.instance == null) EditorGUILayout.HelpBox("There's no Rain Coverage instance in the scene. Please add one to make the rain shaders work correctly.", MessageType.Warning);
            else
            {
                GUILayout.Space(5);
                GUILayout.BeginHorizontal(NL_Styles.lineB);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Select Global Coverage Instance", GUILayout.MaxWidth(200))) Selection.activeGameObject = RainCoverage.instance.gameObject;
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }

            GUILayout.BeginHorizontal(NL_Styles.lineB);
            if(coverage.prop != null) ToggleValueOverride(coverage, out coverage.localVal);
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            if (coverage.prop.floatValue == 1)//coverage switch
            {
                NL_Utilities.BeginUICategory("MASKS", NL_Styles.lineB);

                GUILayout.BeginHorizontal();
                ToggleValueOverride(paintableCoverage, out paintableCoverage.localVal);
                EditorGUI.BeginDisabledGroup(material.GetFloat("_PaintableCoverage") == 0);
                if (GUILayout.Button("Open Total Brush", GUILayout.MaxWidth(120))) NL_TotalBrush.OpenWindowExternal(material.shader.name.Contains("Terrain") ? NL_TotalBrush.Mode.Terrain : NL_TotalBrush.Mode.Mesh);
                EditorGUI.EndDisabledGroup();
                GUILayout.EndHorizontal();
                if (material.GetFloat("_PaintableCoverage") == 1 && m_MaterialEditor.IsInstancingEnabled())
                    EditorGUILayout.HelpBox("GPU Instancing is enabled on this material, this will break vertex colors " +
                        "on different objects. Consider disable instancing if you want to use vertex colors.", MessageType.Warning);

                if (terrainHolesTexture == null)
                {
                    GUILayout.BeginHorizontal();
                    ToggleValueOverride(useAveragedNormals, out useAveragedNormals.localVal);

                    EditorGUI.BeginDisabledGroup(useAveragedNormals.prop.floatValue == 0);
                    if (GUILayout.Button("Average Normals", GUILayout.MaxWidth(120))) NL_TotalBrush.AverageNormals();
                    EditorGUI.EndDisabledGroup();
                    GUILayout.EndHorizontal();
                }

                if (material.HasProperty("_StochasticOverride"))
                    ToggleValueOverride(stochastic, out stochastic.localVal);

                NL_Utilities.EndUICategory();

                NL_Utilities.BeginUICategory("WETNESS", NL_Styles.lineB);
                

                ColorValueOverride(wetColor, out wetColor.localVal);
                FloatValueOverride(wetnessAmount, out wetnessAmount.localVal);
                NL_Utilities.EndUICategory();

                NL_Utilities.BeginUICategory("PUDDLES", NL_Styles.lineB);
                FloatValueOverride(puddlesAmount, out puddlesAmount.localVal);
                FloatValueOverride(puddlesMult, out puddlesMult.localVal);
                //FloatValueOverride(puddlesBlendStrength, out puddlesBlendStrength.localVal, new Vector2(1, float.PositiveInfinity));
                //FloatValueOverride(puddlesBlendContrast, out puddlesBlendContrast.localVal, new Vector2(0, float.PositiveInfinity), false, 1);
                RangeValueOverride(puddlesRange, out puddlesRange.localVal);
                FloatValueOverride(puddlesTiling, out puddlesTiling.localVal, new Vector2(0, float.PositiveInfinity));
                FloatValueOverride(puddlesSlope, out puddlesSlope.localVal);
                NL_Utilities.EndUICategory();

                NL_Utilities.BeginUICategory("RIPPLES AND SPOTS", NL_Styles.lineB);
                ToggleValueOverride(ripples, out ripples.localVal);
                FloatValueOverride(ripplesAmount, out ripplesAmount.localVal, new Vector2(0, 15), true);
                FloatValueOverride(ripplesIntensity, out ripplesIntensity.localVal, new Vector2(0, float.PositiveInfinity));
                FloatValueOverride(ripplesFPS, out ripplesFPS.localVal, new Vector2(0, 120), true);
                FloatValueOverride(ripplesTiling, out ripplesTiling.localVal, new Vector2(0, float.PositiveInfinity));
                FloatValueOverride(spotsIntensity, out spotsIntensity.localVal);
                if (spotsIntensity.prop.floatValue > 0)
                    FloatValueOverride(spotsAmount, out spotsAmount.localVal);
                NL_Utilities.EndUICategory();

                NL_Utilities.BeginUICategory("DRIPS", NL_Styles.lineB);
                ToggleValueOverride(drips, out drips.localVal);
                FloatValueOverride(dripsIntensity, out dripsIntensity.localVal);
                FloatValueOverride(dripsSpeed, out dripsSpeed.localVal);
                Vector2DValueOverride(dripsTiling, out dripsTiling.localVal);
                FloatValueOverride(distortionAmount, out distortionAmount.localVal, new Vector2(0, float.PositiveInfinity));
                if (distortionAmount.prop.floatValue > 0)
                    FloatValueOverride(distortionTiling, out distortionTiling.localVal, new Vector2(0, float.PositiveInfinity), false, 1);
                NL_Utilities.EndUICategory();

                NL_Utilities.BeginUICategory("AREA MASK", NL_Styles.lineB);
                FloatValueOverride(coverageAreaMaskRange, out coverageAreaMaskRange.localVal);
                FloatValueOverride(coverageAreaBias, out coverageAreaBias.localVal);
                FloatValueOverride(coverageLeakReduction, out coverageLeakReduction.localVal);
                FloatValueOverride(precipitationDirOffset, out precipitationDirOffset.localVal);
                RangeValueOverride(precipitationDirRange, out precipitationDirRange.localVal);
                NL_Utilities.EndUICategory();

                NL_Utilities.BeginUICategory("BLEND BY NORMALS", NL_Styles.lineB);
                FloatValueOverride(blendByNormalsStrength, out blendByNormalsStrength.localVal, new Vector2(0, float.PositiveInfinity));
                FloatValueOverride(blendByNormalsPower, out blendByNormalsPower.localVal, new Vector2(0, float.PositiveInfinity));
                NL_Utilities.EndUICategory();

                if (distanceFadeStart.prop != null)
                {
                    NL_Utilities.BeginUICategory("DISTANCE FADE", NL_Styles.lineB);
                    FloatValueOverride(distanceFadeStart, out distanceFadeStart.localVal);
                    FloatValueOverride(distanceFadeFalloff, out distanceFadeFalloff.localVal);
                    NL_Utilities.EndUICategory();
                }
            }
        }
    }

    override public void SetMaterialKeywords(Material material)
    {
        base.SetMaterialKeywords(material);

        if (material.HasProperty(ripples.ovrdName) && material.GetFloat(ripples.ovrdName) == 1)
            SetKeyword(material, ripples.keywordName, material.GetFloat(GetPropName(ripples.ovrdName)) == 1);
        if (material.HasProperty(drips.ovrdName) && material.GetFloat(drips.ovrdName) == 1)
            SetKeyword(material, drips.keywordName, material.GetFloat(GetPropName(drips.ovrdName)) == 1);
    }

    override public void ValidateMaterial(Material material)
    {
        SetMaterialKeywords(material);
        /*
        if(ripplesTex.localVal != null) 
            material.SetFloat("_RipplesFramesCount", ripplesTex.localVal.depth);
        */
        SnowCoverage.UpdateMtl(material);
    }
}
#endif
