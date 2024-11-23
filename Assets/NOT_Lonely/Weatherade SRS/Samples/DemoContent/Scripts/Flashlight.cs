using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NOT_Lonely.Weatherade;

public class Flashlight : MonoBehaviour
{
    public bool state = false;
    public Light spotLight;
    public AudioSource soundFx;
    private NL_VolumetricLight[] volumetrics;
    
    // Start is called before the first frame update
    void Awake()
    {
        volumetrics = spotLight.GetComponentsInChildren<NL_VolumetricLight>();

        spotLight.enabled = state;
        for (int i = 0; i < volumetrics.Length; i++)
        {
            volumetrics[i].enabled = state;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            spotLight.enabled = !spotLight.enabled;
            for (int i = 0; i < volumetrics.Length; i++)
            {
                volumetrics[i].enabled = !volumetrics[i].enabled;
            }

            if (soundFx != null) soundFx.Play();
        }
    }
}
