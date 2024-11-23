#ifndef SRS_SNOW_VERT_FRAG
#define SRS_SNOW_VERT_FRAG 

#include "Lighting.cginc"
#include "UnityPBSLighting.cginc"
#include "AutoLight.cginc"
#include "UnityStandardBRDF.cginc"

#pragma editor_sync_compilation

sampler3D _DitherMaskLOD;

#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
#endif

#if defined(SRS_TERRAIN)
    #define TERRAIN_INSTANCED_PERPIXEL_NORMAL
    #if defined(_PAINTABLE_COVERAGE_ON)
        UNITY_DECLARE_TEX2D_NOSAMPLER(_PaintedMask);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_PaintedMaskNormal);
        float4 _PaintedMask_TexelSize;
    #endif
    #if !defined(SHADOW_CASTER_PASS) && !defined (_REPLACEMENT)
        UNITY_DECLARE_TEX2D_NOSAMPLER(_AlbedoLOD);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_NormalLOD);
    #endif
#endif

#if !defined(SHADOW_CASTER_PASS)        	
    #pragma multi_compile_local __ _NORMALMAP
#else
    #if defined(_RENDERING_FADE) || defined(_RENDERING_TRANSPARENT)
	    #define SHADOWS_SEMITRANSPARENT 1
    #endif
#endif

uniform float _Cutoff;
uniform float _MaskCoverageByAlpha;
uniform float _CoverageLeakReduction;
uniform float _CullMode;

#pragma multi_compile_local_fragment __ _ALPHATEST_ON

struct VertexData
{
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    #if !defined(SRS_TERRAIN)
        float4 color : COLOR0;
    #endif
    #if !defined(SHADOW_CASTER_PASS) && !defined (_REPLACEMENT)
        float4 tangent : TANGENT;
        float2 uv1 : TEXCOORD1;
    #endif
    float3 uv : TEXCOORD0;

    #if defined(_USE_AVERAGED_NORMALS)
        half3 unifiedNormal : TEXCOORD3;
    #endif

    float2 uv2 : TEXCOORD2;

    UNITY_VERTEX_INPUT_INSTANCE_ID
};
struct v2fVertex
{   
    UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO

    float4 uv : TEXCOORD0;  
    float4 pos : SV_POSITION;

    #if !defined(SRS_TERRAIN)
        float4 color : COLOR0;
    #endif

    #if defined(_SPARKLE_ON)
        float2 ssUV : COLOR3;
    #endif
        
    #if !defined(SHADOW_CASTER_PASS) && !defined (_REPLACEMENT)
        #if defined(_USE_AVERAGED_NORMALS)
            half3 unifiedWorldNormal : COLOR1;
        #endif 
        half3 tspace0 : TEXCOORD4;
        half3 tspace1 : TEXCOORD5;
        half3 tspace2 : TEXCOORD6;
        half4 tangent : TANGENT;
        float2 lightmapUV : TEXCOORD3;
        #if defined(DYNAMICLIGHTMAP_ON)
		    float2 dynamicLightmapUV : TEXCOORD7;
	    #endif
        float3 worldNormal : NORMAL;
        float eyeDepth : COLOR2;
        float3 worldPos : COLOR4;

        UNITY_SHADOW_COORDS(1)
        #ifndef TERRAIN_BASE_PASS
            UNITY_FOG_COORDS(2) 
        #endif
    #endif
};

struct v2f
{   
    UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO

    float4 uv : TEXCOORD0;

    #if defined(LOD_FADE_CROSSFADE)
		UNITY_VPOS_TYPE vpos : VPOS;
	#else
		float4 pos : SV_POSITION;
	#endif 

    #if !defined(SRS_TERRAIN)
        float4 color : COLOR0;
    #endif

    #if defined(_SPARKLE_ON)
        float2 ssUV : COLOR3;
    #endif
        
    #if !defined(SHADOW_CASTER_PASS) && !defined (_REPLACEMENT)
        #if defined(_USE_AVERAGED_NORMALS)
            half3 unifiedWorldNormal : COLOR1;
        #endif 
        half3 tspace0 : TEXCOORD4;
        half3 tspace1 : TEXCOORD5;
        half3 tspace2 : TEXCOORD6;
        half4 tangent : TANGENT;
        float2 lightmapUV : TEXCOORD3;
        #if defined(DYNAMICLIGHTMAP_ON)
		    float2 dynamicLightmapUV : TEXCOORD7;
	    #endif
        float3 worldNormal : NORMAL;
        float eyeDepth : COLOR2;
        float3 worldPos : COLOR4;

