#ifndef SRS_SNOW_COVERAGE
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
#pragma exclude_renderers gles
#define SRS_SNOW_COVERAGE

#pragma shader_feature_local _PAINTABLE_COVERAGE_ON
#pragma shader_feature_local _TRACES_ON
#ifdef _TRACES_ON
    #pragma shader_feature_local _TRACE_DETAIL
#endif
#pragma shader_feature_local _DISPLACEMENT_ON
#pragma shader_feature_local _SSS_ON
#pragma shader_feature_local _SPARKLE_ON
#ifdef _SPARKLE_ON
    #pragma shader_feature_local _SPARKLE_TEX_SS
    #pragma shader_feature_local _SPARKLE_TEX_LS
#endif

#if defined(SRS_TERRAIN)
    #pragma shader_feature_local _STOCHASTIC_ON
#endif

sampler2D _CoverageTex0;
//UNITY_DECLARE_TEX2D(_CoverageTex0);

#if defined(_SPARKLE_TEX_LS) || (_SPARKLE_TEX_SS)
    UNITY_DECLARE_TEX2D_NOSAMPLER(_SparkleTex);
#endif

UNITY_DECLARE_TEX2D(_SRS_depth);
UNITY_DECLARE_TEX2D(_SRS_TraceTex);
UNITY_DECLARE_TEX2D(_TraceDetailTex);

SamplerState sampler_linear_repeat;
SamplerState sampler_linear_clamp;
SamplerState vertex_linear_clamp_sampler;

uniform float _CovMasks0_triBlendContrast;
uniform float4x4 _SRS_TraceSurfCamMatrix;
uniform float _TracesNormalScale;
uniform float _TraceDetailTiling;
uniform float _TraceDetailNormalScale;
uniform float _TraceDetailIntensity;
uniform float _TracesBlendFactor;
uniform float2 _TracesColorBlendRange;
uniform float3 _TracesColor;
uniform float _SSS_intensityOverride;
uniform float _CoverageDisplacement;
uniform float2 _PrecipitationDirRange;
uniform float3 _depthCamDir;
uniform float _PrecipitationDirOffset;
uniform float _CoverageAmount;
uniform float _CoverageTiling;
uniform float _CoverageDisplacementOffset;
uniform float _CoverageAreaMaskRange;
uniform float4x4 _depthCamMatrix;
uniform float _CoverageAreaBias;

uniform float _HeightMap0Contrast;
uniform float _DistanceFadeFalloff;
uniform float _DistanceFadeStart;
uniform float _MicroReliefFadeDistance;
uniform float _CoverageMicroRelief;
uniform float _CoverageNormalScale0;
uniform float _BaseCoverageNormalsBlend;
uniform float _CoverageNormalsOverlay;
uniform float _BlendByNormalsPower;
uniform float _CoverageAreaFalloffHardness;
uniform float _BlendByNormalsStrength;
uniform float _SSS_intensity;
uniform float _SparklesAmount;
uniform float _SparkleDistFalloff;
uniform float _LocalSparkleTiling;
uniform float _ScreenSpaceSparklesTiling;
uniform float _SparklesBrightness;
uniform float _SparkleBrightnessRT;
uniform float _SparklesLightmapMaskPower;
uniform float _SparklesHighlightMaskExpansion;
//uniform float _SnowAOIntensity;
uniform float _CoverageSmoothnessContrast;
uniform float4 _CoverageColor;
uniform float _TessEdgeL;
uniform float _TessMaxDisp;

// Reoriented Normal Mapping
float3 WorldToTangentNormalVector(v2f IN, float3 normal) {
    float3 t2w0 = CalcWorldNormalVector(IN, float3(1,0,0));
    float3 t2w1 = CalcWorldNormalVector(IN, float3(0,1,0));
    float3 t2w2 = CalcWorldNormalVector(IN, float3(0,0,1));
    float3x3 t2w = float3x3(t2w0, t2w1, t2w2);
    return mul(t2w, normal);
}

