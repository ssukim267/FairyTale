namespace NOT_Lonely.Weatherade
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    using UnityEngine.Rendering;
#if USING_URP
    using UnityEngine.Rendering.Universal;
#endif

    [ExecuteInEditMode]
    public class SRS_ParticleSystem : MonoBehaviour
    {
#pragma warning disable 0414
        [SerializeField] private bool mainProps = true;
        [SerializeField] private bool emissionProps = true;
        [SerializeField] private bool lifetimeProps = true;
        [SerializeField] private bool colorProps = true;
        [SerializeField] private bool sparklingProps = true;
        [SerializeField] private bool sizeProps = true;
        [SerializeField] private bool velocityProps = true;
        [SerializeField] private bool rotationProps = true;
        [SerializeField] private bool collisionProps = true;
        [SerializeField] private bool lightingProps = true;
#pragma warning restore 0414

        public enum VelocityMode
        {
            Constant,
            Curve,
            RandomConstantsOverLifetime,
            RandomCurvesOverLifetime
        }

        public enum LifetimeMode
        {
            Constant,
            Random
        }
        public enum ColorMode
        {
            Constant,
            GradientOverLifetime,
            RandomBetweenTwoGradients,
            RandomBetweenTwoGradientsOverLifetime
        }
        public enum ParticlesSizeMode
        {
            Constant,
            Random
        }
        public enum StartRotationMode
        {
            Constant,
            Random
        }
        public enum RotationSpeedMode
        {
            Constant,
            Random
        }

        public enum CollisionDepthSource
        {
            Global,
            ThisEmitter
        }

        //Main settings
        [SerializeField, VectorLabels("X", " Z")] private Vector2 emitterSize = new Vector2(35, 35);
        [SerializeField, VectorLabels("X", " Z")] private Vector2Int maxParticlesCount = new Vector2Int(256, 256);
        [SerializeField] private bool warpPositions = true;
        [SerializeField] private float warpBoundsModifierMin = 50;
        [SerializeField] private float forceTeleportThreshold = 5; 
        [SerializeField] private bool useFollowTarget = true;
        public Transform followTarget;
        public Vector3 targetPositionOffset = new Vector3(0, 15, 0);

        //Emission
        [SerializeField] private bool playOnAwake = true;
        [SerializeField] private bool prewarm = true;
        [SerializeField] private int prewarmSteps = 3000;
        [SerializeField] private float emissionStartingTime = 0;
        [SerializeField, Range(0, 1)] private float emissionRate = 1;
        private float prevarmDeltaTime = 0.3f;

        //Particle size
        public ParticlesSizeMode particlesSizeMode = ParticlesSizeMode.Random;
        [SerializeField] private float pSize = 0.01f;
        [SerializeField, VectorLabels("Min", " Max")] private Vector2 pSizeMinMax = new Vector2(0.0085f, 0.016f);
        [SerializeField] private Vector2 pSizeInternal;

        //Lifetime
        [SerializeField] private float lifetime = 5;
        [SerializeField, VectorLabels("Min", " Max")] private Vector2 lifeTimeMinMax = new Vector2(30, 55);

        //Color
        public ColorMode colorMode = ColorMode.Constant;
        [SerializeField, ColorUsage(true, true)] private Color color = Color.white;
        [SerializeField, GradientUsage(true)] private Gradient colorOverLifetimeA = new Gradient();
        [SerializeField, GradientUsage(true)] private Gradient colorOverLifetimeB = new Gradient();
        [SerializeField] private float gradientsRatio = 0.5f;
        [HideInInspector] public Color colorMultiplier = Color.white;

        //Sparkle
        [SerializeField] private bool useSparkling = true;
        [SerializeField, Range(1, 64)] private int sparklingFrequency = 12;
        [SerializeField] private string sunTag = "Untagged";
        [SerializeField] private GameObject sun;
        [SerializeField] private Light sunLightComp; 
        [SerializeField, ColorUsage(true, true)] private Color lightColor = Color.white;
        [SerializeField, Range(0, 1)] private float sunMaskSize = 0.2f;
        [SerializeField, Range(0, 1)] private float sunMaskSharpness = 0.2f;
        [SerializeField] private float sparklesStartDistance = 4;
        [SerializeField] private bool useSecondaryLights = false;
        const int lightsArrayLength = 16;
        [SerializeField, Range(1, lightsArrayLength)] private int maxLightsCount = 5;
        [SerializeField] private float pointLightsIntensity = 1;
        [SerializeField] private float spotLightsIntensity = 1;
        [SerializeField] private Vector4[] pointLightsPoses = new Vector4[lightsArrayLength];
        [SerializeField] private Color[] pointLightsColors = new Color[lightsArrayLength];
        [SerializeField] private Vector4[] spotsPosRange = new Vector4[lightsArrayLength];
        [SerializeField] private Vector4[] spotsDirAngle = new Vector4[lightsArrayLength];
        [SerializeField] private Color[] spotsColors = new Color[lightsArrayLength];

        private List<Light> pointLights = new List<Light>();
        private List<Light> spotLights = new List<Light>();

        //Velocity
        [SerializeField] private bool isVelocityWS = true;
        public LifetimeMode lifetimeMode = LifetimeMode.Random;
        public VelocityMode velocityMode = VelocityMode.RandomConstantsOverLifetime;
        [SerializeField] private Vector3 velocityMin = new Vector3(-0.5f, -1.5f, -0.5f);
        [SerializeField] private Vector3 velocityMax = new Vector3(0, -0.5f, 0.5f);
        [SerializeField] private Vector3 velocityMinMultiplier = Vector3.one;
        [SerializeField] private Vector3 velocityMaxMultiplier = Vector3.one;
        [SerializeField] private Vector3 swayingFrequency = new Vector3(6, 4, 6);
        [SerializeField] private Vector3 swayingAmplitude = new Vector3(2, 0.1f, 2);

        [SerializeField] private Texture2D overLifetimeGradientTex;
        [SerializeField] private AnimationCurve xVelocityMin = new AnimationCurve();
        [SerializeField] private AnimationCurve yVelocityMin = new AnimationCurve();
        [SerializeField] private AnimationCurve zVelocityMin = new AnimationCurve();
        [SerializeField] private AnimationCurve xVelocityMax = new AnimationCurve();
        [SerializeField] private AnimationCurve yVelocityMax = new AnimationCurve();
        [SerializeField] private AnimationCurve zVelocityMax = new AnimationCurve();

        //Renderer
        [SerializeField] private Texture2D noiseTex;
        [SerializeField] private Material particlesMaterial;
        [SerializeField] private Texture2D particleTexture;
        [SerializeField] private Texture2D particleNormal;
        [SerializeField] private bool useRefraction = false;
        [SerializeField] private float nearBlurFalloff = 1f;
        [SerializeField] private float nearBlurDistance = 1;
        [SerializeField] private float opacityFadeFalloff = 0.3f;
        [SerializeField] private float opacityFadeStartDistance = 0.5f;

        [SerializeField] private bool enableLPPV = false;
        [SerializeField] private bool castShadows = false;
        [SerializeField] private bool enableReflectionProbes = false;
        [SerializeField] private Vector3Int lppvRes = new Vector3Int(8, 8, 8);
        [SerializeField] private float stretchMultiplier = 1;

        //Rotation
        public StartRotationMode startRotationMode = StartRotationMode.Constant;
        public RotationSpeedMode rotationSpeedMode = RotationSpeedMode.Constant;
        [SerializeField] private float startRotation = 0;
        [SerializeField] private float rotationSpeed = 0;
        [SerializeField, VectorLabels("Min", " Max")] private Vector2 startRotationMinMax = Vector2.zero;
        [SerializeField, VectorLabels("Min", " Max")] private Vector2 rotationSpeedMinMax = new Vector2(-10, 10);

        [SerializeField] private RenderTexture SRS_particlesRT_beforeSim;
        [SerializeField] private RenderTexture SRS_particlesRT_afterSim;
        [SerializeField] private Material simulationMtl;

        //Collision
        public CollisionDepthSource collisionDepthSource = CollisionDepthSource.ThisEmitter;
        [SerializeField] private bool enableCollision = true;
        [SerializeField] private bool realtimeCollisionUpdate = true;
        [SerializeField, Range(0, 1)] private float updateRate = 0.5f;
        [SerializeField, Range(16, 1024)] private int depthTextureResolution = 256;
        [SerializeField] private LayerMask collisionMask = ~0;
        [SerializeField] private RenderTexture depthTexture;

        [SerializeField] private MeshFilter mFilter;
        [SerializeField] private MeshRenderer mRenderer;
        [SerializeField] private GameObject go;
        [SerializeField] private Camera depthCam;
#if USING_URP
        [SerializeField] private UniversalAdditionalCameraData urpAdditionalCamData;
#endif
        [SerializeField] private Mesh particlesMesh;
        [SerializeField] private Bounds meshBounds;
        [SerializeField] private Bounds meshBoundsLocal;
        public bool isInitiated { private set; get; } = false;
        [SerializeField] private bool particlesCountMismatch = true;
        [SerializeField] private bool generateMeshAtRuntime = false;
        public Vector2 generatedParticles { private set; get; } = Vector2.zero;
        public bool isPlaying { private set; get; } = false;
        private float emissionRateInternal;
        private float prevEmissionRate;
        [SerializeField] private Vector2 lifetimeMinMaxInternal;
        [SerializeField] private Vector3 simulatedPosMin;
        [SerializeField] private Vector3 simulatedPosMax;
        [SerializeField] private Vector3 cornerA;
        [SerializeField] private Vector3 cornerB;
        [SerializeField] private Vector3 cornerC;
        [SerializeField] private Vector3 cornerD;
        [SerializeField] private Vector3 simPosA;
        [SerializeField] private Vector3 simPosB;
        [SerializeField] private Vector3 simPosC;
        [SerializeField] private Vector3 simPosD;
        [SerializeField] private Vector3 simPosE;
        [SerializeField] private Vector3 simPosF;
        [SerializeField] private Vector3 simPosG;
        [SerializeField] private Vector3 simPosH;
        private Vector2 simTexUVsMultiplier;
        private float simTexRatio;
        [SerializeField, HideInInspector] private Vector2 simTexSize;
        private float simTexYmult;
        private Vector2 lastSimTexSize;

        private Vector2 srs_deltaTime;
        private Vector3 velLocalMin;
        private Vector3 velLocalMax;

        private Vector2 newPos2D;
        private Vector2 lastPos2D;
        private VisibilityCheck visibilityCheck;
        private bool allowSimulation = true;

        public int pCountTotal { private set; get; }
        public float usedMemory { private set; get; }

        //simulation mtl keywords
        [SerializeField] private LocalKeyword collKeyword;
        [SerializeField] private LocalKeyword localDepthKeyword;
        [SerializeField] private LocalKeyword velCurvesKeyword;
        [SerializeField] private LocalKeyword warpPositionsKeyword;

        //render mtl keywords
        [SerializeField] private LocalKeyword randGradientKeyword;
        [SerializeField] private LocalKeyword randGradientOLTKeyword;
        [SerializeField] private LocalKeyword colorGradientKeyword;
        [SerializeField] private LocalKeyword sparklesKeyword;
        [SerializeField] private LocalKeyword refractionKeyword;

        public int instanceId { get; private set; }
        public int srsRendererId;

        private Coroutine startEmittingRoutine;
        private Coroutine stopEmittingRoutine;
        private Coroutine updateCollisionTexRoutine;

#if UNITY_EDITOR
        [MenuItem("GameObject/NOT_Lonely/Weatherade/GPU Particles Emitter", false, 10)]
        public static void CreateSRSParticleEmitter()
        {
            SRS_ParticleSystem srsParticleEmitter = new GameObject("SRS_ParticlesEmitter", typeof(SRS_ParticleSystem)).GetComponent<SRS_ParticleSystem>();
            Selection.activeObject = srsParticleEmitter;

            Texture2D defaultParticleTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/NOT_Lonely/Weatherade SRS/Textures/Snowflake.tif");
            srsParticleEmitter.particleTexture = defaultParticleTex;
            
            Vector3 pos;
            if (SceneView.lastActiveSceneView.camera != null)
                pos = SceneView.lastActiveSceneView.camera.transform.position;
            else pos = Vector3.zero;

            srsParticleEmitter.transform.position = pos + srsParticleEmitter.targetPositionOffset;
        }
#endif

        public void ValidateValues()
        {
            if (Application.isPlaying) return;

            if (generateMeshAtRuntime && particlesMesh != null)
            {
                DestroyImmediate(particlesMesh);
                particlesMesh = null;

                if (SRS_particlesRT_beforeSim != null) SRS_particlesRT_beforeSim.Release();
                if (SRS_particlesRT_afterSim != null) SRS_particlesRT_afterSim.Release();

                DestroyImmediate(SRS_particlesRT_beforeSim);
                DestroyImmediate(SRS_particlesRT_afterSim);

                SRS_particlesRT_beforeSim = null;
                SRS_particlesRT_afterSim = null;
            }

            prewarmSteps = Mathf.Max(1, prewarmSteps);
            emissionStartingTime = Mathf.Max(0, emissionStartingTime);
            emitterSize = new Vector2(Mathf.Max(0, emitterSize.x), Mathf.Max(0, emitterSize.y));
            lifetime = Mathf.Max(0, lifetime);
            lifeTimeMinMax.x = Mathf.Max(0, lifeTimeMinMax.x);
            lifeTimeMinMax.y = Mathf.Max(0, lifeTimeMinMax.y);
            gradientsRatio = Mathf.Clamp01(gradientsRatio);
            sunMaskSize = Mathf.Clamp01(sunMaskSize);
            sunMaskSharpness = Mathf.Clamp01(sunMaskSharpness);
            sparklesStartDistance = Mathf.Max(0, sparklesStartDistance);
            sparklingFrequency = Mathf.Clamp(sparklingFrequency, 1, 64);
            pointLightsIntensity = Mathf.Max(0, pointLightsIntensity);
            spotLightsIntensity = Mathf.Max(0, spotLightsIntensity);  
            nearBlurFalloff = Mathf.Max(0, nearBlurFalloff);
            nearBlurDistance = Mathf.Max(0, nearBlurDistance);
            opacityFadeFalloff = Mathf.Max(0, opacityFadeFalloff);
            opacityFadeStartDistance = Mathf.Max(0, opacityFadeStartDistance);
            stretchMultiplier = Mathf.Max(0, stretchMultiplier);
            updateRate = (float)Math.Round(updateRate, 2);

            lppvRes = new Vector3Int(Mathf.Clamp(Mathf.ClosestPowerOfTwo(lppvRes.x), 1, 32), Mathf.Clamp(Mathf.ClosestPowerOfTwo(lppvRes.y), 1, 32), Mathf.Clamp(Mathf.ClosestPowerOfTwo(lppvRes.z), 1, 32));

            lifetimeMinMaxInternal = new Vector2(lifetimeMode == LifetimeMode.Random ? Mathf.Min(lifeTimeMinMax.x, lifeTimeMinMax.y) : lifetime, lifetimeMode == LifetimeMode.Random ? Mathf.Max(lifeTimeMinMax.x, lifeTimeMinMax.y) : lifetime);
            
            if(velocityMode == VelocityMode.Curve)
            {
                simulatedPosMin = GetSimulatedPos(xVelocityMin, yVelocityMin, zVelocityMin, velocityMinMultiplier, 120);
                simulatedPosMax = Vector3.zero;
            }
            if(velocityMode == VelocityMode.RandomCurvesOverLifetime)
            {
                Vector3 tempA = GetSimulatedPos(xVelocityMin, yVelocityMin, zVelocityMin, velocityMinMultiplier, 120);
                Vector3 tempB = GetSimulatedPos(xVelocityMax, yVelocityMax, zVelocityMax, velocityMaxMultiplier, 120);

                simulatedPosMin = Vector3.Min(tempA, tempB);
                simulatedPosMax = Vector3.Max(tempA, tempB);
            }
            if(velocityMode == VelocityMode.Constant)
            {
                if(isVelocityWS)
                    simulatedPosMin = Vector3.Min(velocityMin, velocityMax) * lifetimeMinMaxInternal.y;
                else
                {
                    Vector3 vectorVS = Vector3.Min(velocityMin, velocityMax) * lifetimeMinMaxInternal.y;
                    simulatedPosMin = NL_Utilities.TransformWorldToLocalDir(transform, vectorVS.x, vectorVS.y, vectorVS.z);
                }
                simulatedPosMax = Vector3.zero;
            }
            if (velocityMode == VelocityMode.RandomConstantsOverLifetime)
            {
                if (isVelocityWS)
                {
                    simulatedPosMin = Vector3.Min(velocityMin, velocityMax) * lifetimeMinMaxInternal.y;
                    simulatedPosMax = Vector3.Max(velocityMin, velocityMax) * lifetimeMinMaxInternal.y;
                }
                else
                {
                    Vector3 vectorMinVS = Vector3.Min(velocityMin, velocityMax) * lifetimeMinMaxInternal.y;
                    Vector3 vectorMaxVS = Vector3.Max(velocityMin, velocityMax) * lifetimeMinMaxInternal.y;

                    simulatedPosMin = NL_Utilities.TransformWorldToLocalDir(transform, vectorMinVS.x, vectorMinVS.y, vectorMinVS.z);
                    simulatedPosMax = NL_Utilities.TransformWorldToLocalDir(transform, vectorMaxVS.x, vectorMaxVS.y, vectorMaxVS.z);
                }
            }

            pSizeInternal = new Vector2(particlesSizeMode == ParticlesSizeMode.Random ? Mathf.Min(pSizeMinMax.x, pSizeMinMax.y) : pSize, particlesSizeMode == ParticlesSizeMode.Random ? Mathf.Max(pSizeMinMax.x, pSizeMinMax.y) : pSize);

            maxParticlesCount.x = Mathf.Clamp(Mathf.ClosestPowerOfTwo(maxParticlesCount.x), 2, 2048);
            maxParticlesCount.y = Mathf.Clamp(Mathf.ClosestPowerOfTwo(maxParticlesCount.y), 2, 2048);
            depthTextureResolution = Mathf.Clamp(Mathf.ClosestPowerOfTwo(depthTextureResolution), 16, 1024);

            CalculateEmitterBounds();

            if(generatedParticles != maxParticlesCount || particlesMesh == null) particlesCountMismatch = true;
            else particlesCountMismatch = false;

            if (emissionRate != prevEmissionRate)
            {
                ChangeEmissionRate(emissionRate);
                prevEmissionRate = emissionRate;
            }

            if (particlesMaterial == null) particlesMaterial = new Material(Shader.Find("Hidden/NOT_Lonely/Weatherade/SRS_ParticleSystem_Render"));

            sparklesKeyword = new LocalKeyword(particlesMaterial.shader, "_SPARKLES");
            randGradientKeyword = new LocalKeyword(particlesMaterial.shader, "_RAND_GRADIENT");
            randGradientOLTKeyword = new LocalKeyword(particlesMaterial.shader, "_RAND_GRADIENT_OLT");
            colorGradientKeyword = new LocalKeyword(particlesMaterial.shader, "_COLOR_GRADIENT");
            refractionKeyword = new LocalKeyword(particlesMaterial.shader, "_REFRACTION");

            UpdateColorOverLife();   
            UpdateParticlesMaterial();
            UpdateSimulationMaterial();
            UpdateSparklesAmount();

            GetMemoryUsage();
        }

        private void GetMemoryUsage()
        {
            pCountTotal = maxParticlesCount.x * maxParticlesCount.y;
            float indexSize = pCountTotal > 65535 ? 4 : 2;

            //overall tex memory = tex size * bits per channel * channels count * tex count;
            float simTexMemory = (pCountTotal * 32 * 4 * 2) / 1000000;

            //mesh memory = (total particles count * vertices per particle * per vertex data size + pCount * indices per particle * index size) / 1000000
            float meshMem = ((pCountTotal * 4 * 36) + (pCountTotal * 6 * indexSize)) / 1000000f;

            usedMemory = meshMem + simTexMemory;
        }

        private IEnumerator CollisionDepthUpdate()
        {
            WaitForSeconds waitFor = new WaitForSeconds(updateRate);

            yield return new WaitForEndOfFrame(); //fix for some cases when not all objects have been initiated

            while (true)
            {
                if (Application.isPlaying)
                {
                    depthUVOffset = Vector2.zero;
                    SetUvs(Vector2.zero);
                    depthCam.Render();
                }

                if (updateRate > 0.025f) yield return waitFor;
                else yield return null;
            }
        }

        private void CreateLPPV()
        {
            mRenderer.lightProbeUsage = LightProbeUsage.UseProxyVolume;
            LightProbeProxyVolume lppv = go.AddComponent<LightProbeProxyVolume>();
            lppv.boundingBoxMode = LightProbeProxyVolume.BoundingBoxMode.AutomaticLocal;
            lppv.resolutionMode = LightProbeProxyVolume.ResolutionMode.Custom;
            lppv.gridResolutionX = lppvRes.x;
            lppv.gridResolutionY = lppvRes.y;
            lppv.gridResolutionZ = lppvRes.z;
        }

        private Vector3 GetSimulatedPos(AnimationCurve curveX, AnimationCurve curveY, AnimationCurve curveZ, Vector3 multiplier, int iterations)
        {
            Vector3 pos = Vector3.zero;
            float step = lifetimeMinMaxInternal.y / iterations;
            float t = 0;

            for (int i = 0; i < iterations; i++)
            {
                t += step;
                pos += new Vector3(curveX.Evaluate(t) * multiplier.x, curveY.Evaluate(t) * multiplier.y, curveZ.Evaluate(t) * multiplier.z) * step;
            }

            if (isVelocityWS) 
                return pos;
            else
            {
                Vector3 localPos = NL_Utilities.TransformWorldToLocalDir(transform, pos.x, pos.y, pos.z);
                return localPos;
            }
        }

        private void CreateSimTex()
        {
            if (SRS_particlesRT_beforeSim != null) SRS_particlesRT_beforeSim.Release();
            if (SRS_particlesRT_afterSim != null) SRS_particlesRT_afterSim.Release();

            SRS_particlesRT_beforeSim = new RenderTexture((int)simTexSize.x, (int)simTexSize.y, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            SRS_particlesRT_afterSim = new RenderTexture((int)simTexSize.x, (int)simTexSize.y, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            SRS_particlesRT_beforeSim.filterMode = FilterMode.Point;
            SRS_particlesRT_afterSim.filterMode = FilterMode.Point;
        }

        void UpdateParticlesMaterial()
        {
            if (particlesMaterial == null) particlesMaterial = new Material(Shader.Find("Hidden/NOT_Lonely/Weatherade/SRS_ParticleSystem_Render"));

            simTexRatio = (float)maxParticlesCount.y / (float)maxParticlesCount.x;

            //Square/horizontal or vertical check
            if (simTexRatio <= 1)
            {
                simTexSize = maxParticlesCount;
                simTexYmult = simTexRatio < 1 ? simTexRatio : 1; // check if the tex has a horizontal or square layout
            }
            else 
            {
                simTexSize = new Vector2(maxParticlesCount.y, maxParticlesCount.x);
                simTexYmult = (float)maxParticlesCount.x / (float)maxParticlesCount.y;
            }

            if(!generateMeshAtRuntime) CreateSimTex();

            particlesMaterial.SetVector("_SimTexSize", simTexSize);

            if (simTexSize != lastSimTexSize || noiseTex == null) noiseTex = NL_Utilities.Generate2dNoise((int)simTexSize.x, (int)simTexSize.y);
            if (simulationMtl == null) simulationMtl = new Material(Shader.Find("Hidden/NOT_Lonely/Weatherade/SRS_ParticleSystem_Simulation"));
            simulationMtl.SetTexture("_noiseTex", noiseTex);

            particlesMaterial.SetKeyword(sparklesKeyword, useSparkling);
            particlesMaterial.SetKeyword(refractionKeyword, useRefraction);

            sun = GameObject.FindGameObjectWithTag(sunTag);

            if (sun != null)
            {
                sunLightComp = sun.GetComponent<Light>();
                lightColor.a = 1;

                particlesMaterial.SetVector("_lightDirection", sun.transform.forward);
                particlesMaterial.SetColor("_lightColor", sunLightComp == null ? lightColor : new Color(sunLightComp.color.r, sunLightComp.color.g, sunLightComp.color.b, 1) * sunLightComp.intensity * lightColor);
                particlesMaterial.SetFloat("_sunMaskSize", sunMaskSize);
                particlesMaterial.SetFloat("_sunMaskSharpness", sunMaskSharpness);
                particlesMaterial.SetFloat("_sparklesStartDistance", sparklesStartDistance);
            }
            else
            {
                particlesMaterial.SetColor("_lightColor", new Color(0,0,0,0));
            }

            lastSimTexSize = simTexSize;
        }

        void UpdateSimulationMaterial()
        {
            if(simulationMtl == null) simulationMtl = new Material(Shader.Find("Hidden/NOT_Lonely/Weatherade/SRS_ParticleSystem_Simulation"));

            simulationMtl.SetKeyword(collKeyword, enableCollision);
            simulationMtl.SetKeyword(warpPositionsKeyword, warpPositions);

            if (enableCollision && collisionDepthSource == CollisionDepthSource.ThisEmitter)
            {
                depthTexture = NL_Utilities.UpdateOrCreateRT(depthTexture, depthTextureResolution, RenderTextureFormat.Depth, "SRS_ParticlesLocalDepth");

                simulationMtl.SetKeyword(localDepthKeyword, true);
                simulationMtl.SetTexture("_SRS_localDepth", depthTexture);
            }
            else
            {
                if (depthTexture != null) depthTexture.Release();
                simulationMtl.SetKeyword(localDepthKeyword, false);
            }

            if (velocityMode == VelocityMode.Curve || velocityMode == VelocityMode.RandomCurvesOverLifetime)
            {
                simulationMtl.SetKeyword(velCurvesKeyword, true);
                UpdateVelocityOverLife();
            }
            else
            {
                simulationMtl.SetKeyword(velCurvesKeyword, false);
            }

            simulationMtl.SetFloat("_velCurvesRand", velocityMode == VelocityMode.RandomCurvesOverLifetime ? 0 : 1);

            if (!generateMeshAtRuntime) simulationMtl.SetTexture("_SRS_particles", SRS_particlesRT_beforeSim);
        }

        void UpdateColorOverLife()
        {
            if (overLifetimeGradientTex == null) CreateGradientTex();

            overLifetimeGradientTex = NL_Utilities.UpdateGradientTex(colorOverLifetimeA, colorOverLifetimeB, overLifetimeGradientTex); 

            if (colorMode == ColorMode.Constant) particlesMaterial.SetColor("_color", color);
            particlesMaterial.SetTexture("_gradientTexOLT", overLifetimeGradientTex);

            particlesMaterial.SetKeyword(colorGradientKeyword, colorMode != ColorMode.Constant);
            particlesMaterial.SetKeyword(randGradientKeyword, colorMode == ColorMode.RandomBetweenTwoGradients || colorMode == ColorMode.RandomBetweenTwoGradientsOverLifetime);
            particlesMaterial.SetKeyword(randGradientOLTKeyword, colorMode == ColorMode.RandomBetweenTwoGradientsOverLifetime);
        }

        void UpdateSparklesAmount()
        {
            if (overLifetimeGradientTex == null) CreateGradientTex();
            overLifetimeGradientTex = NL_Utilities.UpdateGradientTexWithSparklesAmount(overLifetimeGradientTex, sparklingFrequency);
            simulationMtl.SetTexture("_gradientTexOLT", overLifetimeGradientTex);
        }

        void UpdateVelocityOverLife()
        {
            if (overLifetimeGradientTex == null) CreateGradientTex();

            overLifetimeGradientTex = NL_Utilities.UpdateGradientTexWithVelocity(xVelocityMin, yVelocityMin, zVelocityMin, xVelocityMax, yVelocityMax, zVelocityMax, overLifetimeGradientTex, isVelocityWS ? null : transform);
            simulationMtl.SetTexture("_gradientTexOLT", overLifetimeGradientTex);
        }

        private void CreateGradientTex()
        {
            overLifetimeGradientTex = new Texture2D(256, 4, TextureFormat.RGBAFloat, false, true);
            overLifetimeGradientTex.wrapMode = TextureWrapMode.Clamp;
            overLifetimeGradientTex.filterMode = FilterMode.Point;
            overLifetimeGradientTex.anisoLevel = 0;
        }

        private void Awake()
        {
            if (!Application.isPlaying) return;

            //if (!isInitiated) Init();
        }

        private void Start()
        {
            if (!Application.isPlaying) return;

            if (!isInitiated)
            {
                Init();
            }
            else
            {
                if (playOnAwake) StartEmitting(prewarm ? 0 : emissionStartingTime);
            }
        }

        private void OnEnable()
        {
            /*
            if (SRS_Manager.srs_dataTransfer == null) SRS_Manager.srs_dataTransfer = NL_Utilities.FindObjectOfType<SRS_DataTransfer>();
            if (SRS_Manager.srs_dataTransfer == null)
                SRS_Manager.srs_dataTransfer = new GameObject("SRS_Data Transfer").AddComponent<SRS_DataTransfer>();
            */

            if (!Application.isPlaying)
            {
                int curInstanceId = GetInstanceID();

                if (curInstanceId != instanceId)
                {
                    instanceId = curInstanceId;
                    simulationMtl = new Material(Shader.Find("Hidden/NOT_Lonely/Weatherade/SRS_ParticleSystem_Simulation"));
                    particlesMaterial = new Material(Shader.Find("Hidden/NOT_Lonely/Weatherade/SRS_ParticleSystem_Render"));
                    particlesMesh = GenerateParticlesMeshInternal(maxParticlesCount.x, maxParticlesCount.y, emitterSize);
                    CreateGradientTex();
                }

                collKeyword = new LocalKeyword(simulationMtl.shader, "_SRS_COLLISION");
                localDepthKeyword =  new LocalKeyword(simulationMtl.shader, "_SRS_LOCALDEPTH");
                velCurvesKeyword = new LocalKeyword(simulationMtl.shader, "_VEL_CURVES");
                warpPositionsKeyword = new LocalKeyword(simulationMtl.shader, "_WARP_POSITIONS");

                ValidateValues();

#if USING_URP && UNITY_EDITOR
                srsRendererId = URP_RendererSetter.SetWeatheradeRenderer(AssetDatabase.LoadAssetAtPath<ScriptableRendererData>("Assets/NOT_Lonely/Weatherade SRS/Resources/SRS_DepthRenderer.asset"));
#endif
                return;
            }

            if (useFollowTarget)
            {
                if (followTarget == null && Camera.main != null) followTarget = Camera.main.transform; // try use the main camera as a follow target if it's not set manually
                if (followTarget != null) transform.position = followTarget.position + targetPositionOffset;
                else Debug.LogWarning($"{gameObject.name}: the follow target is not provided and the camera with the 'MainCamera' tag is not found. The {gameObject.name} particle system will remain in place.");
            }

            /*
            if (!isInitiated)
            {
                Init();
            }
            else
            {
                if (playOnAwake) StartEmitting(prewarm ? 0 : emissionStartingTime);
            }  
            */

            if (isInitiated && playOnAwake) ChangeEmissionRate(emissionRate);
        }

        private void OnDisable()
        {
            if (!Application.isPlaying) return;

            isPlaying = false;
            StopAllCoroutines();
        }

        private void OnDestroy()
        {
            if(Application.isPlaying)
                visibilityCheck.onVisibilityChanged -= OnVisibilityChanged;
        }

        private void Init()
        {
            isInitiated = true;
            StartCoroutine(InitRoutine());
        }

        /// <summary>
        /// Set the Follow Target for the particle system. The particles system will start following the new target in the next frame.
        /// If set to null, then the particles system will remain in place where.
        /// </summary>
        /// <param name="target">New Follow Target transform.</param>
        public void SetFollowTarget(Transform target)
        {
            followTarget = target;
        }

        /// <summary>
        /// Enable/disable the following.
        /// </summary>
        /// <param name="state">State value.</param>
        public void SwitchFollowTargetUsage(bool state)
        {
            useFollowTarget = state;
        }

        /// <summary>
        /// Set the Follow Target offset for the particles system.
        /// </summary>
        /// <param name="offset">The offset vector.</param>
        public void SetFollowTargetOffset(Vector3 offset)
        {
            targetPositionOffset = offset;
        }

        public void StartEmitting(float startEmittingTime = 0)
        {
            if (!isPlaying)
            {
                isPlaying = true;
                if(startEmittingRoutine != null) startEmittingRoutine = null;
                startEmittingRoutine = StartCoroutine(StartEmittingRoutine(startEmittingTime));
            }
        }

        private IEnumerator GetDepthOnce()
        {
            yield return new WaitForEndOfFrame();

            depthUVOffset = Vector2.zero;
            SetUvs(Vector2.zero);
            depthCam.Render();
        }

        private IEnumerator StartEmittingRoutine(float startEmittingTime = 0)
        {
            mRenderer.enabled = true;
            
            simulationMtl.SetFloat("_emissionFlag", 1);

            /*
            if (enableCollision && collisionDepthSource == CollisionDepthSource.ThisEmitter && realtimeCollisionUpdate) StartCoroutine(CollisionDepthUpdate());

            if (!realtimeCollisionUpdate)
            {
                if (enableCollision && collisionDepthSource == CollisionDepthSource.ThisEmitter) StartCoroutine(GetDepthOnce());
            }
            */

            if(enableCollision && collisionDepthSource == CollisionDepthSource.ThisEmitter)
            {
                if (realtimeCollisionUpdate) StartCoroutine(CollisionDepthUpdate());
                else StartCoroutine(GetDepthOnce());
            }

            if (startEmittingTime != 0)
            {
                emissionRateInternal = 0;
                simulationMtl.SetFloat("_particlesPercentage", emissionRateInternal);
                
                simulationMtl.SetFloat("_isPrewarming", 0);

                float cachedLifetimeMin = lifetimeMinMaxInternal.x;
                lifetimeMinMaxInternal.x = 0;
                simulationMtl.SetVector("_lifetimeMinMax", lifetimeMinMaxInternal);

                StartCoroutine(UpdateSimulation());

                float initPercentage = emissionRateInternal;
                float targetPercentage = emissionRate;
                float tStep = 1f / startEmittingTime;
                float t = 0;

                while (true)
                {
                    if (t < 1)
                    {
                        simulationMtl.SetFloat("_particlesPercentage", emissionRateInternal);
                        lifetimeMinMaxInternal.x = cachedLifetimeMin;
                        t += Time.deltaTime * tStep;
                        emissionRateInternal = Mathf.Lerp(initPercentage, targetPercentage, t);
                    }
                    else
                    {
                        emissionRateInternal = targetPercentage;
                        simulationMtl.SetFloat("_particlesPercentage", emissionRateInternal);
                    }

                    if (t >= 1) StopCoroutine(startEmittingRoutine);

                    yield return null;
                }
            }
            else
            {
                emissionRateInternal = emissionRate;
                if(!prewarm) simulationMtl.SetFloat("_particlesPercentage", emissionRateInternal);
                StartCoroutine(UpdateSimulation());
            }

            yield return null;         
        }

        public void UpdateCollisionTexture()
        {
            if (updateCollisionTexRoutine != null) updateCollisionTexRoutine = null;
            updateCollisionTexRoutine = StartCoroutine(UpdateCollisionTex());
        }

        private IEnumerator UpdateCollisionTex()
        {
            if (enableCollision && collisionDepthSource == CollisionDepthSource.ThisEmitter) depthCam.Render();
            yield return null;
        }

        /// <summary>
        /// Change the emission rate of the particle system.
        /// </summary>
        /// <param name="rate">The new rate.</param>
        public void ChangeEmissionRate(float rate)
        {
            if (!Application.isPlaying) return;

            emissionRate = rate;
            emissionRateInternal = rate;

            if(simulationMtl != null) simulationMtl.SetFloat("_particlesPercentage", rate);

            if(emissionRate == 0)
            {
                if (stopEmittingRoutine != null) StopCoroutine(stopEmittingRoutine);
                stopEmittingRoutine = StartCoroutine(StoppingRoutine());
            }else if(!isPlaying)
            {
                StartEmitting(prewarm ? 0 : emissionStartingTime);
            }
        }

        /// <summary>
        /// Stop emmitting new particles.
        /// </summary>
        public void StopEmitting()
        {
            if (stopEmittingRoutine != null) StopCoroutine(stopEmittingRoutine);
            stopEmittingRoutine = StartCoroutine(StoppingRoutine());
        }

        private IEnumerator StoppingRoutine()
        {
            simulationMtl.SetFloat("_emissionFlag", 0);
            float stoppingTime = lifetimeMinMaxInternal.y;

            while (true)
            {
                stoppingTime -= Time.deltaTime;

                if (stoppingTime <= 0)
                {
                    StopAllCoroutines();
                    mRenderer.enabled = false;
                    isPlaying = false;
                }

                yield return null;
            }
        }

        IEnumerator InitRoutine()
        {
            InitEmmiter();
            InitDepthCamera();

#if UNITY_6000_0_OR_NEWER && USING_URP //In Unity 6+ URP use a dictionary to store the Camera/RTHandle pair which then passed into the SRS_RenderDepthWithReplacement render feature
            SRS_Manager.InitDataTransfer();
            SRS_Manager.srs_dataTransfer.AddCameraRTHandlePair(depthCam, RTHandles.Alloc(depthTexture));
            SRS_Manager.srs_dataTransfer.UpdateCameraRTHandlePairs();
#endif

            if (enableLPPV) CreateLPPV();
            else mRenderer.lightProbeUsage = LightProbeUsage.Off;

            mRenderer.shadowCastingMode = castShadows ? ShadowCastingMode.TwoSided : ShadowCastingMode.Off;
            mRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.Camera;
            mRenderer.reflectionProbeUsage = enableReflectionProbes ? ReflectionProbeUsage.BlendProbes : ReflectionProbeUsage.Off;

            if (colorMode == ColorMode.Constant) particlesMaterial.SetColor("_color", color);
            else particlesMaterial.SetTexture("_gradientTexOLT", overLifetimeGradientTex);

            if (prewarm)
            {
                emissionRateInternal = emissionRate;

                if(playOnAwake)
                    ChangeEmissionRate(emissionRateInternal);
                else if(simulationMtl != null) simulationMtl.SetFloat("_particlesPercentage", emissionRateInternal);


                simulationMtl.SetFloat("_emissionFlag", 1);

                simulationMtl.SetFloat("_isPrewarming", 1);

                for (int i = 0; i < prewarmSteps; i++)
                {
                    Shader.SetGlobalVector("_srs_deltaTime", Vector2.one * prevarmDeltaTime);
                    SimOneStep(true);
                }

                if (enableCollision)
                {
                    if (collisionDepthSource == CollisionDepthSource.ThisEmitter)
                    {
                        yield return new WaitForEndOfFrame(); //fix for some cases when not all objects have been initiated
                        depthCam.Render();
                        yield return new WaitForEndOfFrame();
                    }
                    else
                    {
                        yield return new WaitForEndOfFrame();
                    }
                }

                simulationMtl.SetFloat("_isPrewarming", 0);
            }
            else
            {
                StartEmitting(emissionStartingTime);
            }
            
            yield return null;
        }

        private void InitEmmiter()
        {
            go = new GameObject("Emitter");
            go.layer = gameObject.layer;
            go.transform.parent = transform;
            go.transform.localPosition = Vector3.zero;
            go.transform.localEulerAngles = new Vector3(0, 0, 0);

            visibilityCheck = go.AddComponent<VisibilityCheck>();
            visibilityCheck.onVisibilityChanged += OnVisibilityChanged;

            mRenderer = go.gameObject.AddComponent<MeshRenderer>();
            mFilter = go.gameObject.AddComponent<MeshFilter>();

            if (particlesMesh == null || particlesCountMismatch)
            {
                particlesMesh = GenerateParticlesMeshInternal(maxParticlesCount.x, maxParticlesCount.y, emitterSize);
                CreateSimTex();
                simulationMtl.SetTexture("_SRS_particles", SRS_particlesRT_beforeSim);
            }
            mFilter.sharedMesh = particlesMesh;
            mRenderer.enabled = false;

            mRenderer.sharedMaterial = particlesMaterial;
        }

        private void OnVisibilityChanged(bool isVisible)
        {
            allowSimulation = isVisible;
        }

        public void GenerateParticlesMesh()
        {
            particlesMesh = GenerateParticlesMeshInternal(maxParticlesCount.x, maxParticlesCount.y, emitterSize);
        }

        private Mesh GenerateParticlesMeshInternal(int maxParticlesCountX, int maxParticlesCountZ, Vector2 meshSize)
        {
            Mesh mesh = new Mesh();
            mesh.name = "SRS_ParticlesMesh";

            IndexFormat indexFormat = IndexFormat.UInt16;

            if (maxParticlesCountX * maxParticlesCountZ > 65535)
                indexFormat = IndexFormat.UInt32;
            else indexFormat = IndexFormat.UInt16;

            mesh.indexFormat = indexFormat;

            List<Vector3> allVertices = new List<Vector3>();
            List<int> allTris = new List<int>();
            List<Vector3> allNormals = new List<Vector3>();
            List<Vector2> allUVs = new List<Vector2>();
            List<Color> allColors = new List<Color>();

            float xVertStep = 0;
            float yVertStep = 0;
            int xCounter = 0;

            for (int i = 0; i < maxParticlesCountX * maxParticlesCountZ; i++)
            {
                if (xCounter == maxParticlesCountX - 1)
                {
                    xCounter = 0;
                    xVertStep = 0;
                    yVertStep += meshSize.y / maxParticlesCountZ;
                }
                else
                {
                    if (i > 0)
                    {
                        xVertStep += meshSize.x / maxParticlesCountX;
                        xCounter++;
                    }
                }

                Vector3 vertexStep = new Vector3(xVertStep, 0, yVertStep) - new Vector3(meshSize.x, 0, meshSize.y) / 2;

                Vector3[] vertices = new Vector3[4]
                {
                vertexStep,
                vertexStep,
                vertexStep,
                vertexStep
                };

                int triStep = i == 0 ? 0 : 4 * i;

                int[] tris = new int[6]
                {
                // lower left triangle
                0 + triStep, 2 + triStep, 1 + triStep,

                // upper right triangle
                1 + triStep, 2 + triStep, 3 + triStep,
                };

                Vector3[] normals = new Vector3[4]
                {
                Vector3.down,
                Vector3.down,
                Vector3.down,
                Vector3.down
                };

                Vector2[] uv = new Vector2[4]
                {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
                };

                float randomAlpha = UnityEngine.Random.Range(0f, 1f);
                Vector3 quadPos = (vertices[0] + vertices[1] + vertices[2] + vertices[3]) / 4;

                Color quadColor = new Color(quadPos.x, quadPos.y, quadPos.z, randomAlpha);

                Color[] colors = new Color[4]
                {
                quadColor,
                quadColor,
                quadColor,
                quadColor
                };

                allVertices.AddRange(vertices);
                allTris.AddRange(tris);
                //allNormals.AddRange(normals);
                allUVs.AddRange(uv);
                allColors.AddRange(colors);
            }

            mesh.vertices = allVertices.ToArray();
            mesh.triangles = allTris.ToArray();
            //mesh.normals = allNormals.ToArray();
            mesh.uv = allUVs.ToArray();
            mesh.colors = allColors.ToArray();

            mesh.RecalculateBounds();
            mesh.bounds = meshBoundsLocal;

            generatedParticles = new Vector2(maxParticlesCountX, maxParticlesCountZ);
            particlesCountMismatch = false;

            /*
            AssetDatabase.CreateAsset(mesh, "Assets/snowfallMesh.asset");
            AssetDatabase.SaveAssets();
            */

            return mesh;
        }

        public void CalculateEmitterBounds()
        {
            meshBounds.size = Vector3.zero;

            cornerA = transform.position - (transform.right * emitterSize.x / 2) - (transform.forward * emitterSize.y / 2);
            cornerB = transform.position + (transform.right * emitterSize.x / 2) + (transform.forward * emitterSize.y / 2);

            cornerC = transform.position + (transform.right * emitterSize.x / 2) - (transform.forward * emitterSize.y / 2);
            cornerD = transform.position - (transform.right * emitterSize.x / 2) + (transform.forward * emitterSize.y / 2);

            if (warpPositions)
            {
                meshBounds.Encapsulate(cornerA);
                meshBounds.Encapsulate(cornerB);
                meshBounds.Encapsulate(cornerC);
                meshBounds.Encapsulate(cornerD);
                meshBounds.Encapsulate(new Vector3(transform.position.x, transform.position.y + simulatedPosMin.y + warpBoundsModifierMin, transform.position.z));
                meshBounds.Encapsulate(new Vector3(transform.position.x, transform.position.y + simulatedPosMax.y /* + warpBoundsModifierMin*/, transform.position.z));
            }
            else
            {

                Vector3 halfSwayingAmp = swayingAmplitude / 2;

                Vector3 m = new Vector3(simulatedPosMax.x, 0, simulatedPosMax.z);
                if (velocityMode == VelocityMode.RandomConstantsOverLifetime) m = Vector3.zero;

                simPosA = simulatedPosMin + cornerA - halfSwayingAmp;
                simPosB = simulatedPosMin + cornerB - halfSwayingAmp;
                simPosC = simulatedPosMax + m + cornerA + halfSwayingAmp;
                simPosD = simulatedPosMax + m + cornerB + halfSwayingAmp;
                simPosE = simulatedPosMin + cornerC - halfSwayingAmp;
                simPosF = simulatedPosMin + cornerD - halfSwayingAmp;
                simPosG = simulatedPosMax + m + cornerC + halfSwayingAmp;
                simPosH = simulatedPosMax + m + cornerD + halfSwayingAmp;

                meshBounds.center = (simPosA + simPosB + simPosC + simPosD + cornerA + cornerB + simPosE + simPosF + simPosG + simPosH) / 10;

                meshBounds.Encapsulate(cornerA);
                meshBounds.Encapsulate(cornerB);
                meshBounds.Encapsulate(cornerC);
                meshBounds.Encapsulate(cornerD);

                meshBounds.Encapsulate(simPosA);
                meshBounds.Encapsulate(simPosB);
                meshBounds.Encapsulate(simPosC);
                meshBounds.Encapsulate(simPosD);
                meshBounds.Encapsulate(simPosE);
                meshBounds.Encapsulate(simPosF);
                meshBounds.Encapsulate(simPosG);
                meshBounds.Encapsulate(simPosH);
            }

            Vector3 min = transform.InverseTransformPoint(meshBounds.min);
            Vector3 max = transform.InverseTransformPoint(meshBounds.max);
            Vector3 center = transform.InverseTransformPoint(meshBounds.center);

            meshBoundsLocal.min = min;
            meshBoundsLocal.max = max;
            meshBoundsLocal.center = center;

            if (particlesMesh != null) particlesMesh.bounds = meshBoundsLocal;
        }

        private void InitDepthCamera()
        {
            if (depthCam != null || !enableCollision || collisionDepthSource == CollisionDepthSource.Global) return;

            depthCam = new GameObject("Depth Camera").AddComponent<Camera>();

            depthCam.transform.parent = transform;
            depthCam.transform.localPosition = Vector3.zero;
            depthCam.transform.localEulerAngles = Vector3.right * 90;

#if USING_URP
            if (urpAdditionalCamData == null)
                urpAdditionalCamData = depthCam.GetComponent<UniversalAdditionalCameraData>();
            if (urpAdditionalCamData == null)
                urpAdditionalCamData = depthCam.gameObject.AddComponent<UniversalAdditionalCameraData>();

            urpAdditionalCamData.SetRenderer(srsRendererId);

            urpAdditionalCamData.renderShadows = false;
            urpAdditionalCamData.antialiasing = AntialiasingMode.None;
            urpAdditionalCamData.dithering = false;
#endif

            depthCam.depth = -10000;
            depthCam.orthographic = true;
            depthCam.aspect = 1;
            depthCam.depthTextureMode = DepthTextureMode.Depth;
            depthCam.forceIntoRenderTexture = true;
            depthCam.cullingMask = collisionMask;
            depthCam.targetTexture = depthTexture;
            depthCam.orthographicSize = emitterSize.x / 2;
            depthCam.nearClipPlane = 0.1f;
            depthCam.farClipPlane = Mathf.Abs(velocityMax.y * lifetimeMinMaxInternal.y);
#if !USING_URP
            depthCam.SetReplacementShader(Shader.Find("Hidden/NOT_Lonely/Weatherade/DepthRenderer"), "SRSGroupName");
#endif
            depthCam.enabled = false;
        }

        Vector2 depthUVOffset;
        private void SetUvs(Vector2 posDelta)
        {
            float fU = 1 / emitterSize.x;
            float fV = 1 / emitterSize.y;
            float u = posDelta.x * fU;
            float v = posDelta.y * fV;

            depthUVOffset += new Vector2(u, v);

            simulationMtl.SetVector("_uvOffset", depthUVOffset);
        }

        IEnumerator UpdateSimulation()
        {
            while (true)
            {
                if (allowSimulation)
                {
                    SimOneStep();
                }
                yield return null;
            }
        }

        Vector3 lastPos;
        private void SimOneStep(bool isPrewarming = false)
        {
            if (Vector3.Distance(transform.position, lastPos) >= forceTeleportThreshold)
            {
                simulationMtl.SetFloat("_Teleporting", 1);
                simulationMtl.SetVector("_AddPos", transform.position - lastPos);
                
                Graphics.Blit(SRS_particlesRT_beforeSim, SRS_particlesRT_afterSim, simulationMtl);
                Graphics.Blit(SRS_particlesRT_afterSim, SRS_particlesRT_beforeSim);

                lastPos = transform.position;
                simulationMtl.SetFloat("_Teleporting", 0);
                return;
            }

            lastPos = transform.position;

            if (isPrewarming == false)
            {
                srs_deltaTime.x = Time.deltaTime;
                srs_deltaTime.y = Time.timeScale == 0 ? 1 / Time.unscaledDeltaTime : 1 / Time.deltaTime;
                Shader.SetGlobalVector("_srs_deltaTime", srs_deltaTime);
                SendPointAndSpotSparklesData();

                if (sun != null)
                {
                    particlesMaterial.SetVector("_lightDirection", sun.transform.forward);
                    particlesMaterial.SetColor("_lightColor", sunLightComp == null ? lightColor : new Color(sunLightComp.color.r, sunLightComp.color.g, sunLightComp.color.b, 1) * sunLightComp.intensity * lightColor);
                    particlesMaterial.SetFloat("_sunMaskSize", sunMaskSize);
                    particlesMaterial.SetFloat("_sunMaskSharpness", sunMaskSharpness);
                    particlesMaterial.SetFloat("_sparklesStartDistance", sparklesStartDistance);
                }
                else
                {
                    particlesMaterial.SetColor("_lightColor", new Color(0, 0, 0, 0));
                }
            }

            Shader.SetGlobalFloat("_TimeScale", Time.timeScale == 0 ? 1 : Time.timeScale);
            simulationMtl.SetMatrix("_emitterMatrix", transform.localToWorldMatrix);
            simulationMtl.SetVector("_emitterSize", new Vector3(emitterSize.x, meshBounds.size.y, emitterSize.y));
            
            newPos2D = new Vector2(transform.position.x, transform.position.z);
            if (lastPos2D != newPos2D) SetUvs(newPos2D - lastPos2D);  
            lastPos2D = newPos2D;

            if (isVelocityWS)
            {
                if (velocityMode == VelocityMode.Constant)
                {
                    simulationMtl.SetVector("_velocityMin", velocityMin);
                    simulationMtl.SetVector("_velocityMax", velocityMin);
                }
                else if(velocityMode == VelocityMode.RandomConstantsOverLifetime)
                {
                    simulationMtl.SetVector("_velocityMin", velocityMin);
                    simulationMtl.SetVector("_velocityMax", velocityMax);
                }
            }
            else
            {
                velLocalMin = NL_Utilities.TransformWorldToLocalDir(transform, velocityMin.x, velocityMin.y, velocityMin.z);
                velLocalMax = NL_Utilities.TransformWorldToLocalDir(transform, velocityMax.x, velocityMax.y, velocityMax.z);

                if (velocityMode == VelocityMode.Constant)
                {
                    simulationMtl.SetVector("_velocityMin", velLocalMin);
                    simulationMtl.SetVector("_velocityMax", velLocalMin);
                }
                else if (velocityMode == VelocityMode.RandomConstantsOverLifetime)
                {
                    simulationMtl.SetVector("_velocityMin", velLocalMin);
                    simulationMtl.SetVector("_velocityMax", velLocalMax);
                }
            }

            //simulationMtl.SetVector("_boundsCenter", ((simulatedPosMin + transform.position) + transform.position) / 2);
            simulationMtl.SetVector("_velocityMinMultiplier", velocityMinMultiplier);
            simulationMtl.SetVector("_velocityMaxMultiplier", velocityMaxMultiplier);
            simulationMtl.SetVector("_lifetimeMinMax", lifetimeMinMaxInternal);
            simulationMtl.SetVector("_swayingFrequency", swayingFrequency);
            simulationMtl.SetVector("_swayingAmplitude", swayingAmplitude);
  
            if (enableCollision && collisionDepthSource == CollisionDepthSource.ThisEmitter) simulationMtl.SetMatrix("_snowfallDepthCamMatrix", GL.GetGPUProjectionMatrix(depthCam.projectionMatrix, true) * depthCam.worldToCameraMatrix);

            //Shader.SetGlobalVector("_viewCamForward", Camera.main.transform.forward);

            Graphics.Blit(SRS_particlesRT_beforeSim, SRS_particlesRT_afterSim, simulationMtl);

            //render material update
            particlesMaterial.SetTexture("_SRS_particles", SRS_particlesRT_afterSim);
            particlesMaterial.SetTexture("_MainTex", particleTexture);
            particlesMaterial.SetTexture("_Normal", particleNormal);
            particlesMaterial.SetVector("_sizeMinMax", pSizeInternal);
            particlesMaterial.SetColor("_ColorMultiplier", colorMultiplier);
            particlesMaterial.SetVector("_startRotationMinMax", startRotationMode == StartRotationMode.Constant ? Vector2.one * startRotation : startRotationMinMax);
            particlesMaterial.SetVector("_rotationSpeedMinMax", rotationSpeedMode == RotationSpeedMode.Constant ? Vector2.one * rotationSpeed : rotationSpeedMinMax);

            particlesMaterial.SetFloat("_gradientsRatio", gradientsRatio);
            particlesMaterial.SetFloat("_NearBlurFalloff", nearBlurFalloff);
            particlesMaterial.SetFloat("_NearBlurDistance", nearBlurDistance);
            particlesMaterial.SetFloat("_OpacityFadeFalloff", opacityFadeFalloff);
            particlesMaterial.SetFloat("_OpacityFadeStartDistance", opacityFadeStartDistance);

            particlesMaterial.SetFloat("_stretchingMultiplier", stretchMultiplier + 1);

            Graphics.Blit(SRS_particlesRT_afterSim, SRS_particlesRT_beforeSim);
        }

        private void SendPointAndSpotSparklesData()
        {
            //point and spot lights sparkles
            if (!useSecondaryLights) return;

            pointLights = SRS_Manager.pointLights;
            spotLights = SRS_Manager.spotLights;

            pointLights = pointLights.OrderBy(x => Vector3.Distance(x.transform.position, transform.position)).ToList();
            spotLights = spotLights.OrderBy(x => Vector3.Distance(x.transform.position, transform.position)).ToList();

            for (int i = 0; i < pointLightsPoses.Length; i++)
            {
                if (i < pointLights.Count)
                {
                    pointLightsPoses[i] = pointLights[i].transform.position;
                    pointLightsPoses[i].w = pointLights[i].range * pointLights[i].intensity;
                    pointLightsColors[i] = pointLights[i].color;
                }
                else
                {
                    pointLightsPoses[i] = Vector4.zero;
                    pointLightsColors[i] = Color.black;
                }

                if (i < spotLights.Count)
                {
                    spotsPosRange[i] = spotLights[i].transform.position;
                    spotsPosRange[i].w = spotLights[i].range;
                    spotsDirAngle[i] = spotLights[i].transform.forward;
                    spotsDirAngle[i].w = spotLights[i].spotAngle;
                    spotsColors[i] = spotLights[i].color;
                    spotsColors[i].a = spotLights[i].intensity;
                }
                else
                {
                    spotsPosRange[i] = Vector4.zero;
                }
            }

            particlesMaterial.SetInt("_lightsCount", Mathf.Min(maxLightsCount, lightsArrayLength));

            particlesMaterial.SetFloat("_pointLightsIntensity", pointLightsIntensity);
            particlesMaterial.SetVectorArray("_srs_lightsPositions", pointLightsPoses);
            particlesMaterial.SetColorArray("_srs_lightsColors", pointLightsColors);

            particlesMaterial.SetFloat("_spotLightsIntensity", spotLightsIntensity);
            particlesMaterial.SetVectorArray("_srs_spotsPosRange", spotsPosRange);
            particlesMaterial.SetVectorArray("_srs_spotsDirAngle", spotsDirAngle);
            particlesMaterial.SetColorArray("_srs_spotsColors", spotsColors);
        }

        private void Update()
        {
            if (useFollowTarget && followTarget != null) transform.position = followTarget.position + targetPositionOffset;

            if (!Application.isPlaying) return;
            
            /*
            if (Input.GetKeyDown(KeyCode.Space)) StartEmitting(emissionStartingTime);
            if (Input.GetKeyDown(KeyCode.Q)) StopEmitting();
            */
            
        }
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0, 0.656f, 0.9f, 1);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(emitterSize.x, 0, emitterSize.y));

            if (!isVelocityWS) NL_Utilities.DrawArrowGizmo(transform.position, -transform.up * meshBounds.size.y, new Color(0, 0.656f, 0.9f, 1), 0, Color.black, 0.5f);

            Gizmos.DrawIcon(transform.position, "NOT_Lonely/Weatherade/SRS_ParticlesIcon.png", true);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0, 0.656f, 0.9f, 0.15f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(Vector3.zero, new Vector3(emitterSize.x, 0, emitterSize.y));

            if (!Application.isPlaying)
            {
                Gizmos.color = new Color(0, 0.656f, 0.9f, 0.15f);
                Gizmos.matrix = Matrix4x4.identity;
                Gizmos.DrawCube(meshBounds.center, meshBounds.size);

                Gizmos.color = new Color(0, 0.656f, 0.9f, 1);
                Gizmos.matrix = Matrix4x4.identity;
                Gizmos.DrawWireCube(meshBounds.center, meshBounds.size);
            }
        }
#endif
    }
}
