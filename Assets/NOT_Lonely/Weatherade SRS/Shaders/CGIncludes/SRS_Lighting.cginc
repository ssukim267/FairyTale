#ifndef SRS_LIGHTING_INCLUDED
#define SRS_LIGHTING_INCLUDED

#if defined(LIGHTMAP_ON) && defined(SHADOWS_SCREEN)
	#if defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK)
		#define SUBTRACTIVE_LIGHTING 1
	#endif
#endif

float FadeShadows (v2f i, float attenuation) {
    #if HANDLE_SHADOWS_BLENDING_IN_GI
	    float viewZ = dot(_WorldSpaceCameraPos - i.worldPos, UNITY_MATRIX_V[2].xyz);
	    float shadowFadeDistance = UnityComputeShadowFadeDistance(i.worldPos, viewZ);
	    float shadowFade = UnityComputeShadowFade(shadowFadeDistance);
        float bakedAttenuation = UnitySampleBakedOcclusion(i.lightmapUV, i.worldPos);
	    attenuation = UnityMixRealtimeAndBakedShadows(attenuation, bakedAttenuation, shadowFade);
    #endif
	return attenuation;
}

void ApplySubtractiveLighting (v2f i, inout UnityIndirect indirectLight) 
{
    #if SUBTRACTIVE_LIGHTING
		UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos);
		attenuation = FadeShadows(i, attenuation);
        float ndotl = saturate(dot(i.worldNormal, _WorldSpaceLightPos0.xyz));
        float3 shadowedLightEstimate = ndotl * (1 - attenuation) * _LightColor0.rgb;
        float3 subtractedLight = indirectLight.diffuse - shadowedLightEstimate;
        subtractedLight = max(subtractedLight, unity_ShadowColor.rgb);
        subtractedLight = lerp(subtractedLight, indirectLight.diffuse, _LightShadowData.x);
        indirectLight.diffuse = min(subtractedLight, indirectLight.diffuse);
	#endif
}


UnityIndirect CreateIndirectLight (v2f i, float3 viewDir, float smoothness, float ao = 1) {
	UnityIndirect indirectLight;
	indirectLight.diffuse = 0;
    indirectLight.specular = 0;
    #if defined(FORWARD_BASE_PASS) || defined(DEFERRED_PASS)

        #if defined(LIGHTMAP_ON)
			indirectLight.diffuse = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lightmapUV)) * ao;
            #if defined(DIRLIGHTMAP_COMBINED)
				float4 lightmapDirection = UNITY_SAMPLE_TEX2D_SAMPLER(unity_LightmapInd, unity_Lightmap, i.lightmapUV);
                indirectLight.diffuse = DecodeDirectionalLightmap(indirectLight.diffuse, lightmapDirection, i.worldNormal);
			#endif
            ApplySubtractiveLighting(i, indirectLight);
        #endif
        #if defined(DYNAMICLIGHTMAP_ON)
			float3 dynamicLightDiffuse = DecodeRealtimeLightmap(
				UNITY_SAMPLE_TEX2D(unity_DynamicLightmap, i.dynamicLightmapUV)
			);

			#if defined(DIRLIGHTMAP_COMBINED)
				float4 dynamicLightmapDirection = UNITY_SAMPLE_TEX2D_SAMPLER(
					unity_DynamicDirectionality, unity_DynamicLightmap,
					i.dynamicLightmapUV
				);
            	indirectLight.diffuse += DecodeDirectionalLightmap(
            		dynamicLightDiffuse, dynamicLightmapDirection, i.worldNormal
            	);
			#else
				indirectLight.diffuse += dynamicLightDiffuse;
			#endif
		#endif
        #if !defined(LIGHTMAP_ON) && !defined(DYNAMICLIGHTMAP_ON)
            #if UNITY_LIGHT_PROBE_PROXY_VOLUME
                if (unity_ProbeVolumeParams.x == 1) 
                {
                    indirectLight.diffuse = SHEvalLinearL0L1_SampleProbeVolume(float4(i.worldNormal, 1), i.worldPos);
					indirectLight.diffuse = max(0, indirectLight.diffuse);
                    #if defined(UNITY_COLORSPACE_GAMMA)
			            indirectLight.diffuse = LinearToGammaSpace(indirectLight.diffuse);
			        #endif
			    }
			    else 
                {
			    	indirectLight.diffuse += max(0, ShadeSH9(float4(i.worldNormal, 1)));
			    }
			#else
			    indirectLight.diffuse += max(0, ShadeSH9(float4(i.worldNormal, 1)));
            #endif
		#endif

        float3 reflectionDir = reflect(-viewDir, i.worldNormal);
        
        Unity_GlossyEnvironmentData envData;
		envData.roughness = 1 - smoothness;
		envData.reflUVW = BoxProjectedCubemapDirection(reflectionDir, i.worldPos, unity_SpecCube0_ProbePosition, unity_SpecCube0_BoxMin, unity_SpecCube0_BoxMax);
		float3 probe0 = Unity_GlossyEnvironment(UNITY_PASS_TEXCUBE(unity_SpecCube0), unity_SpecCube0_HDR, envData);
    
        #if UNITY_SPECCUBE_BLENDING
            float interpolator = unity_SpecCube0_BoxMin.w;
            UNITY_BRANCH
            if(interpolator < 0.99999){
                envData.reflUVW = BoxProjectedCubemapDirection(reflectionDir, i.worldPos, unity_SpecCube1_ProbePosition, unity_SpecCube1_BoxMin, unity_SpecCube1_BoxMax);
                float3 probe1 = Unity_GlossyEnvironment(UNITY_PASS_TEXCUBE_SAMPLER(unity_SpecCube1, unity_SpecCube0), unity_SpecCube1_HDR, envData);
                indirectLight.specular = lerp(probe1, probe0, interpolator) * ao;
            }
            else
            {
                indirectLight.specular = probe0 * ao;
            }
        #else
            indirectLight.specular = probe0 * ao;
        #endif
    #endif
    
	
	return indirectLight;
}

