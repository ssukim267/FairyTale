// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Hidden/NOT_Lonely/Weatherade/Rain Coverage (Terrain-Bake)" {
    Properties {
        // used in fallback on old cards & base map
        [HideInInspector] _MainTex ("BaseMap (RGB)", 2D) = "white" {}
        [HideInInspector] _Color ("Main Color", Color) = (1,1,1,1)
        [HideInInspector] _TerrainHolesTexture("Holes Map (RGB)", 2D) = "white" {}
        
        //rain shader properties
		[Toggle(_COVERAGE_ON)] _Coverage("Coverage", Float) = 1
		[Toggle(_STOCHASTIC_ON)] _Stochastic("Stochastic", Float) = 0
		_PaintedMask("PaintedMask", 2D) = "gray" {}
		_AlbedoLOD("AlbedoLOD", 2D) = "white" {}
		_NormalLOD("NormalLOD", 2D) = "bump" {}
		_DistanceFadeStart("DistanceFadeStart", Float) = 150
		_DistanceFadeFalloff("DistanceFadeFalloff", Float) = 1
		//_RipplesTex("RipplesTex", 2DArray) = "bump" {}
		//("PrimaryMasks", 2D) = "black" {}
		_WetnessAmount("WetnessAmount", Range( 0 , 1)) = 1
		[NoAlpha]_WetColor("WetColor", Color) = (0.8349056,0.9156185,1,1)
		_PuddlesTiling("PuddlesTiling", Float) = 1
		_PuddlesAmount("PuddlesAmount", Range( 0 , 1)) = 0.5
		_PuddlesMult("PuddlesMult", Range(0, 1)) = 0.8
		_PrecipitationDirOffset("PrecipitationDirOffset", Range( -1 , 2)) = 0
		_CoverageAreaBias("Coverage Area Bias", Range( 0.001 , 0.3)) = 0.001
		_CoverageLeakReduction("CoverageLeakReduction", Range( 0.0 , 0.99)) = 0.0
		_BlendByNormalsStrength("Blend By Normals Strength", Float) = 1
		_BlendByNormalsPower("Blend By Normals Power", Float) = 1
		_PuddlesSlope("PuddlesSlope", Range( 0 , 1)) = 0.05
		_CoverageAreaFalloffHardness("CoverageAreaFalloffHardness", Range( 0 , 1)) = 0.5
		_PrecipitationDirRange("PrecipitationDirRange", Vector) = (0,1,0,0)
		_CoverageAreaMaskRange("CoverageAreaMaskRange", Range(0, 1)) = 1
		
		[Toggle(_DRIPS_ON)] _Drips("Drips", Float) = 1
		_DripsTiling("DripsTiling", Vector) = (2.5,1,0,0)
		_DripsIntensity("DripsIntensity", Range( 0 , 5)) = 1
		_DripsSpeed("DripsSpeed", Float) = 0.2
		_PuddlesRange("PuddlesRange", Vector) = (0,1,0,0)
		_RipplesTiling("RipplesTiling", Float) = 1
		_SpotsAmount("SpotsAmount", Range( 0 , 1)) = 0.85

		[Toggle(_RIPPLES_ON)] _Ripples("Ripples", Float) = 1
		_RipplesAmount("RipplesAmount", Range(0, 15)) = 3
		_RipplesIntensity("RipplesIntensity", Float) = 0.5
		_DistortionTiling("DistortionTiling", Float) = 2
		_DistortionAmount("DistortionAmount", Float) = 0.002
		_SpotsIntensity("SpotsIntensity", Range( 0 , 5)) = 1
		_PuddlesBlendContrast("PuddlesBlendContrast", Range( 0 , 1)) = 0
		_PuddlesBlendStrength("PuddlesBlendStrength", Float) = 1
		_RipplesFramesCount("RipplesFramesCount", Range( 0 , 64)) = 64
		_RipplesFPS("RipplesFPS", Range( 0 , 120)) = 30
		[Toggle(_PAINTABLE_COVERAGE_ON)] _PaintableCoverage("PaintableCoverage", Float) = 0
		[Toggle(_DRIPS_ON)] _Drips("Drips", Float) = 1
		[HideInInspector] _TilingMultiplier("TilingMultiplier", Range(0 , 1)) = 1 // needed to set adjust the splats tiling when baking the distant map
		_MapID("MapID", float) = 0 //needed to set a particular map when baking the distant map

		//rain shader overrides
		_CoverageOverride("CoverageOverride", Float) = 0
		_StochasticOverride("StochasticOverride", Float) = 0
		_WetColorOverride("WetColorOverride", Float) = 0
		_EmissionMaskingOverride("EmissionMaskingOverride", Float) = 0
		_PaintableCoverageOverride("PaintableCoverageOverride", Float) = 0
		_DripsOverride("DripsOverride", Float) = 0
		_RipplesOverride("RipplesOverride", Float) = 0
		//_PrimaryMasksOverride("PrimaryMasksOverride", Float) = 0
		_WetnessAmountOverride("WetnessAmountOverride", Float) = 0
		_BlendByNormalsPowerOverride("BlendByNormalsPowerOverride", Float) = 0
		_BlendByNormalsStrengthOverride("BlendByNormalsStrengthOverride", Float) = 0
		_PrecipitationDirOffsetOverride("PrecipitationDirOffsetOverride", Float) = 0
		_PrecipitationDirRangeOverride("PrecipitationDirRangeOverride", Float) = 0
		_CoverageAreaMaskRangeOverride("CoverageAreaMaskRangeOverride", Float) = 0
		_CoverageAreaBiasOverride("CoverageAreaBiasOverride", Float) = 0
		_CoverageLeakReductionOverride("CoverageLeakReductionOverride", Float) = 0
		_PuddlesSlopeOverride("PuddlesSlopeOverride", Float) = 0
		_PuddlesAmountOverride("PuddlesAmountOverride", Float) = 0
		_PuddlesMultOverride("PuddlesMultOverride", Float) = 0
		_PuddlesRangeOverride("PuddlesRangeOverride", Float) = 0
		_RipplesAmountOverride("RipplesAmountOverride", Float) = 0
		_PuddlesTilingOverride("PuddlesTilingOverride", Float) = 0
		_RipplesFramesCountOverride("RipplesFramesCountOverride", Float) = 0
		_RipplesFPSOverride("RipplesFPSOverride", Float) = 0
		_RipplesTilingOverride("RipplesTilingOverride", Float) = 0
		_DistortionAmountOverride("DistortionAmountOverride", Float) = 0
		_DistortionTilingOverride("DistortionTilingOverride", Float) = 0
		_DripsTilingOverride("DripsTilingOverride", Float) = 0
		_DripsSpeedOverride("DripsSpeedOverride", Float) = 0
		_DripsIntensityOverride("DripsIntensityOverride", Float) = 0
		_PuddlesBlendContrastOverride("PuddlesBlendContrastOverride", Float) = 0
		_SpotsIntensityOverride("SpotsIntensityOverride", Float) = 0
		_PuddlesBlendStrengthOverride("PuddlesBlendStrengthOverride", Float) = 0
		_SpotsAmountOverride("SpotsAmountOverride", Float) = 0
		//_RipplesTexOverride("RipplesTexOverride", Float) = 0
		_RipplesIntensityOverride("RipplesIntensityOverride", Float) = 0
    }

    SubShader
    {
		Tags
		{
			"TerrainCompatible" = "True"
			"Queue" = "Geometry-100"
    	    "RenderType" = "Opaque"
		}
		
        Pass
        {
            Tags 
            {
            "LightMode"="ForwardBase"
            }

			ZWrite On

            CGPROGRAM
			#define SRS_TERRAIN
			#define FORWARD_BASE_PASS

			#pragma shader_feature_local _COVERAGE_ON
			#pragma shader_feature_local_fragment _PAINTABLE_COVERAGE_ON
			#pragma shader_feature_local_fragment _DRIPS_ON
			#pragma shader_feature_local_fragment _STOCHASTIC_ON
			#pragma shader_feature_local _RIPPLES_ON
			
			#pragma target 3.0

            #pragma vertex vert
            #pragma fragment frag
			
			#pragma multi_compile_instancing
			#pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap

			#include_with_pragmas "../../CGIncludes/SRS_TerrainBake.cginc"
			#include_with_pragmas "../../CGIncludes/SRS_VertFrag.cginc"

            ENDCG
        }

    }

    Dependency "AddPassShader" = "Hidden/NOT_Lonely/Weatherade/RainTerrainBakeAddPass"
    CustomEditor "NL_SRS_RainCoverage_GUI"

    //Fallback "Nature/Terrain/Diffuse"
}
