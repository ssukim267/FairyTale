#if UNITY_EDITOR
namespace NOT_Lonely.Weatherade
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.TestTools;

    [CustomEditor(typeof(SnowCoverage))]
    public class SnowCoverage_editor : Editor
    {
        private SnowCoverage snowCoverage;

        private SerializedProperty depthTextureResolution;
        private SerializedProperty areaSize;
        private SerializedProperty areaDepth;
        private SerializedProperty texVSM;
        private SerializedProperty texBlured;
        private SerializedProperty texRGBA;
        private SerializedProperty depthTexture;
        private SerializedProperty depthCopyMtl;
        private SerializedProperty blurKernelSize;
        private SerializedProperty depthLayerMask;
        private SerializedProperty depthTextureFormat;
        private SerializedProperty coverageAreaFalloffHardness;
        private SerializedProperty coverageAmount;
        private SerializedProperty heightMap0Contrast;
        private SerializedProperty distanceFadeStart;
        private SerializedProperty distanceFadeFalloff;

        //Traces
        private SerializedProperty traces;
        private SerializedProperty tracesNormalScale;
        private SerializedProperty tracesBlendFactor;
        private SerializedProperty tracesColor;
        private SerializedProperty tracesColorBlendRange;
        private SerializedProperty traceDetail;
        private SerializedProperty traceDetailTex;
        private SerializedProperty traceDetailTiling;
        private SerializedProperty traceDetailNormalScale;
        private SerializedProperty traceDetailIntensity;

        //Tessellation
        private SerializedProperty tessellation;
        private SerializedProperty tessEdgeL;
        private SerializedProperty tessFactorSnow;
        private SerializedProperty tessMaxDisp;
        private SerializedProperty tessSnowdriftRange;

        private SerializedProperty coverage;
        private SerializedProperty paintableCoverage;
        private SerializedProperty stochastic;
        private SerializedProperty useAveragedNormals;
        private SerializedProperty coverageTex0;
        private SerializedProperty coverageColor;
        private SerializedProperty emissionMasking;
        private SerializedProperty maskByAlpha;

        private SerializedProperty coverageNormalScale0;
        private SerializedProperty coverageNormalsOverlay;
        //private SerializedProperty baseCoverageNormalsBlend;
        private SerializedProperty coverageTiling;
        private SerializedProperty coverageAreaBias;
        private SerializedProperty coverageLeakReduction;
        private SerializedProperty coverageAreaMaskRange;
        private SerializedProperty precipitationDirOffset;
        private SerializedProperty precipitationDirRange;
        private SerializedProperty blendByNormalsStrength;
        private SerializedProperty blendByNormalsPower;

        //Displacement
        private SerializedProperty displacement;
        private SerializedProperty coverageDisplacement;
        private SerializedProperty coverageDisplacementOffset;

        //Sparkle and SSS
        private SerializedProperty sss;
        private SerializedProperty sss_intensity;
        private SerializedProperty sparkle;
        private SerializedProperty sparkleTex;
        private SerializedProperty sparklesAmount;
        private SerializedProperty sparkleDistFalloff;
        private SerializedProperty sparkleTexSS;
        private SerializedProperty sparkleTexLS;
        private SerializedProperty localSparkleTiling;
        private SerializedProperty screenSpaceSparklesTiling; 
        private SerializedProperty sparklesBrightness;
        private SerializedProperty sparklesLightmapMaskPower;
        private SerializedProperty sparklesHighlightMaskExpansion;

        //private SerializedProperty snowAOIntensity;

        private SerializedProperty useFollowTarget;
        private SerializedProperty followTarget;
        private SerializedProperty targetPositionOffsetY;
        private SerializedProperty updateRate;
        private SerializedProperty updateDistanceThreshold;

        private SerializedProperty depthTexProps;
        private SerializedProperty surfaceProps;
        private SerializedProperty basicSettingsFoldout;
        private SerializedProperty areaMaskFoldout;
        private SerializedProperty tessellationFoldout;
        private SerializedProperty displacementFoldout;
        private SerializedProperty tracesFoldout;
        private SerializedProperty blendByNormalFoldout;
        private SerializedProperty distanceFadeFoldout;
        private SerializedProperty sparklesFoldout;

        private GUIContent areaSizeLabel = new GUIContent("Horizontal Size", "The local horizontal size of the snow/rain area.");
        private GUIContent areaDepthLabel = new GUIContent("Depth", "The local depth of the snow/rain area.");
        private GUIContent depthLayerMaskLabel = new GUIContent("Affected Layers", "Objects on these layers will be visible to the Weatherade Coverage Instance, when it builds the coverage mask.");
        private GUIContent depthTextureFormatLabel = new GUIContent("Depth Texture Format", "RGHalf is the most cheap option, but can provide banding artifacts at the coverage transitions. " +
            "RGBA options are needed if you want to use the SRS Particle System with the collision mode set to Global.");
        private GUIContent depthTextureResolutionLabel = new GUIContent("Depth Texture Resolution", "The depth texture resolution affects the quality of the effect.");
        private GUIContent blurKernelSizeLabel = new GUIContent("Blur Kernel Size", "The larger the value, the more blur will be applied to the coverage mask. It also affects performance accordingly.");
        private GUIContent coverageAreaFalloffHardnessLabel = new GUIContent("Area Falloff Hardness", "How hard the coverage area border will be. Values lower than 1 will add a gradient from the area center to the borders.");
        private GUIContent useFollowTargetLabel = new GUIContent(
            "Follow Target", "Use an object that this coverage area will follow." +
            "\nIf the object is not specified, then the first found camera with the 'MainCamera' tag will be used." +
            "\nThe coverage area will remain in place if the checkbox is disabled, or the object is not specified and the camera is not found.");
        private GUIContent followTargetLabel = new GUIContent("Follow Target", "The object that this coverage area will follow. " +
            "If the object is not specified, then a first found camera with the 'MainCamera' tag will be used." +
            "If no camera found, the coverage area will remain in place.");
        private GUIContent targetPosOffsetYLabel = new GUIContent("Offset", "The position offset from the 'Follow Target'.");
        private GUIContent updateRateLabel = new GUIContent("Check Interval", "Interval in seconds between 'Follow Target' and volume distance checks. If set to 0, then the check will be performed every frame.");
        private GUIContent updateDistanceThresholdLabel = new GUIContent("Distance Threshold", 
            "How far the 'Follow Target' object must move from the center of the volume (including 'Offset') to update the volume's position. " +
            "Example: 0 - update every 'Check Interval', 0.5 - update, when the 'Follow Target' is halfway from the volume's center.");
        private Texture2D cover;

        private void OnEnable()
        {
            texVSM = serializedObject.FindProperty("texVSM");
            depthTexture = serializedObject.FindProperty("depthTexture");
            depthCopyMtl = serializedObject.FindProperty("depthCopyMtl");
            texBlured = serializedObject.FindProperty("texBlured");
            texRGBA = serializedObject.FindProperty("texRGBA");

            coverageAmount = serializedObject.FindProperty("coverageAmount");
            heightMap0Contrast = serializedObject.FindProperty("heightMap0Contrast");
            paintableCoverage = serializedObject.FindProperty("paintableCoverage");
            sss = serializedObject.FindProperty("sss");
            sss_intensity = serializedObject.FindProperty("sss_intensity");

            //Traces
            traces = serializedObject.FindProperty("traces");
            tracesNormalScale = serializedObject.FindProperty("tracesNormalScale");
            tracesBlendFactor = serializedObject.FindProperty("tracesBlendFactor");
            tracesColor = serializedObject.FindProperty("tracesColor");
            tracesColorBlendRange = serializedObject.FindProperty("tracesColorBlendRange");
            traceDetail = serializedObject.FindProperty("traceDetail");
            traceDetailTex = serializedObject.FindProperty("traceDetailTex");
            traceDetailTiling = serializedObject.FindProperty("traceDetailTiling");
            traceDetailNormalScale = serializedObject.FindProperty("traceDetailNormalScale");
            traceDetailIntensity = serializedObject.FindProperty("traceDetailIntensity");

            //Tessellation
            tessellation = serializedObject.FindProperty("tessellation");
            tessEdgeL = serializedObject.FindProperty("tessEdgeL");
            tessFactorSnow = serializedObject.FindProperty("tessFactorSnow");
            tessMaxDisp = serializedObject.FindProperty("tessMaxDisp");
            tessSnowdriftRange = serializedObject.FindProperty("tessSnowdriftRange");

            distanceFadeStart = serializedObject.FindProperty("distanceFadeStart");
            distanceFadeFalloff = serializedObject.FindProperty("distanceFadeFalloff");
            stochastic = serializedObject.FindProperty("stochastic");
            coverage = serializedObject.FindProperty("coverage");
            coverageTex0 = serializedObject.FindProperty("coverageTex0");
            useAveragedNormals = serializedObject.FindProperty("useAveragedNormals");
            coverageColor = serializedObject.FindProperty("coverageColor");
            emissionMasking = serializedObject.FindProperty("emissionMasking");
            //maskByAlpha = serializedObject.FindProperty("maskByAlpha");

            coverageNormalScale0 = serializedObject.FindProperty("coverageNormalScale0");
            coverageNormalsOverlay = serializedObject.FindProperty("coverageNormalsOverlay");
            //baseCoverageNormalsBlend = serializedObject.FindProperty("baseCoverageNormalsBlend");
            coverageTiling = serializedObject.FindProperty("coverageTiling");
            coverageAreaBias = serializedObject.FindProperty("coverageAreaBias");
            coverageLeakReduction = serializedObject.FindProperty("coverageLeakReduction");
            coverageAreaMaskRange = serializedObject.FindProperty("coverageAreaMaskRange");
            precipitationDirOffset = serializedObject.FindProperty("precipitationDirOffset");
            precipitationDirRange = serializedObject.FindProperty("precipitationDirRange");
            blendByNormalsStrength = serializedObject.FindProperty("blendByNormalsStrength");
            blendByNormalsPower = serializedObject.FindProperty("blendByNormalsPower");

            displacement = serializedObject.FindProperty("displacement");
            coverageDisplacement = serializedObject.FindProperty("coverageDisplacement");
            coverageDisplacementOffset = serializedObject.FindProperty("coverageDisplacementOffset");
           

            //Sparkle
            sparkle = serializedObject.FindProperty("sparkle");
            sparkleTex = serializedObject.FindProperty("sparkleTex");
            sparklesAmount = serializedObject.FindProperty("sparklesAmount");
            sparkleDistFalloff = serializedObject.FindProperty("sparkleDistFalloff");
            sparkleTexSS = serializedObject.FindProperty("sparkleTexSS");
            sparkleTexLS = serializedObject.FindProperty("sparkleTexLS");
            localSparkleTiling = serializedObject.FindProperty("localSparkleTiling");
            screenSpaceSparklesTiling = serializedObject.FindProperty("screenSpaceSparklesTiling");
            sparklesBrightness = serializedObject.FindProperty("sparklesBrightness");

            sparklesLightmapMaskPower = serializedObject.FindProperty("sparklesLightmapMaskPower");
            sparklesHighlightMaskExpansion = serializedObject.FindProperty("sparklesHighlightMaskExpansion");

            //snowAOIntensity = serializedObject.FindProperty("snowAOIntensity");

            areaSize = serializedObject.FindProperty("areaSize");
            areaDepth = serializedObject.FindProperty("areaDepth");
            depthLayerMask = serializedObject.FindProperty("depthLayerMask");
            depthTextureFormat = serializedObject.FindProperty("depthTextureFormat");
            depthTextureResolution = serializedObject.FindProperty("depthTextureResolution");
            blurKernelSize = serializedObject.FindProperty("blurKernelSize");
            coverageAreaFalloffHardness = serializedObject.FindProperty("coverageAreaFalloffHardness");
            useFollowTarget = serializedObject.FindProperty("useFollowTarget");
            followTarget = serializedObject.FindProperty("followTarget");
            targetPositionOffsetY = serializedObject.FindProperty("targetPositionOffsetY");
            updateRate = serializedObject.FindProperty("updateRate");
            updateDistanceThreshold = serializedObject.FindProperty("updateDistanceThreshold");

            depthTexProps = serializedObject.FindProperty("depthTexProps");
            surfaceProps = serializedObject.FindProperty("surfaceProps");
            basicSettingsFoldout = serializedObject.FindProperty("basicSettingsFoldout");
            areaMaskFoldout = serializedObject.FindProperty("areaMaskFoldout");
            tessellationFoldout = serializedObject.FindProperty("tessellationFoldout");
            displacementFoldout = serializedObject.FindProperty("displacementFoldout");
            tracesFoldout = serializedObject.FindProperty("tracesFoldout");
            blendByNormalFoldout = serializedObject.FindProperty("blendByNormalFoldout");
            distanceFadeFoldout = serializedObject.FindProperty("distanceFadeFoldout");
            sparklesFoldout = serializedObject.FindProperty("sparklesFoldout");

            cover = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/NOT_Lonely/Weatherade SRS/UI/SnowCover.png", typeof(Texture2D));

            SceneView.duringSceneGui += SceneViewHandles;
            Undo.undoRedoPerformed += OnUndoRedoCallback;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui += SceneViewHandles;
            Undo.undoRedoPerformed -= OnUndoRedoCallback;
        }

        void OnUndoRedoCallback()
        {
            if (snowCoverage != null) snowCoverage.ValidateValues();
        }

        public override void OnInspectorGUI()
        {
            snowCoverage = target as SnowCoverage;
            if (NL_Styles.lineB == null || NL_Styles.lineB.normal.background == null) NL_Styles.GetStyles();

            EditorGUI.BeginChangeCheck();

            float inspectorWidth = EditorGUIUtility.currentViewWidth;
            float imageWidth = inspectorWidth - 40;
            float imageHeight = imageWidth * cover.height / cover.width;
            Rect rect = GUILayoutUtility.GetRect(imageWidth, imageHeight);
            GUI.DrawTexture(rect, cover, ScaleMode.ScaleToFit);

            NL_Utilities.DrawCenteredBoldHeader("WEATHERADE SNOW COVERAGE");

            float currentInspectorWidth = EditorGUIUtility.currentViewWidth - 24;
            float offset = currentInspectorWidth - (currentInspectorWidth / 1.6f);

            if (NL_Utilities.DrawFoldout(depthTexProps, "MAIN SETTINGS"))
            {
                GUILayout.BeginVertical(NL_Styles.lineA);
                EditorGUILayout.PropertyField(areaSize, areaSizeLabel);
                GUILayout.EndVertical();

                GUILayout.BeginVertical(NL_Styles.lineB);
                EditorGUILayout.PropertyField(areaDepth, areaDepthLabel);
                GUILayout.EndVertical();

                GUILayout.BeginVertical(NL_Styles.lineA);
                EditorGUILayout.PropertyField(depthLayerMask, depthLayerMaskLabel);
                GUILayout.EndVertical();

                GUILayout.BeginVertical(NL_Styles.lineB);
                EditorGUILayout.PropertyField(depthTextureFormat, depthTextureFormatLabel);
                GUILayout.EndVertical();

                GUILayout.BeginVertical(NL_Styles.lineA);
                EditorGUILayout.PropertyField(depthTextureResolution, depthTextureResolutionLabel);
                GUILayout.EndVertical();

                GUILayout.BeginVertical(NL_Styles.lineB);
                EditorGUILayout.PropertyField(blurKernelSize, blurKernelSizeLabel);
                GUILayout.EndVertical();

                GUILayout.BeginVertical(NL_Styles.lineA);
                EditorGUILayout.PropertyField(coverageAreaFalloffHardness, coverageAreaFalloffHardnessLabel);
                GUILayout.EndVertical();

                //Follow target
                GUILayout.BeginHorizontal(NL_Styles.lineB);
                EditorGUILayout.PropertyField(useFollowTarget, useFollowTargetLabel);
                EditorGUILayout.PropertyField(followTarget, new GUIContent());
                GUILayout.EndHorizontal();

                GUILayout.BeginVertical(NL_Styles.lineB);
                if (useFollowTarget.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(targetPositionOffsetY, targetPosOffsetYLabel);
                    EditorGUILayout.PropertyField(updateRate, updateRateLabel);
                    EditorGUILayout.PropertyField(updateDistanceThreshold, updateDistanceThresholdLabel);
                    EditorGUI.indentLevel--;
                }

                GUILayout.EndVertical();

                GUILayout.Space(1);
                if (GUILayout.Button(new GUIContent("Update", "Press this button if you changed one of the following properties: 'Affected Layers', 'Depth Texture Format', 'Depth Texture Resolution', 'Blur Kernel Size'.")))
                {
                    snowCoverage.Init();
                    EditorUtility.SetDirty(snowCoverage);
                }

                GUILayout.Space(10);
            }

            GUILayout.Space(1);

            if (NL_Utilities.DrawFoldout(surfaceProps, "GLOBAL SURFACE SETTINGS"))
            {
                GUILayout.BeginHorizontal(NL_Styles.lineB);
                EditorGUILayout.PropertyField(coverage, NL_Styles.coverageText);
                GUILayout.EndHorizontal();

                NL_Utilities.BeginUICategory("BASIC SETTINGS", NL_Styles.lineB, basicSettingsFoldout);
                if (basicSettingsFoldout.boolValue)
                {
                    EditorGUILayout.PropertyField(paintableCoverage, NL_Styles.paintableCoverageText);
                    EditorGUILayout.PropertyField(useAveragedNormals, NL_Styles.useAveragedNormalsText);
                    EditorGUILayout.PropertyField(stochastic, NL_Styles.stochasticText);
                    EditorGUILayout.PropertyField(coverageTex0, NL_Styles.coverageTex0Text);
                    EditorGUILayout.PropertyField(coverageTiling, NL_Styles.coverageTilingText);
                    EditorGUILayout.PropertyField(coverageColor, NL_Styles.coverageColorText);
                    EditorGUILayout.PropertyField(coverageNormalScale0, NL_Styles.coverageNormalScale0Text);
                    EditorGUILayout.PropertyField(coverageAmount, NL_Styles.coverageAmountText);
                    EditorGUILayout.PropertyField(emissionMasking, NL_Styles.emissionMaskingText);
                    EditorGUILayout.PropertyField(coverageNormalsOverlay, NL_Styles.coverageNormalsOverlayText);
                    
                }
                NL_Utilities.EndUICategory();

                NL_Utilities.BeginUICategory("AREA MASK", NL_Styles.lineB, areaMaskFoldout);
                if (areaMaskFoldout.boolValue)
                {
                    //coverageAreaMaskRange.vector2Value = DrawRangeSlider(coverageAreaMaskRange.vector2Value, NL_Styles.coverageAreaMaskRangeText, offset);
                    EditorGUILayout.PropertyField(coverageAreaMaskRange, NL_Styles.coverageAreaMaskRangeText);
                    EditorGUILayout.PropertyField(coverageAreaBias, NL_Styles.coverageAreaBiasText);
                    EditorGUILayout.PropertyField(coverageLeakReduction, NL_Styles.coverageLeakReductionText);
                    EditorGUILayout.PropertyField(precipitationDirOffset, NL_Styles.precipitationDirOffsetText);
                    precipitationDirRange.vector2Value = DrawRangeSlider(precipitationDirRange.vector2Value, NL_Styles.precipitationDirRangeText, offset);
                }
                NL_Utilities.EndUICategory();

                NL_Utilities.BeginUICategory("TESSELLATION", NL_Styles.lineB, tessellationFoldout);
                if (tessellationFoldout.boolValue)
                {
                    EditorGUILayout.PropertyField(tessellation, NL_Styles.tessellationText);
                    EditorGUILayout.PropertyField(tessEdgeL, NL_Styles.tessEdgeLText);
                    EditorGUILayout.PropertyField(tessFactorSnow, NL_Styles.tessFactorSnowText);
                    tessSnowdriftRange.vector2Value = DrawRangeSlider(tessSnowdriftRange.vector2Value, NL_Styles.tessSnowdriftRangeText, offset);
                    EditorGUILayout.PropertyField(tessMaxDisp, NL_Styles.tessMaxDispText);
                }
                NL_Utilities.EndUICategory();

                NL_Utilities.BeginUICategory("DISPLACEMENT", NL_Styles.lineB, displacementFoldout);
                if (displacementFoldout.boolValue)
                {
                    EditorGUILayout.PropertyField(displacement, NL_Styles.displacementText);
                    EditorGUILayout.PropertyField(heightMap0Contrast, NL_Styles.heightMap0ContrastText);
                    EditorGUILayout.PropertyField(coverageDisplacement, NL_Styles.coverageDisplacementText);
                    EditorGUILayout.PropertyField(coverageDisplacementOffset, NL_Styles.coverageDisplacementOffsetText);
                }
                NL_Utilities.EndUICategory();

                NL_Utilities.BeginUICategory("TRACES", NL_Styles.lineB, tracesFoldout);
                if (tracesFoldout.boolValue)
                {
                    if (!snowCoverage.hasTracesComponent)
                    {
                        EditorGUILayout.HelpBox("There's no SRS_TracesRenderer component on this gameobject. Please add one to use this feature.", MessageType.Warning);
                        if(GUILayout.Button("Add Now")) snowCoverage.gameObject.AddComponent<SRS_TraceMaskGenerator>();
                    }
                    EditorGUILayout.PropertyField(traces, NL_Styles.tracesText);
                    /*
                    if (traceDetailTex.objectReferenceValue == null) traceDetail.boolValue = false;
                    else traceDetail.boolValue = true;
                    */
                    
                    EditorGUILayout.PropertyField(tracesColor, NL_Styles.tracesColorText);
                    EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(tracesBlendFactor, NL_Styles.tracesBlendFactorText);
                        tracesColorBlendRange.vector2Value = DrawRangeSlider(tracesColorBlendRange.vector2Value, NL_Styles.tracesColorBlendRangeText, offset);
                    EditorGUI.indentLevel--;
                    EditorGUILayout.PropertyField(tracesNormalScale, NL_Styles.tracesNormalScaleText);

                    EditorGUILayout.PropertyField(traceDetail, NL_Styles.traceDetailText);
                    EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(traceDetailTex, new GUIContent("Detail Texture"));
                        EditorGUILayout.PropertyField(traceDetailTiling, new GUIContent("Tiling"));
                        EditorGUILayout.PropertyField(traceDetailNormalScale, new GUIContent("Normal Scale"));
                        EditorGUILayout.PropertyField(traceDetailIntensity, new GUIContent("Details Intensity"));
                    EditorGUI.indentLevel--;   
                }
                NL_Utilities.EndUICategory();

                NL_Utilities.BeginUICategory("BLEND BY NORMALS", NL_Styles.lineB, blendByNormalFoldout);
                if (blendByNormalFoldout.boolValue)
                {
                    EditorGUILayout.PropertyField(blendByNormalsStrength, NL_Styles.blendByNormalsStrengthText);
                    EditorGUILayout.PropertyField(blendByNormalsPower, NL_Styles.blendByNormalsPowerText);
                }
                NL_Utilities.EndUICategory();

                NL_Utilities.BeginUICategory("DISTANCE FADE", NL_Styles.lineB, distanceFadeFoldout);
                if (distanceFadeFoldout.boolValue)
                {
                    EditorGUILayout.PropertyField(distanceFadeStart, NL_Styles.distanceFadeStartText);
                    EditorGUILayout.PropertyField(distanceFadeFalloff, NL_Styles.distanceFadeFalloffText);
                }
                NL_Utilities.EndUICategory();

                NL_Utilities.BeginUICategory("SPARKLE AND SSS", NL_Styles.lineB, sparklesFoldout);
                if (sparklesFoldout.boolValue)
                {
                    EditorGUILayout.PropertyField(sss, NL_Styles.sssText);
                    EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(sss_intensity, NL_Styles.sss_intensityText);
                    EditorGUI.indentLevel--;

                    EditorGUILayout.PropertyField(sparkle, NL_Styles.sparkleText);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(sparkleTex, NL_Styles.sparkleTexText);
                    EditorGUILayout.PropertyField(sparklesAmount, NL_Styles.sparklesAmountText);
                    EditorGUILayout.PropertyField(sparklesBrightness, NL_Styles.sparklesBrightnessText);
                    EditorGUILayout.PropertyField(sparkleDistFalloff, NL_Styles.sparkleDistFalloffText);
                    if (sparkleTex.objectReferenceValue == null && sparkleTexLS.enumValueIndex == 1)
                        EditorGUILayout.HelpBox("'Sparkle Mask' texure is not provided. Set it or consider using 'Main Coverage Tex Alpha'.", MessageType.Warning);
                    EditorGUILayout.PropertyField(sparkleTexLS, NL_Styles.sparkleTexLSText);
                    EditorGUI.BeginDisabledGroup(sparkleTexLS.enumValueIndex == 0);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(localSparkleTiling, NL_Styles.localSparkleTilingText);
                    EditorGUI.indentLevel--;
                    EditorGUI.EndDisabledGroup();
                    if (sparkleTex.objectReferenceValue == null && sparkleTexSS.enumValueIndex == 1)
                        EditorGUILayout.HelpBox("'Sparkle Mask' texure is not provided. Set it or consider using 'Main Coverage Tex Alpha'.", MessageType.Warning);
                    EditorGUILayout.PropertyField(sparkleTexSS, NL_Styles.sparkleTexSSText);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(screenSpaceSparklesTiling, NL_Styles.screenSpaceSparklesTilingText);
                    EditorGUI.indentLevel--;

                    EditorGUILayout.PropertyField(sparklesHighlightMaskExpansion, NL_Styles.sparkleHighlightMaskExpText);
                    EditorGUILayout.PropertyField(sparklesLightmapMaskPower, NL_Styles.sparkleLightmapMaskPowText);

                    EditorGUI.indentLevel--;
                }
                NL_Utilities.EndUICategory();
            }

            Undo.RecordObject(snowCoverage, "Weatherade Coverage Value Changed");

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                snowCoverage.ValidateValues();

                snowCoverage.CalculateTargetPosOffsetsXZ();

                if (!Application.isPlaying)
                {
                    if (followTarget.objectReferenceValue != null && (followTarget.objectReferenceValue != lastFollowTarget || snowCoverage.targetPosOffset != lastOffset)) snowCoverage.UpdateSRSPosition();

                    lastFollowTarget = followTarget.objectReferenceValue as Transform;
                    lastOffset = snowCoverage.targetPosOffset;
                }
            }
        }

        private Vector2 DrawRangeSlider(Vector2 sliderMinMax, GUIContent label, float labelOffset, float minLimit = 0, float maxLimit = 1)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            GUILayout.Space(2);
            sliderMinMax.x = EditorGUILayout.FloatField(sliderMinMax.x, GUILayout.MaxWidth(50));
            EditorGUILayout.MinMaxSlider(ref sliderMinMax.x, ref sliderMinMax.y, minLimit, maxLimit);
            sliderMinMax.y = EditorGUILayout.FloatField(sliderMinMax.y, GUILayout.MaxWidth(50));

            GUILayout.EndHorizontal();

            return sliderMinMax;
        }

        private Vector3 lastOffset;
        private Transform lastFollowTarget;
        private void SceneViewHandles(SceneView sceneView)
        {


            //snowCoverage.UpdateSRSPosition();
        }

        private void OnSceneGUI()
        {
            if (snowCoverage == null || !snowCoverage.enabled || !Selection.Contains(snowCoverage.gameObject)) return;

        }

    }
}
#endif
