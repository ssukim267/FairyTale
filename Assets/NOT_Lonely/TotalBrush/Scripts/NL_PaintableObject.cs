#if UNITY_EDITOR
namespace NOT_Lonely.TotalBrush
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;

    [ExecuteInEditMode]
    //[Serializable]
    public class NL_PaintableObject : MonoBehaviour
    {
        public NL_PaintableObjectData data;
        public MeshFilter mFilter;
        public Mesh vStreamsMesh;
        public Mesh cachedStreams;
        public MeshRenderer mRenderer;
        public Color[] tempColors;
        public List<Vector3> unifiedNormals = new List<Vector3>();
        [NonSerialized] public MeshCollider mCollider;

        [HideInInspector] public int vCount;

        [HideInInspector] public int c_kernelId;
        [HideInInspector] public uint c_threadGroupSize;
        [HideInInspector] public int threadGroups;

        public int instanceId
        {
            get; private set;
        }

        private const int vertStride = sizeof(float) * (3 + 3 + 3 + 4);

        private void Awake()
        {
            if (Application.isPlaying)
                return;

            CheckInstance();
            UpdateVertexStreams();
        }

        public void CheckStreams()
        {
            Debug.Log($"{gameObject.name} streams = {mRenderer.additionalVertexStreams}");
        }

        private void CheckInstance()
        {
            int curInstanceId = GetInstanceID();

            if (curInstanceId != instanceId)
            {
                vStreamsMesh = null;
            }
        }

        private void OnEnable()
        {
            if(!Application.isPlaying)
                Undo.undoRedoPerformed += UpdateVertexStreamsOnUndo;
        }

        private void OnDisable()
        {
            if (!Application.isPlaying)
                Undo.undoRedoPerformed -= UpdateVertexStreamsOnUndo;
        }

        public void DeletePaintedData(bool restoreLastStreams)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            mRenderer.additionalVertexStreams = restoreLastStreams ? cachedStreams : null;

            tempColors = null;
            if (cachedStreams != null)
            {
                if (mRenderer.additionalVertexStreams != null) mRenderer.additionalVertexStreams.UploadMeshData(true);
                cachedStreams = null;
            }
            if (vStreamsMesh != null) DestroyImmediate(vStreamsMesh);
        }

        private void OnDestroy()
        {
            if (Application.isPlaying && !gameObject.scene.IsValid()) return;

            //mRenderer.additionalVertexStreams = null;
            DisposeAll();  
        }

        public void DisposeAll()
        {
            //tempColors = null;
            if (mCollider != null) DestroyImmediate(mCollider.gameObject);

            if (data == null) return;

            if (data.vertBufferSource != null) data.vertBufferSource.Dispose();
            if (data.vertBufferCalculated != null) data.vertBufferCalculated.Dispose();
            if (data.c_vPosBufferWS != null) data.c_vPosBufferWS.Dispose();
            if (data.c_vPosBufferSS != null) data.c_vPosBufferSS.Dispose();

            DestroyImmediate(data);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        public void InitPaintableObject()
        {
            mFilter = GetComponent<MeshFilter>();
            mRenderer = GetComponent<MeshRenderer>();

            CreateMeshCollider();

            data = ScriptableObject.CreateInstance<NL_PaintableObjectData>();

            CreateVertexStream(GetComponent<MeshFilter>().sharedMesh.vertices);
        }

        public void CreateMeshCollider()
        {
            mCollider = new GameObject($"{gameObject.name}_tempCollider").AddComponent<MeshCollider>();
            mCollider.transform.parent = transform.parent;
            mCollider.transform.localPosition = transform.localPosition;
            mCollider.transform.localScale = transform.localScale;
            mCollider.transform.localRotation = transform.localRotation;
            mCollider.sharedMesh = mFilter.sharedMesh;
            mCollider.gameObject.hideFlags = HideFlags.HideAndDontSave;
        }

        public void InitVertexStream()
        {
            if (mRenderer.additionalVertexStreams != null)
            {
                List<Vector3> uniN = new List<Vector3>();
                mRenderer.additionalVertexStreams.GetUVs(3, uniN);

                if (uniN.Count > 0) unifiedNormals = uniN;

                if (mRenderer.additionalVertexStreams.colors != null) GetColors(mRenderer.additionalVertexStreams.colors);
            }
        }

        public void CreateVertexStream(Vector3[] sourceVertices)
        {
            mRenderer = GetComponent<MeshRenderer>();
            if (vStreamsMesh == null)
            {
                vStreamsMesh = new Mesh();

                vStreamsMesh.vertices = sourceVertices;
                tempColors = new Color[sourceVertices.Length];

                Mesh sM = GetComponent<MeshFilter>().sharedMesh;

                //try get unified normals from vertex stream UV4
                if (mRenderer.additionalVertexStreams != null)
                {
                    mRenderer.additionalVertexStreams.GetUVs(3, unifiedNormals);
                }

                //if nothing returned at the previous step, try get original mesh normals
                if(unifiedNormals.Count == 0 && sM.normals != null && sM.normals.Length > 0)
                {
                    unifiedNormals = sM.normals.ToList();
                }

                if (mRenderer.additionalVertexStreams != null && mRenderer.additionalVertexStreams.colors != null && mRenderer.additionalVertexStreams.colors.Length > 0)
                {
                    cachedStreams = mRenderer.additionalVertexStreams;
                    tempColors = mRenderer.additionalVertexStreams.colors;  
                }
                else if ((sM.colors != null && sM.colors.Length > 0))
                {
                    tempColors = sM.colors;
                }
                else
                {
                    for (int i = 0; i < tempColors.Length; i++)
                    {
                        tempColors[i] = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                    }
                }
            }
            else
            {
                if (mRenderer.additionalVertexStreams != null && mRenderer.additionalVertexStreams.colors != null && mRenderer.additionalVertexStreams.colors.Length > 0)
                {

                }
                else
                {
                    tempColors = vStreamsMesh.colors;
                }
            }

            UpdateVertexStreams();
        }

        public void GetColors(Color[] sourceColors)
        {
            tempColors = sourceColors;
        }

        public void UpdateAveragedNormals(Vector3[] uniNormals)
        {
            if (uniNormals != null && uniNormals.Length > 0) unifiedNormals = uniNormals.ToList();
            UpdateVertexStreams();
        }

        public void UpdateVertexStreams()
        {
            if (mRenderer == null) return;

            if (vStreamsMesh == null)
            {
                vStreamsMesh = new Mesh();
                vStreamsMesh.vertices = mFilter.sharedMesh.vertices;
            }

            if(unifiedNormals.Count > 0)
            {
                if(unifiedNormals.Count != mFilter.sharedMesh.normals.Length)
                {
                    Debug.LogWarning($"{gameObject.name} object's stream vertex count is wrong. Please clear the stream.");
                    return;
                }
                vStreamsMesh.SetUVs(3, unifiedNormals);
            }

            if (tempColors != null && tempColors.Length > 0)
            {
                if (tempColors.Length != mFilter.sharedMesh.vertices.Length)
                {
                    Debug.LogWarning($"{gameObject.name} object's stream vertex count is wrong. Please clear the stream.");
                    return;
                }
                
                vStreamsMesh.SetColors(tempColors);
            }

            vStreamsMesh.UploadMeshData(true);
            mRenderer.additionalVertexStreams = vStreamsMesh;
           
            EditorUtility.SetDirty(mRenderer);
        }

        private void UpdateVertexStreamsOnUndo()
        {
            if (Selection.Contains(gameObject)) UpdateVertexStreams();
        }

        public void ConvertLocalToWorld(ComputeShader c_shader)
        {
            vCount = mFilter.sharedMesh.vertexCount;
            data.c_vPosBufferWS = new ComputeBuffer(vCount, sizeof(float) * 3);
            data.vPositionsWS = new Vector3[vCount];
            data.c_vPosBufferWS.SetData(mFilter.sharedMesh.vertices);

            c_kernelId = c_shader.FindKernel("ConvertLocalToWorld");
            c_shader.GetKernelThreadGroupSizes(c_kernelId, out c_threadGroupSize, out _, out _);

            c_shader.SetBuffer(c_kernelId, "vPositionsBufferWS", data.c_vPosBufferWS);
            c_shader.SetMatrix("localToWorldMatrix", mRenderer.localToWorldMatrix);
            c_shader.SetInt("vCount", vCount);

            threadGroups = Mathf.CeilToInt(((float)vCount / c_threadGroupSize));
            c_shader.Dispatch(c_kernelId, threadGroups, 1, 1);

            data.c_vPosBufferWS.GetData(data.vPositionsWS);

            data.c_vPosBufferWS.Dispose();
        }


        public void ConvertLocalToScreen(ComputeShader c_shader, Matrix4x4 viewProjMatrix, Vector2 cameraPixelSize)
        {
            vCount = mFilter.sharedMesh.vertexCount;
            data.c_vPosBufferSS = new ComputeBuffer(vCount, sizeof(float) * 3);
            data.vPositionsSS = new Vector3[vCount];
            data.c_vPosBufferSS.SetData(mFilter.sharedMesh.vertices);

            c_kernelId = c_shader.FindKernel("ConvertLocalToScreen");
            c_shader.GetKernelThreadGroupSizes(c_kernelId, out c_threadGroupSize, out _, out _);

            c_shader.SetBuffer(c_kernelId, "vPositionsBufferSS", data.c_vPosBufferSS);
            c_shader.SetMatrix("localToWorldMatrix", mRenderer.localToWorldMatrix);
            c_shader.SetMatrix("viewProjMatrix", viewProjMatrix);
            c_shader.SetVector("cameraPixelSize", cameraPixelSize);
            c_shader.SetInt("vCount", vCount);

            threadGroups = Mathf.CeilToInt(((float)vCount / c_threadGroupSize));
            c_shader.Dispatch(c_kernelId, threadGroups, 1, 1);

            data.c_vPosBufferSS.GetData(data.vPositionsSS);

            data.c_vPosBufferSS.Dispose();
        }

        public void InitVertColorBuffers(ComputeShader c_shader)
        {
            vCount = mFilter.sharedMesh.vertexCount;

            data.vertSource = new NL_PaintableObjectData.SourceVertex[vCount];
            data.vertCalculated = new NL_PaintableObjectData.CalculatedVertex[vCount];
            data.vertBufferSource = new GraphicsBuffer(GraphicsBuffer.Target.Structured, data.vertSource.Length, vertStride);
            data.vertBufferCalculated = new GraphicsBuffer(GraphicsBuffer.Target.Structured, data.vertSource.Length, vertStride);
        }
    }
}
#endif
