using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NL_Thundershtorm;

public class NL_Thundershtorm : MonoBehaviour
{
    public bool playOnEnable = true;
    [Range(1, 60)] public float intervalMin = 5;
    [Range(1, 60)] public float intervalMax = 15;
    [Range(500, 3000)] public float lightningDistanceMin = 500;
    [Range(500, 3000)] public float lightningDistanceMax = 3000;

    [Serializable]
    public class Lightning
    {
        public MeshRenderer meshRenderer;
        public float lightSourceRange = 1500;
        public float intensity = 2;
        public float durationMin = 0.05f;
        public float durationMax = 0.6f;
        public Color lightColor = Color.white;
        [ColorUsage(true, true)]public Color lightningColor = Color.white;
        public LayerMask layerMask = ~0;
        [HideInInspector]public Light lightSource;
    }

    public Lightning lightning;

    public AudioClip[] thunderSoundFX;

    private AudioSource audioSource;

    private AudioClip lastClip;
    private AudioClip curClip;

    private readonly float soundSpeed = 343;
    private readonly Vector2 lightningHorizonPosMinMax = new Vector2(0.8f, 0.95f);

    void Awake()
    {
        SetupLight();
        SetupAudioSource();

        lightning.meshRenderer.enabled = false;
    }

    private void OnEnable()
    {
        if (playOnEnable) StartCoroutine(PlayRandomly());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        if(lightning.lightSource != null) lightning.lightSource.intensity = 0;
    }

    private void SetupAudioSource()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void SetupLight()
    {
        lightning.lightSource = new GameObject("Lightning Light").AddComponent<Light>();
        lightning.lightSource.type = LightType.Point;
        lightning.lightSource.intensity = 0;
        lightning.lightSource.range = lightning.lightSourceRange;
        lightning.lightSource.shadows = LightShadows.Hard;
        lightning.lightSource.cullingMask = lightning.layerMask;
    }

    private IEnumerator PlayRandomly()
    {
        while (true)
        {
            float wait = UnityEngine.Random.Range(intervalMin, intervalMax);
            float distance = UnityEngine.Random.Range(lightningDistanceMin, lightningDistanceMax);

            yield return new WaitForSeconds(wait);

           StartCoroutine(MakeLightning(distance));
        }
    }

    private IEnumerator MakeLightning(float distance)
    {
        Transform camTransform = Camera.main.transform;

        Vector3 randomPoint = UnityEngine.Random.onUnitSphere;

        randomPoint.y = Mathf.Abs(randomPoint.y);

        if (randomPoint.y < lightningHorizonPosMinMax.x) randomPoint.y = lightningHorizonPosMinMax.x;
        if (randomPoint.y > lightningHorizonPosMinMax.y) randomPoint.y = lightningHorizonPosMinMax.y;

        Vector3 lightPos = camTransform.position + randomPoint * distance;
        
        lightning.lightSource.transform.position = lightPos;

        yield return StartCoroutine(LightningAnim(distance));

        yield return new WaitForSeconds(distance / soundSpeed);

        StartCoroutine(MakeThunder(distance));
    }

    private IEnumerator LightningAnim(float distance)
    {
        float t = UnityEngine.Random.Range(lightning.durationMin, lightning.durationMax);
        float intensity = 0;

        Shader.SetGlobalFloat("LightningRadius", 1);
        Shader.SetGlobalColor("LightningColor", lightning.lightColor);
        Shader.SetGlobalVector("LightningPosition", lightning.lightSource.transform.position);

        lightning.meshRenderer.transform.position = lightning.lightSource.transform.position;
        lightning.meshRenderer.transform.localScale = new Vector3(0.25f, 1.5f, 0.25f) * distance * 0.5f;
        lightning.meshRenderer.material.color = lightning.lightningColor;

        lightning.meshRenderer.enabled = true;

        StartCoroutine(FadeLightning(lightning.durationMax));

        while (t > 0)
        {
            intensity = UnityEngine.Random.Range(0, lightning.intensity);
            lightning.lightSource.intensity = intensity;
            Shader.SetGlobalFloat("LightningIntensity", intensity);

            t -= Time.deltaTime;
            yield return null;
        }

        lightning.lightSource.intensity = 0;
        Shader.SetGlobalFloat("LightningIntensity", 0);
        
    }

    IEnumerator FadeLightning(float duration)
    {
        float t = 1;
        while (t > 0)
        {
            t -= Time.deltaTime;
            lightning.meshRenderer.material.SetFloat("_Intensity", Mathf.Lerp(0, lightning.intensity, t));
            yield return null; 
        }

        lightning.meshRenderer.material.SetFloat("_Intensity", 0);
        lightning.meshRenderer.enabled = false;
    }

    private IEnumerator MakeThunder(float distance)
    {
        if (thunderSoundFX.Length > 1)
        {
            while (lastClip == curClip)
            {
                curClip = thunderSoundFX[UnityEngine.Random.Range(0, thunderSoundFX.Length)];
                yield return null;
            }
        }
        else
        {
            curClip = thunderSoundFX[0];
            yield return null;
        }

        if (curClip != null)
        {
            audioSource.pitch = UnityEngine.Random.Range(0.6f, 1);
            audioSource.PlayOneShot(curClip);
            lastClip = curClip;
        }
        else
        {
            Debug.LogWarning("No Thunder Sound FX is null!");
        }
    }
}
