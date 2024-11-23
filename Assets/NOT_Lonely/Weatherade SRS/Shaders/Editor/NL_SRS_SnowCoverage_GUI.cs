#if UNITY_EDITOR
using NOT_Lonely.TotalBrush;
using NOT_Lonely.Weatherade;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
public class NL_SRS_SnowCoverage_GUI : SRS_CoverageShaderGUI_base
{
    #region CoverageProperties
    TextureOverridable coverageTex0 = new TextureOverridable() { propName = "_CoverageTex0" };
    FloatOverridable coverageTiling = new FloatOverridable() { propName = "_CoverageTiling"};
    FloatOverridable coverageAmount = new FloatOverridable() { propName = "_CoverageAmount" };
    FloatOverridable heightMap0Contrast = new FloatOverridable() { propName = "_HeightMap0Contrast" };
    FloatOverridable coverageNormalScale0 = new FloatOverridable() { propName = "_CoverageNormalScale0" };
    ColorOverridable coverageColor = new ColorOverridable() { propName = "_CoverageColor" };
    FloatOverridable coverageNormalsOverlay = new FloatOverridable() { propName = "_CoverageNormalsOverlay" };
    
    //Traces
    ToggleOverridable traces = new ToggleOverridable() { propName = "_Traces", keywordName = "_TRACES_ON"};
    ToggleOverridable traceDetail = new ToggleOverridable() { propName = "_TraceDetail", keywordName = "_TRACE_DETAIL" };
    FloatOverridable tracesNormalScale = new FloatOverridable() { propName = "_TracesNormalScale" };
    FloatOverridable tracesBlendFactor = new FloatOverridable() { propName = "_TracesBlendFactor" };
    
    //Tessellation
    ToggleOverridable tessellation = new ToggleOverridable() { propName = "_Tessellation" };
    FloatOverridable tessEdgeL = new FloatOverridable() { propName = "_TessEdgeL" };
    FloatOverridable tessFactorSnow = new FloatOverridable() { propName = "_TessFactorSnow" };
    Vector2DOverridable tessSnowdriftRange = new Vector2DOverridable() { propName = "_TessSnowdriftRange" };
    FloatOverridable tessMaxDisp = new FloatOverridable() { propName = "_TessMaxDisp" };

    //Displacement
    ToggleOverridable displacement = new ToggleOverridable() { propName = "_Displacement", keywordName = "_DISPLACEMENT_ON" };
    FloatOverridable coverageDisplacement = new FloatOverridable() { propName = "_CoverageDisplacement" };
    FloatOverridable coverageDisplacementOffset = new FloatOverridable() { propName = "_CoverageDisplacementOffset" };
    
    //Sparkle
    ToggleOverridable sparkle = new ToggleOverridable() { propName = "_Sparkle", keywordName = "_SPARKLE_ON" };
    ToggleOverridable sss = new ToggleOverridable() { propName = "_Sss", keywordName = "_SSS_ON" };

    FloatOverridable emissionMasking = new FloatOverridable() { propName = "_EmissionMasking" };
    FloatOverridable maskCoverageByAlpha = new FloatOverridable() { propName = "_MmaskCoverageByAlpha" };
    #endregion

    public override void FindProperties()
    {
        base.FindProperties();

        #region CoveragePropsFind

        InitOverridable(coverageTex0);
        InitOverridable(coverageTiling);
        InitOverridable(coverageAmount);
        InitOverridable(heightMap0Contrast);
        InitOverridable(coverageNormalScale0);
        InitOverridable(coverageColor);
        //InitOverridable(baseCoverageNormalsBlend);
        InitOverridable(coverageNormalsOverlay);
        InitOverridable(traces);
        InitOverridable(traceDetail);
        InitOverridable(tracesNormalScale);
        InitOverridable(tracesBlendFactor);
        InitOverridable(tessellation);
        InitOverridable(tessEdgeL);
        InitOverridable(tessFactorSnow);
        InitOverridable(tessSnowdriftRange);
        InitOverridable(tessMaxDisp);
        InitOverridable(displacement);
        InitOverridable(coverageDisplacement);
        InitOverridable(coverageDisplacementOffset);
        InitOverridable(sparkle);
        InitOverridable(sss);
        InitOverridable(emissionMasking);
        InitOverridable(maskCoverageByAlpha);
        
        #endregion
    }

