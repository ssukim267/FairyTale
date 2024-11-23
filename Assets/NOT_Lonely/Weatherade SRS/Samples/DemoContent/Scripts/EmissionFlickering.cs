using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmissionFlickering : MonoBehaviour
{
    [Tooltip("Put MeshRenderes here")]
    public MeshRenderer[] objectsToFlicker;
    [Tooltip("If enabled, the flickering animation will be applied simultaneously to all filering objects. If disabled, the objects will flicker in randomly.")]
    public bool simultaneouslyForAll = true;
    [Tooltip("Speed of the flickering animation.")]
    public Vector2 flickerSpeedMinMax = new Vector2(0.7f, 1);
    [Tooltip("Min and max intensities.")]
    public Vector2 flickerIntensityMinMax = new Vector2(1f, 1.2f);
    [Tooltip("Flickering animation update interval. 0 - update every frame, 1 - update once per second.")]
    public float updateInterval = 0.1f;

    private MaterialPropertyBlock[] propertyBlocks;
    private Color[] colors;
    private float currentIntensity;
    private float currentFlickerSpeed;
    private float[] currentFlickerSpeeds;
    private WaitForSeconds waitForRndSpeedChange;
    private WaitForSeconds waitForFlicker;

    private void Awake()
    {
        propertyBlocks = new MaterialPropertyBlock[objectsToFlicker.Length];
        colors = new Color[objectsToFlicker.Length];
        currentIntensity = Random.Range(flickerIntensityMinMax.x, flickerIntensityMinMax.y);
        currentFlickerSpeed = Random.Range(flickerSpeedMinMax.x, flickerSpeedMinMax.y);

        if(!simultaneouslyForAll) currentFlickerSpeeds = new float[objectsToFlicker.Length];

        for (int i = 0; i < objectsToFlicker.Length; i++)
        {
            propertyBlocks[i] = new MaterialPropertyBlock();
            colors[i] = objectsToFlicker[i].sharedMaterial.GetColor("_EmissionColor");
        }

        waitForRndSpeedChange = new WaitForSeconds(1);
        waitForFlicker = new WaitForSeconds(updateInterval);

        StartCoroutine(Flicker());
    }

    private float GetIntensity(int speedId = -1)
    {
        float speed = speedId == -1 ? currentFlickerSpeed : currentFlickerSpeeds[speedId];
        return Mathf.PingPong(Time.time * speed, flickerIntensityMinMax.y - flickerIntensityMinMax.x) + flickerIntensityMinMax.x;
    }

    IEnumerator Flicker()
    {
        StartCoroutine(GetRandomSpeed());

        while (true)
        {
            if(simultaneouslyForAll)
                currentIntensity = GetIntensity();

            for (int i = 0; i < objectsToFlicker.Length; i++)
            {
                float intensity = 0;
                if (simultaneouslyForAll)
                    intensity = currentIntensity;
                else
                    intensity = GetIntensity(i);

                Color emissionColor = objectsToFlicker[i].sharedMaterial.GetColor("_EmissionColor");

                emissionColor *= intensity;

                propertyBlocks[i].SetColor("_EmissionColor", emissionColor);
                objectsToFlicker[i].SetPropertyBlock(propertyBlocks[i]);
            }

            //Check if update interval is to low, update every frame instead to prevent over calculations
            if(updateInterval < 0.0222f)
                yield return null;
            else
                yield return waitForFlicker;
        }
    }

    IEnumerator GetRandomSpeed()
    {
        while (true)
        {
            if(simultaneouslyForAll)
                currentFlickerSpeed = Random.Range(flickerSpeedMinMax.x, flickerSpeedMinMax.y);
            else
            {
                for (int i = 0; i < currentFlickerSpeeds.Length; i++)
                {
                    currentFlickerSpeeds[i] = Random.Range(flickerSpeedMinMax.x, flickerSpeedMinMax.y);
                }
            }

            yield return waitForRndSpeedChange;
        }
    }
}
