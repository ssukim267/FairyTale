Shader "Hidden/NOT_Lonely/Weatherade/NL_TraceMaskGen"
{
    Properties
    {

        _SRS_SurfaceDepth("SRS_SurfaceDepth", 2D) = "white" {}
		_SRS_TraceObjsDepth("SRS_TraceObjsDepth", 2D) = "white" {}
		_SRS_LastTraceMask("SRS_LastTraceMask", 2D) = "white" {}
        _SRS_TraceMask("SRS_TraceMask", 2D) = "white" {}
        _SRS_TraceMaskBasic("SRS_TraceMaskBasic", 2D) = "bump" {}

        _NormalSpread("NormalSpread", Float) = 0.001
		_UVOffset("_UVOffset", Vector) = (0,0,0,0)
		_DecaySpeed("DecaySpeed", Float) = 0.001
		_AreaFalloffHardness("_AreaFalloffHardness", Float) = 0
        _DepthBias("DepthBias", Float) = 0
		_VolumeDepth("_VolumeDepth", Float) = 0
		_NoiseTiling("NoiseTiling", Float) = 60
		_NoiseIntensity("NoiseIntensity", Float) = 0.0
        _SourceMaskBrightness("SourceMaskBrightness", Float) = 8.0
        _BorderIntensity("BorderIntensity", Float) = 1
        _Indent("Indent", Float) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Name "Primary Mask"
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"
            #include "../CGIncludes/SRS_CoverageCommon.cginc"

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

            
            UNITY_DECLARE_TEX2D(_SRS_TraceObjsDepth);
			UNITY_DECLARE_TEX2D_NOSAMPLER(_SRS_SurfaceDepth);
            UNITY_DECLARE_TEX2D_NOSAMPLER(_SRS_LastTraceMask);
			uniform float _VolumeDepth;
            uniform float _DepthBias;
            uniform float _AreaFalloffHardness;
			uniform float2 _UVOffset;
			uniform float _DecaySpeed;

            v2f Vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 Frag (v2f i) : SV_Target
            {
                float4 color = 0;

                float borderMask = CalcBorderMask(_AreaFalloffHardness, i.uv);

                float2 traceObjDepthUV = float2(i.uv.x, 1 - i.uv.y);

                float traceObjDepth = 1.0 - UNITY_SAMPLE_TEX2D(_SRS_TraceObjsDepth, traceObjDepthUV).r;
                float surfDepth = UNITY_SAMPLE_TEX2D_SAMPLER(_SRS_SurfaceDepth, _SRS_TraceObjsDepth, i.uv).r;
                float lastTraceMask = UNITY_SAMPLE_TEX2D_SAMPLER(_SRS_LastTraceMask, _SRS_TraceObjsDepth, i.uv + _UVOffset).r;

                lastTraceMask = min(lastTraceMask - (unity_DeltaTime.x * _DecaySpeed), 0.5);
                
                float diff = surfDepth - traceObjDepth;
                float traceCombined = (diff - _DepthBias) * _VolumeDepth;

                float finalTrace = saturate(min(max(traceCombined, lastTraceMask), borderMask));

                return finalTrace;
            }
            ENDCG
        }

        Pass
        {
            Name "Detail Mask"
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"
            #include "../CGIncludes/SRS_CoverageCommon.cginc"

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

            UNITY_DECLARE_TEX2D(_SRS_TraceMask);
			uniform float _NoiseTiling;
			uniform float _NoiseIntensity;
            uniform float _SourceMaskBrightness;
            uniform float _BorderIntensity;
            uniform float _Indent;

            float3 mod2D( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }
            float2 mod2D( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }
			float3 permute( float3 x ) { return mod2D( ( ( x * 34.0 ) + 1.0 ) * x ); }
			float SimplexNoise(float2 v)
			{
				const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
				float2 i = floor( v + dot( v, C.yy ) );
				float2 x0 = v - i + dot( i, C.xx );
				float2 i1;
				i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
				float4 x12 = x0.xyxy + C.xxzz;
				x12.xy -= i1;
				i = mod2D( i );
				float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
				float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
				m = m * m;
				m = m * m;
				float3 x = 2.0 * frac( p * C.www ) - 1.0;
				float3 h = abs( x ) - 0.5;
				float3 ox = floor( x + 0.5 );
				float3 a0 = x - ox;
				m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
				float3 g;
				g.x = a0.x * x0.x + h.x * x0.y;
				g.yz = a0.yz * x12.xz + h.yz * x12.yw;
				return 130.0 * dot( m, g );
			}
			

            v2f Vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float Frag (v2f i) : SV_Target
            {
                float4 color = 0;

                float snowHeight = 0.5;

                float inputTraceMask =  UNITY_SAMPLE_TEX2D(_SRS_TraceMask, i.uv).r;
                float noise = SimplexNoise(i.uv * _NoiseTiling);//replace this with a texture sample later

                float mask = saturate(inputTraceMask * _SourceMaskBrightness);
                float border = sin(UNITY_PI * mask) * _BorderIntensity; 
                float snowWithBorder = saturate(border * lerp(1, noise, _NoiseIntensity) + snowHeight);

                float indentation = (1 - mask) + pow(lerp(0, noise, _NoiseIntensity), 2);
                indentation = lerp(1, indentation, _Indent);

                float trace = snowWithBorder * indentation;
                trace = lerp(snowHeight, trace, saturate(inputTraceMask * 150));

                return trace;
            }
            ENDCG
        }

        Pass
        {
            //Generate normal map and merge it with the mask into a single texture
            Name "Composite Mask"
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"
            #include "../CGIncludes/SRS_CoverageCommon.cginc"

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

            UNITY_DECLARE_TEX2D(_SRS_TraceMask);
            UNITY_DECLARE_TEX2D(_SRS_TraceMaskBasic);
            uniform float _AreaFalloffHardness;
            uniform float _NormalSpread;

            v2f Vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 Frag (v2f i) : SV_Target
            {
                float4 color = 0;

                float borderMask = CalcBorderMask(_AreaFalloffHardness, i.uv);

                //float _NormalSpread = 0.001;
                float normalScale = 0.015;

                float2 uvRight = float2(i.uv.x + _NormalSpread, i.uv.y);
                float2 uvLeft = float2(i.uv.x - _NormalSpread, i.uv.y);
                float2 uvUp = float2(i.uv.x, i.uv.y + _NormalSpread);
                float2 uvDown = float2(i.uv.x, i.uv.y - _NormalSpread);

                float maskRight = UNITY_SAMPLE_TEX2D(_SRS_TraceMask, uvRight).r * normalScale;
                float maskLeft = UNITY_SAMPLE_TEX2D(_SRS_TraceMask, uvLeft).r * normalScale;
                float maskUp = UNITY_SAMPLE_TEX2D(_SRS_TraceMask, uvUp).r * normalScale;
                float maskDown = UNITY_SAMPLE_TEX2D(_SRS_TraceMask, uvDown).r * normalScale;

                float3 lhs = float3(0, maskUp, _NormalSpread) - float3(0, maskDown, -_NormalSpread);
                float3 rhs = float3(_NormalSpread, maskRight, 0) - float3(-_NormalSpread, maskLeft, 0);

                float2 normal = normalize(cross(lhs, rhs)).xz * 0.5 + 0.5;
                float basicMask = UNITY_SAMPLE_TEX2D(_SRS_TraceMaskBasic, i.uv).x;
                float traceMask = UNITY_SAMPLE_TEX2D(_SRS_TraceMask, i.uv).r;
                traceMask = lerp(0.5, traceMask, borderMask);
                float4 finalTraceTex = float4(normal, basicMask * 10, traceMask);

                return finalTraceTex;
            }
            ENDCG
        }
    }
}