    public override void ShaderPropertiesGUI(Material material)
    {
        base.ShaderPropertiesGUI(material);

        if (coverageProperties)
        {
            if (SnowCoverage.instance == null)
            {
                EditorGUILayout.HelpBox("There's no Snow Coverage instance in the scene. Please add one to make the snow shaders work correctly.", MessageType.Warning);
                if (GUILayout.Button("Fix Now"))
                {
                    SnowCoverage.CreateNewSnowCoverageInstance();
                    SnowCoverage.UpdateMtl(material);
                }
            }
            else
            {
                GUILayout.Space(5);
                GUILayout.BeginHorizontal(NL_Styles.lineB);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Select Global Coverage Instance", GUILayout.MaxWidth(200))) Selection.activeGameObject = SnowCoverage.instance.gameObject;
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
            }

            GUILayout.BeginHorizontal(NL_Styles.lineB);
            ToggleValueOverride(coverage, out coverage.localVal);
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            if(coverage.prop.floatValue == 1)//coverage switch
            {
                NL_Utilities.BeginUICategory("BASIC SETTINGS", NL_Styles.lineB);

                GUILayout.BeginHorizontal();
                ToggleValueOverride(paintableCoverage, out paintableCoverage.localVal);

                EditorGUI.BeginDisabledGroup(paintableCoverage.prop.floatValue == 0);
                if (GUILayout.Button("Open Total Brush", GUILayout.MaxWidth(120))) NL_TotalBrush.OpenWindowExternal(material.shader.name.Contains("Terrain") ? NL_TotalBrush.Mode.Terrain : NL_TotalBrush.Mode.Mesh);
                EditorGUI.EndDisabledGroup();
                GUILayout.EndHorizontal();
                if (paintableCoverage.prop.floatValue == 1 && m_MaterialEditor.IsInstancingEnabled()) 
                    EditorGUILayout.HelpBox("GPU Instancing is enabled on this material, this will break vertex colors on different objects. Consider disable instancing if you want to use vertex colors.", MessageType.Warning);

                if (terrainHolesTexture == null)
                {
                    GUILayout.BeginHorizontal();
                    ToggleValueOverride(useAveragedNormals, out useAveragedNormals.localVal);

                    EditorGUI.BeginDisabledGroup(useAveragedNormals.prop.floatValue == 0);
                    if (GUILayout.Button("Average Normals", GUILayout.MaxWidth(120))) NL_TotalBrush.AverageNormals();
                    EditorGUI.EndDisabledGroup();
                    GUILayout.EndHorizontal();
                }

                if (terrainHolesTexture != null)
                    ToggleValueOverride(stochastic, out stochastic.localVal);

                TextureValueOverride(coverageTex0, out coverageTex0.localVal);
                FloatValueOverride(coverageTiling, out coverageTiling.localVal);
                ColorValueOverride(coverageColor, out coverageColor.localVal);
                FloatValueOverride(coverageNormalScale0, out coverageNormalScale0.localVal);
                FloatValueOverride(coverageAmount, out coverageAmount.localVal);
                
                if (emissionMasking.prop != null)
                    FloatValueOverride(emissionMasking, out emissionMasking.localVal);
                if (maskCoverageByAlpha.prop != null && (BlendMode)blendMode.floatValue != BlendMode.Opaque)
                    FloatValueOverride(maskCoverageByAlpha, out maskCoverageByAlpha.localVal);
                //FloatValueOverride(baseCoverageNormalsBlend, out baseCoverageNormalsBlend.localVal);
                FloatValueOverride(coverageNormalsOverlay, out coverageNormalsOverlay.localVal);
                NL_Utilities.EndUICategory();

                NL_Utilities.BeginUICategory("AREA MASK", NL_Styles.lineB);
                FloatValueOverride(coverageAreaMaskRange, out coverageAreaMaskRange.localVal);
                FloatValueOverride(coverageAreaBias, out coverageAreaBias.localVal);
                FloatValueOverride(coverageLeakReduction, out coverageLeakReduction.localVal);
                FloatValueOverride(precipitationDirOffset, out precipitationDirOffset.localVal);
                RangeValueOverride(precipitationDirRange, out precipitationDirRange.localVal);
                NL_Utilities.EndUICategory();

                if (material.HasFloat("_TessellationOverride"))
                {
                    NL_Utilities.BeginUICategory("TESSELLATION", NL_Styles.lineB);
                    ToggleValueOverride(tessellation, out tessellation.localVal);

                    if (tessellation.prop.floatValue == 1)
                    {
                        FloatValueOverride(tessEdgeL, out tessEdgeL.localVal);
                        FloatValueOverride(tessFactorSnow, out tessFactorSnow.localVal);
                        RangeValueOverride(tessSnowdriftRange, out tessSnowdriftRange.localVal);
                        FloatValueOverride(tessMaxDisp, out tessMaxDisp.localVal);
                    }
                    NL_Utilities.EndUICategory();
                }

                if (coverageDisplacement != null)
                {
                    NL_Utilities.BeginUICategory("DISPLACEMENT", NL_Styles.lineB);
                    ToggleValueOverride(displacement, out displacement.localVal);
                    if (displacement.prop.floatValue == 1)
                    {
                        FloatValueOverride(heightMap0Contrast, out heightMap0Contrast.localVal);
                        FloatValueOverride(coverageDisplacement, out coverageDisplacement.localVal, new Vector2(0, float.PositiveInfinity));
                        FloatValueOverride(coverageDisplacementOffset, out coverageDisplacementOffset.localVal);
                    }
                    NL_Utilities.EndUICategory();
                }

                if (material.HasFloat("_TracesOverride"))
                {
                    NL_Utilities.BeginUICategory("TRACES", NL_Styles.lineB);
                    ToggleValueOverride(traces, out traces.localVal);
                    if (traces.prop.floatValue == 1)
                    {
                        FloatValueOverride(tracesBlendFactor, out tracesBlendFactor.localVal);
                        FloatValueOverride(tracesNormalScale, out tracesNormalScale.localVal);
                        ToggleValueOverride(traceDetail, out traceDetail.localVal);
                    }
                    NL_Utilities.EndUICategory();
                }

                NL_Utilities.BeginUICategory("BLEND BY NORMALS", NL_Styles.lineB);
                FloatValueOverride(blendByNormalsStrength, out blendByNormalsStrength.localVal, new Vector2(0, float.PositiveInfinity));
                FloatValueOverride(blendByNormalsPower, out blendByNormalsPower.localVal, new Vector2(0, float.PositiveInfinity));
                NL_Utilities.EndUICategory();

                if (distanceFadeStart.prop != null)
                {
                    NL_Utilities.BeginUICategory("DISTANCE FADE", NL_Styles.lineB);
                    FloatValueOverride(distanceFadeStart, out distanceFadeStart.localVal, new Vector2(0, float.PositiveInfinity));
                    FloatValueOverride(distanceFadeFalloff, out distanceFadeFalloff.localVal, new Vector2(0, float.PositiveInfinity));
                    NL_Utilities.EndUICategory();
                }

                NL_Utilities.BeginUICategory("SPARKLE AND SSS", NL_Styles.lineB);
                //EditorGUILayout.HelpBox("To take effect in the Deferred rendering path these two toggles must be on/off together.", MessageType.Info);
                ToggleValueOverride(sparkle, out sparkle.localVal);
                ToggleValueOverride(sss, out sss.localVal);
                NL_Utilities.EndUICategory();
            }//coverage switch
        }
    }

