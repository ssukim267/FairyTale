Shader "NOT_Lonely/Weatherade/Rain Coverage"
{
    Properties
    {
        //Standard shader properties
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
	    //_Emission("_Emission", Color) = (0,0,0)
	    _UseEmissionMap("Use Emission Map", Float) = 0 
	    _SpecularHighlights("Specular Highlights", Float) = 1
	    _GlossyReflections("Glossy Reflections", Float) = 1
	    [HideInInspector] _Mode ("__mode", Float) = 0.0
	    [HideInInspector] _SrcBlend ("_SrcBlend", Float) = 1
	    [HideInInspector] _DstBlend ("_DstBlend", Float) = 0
	    [HideInInspector] _ZWrite ("_ZWrite", Float) = 1
	    [Toggle]_AlphaToCoverage("AlphaToCoverage", Float) = 0
	    _CullMode("CullMode", Float) = 2

        //Rain coverage properties
        [Toggle(_COVERAGE_ON)] _Coverage("Coverage", Float) = 1
		[Toggle(_STOCHASTIC_ON)] _Stochastic("Stochastic", Float) = 0
		[Toggle(_USE_AVERAGED_NORMALS)] _UseAveragedNormals("_UseAveragedNormals", Float) = 0
		_PaintedMask("PaintedMask", 2D) = "gray" {}
		//_RipplesTex("RipplesTex", 2DArray) = "bump" {}
		//_PrimaryMasks("PrimaryMasks", 2D) = "black" {}
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

        //rain shader overrides
		_UseAveragedNormalsOverride("UseAveragedNormalsOverride", Float) = 0
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
		_PuddlesRangeOverride("PuddlesRangeOverride", Float) = 0
		_PuddlesMultOverride("PuddlesMultOverride", Float) = 0
		_PuddlesTilingOverride("PuddlesTilingOverride", Float) = 0
		_RipplesAmountOverride("RipplesAmountOverride", Float) = 0
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
			"SRSGroupName" = "Opaque"
		}
		
        Pass
        {
            Name "FORWARD"
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
			#pragma multi_compile_fwdbase
			#pragma multi_compile_fog	
			#pragma multi_compile _ LIGHTMAP_ON
			#pragma shader_feature_fragment _EMISSION
			#pragma shader_feature _ _RENDERING_CUTOUT _RENDERING_FADE _RENDERING_TRANSPARENT
			#pragma shader_feature_local _METALLIC_MAP
			#pragma shader_feature_local_fragment _OCCLUSION_MAP
			#pragma shader_feature_local_fragment _EMISSION_MAP
			#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
			#pragma shader_feature_local_fragment _OCCLUSION_TEX_METALLIC_CHANNEL_G
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile _ _USE_BLUE_NOISE_DITHER

			#pragma shader_feature_local _COVERAGE_ON
			#pragma shader_feature_local _USE_AVERAGED_NORMALS
			#pragma shader_feature_local_fragment _PAINTABLE_COVERAGE_ON
			#pragma shader_feature_local_fragment _DRIPS_ON
			#pragma shader_feature_local_fragment _STOCHASTIC_ON
			#pragma shader_feature_local _RIPPLES_ON

			#pragma multi_compile_instancing
			
			#pragma target 3.0

            #pragma vertex vert
            #pragma fragment frag

			#include_with_pragmas "../../CGIncludes/SRS_VertFrag.cginc"

            ENDCG
        }
		
		Pass
        {   
            Name "FORWARD_DELTA"
            Tags 
            {
            "LightMode"="ForwardAdd"
            }

			Blend [_SrcBlend] One
			ZWrite Off
			Cull [_CullMode]

            CGPROGRAM
			#pragma multi_compile_fwdadd_fullshadows
			#pragma multi_compile_fog
			#pragma shader_feature _ _RENDERING_CUTOUT _RENDERING_FADE _RENDERING_TRANSPARENT
			#pragma shader_feature_local _METALLIC_MAP
			#pragma shader_feature_local_fragment _OCCLUSION_MAP
			#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
			#pragma shader_feature_local_fragment _OCCLUSION_TEX_METALLIC_CHANNEL_G
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile _ _USE_BLUE_NOISE_DITHER

			#pragma shader_feature_local _COVERAGE_ON
			#pragma shader_feature_local_fragment _DRIPS_ON
			#pragma shader_feature_local_fragment _STOCHASTIC_ON
			#pragma shader_feature_local _RIPPLES_ON

			#pragma multi_compile_instancing
			
			#pragma target 3.0

            #pragma vertex vert
            #pragma fragment frag
			
			#include_with_pragmas "../../CGIncludes/SRS_VertFrag.cginc"

            ENDCG
        }

		Pass
        {
            Name "DEFERRED"
            Tags 
            {
            "LightMode"="Deferred"
            }
			Cull [_CullMode]

            CGPROGRAM
			#define DEFERRED_PASS
			#pragma exclude_renderers nomrt
			#pragma multi_compile_prepassfinal
			#pragma multi_compile _ UNITY_HDR_ON

			#pragma shader_feature_fragment _EMISSION
			#pragma shader_feature _ _RENDERING_CUTOUT _RENDERING_FADE _RENDERING_TRANSPARENT
			#pragma shader_feature_local _METALLIC_MAP
			#pragma shader_feature_local_fragment _OCCLUSION_MAP
			#pragma shader_feature_local_fragment _EMISSION_MAP
			#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
			#pragma shader_feature_local_fragment _OCCLUSION_TEX_METALLIC_CHANNEL_G
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile _ _USE_BLUE_NOISE_DITHER

			#pragma shader_feature_local _COVERAGE_ON
			#pragma shader_feature_local _USE_AVERAGED_NORMALS
			#pragma shader_feature_local_fragment _PAINTABLE_COVERAGE_ON
			#pragma shader_feature_local_fragment _DRIPS_ON
			#pragma shader_feature_local_fragment _STOCHASTIC_ON
			#pragma shader_feature_local _RIPPLES_ON
			
			#pragma multi_compile_instancing
			
			#pragma target 3.0

            #pragma vertex vert
            #pragma fragment frag
			
			#include_with_pragmas "../../CGIncludes/SRS_VertFrag.cginc"

            ENDCG
        }
		
		Pass
        {
            Name "ShadowCaster"
            Tags 
            {
            "LightMode"="ShadowCaster"
            }
			
			ZWrite On
			Cull [_CullMode]

            CGPROGRAM
			#define SHADOW_CASTER_PASS
			#pragma shader_feature _ _RENDERING_CUTOUT _RENDERING_FADE _RENDERING_TRANSPARENT
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile _ _USE_BLUE_NOISE_DITHER

			#pragma multi_compile_instancing
			
			#pragma target 3.0

            #pragma vertex vert
            #pragma fragment frag

			#include_with_pragmas "../../CGIncludes/SRS_VertFrag.cginc"

            ENDCG
        }
		Pass
        {
            Name "META"
            Tags { "LightMode"="Meta" }

			Cull Off

            CGPROGRAM
			#pragma target 3.0

			#pragma multi_compile_local __ _EMISSION
			#pragma shader_feature_local _METALLIC_MAP
			#pragma shader_feature_local_fragment _EMISSION_MAP
			#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			#pragma vertex VertMeta
            #pragma fragment FragMeta

			#include_with_pragmas "../../CGIncludes/SRS_Lightmapping.cginc"

            ENDCG
        }
    }

	CustomEditor "NL_SRS_RainCoverage_GUI"

    FallBack "Diffuse"
}
