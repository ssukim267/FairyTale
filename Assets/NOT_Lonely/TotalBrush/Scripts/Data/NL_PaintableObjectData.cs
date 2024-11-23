namespace NOT_Lonely.TotalBrush
{
    using UnityEngine;

    public class NL_PaintableObjectData : ScriptableObject
    {
        public Vector3[] vPositionsWS;
        public Vector3[] vPositionsSS;
        public Vector3[] normals;

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct SourceVertex
        {
            public Vector3 pos;
            public Vector3 posSS;
            public Vector3 normal;
            public Vector4 color;
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct CalculatedVertex
        {
            public Vector3 pos;
            public Vector3 posSS;
            public Vector3 normal;
            public Vector4 color;
        }

        public SourceVertex[] vertSource;
        public CalculatedVertex[] vertCalculated;

        public GraphicsBuffer vertBufferSource;
        public GraphicsBuffer vertBufferCalculated;
        public ComputeBuffer c_vPosBufferWS;
        public ComputeBuffer c_vPosBufferSS;
    }
}
