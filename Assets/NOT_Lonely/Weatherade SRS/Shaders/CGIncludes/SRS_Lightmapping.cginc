#ifndef SRS_LIGHTMAPPING_INCLUDED
#define SRS_LIGHTMAPPING_INCLUDED

#include "UnityPBSLighting.cginc"
#include "UnityMetaPass.cginc"
#include "SRS_CoverageCommon.cginc"

sampler2D _MainTex;
sampler2D _EmissionMap;
sampler2D _MetallicGlossMap;
float4 _MainTex_ST;
float4 _Color;
float3 _EmissionColor;
float _Metallic, _Glossiness, _GlossMapScale;
float _CoverageAmount;
float _EmissionMasking;
float _PrecipitationDirRange;
float3 _depthCamDir;
float _PrecipitationDirOffset;

UNITY_DECLARE_TEX2D(_SRS_depth);

struct VertexData {
	float4 vertex : POSITION;
    float3 normal : NORMAL;
    float4 color : COLOR0;
	float2 uv : TEXCOORD0;
	float2 uv1 : TEXCOORD1;
    float2 uv2 : TEXCOORD2;
};

struct v2f {
	float4 pos : SV_POSITION;
	float4 uv : TEXCOORD0;
    float snowMask : TEXCOORD1;
};

v2f VertMeta(VertexData v)
{
    v2f i;
    UNITY_INITIALIZE_OUTPUT(v2f, i);
    
    i.pos = UnityMetaVertexPosition(
		v.vertex, v.uv1, v.uv2, unity_LightmapST, unity_DynamicLightmapST
	);

    i.uv.xy = TRANSFORM_TEX(v.uv.xy, _MainTex);
    
    float3 worldNormal = UnityObjectToWorldNormal(v.normal);

    float4 addMask = float4(0, 0, 0, 0);
	float4 eraseMask = float4(1, 1, 1, 1);

	#ifdef _PAINTABLE_COVERAGE_ON
        PaintMaskRGBA(v.color, addMask, eraseMask);
	#endif

    float covAmount = (eraseMask.r * _CoverageAmount);

    //float dirMask = DirMask(_PrecipitationDirRange, _depthCamDir, worldNormal, _PrecipitationDirOffset); //precipitation direction mask
    //i.snowMask = saturate(covAmount * dirMask);

    return i;
}


float4 FragMeta (v2f i) : SV_TARGET 
{
	UnityMetaInput surfaceData;

    #if defined(_EMISSION)
        #if defined(_EMISSION_MAP)
            float3 emission = tex2D(_EmissionMap, i.uv.xy).rgb * _EmissionColor;
        #else
            float3 emission = _EmissionColor;
        #endif
    #else
        float3 emission = 0;
    #endif

    //emission *= lerp(1, 1-i.snowMask, _EmissionMasking);

	surfaceData.Emission = emission;

    float4 mainTexSampled = tex2D(_MainTex, i.uv.xy);
    float3 albedo = mainTexSampled.rgb * _Color.rgb;
    
    #if defined(_METALLIC_MAP)
        float4 metallicMap = tex2D(_MetallicGlossMap, i.uv.xy);
        float metallic = metallicMap.r;
    #else
        float metallic = _Metallic;
    #endif

    //Get smoothness
    #if !defined (_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A)
        #if defined(_METALLIC_MAP)
            float smoothness = metallicMap.a * _GlossMapScale; //smoothness from metallic alpha
        #else
            float smoothness = _Glossiness;
        #endif
    #else //smoothness from albedo alpha
        float smoothness = mainTexSampled.a * _GlossMapScale;
    #endif

	float oneMinusReflectivity;
	surfaceData.Albedo = DiffuseAndSpecularFromMetallic(
		albedo, metallic,
		surfaceData.SpecularColor, oneMinusReflectivity
	);
	return UnityMetaFragment(surfaceData);
}

#endif