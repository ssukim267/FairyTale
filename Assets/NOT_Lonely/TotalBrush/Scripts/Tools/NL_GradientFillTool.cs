#if UNITY_EDITOR
namespace NOT_Lonely.TotalBrush
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public class NL_GradientFillTool
    {
        public static void ApplyGradient(ComputeShader c_shader, Vector3 gradientStartPoint, Vector3 gradientEndPoint, bool clampGradientStart, bool clampGradientEnd, Texture2D gradient, Vector2 targetValMinMax, float opacity, RenderTexture depth, Matrix4x4 depthCamMatrix, float cullingBias, float nDevThreshold, bool[] channelMask, List<NL_PaintableObject> paintableObjects)
        {
            gradientStartPoint.y = SceneView.lastActiveSceneView.camera.pixelHeight - gradientStartPoint.y;
            gradientEndPoint.y = SceneView.lastActiveSceneView.camera.pixelHeight - gradientEndPoint.y;

            Vector3 gradientAxis = (gradientEndPoint - gradientStartPoint).normalized;

            int[] cm = NL_TotalBrushUtilities.ConvertChannelMask(channelMask);

            for (int i = 0; i < paintableObjects.Count; i++)
            {
                NL_PaintableObject obj = paintableObjects[i];

                if (obj.data.vPositionsSS == null) return;

                NL_TotalBrushUtilities.SetSourceVetData(obj, c_shader, "GradientFillVertexColors", obj.data.vPositionsWS, obj.data.vPositionsSS, obj.data.normals, obj.tempColors);

                c_shader.SetVector("gradientStartPoint", gradientStartPoint);
                c_shader.SetVector("gradientEndPoint", gradientEndPoint);
                c_shader.SetBool("clampGradientStart", clampGradientStart);
                c_shader.SetBool("clampGradientEnd", clampGradientEnd);
                c_shader.SetVector("gradientAxis", gradientAxis);
                c_shader.SetVector("camForward", SceneView.lastActiveSceneView.camera.transform.forward);
                c_shader.SetVector("targetValMinMax", targetValMinMax);
                c_shader.SetFloat("opacity", opacity);
                c_shader.SetFloat("cullingBias", cullingBias);
                c_shader.SetFloat("nDevThreshold", nDevThreshold);
                c_shader.SetTexture(obj.c_kernelId, "gradientTex", gradient);
                c_shader.SetMatrix("depthCamMatrix", depthCamMatrix);
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