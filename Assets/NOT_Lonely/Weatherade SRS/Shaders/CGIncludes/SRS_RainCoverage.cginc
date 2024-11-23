
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
#pragma exclude_renderers gles
//rain shader variables
#pragma editor_sync_compilation
UNITY_DECLARE_TEX2D(_SRS_depth);
UNITY_DECLARE_TEX2D(_PrimaryMasks);
//UNITY_DECLARE_TEX2DARRAY(_RipplesTex);
#if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (defined(SHADER_TARGET_SURFACE_ANALYSIS) && !defined(SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER))|| defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE) || defined(SHADER_API_VULKAN) || defined(SHADER_API_METAL)
	#define TEXTURE_ARRAY_SUPPORTED 1
#endif

#if TEXTURE_ARRAY_SUPPORTED
	SamplerState sampler_RipplesTex;
	Texture2DArray _RipplesTex;
#endif
SamplerState vertex_linear_clamp_sampler;
uniform float _CoverageAreaBias;
uniform float _CoverageAreaMaskRange;
uniform float3 _WetColor;
uniform float _PuddlesMult;
uniform float _WetnessAmount;
uniform float _SpotsAmount;
uniform float _SpotsIntensity;
uniform float _DistanceFadeFalloff;
uniform float _DistanceFadeStart;
uniform float _RipplesAmount;
uniform float _RipplesTiling;
uniform float _RipplesFPS;
uniform float _RipplesFramesCount;
uniform float _RipplesIntensity;
uniform float _PuddlesBlendContrast;
uniform float2 _PuddlesRange;
uniform float _PuddlesAmount;
uniform float _PuddlesTiling;
uniform float _PuddlesSlope;
uniform float _PuddlesBlendStrength;
uniform float _BlendByNormalsPower;
uniform float _BlendByNormalsStrength;
uniform float2 _PrecipitationDirRange;
uniform float3 _depthCamDir;
uniform float _PrecipitationDirOffset;
uniform float _CoverageAreaFalloffHardness;
uniform float _DripsSpeed;
uniform float _DistortionTiling;
uniform float _DistortionAmount;
uniform float2 _DripsTiling;
uniform float _DripsIntensity;
uniform float4 _PaintedMask_ST;
uniform float4x4 _depthCamMatrix;

