using UnityEngine;
using System.Collections;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    public float updateInterval = 0.2f;
    private float count;
    private TextMeshProUGUI text;
    private WaitForSeconds waitFor;
    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        waitFor = new WaitForSeconds(updateInterval);
    }

    private void OnEnable()
    {
        StartCoroutine(CountFPS());
    }

    IEnumerator CountFPS()
    {
        while (true)
        {
            count = 1f / Time.unscaledDeltaTime;
            text.text = Mathf.Round(count).ToString();
            yield return waitFor;
        }
    }
}