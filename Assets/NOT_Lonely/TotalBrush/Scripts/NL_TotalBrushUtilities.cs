#if UNITY_EDITOR
namespace NOT_Lonely.TotalBrush
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public class NL_TotalBrushUtilities
    {
        public static string GetToolPath(ScriptableObject so)
        {
            var script = MonoScript.FromScriptableObject(so);
            string toolRootPath = AssetDatabase.GetAssetPath(script);
            toolRootPath = toolRootPath.Replace('\\', '/');
            toolRootPath = toolRootPath.Replace("Scripts/Editor/" + script.name + ".cs", "");

            return toolRootPath;
        }

        public static int[] ConvertChannelMask(bool[] sourceChannelMask)
        {
            int[] cm = new int[sourceChannelMask.Length];

            for (int i = 0; i < cm.Length; i++)
            {
                cm[i] = sourceChannelMask[i] ? 1 : 0;
            }

            return cm;
        }

        public static void SetSourceVetData(NL_PaintableObject obj, ComputeShader c_shader, string kernelName, Vector3[] positions, Vector3[] positionsSS, Vector3[] normals, Color[] colors)
        {
            obj.c_kernelId = c_shader.FindKernel(kernelName);
            c_shader.GetKernelThreadGroupSizes(obj.c_kernelId, out obj.c_threadGroupSize, out _, out _);
            obj.threadGroups = Mathf.CeilToInt(((float)obj.vCount / obj.c_threadGroupSize));

            c_shader.SetInt("vCount", obj.vCount);

            for (int i = 0; i < obj.vCount; i++)
            {
                if (positions != null) obj.data.vertSource[i].pos = positions[i];
                if (positionsSS != null) obj.data.vertSource[i].posSS = positionsSS[i];
                if (normals != null) obj.data.vertSource[i].normal = normals[i];
                if (colors != null) obj.data.vertSource[i].color = colors[i];
            }

            c_shader.SetBuffer(obj.c_kernelId, "SourceVertices", obj.data.vertBufferSource);
            c_shader.SetBuffer(obj.c_kernelId, "CalculatedVertices", obj.data.vertBufferCalculated);
        }

        public static void CalculateAndApplyVertColors(NL_PaintableObject obj, ComputeShader c_shader)
        {
            obj.data.vertBufferSource.SetData(obj.data.vertSource);

            c_shader.Dispatch(obj.c_kernelId, obj.threadGroups, 1, 1);

            obj.data.vertBufferCalculated.GetData(obj.data.vertCalculated);

            for (int i = 0; i < obj.vCount; i++)
            {
                obj.tempColors[i] = obj.data.vertCalculated[i].color;
                //Debug.Log($"{obj.gameObject.name} color = {obj.tempColors[i]}");
            }

            obj.UpdateVertexStreams();
        }

        public static float CalculateChannel(float channelValue, float targetVal, float opacity, float toolChannelColor, float falloff = 1)
        {
            float c = channelValue;
            float finalBrush = toolChannelColor * falloff * opacity;

            c += finalBrush;

            c = Mathf.Clamp(c, 0.01f, targetVal);
            c = Mathf.Lerp(channelValue, c, opacity < 0 ? toolChannelColor * falloff * 0.1f : finalBrush);

            return c;
        }

        public static Texture2D CreateGradientTex(int width, int height)
        {
            Texture2D tex = new Texture2D(width, height);

            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Point;
            tex.anisoLevel = 0;

            return tex;
        }

        public static Texture2D UpdateGradientTex(Texture2D gradientTex, Gradient colorGradient, float linearIntensity)
        {
            for (int x = 0; x < gradientTex.width; x++)
            {
                float t = (float)x / gradientTex.width;

                Color gColor = colorGradient.Evaluate(t) * linearIntensity;

                gColor = gColor.gamma;
                gradientTex.SetPixel(x, 0, gColor);
            }

            gradientTex.Apply();

            return gradientTex;
        }

        public static Texture2D UpdateGradientTex(Texture2D gradientTex, Color color, AnimationCurve curve)
        {
            for (int x = 0; x < gradientTex.width; x++)
            {
                float t = (float)x / gradientTex.width;

                color *= curve.Evaluate(t);

                color = color.gamma;
                gradientTex.SetPixel(x, 0, color);
            }

            gradientTex.Apply();

            return gradientTex;
        }

        public static Bounds CalculateBounds(List<NL_PaintableObject> paintableObjects)
        {
            Bounds bounds = new();

            Vector3 center;
            Vector3 centersAdd = Vector3.zero;
            Vector3 max;
            Vector3 min;

            float minX = float.PositiveInfinity, minY = float.PositiveInfinity, minZ = float.PositiveInfinity, maxX = float.NegativeInfinity, maxY = float.NegativeInfinity, maxZ = float.NegativeInfinity;

            for (int i = 0; i < paintableObjects.Count; i++)
            {
                Bounds pObjBounds = paintableObjects[i].mRenderer.bounds;

                if (pObjBounds.min.x < minX) minX = pObjBounds.min.x;
                if (pObjBounds.min.y < minY) minY = pObjBounds.min.y;
                if (pObjBounds.min.z < minZ) minZ = pObjBounds.min.z;

                if (pObjBounds.max.x > maxX) maxX = pObjBounds.max.x;
                if (pObjBounds.max.y > maxY) maxY = pObjBounds.max.y;
                if (pObjBounds.max.z > maxZ) maxZ = pObjBounds.max.z;

                centersAdd += paintableObjects[i].mRenderer.bounds.center;
            }

            center = centersAdd / paintableObjects.Count;
            min = new Vector3(minX, minY, minZ);
            max = new Vector3(maxX, maxY, maxZ);

            bounds.center = center;
            bounds.max = max;
            bounds.min = min;

            return bounds;
        }


        public static void DrawGradientPoint(Vector3 point, Color color, Color outlineColor, Camera cam)
        {
            float size = cam.orthographic ? cam.orthographicSize / 50 : cam.fieldOfView / 500;
            Handles.color = outlineColor;
            Handles.DrawSolidDisc(point, -cam.transform.forward, size);
            Handles.color = color;
            Handles.DrawSolidDisc(point, -cam.transform.forward, size * 0.7f);
        }

        struct Vertex
        {
            public int index;
            public Vector3 normal;
        }

        public static Vector3[] CalcAveragedNormals(Mesh mesh)
        {
            Vector3[] unifiedNormals = mesh.normals;

            Vector3[] originalNormals = mesh.normals;
            Vector3[] sourceVertPos = mesh.vertices;

            bool matchFound = false;
            List<List<Vertex>> allMatches = new List<List<Vertex>>();
            List<Vertex> matchVertices = new List<Vertex>();
            Vertex vertex;

            for (int i = 0; i < sourceVertPos.Length; i++)
            {
                for (int j = 0; j < sourceVertPos.Length; j++)
                {
                    if (i == j) continue;
                    if (Vector3.Distance(sourceVertPos[i], sourceVertPos[j]) < 0.002)
                    {
                        if (!matchFound)
                        {
                            matchVertices = new List<Vertex>();
                            matchFound = true;
                        }

                        vertex = new Vertex();
                        
                        vertex.index = j;
                        vertex.normal = originalNormals[j];
                        
                        matchVertices.Add(vertex);
                    }   
                }
                
                if (matchFound) 
                {
                    vertex = new Vertex();
                    
                    vertex.index = i;
                    vertex.normal = originalNormals[i];
                    
                    matchVertices.Add(vertex);

                    allMatches.Add(matchVertices);
                }

                matchFound = false;
            }

            for (int i = 0; i < allMatches.Count; i++)
            {
                Vector3 averageVector = Vector3.zero;
                int vCount = 0;

                for (int j = 0; j < allMatches[i].Count; j++)
                {
                    averageVector += allMatches[i][j].normal;
                    vCount++;
                }

                averageVector = averageVector.normalized;

                for (int j = 0; j < allMatches[i].Count; j++)
                {
                    unifiedNormals[allMatches[i][j].index] = averageVector;
                }
            }

            return unifiedNormals;
        }
    }
}
#endif