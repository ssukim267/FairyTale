using NOT_Lonely.Weatherade;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SRS_Manager
{
    public static SRS_DataTransfer srs_dataTransfer;
    public static List<Material> coverageMaterials = new List<Material>();
    public static List<Light> pointLights = new List<Light>();
    public static List<Light> spotLights = new List<Light>();

    public static void InitDataTransfer()
    {
        if (srs_dataTransfer == null) srs_dataTransfer = NL_Utilities.FindObjectOfType<SRS_DataTransfer>(true);
        if (srs_dataTransfer == null) srs_dataTransfer = new GameObject("SRS_Data Transfer").AddComponent<SRS_DataTransfer>();
    }
}
