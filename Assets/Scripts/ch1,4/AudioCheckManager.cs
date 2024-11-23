using System.Collections;
using UnityEngine;


public class AudioCheckManager : MonoBehaviour
{
    public AudioSource audioSource; 
    public int sampleSize = 300; 
    public float silenceThreshold = 0.1f; 
    public float maxSilenceTime = 5f;

    private bool isChecking = false;

    public void StartChecking(System.Action onSilenceDetected)
    {
        if (isChecking) return;

        isChecking = true;
        StartCoroutine(CheckSilence(onSilenceDetected));
    }

    public void StopChecking()
    {
        isChecking = false;
        StopAllCoroutines();
    }

    private IEnumerator CheckSilence(System.Action onSilenceDetected)
    {
        float[] samples = new float[sampleSize];
        float silenceDuration = 0f;

        while (isChecking)
        {
            
            audioSource.GetOutputData(samples, 0);

            bool hasSound = false;
            foreach (float sample in samples)
            {
                if (Mathf.Abs(sample) > silenceThreshold)
                {
                    hasSound = true;
                    break;
                }
            }

            if (!hasSound)
            {
                silenceDuration += Time.deltaTime;

                if (silenceDuration >= maxSilenceTime)
                {
                    onSilenceDetected?.Invoke();
                    yield break;
                }
            }
            else
            {
                silenceDuration = 0f; 
            }

            yield return null;
        }
    }
}