float BrightnessContrast(float value, float brightness, float contrast)
{
    return value = saturate((value - 0.5) * max(contrast, 0.0) + 0.5 + brightness); //adjust mask brightness/contrast
}

#if !defined(SHADOW_CASTER_PASS) && !defined (_REPLACEMENT)
void SnowCoverage(inout v2f IN, inout fixed4 mixedDiffuse, inout float alpha, fixed3 baseNormal, inout float metallic, inout float ao, inout float3 worldMasks, inout fixed finalSnowMask, inout float snowHeight, in float2 splatUV, in float3 geomNormal, inout float3 debug)
{
    float3 approximateSurfHeight = (CalcWorldNormalVector(IN, baseNormal));

    //sample VSM (depth coverage mask)
	float4 relativeSurfPos = mul(_depthCamMatrix, float4(IN.worldPos, 1.0));
	float2 srsDepthUV = (0.5 + ( 0.5 * relativeSurfPos)).xy;
	srsDepthUV.y = 1 - srsDepthUV.y;
	float2 moments = UNITY_SAMPLE_TEX2D(_SRS_depth, srsDepthUV).xy;
	float basicAreaMask = ComputeBasicAreaMask(moments, relativeSurfPos.z, _CoverageAreaBias, _CoverageAreaMaskRange, _CoverageLeakReduction); //basic occluded area mask
    
    float3 absGeomNormal = abs(geomNormal);
	worldMasks = saturate(absGeomNormal - (0.3).xxx);

    //worldMasks = saturate(pow(geomNormal, 4));
    //worldMasks /= max(dot(worldMasks, half3(1,1,1)), 0.0001);

    #if defined (SRS_TERRAIN) && !defined(SRS_TERRAIN_BAKE_SHADER)
        float distanceFade = saturate((IN.eyeDepth -_ProjectionParams.y - _DistanceFadeStart) / _DistanceFadeFalloff);
    #else
        float distanceFade = 0;
    #endif

    float4 addMask = float4(0, 0, 0, 0);
	float4 eraseMask = float4(1, 1, 1, 1);

	#ifdef _PAINTABLE_COVERAGE_ON
        #ifdef SRS_TERRAIN
            float2 covSplatUV = (IN.uv.xy * (_PaintedMask_TexelSize.zw - 1.0f) + 0.5f) * _PaintedMask_TexelSize.xy;
            float4 paintedMask = UNITY_SAMPLE_TEX2D_SAMPLER(_PaintedMask, _SRS_depth, covSplatUV); // reuse splatUV
            
            half3 paintedMaskNormal = UNITY_SAMPLE_TEX2D_SAMPLER(_PaintedMaskNormal, _SRS_depth, covSplatUV) * 2 - 1;
            paintedMaskNormal = paintedMaskNormal.xzy;

		    PaintMaskRGBA(paintedMask, addMask, eraseMask);
        #else
            PaintMaskRGBA(IN.color, addMask, eraseMask);
        #endif
	#endif

    float covAmount = (eraseMask.r * _CoverageAmount);
	float covAmountFinal = (1.0 - (covAmount * 2.0));

    #ifdef SRS_TERRAIN
        //Use simple world space or stochastic sampling for terrains
        float2 covMasks0_uv = IN.worldPos.xz * _CoverageTiling.xx;
        
        #ifdef _STOCHASTIC_ON
            float4 coverageMasks0 = StochasticTex2D(_CoverageTex0, covMasks0_uv);
        #else
            float4 coverageMasks0 = tex2D(_CoverageTex0, covMasks0_uv);
        #endif

        float2 heightAndSmoothness = coverageMasks0.zw;

        half3 snowNormals = ConstructNormal(coverageMasks0.rg, _CoverageNormalScale0);
        snowNormals = BlendReorientedNormal(half3(geomNormal.xz, absGeomNormal.y), snowNormals);
        snowNormals = snowNormals.xzy;
    #else
        //triplanar sampling for regular meshes
        /*
        fixed dither = Dither(IN.sPos);
        float4 covTexSampled = SingleSampleTriplanar(_CoverageTex0, IN.worldPos, geomNormal, _CovMasks0_triBlendContrast, _CoverageTiling, dither);
        float2 heightAndSmoothness = covTexSampled.zw;
        */

        float2 uvX = IN.worldPos.zy * _CoverageTiling.xx;
        float2 uvY = IN.worldPos.xz * _CoverageTiling.xx;
        float2 uvZ = IN.worldPos.xy * _CoverageTiling.xx;

        float4 coverageMasks0X = tex2D(_CoverageTex0, uvX);
	    float4 coverageMasks0Y = tex2D(_CoverageTex0, uvY);
	    float4 coverageMasks0Z = tex2D(_CoverageTex0, uvZ);

        float2 heightAndSmoothness = saturate(coverageMasks0Y.zw * worldMasks.y + coverageMasks0X.zw * worldMasks.x + coverageMasks0Z.zw * worldMasks.z);

        half3 snowNormalX = ConstructNormal(coverageMasks0X.rg, _CoverageNormalScale0);
        half3 snowNormalY = ConstructNormal(coverageMasks0Y.rg, _CoverageNormalScale0);
        half3 snowNormalZ = ConstructNormal(coverageMasks0Z.rg, _CoverageNormalScale0);
    
	    //swizzle world normals to match tangent space and apply reoriented normal mapping blend
        snowNormalX = BlendReorientedNormal(half3(geomNormal.zy, absGeomNormal.x), snowNormalX);
        snowNormalY = BlendReorientedNormal(half3(geomNormal.xz, absGeomNormal.y), snowNormalY);
        snowNormalZ = BlendReorientedNormal(half3(geomNormal.xy, absGeomNormal.z), snowNormalZ);
    
        //prevent return value of 0
        half3 axisSign = geomNormal < 0 ? -1 : 1;
    
        // apply world space sign to tangent space Z
        snowNormalX.z *= axisSign.x;
        snowNormalY.z *= axisSign.y;
        snowNormalZ.z *= axisSign.z;
    
        //swizzle tangent normals to match world normal and blend together
        half3 snowNormals = normalize(
        snowNormalX.zyx * worldMasks.x +
        snowNormalY.xzy * worldMasks.y +
        snowNormalZ.xyz * worldMasks.z
        );
    #endif
	
	//float snowHeightMix = saturate(smoothstep(covAmountFinal, 1.0, heightAndSmoothness.x));
    float snowHeightMix = lerp(0, saturate(heightAndSmoothness.x + heightAndSmoothness.x + 0.2), covAmount);
    snowHeight = heightAndSmoothness.x;

	float dirMask = DirMask(_PrecipitationDirRange, _depthCamDir, geomNormal, _PrecipitationDirOffset); //precipitation direction mask
	float volumeMask = CalcVolumeMask(_CoverageAreaFalloffHardness, srsDepthUV, relativeSurfPos.z); //Weatherade volume mask
    float outsideAreaMaskVal = saturate(smoothstep(0, _CoverageAreaMaskRange * 10, 5) * 2);
    //float snowMask = lerp(snowHeightMix * outsideAreaMaskVal, basicAreaMask * snowHeightMix, volumeMask) * dirMask;
    float transitionFactor = 2;
    float heightCalc = saturate(heightAndSmoothness.x * transitionFactor - (transitionFactor * 0.5));
    float transitionGradient = smoothstep(heightCalc, 1, pow(basicAreaMask, 0.5)) * covAmount;
    float snowMask = lerp(snowHeightMix * outsideAreaMaskVal, transitionGradient, volumeMask) * dirMask;
	/*
    #if defined(SRS_TERRAIN)
        snowMask = saturate(snowMask + smoothstep(0, 0.2, addMask.r));
    #else
        snowMask = saturate(snowMask + addMask.r);
    #endif
    */
    snowMask = saturate(snowMask + addMask.r);
    snowMask = HeightBlendTriplanar(snowMask, approximateSurfHeight, worldMasks, _BlendByNormalsPower, _BlendByNormalsStrength);

	float3 covOverlayNormals = lerp(IN.worldNormal, snowNormals, _CoverageNormalsOverlay);

    #if defined(_DISPLACEMENT_ON)
        float dirMulSnowHeight = snowHeightMix * pow(saturate(dirMask * 2), 10);
        float basicMask = basicAreaMask * covAmount + addMask.r;
        float basicMaskRemaped = smoothstep(_CoverageDisplacementOffset * 0.8, _CoverageDisplacementOffset, basicMask);

        float finalDispMask = lerp(dirMulSnowHeight, basicMaskRemaped, dirMask);
        finalDispMask = saturate(lerp(1, finalDispMask, volumeMask * saturate(_CoverageDisplacement * _CoverageDisplacement * 100)));
        half3 overrideNormalsByDisp = lerp(covOverlayNormals, snowNormals, finalDispMask);
        overrideNormalsByDisp = lerp(IN.worldNormal, overrideNormalsByDisp, snowMask);
        half3 finalNormals = overrideNormalsByDisp;
    #else
        half3 finalNormals = lerp(IN.worldNormal, covOverlayNormals, snowMask);
    #endif

    float3 finalColor = _CoverageColor.rgb;

    #ifdef _TRACES_ON
        relativeSurfPos = mul(_SRS_TraceSurfCamMatrix, float4(IN.worldPos, 1));
    	float2 traceMaskUV = (0.5 + ( 0.5 * relativeSurfPos)).xy;
    	traceMaskUV.y = 1 - traceMaskUV.y;
        
        float4 sampledTraces = UNITY_SAMPLE_TEX2D(_SRS_TraceTex, traceMaskUV);

        half3 traceNormals = ConstructNormal(sampledTraces.rg, _TracesNormalScale);

        //Blend t-space trace normals with the surface world normals
        traceNormals = BlendReorientedNormal(half3(finalNormals.xz, finalNormals.y), traceNormals);
        traceNormals = traceNormals.xzy;

        float traceMask = saturate(sampledTraces.z * 5);
        float3 traceDetailSampled = float3(0, 0, 1);
        #ifdef _TRACE_DETAIL
            traceDetailSampled = UNITY_SAMPLE_TEX2D(_TraceDetailTex, IN.worldPos.xz * _TraceDetailTiling);
            float3 traceDetailNormals = ConstructNormal(traceDetailSampled.rg, _TraceDetailNormalScale);
            traceDetailNormals = lerp(float3(0, 0, 1), traceDetailNormals, lerp(0, saturate(sampledTraces.a + 0.5), traceMask));
            traceNormals = BlendReorientedNormal(half3(traceNormals.xz, traceNormals.y), traceDetailNormals);
            traceNormals = traceNormals.xzy;
        #endif

        finalNormals = lerp(finalNormals, traceNormals, snowMask * pow(dirMask, 10));
        float2 blendRange = _TracesColorBlendRange * 0.5 + 0.5;

        finalColor = lerp(finalColor, finalColor * _TracesColor, smoothstep(blendRange.x, blendRange.y, 1 - sampledTraces.w) * _TracesBlendFactor * traceDetailSampled.z);
    #endif

    #if defined(_PAINTABLE_COVERAGE_ON) && defined(SRS_TERRAIN)
        half3 blendWithPaintedNormals = BlendReorientedNormal(half3(finalNormals.xz, finalNormals.y), paintedMaskNormal);
        blendWithPaintedNormals = blendWithPaintedNormals.xzy;
        finalNormals = lerp(finalNormals, blendWithPaintedNormals, snowMask);
    #endif

    float snowSmoothness = heightAndSmoothness.y;
    float4 snowColor = float4(finalColor, snowSmoothness);
	float4 blendAlbedo = lerp(mixedDiffuse, snowColor, snowMask);

    #if defined(SRS_TERRAIN)
		half3 distantNormal = UNITY_SAMPLE_TEX2D_SAMPLER(_NormalLOD, _SRS_depth, splatUV);
		distantNormal = distantNormal * 2 - 1;
		distantNormal = distantNormal.xzy;
		IN.worldNormal = lerp(finalNormals, distantNormal, distanceFade);

		mixedDiffuse = lerp(blendAlbedo, UNITY_SAMPLE_TEX2D_SAMPLER(_AlbedoLOD, _SRS_depth, splatUV) , distanceFade);
	#else
		IN.worldNormal = finalNormals;
		mixedDiffuse = blendAlbedo;
	#endif

    #if defined(FORWARD_BASE_PASS) && defined(_DISPLACEMENT_ON)
        snowMask *= finalDispMask;
    #endif

    finalSnowMask = snowMask;

    metallic = lerp(metallic, 0, finalSnowMask);
    ao = lerp(ao, 1, finalSnowMask);
    
    alpha = lerp(saturate(finalSnowMask + alpha), alpha, _MaskCoverageByAlpha);
}
#endif

