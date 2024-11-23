Shader "NOT_Lonely/Weatherade/Extra/NL_DepthOccluder"
{
    Properties
    {
        [Toggle(_RENDER)] _Render("Render (debug)", Float) = 0
        _VertexPush("Vertex Push", Float) = 0.1
    }
    SubShader
    {
        Tags 
        { 
        "Queue"="Geometry+10" 
        "SRSGroupName" = "DepthOccluder"
        }
        
        //ColorMask RGBA
        ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha

        Pass 
        {
            CGPROGRAM
            #pragma shader_feature_local _RENDER

            #include "UnityCG.cginc"

            #pragma vertex Vert
            #pragma fragment Frag

            uniform float _VertexPush;

            struct VertexData 
            {
            #ifdef _RENDER
            	float4 vertex : POSITION;
                float3 normal : NORMAL;
            #else
                float4 vertex : POSITION;
            #endif
            };

            struct v2f 
            {
            #ifdef _RENDER
                float3 worldPos : TEXCOORD0;
                float3 worldNormal : NORMAL;
            #endif
            	float4 pos : SV_POSITION;
            };
            
            v2f Vert(VertexData v)
            {
                v2f i;
                #ifdef _RENDER
                    v.vertex.xyz += v.normal * _VertexPush;
                    i.pos = UnityObjectToClipPos(v.vertex);
                    i.worldPos = mul(unity_ObjectToWorld, v.vertex);
                    i.worldNormal = UnityObjectToWorldDir(v.normal);
                #else
                    i.pos = float4(0, 0, 0, 0);
                #endif
                return i;
            }
            float4 Frag(v2f i) : SV_TARGET
            {   
                #ifdef _RENDER
                    float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                    float fresnel = dot(i.worldNormal, viewDir);
                    float3 color = float3(0.137, 0.713, 1) * fresnel;
            	    return float4(color, 0.5);
                #else
                    clip(-1);
                	return (0).xxxx;
                #endif
            }

            ENDCG
        }
    }
}