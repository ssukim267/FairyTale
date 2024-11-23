#if !defined(SRS_DEFERRED_SHADING)
#define SRS_DEFERRED_SHADING

#pragma multi_compile SPARKLE_ON
#pragma multi_compile SSS_ON
#ifdef SPARKLE_ON
    #pragma multi_compile SPARKLE_TEX_LS
    #pragma multi_compile SPARKLE_TEX_SS
#endif

#if defined(SPARKLE_TEX_LS) || (SPARKLE_TEX_SS)
    UNITY_DECLARE_TEX2D(_SparkleTex);
#endif

#if !defined(SPARKLE_TEX_LS) || !(SPARKLE_TEX_SS)
    UNITY_DECLARE_TEX2D(_CoverageMasksTex);
#endif

uniform float _SparklesAmount;
uniform float _SparkleDistFalloff;
uniform float _LocalSparkleTiling;
uniform float _ScreenSpaceSparklesTiling;
uniform float _SparklesBrightness;
uniform float _SparklesLightmapMaskPower;
uniform float _SparklesHighlightMaskExpansion;
uniform float _SSS_intensity;

float GetShadowMaskAttenuation (float2 uv) {
	float attenuation = 1;
	#if defined (SHADOWS_SHADOWMASK)
		float4 mask = tex2D(_CameraGBufferTexture4, uv);
		attenuation = saturate(dot(mask, unity_OcclusionMaskSelector));
	#endif
	return attenuation;
}

UnityLight CreateLight (float2 uv, float3 worldPos, float viewZ, inout float atten) {
	UnityLight light;
	light.dir = -_LightDir;
    float attenuation = 1;
	float shadowAttenuation = 1;
    bool shadowed = false;
    #if defined(DIRECTIONAL) || defined(DIRECTIONAL_COOKIE)
		light.dir = -_LightDir;
        #if defined(DIRECTIONAL_COOKIE)
	    	float2 uvCookie = mul(unity_WorldToLight, float4(worldPos, 1)).xy;
	    	attenuation *= tex2Dbias(_LightTexture0, float4(uvCookie, 0, -8)).w;
	    #endif

	    #if defined(SHADOWS_SCREEN)
            shadowed = true;
	    	shadowAttenuation = tex2D(_ShadowMapTexture, uv).r;
	    #endif
    #else
        float3 lightVec = _LightPos.xyz - worldPos;
		light.dir = normalize(lightVec);
        attenuation *= tex2D(_LightTextureB0, (dot(lightVec, lightVec) * _LightPos.w).rr).UNITY_ATTEN_CHANNEL;
        #if defined(SPOT)
            float4 uvCookie = mul(unity_WorldToLight, float4(worldPos, 1));
		    uvCookie.xy /= uvCookie.w;
		    attenuation *= tex2Dbias(_LightTexture0, float4(uvCookie.xy, 0, -8)).w;

            attenuation *= uvCookie.w < 0;
            #if defined(SHADOWS_DEPTH)
		    	shadowed = true;
                shadowAttenuation = UnitySampleShadowmap(mul(unity_WorldToShadow[0], float4(worldPos, 1)));
		    #endif
        #else
            #if defined(POINT_COOKIE)
				float3 uvCookie = mul(unity_WorldToLight, float4(worldPos, 1)).xyz;
				attenuation *= texCUBEbias(_LightTexture0, float4(uvCookie, -8)).w;
			#endif
            #if defined(SHADOWS_CUBE)
				shadowed = true;
				shadowAttenuation = UnitySampleShadowmap(-lightVec);
			#endif
        #endif
	#endif

    #if defined(SHADOWS_SHADOWMASK)
		shadowed = true;
	#endif

    if (shadowed) {
		float shadowFadeDistance = UnityComputeShadowFadeDistance(worldPos, viewZ);
		float shadowFade = UnityComputeShadowFade(shadowFadeDistance);
        
        shadowAttenuation = UnityMixRealtimeAndBakedShadows(
			shadowAttenuation, GetShadowMaskAttenuation(uv), shadowFade
		);
        
        #if defined(UNITY_FAST_COHERENT_DYNAMIC_BRANCHING) && defined(SHADOWS_SOFT)
			#if !defined(SHADOWS_SHADOWMASK)
                UNITY_BRANCH
			    if (shadowFade > 0.99) 
                {
			    	shadowAttenuation = 1;
			    }
            #endif
		#endif
	}

    attenuation *= shadowAttenuation;
    atten = attenuation;
	light.color = _LightColor.rgb * attenuation;
	return light;
}

float BrightnessAndContrast(float value, float brightness, float contrast)
{
    return value = saturate((value - 0.5) * max(contrast, 0.0) + 0.5 + brightness); //adjust mask brightness/contrast
}

