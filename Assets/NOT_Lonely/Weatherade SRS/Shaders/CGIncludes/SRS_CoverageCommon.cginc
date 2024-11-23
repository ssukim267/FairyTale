//Common functions for the Weatherade coverage shaders
#ifndef SRS_COVERAGE_COMMON
#define SRS_COVERAGE_COMMON

#pragma exclude_renderers gles

#if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (defined(SHADER_TARGET_SURFACE_ANALYSIS) && !defined(SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER))
    #define SAMPLE_TEXTURE2D_LOD(tex,samplerTex,coord,lod) tex.SampleLevel(samplerTex,coord, lod)
#else
    #define SAMPLE_TEXTURE2D_LOD(tex,samplerTex,coord,lod) tex2Dlod(tex,float4(coord,0,lod))
#endif

#if defined(_USE_BLUE_NOISE_DITHER)
UNITY_DECLARE_TEX2DARRAY(_BlueNoise);
float4 _BlueNoise_TexelSize;
#endif

uniform float _VsmExp;

#if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE) || defined(SHADER_API_VULKAN) || defined(SHADER_API_METAL)
	#define UNITY_SAMPLE_TEX2DARRAY_GRAD(tex,coord,dx,dy) tex.SampleGrad (sampler##tex,coord,dx,dy)
#else
	#if defined(UNITY_COMPILER_HLSL2GLSL) || defined(SHADER_TARGET_SURFACE_ANALYSIS)
		#define UNITY_SAMPLE_TEX2DARRAY_GRAD(tex,coord,dx,dy) tex2DArray(tex,coord,dx,dy)
	#endif
#endif

//height blend coverage with the base surface
float HeightBlendTriplanar(float sourceMask, float3 height, float3 worldMasks, float blendHardness, float blendStrength)
{
	float blendTop = (height.x + height.z) * worldMasks.y * 0.3;
	float blendSides = (worldMasks.x + worldMasks.z) * height.y;
	float heightMix = blendTop + blendSides;
	heightMix = saturate(heightMix * heightMix * blendHardness);
    float blendResult = saturate(pow(((heightMix * sourceMask) * 4) + (sourceMask * 2), blendStrength));
    return blendResult;
}

float BlendBasicMaskWithHeight(float height1, float height2, float blendFactor)
{
	float input1 = 1;
	float input2 = 0;
    float height_start = max(height1, height2) - blendFactor;
    float level1 = max(height1 - height_start, 0);
    float level2 = max(height2 - height_start, 0);
    return ((input1 * level1) + (input2 * level2)) / (level1 + level2);
}

float3 PerturbNormal( float3 surf_pos, float3 surf_norm, float height, float scale )
{
	// "Bump Mapping Unparametrized Surfaces on the GPU" by Morten S. Mikkelsen
	float3 vSigmaS = ddx( surf_pos );
	float3 vSigmaT = ddy( surf_pos );
	float3 vN = surf_norm ;
	float3 vR1 = cross( vSigmaT , vN );
	float3 vR2 = cross( vN , vSigmaS );
	float fDet = dot( vSigmaS , vR1 );
	float dBs = ddx( height );
	float dBt = ddy( height );
	float3 vSurfGrad = scale * 0.05 * sign( fDet ) * ( dBs * vR1 + dBt * vR2 );
	return normalize (abs(fDet) * vN - vSurfGrad);
}

float linstep(float a, float b, float v) {
    return saturate((v - a) / (b - a));
}

float reduceLightBleeding(float pMax, float amount) {
   // Remove the [0, amount] tail and linearly rescale (amount, 1].
   return linstep(amount, 1.0, pMax);
}

float chebyshevUpperBound(float2 moments, float mean, float minVariance, float lightBleedingReduction) {
    // Compute variance
    float variance = moments.y - (moments.x * moments.x);
    variance = max(variance, minVariance);

    // Compute probabilistic upper bound
    float d = mean - moments.x;
    float pMax = variance / (variance + (d * d));

    pMax = reduceLightBleeding(pMax, lightBleedingReduction);

    // One-tailed Chebyshev
    return (mean <= moments.x ? 1.0 : pMax);
}

float ComputeBasicAreaMask(float2 moments, float z, float vsmBias, float coverageAreaMaskRange, float bleedReduction) {

    z = 2.0 * -z - 1.0;
    float warpedDepth = exp(_VsmExp * z);

    float VSMBias = vsmBias;
    float depthScale = VSMBias * _VsmExp * warpedDepth;
    float minVariance1 = depthScale * depthScale;
	float mask = chebyshevUpperBound(moments.xy, warpedDepth, minVariance1, bleedReduction);
	mask = saturate(smoothstep(0, 10 * coverageAreaMaskRange, (5.0 * mask)) * 2);
	mask = sqrt(mask);
    return mask;
}

