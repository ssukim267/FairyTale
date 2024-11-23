Shader "Hidden/NOT_Lonely/TotalBrush/MaskPainting"
{
    Properties
    {
        _SplatTex("_SplatTex", 2D) = "white" {}
		_BrushAlpha("BrushAlpha", 2D) = "white" {}
		_BrushPos("_BrushPos", Vector) = (0,0,0,0)
		_BrushSize("BrushSize", Vector) = (0,0,0,0)
		_BrushOpacity("_BrushOpacity", Vector) = (0,0,0,0)
		_IsFilling("_IsFilling", Float) = 0
		_FillingValue("_FillingValue", Vector) = (0,0,0,0)
		_BrushAngle("_BrushAngle", Float) = 0
		_TargetValMinMax("_TargetValMinMax", Vector) = (0,0,0,0)
        _NormalSpread("NormalSpread", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Name "Paint Mask"
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
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };


            uniform float _IsFilling;
			uniform float4 _FillingValue;
            UNITY_DECLARE_TEX2D(_BrushAlpha);
			UNITY_DECLARE_TEX2D(_SplatTex);
			uniform float4 _SplatTex_ST;
			uniform float4 _BrushOpacity;
			uniform float2 _BrushPos;
			uniform float2 _BrushSize;
			uniform float _BrushAngle;
			uniform float2 _TargetValMinMax;

			float CheckBrushBoundaries(float2 uv)
			{
				if(uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
				    return 0;
				else
				    return 1;
			}

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f IN) : SV_Target
            {
				float cs = cos(_BrushAngle);
				float sn = sin(_BrushAngle);

                float2 brushUV = (((IN.uv - _BrushPos) / _BrushSize ) + 0.5);
				brushUV = mul(brushUV - float2(0.5, 0.5), float2x2(cs, -sn, sn, cs)) + float2(0.5, 0.5);
                
                float2 uv_SplatTex = IN.uv * _SplatTex_ST.xy + _SplatTex_ST.zw;
				float4 splatTex = UNITY_SAMPLE_TEX2D(_SplatTex, uv_SplatTex);

                //Paint
                float brushSampled = UNITY_SAMPLE_TEX2D(_BrushAlpha, brushUV).r * CheckBrushBoundaries(brushUV);
                float4 brushMulOpacity = brushSampled * _BrushOpacity; //brush mask multiplied by the RGBA opacity values. Result values are in -1, 1 range.
                
				float4 curPaint = clamp((brushMulOpacity + splatTex), _TargetValMinMax.xxxx, _TargetValMinMax.yyyy);

                float tR, tG, tB, tA;

                tR = _BrushOpacity.r < 0 ? (abs(_BrushOpacity.r) * brushSampled) * brushSampled : brushMulOpacity.r;
                tG = _BrushOpacity.g < 0 ? (abs(_BrushOpacity.g) * brushSampled) * brushSampled : brushMulOpacity.g;
                tB = _BrushOpacity.b < 0 ? (abs(_BrushOpacity.b) * brushSampled) * brushSampled : brushMulOpacity.b;
                tA = _BrushOpacity.a < 0 ? (abs(_BrushOpacity.a) * brushSampled) * brushSampled : brushMulOpacity.a;

                float r = lerp(splatTex.r, curPaint.r, tR);
                float g = lerp(splatTex.g, curPaint.g, tG);
                float b = lerp(splatTex.b, curPaint.b, tB);
                float a = lerp(splatTex.a, curPaint.a, tA);
                //----------------------------------------
                
                //Fill
                float fR = _FillingValue.r == -1 ? 0 : 1;
                float fG = _FillingValue.g == -1 ? 0 : 1;
                float fB = _FillingValue.b == -1 ? 0 : 1;
                float fA = _FillingValue.a == -1 ? 0 : 1;

                float fillR = lerp(splatTex.r, _FillingValue.r, fR);
                float fillG = lerp(splatTex.g, _FillingValue.g, fG);
                float fillB = lerp(splatTex.b, _FillingValue.b, fB);
                float fillA = lerp(splatTex.a, _FillingValue.a, fA);

                float4 fill = float4(fillR, fillG, fillB, fillA);
                //----------------------------------------

                float4 finalPaint = float4(r, g, b, a);
                float4 result = saturate((_IsFilling == 1.0 ? fill : finalPaint));
				return result; 
            }
            ENDCG
        }

        Pass
        {
            Name "Calculate Normal"
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

            UNITY_DECLARE_TEX2D(_SplatTex);
            uniform float _NormalSpread;
            uniform float _NormalScale;

            v2f Vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half3 Frag (v2f IN) : SV_Target
            {
                float3 color = 0;

                //float normalScale = 0.0075;

                float2 uvRight = float2(IN.uv.x + _NormalSpread, IN.uv.y);
                float2 uvLeft = float2(IN.uv.x - _NormalSpread, IN.uv.y);
                float2 uvUp = float2(IN.uv.x, IN.uv.y + _NormalSpread);
                float2 uvDown = float2(IN.uv.x, IN.uv.y - _NormalSpread);

                float maskRight = UNITY_SAMPLE_TEX2D(_SplatTex, uvRight).r * 0.5 * _NormalScale;
                float maskLeft = UNITY_SAMPLE_TEX2D(_SplatTex, uvLeft).r  * 0.5 * _NormalScale;
                float maskUp = UNITY_SAMPLE_TEX2D(_SplatTex, uvUp).r * 0.5 * _NormalScale;
                float maskDown = UNITY_SAMPLE_TEX2D(_SplatTex, uvDown).r * 0.5 * _NormalScale;

                float3 lhs = float3(0, maskUp, _NormalSpread) - float3(0, maskDown, -_NormalSpread);
                float3 rhs = float3(_NormalSpread, maskRight, 0) - float3(-_NormalSpread, maskLeft, 0);

                float3 normal = normalize(cross(lhs, rhs)) * 0.5 + 0.5;

                return normal;
            }
            ENDCG
        }
    }
}
