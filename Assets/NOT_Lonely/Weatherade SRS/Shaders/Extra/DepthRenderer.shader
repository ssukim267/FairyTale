//This shader is intendend to render surface depth for usage with the Weatherade 

//Different sub shaders used to render diffrent types of surfaces such as terrain, opaque meshes, 
//alpha-cutout meshes, transparent meshes and invisible depth occluders

//To determinate what sub shader will be used to render a particular type of mesh, 
//use the "SRSGroupName" tag with one of the following values: "Terrain", "Opaque", "Cutout", "Transparent", "DepthOccluder".
//Due to this, your custom shader must have one of these tag key-value pair to work with Weatherade

//For more info see the first part of the Unity's doc about the replacement shader feature: https://docs.unity3d.com/Manual/SL-ShaderReplacement.html
Shader "Hidden/NOT_Lonely/Weatherade/DepthRenderer" {
    Properties {
        // used in fallback on old cards & base map
        [HideInInspector] _MainTex ("BaseMap (RGB)", 2D) = "white" {}
        [HideInInspector] _Color ("Main Color", Color) = (1,1,1,1)
        [HideInInspector] _TerrainHolesTexture("Holes Map (RGB)", 2D) = "white" {}
		[HideInInspector] _Cutoff("Cutoff", Range( 0 , 1)) = 0.5

        //snow shader properties
        [Toggle(_COVERAGE_ON)] _Coverage("Coverage", Float) = 0
		[Toggle(_PAINTABLE_COVERAGE_ON)] _PaintableCoverage("PaintableCoverage", Float) = 0
		[Toggle(_SPARKLE_ON)] _Sparkle("Sparkle", Float) = 0
		[Toggle(_SPARKLE_TEX_SS)] _SparkleTexSS("Sparkle Tex SS", Float) = 0
		[Toggle(_SPARKLE_TEX_LS)] _SparkleTexLS("Sparkle Tex LS", Float) = 0

		[Toggle(_TESSELLATION_ON)] _Tessellation("Tessellation", Float) = 0
		[Toggle(_DISPLACEMENT_ON)] _Displacement("Displacement", Float) = 0

		_PrecipitationDirOffset("PrecipitationDirOffset", Range( -1 , 1)) = 0
		_CoverageMasksTex("CoverageMasksTex", 2D) = "bump" {}
		
		_CoverageAmount("CoverageAmount", Range( 0 , 1)) = 1
		[NoAlpha]_CoverageColor("CoverageColor", Color) = (0.8349056,0.9156185,1,1)
		_CoverageSmoothnessContrast("Coverage Smoothness Contrast", Range( 0 , 1)) = 0.1
		_CoverageMicroRelief("Coverage Micro Relief", Range( 0 , 1)) = 0.05
		_MicroReliefFadeDistance("Micro Relief Fade Distance", Float) = 2
		_BaseCoverageNormalsBlend("Base/CoverageNormalsBlend", Range( 0 , 1)) = 0.2588235
		_CoverageNormalsOverlay("Coverage Normals Overlay", Range( 0 , 1)) = 0.5080121
		_CoverageTiling("Coverage Tiling", Float) = 0.14
		_CoverageAreaBias("Coverage Area Bias", Range( 0.001 , 0.3)) = 0.185
		_CoverageNormalScale("Coverage Normal Scale", Float) = 1
		_BlendByNormalsStrength("Blend By Normals Strength", Float) = 2
		_BlendByNormalsPower("Blend By Normals Power", Float) = 5
		//Sparkle
		_SparkleTex("SparkleTex", 2D) = "black" {}
		_SparklesAmount("Sparkles Amount", Range(0, 0.999)) = 0.95
		_SparkleDistFalloff("Sparkle Distance Falloff", Float) = 50
		_LocalSparkleTiling ("Local Sparkle Tiling", Float) = 1
		_ScreenSpaceSparklesTiling("Screen Space Sparkles Tiling", Float) = 2
		_SparklesBrightness("Sparkles Brightness", Float) = 30
		_SparkleBrightnessRT("Sparkles Brightness RT", Float) = 4
		_SparklesLightmapMaskPower("Sparkles Lightmap Mask Power", Float) = 4.5
		_SparklesHighlightMaskExpansion("Sparkles Highlight Mask Expansion", Range( 0 , 0.99)) = 0.8

		_SnowAOIntensity("Snow AO Intensity", Range(0, 1)) = 1

		_CoverageAreaFalloffHardness("CoverageAreaFalloffHardness", Range( 0 , 1)) = 0.5
		_PaintedMask("PaintedMask", 2D) = "gray" {}
		_AlbedoLOD("AlbedoLOD", 2D) = "white" {}
		_NormalLOD("NormalLOD", 2D) = "bump" {}
		_DistanceFadeStart("DistanceFadeStart", Float) = 150
		_DistanceFadeFalloff("DistanceFadeFalloff", Float) = 1
		_CoverageDisplacementOffset("CoverageDisplacementOffset", Range( 0 , 1)) = 0.5
		_TessEdgeL("TessEdgeL", Range( 5 , 100)) = 20
		_TessMaxDisp("TessMaxDisp", Float) = 0.45

		[Toggle(_TRACES_ON)] _Traces("Traces", Float) = 0
		[Toggle(_TRACE_DETAIL)] _TraceDetail("TraceDetail", Float) = 0
		_TracesNormalScale("TracesNormalScale", Float) = 5
		_TraceDetailTex("Trace Detail Tex", 2D) = "bump" {}
		_TraceDetailTiling("TraceDetailTiling", Float) = 50
		_TraceDetailNormalScale("TraceDetailNormalScale", Float) = 1
		_TraceDetailIntensity("TraceDetailIntensity", Range(0, 1)) = 0.5
		_TracesBlendFactor("TracesBlendFactor", Range(0, 1)) = 0.25

		_SSS_intensity("SSS_intensity", Float) = 1

		_PrecipitationDirRange("PrecipitationDirRange", Vector) = (0,1,0,0)
		_CoverageAreaMaskRange("CoverageAreaMaskRange", Vector) = (0,1,0,0)
		_CoverageDisplacement("CoverageDisplacement", Float) = 0.5
		_MapID("MapID", float) = 0 //needed to set a particular map when baking the distant map
		[HideInInspector] _TilingMultiplier("TilingMultiplier", Range(0 , 1)) = 1 // needed to set adjust the splats tiling when baking the distant map
    }

	//Terrain
    SubShader {
        Name"DEPTH RENDERER: Terrain"
        Tags { "SRSGroupName" = "Terrain" }
        
        ZWrite On

        Pass {

            CGPROGRAM
            #define SRS_TERRAIN
            #pragma shader_feature_local _COVERAGE_ON
			#pragma multi_compile_instancing
			#pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap 
            #define _REPLACEMENT
            #pragma vertex vert
            #pragma fragment frag
            #include "../CGIncludes/SRS_VertFrag.cginc"
            ENDCG
        }
    }

	//depth occluders
	SubShader
    {
        Name"DEPTH RENDERER: DepthOccluder"
        Tags 
        { 
        "Queue"="Geometry+10" 
        "SRSGroupName" = "DepthOccluder"
        }

        ZWrite On

        Pass 
        {
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            uniform float _VertexPush;

            struct VertexData {
            	float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f {
            	float4 pos : SV_POSITION;
            };

            v2f Vert(VertexData v)
            {
                v2f i;
                v.vertex.xyz += v.normal * _VertexPush;
                i.pos = UnityObjectToClipPos(v.vertex);
                return i;
            }

            float4 Frag(v2f i) : SV_TARGET
            {
            	return float4(0.5, 0.5, 0.5, 1);
            }

            ENDCG
        }
    }

	//geometry
	SubShader {
        Name"DEPTH RENDERER: Opaque"
        Tags {
            "Queue" = "Geometry+0"
            "RenderType" = "Opaque"
			"SRSGroupName" = "Opaque"
        }
		ZWrite On

        Pass {
            CGPROGRAM
            #pragma multi_compile_instancing
			#pragma instancing_options nolightprobe nolightmap 
            #define _REPLACEMENT
            #include "../CGIncludes/SRS_VertFrag.cginc"

            float4 DepthRendererFrag(v2f i) : SV_TARGET
            {
                return float4(0.5, 0.5, 0.5, 1);
            }

            #pragma vertex vert
            #pragma fragment DepthRendererFrag
            
            ENDCG
        }
    }

	//cutout
	SubShader {
        Name "DEPTH RENDERER: Cutout"
        Tags {
            "Queue" = "AlphaTest+0"
            "RenderType" = "Opaque"
			"SRSGroupName" = "Cutout"
        }

        ZWrite On

        Pass {
            CGPROGRAM
            #pragma multi_compile_instancing
			#pragma instancing_options nolightprobe nolightmap 
            #define _REPLACEMENT
            #define _RENDERING_CUTOUT
            #include "../CGIncludes/SRS_VertFrag.cginc"

            float4 DepthRendererFrag(v2f i) : SV_TARGET
            {
                float2 uv_MainTex = i.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw;
			    float alpha = UNITY_SAMPLE_TEX2D(_MainTex, uv_MainTex).a * UNITY_ACCESS_INSTANCED_PROP(InstancedProps, _Color).a;
                clip(alpha - _Cutoff);
                return float4(0.5, 0.5, 0.5, 1);
            }

            #pragma vertex vert
            #pragma fragment DepthRendererFrag
            
            ENDCG
        }
    }

    //fade (works like cutout)
	SubShader {
        Name "DEPTH RENDERER: Fade"
        Tags {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
			"SRSGroupName" = "Fade"
        }

        ZWrite On

        Pass {
            CGPROGRAM
            #pragma multi_compile_instancing
			#pragma instancing_options nolightprobe nolightmap 
            #define _REPLACEMENT
            #define _RENDERING_TRANSPARENT
            #include "../CGIncludes/SRS_VertFrag.cginc"

            float4 DepthRendererFrag(v2f i) : SV_TARGET
            {
                float2 uv_MainTex = i.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw;
			    float alpha = UNITY_SAMPLE_TEX2D(_MainTex, uv_MainTex).a * UNITY_ACCESS_INSTANCED_PROP(InstancedProps, _Color).a;
                clip(alpha - _Cutoff);
                return float4(0.5, 0.5, 0.5, 1);
            }

            #pragma vertex vert
            #pragma fragment DepthRendererFrag
            
            ENDCG
        }
    }

	//transparent (works like opaque)
	SubShader {
        Name "DEPTH RENDERER: Transparent"
        Tags {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
			"SRSGroupName" = "Transparent"
        }

        ZWrite On

        Pass {
            CGPROGRAM
            #pragma multi_compile_instancing
			#pragma instancing_options nolightprobe nolightmap 
            #define _REPLACEMENT
            #define _RENDERING_TRANSPARENT
            #include "../CGIncludes/SRS_VertFrag.cginc"

            float4 DepthRendererFrag(v2f i) : SV_TARGET
            {
                return float4(0.5, 0.5, 0.5, 1); //treat transparent surfaces as full opaque surfaces
            }

            #pragma vertex vert
            #pragma fragment DepthRendererFrag
            
            ENDCG
        }
    }
}
