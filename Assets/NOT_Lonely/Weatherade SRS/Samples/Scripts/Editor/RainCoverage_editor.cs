#if UNITY_EDITOR
namespace NOT_Lonely.Weatherade
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Rendering;
    using UnityEngine.TestTools;

    [CustomEditor(typeof(RainCoverage))]
    public class RainCoverage_editor : Editor
    {
        private RainCoverage rainCoverage;

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
        private SerializedProperty useFollowTarget;
        private SerializedProperty followTarget;
        private SerializedProperty targetPositionOffsetY;
        private SerializedProperty updateRate;
        private SerializedProperty updateDistanceThreshold;
        private SerializedProperty coverage;
        private SerializedProperty primaryMasks;
        private SerializedProperty ripplesTex;
        private SerializedProperty stochastic;
        private SerializedProperty paintableCoverage;
        private SerializedProperty wetColor;
        private SerializedProperty wetnessAmount;
        private SerializedProperty puddlesAmount;
        private SerializedProperty puddlesMult;
        private SerializedProperty puddlesRange;
        private SerializedProperty puddlesTiling;
        private SerializedProperty puddlesSlope;
        private SerializedProperty ripples;
        private SerializedProperty ripplesAmount;
        private SerializedProperty ripplesIntensity;
        private SerializedProperty ripplesTiling;
        private SerializedProperty ripplesFPS;
        private SerializedProperty spotsIntensity;
        private SerializedProperty spotsAmount;
        private SerializedProperty drips;
        private SerializedProperty dripsIntensity;
        private SerializedProperty dripsSpeed;
        private SerializedProperty dripsTiling;
        private SerializedProperty distortionTiling;
        private SerializedProperty distortionAmount;

        private SerializedProperty distanceFadeStart;
        private SerializedProperty distanceFadeFalloff;

        private SerializedProperty coverageAreaBias;
        private SerializedProperty coverageLeakReduction;
        private SerializedProperty coverageAreaMaskRange;
        private SerializedProperty precipitationDirOffset;
        private SerializedProperty precipitationDirRange;
        private SerializedProperty blendByNormalsStrength;
        private SerializedProperty blendByNormalsPower;

        private SerializedProperty depthTexProps;
        private SerializedProperty surfaceProps;
        private SerializedProperty textureMasksFoldout;
        private SerializedProperty areaMaskFoldout;
        private SerializedProperty wetnessFoldout;
        private SerializedProperty puddlesFoldout;
        private SerializedProperty dripsFoldout;
        private SerializedProperty ripplesAndSpotsFoldout;
        private SerializedProperty blendByNormalFoldout;
        private SerializedProperty distanceFadeFoldout;
        
        private GUIContent areaSizeLabel = new GUIContent("Horizontal Size", "The local horizontal size of the snow/rain area.");
        private GUIContent areaDepthLabel = new GUIContent("Depth", "The local depth of the snow/rain area.");
        private GUIContent depthLayerMaskLabel = new GUIContent("Affected Layers", "Objects on these layers will be visible to the Weatherade Coverage Instance, when it builds the coverage mask.");
        private GUIContent depthTextureFormatLabel = new GUIContent("Depth Texture Format", "The texture channels format.");
        private GUIContent depthTextureResolutionLabel = new GUIContent("Depth Texture Resolution", "The depth texture resolution affects the quality of the effect.");
        private GUIContent blurKernelSizeLabel = new GUIContent("Blur Kernel Size", "The larger the value, the more blur will be applied to the coverage mask. It also affects performance accordingly.");
        private GUIContent coverageAreaFalloffHardnessLabel = new GUIContent("Area Falloff Hardness", "How hard the coverage area border will be. Values lower than 1 will add a gradient from the area center to the borders.");
        private GUIContent useFollowTargetLabel = new GUIContent(
            "Follow Target", "Use an object that this coverage area will follow." +
            "\nIf the object is not specified, then the first found camera with the 'MainCamera' tag will be used." +
            "\nThe coverage area will remain in place if the checkbox is disabled, or the object is not specified and the camera is not found.");
        private GUIContent followTargetLabel = new GUIContent("Follow Target", "The object that this coverage area will follow. If the object is not specified, then this coverage area will remain in place.");
        private GUIContent targetPosOffsetYLabel = new GUIContent("Offset", "The position offset from the 'Follow Target'.");
        private GUIContent updateRateLabel = new GUIContent("Check Interval", "Interval in seconds between 'Follow Target' and volume distance checks. If set to 0, then the check will be performed every frame.");
        private GUIContent updateDistanceThresholdLabel = new GUIContent("Distance Threshold", "How far the 'Follow Target' object must move from the center of the volume (including 'Offset') to update the volume's position. Example: 0 - update every 'Check Interval', 0.5 - update, when the 'Follow Target' is halfway from the volume's center.");

        private Texture2D cover;
        private Vector3 lastOffset;
        private Transform lastFollowTarget;

        private void OnEnable()
        {
            texVSM = serializedObject.FindProperty("texVSM");
            depthTexture = serializedObject.FindProperty("depthTexture");
            depthCopyMtl = serializedObject.FindProperty("depthCopyMtl");
            texBlured = serializedObject.FindProperty("texBlured");
            texRGBA = serializedObject.FindProperty("texRGBA");

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

            coverage = serializedObject.FindProperty("coverage");
            primaryMasks = serializedObject.FindProperty("primaryMasks");
            ripples = serializedObject.FindProperty("ripples");
            ripplesTex = serializedObject.FindProperty("ripplesTex");
            stochastic = serializedObject.FindProperty("stochastic");
            paintableCoverage = serializedObject.FindProperty("paintableCoverage");
            wetColor = serializedObject.FindProperty("wetColor");
            puddlesAmount = serializedObject.FindProperty("puddlesAmount");
            puddlesMult = serializedObject.FindProperty("puddlesMult");
            wetnessAmount = serializedObject.FindProperty("wetnessAmount");
            puddlesRange = serializedObject.FindProperty("puddlesRange");
            puddlesTiling = serializedObject.FindProperty("puddlesTiling");
            puddlesSlope = serializedObject.FindProperty("puddlesSlope");
            ripplesAmount = serializedObject.FindProperty("ripplesAmount");
            ripplesIntensity = serializedObject.FindProperty("ripplesIntensity");
            ripplesTiling = serializedObject.FindProperty("ripplesTiling");
            ripplesFPS = serializedObject.FindProperty("ripplesFPS");
            spotsIntensity = serializedObject.FindProperty("spotsIntensity");
            spotsAmount = serializedObject.FindProperty("spotsAmount");
            drips = serializedObject.FindProperty("drips");
            dripsIntensity = serializedObject.FindProperty("dripsIntensity");
            dripsSpeed = serializedObject.FindProperty("dripsSpeed");
            dripsTiling = serializedObject.FindProperty("dripsTiling");
            distortionTiling = serializedObject.FindProperty("distortionTiling");
            distortionAmount = serializedObject.FindProperty("distortionAmount");

            distanceFadeStart = serializedObject.FindProperty("distanceFadeStart");
            distanceFadeFalloff = serializedObject.FindProperty("distanceFadeFalloff");
            coverageAreaBias = serializedObject.FindProperty("coverageAreaBias");
            coverageLeakReduction = serializedObject.FindProperty("coverageLeakReduction");
            coverageAreaMaskRange = serializedObject.FindProperty("coverageAreaMaskRange");
            precipitationDirOffset = serializedObject.FindProperty("precipitationDirOffset");
            precipitationDirRange = serializedObject.FindProperty("precipitationDirRange");
            blendByNormalsStrength = serializedObject.FindProperty("blendByNormalsStrength");
            blendByNormalsPower = serializedObject.FindProperty("blendByNormalsPower");

            depthTexProps = serializedObject.FindProperty("depthTexProps");
            surfaceProps = serializedObject.FindProperty("surfaceProps");
            textureMasksFoldout = serializedObject.FindProperty("textureMasksFoldout");
            areaMaskFoldout = serializedObject.FindProperty("areaMaskFoldout");
            wetnessFoldout = serializedObject.FindProperty("wetnessFoldout");
            puddlesFoldout = serializedObject.FindProperty("puddlesFoldout");
            dripsFoldout = serializedObject.FindProperty("dripsFoldout");
            blendByNormalFoldout = serializedObject.FindProperty("blendByNormalFoldout");
            distanceFadeFoldout = serializedObject.FindProperty("distanceFadeFoldout");
            ripplesAndSpotsFoldout = serializedObject.FindProperty("ripplesAndSpotsFoldout");

            cover = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/NOT_Lonely/Weatherade SRS/UI/RainCover.png", typeof(Texture2D));

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
            if (rainCoverage != null) rainCoverage.ValidateValues();
        }

        public override void OnInspectorGUI()
        {
            rainCoverage = target as RainCoverage;
            if (NL_Styles.lineB == null || NL_Styles.lineB.normal.background == null) NL_Styles.GetStyles();

            EditorGUI.BeginChangeCheck();
            GUILayout.Space(5);

            float inspectorWidth = EditorGUIUtility.currentViewWidth;
            float imageWidth = inspectorWidth - 40;
            float imageHeight = imageWidth * cover.height / cover.width;
            Rect rect = GUILayoutUtility.GetRect(imageWidth, imageHeight);
            GUI.DrawTexture(rect, cover, ScaleMode.ScaleToFit);

            NL_Utilities.DrawCenteredBoldHeader("WEATHERADE RAIN COVERAGE");

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
                    rainCoverage.Init();
                    EditorUtility.SetDirty(rainCoverage);
                }

                GUILayout.Space(10);
            }

            GUILayout.Space(1);

            if (NL_Utilities.DrawFoldout(surfaceProps, "GLOBAL SURFACE SETTINGS"))
            {
                EditorGUILayout.PropertyField(coverage, NL_Styles.coverageText);

                NL_Utilities.BeginUICategory("MASKS", NL_Styles.lineB, textureMasksFoldout);
                if (textureMasksFoldout.boolValue)
                {
                    EditorGUILayout.PropertyField(paintableCoverage, NL_Styles.paintableWetnessText);
                    EditorGUILayout.PropertyField(primaryMasks, NL_Styles.primaryMasksText);
                    EditorGUILayout.PropertyField(ripplesTex, NL_Styles.ripplesTexText);
                    EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(stochastic, NL_Styles.stochasticText);
                    EditorGUI.indentLevel--;
                }
                NL_Utilities.EndUICategory();

                NL_Utilities.BeginUICategory("WETNESS", NL_Styles.lineB, wetnessFoldout);
                if (wetnessFoldout.boolValue)
                {
                    EditorGUILayout.PropertyField(wetColor, NL_Styles.wetColorText);
                    EditorGUILayout.PropertyField(wetnessAmount, NL_Styles.wetnessAmountText);
                }
                NL_Utilities.EndUICategory();

                NL_Utilities.BeginUICategory("PUDDLES", NL_Styles.lineB, puddlesFoldout);
                if (puddlesFoldout.boolValue)
                {
                    EditorGUILayout.PropertyField(puddlesAmount, NL_Styles.puddlesAmountText);
                    EditorGUILayout.PropertyField(puddlesMult, NL_Styles.puddlesMultText);
                    puddlesRange.vector2Value = DrawRangeSlider(puddlesRange.vector2Value, NL_Styles.puddlesRangeText, offset);
                    EditorGUILayout.PropertyField(puddlesTiling, NL_Styles.puddlesTilingText);
                    EditorGUILayout.PropertyField(puddlesSlope, NL_Styles.puddlesSlopeText);
                }
                NL_Utilities.EndUICategory();

                NL_Utilities.BeginUICategory("RIPPLES AND SPOTS", NL_Styles.lineB, ripplesAndSpotsFoldout);
                if (ripplesAndSpotsFoldout.boolValue)
                {
                    EditorGUILayout.PropertyField(ripples, NL_Styles.ripplesText);
                    EditorGUILayout.PropertyField(ripplesAmount, NL_Styles.ripplesAmountText);  
                    EditorGUILayout.PropertyField(ripplesIntensity, NL_Styles.ripplesIntensityText);
                    EditorGUILayout.PropertyField(ripplesFPS, NL_Styles.ripplesFPSText);
                    EditorGUILayout.PropertyField(ripplesTiling, NL_Styles.ripplesTilingText);
                    EditorGUILayout.PropertyField(spotsIntensity, NL_Styles.spotsIntensityText);
                    EditorGUILayout.PropertyField(spotsAmount, NL_Styles.spotsAmountText);
                }
                NL_Utilities.EndUICategory();

                NL_Utilities.BeginUICategory("DRIPS", NL_Styles.lineB, dripsFoldout);
                if (dripsFoldout.boolValue)
                {
                    EditorGUILayout.PropertyField(drips, NL_Styles.dripsText);
                    EditorGUILayout.PropertyField(dripsIntensity, NL_Styles.dripsIntensityText);
                    EditorGUILayout.PropertyField(dripsSpeed, NL_Styles.dripsSpeedText);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(dripsTiling, NL_Styles.dripsTilingText);
                    EditorGUI.indentLevel--;
                    EditorGUILayout.PropertyField(distortionAmount, NL_Styles.distortionAmountText);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(distortionTiling, NL_Styles.distortionTilingText);
                    EditorGUI.indentLevel--;
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
            }

            Undo.RecordObject(rainCoverage, "Weatherade Coverage Value Changed");

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                rainCoverage.ValidateValues();

                rainCoverage.CalculateTargetPosOffsetsXZ();

                if (!Application.isPlaying)
                {
                    if (followTarget.objectReferenceValue != null && (followTarget.objectReferenceValue != lastFollowTarget || rainCoverage.targetPosOffset != lastOffset)) rainCoverage.UpdateSRSPosition();

                    lastFollowTarget = followTarget.objectReferenceValue as Transform;
                    lastOffset = rainCoverage.targetPosOffset;
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

        private void SceneViewHandles(SceneView sceneView)
        {
            //rainCoverage.UpdateSRSPosition();
        }

        private void OnSceneGUI()
        {
            //if (rainCoverage == null || !rainCoverage.enabled || !Selection.Contains(rainCoverage.gameObject)) return;
        }

    }
}
#endif