float ComputeBasicAreaMask(float2 moments, float z, float coverageAreaBias, float coverageAreaMaskRange)
{
	//float exponent = 5;
	float mean = exp(((((z * -1.0) * 2.0) - 1.0) * _VsmExp));
	//compute variance
	float minVariance = (mean * _VsmExp * coverageAreaBias);
	minVariance = (minVariance * minVariance);
	float variance = moments.y - (moments.x * moments.x);
	variance = max(variance, minVariance);
	
	//Probabilistic upper bound
	float d = (mean - moments.x);
	float pMax = saturate((variance / (variance + (d * d))));
	float basicAreaMask = 0;
	
	if( mean <= moments.x )
		basicAreaMask = 1.0;
	else
		basicAreaMask = pMax;
		
	basicAreaMask = saturate(smoothstep(0, 10 * coverageAreaMaskRange, (5.0 * basicAreaMask)) * 2);
	
	return basicAreaMask;
}

void PaintMaskRGBA(float4 paintedMask, out float4 addMask, out float4 eraseMask)
{
	float4 paintMode = round(paintedMask);
	float4 addCov = (paintedMask - 0.5) * 2.0;
	float4 removeCov = paintedMask * 2;
	float4 paintResult = lerp(removeCov, addCov, paintMode);
	
	addMask = lerp(0, paintResult, paintMode);
	eraseMask = lerp(paintResult, 1, paintMode);
}

float PaintMask(float paintInput, out float addMask, out float eraseMask)
{
	float paintMode = round(paintInput);
	float addCov = (paintInput - 0.5) * 2.0;
	float removeCov = paintInput * 2;
	float paintResult = lerp(removeCov, addCov, paintMode);
	
	addMask = lerp(0, paintResult, paintMode);
	eraseMask = lerp(paintResult, 1, paintMode);
}

float DirMask(float2 dirRange, float3 depthCamDir, float3 surfNormal, float dirOffset)
{
	float dirMask = smoothstep(dirRange.x, dirRange.y, (1.0 - saturate((dot(depthCamDir, surfNormal) + 1.0) - dirOffset)));
	return saturate(dirMask);
}

float CalcVolumeMask(float covAreaHardness, float2 uv, float relativeDepth)
{
	float volumeMask = min(min((1.0 - uv.x), (1.0 - uv.y)), min(uv.x, uv.y));
	volumeMask = min(volumeMask, relativeDepth);
	volumeMask = smoothstep(0, (1.0 - ((covAreaHardness * 0.5) + 0.5)), volumeMask);
	
	return volumeMask;
}

float CalcBorderMask(float covAreaHardness, float2 uv)
{
	float borderMask = min(min((1.0 - uv.x), (1.0 - uv.y)), min(uv.x, uv.y));
	borderMask = smoothstep(0.0, (1.0 - ((covAreaHardness * 0.5) + 0.5)), borderMask);
	return borderMask;
}


float SplatMask(float covAreaHardness, float2 uv, float relativeDepth, float a, float b)
{
	float volumeMask = smoothstep(0.0, (1.0 - ((covAreaHardness * 0.5) + 0.5)), min(min(min((1.0 - uv.x), (1.0 - uv.y)), min(uv.x, uv.y)), relativeDepth));
	float splatMask = saturate(lerp(a, (a * b), volumeMask));
	return splatMask;
}

float3 CalculateDistantNormals(float3 distantNormal, float3 geomN)
{
	//#if defined(UNITY_INSTANCING_ENABLED) && !defined(SHADER_API_D3D11_9X) && defined(TERRAIN_INSTANCED_PERPIXEL_NORMAL)
		float3 geomTangent = normalize(cross(geomN, float3(0, 0, 1)));
	    float3 geomBitangent = normalize(cross(geomTangent, geomN));
		distantNormal = distantNormal.x * geomTangent + distantNormal.y * geomBitangent + distantNormal.z * geomN;  
		distantNormal = distantNormal.xzy;
	//#endif
	
	return distantNormal;
}

half3 ConstructNormal(half2 inputVector, float scale)
{
	half3 n;
    n.xy = inputVector.xy * 2 - 1;
    n.xy *= scale;
    n.z = sqrt(1 - saturate(dot(n.xy, n.xy)));
    n = normalize(n);
	return n;
}

half3 BlendReorientedNormal(half3 n1, half3 n2)
{
    n1.z += 1;
    n2.xy = -n2.xy;
    return n1 * dot(n1, n2) / n1.z - n2;
}

float2 Hash2D2D (float2 s)
{
	return frac(sin(fmod(float2(dot(s, float2(127.1,311.7)), dot(s, float2(269.5,183.3))), 3.14159))*43758.5453);
}

