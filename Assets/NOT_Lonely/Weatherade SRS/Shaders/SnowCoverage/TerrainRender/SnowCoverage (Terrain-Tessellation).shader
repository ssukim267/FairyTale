Shader "Hidden/NOT_Lonely/Weatherade/Snow Coverage (Terrain-Tessellation)"
{
    Properties
    {
        // used in fallback on old cards & base map
        [HideInInspector] _MainTex ("BaseMap (RGB)", 2D) = "white" {}
        [HideInInspector] _Color ("Main Color", Color) = (1,1,1,1)
        [HideInInspector] _TerrainHolesTexture("Holes Map (RGB)", 2D) = "white" {}
        
        //snow shader properties
		[Toggle(_COVERAGE_ON)] _Coverage("Coverage", Float) = 1
		[Toggle(_PAINTABLE_COVERAGE_ON)] _PaintableCoverage("PaintableCoverage", Float) = 0
		[Toggle(_SPARKLE_ON)] _Sparkle("Sparkle", Float) = 0
		[Toggle(_SSS_ON)] _Sss("SSS", Float) = 0
		[Toggle(_SPARKLE_TEX_SS)] _SparkleTexSS("Sparkle Tex SS", Float) = 0
		[Toggle(_SPARKLE_TEX_LS)] _SparkleTexLS("Sparkle Tex LS", Float) = 0

		[Toggle] _Tessellation("Tessellation", Float) = 0
		[Toggle(_DISPLACEMENT_ON)] _Displacement("Displacement", Float) = 0
		[Toggle(_STOCHASTIC_ON)] _Stochastic("Stochastic", Float) = 0

		_PrecipitationDirOffset("PrecipitationDirOffset", Range( -1 , 1)) = 0

		//_BlueNoise("BlueNoise", 2DArray) = "white" {}
		_CoverageTex0("CoverageTex0", 2D) = "bump" {}
		_CovMasks0_triBlendContrast("CovMasks0_triBlendContrast", Float) = 2.5
		_CoverageAmount("CoverageAmount", Range( 0 , 1)) = 1
		_HeightMap0Contrast("HeightMap0Contrast", Range(0, 1)) = 0.2
		[NoAlpha]_CoverageColor("CoverageColor", Color) = (0.8349056,0.9156185,1,1)
		_CoverageSmoothnessContrast("Coverage Smoothness Contrast", Range( 0 , 1)) = 0.1
		_CoverageMicroRelief("Coverage Micro Relief", Range( 0 , 1)) = 0.05
		_MicroReliefFadeDistance("Micro Relief Fade Distance", Float) = 2
		_BaseCoverageNormalsBlend("Base/CoverageNormalsBlend", Range( 0 , 1)) = 0.2588235
		_CoverageNormalsOverlay("Coverage Normals Overlay", Range( 0 , 1)) = 0.5080121
		_CoverageTiling("Coverage Tiling", Float) = 0.14
		_CoverageAreaBias("Coverage Area Bias", Range( 0.001 , 0.3)) = 0.001
		_CoverageLeakReduction("CoverageLeakReduction", Range( 0.0 , 0.99)) = 0.0
		_CoverageNormalScale0("Coverage Normal Scale 0", Float) = 1
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

		//_SnowAOIntensity("Snow AO Intensity", Range(0, 1)) = 1

		_CoverageAreaFalloffHardness("CoverageAreaFalloffHardness", Range( 0 , 1)) = 0.5
		_PaintedMask("PaintedMask", 2D) = "gray" {}
		_PaintedMaskNormal("PaintedMaskNormal", 2D) = "bump" {}
		_AlbedoLOD("AlbedoLOD", 2D) = "white" {}
		_NormalLOD("NormalLOD", 2D) = "bump" {}
		_DistanceFadeStart("DistanceFadeStart", Float) = 150
		_DistanceFadeFalloff("DistanceFadeFalloff", Float) = 1
		_CoverageDisplacementOffset("CoverageDisplacementOffset", Range( 0 , 1)) = 0.5
		_TessFactorSnow("TessFactorSnow", Range(0, 1)) = 0.5
		_TessEdgeL("TessEdgeL", Range( 5 , 100)) = 20
		_TessMaxDisp("TessMaxDisp", Float) = 0.45
		_TessSnowdriftRange("TessSnowdriftRange", Vector) = (0.5, 0.8, 0, 0)

		[Toggle(_TRACES_ON)] _Traces("Traces", Float) = 0
		[Toggle(_TRACE_DETAIL)] _TraceDetail("TraceDetail", Float) = 0
		_TracesNormalScale("TracesNormalScale", Float) = 5
		//_TraceDetailTex("Trace Detail Tex", 2D) = "bump" {}
		_TraceDetailTiling("TraceDetailTiling", Float) = 50
		_TraceDetailNormalScale("TraceDetailNormalScale", Float) = 1
		_TraceDetailIntensity("TraceDetailIntensity", Range(0, 1)) = 0.5
		[NoAlpha]_TracesColor("TracesColor", Color) = (0.1, 0.25, 0.4, 1)
		_TracesBlendFactor("TracesBlendFactor", Range(0, 1)) = 0.5
		_TracesColorBlendRange("TracesColorBlendRange", Vector) = (0, 0.5, 0, 0)

		_SSS_intensity("SSS_intensity", Float) = 1

		_PrecipitationDirRange("PrecipitationDirRange", Vector) = (0,1,0,0)
		_CoverageAreaMaskRange("CoverageAreaMaskRange", Range(0, 1)) = 1
		_CoverageDisplacement("CoverageDisplacement", Float) = 0.5
		_MapID("MapID", float) = 0 //needed to set a particular map when baking the distant map
		[HideInInspector] _TilingMultiplier("TilingMultiplier", Range(0 , 1)) = 1 // needed to set adjust the splats tiling when baking the distant map

		//snow shader overrides
		_CoverageOverride("CoverageOverride", Float) = 0
		_CoverageColorOverride("CoverageColorOverride", Float) = 0
		_CoverageDisplacementOverride("CoverageDisplacementOverride", Float) = 0
		
		//Traces
		_TracesOverride("TracesOverride", Float) = 0
		_TraceDetailOverride("TraceDetailOverride", Float) = 0
		_TracesNormalScaleOverride("TracesNormalScaleOverride", Float) = 0
		_TraceDetailTexOverride("TraceDetailTexOverride", Float) = 0
		_TraceDetailTilingOverride("TraceDetailTilingOverride", Float) = 0
		_TraceDetailNormalScaleOverride("TraceDetailNormalScaleOverride", Float) = 0
		_TraceDetailIntensityOverride("TraceDetailIntensityOverride", Float) = 0
		_TracesBlendFactorOverride("TracesBlendFactorOverride", Float) = 0
		_TracesColorOverride("TracesColorOverride", Float) = 0
		_TracesColorBlendRangeOverride("TracesColorBlendRangeOverride", Float) = 0
		
		_CoverageDisplacementOffsetOverride("CoverageDisplacementOffsetOverride", Float) = 0
		_DistanceFadeFalloffOverride("DistanceFadeFalloffOverride", Float) = 0
		_DistanceFadeStartOverride("DistanceFadeStartOverride", Float) = 0
		_PaintableCoverageOverride("PaintableCoverageOverride", Float) = 0
		_CoverageTex0Override("CoverageTex0Override", Float) = 0
		_CovMasks0_triBlendContrast("CovMasks0_triBlendContrastOverride", Float) = 0
		_CoverageAmountOverride("CoverageAmountOverride", Float) = 0
		_HeightMap0ContrastOverride("HeightMap0ContrastOverride", Float) = 0
		_PrecipitationDirOffsetOverride("PrecipitationDirOffsetOverride", Float) = 0

		//Sparkle
		_SparkleOverride("SparkleOverride", Float) = 0
		_SssOverride("SssOverride", Float) = 0
		_SparkleTexSSOverride("SparkleTexSSOverride", Float) = 0
		_SparkleTexLSOverride("SparkleTexLSOverride", Float) = 0
		_SparklesAmountOverride("SparklesAmountOverride", Float) = 0
		_SparkleDistFalloffOverride("SparkleDistFalloffOverride", Float) = 0
		_SparkleTexOverride("SparkleTexOverride", Float) = 0
		_LocalSparkleTilingOverride("LocalSparkleTilingOverride", Float) = 0
		_SparklesLightmapMaskPowerOverride("SparklesLightmapMaskPowerOverride", Float) = 0
		_SparklesLightmapMaskPowerOverride("SparklesLightmapMaskPowerOverride", Float) = 0
		_SparklesBrightnessOverride("SparklesBrightnessOverride", Float) = 0
		_SparkleBrightnessRTOverride("SparkleBrightnessRT", Float) = 0
		_ScreenSpaceSparklesTilingOverride("ScreenSpaceSparklesTilingOverride", Float) = 0
		_SparklesHighlightMaskExpansionOverride("SparklesHighlightMaskExpansionOverride", Float) = 0

		//_SnowAOIntensityOverride("SnowAOIntensityOverride", Float) = 0
		_BlendByNormalsPowerOverride("BlendByNormalsPowerOverride", Float) = 0
		_BlendByNormalsStrengthOverride("BlendByNormalsStrengthOverride", Float) = 0
		_CoverageNormalScale0Override("CoverageNormalScaleOverride", Float) = 0
		_PrecipitationDirRangeOverride("PrecipitationDirRangeOverride", Float) = 0
		_CoverageAreaMaskRangeOverride("CoverageAreaMaskRangeOverride", Float) = 0
		_CoverageAreaBiasOverride("CoverageAreaBiasOverride", Float) = 0
		_CoverageLeakReductionOverride("CoverageLeakReductionOverride", Float) = 0
		_CoverageTilingOverride("CoverageTilingOverride", Float) = 0
		_BaseCoverageNormalsBlendOverride("BaseCoverageNormalsBlendOverride", Float) = 0
		_CoverageNormalsOverlayOverride("CoverageNormalsOverlayOverride", Float) = 0
		_MicroReliefFadeOverride("MicroReliefFadeOverride", Float) = 0
		_CoverageMicroReliefOverride("CoverageMicroReliefOverride", Float) = 0
		_CoverageSmoothnessContrastOverride("CoverageSmoothnessContrastOverride", Float) = 0
		_DisplacementOverride("DisplacementOverride", Float) = 0
		_StochasticOverride("StochasticOverride", Float) = 0
		

		_TessellationOverride("TessellationOverride", Float) = 0
		_TessFactorSnowOverride("TessFactorSnowOverride", Float) = 0
		_TessEdgeLOverride("TessEdgeLOverride", Float) = 0
		_TessMaxDispOverride("TessMaxDispOverride", Float) = 0
		_TessSnowdriftRangeOverride("TessSnowdriftRangeOverride", Float) = 0

		_SSS_intensityOverride("SSS_intensityOverride", Float) = 0

		[HideInInspector] _Mode ("__mode", Float) = 0.0
    }

	CGINCLUDE
		#define SRS_SNOW_COVERAGE_SHADER //define this shader as a snow coverage shader
	ENDCG
	
	SubShader
    {
		LOD 300
		Tags
		{
			"TerrainCompatible" = "True"
			"Queue" = "Geometry-100"
    	    "RenderType" = "Opaque"
			"SRSGroupName" = "Terrain"
		}
		
        Pass
        {
            Tags { "LightMode"="ForwardBase" }
			ZWrite On

            CGPROGRAM
			#define SRS_TERRAIN
			#define FORWARD_BASE_PASS
			#pragma shader_feature_local _COVERAGE_ON
			#pragma multi_compile _ SHADOWS_SCREEN
			#pragma multi_compile_fwdbase
			#pragma multi_compile_fog	
			#pragma multi_compile _ LIGHTMAP_ON
			
			#pragma target 4.6

			#pragma multi_compile_instancing

			#pragma vertex TessellationVertexProgram
			#pragma hull HullProgram
			#pragma domain DomainProgram
            #pragma fragment frag

			#include_with_pragmas "../../CGIncludes/SRS_VertFrag.cginc"
			#include_with_pragmas "../../CGIncludes/SRS_Tessellation.cginc"

            ENDCG
        }
		
		Pass
        {
            Tags { "LightMode"="ForwardAdd" } 

			Blend One One
			ZWrite Off

            CGPROGRAM
			#define SRS_TERRAIN
			#pragma shader_feature_local _COVERAGE_ON
			#pragma multi_compile_fwdadd_fullshadows
			#pragma multi_compile_fog

			#pragma target 4.6

			#pragma multi_compile_instancing

			#pragma vertex TessellationVertexProgram
			#pragma hull HullProgram
			#pragma domain DomainProgram
            #pragma fragment frag  
			
			#include_with_pragmas "../../CGIncludes/SRS_VertFrag.cginc"
			#include_with_pragmas "../../CGIncludes/SRS_Tessellation.cginc"

            ENDCG
        }

		Pass
        {
            Tags { "LightMode"="Deferred" }

            CGPROGRAM
			#define SRS_TERRAIN
			#define DEFERRED_PASS
			#pragma multi_compile_prepassfinal
			#pragma shader_feature_local _COVERAGE_ON
			#pragma multi_compile _ SHADOWS_SCREEN
			#pragma exclude_renderers nomrt
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma multi_compile _ UNITY_HDR_ON
			#pragma target 4.6

			#pragma multi_compile_instancing   

			#pragma vertex TessellationVertexProgram
			#pragma hull HullProgram
			#pragma domain DomainProgram
            #pragma fragment frag
			
			#include_with_pragmas "../../CGIncludes/SRS_VertFrag.cginc"
			#include_with_pragmas "../../CGIncludes/SRS_Tessellation.cginc"

            ENDCG
        }
		
		Pass
        {
            Tags { "LightMode"="ShadowCaster" }

			ZWrite On

            CGPROGRAM
			#define SRS_TERRAIN
			#define SHADOW_CASTER_PASS
			#pragma shader_feature_local _COVERAGE_ON
			#pragma target 4.6

			#pragma multi_compile_instancing 

			#pragma vertex TessellationVertexProgram
			#pragma hull HullProgram
			#pragma domain DomainProgram
            #pragma fragment frag
			

			#include_with_pragmas "../../CGIncludes/SRS_VertFrag.cginc"
			#include_with_pragmas "../../CGIncludes/SRS_Tessellation.cginc"

            ENDCG
        }
		Pass
        {
            Tags { "LightMode"="Meta" }

			Cull Off

            CGPROGRAM
			#pragma target 3.0

			#pragma vertex VertMeta
            #pragma fragment FragMeta

			#include_with_pragmas "../../CGIncludes/SRS_TerrainLightmapping.cginc"

            ENDCG
        }
		UsePass "Hidden/Nature/Terrain/Utilities/PICKING"
        UsePass "Hidden/Nature/Terrain/Utilities/SELECTION"
    }
	Fallback "NOT_Lonely/Weatherade/Snow Coverage (Terrain)"
    Dependency "AddPassShader" = "Hidden/NOT_Lonely/Weatherade/SnowCoverageTerrain (AddPass-Tessellation)"
    Dependency "BaseMapShader" = "Hidden/TerrainEngine/Splatmap/Standard-Base"
    Dependency "BaseMapGenShader" = "Hidden/NOT_Lonely/Weatherade/SnowCoverage_TerrainBaseGen"
    CustomEditor "NL_SRS_SnowCoverage_GUI"
}