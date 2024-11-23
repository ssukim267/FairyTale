#ifndef SRS_TESSELLATION_INCLUDED
#define SRS_TESSELLATION_INCLUDED 

uniform float2 _TessSnowdriftRange;
uniform float _TessFactorSnow;

struct TessellationFactors {
    float edge[3] : SV_TessFactor;
    float inside : SV_InsideTessFactor;
};


struct TessellationControlPoint {
	float4 vertex : INTERNALTESSPOS;
	float3 normal : NORMAL;
    float3 uv : TEXCOORD0;
    #if !defined(SRS_TERRAIN)
        float4 color : COLOR0;
    #endif
    #if defined(_USE_AVERAGED_NORMALS)
        half3 unifiedNormal : TEXCOORD3;
    #endif
    #if !defined (SHADOW_CASTER_PASS)
	    float4 tangent : TANGENT;
	    float2 uv1 : TEXCOORD1;
    #endif
    UNITY_VERTEX_INPUT_INSTANCE_ID 
    UNITY_VERTEX_OUTPUT_STEREO
};

bool TriangleIsBelowClipPlane (float3 p0, float3 p1, float3 p2, int planeIndex, float bias) {
	float4 plane = unity_CameraWorldClipPlanes[planeIndex];
	return
		dot(float4(p0, 1), plane) < bias &&
		dot(float4(p1, 1), plane) < bias &&
		dot(float4(p2, 1), plane) < bias;
}

bool TriangleIsCulled (float3 p0, float3 p1, float3 p2, float bias) {
	return TriangleIsBelowClipPlane(p0, p1, p2, 0, bias) || 
    TriangleIsBelowClipPlane(p0, p1, p2, 1, bias) || 
    TriangleIsBelowClipPlane(p0, p1, p2, 2, bias) || 
    TriangleIsBelowClipPlane(p0, p1, p2, 3, bias);
}

bool IsBackFace(float4 p0Pos, float4 p1Pos, float4 p2Pos, float bias)
{
    p0Pos = UnityObjectToClipPos(p0Pos);
    p1Pos = UnityObjectToClipPos(p1Pos);
    p2Pos = UnityObjectToClipPos(p2Pos);
    
    float3 p0 = p0Pos.xyz / p0Pos.w;
    float3 p1 = p1Pos.xyz / p1Pos.w;
    float3 p2 = p2Pos.xyz / p2Pos.w;

    #if UNITY_REVERSED_Z
        return cross(p1 - p0, p2 - p0).z < bias;
    #else
        return cross(p1 - p0, p2 - p0).z > bias;
    #endif
}

float3 BarycentricInterpolate(float3 bary, float3 a, float3 b, float3 c) {
    return bary.x * a + bary.y * b + bary.z * c;
}

float3 PhongProjectedPos(float3 flatPosWS, float3 cornerPosWS, float3 normalWS)
{
    return flatPosWS - dot(flatPosWS - cornerPosWS, normalWS) * normalWS;
}

float3 CalcPhongPos(float3 bary, float phongFactor, float3 p0PosWS, float3 p0NormalWS, float3 p1PosWS, float3 p1NormalWS, float3 p2PosWS, float3 p2NormalWS)
{
    float3 flatPosWS = BarycentricInterpolate(bary, p0PosWS, p1PosWS, p2PosWS);
    float3 smoothedPosWS = 
        bary.x * PhongProjectedPos(flatPosWS, p0PosWS, p0NormalWS) +
        bary.y * PhongProjectedPos(flatPosWS, p1PosWS, p1NormalWS) +
        bary.z * PhongProjectedPos(flatPosWS, p2PosWS, p2NormalWS);
    return lerp(flatPosWS, smoothedPosWS, phongFactor);
}

float TessellationEdgeFactor (float3 p0, float3 p1, float mask) 
{
	float edgeLength = distance(p0, p1);

	float3 edgeCenter = (p0 + p1) * 0.5;
	float viewDistance = distance(edgeCenter, _WorldSpaceCameraPos);
    float factor = edgeLength * _ScreenParams.y / (_TessEdgeL * viewDistance);
	//return factor;
    return max(1, factor * mask);
}

