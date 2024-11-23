#ifndef SRS_TERRAIN_SPLATMIX_INCLUDED
#define SRS_TERRAIN_SPLATMIX_INCLUDED 

#if defined(_NORMALMAP) && !defined(_TERRAIN_NORMAL_MAP)
    #define _TERRAIN_NORMAL_MAP
#elif !defined(_NORMALMAP) && defined(_TERRAIN_NORMAL_MAP)
    #define _NORMALMAP
#endif


#if defined(SHADER_API_GLCORE) || defined(SHADER_API_GLES3) || defined(SHADER_API_GLES)
    // GL doesn't support sperating the samplers from the texture object
    #undef SUPPORT_SEPARATE_SAMPLER
#else
    #define SUPPORT_SEPARATE_SAMPLER
#endif

float _TilingMultiplier;

#if !defined (_REPLACEMENT)
    sampler2D _Control;
    float4 _Control_ST;
    float4 _Control_TexelSize;
    UNITY_DECLARE_TEX2D_NOSAMPLER(_Splat0);
    UNITY_DECLARE_TEX2D_NOSAMPLER(_Splat1);
    UNITY_DECLARE_TEX2D_NOSAMPLER(_Splat2);
    UNITY_DECLARE_TEX2D_NOSAMPLER(_Splat3);
    SamplerState sampler_Splat0;

    float4 _Splat0_ST, _Splat1_ST, _Splat2_ST, _Splat3_ST;
#endif

#if defined(UNITY_INSTANCING_ENABLED) && !defined(SHADER_API_D3D11_9X)
    // Some drivers have undefined behaviors when samplers are used from the vertex shader
    // with anisotropic filtering enabled. This causes some artifacts on some devices. To be
    // sure to avoid this we use the vertex_linear_clamp_sampler sampler to sample terrain
    // maps from the VS when we can.
    #if defined(SUPPORT_SEPARATE_SAMPLER)
        UNITY_DECLARE_TEX2D(_TerrainHeightmapTexture);
        UNITY_DECLARE_TEX2D(_TerrainNormalmapTexture);
        SamplerState sampler__TerrainNormalmapTexture;
    #else
        sampler2D _TerrainHeightmapTexture;
        sampler2D _TerrainNormalmapTexture;
    #endif

    float4 _TerrainHeightmapRecipSize;   // float4(1.0f/width, 1.0f/height, 1.0f/(width-1), 1.0f/(height-1))
    float4 _TerrainHeightmapScale;       // float4(hmScale.x, hmScale.y / (float)(kMaxHeight), hmScale.z, 0.0f)
#endif

UNITY_INSTANCING_BUFFER_START(Terrain)
    UNITY_DEFINE_INSTANCED_PROP(float4, _TerrainPatchInstanceData) // float4(xBase, yBase, skipScale, ~)
UNITY_INSTANCING_BUFFER_END(Terrain)

#if !defined (_REPLACEMENT)
    #ifdef _NORMALMAP
        UNITY_DECLARE_TEX2D_NOSAMPLER(_Normal0);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_Normal1);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_Normal2);
        UNITY_DECLARE_TEX2D_NOSAMPLER(_Normal3);
        float _NormalScale0, _NormalScale1, _NormalScale2, _NormalScale3;
    #endif
#endif

#ifdef _ALPHATEST_ON
    sampler2D _TerrainHolesTexture;

    void ClipHoles(float2 uv)
    {
        float hole = tex2D(_TerrainHolesTexture, uv).r;
        clip(hole == 0.0f ? -1 : 1);
    }
#endif

