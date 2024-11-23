Shader "Hidden/NOT_Lonely/NL_GaussianBlur"
{
    Properties
    {
        _tex("tex", 2D) = "white" {}
		_kernelSize("kernelSize", Int) = 21
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Name "Horizontal R channel"
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

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

            UNITY_DECLARE_TEX2D(_tex);
			float4 _tex_TexelSize;
			uniform int _kernelSize;

			float BlurHorizontal(float2 uv, float texelSize, int kernelSize)
			{
				float col = float(0.0);
				int upper = ((kernelSize - 1) / 2.0);
				int lower = -upper;
				for (int x = lower; x <= upper; ++x)
				{
                    col += UNITY_SAMPLE_TEX2D(_tex, uv + float2(texelSize * x, 0.0)).r;
				}
				col /= kernelSize;
				return col;
			}

            v2f Vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 Frag (v2f i) : SV_Target
            {
				int kernelSize = (int)(_kernelSize + 1);
				float blured = BlurHorizontal(i.uv, _tex_TexelSize.x, kernelSize);
				float4 color = float4(blured, 0, 0, 1);

                return color;
            }
            ENDCG
        }

        Pass
        {
            Name "Vertical R channel"
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

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

            UNITY_DECLARE_TEX2D(_tex);
			float4 _tex_TexelSize;
			uniform int _kernelSize;

			float BlurVertical(float2 uv, float texelSize, int kernelSize)
			{
                float col = float(0.0);
				int lower = -((kernelSize - 1) / 2.0);
				int upper = -lower;
				for (int y = lower; y <= upper; ++y)
				{
                    col += UNITY_SAMPLE_TEX2D(_tex, uv + float2(0.0, texelSize * y)).r;
				}
				col /= kernelSize;
				return col;
			}

            v2f Vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 Frag (v2f i) : SV_Target
            {
				int kernelSize = (int)(_kernelSize + 1);
				float blured = BlurVertical(i.uv, _tex_TexelSize.x, kernelSize);
				float4 color = float4(blured, 0, 0, 1);

                return color;
            }
            ENDCG
        }

        Pass
        {
            Name "RG Horizontal"
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

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

            UNITY_DECLARE_TEX2D(_tex);
			float4 _tex_TexelSize;
			uniform int _kernelSize;

			float2 BlurHorizontal(float2 uv, float texelSize, int kernelSize)
			{
				float2 col = float2(0, 0);
				int upper = ((kernelSize - 1) / 2.0);
				int lower = -upper;
				for (int x = lower; x <= upper; ++x)
				{
                    col += UNITY_SAMPLE_TEX2D(_tex, uv + float2(texelSize * x, 0.0)).rg;
				}
				col /= kernelSize;
				return col;
			}

            v2f Vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 Frag (v2f i) : SV_Target
            {
				int kernelSize = (int)(_kernelSize + 1);
				float2 blured = BlurHorizontal(i.uv, _tex_TexelSize.x, kernelSize);

                return float4(blured, 0, 1);
            }
            ENDCG
        }

         Pass
        {
            Name "RG Vertical"
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

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

            UNITY_DECLARE_TEX2D(_tex);
			float4 _tex_TexelSize;
			uniform int _kernelSize;

			float2 BlurVertical(float2 uv, float texelSize, int kernelSize)
			{
                float2 col = float2(0, 0);
				int lower = -((kernelSize - 1) / 2.0);
				int upper = -lower;
				for (int y = lower; y <= upper; ++y)
				{
                    col += UNITY_SAMPLE_TEX2D(_tex, uv + float2(0.0, texelSize * y)).rg;
				}
				col /= kernelSize;
				return col;
			}

            v2f Vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 Frag (v2f i) : SV_Target
            {
				int kernelSize = (int)(_kernelSize + 1);
				float2 blured = BlurVertical(i.uv, _tex_TexelSize.x, kernelSize);

                return float4(blured, 0, 1);
            }
            ENDCG
        }

        Pass
        {
            Name "RGBA Horizontal"
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

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

            UNITY_DECLARE_TEX2D(_tex);
			float4 _tex_TexelSize;
			uniform int _kernelSize;

			float4 BlurHorizontal(float2 uv, float texelSize, int kernelSize)
			{
				float4 col = float4(0, 0, 0, 0);
				int upper = ((kernelSize - 1) / 2.0);
				int lower = -upper;
				for (int x = lower; x <= upper; ++x)
				{
                    col += UNITY_SAMPLE_TEX2D(_tex, uv + float2(texelSize * x, 0.0));
				}
				col /= kernelSize;
				return col;
			}

            v2f Vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 Frag (v2f i) : SV_Target
            {
				int kernelSize = (int)(_kernelSize + 1);
				float4 blured = BlurHorizontal(i.uv, _tex_TexelSize.x, kernelSize);

                return blured;
            }
            ENDCG
        }

        Pass
        {
            Name "RGBA Vertical"
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

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

            UNITY_DECLARE_TEX2D(_tex);
			float4 _tex_TexelSize;
			uniform int _kernelSize;

			float4 BlurVertical(float2 uv, float texelSize, int kernelSize)
			{
                float4 col = float4(0, 0, 0, 0);
				int lower = -((kernelSize - 1) / 2.0);
				int upper = -lower;
				for (int y = lower; y <= upper; ++y)
				{
                    col += UNITY_SAMPLE_TEX2D(_tex, uv + float2(0.0, texelSize * y));
				}
				col /= kernelSize;
				return col;
			}

            v2f Vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 Frag (v2f i) : SV_Target
            {
				int kernelSize = (int)(_kernelSize + 1);
				float4 blured = BlurVertical(i.uv, _tex_TexelSize.x, kernelSize);

                return blured;
            }
            ENDCG
        }
    }
}