        UNITY_SHADOW_COORDS(1)
        #ifndef TERRAIN_BASE_PASS
            UNITY_FOG_COORDS(2) 
        #endif
    #endif
};

struct FragmentOutput 
{
	#if defined(DEFERRED_PASS)
		float4 gBuffer0 : SV_Target0;
		float4 gBuffer1 : SV_Target1;
		float4 gBuffer2 : SV_Target2;
		float4 gBuffer3 : SV_Target3;
        #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
			float4 gBuffer4 : SV_Target4;
		#endif
	#else
		float4 color : SV_Target;
	#endif
};

	half3 CalcWorldNormalVector(v2f i, float3 tNormal)
	{
	    half3 worldNormal = half3(0, 0, 1);
	    #if !defined(SHADOW_CASTER_PASS) && !defined (_REPLACEMENT)
	        worldNormal.x = dot(i.tspace0, tNormal);
	        worldNormal.y = dot(i.tspace1, tNormal);
	        worldNormal.z = dot(i.tspace2, tNormal);
	    #endif
	    return worldNormal;
	}

#include_with_pragmas "../CGIncludes/SRS_CoverageCommon.cginc"
#if defined(SRS_SNOW_COVERAGE_SHADER)
    #include_with_pragmas "../CGIncludes/SRS_SnowCoverage.cginc"
#else
    #include_with_pragmas "../CGIncludes/SRS_RainCoverage.cginc"
#endif
#if defined(SRS_TERRAIN)
    #include_with_pragmas "../CGIncludes/SRS_TerrainSplatmix.cginc"
#else
    #include_with_pragmas "../CGIncludes/SRS_Base.cginc"
#endif

#if !defined(SHADOW_CASTER_PASS) && !defined (_REPLACEMENT)
    #include_with_pragmas "../CGIncludes/SRS_Lighting.cginc"
#endif

