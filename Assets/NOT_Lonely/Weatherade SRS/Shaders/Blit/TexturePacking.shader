Shader "Hidden/NOT_Lonely/Weatherade/NL_TexturePacking"
{
    Properties
    {
        _DepthTex("DepthTex", 2D) = "white" {}
        _rgb("rgb", 2D) = "red" {}
		_a("a", 2D) = "black" {}
        _rg("rg", 2D) = "red" {}
		_b("b", 2D) = "black" {}
        //_VsmExp("VsmExp", float) = 0
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Name "Depth to VSM"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            UNITY_DECLARE_TEX2D(_DepthTex);
            uniform float _VsmExp;

            float2 frag (v2f i) : SV_Target
            {
                //_VsmExp = 5;
                float2 result;
                float depth = UNITY_SAMPLE_TEX2D(_DepthTex, i.uv).r * -1;
                
                depth = 2.0 * depth - 1.0;
                depth = exp(_VsmExp * depth);
                
                result.x = depth;
                result.y = depth * depth;
                return result;
            }
            ENDCG
        }

        Pass
        {
            Name "Pack to RGB"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            UNITY_DECLARE_TEX2D(_rg);
            UNITY_DECLARE_TEX2D(_b);

            float3 frag (v2f i) : SV_Target
            {
                float2 rg = UNITY_SAMPLE_TEX2D(_rg, i.uv).rg;
                float b = UNITY_SAMPLE_TEX2D(_b, i.uv);
                
                return float3(rg, b);
            }
            ENDCG
        }

        Pass
        {
            Name "Pack to RGBA"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            UNITY_DECLARE_TEX2D(_rgb);
            UNITY_DECLARE_TEX2D(_a);

            float4 frag (v2f i) : SV_Target
            {
                float3 rgb = UNITY_SAMPLE_TEX2D(_rgb, i.uv).rgb;
                float a = UNITY_SAMPLE_TEX2D(_a, i.uv);
                
                return float4(rgb, a);
            }
            ENDCG
        }
    }
}
