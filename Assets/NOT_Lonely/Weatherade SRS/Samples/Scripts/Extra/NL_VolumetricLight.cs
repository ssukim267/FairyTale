namespace NOT_Lonely.Weatherade
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    [RequireComponent(typeof(Light))]

#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif

    public class NL_VolumetricLight : MonoBehaviour
    {
        [SerializeField] private bool realtimeUpdate = true;
        [SerializeField] private float intensityMultiplier = 1;
        [SerializeField] private float rangeMultiplier = 1;
        [SerializeField] private float beamStartRadius = 0.2f;
        [SerializeField] private float maskHardness = 0.02f;
        [SerializeField] private float noiseIntensity = 1;
        [SerializeField] private float noiseTiling = 1;
        [SerializeField] private Vector2 noiseSpeed = new Vector2(0.01f, -0.01f);
        [SerializeField] private float intersectionsDepthFade = 2;
        [SerializeField] private float cameraFadeDistance = 10;
        [SerializeField] private float zOffset = 0.5f;
        [SerializeField] private Texture2D noiseTexture;

        [HideInInspector] public Light lightComp;
        [SerializeField] private MaterialPropertyBlock pb;
        [SerializeField] private Bounds volBounds;
        [SerializeField] private MeshRenderer volRenderer;
        [SerializeField] private MeshFilter volFilter;
        [SerializeField] private GameObject volObj;

        [SerializeField] private float endRadius;
        [SerializeField] private float mostFarPoint;
        [SerializeField] private LightType lightType;
        [SerializeField] private int instanceId;
        [SerializeField] private float randomValue;

        public delegate void OnLightSwitch(Light srs_light, bool state);
        public OnLightSwitch onLightSwitch;

        public void ValidateValues()
        {
            rangeMultiplier = Mathf.Max(0, rangeMultiplier);
            intensityMultiplier = Mathf.Max(0, intensityMultiplier);
            maskHardness = Mathf.Max(0.001f, maskHardness);
            intersectionsDepthFade = Mathf.Max(0, intersectionsDepthFade);
            zOffset = Mathf.Max(0, zOffset);
            noiseIntensity = Mathf.Clamp01(noiseIntensity);
            beamStartRadius = Mathf.Max(0, beamStartRadius);
        }

        public void UpdateValues()
        {
            InitLight();
        }

        private void InitLight()
        {
            if (lightComp == null) lightComp = GetComponent<Light>();
            if (pb == null) pb = new MaterialPropertyBlock();

            if (volObj == null)
            {
                volObj = new GameObject("VolumetricLight");

                volFilter = volObj.AddComponent<MeshFilter>();
                volRenderer = volObj.AddComponent<MeshRenderer>();
                SetupRenderer(volRenderer);
            }

            if (lightComp.type == LightType.Point)
            {
                volFilter.sharedMesh = Resources.Load<Mesh>("NL_Quad_mesh");
                volRenderer.sharedMaterial = Resources.Load<Material>("NL_Halo_mtl");
            }

            if (lightComp.type == LightType.Spot)
            {
                volFilter.sharedMesh = Resources.Load<Mesh>("NL_LightBeam_mesh");
                volRenderer.sharedMaterial = Resources.Load<Material>("NL_LightBeam_mtl");
            }

            volBounds = new Bounds();

            if (!realtimeUpdate) UpdateVolumetric();

            if (!volRenderer.enabled) volRenderer.enabled = true;
        }

        void OnEnable()
        {
            int curInstanceId = GetInstanceID();

            if (curInstanceId != instanceId)
            {
                instanceId = curInstanceId;


                if (SRS_Manager.srs_dataTransfer == null) SRS_Manager.srs_dataTransfer = NL_Utilities.FindObjectOfType<SRS_DataTransfer>(true);
                if (SRS_Manager.srs_dataTransfer == null) SRS_Manager.srs_dataTransfer = new GameObject("SRS_Data Transfer").AddComponent<SRS_DataTransfer>();


                randomValue = Random.Range(0, 0.99f);
            }

            InitLight();

            if (lightComp.type == LightType.Point) SRS_Manager.pointLights.Add(lightComp);
            if (lightComp.type == LightType.Spot) SRS_Manager.spotLights.Add(lightComp);

            onLightSwitch?.Invoke(lightComp, true);
        }

        private void SetupRenderer(MeshRenderer rend)
        {
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rend.receiveShadows = false;
            rend.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            rend.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            rend.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;

            rend.transform.parent = transform;
            rend.transform.localPosition = Vector3.zero;
            rend.transform.localEulerAngles = Vector3.zero;
        }

        private void OnDisable()
        {
            if (lightComp.type == LightType.Point) SRS_Manager.pointLights.Remove(lightComp);
            if (lightComp.type == LightType.Spot) SRS_Manager.spotLights.Remove(lightComp);
            volRenderer.enabled = false;

            onLightSwitch?.Invoke(lightComp, false);
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                if (lightType != lightComp.type) InitLight();
                lightType = lightComp.type;
            }

            if (!realtimeUpdate) return;
            if (volRenderer != null && volRenderer.enabled) UpdateVolumetric();
        }

        public void UpdateVolumetric()
        {
            if (lightComp.type == LightType.Spot)
            {
                endRadius = lightComp.range * (Mathf.Tan((lightComp.spotAngle / 2) * Mathf.Deg2Rad));
                volBounds.center = transform.position + transform.forward * lightComp.range * rangeMultiplier * 0.5f;
                mostFarPoint = Mathf.Sqrt(((lightComp.range * rangeMultiplier) / 2) * ((lightComp.range * rangeMultiplier) / 2) + endRadius * endRadius);
                volBounds.extents = Vector3.one * mostFarPoint;

                pb.SetVector("_beamDir", transform.forward);
                pb.SetFloat("_startRadius", beamStartRadius);
                pb.SetFloat("_endRadius", endRadius);
                pb.SetFloat("_length", lightComp.range);
                pb.SetFloat("_spotAngle", lightComp.spotAngle);
            }
            if (lightComp.type == LightType.Point)
            {
                volBounds.center = transform.position;
                volBounds.extents = Vector3.one * lightComp.range * rangeMultiplier;

                pb.SetFloat("_haloSize", volBounds.extents.x);
                pb.SetFloat("_zOffset", zOffset);

                volRenderer.transform.rotation = Quaternion.identity;
            }

            pb.SetFloat("_rangeMultiplier", rangeMultiplier);
            pb.SetColor("_color", lightComp.color);
            pb.SetFloat("_intensity", lightComp.intensity * intensityMultiplier);
            pb.SetFloat("_noiseIntensity", noiseIntensity);
            pb.SetVector("_noiseSpeed", noiseSpeed);
            pb.SetFloat("_noiseTiling", noiseTiling);
            pb.SetFloat("_maskHardness", maskHardness);
            pb.SetFloat("_intersectionsDepthFade", intersectionsDepthFade);
            pb.SetFloat("_cameraFadeDistance", cameraFadeDistance);
            pb.SetFloat("_randomValue", randomValue);
            if (noiseTexture != null) pb.SetTexture("_noise", noiseTexture);

            volRenderer.SetPropertyBlock(pb);

            volRenderer.bounds = volBounds;
        }

        public void DisableVolumetric()
        {
            if (volRenderer == null || !volRenderer.enabled) return;
            volRenderer.enabled = false;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            /*
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(beamBounds.center, 0.2f);

            Gizmos.DrawWireSphere(beamBounds.center, mostFarPoint);

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(beamBounds.center, beamBounds.size);
            */

            /*
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(haloBounds.center, haloBounds.size);
            */

        }
#endif
    }
}
