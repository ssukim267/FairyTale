using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NOT_Lonely.Weatherade;
using UnityEngine.Events;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif
using static NL_TimeOfDay;

#if UNITY_EDITOR
using UnityEditor;
[ExecuteInEditMode]
#endif
public class NL_TimeOfDay : MonoBehaviour
{
    [Serializable]
    public class Sun
    {
        public Vector3 rotation;
        public float intensity;
        [ColorUsage(false)]public Color color;
    }

    [Serializable]
    public class Skybox
    {
        [Header("Colors")]
        [ColorUsage(false, true)] public Color skyColor = new Color(1.2f, 2.1f, 3.5f);
        [ColorUsage(false, true)] public Color groundColor = new Color(0.2f, 0.15f, 0.1f);
       
        [Header("Fog")]        
        [Range(0, 1)] public float fogHeight = 1;
        [Range(0, 1)] public float fogSmoothness = 0.3f;
        [Range(0, 1)] public float fogFill = 0;

        [Header("Sundisk")]
        [Range(0, 1)] public float sunSize = 0.02f;
        [Range(0, 1)] public float sunSharpness = 0.999f;
        public float sunIntensity = 10;

        [Header("Scattering")]
        [Range(0, 2)] public float scatteringAmount = 1.2f;
        public float scatteringbrightness = 6;

        [Header("Clouds")]
        public float cloudsIntensity = 0.35f;
    }

    [Serializable]
    public class Fog
    {
        [ColorUsage(false)] public Color fogColor = new Color(0.78f, 0.92f, 1f);
        [Range(0, 1)] public float fogDensity = 0.008f;
    }

    [Serializable]
    public class ArtificialLight
    {
        public string name = "Light Name";
        public bool state = false;
        [Tooltip("Normalized transition time, when the light will be switched.")]
        [Range(0, 1)] public float switchTime = 0.8f;
        public Light[] lights;
        public Renderer[] emissiveRenderers;
        [ColorUsage(false, true)] public Color color;
        public UnityEvent OnSwitched;
        public MaterialPropertyBlock[] propBlocks;
    }

    [Serializable]
    public class TimeOfDayState
    {
        public string name = "Default State";
        public float transitionSpeed = 0.1f;
        public Sun sun;
        public Skybox skybox;
        [ColorUsage(false, true)] public Color rainbowColor = Color.black;
        public Fog fog;
        public ArtificialLight[] artificialLights;
        public ReflectionProbe[] reflectionProbes;
#if UNITY_POST_PROCESSING_STACK_V2
        public PostProcessVolume[] postProcessingVolumes;
#endif
        [ColorUsage(true, true), Tooltip("The number of elements must be equal to the 'Srs Particles' count.")]
        public Color[] srsParticleColors;
        
        [Space(10)]
        public UnityEvent OnTransitionStart;
        public UnityEvent OnTransitionComplete;
        public UnityEvent OnTransitionToThisStateComplete;
    }

    [SerializeField, HideInInspector] private int currStateId = 0;
    [SerializeField, HideInInspector] private int targetStateId;
    [HideInInspector] public Material skyMtl;
    private WaitForSeconds waitFor;

    [Range(0, 5), Tooltip("The state update interval in seconds. If set to 0, then will updated every frame.")]
    public float updateInterval = 0;
    public AnimationCurve transitionCurve;
    public Light sunlight;
    public MeshRenderer rainbowRenderer;
    public SRS_ParticleSystem[] srsParticles;
    public TimeOfDayState[] states;

    private Coroutine updateRoutine = null;

    private void OnEnable()
    {
        if (!Application.isPlaying)
        {
            GetRendererPropertyBlocks();

            skyMtl = RenderSettings.skybox;
            ChangeStateInstant(currStateId);
        }
    }

    private void GetRendererPropertyBlocks()
    {
        for (int i = 0; i < states.Length; i++)
        {
            if (states[i].artificialLights == null) return;

            for (int j = 0; j < states[i].artificialLights.Length; j++)
            {
                states[i].artificialLights[j].propBlocks = new MaterialPropertyBlock[states[i].artificialLights[j].emissiveRenderers.Length];

                for (int k = 0; k < states[i].artificialLights[j].emissiveRenderers.Length; k++)
                {
                    if (states[i].artificialLights[j].emissiveRenderers[k] != null)
                    {
                        states[i].artificialLights[j].propBlocks[k] = new MaterialPropertyBlock();
                        states[i].artificialLights[j].emissiveRenderers[k].GetPropertyBlock(states[i].artificialLights[j].propBlocks[k]);
                    }
                }
            }
        }
    }