    override public void SetMaterialKeywords(Material material)
    {
        base.SetMaterialKeywords(material);

        if (material.HasProperty("_TessellationOverride") && material.GetFloat("_TessellationOverride") == 1)
        {
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
        }
            
        if (material.HasProperty(displacement.ovrdName) && material.GetFloat(displacement.ovrdName) == 1)
            SetKeyword(material, displacement.keywordName, material.GetFloat(GetPropName(displacement.ovrdName)) == 1);  
        if (material.HasProperty(traces.ovrdName) && material.GetFloat(traces.ovrdName) == 1)
            SetKeyword(material, traces.keywordName, material.GetFloat(GetPropName(traces.ovrdName)) == 1);
        if (material.HasProperty(traceDetail.ovrdName) && material.GetFloat(traceDetail.ovrdName) == 1)
            SetKeyword(material, traceDetail.keywordName, material.GetFloat(GetPropName(traceDetail.ovrdName)) == 1);
        if (material.HasProperty(sparkle.ovrdName) && material.GetFloat(sparkle.ovrdName) == 1)
            SetKeyword(material, sparkle.keywordName, material.GetFloat(GetPropName(sparkle.ovrdName)) == 1);
        if (material.HasProperty(sss.ovrdName) && material.GetFloat(sss.ovrdName) == 1)
            SetKeyword(material, sss.keywordName, material.GetFloat(GetPropName(sss.ovrdName)) == 1);
    }

    override public void ValidateMaterial(Material material)
    {
        SetMaterialKeywords(material);
        SnowCoverage.UpdateMtl(material);
    }
}
#endif