float4x3 CalcBW_vx(float2 UV)
{

	float2 skewUV = mul(float2x2 (1.0 , 0.0 , -0.57735027 , 1.15470054), UV * 3.464);

	float2 vxID = float2 (floor(skewUV));
	float3 barry = float3 (frac(skewUV), 0);
	barry.z = 1.0-barry.x-barry.y;
 
	float4x3 BW_vx = ((barry.z>0) ? 
		float4x3(float3(vxID, 0), float3(vxID + float2(0, 1), 0), float3(vxID + float2(1, 0), 0), barry.zyx) :
		float4x3(float3(vxID + float2 (1, 1), 0), float3(vxID + float2 (1, 0), 0), float3(vxID + float2 (0, 1), 0), float3(-barry.z, 1.0-barry.y, 1.0-barry.x)));
	
	return BW_vx;
}
 
float4 StochasticTex2D(sampler2D tex, float2 UV)
{
	float4x3 BW_vx = CalcBW_vx(UV);

	float2 dx = ddx(UV);
	float2 dy = ddy(UV);
 
	return mul(tex2D(tex, UV + Hash2D2D(BW_vx[0].xy), dx, dy), BW_vx[3].x) + 
			mul(tex2D(tex, UV + Hash2D2D(BW_vx[1].xy), dx, dy), BW_vx[3].y) + 
			mul(tex2D(tex, UV + Hash2D2D(BW_vx[2].xy), dx, dy), BW_vx[3].z);
}

float4 StochasticTex2DLod(sampler2D tex, float2 UV, float lod)
{
	float4x3 BW_vx = CalcBW_vx(UV);

	return mul(tex2Dlod(tex, float4(UV + Hash2D2D(BW_vx[0].xy), 0, lod)), BW_vx[3].x) + 
			mul(tex2Dlod(tex, float4(UV + Hash2D2D(BW_vx[1].xy), 0, lod)), BW_vx[3].y) + 
			mul(tex2Dlod(tex, float4(UV + Hash2D2D(BW_vx[2].xy), 0, lod)), BW_vx[3].z);
}

#if defined (_USE_BLUE_NOISE_DITHER)
fixed Dither(float4 screenPos)
{
	half framesCount = 32;
	float2 uv = (screenPos.xy / screenPos.w) * _ScreenParams.xy * _BlueNoise_TexelSize.xy;
	float frameNumber = unity_DeltaTime.y * _Time.y + (framesCount - 1);
	frameNumber /= framesCount;
	frameNumber = frac(frameNumber) * framesCount;
	float oneMinusTexelS = 1 - _BlueNoise_TexelSize.x * _BlueNoise_TexelSize.y;

	fixed dither = UNITY_SAMPLE_TEX2DARRAY_LOD(_BlueNoise, float3(uv, frameNumber), 0).r * oneMinusTexelS + oneMinusTexelS;

	return dither;
}

void AnimatedCrossFadeLOD(float2 vpos)
{
    float2 uv = vpos/16; //devide by texture size to get a pixel perfect texture on screen
    half framesCount = 32;
	float frameNumber = unity_DeltaTime.y * _Time.y + (framesCount - 1);
	frameNumber /= framesCount;
	frameNumber = frac(frameNumber) * framesCount;
	float oneMinusTexelS = 1 - _BlueNoise_TexelSize.x * _BlueNoise_TexelSize.y;
	fixed dither = UNITY_SAMPLE_TEX2DARRAY_LOD(_BlueNoise, float3(uv, frameNumber), 0).r;
    float sgn = unity_LODFade.x > 0 ? 1.0f : -1.0f;
    clip(unity_LODFade.x - dither * sgn);
}
#endif

float4 SingleSampleTriplanar(sampler2D sam, float3 position, float3 normal, float contrast, float tiling, float dither)
{
	float2 uvX = position.yz;
	float2 uvY = position.zx;
	float2 uvZ = position.xy;
	
	float3 alpha = pow(abs(normal), contrast);
	alpha /= dot(1, alpha);
	dither *= 0.5;

	float3 duvwdx = ddx( position * tiling);
	float3 duvwdy = ddy( position * tiling );

	float2 duvdx; 
	float2 duvdy; 

	float2 uv;
	if (alpha.x > dither) {
	    uv = uvX;
	    duvdx = duvwdx.yz;
	    duvdy = duvwdy.yz;
	} else if (1.0 - alpha.z > dither) {
	    uv = uvY;
	    duvdx = duvwdx.zx;
	    duvdy = duvwdy.zx;	
	} else {
	    uv = uvZ;
	    duvdx = duvwdx.xy;
	    duvdy = duvwdy.xy;
	}
	uv *= tiling;

	float4 col = tex2D( sam, uv, duvdx, duvdy);

	return col;
}
#endif
