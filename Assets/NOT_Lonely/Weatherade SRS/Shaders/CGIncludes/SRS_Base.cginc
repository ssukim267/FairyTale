#ifndef SRS_BASE_INCLUDED
#define SRS_BASE_INCLUDED 
#pragma shader_feature _SPECULARHIGHLIGHTS_OFF
#pragma shader_feature _GLOSSYREFLECTIONS_OFF

UNITY_DECLARE_TEX2D(_MainTex);
float4 _MainTex_ST;
float4 _MainTex_TexelSize;

#if !defined(SRS_TERRAIN)
    UNITY_INSTANCING_BUFFER_START(InstancedProps)
        UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
    UNITY_INSTANCING_BUFFER_END(InstancedProps)
#endif

#if !defined (_REPLACEMENT)
    UNITY_DECLARE_TEX2D(_MetallicGlossMap);
    UNITY_DECLARE_TEX2D(_OcclusionMap);
    UNITY_DECLARE_TEX2D(_EmissionMap);
    uniform float _Metallic;
    uniform float _GlossMapScale;
    uniform float _Glossiness;
    uniform float _OcclusionStrength;
    uniform float3 _EmissionColor;
    //uniform float4 _Color;
    UNITY_DECLARE_TEX2D(_BumpMap);
    uniform float _BumpScale;
    uniform float _EmissionMasking;
#endif

void Vert(inout v2fVertex o, inout VertexData v)
{
    half3 worldNormal = normalize(UnityObjectToWorldNormal(v.normal));
    o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
    #if !defined(SHADOW_CASTER_PASS) && !defined (_REPLACEMENT)
        o.tangent = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);
        
        //compute tSpace matrix 
        half3 wTangent = UnityObjectToWorldDir(v.tangent.xyz);
        half tangentSign = v.tangent.w * unity_WorldTransformParams.w;

        #if defined(_USE_AVERAGED_NORMALS)
            half3 wNormal = normalize(UnityObjectToWorldNormal(v.unifiedNormal));
        #else
            half3 wNormal = worldNormal;
        #endif

        half3 wBitangent = cross(wNormal, wTangent) * tangentSign;

        o.tspace0 = half3(wTangent.x, wBitangent.x, wNormal.x);
        o.tspace1 = half3(wTangent.y, wBitangent.y, wNormal.y);
        o.tspace2 = half3(wTangent.z, wBitangent.z, wNormal.z);
    #endif

    #if defined(_COVERAGE_ON)
        #if defined(_USE_AVERAGED_NORMALS)
            half3 unifiedNormal = UnityObjectToWorldNormal(v.unifiedNormal);

            #if !defined(SHADOW_CASTER_PASS) && !defined (_REPLACEMENT)
                o.unifiedWorldNormal = unifiedNormal;
            #endif
            half3 displacementNormal = unifiedNormal;
        #else
            half3 displacementNormal = worldNormal;
        #endif
        #if defined(SRS_SNOW_COVERAGE)
            Displace(v, displacementNormal);
        #endif
    #endif

    #ifndef SHADOW_CASTER_PASS
        float4 pos = UnityObjectToClipPos(v.vertex);
        o.pos = pos;

        #if !defined (_REPLACEMENT)
            o.color = v.color;
            o.eyeDepth = -UnityObjectToViewPos(v.vertex.xyz).z;
            UNITY_TRANSFER_FOG(o, pos);
        
            #if defined(LIGHTMAP_ON)
            	o.lightmapUV = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
		        o.dynamicLightmapUV = v.uv2 * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
	        #endif

            o.worldPos = mul(unity_ObjectToWorld, v.vertex);
            o.worldNormal = worldNormal;

            #if defined(_SPARKLE_ON) && defined (_COVERAGE_ON)/* || SHADOWS_SEMITRANSPARENT*/
                float4 screenPos = ComputeScreenPos(pos);
                float2 ssUV = screenPos / screenPos.w;
                float ratio = _ScreenParams.x / _ScreenParams.y;
                ssUV.x = ssUV.x * ratio;
                o.ssUV = ssUV;
            #endif
        #endif   
    #endif
}

#if !defined (_REPLACEMENT)
void BaseMapping(inout v2f IN, out half4 albedo, inout float alpha, inout half3 baseNormal, out float metallic, out float ao, out float3 emission, out half3 geomN)
{
    float4 mainTexSampled = UNITY_SAMPLE_TEX2D(_MainTex, IN.uv.xy);

    float4 metallicMap = 0;
    float smoothness = 0;
    emission = 0;

    #if defined(_METALLIC_MAP)
        metallicMap = UNITY_SAMPLE_TEX2D(_MetallicGlossMap, IN.uv.xy);
        metallic = metallicMap.r;
    #else
        metallic = _Metallic;
    #endif

    //Get transparency
    #if (_RENDERING_FADE) || (_RENDERING_TRANSPARENT)
        alpha = mainTexSampled.a * UNITY_ACCESS_INSTANCED_PROP(InstancedProps, _Color).a;
    #elif defined(_RENDERING_CUTOUT)
        alpha = mainTexSampled.a * UNITY_ACCESS_INSTANCED_PROP(InstancedProps, _Color).a;
    #else
        alpha = 1;
    #endif

    //Get smoothness
    #if !defined (_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A)
        #if defined(_METALLIC_MAP)
            smoothness = metallicMap.a * _GlossMapScale; //smoothness from metallic alpha
        #else
            smoothness = _Glossiness;
        #endif
    #else //smoothness from albedo alpha
        smoothness = mainTexSampled.a * _GlossMapScale;
    #endif

    //Get AO
    #if defined(_OCCLUSION_TEX_METALLIC_CHANNEL_G)
        #if defined(_METALLIC_MAP)
            ao = lerp(1, metallicMap.g, _OcclusionStrength); //AO from metallic G channel
        #else
            ao = 1;
        #endif
    #else
        #if defined(_OCCLUSION_MAP)
            ao = lerp(1, UNITY_SAMPLE_TEX2D(_OcclusionMap, IN.uv.xy).g, _OcclusionStrength); //AO from occlusion map G channel
        #else
            ao = 1;
        #endif
    #endif

    //Get Emission
    #if defined(_EMISSION)
        #if defined(_EMISSION_MAP)
            emission = UNITY_SAMPLE_TEX2D(_EmissionMap, IN.uv.xy).rgb * _EmissionColor;
        #else
            emission = _EmissionColor;
        #endif
    #else
        emission = 0;
    #endif

    albedo.rgb = mainTexSampled.rgb * UNITY_ACCESS_INSTANCED_PROP(InstancedProps, _Color).rgb;

    albedo.a = smoothness;

    geomN = normalize(CalcWorldNormalVector(IN, float3(0, 0, 1)));

    #ifdef _NORMALMAP
        float3 tSpaceNormal = UnpackNormalWithScale(UNITY_SAMPLE_TEX2D(_BumpMap, IN.uv.xy), _BumpScale);
        baseNormal = tSpaceNormal;

        float3 binormal = cross(IN.worldNormal, IN.tangent.xyz) * IN.tangent.w;
        IN.worldNormal = normalize(tSpaceNormal.x * IN.tangent + tSpaceNormal.y * binormal + tSpaceNormal.z * IN.worldNormal);
    #endif
}
#endif
#endif // SRS_BASE_INCLUDED