#if !defined(SHADOW_CASTER_PASS) && !defined (_REPLACEMENT)
void RainCoverage(inout v2f IN, inout float4 mixedDiffuse, inout float alpha, in float2 splatUV, in float3 geomN)
{
	#if defined(_RENDERING_CUTOUT)
        clip(alpha - _Cutoff);
    #endif

	float3 surfNormal = geomN;

	float approximateSurfHeight = (CalcWorldNormalVector(IN, IN.worldNormal)).y;

	//drips mask
	float3 absNormSurfNormal = abs(surfNormal);
	float xMask = (absNormSurfNormal.x - 0.3);
	float zMask = (absNormSurfNormal.z - 0.3);

	float4 addMask = float4(0, 0, 0, 0);
	float4 eraseMask = float4(1, 1, 1, 1);

	#ifdef _PAINTABLE_COVERAGE_ON
        #ifdef SRS_TERRAIN
            float2 covSplatUV = (IN.uv.xy * (_PaintedMask_TexelSize.zw - 1.0f) + 0.5f) * _PaintedMask_TexelSize.xy;
            float4 paintedMask = UNITY_SAMPLE_TEX2D_SAMPLER(_PaintedMask, _SRS_depth, covSplatUV); // reuse splatUV
		    PaintMaskRGBA(paintedMask, addMask, eraseMask);
        #else
            PaintMaskRGBA(IN.color, addMask, eraseMask);
        #endif
	#endif

	//sample VSM (depth coverage mask)
	float4 relativeSurfPos = mul(_depthCamMatrix, float4(IN.worldPos , 1.0));
	//float mean = exp(((((relativeSurfPos.z * -1.0) * 2.0) - 1.0) * 5.0));
	float2 srsDepthUV = (0.5 + ( 0.5 * relativeSurfPos)).xy;
	srsDepthUV.y = 1 - srsDepthUV.y;
	float2 moments = UNITY_SAMPLE_TEX2D(_SRS_depth, srsDepthUV).xy;
	
	float basicAreaMask = ComputeBasicAreaMask(moments, relativeSurfPos.z, _CoverageAreaBias, _CoverageAreaMaskRange, _CoverageLeakReduction); //basic occluded area mask
	float dirMask = DirMask(_PrecipitationDirRange, _depthCamDir, surfNormal, _PrecipitationDirOffset); //precipitation direction mask
	float volumeMask = CalcVolumeMask(_CoverageAreaFalloffHardness, srsDepthUV, relativeSurfPos.z);
	float2 covAreaMaskRange = _CoverageAreaMaskRange * 10;
	float outsideAreaMaskVal = saturate(smoothstep(0, _CoverageAreaMaskRange * 10, 5) * 2);
	//float outsideAreaMaskVal = saturate(smoothstep(covAreaMaskRange.x, covAreaMaskRange.y, 5) * 2);
	volumeMask = lerp(outsideAreaMaskVal, basicAreaMask, volumeMask);
	//debug = volumeMask;

	//Drips
	float3 blendNormals = IN.worldNormal;
	float dripsMask = 0;
	
	#if defined (_DRIPS_ON)	
		half3 n;
		#if defined(_USE_AVERAGED_NORMALS)
			n = IN.unifiedWorldNormal;
		#else
			n = surfNormal;
		#endif
		float3 dripsNormals;
		float3 surfTangent = CalcWorldNormalVector(IN, float3(1, 0, 0));
		float3 surfBitangent = CalcWorldNormalVector(IN, float3(0, 1, 0));
		float3x3 worldToTangentMatrix = float3x3(surfTangent, surfBitangent, surfNormal);

		float dripsDistMask = max((xMask * UNITY_SAMPLE_TEX2D(_PrimaryMasks, (IN.worldPos.zy * _DistortionTiling)).b), (UNITY_SAMPLE_TEX2D(_PrimaryMasks, (_DistortionTiling * IN.worldPos.xy)).b * zMask));
		float dripsDistAmount = (_DistortionAmount + saturate(((surfNormal.y - 0.5) * 0.15)));
		float3 dripsDistNormals = PerturbNormal(IN.worldPos, surfNormal, dripsDistMask, _DistortionAmount);

		float dripsSpeed = (_Time.y * _DripsSpeed);
		float2 worldToTangentDir = mul(worldToTangentMatrix, dripsDistNormals).xy;
		float2 dripsUV_distort_x = (worldToTangentDir + (_DripsTiling * IN.worldPos.zy));
		float2 dripsUV_distort_z = (worldToTangentDir + (_DripsTiling * IN.worldPos.xy));
		
		float2 dripsUV_x = (dripsSpeed * 0.6 * float2(0, 1) + dripsUV_distort_x);
		float2 dripsUV_z = (dripsSpeed * 0.6 * float2(0, 1) + dripsUV_distort_z);

		float dripsX = lerp(0.0, UNITY_SAMPLE_TEX2D(_PrimaryMasks, dripsUV_x).g, xMask);
		float dripsZ = lerp(0.0, UNITY_SAMPLE_TEX2D(_PrimaryMasks, dripsUV_z).g, zMask);

		//extra drips layer
		float2 dripsUV_distort_x_extra = (worldToTangentDir + (_DripsTiling * 0.7 * IN.worldPos.zy + 0.3));
		float2 dripsUV_distort_z_extra = (worldToTangentDir + (_DripsTiling * 0.7 * IN.worldPos.xy + 0.3));

		float2 dripsUV_x_extra = (dripsSpeed * float2(0, 1) + dripsUV_distort_x_extra);
		float2 dripsUV_z_extra = (dripsSpeed * float2(0, 1) + dripsUV_distort_z_extra);

		float dripsX_extra = lerp(0.0, UNITY_SAMPLE_TEX2D(_PrimaryMasks, dripsUV_x_extra).g, xMask);
		float dripsZ_extra = lerp(0.0, UNITY_SAMPLE_TEX2D(_PrimaryMasks, dripsUV_z_extra).g, zMask);

		dripsX = max(dripsX, dripsX_extra);
		dripsZ = max(dripsZ, dripsZ_extra);
		//

		float dripsBlend = max(dripsX, dripsZ);

		dripsMask = saturate((dripsBlend * saturate((_DripsIntensity * saturate((eraseMask.a * (addMask.a + (volumeMask * dirMask))))))));
		dripsNormals = mul(worldToTangentMatrix, PerturbNormal(IN.worldPos, surfNormal, dripsMask, 1));

		blendNormals = lerp(IN.worldNormal, dripsNormals, dripsMask);
	#endif

	half3 flatNormal = BlendReorientedNormal(half3(surfNormal.xz, absNormSurfNormal.y), half3(0, 0, 1)).xzy;//flat normal
	half3 rippleNormals = 0;
	float spots = 0;
	#if defined(_RIPPLES_ON)
		float2 ripplesAndSpotsUV = IN.worldPos.xz * _RipplesTiling;
		float rippleNormalAmp = 0;
		
		#if TEXTURE_ARRAY_SUPPORTED	
			float2 seed = float2(123.456, 789.012);
			float2 rotationRange = float2(0, 360);
			float2 offsetRange = float2(-1, 1);
			float2 scaleRange = float2(1, 2);
			float3 ripplesAndSpots = 0;
			float3 sampled;
			float ripplesAmount = round(_RipplesAmount);
			

			for(int i = 0; i < ripplesAmount; i++)
			{
				seed = frac(seed * 123.456);
				float curFrame = frac((((_RipplesFPS * (_Time.y + seed.y * i)) + (_RipplesFramesCount - 1.0)) / _RipplesFramesCount)) * _RipplesFramesCount;
				
				float2 randScale = lerp(scaleRange.x, scaleRange.y, seed.x);
				float2 randOffset = lerp(offsetRange.x, offsetRange.x, seed);
				float randRot = radians(lerp(rotationRange.x, rotationRange.y, seed.y));
				float2x2 rotMatrix = float2x2(cos(randRot), -sin(randRot), sin(randRot), cos(randRot));
				float2 uv = mul(rotMatrix, (ripplesAndSpotsUV * randScale) + randOffset);
				
				#if defined(_STOCHASTIC_ON)
					float4x3 BW_vx = CalcBW_vx(uv);
					float2 dx = ddx(uv);
					float2 dy = ddy(uv);

					float3 sampled = mul(UNITY_SAMPLE_TEX2DARRAY_GRAD(_RipplesTex, float3(uv + Hash2D2D(BW_vx[0].xy), curFrame), dx, dy), BW_vx[3].x) + 
					mul(UNITY_SAMPLE_TEX2DARRAY_GRAD(_RipplesTex, float3(uv + Hash2D2D(BW_vx[1].xy), curFrame), dx, dy), BW_vx[3].y) + 
					mul(UNITY_SAMPLE_TEX2DARRAY_GRAD(_RipplesTex, float3(uv + Hash2D2D(BW_vx[2].xy), curFrame), dx, dy), BW_vx[3].z);
				#else
					float3 sampled = UNITY_SAMPLE_TEX2DARRAY(_RipplesTex, float3(uv, curFrame));
				#endif
				
				ripplesAndSpots += sampled;
			}			
			ripplesAndSpots /= ripplesAmount;
			rippleNormalAmp = ripplesAmount;
		#else
			float3 ripplesAndSpots = float3(0, 0, 0);
		#endif

		float ripplesAndSpotsMask = saturate(((surfNormal.y * volumeMask * dirMask) + addMask.b)) * eraseMask.b;
		float ripplesIntensity = _RipplesIntensity * ripplesAndSpotsMask * rippleNormalAmp;
		
		if(ripplesAmount > 0)
		{
			rippleNormals = UnpackScaleNormal(float4(ripplesAndSpots.r , ripplesAndSpots.g, 0.0, 1.0), ripplesIntensity);
			rippleNormals = BlendReorientedNormal(half3(surfNormal.xz, absNormSurfNormal.y), rippleNormals);
    		rippleNormals = rippleNormals.xzy;
			spots = saturate((((ripplesAndSpots.b * step((1.0 - _SpotsAmount), ripplesAndSpots.b)) * ripplesAndSpotsMask * 2) * _SpotsIntensity) * 5);
		}
		else
		{
			rippleNormals = flatNormal;
			spots = 0;
		}
	#else
		rippleNormals = flatNormal; // use flat normal
		spots = 0;
	#endif
	
	float dripsAndSpots = saturate((dripsMask + spots));
	
	float2 puddlesUV = _PuddlesTiling * IN.worldPos.xz * 0.1;
	float puddlesMask = UNITY_SAMPLE_TEX2D(_PrimaryMasks, puddlesUV).r;
	puddlesMask = smoothstep(_PuddlesRange.x, _PuddlesRange.y, _PuddlesAmount * puddlesMask);
	float puddlesSlope = smoothstep((1.0 - _PuddlesSlope), 1.0, surfNormal.y);
	float finalPuddlesMask = saturate(pow((((1.0 - smoothstep(0, 1.0, approximateSurfHeight)) * saturate((eraseMask.g * (addMask.g + (puddlesMask * volumeMask)) * puddlesSlope))) * 4) + (saturate((eraseMask.g * (addMask.g + (puddlesMask * volumeMask)) * puddlesSlope)) * 2), 1));
	
	blendNormals = lerp(blendNormals, rippleNormals, finalPuddlesMask);
	float wetnessBlendByNormals = smoothstep(_BlendByNormalsPower, 1.0, approximateSurfHeight);
	float finalAreaMask = saturate(pow(((wetnessBlendByNormals*( eraseMask.r * ( volumeMask + addMask.r ) ))*4)+(( eraseMask.r * ( volumeMask + addMask.r ) )*2),_BlendByNormalsStrength));
	
	#ifdef SRS_TERRAIN_BAKE_SHADER
		float distanceFade = 0;
	#else
		float distanceFade = saturate((IN.eyeDepth -_ProjectionParams.y - _DistanceFadeStart) / _DistanceFadeFalloff);
	#endif

	float wetness = _WetnessAmount * dirMask;

	float3 blendAlbedo = lerp(mixedDiffuse.xyz, (mixedDiffuse.xyz * _WetColor), finalAreaMask * wetness);
	blendAlbedo = lerp(blendAlbedo, blendAlbedo * _PuddlesMult, finalPuddlesMask);

	float blendSmoothness = lerp(saturate((mixedDiffuse.a + (wetness * finalAreaMask))), 0.9 ,saturate(dripsAndSpots * 2.0));
	blendSmoothness = lerp(blendSmoothness, 0.99, finalPuddlesMask);
	float4 blendAlbedoAndSmoothness = float4(blendAlbedo, blendSmoothness);

	#if defined(SRS_TERRAIN)
		float3 distantNormal = UNITY_SAMPLE_TEX2D_SAMPLER(_NormalLOD, _PrimaryMasks, splatUV);
		distantNormal = distantNormal * 2 - 1;
		distantNormal = distantNormal.xzy;
		IN.worldNormal = lerp(blendNormals, distantNormal, distanceFade);

		mixedDiffuse = lerp(blendAlbedoAndSmoothness, UNITY_SAMPLE_TEX2D_SAMPLER(_AlbedoLOD, _PrimaryMasks, splatUV) , distanceFade);
	#else
		IN.worldNormal = blendNormals;
		mixedDiffuse = blendAlbedoAndSmoothness;
	#endif
}
#endif