v2fVertex vert (VertexData v)
{
    UNITY_SETUP_INSTANCE_ID(v)
    v2fVertex o;
    UNITY_INITIALIZE_OUTPUT(v2fVertex, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	UNITY_TRANSFER_INSTANCE_ID(v, o);

    #if defined(SRS_TERRAIN)
        TerrainVert(o, v); 
    #else
        //Regular mesh vert function here
        Vert(o, v);
    #endif
    #if !defined(DEFERRED_PASS) && !defined(SHADOW_CASTER_PASS) && !defined (_REPLACEMENT)
	    UNITY_TRANSFER_SHADOW(o, v.uv1)  
    #endif

    #if defined(SHADOW_CASTER_PASS)
        float4 position = UnityClipSpaceShadowCasterPos(v.vertex.xyz, v.normal);
	    o.pos = UnityApplyLinearShadowBias(position);
    #endif

    return o;
}

FragmentOutput frag (v2f i, bool facing : SV_IsFrontFace)
{
    FragmentOutput output;

    UNITY_INITIALIZE_OUTPUT(FragmentOutput, output);
    UNITY_SETUP_INSTANCE_ID(i);
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

    #if defined(LOD_FADE_CROSSFADE)
        #if defined(_USE_BLUE_NOISE_DITHER)
	        AnimatedCrossFadeLOD(i.vpos);
        #else
            UnityApplyDitherCrossFade(i.vpos);
        #endif
	#endif

    #if !defined(SHADOW_CASTER_PASS) && !defined (_REPLACEMENT)
        half4 splat_control;
        float2 splatUV = 0;
        float3 geomN = 0;
        half weight;
        half4 mixedDiffuse = 0;
        float alpha = 1;
        half ao = 1;
        float3 debug = 0;
	    half3 baseNormal = 0;
	    float3 worldMasks = 0;
	    half finalCoverageMask = 0;
	    float snowHeight = 0;
	    float3 specularTint;
        float metallic = 0;
        float3 emission = 0;
        
        #if defined(SRS_TERRAIN)
            SplatmapMix(i, weight, mixedDiffuse, baseNormal, splatUV, geomN);
        #else
            //Regular mesh fragment part: base albedo, smoothness, unpack normals etc.
            BaseMapping(i, mixedDiffuse, alpha, baseNormal, metallic, ao, emission, geomN);
        
        {//Invert normals depending on facing
            fixed face = facing ? 1.0 : -1.0;
            if(_CullMode != 2) i.worldNormal *= face;
            #if !defined(_RENDERING_FADE) && !defined(_RENDERING_TRANSPARENT)
                if(_CullMode != 2) geomN *= face;
            #endif
        }
        #endif
        
        #if defined(_COVERAGE_ON)
            #if defined(SRS_SNOW_COVERAGE_SHADER)
                SnowCoverage(i, mixedDiffuse, alpha, baseNormal, metallic, ao, worldMasks, finalCoverageMask, snowHeight, splatUV, geomN, debug);
            #else
                RainCoverage(i, mixedDiffuse, alpha, splatUV, geomN);
            #endif
        #endif

        #if defined(_RENDERING_CUTOUT)
            clip(alpha - _Cutoff);
        #endif

        //Lighting
        float4 color = 0;
        #if !defined(SRS_TERRAIN_BAKE_SHADER) && !defined(SRS_RAIN_TERRAIN_BAKE_SHADER)
            i.worldNormal = normalize(i.worldNormal);

            float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);

	        float oneMinusReflectivity;
            mixedDiffuse.rgb = DiffuseAndSpecularFromMetallic(mixedDiffuse, metallic, specularTint, oneMinusReflectivity);

            #if defined(DEFERRED_PASS)
                color = mixedDiffuse;
            #endif

            #if defined(_RENDERING_TRANSPARENT)
	            mixedDiffuse.rgb *= alpha;
	            alpha = 1 - oneMinusReflectivity + alpha * oneMinusReflectivity;
	        #endif

            float attenuation;
            UnityLight light = CreateLight(i, attenuation);

            UnityIndirect indirectLight = CreateIndirectLight(i, viewDir, mixedDiffuse.a, ao);

            float3 lightColor = light.color;

            #if defined(LIGHTMAP_ON)
	            #if defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK)
	            	lightColor = _LightColor0.rgb;
                    attenuation = indirectLight.diffuse;
	            #endif
            #endif  

            #if !defined(DEFERRED_PASS) && defined(_COVERAGE_ON) && defined(SRS_SNOW_COVERAGE_SHADER)
                float colorEnhance = pow(mixedDiffuse.a, 3) * finalCoverageMask * attenuation * 2;
                mixedDiffuse.rgb += colorEnhance;
            #endif

            mixedDiffuse.rgb = UNITY_BRDF_PBS(mixedDiffuse, specularTint, oneMinusReflectivity, mixedDiffuse.a, i.worldNormal, viewDir, light, indirectLight);

            #if !defined(SRS_TERRAIN) && defined(_EMISSION) 
                #if defined(_COVERAGE_ON) && defined(SRS_SNOW_COVERAGE_SHADER)
                    emission *= lerp(1, 1-finalCoverageMask, _EmissionMasking);
                #endif
                mixedDiffuse.rgb += emission;
            #endif

            #if !defined(DEFERRED_PASS) && defined(_COVERAGE_ON) && defined(SRS_SNOW_COVERAGE_SHADER)
                #ifdef _SPARKLE_ON
                    #ifdef _SPARKLE_TEX_LS
                        half extraSparkleX = UNITY_SAMPLE_TEX2D_SAMPLER(_SparkleTex, _SRS_depth, i.worldPos.zy * _LocalSparkleTiling);
                        half extraSparkleY = UNITY_SAMPLE_TEX2D_SAMPLER(_SparkleTex, _SRS_depth, i.worldPos.xz * _LocalSparkleTiling);
                        half extraSparkleZ = UNITY_SAMPLE_TEX2D_SAMPLER(_SparkleTex, _SRS_depth, i.worldPos.xy * _LocalSparkleTiling);
                        half localSparkleTex = saturate(extraSparkleY * worldMasks.y + extraSparkleX * worldMasks.x + extraSparkleZ * worldMasks.z);
                    #else
                        half localSparkleTex = mixedDiffuse.a;
                    #endif
                    float sparkleDistMask = 1 - saturate((i.eyeDepth -_ProjectionParams.y) / _SparkleDistFalloff);

                    half sparkleMask = CalcSparkle(i, localSparkleTex, mixedDiffuse.a, attenuation, 
                                    sparkleDistMask, _ScreenSpaceSparklesTiling, _SparklesAmount, _SparklesBrightness, 
                                    _SparklesHighlightMaskExpansion);
                    mixedDiffuse.xyz += half4(lightColor, 1) * sparkleMask * finalCoverageMask;
                #endif
                #if defined(_SSS_ON)
                    float sssDistFade = saturate((i.eyeDepth -_ProjectionParams.y) / 50);
                    float sssMask = SSS(i, finalCoverageMask, attenuation, sssDistFade, _SSS_intensity);
                    mixedDiffuse.xyz += float4(lightColor, 1) * sssMask;
                #endif
            #endif
            //lighting end
        #endif //!defined SRS_TERRAIN_BAKE_SHADER && !defined(SRS_RAIN_TERRAIN_BAKE_SHADER)

        #if !defined(DEFERRED_PASS)
            color = mixedDiffuse;
            #ifdef SRS_TERRAIN
                color *= weight;
            #endif
            #ifdef TERRAIN_SPLAT_ADDPASS
                UNITY_APPLY_FOG_COLOR(i.fogCoord, color, fixed4(0,0,0,0));
            #else
                UNITY_APPLY_FOG(i.fogCoord, color);
            #endif
        #endif
        
        #if defined(DEFERRED_PASS)
            UNITY_INITIALIZE_OUTPUT(FragmentOutput, output);
            #if !defined(UNITY_HDR_ON)
                mixedDiffuse.rgb = exp2(-mixedDiffuse.rgb);
		    #endif

            #if !defined(SRS_TERRAIN) && defined(_EMISSION)
                #if defined(_COVERAGE_ON) && defined(SRS_SNOW_COVERAGE_SHADER)
                    emission *= lerp(1, 1-finalCoverageMask, _EmissionMasking);
                #endif
                color.rgb += emission;
            #endif

        	output.gBuffer0.rgb = color.rgb;
        	output.gBuffer0.a = ao;
        	output.gBuffer1.rgb = specularTint;
        	output.gBuffer1.a = mixedDiffuse.a;

            //pack the 1-snowMask into the spare A channel of the gBuffer2 to use it later in the deferred shading
            //one minus used as a workaround to prevent the snow effects appear on the standard and other shaders.
            #if !defined(_SPARKLE_ON) && !defined(_SSS_ON)
                finalCoverageMask = 0;
            #endif
            #if defined(SRS_SNOW_COVERAGE_SHADER)
        	    output.gBuffer2 = float4(i.worldNormal * 0.5 + 0.5, 1 - finalCoverageMask);
            #else
                output.gBuffer2 = float4(i.worldNormal * 0.5 + 0.5, 1);
            #endif
            
            #ifdef SRS_TERRAIN
                //multiply by weight to fix sharp edges between terrain passes                                                                     
        	    output.gBuffer3 = mixedDiffuse * weight;
            #else
                output.gBuffer3 = mixedDiffuse;
            #endif

            #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
                float2 shadowUV = 0;
			        #if defined(LIGHTMAP_ON)
			        	shadowUV = i.lightmapUV;
			        #endif
			    output.gBuffer4 = UnityGetRawBakedOcclusions(shadowUV, i.worldPos.xyz);
		    #endif
            #ifdef SRS_TERRAIN 
                UnityStandardDataApplyWeightToGbuffer(output.gBuffer0, output.gBuffer1, output.gBuffer2, weight); //fix final buffers using weight
            #endif
        #else
            #if defined (SRS_TERRAIN_BAKE_SHADER)
                SelectMap(color, i.worldNormal, geomN, finalCoverageMask, color.rgb);
            #endif
        	output.color = float4(color.rgb, alpha);
        #endif
    #elif defined(SHADOW_CASTER_PASS) && !defined(_REPLACEMENT)
        #if !defined(SRS_TERRAIN)
            #if defined(_RENDERING_CUTOUT) || SHADOWS_SEMITRANSPARENT
                float alpha = UNITY_ACCESS_INSTANCED_PROP(InstancedProps, _Color).a * UNITY_SAMPLE_TEX2D(_MainTex, i.uv.xy).a;
                clip(alpha - _Cutoff);
            #endif
        #elif defined(_ALPHATEST_ON)
            ClipHoles(i.uv.xy);
        #endif        
    #else
        output.color = float4(0.5, 0.8, 0.9, 1);
    #endif
    return output;
}

#endif