TessellationFactors PatchConstantFunction (InputPatch<TessellationControlPoint, 3> patch) {
    float3 p0 = mul(unity_ObjectToWorld, patch[0].vertex).xyz;
	float3 p1 = mul(unity_ObjectToWorld, patch[1].vertex).xyz;
	float3 p2 = mul(unity_ObjectToWorld, patch[2].vertex).xyz;

    float4 p0PosCS = UnityObjectToClipPos(patch[0].vertex);
    float4 p1PosCS = UnityObjectToClipPos(patch[1].vertex);
    float4 p2PosCS = UnityObjectToClipPos(patch[2].vertex);

	TessellationFactors f = (TessellationFactors)0;
    if (TriangleIsCulled(p0, p1, p2, -_TessMaxDisp)/* || IsBackFace(patch[0].vertex, patch[1].vertex, patch[2].vertex, -0.001)*/) {
		f.edge[0] = f.edge[1] = f.edge[2] = f.inside = 0;
	}
	else {

        float3 multipliers;
        [unroll] for(int i = 0; i < 3; i++)
        {
            multipliers[i] = patch[i].uv.z;
        }

        float3 mask;
        mask.x = (multipliers[1] + multipliers[2]) / 2;
        mask.y = (multipliers[2] + multipliers[0]) / 2;
        mask.z = (multipliers[0] + multipliers[1]) / 2;
        
        f.edge[0] = TessellationEdgeFactor(p1, p2, mask.x);
        f.edge[1] = TessellationEdgeFactor(p2, p0, mask.y);
        f.edge[2] = TessellationEdgeFactor(p0, p1, mask.z);
        //f.inside = (f.edge[0] + f.edge[1] + f.edge[2]) / 3;
	    f.inside = (TessellationEdgeFactor(p1, p2, mask.x) + TessellationEdgeFactor(p2, p0, mask.y) + TessellationEdgeFactor(p0, p1, mask.z)) * (1 / 3.0);
    }
	return f;
}

[UNITY_domain("tri")]
[UNITY_outputcontrolpoints(3)]
[UNITY_outputtopology("triangle_cw")]
[UNITY_partitioning("fractional_odd")]
[UNITY_patchconstantfunc("PatchConstantFunction")]
TessellationControlPoint HullProgram (InputPatch<TessellationControlPoint, 3> patch, uint id : SV_OutputControlPointID) 
{
    DEFAULT_UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(patch[id]);
	return patch[id];
}

[UNITY_domain("tri")]
v2f DomainProgram (TessellationFactors factors, OutputPatch<TessellationControlPoint, 3> patch, float3 barycentricCoordinates : SV_DomainLocation) 
{
	VertexData data;
    
    UNITY_INITIALIZE_OUTPUT(VertexData, data);
    UNITY_TRANSFER_INSTANCE_ID(patch[0], data);
    
    /*
    float3 posWS = CalcPhongPos(barycentricCoordinates, 1, patch[0].vertex, patch[0].normal, patch[1].vertex, patch[1].normal, patch[2].vertex, patch[2].normal);
    data.vertex = float4(posWS, 1);
    */
    #define DOMAIN_PROGRAM_INTERPOLATE(fieldName) data.fieldName = \
		patch[0].fieldName * barycentricCoordinates.x + \
		patch[1].fieldName * barycentricCoordinates.y + \
		patch[2].fieldName * barycentricCoordinates.z;

	DOMAIN_PROGRAM_INTERPOLATE(vertex)
    DOMAIN_PROGRAM_INTERPOLATE(normal)
    DOMAIN_PROGRAM_INTERPOLATE(uv)

    #if !defined (SHADOW_CASTER_PASS)
	    DOMAIN_PROGRAM_INTERPOLATE(tangent)
	    DOMAIN_PROGRAM_INTERPOLATE(uv1)
    #endif

    #if !defined(SRS_TERRAIN)
        DOMAIN_PROGRAM_INTERPOLATE(color)
        #if defined(_USE_AVERAGED_NORMALS)
            DOMAIN_PROGRAM_INTERPOLATE(unifiedNormal)
        #endif
    #endif
    return vert(data);
}

