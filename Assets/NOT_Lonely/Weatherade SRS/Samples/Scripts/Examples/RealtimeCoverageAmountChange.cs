using NOT_Lonely.Weatherade;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealtimeCoverageAmountChange : MonoBehaviour
{
    [SerializeField] private float startAmount = 0;
    [SerializeField] private float endAmount = 0.75f;

    [SerializeField] private bool changeOnStart = true;
    [SerializeField] private float changeTime = 15;

    private float changeSpeed => 1 / changeTime;

    private SnowCoverage snowCoverage;


    void Start()
    {
        snowCoverage = (SnowCoverage)CoverageBase.instance;

        if (changeOnStart)
            StartCoroutine(ChangeCoverageAmountGradually());
    }

    private IEnumerator ChangeCoverageAmountGradually()
    {
        float t = 0;
        while(t < 1)
        {
            t += Time.deltaTime * changeSpeed;
            SetCoverageAmount(Mathf.Lerp(startAmount, endAmount, t));
            yield return null;
        }

        SetCoverageAmount(endAmount);
    }

    /// <summary>
    /// Set the amount of snow coverage.
    /// </summary>
    /// <param name="amount">Representation of the 'Amount' value of the Snow Coverage Instance. 0-1 range is used.</param>
    public void SetCoverageAmount(float amount)
    {
        snowCoverage.coverageAmount = amount;
        snowCoverage.UpdateCoverageMaterials();
    }
}
