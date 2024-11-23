#if UNITY_EDITOR
namespace NOT_Lonely.TotalBrush
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public class NL_BrushTool
    {
        public static void PaintVerices(ComputeShader c_shader, Vector2 targetValMinMax, float opacity, Vector3 brushPosition, float brushSize, Texture2D brush, RenderTexture depth, Matrix4x4 depthCamMatrix, float cullingBias, float nDevThreshold, bool[] channelMask, List<NL_PaintableObject> paintableObjects, bool splatMode)
        {
            int[] cm = NL_TotalBrushUtilities.ConvertChannelMask(channelMask);

            for (int i = 0; i < paintableObjects.Count; i++)
            {
                NL_PaintableObject obj = paintableObjects[i];
                if (obj.data.vPositionsWS == null) return;

                NL_TotalBrushUtilities.SetSourceVetData(obj, c_shader, "BrushPaintVertexColors", obj.data.vPositionsWS, null, obj.data.normals, obj.tempColors);

                c_shader.SetVector("camForward", SceneView.lastActiveSceneView.camera.transform.forward);
                c_shader.SetVector("targetValMinMax", targetValMinMax);

                c_shader.SetBool("splatMode", splatMode);
                c_shader.SetFloat("opacity", opacity);
                c_shader.SetVector("brushPosition", brushPosition);
                c_shader.SetFloat("brushSize", brushSize);
                c_shader.SetFloat("cullingBias", cullingBias);
                c_shader.SetFloat("nDevThreshold", nDevThreshold);
                c_shader.SetMatrix("depthCamMatrix", depthCamMatrix);
                c_shader.SetTexture(obj.c_kernelId, "brush", brush);
                c_shader.SetTexture(obj.c_kernelId, "depth", depth);

                c_shader.SetInt("maskR", cm[0]);
                c_shader.SetInt("maskG", cm[1]);
                c_shader.SetInt("maskB", cm[2]);
                c_shader.SetInt("maskA", cm[3]);

                NL_TotalBrushUtilities.CalculateAndApplyVertColors(obj, c_shader);
            }
        }
    }
}
#endif