    private void Awake()
    {
        if (!Application.isPlaying) return;

        GetRendererPropertyBlocks();
        skyMtl = RenderSettings.skybox;
        waitFor = new WaitForSeconds(updateInterval);
        ChangeStateInstant(currStateId);
    }

    private void Start()
    {

    }

    public void ValidateValues()
    {
        targetStateId = Mathf.Clamp(targetStateId, 0, states.Length - 1);
    }

    /*
    private WaitForSeconds envirUpdWaitInterval = new WaitForSeconds(1);
    private Coroutine updateEnvirRoutine;
    IEnumerator UpdateEnvir()
    {
        while (true)
        {
            if(lerpValue < 1)
            {
                DynamicGI.UpdateEnvironment();
            }
            else
            {
                DynamicGI.UpdateEnvironment();
                updateEnvirRoutine = null;
                yield break;
            }

            Debug.Log("call");
            
            yield return envirUpdWaitInterval;
        }
    }
    */

    private void UpdateProps(int stateId, float lerpValue)
    {
        TimeOfDayState curState = states[currStateId];
        TimeOfDayState targState = states[stateId];

        if (sunlight != null) 
        {
            sunlight.transform.eulerAngles = Vector3.Slerp(curState.sun.rotation, targState.sun.rotation, lerpValue);
            sunlight.color = Color.Lerp(curState.sun.color, targState.sun.color, lerpValue);
            sunlight.intensity = Mathf.Lerp(curState.sun.intensity, targState.sun.intensity, lerpValue);
        }

        if(skyMtl != null)
        {
            skyMtl.SetColor("_Color", Color.Lerp(curState.skybox.skyColor, targState.skybox.skyColor, lerpValue));
            skyMtl.SetColor("_GroundColor", Color.Lerp(curState.skybox.groundColor, targState.skybox.groundColor, lerpValue));
            skyMtl.SetFloat("_FogHeight", Mathf.Lerp(curState.skybox.fogHeight, targState.skybox.fogHeight, lerpValue));
            skyMtl.SetFloat("_FogSmoothness", Mathf.Lerp(curState.skybox.fogSmoothness, targState.skybox.fogSmoothness, lerpValue));
            skyMtl.SetFloat("_FogFill", Mathf.Lerp(curState.skybox.fogFill, targState.skybox.fogFill, lerpValue));
            skyMtl.SetFloat("_SunDiskSize", Mathf.Lerp(curState.skybox.sunSize, targState.skybox.sunSize, lerpValue));
            skyMtl.SetFloat("_SunDiskSharpness", Mathf.Lerp(curState.skybox.sunSharpness, targState.skybox.sunSharpness, lerpValue));
            skyMtl.SetFloat("_SunDiskIntensity", Mathf.Lerp(curState.skybox.sunIntensity, targState.skybox.sunIntensity, lerpValue));
            skyMtl.SetFloat("_ScatteringSize", Mathf.Lerp(curState.skybox.scatteringAmount, targState.skybox.scatteringAmount, lerpValue));
            skyMtl.SetFloat("_ScatteringBrightness", Mathf.Lerp(curState.skybox.scatteringbrightness, targState.skybox.scatteringbrightness, lerpValue));
            skyMtl.SetFloat("_CloudsIntensity", Mathf.Lerp(curState.skybox.cloudsIntensity, targState.skybox.cloudsIntensity, lerpValue));
        }

        if(rainbowRenderer != null)
        {
            rainbowRenderer.sharedMaterial.color = Color.Lerp(curState.rainbowColor, targState.rainbowColor, lerpValue);
        }

        RenderSettings.fogColor = Color.Lerp(curState.fog.fogColor, targState.fog.fogColor, lerpValue);
        RenderSettings.fogDensity = Mathf.Lerp(curState.fog.fogDensity, targState.fog.fogDensity, lerpValue);

        if(targState.artificialLights != null)
        {
            GetRendererPropertyBlocks();

            for (int i = 0; i < targState.artificialLights.Length; i++)
            {
                if (lerpValue >= targState.artificialLights[i].switchTime) 
                {
                    //Switch light sources
                    if (targState.artificialLights[i].lights != null)
                    {
                        for (int j = 0; j < targState.artificialLights[i].lights.Length; j++)
                        {
                            if (targState.artificialLights[i].lights[j] != null)
                                targState.artificialLights[i].lights[j].enabled = targState.artificialLights[i].state;
                        }
                    }
                    //Switch emission
                    for (int r = 0; r < targState.artificialLights[i].emissiveRenderers.Length; r++)
                    {
                        if (targState.artificialLights[i].emissiveRenderers[r] != null)
                        {
                            targState.artificialLights[i].propBlocks[r].SetColor("_EmissionColor", targState.artificialLights[i].color);
                            targState.artificialLights[i].emissiveRenderers[r].SetPropertyBlock(targState.artificialLights[i].propBlocks[r]);
                        }
                    }

                    targState.artificialLights[i].OnSwitched.Invoke();
                }
            }
        }

        if(targState.reflectionProbes != null)
        {
            for (int i = 0; i < targState.reflectionProbes.Length; i++)
            {
                if (targState.reflectionProbes[i] != null)
                    targState.reflectionProbes[i].RenderProbe();
            }
        }

        if(srsParticles != null)
        {
            for (int i = 0; i < srsParticles.Length; i++)
            {
                if (srsParticles[i] != null)
                {
                    srsParticles[i].colorMultiplier = Color.Lerp(curState.srsParticleColors[i], targState.srsParticleColors[i], lerpValue);
                }
            }
        }

#if UNITY_POST_PROCESSING_STACK_V2
        if (curState.postProcessingVolumes != null)
        {
            for (int i = 0; i < curState.postProcessingVolumes.Length; i++)
            {
                if (curState.postProcessingVolumes[i] != null)
                    curState.postProcessingVolumes[i].weight = Mathf.Lerp(1, 0, lerpValue);
            }
        }

        if (targState.postProcessingVolumes != null)
        {
            for (int i = 0; i < targState.postProcessingVolumes.Length; i++)
            {
                if (targState.postProcessingVolumes[i] != null)
                    targState.postProcessingVolumes[i].weight = Mathf.Lerp(0, 1, lerpValue);
            }
        }
#endif

        DynamicGI.UpdateEnvironment();
    }