UnityLight CreateLight(v2f i, inout fixed atten)
{
    UnityLight light;
    atten = 1;

    #if defined(DEFERRED_PASS) || SUBTRACTIVE_LIGHTING
		light.dir = float3(0, 1, 0);
		light.color = 0;
	#else
        #if defined(POINT) || defined(POINT_COOKIE) || defined(DIRECTIONAL_COOKIE) || defined(SPOT) 
	        light.dir = normalize(_WorldSpaceLightPos0.xyz - i.worldPos);
        #else
            light.dir = _WorldSpaceLightPos0.xyz;
        #endif

        UNITY_LIGHT_ATTENUATION(attenuation, i, i.worldPos);
        attenuation = FadeShadows(i, attenuation);
        atten = attenuation;

        light.color = _LightColor0.rgb * attenuation;
    #endif
    return light;
}

float BrightnessAndContrast(float value, float brightness, float contrast)
{
    return value = saturate((value - 0.5) * max(contrast, 0.0) + 0.5 + brightness); //adjust mask brightness/contrast
}

#if defined(_SPARKLE_ON)
    half CalcSparkle(v2f IN, half sparkleTexMask, fixed sm, half shadowMask, float disFadeMask, 
                    float ssTiling, float amount, float brightness, float expansion)
    {
        float3 worldLightDir = normalize(UnityWorldSpaceLightDir(IN.worldPos));
        float3 viewDir = normalize(UnityWorldSpaceViewDir(IN.worldPos));
        half highlightMask = dot(normalize(worldLightDir + viewDir), IN.worldNormal);
        highlightMask = pow(max(highlightMask, 0), (1 - expansion) * 128);
    
        float2 ssUV = IN.ssUV;
    
        #if UNITY_SINGLE_PASS_STEREO
            float4 scaleOffset = unity_StereoScaleOffset[unity_StereoEyeIndex];
            ssUV = (ssUV - scaleOffset.zw) / scaleOffset.xy;
        #endif
    
        ssUV *= ssTiling;
    
    #ifdef _SPARKLE_TEX_SS
        half screenSpaceSparkle = UNITY_SAMPLE_TEX2D_SAMPLER(_SparkleTex, _SRS_depth, ssUV).r;
    #else
        half screenSpaceSparkle = tex2D(_CoverageTex0, ssUV).w;
    #endif
        half ampSm = pow(sm, 2) * _SparklesBrightness * 0.3;
    
        #if defined(LIGHTMAP_ON)
    	    #if defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK)
    	    	shadowMask = BrightnessAndContrast(shadowMask, 0.5, _SparklesLightmapMaskPower);
    	    #endif
        #endif  
    
        half mix = sparkleTexMask * screenSpaceSparkle;
        half b = highlightMask * shadowMask;
    
        half sparkleMask = step(1 - amount, mix) * brightness * b; //add highlight based sparkles
        sparkleMask += step(1 - amount * 0.9, mix) * brightness * shadowMask * 0.05; //add general sparkles
        sparkleMask += ampSm * b * amount * 1.5; //increase local space highlight 
        sparkleMask *= disFadeMask;
    
        return sparkleMask;
    }
#endif
#if defined(_SSS_ON)
    float SSS(v2f IN, float snowMask, float shadowMask, float distFadeMask, float intensity)    
    {
        #if defined(LIGHTMAP_ON)
    	    #if defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK)
    	    	shadowMask = BrightnessAndContrast(shadowMask, 0.5, 2);
    	    #endif
        #endif  
    
        float3 worldLightDir = UnityWorldSpaceLightDir(IN.worldPos) * 2;
        float3 viewDir = normalize(UnityWorldSpaceViewDir(IN.worldPos));
        float sssMask = saturate(dot(-normalize(IN.worldNormal + worldLightDir), viewDir)) * shadowMask;
        sssMask = pow(sssMask, 2) * intensity * (1 - distFadeMask);
        return sssMask * snowMask;
    }
#endif
#endif