void TerrainVert(inout v2fVertex o, inout VertexData v)
{
#if defined(UNITY_INSTANCING_ENABLED) && !defined(SHADER_API_D3D11_9X) 
    float2 patchVertex = v.vertex.xy;
    float4 instanceData = UNITY_ACCESS_INSTANCED_PROP(Terrain, _TerrainPatchInstanceData);

    float4 uvscale = instanceData.z * _TerrainHeightmapRecipSize;
    float4 uvoffset = instanceData.xyxy * uvscale;
    uvoffset.xy += 0.5f * _TerrainHeightmapRecipSize.xy;
    float2 sampleCoords = (patchVertex.xy * uvscale.xy + uvoffset.xy);

    #if defined(SUPPORT_SEPARATE_SAMPLER)
        float hm = UnpackHeightmap(_TerrainHeightmapTexture.SampleLevel(vertex_linear_clamp_sampler, sampleCoords, 0));
    #else
        float hm = UnpackHeightmap(tex2Dlod(_TerrainHeightmapTexture, float4(sampleCoords, 0, 0)));
    #endif

    v.vertex.xz = (patchVertex.xy + instanceData.xy) * _TerrainHeightmapScale.xz * instanceData.z;  //(x + xBase) * hmScale.x * skipScale;
    v.vertex.y = hm * _TerrainHeightmapScale.y;
    v.vertex.w = 1.0f;

    v.uv.xy = (patchVertex.xy * uvscale.zw + uvoffset.zw);
    
    #if !defined(SHADOW_CASTER_PASS) && !defined (_REPLACEMENT)
        v.uv1 = v.uv.xy;
    #endif
    
    //sample terrain normal texture here to get an actual normal for the displacement vector
    #if defined(SUPPORT_SEPARATE_SAMPLER)
        float3 nor = _TerrainNormalmapTexture.SampleLevel(vertex_linear_clamp_sampler, sampleCoords, 0).xyz;
    #else
        float3 nor = tex2Dlod(_TerrainNormalmapTexture, float4(sampleCoords, 0, 0)).xyz;
    #endif
    half3 worldNormal = 2.0f * nor - 1.0f;
    #ifdef TERRAIN_INSTANCED_PERPIXEL_NORMAL
        o.uv.zw = sampleCoords;
    #endif
    v.normal = worldNormal;
#else
    half3 worldNormal = normalize(UnityObjectToWorldNormal(v.normal));
#endif
    
    o.uv.xy = v.uv.xy;

#if !defined(SHADOW_CASTER_PASS) && !defined (_REPLACEMENT)
    v.tangent.xyz = cross(worldNormal, float3(0,0,1));
    v.tangent.w = -1;

    //compute tSpace matrix 
    half3 wTangent = UnityObjectToWorldDir(v.tangent.xyz);
    half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
    half3 wBitangent = cross(worldNormal, wTangent) * tangentSign;

    o.tspace0 = half3(wTangent.x, wBitangent.x, worldNormal.x);
    o.tspace1 = half3(wTangent.y, wBitangent.y, worldNormal.y);
    o.tspace2 = half3(wTangent.z, wBitangent.z, worldNormal.z);
#endif
    #if defined (_COVERAGE_ON) && defined(SRS_SNOW_COVERAGE)
        Displace(v, worldNormal);
    #endif

    //dirty fix for motion vectors issue in Unity 2023 that cause the terrain gets blured 
    //when it's not in world zero or use a custom shader with tessellation.
    //v.vertex.xyz += v.normal * 0.001; 

#ifndef SHADOW_CASTER_PASS
    
    float4 pos = UnityObjectToClipPos(v.vertex);
    o.pos = pos;

    #if !defined (_REPLACEMENT)
        #ifndef TERRAIN_BASE_PASS
            o.eyeDepth = -UnityObjectToViewPos(v.vertex.xyz).z;
            UNITY_TRANSFER_FOG(o, pos);
        #endif
            #if defined(LIGHTMAP_ON)
        		o.lightmapUV = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
        	#endif

        o.worldPos = mul(unity_ObjectToWorld, v.vertex);
        o.worldNormal = worldNormal;
       
        #if defined(_SPARKLE_ON) && defined (_COVERAGE_ON)/* || SHADOWS_SEMITRANSPARENT*/
            float4 screenPos = ComputeScreenPos(pos);
            float2 ssUV = screenPos.xy / screenPos.w;
            float ratio = _ScreenParams.x / _ScreenParams.y;
            ssUV.x = ssUV.x * ratio;
            o.ssUV = ssUV;
        #endif
    #endif   
#endif
}