half CalcSparkle(float3 viewDir, float3 worldLightDir, fixed3 worldNormal, float2 ssUV, half sparkleTexLS, fixed sm, half attenuation, float disFadeMask, 
                float amount, float brightness, float expansion)
{
    half highlightMask = dot(normalize(worldLightDir + viewDir), worldNormal);
    highlightMask = pow(max(highlightMask, 0), (1 - expansion) * 128);

    ssUV.x = ssUV.x * (_ScreenParams.x / _ScreenParams.y);
/*
    #if defined(LIGHTMAP_ON)
	    #if defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK)
	    	attenuation = BrightnessAndContrast(attenuation, 0.5, _SparklesLightmapMaskPower);
	    #endif
    #endif 
    */

    #ifdef SPARKLE_TEX_SS
        half sparkleTexSS = UNITY_SAMPLE_TEX2D(_SparkleTex, ssUV).r;
    #else
        half sparkleTexSS = UNITY_SAMPLE_TEX2D(_CoverageMasksTex, ssUV).w;
    #endif

    half ampSm = pow(sm, 2) * brightness * 0.3;

    half mix = sparkleTexLS * sparkleTexSS; 
    half b = highlightMask * attenuation;
    
    half sparkleMask = step(1 - amount, mix) * brightness * b; //add highlight based sparkles
    sparkleMask += step(1 - amount * 0.9, mix) * brightness * attenuation * 0.05; //add general sparkles
    sparkleMask += ampSm * b * amount * 1.5; //increase local space highlight 
    return clamp(sparkleMask, 0, 100) * disFadeMask;
}

float SSS(float3 viewDir, float3 worldLightDir, fixed3 worldNormal, float snowMask, float attenuation, float distFadeMask, float intensity)
{
/*
    #if defined(LIGHTMAP_ON)
	    #if defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK)
	    	attenuation = BrightnessAndContrast(attenuation, 0.5, 2);
	    #endif
    #endif 
    */

    worldLightDir *= 2;
    float sssMask = saturate(dot(-normalize(worldNormal + worldLightDir), viewDir)) * attenuation;
    sssMask = pow(sssMask, 2) * intensity * (1 - distFadeMask);
    return sssMask * snowMask;
}

float4 FragmentProgram (Interpolators i) : SV_Target {
    float2 uv = i.uv.xy / i.uv.w;

    float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
	depth = Linear01Depth(depth);

    float3 rayToFarPlane = i.ray * _ProjectionParams.z / i.ray.z;
    float3 viewPos = rayToFarPlane * depth;
    float3 worldPos = mul(unity_CameraToWorld, float4(viewPos, 1)).xyz;

    float3 viewDir = normalize(_WorldSpaceCameraPos - worldPos);

    float3 albedo = tex2D(_CameraGBufferTexture0, uv).rgb;
	float3 specularTint = tex2D(_CameraGBufferTexture1, uv).rgb;
	float smoothness = tex2D(_CameraGBufferTexture1, uv).a;
	float4 gBuffer2 = tex2D(_CameraGBufferTexture2, uv);
    float3 normal = gBuffer2.rgb * 2 - 1;
    half finalSnowMask = 1 - gBuffer2.a; // unpack the snow mask as 1-mask
    float oneMinusReflectivity = 1 - SpecularStrength(specularTint);

    float attenuation;
    UnityLight light = CreateLight(uv, worldPos, viewPos.z, attenuation);

	UnityIndirect indirectLight;
	indirectLight.diffuse = 0;
	indirectLight.specular = 0;

    float colorEnhance = pow(smoothness, 3) * finalSnowMask * attenuation * 4;
    albedo.xyz += colorEnhance;

    oneMinusReflectivity *= attenuation;

	float4 color = UNITY_BRDF_PBS(
    	albedo, specularTint, oneMinusReflectivity, smoothness,
    	normal, viewDir, light, indirectLight
    );

    float3 absGeomNormal = abs(normal);
	float3 worldMasks = saturate(absGeomNormal - (0.3).xxx);

    //color.xyz += float4(1, 1,1 , 1);

    #if defined(SPARKLE_ON)
        #ifdef SPARKLE_TEX_LS
            half extraSparkleX = UNITY_SAMPLE_TEX2D(_SparkleTex, worldPos.zy * _LocalSparkleTiling);
            half extraSparkleY = UNITY_SAMPLE_TEX2D(_SparkleTex, worldPos.xz * _LocalSparkleTiling);
            half extraSparkleZ = UNITY_SAMPLE_TEX2D(_SparkleTex, worldPos.xy * _LocalSparkleTiling);
            half localSparkleTex = saturate(extraSparkleY * worldMasks.y + extraSparkleX * worldMasks.x + extraSparkleZ * worldMasks.z);
        #else
            half localSparkleTex = smoothness;
        #endif

        float sparkleDistMask = 1 - saturate((viewPos.z -_ProjectionParams.y) / _SparkleDistFalloff);
        
        half sparkleMask = CalcSparkle(viewDir, light.dir, normal, uv * _ScreenSpaceSparklesTiling, localSparkleTex, smoothness, attenuation, 
                        sparkleDistMask, _SparklesAmount, _SparklesBrightness, 
                        _SparklesHighlightMaskExpansion);

        color.xyz += fixed4(light.color, 1) * sparkleMask * finalSnowMask;     
    #endif

    #if defined(SSS_ON)
        float sssDistFade = saturate((viewPos.z -_ProjectionParams.y) / 50);
        float sssMask = SSS(viewDir, light.dir, normal, finalSnowMask, attenuation, sssDistFade, _SSS_intensity);
        color.xyz += float4(light.color, 1) * sssMask;
    #endif

    #if !defined(UNITY_HDR_ON)
		color = exp2(-color);
	#endif

	return color;
}
#endif