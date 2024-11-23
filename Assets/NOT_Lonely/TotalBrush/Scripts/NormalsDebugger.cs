#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class NormalsDebugger : MonoBehaviour
{
    [SerializeField] private bool showOriginalNormals;
    [SerializeField] private bool showUnifiedNormals;
    [SerializeField] private bool showVertices;
    [SerializeField] private float length = 0.5f;
    [SerializeField] private float verticesSize = 0.005f;

    [SerializeField] private Vector3[] vPos;
    [SerializeField] private Vector3[] originalNormals;
    [SerializeField] private List<Vector3> unifiedNormals = new List<Vector3>();

    private void OnEnable()
    {
        originalNormals = gameObject.GetComponent<MeshFilter>().sharedMesh.normals;
        vPos = gameObject.GetComponent<MeshFilter>().sharedMesh.vertices;

        Mesh streams = gameObject.GetComponent<MeshRenderer>().additionalVertexStreams;
        unifiedNormals = new List<Vector3>();
        if (streams != null)
        {
            streams.GetUVs(3, unifiedNormals);
        }
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.matrix = transform.localToWorldMatrix;
        Handles.matrix = transform.localToWorldMatrix;

        for (int i = 0; i < vPos.Length; i++)
        {
            if (showOriginalNormals)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(vPos[i], originalNormals[i] * length);
            }

            if (showVertices)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(vPos[i], Vector3.one * verticesSize);
            }

            if (showUnifiedNormals)
            {
                if (unifiedNormals.Count > 0)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawRay(vPos[i], unifiedNormals[i] * length);
                }
            }
            //Handles.Label(vPos[i] + unifiedNormals[i] * length, i.ToString());
        }
    }
}
#endif