#if !defined(SHADOW_CASTER_PASS) && !defined (_REPLACEMENT)
void SplatmapMix(inout v2f IN, out half weight, out fixed4 mixedDiffuse, inout fixed3 splatsNormal, out float2 splatUV, out float3 geomN)
{
    #ifdef _ALPHATEST_ON
        ClipHoles(IN.uv.xy);
    #endif
    
    geomN = float3(0, 1, 0);

    // adjust splatUVs so the edges of the terrain tile lie on pixel centers
    splatUV = (IN.uv.xy * (_Control_TexelSize.zw - 1.0f) + 0.5f) * _Control_TexelSize.xy;
    half4 splat_control = tex2D(_Control, splatUV);
    weight = dot(splat_control, half4(1,1,1,1));

    #if !defined(SHADER_API_MOBILE) && defined(TERRAIN_SPLAT_ADDPASS)
        clip(weight == 0.0f ? -1 : 1);
    #endif

    // Normalize weights before lighting and restore weights in final modifier functions so that the overal
    // lighting result can be correctly weighted.
    splat_control /= (weight + 1e-3f);

    float2 uvSplat0 = IN.uv.xy * _Splat0_ST.xy * _TilingMultiplier + _Splat0_ST.zw;
    float2 uvSplat1 = IN.uv.xy * _Splat1_ST.xy * _TilingMultiplier + _Splat1_ST.zw;
    float2 uvSplat2 = IN.uv.xy * _Splat2_ST.xy * _TilingMultiplier + _Splat2_ST.zw;
    float2 uvSplat3 = IN.uv.xy * _Splat3_ST.xy * _TilingMultiplier + _Splat3_ST.zw;

    mixedDiffuse = 0.0f;
    mixedDiffuse += splat_control.r * UNITY_SAMPLE_TEX2D_SAMPLER(_Splat0, _Splat0, uvSplat0);
    mixedDiffuse += splat_control.g * UNITY_SAMPLE_TEX2D_SAMPLER(_Splat1, _Splat0, uvSplat1);
    mixedDiffuse += splat_control.b * UNITY_SAMPLE_TEX2D_SAMPLER(_Splat2, _Splat0, uvSplat2);
    mixedDiffuse += splat_control.a * UNITY_SAMPLE_TEX2D_SAMPLER(_Splat3, _Splat0, uvSplat3);

    #ifdef _NORMALMAP
        IN.worldNormal  = UnpackNormalWithScale(UNITY_SAMPLE_TEX2D_SAMPLER(_Normal0, _Splat0, uvSplat0), _NormalScale0) * splat_control.r;
        IN.worldNormal += UnpackNormalWithScale(UNITY_SAMPLE_TEX2D_SAMPLER(_Normal1, _Splat0, uvSplat1), _NormalScale1) * splat_control.g;
        IN.worldNormal += UnpackNormalWithScale(UNITY_SAMPLE_TEX2D_SAMPLER(_Normal2, _Splat0, uvSplat2), _NormalScale2) * splat_control.b;
        IN.worldNormal += UnpackNormalWithScale(UNITY_SAMPLE_TEX2D_SAMPLER(_Normal3, _Splat0, uvSplat3), _NormalScale3) * splat_control.a;
#if defined(SHADER_API_SWITCH)
        IN.worldNormal.z += UNITY_HALF_MIN; // to avoid nan after normalizing
#else
        IN.worldNormal.z += 1e-5f; // to avoid nan after normalizing
#endif     
    #endif

    splatsNormal = IN.worldNormal;

    #if defined(INSTANCING_ON) && defined(SHADER_TARGET_SURFACE_ANALYSIS) && defined(TERRAIN_INSTANCED_PERPIXEL_NORMAL)
        IN.worldNormal = float3(0, 0, 1); // make sure that surface shader compiler realizes we write to normal, as UNITY_INSTANCING_ENABLED is not defined for SHADER_TARGET_SURFACE_ANALYSIS.
    #endif

    #if defined(UNITY_INSTANCING_ENABLED) && !defined(SHADER_API_D3D11_9X) && defined(TERRAIN_INSTANCED_PERPIXEL_NORMAL)

        #if defined(SUPPORT_SEPARATE_SAMPLER)
            float3 geomNormal = normalize(_TerrainNormalmapTexture.Sample(sampler__TerrainNormalmapTexture, IN.uv.zw).xyz * 2 - 1);
        #else
            float3 geomNormal = normalize(tex2D(_TerrainNormalmapTexture, IN.uv.zw).xyz * 2 - 1);
        #endif

        #ifdef _NORMALMAP
        /*
            float3 geomTangent = normalize(cross(geomNormal, float3(0, 0, 1)));
            float3 geomBitangent = normalize(cross(geomTangent, geomNormal));
            IN.worldNormal = IN.worldNormal.x * geomTangent
                          + IN.worldNormal.y * geomBitangent
                          + IN.worldNormal.z * geomNormal;
                          */
            IN.worldNormal = BlendReorientedNormal(half3(geomNormal.xz, abs(geomNormal).y), IN.worldNormal);
            IN.worldNormal = IN.worldNormal.xzy;
        
        #else
            IN.worldNormal = geomNormal;
        #endif
        //IN.worldNormal = IN.worldNormal.xzy;
        
        geomN = geomNormal;
    #else
        geomN = normalize(CalcWorldNormalVector(IN, float3(0, 0, 1)));

        IN.worldNormal = BlendReorientedNormal(half3(geomN.xz, abs(geomN).y), IN.worldNormal);
        IN.worldNormal = IN.worldNormal.xzy;
        
/*
        float3 geomTangent = normalize(cross(geomN, float3(0, 0, 1)));
        float3 geomBitangent = normalize(cross(geomTangent, geomN));
        IN.worldNormal = IN.worldNormal.x * geomTangent
                          + IN.worldNormal.y * geomBitangent
                          + IN.worldNormal.z * geomN;
                          */
        //IN.worldNormal = IN.worldNormal.xzy;
    #endif
}
#endif

#endif // SRS_TERRAIN_SPLATMIX_INCLUDED