void Displace(inout VertexData v, float3 worldNormal)
{
#ifdef _DISPLACEMENT_ON
    float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
	float3 worldMasks = saturate(abs(worldNormal) - (0.3).xxx);

    float4 addMask = float4(0, 0, 0, 0);
	float4 eraseMask = float4(1, 1, 1, 1);

	#ifdef _PAINTABLE_COVERAGE_ON
        #ifdef SRS_TERRAIN
            float2 covSplatUV = (v.uv.xy * (_PaintedMask_TexelSize.zw - 1.0f) + 0.5f) * _PaintedMask_TexelSize.xy;
		    float4 paintedMask = SAMPLE_TEXTURE2D_LOD(_PaintedMask, /* sampler_SRS_depth*/ sampler_linear_clamp, covSplatUV, 0.0); //TODO: add macros to support sampling on all platforms
		    PaintMaskRGBA(paintedMask, addMask, eraseMask);
        #else
            PaintMaskRGBA(v.color, addMask, eraseMask);
        #endif
	#endif

    float covAmount = (eraseMask.r * _CoverageAmount);
    float covAmountFinal = (1.0 - (covAmount * 2.0));

    //sample VSM (depth coverage mask)
	float4 relativeSurfPos = mul(_depthCamMatrix, float4(worldPos, 1));
	//float mean = exp(((((relativeSurfPos.z * -1.0) * 2.0) - 1.0) * 5.0));
	float2 srsDepthUV = (0.5 + ( 0.5 * relativeSurfPos)).xy;
	srsDepthUV.y = 1 - srsDepthUV.y;

    float2 moments = SAMPLE_TEXTURE2D_LOD(_SRS_depth, /* sampler_SRS_depth */ sampler_linear_clamp, srsDepthUV, 0.0).xy; //TODO: add macros to support sampling on all platforms
    float basicAreaMask = ComputeBasicAreaMask(moments, relativeSurfPos.z, _CoverageAreaBias, _CoverageAreaMaskRange, _CoverageLeakReduction); //basic occluded area mask
    
    #if !defined(SRS_TERRAIN)
        basicAreaMask = saturate(basicAreaMask  + addMask.r);
    #endif

    #ifdef SRS_TERRAIN
        //Use simple world space or stochastic sampling for terrains
        float2 covMasks0_uv = worldPos.xz * _CoverageTiling.xx;
        #ifdef _STOCHASTIC_ON
            float snowHeightMix = StochasticTex2DLod(_CoverageTex0, covMasks0_uv, 4).z; //use downsampled height map to prevent high frequency noise
        #else
            float snowHeightMix = tex2Dlod(_CoverageTex0, float4(covMasks0_uv, 0, 4)).z; //use downsampled height map to prevent high frequency noise
        #endif
    #else
        //calculate triplanar uvs
        float2 uvX = worldPos.zy * _CoverageTiling.xx;
        float2 uvY = worldPos.xz * _CoverageTiling.xx;
        float2 uvZ = worldPos.xy * _CoverageTiling.xx;

        //use downsampled height map to prevent high frequency noise
        float coverageMasks0X = tex2Dlod(_CoverageTex0, float4(uvX, 0, 4)).z;
	    float coverageMasks0Y = tex2Dlod(_CoverageTex0, float4(uvY, 0, 4)).z;
	    float coverageMasks0Z = tex2Dlod(_CoverageTex0, float4(uvZ, 0, 4)).z;
        float snowHeightMix = saturate(coverageMasks0Y * worldMasks.y + coverageMasks0X * worldMasks.x + coverageMasks0Z * worldMasks.z);
    #endif
	
    //snowHeightMix = saturate(smoothstep(-covAmount, 1.0, snowHeightMix));
    float dirMask = DirMask(_PrecipitationDirRange, _depthCamDir, worldNormal, _PrecipitationDirOffset); //precipitation direction mask
    snowHeightMix = BrightnessContrast(snowHeightMix, 0, _HeightMap0Contrast); 
    //snowHeightMix *= pow(dirMask, 10);
    snowHeightMix *= dirMask * eraseMask.r;

    float heightInsideArea = snowHeightMix * basicAreaMask;
    float heightOutsideArea = snowHeightMix;
    float volumeMask = CalcVolumeMask(_CoverageAreaFalloffHardness, srsDepthUV, relativeSurfPos.z); //Weatherade volume mask

    float insideOutsideComposed = lerp(heightOutsideArea, heightInsideArea, volumeMask);

    insideOutsideComposed = smoothstep(_CoverageDisplacementOffset, 1, insideOutsideComposed);
    
    float covAmountBiased = saturate(_CoverageAmount - 0.5);

    #if defined(SRS_TERRAIN)
        float disp = lerp(0, insideOutsideComposed + smoothstep(0, 2, saturate(addMask.r - 0.3)), covAmountBiased);
    #else
        float disp = lerp(0, insideOutsideComposed, covAmountBiased);
    #endif

    #ifdef _TRACES_ON
        relativeSurfPos = mul(_SRS_TraceSurfCamMatrix, float4(worldPos, 1));
    	float2 traceMaskUV = (0.5 + ( 0.5 * relativeSurfPos)).xy;
    	traceMaskUV.y = 1 - traceMaskUV.y;

        float2 traceMaskSampled = SAMPLE_TEXTURE2D_LOD(_SRS_TraceTex, /* sampler_SRS_TraceTex */ sampler_linear_repeat, traceMaskUV, 0.0).zw; //TODO: add macros to support sampling on all platforms
        float traceDispMask = traceMaskSampled.y;
        
        #ifdef _TRACE_DETAIL
            float traceDetailMask = saturate(traceMaskSampled.x * 2);
            float traceHeightDetails = SAMPLE_TEXTURE2D_LOD(_TraceDetailTex, /* sampler_TraceDetailTex */ sampler_linear_repeat, v.uv.xy * _TraceDetailTiling, 0.0).z; //TODO: add macros to support sampling on all platforms
            traceDispMask *= lerp(1, traceHeightDetails, traceDetailMask * _TraceDetailIntensity);
        #endif

        float finalDispMask = disp * traceDispMask * 4 * pow(dirMask, 10);
    #else
        float finalDispMask = disp * 2;
    #endif

    #if defined(_USE_AVERAGED_NORMALS)
        half3 dispNormal = v.unifiedNormal;
    #else
        half3 dispNormal = v.normal;
    #endif

    v.vertex.xyz += dispNormal * finalDispMask  * _CoverageDisplacement;
#endif //Displacement end
}

#endif