    public void ChangeStateGradually(int stateId)
    {
        if (updateRoutine == null && stateId != currStateId) updateRoutine = StartCoroutine(UpdateState(stateId));
    }

    public void ChangeStateInstant(int stateId)
    {
        UpdateProps(stateId, 1);
        currStateId = stateId;
    }

    //float lerpValue;
    private IEnumerator UpdateState(int stateId, bool instant = false)
    {
        float t = 0;
        float lerpVal = 0;

        states[stateId].OnTransitionStart.Invoke();

        /*
        if(updateEnvirRoutine != null) StopCoroutine(updateEnvirRoutine);
        updateEnvirRoutine = StartCoroutine(UpdateEnvir());
        */

        while (true)
        {
            if(t < 1)
            {
                t += Time.deltaTime * states[stateId].transitionSpeed;
                lerpVal = transitionCurve.Evaluate(t);
                //lerpValue = lerpVal;
                UpdateProps(stateId, lerpVal);
            }
            else
            {
                //lerpValue = lerpVal;
                UpdateProps(stateId, 1);
                states[stateId].OnTransitionComplete.Invoke();

                currStateId = stateId;

                updateRoutine = null;

                yield break;
            }

            if (updateInterval == 0) yield return null;
            else yield return waitFor;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(NL_TimeOfDay))]
public class NL_TimeOfDay_UI : Editor
{
    private NL_TimeOfDay timeOfDayController;

    SerializedProperty targetStateId;

    private void OnEnable()
    {
        timeOfDayController = (NL_TimeOfDay)target;

        targetStateId = serializedObject.FindProperty("targetStateId");
    }
    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();

        EditorGUILayout.Space(5);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(targetStateId, new GUIContent("Change State", "State ID to transition."));
        if (GUILayout.Button(new GUIContent("Go To State", "Change all the properties to the selection state.")))
        {
            timeOfDayController.skyMtl = RenderSettings.skybox;
            timeOfDayController.ChangeStateInstant(targetStateId.intValue);
        }

        EditorGUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            
            serializedObject.ApplyModifiedProperties();
            timeOfDayController.ValidateValues();
        }
    }
}
#endif