TessellationControlPoint TessellationVertexProgram (VertexData v) 
{
    TessellationControlPoint p;

    UNITY_INITIALIZE_OUTPUT(TessellationControlPoint, p);
    UNITY_TRANSFER_INSTANCE_ID(v, p);

    float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
    float3 relativeSurfPos = mul(_SRS_TraceSurfCamMatrix, float4(worldPos, 1));
    float2 traceMaskUV = (0.5 + ( 0.5 * relativeSurfPos)).xy;
    traceMaskUV.y = 1 - traceMaskUV.y;
    float4 addMask = float4(0, 0, 0, 0);
	float4 eraseMask = float4(1, 1, 1, 1);
	#ifdef _PAINTABLE_COVERAGE_ON
        #ifdef SRS_TERRAIN
            float2 covSplatUV = (v.uv.xy * (_PaintedMask_TexelSize.zw - 1.0f) + 0.5f) * _PaintedMask_TexelSize.xy;
	        float4 paintedMask = SAMPLE_TEXTURE2D_LOD(_PaintedMask, sampler_SRS_depth, covSplatUV, 0.0);
	        PaintMaskRGBA(paintedMask, addMask, eraseMask);
        #else
            PaintMaskRGBA(v.color, addMask, eraseMask);
        #endif
	#endif
    float covAmount = (eraseMask.r * _CoverageAmount);
    float covAmountFinal = (1.0 - (covAmount * 2.0));
    //sample VSM (depth coverage mask)
	relativeSurfPos = mul(_depthCamMatrix, float4(worldPos, 1));
	float2 srsDepthUV = (0.5 + ( 0.5 * relativeSurfPos)).xy;
	srsDepthUV.y = 1 - srsDepthUV.y;
    float2 moments = SAMPLE_TEXTURE2D_LOD(_SRS_depth, vertex_linear_clamp_sampler, srsDepthUV, 0);
	float basicAreaMask = ComputeBasicAreaMask(moments, relativeSurfPos.z, _CoverageAreaBias, _CoverageAreaMaskRange, _CoverageLeakReduction); //basic occluded area mask
    //basicAreaMask *= (covAmount + addMask.r);
    
    float dirMask = DirMask(_PrecipitationDirRange, _depthCamDir, normalize(UnityObjectToWorldNormal(v.normal)), _PrecipitationDirOffset); //precipitation direction mask
    float volumeMask = CalcVolumeMask(_CoverageAreaFalloffHardness, srsDepthUV, relativeSurfPos.z); //Weatherade volume mask
    
    /*
    float dirMulSnowHeight = pow(dirMask, 10);
    float basicMaskRemaped = saturate(basicAreaMask - _CoverageDisplacementOffset); // cheaper interpolation
    */
    float snowHeightMix = lerp(0, 1, covAmount);
    float snowMask = lerp(snowHeightMix, basicAreaMask * snowHeightMix, volumeMask) * dirMask;
    snowMask = saturate(snowMask * 10 + addMask.r);
    float snowAreaTess = lerp(0, 1, snowMask * _TessFactorSnow);
    
    /*
    float finalDispMask = lerp(dirMulSnowHeight, basicMaskRemaped * 2, dirMask) * dirMulSnowHeight;
    finalDispMask = lerp((1 - _CoverageDisplacementOffset) * dirMulSnowHeight, finalDispMask, volumeMask);
    */
    float snowdriftSlopeMask = smoothstep(_TessSnowdriftRange.x, _TessSnowdriftRange.y, basicAreaMask * covAmount + addMask.r);
    float snowdriftSlope = sin(UNITY_PI * snowdriftSlopeMask);
    
    #if defined(_TRACES_ON)
        float traceMaskSampled = SAMPLE_TEXTURE2D_LOD(_SRS_TraceTex, vertex_linear_clamp_sampler, traceMaskUV, 0).z;
        float tessMask = saturate(traceMaskSampled * 150 + snowdriftSlope + snowAreaTess);//pack the trace mask into the mesh uv.z to access it from the tessellator
    #else
        float tessMask = saturate(snowdriftSlope + snowAreaTess);
    #endif
        v.uv.z = tessMask * volumeMask;

	p.vertex = v.vertex;
	p.normal = v.normal;
    p.uv = v.uv;
    #if defined(_USE_AVERAGED_NORMALS)
        p.unifiedNormal = v.unifiedNormal;
    #endif

    #if !defined(SRS_TERRAIN)
        p.color = v.color;
    #endif

    #if !defined (SHADOW_CASTER_PASS)
	    p.tangent = v.tangent;
	    p.uv1 = v.uv1;
    #endif

	return p;
}


#endif