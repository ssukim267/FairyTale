Shader "Hidden/NOT_Lonely/Weatherade/Snow Coverage (Tessellation)"
{
    Properties
    {
        // used in fallback on old cards & base map
        _MainTex ("BaseMap (RGB)", 2D) = "white" {}
		_MetallicGlossMap ("Metallic Map", 2D) = "black" {}
		[Normal] _BumpMap ("Normal", 2D) = "bump" {}
		_OcclusionMap ("Occlusion Map", 2D) = "white" {}
		_EmissionMap("Emission Map", 2D) = "black" {}
		[Gamma]_Metallic("Metallic Scale", Range(0, 1)) = 1
		_GlossMapScale("Gloass Map Scale", Range(0, 1)) = 1
		_Glossiness("Gloassiness", Range(0, 1)) = 0.5
		_BumpScale("Normal Scale", Float) = 1	
		_OcclusionStrength("Occlusion Strength", Range( 0 , 1)) = 1
		_Color ("Main Color", Color) = (1,1,1,1)
		_Cutoff("Alpha Cutoff", Range(0, 1)) = 0.5
		_EmissionColor("Emission Color", Color) = (0,0,0)
		[Enum(Metallic Alpha,0,Albedo Alpha,1)] _SmoothnessTextureChannel ("Smoothness texture channel", Float) = 0
		[Enum(Occlusion Map (G),0,Metallic Map (G),1)] _OcclusionTextureChannel ("Occlusion texture channel", Float) = 0
		[Toggle(_USE_BLUE_NOISE_DITHER)] _UseBlueNoiseDither("UseBlueNoiseDither", Float) = 0

		//toggles
		_UseGlossMap("Use Gloss Map", Float) = 0
		//_Emission("_Emission", Float) = 0 
		_UseEmissionMap("Use Emission Map", Float) = 0 
		_SpecularHighlights("Specular Highlights", Float) = 1
		_GlossyReflections("Glossy Reflections", Float) = 1
		[HideInInspector] _Mode ("__mode", Float) = 0.0
		[HideInInspector] _SrcBlend ("_SrcBlend", Float) = 1
		[HideInInspector] _DstBlend ("_DstBlend", Float) = 0
		[HideInInspector] _ZWrite ("_ZWrite", Float) = 1
		[Toggle]_AlphaToCoverage("AlphaToCoverage", Float) = 0
		_CullMode("CullMode", Float) = 2
        
        //snow shader properties
		[Toggle(_COVERAGE_ON)] _Coverage("Coverage", Float) = 1
		[Toggle(_PAINTABLE_COVERAGE_ON)] _PaintableCoverage("PaintableCoverage", Float) = 0

		[Toggle] _Tessellation("Tessellation", Float) = 0
		[Toggle(_DISPLACEMENT_ON)] _Displacement("Displacement", Float) = 0
		//[Toggle(_STOCHASTIC_ON)] _Stochastic("Stochastic", Float) = 0

		//_BlueNoise("BlueNoise", 2DArray) = "white" {}
		[Toggle(_USE_AVERAGED_NORMALS)] _UseAveragedNormals("_UseAveragedNormals", Float) = 0
		_CoverageTex0("CoverageTex0", 2D) = "bump" {}
		_CovMasks0_triBlendContrast("CovMasks0_triBlendContrast", Float) = 2.5
		_CoverageAmount("CoverageAmount", Range( 0 , 1)) = 1
		_HeightMap0Contrast("HeightMap0Contrast", Range(0, 1)) = 0.2
		_EmissionMasking("EmissionMasking", Range(0, 1)) = 1
		_MaskCoverageByAlpha("MaskCoverageByAlpha", Range(0, 1)) = 1
		[NoAlpha]_CoverageColor("CoverageColor", Color) = (0.8349056,0.9156185,1,1)
		_BaseCoverageNormalsBlend("Base/CoverageNormalsBlend", Range( 0 , 1)) = 0.2588235
		_CoverageNormalsOverlay("Coverage Normals Overlay", Range( 0 , 1)) = 0.5080121
		_CoverageTiling("Coverage Tiling", Float) = 0.14
		_CoverageNormalScale0("Coverage Normal Scale 0", Float) = 1
		_BlendByNormalsStrength("Blend By Normals Strength", Float) = 2
		_BlendByNormalsPower("Blend By Normals Power", Float) = 5
		
		//Sparkle
		[Toggle(_SPARKLE_ON)] _Sparkle("Sparkle", Float) = 0
		[Toggle(_SSS_ON)] _Sss("SSS", Float) = 0
		[Toggle(_SPARKLE_TEX_SS)] _SparkleTexSS("Sparkle Tex SS", Float) = 0
		[Toggle(_SPARKLE_TEX_LS)] _SparkleTexLS("Sparkle Tex LS", Float) = 0
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
		
		_TessEdgeL("TessEdgeL", Range( 5 , 100)) = 20
		_TessFactorSnow("TessFactorSnow", Range(0, 1)) = 0.5
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

		_CoverageAreaMaskRange("CoverageAreaMaskRange", Range(0, 1)) = 1
		_PrecipitationDirRange("PrecipitationDirRange", Vector) = (0,1,0,0)
		_CoverageAreaBias("Coverage Area Bias", Range( 0.001 , 0.3)) = 0.001
		_CoverageLeakReduction("CoverageLeakReduction", Range( 0.0 , 0.99)) = 0.0
		_PrecipitationDirOffset("PrecipitationDirOffset", Range( -1 , 1)) = 0

		//displacement
		_CoverageDisplacement("CoverageDisplacement", Float) = 0.5
		_CoverageDisplacementOffset("CoverageDisplacementOffset", Range( 0 , 1)) = 0.5
		
		//Property overrides
		_CoverageOverride("CoverageOverride", Float) = 0
		_CoverageColorOverride("CoverageColorOverride", Float) = 0
		_CoverageDisplacementOverride("CoverageDisplacementOverride", Float) = 0
		
		//Trace overrides
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
		
		_EmissionMaskingOverride("EmissionMaskingOverride", Float) = 0
		_MaskCoverageByAlphaOverride("MaskCoverageByAlphaOverride", Float) = 0
		_CoverageDisplacementOffsetOverride("CoverageDisplacementOffsetOverride", Float) = 0
		_DistanceFadeFalloffOverride("DistanceFadeFalloffOverride", Float) = 0
		_DistanceFadeStartOverride("DistanceFadeStartOverride", Float) = 0
		_PaintableCoverageOverride("PaintableCoverageOverride", Float) = 0
		_UseAveragedNormalsOverride("UseAveragedNormalsOverride", Float) = 0
		_CoverageTex0Override("CoverageTex0Override", Float) = 0
		_CovMasks0_triBlendContrast("CovMasks0_triBlendContrastOverride", Float) = 0
		_CoverageAmountOverride("CoverageAmountOverride", Float) = 0
		_HeightMap0ContrastOverride("HeightMap0ContrastOverride", Float) = 0
		_PrecipitationDirOffsetOverride("PrecipitationDirOffsetOverride", Float) = 0

		//Sparkle overrides
		_SparkleOverride("SparkleOverride", Float) = 0
		_SssOverride("SssOverride", Float) = 0
		_SSS_intensityOverride("SSS_intensityOverride", Float) = 0
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
		_DisplacementOverride("DisplacementOverride", Float) = 0
		//_StochasticOverride("StochasticOverride", Float) = 0
		
		//Tessellation overrides
		_TessellationOverride("TessellationOverride", Float) = 0
		_TessEdgeLOverride("TessEdgeLOverride", Float) = 0
		_TessFactorSnowOverride("TessFactorSnowOverride", Float) = 0
		_TessMaxDispOverride("TessMaxDispOverride", Float) = 0
		_TessSnowdriftRangeOverride("TessSnowdriftRangeOverride", Float) = 0
    }

	CGINCLUDE
		#define SRS_SNOW_COVERAGE_SHADER //define this shader as a snow coverage shader
	ENDCG

	SubShader
    {
		Tags {"SRSGroupName" = "Opaque"}
		
        Pass
        {
            Tags 
            {
            "LightMode"="ForwardBase"
            }
			Blend [_SrcBlend] [_DstBlend]
			ZWrite [_ZWrite]
			Cull [_CullMode]
			AlphaToMask [_AlphaToCoverage]

            CGPROGRAM
			#define FORWARD_BASE_PASS
			#pragma shader_feature_local _COVERAGE_ON
			#pragma multi_compile_fwdbase
			#pragma multi_compile_fog	
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma shader_feature_local _EMISSION
			#pragma shader_feature _ _RENDERING_CUTOUT _RENDERING_FADE _RENDERING_TRANSPARENT
			#pragma shader_feature_local _METALLIC_MAP
			#pragma shader_feature_local_fragment _OCCLUSION_MAP
			#pragma shader_feature_local_fragment _EMISSION_MAP
			#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
			#pragma shader_feature_local_fragment _OCCLUSION_TEX_METALLIC_CHANNEL_G
			#pragma shader_feature_local _USE_AVERAGED_NORMALS
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma shader_feature_local _USE_BLUE_NOISE_DITHER
			
			#pragma target 4.6

			#pragma vertex TessellationVertexProgram
			#pragma hull HullProgram
			#pragma domain DomainProgram
			#pragma fragment frag
			
			#pragma multi_compile_instancing
			#pragma instancing_options lodfade	  
			
			#include_with_pragmas "../../CGIncludes/SRS_VertFrag.cginc"
			#include_with_pragmas "../../CGIncludes/SRS_Tessellation.cginc"

            ENDCG
        }
		
		Pass
        {
            Tags {"LightMode"="ForwardAdd"}

			Blend [_SrcBlend] One
			ZWrite Off
			Cull [_CullMode]

            CGPROGRAM
			#pragma multi_compile_fwdadd_fullshadows
			#pragma multi_compile_fog
			#pragma shader_feature_local _COVERAGE_ON
			#pragma shader_feature _ _RENDERING_CUTOUT _RENDERING_FADE _RENDERING_TRANSPARENT
			#pragma shader_feature_local _METALLIC_MAP
			#pragma shader_feature_local_fragment _OCCLUSION_MAP
			#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
			#pragma shader_feature_local_fragment _OCCLUSION_TEX_METALLIC_CHANNEL_G
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma shader_feature_local _USE_BLUE_NOISE_DITHER

			#pragma target 4.6

			#pragma vertex TessellationVertexProgram
			#pragma hull HullProgram
			#pragma domain DomainProgram
			#pragma fragment frag

			#pragma multi_compile_instancing
			#pragma instancing_options lodfade
			
			#include_with_pragmas "../../CGIncludes/SRS_VertFrag.cginc"
			#include_with_pragmas "../../CGIncludes/SRS_Tessellation.cginc"


            ENDCG
        }

		Pass
        {
            Tags {"LightMode"="Deferred"}
			Cull [_CullMode]

            CGPROGRAM
			#define DEFERRED_PASS
			#pragma shader_feature_local _COVERAGE_ON
			#pragma exclude_renderers nomrt
			#pragma multi_compile_prepassfinal
			#pragma multi_compile _ UNITY_HDR_ON
			#pragma shader_feature_local _EMISSION
			#pragma shader_feature _ _RENDERING_CUTOUT _RENDERING_FADE _RENDERING_TRANSPARENT
			#pragma shader_feature_local _METALLIC_MAP
			#pragma shader_feature_local_fragment _OCCLUSION_MAP
			#pragma shader_feature_local_fragment _EMISSION_MAP
			#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
			#pragma shader_feature_local_fragment _OCCLUSION_TEX_METALLIC_CHANNEL_G
			#pragma shader_feature_local _USE_AVERAGED_NORMALS
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma shader_feature_local _USE_BLUE_NOISE_DITHER
			
			#pragma target 4.6

			#pragma vertex TessellationVertexProgram
			#pragma hull HullProgram
			#pragma domain DomainProgram
			#pragma fragment frag

			#pragma multi_compile_instancing 
			#pragma instancing_options lodfade
			
			#include_with_pragmas "../../CGIncludes/SRS_VertFrag.cginc"
			#include_with_pragmas "../../CGIncludes/SRS_Tessellation.cginc"

            ENDCG
        }
		
		Pass
        {
            Tags {"LightMode"="ShadowCaster"}
			
			ZWrite On
			Cull [_CullMode]

            CGPROGRAM
			#define SHADOW_CASTER_PASS
			#pragma target 4.6
			#pragma shader_feature_local _COVERAGE_ON
			#pragma shader_feature _ _RENDERING_CUTOUT _RENDERING_FADE _RENDERING_TRANSPARENT
			#pragma shader_feature_local _USE_AVERAGED_NORMALS
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma shader_feature_local _USE_BLUE_NOISE_DITHER

			#pragma vertex TessellationVertexProgram
			#pragma hull HullProgram
			#pragma domain DomainProgram
			#pragma fragment frag

			#pragma multi_compile_instancing
			#pragma instancing_options lodfade

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

			#pragma shader_feature_local _EMISSION
			#pragma shader_feature_local _METALLIC_MAP
			#pragma shader_feature_local_fragment _EMISSION_MAP
			#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			#pragma vertex VertMeta
            #pragma fragment FragMeta

			#include_with_pragmas "../../CGIncludes/SRS_Lightmapping.cginc"

            ENDCG
        }
    }

	Fallback "NOT_Lonely/Weatherade/Snow Coverage"

    CustomEditor "NL_SRS_SnowCoverage_GUI"
}