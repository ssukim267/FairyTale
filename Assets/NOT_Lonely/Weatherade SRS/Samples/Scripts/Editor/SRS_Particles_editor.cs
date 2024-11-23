namespace NOT_Lonely.Weatherade
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using static NOT_Lonely.Weatherade.SRS_ParticleSystem;

    [CustomEditor(typeof(SRS_ParticleSystem))]
    public class SRS_Particles_editor : Editor
    {
        private SRS_ParticleSystem srs_particles;

        SerializedProperty mainProps;
        SerializedProperty emissionProps;
        SerializedProperty lifetimeProps;
        SerializedProperty colorProps;
        SerializedProperty sparklingProps;
        SerializedProperty sizeProps;
        SerializedProperty velocityProps;
        SerializedProperty rotationProps;
        SerializedProperty collisionProps;
        SerializedProperty lightingProps;

        #region SerializedProperties
        private SerializedProperty generateMeshAtRuntime;
        private SerializedProperty maxParticlesCount;
        private SerializedProperty emitterSize;

        private SerializedProperty useFollowTarget;
        private SerializedProperty followTarget;
        private SerializedProperty targetPositionOffset;
        private SerializedProperty warpPositions;
        private SerializedProperty forceTeleportThreshold;
        private SerializedProperty warpBoundsModifierMin;

        private SerializedProperty playOnAwake;
        private SerializedProperty prewarm;
        private SerializedProperty prewarmSteps;
        private SerializedProperty emissionRate;
        private SerializedProperty emissionStartingTime;

        private SerializedProperty pSize;
        private SerializedProperty pSizeMinMax;

        private SerializedProperty lifetime;
        private SerializedProperty lifeTimeMinMax;

        private SerializedProperty color;
        private SerializedProperty colorOverLifetimeA;
        private SerializedProperty colorOverLifetimeB;
        private SerializedProperty gradientsRatio;

        private SerializedProperty useSparkling;
        private SerializedProperty sparklingFrequency;
        private SerializedProperty sunTag;
        private SerializedProperty sun;
        private SerializedProperty lightColor;
        private SerializedProperty sunMaskSize;
        private SerializedProperty sunMaskSharpness;
        private SerializedProperty sparklesStartDistance;
        private SerializedProperty useSecondaryLights;
        private SerializedProperty maxLightsCount;
        private SerializedProperty pointLightsIntensity;
        private SerializedProperty spotLightsIntensity;

        private SerializedProperty isVelocityWS;
        private SerializedProperty velocityMin;
        private SerializedProperty velocityMax;
        private SerializedProperty velocityMinMultiplier;
        private SerializedProperty velocityMaxMultiplier;
        private SerializedProperty swayingAmplitude;
        private SerializedProperty swayingFrequency;

        private SerializedProperty startRotation;
        private SerializedProperty startRotationMinMax;
        private SerializedProperty rotationSpeed;
        private SerializedProperty rotationSpeedMinMax;

        private SerializedProperty enableCollision;
        private SerializedProperty collisionDepthSource;
        private SerializedProperty depthTextureResolution;
        private SerializedProperty mask;
        private SerializedProperty realtimeCollisionUpdate;
        private SerializedProperty updateRate;

        private SerializedProperty particleTexture;
        private SerializedProperty useRefraction;
        private SerializedProperty particleNormal;
        private SerializedProperty stretchMultiplier;
        private SerializedProperty nearBlurFalloff;
        private SerializedProperty nearBlurDistance;
        private SerializedProperty opacityFadeFalloff;
        private SerializedProperty opacityFadeStartDistance;
        private SerializedProperty castShadows;
        private SerializedProperty enableLPPV;
        private SerializedProperty lppvRes;

        private SerializedProperty particlesCountMismatch;
        private SerializedProperty particlesMesh;
        private SerializedProperty xVelocityMin;
        private SerializedProperty yVelocityMin;
        private SerializedProperty zVelocityMin;
        private SerializedProperty xVelocityMax;
        private SerializedProperty yVelocityMax;
        private SerializedProperty zVelocityMax;
        #endregion

        #region GUILabels
        private GUIContent generateMeshAtRuntimeLabel = new GUIContent("Generate Mesh at Runtime", "Generate the particles mesh at runtime only. This will require additional CPU time during the particle initialization phase, but will save some storage memory.");
        private GUIContent emitterSizeLabel = new GUIContent("Emitter Size", "Size of the particles emitter in its local space.");
        private GUIContent followTargetLabel = new GUIContent("Follow Target", "The object that this emitter will follow. If the object is not specified, then this emitter will remain in place.");
        private GUIContent targetPositionOffsetLabel = new GUIContent("Offset", "The position offset from the 'Follow Target'.");
        private GUIContent warpPositionsLabel = new GUIContent("Warp Positions", "When move the emitter, particles will be teleported to the opposite side of the emitter once they reached the border of the emitter bounds.");
        private GUIContent forceTeleportThresholdLabel = new GUIContent("Forced Teleport Threshold", "If this emitter changes its position too much between frames (teleporting in VR or similar cases), instantly teleport the particles to the new emitter position using this threshold value as a trigger.");
        private GUIContent warpBoundsModifierMinLabel = new GUIContent("Height Modifier", "Warp bounds Y size modifier. Use it to shrink or expand the bounds manually.");
        private GUIContent simTexFormatLabel = new GUIContent("Sim Tex Format", "The texture precision used for the simulation process. " +
            "Use 'Float' if your system is very large and/or you are moving it too far away from world zeros, otherwise 'Half' is recommended as it has less overhead.");
        private GUIContent playOnAwakeLabel = new GUIContent("Play On Awake", "If enabled, the system will start playing automatically.");
        private GUIContent prewarmLabel = new GUIContent("Prewarm", "When played, a prewarmed system will be in a state as if it had already playing for some time.");
        private GUIContent prewarmStepsLabel = new GUIContent("Steps", "How many simulation steps are used to prewarm the system.");
        private GUIContent startingTimeLabel = new GUIContent("Starting Time", "For example, if set to 5, the system will gradually increase the number of particles over 5 seconds until it reaches the 'Emission Rate' value.\nAvailable only when 'Prewarm' option is disabled.");
        private GUIContent particlesCountLabel = new GUIContent("Particles Count", "How many particles will be generated in the particle mesh by X and Z axis. Total particles = X * Z.");
        private GUIContent pSizeLabel = new GUIContent("Constant Size", "The constant size of each individual particle.");
        private GUIContent pSizeMinMaxLabel = new GUIContent("Random Size", "Random particle size range.");
        private GUIContent lifetimeLabel = new GUIContent("Constant Lifetime", "How much time the particles will live until reset to the start position.");
        private GUIContent lifetimeMinMaxLabel = new GUIContent("Random Lifetime", "How much time the particles will live until reset to the start position.");
        private GUIContent emissionRateLabel = new GUIContent("Emission Rate", "A percentge of visible particles between 0 and 'Total Particles'");
        private GUIContent colorLabel = new GUIContent("Color", "Constant color of the particles.");
        private GUIContent colorOverLifetimeALabel = new GUIContent("Color Over Life", "Gradient color over life time.");
        private GUIContent randomGradientsLabel = new GUIContent("Random Gradients", "Random color for the particles from these two gradients.");
        private GUIContent gradientsOverLifeLabel = new GUIContent("Gradients Over Life", "Random color for the particles from these two gradients over life time.");
        private GUIContent useSparkleLabel = new GUIContent("Enable Sparkle", "The particles will react to the lights of the scene by flickering.");
        private GUIContent sparklingFrequencyLabel = new GUIContent("Frequency", "How often the particles will flicker.");
        private GUIContent sunTagLabel = new GUIContent("Sun Tag", "The object with this tag will be treated as the sun. The direction of its transform will be used to calculate the sparkling area.");
        private GUIContent sunLabel = new GUIContent("Found sun", "Game object found using 'Sun Tag'. Its direction will be used to calculate the particle sparkle effect.");
        private GUIContent sunColorLabel = new GUIContent("Color", "The color that will be used for the sparkle effect.");
        private GUIContent maskExpansionLabel = new GUIContent("Spread", "How far from the 'Sun' will the effect spread.");
        private GUIContent maskSharpnessLabel = new GUIContent("Sharpness", "The sharpness of the sun mask.");
        private GUIContent sparkleDistanceLabel = new GUIContent("Distance", "The distance from the camera where the effect becomes visible.");
        private GUIContent secondaryLightsLabel = new GUIContent("Secondary Lights", "Use point and spot lights in the sparkle effect. To use this feature, the lights must have the NL_VolumetricLight component. This setting has additional performance overhead.");
        private GUIContent maxLightsCountLabel = new GUIContent("Max Lights", "Maximum lights count used in the sparkle effect calculations. The lights are sorted by the distance between them and this particle emitter.");
        private GUIContent pointLightsIntensityLabel = new GUIContent("Points Multiplier", "Multiply the intensity values of the point lights used to calculate the sparkle effect.");
        private GUIContent spotLightsIntensityLabel = new GUIContent("Spots Multiplier", "Multiply the intensity values of the spot lights used to calculate the sparkle effect.");
        private GUIContent isVelocityWSLabel = new GUIContent("World Space", "Is velocity calculated in the world or local space.");
        private GUIContent velocityLabel = new GUIContent("Constant Velocity", "The constant velocity which will be applied to the particles.");
        private GUIContent velocityMinLabel = new GUIContent("Min Velocity", "The minimum velocity which will be applied to the particles.");
        private GUIContent velocityMaxLabel = new GUIContent("Max Velocity", "The maximum velocity which will be applied to the particles.");
        private GUIContent velocityMulLabel = new GUIContent("Multiplier", "The multiplier vector that is used to scale the original velocity vector. Use it to increase/decrease the influence of the velocity set by the curves. This vector is applied in world space.");
        private GUIContent velocityOverLifeLabel = new GUIContent("Velocity Over Life", "Particle velocity curve during its lifetime.");
        private GUIContent velocityOverLifeALabel = new GUIContent("Velocity Over Life A", "Particle velocity curve during its lifetime. Random values will be selected from the range between curves A and B.");
        private GUIContent velocityOverLifeBLabel = new GUIContent("Velocity Over Life B", "Particle velocity curve during its lifetime. Random values will be selected from the range between curves A and B.");
        private GUIContent swayingAmplitudeLabel = new GUIContent("Sine Amplitude", "The amplitude of an additional world space sine wave swaying applied to the particles velocity.");
        private GUIContent swayingFrequencyLabel = new GUIContent("Frequency", "Sine wave frequency.");
        private GUIContent startRotationLabel = new GUIContent("Start Rotation", "Constant start rotation of the particles.");
        private GUIContent startRotationMinMaxLabel = new GUIContent("Start Rotation Random", "Random range of the start rotation of the particles.");
        private GUIContent rotationSpeedLabel = new GUIContent("Speed", "Constant rotation speed of the particles.");
        private GUIContent rotationSpeedMinMaxLabel = new GUIContent("Speed Random", "Random range of the rotation speed of the particles.");
        private GUIContent enableCollisionLabel = new GUIContent("Enable Particles Collision", "Enable collision to prevent particles from passing through world surfaces.");
        private GUIContent collisionDepthSourceLabel = new GUIContent("Depth Source", "World depth texture source. Used to calculate particles collision. If you have an SRS_CoverageInstance in the scene, you can reuse its depth texture by setting this option to 'Global' and the 'Depth Texture Format' of the coverage instance to 'ARGB Half/ARGB Float'.");
        private GUIContent depthTextureResolutionLabel = new GUIContent("Depth Texture Resolution", "Resolution of the depth texture used for collision calculations. Use the smallest value possible and only increase it if particles start to pass through objects in the scene.");
        private GUIContent maskLabel = new GUIContent("Mask", "Object layers that will be included in the collision depth texture. Treat this as objects that the particles will collide with.");
        private GUIContent realtimeCollisionUpdateLabel = new GUIContent("Realtime Update", "Update the depth texture in realtime with the set interval.");
        private GUIContent updateRateLabel = new GUIContent("Interval", "The interval in seconds for the depth texture update. If set to 0, the emitter will perform depth updates every frame. Use values greater than 0 to improve performance.");
        private GUIContent particleTextureLabel = new GUIContent("Texture", "Particle mask texture. R - far mask, G - near mask. You can use a blurred particle texture in the G channel to simulate the Depth of Field effect.");
        private GUIContent useRefractionLabel = new GUIContent("Refraction", "Enables refraction for the particles. Useful for the rain effects.");
        private GUIContent particleNormalLabel = new GUIContent("Normal", "The normal map texture that is used for refraction.");
        private GUIContent stretchMultiplierLabel = new GUIContent("Stretch", "Stretch factor for particles. Values greater than 0 will stretch the particles in the direction they move. Useful for simulating motion blur for raindrops.");
        private GUIContent nearBlurDistanceLabel = new GUIContent("Blur Distance", "Distance offset from the camera, where the particles blur end.");
        private GUIContent nearBlurFalloffLabel = new GUIContent("Falloff", "The size of the transition between the unblured and blured particle.");
        private GUIContent opacityFadeStartDistanceLabel = new GUIContent("Opacity Fade Start", "The distance from the camera at which the particles will begin to lose their opacity. Useful for preventing clipping of particles when they are too close to the camera.");
        private GUIContent opacityFadeFalloffLabel = new GUIContent("Falloff", "The size of the transition between the original opacity and fully transparent particle.");
        private GUIContent castShadowsLabel = new GUIContent("Cast Shadows", "Make particles cast shadows.");
        private GUIContent enableLPPVLabel = new GUIContent("Light Probe Volume", "Create a light probe proxy volume to lit the particles.");
        private GUIContent lppvResLabel = new GUIContent("Resolution", "The resolution of the light probe proxy volume.");
        #endregion

        private Vector3 lastPos;
        private Quaternion lastRot;

        private void OnEnable()
        {
            mainProps = serializedObject.FindProperty("mainProps");
            emissionProps = serializedObject.FindProperty("emissionProps");
            lifetimeProps = serializedObject.FindProperty("lifetimeProps");
            colorProps = serializedObject.FindProperty("colorProps");
            sparklingProps = serializedObject.FindProperty("sparklingProps"); ;
            sizeProps = serializedObject.FindProperty("sizeProps"); ;
            velocityProps = serializedObject.FindProperty("velocityProps"); ;
            rotationProps = serializedObject.FindProperty("rotationProps");
            collisionProps = serializedObject.FindProperty("collisionProps");
            lightingProps = serializedObject.FindProperty("lightingProps");

            generateMeshAtRuntime = serializedObject.FindProperty("generateMeshAtRuntime");
            maxParticlesCount = serializedObject.FindProperty("maxParticlesCount");
            emitterSize = serializedObject.FindProperty("emitterSize");

            useFollowTarget = serializedObject.FindProperty("useFollowTarget");
            followTarget = serializedObject.FindProperty("followTarget");
            targetPositionOffset = serializedObject.FindProperty("targetPositionOffset");
            warpPositions = serializedObject.FindProperty("warpPositions");
            forceTeleportThreshold = serializedObject.FindProperty("forceTeleportThreshold");
            warpBoundsModifierMin = serializedObject.FindProperty("warpBoundsModifierMin");

            playOnAwake = serializedObject.FindProperty("playOnAwake");
            prewarm = serializedObject.FindProperty("prewarm");
            prewarmSteps = serializedObject.FindProperty("prewarmSteps");
            emissionStartingTime = serializedObject.FindProperty("emissionStartingTime");
            emissionRate = serializedObject.FindProperty("emissionRate");

            pSize = serializedObject.FindProperty("pSize");
            pSizeMinMax = serializedObject.FindProperty("pSizeMinMax");

            lifetime = serializedObject.FindProperty("lifetime");
            lifeTimeMinMax = serializedObject.FindProperty("lifeTimeMinMax");

            color = serializedObject.FindProperty("color");
            colorOverLifetimeA = serializedObject.FindProperty("colorOverLifetimeA");
            colorOverLifetimeB = serializedObject.FindProperty("colorOverLifetimeB");
            gradientsRatio = serializedObject.FindProperty("gradientsRatio");

            useSparkling = serializedObject.FindProperty("useSparkling");
            sparklingFrequency = serializedObject.FindProperty("sparklingFrequency");
            sunTag = serializedObject.FindProperty("sunTag");
            sun = serializedObject.FindProperty("sun");
            lightColor = serializedObject.FindProperty("lightColor");
            sunMaskSize = serializedObject.FindProperty("sunMaskSize");
            sunMaskSharpness = serializedObject.FindProperty("sunMaskSharpness");
            sparklesStartDistance = serializedObject.FindProperty("sparklesStartDistance");
            useSecondaryLights = serializedObject.FindProperty("useSecondaryLights");
            maxLightsCount = serializedObject.FindProperty("maxLightsCount");
            pointLightsIntensity = serializedObject.FindProperty("pointLightsIntensity");
            spotLightsIntensity = serializedObject.FindProperty("spotLightsIntensity");

            isVelocityWS = serializedObject.FindProperty("isVelocityWS");
            velocityMin = serializedObject.FindProperty("velocityMin");
            velocityMax = serializedObject.FindProperty("velocityMax");
            velocityMinMultiplier = serializedObject.FindProperty("velocityMinMultiplier");
            velocityMaxMultiplier = serializedObject.FindProperty("velocityMaxMultiplier");
            swayingAmplitude = serializedObject.FindProperty("swayingAmplitude");
            swayingFrequency = serializedObject.FindProperty("swayingFrequency");
            startRotation = serializedObject.FindProperty("startRotation");
            startRotationMinMax = serializedObject.FindProperty("startRotationMinMax");
            rotationSpeed = serializedObject.FindProperty("rotationSpeed");
            rotationSpeedMinMax = serializedObject.FindProperty("rotationSpeedMinMax");

            enableCollision = serializedObject.FindProperty("enableCollision");
            collisionDepthSource = serializedObject.FindProperty("collisionDepthSource");
            depthTextureResolution = serializedObject.FindProperty("depthTextureResolution");
            mask = serializedObject.FindProperty("collisionMask");
            realtimeCollisionUpdate = serializedObject.FindProperty("realtimeCollisionUpdate");
            updateRate = serializedObject.FindProperty("updateRate");

            particleTexture = serializedObject.FindProperty("particleTexture");
            useRefraction = serializedObject.FindProperty("useRefraction");
            particleNormal = serializedObject.FindProperty("particleNormal");
            stretchMultiplier = serializedObject.FindProperty("stretchMultiplier");
            nearBlurFalloff = serializedObject.FindProperty("nearBlurFalloff");
            nearBlurDistance = serializedObject.FindProperty("nearBlurDistance");
            opacityFadeFalloff = serializedObject.FindProperty("opacityFadeFalloff");
            opacityFadeStartDistance = serializedObject.FindProperty("opacityFadeStartDistance");
            castShadows = serializedObject.FindProperty("castShadows");
            enableLPPV = serializedObject.FindProperty("enableLPPV");
            lppvRes = serializedObject.FindProperty("lppvRes");

            particlesCountMismatch = serializedObject.FindProperty("particlesCountMismatch");
            particlesMesh = serializedObject.FindProperty("particlesMesh");
            xVelocityMin = serializedObject.FindProperty("xVelocityMin");
            yVelocityMin = serializedObject.FindProperty("yVelocityMin");
            zVelocityMin = serializedObject.FindProperty("zVelocityMin");
            xVelocityMax = serializedObject.FindProperty("xVelocityMax");
            yVelocityMax = serializedObject.FindProperty("yVelocityMax");
            zVelocityMax = serializedObject.FindProperty("zVelocityMax");
        }

        public override void OnInspectorGUI()
        {
            srs_particles = target as SRS_ParticleSystem;

            if (NL_Styles.lineB == null || NL_Styles.lineB.normal.background == null) NL_Styles.GetStyles();

            EditorGUI.BeginChangeCheck();

            NL_Utilities.DrawCenteredBoldHeader("WEATHERADE PARTICLE SYSTEM");

            float currentInspectorWidth = EditorGUIUtility.currentViewWidth - 24 - 8;

            if (NL_Utilities.DrawFoldout(mainProps, "MAIN SETTINGS"))
            {
                //Runtime only generation
                GUILayout.BeginHorizontal(NL_Styles.lineA);
                EditorGUILayout.PropertyField(generateMeshAtRuntime, generateMeshAtRuntimeLabel);
                GUILayout.EndHorizontal();

                //Emitter Size
                GUILayout.BeginHorizontal(NL_Styles.lineA);
                EditorGUILayout.PropertyField(emitterSize, emitterSizeLabel);
                GUILayout.EndHorizontal();

                //Particles Count
                GUILayout.BeginVertical(NL_Styles.lineB);
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(maxParticlesCount, particlesCountLabel);
                GUILayout.EndHorizontal();
                GUI.color = new Color(1, 1, 1, 0.5f);
                EditorGUILayout.LabelField(new GUIContent($"Total Particles: {srs_particles.pCountTotal}", "The total amount of particles that will be generated in the particle mesh."), EditorStyles.label);
                EditorGUILayout.LabelField(new GUIContent($"Memory usage: {srs_particles.usedMemory.ToString("0.0")} MBytes", "Amount of megabytes used in memory for the whole particles mesh."), EditorStyles.label);
                GUI.color = Color.white;
                GUILayout.EndVertical();

                //Follow target
                GUILayout.BeginVertical(NL_Styles.lineA);
                GUILayout.BeginHorizontal(NL_Styles.lineA);
                EditorGUILayout.PropertyField(useFollowTarget, followTargetLabel);
                if (useFollowTarget.boolValue)
                {
                    EditorGUILayout.PropertyField(followTarget, new GUIContent());
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(NL_Styles.lineA);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(targetPositionOffset, targetPositionOffsetLabel);
                    EditorGUI.indentLevel--;
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();

                GUILayout.BeginHorizontal(NL_Styles.lineB);
                EditorGUILayout.PropertyField(warpPositions, warpPositionsLabel);
                GUILayout.EndHorizontal();

                if (warpPositions.boolValue)
                {
                    GUILayout.BeginVertical(NL_Styles.lineB);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(warpBoundsModifierMin, warpBoundsModifierMinLabel);
                    EditorGUILayout.PropertyField(forceTeleportThreshold, forceTeleportThresholdLabel);
                    EditorGUI.indentLevel--;
                    GUILayout.EndVertical();
                }

                //Generate Mesh
                if (!generateMeshAtRuntime.boolValue && particlesCountMismatch.boolValue)
                {
                    GUILayout.BeginVertical();
                    EditorGUILayout.HelpBox("The properties have been changed, the particle mesh must be rebuilt. Click the button below to do so, otherwise the mesh will be generated on awake which will take some CPU time.", MessageType.Warning);
                    if (GUILayout.Button(new GUIContent(particlesMesh.objectReferenceValue == null ? "Generate Particles Mesh" : "Update Particles Mesh", "Generate the particle mesh which will be used in the simulation process. \nIf not generated, then the mesh will be created on awake, which can cause a performance decrease.")))
                    {
                        srs_particles.GenerateParticlesMesh();
                    }
                    GUILayout.EndVertical();
                }

                GUILayout.Space(10);
            }

            GUILayout.Space(1);

            if (NL_Utilities.DrawFoldout(emissionProps, "EMISSION"))
            {
                //Play On Awake
                GUILayout.BeginHorizontal(NL_Styles.lineA);
                EditorGUILayout.PropertyField(playOnAwake, playOnAwakeLabel);
                GUILayout.EndHorizontal();

                //Prewarm

                EditorGUILayout.BeginVertical(NL_Styles.lineB);
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(prewarm, prewarmLabel);
                GUILayout.EndHorizontal();

                //Steps
                if (prewarm.boolValue)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(prewarmSteps, prewarmStepsLabel);
                    EditorGUI.indentLevel--;
                    GUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();

                //Emission Starting Time
                if (!prewarm.boolValue)
                {
                    GUILayout.BeginHorizontal(NL_Styles.lineA);
                    EditorGUILayout.PropertyField(emissionStartingTime, startingTimeLabel);
                    GUILayout.EndHorizontal();
                }

                //Emission Rate
                GUILayout.BeginHorizontal(prewarm.boolValue ? NL_Styles.lineA : NL_Styles.lineB);
                EditorGUILayout.PropertyField(emissionRate, emissionRateLabel);
                GUILayout.EndHorizontal();

                GUILayout.Space(10);
            }

            GUILayout.Space(1);

            if (NL_Utilities.DrawFoldout(sizeProps, "PARTICLES SIZE"))
            {
                GUILayout.BeginHorizontal(NL_Styles.lineA);

                if (srs_particles.particlesSizeMode == ParticlesSizeMode.Constant) EditorGUILayout.PropertyField(pSize, pSizeLabel);
                if (srs_particles.particlesSizeMode == ParticlesSizeMode.Random) EditorGUILayout.PropertyField(pSizeMinMax, pSizeMinMaxLabel);

                srs_particles.particlesSizeMode = (ParticlesSizeMode)EditorGUILayout.EnumPopup(srs_particles.particlesSizeMode, NL_Styles.miniEnumBtn, GUILayout.Width(10));
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }

            GUILayout.Space(1);

            if (NL_Utilities.DrawFoldout(lifetimeProps, "LIFETIME"))
            {
                GUILayout.BeginHorizontal(NL_Styles.lineA);
  
                if (srs_particles.lifetimeMode == LifetimeMode.Constant) EditorGUILayout.PropertyField(lifetime, lifetimeLabel);
                else EditorGUILayout.PropertyField(lifeTimeMinMax, lifetimeMinMaxLabel);

                srs_particles.lifetimeMode = (LifetimeMode)EditorGUILayout.EnumPopup(srs_particles.lifetimeMode, NL_Styles.miniEnumBtn, GUILayout.Width(10));

                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }

            GUILayout.Space(1);

            if (NL_Utilities.DrawFoldout(colorProps, "COLOR"))
            {
                //Color constant
                if (srs_particles.colorMode == ColorMode.Constant)
                {
                    GUILayout.BeginHorizontal(NL_Styles.lineA);
                    EditorGUILayout.PropertyField(color, colorLabel);
                    srs_particles.colorMode = (ColorMode)EditorGUILayout.EnumPopup(srs_particles.colorMode, NL_Styles.miniEnumBtn, GUILayout.Width(10));
                    GUILayout.EndHorizontal();
                }
                //Color gradient
                if (srs_particles.colorMode == ColorMode.GradientOverLifetime)
                {
                    GUILayout.BeginHorizontal(NL_Styles.lineA);
                    EditorGUILayout.PropertyField(colorOverLifetimeA, colorOverLifetimeALabel);
                    srs_particles.colorMode = (ColorMode)EditorGUILayout.EnumPopup(srs_particles.colorMode, NL_Styles.miniEnumBtn, GUILayout.Width(10));
                    GUILayout.EndHorizontal();
                }
                //Color random gradients and gradients over lifetime
                if (srs_particles.colorMode == ColorMode.RandomBetweenTwoGradientsOverLifetime || srs_particles.colorMode == ColorMode.RandomBetweenTwoGradients)
                {
                    GUILayout.BeginHorizontal(NL_Styles.lineA);
                    EditorGUILayout.PrefixLabel(srs_particles.colorMode == ColorMode.RandomBetweenTwoGradients ? randomGradientsLabel : gradientsOverLifeLabel);
                    EditorGUILayout.PropertyField(colorOverLifetimeA, new GUIContent());
                    gradientsRatio.floatValue = EditorGUILayout.FloatField(gradientsRatio.floatValue, NL_Styles.centeredTextField, GUILayout.MaxWidth(30));
                    EditorGUILayout.PropertyField(colorOverLifetimeB, new GUIContent());
                    srs_particles.colorMode = (ColorMode)EditorGUILayout.EnumPopup(srs_particles.colorMode, NL_Styles.miniEnumBtn, GUILayout.Width(10));
                    GUILayout.EndHorizontal();
                }

                GUILayout.Space(10);
            }

            GUILayout.Space(1);

            if (NL_Utilities.DrawFoldout(sparklingProps, "SPARKLE"))
            {
                GUILayout.BeginHorizontal(NL_Styles.lineA);
                EditorGUILayout.PropertyField(useSparkling, useSparkleLabel);
                GUILayout.EndHorizontal();

                if (useSparkling.boolValue)
                {
                    GUILayout.BeginHorizontal(NL_Styles.lineB);
                    EditorGUILayout.PropertyField(sparklingFrequency, sparklingFrequencyLabel);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginVertical(NL_Styles.lineA);

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(sunTagLabel);
                    sunTag.stringValue = EditorGUILayout.TagField(sunTag.stringValue);
                    GUILayout.EndHorizontal();

                    if (sun.objectReferenceValue != null)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.PropertyField(sun, sunLabel);
                        EditorGUI.EndDisabledGroup();
                        EditorGUI.indentLevel--;
                    }
                    else EditorGUILayout.HelpBox("Sun not found! Please make sure you have an object with the selected 'Sun Tag' in the scene.", MessageType.Warning);

                    EditorGUI.indentLevel++;

                    if (sun.objectReferenceValue != null)
                    {
                        EditorGUILayout.PropertyField(lightColor, sunColorLabel);
                        EditorGUILayout.PropertyField(sunMaskSize, maskExpansionLabel);
                        EditorGUILayout.PropertyField(sunMaskSharpness, maskSharpnessLabel);
                        EditorGUILayout.PropertyField(sparklesStartDistance, sparkleDistanceLabel);
                    }
                    EditorGUI.indentLevel--;
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical(NL_Styles.lineB);
                    EditorGUILayout.PropertyField(useSecondaryLights, secondaryLightsLabel);

                    if (useSecondaryLights.boolValue)
                    {
                        EditorGUI.indentLevel++;
                        EditorGUILayout.PropertyField(maxLightsCount, maxLightsCountLabel);
                        EditorGUILayout.PropertyField(pointLightsIntensity, pointLightsIntensityLabel);
                        EditorGUILayout.PropertyField(spotLightsIntensity, spotLightsIntensityLabel);
                        EditorGUI.indentLevel--;
                    }
                    GUILayout.EndVertical();
                }

                GUILayout.Space(10);
            }

            GUILayout.Space(1);

            if (NL_Utilities.DrawFoldout(velocityProps, "VELOCITY"))
            {
                /*
                GUILayout.BeginHorizontal(NL_Styles.lineA);
                EditorGUILayout.PropertyField(isVelocityWS, isVelocityWSLabel);
                GUILayout.EndHorizontal();
                */

                if (srs_particles.velocityMode == VelocityMode.Constant)
                {
                    GUILayout.BeginHorizontal(NL_Styles.lineB);
                    EditorGUILayout.PropertyField(velocityMin, velocityLabel);
                    srs_particles.velocityMode = (VelocityMode)EditorGUILayout.EnumPopup(srs_particles.velocityMode, NL_Styles.miniEnumBtn, GUILayout.Width(10));
                    GUILayout.EndHorizontal();
                }
                if (srs_particles.velocityMode == VelocityMode.RandomConstantsOverLifetime)
                {
                    GUILayout.BeginHorizontal(NL_Styles.lineB);
                    EditorGUILayout.PropertyField(velocityMin, velocityMinLabel);
                    srs_particles.velocityMode = (VelocityMode)EditorGUILayout.EnumPopup(srs_particles.velocityMode, NL_Styles.miniEnumBtn, GUILayout.Width(10));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal(NL_Styles.lineB);
                    EditorGUILayout.PropertyField(velocityMax, velocityMaxLabel);
                    GUILayout.Space(16);
                    GUILayout.EndHorizontal();
                }
                if (srs_particles.velocityMode == VelocityMode.Curve)
                {
                    GUILayout.BeginVertical(NL_Styles.lineB);

                    GUILayout.BeginHorizontal();

                    EditorGUILayout.PrefixLabel(velocityOverLifeLabel);
                    EditorGUILayout.LabelField("X", GUILayout.MaxWidth(10));
                    xVelocityMin.animationCurveValue = EditorGUILayout.CurveField(xVelocityMin.animationCurveValue, Color.red, new Rect(), GUILayout.MinWidth(30));
                    EditorGUILayout.LabelField("Y", GUILayout.MaxWidth(10));
                    yVelocityMin.animationCurveValue = EditorGUILayout.CurveField(yVelocityMin.animationCurveValue, Color.green, new Rect(), GUILayout.MinWidth(30));
                    EditorGUILayout.LabelField("Z", GUILayout.MaxWidth(10));
                    zVelocityMin.animationCurveValue = EditorGUILayout.CurveField(zVelocityMin.animationCurveValue, Color.blue, new Rect(), GUILayout.MinWidth(30));

                    srs_particles.velocityMode = (VelocityMode)EditorGUILayout.EnumPopup(srs_particles.velocityMode, NL_Styles.miniEnumBtn, GUILayout.Width(10));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(velocityMinMultiplier, velocityMulLabel);
                    GUILayout.Space(16);
                    EditorGUI.indentLevel--;
                    GUILayout.EndHorizontal();

                    GUILayout.EndVertical();
                }

                if (srs_particles.velocityMode == VelocityMode.RandomCurvesOverLifetime)
                {
                    GUILayout.BeginVertical(NL_Styles.lineB);

                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(velocityOverLifeALabel);
                    EditorGUILayout.LabelField("X", GUILayout.MaxWidth(10));
                    xVelocityMin.animationCurveValue = EditorGUILayout.CurveField(xVelocityMin.animationCurveValue, Color.red, new Rect(), GUILayout.MinWidth(30));
                    EditorGUILayout.LabelField("Y", GUILayout.MaxWidth(10));
                    yVelocityMin.animationCurveValue = EditorGUILayout.CurveField(yVelocityMin.animationCurveValue, Color.green, new Rect(), GUILayout.MinWidth(30));
                    EditorGUILayout.LabelField("Z", GUILayout.MaxWidth(10));
                    zVelocityMin.animationCurveValue = EditorGUILayout.CurveField(zVelocityMin.animationCurveValue, Color.blue, new Rect(), GUILayout.MinWidth(30));
                    srs_particles.velocityMode = (VelocityMode)EditorGUILayout.EnumPopup(srs_particles.velocityMode, NL_Styles.miniEnumBtn, GUILayout.Width(10));
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(velocityMinMultiplier, velocityMulLabel);
                    GUILayout.Space(16);
                    EditorGUI.indentLevel--;
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical(NL_Styles.lineA);
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.PrefixLabel(velocityOverLifeBLabel);
                    EditorGUILayout.LabelField("X", GUILayout.MaxWidth(10));
                    xVelocityMax.animationCurveValue = EditorGUILayout.CurveField(xVelocityMax.animationCurveValue, Color.red, new Rect(), GUILayout.MinWidth(30));
                    EditorGUILayout.LabelField("Y", GUILayout.MaxWidth(10));
                    yVelocityMax.animationCurveValue = EditorGUILayout.CurveField(yVelocityMax.animationCurveValue, Color.green, new Rect(), GUILayout.MinWidth(30));
                    EditorGUILayout.LabelField("Z", GUILayout.MaxWidth(10));
                    zVelocityMax.animationCurveValue = EditorGUILayout.CurveField(zVelocityMax.animationCurveValue, Color.blue, new Rect(), GUILayout.MinWidth(30));
                    GUILayout.Space(16);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(velocityMaxMultiplier, velocityMulLabel);
                    GUILayout.Space(16);
                    EditorGUI.indentLevel--;
                    GUILayout.EndHorizontal();

                    GUILayout.EndVertical();
                }

                GUILayout.BeginVertical(srs_particles.velocityMode == VelocityMode.RandomCurvesOverLifetime ? NL_Styles.lineB : NL_Styles.lineA);
                EditorGUILayout.PropertyField(swayingAmplitude, swayingAmplitudeLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(swayingFrequency, swayingFrequencyLabel);
                EditorGUI.indentLevel--;
                GUILayout.EndVertical();
                GUILayout.Space(10);
            }

            GUILayout.Space(1);

            if (NL_Utilities.DrawFoldout(rotationProps, "ROTATION"))
            {
                GUILayout.BeginHorizontal(NL_Styles.lineA);
                if (srs_particles.startRotationMode == StartRotationMode.Constant) EditorGUILayout.PropertyField(startRotation, startRotationLabel);
                if (srs_particles.startRotationMode == StartRotationMode.Random) EditorGUILayout.PropertyField(startRotationMinMax, startRotationMinMaxLabel);

                srs_particles.startRotationMode = (StartRotationMode)EditorGUILayout.EnumPopup(srs_particles.startRotationMode, NL_Styles.miniEnumBtn, GUILayout.Width(10));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(NL_Styles.lineB);
                if (srs_particles.rotationSpeedMode == RotationSpeedMode.Constant) EditorGUILayout.PropertyField(rotationSpeed, rotationSpeedLabel);
                if (srs_particles.rotationSpeedMode == RotationSpeedMode.Random) EditorGUILayout.PropertyField(rotationSpeedMinMax, rotationSpeedMinMaxLabel);

                srs_particles.rotationSpeedMode = (RotationSpeedMode)EditorGUILayout.EnumPopup(srs_particles.rotationSpeedMode, NL_Styles.miniEnumBtn, GUILayout.Width(10));
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
            }

            GUILayout.Space(1);

            if (NL_Utilities.DrawFoldout(collisionProps, "COLLISION"))
            {
                GUILayout.BeginHorizontal(NL_Styles.lineA);
                EditorGUILayout.PropertyField(enableCollision, enableCollisionLabel);
                GUILayout.EndHorizontal();

                if (enableCollision.boolValue)
                {
                    GUILayout.BeginHorizontal(NL_Styles.lineB);
                    EditorGUILayout.PropertyField(collisionDepthSource, collisionDepthSourceLabel);
                    GUILayout.EndHorizontal();

                    if (srs_particles.collisionDepthSource == CollisionDepthSource.ThisEmitter)
                    {
                        GUILayout.BeginHorizontal(NL_Styles.lineA);
                        EditorGUILayout.PropertyField(depthTextureResolution, depthTextureResolutionLabel);
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal(NL_Styles.lineB);
                        EditorGUILayout.PropertyField(mask, maskLabel);
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal(NL_Styles.lineA);
                        EditorGUILayout.PropertyField(realtimeCollisionUpdate, realtimeCollisionUpdateLabel);
                        GUILayout.EndHorizontal();

                        if (realtimeCollisionUpdate.boolValue)
                        {
                            GUILayout.BeginHorizontal(NL_Styles.lineA);
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(updateRate, updateRateLabel);
                            EditorGUI.indentLevel--;
                            GUILayout.EndHorizontal();
                        }
                    }
                    else
                    {
                        if (CoverageBase.instance == null) EditorGUILayout.HelpBox("There's no SRS_CoverageInstance in the scene. Please add one to use the global depth as a collision source or switch to local source.", MessageType.Warning);
                    }
                }
                else
                {

                }

                GUILayout.Space(10);
            }

            GUILayout.Space(1);

            if (NL_Utilities.DrawFoldout(lightingProps, "RENDERER"))
            {
                GUILayout.BeginHorizontal(NL_Styles.lineA);
                EditorGUILayout.PropertyField(particleTexture, particleTextureLabel);
                GUILayout.EndHorizontal();

                GUILayout.BeginVertical(NL_Styles.lineB);
                EditorGUILayout.PropertyField(useRefraction, useRefractionLabel);
                if (useRefraction.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(particleNormal, particleNormalLabel);
                    EditorGUI.indentLevel--;
                }
                GUILayout.EndVertical();

                GUILayout.BeginHorizontal(NL_Styles.lineA);
                EditorGUILayout.PropertyField(stretchMultiplier, stretchMultiplierLabel);
                GUILayout.EndHorizontal();

                GUILayout.BeginVertical(NL_Styles.lineB);
                EditorGUILayout.PropertyField(nearBlurDistance, nearBlurDistanceLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(nearBlurFalloff, nearBlurFalloffLabel);
                EditorGUI.indentLevel--;
                GUILayout.EndVertical();

                GUILayout.BeginVertical(NL_Styles.lineA);
                EditorGUILayout.PropertyField(opacityFadeStartDistance, opacityFadeStartDistanceLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(opacityFadeFalloff, opacityFadeFalloffLabel);
                EditorGUI.indentLevel--;
                GUILayout.EndVertical();
                
                /*
                GUILayout.BeginHorizontal(NL_Styles.lineB);
                EditorGUILayout.PropertyField(castShadows, castShadowsLabel);
                GUILayout.EndHorizontal();
                
                GUILayout.BeginVertical(NL_Styles.lineA);
                EditorGUILayout.PropertyField(enableLPPV, enableLPPVLabel);

                if (enableLPPV.boolValue)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(lppvRes, lppvResLabel);
                    EditorGUI.indentLevel--;
                }
                GUILayout.EndVertical();

                GUILayout.Space(10);
                */
            }

            Undo.RecordObject(srs_particles, "Weatherade Particles Value Changed");

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                srs_particles.ValidateValues();
            }
        }

        private void OnSceneGUI()
        {
            if (srs_particles != null)
            {
                if (!Selection.Contains(srs_particles.gameObject) || Application.isPlaying) return;

                if (srs_particles.transform.rotation != lastRot || srs_particles.transform.position != lastPos)
                {
                    srs_particles.ValidateValues();
                }

                lastPos = srs_particles.transform.position;
                lastRot = srs_particles.transform.rotation;
            }
        }
    }
}
