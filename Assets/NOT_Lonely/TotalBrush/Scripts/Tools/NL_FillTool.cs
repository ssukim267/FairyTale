#if UNITY_EDITOR
namespace NOT_Lonely.TotalBrush
{
    using System.Collections.Generic;
    using UnityEngine;

    public class NL_FillTool
    {
        public static void Fill(ComputeShader c_shader, Color fillColor, bool[] channelMask, List<NL_PaintableObject> paintableObjects, bool splatMode)
        {
            int[] cm = NL_TotalBrushUtilities.ConvertChannelMask(channelMask);

            for (int i = 0; i < paintableObjects.Count; i++)
            {
                NL_PaintableObject obj = paintableObjects[i];

                NL_TotalBrushUtilities.SetSourceVetData(obj, c_shader, "FillVertexColors", null, null, null, obj.tempColors);

                c_shader.SetBool("splatMode", splatMode);
                c_shader.SetVector("fillColor", fillColor);

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
