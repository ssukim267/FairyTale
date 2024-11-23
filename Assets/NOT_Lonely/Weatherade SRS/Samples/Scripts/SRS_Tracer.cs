using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SRS_Tracer : MonoBehaviour
{
    public string tracerLayerName = "Dynamic";
    public bool parent = true;
    public float vertexPush = 0;
    public Renderer[] renderers;

    private List<Transform> tracers = new List<Transform>();

    void OnEnable()
    {
        SetupTracers();
    }

    private void Update()
    {
        if (parent) return;

        for (int i = 0; i < tracers.Count; i++)
        {
            tracers[i].transform.SetPositionAndRotation(renderers[i].transform.position, renderers[i].transform.rotation);
            tracers[i].transform.localScale = renderers[i].transform.lossyScale;
        }
    }

    private void SetupTracers()
    {
        tracers = new List<Transform>();

        Material tracerMtl = new Material(Shader.Find("NOT_Lonely/Weatherade/Extra/NL_DepthOccluder"));
        tracerMtl.name = "TracerMaterial";
        tracerMtl.SetFloat("_VertexPush", vertexPush);

        if (renderers == null || renderers.Length == 0)
        {
            renderers = new Renderer[1];
            renderers[0] = GetComponent<Renderer>();
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;

            GameObject tracerObj = new GameObject($"{renderers[i].name}_SRS Tracer");

            tracerObj.layer = LayerMask.NameToLayer(tracerLayerName);

            if (parent) tracerObj.transform.parent = renderers[i].transform;
            else tracers.Add(tracerObj.transform);

            tracerObj.transform.localPosition = Vector3.zero;
            transform.localEulerAngles = Vector3.zero;
            transform.localScale = Vector3.one;

            Material[] mtls = renderers[i].materials;
            for (int m = 0; m < mtls.Length; m++)
            {
                mtls[m] = tracerMtl;
            }

            if (renderers[i] is MeshRenderer)
            {
                MeshFilter mFilter = tracerObj.AddComponent<MeshFilter>();
                MeshRenderer mRnd = tracerObj.AddComponent<MeshRenderer>();
                mFilter.sharedMesh = renderers[i].GetComponent<MeshFilter>().sharedMesh;
                SetupRenderer(mRnd, mtls);
            }
            else if(renderers[i] is SkinnedMeshRenderer)
            {
                SkinnedMeshRenderer srcSRnd = renderers[i] as SkinnedMeshRenderer;
                SkinnedMeshRenderer sRnd = tracerObj.AddComponent<SkinnedMeshRenderer>();
                sRnd.sharedMesh = srcSRnd.sharedMesh;
                sRnd.localBounds = srcSRnd.localBounds;
                sRnd.quality = srcSRnd.quality;
                sRnd.updateWhenOffscreen = srcSRnd.updateWhenOffscreen;
                sRnd.bones = srcSRnd.bones;
                sRnd.rootBone = srcSRnd.rootBone;
                sRnd.skinnedMotionVectors = false;
                SetupRenderer(sRnd, mtls);
            }
            
            /*
            
            mRnd.materials = mtls;
            mRnd.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mRnd.receiveGI = ReceiveGI.Lightmaps;
            mRnd.receiveShadows = false;
            mRnd.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            */
        }
    }

    private void SetupRenderer(Renderer rnd, Material[] mtls)
    {
        rnd.materials = mtls;
        rnd.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        rnd.receiveShadows = false;
        rnd.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        rnd.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
    }
}
